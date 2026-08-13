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

            // Fast path: both chars of the "\r\n" the probe lets through are controls and so
            // are not counted here either way, matching the element != "\r\n" test below.
            if (TextElements.AreAllSingleChar(s, out _))
            {
                var simpleLength = 0;
                foreach (var c in s)
                {
                    if (!char.IsControl(c))
                    {
                        simpleLength++;
                    }
                }

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
