using System.Diagnostics;
using XXml.ValueObjects;
using XXml.XmlEntities;

namespace XXml.InternalEntities;

internal static class XmlnsHelper
{
    private const uint BytesXmln = (byte) 'x' + ((byte) 'm' << 8) + ((byte) 'l' << 16) + ((byte) 'n' << 24);
    private const uint BytesSColon = (byte) 's' + ((byte) ':' << 8);

    public static unsafe bool TryResolveNamespaceAlias(ReadOnlySpan<byte> nsName, XmlNode node, out RawString alias)
    {
        // xmlns:[alias]="[nsName]"
        // nsName -> alias

        var currentNode = node;
        while (true)
        {
            if (currentNode.HasXmlNamespaceAttr)
                foreach (var attr in currentNode.Attributes)
                {
                    var attrName = attr.Name;
                    if (attrName.Length >= 5 && *(uint*) attrName.GetPtr() == BytesXmln
                                             && attrName.At(4) == (byte) 's')
                    {
                        if (attrName.Length == 5 && attr.Value == nsName)
                            // xmlns="[nsName]"
                            return Validate(RawString.Empty, nsName, node, out alias);
                        if (attrName.Length >= 7 && attrName[5] == (byte) ':' && attr.Value == nsName)
                            // xmlns:[alias]="[nsName]"
                            return Validate(attrName.Slice(6), nsName, node, out alias);
                    }
                }

            if (currentNode.TryGetParent(out currentNode) == false)
            {
                alias = RawString.Empty;
                return false;
            }
        }

        static bool Validate(RawString aliasToValidate, ReadOnlySpan<byte> nsName, XmlNode targetNode, out RawString alias)
        {
            if (TryFindXmlnsRecursively(targetNode, aliasToValidate.AsSpan(), out var nsn))
            {
                if (nsn == nsName)
                {
                    alias = aliasToValidate;
                    return true;
                }

                alias = RawString.Empty;
                return false;
            }

            Debug.Fail("Why are you getting here?");
            alias = RawString.Empty;
            return false;
        }
    }

    public static bool TryGetAttributeFullName(XmlAttribute attr, out RawString nsName, out RawString name)
    {
        RawString nsAlias;
        var attrName = attr.Name;
        if (attr.Node.TryGetValue(out var node) == false)
        {
            nsName = RawString.Empty;
            name = attr.Name;
            return false;
        }

        for (var i = 0; i < attrName.Length; i++)
            if (attrName.At(i) == (byte) ':')
            {
                nsAlias = attrName.SliceUnsafe(0, i);
                name = attrName.SliceUnsafe(i + 1, attrName.Length - i - 1);
                return TryFindXmlnsRecursively(node, nsAlias.AsSpan(), out nsName);
            }

        name = attrName;
        return TryFindXmlnsRecursively(node, ReadOnlySpan<byte>.Empty, out nsName);
    }

    public static bool TryGetNodeFullName(XmlNode node, out RawString nsName, out RawString name)
    {
        RawString nsAlias;
        var nodeName = node.Name;
        for (var i = 0; i < nodeName.Length; i++)
            if (nodeName.At(i) == (byte) ':')
            {
                nsAlias = nodeName.SliceUnsafe(0, i);
                name = nodeName.SliceUnsafe(i + 1, nodeName.Length - i - 1);
                return TryFindXmlnsRecursively(node, nsAlias.AsSpan(), out nsName);
            }

        name = nodeName;
        return TryFindXmlnsRecursively(node, ReadOnlySpan<byte>.Empty, out nsName);
    }

    private static bool TryFindXmlnsRecursively(XmlNode target, ReadOnlySpan<byte> alias, out RawString nsName)
    {
        // xmlns:[alias]="[nsName]"
        // alias -> nsName (recursive)

        var current = target;

        while (true)
        {
            if (TryFindXmlns(current, alias, out var nsn))
            {
                nsName = nsn;
                return true;
            }

            if (current.TryGetParent(out current) == false)
            {
                nsName = RawString.Empty;
                return false;
            }
        }
    }

    private static unsafe bool TryFindXmlns(XmlNode target, ReadOnlySpan<byte> alias, out RawString nsName)
    {
        // xmlns:[alias]="[nsName]"
        // alias -> nsName (not recursive)

        if (alias.IsEmpty)
            foreach (var attr in target.Attributes)
            {
                var attrName = attr.Name;
                if (attrName.Length == 5 && *(uint*) attrName.GetPtr() == BytesXmln
                                         && attrName.At(4) == (byte) 's')
                {
                    nsName = attr.Value;
                    return true;
                }
            }
        else
            foreach (var attr in target.Attributes)
            {
                var attrName = attr.Name;
                if (attrName.Length >= 7 && *(uint*) attrName.GetPtr() == BytesXmln
                                         && *(ushort*) (attrName.GetPtr() + 4) == BytesSColon
                                         && attrName.Slice(6) == alias)
                {
                    nsName = attr.Value;
                    return true;
                }
            }

        nsName = RawString.Empty;
        return false;
    }
}