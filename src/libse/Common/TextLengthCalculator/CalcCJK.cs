using System.Globalization;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.Common.TextLengthCalculator
{
    public class CalcCjk : ICalcLength
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
                        if (JapaneseHalfWidthCharacters.Contains(ch))
                        {
                            length += 0.5m;
                        }
                        else if (ChineseFullWidthPunctuations.Contains(ch) ||
                                 JapaneseFullWidthCharacters.Contains(ch) ||
                                 LanguageAutoDetect.Letters.Japanese.Contains(ch) ||
                                 LanguageAutoDetect.Letters.Korean.Contains(ch) ||
                                 IsCjk(ch))
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
                    if (JapaneseHalfWidthCharacters.Contains(element))
                    {
                        length += 0.5m;
                    }
                    else if (ChineseFullWidthPunctuations.Contains(element) ||
                             LanguageAutoDetect.Letters.Japanese.Contains(element) ||
                             LanguageAutoDetect.Letters.Korean.Contains(element) ||
                             CjkCharRegex.IsMatch(element))
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

        public const string JapaneseHalfWidthCharacters = "｡｢｣､･ｦｧｨｩｪｫｬｭｮｯｰｱｲｳｴｵｶｷｸｹｺｻｼｽｾｿﾀﾁﾂﾃﾄﾅﾆﾇﾈﾉﾊﾋﾌﾍﾎﾏﾐﾑﾒﾓﾔﾕﾖﾗﾘﾙﾚﾛﾜﾝﾞﾟ";
        public const string JapaneseFullWidthCharacters = "ぁあぃいぅうぇえぉおァアィイゥウェエォオㇰㇱㇲㇳㇴㇵㇶㇷㇸㇹ一二三四五六七八九十学校日本、。・「」々〆〇";
        public const string ChineseFullWidthPunctuations = "，。、：；？！…“”—‘’（）【】「」『』〔〕《》〈〉";

        public static readonly Regex CjkCharRegex = new Regex(@"\p{IsHangulJamo}|" +
                                                              @"\p{IsCJKRadicalsSupplement}|" +
                                                              @"\p{IsCJKSymbolsandPunctuation}|" +
                                                              @"\p{IsEnclosedCJKLettersandMonths}|" +
                                                              @"\p{IsCJKCompatibility}|" +
                                                              @"\p{IsCJKUnifiedIdeographsExtensionA}|" +
                                                              @"\p{IsCJKUnifiedIdeographs}|" +
                                                              @"\p{IsHangulSyllables}|" +
                                                              @"\p{IsCJKCompatibilityForms}", RegexOptions.Compiled);
        /// <summary>
        /// True for the Unicode blocks <see cref="CjkCharRegex"/> matches, plus Hiragana.
        /// This runs once per character of every line the CJK length calculators measure (the
        /// subtitle grid re-reads those on each repaint), so it tests the block ranges directly
        /// instead of allocating a one-character string and running the regex over it.
        /// CalcCjkTest.IsCjk_MatchesRegexForEveryChar pins it to the regex for all 65536 chars.
        /// </summary>
        public static bool IsCjk(char c)
        {
            var v = (int)c;
            return v >= 0x1100 && v <= 0x11FF ||   // Hangul Jamo
                   v >= 0x2E80 && v <= 0x2EFF ||   // CJK Radicals Supplement
                   v >= 0x3000 && v <= 0x303F ||   // CJK Symbols and Punctuation
                   v >= 0x3040 && v <= 0x309F ||   // Hiragana
                   v >= 0x3200 && v <= 0x32FF ||   // Enclosed CJK Letters and Months
                   v >= 0x3300 && v <= 0x33FF ||   // CJK Compatibility
                   v >= 0x3400 && v <= 0x4DBF ||   // CJK Unified Ideographs Extension A
                   v >= 0x4E00 && v <= 0x9FFF ||   // CJK Unified Ideographs
                   v >= 0xAC00 && v <= 0xD7AF ||   // Hangul Syllables
                   v >= 0xFE30 && v <= 0xFE4F;     // CJK Compatibility Forms
        }
    }
}
