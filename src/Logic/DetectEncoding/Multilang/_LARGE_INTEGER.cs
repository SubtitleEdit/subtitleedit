namespace MultiLanguage
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [StructLayout(LayoutKind.Sequential, Pack=8)]
    public struct _LARGE_INTEGER
    {
        public long QuadPart;
    }
}
