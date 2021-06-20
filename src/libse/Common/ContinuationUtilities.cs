using Nikse.SubtitleEdit.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.Common
{
    public static class ContinuationUtilities
    {
        private static readonly string Dashes = "-‐–—";
        private static readonly string Quotes = "'\"“”‘’«»‹›„“‚‘";
        private static readonly string SingleQuotes = "'‘’‘";
        private static readonly string DoubleQuotes = "''‘‘’’‚‚‘‘";
        private static readonly string MusicSymbols = "♪♫#*¶";
        private static readonly string ExplanationQuotes = "'\"“”‘’«»‹›";

        private static readonly List<string> LanguagesWithoutCaseDistinction = new List<string>
        {
            "am", "ar", "as", "az", "bn", "my", "zh", "ka", "gu", "he", "hi", "ja", "kn", "ks", "km", "ko", "ku", "lo",
            "ml", "ps", "fa", "pa", "sd", "si", "su", "ta", "te", "th", "bo", "ti", "ur", "ug", "yi"
        };

        private static readonly Dictionary<char, char> QuotePairs = new Dictionary<char, char>
        {
            {'\'', '\''},
            {'"', '"'},
            {'“', '”'},
            {'”', '“'},
            {'‘', '’'},
            {'’', '‘'},
            {'«', '»'},
            {'»', '«'},
            {'‹', '›'},
            {'›', '‹'}
        };

        public static readonly List<string> Prefixes = new List<string> { "...", "..", "-", "‐", "–", "—", "…" };
        public static readonly List<string> DashPrefixes = new List<string> { "-", "‐", "–", "—" };
        public static readonly List<string> Suffixes = new List<string> { "...", "..", "-", "‐", "–", "—", "…" };

        public static string SanitizeString(string input, bool removeDashes)
        {
            // Return if empty string
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            string checkString = input;
            checkString = Regex.Replace(checkString, "<.*?>", string.Empty);
            checkString = Regex.Replace(checkString, "\\(.*?\\)", string.Empty, RegexOptions.Singleline);
            checkString = Regex.Replace(checkString, "\\[.*?\\]", string.Empty, RegexOptions.Singleline);
            checkString = Regex.Replace(checkString, "\\{.*?\\}", string.Empty);

            if (Configuration.Settings.General.FixContinuationStyleIgnoreLyrics)
            {
                foreach (char c in MusicSymbols)
                {
                    checkString = Regex.Replace(checkString, "\\" + c + ".*?\\" + c, string.Empty, RegexOptions.Singleline);
                }
            }

            checkString = checkString.Trim();

            // Remove string elevation
            if (checkString.EndsWith("\n_", StringComparison.Ordinal) || checkString.EndsWith("\n.", StringComparison.Ordinal)
                || checkString.EndsWith("\n _", StringComparison.Ordinal) || checkString.EndsWith("\n .", StringComparison.Ordinal)
                || checkString.EndsWith("\n _", StringComparison.Ordinal) || checkString.EndsWith("\n .", StringComparison.Ordinal) /* Alt+255 */)
            {
                checkString = checkString.Substring(0, checkString.Length - 1).Trim();
            }

            // Return if empty string by now
            if (string.IsNullOrEmpty(checkString))
            {
                return "";
            }

            // Remove >> from the beginning
            while (checkString.StartsWith('>'))
            {
                checkString = checkString.Substring(1).Trim();
            }

            // Remove SPEAKER: from the beginning
            if (checkString.Contains(":"))
            {
                string[] split = checkString.Split(':');
                if (string.IsNullOrEmpty(split[0].Trim()))
                {
                    checkString = checkString.Substring(1).Trim();
                }
                else if (IsAllCaps(split[0]))
                {
                    var newCheckString = string.Join(":", split.Skip(1)).Trim();
                    if (!string.IsNullOrEmpty(newCheckString))
                    {
                        checkString = newCheckString;
                    }
                }
            }

            // Remove dashes from the beginning
            if (removeDashes)
            {
                if (checkString.Length > 1 && Dashes.Contains(checkString[0]) && (checkString[1] != '\r' && checkString[1] != '\n'))
                {
                    checkString = checkString.Substring(1).Trim();
                }
            }

            // Remove single-char quotes from the beginning
            if (checkString.Length > 0 && Quotes.Contains(checkString[0]))
            {
                if (checkString.Length > 3 && SingleQuotes.Contains(checkString[0]) && char.IsLetter(checkString[1]) && !char.IsUpper(checkString[1]) && char.IsWhiteSpace(checkString[2]) && char.IsLetter(checkString[3]))
                {
                    // 's Avonds -- don't remove
                }
                else if (checkString.Length > 2 && SingleQuotes.Contains(checkString[0]) && checkString.Substring(1).StartsWith("cause", StringComparison.Ordinal))
                {
                    // 'cause -- don't remove
                }
                else
                {
                    checkString = checkString.Substring(1).Trim();
                }
            }

            // Remove double-char quotes from the beginning
            if (checkString.Length > 1 && DoubleQuotes.Contains(checkString.Substring(0, 2)))
            {
                checkString = checkString.Substring(2).Trim();
            }

            // Remove music symbols from the beginning
            if (checkString.Length > 0 && MusicSymbols.Contains(checkString[0]))
            {
                checkString = checkString.Substring(1).Trim();
            }

            // Remove single-char quotes from the ending
            if (checkString.Length > 0 && Quotes.Contains(checkString[checkString.Length - 1]))
            {
                if (checkString[checkString.Length - 1] == '\'' && checkString.EndsWith("in'", StringComparison.Ordinal) && checkString.Length > 4 && char.IsLetter(checkString[checkString.Length - 4]))
                {
                    // nothin' -- Don't remove
                }
                else if (checkString[checkString.Length - 1] == '\'' && (checkString.EndsWith("déj'", StringComparison.Ordinal) || checkString.EndsWith("ap'", StringComparison.Ordinal) || checkString.EndsWith("app'", StringComparison.Ordinal)))
                {
                    // déj' -- Don't remove
                }
                else
                {
                    checkString = checkString.Substring(0, checkString.Length - 1).Trim();
                }
            }

            // Remove double-char quotes from the ending
            if (checkString.Length > 1 && DoubleQuotes.Contains(checkString.Substring(checkString.Length - 2, 2)))
            {
                checkString = checkString.Substring(0, checkString.Length - 2).Trim();
            }

            // Remove music symbols from the ending
            if (checkString.Length > 0 && MusicSymbols.Contains(checkString[checkString.Length - 1]))
            {
                checkString = checkString.Substring(0, checkString.Length - 1).Trim();
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
            if (checkString.EndsWith("\n_", StringComparison.Ordinal) || checkString.EndsWith("\n.", StringComparison.Ordinal)
                || checkString.EndsWith("\n _", StringComparison.Ordinal) || checkString.EndsWith("\n .", StringComparison.Ordinal)
                || checkString.EndsWith("\n _", StringComparison.Ordinal) || checkString.EndsWith("\n .", StringComparison.Ordinal) /* Alt+255 */)
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

        public static string GetFirstWord(string input)
        {
            // Return if empty string
            if (string.IsNullOrEmpty(input))
            {
                return "";
            }

            string[] split = input.Split(' ');
            string firstWord = split.First();

            // For "... this is a test" we would only have the prefix in the first split item
            foreach (string prefix in Prefixes)
            {
                if (firstWord == prefix && split.Length > 1)
                {
                    firstWord = split[0] + " " + split[1];
                    break;
                }
            }

            return firstWord;
        }

        public static string GetLastWord(string input)
        {
            // Return if empty string
            if (string.IsNullOrEmpty(input))
            {
                return "";
            }

            string[] split = input.Split(' ');
            string lastWord = split.Last();

            // For "This is a test ..." we would only have the suffix in the last split item
            foreach (string suffix in Suffixes)
            {
                if (lastWord == suffix && split.Length > 1)
                {
                    lastWord = split[split.Length - 2] + " " + split[split.Length - 1];
                    break;
                }
            }

            return lastWord;
        }

        public static bool ShouldAddSuffix(string input, ContinuationProfile profile, bool sanitize, bool gap)
        {
            string text = sanitize ? SanitizeString(input) : input;
            bool applyIfComma = gap && profile.UseDifferentStyleGap ? profile.GapSuffixApplyIfComma : profile.SuffixApplyIfComma;

            // Return if empty string
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            if ((EndsWithNothing(text, profile) || (applyIfComma && text.EndsWith(',')) || HasSuffix(text, profile)) && !text.EndsWith("--", StringComparison.Ordinal) && !text.EndsWith(':') && !text.EndsWith(';'))
            {
                return true;
            }

            return false;
        }
        
        public static bool ShouldAddSuffix(string input, ContinuationProfile profile, bool sanitize)
        {
            return ShouldAddSuffix(input, profile, sanitize, false);
        }

        public static bool ShouldAddSuffix(string input, ContinuationProfile profile)
        {
            return ShouldAddSuffix(input, profile, true);
        }

        public static string AddSuffixIfNeeded(string originalText, ContinuationProfile profile, bool gap, bool addComma)
        {
            string text = SanitizeString(originalText);

            // Return if empty string
            if (string.IsNullOrEmpty(text))
            {
                return originalText;
            }

            // Return if only suffix/prefix
            if (IsOnlySuffix(text, profile) || IsOnlyPrefix(text, profile))
            {
                return originalText;
            }

            // Get last word
            var lastWord = GetLastWord(text);
            var newLastWord = lastWord;

            if (gap && profile.UseDifferentStyleGap)
            {
                // Make new last word
                var gapAddEnd = (profile.GapSuffixAddSpace ? " " : "") + profile.GapSuffix;

                if (gapAddEnd.Length == 0 || !newLastWord.EndsWith(gapAddEnd, StringComparison.Ordinal))
                {
                    newLastWord = newLastWord.TrimEnd(',') + ((lastWord.EndsWith(',') || addComma) && !profile.GapSuffixReplaceComma ? "," : "") + gapAddEnd;
                }
            }
            else
            {
                // Make new last word
                var addEnd = (profile.SuffixAddSpace ? " " : "") + profile.Suffix;

                if (addEnd.Length == 0 || !newLastWord.EndsWith(addEnd, StringComparison.Ordinal))
                {
                    newLastWord = newLastWord.TrimEnd(',') + ((lastWord.EndsWith(',') || addComma) && !profile.SuffixReplaceComma ? "," : "") + addEnd;
                }
            }

            // Check if it's not surrounded by HTML tags or quotes, then we'll place it outside the tags (remove comma if present)
            // Only if it's not a tag/quote across the whole subtitle.
            var wordIndex = originalText.LastIndexOf(lastWord.TrimEnd(','), StringComparison.Ordinal);
            if (wordIndex >= 0 && wordIndex < originalText.Length - 1)
            {
                bool needsInsertion = false;
                int currentIndex = wordIndex + lastWord.TrimEnd(',').Length;

                if (currentIndex < originalText.Length && ExplanationQuotes.Contains(originalText[currentIndex])
                                                       && !IsFullLineQuote(originalText, currentIndex, QuotePairs[originalText[currentIndex]], originalText[currentIndex]))
                {
                    char quote = originalText[currentIndex];
                    while (currentIndex + 1 < originalText.Length && originalText[currentIndex] != quote)
                    {
                        currentIndex++;
                    }

                    needsInsertion = true;
                }

                if (currentIndex < originalText.Length && originalText[currentIndex] == '<' && !IsFullLineTag(originalText, currentIndex))
                {
                    while (currentIndex + 1 < originalText.Length && originalText[currentIndex] != '>')
                    {
                        currentIndex++;
                    }

                    needsInsertion = true;
                }

                if (needsInsertion)
                {
                    var suffix = newLastWord.Replace(lastWord.TrimEnd(','), "");

                    if (currentIndex + 1 < originalText.Length && originalText[currentIndex + 1] == ',')
                    {
                        originalText = originalText.Remove(currentIndex + 1, 1);
                    }

                    return originalText.Insert(currentIndex + 1, suffix);
                }
            }

            // Replace it
            return ReplaceLastOccurrence(originalText, lastWord, newLastWord);
        }

        public static string AddSuffixIfNeeded(string originalText, ContinuationProfile profile, bool gap)
        {
            return AddSuffixIfNeeded(originalText, profile, gap, false);
        }

        public static string AddPrefixIfNeeded(string originalText, ContinuationProfile profile, bool shouldRemoveDashesDuringSanitization, bool gap)
        {
            // Decide if we need to remove dashes
            string textWithDash = SanitizeString(originalText, false);
            string textWithoutDash = SanitizeString(originalText, true);
            bool removeDashesDuringSanitization = shouldRemoveDashesDuringSanitization;

            // Return if empty string
            if (string.IsNullOrEmpty(textWithDash) && string.IsNullOrEmpty(textWithoutDash))
            {
                return originalText;
            }

            // If we're using a profile with dashes, count those as dashes instead of dialog dashes.
            if (removeDashesDuringSanitization)
            {
                foreach (string prefix in DashPrefixes)
                {
                    if ((!gap && profile.Prefix == prefix)
                        || (gap && profile.UseDifferentStyleGap && profile.GapPrefix == prefix)
                        || (gap && !profile.UseDifferentStyleGap && profile.Prefix == prefix))
                    {
                        removeDashesDuringSanitization = false;
                        break;
                    }
                }
            }

            // If there is only a dash on the first line, count it as dash instead of dialog dash.
            if (removeDashesDuringSanitization && textWithDash != null)
            {
                var split = textWithDash.SplitToLines();
                int lastLineWithDash = -1;

                for (var i = 0; i < split.Count; i++)
                {
                    foreach (string prefix in DashPrefixes)
                    {
                        if (split[i].StartsWith(prefix, StringComparison.Ordinal))
                        {
                            lastLineWithDash = i;
                            break;
                        }
                    }
                }

                if (lastLineWithDash == 0)
                {
                    removeDashesDuringSanitization = false;
                }
            }

            var text = removeDashesDuringSanitization ? textWithoutDash : textWithDash;

            // Return if empty string
            if (string.IsNullOrEmpty(text))
            {
                return originalText;
            }

            // Return if only suffix/prefix
            if (IsOnlySuffix(text, profile) || IsOnlyPrefix(text, profile))
            {
                return originalText;
            }

            // Get first word of the paragraph
            var firstWord = GetFirstWord(text);
            var newFirstWord = firstWord;

            if (gap && profile.UseDifferentStyleGap)
            {
                // Make new first word
                var gapAddStart = profile.GapPrefix + (profile.GapPrefixAddSpace ? " " : "");

                if (gapAddStart.Length == 0 || !newFirstWord.StartsWith(gapAddStart, StringComparison.Ordinal))
                {
                    newFirstWord = gapAddStart + newFirstWord;
                }
            }
            else
            {
                // Make new first word
                var addStart = profile.Prefix + (profile.PrefixAddSpace ? " " : "");
                if (addStart.Length == 0 || !newFirstWord.StartsWith(addStart, StringComparison.Ordinal))
                {
                    newFirstWord = addStart + newFirstWord;
                }
            }

            // Check if it's not surrounded by HTML tags or quotes, then we'll place it outside the tags
            // Only if it's not a tag/quote across the whole subtitle.
            var wordIndex = originalText.IndexOf(firstWord, StringComparison.Ordinal);
            if (wordIndex >= 1)
            {
                var needsInsertion = false;
                var currentIndex = wordIndex - 1;
                if (currentIndex >= 0 && ExplanationQuotes.Contains(originalText[currentIndex])
                                      && !IsFullLineQuote(originalText, currentIndex + 1, originalText[currentIndex], QuotePairs[originalText[currentIndex]]))
                {
                    char quote = originalText[currentIndex];
                    while (currentIndex - 1 >= 0 && originalText[currentIndex] != quote)
                    {
                        currentIndex--;
                    }

                    needsInsertion = true;
                }

                if (currentIndex >= 0 && originalText[currentIndex] == '>' && !IsFullLineTag(originalText, currentIndex))
                {
                    while (currentIndex - 1 >= 0 && originalText[currentIndex] != '<')
                    {
                        currentIndex--;
                    }

                    needsInsertion = true;
                }

                if (needsInsertion)
                {
                    var prefix = newFirstWord.Replace(firstWord, "");
                    return originalText.Insert(currentIndex, prefix);
                }
            }

            // Replace it
            return ReplaceFirstOccurrence(originalText, firstWord, newFirstWord);
        }

        public static string AddPrefixIfNeeded(string originalText, ContinuationProfile profile, bool gap)
        {
            return AddPrefixIfNeeded(originalText, profile, true, gap);
        }

        public static string RemoveSuffix(string originalText, ContinuationProfile profile, List<string> additionalSuffixes, bool addComma)
        {
            string text = SanitizeString(originalText);

            // Return if empty string
            if (string.IsNullOrEmpty(text))
            {
                return originalText;
            }

            // Get last word
            var lastWord = GetLastWord(text);
            var newLastWord = lastWord;
            foreach (string suffix in Suffixes.Union(additionalSuffixes))
            {
                if (newLastWord.EndsWith(suffix, StringComparison.Ordinal) && !newLastWord.EndsWith(Environment.NewLine + suffix, StringComparison.Ordinal))
                {
                    newLastWord = newLastWord.Substring(0, newLastWord.Length - suffix.Length);
                }
            }
            newLastWord = newLastWord.Trim();

            if (addComma && !newLastWord.EndsWith(','))
            {
                newLastWord += ",";
            }

            string result;

            // If we can find it...
            if (originalText.LastIndexOf(lastWord, StringComparison.Ordinal) >= 0)
            {
                // Replace it
                result = ReplaceLastOccurrence(originalText, lastWord, newLastWord);
            }
            else
            {
                // Just remove whatever suffix we need to remove
                var suffix = lastWord.Replace(newLastWord, "");
                result = ReplaceLastOccurrence(originalText, suffix, "");
            }

            // Return original if empty string
            if (string.IsNullOrEmpty(result))
            {
                return originalText;
            }

            return result;
        }

        public static string RemoveSuffix(string originalText, ContinuationProfile profile)
        {
            return RemoveSuffix(originalText, profile, new List<string>(), false);
        }

        public static string RemoveSuffix(string originalText, ContinuationProfile profile, bool addComma)
        {
            return RemoveSuffix(originalText, profile, new List<string>(), addComma);
        }

        public static string RemovePrefix(string originalText, ContinuationProfile profile, bool shouldRemoveDashesDuringSanitization, bool gap)
        {
            // Decide if we need to remove dashes
            var textWithDash = SanitizeString(originalText, false);
            var textWithoutDash = SanitizeString(originalText, true);
            string leadingDialogDash = null;
            bool removeDashesDuringSanitization = shouldRemoveDashesDuringSanitization;

            // Return if empty string
            if (string.IsNullOrEmpty(textWithDash) && string.IsNullOrEmpty(textWithoutDash))
            {
                return originalText;
            }

            // If we're using a profile with dashes, count those as dashes instead of dialog dashes.
            if (removeDashesDuringSanitization)
            {
                foreach (string prefix in DashPrefixes)
                {
                    if ((!gap && profile.Prefix == prefix)
                        || (gap && profile.UseDifferentStyleGap && profile.GapPrefix == prefix)
                        || (gap && !profile.UseDifferentStyleGap && profile.Prefix == prefix))
                    {
                        removeDashesDuringSanitization = false;
                        leadingDialogDash = prefix;
                        break;
                    }
                }
            }

            // If there is only a dash on the first line, count it as dash instead of dialog dash.
            if (removeDashesDuringSanitization && textWithDash != null)
            {
                var split = textWithDash.SplitToLines();
                int lastLineWithDash = -1;

                for (var i = 0; i < split.Count; i++)
                {
                    foreach (string prefix in DashPrefixes)
                    {
                        if (split[i].StartsWith(prefix, StringComparison.Ordinal))
                        {
                            lastLineWithDash = i;
                            break;
                        }
                    }
                }

                if (lastLineWithDash == 0)
                {
                    removeDashesDuringSanitization = false;
                }
            }

            string text = removeDashesDuringSanitization ? textWithoutDash : textWithDash;

            // Return if empty string
            if (string.IsNullOrEmpty(text))
            {
                return originalText;
            }

            // Get first word of the paragraph
            string firstWord = GetFirstWord(text);
            string newFirstWord = firstWord;

            if (leadingDialogDash != null)
            {
                newFirstWord = newFirstWord.TrimStart(Convert.ToChar(leadingDialogDash)).Trim();
            }

            foreach (string prefix in Prefixes)
            {
                if (newFirstWord.StartsWith(prefix, StringComparison.Ordinal) && !newFirstWord.EndsWith(prefix + Environment.NewLine, StringComparison.Ordinal))
                {
                    newFirstWord = newFirstWord.Substring(prefix.Length);
                }
            }
            newFirstWord = newFirstWord.Trim();

            string result = null;

            // If we can find it...
            if (originalText.IndexOf(firstWord, StringComparison.Ordinal) >= 0)
            {
                // Replace it
                result = ReplaceFirstOccurrence(originalText, firstWord, newFirstWord);
            }
            else if (newFirstWord.Length > 0)
            {
                // Just remove whatever prefix we need to remove
                var prefix = firstWord.Replace(newFirstWord, "");
                result = ReplaceFirstOccurrence(originalText, prefix, "");
            }

            // Return original if empty string
            if (string.IsNullOrEmpty(result))
            {
                return originalText;
            }

            return result;
        }

        public static string RemovePrefix(string originalText, ContinuationProfile profile, bool gap)
        {
            return RemovePrefix(originalText, profile, true, gap);
        }

        public static string RemovePrefix(string originalText, ContinuationProfile profile)
        {
            return RemovePrefix(originalText, profile, true, false);
        }

        public static string RemoveAllPrefixes(string originalText, ContinuationProfile profile)
        {
            var text = originalText;

            // Return if empty string
            if (string.IsNullOrEmpty(text))
            {
                return originalText;
            }

            while (HasPrefix(SanitizeString(text, false), profile))
            {
                text = RemovePrefix(text, profile, false, false /* Not used because of false before */);
            }
            return text;
        }

        public static bool IsNewSentence(string input, bool iNewSentence)
        {
            // Return if empty string
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }

            if (!iNewSentence)
            {
                if (input.StartsWith("I ", StringComparison.Ordinal) || input.StartsWith("I'", StringComparison.Ordinal))
                {
                    // English I
                    return false;
                }
            }

            if (input.Length > 0 && char.IsLetter(input[0]) && char.IsUpper(input[0]))
            {
                // First letter
                return true;
            }

            if (input.Length > 1 && char.IsLetter(input[0]) && !char.IsUpper(input[0]) && char.IsLetter(input[1]) && char.IsUpper(input[1]))
            {
                // iPhone
                return true;
            }

            if (input.Length > 3 && char.IsPunctuation(input[0]) && char.IsLetter(input[1]) && !char.IsUpper(input[1]) && char.IsWhiteSpace(input[2]) && char.IsLetter(input[3]) && char.IsUpper(input[3]))
            {
                // 's Avonds
                return true;
            }

            if (input.Length > 1 && "¿¡".Contains(input[0]) && char.IsLetter(input[1]) && char.IsUpper(input[1]))
            {
                // Spanish
                return true;
            }

            return false;
        }

        public static bool IsNewSentence(string input)
        {
            return IsNewSentence(input, false);
        }

        public static bool IsEndOfSentence(string input)
        {
            // Return if empty string
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }

            return input.EndsWith('.') && !input.EndsWith("..", StringComparison.Ordinal) ||
                   input.EndsWith('?') ||
                   input.EndsWith('!') ||
                   input.EndsWith(';') /* Greek question mark */ ||
                   input.EndsWith("--", StringComparison.Ordinal);
        }

        public static bool EndsWithNothing(string input, ContinuationProfile profile)
        {
            // Return if empty string
            if (string.IsNullOrEmpty(input))
            {
                return true;
            }

            return !HasSuffixUnsafe(input, profile) && !IsEndOfSentence(input) && !input.EndsWith(',') && !input.EndsWith(':') && !input.EndsWith(';') && !input.EndsWith('-');
        }

        public static bool IsAllCaps(string input)
        {
            // Return if empty string
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }

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

            // Return if empty string
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }

            while (input.IndexOf("<i>", StringComparison.Ordinal) >= 0)
            {
                var startIndex = input.IndexOf("<i>", StringComparison.Ordinal);
                var endIndex = input.IndexOf("</i>", StringComparison.Ordinal);
                var textToRemove = endIndex >= 0 ? input.Substring(startIndex, (endIndex + 4) - startIndex) : input.Substring(startIndex);
                input = input.Replace(textToRemove, "");
            }

            foreach (var c in input)
            {
                if (c != '\n' && c != '\r')
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsBold(string input)
        {
            input = ExtractParagraphOnly(input);

            // Return if empty string
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }

            while (input.IndexOf("<b>", StringComparison.Ordinal) >= 0)
            {
                var startIndex = input.IndexOf("<b>", StringComparison.Ordinal);
                var endIndex = input.IndexOf("</b>", StringComparison.Ordinal);
                var textToRemove = endIndex >= 0 ? input.Substring(startIndex, (endIndex + 4) - startIndex) : input.Substring(startIndex);
                input = input.Replace(textToRemove, "");
            }

            foreach (var c in input)
            {
                if (c != '\n' && c != '\r')
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsFullLineTag(string input, int position)
        {
            input = ExtractParagraphOnly(input);

            // Return if empty string
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }

            var lineStartIndex = (position > 0 && position < input.Length) ? input.LastIndexOf("\n", position, StringComparison.Ordinal) : 0;
            if (lineStartIndex == -1)
            {
                lineStartIndex = 0;
            }

            var lineEndIndex = (position > 0 && position < input.Length) ? input.IndexOf("\n", position, StringComparison.Ordinal) : input.Length;
            if (lineEndIndex == -1)
            {
                lineEndIndex = input.Length;
            }

            input = input.Substring(lineStartIndex, lineEndIndex - lineStartIndex);

            var startIndex = input.IndexOf("<", StringComparison.Ordinal);
            var endIndex = input.LastIndexOf("</", StringComparison.Ordinal);
            if (endIndex >= 0)
            {
                if (startIndex == endIndex)
                {
                    startIndex = 0;
                    endIndex = input.IndexOf(">", endIndex, StringComparison.Ordinal) + 1;
                }
                else
                {
                    endIndex = input.IndexOf(">", endIndex, StringComparison.Ordinal) + 1;
                }
            }
            else
            {
                endIndex = input.Length;
            }

            var textToRemove = input.Substring(startIndex, endIndex - startIndex);
            if (!string.IsNullOrEmpty(textToRemove))
            {
                input = input.Replace(textToRemove, "");
            }

            foreach (var c in input)
            {
                if (char.IsLetterOrDigit(c))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsFullLineQuote(string originalInput, int position, char quoteStart, char quoteEnd)
        {
            string input = ExtractParagraphOnly(originalInput);

            // Return if empty string
            if (string.IsNullOrEmpty(originalInput))
            {
                return false;
            }

            // Shift index if needed after deleting { } tags
            position -= Math.Max(0, originalInput.IndexOf(input, StringComparison.Ordinal));

            var startIndex = position;
            var endIndex = position;

            while (startIndex >= 0 && !(input[startIndex] == quoteStart
                                        && (startIndex - 1 >= 0 && (!char.IsLetterOrDigit(input[startIndex - 1]) && input[startIndex - 1] != '\r' && input[startIndex - 1] != '\n'))))
            {
                if (startIndex - 1 >= 0 && (input[startIndex - 1] == '\r' || input[startIndex - 1] == '\n'))
                {
                    startIndex = 0;
                    break;
                }

                startIndex--;
            }

            while (endIndex < input.Length && !(input[endIndex] == quoteEnd
                                                && (endIndex + 1 < input.Length && (!char.IsLetterOrDigit(input[endIndex + 1]) && input[endIndex + 1] != '\r' && input[endIndex + 1] != '\n'))))
            {
                if (endIndex + 1 < input.Length && (input[endIndex + 1] == '\r' || input[endIndex + 1] == '\n'))
                {
                    endIndex = input.Length;
                    break;
                }

                endIndex++;
            }

            startIndex = Math.Max(0, startIndex);
            endIndex = Math.Min(input.Length, endIndex);

            var textToRemove = input.Substring(startIndex, endIndex - startIndex);
            if (!string.IsNullOrEmpty(textToRemove))
            {
                input = input.Replace(textToRemove, "");
            }

            foreach (var c in input)
            {
                if (char.IsLetterOrDigit(c))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsOnlyPrefix(string input, ContinuationProfile profile)
        {
            var checkString = input;

            if (string.IsNullOrEmpty(input.Trim()))
            {
                return false;
            }

            if (profile.Prefix.Length > 0)
            {
                checkString = checkString.Replace(profile.Prefix, "");
            }

            if (profile.UseDifferentStyleGap && profile.GapPrefix.Length > 0)
            {
                checkString = checkString.Replace(profile.GapPrefix, "");
            }

            foreach (string prefix in Prefixes)
            {
                checkString = checkString.Replace(prefix, "");
            }

            checkString = checkString.Trim();

            return string.IsNullOrEmpty(checkString);
        }

        public static bool IsOnlySuffix(string input, ContinuationProfile profile)
        {
            var checkString = input;

            if (string.IsNullOrEmpty(input.Trim()))
            {
                return false;
            }

            if (profile.Suffix.Length > 0)
            {
                checkString = checkString.Replace(profile.Suffix, "");
            }

            if (profile.UseDifferentStyleGap && profile.GapSuffix.Length > 0)
            {
                checkString = checkString.Replace(profile.GapSuffix, "");
            }

            foreach (string suffix in Suffixes)
            {
                checkString = checkString.Replace(suffix, "");
            }

            checkString = checkString.Trim();

            return string.IsNullOrEmpty(checkString);
        }

        public static bool HasPrefix(string input, ContinuationProfile profile)
        {
            // Return if only prefix
            if (IsOnlyPrefix(input, profile))
            {
                return false;
            }

            if (profile.Prefix.Length > 0 && input.StartsWith(profile.Prefix, StringComparison.Ordinal) && !input.StartsWith(profile.Prefix + Environment.NewLine, StringComparison.Ordinal))
            {
                return true;
            }

            if (profile.UseDifferentStyleGap && profile.GapPrefix.Length > 0 && input.StartsWith(profile.GapPrefix, StringComparison.Ordinal) && !input.StartsWith(profile.GapPrefix + Environment.NewLine, StringComparison.Ordinal))
            {
                return true;
            }

            foreach (string prefix in Prefixes)
            {
                if (input.StartsWith(prefix, StringComparison.Ordinal) && !input.StartsWith(prefix + Environment.NewLine, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool HasSuffix(string input, ContinuationProfile profile)
        {
            // Return if only suffix
            if (IsOnlySuffix(input, profile))
            {
                return false;
            }

            if (profile.Suffix.Length > 0 && input.EndsWith(profile.Suffix, StringComparison.Ordinal) && !input.EndsWith(Environment.NewLine + profile.Suffix, StringComparison.Ordinal))
            {
                return true;
            }

            if (profile.UseDifferentStyleGap && profile.GapSuffix.Length > 0 && input.EndsWith(profile.GapSuffix) && !input.EndsWith(Environment.NewLine + profile.GapSuffix, StringComparison.Ordinal))
            {
                return true;
            }

            foreach (string suffix in Suffixes)
            {
                if (input.EndsWith(suffix, StringComparison.Ordinal) && !input.EndsWith(Environment.NewLine + suffix, StringComparison.Ordinal) && input.Length > suffix.Length)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasSuffixUnsafe(string input, ContinuationProfile profile)
        {
            if (profile.Suffix.Length > 0 && input.EndsWith(profile.Suffix, StringComparison.Ordinal) && !input.EndsWith(Environment.NewLine + profile.Suffix, StringComparison.Ordinal))
            {
                return true;
            }

            if (profile.UseDifferentStyleGap && profile.GapSuffix.Length > 0 && input.EndsWith(profile.GapSuffix, StringComparison.Ordinal) && !input.EndsWith(Environment.NewLine + profile.GapSuffix, StringComparison.Ordinal))
            {
                return true;
            }

            foreach (string suffix in Suffixes)
            {
                if (input.EndsWith(suffix, StringComparison.Ordinal) && !input.EndsWith(Environment.NewLine + suffix, StringComparison.Ordinal) && input.Length > suffix.Length)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool StartsWithConjunction(string input, string language)
        {
            // Return if empty string
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }

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
            else if (language == "pt")
            {
                conjunctions = new List<string>
                {
                    "mas", "nem", "por exemplo", "e", "bem com", "todavia", "no entanto", "mas também", "como também",
                    "bem como", "porém", "por isso", "porque", "portanto"
                };
            }
            
            if (conjunctions != null)
            {
                foreach (string conjunction in conjunctions)
                {
                    if (input.StartsWith(conjunction + " ", StringComparison.Ordinal) || input.StartsWith(conjunction + ",", StringComparison.Ordinal) || input.StartsWith(conjunction + ":", StringComparison.Ordinal))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static Tuple<string, string> MergeHelper(string input, string nextInput, ContinuationProfile profile, string language)
        {
            // Convert for Arabic
            if (language == "ar")
            {
                input = ConvertToForArabic(input);
                nextInput = ConvertToForArabic(nextInput);
            }

            var thisText = SanitizeString(input);
            var nextText = SanitizeString(nextInput);
            var nextTextWithDash = SanitizeString(nextInput, false);

            // Remove any prefix and suffix when:
            // - Title 1 ends with a suffix AND title 2 starts with a prefix AND it's both the same type
            // - Title 2 is a continuing sentence, but only remove a leading dash if it's a dialog
            if (HasSuffix(thisText, profile) && HasPrefix(nextTextWithDash, profile) && thisText[thisText.Length - 1] == nextTextWithDash[0]
                || !string.IsNullOrEmpty(nextText) && !IsNewSentence(nextText) && !IsEndOfSentence(thisText)
                && (!Dashes.Contains(nextTextWithDash[0]) || Dashes.Contains(nextTextWithDash[0]) && nextTextWithDash.IndexOf(nextTextWithDash[0], 1) != -1))
            {
                var newNextText = RemoveAllPrefixes(nextInput, profile);
                var newText = RemoveSuffix(input, profile, StartsWithConjunction(newNextText, language));

                input = newText;
                nextInput = newNextText;
            }

            // Convert back for Arabic
            if (language == "ar")
            {
                input = ConvertBackForArabic(input);
                nextInput = ConvertBackForArabic(nextInput);
            }

            return new Tuple<string, string>(input, nextInput);
        }

        public static string ConvertToForArabic(string input)
        {
            return input.Replace("،", ",").Replace("؟", "?");
        }

        public static string ConvertBackForArabic(string input)
        {
            return input.Replace(",", "،").Replace("?", "؟");
        }

        public static bool IsArabicInsert(string originalInput, string sanitizedInput)
        {
            string input = ExtractParagraphOnly(originalInput);
            input = Regex.Replace(input, "<.*?>", string.Empty);

            if (input.Length >= 2)
            {
                if (Quotes.Contains(input[0]) && Quotes.Contains(input[input.Length - 1]) && !sanitizedInput.EndsWith(",") && !IsEndOfSentence(sanitizedInput))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsLanguageWithoutCaseDistinction(string language)
        {
            return LanguagesWithoutCaseDistinction.Contains(language);
        }

        public static int GetMinimumGapMs()
        {
            return Math.Max(Configuration.Settings.General.MinimumMillisecondsBetweenLines + 5, Configuration.Settings.General.ContinuationPause);
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
                case ContinuationStyle.LeadingTrailingEllipsis:
                    return 7;
                case ContinuationStyle.NoneEllipsisForPauses:
                    return 8;
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
                case 7:
                    return ContinuationStyle.LeadingTrailingEllipsis;
                case 8:
                    return ContinuationStyle.NoneEllipsisForPauses;
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
                        SuffixApplyIfComma = false,
                        SuffixAddSpace = false,
                        SuffixReplaceComma = false,
                        Prefix = "",
                        PrefixAddSpace = false,
                        UseDifferentStyleGap = true,
                        GapSuffix = "...",
                        GapSuffixApplyIfComma = true,
                        GapSuffixAddSpace = false,
                        GapSuffixReplaceComma = true,
                        GapPrefix = "",
                        GapPrefixAddSpace = false
                    };
                case ContinuationStyle.NoneEllipsisForPauses:
                    return new ContinuationProfile
                    {
                        Suffix = "",
                        SuffixApplyIfComma = false,
                        SuffixAddSpace = false,
                        SuffixReplaceComma = false,
                        Prefix = "",
                        PrefixAddSpace = false,
                        UseDifferentStyleGap = true,
                        GapSuffix = "…",
                        GapSuffixApplyIfComma = true,
                        GapSuffixAddSpace = false,
                        GapSuffixReplaceComma = true,
                        GapPrefix = "",
                        GapPrefixAddSpace = false
                    };
                case ContinuationStyle.NoneLeadingTrailingDots:
                    return new ContinuationProfile
                    {
                        Suffix = "",
                        SuffixApplyIfComma = false,
                        SuffixAddSpace = false,
                        SuffixReplaceComma = false,
                        Prefix = "",
                        PrefixAddSpace = false,
                        UseDifferentStyleGap = true,
                        GapSuffix = "...",
                        GapSuffixApplyIfComma = true,
                        GapSuffixAddSpace = false,
                        GapSuffixReplaceComma = true,
                        GapPrefix = "...",
                        GapPrefixAddSpace = false
                    };
                case ContinuationStyle.OnlyTrailingDots:
                    return new ContinuationProfile
                    {
                        Suffix = "...",
                        SuffixApplyIfComma = true,
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
                        SuffixApplyIfComma = true,
                        SuffixAddSpace = false,
                        SuffixReplaceComma = true,
                        Prefix = "...",
                        PrefixAddSpace = false,
                        UseDifferentStyleGap = false
                    };
                case ContinuationStyle.LeadingTrailingEllipsis:
                    return new ContinuationProfile
                    {
                        Suffix = "…",
                        SuffixApplyIfComma = true,
                        SuffixAddSpace = false,
                        SuffixReplaceComma = true,
                        Prefix = "…",
                        PrefixAddSpace = false,
                        UseDifferentStyleGap = false
                    };
                case ContinuationStyle.LeadingTrailingDash:
                    return new ContinuationProfile
                    {
                        Suffix = "-",
                        SuffixApplyIfComma = true,
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
                        SuffixApplyIfComma = true,
                        SuffixAddSpace = true,
                        SuffixReplaceComma = true,
                        Prefix = "-",
                        PrefixAddSpace = true,
                        UseDifferentStyleGap = true,
                        GapSuffix = "...",
                        GapSuffixApplyIfComma = true,
                        GapSuffixAddSpace = false,
                        GapSuffixReplaceComma = true,
                        GapPrefix = "...",
                        GapPrefixAddSpace = false
                    };
                default:
                    return new ContinuationProfile
                    {
                        Suffix = "",
                        SuffixApplyIfComma = false,
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
            public bool SuffixApplyIfComma { get; set; }
            public bool SuffixAddSpace { get; set; }
            public bool SuffixReplaceComma { get; set; }
            public string Prefix { get; set; }
            public bool PrefixAddSpace { get; set; }
            public bool UseDifferentStyleGap { get; set; }
            public string GapSuffix { get; set; }
            public bool GapSuffixApplyIfComma { get; set; }
            public bool GapSuffixAddSpace { get; set; }
            public bool GapSuffixReplaceComma { get; set; }
            public string GapPrefix { get; set; }
            public bool GapPrefixAddSpace { get; set; }
        }
    }
}
