namespace MultiLanguage
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [StructLayout(LayoutKind.Sequential, Pack=4)]
    public struct _FILETIME
    {
        public uint dwLowDateTime;
        public uint dwHighDateTime;
    }
}
