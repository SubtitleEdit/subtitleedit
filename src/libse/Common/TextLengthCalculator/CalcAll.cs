using System;
using System.Globalization;

namespace Nikse.SubtitleEdit.Core.Common.TextLengthCalculator
{
    public class CalcAll : ICalcLength
    {
        /// <summary>
        /// Calculate length of all text including space but excluding composition characters (tags are not counted).
        /// </summary>
        public decimal CountCharacters(string text, bool forCps)
        {
            var s = HtmlUtil.RemoveHtmlTags(text, true);

            // Fast path: runs per grid-row repaint, per keystroke and per waveform frame, so
            // avoid StringInfo.GetTextElementEnumerator's heap allocation and culture-aware
            // grapheme walk when it cannot matter. Everything that can make a text element
            // longer than one char (combining marks, ZWJ, prepend characters, surrogates) or
            // hit the skip list below (zero-width/BiDi controls) is >= U+0300 - except "\r\n",
            // whose chars are both controls and thus not counted either way - so counting
            // non-controls below U+0300 is identical. U+2010-U+2027 (typographic dashes,
            // curly quotes, ellipsis - common in professional subs) are plain punctuation,
            // never part of a longer element and not in the skip list, so count them too;
            // any other char >= U+0300 falls back to the full text-element walk.
            var span = s.AsSpan();
            var isSimple = true;
#if NET8_0_OR_GREATER
            // Both tests below are sparse - a subtitle line has no character above U+02FF at
            // all, and at most a line break's worth of control characters - so hop to the
            // exceptions with a vectorized search instead of testing every character twice.
            var pos = 0;
            while (pos < span.Length)
            {
                var relative = span.Slice(pos).IndexOfAnyExceptInRange('\u0000', '\u02FF');
                if (relative < 0)
                {
                    break;
                }

                var at = pos + relative;
                var c = span[at];
                if (c < '\u2010' || c > '\u2027')
                {
                    isSimple = false;
                    break;
                }

                pos = at + 1;
            }

            if (isSimple)
            {
                // char.IsControl is exactly U+0000-U+001F plus U+007F-U+009F, so the count of
                // non-controls is the length minus the hits in those two ranges.
                var controls = CountInRange(span, '\u0000', '\u001F') + CountInRange(span, '\u007F', '\u009F');
                return s.Length - controls;
            }
#else
            var simpleLength = 0;
            for (var i = 0; i < span.Length; i++)
            {
                var c = span[i];
                if (c >= '\u0300' && (c < '\u2010' || c > '\u2027'))
                {
                    isSimple = false;
                    break;
                }

                if (!char.IsControl(c))
                {
                    simpleLength++;
                }
            }

            if (isSimple)
            {
                return simpleLength;
            }
#endif

            const char zeroWidthSpace = '\u200B';
            const char zeroWidthNoBreakSpace = '\uFEFF';
            var length = 0;
            for (var en = StringInfo.GetTextElementEnumerator(s); en.MoveNext();)
            {
                var element = en.GetTextElement();
                if (element.Length == 1)
                {
                    var ch = element[0];
                    if (!char.IsControl(ch) &&
                        ch != zeroWidthSpace &&
                        ch != zeroWidthNoBreakSpace &&
                        ch != '\u200E' &&
                        ch != '\u200F' &&
                        ch != '\u202A' &&
                        ch != '\u202B' &&
                        ch != '\u202C' &&
                        ch != '\u202D' &&
                        ch != '\u202E')
                    {
                        length++;
                    }
                }
                else
                {
                    if (element != "\r\n")
                    {
                        length++;
                    }
                }
            }

            return length;
        }

#if NET8_0_OR_GREATER
        /// <summary>
        /// Number of characters in [from, to]. Control characters are sparse in subtitle text
        /// (a line break at most), so hopping to them beats a per-character range test.
        /// </summary>
        private static int CountInRange(ReadOnlySpan<char> span, char from, char to)
        {
            var count = 0;
            var pos = 0;
            while (pos < span.Length)
            {
                var relative = span.Slice(pos).IndexOfAnyInRange(from, to);
                if (relative < 0)
                {
                    break;
                }

                count++;
                pos += relative + 1;
            }

            return count;
        }
#endif
    }
}
