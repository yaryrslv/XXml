using System.Runtime.CompilerServices;

namespace XXml.Internal;

internal static class UnsafeHelper
{
    public static void SkipInit<T>(out T value)
    {
#if !NETCOREAPP3_1
        Unsafe.SkipInit(out value);
#else
            value = default!;
#endif
    }
}