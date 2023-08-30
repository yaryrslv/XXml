using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using XXml.XmlEntities;

namespace XXml.InternalEntities;

[DebuggerDisplay("NodeStack[{Count}]")]
[DebuggerTypeProxy(typeof(NodeStackDebuggerTypeProxy))]
internal unsafe struct NodeStack : IDisposable
{
    private XmlNodeStruct** _ptr;

    private int Capacity { get; set; }

    public int Count { get; private set; }

    public NodeStack(int capacity)
    {
        Debug.Assert(capacity >= 0);
        _ptr = (XmlNodeStruct**) Marshal.AllocHGlobal(capacity * sizeof(XmlNodeStruct*));
        AllocationSafety.Add(capacity * sizeof(XmlNodeStruct*));
        Capacity = capacity;
        Count = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Push(XmlNodeStruct* value)
    {
        if (Capacity == Count) GrowUp();
        Debug.Assert(Capacity > Count);
        _ptr[Count] = value;
        Count++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public XmlNodeStruct* Pop()
    {
        if (Count == 0) ThrowHelper.ThrowInvalidOperation("Stack has no items.");
        Count--;
        return _ptr[Count];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly XmlNodeStruct* Peek()
    {
        if (Count == 0) ThrowHelper.ThrowInvalidOperation("Stack has no items.");
        return _ptr[Count - 1];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryPeek(out XmlNodeStruct* item)
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
        AllocationSafety.Remove(Capacity * sizeof(XmlNodeStruct*));
        Capacity = 0;
        Count = 0;
    }

    [MethodImpl(MethodImplOptions.NoInlining)] // uncommon path, no inlining
    private void GrowUp()
    {
        var newCapacity = Math.Max(4, Capacity * 2);
        var ptr = (XmlNodeStruct**) Marshal.AllocHGlobal(newCapacity * sizeof(XmlNodeStruct*));
        AllocationSafety.Add(newCapacity * sizeof(XmlNodeStruct*));
        try
        {
            SpanHelper.CreateSpan<IntPtr>(_ptr, Count).CopyTo(SpanHelper.CreateSpan<IntPtr>(ptr, newCapacity));
            Marshal.FreeHGlobal((IntPtr) _ptr);
            AllocationSafety.Remove(Capacity * sizeof(XmlNodeStruct*));
            _ptr = ptr;
            Capacity = newCapacity;
        }
        catch
        {
            Marshal.FreeHGlobal((IntPtr) ptr);
            AllocationSafety.Remove(newCapacity * sizeof(XmlNodeStruct*));
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
        public XmlNodeStruct*[] Items
        {
            get
            {
                var array = new XmlNodeStruct*[_entity.Count];
                for (var i = 0; i < array.Length; i++) array[i] = _entity._ptr[i];
                return array;
            }
        }
    }
}