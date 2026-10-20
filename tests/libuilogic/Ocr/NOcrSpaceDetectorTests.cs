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

    private static NikseBitmap2 Ink(int width, int height, params (int X, int Y, int W, int H)[] rects)
    {
        var bitmap = new NikseBitmap2(width, height);
        foreach (var (x0, y0, w, h) in rects)
        {
            for (var y = y0; y < y0 + h; y++)
            {
                for (var x = x0; x < x0 + w; x++)
                {
                    bitmap.SetPixel(x, y, SkiaSharp.SKColors.White);
                }
            }
        }

        return bitmap;
    }

    /// <summary>
    /// "of you": the hook of "f" and the tail of "y" reach into the space, so their boxes are only
    /// 4 px apart on a line whose word spaces are 10 px - the bodies are 16 px apart.
    /// </summary>
    private static List<ImageSplitterItem2> OverhangLine(NikseBitmap2 right, int rightTop)
    {
        // Calibration: letter gaps 2 px and word spaces 10 px on a 30 px line (threshold ~6 px).
        var list = Line(30, 2, 10, 2, 10, 2);
        var f = Ink(16, 30, (0, 0, 4, 30), (4, 0, 12, 4)); // stem + hook over the gap
        list.Add(new ImageSplitterItem2(" ") { SpacePixels = 10 });
        list.Add(new ImageSplitterItem2(100, 0, f) { Top = 0 });
        list.Add(new ImageSplitterItem2(" ") { SpacePixels = 4 });
        list.Add(new ImageSplitterItem2(100 + 16 + 4, rightTop, right) { Top = rightTop });
        return list;
    }

    [Fact]
    public void OverhangingLetters_GapBetweenBodiesCounts()
    {
        var y = Ink(12, 20, (4, 0, 8, 16), (0, 16, 6, 4)); // x-high body + tail reaching left
        var result = new NOcrSpaceDetector().RemoveFalseSpaces(OverhangLine(y, 10), out _);

        Assert.Contains(4, SpacesKept(result));
    }

    /// <summary>
    /// Calibration line (letter gaps 2 px, word spaces 10 px) followed by the pair under test at
    /// <paramref name="boxGap"/> px. Letters 30 px high: the jump 5 -> 10 px puts the threshold
    /// at 8 px, 2 -> 10 at 6 px.
    /// </summary>
    private static List<ImageSplitterItem2> PairLine(NikseBitmap2 left, int leftTop, NikseBitmap2 right, int rightTop, int boxGap)
    {
        var list = Line(30, 2, 10, 2, 10, 2);
        list.Add(new ImageSplitterItem2(" ") { SpacePixels = 10 });
        list.Add(new ImageSplitterItem2(100, leftTop, left) { Top = leftTop });
        list.Add(new ImageSplitterItem2(" ") { SpacePixels = boxGap });
        list.Add(new ImageSplitterItem2(100 + left.Width + boxGap, rightTop, right) { Top = rightTop });
        return list;
    }

    // Stem with an arm over the gap on the top rows ("f", "r", "T"): the box ends 12 px right of the foot.
    private static NikseBitmap2 ArmedGlyph() => Ink(16, 30, (0, 0, 4, 30), (4, 0, 12, 12));

    // Stem with 6 px of empty box to its left on every row ("j" seen from above its hook).
    private static NikseBitmap2 OffsetStem() => Ink(14, 30, (6, 0, 8, 30));

    [Fact]
    public void OverhangingPair_MostOfASpaceApart_GetsHalfTheBodyGap()
    {
        // "f j" in a bold font: boxes 5 px apart (threshold 8), bodies 11 px apart. A quarter of
        // the extra (6.5) is not a space, half of it (8) is.
        var result = new NOcrSpaceDetector().RemoveFalseSpaces(PairLine(ArmedGlyph(), 0, OffsetStem(), 0, 5), out var threshold);

        Assert.Equal(8, threshold);
        Assert.Contains(5, SpacesKept(result));
    }

    [Fact]
    public void KernedPair_BoxesAlmostTouching_StaysALetterGap()
    {
        // "To": the same glyphs 2 px apart (threshold 6, bodies 8 apart) - only a quarter counts.
        var result = new NOcrSpaceDetector().RemoveFalseSpaces(PairLine(ArmedGlyph(), 0, OffsetStem(), 0, 2), out var threshold);

        Assert.Equal(6, threshold);
        Assert.DoesNotContain(2, SpacesKept(result));
    }

    [Fact]
    public void HollowNeighbourWithoutOverhang_StaysALetterGap()
    {
        // "n?": the hook of "?" is hollow where it shares rows with "n", so the bodies are 13 px
        // apart on boxes 7 px apart (threshold 9) - but neither glyph overhangs its own box.
        var n = Ink(16, 30, (0, 0, 16, 30));
        var questionMark = Ink(14, 40, (0, 0, 14, 4), (6, 4, 8, 36));
        var result = new NOcrSpaceDetector().RemoveFalseSpaces(PairLine(n, 10, questionMark, 0, 7), out var threshold);

        Assert.Equal(9, threshold);
        Assert.DoesNotContain(7, SpacesKept(result));
    }

    [Fact]
    public void Colon_IsNoLetterForTheBodyGap()
    {
        // Two dots: its "body gap" to the "f" says nothing about a space.
        var colon = Ink(6, 17, (0, 0, 6, 5), (0, 12, 6, 5));
        var result = new NOcrSpaceDetector().RemoveFalseSpaces(OverhangLine(colon, 10), out _);

        Assert.DoesNotContain(4, SpacesKept(result));
    }
}
