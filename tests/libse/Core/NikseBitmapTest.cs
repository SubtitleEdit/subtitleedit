using Nikse.SubtitleEdit.Core.Common;
using SkiaSharp;

namespace LibSETests.Core;

public class NikseBitmapTest
{

    [Fact]
    public void NikseBitmapSetGetPixel()
    {
        var nbmp = new NikseBitmap(10, 11);
        var c1 = ColorUtils.FromArgb(0, 1, 2, 3);
        var c2 = ColorUtils.FromArgb(6, 7, 8, 9);
        for (int y = 0; y < nbmp.Height; y++)
        {
            for (int x = 0; x < nbmp.Width; x++)
            {
                if (x % 2 == 0)
                    nbmp.SetPixel(x, y, c1);
                else
                    nbmp.SetPixel(x, y, c2);
            }
        }
        for (int y = 0; y < nbmp.Height; y++)
        {
            for (int x = 0; x < nbmp.Width; x++)
            {
                var c = nbmp.GetPixel(x, y);
                if (x % 2 == 0)
                    Assert.Equal(c1, c);
                else
                    Assert.Equal(c2, c);
            }
        }
    }

    [Fact]
    public void IsImageOnlyTransparentEmptyBitmapReturnsTrue()
    {
        var nbmp = new NikseBitmap(5, 5);
        Assert.True(nbmp.IsImageOnlyTransparent());
    }

    [Fact]
    public void IsImageOnlyTransparentOpaqueBlackReturnsFalse()
    {
        // Regression: the check used to read the blue byte instead of alpha, so an
        // opaque all-black (or any blue-less) image was reported as fully transparent.
        var nbmp = new NikseBitmap(5, 5);
        for (int y = 0; y < nbmp.Height; y++)
        {
            for (int x = 0; x < nbmp.Width; x++)
            {
                nbmp.SetPixel(x, y, ColorUtils.FromArgb(255, 0, 0, 0));
            }
        }

        Assert.False(nbmp.IsImageOnlyTransparent());
    }

    [Fact]
    public void IsImageOnlyTransparentSingleVisiblePixelReturnsFalse()
    {
        var nbmp = new NikseBitmap(5, 5);
        nbmp.SetPixel(3, 2, ColorUtils.FromArgb(1, 0, 0, 0));
        Assert.False(nbmp.IsImageOnlyTransparent());
    }

    private static NikseBitmap MakeNumberedBitmap(int width, int height)
    {
        // Every pixel gets a unique opaque color derived from its coordinates.
        var nbmp = new NikseBitmap(width, height);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                nbmp.SetPixel(x, y, ColorUtils.FromArgb(255, x, y, 100));
            }
        }

        return nbmp;
    }

    [Fact]
    public void CopyRectangleOffsetRectCopiesCorrectPixels()
    {
        var source = MakeNumberedBitmap(10, 10);
        var copy = source.CopyRectangle(new SKRectI(2, 3, 7, 8));

        Assert.Equal(5, copy.Width);
        Assert.Equal(5, copy.Height);
        for (int y = 0; y < copy.Height; y++)
        {
            for (int x = 0; x < copy.Width; x++)
            {
                Assert.Equal(source.GetPixel(x + 2, y + 3), copy.GetPixel(x, y));
            }
        }
    }

    [Fact]
    public void CopyRectangleRectExceedingBoundsIsClamped()
    {
        var source = MakeNumberedBitmap(10, 10);
        var copy = source.CopyRectangle(new SKRectI(4, 5, 20, 20));

        Assert.Equal(6, copy.Width);
        Assert.Equal(5, copy.Height);
        for (int y = 0; y < copy.Height; y++)
        {
            for (int x = 0; x < copy.Width; x++)
            {
                Assert.Equal(source.GetPixel(x + 4, y + 5), copy.GetPixel(x, y));
            }
        }
    }

    [Fact]
    public void CopyRectangleSkRectOverloadMatchesSkRectI()
    {
        var source = MakeNumberedBitmap(10, 10);
        var copy = source.CopyRectangle(new SKRect(2, 3, 7, 8));

        Assert.Equal(5, copy.Width);
        Assert.Equal(5, copy.Height);
        for (int y = 0; y < copy.Height; y++)
        {
            for (int x = 0; x < copy.Width; x++)
            {
                Assert.Equal(source.GetPixel(x + 2, y + 3), copy.GetPixel(x, y));
            }
        }
    }
}
