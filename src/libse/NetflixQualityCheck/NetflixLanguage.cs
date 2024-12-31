namespace Nikse.SubtitleEdit.Core.NetflixQualityCheck
{
    public static class NetflixLanguage
    {
        public static string GlyphCheckReport { get; set; } = "Invalid character {0} found at column {1}";
        public static string WhiteSpaceCheckForXReport { get; set; } = "White space issue ({0}) found at column {1}.";
        public static string WhiteSpaceBeforePunctuation { get; set; } = "missing before punctuation";
        public static string WhiteSpaceLineEnding { get; set; } = "line ending";
        public static string WhiteSpaceCheckconsecutive { get; set; } = "2+ consecutive";
    }
}
