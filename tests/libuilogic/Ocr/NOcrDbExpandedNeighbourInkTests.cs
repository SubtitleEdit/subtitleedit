using Nikse.SubtitleEdit.UiLogic.Ocr;
using SkiaSharp;

namespace LibUiLogicTests.Ocr;

/// <summary>
/// Issue #14768: italic "I'll" came out of nOCR as I ' 'll - the apostrophe twice. The
/// apostrophe matched on its own, and then the trained expanded "'ll" matched again with the
/// scan starting at the first "l": in italics the apostrophe sits inside the x-range of that
/// "l", and the expanded match sampled the parent bitmap, so the apostrophe's real ink
/// satisfied the candidate's apostrophe lines. Foreground points of an expanded candidate must
/// only count ink that belongs to the splitter items the candidate would claim.
/// </summary>
public class NOcrDbExpandedNeighbourInkTests
{
    // Two slanted 6 px bars (top x offset 14 and 28, sliding 12 px left over 40 rows) with a
    // 10x10 apostrophe block at the top-left. The apostrophe ends at x=9, the first bar starts
    // at x=2 near its bottom, so their bounding boxes overlap but their pixels do not.
    private const int BarWidth = 6;
    private const int Height = 40;

    private static int BarLeft(int top, int y) => top - (int)Math.Round(y * 12 / 39.0);

    private static bool IsApostrophe(int x, int y) => x < 10 && y < 10;
    private static bool IsBar1(int x, int y) => x >= BarLeft(14, y) && x < BarLeft(14, y) + BarWidth;
    private static bool IsBar2(int x, int y) => x >= BarLeft(28, y) && x < BarLeft(28, y) + BarWidth;

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

    private static (NikseBitmap2 Parent, List<ImageSplitterItem2> Letters) MakeLine()
    {
        var parent = Render(40, Height + 4, 0, 0, (x, y) => IsApostrophe(x, y) || IsBar1(x, y) || IsBar2(x, y));
        var apostrophe = new ImageSplitterItem2(0, 0, Render(10, 10, 0, 0, IsApostrophe)) { Top = 0 };
        var bar1 = new ImageSplitterItem2(2, 0, Render(18, Height, 2, 0, IsBar1)) { Top = 0 };
        var bar2 = new ImageSplitterItem2(16, 0, Render(18, Height, 16, 0, IsBar2)) { Top = 0 };
        return (parent, new List<ImageSplitterItem2> { apostrophe, bar1, bar2 });
    }

    /// <summary>
    /// "'ll" as trained from the whole 34x40 group: a line through the apostrophe, one through
    /// each bar at row 20 and a background line in the gap between the bars.
    /// </summary>
    private static NOcrChar MakeApostropheLl(int expandCount)
    {
        var c = new NOcrChar("'ll") { Width = 34, Height = Height, MarginTop = 0, ExpandCount = expandCount, Italic = true };
        c.LinesForeground.Add(new NOcrLine(new OcrPoint(3, 1), new OcrPoint(3, 8)));
        c.LinesForeground.Add(new NOcrLine(new OcrPoint(10, 19), new OcrPoint(10, 21)));
        c.LinesForeground.Add(new NOcrLine(new OcrPoint(24, 19), new OcrPoint(24, 21)));
        c.LinesBackground.Add(new NOcrLine(new OcrPoint(17, 19), new OcrPoint(17, 21)));
        return c;
    }

    private static NOcrDb MakeDb(params NOcrChar[] chars)
    {
        var db = new NOcrDb(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".nocr"));
        foreach (var c in chars)
        {
            db.Add(c);
        }

        return db;
    }

    [Fact]
    public void ExpandedMatch_DoesNotUseInkOfNeighbourOutsideGroup()
    {
        // A "'ll" trained as two parts (apostrophe+l fused, l). Starting the scan at the first
        // bar, its group is 32x40 - within the exact size gate of the 34 px trained glyph - and
        // the shifted apostrophe line lands on the real apostrophe in the parent bitmap.
        var (parent, letters) = MakeLine();
        var db = MakeDb(MakeApostropheLl(2));

        var match = db.GetMatchExpanded(parent, letters[1], 1, letters);

        Assert.Null(match);
    }

    [Fact]
    public void ExpandedMatch_StillMatchesWhenGroupOwnsAllInk()
    {
        // The counterpart guard: the same lines as a three-part glyph, scanned from the
        // apostrophe, claim exactly the three items and must still match.
        var (parent, letters) = MakeLine();
        var db = MakeDb(MakeApostropheLl(3));

        var match = db.GetMatchExpanded(parent, letters[0], 0, letters);

        Assert.NotNull(match);
        Assert.Equal("'ll", match.Text);
    }
}
