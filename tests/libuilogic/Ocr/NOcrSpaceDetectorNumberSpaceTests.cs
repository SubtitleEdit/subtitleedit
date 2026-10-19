using Nikse.SubtitleEdit.UiLogic.Ocr;

namespace LibUiLogicTests.Ocr;

/// <summary>
/// The narrow "1" leaves most of its advance width empty, so the gap next to it beats the
/// word-space threshold while the rest of the number stays glued: "1 5", "1 940", "15 1",
/// "1 :18". <see cref="NOcrSpaceDetector.RemoveFalseSpacesInNumbers"/> drops those spaces after
/// matching, when the glyphs are known (shared by the OCR window, batch convert and seconv).
/// </summary>
public class NOcrSpaceDetectorNumberSpaceTests
{
    private static NOcrChar Glyph(string text, bool italic = false)
    {
        return new NOcrChar(text) { Italic = italic, ImageSplitterItem = new ImageSplitterItem2(0, 0, new NikseBitmap2(10, 30)) };
    }

    private static NOcrChar Space(int pixels)
    {
        return new NOcrChar(" ") { ImageSplitterItem = new ImageSplitterItem2(" ") { SpacePixels = pixels } };
    }

    private static string Fix(int pixelsAreSpace, params NOcrChar[] matches)
    {
        return string.Concat(NOcrSpaceDetector.RemoveFalseSpacesInNumbers(matches.ToList(), pixelsAreSpace).Select(p => p.Text));
    }

    [Fact]
    public void OneBeforeDigit_GapUnderTwiceTheThreshold_IsNoSpace()
    {
        // "11" split 16 px apart on a line whose threshold is 12 (word spaces 20-27 px).
        Assert.Equal("11", Fix(12, Glyph("1"), Space(16), Glyph("1")));
        Assert.Equal("1940", Fix(10, Glyph("1"), Space(13), Glyph("9"), Glyph("4"), Glyph("0")));
    }

    [Fact]
    public void DigitBeforeOne_IsTreatedTheSame()
    {
        // "151": the left bearing of the last "1" opens the gap after the "5".
        Assert.Equal("151", Fix(10, Glyph("1"), Glyph("5"), Space(15), Glyph("1")));
    }

    [Fact]
    public void OtherDigitPairs_KeepTheirSpace()
    {
        // No "1" on either side: a 15 px gap after "4" is a word space ("4 5").
        Assert.Equal("4 5", Fix(10, Glyph("4"), Space(15), Glyph("5")));
    }

    [Fact]
    public void OneBeforePunctuation_GapUnderTwiceTheThreshold_IsNoSpace()
    {
        Assert.Equal("1.21", Fix(11, Glyph("1"), Space(15), Glyph("."), Glyph("2"), Glyph("1")));
        Assert.Equal("1:18", Fix(12, Glyph("1"), Space(15), Glyph(":"), Glyph("1"), Glyph("8")));
        Assert.Equal("groep 11.", Fix(11, Glyph("g"), Glyph("r"), Glyph("o"), Glyph("e"), Glyph("p"), Space(20), Glyph("1"), Space(16), Glyph("1"), Space(15), Glyph(".")));
    }

    [Fact]
    public void OneBeforeLetter_OnlySlightlyOverTheThreshold_IsNoSpace()
    {
        Assert.Equal("1van", Fix(12, Glyph("1"), Space(15), Glyph("v"), Glyph("a"), Glyph("n")));
        Assert.Equal("1 van", Fix(12, Glyph("1"), Space(27), Glyph("v"), Glyph("a"), Glyph("n")));
        Assert.Equal("141 -", Fix(15, Glyph("1"), Glyph("4"), Glyph("1"), Space(28), Glyph("-")));
    }

    [Fact]
    public void ItalicDigits_OnlyDigitPairsAreJoined()
    {
        // The slant of italic digits opens the gap between "1" and "8" too.
        Assert.Equal("18", Fix(9, Glyph("1", italic: true), Space(12), Glyph("8", italic: true)));
        // Italic "1" before a word or punctuation is left alone: the slant already closes real spaces.
        Assert.Equal("1 v", Fix(12, Glyph("1", italic: true), Space(14), Glyph("v", italic: true)));
        Assert.Equal("1 .", Fix(12, Glyph("1", italic: true), Space(14), Glyph(".", italic: true)));
    }

    [Fact]
    public void SpaceWithoutSplitterItem_IsLeftAlone()
    {
        var space = new NOcrChar(" ");
        var result = NOcrSpaceDetector.RemoveFalseSpacesInNumbers(new List<NOcrChar> { Glyph("1"), space, Glyph("1") }, 12);

        Assert.Equal(3, result.Count);
    }
}
