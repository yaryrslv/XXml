using System.Text;

namespace XXml.Internal;

internal sealed class Utf8ExceptionFallbackEncoding : UTF8Encoding
{
    private Utf8ExceptionFallbackEncoding() : base(false, true)
    {
    }

    public static Utf8ExceptionFallbackEncoding Instance { get; } = new();
}