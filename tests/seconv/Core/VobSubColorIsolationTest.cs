using Nikse.SubtitleEdit.Core.Common;
using SkiaSharp;
using Xunit;

namespace SeConvTests.Core;

/// <summary>
/// Histogram-based colour isolation for VobSub OCR bitmaps (issue #12293): the dominant
/// opaque colour (glyph fill) must survive as black, every other opaque colour (outline /
/// anti-alias tiers) must collapse to the white background, and the result is always an
/// opaque black-on-white bitmap.
/// </summary>
public class VobSubColorIsolationTest
{
    private static SKBitmap MakeBitmap(int width, int height, Func<int, int, SKColor> pixel)
    {
        var bmp = new SKBitmap(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Unpremul));
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                bmp.SetPixel(x, y, pixel(x, y));
            }
        }
        return bmp;
    }

    [Fact]
    public void Isolate_KeepsDominantFillAsBlack_DropsOutlineToWhite()
    {
        // A 10x10 frame: transparent background, a solid fill block (dominant opaque colour)
        // and a couple of "outline" pixels in a different opaque colour. The fill must become
        // black; the outline and the transparent background must become white.
        var fill = new SKColor(200, 30, 30);      // most frequent opaque colour
        var outline = new SKColor(40, 40, 40);    // less frequent opaque colour
        using var source = MakeBitmap(10, 10, (x, y) =>
        {
            if (x is >= 2 and <= 6 && y is >= 2 and <= 6)
            {
                return fill; // 25 pixels
            }
            if (y == 1 && x is >= 2 and <= 3)
            {
                return outline; // 2 pixels
            }
            return SKColors.Transparent;
        });

        using var result = VobSubColorIsolation.Isolate(source);

        Assert.Equal(SKAlphaType.Opaque, result.AlphaType);
        Assert.Equal(SKColors.Black, result.GetPixel(4, 4));   // inside the fill block
        Assert.Equal(SKColors.White, result.GetPixel(2, 1));   // an outline pixel
        Assert.Equal(SKColors.White, result.GetPixel(0, 0));   // transparent background
        Assert.Equal(255, result.GetPixel(4, 4).Alpha);        // fully opaque output
    }

    [Fact]
    public void Isolate_AllTransparent_ReturnsSolidWhite()
    {
        using var source = MakeBitmap(6, 4, (_, _) => SKColors.Transparent);

        using var result = VobSubColorIsolation.Isolate(source);

        Assert.Equal(SKAlphaType.Opaque, result.AlphaType);
        for (var y = 0; y < result.Height; y++)
        {
            for (var x = 0; x < result.Width; x++)
            {
                Assert.Equal(SKColors.White, result.GetPixel(x, y));
            }
        }
    }

    [Fact]
    public void Isolate_SingleOpaqueColor_BecomesBlackOnWhite()
    {
        // Only one opaque colour present (no outline) — it is the fill, so it becomes black.
        var text = new SKColor(255, 255, 255);
        using var source = MakeBitmap(5, 5, (x, y) => (x == 2 && y is >= 1 and <= 3) ? text : SKColors.Transparent);

        using var result = VobSubColorIsolation.Isolate(source);

        Assert.Equal(SKColors.Black, result.GetPixel(2, 2));
        Assert.Equal(SKColors.White, result.GetPixel(0, 0));
    }

    [Fact]
    public void Isolate_SemiTransparentBelowThreshold_TreatedAsBackground()
    {
        // A pixel whose alpha is below the threshold must be ignored (background), even if it
        // would otherwise be the most frequent colour.
        var faint = new SKColor(200, 30, 30, 64);  // alpha 64 < default threshold 128
        var solid = new SKColor(10, 220, 10, 255);
        using var source = MakeBitmap(8, 8, (x, y) =>
        {
            if (y < 6)
            {
                return faint; // many faint pixels, but below threshold
            }
            return (x < 2) ? solid : SKColors.Transparent; // few solid pixels
        });

        using var result = VobSubColorIsolation.Isolate(source);

        // Faint region → background (white); the only above-threshold colour is the fill (black).
        Assert.Equal(SKColors.White, result.GetPixel(4, 0));
        Assert.Equal(SKColors.Black, result.GetPixel(0, 7));
    }
}
