using System.Buffers;
using System.Buffers.Text;
using System.Runtime.CompilerServices;
using XXml.Internal;
using XXml.ValueObjects;

namespace XXml.XmlEntities;

public readonly struct XmlEntityTable
{
    private readonly Option<RawStringTable> _rawStringTable;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal XmlEntityTable(RawStringTable rawStringTable)
    {
        _rawStringTable = rawStringTable;
    }

    /// <summary>Проверка необходимости разрешения входной строки.</summary>
    /// <param name="str">строка для проверки</param>
    /// <param name="requiredBufferLength">длина байта, необходимая для разрешения</param>.
    /// <returns>состояние чекера</returns>
    public unsafe XmlEntityResolverState CheckNeedToResolve(ReadOnlySpan<byte> str, out int requiredBufferLength)
    {
        fixed (byte* p = str)
        {
            // Безопасно создать RawString из ReadOnlySpan<byte>
            // Поскольку метод нигде не хранит экземпляр RawString.
            return CheckNeedToResolve(new RawString(p, str.Length), out requiredBufferLength);
        }
    }

    /// <summary>Проверка необходимости разрешения входной строки.</summary>
    /// <param name="str">строка для проверки</param>
    /// <param name="requiredBufferLength">длина байта, необходимая для разрешения</param>.
    /// <returns>состояние чекера</returns>
    [SkipLocalsInit]
    public unsafe XmlEntityResolverState CheckNeedToResolve(RawString str, out int requiredBufferLength)
    {
        const int exBufLen = 5; // Символ юникода может иметь размер до 5 байт.
        var exBuf = stackalloc byte[exBufLen];

        var len = str.Length;
        var pos = -1;
        var needToResolve = false;

        for (var i = 0; i < str.Length; i++)
        {
            var c = str.At(i);
            if (c == '&')
            {
                if (pos >= 0) goto CanNotResolve;
                needToResolve = true;
                pos = i + 1;
                continue;
            }

            if (c == ';')
            {
                if (pos < 0) goto CanNotResolve;
                var alias = str.SliceUnsafe(pos, i - pos);
                if (TryGetValue(alias, out var value) == false)
                {
                    var tmp = SpanHelper.CreateSpan<byte>(exBuf, exBufLen);
                    if (TryUnicodePointToUtf8(alias, tmp, out var byteLen) == false) goto CanNotResolve;
                    value = tmp.Slice(0, byteLen);
                }
                else
                {
                    // Значение entity может содержать другой псевдоним enity.
                    var recursiveResolveNeeded = CheckNeedToResolve(value, out var l);
                    if (recursiveResolveNeeded == XmlEntityResolverState.CannotResolve)
                        goto CanNotResolve;
                    if (recursiveResolveNeeded == XmlEntityResolverState.NeedToResolve) len = len - value.Length + l;
                }

                len = len - 2 - alias.Length + value.Length;
                pos = -1;
            }
        }

        if (pos >= 0) goto CanNotResolve;

        requiredBufferLength = len;
        return needToResolve ? XmlEntityResolverState.NeedToResolve : XmlEntityResolverState.NoNeeded;

        CanNotResolve:
        {
            requiredBufferLength = 0;
            return XmlEntityResolverState.CannotResolve;
        }
    }

    /// <summary>Получение длины байта буфера, которая необходима резольверу для разрешения строки.</summary>
    /// <param name="str">строка для проверки</param>
    /// <returns>byte length of a buffer</returns>
    public unsafe int GetResolvedByteLength(ReadOnlySpan<byte> str)
    {
        fixed (byte* p = str)
        {
            // Безопасно создать RawString из ReadOnlySpan<byte>
            // Поскольку метод нигде не хранит экземпляр RawString.
            return GetResolvedByteLength(new RawString(p, str.Length));
        }
    }

    /// <summary>Получение длины байта буфера, которая необходима резолверу для резолва строки.</summary>
    /// <param name="str">строка для проверки</param>
    /// <returns>byte length of a buffer</returns>
    public int GetResolvedByteLength(RawString str)
    {
        var state = CheckNeedToResolve(str, out var requiredBufLen);
        if (state == XmlEntityResolverState.CannotResolve) throw new ArgumentException("Could not resolve the input string.");
        return requiredBufLen;
    }

    /// <summary>Преобразование входной utf-8-строки в <see langword="string" /></summary>.
    /// <param name="str">строка формата utf-8 для преобразования</param>
    /// <returns>resolved <see langword="string" /></returns>
    public unsafe string ResolveToString(ReadOnlySpan<byte> str)
    {
        fixed (byte* p = str)
        {
            // Безопасно создать RawString из ReadOnlySpan<byte>
            // Поскольку метод нигде не хранит экземпляр RawString.
            return ResolveToString(new RawString(p, str.Length));
        }
    }

    /// <summary>Преобразование входной utf-8-строки в <see langword="string" /></summary>.
    /// <param name="str">строка формата utf-8 для преобразователя</param>
    /// <returns>resolved <see langword="string" /></returns>
    [SkipLocalsInit]
    public unsafe string ResolveToString(RawString str)
    {
        var byteLen = GetResolvedByteLength(str);
        const int threshold = 128;
        if (byteLen <= threshold)
        {
            Span<byte> buf = stackalloc byte[threshold];
            Resolve(str, buf);
            fixed (byte* ptr = buf)
            {
                return Utf8ExceptionFallbackEncoding.Instance.GetString(ptr, byteLen);
            }
        }
        else
        {
            var buf = ArrayPool<byte>.Shared.Rent(byteLen);
            try
            {
                Resolve(str, buf.AsSpan());
                fixed (byte* ptr = buf)
                {
                    return Utf8ExceptionFallbackEncoding.Instance.GetString(ptr, byteLen);
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buf);
            }
        }
    }

    /// <summary>Разрешить строку.</summary>
    /// <param name="str">строка для резолва</param>
    /// <returns>resolved utf-8 string as byte array</returns>
    public unsafe byte[] Resolve(ReadOnlySpan<byte> str)
    {
        fixed (byte* p = str)
        {
            // Безопасно создать RawString из ReadOnlySpan<byte>
            // Поскольку метод нигде не хранит экземпляр RawString.
            return Resolve(new RawString(p, str.Length));
        }
    }

    /// <summary>Разрешить строку.</summary>
    /// <param name="str">строка для резолва</param>
    /// <returns>resolved utf-8 string as byte array</returns>
    [SkipLocalsInit]
    public byte[] Resolve(RawString str)
    {
        var byteLen = GetResolvedByteLength(str);
        if (byteLen <= 128)
        {
            Span<byte> buf = stackalloc byte[byteLen];
            Resolve(str, buf);
            return buf.ToArray();
        }
        else
        {
            var buf = ArrayPool<byte>.Shared.Rent(byteLen);
            try
            {
                Resolve(str, buf.AsSpan());

                // Не возвращайте buf! Должен быть скопирован
                return buf.AsSpan(0, byteLen).ToArray();
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buf);
            }
        }
    }

    /// <summary>Разрешить строку в указанный буфер.</summary>
    /// <remarks>Буфер должен быть достаточно большим для резолва.</remarks>
    /// <param name="str">строка для преобразования</param>
    /// <param name="bufferToResolve">буфер, используемый при резолве строки</param>
    /// <returns>байт длины разрешенной строки</returns>
    public unsafe int Resolve(ReadOnlySpan<byte> str, Span<byte> bufferToResolve)
    {
        fixed (byte* p = str)
        {
            // It is safe to create RawString from ReadOnlySpan<byte>
            // because the method does not store the RawString instance anywhere.
            return Resolve(new RawString(p, str.Length), bufferToResolve);
        }
    }

    /// <summary>Разрешить строку в указанный буфер.</summary>
    /// <remarks>Буфер должен быть достаточно большим для разрешения.</remarks>
    /// <param name="str">строка для преобразования</param>
    /// <param name="bufferToResolve">буфер, используемый при разрешении строки</param>
    /// <returns>байт длины разрешенной строки</returns>
    [SkipLocalsInit]
    public unsafe int Resolve(RawString str, Span<byte> bufferToResolve)
    {
        const int ExBufLen = 5;
        var exBuf = stackalloc byte[ExBufLen];

        // Use pointer to avoid the overhead in case of SlowSpan runtime.
        fixed (byte* buf = bufferToResolve)
        {
            var i = 0;
            var j = 0;
            None:
            {
                if (i >= str.Length) goto End;
                var c = str.At(i++);
                if (c == '&') goto Alias;
                if (j >= bufferToResolve.Length) throw new ArgumentOutOfRangeException("Buffer is too short.");
                buf[j++] = c;
                goto None;
            }

            Alias:
            {
                var start = i;
                while (true)
                {
                    if (i >= str.Length) throw new FormatException($"Cannot end with '&'. Invalid input string: '{str}'");
                    if (str.At(i++) == ';')
                    {
                        var alias = str.SliceUnsafe(start, i - 1 - start);
                        if (TryGetValue(alias, out var value) == false)
                        {
                            var tmp = SpanHelper.CreateSpan<byte>(exBuf, ExBufLen);
                            if (TryUnicodePointToUtf8(alias, tmp, out var byteLen) == false) throw new FormatException($"Could not resolve the entity: '&{alias};'");
                            value = tmp.Slice(0, byteLen);
                        }
                        else
                        {
                            // Значение entity может содержать другой псевдоним enity.
                            var recursiveResolveNeeded = CheckNeedToResolve(value, out var l);
                            if (recursiveResolveNeeded == XmlEntityResolverState.CannotResolve)
                                throw new FormatException("Could not resolve an entity");
                            if (recursiveResolveNeeded == XmlEntityResolverState.NeedToResolve) value = Resolve(value);
                        }

                        // Если моя реализация верна, то value.Length не будет равна нулю, но лучше всё равно чекнуть (value.Length > 0)
                        {
                            if (j + value.Length - 1 >= bufferToResolve.Length) throw new ArgumentOutOfRangeException("Buffer is too short.");

                            fixed (byte* v = value)
                            {
                                Buffer.MemoryCopy(v, buf + j, value.Length, value.Length);
                            }

                            j += value.Length;
                        }

                        break;
                    }
                }

                goto None;
            }

            End:
            {
                return j;
            }
        }
    }

    private bool TryGetValue(RawString alias, out ReadOnlySpan<byte> value)
    {
        if (PredefinedEntityTable.TryGetPredefinedValue(alias, out value)) return true;

        if (_rawStringTable.TryGetValue(out var table) == false)
        {
            value = ReadOnlySpan<byte>.Empty;
            return false;
        }

        var success = table.TryGetValue(alias, out var v);
        value = v.AsSpan();
        return success;
    }

    private static bool TryUnicodePointToUtf8(RawString str, Span<byte> buffer, out int byteLength)
    {
        // str ~= "#1234" or "#x12AB"

        if (str.Length < 2 || str.At(0) != '#')
        {
            byteLength = 0;
            return false;
        }

        if (str.At(1) == 'x')
        {
            var hex = str.SliceUnsafe(2, str.Length - 2).AsSpan();
            if (Utf8Parser.TryParse(hex, out uint codePoint, out _, 'x') == false)
            {
                byteLength = 0;
                return false;
            }

            return UnicodeHelper.TryEncodeCodePointToUtf8(codePoint, buffer, out byteLength);
        }
        else
        {
            if (str.SliceUnsafe(1, str.Length - 1).TryToUInt32(out var codePoint) == false)
            {
                byteLength = 0;
                return false;
            }

            return UnicodeHelper.TryEncodeCodePointToUtf8(codePoint, buffer, out byteLength);
        }
    }
}

/// <summary>Состояние <see cref="XmlEntityTable" /></summary>.
public enum XmlEntityResolverState
{
    /// <summary>Не требуется разрешать строку.</summary>
    NoNeeded,

    /// <summary>Нужно разрешить строку.</summary>
    NeedToResolve,

    /// <summary>Строка недействительна, резолвер не может ее разрешить.</summary>
    CannotResolve
}