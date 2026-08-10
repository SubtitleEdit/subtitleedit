using Nikse.SubtitleEdit.UiLogic.Ocr;
using SkiaSharp;

namespace LibUiLogicTests.Ocr;

/// <summary>
/// Pins the small-glyph protections in <see cref="NOcrDb"/>: solid tiny glyphs (dot, dash,
/// apostrophe) can't be pixel-discriminated, so the matcher must gate them by aspect ratio,
/// and expanded characters must match scaled to the target group's actual size.
/// </summary>
public class NOcrDbSensitiveTests
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

    [Fact]
    public void SmallGlyph_AspectRatioGate_BlocksSolidBlobSteal()
    {
        // A solid 12x4 dash. A dot-like 6x6 DB entry with only foreground lines scores ZERO
        // pixel errors on it (all its lines land on the solid bar), and the old 8px
        // size-tolerance pass had no aspect gate at all - so "." claimed "-". The aspect-ratio
        // gate for small glyphs (area < 150) must reject it: 100% vs 33% aspect is a 3x ratio.
        var dash = MakeSolidBitmap(12, 4);

        var dot = new NOcrChar(".") { Width = 6, Height = 6, MarginTop = 20 };
        dot.LinesForeground.Add(new NOcrLine(new OcrPoint(1, 1), new OcrPoint(4, 1)));
        dot.LinesForeground.Add(new NOcrLine(new OcrPoint(1, 3), new OcrPoint(4, 3)));
        dot.LinesForeground.Add(new NOcrLine(new OcrPoint(2, 1), new OcrPoint(2, 4)));

        var db = new NOcrDb(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".nocr"));
        db.Add(dot);

        // Sanity: pixel evidence alone can't tell them apart.
        Assert.True(NOcrDb.IsMatch(dash, dot, 0));

        var match = db.GetMatchSingle(dash, 20, true, 25, lastDitch: true);

        Assert.Null(match);
    }

    [Fact]
    public void SmallGlyph_SameAspect_StillMatches()
    {
        // Same setup, but the DB entry has a dash-like aspect - it must still match.
        var dash = MakeSolidBitmap(12, 4);

        var trainedDash = new NOcrChar("-") { Width = 10, Height = 4, MarginTop = 20 };
        trainedDash.LinesForeground.Add(new NOcrLine(new OcrPoint(1, 1), new OcrPoint(8, 1)));
        trainedDash.LinesForeground.Add(new NOcrLine(new OcrPoint(1, 2), new OcrPoint(8, 2)));

        var db = new NOcrDb(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".nocr"));
        db.Add(trainedDash);

        var match = db.GetMatchSingle(dash, 20, true, 25, lastDitch: true);

        Assert.NotNull(match);
        Assert.Equal("-", match!.Text);
    }

    [Fact]
    public void ExpandedMatch_ScalesToTargetGroupSize()
    {
        // A two-part expanded character (like a quote: two bars) trained at 12x10, recognized
        // against a group whose actual bounding box is 10x8. The old "scaled" pass walked the
        // lines at the character's own size, so any size difference failed and the single-part
        // fallback turned every cross-size " into '. The scaled pass must map lines onto the
        // group's real size.
        var parentBmp = new SKBitmap(30, 20);
        using (var canvas = new SKCanvas(parentBmp))
        {
            canvas.Clear(SKColors.Transparent);
            using var paint = new SKPaint { Color = SKColors.White };
            canvas.DrawRect(4, 0, 2, 8, paint);  // bar 1 at x=4..5
            canvas.DrawRect(12, 0, 2, 8, paint); // bar 2 at x=12..13
        }

        var parent = new NikseBitmap2(parentBmp);
        var bar1 = new ImageSplitterItem2(4, 0, MakeSolidBitmap(2, 8)) { Top = 0 };
        var bar2 = new ImageSplitterItem2(12, 0, MakeSolidBitmap(2, 8)) { Top = 0 };
        var letters = new List<ImageSplitterItem2> { bar1, bar2 };

        var quote = new NOcrChar("\"") { Width = 12, Height = 10, MarginTop = 0, ExpandCount = 2 };
        quote.LinesForeground.Add(new NOcrLine(new OcrPoint(1, 1), new OcrPoint(1, 8)));
        quote.LinesForeground.Add(new NOcrLine(new OcrPoint(9, 1), new OcrPoint(9, 8)));
        quote.LinesBackground.Add(new NOcrLine(new OcrPoint(5, 0), new OcrPoint(5, 9)));

        var db = new NOcrDb(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".nocr"));
        db.Add(quote);

        var match = db.GetMatchExpanded(parent, bar1, 0, letters);

        Assert.NotNull(match);
        Assert.Equal("\"", match!.Text);
        Assert.Equal(2, match.ExpandCount);
    }
}
