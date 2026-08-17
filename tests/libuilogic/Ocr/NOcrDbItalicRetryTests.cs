using Nikse.SubtitleEdit.UiLogic.Ocr;

namespace LibUiLogicTests.Ocr;

/// <summary>
/// SE 4 had an "un-italic" retry in nOCR matching: a glyph that matched nothing upright was
/// de-slanted by the italic factor and matched once more, with a hit returned as italic. The
/// pass was lost in the SE 5 port, so italic subtitles produced unknown glyph after unknown
/// glyph (#13660). These tests pin the restored behavior.
/// </summary>
public class NOcrDbItalicRetryTests
{
    private const double Slant = 0.25;

    private static NikseBitmap2 MakeBar(bool italic)
    {
        // A 4x24 vertical bar; the italic variant leans right by Slant (top further right).
        const int h = 24;
        const int w = 4;
        var maxShift = italic ? (int)(h * Slant) : 0;
        var width = w + maxShift;
        var data = new byte[width * h * 4];
        for (var y = 0; y < h; y++)
        {
            var shift = italic ? (int)Math.Round((h - 1 - y) * Slant) : 0;
            for (var x = 0; x < w; x++)
            {
                var i = (x + shift + y * width) * 4;
                data[i] = data[i + 1] = data[i + 2] = data[i + 3] = 255;
            }
        }

        return new NikseBitmap2(width, h, data);
    }

    private static NOcrDb MakeDbWithUprightBar()
    {
        var db = new NOcrDb(Path.Combine(Path.GetTempPath(), $"nocr_italic_test_{Guid.NewGuid():N}.nocr"));
        var ch = new NOcrChar { Text = "l", Width = 4, Height = 24, MarginTop = 0 };
        // Foreground strokes down the bar, background strokes just outside its corners.
        ch.LinesForeground.Add(new NOcrLine(new OcrPoint(1, 1), new OcrPoint(1, 22)));
        ch.LinesForeground.Add(new NOcrLine(new OcrPoint(2, 1), new OcrPoint(2, 22)));
        db.Add(ch);
        return db;
    }

    [Fact]
    public void UprightBar_MatchesWithoutRetry()
    {
        var db = MakeDbWithUprightBar();
        var bar = MakeBar(italic: false);
        var item = new ImageSplitterItem2(0, 0, bar);
        var letters = new List<ImageSplitterItem2> { item };

        var match = db.GetMatch(bar, letters, item, 0, true, 6);

        Assert.NotNull(match);
        Assert.Equal("l", match.Text);
        Assert.False(match.Italic);
    }

    [Fact]
    public void ItalicBar_FailsUpright_ButMatchesViaUnItalicRetry()
    {
        var db = MakeDbWithUprightBar();
        var bar = MakeBar(italic: true);
        var item = new ImageSplitterItem2(0, 0, bar);
        var letters = new List<ImageSplitterItem2> { item };

        var upright = db.GetMatch(bar, letters, item, 0, true, 6);
        Assert.Null(upright); // slanted glyph matches nothing upright

        var retried = db.GetMatch(bar, letters, item, 0, true, 6, italicFactor: Slant);
        Assert.NotNull(retried);
        Assert.Equal("l", retried.Text);
        Assert.True(retried.Italic); // the retry marks the hit as italic
    }

    [Fact]
    public void ItalicRetry_DoesNotMutateTheDatabaseEntry()
    {
        var db = MakeDbWithUprightBar();
        var bar = MakeBar(italic: true);
        var item = new ImageSplitterItem2(0, 0, bar);
        var letters = new List<ImageSplitterItem2> { item };

        _ = db.GetMatch(bar, letters, item, 0, true, 6, italicFactor: Slant);

        Assert.False(db.OcrCharacters[0].Italic); // the stored character stays upright
    }
}
