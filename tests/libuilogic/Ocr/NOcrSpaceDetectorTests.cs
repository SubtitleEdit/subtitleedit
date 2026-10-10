using Nikse.SubtitleEdit.UiLogic.Ocr;

namespace LibUiLogicTests.Ocr;

/// <summary>
/// Word spaces are decided relative to the line height and per image, because a fixed "pixels
/// are space" value dropped every space of DVD-sized text (7-9 px gaps under the default 12) and
/// split widely tracked all-caps text into single letters (12 px letter gaps).
/// </summary>
public class NOcrSpaceDetectorTests
{
    // One text line: a glyph of the given height, then for each gap a space item and a glyph.
    private static List<ImageSplitterItem2> Line(int height, params int[] gaps)
    {
        var list = new List<ImageSplitterItem2> { Glyph(height) };
        foreach (var gap in gaps)
        {
            list.Add(new ImageSplitterItem2(" ") { SpacePixels = gap });
            list.Add(Glyph(height));
        }

        return list;
    }

    private static ImageSplitterItem2 Glyph(int height)
    {
        return new ImageSplitterItem2(0, 0, new NikseBitmap2(10, height)) { Top = 0 };
    }

    private static List<int> SpacesKept(List<ImageSplitterItem2> letters)
    {
        return letters.Where(p => p.SpecialCharacter == " ").Select(p => p.SpacePixels).ToList();
    }

    [Fact]
    public void DvdSizedText_KeepsSmallWordSpaces()
    {
        var detector = new NOcrSpaceDetector();

        // 28 px line: letter gaps 2-3 px, word spaces 8-9 px (all dropped by the old fixed 12).
        var result = detector.RemoveFalseSpaces(Line(28, 2, 3, 8, 2, 3, 9, 2), out _);

        Assert.Equal(new List<int> { 8, 9 }, SpacesKept(result));
    }

    [Fact]
    public void TrackedAllCaps_LetterGapsAreNotSpaces()
    {
        var detector = new NOcrSpaceDetector();

        // 40 px all-caps line with wide tracking: 10-13 px letter gaps, 22-24 px word spaces.
        var result = detector.RemoveFalseSpaces(Line(40, 12, 11, 13, 22, 10, 12, 24, 11), out var pixelsAreSpace);

        Assert.Equal(new List<int> { 22, 24 }, SpacesKept(result));
        Assert.InRange(pixelsAreSpace, 14, 22);
    }

    [Fact]
    public void OneWordLine_UsesThresholdOfEarlierImages()
    {
        var detector = new NOcrSpaceDetector();
        detector.RemoveFalseSpaces(Line(40, 12, 11, 13, 22, 10, 12, 24, 11), out _);

        // No word space to calibrate on - the 13 px letter gaps must not turn into spaces,
        // which the default relative threshold (0.22 x 40 = 9 px) would do.
        var result = detector.RemoveFalseSpaces(Line(40, 12, 13, 11), out _);

        Assert.Empty(SpacesKept(result));
    }

    [Fact]
    public void NoHistory_FallsBackToDefaultRelativeThreshold()
    {
        var detector = new NOcrSpaceDetector();

        // 50 px line, 0.22 x 50 = 11 px.
        var result = detector.RemoveFalseSpaces(Line(50, 4, 4), out var pixelsAreSpace);

        Assert.Empty(SpacesKept(result));
        Assert.Equal(11, pixelsAreSpace);
    }

    [Fact]
    public void LineBreaks_AreKept_AndEachLineUsesItsOwnHeight()
    {
        var detector = new NOcrSpaceDetector();
        var letters = Line(30, 2, 9, 2);
        letters.Add(new ImageSplitterItem2(Environment.NewLine));
        letters.AddRange(Line(60, 4, 18, 4));

        var result = detector.RemoveFalseSpaces(letters, out _);

        Assert.Single(result, p => p.SpecialCharacter == Environment.NewLine);
        Assert.Equal(new List<int> { 9, 18 }, SpacesKept(result));
    }
}
