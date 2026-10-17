using Nikse.SubtitleEdit.UiLogic.Ocr;

namespace LibUiLogicTests.Ocr;

/// <summary>
/// Accented letters take their case from the height above the baseline: an accent on a capital
/// stands well above the cap height, one on a lowercase letter about reaches it.
/// </summary>
public class NOcrAccentCaseTests
{
    private const int Baseline = 60;

    private static ImageSplitterItem2 Glyph(int height, int top) =>
        new(0, top, new NikseBitmap2(20, height)) { Top = top, Baseline = Baseline };

    /// <summary>x-height 20, cap height 30 (Latin reference letters), baseline at 60.</summary>
    private static NOcrCaseFixer WarmedUpFixer()
    {
        var fixer = new NOcrCaseFixer();
        for (var i = 0; i < 3; i++)
        {
            fixer.FixUppercaseLowercaseIssues(Glyph(20, Baseline - 20), new NOcrChar("e"));
            fixer.FixUppercaseLowercaseIssues(Glyph(20, Baseline - 20), new NOcrChar("a"));
            fixer.FixUppercaseLowercaseIssues(Glyph(30, Baseline - 30), new NOcrChar("E"));
            fixer.FixUppercaseLowercaseIssues(Glyph(30, Baseline - 30), new NOcrChar("H"));
        }

        return fixer;
    }

    [Theory]
    [InlineData("Ì", 31, "ì")] // Italian "così": the grave on "i" reaches about the cap height
    [InlineData("Ì", 40, "Ì")] // on "I" it stands well above it
    [InlineData("ù", 40, "Ù")]
    [InlineData("Ò", 32, "ò")]
    public void AccentAbove_CaseFromHeightAboveBaseline(string matched, int height, string expected)
    {
        Assert.Equal(expected, WarmedUpFixer().FixUppercaseLowercaseIssues(Glyph(height, Baseline - height), new NOcrChar(matched)));
    }

    [Theory]
    [InlineData("Ç", 20, "ç")] // cedilla below: the body is x-high
    [InlineData("ç", 30, "Ç")] // the body is cap-high
    public void CedillaBelow_CaseFromBodyHeight(string matched, int bodyHeight, string expected)
    {
        // The cedilla hangs 6 px below the baseline.
        var glyph = new ImageSplitterItem2(0, Baseline - bodyHeight, new NikseBitmap2(20, bodyHeight + 6)) { Top = Baseline - bodyHeight, Baseline = Baseline };

        Assert.Equal(expected, WarmedUpFixer().FixUppercaseLowercaseIssues(glyph, new NOcrChar(matched)));
    }
}
