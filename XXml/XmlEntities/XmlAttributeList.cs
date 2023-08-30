using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using XXml.InternalEntities;
using XXml.ValueObjects;

namespace XXml.XmlEntities;

/// <summary>Предоставляет список <see cref="XmlAttribute" /></summary>
/// .
[DebuggerDisplay("XmlAttribute[{Count}]")]
[DebuggerTypeProxy(typeof(XmlAttributeListTypeProxy))]
public readonly unsafe struct XmlAttributeList : ICollection<XmlAttribute>
{
    private readonly IntPtr _node; // XmlNodeStruct*

    private readonly int StartIndex => ((XmlNodeStruct*) _node)->AttrIndex;
    private CustomList<XmlAttributeStruct> List => ((XmlNodeStruct*) _node)->WholeAttrs;

    /// <summary>Получение количества атрибутов</summary>
    public int Count => ((XmlNodeStruct*) _node)->AttrCount;

    internal XmlNode Node => new((XmlNodeStruct*) _node);

    bool ICollection<XmlAttribute>.IsReadOnly => true;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal XmlAttributeList(XmlNodeStruct* node)
    {
        _node = (IntPtr) node;
    }

    public XmlAttribute First()
    {
        if (Count == 0) ThrowHelper.ThrowInvalidOperation("Sequence contains no elements.");
        return new XmlAttribute(List.At(StartIndex));
    }

    public XmlAttribute First(Func<XmlAttribute, bool>? predicate)
    {
        if (FirstOrDefault(predicate).TryGetValue(out var attr) == false) ThrowHelper.ThrowInvalidOperation("Sequence contains no matching elements.");
        return attr;
    }

    public Option<XmlAttribute> FirstOrDefault()
    {
        return Count == 0 ? default : new XmlAttribute(List.At(StartIndex));
    }

    private Option<XmlAttribute> FirstOrDefault(Func<XmlAttribute, bool>? predicate)
    {
        if (predicate is null) ThrowHelper.ThrowNullArg(nameof(predicate));

        foreach (var attr in this.Where(predicate))
            return attr;

        return default;
    }

    void ICollection<XmlAttribute>.Add(XmlAttribute item)
    {
        throw new NotSupportedException();
    }

    void ICollection<XmlAttribute>.Clear()
    {
        throw new NotSupportedException();
    }

    bool ICollection<XmlAttribute>.Contains(XmlAttribute item)
    {
        throw new NotSupportedException();
    }

    void ICollection<XmlAttribute>.CopyTo(XmlAttribute[] array, int arrayIndex)
    {
        throw new NotSupportedException();
    }

    bool ICollection<XmlAttribute>.Remove(XmlAttribute item)
    {
        throw new NotSupportedException();
    }

    internal void CopyTo(Span<XmlAttribute> span)
    {
        // Only for debugger

        var dest = MemoryMarshal.Cast<XmlAttribute, IntPtr>(span);
        fixed (IntPtr* buf = dest)
        {
            List.CopyItemsPointer((XmlAttributeStruct**) buf, dest.Length, StartIndex, Count);
        }
    }

    public Enumerator GetEnumerator()
    {
        return new Enumerator(List.GetEnumerator(StartIndex, Count));
    }

    IEnumerator<XmlAttribute> IEnumerable<XmlAttribute>.GetEnumerator()
    {
        return new EnumeratorClass(List.GetEnumerator(StartIndex, Count));
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return new EnumeratorClass(List.GetEnumerator(StartIndex, Count));
    }

    public struct Enumerator : IEnumerator<XmlAttribute>
    {
        private CustomList<XmlAttributeStruct>.Enumerator _enumerator; // mutable object, don't make it readonly

        public XmlAttribute Current => new(_enumerator.Current);

        object IEnumerator.Current => *_enumerator.Current;

        internal Enumerator(in CustomList<XmlAttributeStruct>.Enumerator enumerator)
        {
            _enumerator = enumerator;
        }

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

    private sealed class EnumeratorClass : IEnumerator<XmlAttribute>
    {
        private CustomList<XmlAttributeStruct>.Enumerator _enumerator; // mutable object, don't make it readonly

        internal EnumeratorClass(in CustomList<XmlAttributeStruct>.Enumerator enumerator)
        {
            _enumerator = enumerator;
        }

        public XmlAttribute Current => new(_enumerator.Current);

        object IEnumerator.Current => *_enumerator.Current;

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

    private class XmlAttributeListTypeProxy
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private XmlAttributeList _entity;

        public XmlAttributeListTypeProxy(XmlAttributeList entity)
        {
            _entity = entity;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public XmlAttribute[] Items
        {
            get
            {
                var array = new XmlAttribute[_entity.Count];
                _entity.CopyTo(array);
                return array;
            }
        }
    }
}