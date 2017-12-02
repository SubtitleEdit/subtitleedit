using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Nikse.SubtitleEdit.Core
{
    public static class StringExtensions
    {
        public static bool LineStartsWithHtmlTag(this string text, bool threeLengthTag, bool includeFont = false)
        {
            if (text == null || (!threeLengthTag && !includeFont))
                return false;
            return StartsWithHtmlTag(text, threeLengthTag, includeFont);
        }

        public static bool LineEndsWithHtmlTag(this string text, bool threeLengthTag, bool includeFont = false)
        {
            if (text == null)
                return false;

            var len = text.Length;
            if (len < 6 || text[len - 1] != '>')
                return false;

            // </font> </i>
            if (threeLengthTag && len > 3 && text[len - 4] == '<' && text[len - 3] == '/')
                return true;
            if (includeFont && len > 8 && text[len - 7] == '<' && text[len - 6] == '/')
                return true;
            return false;
        }

        public static bool LineBreakStartsWithHtmlTag(this string text, bool threeLengthTag, bool includeFont = false)
        {
            if (text == null || (!threeLengthTag && !includeFont))
                return false;
            var newLineIdx = text.IndexOf(Environment.NewLine, StringComparison.Ordinal);
            if (newLineIdx < 0 || text.Length < newLineIdx + 5)
                return false;
            text = text.Substring(newLineIdx + 2);
            return StartsWithHtmlTag(text, threeLengthTag, includeFont);
        }

        private static bool StartsWithHtmlTag(string text, bool threeLengthTag, bool includeFont)
        {
            if (threeLengthTag && text.Length >= 3 && text[0] == '<' && text[2] == '>' && (text[1] == 'i' || text[1] == 'I' || text[1] == 'u' || text[1] == 'U' || text[1] == 'b' || text[1] == 'B'))
                return true;
            if (includeFont && text.Length > 5 && text.StartsWith("<font", StringComparison.OrdinalIgnoreCase))
                return text.IndexOf('>', 5) >= 5; // <font> or <font color="#000000">
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

        public static List<string> SplitToLines(this string s)
        {
            //original non-optimized version: return source.Replace("\r\r\n", "\n").Replace("\r\n", "\n").Replace('\r', '\n').Replace('\u2028', '\n').Split('\n');

            var lines = new List<string>();
            int start = 0;
            int max = s.Length;
            int i = 0;
            while (i < max)
            {
                var ch = s[i];
                if (ch == '\r')
                {
                    if (i < s.Length - 2 && s[i + 1] == '\r' && s[i + 2] == '\n') // \r\r\n
                    {
                        lines.Add(start < i ? s.Substring(start, i - start) : string.Empty);
                        i += 3;
                        start = i;
                        continue;
                    }
                    if (i < s.Length - 1 && s[i + 1] == '\n') // \r\n
                    {
                        lines.Add(start < i ? s.Substring(start, i - start) : string.Empty);
                        i += 2;
                        start = i;
                        continue;
                    }
                    lines.Add(start < i ? s.Substring(start, i - start) : string.Empty);
                    i++;
                    start = i;
                    continue;
                }
                if (ch == '\n' || ch == '\u2028')
                {
                    lines.Add(start < i ? s.Substring(start, i - start) : string.Empty);
                    i++;
                    start = i;
                    continue;
                }
                i++;
            }
            lines.Add(start < i ? s.Substring(start, i - start) : string.Empty);
            return lines;
        }

        public static int CountWords(this string source)
        {
            return HtmlUtil.RemoveHtmlTags(source, true).Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
        }

        // http://www.codeproject.com/Articles/43726/Optimizing-string-operations-in-C
        public static int FastIndexOf(this string source, string pattern)
        {
            if (pattern == null) throw new ArgumentNullException();
            if (pattern.Length == 0) return 0;
            if (pattern.Length == 1) return source.IndexOf(pattern[0]);
            int limit = source.Length - pattern.Length + 1;
            if (limit < 1) return -1;
            // Store the first 2 characters of "pattern"
            char c0 = pattern[0];
            char c1 = pattern[1];
            // Find the first occurrence of the first character
            int first = source.IndexOf(c0, 0, limit);
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
                bool found = true;
                for (var j = 2; j < pattern.Length; j++)
                    if (source[first + j] != pattern[j])
                    {
                        found = false;
                        break;
                    }
                // If the whole word was found, return its index, otherwise try again
                if (found) return first;
                first = source.IndexOf(c0, ++first, limit - first);
            }
            return -1;
        }

        public static int IndexOfAny(this string s, string[] words, StringComparison comparisonType)
        {
            if (words == null || string.IsNullOrEmpty(s))
                return -1;
            for (int i = 0; i < words.Length; i++)
            {
                var idx = s.IndexOf(words[i], comparisonType);
                if (idx >= 0)
                    return idx;
            }
            return -1;
        }

        public static string FixExtraSpaces(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return s;

            int len = s.Length;
            var sb = new StringBuilder(s.Length);
            bool lastWasWhitespace = false;
            for (int i = 0; i < len; i++)
            {
                char ch = s[i];
                if (ch == ' ')
                {
                    if (lastWasWhitespace)
                    {
                        continue;
                    }
                    if (i < len - 1)
                    {
                        var next = s[i + 1];
                        if (next == '\n' || next == '\r')
                        {
                            continue;
                        }
                    }
                    sb.Append(ch);
                    lastWasWhitespace = true;
                }
                else
                {
                    sb.Append(ch);
                    lastWasWhitespace = ch == '\n' || ch == '\r';
                }
            }
            return sb.ToString();
        }

        public static bool ContainsLetter(this string s)
        {
            if (s != null)
            {
                foreach (var index in StringInfo.ParseCombiningCharacters(s))
                {
                    var uc = CharUnicodeInfo.GetUnicodeCategory(s, index);
                    if (uc == UnicodeCategory.LowercaseLetter || uc == UnicodeCategory.UppercaseLetter || uc == UnicodeCategory.TitlecaseLetter || uc == UnicodeCategory.ModifierLetter || uc == UnicodeCategory.OtherLetter)
                        return true;
                }
            }
            return false;
        }

        public static string RemoveControlCharacters(this string s)
        {
            int max = s.Length;
            var newStr = new char[max];
            int newIdx = 0;
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

        public static string RemoveControlCharactersButWhiteSpace(this string s)
        {
            int max = s.Length;
            var newStr = new char[max];
            int newIdx = 0;
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
                ci = CultureInfo.CurrentCulture;
            if (si.LengthInTextElements > 0)
                s = si.SubstringByTextElements(0, 1).ToUpper(ci);
            if (si.LengthInTextElements > 1)
                s += si.SubstringByTextElements(1);
            return s;
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
                    sb.Append(character);
                else
                    sb.Append("\\u" + Convert.ToUInt32(character) + "?");
            }
            return sb.ToString();
        }

        public static string FromRtf(this string value)
        {
            return RichTextToPlainText.ConvertToText(value);
        }

    }
}
