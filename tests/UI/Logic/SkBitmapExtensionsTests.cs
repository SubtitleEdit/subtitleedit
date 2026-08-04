using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Logic;
using SkiaSharp;
using System.Runtime.InteropServices;

namespace UITests.Logic;

public class SkBitmapExtensionsTests
{
    // An 8-bit grayscale source, as InpaintDelogo emits (issue #12694): left half black,
    // right half white.
    private static SKBitmap MakeGray8Bitmap(int width = 32, int height = 8)
    {
        var bitmap = new SKBitmap(new SKImageInfo(width, height, SKColorType.Gray8, SKAlphaType.Opaque));
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                bitmap.SetPixel(x, y, x < width / 2 ? SKColors.Black : SKColors.White);
            }
        }

        return bitmap;
    }

    [Fact]
    public void DecodeToBgra8888NormalizesGrayscalePng()
    {
        using var gray = MakeGray8Bitmap();
        using var image = SKImage.FromBitmap(gray);
        using var png = image.Encode(SKEncodedImageFormat.Png, 100);

        // Sanity: a plain decode keeps the file's 1-byte/pixel format - the trap the
        // helper exists to avoid.
        using (var plain = SKBitmap.Decode(png.ToArray()))
        {
            Assert.Equal(SKColorType.Gray8, plain.ColorType);
        }

        using var decoded = SkBitmapExtensions.DecodeToBgra8888(png.ToArray());

        Assert.Equal(SKColorType.Bgra8888, decoded.ColorType);
        Assert.Equal(32, decoded.Width);
        Assert.Equal(8, decoded.Height);
        Assert.Equal((byte)0, decoded.GetPixel(0, 0).Red);
        Assert.Equal((byte)255, decoded.GetPixel(31, 0).Red);
    }

    [Fact]
    public void ImportImageItemDecodesRealInpaintDelogoImage()
    {
        // Real grayscale PNG from issue #12694's sample zip - the InpaintDelogo naming
        // carries the timecodes, and the 8-bit grayscale payload used to decode to
        // Gray8 and render as noise.
        var path = Path.Combine(AppContext.BaseDirectory, "Files", "00_02_09_640__00_02_16_920.png");
        var item = new Nikse.SubtitleEdit.Features.Files.ImportImages.ImportImageItem(path);

        Assert.Equal(TimeSpan.Parse("00:02:09.640"), item.Start);
        Assert.Equal(TimeSpan.Parse("00:02:16.920"), item.End);

        using var bitmap = item.GetBitmap();
        Assert.Equal(SKColorType.Bgra8888, bitmap.ColorType);
        Assert.Equal(1120, bitmap.Width);
        Assert.Equal(140, bitmap.Height);

        // The image is white text on black: both extremes must be present, and every
        // pixel gray (R==G==B) - garbled stride reads produced colored noise instead.
        var sawBlack = false;
        var sawWhite = false;
        for (var y = 0; y < bitmap.Height; y += 4)
        {
            for (var x = 0; x < bitmap.Width; x += 4)
            {
                var pixel = bitmap.GetPixel(x, y);
                Assert.Equal(pixel.Red, pixel.Green);
                Assert.Equal(pixel.Red, pixel.Blue);
                sawBlack |= pixel.Red < 32;
                sawWhite |= pixel.Red > 224;
            }
        }

        Assert.True(sawBlack, "expected black background pixels");
        Assert.True(sawWhite, "expected white text pixels");
    }

    [AvaloniaFact]
    public void ToAvaloniaBitmapConvertsGray8WithoutGarbling()
    {
        using var gray = MakeGray8Bitmap();

        var avaloniaBitmap = gray.ToAvaloniaBitmap();

        Assert.Equal(32, avaloniaBitmap.PixelSize.Width);
        Assert.Equal(8, avaloniaBitmap.PixelSize.Height);

        // Read back the BGRA pixels: left half black, right half white - the old
        // 4-bytes/pixel walk over a 1-byte/pixel buffer produced neither.
        var buffer = new byte[32 * 8 * 4];
        var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
        try
        {
            avaloniaBitmap.CopyPixels(
                new Avalonia.PixelRect(0, 0, 32, 8),
                handle.AddrOfPinnedObject(),
                buffer.Length,
                32 * 4);
        }
        finally
        {
            handle.Free();
        }

        Assert.Equal(0, buffer[0]); // first pixel blue channel: black
        Assert.Equal(255, buffer[3]); // first pixel alpha: opaque
        Assert.Equal(255, buffer[31 * 4]); // last pixel of row 0, blue channel: white
    }

    // A premultiplied half-transparent white pixel, i.e. what ToSkBitmap hands the
    // Binary edit image tools: straight (255,255,255,128) is stored as (128,128,128,128).
    private static SKBitmap MakePremultipliedHalfTransparentWhite(int width = 1, int height = 1)
    {
        var bitmap = new SKBitmap(new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul));
        var bytes = new byte[width * height * 4];
        for (var i = 0; i < width * height; i++)
        {
            bytes[i * 4] = 128; // blue
            bytes[i * 4 + 1] = 128; // green
            bytes[i * 4 + 2] = 128; // red
            bytes[i * 4 + 3] = 128; // alpha
        }

        Marshal.Copy(bytes, 0, bitmap.GetPixels(), bytes.Length);
        return bitmap;
    }

    [Fact]
    public void ToUnpremultipliedRecoversStraightColor()
    {
        using var premultiplied = MakePremultipliedHalfTransparentWhite();

        using var straight = premultiplied.ToUnpremultiplied();

        Assert.Equal(SKAlphaType.Unpremul, straight.AlphaType);
        Assert.Equal(SKColorType.Bgra8888, straight.ColorType);

        var pixel = straight.GetPixel(0, 0);
        Assert.Equal((byte)255, pixel.Red);
        Assert.Equal((byte)255, pixel.Green);
        Assert.Equal((byte)255, pixel.Blue);
        Assert.Equal((byte)128, pixel.Alpha);
    }

    // The image tools index pixels as y * Width + x, which assumes tightly packed rows.
    [Theory]
    [InlineData(1)]
    [InlineData(7)]
    [InlineData(401)]
    [InlineData(911)]
    public void ToUnpremultipliedKeepsRowsTightlyPacked(int width)
    {
        using var premultiplied = MakePremultipliedHalfTransparentWhite(width, 3);

        using var straight = premultiplied.ToUnpremultiplied();

        Assert.Equal(width * 4, straight.RowBytes);
        Assert.Equal((byte)255, straight.GetPixel(width - 1, 2).Red);
    }

    [Fact]
    public void ToUnpremultipliedHandlesEmptyBitmap()
    {
        using var empty = new SKBitmap(0, 0);

        using var straight = empty.ToUnpremultiplied();

        Assert.NotNull(straight);
    }

    // Lowering alpha in straight space and letting ToAvaloniaBitmap premultiply again must
    // keep R,G,B <= A. Editing the premultiplied bytes directly left RGB at 128 with A at 64,
    // which Skia renders as clipped, over-bright edges.
    [AvaloniaFact]
    public void AlphaEditRoundTripStaysValidPremultiplied()
    {
        using var premultiplied = MakePremultipliedHalfTransparentWhite();
        using var straight = premultiplied.ToUnpremultiplied();

        // What the image tools do: change alpha only, colour is left alone.
        using var edited = new SKBitmap(new SKImageInfo(1, 1, SKColorType.Bgra8888, SKAlphaType.Unpremul));
        var source = straight.GetPixel(0, 0);
        Marshal.Copy(new[] { (byte)source.Blue, (byte)source.Green, (byte)source.Red, (byte)64 }, 0, edited.GetPixels(), 4);

        var avaloniaBitmap = edited.ToAvaloniaBitmap();

        var buffer = new byte[4];
        var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
        try
        {
            avaloniaBitmap.CopyPixels(new Avalonia.PixelRect(0, 0, 1, 1), handle.AddrOfPinnedObject(), buffer.Length, 4);
        }
        finally
        {
            handle.Free();
        }

        var alpha = buffer[3];
        Assert.Equal(64, alpha);
        Assert.True(buffer[0] <= alpha, $"blue {buffer[0]} exceeds alpha {alpha}");
        Assert.True(buffer[1] <= alpha, $"green {buffer[1]} exceeds alpha {alpha}");
        Assert.True(buffer[2] <= alpha, $"red {buffer[2]} exceeds alpha {alpha}");
    }
}
