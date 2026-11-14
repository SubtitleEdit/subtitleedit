namespace Nikse.SubtitleEdit.UiLogic.Ocr.FixEngine
{
    public sealed class OcrFixRegexRule
    {
        public string Find { get; }
        public string ReplaceWith { get; }
        public bool IsUserRule { get; }
        public bool IsEnabled { get; }

        /// <summary>
        /// The word to look up (e.g. "$1") for a rule from "RegularExpressionsIfSpelledCorrectly":
        /// the replacement is only made when the dictionary knows it. Empty for a plain rule.
        /// </summary>
        public string SpellCheck { get; }

        /// <summary>Set on a few shipped spell-checked rules: replace every From with To in the match.</summary>
        public string ReplaceAllFrom { get; }
        public string ReplaceAllTo { get; }

        public bool IsSpellChecked => SpellCheck.Length > 0;

        public OcrFixRegexRule(string find, string replaceWith, bool isUserRule, bool isEnabled,
            string spellCheck = "", string replaceAllFrom = "", string replaceAllTo = "")
        {
            Find = find;
            ReplaceWith = replaceWith;
            IsUserRule = isUserRule;
            IsEnabled = isEnabled;
            SpellCheck = spellCheck ?? string.Empty;
            ReplaceAllFrom = replaceAllFrom ?? string.Empty;
            ReplaceAllTo = replaceAllTo ?? string.Empty;
        }
    }
}
