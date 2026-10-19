using Nikse.SubtitleEdit.UiLogic.Ocr;
using SkiaSharp;

namespace LibUiLogicTests.Ocr;

/// <summary>
/// Two letters drawn touching are one glyph that fits nothing; <see cref="NOcrDb.GetMatch"/>
/// reads it as the two letters on either side of its thinnest column.
/// </summary>
public class NOcrDbTouchingLettersTests
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

    private static string? Match(NOcrDb db, NikseBitmap2 glyph)
    {
        var item = new ImageSplitterItem2(0, 0, glyph) { Top = 0 };
        return db.GetMatch(new NikseBitmap2(100, 40), [item], item, 0, true, 25)?.Text;
    }

    [Fact]
    public void TouchingBarAndRing_ReadAsTwoLetters()
    {
        // "lo" with a one-pixel bridge between the letters.
        var glyph = Draw(27, 30, (c, p) =>
        {
            c.DrawRect(0, 0, 6, 30, p);
            c.DrawRect(6, 20, 1, 1, p);
            c.DrawRect(7, 10, 20, 4, p);
            c.DrawRect(7, 26, 20, 4, p);
            c.DrawRect(7, 10, 4, 20, p);
            c.DrawRect(23, 10, 4, 20, p);
        });

        Assert.Equal("lo", Match(Db(), glyph));
    }

    [Fact]
    public void LetterAndDigitHalves_AreNotAPair()
    {
        // Same glyph, but the bar is trained as "1": "1o" is no pair of touching letters.
        var db = new NOcrDb(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".nocr"));
        db.Add(Entry("1", Bar(), 0));
        db.Add(Entry("o", Ring(), 10));
        var glyph = Draw(27, 30, (c, p) =>
        {
            c.DrawRect(0, 0, 6, 30, p);
            c.DrawRect(6, 20, 1, 1, p);
            c.DrawRect(7, 10, 20, 4, p);
            c.DrawRect(7, 26, 20, 4, p);
            c.DrawRect(7, 10, 4, 20, p);
            c.DrawRect(23, 10, 4, 20, p);
        });

        Assert.NotEqual("1o", Match(db, glyph));
    }

    [Fact]
    public void NarrowUnknownGlyph_IsNotSplit()
    {
        // Taller than wide and unlike anything trained: stays unrecognised.
        var glyph = Draw(8, 30, (c, p) =>
        {
            c.DrawRect(0, 0, 8, 3, p);
            c.DrawRect(0, 27, 8, 3, p);
            c.DrawRect(3, 0, 2, 30, p);
        });

        Assert.Null(Match(Db(), glyph));
    }
}
