using Nikse.SubtitleEdit.UiLogic.Ocr;
using SkiaSharp;

namespace LibUiLogicTests.Ocr;

/// <summary>
/// Pins the best-match-within-pass semantics of <see cref="NOcrDb.GetMatchSingle"/>: when
/// several candidates fit a pass's error budget, the one with the fewest wrong pixels wins -
/// not the one that happens to come first in the list.
/// </summary>
public class NOcrDbBestMatchTests
{
    /// <summary>10x10 bitmap with an opaque 3px vertical bar at x = 4..6.</summary>
    private static NikseBitmap2 MakeBarBitmap()
    {
        var bmp = new SKBitmap(10, 10);
        using (var canvas = new SKCanvas(bmp))
        {
            canvas.Clear(SKColors.Transparent);
            using var paint = new SKPaint { Color = SKColors.White };
            canvas.DrawRect(4, 0, 3, 10, paint);
        }

        return new NikseBitmap2(bmp);
    }

    [Fact]
    public void GetMatchSingle_PrefersFewestErrors_OverListOrder()
    {
        var bitmap = MakeBarBitmap();

        // Both characters are 20x20 (so the strict size passes are skipped and matching lands
        // in the aspect-only pass with a 3-error budget) and match the bar's foreground.
        // The decoy also has a short background line that lands ON the bar when scaled
        // (2 wrong pixels) - still within the pass budget, so the old first-match-wins
        // algorithm returned it. The correct character matches with 0 errors.
        var decoy = new NOcrChar("#") { Width = 20, Height = 20, MarginTop = 0 };
        decoy.LinesForeground.Add(new NOcrLine(new OcrPoint(10, 0), new OcrPoint(10, 19)));
        decoy.LinesBackground.Add(new NOcrLine(new OcrPoint(8, 0), new OcrPoint(8, 1)));

        var correct = new NOcrChar("l") { Width = 20, Height = 20, MarginTop = 0 };
        correct.LinesForeground.Add(new NOcrLine(new OcrPoint(10, 0), new OcrPoint(10, 19)));
        correct.LinesBackground.Add(new NOcrLine(new OcrPoint(1, 0), new OcrPoint(1, 19)));

        var db = new NOcrDb(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".nocr"));
        db.Add(correct);
        db.Add(decoy); // Add inserts at the front, so the decoy is FIRST in the list

        // Sanity: the decoy really does fit the 3-error budget (the old algorithm would
        // have accepted it), and the correct char matches perfectly.
        Assert.True(NOcrDb.IsMatch(bitmap, decoy, 3));
        Assert.False(NOcrDb.IsMatch(bitmap, decoy, 1));
        Assert.True(NOcrDb.IsMatch(bitmap, correct, 0));

        var match = db.GetMatchSingle(bitmap, 0, false, 25);

        Assert.NotNull(match);
        Assert.Equal("l", match!.Text);
    }

    [Fact]
    public void GetMatchSingle_EqualErrors_KeepsNewestFirst()
    {
        var bitmap = MakeBarBitmap();

        // Two equally good candidates: the newest added (front of the list) must win, so a
        // user's just-added correction takes precedence over an equally-matching older entry.
        var older = new NOcrChar("I") { Width = 20, Height = 20, MarginTop = 0 };
        older.LinesForeground.Add(new NOcrLine(new OcrPoint(10, 0), new OcrPoint(10, 19)));

        var newer = new NOcrChar("l") { Width = 20, Height = 20, MarginTop = 0 };
        newer.LinesForeground.Add(new NOcrLine(new OcrPoint(10, 0), new OcrPoint(10, 19)));

        var db = new NOcrDb(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".nocr"));
        db.Add(older);
        db.Add(newer);

        var match = db.GetMatchSingle(bitmap, 0, false, 25);

        Assert.NotNull(match);
        Assert.Equal("l", match!.Text);
    }
}
