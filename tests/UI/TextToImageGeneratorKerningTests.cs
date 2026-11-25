using Nikse.SubtitleEdit.Logic.Media;
using SkiaSharp;

namespace UITests;

public class TextToImageGeneratorKerningTests
{
    [Theory]
    [InlineData(1f)]
    [InlineData(4f)]
    public void ZeroKerningPreservesBackground(float cornerRadius)
    {
        using var bitmap = TextToImageGenerator.GenerateImage(
            "Subtitle", "Arial", 24f, false,
            SKColors.White, SKColors.Black, SKColors.Black, SKColors.Blue,
            0f, 0f, kerning: 0f, cornerRadius: cornerRadius);

        Assert.True(bitmap.Width > 0);
        Assert.True(bitmap.Height > 0);
        Assert.Equal(SKColors.Blue, bitmap.GetPixel(0, 0));
    }

    [Fact]
    public void ZeroKerningAndCornerRadiusReturnsImage()
    {
        using var bitmap = TextToImageGenerator.GenerateImage(
            "Subtitle", "Arial", 24f, false,
            SKColors.White, SKColors.Black, SKColors.Black, SKColors.Blue,
            0f, 0f, kerning: 0f, cornerRadius: 0f);

        Assert.True(bitmap.Width > 0);
        Assert.True(bitmap.Height > 0);
        Assert.Equal(SKColors.Blue, bitmap.GetPixel(0, 0));
    }
}
