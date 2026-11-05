namespace Nikse.SubtitleEdit.UiLogic.Ocr.FixEngine
{
    public sealed class OcrFixRegexRule
    {
        public string Find { get; }
        public string ReplaceWith { get; }
        public bool IsUserRule { get; }
        public bool IsEnabled { get; }

        public OcrFixRegexRule(string find, string replaceWith, bool isUserRule, bool isEnabled)
        {
            Find = find;
            ReplaceWith = replaceWith;
            IsUserRule = isUserRule;
            IsEnabled = isEnabled;
        }
    }
}
