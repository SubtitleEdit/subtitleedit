using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Nikse.SubtitleEdit.Core.Enums;

namespace Nikse.SubtitleEdit.Core
{
    public static class ContinuationUtilities
    {
        private static readonly string Dashes = "-‐–—";
        private static readonly string Quotes = "'\"“”‘’«»‹›„“‚‘";
        private static readonly string SingleQuotes = "'‘’‘";
        private static readonly string DoubleQuotes = "''‘‘’’‚‚‘‘";

        public static readonly List<string> Prefixes = new List<string> { "...", "..", "-", "‐", "–", "—", "…" };
        public static readonly List<string> Suffixes = new List<string> { "...", "..", "-", "‐", "–", "—", "…" };

        public static string SanitizeString(string input, bool removeDashes)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            string checkString = input;
            checkString = Regex.Replace(checkString, "<.*?>", string.Empty);
            checkString = Regex.Replace(checkString, "\\(.*?\\)", string.Empty);
            checkString = Regex.Replace(checkString, "\\[.*?\\]", string.Empty);
            checkString = Regex.Replace(checkString, "\\{.*?\\}", string.Empty);
            checkString = checkString.Trim();

            // Remove string elevation
            if (checkString.EndsWith("\r\n_", StringComparison.Ordinal) ||
                checkString.EndsWith("\r\n.", StringComparison.Ordinal) ||
                checkString.EndsWith("\n_", StringComparison.Ordinal) ||
                checkString.EndsWith("\n.", StringComparison.Ordinal))
            {
                checkString = checkString.Substring(0, checkString.Length - 1).Trim();
            }

            // Remove dashes from the beginning
            if (removeDashes)
            {
                if (Dashes.Contains(checkString[0]))
                {
                    checkString = checkString.Substring(1).Trim();
                }
            }

            // Remove single-char quotes from the beginning
            if (Quotes.Contains(checkString[0]))
            {
                if (SingleQuotes.Contains(checkString[0]) && char.IsLetter(checkString[1]) && !char.IsUpper(checkString[1]) && char.IsWhiteSpace(checkString[2]) && char.IsLetter(checkString[3]))
                {
                    // 's Avonds -- don't remove
                }
                else
                {
                    checkString = checkString.Substring(1).Trim();
                }
            }

            // Remove double-char quotes from the beginning
            if (DoubleQuotes.Contains(checkString.Substring(0, 2)))
            {
                checkString = checkString.Substring(2).Trim();
            }

            // Remove single-char quotes from the ending
            if (Quotes.Contains(checkString[checkString.Length - 1]))
            {
                if (checkString[checkString.Length - 1] == '\'' && checkString.EndsWith("in'") && char.IsLetter(checkString[checkString.Length - 4]))
                {
                    // nothin' -- Don't remove
                }
                else if (checkString[checkString.Length - 1] == '\'' && (checkString.EndsWith("déj'") || checkString.EndsWith("ap'") || checkString.EndsWith("app'")))
                {
                    // déj' -- Don't remove
                }
                else
                {
                    checkString = checkString.Substring(0, checkString.Length - 1).Trim();
                }
            }

            // Remove double-char quotes from the ending
            if (DoubleQuotes.Contains(checkString.Substring(checkString.Length - 2, 2)))
            {
                checkString = checkString.Substring(0, checkString.Length - 2).Trim();
            }

            return checkString;
        }

        public static string SanitizeString(string input)
        {
            return SanitizeString(input, true);
        }

        public static string ExtractParagraphOnly(string input, bool removeDashes)
        {
            string checkString = input;
            checkString = Regex.Replace(checkString, "\\{.*?\\}", string.Empty);
            checkString = checkString.Trim();

            // Remove string elevation
            if (checkString.EndsWith("\r\n_") || checkString.EndsWith("\r\n.") || checkString.EndsWith("\n_") || checkString.EndsWith("\n."))
            {
                checkString = checkString.Substring(0, checkString.Length - 1).Trim();
            }

            return checkString;
        }

        public static string ExtractParagraphOnly(string input)
        {
            return ExtractParagraphOnly(input, true);
        }

