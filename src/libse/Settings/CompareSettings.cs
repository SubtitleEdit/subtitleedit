namespace Nikse.SubtitleEdit.Core.Settings
{
    public class CompareSettings
    {
        public bool ShowOnlyDifferences { get; set; }
        public bool OnlyLookForDifferenceInText { get; set; }
        public bool IgnoreLineBreaks { get; set; }
        public bool IgnoreWhitespace { get; set; }
        public bool IgnoreFormatting { get; set; }

        public CompareSettings()
        {
            OnlyLookForDifferenceInText = true;
        }
    }
}