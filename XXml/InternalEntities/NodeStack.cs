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

    private int _capacity;

    private int _count;
    public readonly int Count => _count;

    public NodeStack(int capacity)
    {
        Debug.Assert(capacity >= 0);
        _ptr = (XmlNodeStruct**) Marshal.AllocHGlobal(capacity * sizeof(XmlNodeStruct*));
        AllocationSafety.Add(capacity * sizeof(XmlNodeStruct*));
        _capacity = capacity;
        _count = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Push(XmlNodeStruct* value)
    {
        if (_capacity == _count) GrowUp();
        Debug.Assert(_capacity > _count);
        _ptr[_count] = value;
        _count++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public XmlNodeStruct* Pop()
    {
        if (_count == 0) ThrowHelper.ThrowInvalidOperation("Stack has no items.");
        _count--;
        return _ptr[_count];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly XmlNodeStruct* Peek()
    {
        if (_count == 0) ThrowHelper.ThrowInvalidOperation("Stack has no items.");
        return _ptr[_count - 1];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryPeek(out XmlNodeStruct* item)
    {
        if (_count == 0)
        {
            item = null;
            return false;
        }

        item = _ptr[_count - 1];
        return true;
    }

    public void Dispose()
    {
        Marshal.FreeHGlobal((IntPtr) _ptr);
        AllocationSafety.Remove(_capacity * sizeof(XmlNodeStruct*));
        _capacity = 0;
        _count = 0;
    }

    [MethodImpl(MethodImplOptions.NoInlining)] // uncommon path, no inlining
    private void GrowUp()
    {
        var newCapacity = Math.Max(4, _capacity * 2);
        var ptr = (XmlNodeStruct**) Marshal.AllocHGlobal(newCapacity * sizeof(XmlNodeStruct*));
        AllocationSafety.Add(newCapacity * sizeof(XmlNodeStruct*));
        try
        {
            SpanHelper.CreateSpan<IntPtr>(_ptr, _count).CopyTo(SpanHelper.CreateSpan<IntPtr>(ptr, newCapacity));
            Marshal.FreeHGlobal((IntPtr) _ptr);
            AllocationSafety.Remove(_capacity * sizeof(XmlNodeStruct*));
            _ptr = ptr;
            _capacity = newCapacity;
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
                var array = new XmlNodeStruct*[_entity._count];
                for (var i = 0; i < array.Length; i++) array[i] = _entity._ptr[i];
                return array;
            }
        }
    }
}