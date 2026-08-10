using System.Text.RegularExpressions;
using Nikse.SubtitleEdit.Core.Common.TextLengthCalculator;

namespace LibSETests.Common;

public class CalcCjkTest
{
    // IsCjk used to allocate a one-character string and run CjkCharRegex over it, once per
    // character of every measured line. It now tests the block ranges directly, so pin the
    // rewrite to the regex it replaced across the whole BMP - a single mistyped range would
    // otherwise silently shift CJK character counts.
    [Fact]
    public void IsCjk_MatchesRegexForEveryChar()
    {
        var regex = new Regex(@"\p{IsHangulJamo}|" +
                              @"\p{IsCJKRadicalsSupplement}|" +
                              @"\p{IsCJKSymbolsandPunctuation}|" +
                              @"\p{IsEnclosedCJKLettersandMonths}|" +
                              @"\p{IsCJKCompatibility}|" +
                              @"\p{IsCJKUnifiedIdeographsExtensionA}|" +
                              @"\p{IsCJKUnifiedIdeographs}|" +
                              @"\p{IsHangulSyllables}|" +
                              @"\p{IsCJKCompatibilityForms}");

        for (var i = 0; i <= char.MaxValue; i++)
        {
            var c = (char)i;

            // Hiragana was a hard-coded fast path in the old implementation, on top of the regex.
            var expected = regex.IsMatch(c.ToString()) || (i >= 0x3040 && i <= 0x309F);

            Assert.True(expected == CalcCjk.IsCjk(c), $"U+{i:X4} expected {expected}");
        }
    }
}
