using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.Core.DetectEncoding.Multilang
{
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
