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
}
