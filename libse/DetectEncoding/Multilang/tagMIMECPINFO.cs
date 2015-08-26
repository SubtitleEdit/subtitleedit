using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.Core.DetectEncoding.Multilang
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct tagMIMECPINFO
    {
        public uint dwFlags;
        public uint uiCodePage;
        public uint uiFamilyCodePage;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x40)]
        public ushort[] wszDescription;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
        public ushort[] wszWebCharset;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
        public ushort[] wszHeaderCharset;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
        public ushort[] wszBodyCharset;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x20)]
        public ushort[] wszFixedWidthFont;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x20)]
        public ushort[] wszProportionalFont;
        public byte bGDICharset;
    }
}
