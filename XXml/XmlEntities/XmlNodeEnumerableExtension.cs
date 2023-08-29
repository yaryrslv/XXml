using System.Buffers;
using System.Runtime.CompilerServices;
using XXml.Internal;
using XXml.ValueObjects;

namespace XXml.XmlEntities;

/// <summary>Расширения для перечисления <see cref="XmlNode" />.</summary>.
public static class XmlNodeEnumerableExtension
{
    private const string NoMatchingMessage = "Sequence contains no matching elements.";
    
    /// <summary>Найти ноду по имени. Возвращает первый найденный узел.</summary>
    /// <param name="source">список исходных узлов для перечисления</param>
    /// <param name="name">имя узла для поиска</param>
    /// <returns> найденный узел в виде <see cref="Option{T}" /></returns>
    public static Option<XmlNode> FindOrDefault<TNodes>(this TNodes source, ReadOnlySpan<byte> name) where TNodes : IEnumerable<XmlNode>
    {
        foreach (var child in source)
            if (child.Name == name)
                return child;
        return Option<XmlNode>.Null;
    }

    /// <summary>Найти ноду по имени. Возвращает первый найденный узел.</summary>
    /// <param name="source">список исходных узлов для перечисления</param>
    /// <param name="name">имя узла для поиска</param>
    /// <returns> найденный узел в виде <see cref="Option{T}" /></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<XmlNode> FindOrDefault<TNodes>(this TNodes source, RawString name) where TNodes : IEnumerable<XmlNode>
    {
        return FindOrDefault(source, name.AsSpan());
    }

    /// <summary>Найти ноду по имени. Возвращает первый найденный узел.</summary>
    /// <param name="source">список исходных узлов для перечисления</param>
    /// <param name="name">имя узла для поиска</param>
    /// <returns> найденный узел в виде <see cref="Option{T}" /></returns>
    public static unsafe Option<XmlNode> FindOrDefault<TNodes>(this TNodes source, ReadOnlySpan<char> name) where TNodes : IEnumerable<XmlNode>
    {
        if (name.IsEmpty) return Option<XmlNode>.Null;

        var utf8 = Utf8ExceptionFallbackEncoding.Instance;
        var byteLen = utf8.GetByteCount(name);

        const int threshold = 128;
        if (byteLen <= threshold)
        {
            var buf = stackalloc byte[threshold];
            fixed (char* ptr = name)
            {
                utf8.GetBytes(ptr, name.Length, buf, byteLen);
            }

            var span = SpanHelper.CreateReadOnlySpan<byte>(buf, byteLen);
            return FindOrDefault(source, span);
        }

        var rentArray = ArrayPool<byte>.Shared.Rent(byteLen);
        try
        {
            fixed (byte* buf = rentArray)
            fixed (char* ptr = name)
            {
                utf8.GetBytes(ptr, name.Length, buf, byteLen);
                var span = SpanHelper.CreateReadOnlySpan<byte>(buf, byteLen);
                return FindOrDefault(source, span);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rentArray);
        }
    }

    /// <summary>Найти ноду по имени. Возвращает первый найденный узел.</summary>
    /// <param name="source">список исходных узлов для перечисления</param>
    /// <param name="name">имя узла для поиска</param>
    /// <returns> найденный узел в виде <see cref="Option{T}" /></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<XmlNode> FindOrDefault<TNodes>(this TNodes source, string name) where TNodes : IEnumerable<XmlNode>
    {
        return FindOrDefault(source, name.AsSpan());
    }

