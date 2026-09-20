#if NET8_0_OR_GREATER
using System.Buffers;
#else
using System.Collections.Generic;
#endif
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
            return Count(s, skipSpace: false, includeJapaneseFullWidth: true);
        }

        /// <summary>
        /// Shared body of the CJK calculators. Runs per grid-row repaint, per keystroke and per
        /// waveform frame, so the common case - every char its own text element - counts the
        /// chars directly instead of walking StringInfo.GetTextElementEnumerator (an enumerator
        /// plus one string per element), and set membership is a SearchValues lookup rather than
        /// a linear scan over each literal per character.
        /// </summary>
        /// <param name="s">Text to measure.</param>
        /// <param name="skipSpace">CalcCjkNoSpace: ' ' scores 0.</param>
        /// <param name="includeJapaneseFullWidth">CalcCjk also scores <see cref="JapaneseFullWidthCharacters"/> as full width (single-char elements only, as before).</param>
        internal static decimal Count(string s, bool skipSpace, bool includeJapaneseFullWidth)
        {
            decimal length = 0;

            if (TextElements.AreAllSingleChar(s, out var crLfCount))
            {
                foreach (var c in s)
                {
                    length += CharWeight(c, skipSpace, includeJapaneseFullWidth);
                }

                // "\r\n" is the one multi-char element that survives the probe. Its chars are
                // controls, so the loop above added nothing for them - but the element walk
                // below scores the pair like any other unknown multi-char element: 0.5.
                for (var i = 0; i < crLfCount; i++)
                {
                    length += 0.5m;
                }

                return length;
            }

            for (var en = StringInfo.GetTextElementEnumerator(s); en.MoveNext();)
            {
                var element = en.GetTextElement();
                if (element.Length == 1)
                {
                    length += CharWeight(element[0], skipSpace, includeJapaneseFullWidth);
                }
                else
                {
                    length += ElementWeight(element);
                }
            }

            return length;
        }

        private static decimal CharWeight(char ch, bool skipSpace, bool includeJapaneseFullWidth)
        {
            const char zeroWidthSpace = '\u200B';
            const char zeroWidthNoBreakSpace = '\uFEFF';
            if (char.IsControl(ch) ||
                skipSpace && ch == ' ' ||
                ch == zeroWidthSpace ||
                ch == zeroWidthNoBreakSpace ||
                ch == '\u200E' ||
                ch == '\u200F' ||
                ch == '\u202A' ||
                ch == '\u202B' ||
                ch == '\u202C' ||
                ch == '\u202D' ||
                ch == '\u202E')
            {
                return 0;
            }

            if (JapaneseHalfWidthSet.Contains(ch))
            {
                return 0.5m;
            }

            // Pure OR, so the cheap range test goes first.
            if (IsCjk(ch) || (includeJapaneseFullWidth ? FullWidthWithJapaneseSet : FullWidthSet).Contains(ch))
            {
                return 1;
            }

            return 0.5m;
        }

        /// <summary>
        /// Multi-char text element (grapheme cluster). Kept as the substring probes it always was
        /// (<c>string.Contains(string)</c>) - this is the rare path.
        /// </summary>
        private static decimal ElementWeight(string element)
        {
            if (JapaneseHalfWidthCharacters.Contains(element))
            {
                return 0.5m;
            }

            if (ChineseFullWidthPunctuations.Contains(element) ||
                LanguageAutoDetect.Letters.Japanese.Contains(element) ||
                LanguageAutoDetect.Letters.Korean.Contains(element) ||
                CjkCharRegex.IsMatch(element))
            {
                return 1;
            }

            return 0.5m;
        }

#if NET8_0_OR_GREATER
        private static readonly SearchValues<char> JapaneseHalfWidthSet = SearchValues.Create(JapaneseHalfWidthCharacters);
        private static readonly SearchValues<char> FullWidthSet = SearchValues.Create(ChineseFullWidthPunctuations + LanguageAutoDetect.Letters.Japanese + LanguageAutoDetect.Letters.Korean);
        private static readonly SearchValues<char> FullWidthWithJapaneseSet = SearchValues.Create(ChineseFullWidthPunctuations + JapaneseFullWidthCharacters + LanguageAutoDetect.Letters.Japanese + LanguageAutoDetect.Letters.Korean);
#else
        // netstandard2.1 has no SearchValues; a HashSet still beats the linear scan per character.
        private static readonly HashSet<char> JapaneseHalfWidthSet = new HashSet<char>(JapaneseHalfWidthCharacters);
        private static readonly HashSet<char> FullWidthSet = new HashSet<char>(ChineseFullWidthPunctuations + LanguageAutoDetect.Letters.Japanese + LanguageAutoDetect.Letters.Korean);
        private static readonly HashSet<char> FullWidthWithJapaneseSet = new HashSet<char>(ChineseFullWidthPunctuations + JapaneseFullWidthCharacters + LanguageAutoDetect.Letters.Japanese + LanguageAutoDetect.Letters.Korean);
#endif

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
