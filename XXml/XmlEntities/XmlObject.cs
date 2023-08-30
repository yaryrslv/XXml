using System.Runtime.CompilerServices;
using XXml.Internal;
using XXml.ValueObjects;

namespace XXml.XmlEntities;

/// <summary>Xml-структурный объект, разобранный из xml-файла.</summary>
/// <remarks>
///     [ВНИМАНИЕ] После вызова <see cref="IsDisposed" /> НЕЛЬЗЯ вызывать никаких методов или свойств, кроме
///     <see cref="IsDisposed" />.
///     <see cref="Dispose" />.
/// </remarks>
public sealed class XmlObject : IXmlObject
{
    private readonly XmlObjectCore _core;

    internal XmlObject(in XmlObjectCore core)
    {
        _core = core;
    }

    /// <summary>Диспоз объекта xml и освобождение всех имеющихся в нем воспоминаний.</summary>
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _core.Dispose();
    }

    /// <summary>Получение информации о том, является ли объект xml задиспоженным или нет.</summary>
    /// <remarks>Не вызывайте никаких других методов или свойств, если это свойство равно false.</remarks>
    public bool IsDisposed => _core.IsDisposed;

    /// <summary>Получение рут ноды</summary>
    public XmlNode Root => _core.Root;

    /// <summary>Получение декларации xml</summary>
    public Option<XmlDeclaration> Declaration => _core.Declaration;

    /// <summary>Получение объявления типа xml-документа</summary>
    /// .
    public Option<XmlDocumentType> DocumentType => _core.DocumentType;

    /// <summary>Получение таблицы xml-сущностей</summary>
    public XmlEntityTable EntityTable => _core.EntityTable;

    /// <summary>Получение целой xml-строки в виде utf-8-байтовых данных.</summary>
    /// <returns>whole xml string</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RawString AsRawString()
    {
        return _core.AsRawString();
    }

    /// <summary>Получение нарезанной xml-строки в виде utf-8-байтовых данных.</summary>
    /// <param name="start">смещение начального байта</param>
    /// <returns>sliced xml string</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RawString AsRawString(int start)
    {
        return _core.AsRawString(start);
    }

    /// <summary>Получение нарезанной xml-строки в виде utf-8-байтовых данных.</summary>
    /// <param name="start">смещение начального байта</param>
    /// <param name="length">длина байта</param>
    /// <returns>sliced xml string</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RawString AsRawString(int start, int length)
    {
        return _core.AsRawString(start, length);
    }

    /// <summary>Получение нарезанной xml-строки в виде utf-8-байтовых данных.</summary>
    /// <param name="range">диапазон данных</param>
    /// <returns>sliced xml string</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RawString AsRawString(DataRange range)
    {
        return _core.AsRawString(range);
    }

    /// <summary>Получение всех нод (целевой тип - <see cref="XmlNodeType.ElementNode" />)</summary>
    /// <returns>all element nodes</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AllNodeList GetAllNodes()
    {
        return _core.GetAllNodes();
    }

    /// <summary>Получение всех узлов по заданному типу ноды</summary>
    /// <param name="targetType">тип узла</param>
    /// <returns>all nodes</returns>
    public AllNodeList GetAllNodes(XmlNodeType? targetType)
    {
        return _core.GetAllNodes(targetType);
    }

    public DataLocation GetLocation(XmlNode node)
    {
        return _core.GetLocation(node);
    }

    public DataLocation GetLocation(XmlAttribute attr)
    {
        return _core.GetLocation(attr);
    }

    public DataLocation GetLocation(RawString str)
    {
        return _core.GetLocation(str);
    }

    public DataLocation GetLocation(DataRange range)
    {
        return _core.GetLocation(range);
    }

    public DataRange GetRange(XmlNode node)
    {
        return _core.GetRange(node);
    }

    public DataRange GetRange(XmlAttribute attr)
    {
        return _core.GetRange(attr);
    }

    public DataRange GetRange(RawString str)
    {
        return _core.GetRange(str);
    }

    ~XmlObject()
    {
        _core.Dispose();
    }

    public override string ToString()
    {
        return AsRawString().ToString();
    }
}