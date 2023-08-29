using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace XXml.Internal;

[DebuggerDisplay("NodeStack[{Count}]")]
[DebuggerTypeProxy(typeof(NodeStackDebuggerTypeProxy))]
internal unsafe struct NodeStack : IDisposable
{
    private XmlNode_** _ptr;

    private int Capacity { get; set; }

    public int Count { get; private set; }

    public NodeStack(int capacity)
    {
        Debug.Assert(capacity >= 0);
        _ptr = (XmlNode_**) Marshal.AllocHGlobal(capacity * sizeof(XmlNode_*));
        AllocationSafety.Add(capacity * sizeof(XmlNode_*));
        Capacity = capacity;
        Count = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Push(XmlNode_* value)
    {
        if (Capacity == Count) GrowUp();
        Debug.Assert(Capacity > Count);
        _ptr[Count] = value;
        Count++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public XmlNode_* Pop()
    {
        if (Count == 0) ThrowHelper.ThrowInvalidOperation("Stack has no items.");
        Count--;
        return _ptr[Count];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public XmlNode_* Peek()
    {
        if (Count == 0) ThrowHelper.ThrowInvalidOperation("Stack has no items.");
        return _ptr[Count - 1];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryPeek(out XmlNode_* item)
    {
        if (Count == 0)
        {
            item = null;
            return false;
        }

        item = _ptr[Count - 1];
        return true;
    }

    public void Dispose()
    {
        Marshal.FreeHGlobal((IntPtr) _ptr);
        AllocationSafety.Remove(Capacity * sizeof(XmlNode_*));
        Capacity = 0;
        Count = 0;
    }

    [MethodImpl(MethodImplOptions.NoInlining)] // uncommon path, no inlining
    private void GrowUp()
    {
        var newCapacity = Math.Max(4, Capacity * 2);
        var ptr = (XmlNode_**) Marshal.AllocHGlobal(newCapacity * sizeof(XmlNode_*));
        AllocationSafety.Add(newCapacity * sizeof(XmlNode_*));
        try
        {
            SpanHelper.CreateSpan<IntPtr>(_ptr, Count).CopyTo(SpanHelper.CreateSpan<IntPtr>(ptr, newCapacity));
            Marshal.FreeHGlobal((IntPtr) _ptr);
            AllocationSafety.Remove(Capacity * sizeof(XmlNode_*));
            _ptr = ptr;
            Capacity = newCapacity;
        }
        catch
        {
            Marshal.FreeHGlobal((IntPtr) ptr);
            AllocationSafety.Remove(newCapacity * sizeof(XmlNode_*));
            throw;
        }
    }


    private class NodeStackDebuggerTypeProxy
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private NodeStack _entity;

        public NodeStackDebuggerTypeProxy(NodeStack entity)
        {
            _entity = entity;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public XmlNode_*[] Items
        {
            get
            {
                var array = new XmlNode_*[_entity.Count];
                for (var i = 0; i < array.Length; i++) array[i] = _entity._ptr[i];
                return array;
            }
        }
    }
}