using Nikse.SubtitleEdit.UiLogic.Ocr;
using SkiaSharp;

namespace LibUiLogicTests.Ocr;

/// <summary>
/// Matching scales a database entry to the glyph, so a tiny entry with a few lines (an
/// apostrophe) fitted the stem of a much bigger letter: "test" was read as "'est" on Linux CI.
/// </summary>
public class NOcrDbSmallGlyphScaleTests
{
    private static NikseBitmap2 Bar(int width, int height)
    {
        using var bitmap = new SKBitmap(width, height);
        bitmap.Erase(SKColors.White);
        return new NikseBitmap2(bitmap);
    }

    private static NOcrDb DbWithApostrophe()
    {
        var db = new NOcrDb(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".nocr"));
        var apostrophe = new NOcrChar("'") { Width = 3, Height = 9, MarginTop = 0 };
        apostrophe.LinesForeground.Add(new NOcrLine(new OcrPoint(1, 0), new OcrPoint(1, 8)));
        db.Add(apostrophe);
        return db;
    }

    [Fact]
    public void SmallEntry_DoesNotMatchGlyphManyTimesItsSize()
    {
        var db = DbWithApostrophe();

        Assert.Null(db.GetMatchSingle(Bar(8, 22), 0, true, 25, lastDitch: true));
    }

    [Fact]
    public void SmallEntry_StillMatchesAtTwiceItsSize()
    {
        var db = DbWithApostrophe();

        Assert.Equal("'", db.GetMatchSingle(Bar(6, 18), 0, true, 25)?.Text);
    }
}
