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
            var simpleLength = 0;
            var isSimple = true;
            for (var i = 0; i < s.Length; i++)
            {
                var c = s[i];
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
    }
}
