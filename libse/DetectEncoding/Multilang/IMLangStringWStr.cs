using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable 0108

namespace Nikse.SubtitleEdit.Core.DetectEncoding.Multilang
{
    [ComImport, InterfaceType((short)1), ComConversionLoss, Guid("C04D65D0-B70D-11D0-B188-00AA0038C969")]
    public interface IMLangStringWStr : IMLangString
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void Sync([In] int fNoAccess);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int GetLength();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void SetMLStr([In] int lDestPos, [In] int lDestLen, [In, MarshalAs(UnmanagedType.IUnknown)] object pSrcMLStr, [In] int lSrcPos, [In] int lSrcLen);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetMLStr([In] int lSrcPos, [In] int lSrcLen, [In, MarshalAs(UnmanagedType.IUnknown)] object pUnkOuter, [In] uint dwClsContext, [In] ref Guid piid, [MarshalAs(UnmanagedType.IUnknown)] out object ppDestMLStr, out int plDestPos, out int plDestLen);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void SetWStr([In] int lDestPos, [In] int lDestLen, [In] ref ushort pszSrc, [In] int cchSrc, out int pcchActual, out int plActualLen);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void SetStrBufW([In] int lDestPos, [In] int lDestLen, [In, MarshalAs(UnmanagedType.Interface)] IMLangStringBufW pSrcBuf, out int pcchActual, out int plActualLen);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetWStr([In] int lSrcPos, [In] int lSrcLen, out ushort pszDest, [In] int cchDest, out int pcchActual, out int plActualLen);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetStrBufW([In] int lSrcPos, [In] int lSrcMaxLen, [MarshalAs(UnmanagedType.Interface)] out IMLangStringBufW ppDestBuf, out int plDestLen);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void LockWStr([In] int lSrcPos, [In] int lSrcLen, [In] int lFlags, [In] int cchRequest, [Out] IntPtr ppszDest, out int pcchDest, out int plDestLen);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void UnlockWStr([In] ref ushort pszSrc, [In] int cchSrc, out int pcchActual, out int plActualLen);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void SetLocale([In] int lDestPos, [In] int lDestLen, [In] uint locale);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetLocale([In] int lSrcPos, [In] int lSrcMaxLen, out uint plocale, out int plLocalePos, out int plLocaleLen);
    }
}

#pragma warning restore 0108
