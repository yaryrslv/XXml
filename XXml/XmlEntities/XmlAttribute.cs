using System.Buffers;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using XXml.InternalEntities;
using XXml.ValueObjects;

namespace XXml.XmlEntities;

/// <summary>Атрибут ноды в xml</summary>
[DebuggerDisplay("{ToString(),nq}")]
public readonly unsafe struct XmlAttribute : IEquatable<XmlAttribute>, IReference
{
    // Не добавляйте никаких других полей. Шаблон должен быть таким же, как у IntPtr.
    private readonly IntPtr _attr; // XmlAttributeStruct*

    public bool IsNull => _attr == IntPtr.Zero;

    /// <summary>Получение имени атрибута</summary>
    public ref readonly RawString Name => ref ((XmlAttributeStruct*) _attr)->Name;

    /// <summary>Получение значения атрибута</summary>
    public ref readonly RawString Value => ref ((XmlAttributeStruct*) _attr)->Value;

    internal Option<XmlNode> Node => ((XmlAttributeStruct*) _attr)->Node;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal XmlAttribute(XmlAttributeStruct* attr)
    {
        _attr = (IntPtr) attr;
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void Deconstruct(out RawString name, out RawString value)
    {
        name = Name;
        value = Value;
    }

    public bool IsName(ReadOnlySpan<byte> namespaceName, ReadOnlySpan<byte> name)
    {
        if (namespaceName.IsEmpty || name.IsEmpty) return false;
        if (Node.TryGetValue(out var node) == false) return false;
        if (XmlnsHelper.TryResolveNamespaceAlias(namespaceName, node, out var alias) == false) return false;
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

    /// <summary>Получение полного имени атрибута. Возвращает false, если полное имя не может быть зарезовлено.</summary>
    /// <param name="namespaceName">
    ///     имя пространства имен атрибута
    ///     <para />
    ///     ex) "abcde" в случае, если атрибут является a:bar="123" в &lt;node xmlns:a="abcde" a:bar="123" /&gt;
    ///     <para />
    /// </param>
    /// <param name="name">
    ///     локальное имя атрибута
    ///     <para />
    ///     ex) "bar" в случае, когда атрибут является a:bar="123" в &lt;node xmlns:a="abcde" a:bar="123" /&gt;
    ///     <para />
    /// </param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetFullName(out RawString namespaceName, out RawString name)
    {
        return XmlnsHelper.TryGetAttributeFullName(this, out namespaceName, out name);
    }

    /// <summary>
    ///     Получение полного имени атрибута. Метод бросает <see cref="InvalidOperationException" />, если полное имя не
    ///     удалось зарезолвить.
    /// </summary>
    /// <remarks>
    ///     ex) Возвращает ("abcde", "bar") в случае, если атрибутом является a:bar="123" в &lt;node xmlns:a="abcde"
    ///     a:bar="123" /&gt;
    ///     <para />
    /// </remarks>
    /// <exception cref="InvalidOperationException">полное имя не удалось разрешить</exception>
    /// .
    /// <returns>Пара из имени пространства имен и локального имени</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (RawString NamespaceName, RawString Name) GetFullName()
    {
        if (XmlnsHelper.TryGetAttributeFullName(this, out var namespaceName, out var name) == false)
        {
            ThrowNoNamespace();

            static void ThrowNoNamespace()
            {
                throw new InvalidOperationException("Could not resolve the full name of the node.");
            }
        }

        return (namespaceName, name);
    }

    internal RawString AsRawString()
    {
        return _attr == IntPtr.Zero ? RawString.Empty : ((XmlAttributeStruct*) _attr)->AsRawString();
    }

    public override bool Equals(object? obj)
    {
        return obj is XmlAttribute attribute && Equals(attribute);
    }

    public bool Equals(XmlAttribute other)
    {
        return _attr == other._attr;
    }

    public override int GetHashCode()
    {
        return _attr.GetHashCode();
    }

    public override string ToString()
    {
        return _attr == IntPtr.Zero ? "" : ((XmlAttributeStruct*) _attr)->ToString();
    }

    public static implicit operator (RawString Name, RawString Value)(XmlAttribute attr)
    {
        return (attr.Name, attr.Value);
    }

    public static bool operator ==(XmlAttribute attr, ValueTuple<RawString, RawString> pair)
    {
        return attr.Name == pair.Item1 && attr.Value == pair.Item2;
    }

    public static bool operator !=(XmlAttribute attr, ValueTuple<RawString, RawString> pair)
    {
        return !(attr == pair);
    }

    public static bool operator ==(ValueTuple<RawString, RawString> pair, XmlAttribute attr)
    {
        return attr == pair;
    }

    public static bool operator !=(ValueTuple<RawString, RawString> pair, XmlAttribute attr)
    {
        return !(attr == pair);
    }

    public static bool operator ==(XmlAttribute attr, ValueTuple<string, string> pair)
    {
        return attr.Name == pair.Item1 && attr.Value == pair.Item2;
    }

    public static bool operator !=(XmlAttribute attr, ValueTuple<string, string> pair)
    {
        return !(attr == pair);
    }

    public static bool operator ==(ValueTuple<string, string> pair, XmlAttribute attr)
    {
        return attr == pair;
    }

    public static bool operator !=(ValueTuple<string, string> pair, XmlAttribute attr)
    {
        return !(attr == pair);
    }
}

[DebuggerDisplay("{ToString(),nq}")]
internal readonly unsafe struct XmlAttributeStruct
{
    /// <summary>Имя атрибута</summary>
    public readonly RawString Name;

    /// <summary>Значение атрибута</summary>
    public readonly RawString Value;

    public readonly Option<XmlNode> Node;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public XmlAttributeStruct(RawString name, RawString value, XmlNodeStruct* node)
    {
        // [NOTE]
        // 'node' имеет значение null, если атрибут принадлежит xml-объявлению.

        Name = name;
        Value = value;
        Node = new XmlNode(node);
    }

    public RawString AsRawString()
    {
        // <foo name="value" />
        //      |          |
        //      `- head    |
        //                 `- end

        var head = Name.GetPtr();
        var len = Value.GetPtr() + Value.Length + 1 - head;
        return new RawString(head, checked((int) (uint) len));
    }

    public override string ToString()
    {
        return $"{Name.ToString()}=\"{Value.ToString()}\"";
    }
}