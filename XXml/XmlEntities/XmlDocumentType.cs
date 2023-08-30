using System.Diagnostics;
using System.Runtime.CompilerServices;
using XXml.ValueObjects;

namespace XXml.XmlEntities;

[DebuggerDisplay("{ToString(),nq}")]
public readonly unsafe struct XmlDocumentType : IEquatable<XmlDocumentType>, IReference
{
    private readonly XmlDocumentTypeCustom* _docType;

    public bool IsNull => _docType == null;

    public RawString Name => _docType->Name;

    public RawString InternalSubset => _docType->InternalSubset;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal XmlDocumentType(XmlDocumentTypeCustom* docType)
    {
        _docType = docType;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RawString AsRawString()
    {
        return _docType != null ? _docType->Body : RawString.Empty;
    }

    public override bool Equals(object? obj)
    {
        return obj is XmlDocumentType type && Equals(type);
    }

    public bool Equals(XmlDocumentType other)
    {
        return _docType == other._docType;
    }

    public override int GetHashCode()
    {
        return new IntPtr(_docType).GetHashCode();
    }

    public override string ToString()
    {
        return AsRawString().ToString();
    }

    public static bool operator ==(XmlDocumentType left, XmlDocumentType right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(XmlDocumentType left, XmlDocumentType right)
    {
        return !(left == right);
    }
}

internal struct XmlDocumentTypeCustom
{
    public RawString Body;
    public RawString Name;
    public RawString InternalSubset;
}