using Nikse.SubtitleEdit.UiLogic.Ocr;
using SkiaSharp;

namespace LibUiLogicTests.Ocr;

public class SplitterCrumbLineTests
{
    [Theory]
    [InlineData("(heavy breathing)")]
    [InlineData("(gasps)")]
    public void OneLargeLine_IsNotCutIntoABogusSecondLine(string text)
    {
        // Large text with tall parentheses is over 2.2x the minimum line height, so it went
        // through the up/down splitter, which shaved the tip off a parenthesis into a line of
        // its own (read as "*").
        using var bmp = NOcrTrainer.RenderCharacterImage(text, SKTypeface.Default.FamilyName, 72, true, false);
        var parent = new NikseBitmap2(bmp!);
        parent.MakeTwoColor(200);
        parent.CropTop(0, new SKColor(0, 0, 0, 0));

        var items = NikseBitmapImageSplitter2.SplitBitmapToLettersNew(parent, 1, false, true, 25, true);

        Assert.DoesNotContain(items, i => i.SpecialCharacter == Environment.NewLine);
    }
}
