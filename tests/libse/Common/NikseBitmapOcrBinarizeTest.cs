using Nikse.SubtitleEdit.Core.Common;
using SkiaSharp;

namespace LibSETests.Common;

public class NikseBitmapOcrBinarizeTest
{
    // Bright subtitle text of any colour (white, yellow, red, …) must survive the OCR binarization
    // as black "ink"; the dark/transparent background must become white. The old MakeOneColor keyed
    // only on the blue channel, so yellow/red text (blue ≈ 0) was blanked entirely.
    [Fact]
    public void MakeBlackAndWhiteForOcr_KeepsColouredTextAsBlack()
    {
        using var bmp = new SKBitmap(new SKImageInfo(5, 1, SKColorType.Bgra8888, SKAlphaType.Unpremul));
        bmp.SetPixel(0, 0, new SKColor(255, 255, 255)); // white text
        bmp.SetPixel(1, 0, new SKColor(255, 255, 0));   // yellow text
        bmp.SetPixel(2, 0, new SKColor(255, 0, 0));     // red text
        bmp.SetPixel(3, 0, new SKColor(0, 0, 0));       // black background
        bmp.SetPixel(4, 0, new SKColor(0, 0, 0, 0));    // transparent background

        var nbmp = new NikseBitmap(bmp);
        nbmp.MakeBlackAndWhiteForOcr();

        Assert.True(IsBlack(nbmp.GetPixel(0, 0)), "white text should become black ink");
        Assert.True(IsBlack(nbmp.GetPixel(1, 0)), "yellow text should become black ink");
        Assert.True(IsBlack(nbmp.GetPixel(2, 0)), "red text should become black ink");
        Assert.True(IsWhite(nbmp.GetPixel(3, 0)), "black background should become white");
        Assert.True(IsWhite(nbmp.GetPixel(4, 0)), "transparent background should become white");
    }

    private static bool IsBlack(SKColor c) => c.Red < 64 && c.Green < 64 && c.Blue < 64;

    private static bool IsWhite(SKColor c) => c.Red > 192 && c.Green > 192 && c.Blue > 192;
}
