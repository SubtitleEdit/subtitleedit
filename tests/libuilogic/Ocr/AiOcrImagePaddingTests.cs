using Nikse.SubtitleEdit.UiLogic.LlamaCpp;
using Nikse.SubtitleEdit.UiLogic.Ocr;
using SkiaSharp;

namespace LibUiLogicTests.Ocr;

// Vision-model OCR input: a black square for GLM-OCR, a thin border for HunyuanOCR and QARI.
public class AiOcrImagePaddingTests
{
    private static SKBitmap WhiteImage(int width, int height)
    {
        var bitmap = new SKBitmap(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul));
        bitmap.Erase(SKColors.White);
        return bitmap;
    }

    [Fact]
    public void Square_CentersWithTwentyPercentMargin()
    {
        using var source = WhiteImage(1280, 240);
        using var padded = AiOcrImagePadding.Pad(source, AiOcrPadding.Square);

        Assert.Equal(1536, padded.Width);
        Assert.Equal(1536, padded.Height);
        Assert.Equal(SKColors.Black, padded.GetPixel(0, 0));
        Assert.Equal(SKColors.White, padded.GetPixel(768, 768));
    }

    [Fact]
    public void Border_KeepsAspectWithTenPercentOfHeight()
    {
        using var source = WhiteImage(1280, 240);
        using var padded = AiOcrImagePadding.Pad(source, AiOcrPadding.Border);

        Assert.Equal(1328, padded.Width);
        Assert.Equal(288, padded.Height);
        Assert.Equal(SKColors.Black, padded.GetPixel(23, 144));
        Assert.Equal(SKColors.White, padded.GetPixel(24, 144));
        Assert.Equal(SKColors.White, padded.GetPixel(664, 24));
        Assert.Equal(SKColors.Black, padded.GetPixel(664, 23));
    }

    [Fact]
    public void Border_SmallImage_AtLeastTenPixels()
    {
        using var source = WhiteImage(89, 48);
        using var padded = AiOcrImagePadding.Pad(source, AiOcrPadding.Border);

        Assert.Equal(109, padded.Width);
        Assert.Equal(68, padded.Height);
    }

    [Theory]
    [InlineData("GLM-OCR-Q8_0", AiOcrPadding.Square)]
    [InlineData("HunyuanOCR-Q8_0", AiOcrPadding.Border)]
    [InlineData("qari-ocr-0.2.2.1-vl-2b-instruct-merged.q8_0", AiOcrPadding.Border)]
    [InlineData("glmocr", AiOcrPadding.Square)]
    [InlineData("my-own-model", AiOcrPadding.Square)]
    [InlineData("", AiOcrPadding.Square)]
    [InlineData(null, AiOcrPadding.Square)]
    public void GetOcrPadding_CuratedModelsOnly(string? modelName, AiOcrPadding expected)
    {
        Assert.Equal(expected, LlamaCppServerManager.GetOcrPadding(modelName));
    }
}
