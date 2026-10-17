using Nikse.SubtitleEdit.UiLogic.Ocr;
using SkiaSharp;

namespace LibUiLogicTests.Ocr;

/// <summary>
/// A heavy period and a short dash are both a solid blob, so pixels can't tell them apart; the
/// line's baseline can. <see cref="NOcrDb.GetMatch"/> keeps dashes off the baseline and periods
/// above it, and reads comma/quote shapes by their height.
/// </summary>
public class NOcrDbBaselinePlacementTests
{
    private const int Baseline = 30;

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

    private static NOcrChar Entry(string text, NikseBitmap2 bitmap, int marginTop)
    {
        var oc = new NOcrChar(text) { Width = bitmap.Width, Height = bitmap.Height, MarginTop = marginTop };
        NOcrChar.GenerateLineSegments(30, false, oc, bitmap);
        return oc;
    }

    /// <summary>Database where <paramref name="first"/> and <paramref name="second"/> fit the
    /// same bitmap equally well, with <paramref name="first"/> winning ties (front of the list).</summary>
    private static NOcrDb TwoLookalikes(string first, string second, NikseBitmap2 bitmap, int marginTop)
    {
        var db = new NOcrDb(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".nocr"));
        db.Add(Entry(second, bitmap, marginTop));
        db.Add(Entry(first, bitmap, marginTop));
        return db;
    }

    /// <summary>Three cap-high glyphs and then <paramref name="mark"/> at <paramref name="top"/>.</summary>
    private static List<ImageSplitterItem2> Line(NikseBitmap2 mark, int top, int letters = 3)
    {
        var list = new List<ImageSplitterItem2>();
        for (var i = 0; i < letters; i++)
        {
            list.Add(new ImageSplitterItem2(i * 15, 0, Solid(10, Baseline)) { Top = 0, Baseline = Baseline });
        }

        list.Add(new ImageSplitterItem2(letters * 15, top, mark) { Top = top, Baseline = Baseline });
        return list;
    }

    private static string? Match(NOcrDb db, List<ImageSplitterItem2> line)
    {
        var item = line[^1];
        return db.GetMatch(new NikseBitmap2(200, 40), line, item, item.Top, true, 25)?.Text;
    }

    [Fact]
    public void BlobOnBaseline_IsPeriodNotDash()
    {
        var blob = Solid(8, 8);
        var db = TwoLookalikes("-", ".", blob, Baseline - 8);

        Assert.Equal(".", Match(db, Line(blob, Baseline - 8)));
    }

    [Fact]
    public void BlobAboveBaseline_IsDashNotPeriod()
    {
        var blob = Solid(8, 8);
        var db = TwoLookalikes(".", "-", blob, 12);

        Assert.Equal("-", Match(db, Line(blob, 12)));
    }

    [Fact]
    public void LineOfMarksOnly_IsLeftToThePixels()
    {
        // "■ ■ ■": no letters to measure a baseline by, so the first lookalike still wins.
        var blob = Solid(8, 8);
        var db = TwoLookalikes("-", ".", blob, 0);
        var line = Enumerable.Range(0, 3)
            .Select(i => new ImageSplitterItem2(i * 15, 0, blob) { Top = 0, Baseline = 8 })
            .ToList();

        Assert.Equal(NOcrDb.BaselinePlacement.Unknown, NOcrDb.GetBaselinePlacement(line, line[^1]));
        Assert.Equal("-", db.GetMatch(new NikseBitmap2(200, 40), line, line[^1], 0, true, 25)?.Text);
    }

    [Fact]
    public void CommaShapeAboveBaseline_IsApostrophe()
    {
        var mark = Solid(4, 9);
        var db = new NOcrDb(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".nocr"));
        // Trained midway, so the loose passes' top-margin tolerance admits it at either height.
        db.Add(Entry(",", mark, 13));

        Assert.Equal("'", Match(db, Line(mark, 2)));
        Assert.Equal(",", Match(db, Line(mark, Baseline - 6)));
    }

    [Fact]
    public void QuoteShapeOnBaseline_IsComma()
    {
        var mark = Solid(4, 9);
        var db = new NOcrDb(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".nocr"));
        // Trained midway, so the loose passes' top-margin tolerance admits it at either height.
        db.Add(Entry("'", mark, 13));

        Assert.Equal(",", Match(db, Line(mark, Baseline - 6)));
        Assert.Equal("'", Match(db, Line(mark, 2)));
    }

    [Fact]
    public void CommaShapeAtTopOfLine_WithOnlyBaselineCommaEntry_IsApostrophe()
    {
        // A curly apostrophe is a raised comma; the only comma entry was trained at the
        // baseline, so the top-margin gate kept it off the mark and it came out as "*".
        var mark = Solid(4, 9);
        var db = new NOcrDb(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".nocr"));
        db.Add(Entry(",", mark, Baseline - 6));

        Assert.Equal("'", Match(db, Line(mark, 0)));
    }

    [Fact]
    public void SquareDotReadAsComma_NotHangingBelowBaseline_IsPeriod()
    {
        // A heavy dot fits a comma entry scaled down; it sits on the baseline instead of hanging.
        var dot = Solid(8, 8);
        var db = new NOcrDb(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".nocr"));
        db.Add(Entry(",", dot, Baseline - 8));

        Assert.Equal(".", Match(db, Line(dot, Baseline - 8)));
    }

    [Fact]
    public void CommaHangingBelowBaseline_StaysComma()
    {
        var comma = Solid(8, 14);
        var db = new NOcrDb(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".nocr"));
        db.Add(Entry(",", comma, Baseline - 8));

        Assert.Equal(",", Match(db, Line(comma, Baseline - 8)));
    }

    [Theory]
    [InlineData("ъ", "Ї", "ы")]
    [InlineData("ъ", "/", "ы")]
    [InlineData("ь", "і", "ы")]
    [InlineData("Ъ", "І", "Ы")]
    public void SoftOrHardSignFollowedBySameHeightBar_IsYery(string sign, string bar, string expected)
    {
        // Heavy italic: the soft-sign half reads as a hard sign, the bar as "Ї" or "/".
        var signBitmap = Solid(14, 20);
        var barBitmap = Solid(4, 20);
        var db = new NOcrDb(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".nocr"));
        db.Add(Entry(sign, signBitmap, 10));
        db.Add(Entry(bar, barBitmap, 10));
        var line = new List<ImageSplitterItem2>
        {
            new(0, 10, signBitmap) { Top = 10, Baseline = Baseline },
            new(16, 10, barBitmap) { Top = 10, Baseline = Baseline },
        };

        var match = db.GetMatch(new NikseBitmap2(200, 40), line, line[0], 10, true, 25);

        Assert.Equal(expected, match?.Text);
        Assert.Equal(2, match?.ExpandCount);
    }
}
