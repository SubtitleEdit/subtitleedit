using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Dictionaries;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Core.Forms
{
    public class RemoveTextForHISettings
    {
        public bool OnlyIfInSeparateLine { get; set; }
        public bool RemoveIfAllUppercase { get; set; }
        public bool RemoveTextBeforeColon { get; set; }
        public bool RemoveTextBeforeColonOnlyUppercase { get; set; }
        public bool ColonSeparateLine { get; set; }
        public bool RemoveWhereContains { get; set; }
        public List<string> RemoveIfTextContains { get; set; }
        public bool RemoveTextBetweenCustomTags { get; set; }
        public bool RemoveInterjections { get; set; }
        public bool RemoveInterjectionsOnlySeparateLine { get; set; }
        public bool RemoveTextBetweenSquares { get; set; }
        public bool RemoveTextBetweenBrackets { get; set; }
        public bool RemoveTextBetweenQuestionMarks { get; set; }
        public bool RemoveTextBetweenParentheses { get; set; }
        public string CustomStart { get; set; }
        public string CustomEnd { get; set; }
        public NameList NameList { get; set; }

        public RemoveTextForHISettings(Subtitle subtitle)
        {
            OnlyIfInSeparateLine = Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBetweenOnlySeparateLines;
            RemoveIfAllUppercase = Configuration.Settings.RemoveTextForHearingImpaired.RemoveIfAllUppercase;
            RemoveTextBeforeColon = Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBeforeColon;
            RemoveTextBeforeColonOnlyUppercase = Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBeforeColonOnlyIfUppercase;
            ColonSeparateLine = Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBeforeColonOnlyOnSeparateLine;
            RemoveWhereContains = Configuration.Settings.RemoveTextForHearingImpaired.RemoveIfContains;
            RemoveIfTextContains = new List<string>();
            foreach (string item in Configuration.Settings.RemoveTextForHearingImpaired.RemoveIfContainsText.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                RemoveIfTextContains.Add(item.Trim());
            }
            RemoveTextBetweenCustomTags = Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBetweenCustom;
            RemoveInterjections = Configuration.Settings.RemoveTextForHearingImpaired.RemoveInterjections;
            RemoveTextBetweenSquares = Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBetweenBrackets;
            RemoveTextBetweenBrackets = Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBetweenCurlyBrackets;
            RemoveTextBetweenQuestionMarks = Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBetweenQuestionMarks;
            RemoveTextBetweenParentheses = Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBetweenParentheses;
            CustomStart = Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBetweenCustomBefore;
            CustomEnd = Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBetweenCustomAfter;
            var languageName = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle);
            NameList = new NameList(Configuration.DictionariesDirectory, languageName, Configuration.Settings.WordLists.UseOnlineNames, Configuration.Settings.WordLists.NamesUrl);
        }

    }
}
