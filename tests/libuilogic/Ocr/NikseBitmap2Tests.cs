using Nikse.SubtitleEdit.UiLogic.Ocr;
using SkiaSharp;

namespace LibUiLogicTests.Ocr;

public class NikseBitmap2Tests
{
    [Fact]
    public void IsImageOnlyTransparent_EmptyBitmap_ReturnsTrue()
    {
        var nbmp = new NikseBitmap2(5, 5);
        Assert.True(nbmp.IsImageOnlyTransparent());
    }

    /// <summary>
    /// Regression: the check used to read the blue byte instead of alpha, so an opaque
    /// blue-less image (black, yellow, red, green text) was reported as fully transparent
    /// and its line parts got dropped by the OCR image splitter.
    /// </summary>
    [Fact]
    public void IsImageOnlyTransparent_OpaqueBlack_ReturnsFalse()
    {
        var nbmp = new NikseBitmap2(5, 5);
        for (var y = 0; y < nbmp.Height; y++)
        {
            for (var x = 0; x < nbmp.Width; x++)
            {
                nbmp.SetPixel(x, y, new SKColor(0, 0, 0, 255));
            }
        }

        Assert.False(nbmp.IsImageOnlyTransparent());
    }

    [Fact]
    public void IsImageOnlyTransparent_SingleVisiblePixel_ReturnsFalse()
    {
        var nbmp = new NikseBitmap2(5, 5);
        nbmp.SetPixel(3, 2, new SKColor(0, 0, 0, 1));
        Assert.False(nbmp.IsImageOnlyTransparent());
    }

    /// <summary>
    /// Regression: GetPixel read the BGRA bytes back as if they were ARGB, so a pixel did not
    /// round-trip through SetPixel - opaque yellow came back as transparent cyan, and the
    /// Alpha that nOCR's match points test was really the blue byte.
    /// </summary>
    [Theory]
    [InlineData(255, 255, 0, 255)] // opaque yellow
    [InlineData(255, 0, 0, 255)] // opaque red
    [InlineData(10, 20, 30, 128)] // semi-transparent
    [InlineData(255, 255, 255, 255)] // white
    [InlineData(0, 0, 0, 0)] // transparent
    public void GetPixel_ReturnsWhatSetPixelWrote(byte red, byte green, byte blue, byte alpha)
    {
        var color = new SKColor(red, green, blue, alpha);
        var nbmp = new NikseBitmap2(3, 2);
        nbmp.SetPixel(2, 1, color);

        Assert.Equal(color, nbmp.GetPixel(2, 1));
        Assert.Equal(alpha, nbmp.GetPixel(2, 1).Alpha);
    }

    [Fact]
    public void GetPixel_MatchesSkBitmapPixel()
    {
        using var skBitmap = new SKBitmap(2, 1, SKColorType.Bgra8888, SKAlphaType.Unpremul);
        skBitmap.SetPixel(1, 0, new SKColor(200, 100, 50, 255));

        var nbmp = new NikseBitmap2(skBitmap);

        Assert.Equal(new SKColor(200, 100, 50, 255), nbmp.GetPixel(1, 0));
    }
}
