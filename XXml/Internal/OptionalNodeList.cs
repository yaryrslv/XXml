using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using XXml.ValueObjects;
using XXml.XmlEntities;

namespace XXml.Internal;

internal readonly unsafe struct OptionalNodeList : IDisposable, IReference
{
    private readonly IntPtr _list; // OptionalNodeListType*

    public bool IsNull => _list == IntPtr.Zero;

    public XmlDeclarationType* Declaration => &((OptionalNodeListType*) _list)->DeclarationType;

    public XmlDocumentTypeCustom* DocumentType => &((OptionalNodeListType*) _list)->DocumentType;

    private OptionalNodeList(IntPtr list)
    {
        _list = list;
    }

    public static OptionalNodeList Create()
    {
        var ptr = (OptionalNodeListType*) Marshal.AllocHGlobal(sizeof(OptionalNodeListType));
        AllocationSafety.Add(sizeof(OptionalNodeListType));
        *ptr = default;
        return new OptionalNodeList((IntPtr) ptr);
    }

    public void Dispose()
    {
        Marshal.FreeHGlobal(_list);
        AllocationSafety.Remove(sizeof(OptionalNodeListType));
        Unsafe.AsRef(_list) = IntPtr.Zero;
    }


    private struct OptionalNodeListType
    {
        public XmlDeclarationType DeclarationType;
        public XmlDocumentTypeCustom DocumentType;
    }
}