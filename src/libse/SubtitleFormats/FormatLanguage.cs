namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public static class FormatLanguage
    {
        public static string LineNumberXErrorReadingFromSourceLineY { get; set; } = "Line {0} - error reading: {1}";
        public static string LineNumberXErrorReadingTimeCodeFromSourceLineY { get; set; } = "Line {0} - error reading time code: {1}";
        public static string LineNumberXExpectedNumberFromSourceLineY { get; set; } = "Line {0} - expected subtitle number: {1}";
        public static string LineNumberXExpectedEmptyLine { get; set; } = "Line {0} - expected empty line but found number + time code (number skipped): {1}";
    }
}
