using System.Globalization;

namespace Nikse.SubtitleEdit.Core.Common.TextLengthCalculator
{
    public class CalcIgnoreArabicDiacriticsNoSpace : ICalcLength
    {
        /// <summary>
        /// Calculate all text excluding Arabic Diacritics and space (tags are not counted).
        /// </summary>
        public decimal CountCharacters(string text, bool forCps)
        {
            var s = HtmlUtil.RemoveHtmlTags(text, true);

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
                else if (element != "\u200E" &&
                         element != "\u200F" &&
                         element != "\u202A" &&
                         element != "\u202B" &&
                         element != "\u202C" &&
                         element != "\u202D" &&
                         element != "\u202E" &&
                         element != "\u064B" &&
                         element != "\u064C" &&
                         element != "\u064D" &&
                         element != "\u064E" &&
                         element != "\u064F" &&
                         element != "\u0650" &&
                         element != "\u0651" &&
                         element != "\u0652" &&
                         element != "\u0653")
                {
                    length++;
                }
            }

            return length;
        }
    }
}
