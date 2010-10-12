namespace MultiLanguage
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [StructLayout(LayoutKind.Sequential, Pack=8)]
    public struct tagSCRIPFONTINFO
    {
        public long scripts;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x20)]
        public ushort[] wszFont;
    }
}
