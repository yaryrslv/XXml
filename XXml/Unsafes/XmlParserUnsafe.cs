using System.Text;
using XXml.Internal;
using XXml.XmlEntities;

namespace XXml.Unsafes;

/// <summary>
///     [ПРЕДУПРЕЖДЕНИЕ] Не используйте это, если не знаете, как использовать. Класс скрыт.
///     <para />
///     *** Утечки памяти происходят при неправильном использовании. ***
///     <para />
///     Объект, возвращаемый методами, ДОЛЖЕН быть задиспожен после его использования.
///     <para />
/// </summary>
public static class XmlParserUnsafe
{
    /// <summary>
    ///     [ПРЕДУПРЕЖДЕНИЕ] Не используйте это, если не знаете, как использовать. Метод скрыт.
    ///     <para />
    ///     *** При неправильном использовании происходит утечка памяти. ***
    ///     <para />
    ///     Объект, возвращаемый методом, ДОЛЖЕН быть задиспожен после его использования.
    ///     <para />
    /// </summary>
    /// <param name="utf8Text">utf-8 string to parse</param>
    /// <returns>xml object</returns>
    public static XmlObjectUnsafe ParseUnsafe(ReadOnlySpan<byte> utf8Text)
    {
        var buf = new UnmanagedBuffer(utf8Text);
        try
        {
            return XmlObjectUnsafe.Create(XmlParser.ParseCore(ref buf, utf8Text.Length));
        }
        catch
        {
            buf.Dispose();
            throw;
        }
    }

    /// <summary>
    ///     [ПРЕДУПРЕЖДЕНИЕ] Не используйте это, если не знаете, как использовать. Метод скрыт.
    ///     <para />
    ///     *** При неправильном использовании происходит утечка памяти. ***
    ///     <para />
    ///     Объект, возвращаемый методом, ДОЛЖЕН быть задиспожен после его использования.
    ///     <para />
    /// </summary>
    /// <param name="stream">stream to parse</param>
    /// <returns>xml object</returns>
    public static XmlObjectUnsafe ParseUnsafe(Stream stream)
    {
        var fileSizeHint = stream.CanSeek ? (int) stream.Length : 1024 * 1024;
        return ParseUnsafe(stream, fileSizeHint);
    }

    /// <summary>
    ///     [ПРЕДУПРЕЖДЕНИЕ] Не используйте это, если не знаете, как использовать. Метод скрыт.
    ///     <para />
    ///     *** При неправильном использовании происходит утечка памяти. ***
    ///     <para />
    ///     Объект, возвращаемый методом, ДОЛЖЕН быть задиспожен после его использования.
    ///     <para />
    /// </summary>
    /// <param name="stream">stream to parse</param>
    /// <param name="fileSizeHint">file size hint</param>
    /// <returns>xml object</returns>
    public static XmlObjectUnsafe ParseUnsafe(Stream stream, int fileSizeHint)
    {
        if (stream is null) ThrowHelper.ThrowNullArg(nameof(stream));
        var (buf, length) = stream!.ReadAllToUnmanaged(fileSizeHint);
        try
        {
            return XmlObjectUnsafe.Create(XmlParser.ParseCore(ref buf, length));
        }
        catch
        {
            buf.Dispose();
            throw;
        }
    }

    /// <summary>
    ///     [ПРЕДУПРЕЖДЕНИЕ] Не используйте это, если не знаете, как использовать. Метод скрыт.
    ///     <para />
    ///     *** При неправильном использовании происходит утечка памяти. ***
    ///     <para />
    ///     Объект, возвращаемый методом, ДОЛЖЕН быть задиспожен после его использования.
    ///     <para />
    /// </summary>
    /// <param name="filePath">file path to parse</param>
    /// <returns>xml object</returns>
    public static XmlObjectUnsafe ParseFileUnsafe(string filePath)
    {
        return XmlObjectUnsafe.Create(XmlParser.ParseFileCore(filePath, Encoding.UTF8));
    }

    /// <summary>
    ///     [ПРЕДУПРЕЖДЕНИЕ] Не используйте это, если не знаете, как использовать. Метод скрыт.
    ///     <para />
    ///     *** При неправильном использовании происходит утечка памяти. ***
    ///     <para />
    ///     Объект, возвращаемый методом, ДОЛЖЕН быть задиспожен после его использования.
    ///     <para />
    /// </summary>
    /// <param name="filePath">file path to parse</param>
    /// <param name="encoding">encoding of the file</param>
    /// <returns>xml object</returns>
    public static XmlObjectUnsafe ParseFileUnsafe(string filePath, Encoding encoding)
    {
        return XmlObjectUnsafe.Create(XmlParser.ParseFileCore(filePath, encoding));
    }
}