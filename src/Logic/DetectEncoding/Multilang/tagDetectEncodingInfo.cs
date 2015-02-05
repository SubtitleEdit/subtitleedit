namespace MultiLanguage
{
    using System.Runtime.InteropServices;

    /// Thanks to jannewe for finding the fix!
    /// http://www.codeproject.com/KB/recipes/DetectEncoding.aspx?msg=3247475#xx3247475xx
    [StructLayout(LayoutKind.Sequential)]
    public struct DetectEncodingInfo
    {
        public uint nLangID;
        public uint nCodePage;
        public int nDocPercent;
        public int nConfidence;
    }
}
