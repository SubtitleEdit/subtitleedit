using Nikse.SubtitleEdit.UiLogic.Ocr;
using SkiaSharp;

namespace LibUiLogicTests.Ocr;

/// <summary>
/// The whole-buffer scans and pixel writes on <see cref="NikseBitmap2"/> run a vector pass with
/// a scalar tail. These tests pin every one of them to the plain byte-at-a-time implementation
/// they replaced, over image widths that land on, before and after a vector boundary (and the
/// 1-pixel and 1-row degenerate cases), so a wrong tail or a wrong lane mask fails here rather
/// than showing up as a subtly mis-cropped OCR glyph.
/// </summary>
public class NikseBitmap2VectorPathTests
{
    public static TheoryData<int, int, int> Sizes()
    {
        var data = new TheoryData<int, int, int>();
        var seed = 1;
        foreach (var width in new[] { 1, 2, 3, 4, 5, 7, 8, 9, 15, 16, 17, 31, 33, 64, 137 })
        {
            foreach (var height in new[] { 1, 2, 5, 17 })
            {
                data.Add(width, height, seed++);
            }
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(Sizes))]
    public void TransparencyScans_MatchScalarReference(int width, int height, int seed)
    {
        foreach (var pixels in Images(width, height, seed))
        {
            var bmp = new NikseBitmap2(width, height, (byte[])pixels.Clone());

            Assert.Equal(RefIsImageOnlyTransparent(pixels), bmp.IsImageOnlyTransparent());
            Assert.Equal(RefCalcBottomTransparent(pixels, width, height), bmp.CalcBottomTransparent());

            for (var y = 0; y < height; y++)
            {
                Assert.Equal(RefRowAlphaAtMost(pixels, width, y, 0), bmp.IsLineTransparent(y));
                Assert.Equal(RefRowAlphaAtMost(pixels, width, y, 1), bmp.IsHorizontalLineTransparent(y));
            }
        }
    }

    [Theory]
    [MemberData(nameof(Sizes))]
    public void CropTopTransparent_MatchesScalarReference(int width, int height, int seed)
    {
        foreach (var pixels in Images(width, height, seed))
        {
            foreach (var margin in new[] { 0, 1, 3 })
            {
                var expectedTop = RefCropTopTransparent(pixels, width, height, margin, out var expectedPixels, out var expectedHeight);
                var bmp = new NikseBitmap2(width, height, (byte[])pixels.Clone());
                Assert.Equal(expectedTop, bmp.CropTopTransparent(margin));
                Assert.Equal(expectedHeight, bmp.Height);
                Assert.Equal(expectedPixels, bmp.GetPixelData().ToArray());
            }
        }
    }

    [Theory]
    [MemberData(nameof(Sizes))]
    public void CropTransparentSides_MatchesScalarReference(int width, int height, int seed)
    {
        foreach (var pixels in Images(width, height, seed))
        {
            RefOpaqueSides(pixels, width, height, out var left, out var right);
            var cropped = new NikseBitmap2(width, height, (byte[])pixels.Clone()).CropTransparentSides();

            if (left < 0 || (left == 0 && right == width - 1))
            {
                Assert.Equal(width, cropped.Width);
                Assert.Equal(pixels, cropped.GetPixelData().ToArray());
                continue;
            }

            Assert.Equal(right - left + 1, cropped.Width);
            Assert.Equal(height, cropped.Height);
            var expected = new NikseBitmap2(width, height, (byte[])pixels.Clone())
                .CopyRectangle(new NikseRectangle(left, 0, right - left + 1, height));
            Assert.Equal(expected.GetPixelData().ToArray(), cropped.GetPixelData().ToArray());
        }
    }

