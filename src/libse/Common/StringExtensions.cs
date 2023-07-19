using Nikse.SubtitleEdit.Core.Common.TextLengthCalculator;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Nikse.SubtitleEdit.Core.Common
{
    public static class StringExtensions
    {
        public static char[] UnicodeControlChars { get; } = { '\u200E', '\u200F', '\u202A', '\u202B', '\u202C', '\u202D', '\u202E' };

        public static bool LineStartsWithHtmlTag(this string text, bool threeLengthTag, bool includeFont = false)
        {
            if (text == null || !threeLengthTag && !includeFont)
            {
                return false;
            }

            return StartsWithHtmlTag(text, threeLengthTag, includeFont);
        }

        public static bool LineEndsWithHtmlTag(this string text, bool threeLengthTag, bool includeFont = false)
        {
            if (text == null)
            {
                return false;
            }

            var len = text.Length;
            if (len < 6 || text[len - 1] != '>')
            {
                return false;
            }

            // </font> </i>
            if (threeLengthTag && text[len - 4] == '<' && text[len - 3] == '/')
            {
                return true;
            }

            if (includeFont && len > 8 && text[len - 7] == '<' && text[len - 6] == '/')
            {
                return true;
            }

            return false;
        }

        public static bool LineBreakStartsWithHtmlTag(this string text, bool threeLengthTag, bool includeFont = false)
        {
            if (text == null || (!threeLengthTag && !includeFont))
            {
                return false;
            }

            var newLineIdx = text.IndexOf(Environment.NewLine, StringComparison.Ordinal);
            if (newLineIdx < 0 || text.Length < newLineIdx + 5)
            {
                return false;
            }

            text = text.Substring(newLineIdx + 2);
            return StartsWithHtmlTag(text, threeLengthTag, includeFont);
        }

        private static bool StartsWithHtmlTag(string text, bool threeLengthTag, bool includeFont)
        {
            if (threeLengthTag && text.Length >= 3 && text[0] == '<' && text[2] == '>' && (text[1] == 'i' || text[1] == 'I' || text[1] == 'u' || text[1] == 'U' || text[1] == 'b' || text[1] == 'B'))
            {
                return true;
            }

            if (includeFont && text.Length > 5 && text.StartsWith("<font", StringComparison.OrdinalIgnoreCase))
            {
                return text.IndexOf('>', 5) >= 5; // <font> or <font color="#000000">
            }

            return false;
        }

        public static bool StartsWith(this string s, char c)
        {
            return s.Length > 0 && s[0] == c;
        }

        public static bool StartsWith(this StringBuilder sb, char c)
        {
            return sb.Length > 0 && sb[0] == c;
        }

        public static bool EndsWith(this string s, char c)
        {
            return s.Length > 0 && s[s.Length - 1] == c;
        }

        public static bool EndsWith(this StringBuilder sb, char c)
        {
            return sb.Length > 0 && sb[sb.Length - 1] == c;
        }

        public static bool Contains(this string source, char value)
        {
            return source.IndexOf(value) >= 0;
        }

        public static bool Contains(this string source, char[] value)
        {
            return source.IndexOfAny(value) >= 0;
        }

        public static bool Contains(this string source, string value, StringComparison comparisonType)
        {
            return source.IndexOf(value, comparisonType) >= 0;
        }

        public static List<string> SplitToLines(this string s) => SplitToLines(s, s.Length);
        
        public static List<string> SplitToLines(this string s, int max)
        {
            //original non-optimized version: return source.Replace("\r\r\n", "\n").Replace("\r\n", "\n").Replace('\r', '\n').Replace('\u2028', '\n').Split('\n');

            var lines = new List<string>();
            var start = 0;
            var i = 0;

            if (s.Length < max)
            {
                max = s.Length;
            }

            while (i < max)
            {
                var ch = s[i];
                if (ch == '\r')
                {
                    if (i < max - 2 && s[i + 1] == '\r' && s[i + 2] == '\n') // \r\r\n
                    {
                        lines.Add(s.Substring(start, i - start));
                        i += 3;
                        start = i;
                        continue;
                    }

                    if (i < max - 1 && s[i + 1] == '\n') // \r\n
                    {
                        lines.Add(s.Substring(start, i - start));
                        i += 2;
                        start = i;
                        continue;
                    }

                    lines.Add(s.Substring(start, i - start));
                    i++;
                    start = i;
                    continue;
                }

                if (ch == '\n' || ch == '\u2028')
                {
                    lines.Add(s.Substring(start, i - start));
                    i++;
                    start = i;
                    continue;
                }

                i++;
            }

            lines.Add(s.Substring(start, i - start));
            return lines;
        }

        public static int CountWords(this string source)
        {
            return HtmlUtil.RemoveHtmlTags(source, true).Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
        }

        // http://www.codeproject.com/Articles/43726/Optimizing-string-operations-in-C
        public static int FastIndexOf(this string source, string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
            {
                return -1;
            }

            var c0 = pattern[0];
            if (pattern.Length == 1)
            {
                return source.IndexOf(c0);
            }

            var limit = source.Length - pattern.Length + 1;
            if (limit < 1)
            {
                return -1;
            }

            var c1 = pattern[1];

            // Find the first occurrence of the first character
            var first = source.IndexOf(c0, 0, limit);
            while (first != -1)
            {
                // Check if the following character is the same like
                // the 2nd character of "pattern"
                if (source[first + 1] != c1)
                {
                    first = source.IndexOf(c0, ++first, limit - first);
                    continue;
                }

                // Check the rest of "pattern" (starting with the 3rd character)
                var found = true;
                for (var j = 2; j < pattern.Length; j++)
                {
                    if (source[first + j] != pattern[j])
                    {
                        found = false;
                        break;
                    }
                }

                // If the whole word was found, return its index, otherwise try again
                if (found)
                {
                    return first;
                }

                first = source.IndexOf(c0, ++first, limit - first);
            }

            return -1;
        }

        public static int IndexOfAny(this string s, string[] words, StringComparison comparisonType)
        {
            if (words == null || string.IsNullOrEmpty(s))
            {
                return -1;
            }

            for (var i = 0; i < words.Length; i++)
            {
                var idx = s.IndexOf(words[i], comparisonType);
                if (idx >= 0)
                {
                    return idx;
                }
            }

            return -1;
        }

        public static string FixExtraSpaces(this string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return s;
            }

            const char whiteSpace = ' ';
            var k = -1;
            for (var i = s.Length - 1; i >= 0; i--)
            {
                var ch = s[i];
                if (k < 2)
                {
                    if (ch == whiteSpace)
                    {
                        k = i + 1;
                    }
                }
                else if (ch != whiteSpace)
                {
                    // only keep white space if it doesn't succeed/precede CRLF
                    var skipCount = (ch == '\n' || ch == '\r') || (k < s.Length && (s[k] == '\n' || s[k] == '\r')) ? 1 : 2;

                    // extra space found
                    if (k - (i + skipCount) >= 1)
                    {
                        s = s.Remove(i + 1, k - (i + skipCount));
                    }

                    // Reset remove length.
                    k = -1;
                }
            }

            return s;
        }

        public static bool ContainsLetter(this string s)
        {
            if (s != null)
            {
                foreach (var index in StringInfo.ParseCombiningCharacters(s))
                {
                    var uc = CharUnicodeInfo.GetUnicodeCategory(s, index);
                    if (uc == UnicodeCategory.LowercaseLetter || uc == UnicodeCategory.UppercaseLetter || uc == UnicodeCategory.TitlecaseLetter || uc == UnicodeCategory.ModifierLetter || uc == UnicodeCategory.OtherLetter)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool ContainsNumber(this string s)
        {
            if (s == null)
            {
                return false;
            }

            var max = s.Length;
            for (var index = 0; index < max; index++)
            {
                var ch = s[index];
                if (char.IsNumber(ch))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool ContainsUnicodeControlChars(this string s)
        {
            return s.Contains(UnicodeControlChars);
        }

        public static bool ContainsNonStandardNewLines(this string s)
        {
            if (Environment.NewLine == "\r\n")
            {
                var i = 0;
                while (i < s.Length)
                {
                    var ch = s[i];
                    if (ch == '\r')
                    {
                        if (i >= s.Length - 1 || s[i + 1] != '\n')
                        {
                            return true;
                        }

                        i++;
                    }
                    else if (ch == '\n')
                    {
                        return true;
                    }

                    i++;
                }

                return false;
            }

            if (Environment.NewLine == "\n")
            {
                return s.IndexOf('\r') >= 0;
            }

            s = s.Replace(Environment.NewLine, string.Empty);
            return s.IndexOf('\n') >= 0 || s.IndexOf('\r') >= 0;
        }

        public static string RemoveControlCharacters(this string s)
        {
            var max = s.Length;
            var newStr = new char[max];
            var newIdx = 0;
            for (int index = 0; index < max; index++)
            {
                var ch = s[index];
                if (!char.IsControl(ch))
                {
                    newStr[newIdx++] = ch;
                }
            }

            return new string(newStr, 0, newIdx);
        }

        public static bool IsOnlyControlCharactersOrWhiteSpace(this string s)
        {
            if (s == null)
            {
                return true;
            }

            var max = s.Length;
            for (var index = 0; index < max; index++)
            {
                var ch = s[index];
                if (!char.IsControl(ch) && !char.IsWhiteSpace(ch) && !UnicodeControlChars.Contains(ch))
                {
                    return false;
                }
            }

            return true;
        }

        public static string RemoveControlCharactersButWhiteSpace(this string s)
        {
            var max = s.Length;
            var newStr = new char[max];
            var newIdx = 0;
            for (int index = 0; index < max; index++)
            {
                var ch = s[index];
                if (!char.IsControl(ch) || ch == '\u000d' || ch == '\u000a' || ch == '\u0009')
                {
                    newStr[newIdx++] = ch;
                }
            }

            return new string(newStr, 0, newIdx);
        }

        public static string CapitalizeFirstLetter(this string s, CultureInfo ci = null)
        {
            var si = new StringInfo(s);
            if (ci == null)
            {
                ci = CultureInfo.CurrentCulture;
            }

            if (si.LengthInTextElements > 0)
            {
                s = si.SubstringByTextElements(0, 1).ToUpper(ci);
            }

            if (si.LengthInTextElements > 1)
            {
                s += si.SubstringByTextElements(1);
            }

            return s;
        }

        public static string ToProperCase(this string input, SubtitleFormat format)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return input;
            }

            var sb = new StringBuilder();
            var tags = RemoveAndSaveTags(input, sb, format);
            var properCaseText = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(sb.ToString().ToLowerInvariant());
            return RestoreSavedTags(properCaseText, tags);
        }

        public static string ToggleCasing(this string input, SubtitleFormat format, string overrideFromStringInit = null)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return input;
            }

            var sb = new StringBuilder();
            var tags = RemoveAndSaveTags(input, sb, format);
            var text = sb.ToString();

            var containsLowercase = false;
            var containsUppercase = false;
            var stringInit = overrideFromStringInit != null ? HtmlUtil.RemoveHtmlTags(overrideFromStringInit, true) : text;
            for (var i = 0; i < stringInit.Length; i++)
            {
                var ch = stringInit[i];
                if (char.IsNumber(ch))
                {
                    continue;
                }

                if (!containsLowercase && char.IsLower(ch))
                {
                    containsLowercase = true;
                }
                else if (!containsUppercase && char.IsUpper(ch))
                {
                    containsUppercase = true;
                }
            }

            if (containsUppercase && containsLowercase)
            {
                return RestoreSavedTags(text.ToUpperInvariant(), tags);
            }

            if (containsUppercase)
            {
                return RestoreSavedTags(text.ToLowerInvariant(), tags);
            }

            return RestoreSavedTags(System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(text), tags);
        }

        private static string RestoreSavedTags(string input, List<KeyValuePair<int, string>> tags)
        {
            var s = input;
            for (var index = tags.Count - 1; index >= 0; index--)
            {
                var keyValuePair = tags[index];
                if (keyValuePair.Key >= s.Length)
                {
                    s += keyValuePair.Value;
                }
                else
                {
                    s = s.Insert(keyValuePair.Key, keyValuePair.Value);
                }
            }

            return s;
        }

        private static List<KeyValuePair<int, string>> RemoveAndSaveTags(string input, StringBuilder sb, SubtitleFormat format)
        {
            var sbTag = new StringBuilder();
            var tags = new List<KeyValuePair<int, string>>();
            var tagOn = false;
            var tagIndex = 0;
            var skipNext = false;
            var isAssa = format != null
                         && (format.GetType() == typeof(AdvancedSubStationAlpha) || format.GetType() == typeof(SubStationAlpha));
            for (var index = 0; index < input.Length; index++)
            {
                if (skipNext)
                {
                    skipNext = false;
                    continue;
                }

                var ch = input[index];

                if (!tagOn && isAssa && ch == '\\' 
                           && (input.Substring(index).StartsWith("\\N") 
                               || input.Substring(index).StartsWith("\\n")
                               || input.Substring(index).StartsWith("\\h")))
                {
                    tags.Add(new KeyValuePair<int, string>(index, input.Substring(index, 2)));
                    skipNext = true;
                    continue;
                }

                if (tagOn && (ch == '>' || ch == '}'))
                {
                    sbTag.Append(ch);
                    tagOn = false;
                    tags.Add(new KeyValuePair<int, string>(tagIndex, sbTag.ToString()));
                    sbTag.Clear();
                    continue;
                }

                if (!tagOn && ch == '<')
                {
                    var s = input.Substring(index);
                    if (
                        s.StartsWith("<i>", StringComparison.OrdinalIgnoreCase) ||
                        s.StartsWith("</i>", StringComparison.OrdinalIgnoreCase) ||
                        s.StartsWith("<b>", StringComparison.OrdinalIgnoreCase) ||
                        s.StartsWith("</b>", StringComparison.OrdinalIgnoreCase) ||
                        s.StartsWith("<u>", StringComparison.OrdinalIgnoreCase) ||
                        s.StartsWith("</u>", StringComparison.OrdinalIgnoreCase) ||
                        s.StartsWith("<box>", StringComparison.OrdinalIgnoreCase) ||
                        s.StartsWith("</box>", StringComparison.OrdinalIgnoreCase) ||
                        s.StartsWith("<font ", StringComparison.OrdinalIgnoreCase) ||
                        s.StartsWith("</font>", StringComparison.OrdinalIgnoreCase) ||
                        s.StartsWith("<span", StringComparison.OrdinalIgnoreCase) ||
                        s.StartsWith("</span>", StringComparison.OrdinalIgnoreCase) ||
                        s.StartsWith("<rt", StringComparison.OrdinalIgnoreCase) ||
                        s.StartsWith("</rt", StringComparison.OrdinalIgnoreCase) ||
                        s.StartsWith("<ruby", StringComparison.OrdinalIgnoreCase) ||
                        s.StartsWith("</ruby>", StringComparison.OrdinalIgnoreCase) ||
                        s.StartsWith("<c", StringComparison.OrdinalIgnoreCase) ||
                        s.StartsWith("</c", StringComparison.OrdinalIgnoreCase) ||
                        s.StartsWith("<v", StringComparison.OrdinalIgnoreCase) ||
                        s.StartsWith("</v>", StringComparison.OrdinalIgnoreCase))
                    {
                        tagOn = true;
                        tagIndex = sb.Length;
                    }
                }
                else if (!tagOn && ch == '{')
                {
                    var s = input.Substring(index);
                    if (s.StartsWith("{\\", StringComparison.Ordinal))
                    {
                        tagOn = true;
                        tagIndex = index;
                    }
                }

                if (tagOn)
                {
                    sbTag.Append(ch);
                }
                else
                {
                    sb.Append(ch);
                }
            }

            return tags;
        }

        public static string ToRtf(this string value)
        {
            return @"{\rtf1\ansi\ansicpg1252\deff0{\fonttbl\f0\fswiss Helvetica;}\f0\pard " + value.ToRtfPart() + @"\par" + Environment.NewLine + "}";
        }

        public static string ToRtfPart(this string value)
        {
            // special RTF chars
            var backslashed = new StringBuilder(value);
            backslashed.Replace(@"\", @"\\");
            backslashed.Replace(@"{", @"\{");
            backslashed.Replace(@"}", @"\}");
            backslashed.Replace(Environment.NewLine, @"\par" + Environment.NewLine);

            // convert string char by char
            var sb = new StringBuilder();
            foreach (char character in backslashed.ToString())
            {
                if (character <= 0x7f)
                {
                    sb.Append(character);
                }
                else
                {
                    sb.Append("\\u" + Convert.ToUInt32(character) + "?");
                }
            }

            return sb.ToString();
        }

        public static string FromRtf(this string value)
        {
            return RichTextToPlainText.ConvertToText(value);
        }

        public static string RemoveChar(this string value, char charToRemove)
        {
            char[] array = new char[value.Length];
            int arrayIndex = 0;
            for (int i = 0; i < value.Length; i++)
            {
                char ch = value[i];
                if (ch != charToRemove)
                {
                    array[arrayIndex++] = ch;
                }
            }

            return new string(array, 0, arrayIndex);
        }

        public static string RemoveChar(this string value, char charToRemove, char charToRemove2)
        {
            char[] array = new char[value.Length];
            int arrayIndex = 0;
            for (int i = 0; i < value.Length; i++)
            {
                char ch = value[i];
                if (ch != charToRemove && ch != charToRemove2)
                {
                    array[arrayIndex++] = ch;
                }
            }

            return new string(array, 0, arrayIndex);
        }

        public static string RemoveChar(this string value, params char[] charsToRemove)
        {
            var h = new HashSet<char>(charsToRemove);
            char[] array = new char[value.Length];
            int arrayIndex = 0;
            for (int i = 0; i < value.Length; i++)
            {
                char ch = value[i];
                if (!h.Contains(ch))
                {
                    array[arrayIndex++] = ch;
                }
            }

            return new string(array, 0, arrayIndex);
        }

        /// <summary>
        /// Count characters excl. white spaces, ssa-tags, html-tags, control-characters, normal spaces and
        /// Arabic diacritics depending on parameter.
        /// </summary>
        public static int CountCharacters(this string value, string strategy, bool forCps)
        {
            return (int)Math.Round(CalcFactory.MakeCalculator(strategy).CountCharacters(value, forCps), MidpointRounding.AwayFromZero);
        }

        public static decimal CountCharacters(this string value, bool forCps)
        {
            return CalcFactory.MakeCalculator(Configuration.Settings.General.CpsLineLengthStrategy).CountCharacters(value, forCps);
        }

        public static bool HasSentenceEnding(this string value)
        {
            return value.HasSentenceEnding(string.Empty);
        }

        public static bool HasSentenceEnding(this string value, string twoLetterLanguageCode)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            var s = HtmlUtil.RemoveHtmlTags(value, true).TrimEnd('"').TrimEnd('”');
            if (s == string.Empty)
            {
                return false;
            }

            var last = s[s.Length - 1];
            return last == '.' || last == '!' || last == '?' || last == ']' || last == ')' || last == '…' || last == '♪' || last == '؟' ||
                   twoLetterLanguageCode == "el" && last == ';' || twoLetterLanguageCode == "el" && last == '\u037E' ||
                   last == '-' && s.Length > 3 && s.EndsWith("--", StringComparison.Ordinal) && char.IsLetter(s[s.Length - 3]) ||
                   last == '—' && s.Length > 2 && char.IsLetter(s[s.Length - 2]);
        }

        public static string NormalizeUnicode(this string input, Encoding encoding)
        {
            const char defHyphen = '-'; // - Hyphen-minus (\u002D) (Basic Latin)
            const char defColon = ':'; // : Colon (\u003A) (Basic Latin)

            var text = input;

            bool hasSingleMusicNode = true;
            if (encoding.GetString(encoding.GetBytes("♪")) != "♪")
            {
                text = text.Replace('♪', '#');
                hasSingleMusicNode = false;
            }

            if (encoding.GetString(encoding.GetBytes("♫")) != "♫")
            {
                text = text.Replace('♫', hasSingleMusicNode ? '♪' : '#');
            }

            if (encoding.GetString(encoding.GetBytes("©")) != "©")
            {
                text = text.Replace("©", "(Copyright)");
            }

            if (encoding.GetString(encoding.GetBytes("®")) != "®")
            {
                text = text.Replace("®", "(Registered Trademark)");
            }

            if (encoding.GetString(encoding.GetBytes("…")) != "…")
            {
                text = text.Replace("…", "...");
            }

            // Hyphens
            return text.Replace('\u2043', defHyphen) // ⁃ Hyphen bullet (\u2043)
                .Replace('\u2010', defHyphen) // ‐ Hyphen (\u2010)
                .Replace('\u2012', defHyphen) // ‒ Figure dash (\u2012)
                .Replace('\u2013', defHyphen) // – En dash (\u2013)
                .Replace('\u2014', defHyphen) // — Em dash (\u2014)
                .Replace('\u2015', defHyphen) // ― Horizontal bar (\u2015)

                // Colons:
                .Replace('\u02F8', defColon) // ˸ Modifier Letter Raised Colon (\u02F8)
                .Replace('\uFF1A', defColon) // ： Fullwidth Colon (\uFF1A)
                .Replace('\uFE13', defColon) // ︓ Presentation Form for Vertical Colon (\uFE13)

                // Others
                .Replace("⇒", "=>")

                // Spaces
                .Replace('\u00A0', ' ') // No-Break Space
                .Replace("\u200B", string.Empty) // Zero Width Space
                .Replace("\uFEFF", string.Empty) // Zero Width No-Break Space

                // Intellectual property
                .Replace("\u2117", "(Sound-recording Copyright)") // ℗ sound-recording copyright
                .Replace("\u2120", "(Service Mark)") // ℠ service mark
                .Replace("\u2122", "(Trademark)") // ™ trademark

                // RTL/LTR markers
                .Replace("\u202B", string.Empty) // &rlm;
                .Replace("\u202A", string.Empty); // &lmr;
        }
    }
}
