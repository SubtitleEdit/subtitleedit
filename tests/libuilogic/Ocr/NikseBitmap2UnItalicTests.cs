using Nikse.SubtitleEdit.UiLogic.Ocr;

namespace LibUiLogicTests.Ocr;

public class NikseBitmap2UnItalicTests
{
    private static NikseBitmap2 MakeSlantedBar(int stemWidth, int height, double slant)
    {
        var maxShift = (int)(height * slant);
        var width = stemWidth + maxShift;
        var data = new byte[width * height * 4];
        for (var y = 0; y < height; y++)
        {
            var shift = (int)Math.Round((height - 1 - y) * slant); // top leans right
            for (var x = 0; x < stemWidth; x++)
            {
                var i = (x + shift + y * width) * 4;
                data[i] = data[i + 1] = data[i + 2] = data[i + 3] = 255;
            }
        }

        return new NikseBitmap2(width, height, data);
    }

    [Fact]
    public void UnItalic_StraightensASlantedBar()
    {
        var italic = MakeSlantedBar(3, 20, 0.25);
        Assert.Equal(8, italic.Width); // sanity: the slant widened the bounding box

        var straight = italic.UnItalic(0.25);

        // Every row's first opaque column must now line up (within 1px of shear rounding).
        var firstOpaque = new List<int>();
        for (var y = 0; y < straight.Height; y++)
        {
            for (var x = 0; x < straight.Width; x++)
            {
                if (straight.GetAlpha(x, y) != 0)
                {
                    firstOpaque.Add(x);
                    break;
                }
            }
        }

        Assert.Equal(straight.Height, firstOpaque.Count); // no row lost its pixels
        Assert.True(firstOpaque.Max() - firstOpaque.Min() <= 1, "bar is still slanted");
        Assert.True(straight.Width <= 4, $"transparent slack not cropped: width {straight.Width}");
    }

    [Fact]
    public void CropTransparentSides_RemovesOnlyColumns()
    {
        var bmp = new NikseBitmap2(10, 6);
        var data = new byte[10 * 6 * 4];
        // one opaque pixel at (4, 2)
        var idx = (4 + 2 * 10) * 4;
        data[idx] = data[idx + 1] = data[idx + 2] = data[idx + 3] = 255;
        bmp = new NikseBitmap2(10, 6, data);

        var cropped = bmp.CropTransparentSides();

        Assert.Equal(1, cropped.Width);
        Assert.Equal(6, cropped.Height); // rows untouched, vertical margin preserved
        Assert.NotEqual(0, cropped.GetAlpha(0, 2));
    }

    [Fact]
    public void CropTransparentSides_FullyTransparent_ReturnsSameSize()
    {
        var bmp = new NikseBitmap2(7, 5);
        var cropped = bmp.CropTransparentSides();

        Assert.Equal(7, cropped.Width);
        Assert.Equal(5, cropped.Height);
    }
}
