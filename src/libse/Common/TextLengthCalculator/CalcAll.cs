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
                    length++;
                }
            }

            return length;
        }
    }
}
