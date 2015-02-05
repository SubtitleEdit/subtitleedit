namespace MultiLanguage
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct _LARGE_INTEGER
    {
        public long QuadPart;
    }
}
