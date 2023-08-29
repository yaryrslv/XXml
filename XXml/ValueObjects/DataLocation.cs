using System.ComponentModel;
using System.Diagnostics;
using XXml.Internal;

namespace XXml.ValueObjects;

/// <summary>Представляет местоположение данных</summary>.
[DebuggerDisplay("{DebugView,nq}")]
public readonly struct DataLocation : IEquatable<DataLocation>
{
    /// <summary>Номер начальной строки и номер символа данных</summary>.
    public readonly DataLinePosition Start;

    /// <summary>Номер конечной строки и номер символа данных</summary>.
    public readonly DataLinePosition End;

    /// <summary>Диапазон данных в байтах</summary>
    public readonly DataRange Range;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebugView => Start == End ? Start.DebugView : $"{Start.DebugView} - {End.DebugView}";

    /// <summary>Создание местоположения данных</summary>
    /// <param name="start">номер начальной строки и номер символа</param>
    /// <param name="end">номер конечной строки и номер символа</param>
    /// <param name="range">диапазон данных в байтах</param>
    public DataLocation(DataLinePosition start, DataLinePosition end, DataRange range)
    {
        Start = start;
        End = end;
        Range = range;
    }

    /// <summary>Деконструировать <see cref="DataLocation" /></summary>
    /// <param name="start">номер начальной строки и номер символа</param>
    /// <param name="end">номер конечной строки и номер символа</param>
    /// <param name="range">диапазон данных в байтах</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public void Deconstruct(out DataLinePosition start, out DataLinePosition end, out DataRange range)
    {
        start = Start;
        end = End;
        range = Range;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is DataLocation location && Equals(location);
    }

    public bool Equals(DataLocation other)
    {
        return Start.Equals(other.Start) &&
               End.Equals(other.End) &&
               Range.Equals(other.Range);
    }

    /// Невозможно использовать System.HashCode, поскольку проект не зависит от пакета Microsoft.Bcl.HashCode. (на netstandard2.0 / net48)
    /// <inheritdoc />
    public override int GetHashCode()
    {
        return XxHash32.ComputeHash(this);
    }

    public static bool operator ==(in DataLocation left, in DataLocation right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(in DataLocation left, in DataLocation right)
    {
        return !(left == right);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return DebugView;
    }
}

/// <summary>Представляет номер строки и номер символа данных</summary>.
[DebuggerDisplay("{DebugView,nq}")]
public readonly struct DataLinePosition : IEquatable<DataLinePosition>
{
    /// <summary>Номер строки (нулевая нумерация)</summary>
    public readonly int Line;

    /// <summary>Смещение позиции, является числом символов, а не числом байт. (нулевая нумерация)</summary>
    public readonly int Position;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    internal string DebugView => $"(L.{Line}, {Position})";

    /// <summary>Создание позиции строки данных</summary>
    /// <param name="line">номер строки</param>
    /// <param name="position">номер символа в строке</param>
    public DataLinePosition(int line, int position)
    {
        Line = line;
        Position = position;
    }

    /// <summary>Деконструировать <see cref="DataLinePosition" /></summary>
    /// <param name="line">номер строки</param>
    /// <param name="position">номер символа в строке</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public void Deconstruct(out int line, out int position)
    {
        line = Line;
        position = Position;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is DataLinePosition position && Equals(position);
    }

    public bool Equals(DataLinePosition other)
    {
        return Line == other.Line && Position == other.Position;
    }

    // Невозможно использовать System.HashCode, поскольку проект не зависит от пакета Microsoft.Bcl.HashCode. (на netstandard2.0 / net48)
    /// <inheritdoc />
    public override int GetHashCode()
    {
        return XxHash32.ComputeHash(this);
    }

    public static bool operator ==(DataLinePosition left, DataLinePosition right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(DataLinePosition left, DataLinePosition right)
    {
        return !(left == right);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return DebugView;
    }
}

/// <summary>Представляет собой байтовый диапазон данных</summary>
[DebuggerDisplay("{DebugView,nq}")]
public readonly struct DataRange : IEquatable<DataRange>
{
    /// <summary>стартовое смещение в байтах</summary>
    public readonly int Start;

    /// <summary>длина в байтах</summary>
    public readonly int Length;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebugView => $"Start: {Start}, Length: {Length}";

    /// <summary>Создание байтового диапазона данных</summary>
    /// <param name="start">начальное смещение в байтах</param>
    /// <param name="length">длина байта</param>
    public DataRange(int start, int length)
    {
        Start = start;
        Length = length;
    }

    /// <summary>Деконструировать <see cref="DataRange" /></summary>
    /// <param name="start">начальное смещение в байтах</param>
    /// <param name="length">длина байта</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public void Deconstruct(out int start, out int length)
    {
        start = Start;
        length = Length;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is DataRange range && Equals(range);
    }

    public bool Equals(DataRange other)
    {
        return Start == other.Start && Length == other.Length;
    }

    // Невозможно использовать System.HashCode, поскольку проект не зависит от пакета Microsoft.Bcl.HashCode. (на netstandard2.0 / net48)
    /// <inheritdoc />
    public override int GetHashCode()
    {
        return XxHash32.ComputeHash(this);
    }

    public static bool operator ==(DataRange left, DataRange right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(DataRange left, DataRange right)
    {
        return !(left == right);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return DebugView;
    }
}