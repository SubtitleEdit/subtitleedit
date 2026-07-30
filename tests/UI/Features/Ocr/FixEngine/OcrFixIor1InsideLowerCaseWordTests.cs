using Nikse.SubtitleEdit.UiLogic.Ocr.FixEngine;

namespace UITests.Features.Ocr.FixEngine;

// Regression tests for https://github.com/SubtitleEdit/subtitleedit/issues/12996
// A 2014 refactor inverted the Mc/Mac-name guard in FixIor1InsideLowerCaseWord, so the
// hardcoded "uppercase I/1 after a lowercase letter becomes l" rule never fired. The old
// WinForms engine had a second, correct line-level copy that masked this; the SE 5 OCR
// pipeline only has this word-level copy, so llama/Tesseract output like "heleboeI"
// reached users unfixed.
public class OcrFixIor1InsideLowerCaseWordTests
{
    [Theory]
    [InlineData("heleboeI", "heleboel")]
    [InlineData("boeI", "boel")]
    [InlineData("teIt", "telt")]
    [InlineData("sequeI", "sequel")]
    [InlineData("geveI", "gevel")]
    [InlineData("he1p", "help")]
    public void UppercaseIOr1AfterLowercaseLetter_IsFixedToLowercaseL(string input, string expected)
    {
        Assert.Equal(expected, OcrFixReplaceList2.FixIor1InsideLowerCaseWord(input));
    }

    [Theory]
    [InlineData("McIntyre")]
    [InlineData("MacIntosh")]
    public void McAndMacNames_KeepTheirCapitalI(string input)
    {
        Assert.Equal(input, OcrFixReplaceList2.FixIor1InsideLowerCaseWord(input));
    }

    [Theory]
    [InlineData("MI6")] // starts-and-ends-with-number style guards
    [InlineData("2Ic")]
    [InlineData("a1b2")]
    public void WordsWithOtherDigits_AreLeftAlone(string input)
    {
        Assert.Equal(input, OcrFixReplaceList2.FixIor1InsideLowerCaseWord(input));
    }
}
