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
        var tallest = glyphs.MaxBy(g => g.NikseBitmap!.Height)!;
        var letterBottom = tallest.Top + tallest.NikseBitmap!.Height;

        // Round letters overshoot the baseline by a pixel or two; a dash is far above it.
        Assert.All(glyphs, g => Assert.InRange(g.Baseline!.Value, letterBottom - 2, letterBottom + 2));
    }
}
