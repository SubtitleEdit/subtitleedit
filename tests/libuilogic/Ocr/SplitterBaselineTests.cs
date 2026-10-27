using Nikse.SubtitleEdit.UiLogic.Ocr;
using SkiaSharp;

namespace LibUiLogicTests.Ocr;

public class SplitterBaselineTests
{
    [Theory]
    [InlineData("-Tom--")]
    [InlineData("-Jad--")]
    [InlineData("I-I-I-")]
    public void MarksOutnumberingLetters_DoNotSetTheBaseline(string text)
    {
        // The median bottom of "-Tom--" is a dash's, which put the dashes "on the baseline" -
        // where a dash never sits - so they were left unread.
        using var bmp = NOcrTrainer.RenderCharacterImage(text, SKTypeface.Default.FamilyName, 72, true, false);
        var parent = new NikseBitmap2(bmp!);
        parent.MakeTwoColor(200);
        parent.CropTop(0, new SKColor(0, 0, 0, 0));

        var glyphs = NikseBitmapImageSplitter2.SplitBitmapToLettersNew(parent, 1, false, true, 25, true)
            .Where(i => i.NikseBitmap != null)
            .ToList();
        var maxHeight = glyphs.Max(g => g.NikseBitmap!.Height);
        var marks = glyphs.Where(g => g.NikseBitmap!.Height * 20 < maxHeight * 9).ToList();
        var letterBottoms = glyphs.Except(marks).Select(g => g.Top + g.NikseBitmap!.Height).Order().ToList();
        var letterBottom = letterBottoms[(letterBottoms.Count - 1) / 2]; // "J" may hang below the line

        Assert.NotEmpty(marks);
        Assert.All(glyphs, g => Assert.InRange(g.Baseline!.Value, letterBottom - 2, letterBottom + 2));
        Assert.All(marks, m => Assert.True(m.Top + m.NikseBitmap!.Height < letterBottom - 2));
    }

    [Fact]
    public void QuotesAndDashes_DoNotSetTheBaseline()
    {
        // ''I-- with subtitle-font proportions: quotes are 40% of the line height (more than in
        // the test's default font), so they too must count as marks or the dashes stay "on the
        // baseline".
        var bmp = new SKBitmap(200, 40);
        using (var canvas = new SKCanvas(bmp))
        {
            canvas.Clear(SKColors.Transparent);
            using var paint = new SKPaint { Color = SKColors.White };
            canvas.DrawRect(0, 0, 6, 16, paint);    // quote
            canvas.DrawRect(12, 0, 6, 16, paint);   // quote
            canvas.DrawRect(30, 0, 8, 40, paint);   // "I"
            canvas.DrawRect(50, 12, 16, 28, paint); // x-height letter
            canvas.DrawRect(80, 22, 16, 5, paint);  // dash
            canvas.DrawRect(110, 22, 16, 5, paint); // dash
            canvas.DrawRect(140, 22, 16, 5, paint); // dash
        }

        var glyphs = NikseBitmapImageSplitter2.SplitBitmapToLettersNew(new NikseBitmap2(bmp), 1, false, true, 25, true)
            .Where(i => i.NikseBitmap != null)
            .ToList();

        Assert.Equal(7, glyphs.Count);
        Assert.All(glyphs, g => Assert.Equal(40, g.Baseline));
    }
}
