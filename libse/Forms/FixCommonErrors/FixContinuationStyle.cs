using Nikse.SubtitleEdit.Core.Enums;
using Nikse.SubtitleEdit.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class FixContinuationStyle : IFixCommonError
    {
        class ContinuationProfile
        {
            public string Suffix;
            public bool SuffixAddSpace;
            public bool SuffixReplaceComma;
            public string Prefix;
            public bool PrefixAddSpace;
            public bool UseDifferentStyleGap;
            public string GapSuffix;
            public bool GapSuffixAddSpace;
            public bool GapSuffixReplaceComma;
            public string GapPrefix;
            public bool GapPrefixAddSpace;
        }

        private ContinuationProfile continuationProfile = null;
        private List<string> prefixes = null;
        private List<string> suffixes = null;
        private List<string> suffixesToAlwaysFix = new List<string>() { ",", "-", "‐", "–", "—" };
        private List<string> suffixesInterruptions = new List<string>() { "...", "..", "…" };

        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            var language = Configuration.Settings.Language.FixCommonErrors;
            string fixAction = string.Format(language.FixContinuationStyleX, GetContinuationStyle(Configuration.Settings.General.ContinuationStyle));
            int fixCount = 0;

            // Check continuation profile
            if (this.continuationProfile == null)
            {
                this.SetContinuationProfile(Configuration.Settings.General.ContinuationStyle);
            }

            // Prepare variables
            string addEnd = (this.continuationProfile.SuffixAddSpace ? " " : "") + this.continuationProfile.Suffix;
            string addStart = this.continuationProfile.Prefix + (this.continuationProfile.PrefixAddSpace ? " " : "");
            bool replaceComma = this.continuationProfile.SuffixReplaceComma;

            int minGapMs = Configuration.Settings.General.MinimumMillisecondsBetweenLines + 5;

            string gapAddEnd = addEnd;
            string gapAddStart = addStart;
            bool gapReplaceComma = replaceComma;

            if (this.continuationProfile.UseDifferentStyleGap)
            {
                gapAddEnd = (this.continuationProfile.GapSuffixAddSpace ? " " : "") + this.continuationProfile.GapSuffix;
                gapAddStart = this.continuationProfile.GapPrefix + (this.continuationProfile.GapPrefixAddSpace ? " " : "");
                gapReplaceComma = this.continuationProfile.GapSuffixReplaceComma;
            }

            bool inSentence = false;
            bool? inItalicSentence = null;

            // Loop paragraphs
            for (int i = 0; i < subtitle.Paragraphs.Count - 1; i++)
            {
                Paragraph p = subtitle.Paragraphs[i];
                Paragraph pNext = subtitle.Paragraphs[i + 1];
                var oldText = p.Text;
                var oldTextNext = pNext.Text;
                var text = Helper.SanitizeString(p.Text);
                var textNext = Helper.SanitizeString(pNext.Text);
                var isChecked = true;

                // Check if we should fix this paragraph
                if (((!IsEndOfSentence(text) || HasSuffix(text)) && !text.EndsWith("--") && !text.EndsWith(":")) && callbacks.AllowFix(p, fixAction))
                {
                    // If ends with nothing...
                    if (!IsEndOfSentence(text))
                    {
                        // ...ignore inserts
                        if (Configuration.Settings.General.FixContinuationStyleUncheckInsertsAllCaps)
                        {
                            if (IsAllCaps(text))
                            {
                                isChecked = false;
                            }
                        }

                        // ...and italic lyrics    
                        if (Configuration.Settings.General.FixContinuationStyleUncheckInsertsItalic)
                        {
                            if (IsItalic(oldText) && !IsNewSentence(text) && inItalicSentence == false)
                            {
                                isChecked = false;
                            }
                        }

                        // ...and smallcaps inserts or non-italic lyrics
                        if (Configuration.Settings.General.FixContinuationStyleUncheckInsertsLowercase)
                        {
                            if (!IsNewSentence(text) && !inSentence)
                            {
                                isChecked = false;
                            }
                        }
                    }

                    // Get last word
                    string[] split = text.Split(Convert.ToChar(" "));
                    string lastWord = split.Last();
                    string newLastWord = lastWord;

                    // Remove any previous suffixes
                    foreach (string suffix in this.suffixes)
                    {
                        if (newLastWord.EndsWith(suffix)) newLastWord = newLastWord.Substring(0, newLastWord.Length - suffix.Length);
                    }
                    newLastWord = newLastWord.Trim();

                    // Make new last word
                    if (pNext.StartTime.TotalMilliseconds - p.EndTime.TotalMilliseconds > minGapMs)
                    {
                        newLastWord = newLastWord + (lastWord.EndsWith(",") && !gapReplaceComma ? "," : "") + gapAddEnd;
                    }
                    else
                    {
                        newLastWord = newLastWord + (lastWord.EndsWith(",") && !replaceComma ? "," : "") + addEnd;
                    }

                    // Replace it
                    var newText = Helper.ReplaceLastOccurrence(oldText, lastWord, newLastWord);

                    // Commit if changed
                    if (oldText != newText)
                    {
                        p.Text = newText;
                        fixCount++;
                        callbacks.AddFixToListView(p, fixAction, oldText, newText, isChecked);
                    }

                    // Get first word of the next paragraph
                    split = textNext.Split(Convert.ToChar(" "));
                    string firstWord = split.First();
                    string newFirstWord = firstWord;

                    // Remove any previous prefixes
                    foreach (string prefix in this.prefixes)
                    {
                        if (newFirstWord.StartsWith(prefix)) newFirstWord = newFirstWord.Substring(prefix.Length);
                    }
                    newFirstWord = newFirstWord.Trim();

                    // Make new first word
                    if (pNext.StartTime.TotalMilliseconds - p.EndTime.TotalMilliseconds > minGapMs)
                    {
                        newFirstWord = gapAddStart + newFirstWord;
                    }
                    else
                    {
                        newFirstWord = addStart + newFirstWord;
                    }

                    // Replace it
                    var newTextNext = Helper.ReplaceLastOccurrence(oldTextNext, firstWord, newFirstWord);

                    // If ends with dots (possible interruptions), check if next sentence is new sentence, otherwise don't check by default
                    if (text.EndsWith("..") || text.EndsWith("…"))
                    {
                        if (!IsNewSentence(textNext))
                        {
                            isChecked = false;
                        }
                    }

                    // Commit if changed
                    if (oldTextNext != newTextNext)
                    {
                        pNext.Text = newTextNext;
                        fixCount++;
                        callbacks.AddFixToListView(pNext, fixAction, oldTextNext, newTextNext, isChecked);
                    }
                }

                // Detect new sentence
                if (IsNewSentence(text))
                {
                    inSentence = true;

                    if (IsItalic(oldText))
                    {
                        inItalicSentence = true;
                    }
                    else
                    {
                        inItalicSentence = null;
                    }
                }

                // Detect end of sentence
                if (IsEndOfSentence(text))
                {
                    inSentence = false;

                    if (IsItalic(oldText))
                    {
                        inItalicSentence = false;
                    }
                    else
                    {
                        inItalicSentence = null;
                    }
                }
            }

            callbacks.UpdateFixStatus(fixCount, language.FixUnnecessaryLeadingDots, language.XFixContinuationStyle);
        }

        private bool IsNewSentence(string input)
        {
            if (Char.IsLetter(input[0]) && Char.IsUpper(input[0]))
            {
                // First letter
                return true;
            }
            else if (Char.IsLetter(input[0]) && !Char.IsUpper(input[0]) && Char.IsLetter(input[1]) && Char.IsUpper(input[1]))
            {
                // iPhone
                return true;
            }
            else if (Char.IsPunctuation(input[0]) && Char.IsLetter(input[1]) && !Char.IsUpper(input[1]) && Char.IsWhiteSpace(input[2]) && Char.IsLetter(input[3]) && Char.IsUpper(input[3]))
            {
                // 's Avonds
                return true;
            }
            else if ("¿¡".Contains(input[0]) && Char.IsLetter(input[1]) && Char.IsUpper(input[1]))
            {
                // Spanish
                return true;
            }

            return false;
        }

        private bool IsEndOfSentence(string input)
        {
            return (input.EndsWith(".") && !input.EndsWith("..")) || input.EndsWith("?") || input.EndsWith("!") || input.EndsWith(";") /* Greek question mark */ || input.EndsWith("--");
        }

        private bool HasPrefix(string input)
        {
            foreach (string prefix in this.prefixes)
            {
                if (input.StartsWith(prefix))
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasSuffix(string input)
        {
            foreach (string suffix in this.suffixes)
            {
                if (input.EndsWith(suffix))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsAllCaps(string input)
        {
            int totalCount = 0;
            int allCapsCount = 0;

            // Count all caps chars
            for (int i = 0; i < input.Length; i++)
            {
                if (Char.IsLetter(input[i]))
                {
                    totalCount++;

                    if (Char.IsUpper(input[i]))
                    {
                        allCapsCount++;
                    }
                }
            }

            return (double)allCapsCount / (double)totalCount >= 0.80;
        }

        private bool IsItalic(string input)
        {
            input = Helper.ExtractParagraphOnly(input);

            if (input.Length > 2)
            {
                if (input.StartsWith("<i>") && ((input.EndsWith("</i>") && !input.Substring(2).Contains("<i>")) || !input.Contains("</i>")))
                {
                    return true;
                }
            }

            return false;
        }

        private void SetContinuationProfile(ContinuationStyle continuationStyle)
        {
            switch (continuationStyle)
            {
                case ContinuationStyle.NoneLeadingTrailingDots:
                    this.continuationProfile = new ContinuationProfile {
                        Suffix = "",
                        SuffixAddSpace = false,
                        SuffixReplaceComma = false,
                        Prefix = "",
                        PrefixAddSpace = false,
                        UseDifferentStyleGap = true,
                        GapSuffix = "...",
                        GapSuffixAddSpace = false,
                        GapSuffixReplaceComma = true,
                        GapPrefix = "...",
                        GapPrefixAddSpace = false
                    };
                    break;
                case ContinuationStyle.OnlyTrailingDots:
                    this.continuationProfile = new ContinuationProfile
                    {
                        Suffix = "...",
                        SuffixAddSpace = false,
                        SuffixReplaceComma = true,
                        Prefix = "",
                        PrefixAddSpace = false,
                        UseDifferentStyleGap = false
                    };
                    break;
                case ContinuationStyle.LeadingTrailingDots:
                    this.continuationProfile = new ContinuationProfile
                    {
                        Suffix = "...",
                        SuffixAddSpace = false,
                        SuffixReplaceComma = true,
                        Prefix = "...",
                        PrefixAddSpace = false,
                        UseDifferentStyleGap = false
                    };
                    break;
                case ContinuationStyle.LeadingTrailingDash:
                    this.continuationProfile = new ContinuationProfile
                    {
                        Suffix = "-",
                        SuffixAddSpace = true,
                        SuffixReplaceComma = true,
                        Prefix = "-",
                        PrefixAddSpace = true,
                        UseDifferentStyleGap = false
                    };
                    break;
                case ContinuationStyle.LeadingTrailingDashDots:
                    this.continuationProfile = new ContinuationProfile
                    {
                        Suffix = "-",
                        SuffixAddSpace = true,
                        SuffixReplaceComma = true,
                        Prefix = "-",
                        PrefixAddSpace = true,
                        UseDifferentStyleGap = true,
                        GapSuffix = "...",
                        GapSuffixAddSpace = false,
                        GapSuffixReplaceComma = true,
                        GapPrefix = "...",
                        GapPrefixAddSpace = false
                    };
                    break;
                default:
                    this.continuationProfile = new ContinuationProfile
                    {
                        Suffix = "",
                        SuffixAddSpace = false,
                        SuffixReplaceComma = false,
                        Prefix = "",
                        PrefixAddSpace = false,
                        UseDifferentStyleGap = false
                    };
                    break;
            }

            this.prefixes = new List<string>() { "...", "..", "-", "‐", "–", "—", "…", this.continuationProfile.Prefix };
            this.suffixes = new List<string>() { ",", "...", "..", "-", "‐", "–", "—", "…", this.continuationProfile.Suffix };
            
            if (this.continuationProfile.UseDifferentStyleGap)
            {
                this.prefixes.Add(this.continuationProfile.GapPrefix);
                this.suffixes.Add(this.continuationProfile.GapSuffix);
            }
        }

        private static string GetContinuationStyle(ContinuationStyle continuationStyle)
        {
            if (continuationStyle == ContinuationStyle.NoneLeadingTrailingDots)
            {
                return Configuration.Settings.Language.Settings.ContinuationStyleNoneLeadingTrailingDots;
            }
            if (continuationStyle == ContinuationStyle.OnlyTrailingDots)
            {
                return Configuration.Settings.Language.Settings.ContinuationStyleOnlyTrailingDots;
            }
            if (continuationStyle == ContinuationStyle.LeadingTrailingDots)
            {
                return Configuration.Settings.Language.Settings.ContinuationStyleLeadingTrailingDots;
            }
            if (continuationStyle == ContinuationStyle.LeadingTrailingDash)
            {
                return Configuration.Settings.Language.Settings.ContinuationStyleLeadingTrailingDash;
            }
            if (continuationStyle == ContinuationStyle.LeadingTrailingDashDots)
            {
                return Configuration.Settings.Language.Settings.ContinuationStyleLeadingTrailingDashDots;
            }
            return Configuration.Settings.Language.Settings.ContinuationStyleNone;
        }
    }
}
