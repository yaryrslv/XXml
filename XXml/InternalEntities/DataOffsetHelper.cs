using System.Diagnostics;
using XXml.ValueObjects;

namespace XXml.InternalEntities;

internal static unsafe class DataOffsetHelper
{
    public static int? GetOffset(byte* dataHead, int dataLen, byte* target)
    {
        if (dataLen < 0) ThrowHelper.ThrowArgOutOfRange(nameof(dataLen));
        if (CheckContainsMemory(dataHead, dataLen, target, 0) == false) return null;
        var offset = target - dataHead;
        return checked((int) (uint) offset);
    }

    public static DataLocation? GetLocation(byte* dataHead, int dataLen, byte* targetHead, int targetLen)
    {
        if (dataLen < 0) ThrowHelper.ThrowArgOutOfRange(nameof(dataLen));
        if (targetLen < 0) ThrowHelper.ThrowArgOutOfRange(nameof(targetLen));
        if (CheckContainsMemory(dataHead, dataLen, targetHead, targetLen) == false) return null;

        var start = GetLinePositionPrivate(dataHead, dataLen, targetHead);
        var endOffset = GetLinePositionPrivate(targetHead, targetLen, targetHead + targetLen);
        var end = new DataLinePosition(
            start.Line + endOffset.Line,
            endOffset.Line == 0 ? start.Position + endOffset.Position : endOffset.Position
        );

        var byteOffset = checked((int) (uint) (targetHead - dataHead));
        var range = new DataRange(byteOffset, targetLen);
        return new DataLocation(start, end, range);
    }

    public static DataLinePosition? GetLinePosition(byte* dataHead, int dataLen, byte* target)
    {
        if (dataLen < 0) ThrowHelper.ThrowArgOutOfRange(nameof(dataLen));
        if (CheckContainsMemory(dataHead, dataLen, target, 0) == false) return null;
        return GetLinePositionPrivate(dataHead, dataLen, target);
    }

    private static DataLinePosition GetLinePositionPrivate(byte* dataHead, int dataLen, byte* target)
    {
        Debug.Assert(dataLen >= 0);
        Debug.Assert(dataHead <= target);

        var lineNum = 0;
        var lastLineHead = dataHead;
        for (var p = dataHead; p < target; p++)
            if (*p == '\n')
            {
                lineNum++;
                lastLineHead = p + 1;
            }

        var byteCountInLastLine = checked((int) (uint) (target - lastLineHead));
        var utf8 = Utf8ExceptionFallbackEncoding.Instance;
        var pos = utf8.GetCharCount(lastLineHead, byteCountInLastLine);
        return new DataLinePosition(lineNum, pos);
    }

    private static bool CheckContainsMemory(byte* dataHead, int dataLen, byte* targetHead, int targetLen)
    {
        Debug.Assert(targetLen >= 0);
        Debug.Assert(dataLen >= 0);
        return dataHead <= targetHead && targetHead + targetLen <= dataHead + dataLen;
    }
}