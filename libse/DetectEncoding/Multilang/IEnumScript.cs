using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.Core.DetectEncoding.Multilang
{
    [ComImport, Guid("AE5F1430-388B-11D2-8380-00C04F8F5DA1"), InterfaceType((short)1)]
    public interface IEnumScript
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void Clone([MarshalAs(UnmanagedType.Interface)] out IEnumScript ppEnum);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void Next([In] uint celt, out tagSCRIPTINFO rgelt, out uint pceltFetched);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void Reset();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void Skip([In] uint celt);
    }
}
