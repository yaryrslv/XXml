using XXml.ValueObjects;

namespace XXml.InternalEntities;

internal static class PredefinedEntityTable
{
    private static ReadOnlySpan<byte> EntityAmp => new byte[1] {(byte) '&'};
    private static ReadOnlySpan<byte> EntityLt => new byte[1] {(byte) '<'};
    private static ReadOnlySpan<byte> EntityGt => new byte[1] {(byte) '>'};
    private static ReadOnlySpan<byte> EntityQuot => new byte[1] {(byte) '"'};
    private static ReadOnlySpan<byte> EntityApos => new byte[1] {(byte) '\''};

    public static bool TryGetPredefinedValue(in RawString alias, out ReadOnlySpan<byte> value)
    {
        if (alias.Length == 2)
        {
            if (alias.At(0) == 'l' && alias.At(1) == 't')
            {
                // &lt;
                value = EntityLt;
                return true;
            }

            if (alias.At(0) == 'g' && alias.At(1) == 't')
            {
                // &gt;
                value = EntityGt;
                return true;
            }
        }
        else if (alias.Length == 3)
        {
            if (alias.At(0) == 'a' && alias.At(1) == 'm' && alias.At(2) == 'p')
            {
                // &amp;
                value = EntityAmp;
                return true;
            }
        }
        else if (alias.Length == 4)
        {
            if (alias.At(0) == 'q' && alias.At(1) == 'u' && alias.At(2) == 'o' && alias.At(3) == 't')
            {
                // &quot;
                value = EntityQuot;
                return true;
            }

            if (alias.At(0) == 'a' && alias.At(1) == 'p' && alias.At(2) == 'o' && alias.At(3) == 's')
            {
                // &apos;
                value = EntityApos;
                return true;
            }
        }

        value = default;
        return false;
    }
}