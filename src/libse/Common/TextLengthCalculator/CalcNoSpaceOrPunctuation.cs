using System.Globalization;

namespace Nikse.SubtitleEdit.Core.Common.TextLengthCalculator
{
    public class CalcNoSpaceOrPunctuation : ICalcLength
    {
        /// <summary>
        /// Calculate all text except punctuation or space (tags are not counted).
        /// </summary>
        public decimal CountCharacters(string text, bool forCps)
        {
            var s = HtmlUtil.RemoveHtmlTags(text, true);

            // Fast path: the same test as the single-char branch below, plus one per "\r\n" -
            // the only multi-char element that can pass the probe, and any multi-char element
            // counts as one here.
            if (TextElements.AreAllSingleChar(s, out var crLfCount))
            {
                var simpleLength = 0;
                foreach (var c in s)
                {
                    if (IsCounted(c))
                    {
                        simpleLength++;
                    }
                }

                return simpleLength + crLfCount;
            }

            var length = 0;
            for (var en = StringInfo.GetTextElementEnumerator(s); en.MoveNext();)
            {
                var element = en.GetTextElement();
                if (element.Length == 1)
                {
                    if (IsCounted(element[0]))
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

        private static bool IsCounted(char ch)
        {
            const char zeroWidthSpace = '\u200B';
            const char zeroWidthNoBreakSpace = '\uFEFF';
            return !char.IsControl(ch) &&
                   ch != ' ' &&
                   ch != '\t' &&
                   ch != zeroWidthSpace &&
                   ch != zeroWidthNoBreakSpace &&
                   ch != '\u200E' &&
                   ch != '\u200F' &&
                   ch != '\u202A' &&
                   ch != '\u202B' &&
                   ch != '\u202C' &&
                   ch != '\u202D' &&
                   ch != '\u202E' &&
                   ch != '\u02F8' &&// ˸ Modifier Letter Raised Colon (\u02F8)
                   ch != '\uFF1A' && // ： Fullwidth Colon (\uFF1A)
                   ch != '\uFE13' && // ︓ Presentation Form for Vertical Colon (\uFE13)
                   ch != '\u2043' && // ⁃ Hyphen bullet (\u2043)
                   ch != '\u2010' && // ‐ Hyphen (\u2010)
                   ch != '\u2012' && // ‒ Figure dash (\u2012)
                   ch != '\u2013' && // – En dash (\u2013)
                   ch != '-' &&
                   ch != '\'' &&
                   ch != '"' &&
                   ch != ':' &&
                   ch != '(' &&
                   ch != ')' &&
                   ch != '{' &&
                   ch != '}' &&
                   ch != '[' &&
                   ch != ']' &&
                   ch != '…' &&
                   ch != ',' &&
                   ch != '.' &&
                   ch != '!' &&
                   ch != '?' &&
                   ch != '¡' &&
                   ch != '¿';
        }
    }
}
