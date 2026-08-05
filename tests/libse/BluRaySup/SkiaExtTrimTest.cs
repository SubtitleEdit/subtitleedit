using Nikse.SubtitleEdit.Core.BluRaySup;
using SkiaSharp;

namespace LibSETests.BluRaySup;

/// <summary>
/// <see cref="SkiaExt.GetNonTransparentBounds"/> tests eight pixels per branch and stops at the
/// first hit in a row, so the interesting cases are the ones near the block boundary: widths that
/// are not a multiple of eight, content in the first and last pixel of a row, and rows that only
/// have content between bounds already found by an earlier row.
/// </summary>
public class SkiaExtTrimTest
{
    private static SKBitmap MakeBitmap(int width, int height, params (int X, int Y, byte Alpha)[] spots)
    {
        var bitmap = new SKBitmap(width, height);
        bitmap.Erase(SKColors.Transparent);
        foreach (var spot in spots)
        {
            bitmap.SetPixel(spot.X, spot.Y, new SKColor(200, 100, 50, spot.Alpha));
        }

        return bitmap;
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(7, 3)]   // shorter than one block
    [InlineData(8, 8)]   // exactly one block
    [InlineData(9, 5)]   // one block plus a remainder
    [InlineData(17, 17)]
    [InlineData(64, 33)]
    public void GetNonTransparentBounds_FindsSinglePixelAnywhere(int width, int height)
    {
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                using var bitmap = MakeBitmap(width, height, (x, y, 255));

                var bounds = bitmap.GetNonTransparentBounds();

                Assert.False(bounds.IsEmpty);
                Assert.Equal(x, bounds.Left);
                Assert.Equal(x, bounds.Right);
                Assert.Equal(y, bounds.Top);
                Assert.Equal(y, bounds.Bottom);
            }
        }
    }

    [Fact]
    public void GetNonTransparentBounds_AlphaOfOneCounts()
    {
        using var bitmap = MakeBitmap(20, 4, (13, 2, 1));

        var bounds = bitmap.GetNonTransparentBounds();

        Assert.Equal(13, bounds.Left);
        Assert.Equal(13, bounds.Right);
        Assert.Equal(2, bounds.Top);
        Assert.Equal(2, bounds.Bottom);
    }

    [Fact]
    public void GetNonTransparentBounds_FullyTransparentIsEmpty()
    {
        using var bitmap = MakeBitmap(33, 9);

        Assert.True(bitmap.GetNonTransparentBounds().IsEmpty);
    }

    // The right edge is only searched beyond the widest hit so far, so a later row whose content
    // sits inside the bounds found earlier must not narrow them.
    [Fact]
    public void GetNonTransparentBounds_LaterInnerRowDoesNotNarrowTheBounds()
    {
        using var bitmap = MakeBitmap(40, 6, (0, 1, 255), (39, 1, 255), (20, 4, 255));

        var bounds = bitmap.GetNonTransparentBounds();

        Assert.Equal(0, bounds.Left);
        Assert.Equal(39, bounds.Right);
        Assert.Equal(1, bounds.Top);
        Assert.Equal(4, bounds.Bottom);
    }

    [Fact]
    public void TrimTransparentPixels_CropsToTheDrawnPixels()
    {
        using var bitmap = MakeBitmap(30, 20, (5, 4, 255), (11, 9, 255));

        var result = bitmap.TrimTransparentPixels();

        using (result.TrimmedBitmap)
        {
            Assert.Equal(7, result.TrimmedBitmap.Width);
            Assert.Equal(6, result.TrimmedBitmap.Height);
            Assert.Equal(5, result.Left);
            Assert.Equal(4, result.Top);
            Assert.Equal(30 - 11 - 1, result.Right);   // distance from the right edge
            Assert.Equal(20 - 9 - 1, result.Bottom);   // distance from the bottom edge
            Assert.Equal(255, result.TrimmedBitmap.GetPixel(0, 0).Alpha);
            Assert.Equal(255, result.TrimmedBitmap.GetPixel(6, 5).Alpha);
            Assert.Equal(0, result.TrimmedBitmap.GetPixel(3, 3).Alpha);
        }
    }

    [Fact]
    public void TrimTransparentPixels_FullyTransparentKeepsTheOriginalSize()
    {
        using var bitmap = MakeBitmap(12, 7);

        var result = bitmap.TrimTransparentPixels();

        using (result.TrimmedBitmap)
        {
            Assert.Equal(12, result.TrimmedBitmap.Width);
            Assert.Equal(7, result.TrimmedBitmap.Height);
        }
    }

    [Fact]
    public void CropTo_KeepsTheRequestedRectangleAndPadsOutsideItTransparently()
    {
        using var bitmap = MakeBitmap(20, 10, (4, 3, 255));

        using var cropped = bitmap.CropTo(2, 1, 6, 5);

        Assert.Equal(5, cropped.Width);
        Assert.Equal(5, cropped.Height);
        Assert.Equal(255, cropped.GetPixel(2, 2).Alpha);
        Assert.Equal(0, cropped.GetPixel(0, 0).Alpha);
    }
}