    public static Option<XmlNode> FindOrDefault<TNodes>(this TNodes source, ReadOnlySpan<byte> namespaceName, ReadOnlySpan<byte> name) where TNodes : IEnumerable<XmlNode>
    {
        foreach (var child in source)
        {
            var childName = child.Name;
            if (childName.EndsWith(name))
                if (XmlnsHelper.TryResolveNamespaceAlias(namespaceName, child, out var nsAlias))
                {
                    if (nsAlias.IsEmpty)
                    {
                        if (childName.Length == name.Length) return child;
                    }
                    else
                    {
                        if (childName.Length == nsAlias.Length + 1 + name.Length && childName.At(nsAlias.Length) == (byte) ':') return child;
                    }
                }
        }

        return Option<XmlNode>.Null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<XmlNode> FindOrDefault<TNodes>(this TNodes source, ReadOnlySpan<byte> namespaceName, RawString name) where TNodes : IEnumerable<XmlNode>
    {
        return FindOrDefault(source, namespaceName, name.AsSpan());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<XmlNode> FindOrDefault<TNodes>(this TNodes source, RawString namespaceName, ReadOnlySpan<byte> name) where TNodes : IEnumerable<XmlNode>
    {
        return FindOrDefault(source, namespaceName.AsSpan(), name);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<XmlNode> FindOrDefault<TNodes>(this TNodes source, RawString namespaceName, RawString name) where TNodes : IEnumerable<XmlNode>
    {
        return FindOrDefault(source, namespaceName.AsSpan(), name.AsSpan());
    }

    [SkipLocalsInit]
    public static unsafe Option<XmlNode> FindOrDefault<TNodes>(this TNodes source, ReadOnlySpan<char> namespaceName, ReadOnlySpan<char> name) where TNodes : IEnumerable<XmlNode>
    {
        if (namespaceName.IsEmpty || name.IsEmpty) return Option<XmlNode>.Null;

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
            return FindOrDefault(source, nsNameUtf8, nameUtf8);
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
                return FindOrDefault(source, nsNameUtf8, nameUtf8);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rentArray);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<XmlNode> FindOrDefault<TNodes>(this TNodes source, ReadOnlySpan<char> namespaceName, string name) where TNodes : IEnumerable<XmlNode>
    {
        return FindOrDefault(source, namespaceName, name.AsSpan());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<XmlNode> FindOrDefault<TNodes>(this TNodes source, string namespaceName, ReadOnlySpan<char> name) where TNodes : IEnumerable<XmlNode>
    {
        return FindOrDefault(source, namespaceName.AsSpan(), name);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<XmlNode> FindOrDefault<TNodes>(this TNodes source, string namespaceName, string name) where TNodes : IEnumerable<XmlNode>
    {
        return FindOrDefault(source, namespaceName.AsSpan(), name.AsSpan());
    }

    /// <summary>
    /// Находит ноду по имени. Возвращает первый найденный узел или бросает <see cref="InvalidOperationException" />, если он не найден.
    /// </summary>
    /// <param name="source">список исходных узлов для перечисления</param>
    /// <param name="name">имя узла для поиска</param>
    /// <returns>a found node</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XmlNode Find<TNodes>(this TNodes source, ReadOnlySpan<byte> name) where TNodes : IEnumerable<XmlNode>
    {
        if (FindOrDefault(source, name).TryGetValue(out var node) == false) ThrowHelper.ThrowInvalidOperation(NoMatchingMessage);
        return node;
    }

    /// <summary>
    /// Находит ноду по имени. Возвращает первый найденный узел или бросает <see cref="InvalidOperationException" />, если он не найден.
    /// </summary>
    /// <param name="source">список исходных узлов для перечисления</param>
    /// <param name="name">имя узла для поиска</param>
    /// <returns>a found node</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XmlNode Find<TNodes>(this TNodes source, RawString name) where TNodes : IEnumerable<XmlNode>
    {
        if (FindOrDefault(source, name).TryGetValue(out var node) == false) ThrowHelper.ThrowInvalidOperation(NoMatchingMessage);
        return node;
    }

    /// <summary>
    /// Находит ноду по имени. Возвращает первый найденный узел или бросает <see cref="InvalidOperationException" />, если он не найден.
    /// </summary>
    /// <param name="source">список исходных узлов для перечисления</param>
    /// <param name="name">имя узла для поиска</param>
    /// <returns>a found node</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XmlNode Find<TNodes>(this TNodes source, ReadOnlySpan<char> name) where TNodes : IEnumerable<XmlNode>
    {
        if (FindOrDefault(source, name).TryGetValue(out var node) == false) ThrowHelper.ThrowInvalidOperation(NoMatchingMessage);
        return node;
    }

    /// <summary>
    /// Находит ноду по имени. Возвращает первый найденный узел или бросает <see cref="InvalidOperationException" />, если он не найден.
    /// </summary>
    /// <param name="source">список исходных узлов для перечисления</param>
    /// <param name="name">имя узла для поиска</param>
    /// <returns>a found node</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XmlNode Find<TNodes>(this TNodes source, string name) where TNodes : IEnumerable<XmlNode>
    {
        if (FindOrDefault(source, name).TryGetValue(out var node) == false) ThrowHelper.ThrowInvalidOperation(NoMatchingMessage);
        return node;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XmlNode Find<TNodes>(this TNodes source, ReadOnlySpan<byte> namespaceName, ReadOnlySpan<byte> name) where TNodes : IEnumerable<XmlNode>
    {
        if (FindOrDefault(source, namespaceName, name).TryGetValue(out var node) == false) ThrowHelper.ThrowInvalidOperation(NoMatchingMessage);
        return node;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XmlNode Find<TNodes>(this TNodes source, ReadOnlySpan<byte> namespaceName, RawString name) where TNodes : IEnumerable<XmlNode>
    {
        if (FindOrDefault(source, namespaceName, name).TryGetValue(out var node) == false) ThrowHelper.ThrowInvalidOperation(NoMatchingMessage);
        return node;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XmlNode Find<TNodes>(this TNodes source, RawString namespaceName, ReadOnlySpan<byte> name) where TNodes : IEnumerable<XmlNode>
    {
        if (FindOrDefault(source, namespaceName, name).TryGetValue(out var node) == false) ThrowHelper.ThrowInvalidOperation(NoMatchingMessage);
        return node;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XmlNode Find<TNodes>(this TNodes source, RawString namespaceName, RawString name) where TNodes : IEnumerable<XmlNode>
    {
        if (FindOrDefault(source, namespaceName, name).TryGetValue(out var node) == false) ThrowHelper.ThrowInvalidOperation(NoMatchingMessage);
        return node;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XmlNode Find<TNodes>(this TNodes source, ReadOnlySpan<char> namespaceName, ReadOnlySpan<char> name) where TNodes : IEnumerable<XmlNode>
    {
        if (FindOrDefault(source, namespaceName, name).TryGetValue(out var node) == false) ThrowHelper.ThrowInvalidOperation(NoMatchingMessage);
        return node;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XmlNode Find<TNodes>(this TNodes source, ReadOnlySpan<char> namespaceName, string name) where TNodes : IEnumerable<XmlNode>
    {
        if (FindOrDefault(source, namespaceName, name).TryGetValue(out var node) == false) ThrowHelper.ThrowInvalidOperation(NoMatchingMessage);
        return node;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XmlNode Find<TNodes>(this TNodes source, string namespaceName, ReadOnlySpan<char> name) where TNodes : IEnumerable<XmlNode>
    {
        if (FindOrDefault(source, namespaceName, name).TryGetValue(out var node) == false) ThrowHelper.ThrowInvalidOperation(NoMatchingMessage);
        return node;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XmlNode Find<TNodes>(this TNodes source, string namespaceName, string name) where TNodes : IEnumerable<XmlNode>
    {
        if (FindOrDefault(source, namespaceName, name).TryGetValue(out var node) == false) ThrowHelper.ThrowInvalidOperation(NoMatchingMessage);
        return node;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryFind<TNodes>(this TNodes source, ReadOnlySpan<byte> name, out XmlNode node) where TNodes : IEnumerable<XmlNode>
    {
        return FindOrDefault(source, name).TryGetValue(out node);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryFind<TNodes>(this TNodes source, RawString name, out XmlNode node) where TNodes : IEnumerable<XmlNode>
    {
        return FindOrDefault(source, name).TryGetValue(out node);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryFind<TNodes>(this TNodes source, ReadOnlySpan<char> name, out XmlNode node) where TNodes : IEnumerable<XmlNode>
    {
        return FindOrDefault(source, name).TryGetValue(out node);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryFind<TNodes>(this TNodes source, string name, out XmlNode node) where TNodes : IEnumerable<XmlNode>
    {
        return FindOrDefault(source, name).TryGetValue(out node);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryFind<TNodes>(this TNodes source, ReadOnlySpan<byte> namespaceName, ReadOnlySpan<byte> name, out XmlNode node) where TNodes : IEnumerable<XmlNode>
    {
        return FindOrDefault(source, namespaceName, name).TryGetValue(out node);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryFind<TNodes>(this TNodes source, ReadOnlySpan<byte> namespaceName, RawString name, out XmlNode node) where TNodes : IEnumerable<XmlNode>
    {
        return FindOrDefault(source, namespaceName, name).TryGetValue(out node);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryFind<TNodes>(this TNodes source, RawString namespaceName, ReadOnlySpan<byte> name, out XmlNode node) where TNodes : IEnumerable<XmlNode>
    {
        return FindOrDefault(source, namespaceName, name).TryGetValue(out node);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryFind<TNodes>(this TNodes source, RawString namespaceName, RawString name, out XmlNode node) where TNodes : IEnumerable<XmlNode>
    {
        return FindOrDefault(source, namespaceName, name).TryGetValue(out node);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryFind<TNodes>(this TNodes source, ReadOnlySpan<char> namespaceName, ReadOnlySpan<char> name, out XmlNode node) where TNodes : IEnumerable<XmlNode>
    {
        return FindOrDefault(source, namespaceName, name).TryGetValue(out node);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryFind<TNodes>(this TNodes source, ReadOnlySpan<char> namespaceName, string name, out XmlNode node) where TNodes : IEnumerable<XmlNode>
    {
        return FindOrDefault(source, namespaceName, name).TryGetValue(out node);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryFind<TNodes>(this TNodes source, string namespaceName, ReadOnlySpan<char> name, out XmlNode node) where TNodes : IEnumerable<XmlNode>
    {
        return FindOrDefault(source, namespaceName, name).TryGetValue(out node);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryFind<TNodes>(this TNodes source, string namespaceName, string name, out XmlNode node) where TNodes : IEnumerable<XmlNode>
    {
        return FindOrDefault(source, namespaceName, name).TryGetValue(out node);
    }
}