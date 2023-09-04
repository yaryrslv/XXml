using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using XXml.InternalEntities;

namespace XXml.XmlEntities;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
[DebuggerTypeProxy(typeof(XmlNodeDescendantListDebuggerTypeProxy))]
public readonly unsafe struct XmlNodeDescendantList : IEnumerable<XmlNode>
{
    private readonly XmlNodeStruct* _parent;
    private readonly XmlNodeType? _targetType;

    internal XmlNodeDescendantList(XmlNodeStruct* parent, XmlNodeType? targetType)
    {
        Debug.Assert(parent != null);
        _parent = parent;
        _targetType = targetType;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay
    {
        get
        {
            var count = 0;
            foreach (var _ in this) count++;
            return $"XmlNode[{count}]";
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    public struct Enumerator : IEnumerator<XmlNode>
    {
        private CustomList<XmlNodeStruct>.Enumerator _e; //НЕ делать readonly!
        private readonly int _depth;
        private readonly XmlNodeType _targetType;
        private readonly bool _hasTargetType;

        public XmlNode Current => new(_e.Current);

        object IEnumerator.Current => Current;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Enumerator(XmlNodeStruct* parent, XmlNodeType? targetType)
        {
            (_hasTargetType, _targetType) = targetType.HasValue switch
            {
                true => (true, targetType.Value),
                false => (false, default)
            };

            var firstChild = parent->FirstChild;
            if (firstChild == null)
            {
                // default instance is valid.
                _e = default;
                _depth = default;
            }
            else
            {
                _e = parent->WholeNodes.GetEnumerator(firstChild->NodeIndex);
                _depth = parent->Depth;
            }
        }

        public void Dispose()
        {
            _e.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            while(true)
            {
                if (_e.MoveNext() == false) return false;
                if (_e.Current->Depth <= _depth) return false;
                if (_hasTargetType == false || _e.Current->NodeType == _targetType) return true;
            }
        }

        public void Reset()
        {
            _e.Reset();
        }
    }

    private sealed class EnumeratorClass : IEnumerator<XmlNode>
    {
        private Enumerator _e; //НЕ делать readonly!

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal EnumeratorClass(XmlNodeStruct* parent, XmlNodeType? targetType)
        {
            _e = new Enumerator(parent, targetType);
        }

        public XmlNode Current => _e.Current;

        object IEnumerator.Current => _e.Current;

        public void Dispose()
        {
            _e.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            return _e.MoveNext();
        }

        public void Reset()
        {
            _e.Reset();
        }
    }

    internal sealed class XmlNodeDescendantListDebuggerTypeProxy
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly XmlNodeDescendantList _list;

        public XmlNodeDescendantListDebuggerTypeProxy(XmlNodeDescendantList list)
        {
            _list = list;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public IEnumerable<XmlNode> Item => _list.ToArray();
    }
}