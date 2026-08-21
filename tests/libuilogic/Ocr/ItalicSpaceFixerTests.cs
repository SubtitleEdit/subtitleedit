using Nikse.SubtitleEdit.UiLogic.Ocr;

namespace LibUiLogicTests.Ocr;

/// <summary>
/// Italic glyphs lean over the word gaps, so the letter splitter's straight-column space
/// detection undercounts them. ItalicSpaceFixer re-measures the gaps along the italic slant
/// and inserts the spaces that were missed (issue #13660: "hat mir gesagt" OCR'ed as
/// "hatmirgesagt"). These tests pin the insertion rules for both the binary image compare
/// matches and the nOCR matches.
/// </summary>
public class ItalicSpaceFixerTests
{
    private const double UnItalicFactor = 0.33;

    private static NikseBitmap2 MakeBitmap(int width, int height, bool opaque)
    {
        var data = new byte[width * height * 4];
        if (opaque)
        {
            for (var i = 0; i < data.Length; i++)
            {
                data[i] = 255;
            }
        }

        return new NikseBitmap2(width, height, data);
    }

    private static ImageSplitterItem2 MakeLetter(int x, int width) =>
        new ImageSplitterItem2(x, 0, MakeBitmap(width, 20, opaque: true));

    private static BinaryOcrMatcher.CompareMatch MakeMatch(string text, bool italic, ImageSplitterItem2 letter) =>
        new BinaryOcrMatcher.CompareMatch(text, italic, 0, null) { ImageSplitterItem = letter };

    [Fact]
    public void BinaryMatches_WideTransparentGapInItalic_InsertsSpace()
    {
        var parentBitmap = MakeBitmap(60, 30, opaque: false); // fully transparent => whole gap is blank
        var letterA = MakeLetter(0, 10);
        var letterB = MakeLetter(30, 10); // gap of 20 >= pixelsIsSpace
        var letters = new List<ImageSplitterItem2> { letterA, letterB };
        var matches = new List<BinaryOcrMatcher.CompareMatch>
        {
            MakeMatch("a", true, letterA),
            MakeMatch("b", true, letterB),
        };

        var text = ItalicSpaceFixer.GetTextWithMoreSpacesInItalic(matches, letters, parentBitmap, UnItalicFactor, 12);

        Assert.Equal("<i>a b</i>", text);
    }

    [Fact]
    public void BinaryMatches_GapFilledWithPixels_NoSpaceInserted()
    {
        var parentBitmap = MakeBitmap(60, 30, opaque: true); // every angled line hits a pixel
        var letterA = MakeLetter(0, 10);
        var letterB = MakeLetter(30, 10);
        var letters = new List<ImageSplitterItem2> { letterA, letterB };
        var matches = new List<BinaryOcrMatcher.CompareMatch>
        {
            MakeMatch("a", true, letterA),
            MakeMatch("b", true, letterB),
        };

        var text = ItalicSpaceFixer.GetTextWithMoreSpacesInItalic(matches, letters, parentBitmap, UnItalicFactor, 12);

        Assert.Equal("<i>ab</i>", text);
    }

    [Fact]
    public void BinaryMatches_GapNarrowerThanPixelsIsSpace_NoSpaceInserted()
    {
        var parentBitmap = MakeBitmap(60, 30, opaque: false);
        var letterA = MakeLetter(0, 10);
        var letterB = MakeLetter(14, 10); // blank range 10..19 => 9 < pixelsIsSpace
        var letters = new List<ImageSplitterItem2> { letterA, letterB };
        var matches = new List<BinaryOcrMatcher.CompareMatch>
        {
            MakeMatch("a", true, letterA),
            MakeMatch("b", true, letterB),
        };

        var text = ItalicSpaceFixer.GetTextWithMoreSpacesInItalic(matches, letters, parentBitmap, UnItalicFactor, 12);

        Assert.Equal("<i>ab</i>", text);
    }

    [Fact]
    public void BinaryMatches_NonItalic_NoSpaceInserted()
    {
        // The straight-column detection already handles upright text; the fixer must only
        // touch italic matches.
        var parentBitmap = MakeBitmap(60, 30, opaque: false);
        var letterA = MakeLetter(0, 10);
        var letterB = MakeLetter(30, 10);
        var letters = new List<ImageSplitterItem2> { letterA, letterB };
        var matches = new List<BinaryOcrMatcher.CompareMatch>
        {
            MakeMatch("a", false, letterA),
            MakeMatch("b", false, letterB),
        };

        var text = ItalicSpaceFixer.GetTextWithMoreSpacesInItalic(matches, letters, parentBitmap, UnItalicFactor, 12);

        Assert.Equal("ab", text);
    }

    [Fact]
    public void BinaryMatches_ExistingSpaceMatch_NotDoubled()
    {
        var parentBitmap = MakeBitmap(90, 30, opaque: false);
        var letterA = MakeLetter(0, 10);
        var letterB = MakeLetter(30, 10);
        var letters = new List<ImageSplitterItem2> { letterA, letterB };
        var matches = new List<BinaryOcrMatcher.CompareMatch>
        {
            MakeMatch("a", true, letterA),
            new BinaryOcrMatcher.CompareMatch(" ", false, 0, null),
            MakeMatch("b", true, letterB),
        };

        var text = ItalicSpaceFixer.GetTextWithMoreSpacesInItalic(matches, letters, parentBitmap, UnItalicFactor, 12);

        Assert.Equal("<i>a b</i>", text);
    }

    [Fact]
    public void NOcrMatches_WideTransparentGapInItalic_InsertsSpace()
    {
        var parentBitmap = MakeBitmap(60, 30, opaque: false);
        var letterA = MakeLetter(0, 10);
        var letterB = MakeLetter(30, 10);
        var letters = new List<ImageSplitterItem2> { letterA, letterB };
        var matches = new List<NOcrChar>
        {
            new NOcrChar("a") { Italic = true, ImageSplitterItem = letterA },
            new NOcrChar("b") { Italic = true, ImageSplitterItem = letterB },
        };

        var text = ItalicSpaceFixer.GetTextWithMoreSpacesInItalic(matches, letters, parentBitmap, UnItalicFactor, 12);

        Assert.Equal("<i>a b</i>", text);
    }
}
