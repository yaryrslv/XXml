﻿using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using XXml.ValueObjects;
using XXml.XmlEntities;

namespace XXml.InternalEntities;

internal readonly unsafe struct XmlObjectCore : IXmlObject
{
    private readonly IntPtr _rawByteData;
    private readonly int _byteLength;
    private readonly int _offset; // 0 or 3 (3 is UTF-8 BOM)
    private readonly NodeStore _store;
    private readonly OptionalNodeList _optional;
    private readonly RawStringTable _entities;

    public bool IsDisposed => _rawByteData == IntPtr.Zero;

    public XmlNode Root => _store.RootNode;

    public Option<XmlDeclaration> Declaration => new XmlDeclaration(_optional.Declaration);

    public Option<XmlDocumentType> DocumentType => new XmlDocumentType(_optional.DocumentType);

    public XmlEntityTable EntityTable => new(_entities);

    internal XmlObjectCore(ref UnmanagedBuffer buffer, int offset, ref NodeStore store, OptionalNodeList optional, RawStringTable entities)
    {
        buffer.TransferMemoryOwnership(out _rawByteData, out _byteLength);
        _offset = offset;
        _store = store;
        store = default;
        _optional = optional;
        _entities = entities;
    }

    public void Dispose()
    {
        var data = Interlocked.Exchange(ref Unsafe.AsRef(_rawByteData), default);
        if (data != IntPtr.Zero)
        {
            AllocationSafety.Remove(_byteLength);
            Marshal.FreeHGlobal(data);
            Unsafe.AsRef(_byteLength) = 0;
            Unsafe.AsRef(_offset) = 0;
            _store.Dispose();
            _optional.Dispose();
            _entities.Dispose();
        }
    }

    public DataRange GetRange(XmlNode node)
    {
        var offset = DataOffsetHelper.GetOffset((byte*) _rawByteData, _byteLength, node.NodeHeadPtr);
        if (offset.HasValue == false) ThrowHelper.ThrowArg("The node does not belong to the xml.");
        return new DataRange(offset.Value, node.NodeByteLen);
    }

    public DataRange GetRange(XmlAttribute attr)
    {
        var str = attr.AsRawString();
        var offset = DataOffsetHelper.GetOffset((byte*) _rawByteData, _byteLength, str.GetPtr());
        if (offset.HasValue == false) ThrowHelper.ThrowArg("The attribute does not belong to the xml.");
        return new DataRange(offset.Value, str.Length);
    }

    public DataRange GetRange(RawString str)
    {
        var offset = DataOffsetHelper.GetOffset((byte*) _rawByteData, _byteLength, str.GetPtr());
        if (offset.HasValue == false) ThrowHelper.ThrowArg("The string does not belong to the xml.");
        return new DataRange(offset.Value, str.Length);
    }

    public DataLocation GetLocation(XmlNode node)
    {
        var location = DataOffsetHelper.GetLocation((byte*) _rawByteData, _byteLength, node.NodeHeadPtr, node.NodeByteLen);
        if (location.HasValue == false) ThrowHelper.ThrowArg("The node does not belong to the xml.");
        return location.Value;
    }

    public DataLocation GetLocation(XmlAttribute attr)
    {
        var str = attr.AsRawString();
        var location = DataOffsetHelper.GetLocation((byte*) _rawByteData, _byteLength, str.GetPtr(), str.Length);
        if (location.HasValue == false) ThrowHelper.ThrowArg("The attribute does not belong to the xml.");
        return location.Value;
    }

    public DataLocation GetLocation(RawString str)
    {
        var location = DataOffsetHelper.GetLocation((byte*) _rawByteData, _byteLength, str.GetPtr(), str.Length);
        if (location.HasValue == false) ThrowHelper.ThrowArg("The string does not belong to the xml.");
        return location.Value;
    }

    public DataLocation GetLocation(DataRange range)
    {
        var dataHead = (byte*) _rawByteData;
        var location = DataOffsetHelper.GetLocation(dataHead, _byteLength, dataHead + range.Start, range.Length);
        if (location.HasValue == false) ThrowHelper.ThrowArg("The range is out of the xml.");
        return location.Value;
    }

    /// <summary>Получение целой xml строки в виде utf-8-байтовых данных.</summary>
    /// <returns>whole xml string</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RawString AsRawString()
    {
        return new RawString((byte*) _rawByteData + _offset, _byteLength - _offset);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RawString AsRawString(int start)
    {
        return AsRawString().Slice(start);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RawString AsRawString(int start, int length)
    {
        return AsRawString().Slice(start, length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RawString AsRawString(DataRange range)
    {
        return AsRawString().Slice(range);
    }

    /// <summary>Получение всех нод (целевой тип - <see cref="XmlNodeType.ElementNode" />)</summary>
    /// <returns>all element nodes</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AllNodeList GetAllNodes()
    {
        return _store.GetAllNodes(XmlNodeType.ElementNode);
    }

    /// <summary>Получение всех нод по заданному типу ноды</summary>
    /// <param name="targetType">тип узла</param>
    /// <returns>all nodes</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AllNodeList GetAllNodes(XmlNodeType? targetType)
    {
        return _store.GetAllNodes(targetType);
    }
}

internal unsafe struct NodeStore : IDisposable
{
    private CustomList<XmlNodeStruct> _allNodes;
    private CustomList<XmlAttributeStruct> _allAttrs;

    // [NOTE]
    // Не добавляйте ноду из этого свойства напрямую.
    public CustomList<XmlNodeStruct> AllNodes => _allNodes;

    public CustomList<XmlAttributeStruct> AllAttrs => _allAttrs;
    public readonly int NodeCount => _allNodes.Count;
    private int ElementNodeCount { get; set; }

    private readonly int TextNodeCount => _allNodes.Count - ElementNodeCount;

    public XmlNode RootNode => new(_allNodes.FirstItem);

    public static NodeStore Create()
    {
        CustomList<XmlNodeStruct> allNodes = default;
        CustomList<XmlAttributeStruct> allAttrs = default;
        try
        {
            allNodes = CustomList<XmlNodeStruct>.Create();
            allAttrs = CustomList<XmlAttributeStruct>.Create();
            return new NodeStore
            {
                _allNodes = allNodes,
                _allAttrs = allAttrs,
                ElementNodeCount = 0
            };
        }
        catch
        {
            allNodes.Dispose();
            allAttrs.Dispose();
            throw;
        }
    }

    public XmlNodeStruct* AddTextNode(int depth, byte* nodeStrPtr)
    {
        var textNode = _allNodes.GetPointerToAdd(out var nodeIndex);
        *textNode = XmlNodeStruct.CreateTextNode(_allNodes, nodeIndex, depth, nodeStrPtr, _allAttrs);
        return textNode;
    }

    public XmlNodeStruct* AddElementNode(RawString name, int depth, byte* nodeStrPtr)
    {
        var elementNode = _allNodes.GetPointerToAdd(out var nodeIndex);
        *elementNode = XmlNodeStruct.CreateElementNode(_allNodes, nodeIndex, depth, name, nodeStrPtr, _allAttrs);
        ElementNodeCount++;
        return elementNode;
    }

    public readonly AllNodeList GetAllNodes(XmlNodeType? targetType)
    {
        var count = targetType switch
        {
            null => NodeCount,
            XmlNodeType.ElementNode => ElementNodeCount,
            XmlNodeType.TextNode => TextNodeCount,
            _ => default
        };
        return new AllNodeList(_allNodes, count, targetType);
    }

    public CustomList<XmlNodeStruct>.Enumerator GetAllNodesEnumerator()
    {
        return _allNodes.GetEnumerator();
    }

    public readonly void Dispose()
    {
        _allNodes.Dispose();
        _allAttrs.Dispose();
    }
}