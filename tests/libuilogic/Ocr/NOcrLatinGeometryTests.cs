using Nikse.SubtitleEdit.UiLogic.Ocr;
using SkiaSharp;

namespace LibUiLogicTests.Ocr;

/// <summary>
/// Geometry checks that keep lookalikes apart where pixels can't: "p"/"P" by the baseline,
/// accented letters split in parts only with the accent above the letter, accented letters
/// only when they clearly beat the plain one, and the "i" family only for glyphs with a gap.
/// </summary>
public class NOcrLatinGeometryTests
{
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

    /// <summary>A stem with a separate dot above it (an empty row between them).</summary>
    private static NikseBitmap2 Dotted(int width, int height)
    {
        var bmp = new SKBitmap(width, height);
        using (var canvas = new SKCanvas(bmp))
        {
            canvas.Clear(SKColors.Transparent);
            using var paint = new SKPaint { Color = SKColors.White };
            var dot = height * 15 / 100;
            var stem = height * 27 / 100;
            canvas.DrawRect(0, 0, width, dot, paint);
            canvas.DrawRect(0, stem, width, height - stem, paint);
        }

        return new NikseBitmap2(bmp);
    }

    private static NOcrChar Entry(string text, NikseBitmap2 bitmap)
    {
        var oc = new NOcrChar(text) { Width = bitmap.Width, Height = bitmap.Height, MarginTop = 0 };
        NOcrChar.GenerateLineSegments(40, false, oc, bitmap);
        return oc;
    }

    private static NOcrDb Db(params NOcrChar[] entriesLastFirst)
    {
        var db = new NOcrDb(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".nocr"));
        foreach (var e in entriesLastFirst)
        {
            db.Add(e); // Add inserts at the front: the last one added wins ties
        }

        return db;
    }

    [Theory]
    [InlineData("P", 10, 40, "p")] // top at x-height, hangs 10 px below the baseline at 40
    [InlineData("p", 0, 40, "P")]  // sits on the baseline
    [InlineData("P", 0, 40, "P")]
    [InlineData("p", 10, 40, "p")]
    public void CaseFixer_PFromBaseline(string matched, int top, int height, string expected)
    {
        var item = new ImageSplitterItem2(0, top, Solid(20, height)) { Top = top, Baseline = 40 };

        Assert.Equal(expected, new NOcrCaseFixer().FixUppercaseLowercaseIssues(item, new NOcrChar(matched)));
    }

    [Fact]
    public void ExpandedAccentedLetter_DoesNotClaimTwoBarsSideBySide()
    {
        // "ll" in an italic font fitted a two-part "û" entry; neither bar is above the other.
        var parentBmp = new SKBitmap(40, 45);
        using (var canvas = new SKCanvas(parentBmp))
        {
            canvas.Clear(SKColors.Transparent);
            using var paint = new SKPaint { Color = SKColors.White };
            canvas.DrawRect(2, 0, 6, 40, paint);
            canvas.DrawRect(26, 0, 6, 40, paint);
        }

        var bar1 = new ImageSplitterItem2(2, 0, Solid(6, 40)) { Top = 0 };
        var bar2 = new ImageSplitterItem2(26, 0, Solid(6, 40)) { Top = 0 };
        var letters = new List<ImageSplitterItem2> { bar1, bar2 };
        var pair = new NOcrChar("û") { Width = 28, Height = 38, MarginTop = 0, ExpandCount = 2 };
        pair.LinesForeground.Add(new NOcrLine(new OcrPoint(3, 1), new OcrPoint(3, 36)));
        pair.LinesForeground.Add(new NOcrLine(new OcrPoint(22, 1), new OcrPoint(22, 36)));
        pair.LinesBackground.Add(new NOcrLine(new OcrPoint(13, 0), new OcrPoint(13, 37)));
        var db = Db(pair);

        Assert.Null(db.GetMatchExpanded(new NikseBitmap2(parentBmp), bar1, 0, letters));
    }

    [Fact]
    public void AccentedLetterTyingWithPlainLetter_IsPlain()
    {
        // Entries trained at another size, as from fonts (an identical-size entry is a user's
        // exact match and wins as is).
        var glyph = Dotted(7, 37);
        var trained = Dotted(6, 30);
        var db = Db(Entry("i", trained), Entry("ì", trained));

        Assert.Equal("i", db.GetMatchSingle(glyph, 0, true, 25)?.Text);
    }

    [Fact]
    public void AcuteTyingWithPlainLetter_StaysAccented()
    {
        // Only the rare grave "ì" yields to "i" - "í" is an everyday Spanish/Hungarian letter.
        var trained = Dotted(6, 30);
        var db = Db(Entry("i", trained), Entry("í", trained));

        Assert.Equal("í", db.GetMatchSingle(Dotted(7, 37), 0, true, 25)?.Text);
    }

    [Fact]
    public void SolidStroke_IsNotDottedOrAccentedI()
    {
        // A slanted italic "l" is one stroke; an italic "ì" entry fitted it.
        var bar = Solid(8, 40);
        var trained = Solid(6, 32);
        var db = Db(Entry("l", trained), Entry("ì", trained));

        Assert.Equal("l", db.GetMatchSingle(bar, 0, true, 25)?.Text);
    }

    [Fact]
    public void DottedGlyph_StillReadsAsI()
    {
        var glyph = Dotted(7, 37);
        var db = Db(Entry("l", Solid(6, 30)), Entry("i", Dotted(6, 30)));

        Assert.Equal("i", db.GetMatchSingle(glyph, 0, true, 25)?.Text);
    }
}
