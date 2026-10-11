using Nikse.SubtitleEdit.UiLogic.Ocr;
using SkiaSharp;

namespace LibUiLogicTests.Ocr;

/// <summary>
/// "3D" subtitles carry a grey drop shadow that the fixed two-color threshold of 200 keeps as
/// ink, fusing it with the letters; the threshold switches to Otsu's value only then.
/// </summary>
public class OcrTwoColorThresholdTests
{
    private static NikseBitmap2 Make(SKColor fill, int fillPixels, SKColor edge, int edgePixels)
    {
        using var bitmap = new SKBitmap(100, 10);
        bitmap.Erase(SKColors.Transparent);
        var n = 0;
        for (var y = 0; y < 10; y++)
        {
            for (var x = 0; x < 100; x++, n++)
            {
                if (n < fillPixels)
                {
                    bitmap.SetPixel(x, y, fill);
                }
                else if (n < fillPixels + edgePixels)
                {
                    bitmap.SetPixel(x, y, edge);
                }
            }
        }

        return new NikseBitmap2(bitmap);
    }

    [Fact]
    public void WhiteTextWithBlackOutline_KeepsDefault()
    {
        var bitmap = Make(SKColors.White, 400, SKColors.Black, 300);

        Assert.Equal(OcrTwoColorThreshold.Default, OcrTwoColorThreshold.Get(bitmap));
    }

    [Fact]
    public void FewAntiAliasedGreyPixels_KeepsDefault()
    {
        var bitmap = Make(SKColors.White, 400, new SKColor(90, 90, 90), 30);

        Assert.Equal(OcrTwoColorThreshold.Default, OcrTwoColorThreshold.Get(bitmap));
    }

    [Fact]
    public void GreyDropShadow_IsThresholdedAway()
    {
        // Light grey fill with a large mid-grey (sum 240) shadow, which 200 would keep as ink.
        var bitmap = Make(new SKColor(220, 220, 220), 400, new SKColor(80, 80, 80), 300);

        var threshold = OcrTwoColorThreshold.Get(bitmap);

        Assert.InRange(threshold, 241, 660);
    }
}
