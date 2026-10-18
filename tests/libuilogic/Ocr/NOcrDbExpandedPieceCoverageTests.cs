using Nikse.SubtitleEdit.UiLogic.Ocr;
using SkiaSharp;

namespace LibUiLogicTests.Ocr;

/// <summary>
/// The scaled expanded pass stretches a trained multi-part glyph over a group up to 25 px wider
/// than it was drawn with and tolerates a few misplaced points. A shipped "%" drawn with five
/// lines claimed a dialog dash plus an opening quote that way, with none of its lines on the
/// second quote stroke. A scaled match must touch every piece it claims; the exact-size pass
/// is unchanged.
/// </summary>
public class NOcrDbExpandedPieceCoverageTests
{
    private const int BarWidth = 6;
    private const int Height = 40;

    private static int BarLeft(int top, int y) => top - (int)Math.Round(y * 12 / 39.0);

    private static bool IsApostrophe(int x, int y) => x < 10 && y < 10;
    private static bool IsBar1(int x, int y) => x >= BarLeft(14, y) && x < BarLeft(14, y) + BarWidth;
    private static bool IsBar2(int x, int y, int shift) => x >= BarLeft(28 + shift, y) && x < BarLeft(28 + shift, y) + BarWidth;

    private static NikseBitmap2 Render(int width, int height, int offsetX, int offsetY, Func<int, int, bool> ink)
    {
        var bmp = new NikseBitmap2(width, height);
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                if (ink(x + offsetX, y + offsetY))
                {
                    bmp.SetPixel(x, y, SKColors.White);
                }
            }
        }

        return bmp;
    }

    /// <summary>Apostrophe + two slanted bars; <paramref name="shift"/> moves the second bar right.</summary>
    private static (NikseBitmap2 Parent, List<ImageSplitterItem2> Letters) MakeLine(int shift)
    {
        var parent = Render(50, Height + 4, 0, 0, (x, y) => IsApostrophe(x, y) || IsBar1(x, y) || IsBar2(x, y, shift));
        var apostrophe = new ImageSplitterItem2(0, 0, Render(10, 10, 0, 0, IsApostrophe)) { Top = 0 };
        var bar1 = new ImageSplitterItem2(2, 0, Render(18, Height, 2, 0, IsBar1)) { Top = 0 };
        var bar2 = new ImageSplitterItem2(16 + shift, 0, Render(18, Height, 16 + shift, 0, (x, y) => IsBar2(x, y, shift))) { Top = 0 };
        return (parent, new List<ImageSplitterItem2> { apostrophe, bar1, bar2 });
    }

    /// <summary>
    /// "'ll" trained from the unshifted 34x40 group: a line through each bar, and through the
    /// apostrophe unless <paramref name="skipApostrophe"/>.
    /// </summary>
    private static NOcrChar MakeApostropheLl(bool skipApostrophe)
    {
        var c = new NOcrChar("'ll") { Width = 34, Height = Height, MarginTop = 0, ExpandCount = 3, Italic = true };
        if (!skipApostrophe)
        {
            c.LinesForeground.Add(new NOcrLine(new OcrPoint(3, 1), new OcrPoint(3, 8)));
        }

        c.LinesForeground.Add(new NOcrLine(new OcrPoint(10, 19), new OcrPoint(10, 21)));
        c.LinesForeground.Add(new NOcrLine(new OcrPoint(24, 19), new OcrPoint(24, 21)));
        c.LinesBackground.Add(new NOcrLine(new OcrPoint(17, 19), new OcrPoint(17, 21)));
        return c;
    }

    private static NOcrDb MakeDb(NOcrChar c)
    {
        var db = new NOcrDb(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".nocr"));
        db.Add(c);
        return db;
    }

    [Fact]
    public void ScaledMatch_TouchingEveryPiece_IsAccepted()
    {
        // The group is 4 px wider than the entry, so only the scaled pass can fit it.
        var (parent, letters) = MakeLine(4);
        var db = MakeDb(MakeApostropheLl(skipApostrophe: false));

        var match = db.GetMatchExpanded(parent, letters[0], 0, letters);

        Assert.NotNull(match);
        Assert.Equal("'ll", match.Text);
    }

    [Fact]
    public void ScaledMatch_ClaimingAnUntouchedPiece_IsRejected()
    {
        // Same group; the entry's lines only ever land on the two bars, never on the apostrophe
        // it would claim as its first piece.
        var (parent, letters) = MakeLine(4);
        var db = MakeDb(MakeApostropheLl(skipApostrophe: true));

        var match = db.GetMatchExpanded(parent, letters[0], 0, letters);

        Assert.Null(match);
    }

    [Fact]
    public void ExactSizeMatch_IsUnchanged()
    {
        var (parent, letters) = MakeLine(0);
        var db = MakeDb(MakeApostropheLl(skipApostrophe: true));

        var match = db.GetMatchExpanded(parent, letters[0], 0, letters);

        Assert.NotNull(match);
        Assert.Equal("'ll", match.Text);
    }
}