        public static string ReplaceFirstOccurrence(string source, string find, string replace)
        {
            int place = source.IndexOf(find, StringComparison.Ordinal);

            if (place == -1)
            {
                return source;
            }

            string result = source.Remove(place, find.Length).Insert(place, replace);
            return result;
        }

        public static string ReplaceLastOccurrence(string source, string find, string replace)
        {
            int place = source.LastIndexOf(find, StringComparison.Ordinal);

            if (place == -1)
            {
                return source;
            }

            string result = source.Remove(place, find.Length).Insert(place, replace);
            return result;
        }

        public static bool ShouldAddSuffix(string input, ContinuationProfile profile, bool sanitize)
        {
            string text = sanitize ? SanitizeString(input) : input;

            if ((!IsEndOfSentence(text) || text.EndsWith(",") || HasSuffix(text, profile)) && !text.EndsWith("--") && !text.EndsWith(":"))
            {
                return true;
            }

            return false;
        }

        public static bool ShouldAddSuffix(string input, ContinuationProfile profile)
        {
            return ShouldAddSuffix(input, profile, true);
        }

        public static string AddSuffixIfNeeded(string originalText, ContinuationProfile profile, bool gap)
        {
            // Get last word
            string text = SanitizeString(originalText);
            string[] split = text.Split(Convert.ToChar(" "));
            string lastWord = split.Last();
            string newLastWord = lastWord;

            if (gap && profile.UseDifferentStyleGap)
            {
                // Check if needed
                if (profile.GapSuffix.Length == 0 || lastWord.EndsWith(profile.GapSuffix))
                {
                    return text;
                }

                // Make new last word
                string gapAddEnd = (profile.GapSuffixAddSpace ? " " : "") + profile.GapSuffix;
                newLastWord = newLastWord.TrimEnd(',') + (lastWord.EndsWith(",") && !profile.GapSuffixReplaceComma ? "," : "") + gapAddEnd;
            }
            else
            {
                // Check if needed
                if (profile.Suffix.Length == 0 || lastWord.EndsWith(profile.Suffix))
                {
                    return text;
                }

                // Make new last word
                string addEnd = (profile.SuffixAddSpace ? " " : "") + profile.Suffix;
                newLastWord = newLastWord.TrimEnd(',') + (lastWord.EndsWith(",") && !profile.SuffixReplaceComma ? "," : "") + addEnd;
            }

            // Replace it
            return ReplaceLastOccurrence(originalText, lastWord, newLastWord);
        }

        public static string AddPrefixIfNeeded(string originalText, ContinuationProfile profile, bool gap)
        {
            // Get first word of the next paragraph
            string text = SanitizeString(originalText);
            string[] split = text.Split(Convert.ToChar(" "));
            string firstWord = split.First();
            string newFirstWord = firstWord;

            if (gap && profile.UseDifferentStyleGap)
            {
                // Check if needed
                if (profile.GapPrefix.Length == 0 || firstWord.StartsWith(profile.GapPrefix))
                {
                    return text;
                }

                // Make new first word
                string gapAddStart = profile.GapPrefix + (profile.GapPrefixAddSpace ? " " : "");
                newFirstWord = gapAddStart + newFirstWord;
            }
            else
            {
                // Check if needed
                if (profile.Prefix.Length == 0 || firstWord.StartsWith(profile.Prefix))
                {
                    return text;
                }

                // Make new first word
                string addStart = profile.Prefix + (profile.PrefixAddSpace ? " " : "");
                newFirstWord = addStart + newFirstWord;
            }

            // Replace it
            return ReplaceFirstOccurrence(originalText, firstWord, newFirstWord);
        }

        public static string RemoveSuffix(string originalText, ContinuationProfile profile, bool addComma)
        {
            // Get last word
            string text = SanitizeString(originalText);
            string[] split = text.Split(Convert.ToChar(" "));
            string lastWord = split.Last();
            string newLastWord = lastWord;

            foreach (string suffix in Suffixes)
            {
                if (newLastWord.EndsWith(suffix))
                {
                    newLastWord = newLastWord.Substring(0, newLastWord.Length - suffix.Length);
                }
            }
            newLastWord = newLastWord.Trim();

            if (addComma)
            {
                newLastWord = newLastWord + ",";
            }

            // Replace it
            return ReplaceLastOccurrence(originalText, lastWord, newLastWord);
        }

