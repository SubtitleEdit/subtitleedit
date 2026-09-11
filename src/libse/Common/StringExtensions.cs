using Nikse.SubtitleEdit.Core.Common.TextLengthCalculator;
using System;
using System.Buffers;
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

#if NET8_0_OR_GREATER
        private static readonly SearchValues<char> UnicodeControlCharsSearchValues = SearchValues.Create(UnicodeControlChars);
#endif

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
            if (len < 5 || text[len - 1] != '>')
            {
                return false;
            }

            // </font> </i>
            if (threeLengthTag && text[len - 4] == '<' && text[len - 3] == '/')
            {
                return true;
            }

            if (includeFont && len >= 8 && text[len - 7] == '<' && text[len - 6] == '/')
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
            if (newLineIdx < 0 || text.Length < newLineIdx + Environment.NewLine.Length + 3)
            {
                return false;
            }

            text = text.Substring(newLineIdx + Environment.NewLine.Length);
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

        /// <summary>
        /// "Does the text at <paramref name="index"/> start with <paramref name="value"/>" without
        /// allocating the remainder - the <c>s.Substring(index).StartsWith(value)</c> the tag-walking
        /// writer loops used copied the whole rest of the line for every probe of every character.
        /// </summary>
        public static bool StartsWithAt(this string s, int index, string value, StringComparison comparison)
        {
            return s.AsSpan(index).StartsWith(value.AsSpan(), comparison);
        }

        /// <summary>
        /// "Does the text before <paramref name="endExclusive"/> end with <paramref name="value"/>"
        /// without allocating the prefix (<c>s.Substring(0, end).EndsWith(value)</c>).
        /// </summary>
        public static bool EndsWithAt(this string s, int endExclusive, string value, StringComparison comparison)
        {
            return s.AsSpan(0, endExclusive).EndsWith(value.AsSpan(), comparison);
        }

        public static bool StartsWith(this string s, char c)
        {
            return s.Length > 0 && s[0] == c;
        }

        public static bool EndsWith(this string s, char c)
        {
            return s.Length > 0 && s[s.Length - 1] == c;
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

        public static List<string> SplitToLines(this string s) => s.SplitToLines(s.Length);

        /// <summary>
        /// Rewrites every line break to <see cref="Environment.NewLine"/>, so text coming from
        /// the outside (a paste, an API answer) uses the same line break as text SE builds
        /// itself. The breaks recognized - and the "\r\r\n" as two breaks rule (#8854) - are the
        /// ones <see cref="SplitToLines(string)"/> splits on, so a normalize-then-split round
        /// trip keeps the same lines. The input is returned unchanged when it needs no rewrite.
        /// </summary>
        public static string NormalizeLineBreaks(this string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return s;
            }

            var index = s.AsSpan().IndexOfAny('\r', '\n', '\u2028');
            if (index < 0)
            {
                return s;
            }

            var newLine = Environment.NewLine;
            var sb = new StringBuilder(s.Length + 8);
            var start = 0;
            while (index >= 0)
            {
                sb.Append(s, start, index - start).Append(newLine);

                start = index + 1;
                if (s[index] == '\r' && start < s.Length && s[start] == '\n')
                {
                    start++;
                }

                var next = s.AsSpan(start).IndexOfAny('\r', '\n', '\u2028');
                index = next < 0 ? -1 : start + next;
            }

            return sb.Append(s, start, s.Length - start).ToString();
        }

        /// <summary>
        /// Enumerates the lines of <paramref name="s"/> as spans, using the same line break rules
        /// as <see cref="SplitToLines(string)"/>. For callers that only look at each line instead
        /// of keeping it: no list and no string per line is allocated.
        /// </summary>
        public static LineSpanEnumerator EnumerateSpanLines(this string s) => new LineSpanEnumerator(s.AsSpan());

        public ref struct LineSpanEnumerator
        {
            private ReadOnlySpan<char> _remaining;
            private bool _done;

            public LineSpanEnumerator(ReadOnlySpan<char> span)
            {
                _remaining = span;
                Current = default;
                _done = false;
            }

            public ReadOnlySpan<char> Current { get; private set; }

            public LineSpanEnumerator GetEnumerator() => this;

            public bool MoveNext()
            {
                if (_done)
                {
                    return false;
                }

                var idx = _remaining.IndexOfAny('\r', '\n', '\u2028');
                if (idx < 0)
                {
                    Current = _remaining;
                    _done = true;
                    return true;
                }

                Current = _remaining.Slice(0, idx);

                // "\r\r\n" is deliberately two line breaks, same as SplitToLines.
                var skip = idx + 1;
                if (_remaining[idx] == '\r' && idx + 1 < _remaining.Length && _remaining[idx + 1] == '\n')
                {
                    skip++;
                }

                _remaining = _remaining.Slice(skip);
                return true;
            }
        }

        public static List<string> SplitToLines(this string s, int max)
        {
            //original non-optimized version: return source.Replace("\r\r\n", "\n").Replace("\r\n", "\n").Replace('\r', '\n').Replace('\u2028', '\n').Split('\n');
            // See https://github.com/SubtitleEdit/subtitleedit/issues/8854 - "\r\r\n" is
            // deliberately two line breaks (following how VS Code opens such files).
            // Line breaks are sparse, so hop between them with vectorized IndexOfAny
            // instead of testing every char.

            if (s.Length < max)
            {
                max = s.Length;
            }

            var lines = new List<string>();
            var span = s.AsSpan(0, max);
            var start = 0;
            var pos = 0;
            while (true)
            {
                var idx = span.Slice(pos).IndexOfAny('\r', '\n', '\u2028');
                if (idx < 0)
                {
                    break;
                }

                var i = pos + idx;
                lines.Add(s.Substring(start, i - start));
                if (span[i] == '\r' && i + 1 < max && span[i + 1] == '\n') // \r\n
                {
                    i++;
                }

                pos = start = i + 1;
            }

            lines.Add(start == 0 && max == s.Length ? s : s.Substring(start, max - start));
            return lines;
        }

        public static int CountWords(this string source)
        {
            // Called per line on grid repaints (words-per-minute) - count boundaries directly
            // instead of allocating a separator array plus one substring per word.
            var text = HtmlUtil.RemoveHtmlTags(source, true);
            var count = 0;
            var inWord = false;
            for (var i = 0; i < text.Length; i++)
            {
                var ch = text[i];
                if (ch == ' ' || ch == '\n' || ch == '\r')
                {
                    inWord = false;
                }
                else if (!inWord)
                {
                    inWord = true;
                    count++;
                }
            }

            return count;
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

            // Rules (unchanged): a leading space run is kept as-is; an interior/trailing run
            // collapses to one space, or to nothing when a line break precedes or follows it.
            // Single-pass rewrite - the previous reverse scan called string.Remove per space
            // run, allocating a new string for every run on space-heavy lines.
            var len = s.Length;
            var first = 0;
            while (first < len && s[first] == ' ')
            {
                first++;
            }

            // Find the first run that needs a change; most lines have none and return the
            // original instance without allocating.
            var changeAt = -1;
            for (var i = first; i < len; i++)
            {
                if (s[i] != ' ')
                {
                    continue;
                }

                var runStart = i;
                while (i + 1 < len && s[i + 1] == ' ')
                {
                    i++;
                }

                var runEnd = i + 1;
                var before = s[runStart - 1];
                var removeAll = before == '\n' || before == '\r' || (runEnd < len && (s[runEnd] == '\n' || s[runEnd] == '\r'));
                if (runEnd - runStart != (removeAll ? 0 : 1))
                {
                    changeAt = runStart;
                    break;
                }
            }

            if (changeAt < 0)
            {
                return s;
            }

            var buffer = ArrayPool<char>.Shared.Rent(len);
            s.AsSpan(0, changeAt).CopyTo(buffer);
            var pos = changeAt;
            for (var i = changeAt; i < len; i++)
            {
                var ch = s[i];
                if (ch != ' ')
                {
                    buffer[pos++] = ch;
                    continue;
                }

                var runStart = i;
                while (i + 1 < len && s[i + 1] == ' ')
                {
                    i++;
                }

                var runEnd = i + 1;
                var before = s[runStart - 1]; // runStart >= changeAt >= 1 (leading run was skipped)
                var removeAll = before == '\n' || before == '\r' || (runEnd < len && (s[runEnd] == '\n' || s[runEnd] == '\r'));
                if (!removeAll)
                {
                    buffer[pos++] = ' ';
                }
            }

            var result = new string(buffer, 0, pos);
            ArrayPool<char>.Shared.Return(buffer);
            return result;
        }

        // note: replace both input and output variable type with ReadOnlySpan<char> when in more modern .NET
        // that will make it allocation free
        public static string RemoveRecursiveLineBreaks(this string input)
        {
            var len = input.Length;
            var writeIndex = len - 1;
            var isLineBreakAdjacent = false;
            var buffer = new char[len];

            // windows line break style
            var hasCarriageReturn = input.Contains('\r');

            for (int i = len - 1; i >= 0; i--)
            {
                var charAtIndex = input[i];
                // carriage return line feed
                if ((hasCarriageReturn && charAtIndex == '\r') || charAtIndex == '\n')
                {
                    // line break is adjacent but we found another line break - ignore it
                    if (isLineBreakAdjacent)
                    {
                        continue;
                    }

                    // write into buffer and update the flag
                    buffer[writeIndex--] = charAtIndex;
                    isLineBreakAdjacent = charAtIndex == '\r' || (!hasCarriageReturn && charAtIndex == '\n');
                }
                else
                {
                    // write current character to the buffer and decrement the write-index
                    buffer[writeIndex--] = charAtIndex;
                    // update adjacent line break flag
                    isLineBreakAdjacent = false;
                }
            }

            return new string(buffer, writeIndex + 1, len - (writeIndex + 1));
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

            var len = s.Length;
            for (var i = 0; i < len; i++)
            {
                if (CharUtils.IsAsciiDigit(s[i]))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Same answer as <c>string.IsNullOrEmpty(value.RemoveChar(chars))</c>, without building
        /// the stripped string. Prepare <paramref name="chars"/> once in a static field.
        /// </summary>
        public static bool IsOnlyChars(this string value, CharLookup chars) => chars.IsOnly(value);

        /// <summary>
        /// Same answer as <c>string.IsNullOrWhiteSpace(value.RemoveChar(chars))</c>, without
        /// building the stripped string. Prepare <paramref name="chars"/> once in a static field.
        /// </summary>
        public static bool IsOnlyCharsOrWhiteSpace(this string value, CharLookup chars) => chars.IsOnlyOrWhiteSpace(value);

        public static bool ContainsUnicodeControlChars(this string s)
        {
#if NET8_0_OR_GREATER
            return s.AsSpan().ContainsAny(UnicodeControlCharsSearchValues);
#else
            return s.Contains(UnicodeControlChars);
#endif
        }

        public static string RemoveControlCharacters(this string s)
        {
            // char.IsControl matches exactly U+0000-U+001F and U+007F-U+009F; almost all
            // input has none, so return the original instance without allocating.
            var span = s.AsSpan();
#if NET8_0_OR_GREATER
            if (!span.ContainsAnyInRange('\u0000', '\u001F') && !span.ContainsAnyInRange('\u007F', '\u009F'))
            {
                return s;
            }
#else
            var hasControl = false;
            foreach (var c in span)
            {
                if (char.IsControl(c))
                {
                    hasControl = true;
                    break;
                }
            }

            if (!hasControl)
            {
                return s;
            }
#endif

            var count = 0;
            foreach (var ch in span)
            {
                if (char.IsControl(ch))
                {
                    count++;
                }
            }

            return string.Create(s.Length - count, s, (chars, state) =>
            {
                var index = 0;
                foreach (var ch in state)
                {
                    if (!char.IsControl(ch))
                    {
                        chars[index++] = ch;
                    }
                }
            });
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
#if NET8_0_OR_GREATER
                if (!char.IsControl(ch) && !char.IsWhiteSpace(ch) && !UnicodeControlCharsSearchValues.Contains(ch))
#else
                if (!char.IsControl(ch) && !char.IsWhiteSpace(ch) && !UnicodeControlChars.Contains(ch))
#endif
                {
                    return false;
                }
            }

            return true;
        }

        public static string RemoveControlCharactersButWhiteSpace(this string s)
        {
            var max = s.Length;

            // The removed set is char.IsControl minus CR/LF/TAB, i.e. U+0000-U+0008,
            // U+000B-U+000C, U+000E-U+001F and U+007F-U+009F; almost no line contains any
            // of those, so return the original instance without allocating a copy.
            var span = s.AsSpan();
#if NET8_0_OR_GREATER
            if (!span.ContainsAnyInRange('\u0000', '\u0008') &&
                !span.ContainsAnyInRange('\u000B', '\u000C') &&
                !span.ContainsAnyInRange('\u000E', '\u001F') &&
                !span.ContainsAnyInRange('\u007F', '\u009F'))
            {
                return s;
            }
#else
            var hasControl = false;
            foreach (var c in span)
            {
                if (char.IsControl(c) && c != '\u000d' && c != '\u000a' && c != '\u0009')
                {
                    hasControl = true;
                    break;
                }
            }

            if (!hasControl)
            {
                return s;
            }
#endif

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
            if (ci == null)
            {
                ci = CultureInfo.CurrentCulture;
            }

            if (s.Length > 0 && s[0] < 0x80)
            {
                // ASCII first char: it cannot be part of a surrogate pair or carry combining
                // marks, so the StringInfo text-element machinery (which allocates heavily)
                // is not needed.
                var up = char.ToUpper(s[0], ci);
                if (up == s[0])
                {
                    return s;
                }

                return string.Create(s.Length, (s, up), (chars, state) =>
                {
                    chars[0] = state.up;
                    state.s.AsSpan(1).CopyTo(chars.Slice(1));
                });
            }

            var si = new StringInfo(s);
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
            return RestoreSavedAndRemovedTags(properCaseText, tags);
        }

        public static string ToLowercaseButKeepTags(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return input;
            }

            var sb = new StringBuilder();
            var tags = RemoveAndSaveTags(input, sb, new SubRip());
            var lowercaseText = sb.ToString().ToLowerInvariant();
            var result = RestoreSavedAndRemovedTags(lowercaseText, tags);
            return result;
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
                return RestoreSavedAndRemovedTags(text.ToUpperInvariant(), tags);
            }

            if (containsUppercase)
            {
                return RestoreSavedAndRemovedTags(text.ToLowerInvariant(), tags);
            }

            return RestoreSavedAndRemovedTags(System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(text), tags);
        }

        private static string RestoreSavedAndRemovedTags(string input, List<KeyValuePair<int, string>> tags)
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
                           && index + 1 < input.Length
                           && (input[index + 1] == 'N' || input[index + 1] == 'n' || input[index + 1] == 'h'))
                {
                    // sb.Length, not index: every other tag is recorded in tag-stripped
                    // coordinates, which is what RestoreSavedAndRemovedTags re-inserts into. Using
                    // the input offset put "\N" back in the wrong place whenever a tag preceded it
                    // ("{\an8}One\Ntwo" toggled to "{\an8}ONETWO\N").
                    tags.Add(new KeyValuePair<int, string>(sb.Length, input.Substring(index, 2)));
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
                    var s = input.AsSpan(index);
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
                    var s = input.AsSpan(index);
                    if (s.StartsWith("{\\", StringComparison.Ordinal))
                    {
                        tagOn = true;
                        tagIndex = sb.Length;
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

            // Escape non-ASCII chars, appending the escape parts separately - the old
            // "\\u" + value + "?" concatenation allocated two strings per non-ASCII char,
            // which adds up fast on CJK/Cyrillic subtitles.
            var escaped = backslashed.ToString();
            var sb = new StringBuilder(escaped.Length + 16);
            foreach (var character in escaped)
            {
                if (character <= 0x7f)
                {
                    sb.Append(character);
                }
                else
                {
                    sb.Append("\\u").Append((int)character).Append('?');
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
#if NET10_0_OR_GREATER
            var count = value.AsSpan().Count(charToRemove);
#else
            var count = 0;
            for (var i = 0; i < value.Length; i++)
            {
                if (value[i] == charToRemove)
                {
                    count++;
                }
            }
#endif
            if (count == 0)
            {
                return value;
            }

            return string.Create(value.Length - count, (value, charToRemove), (chars, state) =>
            {
                var index = 0;
                foreach (var ch in state.value)
                {
                    if (ch != state.charToRemove)
                    {
                        chars[index++] = ch;
                    }
                }
            });
        }

        public static string RemoveChar(this string value, char charToRemove, char charToRemove2)
        {
            if (charToRemove == charToRemove2)
            {
                return value.RemoveChar(charToRemove);
            }

#if NET10_0_OR_GREATER
            var span = value.AsSpan();
            var count = span.Count(charToRemove) + span.Count(charToRemove2);
#else
            var count = 0;
            for (var i = 0; i < value.Length; i++)
            {
                var ch = value[i];
                if (ch == charToRemove || ch == charToRemove2)
                {
                    count++;
                }
            }
#endif
            if (count == 0)
            {
                return value;
            }

            return string.Create(value.Length - count, (value, charToRemove, charToRemove2), (chars, state) =>
            {
                var index = 0;
                foreach (var ch in state.value)
                {
                    if (ch != state.charToRemove && ch != state.charToRemove2)
                    {
                        chars[index++] = ch;
                    }
                }
            });
        }

#if NET10_0_OR_GREATER
        private ref struct RemoveCharContext
        {
            public string Value;
            public ReadOnlySpan<char> CharsToRemove;
            public int First;
        }

        public static string RemoveChar(this string value, params ReadOnlySpan<char> charsToRemove)
        {
            // Callers pass a handful of literal chars (3-13). Matches are sparse in subtitle
            // text, so hopping between them with vectorized IndexOfAny and block-copying the
            // clean stretches beats testing every char against the removal set.
            var span = value.AsSpan();
            var first = span.IndexOfAny(charsToRemove);
            if (first < 0)
            {
                return value;
            }

            var count = 1;
            var rest = span.Slice(first + 1);
            while (true)
            {
                var i = rest.IndexOfAny(charsToRemove);
                if (i < 0)
                {
                    break;
                }

                count++;
                rest = rest.Slice(i + 1);
            }

            var context = new RemoveCharContext { Value = value, CharsToRemove = charsToRemove, First = first };
            return string.Create(value.Length - count, context, (chars, state) =>
            {
                var src = state.Value.AsSpan();
                src.Slice(0, state.First).CopyTo(chars);
                var written = state.First;
                src = src.Slice(state.First + 1);
                while (true)
                {
                    var i = src.IndexOfAny(state.CharsToRemove);
                    if (i < 0)
                    {
                        src.CopyTo(chars.Slice(written));
                        return;
                    }

                    src.Slice(0, i).CopyTo(chars.Slice(written));
                    written += i;
                    src = src.Slice(i + 1);
                }
            });
        }
#else
        public static string RemoveChar(this string value, params char[] charsToRemove)
        {
            // Callers pass a handful of literal chars (3-13). Matches are sparse in subtitle
            // text, so hopping between them with IndexOfAny and block-copying the clean
            // stretches beats testing every char against the removal set.
            var span = value.AsSpan();
            var first = span.IndexOfAny(charsToRemove);
            if (first < 0)
            {
                return value;
            }

            var count = 1;
            var rest = span.Slice(first + 1);
            while (true)
            {
                var i = rest.IndexOfAny(charsToRemove);
                if (i < 0)
                {
                    break;
                }

                count++;
                rest = rest.Slice(i + 1);
            }

            return string.Create(value.Length - count, (value, charsToRemove, first), (chars, state) =>
            {
                var src = state.value.AsSpan();
                src.Slice(0, state.first).CopyTo(chars);
                var written = state.first;
                src = src.Slice(state.first + 1);
                while (true)
                {
                    var i = src.IndexOfAny<char>(state.charsToRemove);
                    if (i < 0)
                    {
                        src.CopyTo(chars.Slice(written));
                        return;
                    }

                    src.Slice(0, i).CopyTo(chars.Slice(written));
                    written += i;
                    src = src.Slice(i + 1);
                }
            });
        }
#endif

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

        private static bool IsNeutralSentenceEndingChar(char c)
        {
            switch (c)
            {
                case '.':
                case '!':
                case '?':
                case ']':
                case ')':
                case '…':
                case '♪':
                case '؟':
                case '。':
                case '？':
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsGreekSentenceEndingChar(char c)
        {
            return c == '\u037E' || c == ';';
        }

        public static bool HasSentenceEnding(this string value, string twoLetterLanguageCode)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            var len = value.Length;
            var checkIndex = len - 1;

            // skip quotes
            while (checkIndex >= 0 && (value[checkIndex] == '"' || value[checkIndex] == '”'))
            {
                checkIndex--;
            }

            // value contains only quotes
            if (checkIndex < 0)
            {
                return false;
            }


            var charAtIndex = value[checkIndex];
            // handles when sentence ending char is adjacent with html/assa closing tags e.g: </i>, </font>, {\\i0}...
            while (charAtIndex == '>' || charAtIndex == '}')
            {
                if (charAtIndex == '>')
                {
                    checkIndex = value.LastIndexOf('<', checkIndex) - 1;
                }
                else if (charAtIndex == '}')
                {
                    checkIndex = value.LastIndexOf('{', checkIndex) - 1;
                }

                // in this case '>' or '}' is the last char
                if (checkIndex < 0)
                {
                    return false;
                }

                charAtIndex = value[checkIndex];
            }

            // ending with dash/hyphen
            if (charAtIndex == '-')
            {
                // foobar--
                return checkIndex > 1 && char.IsLetter(value[checkIndex - 2]) && value[checkIndex - 1] == '-';
            }

            // em dash: used in written English to indicate an interruption or break in thought
            if (charAtIndex == '—') // U+2014
            {
                // foobar—
                return checkIndex > 0 && char.IsLetter(value[checkIndex - 1]);
            }

            // evaluate culture type
            var isCultureNeutral = twoLetterLanguageCode == null || twoLetterLanguageCode.Equals("el", StringComparison.OrdinalIgnoreCase) == false;
            return IsNeutralSentenceEndingChar(charAtIndex) || (!isCultureNeutral && IsGreekSentenceEndingChar(charAtIndex));
        }
    }
}
