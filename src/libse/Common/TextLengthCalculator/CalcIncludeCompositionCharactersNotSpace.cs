using System.Globalization;

namespace Nikse.SubtitleEdit.Core.Common.TextLengthCalculator
{
    public class CalcIncludeCompositionCharactersNotSpace : ICalcLength
    {
        /// <summary>
        /// Calculate all text including composition characters but not space (tags are not counted).
        /// </summary>
        public decimal CountCharacters(string text, bool forCps)
        {
            var s = HtmlUtil.RemoveHtmlTags(text, true);

            // Fast path: the Arabic composition characters excluded below are all >= U+0300
            // and so cannot pass the probe; "\r\n" can, and this calculator scores a
            // multi-char element by its length, hence the two per pair.
            if (TextElements.AreAllSingleChar(s, out var crLfCount))
            {
                var simpleLength = 0;
                foreach (var c in s)
                {
                    if (!char.IsControl(c) && c != ' ')
                    {
                        simpleLength++;
                    }
                }

                return simpleLength + crLfCount * 2;
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
                        ch != ' ' &&
                        ch != zeroWidthSpace &&
                        ch != zeroWidthNoBreakSpace &&
                        ch != '\u200E' &&
                        ch != '\u200F' &&
                        ch != '\u202A' &&
                        ch != '\u202B' &&
                        ch != '\u202C' &&
                        ch != '\u202D' &&
                        ch != '\u202E' &&
                        !(ch >= '\u064B' && ch <= '\u0653'))
                    {
                        length++;
                    }
                }
                else
                {
                    length += element.Length;
                }
            }

            return length;
        }
    }
}
