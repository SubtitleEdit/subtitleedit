using Nikse.SubtitleEdit.Core.BluRaySup;
using SkiaSharp;

namespace LibSETests.BluRaySup;

/// <summary>
/// <see cref="SkiaExt.IsEqualTo(SKBitmap, SKBitmap)"/> compares the two pixmaps in place over a
/// window sized from the first bitmap's RowBytes, so the cases worth pinning are the ones where
/// the second buffer is not that big.
/// </summary>
public class SkiaExtEqualityTest
{
    private static SKBitmap MakeBitmap(int width, int height, SKColorType colorType = SKColorType.Bgra8888)
    {
        var bitmap = new SKBitmap(new SKImageInfo(width, height, colorType, SKAlphaType.Unpremul));
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                bitmap.SetPixel(x, y, new SKColor((byte)(x * 8), (byte)(y * 8), 128, 255));
            }
        }

        return bitmap;
    }

    [Fact]
    public void IsEqualToMatchesIdenticalBitmaps()
    {
        using var one = MakeBitmap(8, 4);
        using var other = MakeBitmap(8, 4);

        Assert.True(one.IsEqualTo(other));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(3, 2)]
    [InlineData(7, 3)] // the last pixel of the last row
    public void IsEqualToRejectsASingleDifferingPixel(int x, int y)
    {
        using var one = MakeBitmap(8, 4);
        using var other = MakeBitmap(8, 4);
        other.SetPixel(x, y, new SKColor(1, 2, 3, 255));

        Assert.False(one.IsEqualTo(other));
    }

    [Fact]
    public void IsEqualToRejectsDifferentDimensions()
    {
        using var one = MakeBitmap(8, 4);
        using var other = MakeBitmap(4, 8);

        Assert.False(one.IsEqualTo(other));
    }

    [Fact]
    public void IsEqualToMatchesTwoEmptyBitmaps()
    {
        using var one = new SKBitmap(0, 0);
        using var other = new SKBitmap(0, 0);

        Assert.True(one.IsEqualTo(other));
    }

    // Equal width and height but a narrower pixel format: RowBytes * Height for the first bitmap
    // is larger than the whole second buffer. Copying that many bytes out of the second pixmap
    // used to read past the end of its native allocation.
    [Fact]
    public void IsEqualToRejectsBitmapWithNarrowerPixelFormat()
    {
        using var one = MakeBitmap(8, 4);
        using var other = MakeBitmap(8, 4, SKColorType.Rgb565);

        Assert.False(one.IsEqualTo(other));
    }
}
