using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using XXml.Internal;
using XXml.ValueObjects;

namespace XXml.XmlEntities;

[DebuggerDisplay("{DebugDisplay,nq}")]
[DebuggerTypeProxy(typeof(XmlNodeListTypeProxy))]
public readonly unsafe struct XmlNodeList : ICollection<XmlNode>, IReference
{
    private readonly XmlNodeStruct* _parent;
    private readonly XmlNodeType? _targetType;

    internal Option<XmlNode> Parent => new XmlNode(_parent);

    public bool IsNull => _parent == null;

    public bool IsEmpty => Count == 0;

    public int Count => _targetType switch
    {
        null => _parent->ChildCount,
        XmlNodeType.ElementNode => _parent->ChildElementCount,
        XmlNodeType.TextNode => _parent->ChildTextCount,
        _ => 0
    };

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebugDisplay => _parent != null ? $"{nameof(XmlNode)}[{Count}]" : $"{nameof(XmlNode)} (invalid instance)";

    bool ICollection<XmlNode>.IsReadOnly => true;

    internal XmlNodeList(XmlNodeStruct* parent, XmlNodeType? targetType)
    {
        _parent = parent;
        _targetType = targetType;
    }

    public XmlNode First()
    {
        if (FirstOrDefault().TryGetValue(out var node) == false) ThrowHelper.ThrowInvalidOperation("Sequence contains no elements.");
        return node;
    }

    private Option<XmlNode> FirstOrDefault()
    {
        using var e = GetEnumerator();
        return e.MoveNext() == false ? Option<XmlNode>.Null : e.Current;
    }

    public XmlNode First(Func<XmlNode, bool>? predicate)
    {
        if (FirstOrDefault(predicate).TryGetValue(out var node) == false) ThrowHelper.ThrowInvalidOperation("Sequence contains no matching elements.");
        return node;
    }

    private Option<XmlNode> FirstOrDefault(Func<XmlNode, bool>? predicate)
    {
        if (predicate is null) ThrowHelper.ThrowNullArg(nameof(predicate));

        foreach (var node in this.Where(predicate)) return node;

        return Option<XmlNode>.Null;
    }

    public Enumerator GetEnumerator()
    {
        return new Enumerator(_parent, _targetType);
    }

    IEnumerator<XmlNode> IEnumerable<XmlNode>.GetEnumerator()
    {
        return new EnumeratorClass(_parent, _targetType);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return new EnumeratorClass(_parent, _targetType);
    }

    void ICollection<XmlNode>.Add(XmlNode item)
    {
        throw new NotSupportedException();
    }

    void ICollection<XmlNode>.Clear()
    {
        throw new NotSupportedException();
    }

    bool ICollection<XmlNode>.Contains(XmlNode item)
    {
        throw new NotSupportedException();
    }

    void ICollection<XmlNode>.CopyTo(XmlNode[] array, int arrayIndex)
    {
        throw new NotSupportedException();
    }

    bool ICollection<XmlNode>.Remove(XmlNode item)
    {
        throw new NotSupportedException();
    }

    public struct Enumerator : IEnumerator<XmlNode>
    {
        private XmlNodeStruct* _current;
        private XmlNodeStruct* _next;
        private readonly XmlNodeType _targetType;
        private readonly bool _hasTargetType;

        internal Enumerator(XmlNodeStruct* parent, XmlNodeType? targetType)
        {
            _next = parent->FirstChild;
            _current = null;
            (_targetType, _hasTargetType) = targetType.HasValue switch
            {
                true => (targetType.Value, true),
                false => (default, false)
            };
        }

        public XmlNode Current => new(_current);

        object IEnumerator.Current => Current;

        public void Dispose()
        {
        } // nop

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            MoveNext:
            if (_next == null) return false;
            _current = _next;
            _next = _next->Sibling;

            if (_hasTargetType == false || _current->NodeType == _targetType) return true;

            goto MoveNext;
        }

        public void Reset()
        {
            throw new NotSupportedException("Reset() is not supported.");
        }
    }

    private sealed class EnumeratorClass : IEnumerator<XmlNode>
    {
        private Enumerator _enumerator; // mutable object, don't make it readonly.

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal EnumeratorClass(XmlNodeStruct* parent, XmlNodeType? targetType)
        {
            _enumerator = new Enumerator(parent, targetType);
        }

        public XmlNode Current => _enumerator.Current;

        object IEnumerator.Current => Current;

        public void Dispose()
        {
            _enumerator.Dispose();
        }

        public bool MoveNext()
        {
            return _enumerator.MoveNext();
        }

        public void Reset()
        {
            _enumerator.Reset();
        }
    }

    private sealed class XmlNodeListTypeProxy
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private XmlNodeList _entity;

        public XmlNodeListTypeProxy(XmlNodeList entity)
        {
            _entity = entity;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public XmlNode[] Items => _entity.ToArray();
    }
}