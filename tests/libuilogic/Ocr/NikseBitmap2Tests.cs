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
}
