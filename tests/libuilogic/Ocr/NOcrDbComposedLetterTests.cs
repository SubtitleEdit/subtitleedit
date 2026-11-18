using Nikse.SubtitleEdit.UiLogic.Ocr;
using SkiaSharp;

namespace LibUiLogicTests.Ocr;

/// <summary>
/// A Latin database has no entries for "ż", "ś", "ć", "ą", "ę" or "ł", so Polish (and Czech,
/// Slovenian, ...) text came out full of "*". A glyph that matches nothing is read as a letter the
/// database knows plus a mark over it, a letter plus an ogonek, or a stroked "l".
/// </summary>
public class NOcrDbComposedLetterTests
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

    private static void Ring(SKCanvas c, SKPaint p, float top)
    {
        c.DrawRect(0, top, 20, 5, p);
        c.DrawRect(0, top + 15, 20, 5, p);
        c.DrawRect(0, top, 5, 20, p);
        c.DrawRect(15, top, 5, 20, p);
    }

    /// <summary>A 20x20 ring, the database's "e" (any shape will do).</summary>
    private static NikseBitmap2 Body() => Draw(20, 20, (c, p) => Ring(c, p, 0));

    private static void Acute(SKCanvas c, SKPaint p)
    {
        using var path = new SKPath();
        path.MoveTo(10, 0);
        path.LineTo(15, 0);
        path.LineTo(9, 6);
        path.LineTo(4, 6);
        path.Close();
        c.DrawPath(path, p);
    }

    private static NOcrChar Entry(string text, NikseBitmap2 bitmap, int marginTop)
    {
        var oc = new NOcrChar(text) { Width = bitmap.Width, Height = bitmap.Height, MarginTop = marginTop };
        NOcrChar.GenerateLineSegments(60, false, oc, bitmap, NOcrLineAlgorithm.Random, new Random(1));
        return oc;
    }

    private static NOcrDb Db(params NOcrChar[] entries)
    {
        var db = new NOcrDb(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".nocr"));
        foreach (var entry in entries)
        {
            db.Add(entry);
        }

        return db;
    }

    private static string? Match(NOcrDb db, NikseBitmap2 glyph)
    {
        var item = new ImageSplitterItem2(0, 0, glyph) { Top = 0 };
        return db.GetMatch(new NikseBitmap2(100, 40), [item], item, 0, true, 25)?.Text;
    }

    [Fact]
    public void LetterWithAcuteAbove_IsComposed()
    {
        var glyph = Draw(20, 30, (c, p) =>
        {
            Acute(c, p);
            Ring(c, p, 10);
        });

        Assert.Equal("é", Match(Db(Entry("e", Body(), 10)), glyph));
    }

    [Fact]
    public void LetterWithDotAbove_IsComposed()
    {
        var glyph = Draw(20, 30, (c, p) =>
        {
            c.DrawRect(7, 0, 6, 6, p);
            Ring(c, p, 10);
        });

        Assert.Equal("ė", Match(Db(Entry("e", Body(), 10)), glyph));
    }

    [Fact]
    public void LetterWithOgonek_IsComposed()
    {
        // The hook hangs from the right side of the body and turns right.
        var glyph = Draw(24, 28, (c, p) =>
        {
            Ring(c, p, 0);
            c.DrawRect(14, 20, 4, 5, p);
            c.DrawRect(14, 24, 10, 4, p);
        });

        Assert.Equal("ę", Match(Db(Entry("e", Body(), 0)), glyph));
    }

    [Fact]
    public void UnknownBody_StaysUnmatched()
    {
        // A mark over something the database does not know is no letter.
        var glyph = Draw(20, 30, (c, p) =>
        {
            Acute(c, p);
            c.DrawRect(0, 10, 20, 20, p);
        });

        Assert.Null(Match(Db(Entry("e", Body(), 10)), glyph));
    }

    [Fact]
    public void StrokedL_InALine_IsRead()
    {
        // Stem 4 px wide, 24 high; the stroke crosses it rising to the right.
        var stroked = Draw(10, 24, (c, p) =>
        {
            c.DrawRect(3, 0, 4, 24, p);
            c.DrawRect(0, 13, 3, 2, p);
            c.DrawRect(7, 8, 3, 2, p);
        });
        var tall = Draw(12, 24, (c, p) => c.DrawRect(0, 0, 12, 24, p));
        var line = new List<ImageSplitterItem2>
        {
            new(0, 0, tall) { Top = 0 },
            new(20, 0, stroked) { Top = 0 },
            new(40, 0, tall) { Top = 0 },
        };
        var db = Db(Entry("e", Body(), 0));

        Assert.Equal("ł", db.GetMatch(new NikseBitmap2(100, 40), line, line[1], 0, true, 25)?.Text);
    }

    [Theory]
    [InlineData("dot", '\u0307')]
    [InlineData("acute", '\u0301')]
    [InlineData("grave", '\u0300')]
    [InlineData("diaeresis", '\u0308')]
    [InlineData("caron", '\u030C')]
    [InlineData("circumflex", '\u0302')]
    public void ClassifyMark_TellsTheMarksApart(string name, char expected)
    {
        var mark = name switch
        {
            "dot" => Draw(5, 5, (c, p) => c.DrawRect(0, 0, 5, 5, p)),
            "acute" => Draw(11, 6, (c, p) => Acute(c, p)),
            "grave" => Draw(11, 6, (c, p) =>
            {
                using var path = new SKPath();
                path.MoveTo(0, 0);
                path.LineTo(5, 0);
                path.LineTo(11, 6);
                path.LineTo(6, 6);
                path.Close();
                c.DrawPath(path, p);
            }),
            "diaeresis" => Draw(14, 4, (c, p) =>
            {
                c.DrawRect(0, 0, 4, 4, p);
                c.DrawRect(10, 0, 4, 4, p);
            }),
            "caron" => Draw(14, 6, (c, p) =>
            {
                c.DrawRect(0, 0, 3, 3, p);
                c.DrawRect(11, 0, 3, 3, p);
                c.DrawRect(3, 2, 8, 4, p);
            }),
            _ => Draw(14, 6, (c, p) =>
            {
                c.DrawRect(3, 0, 8, 4, p);
                c.DrawRect(0, 3, 3, 3, p);
                c.DrawRect(11, 3, 3, 3, p);
            }),
        };

        Assert.Equal(expected, NOcrDb.ClassifyMark(TrimToInk(mark), 20));
    }

    private static NikseBitmap2 TrimToInk(NikseBitmap2 bitmap)
    {
        int minX = bitmap.Width, minY = bitmap.Height, maxX = -1, maxY = -1;
        for (var y = 0; y < bitmap.Height; y++)
        {
            for (var x = 0; x < bitmap.Width; x++)
            {
                if (bitmap.GetAlpha(x, y) > 150)
                {
                    minX = Math.Min(minX, x);
                    maxX = Math.Max(maxX, x);
                    minY = Math.Min(minY, y);
                    maxY = Math.Max(maxY, y);
                }
            }
        }

        return bitmap.CopyRectangle(new NikseRectangle(minX, minY, maxX - minX + 1, maxY - minY + 1));
    }
}
