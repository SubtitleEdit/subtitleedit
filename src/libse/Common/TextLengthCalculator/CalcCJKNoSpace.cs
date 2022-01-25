namespace Nikse.SubtitleEdit.Core.Common.TextLengthCalculator
{
    public class CalcCjkNoSpace : ICalcLength
    {
        /// <summary>
        /// Calculate all text including space (tags are not counted).
        /// </summary>
        public decimal CountCharacters(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return 0;
            }

            const char zeroWidthSpace = '\u200B';
            const char zeroWidthNoBreakSpace = '\uFEFF';
            var count = 0m;
            var ssaTagOn = false;
            var htmlTagOn = false;
            var max = text.Length;
            for (var i = 0; i < max; i++)
            {
                var ch = text[i];
                if (ssaTagOn)
                {
                    if (ch == '}')
                    {
                        ssaTagOn = false;
                    }
                }
                else if (htmlTagOn)
                {
                    if (ch == '>')
                    {
                        htmlTagOn = false;
                    }
                }
                else if (ch == '{' && i < text.Length - 1 && text[i + 1] == '\\')
                {
                    ssaTagOn = true;
                }
                else if (ch == '<' && i < text.Length - 1 && (text[i + 1] == '/' || char.IsLetter(text[i + 1])) &&
                         text.IndexOf('>', i) > 0 && TextLengthHelper.IsKnownHtmlTag(text, i))
                {
                    htmlTagOn = true;
                }
                else if (!char.IsControl(ch) &&
                         ch != ' ' &&
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
                    if (CalcCjk.JapaneseHalfWidthCharacters.Contains(ch))
                    {
                        count += 0.5m;
                    }
                    else if (CalcCjk.ChineseFullWidthPunctuations.Contains(ch) ||
                             LanguageAutoDetect.KoreanLetters.Contains(ch) ||
                             LanguageAutoDetect.KoreanLetters.Contains(ch) ||
                             CalcCjk.IsCjk(ch))
                    {
                        count++;
                    }
                    else
                    {
                        count += 0.5m;
                    }
                }
            }

            return count;
        }
    }
}
