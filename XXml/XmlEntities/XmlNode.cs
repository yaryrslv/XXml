using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using XXml.Internal;
using XXml.ValueObjects;

namespace XXml.XmlEntities;

/// <summary>Сущность для xml ноды.</summary>
[DebuggerDisplay("{DebugView(),nq}")]
public readonly unsafe struct XmlNode : IEquatable<XmlNode>, IReference
{
    private readonly IntPtr _node; // XmlNodeStruct*

    private string DebugView()
    {
        var node = (XmlNodeStruct*) _node;
        return node == null
            ? ""
            : node->NodeType switch
            {
                XmlNodeType.ElementNode => $"<{node->Name}>",
                XmlNodeType.TextNode => node->InnerText.ToString(),
                _ => ""
            };
    }

    internal byte* NodeHeadPtr => ((XmlNodeStruct*) _node)->NodeStrPtr;
    internal int NodeByteLen => ((XmlNodeStruct*) _node)->NodeStrLength;

    /// <summary>Получить тип ноды</summary>
    public XmlNodeType NodeType => ((XmlNodeStruct*) _node)->NodeType;

    /// <summary>Получение информации о том, является ли нода null. (Действительные узлы всегда возвращают false.)</summary>
    /// .
    public bool IsNull => _node == IntPtr.Zero;

    /// <summary>Получить имя ноды.</summary>
    public RawString Name => ((XmlNodeStruct*) _node)->Name;

    /// <summary>Получение внутреннего текста нодыsummary</summary>
    public RawString InnerText => ((XmlNodeStruct*) _node)->InnerText;

    /// <summary>Получение информации о наличии у узла какого либо аттрибута.</summary>
    public bool HasAttribute => ((XmlNodeStruct*) _node)->HasAttribute;

    /// <summary>Получение атрибутов ноды.</summary>
    public XmlAttributeList Attributes => new((XmlNodeStruct*) _node);

    /// <summary>Получение информации о том, есть ли у ноды дочерние элементы.</summary>
    public bool HasChildren => ((XmlNodeStruct*) _node)->HasChildren;

    /// <summary>Get children of <see cref="XmlNodeType.ElementNode" />. (То же самое, что и <see cref="Children" /> свойство.)</summary>
    public XmlNodeList Children => new((XmlNodeStruct*) _node, XmlNodeType.ElementNode);

    /// <summary>Получение всех дочерних нод <see cref="XmlNodeType.ElementNode" /> способом глубинного поиска.</summary>
    /// .
    public XmlNodeDescendantList Descendants => new((XmlNodeStruct*) _node, XmlNodeType.ElementNode);

    /// <summary>Получение глубины ноды в xml. (Корневая нода равена 0.)</summary>
    /// .
    public int Depth => ((XmlNodeStruct*) _node)->Depth;

    /// <summary>Получение информации о том, является ли данная нода корневой.</summary>
    public bool IsRoot => ((XmlNodeStruct*) _node)->Parent == null;

    /// <summary>Получение родительской ноды.</summary>
    private Option<XmlNode> Parent => new XmlNode(((XmlNodeStruct*) _node)->Parent);

    /// <summary>Получение первой дочерней ноды.</summary>
    private Option<XmlNode> FirstChild => new XmlNode(((XmlNodeStruct*) _node)->FirstChild);

    /// <summary>Получение последней дочерней ноды.</summary>
    private Option<XmlNode> LastChild => new XmlNode(((XmlNodeStruct*) _node)->LastChild);

    // <summary>Получение следующей родственной ноды (В ширину).</summary>
    private Option<XmlNode> NextSibling => new XmlNode(((XmlNodeStruct*) _node)->Sibling);

    internal bool HasXmlNamespaceAttr => ((XmlNodeStruct*) _node)->HasXmlNamespaceAttr;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal XmlNode(XmlNodeStruct* node)
    {
        _node = (IntPtr) node;
    }

    /// <summary>Получение дочерних нод по указанию типа ноды.</summary>
    /// <param name="targetType">целевой тип xml ноды. (Если задано null, возвращаются все типы нод.)</param>
    /// <returns>child nodes</returns>
    /// .
    public XmlNodeList GetChildren(XmlNodeType? targetType)
    {
        return new XmlNodeList((XmlNodeStruct*) _node, targetType);
    }

    /// <summary>Получение нод потомков по указанию типа ноды способом поиска в глубину.</summary>
    /// <param name="targetType">целевой тип xml ноды. (Если задано null, то возвращаются все типы нод).</param>
    /// <returns></returns>
    public XmlNodeDescendantList GetDescendants(XmlNodeType? targetType)
    {
        return new XmlNodeDescendantList((XmlNodeStruct*) _node, targetType);
    }

    /// <summary>Получение строки, которую представляет данная нода в виде <see cref="RawString" />.</summary>
    /// <remarks>Отступ ноды в начале игнорируется.</remarks>
    /// <returns><see cref="RawString" /> данный узел представляет собой</returns>
    /// .
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RawString AsRawString()
    {
        return ((XmlNodeStruct*) _node)->AsRawString();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetParent(out XmlNode parent)
    {
        return Parent.TryGetValue(out parent);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetFirstChild(out XmlNode firstChild)
    {
        return FirstChild.TryGetValue(out firstChild);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetLastChild(out XmlNode lastChild)
    {
        return LastChild.TryGetValue(out lastChild);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryNextSibling(out XmlNode nextSibling)
    {
        return NextSibling.TryGetValue(out nextSibling);
    }

    private bool IsName(ReadOnlySpan<byte> namespaceName, ReadOnlySpan<byte> name)
    {
        if (namespaceName.IsEmpty || name.IsEmpty) return false;
        if (XmlnsHelper.TryResolveNamespaceAlias(namespaceName, this, out var alias) == false) return false;
        if (alias.IsEmpty) return Name == name;

        var nodeName = Name;
        return nodeName.Length == alias.Length + 1 + name.Length
               && nodeName.StartsWith(alias)
               && nodeName.At(alias.Length) == (byte) ':'
               && nodeName.EndsWith(name);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsName(ReadOnlySpan<byte> namespaceName, RawString name)
    {
        return IsName(namespaceName, name.AsSpan());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsName(RawString namespaceName, ReadOnlySpan<byte> name)
    {
        return IsName(namespaceName.AsSpan(), name);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsName(RawString namespaceName, RawString name)
    {
        return IsName(namespaceName.AsSpan(), name.AsSpan());
    }

    [SkipLocalsInit]
    public bool IsName(ReadOnlySpan<char> namespaceName, ReadOnlySpan<char> name)
    {
        if (namespaceName.IsEmpty || name.IsEmpty) return false;

        var utf8 = Utf8ExceptionFallbackEncoding.Instance;
        var nsNameByteLen = utf8.GetByteCount(namespaceName);
        var nameByteLen = utf8.GetByteCount(name);
        var byteLen = nsNameByteLen + nameByteLen;

        const int threshold = 128;
        if (byteLen <= threshold)
        {
            var buf = stackalloc byte[threshold];
            fixed (char* ptr = namespaceName)
            {
                utf8.GetBytes(ptr, namespaceName.Length, buf, nsNameByteLen);
            }

            var nsNameUtf8 = SpanHelper.CreateReadOnlySpan<byte>(buf, nsNameByteLen);
            fixed (char* ptr = name)
            {
                utf8.GetBytes(ptr, name.Length, buf + nsNameByteLen, nameByteLen);
            }

            var nameUtf8 = SpanHelper.CreateReadOnlySpan<byte>(buf + nsNameByteLen, nameByteLen);
            return IsName(nsNameUtf8, nameUtf8);
        }

        var rentArray = ArrayPool<byte>.Shared.Rent(byteLen);
        try
        {
            fixed (byte* buf = rentArray)
            fixed (char* ptr = namespaceName)
            fixed (char* ptr2 = name)
            {
                utf8.GetBytes(ptr, namespaceName.Length, buf, nsNameByteLen);
                var nsNameUtf8 = SpanHelper.CreateReadOnlySpan<byte>(buf, nsNameByteLen);
                utf8.GetBytes(ptr2, name.Length, buf + nsNameByteLen, nameByteLen);
                var nameUtf8 = SpanHelper.CreateReadOnlySpan<byte>(buf + nsNameByteLen, nameByteLen);
                return IsName(nsNameUtf8, nameUtf8);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rentArray);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsName(ReadOnlySpan<char> namespaceName, string name)
    {
        return IsName(namespaceName, name.AsSpan());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsName(string namespaceName, ReadOnlySpan<char> name)
    {
        return IsName(namespaceName.AsSpan(), name);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsName(string namespaceName, string name)
    {
        return IsName(namespaceName.AsSpan(), name.AsSpan());
    }

    /// <summary>Найти потомка по имени. Возвращает первого найденного потомка.</summary>
    /// <param name="name">имя найденной дочерней ноды</param>
    /// <returns> найденная дочерняя нода в виде <see cref="Option{T}" /></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<XmlNode> FindChildOrDefault(RawString name)
    {
        return FindChildOrDefault(name.AsSpan());
    }

    /// <summary>Найти потомка по имени. Возвращает первого найденного потомка.</summary>
    /// <param name="name">имя найденной дочерней ноды</param>
    /// <returns> найденная дочерняя нода в виде <see cref="Option{T}" /></returns>
    public Option<XmlNode> FindChildOrDefault(ReadOnlySpan<byte> name)
    {
        if (name.IsEmpty) return Option<XmlNode>.Null;
        foreach (var child in Children)
            if (child.Name == name)
                return child;
        return Option<XmlNode>.Null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Option<XmlNode> FindChildOrDefault(RawString namespaceName, RawString name)
    {
        return Children.FindOrDefault(namespaceName.AsSpan(), name.AsSpan());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Option<XmlNode> FindChildOrDefault(ReadOnlySpan<byte> namespaceName, RawString name)
    {
        return Children.FindOrDefault(namespaceName, name.AsSpan());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Option<XmlNode> FindChildOrDefault(RawString namespaceName, ReadOnlySpan<byte> name)
    {
        return Children.FindOrDefault(namespaceName.AsSpan(), name);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Option<XmlNode> FindChildOrDefault(ReadOnlySpan<byte> namespaceName, ReadOnlySpan<byte> name)
    {
        return Children.FindOrDefault(namespaceName, name);
    }

    /// <summary>Найти потомка по имени. Возвращает первого найденного потомка.</summary>
    /// <param name="name">имя найденной дочерней ноды</param>
    /// <returns> найденная дочерняя нода в виде <see cref="Option{T}" /></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<XmlNode> FindChildOrDefault(string name)
    {
        return Children.FindOrDefault(name.AsSpan());
    }

    /// <summary>Найти потомка по имени. Возвращает первого найденного потомка.</summary>
    /// <param name="name">имя найденной дочерней ноды</param>
    /// <returns> найденная дочерняя нода в виде <see cref="Option{T}" /></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Option<XmlNode> FindChildOrDefault(ReadOnlySpan<char> name)
    {
        return Children.FindOrDefault(name);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Option<XmlNode> FindChildOrDefault(string namespaceName, string name)
    {
        return Children.FindOrDefault(namespaceName.AsSpan(), name.AsSpan());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Option<XmlNode> FindChildOrDefault(ReadOnlySpan<char> namespaceName, string name)
    {
        return Children.FindOrDefault(namespaceName, name.AsSpan());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Option<XmlNode> FindChildOrDefault(string namespaceName, ReadOnlySpan<char> name)
    {
        return Children.FindOrDefault(namespaceName.AsSpan(), name);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Option<XmlNode> FindChildOrDefault(ReadOnlySpan<char> namespaceName, ReadOnlySpan<char> name)
    {
        return Children.FindOrDefault(namespaceName, name);
    }

    /// <summary>
    ///     Находит потомка по имени. Возвращает первого найденного потомка или кидает <see cref="InvalidOperationException" />
    ///     , если он не найден.
    /// </summary>
    /// <param name="name">имя потомка для поиска</param>
    /// <returns> найденная дочерняя нода</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public XmlNode FindChild(RawString name)
    {
        return FindChild(name.AsSpan());
    }

    /// <summary>
    ///     Находит потомка по имени. Возвращает первого найденного потомка или кидает <see cref="InvalidOperationException" />
    ///     , если он не найден.
    /// </summary>
    /// <param name="name">имя потомка для поиска</param>
    /// <returns> найденная дочерняя нода</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public XmlNode FindChild(ReadOnlySpan<byte> name)
    {
        if (FindChildOrDefault(name).TryGetValue(out var node) == false) ThrowHelper.ThrowInvalidOperation("Sequence contains no matching elements.");
        return node;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public XmlNode FindChild(RawString namespaceName, RawString name)
    {
        return FindChild(namespaceName.AsSpan(), name.AsSpan());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public XmlNode FindChild(ReadOnlySpan<byte> namespaceName, RawString name)
    {
        return FindChild(namespaceName, name.AsSpan());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public XmlNode FindChild(RawString namespaceName, ReadOnlySpan<byte> name)
    {
        return FindChild(namespaceName.AsSpan(), name);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public XmlNode FindChild(ReadOnlySpan<byte> namespaceName, ReadOnlySpan<byte> name)
    {
        if (FindChildOrDefault(namespaceName, name).TryGetValue(out var node) == false) ThrowHelper.ThrowInvalidOperation("Sequence contains no matching elements.");
        return node;
    }

    /// <summary>
    ///     Находит потомка по имени. Возвращает первого найденного потомка или кидает <see cref="InvalidOperationException" />
    ///     , если он не найден.
    /// </summary>
    /// <param name="name">имя потомка для поиска</param>
    /// <returns> найденная дочерняя нода</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public XmlNode FindChild(string name)
    {
        return FindChild(name.AsSpan());
    }

    /// <summary>
    ///     Находит потомка по имени. Возвращает первого найденного потомка или кидает <see cref="InvalidOperationException" />
    ///     , если он не найден.
    /// </summary>
    /// <param name="name">имя потомка для поиска</param>
    /// <returns> найденная дочерняя нода</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public XmlNode FindChild(ReadOnlySpan<char> name)
    {
        if (FindChildOrDefault(name).TryGetValue(out var node) == false) ThrowHelper.ThrowInvalidOperation("Sequence contains no matching elements.");
        return node;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public XmlNode FindChild(string namespaceName, string name)
    {
        return FindChild(namespaceName.AsSpan(), name.AsSpan());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public XmlNode FindChild(ReadOnlySpan<char> namespaceName, string name)
    {
        return FindChild(namespaceName, name.AsSpan());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public XmlNode FindChild(string namespaceName, ReadOnlySpan<char> name)
    {
        return FindChild(namespaceName.AsSpan(), name);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public XmlNode FindChild(ReadOnlySpan<char> namespaceName, ReadOnlySpan<char> name)
    {
        if (FindChildOrDefault(namespaceName, name).TryGetValue(out var node) == false) ThrowHelper.ThrowInvalidOperation("Sequence contains no matching elements.");
        return node;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryFindChild(ReadOnlySpan<byte> name, out XmlNode node)
    {
        return FindChildOrDefault(name).TryGetValue(out node);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryFindChild(RawString name, out XmlNode node)
    {
        return FindChildOrDefault(name).TryGetValue(out node);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryFindChild(RawString namespaceName, RawString name, out XmlNode node)
    {
        return FindChildOrDefault(namespaceName, name).TryGetValue(out node);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryFindChild(ReadOnlySpan<byte> namespaceName, RawString name, out XmlNode node)
    {
        return FindChildOrDefault(namespaceName, name).TryGetValue(out node);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryFindChild(RawString namespaceName, ReadOnlySpan<byte> name, out XmlNode node)
    {
        return FindChildOrDefault(namespaceName, name).TryGetValue(out node);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryFindChild(ReadOnlySpan<byte> namespaceName, ReadOnlySpan<byte> name, out XmlNode node)
    {
        return FindChildOrDefault(namespaceName, name).TryGetValue(out node);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryFindChild(ReadOnlySpan<char> name, out XmlNode node)
    {
        return FindChildOrDefault(name).TryGetValue(out node);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryFindChild(string name, out XmlNode node)
    {
        return FindChildOrDefault(name).TryGetValue(out node);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryFindChild(string namespaceName, string name, out XmlNode node)
    {
        return FindChildOrDefault(namespaceName, name).TryGetValue(out node);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryFindChild(ReadOnlySpan<char> namespaceName, string name, out XmlNode node)
    {
        return FindChildOrDefault(namespaceName, name).TryGetValue(out node);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryFindChild(string namespaceName, ReadOnlySpan<char> name, out XmlNode node)
    {
        return FindChildOrDefault(namespaceName, name).TryGetValue(out node);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryFindChild(ReadOnlySpan<char> namespaceName, ReadOnlySpan<char> name, out XmlNode node)
    {
        return FindChildOrDefault(namespaceName, name).TryGetValue(out node);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<XmlAttribute> FindAttributeOrDefault(RawString name)
    {
        return Attributes.FindOrDefault(name);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<XmlAttribute> FindAttributeOrDefault(ReadOnlySpan<byte> name)
    {
        return Attributes.FindOrDefault(name);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<XmlAttribute> FindAttributeOrDefault(string name)
    {
        return Attributes.FindOrDefault(name);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<XmlAttribute> FindAttributeOrDefault(ReadOnlySpan<char> name)
    {
        return Attributes.FindOrDefault(name);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<XmlAttribute> FindAttributeOrDefault(ReadOnlySpan<byte> namespaceName, ReadOnlySpan<byte> name)
    {
        return Attributes.FindOrDefault(namespaceName, name);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<XmlAttribute> FindAttributeOrDefault(ReadOnlySpan<byte> namespaceName, RawString name)
    {
        return Attributes.FindOrDefault(namespaceName, name);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<XmlAttribute> FindAttributeOrDefault(RawString namespaceName, ReadOnlySpan<byte> name)
    {
        return Attributes.FindOrDefault(namespaceName, name);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<XmlAttribute> FindAttributeOrDefault(RawString namespaceName, RawString name)
    {
        return Attributes.FindOrDefault(namespaceName, name);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<XmlAttribute> FindAttributeOrDefault(ReadOnlySpan<char> namespaceName, ReadOnlySpan<char> name)
    {
        return Attributes.FindOrDefault(namespaceName, name);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<XmlAttribute> FindAttributeOrDefault(ReadOnlySpan<char> namespaceName, string name)
    {
        return Attributes.FindOrDefault(namespaceName, name);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<XmlAttribute> FindAttributeOrDefault(string namespaceName, ReadOnlySpan<char> name)
    {
        return Attributes.FindOrDefault(namespaceName, name);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<XmlAttribute> FindAttributeOrDefault(string namespaceName, string name)
    {
        return Attributes.FindOrDefault(namespaceName, name);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public XmlAttribute FindAttribute(RawString name)
    {
        return Attributes.Find(name);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public XmlAttribute FindAttribute(ReadOnlySpan<byte> name)
    {
        return Attributes.Find(name);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public XmlAttribute FindAttribute(string name)
    {
        return Attributes.Find(name);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public XmlAttribute FindAttribute(ReadOnlySpan<char> name)
    {
        return Attributes.Find(name);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public XmlAttribute FindAttribute(ReadOnlySpan<byte> namespaceName, ReadOnlySpan<byte> name)
    {
        return Attributes.Find(namespaceName, name);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public XmlAttribute FindAttribute(ReadOnlySpan<byte> namespaceName, RawString name)
    {
        return Attributes.Find(namespaceName, name);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public XmlAttribute FindAttribute(RawString namespaceName, ReadOnlySpan<byte> name)
    {
        return Attributes.Find(namespaceName, name);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public XmlAttribute FindAttribute(RawString namespaceName, RawString name)
    {
        return Attributes.Find(namespaceName, name);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public XmlAttribute FindAttribute(ReadOnlySpan<char> namespaceName, ReadOnlySpan<char> name)
    {
        return Attributes.Find(namespaceName, name);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public XmlAttribute FindAttribute(ReadOnlySpan<char> namespaceName, string name)
    {
        return Attributes.Find(namespaceName, name);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public XmlAttribute FindAttribute(string namespaceName, ReadOnlySpan<char> name)
    {
        return Attributes.Find(namespaceName, name);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public XmlAttribute FindAttribute(string namespaceName, string name)
    {
        return Attributes.Find(namespaceName, name);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryFindAttribute(RawString name, out XmlAttribute attribute)
    {
        return Attributes.TryFind(name, out attribute);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryFindAttribute(ReadOnlySpan<byte> name, out XmlAttribute attribute)
    {
        return Attributes.TryFind(name, out attribute);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryFindAttribute(string name, out XmlAttribute attribute)
    {
        return Attributes.TryFind(name, out attribute);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryFindAttribute(ReadOnlySpan<char> name, out XmlAttribute attribute)
    {
        return Attributes.TryFind(name, out attribute);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryFindAttribute(ReadOnlySpan<byte> namespaceName, ReadOnlySpan<byte> name, out XmlAttribute attribute)
    {
        return Attributes.TryFind(namespaceName, name, out attribute);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryFindAttribute(ReadOnlySpan<byte> namespaceName, RawString name, out XmlAttribute attribute)
    {
        return Attributes.TryFind(namespaceName, name, out attribute);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryFindAttribute(RawString namespaceName, ReadOnlySpan<byte> name, out XmlAttribute attribute)
    {
        return Attributes.TryFind(namespaceName, name, out attribute);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryFindAttribute(RawString namespaceName, RawString name, out XmlAttribute attribute)
    {
        return Attributes.TryFind(namespaceName, name, out attribute);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryFindAttribute(ReadOnlySpan<char> namespaceName, ReadOnlySpan<char> name, out XmlAttribute attribute)
    {
        return Attributes.TryFind(namespaceName, name, out attribute);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryFindAttribute(ReadOnlySpan<char> namespaceName, string name, out XmlAttribute attribute)
    {
        return Attributes.TryFind(namespaceName, name, out attribute);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryFindAttribute(string namespaceName, ReadOnlySpan<char> name, out XmlAttribute attribute)
    {
        return Attributes.TryFind(namespaceName, name, out attribute);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryFindAttribute(string namespaceName, string name, out XmlAttribute attribute)
    {
        return Attributes.TryFind(namespaceName, name, out attribute);
    }

    /// <summary>Получение полного имени ноды. Возвращает false, если полное имя не может быть зарезовлено.</summary>
    /// <param name="namespaceName">
    ///     имя пространства имен ноды.
    ///     <para />
    ///     например, "abcde" в случае, если узел нода вид &lt;a:foo xmlns:a="abcde" /&gt;
    ///     <para />
    /// </param>
    /// <param name="name">
    ///     локальное имя ноды
    ///     <para />
    ///     например, "foo" в случае, когда нода имеет вид &lt;a:foo xmlns:a="abcde" /&gt;
    ///     <para />
    /// </param>
    /// <returns>Возможно ли разрешить полное имя ноды</returns>
    /// .
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetFullName(out RawString namespaceName, out RawString name)
    {
        return XmlnsHelper.TryGetNodeFullName(this, out namespaceName, out name);
    }

    /// <summary>
    ///     Получение полного имени ноды. Метод выбрасывает <see cref="InvalidOperationException" />, если полное имя не
    ///     удаллось зарезолвить.
    ///     не разрешено.
    /// </summary>
    /// <remarks>
    ///     ex) Возвращает ("abcde", "foo") в случае, если нода является &lt;a:foo xmlns:a="abcde" /&gt;
    ///     <para />
    /// </remarks>
    /// <exception cref="InvalidOperationException">полное имя не удалось разрешить</exception>
    /// .
    /// <returns>Пара из имени пространства имен и локального имени</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (RawString NamespaceName, RawString Name) GetFullName()
    {
        if (XmlnsHelper.TryGetNodeFullName(this, out var namespaceName, out var name) == false)
        {
            ThrowNoNamespace();

            static void ThrowNoNamespace()
            {
                throw new InvalidOperationException("Could not resolve the full name of the node.");
            }
        }

        return (namespaceName, name);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is XmlNode node && Equals(node);
    }

    /// <summary>Возвращает значение, совпадает ли оно с указанным экземпляром.</summary>
    /// <param name="other">экземпляр для проверки</param>
    /// <returns>equal or not</returns>
    public bool Equals(XmlNode other)
    {
        return _node == other._node;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return _node.GetHashCode();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return _node != IntPtr.Zero ? ((XmlNodeStruct*) _node)->ToString() : "";
    }

    /// <summary>Возвращает true, если оба <see cref="XmlNode" />одинаковых объекта.</summary>
    /// <param name="left">левый операнд</param>
    /// <param name="right">правый операнд</param>
    /// <returns>true, если оба <see cref="XmlNode" />s являются одинаковыми объектами</returns>
    /// .
    public static bool operator ==(XmlNode left, XmlNode right)
    {
        return left.Equals(right);
    }

    /// <summary>Возвращает true, если оба <see cref="XmlNode" />не являются одинаковыми объектами.</summary>
    /// <param name="left">левый операнд</param>
    /// <param name="right">правый операнд</param>
    /// <returns>true, если оба <see cref="XmlNode" />не являются одинаковыми объектами</returns>
    /// .
    public static bool operator !=(XmlNode left, XmlNode right)
    {
        return !(left == right);
    }
}

[DebuggerDisplay("{ToString(),nq}")]
internal unsafe struct XmlNodeStruct
{
    private readonly IntPtr _wholeNodes; // Его тип - CustomList<XmlNodeStruct>, но поле - IntPtr. *** См. комментарий в конструкторе ***.
    public readonly int NodeIndex;
    public readonly int Depth;
    public readonly RawString Name;
    public RawString InnerText;
    public readonly byte* NodeStrPtr;
    public int NodeStrLength;

    public XmlNodeStruct* Parent;
    public XmlNodeStruct* FirstChild;
    public XmlNodeStruct* LastChild;
    public XmlNodeStruct* Sibling;
    public int ChildCount;
    public int ChildElementCount;
    public int ChildTextCount => ChildCount - ChildElementCount;

    public int AttrIndex;
    public int AttrCount;
    public readonly CustomList<XmlAttributeStruct> WholeAttrs;

    public bool HasXmlNamespaceAttr;

    public XmlNodeType NodeType => Name.IsEmpty ? XmlNodeType.TextNode : XmlNodeType.ElementNode;

    public readonly CustomList<XmlNodeStruct> WholeNodes =>
        // См. комментарий в конструкторе, чтобы понять, что означает следующее.
        Unsafe.As<IntPtr, CustomList<XmlNodeStruct>>(ref Unsafe.AsRef(_wholeNodes));

    public bool HasAttribute => AttrCount > 0;

    public bool HasChildren => ChildElementCount > 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private XmlNodeStruct(CustomList<XmlNodeStruct> wholeNodes, int nodeIndex, int depth, RawString name, byte* nodeStrPtr, CustomList<XmlAttributeStruct> wholeAttrs)
    {
        // [NOTE]
        // _wholeNodes является CustomList<XmlNodeStruct>,
        // но XmlNodeStruct не может иметь полей типа CustomList<XmlNodeStruct> из-за ошибки среды выполнения dotnet.
        // CustomList<XmlNodeStruct> имеет ту же компоновку памяти, что и IntPtr.
        // Поэтому XmlNodeStruct имеет 'wholeNodes' как IntPtr.

        Debug.Assert(sizeof(CustomList<XmlNodeStruct>) == sizeof(IntPtr));
        _wholeNodes = Unsafe.As<CustomList<XmlNodeStruct>, IntPtr>(ref wholeNodes);

        NodeIndex = nodeIndex;
        Depth = depth;
        Name = name;
        InnerText = RawString.Empty;
        Parent = null;
        FirstChild = null;
        LastChild = null;
        Sibling = null;
        ChildCount = 0;
        ChildElementCount = 0;
        AttrIndex = 0;
        AttrCount = 0;
        WholeAttrs = wholeAttrs;
        NodeStrPtr = nodeStrPtr;
        NodeStrLength = 0;
        HasXmlNamespaceAttr = false;
    }

    internal static XmlNodeStruct CreateElementNode(CustomList<XmlNodeStruct> wholeNodes, int nodeIndex, int depth, RawString name, byte* nodeStrPtr, CustomList<XmlAttributeStruct> wholeAttrs)
    {
        return new XmlNodeStruct(wholeNodes, nodeIndex, depth, name, nodeStrPtr, wholeAttrs);
    }

    internal static XmlNodeStruct CreateTextNode(CustomList<XmlNodeStruct> wholeNodes, int nodeIndex, int depth, byte* nodeStrPtr, CustomList<XmlAttributeStruct> wholeAttrs)
    {
        return new XmlNodeStruct(wholeNodes, nodeIndex, depth, RawString.Empty, nodeStrPtr, wholeAttrs);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RawString AsRawString()
    {
        return new RawString(NodeStrPtr, NodeStrLength);
    }

    public override string ToString()
    {
        return NodeType switch
        {
            XmlNodeType.ElementNode => Name.ToString(),
            XmlNodeType.TextNode => InnerText.ToString(),
            _ => ""
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AddChildElementNode(XmlNodeStruct* parent, XmlNodeStruct* child)
    {
        Debug.Assert(child != null);
        Debug.Assert(child->NodeType == XmlNodeType.ElementNode);
        if (parent->FirstChild == null)
            parent->FirstChild = child;
        else
            parent->LastChild->Sibling = child;
        parent->LastChild = child;
        parent->ChildCount++;
        parent->ChildElementCount++;
        child->Parent = parent;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AddChildTextNode(XmlNodeStruct* parent, XmlNodeStruct* child)
    {
        Debug.Assert(child != null);
        Debug.Assert(child->NodeType == XmlNodeType.TextNode);
        if (parent->FirstChild == null)
            parent->FirstChild = child;
        else
            parent->LastChild->Sibling = child;
        parent->LastChild = child;
        parent->ChildCount++;
        child->Parent = parent;
    }
}

/// <summary>Тип XML ноды</summary>
public enum XmlNodeType : byte
{
    /// <summary>Элемент ноды</summary>
    ElementNode = 0,

    /// <summary>Тест ноды</summary>
    TextNode = 1
}