        public static string RemoveSuffix(string originalText, ContinuationProfile profile)
        {
            return RemoveSuffix(originalText, profile, false);
        }

        public static string RemovePrefix(string originalText, ContinuationProfile profile)
        {
            // Get first word of the next paragraph
            string text = SanitizeString(originalText);
            string[] split = text.Split(Convert.ToChar(" "));
            string firstWord = split.First();
            string newFirstWord = firstWord;

            foreach (string prefix in Prefixes)
            {
                if (newFirstWord.StartsWith(prefix))
                {
                    newFirstWord = newFirstWord.Substring(prefix.Length);
                }
            }
            newFirstWord = newFirstWord.Trim();

            // Replace it
            return ReplaceFirstOccurrence(originalText, firstWord, newFirstWord);
        }

        public static bool IsNewSentence(string input)
        {
            if (char.IsLetter(input[0]) && char.IsUpper(input[0]))
            {
                // First letter
                return true;
            }

            if (char.IsLetter(input[0]) && !char.IsUpper(input[0]) && char.IsLetter(input[1]) && char.IsUpper(input[1]))
            {
                // iPhone
                return true;
            }

            if (char.IsPunctuation(input[0]) && char.IsLetter(input[1]) && !char.IsUpper(input[1]) && char.IsWhiteSpace(input[2]) && char.IsLetter(input[3]) && char.IsUpper(input[3]))
            {
                // 's Avonds
                return true;
            }

            if ("¿¡".Contains(input[0]) && char.IsLetter(input[1]) && char.IsUpper(input[1]))
            {
                // Spanish
                return true;
            }

            return false;
        }

        public static bool IsEndOfSentence(string input)
        {
            return (input.EndsWith('.') && !input.EndsWith("..", StringComparison.Ordinal)) ||
                   input.EndsWith('?') ||
                   input.EndsWith('!') ||
                   input.EndsWith(';') /* Greek question mark */ ||
                   input.EndsWith("--", StringComparison.Ordinal);
        }

        public static bool IsAllCaps(string input)
        {
            int totalCount = 0;
            int allCapsCount = 0;

            // Count all caps chars
            foreach (var c in input)
            {
                if (char.IsLetter(c))
                {
                    totalCount++;

                    if (char.IsUpper(c))
                    {
                        allCapsCount++;
                    }
                }
            }

            return allCapsCount / (double)totalCount >= 0.80;
        }

        public static bool IsItalic(string input)
        {
            input = ExtractParagraphOnly(input);

            if (input.Length > 2 && input.StartsWith("<i>") && (input.EndsWith("</i>") && !input.Substring(2).Contains("<i>") || !input.Contains("</i>")))
            {
                return true;
            }

            return false;
        }

