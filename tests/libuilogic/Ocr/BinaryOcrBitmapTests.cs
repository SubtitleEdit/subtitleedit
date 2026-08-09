using Nikse.SubtitleEdit.UiLogic.Ocr;

namespace LibUiLogicTests.Ocr;

public class BinaryOcrBitmapTests
{
    private static BinaryOcrBitmap MakeFilled(int width, int height, int y, int skipRow = -1)
    {
        var bmp = new BinaryOcrBitmap(width, height) { Y = y };
        var colored = 0;
        for (var yy = 0; yy < height; yy++)
        {
            if (yy == skipRow)
            {
                continue;
            }

            for (var xx = 0; xx < width; xx++)
            {
                bmp.SetPixel(xx, yy);
                colored++;
            }
        }

        bmp.NumberOfColoredPixels = colored;
        return bmp;
    }

    /// <summary>
    /// The shape predicates cache row/column occupancy scans; SetPixel after a
    /// predicate call must invalidate that cache so results stay correct.
    /// </summary>
    [Fact]
    public void IsLowercaseL_SetPixelAfterPredicate_InvalidatesCache()
    {
        // 5x25 with one transparent row fails IsLowercaseL.
        var bmp = MakeFilled(5, 25, 10, skipRow: 12);
        Assert.False(bmp.IsLowercaseL()); // warm the cache

        // Fill the transparent row; the predicate must see the new pixels.
        for (var x = 0; x < bmp.Width; x++)
        {
            bmp.SetPixel(x, 12);
        }

        bmp.NumberOfColoredPixels = 5 * 25;
        Assert.True(bmp.IsLowercaseL());
    }

    [Fact]
    public void IsLowercaseL_ColorsReplacedAfterPredicate_InvalidatesCache()
    {
        var bmp = MakeFilled(5, 25, 10);
        Assert.True(bmp.IsLowercaseL()); // warm the cache

        // Replace pixel data with an all-transparent buffer; cached rows must not survive.
        bmp.Colors = new byte[5 * 25];
        bmp.NumberOfColoredPixels = 0;
        Assert.False(bmp.IsLowercaseL());
    }

    [Fact]
    public void IsDash_WarmCache_ReturnsSameResult()
    {
        // 12x5 fully filled at Y=15 satisfies IsDash on both cold and warm cache paths.
        var bmp = MakeFilled(12, 5, 15);
        Assert.True(bmp.IsDash());
        Assert.True(bmp.IsDash());
    }
}
