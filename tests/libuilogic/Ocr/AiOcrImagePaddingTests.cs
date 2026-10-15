using Nikse.SubtitleEdit.UiLogic.Ocr;
using SkiaSharp;

namespace LibUiLogicTests.Ocr;

// Vision-model OCR input: a black square with a 20% margin, optionally capped in size (#16043).
public class AiOcrImagePaddingTests
{
    private static SKBitmap WhiteImage(int width, int height)
    {
        var bitmap = new SKBitmap(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul));
        bitmap.Erase(SKColors.White);
        return bitmap;
    }

    [Fact]
    public void PadToSquare_NoCap_KeepsNativeSize()
    {
        using var source = WhiteImage(1280, 240);
        using var square = AiOcrImagePadding.PadToSquare(source, 0);

        Assert.Equal(1536, square.Width);
        Assert.Equal(1536, square.Height);
        Assert.Equal(SKColors.Black, square.GetPixel(0, 0));
        Assert.Equal(SKColors.White, square.GetPixel(768, 768));
    }

    [Fact]
    public void PadToSquare_Cap_ScalesDownKeepingTheMargin()
    {
        using var source = WhiteImage(1280, 240);
        using var square = AiOcrImagePadding.PadToSquare(source, 768);

        Assert.Equal(768, square.Width);
        Assert.Equal(768, square.Height);

        // 1280 of 1536 -> 640 of 768, centered: 64 px black either side.
        Assert.Equal(SKColors.Black, square.GetPixel(60, 384));
        Assert.Equal(SKColors.White, square.GetPixel(70, 384));
        Assert.Equal(SKColors.White, square.GetPixel(697, 384));
        Assert.Equal(SKColors.Black, square.GetPixel(707, 384));
    }

    [Fact]
    public void PadToSquare_CapLargerThanSquare_DoesNotUpscale()
    {
        using var source = WhiteImage(353, 48);
        using var square = AiOcrImagePadding.PadToSquare(source, 1024);

        Assert.Equal(423, square.Width);
        Assert.Equal(423, square.Height);
    }
}