        public static bool HasPrefix(string input, ContinuationProfile profile)
        {
            if (profile.Prefix.Length > 0 && input.StartsWith(profile.Prefix))
            {
                return true;
            }

            if (profile.UseDifferentStyleGap && profile.GapPrefix.Length > 0 && input.StartsWith(profile.GapPrefix))
            {
                return true;
            }

            foreach (string prefix in Prefixes)
            {
                if (input.StartsWith(prefix))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool HasSuffix(string input, ContinuationProfile profile)
        {
            if (profile.Suffix.Length > 0 && input.EndsWith(profile.Suffix))
            {
                return true;
            }

            if (profile.UseDifferentStyleGap && profile.GapSuffix.Length > 0 && input.EndsWith(profile.GapSuffix))
            {
                return true;
            }

            foreach (string suffix in Suffixes)
            {
                if (input.EndsWith(suffix))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool StartsWithConjunction(string input, string language)
        {
            List<string> conjunctions = null;

            if (language == "nl")
            {
                conjunctions = new List<string>
                {
                    "maar", "dus", "omdat", "aangezien", "want", "vermits", "zodat", "opdat", "zoals", "bijvoorbeeld",
                    "net", "behalve", "al", "alhoewel", "hoewel", "ofschoon", "tenzij", "waardoor", "waarna", "misschien", "waarschijnlijk", "vast"
                };
            }
            else if (language == "en")
            {
                conjunctions = new List<string>
                {
                    "and", "but", "for", "nor", "yet", "or", "so", "such as"
                };
            }
            else if (language == "fr")
            {
                conjunctions = new List<string>
                {
                    "mais", "car", "donc", "parce que", "par exemple"
                };
            }

            if (conjunctions != null)
            {
                foreach (string conjunction in conjunctions)
                {
                    if (input.StartsWith(conjunction + " ") || input.StartsWith(conjunction + ",") || input.StartsWith(conjunction + ":"))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static Tuple<string, string> MergeHelper(string input, string nextInput, ContinuationProfile profile, string language)
        {
            var thisText = SanitizeString(input);
            var nextText = SanitizeString(nextInput);
            var nextTextWithDashPrefix = SanitizeString(nextInput, profile.GapPrefix != "-");

            // Remove any prefix and suffix when:
            // - Title 1 ends with a suffix AND title 2 starts with a prefix
            // - Title 1 ends with a suffix and title 2 is a continuing sentence
            if (HasSuffix(thisText, profile) && HasPrefix(nextTextWithDashPrefix, profile)
                || HasSuffix(thisText, profile) && !IsNewSentence(nextText))
            {
                var newNextText = RemovePrefix(nextInput, profile);
                var newText = RemoveSuffix(input, profile, StartsWithConjunction(newNextText, language));
                return new Tuple<string, string>(newText, newNextText);
            }

            return new Tuple<string, string>(input, nextInput);
        }

        public static int GetMinimumGapMs()
        {
            return Math.Max(Configuration.Settings.General.MinimumMillisecondsBetweenLines + 5, 300);
        }

        public static string GetContinuationStyleName(ContinuationStyle continuationStyle)
        {
            switch (continuationStyle)
            {
                case ContinuationStyle.NoneTrailingDots:
                    return Configuration.Settings.Language.Settings.ContinuationStyleNoneTrailingDots;
                case ContinuationStyle.NoneLeadingTrailingDots:
                    return Configuration.Settings.Language.Settings.ContinuationStyleNoneLeadingTrailingDots;
                case ContinuationStyle.OnlyTrailingDots:
                    return Configuration.Settings.Language.Settings.ContinuationStyleOnlyTrailingDots;
                case ContinuationStyle.LeadingTrailingDots:
                    return Configuration.Settings.Language.Settings.ContinuationStyleLeadingTrailingDots;
                case ContinuationStyle.LeadingTrailingDash:
                    return Configuration.Settings.Language.Settings.ContinuationStyleLeadingTrailingDash;
                case ContinuationStyle.LeadingTrailingDashDots:
                    return Configuration.Settings.Language.Settings.ContinuationStyleLeadingTrailingDashDots;
                default:
                    return Configuration.Settings.Language.Settings.ContinuationStyleNone;
            }
        }

        public static int GetIndexFromContinuationStyle(ContinuationStyle continuationStyle)
        {
            switch (continuationStyle)
            {
                case ContinuationStyle.NoneTrailingDots:
                    return 1;
                case ContinuationStyle.NoneLeadingTrailingDots:
                    return 2;
                case ContinuationStyle.OnlyTrailingDots:
                    return 3;
                case ContinuationStyle.LeadingTrailingDots:
                    return 4;
                case ContinuationStyle.LeadingTrailingDash:
                    return 5;
                case ContinuationStyle.LeadingTrailingDashDots:
                    return 6;
                default:
                    return 0;
            }
        }

        public static ContinuationStyle GetContinuationStyleFromIndex(int index)
        {
            switch (index)
            {
                case 1:
                    return ContinuationStyle.NoneTrailingDots;
                case 2:
                    return ContinuationStyle.NoneLeadingTrailingDots;
                case 3:
                    return ContinuationStyle.OnlyTrailingDots;
                case 4:
                    return ContinuationStyle.LeadingTrailingDots;
                case 5:
                    return ContinuationStyle.LeadingTrailingDash;
                case 6:
                    return ContinuationStyle.LeadingTrailingDashDots;
                default:
                    return ContinuationStyle.None;
            }
        }

        public static string GetContinuationStylePreview(ContinuationStyle continuationStyle)
        {
            string line1 = "Lorem ipsum dolor sit amet\nconsectetur adipiscing elit,";
            string line2 = "donec eget turpis consequat\nturpis commodo hendrerit";
            string line3 = "praesent vel velit rutrum tellus\npharetra tristique vel non orci";
            string linePause = "(...)";
            string line4 = "mauris mollis consectetur nibh,\nnec congue est viverra quis.";

            var profile = GetContinuationProfile(continuationStyle);

            return AddSuffixIfNeeded(line1, profile, false) + "\n\n"
                                                            + AddSuffixIfNeeded(AddPrefixIfNeeded(line2, profile, false), profile, false) + "\n\n"
                                                            + AddSuffixIfNeeded(AddPrefixIfNeeded(line3, profile, false), profile, true) + "\n\n"
                                                            + linePause + "\n\n"
                                                            + AddPrefixIfNeeded(line4, profile, true);
        }

        public static ContinuationProfile GetContinuationProfile(ContinuationStyle continuationStyle)
        {
            switch (continuationStyle)
            {
                case ContinuationStyle.NoneTrailingDots:
                    return new ContinuationProfile
                    {
                        Suffix = "",
                        SuffixAddSpace = false,
                        SuffixReplaceComma = false,
                        Prefix = "",
                        PrefixAddSpace = false,
                        UseDifferentStyleGap = true,
                        GapSuffix = "...",
                        GapSuffixAddSpace = false,
                        GapSuffixReplaceComma = true,
                        GapPrefix = "",
                        GapPrefixAddSpace = false
                    };
                case ContinuationStyle.NoneLeadingTrailingDots:
                    return new ContinuationProfile
                    {
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
                case ContinuationStyle.OnlyTrailingDots:
                    return new ContinuationProfile
                    {
                        Suffix = "...",
                        SuffixAddSpace = false,
                        SuffixReplaceComma = true,
                        Prefix = "",
                        PrefixAddSpace = false,
                        UseDifferentStyleGap = false
                    };
                case ContinuationStyle.LeadingTrailingDots:
                    return new ContinuationProfile
                    {
                        Suffix = "...",
                        SuffixAddSpace = false,
                        SuffixReplaceComma = true,
                        Prefix = "...",
                        PrefixAddSpace = false,
                        UseDifferentStyleGap = false
                    };
                case ContinuationStyle.LeadingTrailingDash:
                    return new ContinuationProfile
                    {
                        Suffix = "-",
                        SuffixAddSpace = true,
                        SuffixReplaceComma = true,
                        Prefix = "-",
                        PrefixAddSpace = true,
                        UseDifferentStyleGap = false
                    };
                case ContinuationStyle.LeadingTrailingDashDots:
                    return new ContinuationProfile
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
                default:
                    return new ContinuationProfile
                    {
                        Suffix = "",
                        SuffixAddSpace = false,
                        SuffixReplaceComma = false,
                        Prefix = "",
                        PrefixAddSpace = false,
                        UseDifferentStyleGap = false
                    };
            }
        }

        public class ContinuationProfile
        {
            public string Suffix { get; set; }
            public bool SuffixAddSpace { get; set; }
            public bool SuffixReplaceComma { get; set; }
            public string Prefix { get; set; }
            public bool PrefixAddSpace { get; set; }
            public bool UseDifferentStyleGap { get; set; }
            public string GapSuffix { get; set; }
            public bool GapSuffixAddSpace { get; set; }
            public bool GapSuffixReplaceComma { get; set; }
            public string GapPrefix { get; set; }
            public bool GapPrefixAddSpace { get; set; }
        }
    }
}