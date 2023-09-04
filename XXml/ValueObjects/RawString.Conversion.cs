using System.Buffers;
using System.Buffers.Text;
using System.Runtime.CompilerServices;
using XXml.InternalEntities;

namespace XXml.ValueObjects;

unsafe partial struct RawString
{
    /// <summary>utf-8 bytes of "∞"</summary>
    private static ReadOnlySpan<byte> InfUtf8Str => new byte[] {0xE2, 0x88, 0x9E};

    private const string InvalidFormatMessage = "Invalid format";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ToUpper(Span<byte> buffer)
    {
        if (buffer.Length < Length) ThrowHelper.ThrowArg("buffer is too short.");

        const uint offset = 'z' - (uint) 'a';
        for (var i = 0; i < Length; i++)
            if (At(i) - (uint) 'a' <= offset)
                buffer.At(i) = (byte) (At(i) - 32);
            else
                buffer.At(i) = At(i);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte[] ToUpper()
    {
        if (Length == 0) return Array.Empty<byte>();
        var buf = NewUninitializedByteArrayIfPossible(Length);
        ToUpper(buf);
        return buf;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ToLower(Span<byte> buffer)
    {
        if (buffer.Length < Length) ThrowHelper.ThrowArg("buffer is too short.");

        const uint offset = 'Z' - (uint) 'A';
        for (var i = 0; i < Length; i++)
            if (At(i) - (uint) 'A' <= offset)
                buffer.At(i) = (byte) (At(i) + 32);
            else
                buffer.At(i) = At(i);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte[] ToLower()
    {
        if (Length == 0) return Array.Empty<byte>();
        var buf = NewUninitializedByteArrayIfPossible(Length);
        ToLower(buf);
        return buf;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryToFloat32(out float value)
    {
        if (Utf8Parser.TryParse(AsSpan(), out value, out _)) return true;
        return FloatFallback(AsSpan(), out value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float ToFloat32()
    {
        if (Utf8Parser.TryParse(AsSpan(), out float value, out _)) return value;
        if (FloatFallback(AsSpan(), out value) == false) ThrowHelper.ThrowInvalidOperation(InvalidFormatMessage);
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryToFloat64(out double value)
    {
        if (Utf8Parser.TryParse(AsSpan(), out value, out _)) return true;
        return DoubleFallback(AsSpan(), out value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double ToFloat64()
    {
        if (Utf8Parser.TryParse(AsSpan(), out double value, out _)) return value;
        if (DoubleFallback(AsSpan(), out value) == false) ThrowHelper.ThrowInvalidOperation(InvalidFormatMessage);
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryToInt32(out int value)
    {
        return Utf8Parser.TryParse(AsSpan(), out value, out _);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ToInt32()
    {
        if (Utf8Parser.TryParse(AsSpan(), out int value, out _) == false) ThrowHelper.ThrowInvalidOperation(InvalidFormatMessage);
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryToUInt32(out uint value)
    {
        return Utf8Parser.TryParse(AsSpan(), out value, out _);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint ToUInt32()
    {
        if (Utf8Parser.TryParse(AsSpan(), out uint value, out _) == false) ThrowHelper.ThrowInvalidOperation(InvalidFormatMessage);
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryToInt64(out long value)
    {
        return Utf8Parser.TryParse(AsSpan(), out value, out _);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public long ToInt64()
    {
        if (Utf8Parser.TryParse(AsSpan(), out long value, out _) == false) ThrowHelper.ThrowInvalidOperation(InvalidFormatMessage);
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryToUInt64(out ulong value)
    {
        return Utf8Parser.TryParse(AsSpan(), out value, out _);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong ToUInt64()
    {
        if (Utf8Parser.TryParse(AsSpan(), out ulong value, out _) == false) ThrowHelper.ThrowInvalidOperation(InvalidFormatMessage);
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryToInt16(out short value)
    {
        return Utf8Parser.TryParse(AsSpan(), out value, out _);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short ToInt16()
    {
        if (Utf8Parser.TryParse(AsSpan(), out short value, out _) == false) ThrowHelper.ThrowInvalidOperation(InvalidFormatMessage);
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryToUInt16(out ushort value)
    {
        return Utf8Parser.TryParse(AsSpan(), out value, out _);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ushort ToUInt16()
    {
        if (Utf8Parser.TryParse(AsSpan(), out ushort value, out _) == false) ThrowHelper.ThrowInvalidOperation(InvalidFormatMessage);
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryToInt8(out sbyte value)
    {
        return Utf8Parser.TryParse(AsSpan(), out value, out _);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public sbyte ToInt8()
    {
        if (Utf8Parser.TryParse(AsSpan(), out sbyte value, out _) == false) ThrowHelper.ThrowInvalidOperation(InvalidFormatMessage);
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryToUInt8(out byte value)
    {
        return Utf8Parser.TryParse(AsSpan(), out value, out _);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte ToUInt8()
    {
        if (Utf8Parser.TryParse(AsSpan(), out byte value, out _) == false) ThrowHelper.ThrowInvalidOperation(InvalidFormatMessage);
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SplitRawStrings Split(byte separator)
    {
        return new SplitRawStrings(this, separator);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SplitRawStrings Split(ReadOnlySpan<byte> separator)
    {
        return new SplitRawStrings(this, separator);
    }

    public (RawString, RawString) Split2(byte separator)
    {
        for (var i = 0; i < Length; i++)
            if (((byte*) _ptr)[i] == separator)
            {
                var latterStart = Math.Min(i + 1, Length);
                return (SliceUnsafe(0, i), SliceUnsafe(latterStart, Length - latterStart));
            }

        return (this, Empty);
    }

    public (RawString, RawString) Split2(ReadOnlySpan<byte> separator)
    {
        if (separator.Length > Length) return (this, Empty);
        var maxLoop = Length - separator.Length + 1;
        for (var i = 0; i < maxLoop; i++)
            if (SliceUnsafe(i, separator.Length).SequenceEqual(separator))
            {
                var latterStart = Math.Min(i + separator.Length, Length);
                return (SliceUnsafe(0, i), SliceUnsafe(latterStart, Length - latterStart));
            }

        return (this, Empty);
    }

    public (RawString, RawString) Split2(char separator)
    {
        if (separator < 128)
        {
            return Split2((byte) separator);
        }

        const int charMaxByteCount = 6;
        var buf = stackalloc byte[charMaxByteCount];
        var byteCount = Utf8ExceptionFallbackEncoding.Instance.GetBytes(&separator, 1, buf, charMaxByteCount);
        return Split2(SpanHelper.CreateReadOnlySpan<byte>(buf, byteCount));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (RawString, RawString) Split2(string separator)
    {
        return Split2(separator.AsSpan());
    }

    [SkipLocalsInit]
    private (RawString, RawString) Split2(ReadOnlySpan<char> separator)
    {
        const int charMaxByteCount = 6;
        const int stackBufSize = 128;
        const int thresholdLen = stackBufSize / charMaxByteCount;
        if (separator.Length == 1 && separator[0] < 128)
        {
            return Split2((byte) separator[0]);
        }
        if (separator.Length <= thresholdLen)
        {
            var buf = stackalloc byte[stackBufSize];
            fixed (char* ptr = separator)
            {
                var byteCount = Utf8ExceptionFallbackEncoding.Instance.GetBytes(ptr, separator.Length, buf, stackBufSize);
                return Split2(SpanHelper.CreateReadOnlySpan<byte>(buf, byteCount));
            }
        }
        else
        {
            var utf8 = Utf8ExceptionFallbackEncoding.Instance;
            var byteCount = utf8.GetByteCount(separator);
            var buf = ArrayPool<byte>.Shared.Rent(byteCount);
            try
            {
                fixed (byte* bufPtr = buf)
                fixed (char* ptr = separator)
                {
                    utf8.GetBytes(ptr, separator.Length, bufPtr, byteCount);
                    return Split2(SpanHelper.CreateReadOnlySpan<byte>(bufPtr, byteCount));
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buf);
            }
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool FloatFallback(ReadOnlySpan<byte> span, out float value)
    {
        span = span.Trim();
        if (span.Length == 0)
        {
            value = 0;
            return false;
        }

        if (span.At(0) == '+')
        {
            var slice = span.Slice(1);
            if (IsInfSpan(slice))
            {
                value = float.PositiveInfinity;
                return true;
            }

            if (IsNanSpan(slice))
            {
                value = float.NaN;
                return true;
            }

            value = 0;
            return false;
        }

        if (span.At(0) == '-')
        {
            var slice = span.Slice(1);
            if (IsInfSpan(slice))
            {
                value = float.NegativeInfinity;
                return true;
            }

            if (IsNanSpan(slice))
            {
                value = -float.NaN;
                return true;
            }

            value = 0;
            return false;
        }

        if (IsInfSpan(span))
        {
            value = float.PositiveInfinity;
            return true;
        }

        if (IsNanSpan(span))
        {
            value = float.NaN;
            return true;
        }

        value = 0;
        return false;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool DoubleFallback(ReadOnlySpan<byte> span, out double value)
    {
        span = span.Trim();
        if (span.Length == 0)
        {
            value = 0;
            return false;
        }

        if (span.At(0) == '+')
        {
            var slice = span.Slice(1);
            if (IsInfSpan(slice))
            {
                value = double.PositiveInfinity;
                return true;
            }

            if (IsNanSpan(slice))
            {
                value = double.NaN;
                return true;
            }

            value = 0;
            return false;
        }

        if (span.At(0) == '-')
        {
            var slice = span.Slice(1);
            if (IsInfSpan(slice))
            {
                value = double.NegativeInfinity;
                return true;
            }

            if (IsNanSpan(slice))
            {
                value = -double.NaN;
                return true;
            }

            value = 0;
            return false;
        }

        if (IsInfSpan(span))
        {
            value = double.PositiveInfinity;
            return true;
        }

        if (IsNanSpan(span))
        {
            value = double.NaN;
            return true;
        }

        value = 0;
        return false;
    }

    private static bool IsInfSpan(ReadOnlySpan<byte> span)
    {
        return span.Length == 3 &&
               span.At(0) == InfUtf8Str[0] &&
               span.At(1) == InfUtf8Str[1] &&
               span.At(2) == InfUtf8Str[2];
    }

    private static bool IsNanSpan(ReadOnlySpan<byte> span)
    {
        return span.Length == 3 &&
               (span.At(0) == (byte) 'N' || span.At(0) == (byte) 'n') &&
               (span.At(1) == (byte) 'a' || span.At(1) == (byte) 'A') &&
               (span.At(2) == (byte) 'N' || span.At(2) == (byte) 'n');
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte[] NewUninitializedByteArrayIfPossible(int length)
    {
#if NET5_0_OR_GREATER
        return GC.AllocateUninitializedArray<byte>(length);
#else
            return new byte[length];
#endif
    }
}