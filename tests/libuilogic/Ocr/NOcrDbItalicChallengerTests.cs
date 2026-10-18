using Nikse.SubtitleEdit.UiLogic.Ocr;
using SkiaSharp;

namespace LibUiLogicTests.Ocr;

/// <summary>
/// A slanted glyph often still fits some entry loosely (an italic "t" read as an italic "f", "r"
/// as "g", "y" as "x"), and that loose hit used to end the search before the straightened glyph
/// was tried. <see cref="NOcrDb.GetMatch"/> now challenges an upright match that is not a close
/// one with the straightened glyph, sheared by the configured factor and by a steeper one, and
/// keeps whichever fits clearly better.
/// </summary>
public class NOcrDbItalicChallengerTests
{
    private static NikseBitmap2 Draw(int width, int height, Action<SKCanvas, SKPaint> draw)
    {
        var bmp = new SKBitmap(width, height);
        using (var canvas = new SKCanvas(bmp))
        {
            canvas.Clear(SKColors.Transparent);
            using var paint = new SKPaint { Color = SKColors.White, IsAntialias = false };
            draw(canvas, paint);
        }

        return new NikseBitmap2(bmp);
    }

    /// <summary>A "T": a stem with a crossbar on top.</summary>
    private static NikseBitmap2 Tee() => Draw(20, 30, (c, p) =>
    {
        c.DrawRect(0, 0, 20, 5, p);
        c.DrawRect(7, 0, 6, 30, p);
    });

    /// <summary>Slants an upright glyph: each row moves left by its distance from the top x factor.</summary>
    private static NikseBitmap2 Shear(NikseBitmap2 upright, double factor)
    {
        var maxShift = (int)Math.Round((upright.Height - 1) * factor, MidpointRounding.AwayFromZero);
        var sheared = new NikseBitmap2(upright.Width + maxShift, upright.Height);
        for (var y = 0; y < upright.Height; y++)
        {
            var offset = maxShift - (int)Math.Round(y * factor, MidpointRounding.AwayFromZero);
            for (var x = 0; x < upright.Width; x++)
            {
                if (upright.GetAlpha(x, y) > 0)
                {
                    sheared.SetPixel(x + offset, y, upright.GetPixel(x, y));
                }
            }
        }

        return sheared;
    }

    /// <summary>Seeded, so the lines - and with them how loosely the decoy fits - are the same on every run and platform.</summary>
    private static NOcrChar Entry(string text, NikseBitmap2 bitmap, int marginTop)
    {
        var oc = new NOcrChar(text) { Width = bitmap.Width, Height = bitmap.Height, MarginTop = marginTop };
        NOcrChar.GenerateLineSegments(40, false, oc, bitmap, NOcrLineAlgorithm.Random, new Random(1));
        return oc;
    }

    private const double DecoyShear = 0.15;
    private const double GlyphShear = 0.3;

    /// <summary>
    /// "T" trained upright, plus a decoy "X" trained on a "T" slanted half as much as the glyph -
    /// the decoy fits the glyph loosely (about 3% wrong points), the way an italic "f" entry fits
    /// an italic "t".
    /// </summary>
    private static NOcrDb Db()
    {
        var db = new NOcrDb(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".nocr"));
        db.Add(Entry("T", Tee(), 0));
        db.Add(Entry("X", Shear(Tee(), DecoyShear), 0));
        return db;
    }

    private static NOcrChar? Match(NOcrDb db, NikseBitmap2 glyph, double italicFactor)
    {
        var item = new ImageSplitterItem2(0, 0, glyph) { Top = 0 };
        return db.GetMatch(new NikseBitmap2(100, 40), [item], item, 0, true, 25, italicFactor: italicFactor);
    }

    [Fact]
    public void LooseUprightMatch_LosesToTheStraightenedGlyph()
    {
        var glyph = Shear(Tee(), GlyphShear);

        var match = Match(Db(), glyph, italicFactor: 0.2);

        Assert.NotNull(match);
        Assert.Equal("T", match.Text);
        Assert.True(match.Italic);
    }

    [Fact]
    public void WithoutItalicRetry_TheLooseUprightMatchStands()
    {
        var glyph = Shear(Tee(), GlyphShear);

        var match = Match(Db(), glyph, italicFactor: 0);

        Assert.NotNull(match);
        Assert.Equal("X", match.Text);
    }

    [Fact]
    public void CloseUprightMatch_IsNotChallenged()
    {
        // The glyph is the decoy's own training shape: an exact fit is never second-guessed.
        var glyph = Shear(Tee(), DecoyShear);

        var match = Match(Db(), glyph, italicFactor: 0.2);

        Assert.NotNull(match);
        Assert.Equal("X", match.Text);
        Assert.False(match.Italic);
    }
}
