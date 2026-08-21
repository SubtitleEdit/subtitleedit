using Nikse.SubtitleEdit.UiLogic.Ocr;
using SkiaSharp;

namespace LibUiLogicTests.Ocr;

/// <summary>
/// Regression tests for the scaled expanded match (the pass that lets a multi-part expanded
/// character match at a different size than it was trained at). Its error budget must scale
/// with the number of line points walked, not with the target group's area: an area-scaled
/// budget grows with the group being claimed, so a trained expanded letter pair (e.g. "fi")
/// could swallow two ordinary separately-split letters ("t"+"i", "l"+"o") at ~10% wrong
/// points and hand them back merged as one glyph image.
/// </summary>
public class NOcrDbExpandedMatchTests
{
    private static NikseBitmap2 MakeSolidBitmap(int width, int height)
    {
        var bmp = new SKBitmap(width, height);
        using (var canvas = new SKCanvas(bmp))
        {
            canvas.Clear(SKColors.Transparent);
            using var paint = new SKPaint { Color = SKColors.White };
            canvas.DrawRect(0, 0, width, height, paint);
        }

        return new NikseBitmap2(bmp);
    }

    private static NOcrChar MakeTwoBarPair()
    {
        // An expanded two-part character trained at 28x38: two vertical bars with a verified
        // background gap between them (think "ll" or a large quote pair).
        var pair = new NOcrChar("fi") { Width = 28, Height = 38, MarginTop = 0, ExpandCount = 2 };
        pair.LinesForeground.Add(new NOcrLine(new OcrPoint(3, 1), new OcrPoint(3, 36)));
        pair.LinesForeground.Add(new NOcrLine(new OcrPoint(22, 1), new OcrPoint(22, 36)));
        pair.LinesBackground.Add(new NOcrLine(new OcrPoint(13, 0), new OcrPoint(13, 37)));
        return pair;
    }

    [Fact]
    public void ScaledExpandedMatch_DoesNotClaimGroupWithWrongMiddle()
    {
        // Target group at 32x40 (inside the ±25/±20 size gates and within 15 aspect points of
        // the trained pair): a narrow bar plus a wide solid blob. The blob covers the trained
        // pair's background gap, so a full third of the walked points are wrong - yet the old
        // area-scaled budget (32*40/16 = 80 errors) accepted it and merged the two separate
        // letters into one glyph.
        var parentBmp = new SKBitmap(40, 45);
        using (var canvas = new SKCanvas(parentBmp))
        {
            canvas.Clear(SKColors.Transparent);
            using var paint = new SKPaint { Color = SKColors.White };
            canvas.DrawRect(2, 0, 6, 40, paint);   // "l"-like bar at x=2..7
            canvas.DrawRect(14, 0, 20, 40, paint); // "o"-like blob at x=14..33
        }

        var parent = new NikseBitmap2(parentBmp);
        var bar = new ImageSplitterItem2(2, 0, MakeSolidBitmap(6, 40)) { Top = 0 };
        var blob = new ImageSplitterItem2(14, 0, MakeSolidBitmap(20, 40)) { Top = 0 };
        var letters = new List<ImageSplitterItem2> { bar, blob };

        var db = new NOcrDb(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".nocr"));
        db.Add(MakeTwoBarPair());

        var match = db.GetMatchExpanded(parent, bar, 0, letters);

        Assert.Null(match);
    }

    [Fact]
    public void ScaledExpandedMatch_StillMatchesSameShapeAtOtherSize()
    {
        // The counterpart guard: the same trained pair against a true two-bar group at 30x40
        // (bars where the pair has bars, empty gap where it has background) must still match -
        // that cross-size case is what the scaled pass exists for.
        var parentBmp = new SKBitmap(40, 45);
        using (var canvas = new SKCanvas(parentBmp))
        {
            canvas.Clear(SKColors.Transparent);
            using var paint = new SKPaint { Color = SKColors.White };
            canvas.DrawRect(2, 0, 6, 40, paint);  // bar 1 at x=2..7
            canvas.DrawRect(26, 0, 6, 40, paint); // bar 2 at x=26..31
        }

        var parent = new NikseBitmap2(parentBmp);
        var bar1 = new ImageSplitterItem2(2, 0, MakeSolidBitmap(6, 40)) { Top = 0 };
        var bar2 = new ImageSplitterItem2(26, 0, MakeSolidBitmap(6, 40)) { Top = 0 };
        var letters = new List<ImageSplitterItem2> { bar1, bar2 };

        var db = new NOcrDb(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".nocr"));
        db.Add(MakeTwoBarPair());

        var match = db.GetMatchExpanded(parent, bar1, 0, letters);

        Assert.NotNull(match);
        Assert.Equal("fi", match!.Text);
        Assert.Equal(2, match.ExpandCount);
    }

    [Fact]
    public void ScaledExpandedMatch_RejectsExtremeScaleRatio()
    {
        // A small two-part glyph (10x12, like a trained double quote) must never be "scaled"
        // onto a letter-pair group more than 2.5x its size, even if the walked lines happen to
        // land on text: the absolute ±25/±20 size gates alone would allow it.
        var quote = new NOcrChar("\"") { Width = 10, Height = 12, MarginTop = 0, ExpandCount = 2 };
        quote.LinesForeground.Add(new NOcrLine(new OcrPoint(2, 1), new OcrPoint(2, 10)));
        quote.LinesForeground.Add(new NOcrLine(new OcrPoint(7, 1), new OcrPoint(7, 10)));

        var parentBmp = new SKBitmap(45, 40);
        using (var canvas = new SKCanvas(parentBmp))
        {
            canvas.Clear(SKColors.Transparent);
            using var paint = new SKPaint { Color = SKColors.White };
            canvas.DrawRect(2, 0, 6, 31, paint);
            canvas.DrawRect(20, 0, 8, 31, paint);
        }

        // Group bounding box: 26x31 - 2.6x the trained width, inside all absolute gates
        // (|26-10| < 25, |31-12| < 20, aspect 119 vs 120), and the quote's two scaled bars
        // both land fully on the parts, so only the scale-ratio gate can reject it.
        var parent = new NikseBitmap2(parentBmp);
        var part1 = new ImageSplitterItem2(2, 0, MakeSolidBitmap(6, 31)) { Top = 0 };
        var part2 = new ImageSplitterItem2(20, 0, MakeSolidBitmap(8, 31)) { Top = 0 };
        var letters = new List<ImageSplitterItem2> { part1, part2 };

        var db = new NOcrDb(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".nocr"));
        db.Add(quote);

        var match = db.GetMatchExpanded(parent, part1, 0, letters);

        Assert.Null(match);
    }
}