    [Theory]
    [MemberData(nameof(Sizes))]
    public void PixelWrites_MatchScalarReference(int width, int height, int seed)
    {
        var color = new SKColor(11, 200, 77, 190);
        var background = new SKColor(1, 2, 3);
        var foreground = new SKColor(250, 240, 230);

        foreach (var pixels in Images(width, height, seed))
        {
            AssertSameWrite(pixels, width, height, RefFill, b => b.Fill(color));
            AssertSameWrite(pixels, width, height, RefInvertColors, b => b.InvertColors());
            AssertSameWrite(pixels, width, height, RefReplaceTransparentWith, b => b.ReplaceTransparentWith(color));
            AssertSameWrite(pixels, width, height, RefMakeOneColor, b => b.MakeOneColor(color));
        }

        foreach (var pixels in Images(width, height, seed))
        {
            foreach (var minRgb in new[] { 0, 1, 30, 300, 600, 765, 766 })
            {
                AssertSameWrite(pixels, width, height, p => RefMakeTwoColor(p, minRgb, 0, 0, 0, 0, 255, 255, 255, 255), b => b.MakeTwoColor(minRgb));
                AssertSameWrite(pixels, width, height,
                    p => RefMakeTwoColor(p, minRgb, background.Blue, background.Green, background.Red, 255, foreground.Blue, foreground.Green, foreground.Red, 255),
                    b => b.MakeTwoColor(minRgb, background, foreground));
            }
        }

        return;

        void AssertSameWrite(byte[] pixels, int w, int h, Action<byte[]> reference, Action<NikseBitmap2> candidate)
        {
            var expected = (byte[])pixels.Clone();
            reference(expected);
            var bmp = new NikseBitmap2(w, h, (byte[])pixels.Clone());
            candidate(bmp);
            Assert.Equal(expected, bmp.GetPixelData().ToArray());
        }

        void RefFill(byte[] data)
        {
            for (var i = 0; i < data.Length; i += 4)
            {
                data[i] = color.Blue;
                data[i + 1] = color.Green;
                data[i + 2] = color.Red;
                data[i + 3] = color.Alpha;
            }
        }

        void RefReplaceTransparentWith(byte[] data)
        {
            for (var i = 0; i < data.Length; i += 4)
            {
                if (data[i + 3] < 10)
                {
                    data[i] = color.Blue;
                    data[i + 1] = color.Green;
                    data[i + 2] = color.Red;
                    data[i + 3] = color.Alpha;
                }
            }
        }

        void RefMakeOneColor(byte[] data)
        {
            for (var i = 0; i < data.Length; i += 4)
            {
                if (data[i] > 20)
                {
                    data[i] = color.Blue;
                    data[i + 1] = color.Green;
                    data[i + 2] = color.Red;
                    data[i + 3] = color.Alpha;
                }
                else
                {
                    data[i] = 0;
                    data[i + 1] = 0;
                    data[i + 2] = 0;
                    data[i + 3] = 0;
                }
            }
        }
    }

    private static void RefInvertColors(byte[] data)
    {
        for (var i = 0; i < data.Length; i += 4)
        {
            data[i] = (byte)~data[i];
            data[i + 1] = (byte)~data[i + 1];
            data[i + 2] = (byte)~data[i + 2];
        }
    }

    private static void RefMakeTwoColor(byte[] data, int minRgb, byte bgB, byte bgG, byte bgR, byte bgA, byte fgB, byte fgG, byte fgR, byte fgA)
    {
        for (var i = 0; i < data.Length; i += 4)
        {
            if (data[i + 3] < 1 || data[i] + data[i + 1] + data[i + 2] < minRgb)
            {
                data[i] = bgB;
                data[i + 1] = bgG;
                data[i + 2] = bgR;
                data[i + 3] = bgA;
            }
            else
            {
                data[i] = fgB;
                data[i + 1] = fgG;
                data[i + 2] = fgR;
                data[i + 3] = fgA;
            }
        }
    }

    private static bool RefIsImageOnlyTransparent(byte[] data)
    {
        for (var i = 0; i < data.Length; i += 4)
        {
            if (data[i + 3] != 0)
            {
                return false;
            }
        }

        return true;
    }

