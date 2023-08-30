using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using XXml.InternalEntities;
using XXml.ValueObjects;
using XXml.XmlEntities;

namespace XXml.UnsafeEntities;

/// <summary>
///     [ПРЕДУПРЕЖДЕНИЕ] Не используйте этот метод, если не знаете, как его использовать. Этот метод является скрытым.
///     <para />
///     *** При неправильном использовании происходит утечка памяти. ***
///     <para />
///     Вы ДОЛЖНЫ задиспозить его после использования. Он совместим с <see cref="XmlObject" />
/// </summary>
public readonly unsafe struct XmlObjectUnsafe : IXmlObject
{
    private readonly IntPtr _core; // XmlObjectCore*

    public bool IsDisposed => _core == IntPtr.Zero;

    private XmlObjectUnsafe(IntPtr core)
    {
        _core = core;
    }

    public XmlNode Root => ((XmlObjectCore*) _core)->Root;

    public Option<XmlDeclaration> Declaration => ((XmlObjectCore*) _core)->Declaration;

    public Option<XmlDocumentType> DocumentType => ((XmlObjectCore*) _core)->DocumentType;

    public XmlEntityTable EntityTable => ((XmlObjectCore*) _core)->EntityTable;

    public RawString AsRawString()
    {
        return ((XmlObjectCore*) _core)->AsRawString();
    }

    public RawString AsRawString(int start)
    {
        return ((XmlObjectCore*) _core)->AsRawString(start);
    }

    public RawString AsRawString(int start, int length)
    {
        return ((XmlObjectCore*) _core)->AsRawString(start, length);
    }

    public RawString AsRawString(DataRange range)
    {
        return ((XmlObjectCore*) _core)->AsRawString(range);
    }

    public void Dispose()
    {
        var core = Interlocked.Exchange(ref Unsafe.AsRef(_core), IntPtr.Zero);
        if (core == IntPtr.Zero) return;
        ((XmlObjectCore*) core)->Dispose();
        Marshal.FreeHGlobal(core);
        AllocationSafety.Remove(sizeof(XmlObjectCore));
    }

    /// <summary>Получение всех нод (целевой тип - <see cref="XmlNodeType.ElementNode" />)</summary>
    /// <returns>all element nodes</returns>
    public AllNodeList GetAllNodes()
    {
        return ((XmlObjectCore*) _core)->GetAllNodes();
    }

    /// <summary>Get all nodes by specifying node type</summary>
    /// <param name="targetType">node type</param>
    /// <returns>all nodes</returns>
    public AllNodeList GetAllNodes(XmlNodeType? targetType)
    {
        return ((XmlObjectCore*) _core)->GetAllNodes(targetType);
    }

    public DataLocation GetLocation(XmlNode node)
    {
        return ((XmlObjectCore*) _core)->GetLocation(node);
    }

    public DataLocation GetLocation(XmlAttribute attr)
    {
        return ((XmlObjectCore*) _core)->GetLocation(attr);
    }

    public DataLocation GetLocation(RawString str)
    {
        return ((XmlObjectCore*) _core)->GetLocation(str);
    }

    public DataLocation GetLocation(DataRange range)
    {
        return ((XmlObjectCore*) _core)->GetLocation(range);
    }

    public DataRange GetRange(XmlNode node)
    {
        return ((XmlObjectCore*) _core)->GetRange(node);
    }

    public DataRange GetRange(XmlAttribute attr)
    {
        return ((XmlObjectCore*) _core)->GetRange(attr);
    }

    public DataRange GetRange(RawString str)
    {
        return ((XmlObjectCore*) _core)->GetRange(str);
    }

    public override string ToString()
    {
        return AsRawString().ToString();
    }

    internal static XmlObjectUnsafe Create(in XmlObjectCore core)
    {
        var size = sizeof(XmlObjectCore);
        var ptr = Marshal.AllocHGlobal(size);
        AllocationSafety.Add(size);
        *(XmlObjectCore*) ptr = core;
        return new XmlObjectUnsafe(ptr);
    }
}