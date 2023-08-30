using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using XXml.Internal;
using XXml.ValueObjects;

namespace XXml.XmlEntities;

[DebuggerDisplay("XmlNode[{Count}]")]
[DebuggerTypeProxy(typeof(AllNodeListDebuggerTypeProxy))]
public readonly unsafe struct AllNodeList : IEnumerable<XmlNode>
{
    private readonly CustomList<XmlNodeStruct> _nodes;
    private readonly XmlNodeType? _targetType;

    public int Count { get; }

    internal AllNodeList(CustomList<XmlNodeStruct> nodes, int count, XmlNodeType? targetType)
    {
        _nodes = nodes;
        Count = count;
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
        if (e.MoveNext() == false) return Option<XmlNode>.Null;
        return e.Current;
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

    private Enumerator GetEnumerator()
    {
        return new Enumerator(_nodes.GetEnumerator(), _targetType);
    }

    IEnumerator<XmlNode> IEnumerable<XmlNode>.GetEnumerator()
    {
        return new EnumeratorClass(_nodes.GetEnumerator(), _targetType);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return new EnumeratorClass(_nodes.GetEnumerator(), _targetType);
    }


    public struct Enumerator : IEnumerator<XmlNode>
    {
        private CustomList<XmlNodeStruct>.Enumerator _e;
        private readonly XmlNodeType _targetType;
        private readonly bool _hasTargetType;

        internal Enumerator(CustomList<XmlNodeStruct>.Enumerator e, XmlNodeType? targetType)
        {
            _e = e;
            (_hasTargetType, _targetType) = targetType.HasValue switch
            {
                true => (true, targetType.Value),
                false => (false, default)
            };
        }

        public XmlNode Current => new(_e.Current);

        object IEnumerator.Current => Current;

        public void Dispose()
        {
            _e.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            MoveNext:
            if (_e.MoveNext() == false) return false;
            if (_hasTargetType == false || _e.Current->NodeType == _targetType) return true;
            goto MoveNext;
        }

        public void Reset()
        {
            _e.Reset();
        }
    }

    private sealed class EnumeratorClass : IEnumerator<XmlNode>
    {
        private Enumerator _e;

        internal EnumeratorClass(CustomList<XmlNodeStruct>.Enumerator e, XmlNodeType? targetType)
        {
            _e = new Enumerator(e, targetType);
        }

        public XmlNode Current => _e.Current;

        object IEnumerator.Current => Current;

        public void Dispose()
        {
            _e.Dispose();
        }

        public bool MoveNext()
        {
            return _e.MoveNext();
        }

        public void Reset()
        {
            _e.Reset();
        }
    }

    internal sealed class AllNodeListDebuggerTypeProxy
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly AllNodeList _list;

        public AllNodeListDebuggerTypeProxy(AllNodeList list)
        {
            _list = list;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public XmlNode[] Item => _list.ToArray();
    }
}