    private static bool RefRowAlphaAtMost(byte[] data, int width, int y, byte threshold)
    {
        var widthX4 = width * 4;
        for (var pos = y * widthX4 + 3; pos < y * widthX4 + widthX4; pos += 4)
        {
            if (data[pos] > threshold)
            {
                return false;
            }
        }

        return true;
    }

    private static int RefCalcBottomTransparent(byte[] data, int width, int height)
    {
        var y = height - 1;
        for (; y > 0; y--)
        {
            if (!RefRowAlphaAtMost(data, width, y, 1))
            {
                break;
            }
        }

        return height - y;
    }

    private static int RefCropTopTransparent(byte[] data, int width, int height, int minimumMargin, out byte[] result, out int newHeightOut)
    {
        var widthX4 = width * 4;
        var done = false;
        var newTop = 0;
        var y = 0;
        while (!done && y < height)
        {
            var x = 0;
            while (!done && x < width)
            {
                if (data[x * 4 + y * widthX4 + 3] > 10)
                {
                    done = true;
                    newTop = y - minimumMargin;
                    if (newTop < 0)
                    {
                        newTop = 0;
                    }
                }

                x++;
            }

            y++;
        }

        if (newTop == 0)
        {
            result = (byte[])data.Clone();
            newHeightOut = height;
            return 0;
        }

        newHeightOut = height - newTop;
        result = new byte[newHeightOut * widthX4];
        Buffer.BlockCopy(data, newTop * widthX4, result, 0, result.Length);
        return newTop;
    }

    private static void RefOpaqueSides(byte[] data, int width, int height, out int left, out int right)
    {
        var widthX4 = width * 4;
        left = -1;
        right = -1;
        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                if (data[x * 4 + y * widthX4 + 3] != 0)
                {
                    if (left < 0)
                    {
                        left = x;
                    }

                    right = x;
                    break;
                }
            }
        }
    }

    /// <summary>
    /// A battery per size: fully transparent, fully opaque, opaque only in the first / last
    /// column, opaque only in the first / last row, alpha exactly on the 1 and 10 thresholds,
    /// and random noise.
    /// </summary>
    private static IEnumerable<byte[]> Images(int width, int height, int seed)
    {
        var count = width * height * 4;

        yield return new byte[count];

        var opaque = new byte[count];
        Array.Fill(opaque, (byte)255);
        yield return opaque;

        foreach (var alpha in new byte[] { 1, 2, 10, 11, 255 })
        {
            var firstColumn = new byte[count];
            var lastColumn = new byte[count];
            var firstRow = new byte[count];
            var lastRow = new byte[count];
            for (var y = 0; y < height; y++)
            {
                firstColumn[y * width * 4 + 3] = alpha;
                lastColumn[y * width * 4 + (width - 1) * 4 + 3] = alpha;
            }

            for (var x = 0; x < width; x++)
            {
                firstRow[x * 4 + 3] = alpha;
                lastRow[(height - 1) * width * 4 + x * 4 + 3] = alpha;
            }

            yield return firstColumn;
            yield return lastColumn;
            yield return firstRow;
            yield return lastRow;
        }

        var rnd = new Random(seed);
        for (var i = 0; i < 3; i++)
        {
            var noise = new byte[count];
            rnd.NextBytes(noise);
            yield return noise;
        }

        // Sparse: mostly transparent with a few opaque pixels, like a real glyph row.
        var sparse = new byte[count];
        for (var i = 0; i < count / 4; i++)
        {
            if (rnd.Next(6) == 0)
            {
                sparse[i * 4] = (byte)rnd.Next(256);
                sparse[i * 4 + 1] = (byte)rnd.Next(256);
                sparse[i * 4 + 2] = (byte)rnd.Next(256);
                sparse[i * 4 + 3] = (byte)rnd.Next(1, 256);
            }
        }

        yield return sparse;
    }
}
