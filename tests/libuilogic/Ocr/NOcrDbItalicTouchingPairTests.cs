using Nikse.SubtitleEdit.UiLogic.Ocr;
using SkiaSharp;

namespace LibUiLogicTests.Ocr;

/// <summary>
/// Two touching italic letters ("fa", "is", "ts", "rs" in a bold italic) are cut along a
/// vertical column like upright ones, which on the slanted pair leaves ragged halves that fit
/// nothing. <see cref="NOcrDb.GetMatch"/> now also tries the touching-pair split on the
/// straightened glyph, gives a stroke end that the cut left on the wrong letter back (the arm
/// of "f" over "a"), and reads two halves of one height in one case ("rS" -> "rs").
/// </summary>
public class NOcrDbItalicTouchingPairTests
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

    private static NikseBitmap2 Bar() => Draw(6, 30, (c, p) => c.DrawRect(0, 0, 6, 30, p));

    private static NikseBitmap2 Ring() => Draw(20, 20, (c, p) =>
    {
        c.DrawRect(0, 0, 20, 4, p);
        c.DrawRect(0, 16, 20, 4, p);
        c.DrawRect(0, 0, 4, 20, p);
        c.DrawRect(16, 0, 4, 20, p);
    });

    /// <summary>"lo" with a one-pixel bridge between the letters.</summary>
    private static NikseBitmap2 TouchingBarAndRing() => Draw(27, 30, (c, p) =>
    {
        c.DrawRect(0, 0, 6, 30, p);
        c.DrawRect(6, 20, 1, 1, p);
        c.DrawRect(7, 10, 20, 4, p);
        c.DrawRect(7, 26, 20, 4, p);
        c.DrawRect(7, 10, 4, 20, p);
        c.DrawRect(23, 10, 4, 20, p);
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

    private static NOcrChar Entry(string text, NikseBitmap2 bitmap, int marginTop)
    {
        var oc = new NOcrChar(text) { Width = bitmap.Width, Height = bitmap.Height, MarginTop = marginTop };
        NOcrChar.GenerateLineSegments(40, false, oc, bitmap);
        return oc;
    }

    private static NOcrDb Db()
    {
        var db = new NOcrDb(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".nocr"));
        db.Add(Entry("l", Bar(), 0));
        db.Add(Entry("o", Ring(), 10));
        return db;
    }

    private static NOcrChar? Match(NOcrDb db, NikseBitmap2 glyph, double italicFactor)
    {
        var item = new ImageSplitterItem2(0, 0, glyph) { Top = 0 };
        return db.GetMatch(new NikseBitmap2(100, 40), [item], item, 0, true, 25, italicFactor: italicFactor);
    }

    [Fact]
    public void SlantedTouchingPair_IsSplitAfterStraightening()
    {
        var glyph = Shear(TouchingBarAndRing(), 0.2);

        var match = Match(Db(), glyph, italicFactor: 0.2);

        Assert.NotNull(match);
        Assert.Equal("lo", match.Text);
        Assert.True(match.Italic);
    }

    [Fact]
    public void SlantedTouchingPair_WithoutItalicRetry_StaysUnknown()
    {
        var glyph = Shear(TouchingBarAndRing(), 0.2);

        Assert.Null(Match(Db(), glyph, italicFactor: 0));
    }

    [Theory]
    [InlineData("r", "S", 30, 30, "rs")] // the "S" is a scaled capital on an x-height half
    [InlineData("V", "v", 30, 30, "vv")]
    [InlineData("r", "S", 20, 30, "rS")] // a real capital is taller than its lowercase neighbour
    [InlineData("r", "B", 30, 30, "rB")] // "B" and "b" are different shapes - no scaling confusion
    public void HarmonizeCase_SameHeightHalves_ShareACase(string left, string right, int leftHeight, int rightHeight, string expected)
    {
        var text = NOcrDb.HarmonizeCase(left, right, new NikseBitmap2(10, leftHeight), new NikseBitmap2(10, rightHeight), 0, 0);

        Assert.Equal(expected, text);
    }

    [Fact]
    public void StrokeEndCutOntoTheNextLetter_GoesBackToItsLetter()
    {
        // A stem with an arm reaching right over a ring ("f" over "o"), cut under the arm at
        // column 8: the right half starts with the arm's end above the ring.
        var whole = Draw(30, 30, (c, p) =>
        {
            c.DrawRect(0, 0, 6, 30, p);   // stem
            c.DrawRect(0, 0, 18, 4, p);   // arm, over the gap and the ring
            c.DrawRect(10, 10, 20, 4, p); // ring
            c.DrawRect(10, 26, 20, 4, p);
            c.DrawRect(10, 10, 4, 20, p);
            c.DrawRect(26, 10, 4, 20, p);
        });
        const int cut = 8;
        var left = whole.CopyRectangle(new NikseRectangle(0, 0, cut, 30));
        var right = whole.CopyRectangle(new NikseRectangle(cut + 1, 0, 30 - cut - 1, 30));

        var returned = NOcrDb.ReturnStrokeEnds(whole, cut, left, right);

        Assert.NotSame(left, returned);
        Assert.True(returned.GetAlpha(17, 1) > 0);   // the arm's end is on the left now
        Assert.True(returned.GetAlpha(cut, 1) > 0);  // joined through the cut column
        Assert.True(right.GetAlpha(0, 1) == 0 && right.GetAlpha(8, 1) == 0); // and gone from the right
        Assert.True(right.GetAlpha(1, 10) > 0);      // the ring stays
    }

    [Fact]
    public void DotNextToTheCut_StaysWithItsLetter()
    {
        // A bar, then a dotted stem ("li") cut between them at column 7: the dot touches the cut
        // but nothing continues through it, so it is no stroke end of the bar.
        var whole = Draw(20, 30, (c, p) =>
        {
            c.DrawRect(0, 0, 6, 30, p);   // bar
            c.DrawRect(8, 0, 5, 5, p);    // dot
            c.DrawRect(9, 10, 4, 20, p);  // stem under it
        });
        const int cut = 7;
        var left = whole.CopyRectangle(new NikseRectangle(0, 0, cut, 30));
        var right = whole.CopyRectangle(new NikseRectangle(cut + 1, 0, 20 - cut - 1, 30));

        var returned = NOcrDb.ReturnStrokeEnds(whole, cut, left, right);

        Assert.Same(left, returned);
        Assert.True(right.GetAlpha(0, 1) > 0);
    }
}
