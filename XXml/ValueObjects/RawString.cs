using System.Buffers;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using XXml.Internal;

namespace XXml.ValueObjects;

/// <summary>
/// Предоставляет необработанный байтовый массив utf8, который совместим <see cref="ReadOnlySpan{T}" /> из <see langword="byte" />
/// </summary>
[DebuggerTypeProxy(typeof(RawStringDebuggerTypeProxy))]
[DebuggerDisplay("{ToString()}")]
public readonly unsafe partial struct RawString : IEquatable<RawString>
{
    private readonly IntPtr _ptr;

    /// <summary>Получение пустого экземпляра <see cref="RawString" />.</summary>
    public static RawString Empty => default;

    /// <summary>Получение информации о том, является ли массив байтов пустым или нет.</summary>
    public bool IsEmpty => Length == 0;

    /// <summary>Получение длины байтового массива. (НЕ количество символов)</summary>
    public int Length { get; }

    /// <summary>Получение указателя на начлао набора символов utf-8.</summary>.
    public IntPtr Ptr => _ptr;

    /// <summary>Получение или установка элемента с указанным индексом</summary>
    /// <param name="index">индекс элемента</param>
    /// <returns>the element</returns>
    public ref readonly byte this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if ((uint) index >= (uint) Length) ThrowHelper.ThrowArgOutOfRange(nameof(index));
            return ref ((byte*) _ptr)[index];
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal RawString(byte* ptr, int length)
    {
        Debug.Assert(length >= 0);
        _ptr = (IntPtr) ptr;
        Length = length;
    }

    /// <summary>Получение количества символов</summary>
    /// <returns>characters count</returns>
    public int GetCharCount()
    {
        return Length == 0 ? 0 : Utf8ExceptionFallbackEncoding.Instance.GetCharCount((byte*) _ptr, Length);
    }

    /// <summary>Получение данных байтов только для чтения</summary>.
    /// <returns><see cref="ReadOnlySpan{T}" /> of type <see langword="byte" /></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<byte> AsSpan()
    {
        return SpanHelper.CreateReadOnlySpan<byte>(_ptr.ToPointer(), Length);
    }

    /// <summary>Копирование байтов в новый массив байтов.</summary>
    /// <returns>new array</returns>
    public byte[] ToArray()
    {
        return AsSpan().ToArray();
    }

    /// <summary>Получение фрагмента <see cref="RawString" /></summary>.
    /// <param name="start">начальный индекс фрагмента</param>
    /// <returns>sliced <see cref="RawString" /></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RawString Slice(int start)
    {
        if ((uint) start > (uint) Length) ThrowHelper.ThrowArgOutOfRange(nameof(start));
        return new RawString((byte*) _ptr + start, Length - start);
    }

    /// <summary>Получение фрагмента <see cref="RawString" /></summary>.
    /// <param name="start">начальный индекс фрагмента</param>
    /// <param name="length">длина фрагмента от <paramref name="start" /></param>.
    /// <returns>sliced <see cref="RawString" /></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RawString Slice(int start, int length)
    {
        if ((uint) start > (uint) Length) ThrowHelper.ThrowArgOutOfRange(nameof(start));
        if ((uint) length > (uint) (Length - start)) ThrowHelper.ThrowArgOutOfRange(nameof(length));
        return new RawString((byte*) _ptr + start, length);
    }

    /// <summary>Получение фрагмента <see cref="RawString" /></summary>.
    /// <param name="range">диапазон данных</param>
    /// <returns>sliced <see cref="RawString" /></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RawString Slice(DataRange range)
    {
        return Slice(range.Start, range.Length);
    }

    /// <summary>Обработка невидимых символов. (пробельные символы, '\t', '\r' и '\n')</summary>
    /// <returns>trimmed string</returns>
    public RawString Trim()
    {
        return TrimStart().TrimEnd();
    }

    /// <summary>Обработка невидимых символов начала. (пробельные символы, '\t', '\r' и '\n')</summary>
    /// <returns>trimmed string</returns>
    public RawString TrimStart()
    {
        for (var i = 0; i < Length; i++)
        {
            ref var p = ref ((byte*) _ptr)[i];
            if (p != ' ' && p != '\t' && p != '\r' && p != '\n') return SliceUnsafe(i, Length - i);
        }

        return Empty;
    }

    /// <summary>Обработка невидимых символов конца. (пробельные символы, '\t', '\r' и '\n')</summary>
    /// <returns>trimmed string</returns>
    public RawString TrimEnd()
    {
        for (var i = Length - 1; i >= 0; i--)
        {
            ref var p = ref ((byte*) _ptr)[i];
            if (p != ' ' && p != '\t' && p != '\r' && p != '\n') return SliceUnsafe(0, i + 1);
        }

        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal byte* GetPtr()
    {
        return (byte*) _ptr;
    }

    /// <summary>Получение или установка элемента с указанным индексом.</summary>
    /// <remarks>[ВНИМАНИЕ] Данный метод не проверяет границу индекса!</remarks>
    /// <param name="index">индекс элемента</param>
    /// <returns>reference to the item</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ref byte At(int index)
    {
        // This method is same as this[index], but no boundary check.
#if DEBUG
        if ((uint) index >= (uint) Length) ThrowHelper.ThrowArgOutOfRange(nameof(index));
#endif
        return ref ((byte*) _ptr)[index];
    }

    /// <summary>Получение фрагмента массива</summary>
    /// <remarks>[ВНИМАНИЕ] Граница не проверяется. Будьте осторожны !</remarks>
    /// <param name="start">начальный индекс фрагмента</param>
    /// <param name="length">длина отрезка от <paramref name="start" /></param>.
    /// <returns>sliced array</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal RawString SliceUnsafe(int start, int length)
    {
#if DEBUG
        if ((uint) start > (uint) Length) ThrowHelper.ThrowArgOutOfRange(nameof(start));
        if ((uint) length > (uint) (Length - start)) ThrowHelper.ThrowArgOutOfRange(nameof(length));
#endif
        return new RawString((byte*) _ptr + start, length);
    }

    /// <summary>Получение ссылки на pinnnable.</summary>
    /// <returns>reference to the head of the data</returns>
    [EditorBrowsable(EditorBrowsableState.Never)] // Only for 'fixed' statement
    public ref readonly byte GetPinnableReference()
    {
        return ref Unsafe.AsRef<byte>((void*) _ptr);
    }

    /// <summary>Декодирование байтового массива в формат utf-8 и получение <see langword="string" /></summary>.
    /// <returns>decoded string</returns>
    public override string ToString()
    {
        return IsEmpty ? "" : Utf8ExceptionFallbackEncoding.Instance.GetString((byte*) _ptr, Length);
    }

    public override bool Equals(object? obj)
    {
        return obj is RawString array && Equals(array);
    }

    public bool Equals(RawString other)
    {
        return (_ptr == other._ptr && Length == other.Length) || SequenceEqual(other);
    }

    public bool SequenceEqual(RawString other)
    {
        return AsSpan().SequenceEqual(other.AsSpan());
    }

    public bool SequenceEqual(ReadOnlySpan<byte> other)
    {
        return AsSpan().SequenceEqual(other);
    }

    public bool ReferenceEquals(RawString other)
    {
        return _ptr.Equals(other.Ptr) && Length == other.Length;
    }

    public bool StartsWith(RawString other)
    {
        return AsSpan().StartsWith(other.AsSpan());
    }

    public bool StartsWith(ReadOnlySpan<byte> other)
    {
        return AsSpan().StartsWith(other);
    }

    public bool StartsWith(string other)
    {
        return StartsWith(other.AsSpan());
    }

    [SkipLocalsInit]
    public bool StartsWith(ReadOnlySpan<char> other)
    {
        if (other.Length == 0) return true;
        var utf8 = Utf8ExceptionFallbackEncoding.Instance;
        var byteLen = utf8.GetByteCount(other);
        if (byteLen > Length) return false;

        const int Threshold = 128;
        if (byteLen <= Threshold)
        {
            var buf = stackalloc byte[Threshold];
            fixed (char* ptr = other)
            {
                utf8.GetBytes(ptr, other.Length, buf, byteLen);
            }

            var span = SpanHelper.CreateReadOnlySpan<byte>(buf, byteLen);
            return AsSpan().StartsWith(span);
        }

        var rentArray = ArrayPool<byte>.Shared.Rent(byteLen);
        try
        {
            fixed (byte* buf = rentArray)
            fixed (char* ptr = other)
            {
                utf8.GetBytes(ptr, other.Length, buf, byteLen);
                var span = SpanHelper.CreateReadOnlySpan<byte>(buf, byteLen);
                return AsSpan().StartsWith(span);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rentArray);
        }
    }

    public bool EndsWith(RawString other)
    {
        return AsSpan().EndsWith(other.AsSpan());
    }

    public bool EndsWith(ReadOnlySpan<byte> other)
    {
        return AsSpan().EndsWith(other);
    }

    public bool EndsWith(string other)
    {
        return EndsWith(other.AsSpan());
    }

    [SkipLocalsInit]
    public bool EndsWith(ReadOnlySpan<char> other)
    {
        if (other.Length == 0) return true;
        var utf8 = Utf8ExceptionFallbackEncoding.Instance;
        var byteLen = utf8.GetByteCount(other);
        if (byteLen > Length) return false;

        const int Threshold = 128;
        if (byteLen <= Threshold)
        {
            var buf = stackalloc byte[Threshold];
            fixed (char* ptr = other)
            {
                utf8.GetBytes(ptr, other.Length, buf, byteLen);
            }

            var span = SpanHelper.CreateReadOnlySpan<byte>(buf, byteLen);
            return AsSpan().EndsWith(span);
        }

        var rentArray = ArrayPool<byte>.Shared.Rent(byteLen);
        try
        {
            fixed (byte* buf = rentArray)
            fixed (char* ptr = other)
            {
                utf8.GetBytes(ptr, other.Length, buf, byteLen);
                var span = SpanHelper.CreateReadOnlySpan<byte>(buf, byteLen);
                return AsSpan().EndsWith(span);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rentArray);
        }
    }


    public int IndexOf(byte value)
    {
        var span = AsSpan();
        for (var i = 0; i < span.Length; i++)
            if (span[i] == value)
                return i;
        return -1;
    }

    public DataRange RangeOf(char value)
    {
        if (value < 128)
        {
            // For ASCII
            var index = IndexOf((byte) value);
            return new DataRange(index, index >= 0 ? 1 : 0);
        }

        var utf8 = Utf8ExceptionFallbackEncoding.Instance;
        var buf = stackalloc byte[8];
        var len = utf8.GetBytes(&value, 1, buf, 8);
        return RangeOf(SpanHelper.CreateReadOnlySpan<byte>(buf, len));
    }

    public DataRange RangeOf(RawString value)
    {
        return RangeOf(value.AsSpan());
    }

    public DataRange RangeOf(ReadOnlySpan<byte> value)
    {
        if (value.Length == 0) return new DataRange(0, 0);

        var l = Length + 1 - value.Length;
        var span = AsSpan();
        for (var i = 0; i < l; i++)
            if (span.SliceUnsafe(i, span.Length - i).StartsWith(value))
                return new DataRange(i, value.Length);
        return new DataRange(-1, 0);
    }

    public DataRange RangeOf(string value)
    {
        return RangeOf(value.AsSpan());
    }

    public DataRange RangeOf(ReadOnlySpan<char> value)
    {
        if (value.Length == 0) return new DataRange(0, 0);
        var utf8 = Utf8ExceptionFallbackEncoding.Instance;
        var byteLen = utf8.GetByteCount(value);
        if (byteLen > Length) return new DataRange(-1, 0);

        const int Threshold = 128;
        if (byteLen <= Threshold)
        {
            var buf = stackalloc byte[Threshold];
            fixed (char* ptr = value)
            {
                utf8.GetBytes(ptr, value.Length, buf, byteLen);
            }

            var span = SpanHelper.CreateReadOnlySpan<byte>(buf, byteLen);
            return RangeOf(span);
        }

        var rentArray = ArrayPool<byte>.Shared.Rent(byteLen);
        try
        {
            fixed (byte* buf = rentArray)
            fixed (char* ptr = value)
            {
                utf8.GetBytes(ptr, value.Length, buf, byteLen);
                var span = SpanHelper.CreateReadOnlySpan<byte>(buf, byteLen);
                return RangeOf(span);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rentArray);
        }
    }


    public int LastIndexOf(byte value)
    {
        var span = AsSpan();
        for (var i = span.Length - 1; i >= 0; i--)
            if (span.At(i) == value)
                return i;
        return -1;
    }

    public DataRange LastRangeOf(char value)
    {
        if (value < 128)
        {
            // For ASCII
            var index = LastIndexOf((byte) value);
            return new DataRange(index, index >= 0 ? 1 : 0);
        }

        var utf8 = Utf8ExceptionFallbackEncoding.Instance;
        var buf = stackalloc byte[8];
        var len = utf8.GetBytes(&value, 1, buf, 8);
        return LastRangeOf(SpanHelper.CreateReadOnlySpan<byte>(buf, len));
    }

    public DataRange LastRangeOf(RawString value)
    {
        return LastRangeOf(value.AsSpan());
    }

    public DataRange LastRangeOf(ReadOnlySpan<byte> value)
    {
        if (value.Length == 0) return new DataRange(0, 0);

        var l = Length + 1 - value.Length;
        var span = AsSpan();
        for (var i = l - 1; i >= 0; i--)
            if (span.SliceUnsafe(i, span.Length - i).StartsWith(value))
                return new DataRange(i, value.Length);
        return new DataRange(-1, 0);
    }

    public DataRange LastRangeOf(string value)
    {
        return LastRangeOf(value.AsSpan());
    }

    public DataRange LastRangeOf(ReadOnlySpan<char> value)
    {
        if (value.Length == 0) return new DataRange(0, 0);
        var utf8 = Utf8ExceptionFallbackEncoding.Instance;
        var byteLen = utf8.GetByteCount(value);
        if (byteLen > Length) return new DataRange(-1, 0);

        const int Threshold = 128;
        if (byteLen <= Threshold)
        {
            var buf = stackalloc byte[Threshold];
            fixed (char* ptr = value)
            {
                utf8.GetBytes(ptr, value.Length, buf, byteLen);
            }

            var span = SpanHelper.CreateReadOnlySpan<byte>(buf, byteLen);
            return LastRangeOf(span);
        }

        var rentArray = ArrayPool<byte>.Shared.Rent(byteLen);
        try
        {
            fixed (byte* buf = rentArray)
            fixed (char* ptr = value)
            {
                utf8.GetBytes(ptr, value.Length, buf, byteLen);
                var span = SpanHelper.CreateReadOnlySpan<byte>(buf, byteLen);
                return LastRangeOf(span);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rentArray);
        }
    }


    public bool Contains(byte value)
    {
        return IndexOf(value) >= 0;
    }

    public bool Contains(char value)
    {
        return RangeOf(value).Start >= 0;
    }

    public bool Contains(RawString value)
    {
        return RangeOf(value).Start >= 0;
    }

    public bool Contains(ReadOnlySpan<byte> value)
    {
        return RangeOf(value).Start >= 0;
    }

    public bool Contains(string value)
    {
        return RangeOf(value).Start >= 0;
    }

    public bool Contains(ReadOnlySpan<char> value)
    {
        return RangeOf(value).Start >= 0;
    }


    /// <summary>Вычисление хэш-кода для указанного span по тому же алгоритму, что и <see cref="GetHashCode()" />.</summary>.
    /// <param name="utf8String">span для вычисления хэш-кода</param>
    /// <returns>hash code</returns>.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetHashCode(ReadOnlySpan<byte> utf8String)
    {
        fixed (byte* ptr = utf8String)
        {
            return GetHashCode(ptr, utf8String.Length);
        }
    }

    /// <summary>Вычисление хэш-кода для указанного span по тому же алгоритму, что и <see cref="GetHashCode()" />.</summary>.
    /// <param name="ptr">указатель на байтовую головку span</param>
    /// <param name="length">длина байтового отрезка</param>
    /// <returns>hash code</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetHashCode(IntPtr ptr, int length)
    {
        return GetHashCode((byte*) ptr, length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int GetHashCode(byte* ptr, int length)
    {
        // Возвращает хэш, вычисленный по тому же алгоритму, что и RawString.
        // Этот метод используется в RawStringTable

        return XxHash32.ComputeHash(ptr, length);
    }

    public override int GetHashCode()
    {
        return GetHashCode((byte*) _ptr, Length);
    }

    public static bool operator ==(RawString left, RawString right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(RawString left, RawString right)
    {
        return !(left == right);
    }

    public static bool operator ==(RawString left, ReadOnlySpan<byte> right)
    {
        return left.SequenceEqual(right);
    }

    public static bool operator !=(RawString left, ReadOnlySpan<byte> right)
    {
        return !(left == right);
    }

    public static bool operator ==(ReadOnlySpan<byte> left, RawString right)
    {
        return right == left;
    }

    public static bool operator !=(ReadOnlySpan<byte> left, RawString right)
    {
        return !(left == right);
    }

    [SkipLocalsInit]
    public static bool operator ==(RawString left, ReadOnlySpan<char> right)
    {
        if (right.IsEmpty) return left.IsEmpty;
        var utf8 = Utf8ExceptionFallbackEncoding.Instance;
        var byteLen = utf8.GetByteCount(right);
        if (byteLen != left.Length) return false;

        const int Threshold = 128;
        if (byteLen <= Threshold)
        {
            var buf = stackalloc byte[Threshold];
            fixed (char* ptr = right)
            {
                utf8.GetBytes(ptr, right.Length, buf, byteLen);
            }

            return SpanHelper.CreateReadOnlySpan<byte>(buf, byteLen).SequenceEqual(left.AsSpan());
        }

        var rentArray = ArrayPool<byte>.Shared.Rent(byteLen);
        try
        {
            fixed (byte* buf = rentArray)
            fixed (char* ptr = right)
            {
                utf8.GetBytes(ptr, right.Length, buf, byteLen);
                return SpanHelper.CreateReadOnlySpan<byte>(buf, byteLen).SequenceEqual(left.AsSpan());
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rentArray);
        }
    }

    public static bool operator !=(RawString left, ReadOnlySpan<char> right)
    {
        return !(left == right);
    }

    public static bool operator ==(ReadOnlySpan<char> left, RawString right)
    {
        return right == left;
    }

    public static bool operator !=(ReadOnlySpan<char> left, RawString right)
    {
        return !(left == right);
    }

    public static bool operator ==(RawString left, string right)
    {
        return left == right.AsSpan();
    }

    public static bool operator !=(RawString left, string right)
    {
        return !(left == right);
    }

    public static bool operator ==(string left, RawString right)
    {
        return right == left;
    }

    public static bool operator !=(string left, RawString right)
    {
        return !(right == left);
    }

    //public static implicit operator ReadOnlySpan<byte>(RawString rawString) => rawString.AsSpan();
}

internal sealed class RawStringDebuggerTypeProxy
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly RawString _entity;

    public RawStringDebuggerTypeProxy(RawString entity)
    {
        _entity = entity;
    }

    public byte[] ByteArray => _entity.ToArray();
    public int ByteLength => _entity.Length;
    public int CharCount => _entity.GetCharCount();

    public string[] Lines
    {
        get
        {
            var lines = new List<string>();
            foreach (var line in _entity.Split((byte) '\n'))
                if (line.Length > 0 && line[line.Length - 1] == '\r')
                    lines.Add(line.Slice(0, line.Length - 1).ToString());
                else
                    lines.Add(line.ToString());
            return lines.ToArray();
        }
    }

    public string String => _entity.ToString();
}