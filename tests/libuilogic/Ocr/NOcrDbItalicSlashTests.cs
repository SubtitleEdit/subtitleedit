using Nikse.SubtitleEdit.UiLogic.Ocr;
using SkiaSharp;

namespace LibUiLogicTests.Ocr;

/// <summary>
/// A steep italic "l" or "I" is a slanted bar - the shape of "/" - so an italic line read
/// "/ wou/d abso/ute/y /ove". Among italic letters, a slash whose straightened glyph is a bar is
/// read as "l" or "I" by the letters around it.
/// </summary>
public class NOcrDbItalicSlashTests
{
    private const double Slant = 0.3;

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

    /// <summary>A ring that sits on the baseline, 20 px high.</summary>
    private static NikseBitmap2 Ring() => Draw(20, 20, (c, p) =>
    {
        c.DrawRect(0, 0, 20, 5, p);
        c.DrawRect(0, 15, 20, 5, p);
        c.DrawRect(0, 0, 5, 20, p);
        c.DrawRect(15, 0, 5, 20, p);
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

    private static NOcrChar Entry(string text, NikseBitmap2 bitmap, int marginTop, bool italic = false)
    {
        var oc = new NOcrChar(text) { Width = bitmap.Width, Height = bitmap.Height, MarginTop = marginTop, Italic = italic };
        NOcrChar.GenerateLineSegments(60, false, oc, bitmap, NOcrLineAlgorithm.Random, new Random(1));
        return oc;
    }

    /// <summary>"/" is the slanted bar itself, "l" the upright bar, "o" the ring (italic or not).</summary>
    private static NOcrDb Db(bool italicRing)
    {
        var db = new NOcrDb(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".nocr"));
        db.Add(Entry("/", Shear(Bar(), Slant), 0));
        db.Add(Entry("l", Bar(), 0));
        db.Add(Entry("o", Shear(Ring(), Slant), 10, italicRing));
        return db;
    }

    /// <summary>"o/o" with the bar in the middle.</summary>
    private static string? MatchMiddle(NOcrDb db)
    {
        var line = new List<ImageSplitterItem2>
        {
            new(0, 10, Shear(Ring(), Slant)) { Top = 10 },
            new(30, 0, Shear(Bar(), Slant)) { Top = 0 },
            new(50, 10, Shear(Ring(), Slant)) { Top = 10 },
        };

        return db.GetMatch(new NikseBitmap2(100, 40), line, line[1], 0, true, 25, italicFactor: Slant)?.Text;
    }

    [Fact]
    public void SlashAmongItalicLetters_IsAnL()
    {
        Assert.Equal("l", MatchMiddle(Db(italicRing: true)));
    }

    [Fact]
    public void SlashAmongUprightLetters_StaysASlash()
    {
        Assert.Equal("/", MatchMiddle(Db(italicRing: false)));
    }

    [Theory]
    [InlineData("a", "i", "ing", false, "l")] // "ca/ling"
    [InlineData("P", "e", "ease", false, "l")] // "P/ease"
    [InlineData("H", "L", "L", false, "I")] // all capitals
    [InlineData("", "", "", false, "I")] // "/ would"
    [InlineData("", "'", "'m", false, "I")] // "/'m"
    [InlineData("", "'", "'II", false, "I")] // "/'II" = "I'll"
    [InlineData("", "'", "'edificio", false, "l")] // Italian "l'edificio"
    [InlineData("", "o", "ove", false, "l")] // "would absolutely /ove"
    [InlineData("", "t", "t's", true, "I")] // "/t's time"
    [InlineData("'", "", "", false, "l")] // "that'/"
    public void ChooseBarLetter_FollowsTheWord(string previous, string next, string wordAfter, bool sentenceStart, string expected)
    {
        Assert.Equal(expected, NOcrDb.ChooseBarLetter(previous, next, wordAfter, () => sentenceStart));
    }
}
