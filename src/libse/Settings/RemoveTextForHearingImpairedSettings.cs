namespace Nikse.SubtitleEdit.Core.Settings
{
    public class RemoveTextForHearingImpairedSettings
    {
        public bool RemoveTextBetweenBrackets { get; set; }
        public bool RemoveTextBetweenParentheses { get; set; }
        public bool RemoveTextBetweenCurlyBrackets { get; set; }
        public bool RemoveTextBetweenQuestionMarks { get; set; }
        public bool RemoveTextBetweenCustom { get; set; }
        public string RemoveTextBetweenCustomBefore { get; set; }
        public string RemoveTextBetweenCustomAfter { get; set; }
        public bool RemoveTextBetweenOnlySeparateLines { get; set; }
        public bool RemoveTextBeforeColon { get; set; }
        public bool RemoveTextBeforeColonOnlyIfUppercase { get; set; }
        public bool RemoveTextBeforeColonOnlyOnSeparateLine { get; set; }
        public bool RemoveInterjections { get; set; }
        public bool RemoveInterjectionsOnlyOnSeparateLine { get; set; }
        public bool RemoveIfContains { get; set; }
        public bool RemoveIfAllUppercase { get; set; }
        public string RemoveIfContainsText { get; set; }
        public bool RemoveIfOnlyMusicSymbols { get; set; }

        /// <summary>
        /// Comma-separated all-uppercase words that should NOT be removed by "Remove if all uppercase"
        /// (e.g. acronyms and short interjections like OK, TV, WWE). Single-letter lines (e.g. "I") are
        /// always kept regardless of this list.
        /// </summary>
        public string RemoveIfAllUppercaseWhitelist { get; set; }

        public RemoveTextForHearingImpairedSettings()
        {
            RemoveTextBetweenBrackets = true;
            RemoveTextBetweenParentheses = true;
            RemoveTextBetweenCurlyBrackets = true;
            RemoveTextBetweenQuestionMarks = true;
            RemoveTextBetweenCustom = false;
            RemoveTextBetweenCustomBefore = "¶";
            RemoveTextBetweenCustomAfter = "¶";
            RemoveTextBeforeColon = true;
            RemoveTextBeforeColonOnlyIfUppercase = true;
            RemoveIfContainsText = "¶";
            RemoveIfOnlyMusicSymbols = true;
            RemoveIfAllUppercaseWhitelist = "YES, NO, WHY, HI, OK, TV";
        }
    }
}