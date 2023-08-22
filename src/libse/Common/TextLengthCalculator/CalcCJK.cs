﻿using System.Text.RegularExpressions;

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
                        count += 0.5m;
                    }
                    else if (ChineseFullWidthPunctuations.Contains(ch) ||
                             LanguageAutoDetect.Letters.Japanese.Contains(ch) ||
                             LanguageAutoDetect.Letters.Korean.Contains(ch) ||
                             IsCjk(ch))
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


        public const string JapaneseHalfWidthCharacters = "｡｢｣､･ｦｧｨｩｪｫｬｭｮｯｰｱｲｳｴｵｶｷｸｹｺｻｼｽｾｿﾀﾁﾂﾃﾄﾅﾆﾇﾈﾉﾊﾋﾌﾍﾎﾏﾐﾑﾒﾓﾔﾕﾖﾗﾘﾙﾚﾛﾜﾝﾞﾟ";
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
        public static bool IsCjk(char c)
        {
            var v = (int)c;
            if (v >= 0x3040 && v <= 0x309F) // Hiragana
            {
                return true;
            }

            if (v >= 0x4E00 && v <= 0x9FAF) // Common and uncommon kanji
            {
                return true;
            }

            return CjkCharRegex.IsMatch(c.ToString());
        }
    }
}
