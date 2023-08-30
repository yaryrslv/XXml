using System.ComponentModel;

namespace XXml.ValueObjects;

partial struct RawString
{
    /// <summary>Используйте <see cref="StartsWith(RawString)" /> вместо этого. (Правильное название метода - Start**s**With)</summary>
    /// <param name="other"></param>
    /// <returns></returns>
    [Obsolete("Вместо этого используйте RawString.StartsWith. (Имя метода - Start*s*With)")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool StartWith(RawString other)
    {
        return StartsWith(other);
    }

    /// <summary>
    ///     Используйте <see cref="StartsWith(ReadOnlySpan{byte})" /> вместо этого. (Правильное название метода -
    ///     Start**s**With)
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    [Obsolete("Вместо этого используйте RawString.StartsWith. (Имя метода - Start*s*With)")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool StartWith(ReadOnlySpan<byte> other)
    {
        return StartsWith(other);
    }
}