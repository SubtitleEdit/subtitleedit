namespace MultiLanguage
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [StructLayout(LayoutKind.Sequential, Pack=4)]
    public struct tagMIMECSETINFO
    {
        public uint uiCodePage;
        public uint uiInternetEncoding;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=50)]
        public ushort[] wszCharset;
    }
}
