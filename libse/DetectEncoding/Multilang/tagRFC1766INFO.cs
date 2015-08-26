using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.Core.DetectEncoding.Multilang
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct tagRFC1766INFO
    {
        public uint lcid;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public ushort[] wszRfc1766;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x20)]
        public ushort[] wszLocaleName;
    }
}
