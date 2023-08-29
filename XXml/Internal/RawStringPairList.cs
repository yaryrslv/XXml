using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using XXml.ValueObjects;

namespace XXml.Internal;
// [NOTE]
// - This list re-allocate and copy memory when capacity increases, the address of memories can be changed.

[DebuggerDisplay("{DebugDisplay,nq}")]
[DebuggerTypeProxy(typeof(RawStringPairListTypeProxy))]
internal unsafe ref struct RawStringPairList
{
    private Pair* _ptr;
    private int _capacity;

    public int Count { get; private set; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebugDisplay => $"{nameof(RawStringPairList)} (Count={Count})";

    public ref readonly Pair this[int index]
    {
        get
        {
            // Check index boundary only when DEBUG because this is internal.
            Debug.Assert((uint) index < (uint) Count);
            return ref _ptr[index];
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(in RawString key, in RawString value)
    {
        if (_capacity <= Count) Growup(); // no inlining, uncommon path
        Debug.Assert(_capacity > Count);
        ref var p = ref _ptr[Count++];
        p.Key = key;
        p.Value = value;
    }

    [MethodImpl(MethodImplOptions.NoInlining)] // no inlining, uncommon path
    private void Growup()
    {
        if (_capacity == 0)
        {
            const int initialCapacity = 32;
            var size = initialCapacity * sizeof(Pair);

            // It does not need to be cleared to zero.
            _ptr = (Pair*) Marshal.AllocHGlobal(size);
            AllocationSafety.Add(size);
            _capacity = initialCapacity;
            Count = 0;
        }
        else
        {
            Debug.Assert(_capacity > 0);
            var newCapacity = _capacity * 2;
            var newSize = newCapacity * sizeof(Pair);
            var newPtr = (Pair*) Marshal.AllocHGlobal(newSize);
            AllocationSafety.Add(newSize);

            var sizeToCopy = Count * sizeof(Pair);
            Buffer.MemoryCopy(_ptr, newPtr, sizeToCopy, sizeToCopy);
            Marshal.FreeHGlobal((IntPtr) _ptr);
            AllocationSafety.Remove(_capacity * sizeof(Pair));
            _capacity = newCapacity;
            _ptr = newPtr;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        Marshal.FreeHGlobal((IntPtr) _ptr);
        AllocationSafety.Remove(_capacity * sizeof(Pair));
        _capacity = 0;
        Count = 0;
    }

    [DebuggerDisplay("Key={Key}, Value={Value}")]
    public struct Pair
    {
        public RawString Key;
        public RawString Value;
    }

    private class RawStringPairListTypeProxy
    {
        public RawStringPairListTypeProxy(RawStringPairList entity)
        {
            var items = new Pair[entity.Count];
            for (var i = 0; i < items.Length; i++) items[i] = entity[i];
            Items = items;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public Pair[] Items { get; }
    }
}