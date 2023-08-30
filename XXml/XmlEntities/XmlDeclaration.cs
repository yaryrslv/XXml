using System.Diagnostics;
using System.Runtime.CompilerServices;
using XXml.ValueObjects;

namespace XXml.XmlEntities;

[DebuggerDisplay("{ToString(),nq}")]
public readonly unsafe struct XmlDeclaration : IEquatable<XmlDeclaration>, IReference
{
    private readonly XmlDeclarationType* _declaration;

    public bool IsNull => _declaration == null;

    public Option<XmlAttribute> Version => new XmlAttribute(_declaration->Version);
    public Option<XmlAttribute> Encoding => new XmlAttribute(_declaration->Encoding);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal XmlDeclaration(XmlDeclarationType* declaration)
    {
        _declaration = declaration;
    }

    public RawString AsRawString()
    {
        return _declaration != null ? _declaration->Body : RawString.Empty;
    }

    public override bool Equals(object? obj)
    {
        return obj is XmlDeclaration declaration && Equals(declaration);
    }

    public bool Equals(XmlDeclaration other)
    {
        return _declaration == other._declaration;
    }

    public override int GetHashCode()
    {
        return new IntPtr(_declaration).GetHashCode();
    }

    public override string ToString()
    {
        return _declaration != null ? _declaration->Body.ToString() : "";
    }

    public static bool operator ==(XmlDeclaration left, XmlDeclaration right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(XmlDeclaration left, XmlDeclaration right)
    {
        return !(left == right);
    }
}

internal unsafe struct XmlDeclarationType
{
    public RawString Body;
    public XmlAttributeStruct* Version;
    public XmlAttributeStruct* Encoding;
}