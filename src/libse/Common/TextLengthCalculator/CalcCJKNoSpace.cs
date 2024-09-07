using System.Globalization;

namespace Nikse.SubtitleEdit.Core.Common.TextLengthCalculator
{
    public class CalcCjkNoSpace : ICalcLength
    {
        /// <summary>
        /// Calculate all text including space (tags are not counted).
        /// </summary>
        public decimal CountCharacters(string text, bool forCps)
        {
            if (string.IsNullOrEmpty(text))
            {
                return 0;
            }

            var s = HtmlUtil.RemoveHtmlTags(text, true);

            const char zeroWidthSpace = '\u200B';
            const char zeroWidthNoBreakSpace = '\uFEFF';
            decimal length = 0;
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
                        ch != '\u202E')
                    {
                        if (CalcCjk.JapaneseHalfWidthCharacters.Contains(ch))
                        {
                            length += 0.5m;
                        }
                        else if (CalcCjk.ChineseFullWidthPunctuations.Contains(ch) ||
                                 LanguageAutoDetect.Letters.Japanese.Contains(ch) ||
                                 LanguageAutoDetect.Letters.Korean.Contains(ch) ||
                                 CalcCjk.IsCjk(ch))
                        {
                            length++;
                        }
                        else
                        {
                            length += 0.5m;
                        }
                    }
                }
                else
                {
                    if (CalcCjk.JapaneseHalfWidthCharacters.Contains(element))
                    {
                        length += 0.5m;
                    }
                    else if (CalcCjk.ChineseFullWidthPunctuations.Contains(element) ||
                             LanguageAutoDetect.Letters.Japanese.Contains(element) ||
                             LanguageAutoDetect.Letters.Korean.Contains(element) ||
                             CalcCjk.CjkCharRegex.IsMatch(element))
                    {
                        length++;
                    }
                    else
                    {
                        length += 0.5m;
                    }
                }
            }

            return length;
        }
    }
}
