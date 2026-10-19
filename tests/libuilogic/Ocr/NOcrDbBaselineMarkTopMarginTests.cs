using Nikse.SubtitleEdit.UiLogic.Ocr;
using SkiaSharp;

namespace LibUiLogicTests.Ocr;

/// <summary>
/// A period's trained top margin is a line-height artefact: a "." learnt at the foot of a tall
/// line sat too far from a same-size dot at the foot of a shorter line for any pass's top-margin
/// gate, so round ellipsis dots stayed "*" although the database held their exact size. When a
/// mark sits on the baseline, <see cref="NOcrDb.GetMatch"/> lets baseline marks (".", ",", "„",
/// ...) match by that position instead of by the trained top margin. Everything else keeps the
/// gate.
/// </summary>
public class NOcrDbBaselineMarkTopMarginTests
{
    // The dot was trained at the foot of an 88 px line; the line it is matched in is 57 px.
    private const int TrainedLineHeight = 88;
    private const int LineHeight = 57;
    private const int DotSize = 8;

    private static NikseBitmap2 Solid(int width, int height)
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

    private static NOcrDb DbWith(string text, NikseBitmap2 bitmap, int marginTop)
    {
        var oc = new NOcrChar(text) { Width = bitmap.Width, Height = bitmap.Height, MarginTop = marginTop };
        NOcrChar.GenerateLineSegments(30, false, oc, bitmap);
        var db = new NOcrDb(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".nocr"));
        db.Add(oc);
        return db;
    }

    /// <summary><paramref name="letters"/> cap-high glyphs and then <paramref name="mark"/> at <paramref name="top"/>.</summary>
    private static List<ImageSplitterItem2> Line(NikseBitmap2 mark, int top, int letters = 3)
    {
        var list = new List<ImageSplitterItem2>();
        for (var i = 0; i < letters; i++)
        {
            list.Add(new ImageSplitterItem2(i * 20, 0, Solid(12, LineHeight)) { Top = 0, Baseline = LineHeight });
        }

        list.Add(new ImageSplitterItem2(letters * 20, top, mark) { Top = top, Baseline = LineHeight });
        return list;
    }

    private static string? Match(NOcrDb db, List<ImageSplitterItem2> line)
    {
        var item = line[^1];
        return db.GetMatch(new NikseBitmap2(200, 100), line, item, item.Top, true, 25, lastDitch: true)?.Text;
    }

    [Fact]
    public void PeriodTrainedInTallerLine_MatchesSameSizeDotOnBaseline()
    {
        var dot = Solid(DotSize, DotSize);
        var db = DbWith(".", dot, TrainedLineHeight - DotSize);

        Assert.Equal(".", Match(db, Line(dot, LineHeight - DotSize)));
    }

    [Fact]
    public void CommaTrainedInTallerLine_MatchesSameSizeCommaOnBaseline()
    {
        // A comma hangs a little below the baseline; the splitter's baseline is the letters' foot.
        var comma = Solid(5, 11);
        var db = DbWith(",", comma, TrainedLineHeight - 9);

        Assert.Equal(",", Match(db, Line(comma, LineHeight - 9)));
    }

    [Fact]
    public void WithoutABaseline_TopMarginGateStillApplies()
    {
        // Two letters are too few to measure a baseline by, so the dot's placement is unknown and
        // the period entry trained 31 px lower may not claim it.
        var dot = Solid(DotSize, DotSize);
        var db = DbWith(".", dot, TrainedLineHeight - DotSize);
        var line = Line(dot, LineHeight - DotSize, letters: 2);

        Assert.Equal(NOcrDb.BaselinePlacement.Unknown, NOcrDb.GetBaselinePlacement(line, line[^1]));
        Assert.Null(Match(db, line));
    }

    [Fact]
    public void DashTrainedInTallerLine_CannotClaimDotOnBaseline()
    {
        var dot = Solid(DotSize, DotSize);
        var db = DbWith("-", dot, TrainedLineHeight - DotSize);

        Assert.Null(Match(db, Line(dot, LineHeight - DotSize)));
    }

    [Fact]
    public void PeriodTrainedInTallerLine_CannotClaimBlobAboveBaseline()
    {
        var blob = Solid(DotSize, DotSize);
        var db = DbWith(".", blob, TrainedLineHeight - DotSize);

        Assert.Null(Match(db, Line(blob, 20)));
    }
}
