using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.Core.DetectEncoding.Multilang
{
    [ComImport, Guid("275C23E1-3747-11D0-9FEA-00AA003F8646"), InterfaceType((short)1)]
    public interface IMultiLanguage
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetNumberOfCodePageInfo(out uint pcCodePage);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetCodePageInfo([In] uint uiCodePage, out tagMIMECPINFO pCodePageInfo);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetFamilyCodePage([In] uint uiCodePage, out uint puiFamilyCodePage);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void EnumCodePages([In] uint grfFlags, [MarshalAs(UnmanagedType.Interface)] out IEnumCodePage ppEnumCodePage);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetCharsetInfo([In, MarshalAs(UnmanagedType.BStr)] string charset, out tagMIMECSETINFO pCharsetInfo);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void IsConvertible([In] uint dwSrcEncoding, [In] uint dwDstEncoding);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void ConvertString([In, Out] ref uint pdwMode, [In] uint dwSrcEncoding, [In] uint dwDstEncoding, [In] ref byte pSrcStr, [In, Out] ref uint pcSrcSize, [In] ref byte pDstStr, [In, Out] ref uint pcDstSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void ConvertStringToUnicode([In, Out] ref uint pdwMode, [In] uint dwEncoding, [In] ref sbyte pSrcStr, [In, Out] ref uint pcSrcSize, [In] ref ushort pDstStr, [In, Out] ref uint pcDstSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void ConvertStringFromUnicode([In, Out] ref uint pdwMode, [In] uint dwEncoding, [In] ref ushort pSrcStr, [In, Out] ref uint pcSrcSize, [In] ref sbyte pDstStr, [In, Out] ref uint pcDstSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void ConvertStringReset();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetRfc1766FromLcid([In] uint locale, [MarshalAs(UnmanagedType.BStr)] out string pbstrRfc1766);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetLcidFromRfc1766(out uint plocale, [In, MarshalAs(UnmanagedType.BStr)] string bstrRfc1766);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void EnumRfc1766([MarshalAs(UnmanagedType.Interface)] out IEnumRfc1766 ppEnumRfc1766);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetRfc1766Info([In] uint locale, out tagRFC1766INFO pRfc1766Info);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void CreateConvertCharset([In] uint uiSrcCodePage, [In] uint uiDstCodePage, [In] uint dwProperty, [MarshalAs(UnmanagedType.Interface)] out ICMLangConvertCharset ppMLangConvertCharset);
    }
}
