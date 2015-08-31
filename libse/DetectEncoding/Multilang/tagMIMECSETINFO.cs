using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.Core.DetectEncoding.Multilang
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct tagMIMECSETINFO
    {
        public uint uiCodePage;
        public uint uiInternetEncoding;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
        public ushort[] wszCharset;
    }
}
