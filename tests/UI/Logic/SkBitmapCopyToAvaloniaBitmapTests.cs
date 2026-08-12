using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Nikse.SubtitleEdit.Logic;
using SkiaSharp;

namespace UITests.Logic;

/// <summary>
/// <c>CopyToAvaloniaBitmap</c> is <c>ToAvaloniaBitmap</c> writing into a bitmap the caller already
/// owns, so the waveform's spectrogram block cache can reuse one surface instead of allocating a
/// megabyte-sized bitmap per frame. Both go through the same pixel writer, and these pin that down
/// over the shapes that reach it: premultiplied and straight alpha, fully opaque and fully
/// transparent pixels, and a colour type that has to be converted first.
/// </summary>
public class SkBitmapCopyToAvaloniaBitmapTests
{
    private const int Width = 17; // deliberately not a multiple of anything
    private const int Height = 5;

    [AvaloniaTheory]
    [InlineData(SKColorType.Bgra8888, SKAlphaType.Premul)]
    [InlineData(SKColorType.Bgra8888, SKAlphaType.Unpremul)]
    [InlineData(SKColorType.Rgba8888, SKAlphaType.Premul)]
    [InlineData(SKColorType.Rgba8888, SKAlphaType.Unpremul)]
    public void CopyToAvaloniaBitmap_MatchesToAvaloniaBitmap(SKColorType colorType, SKAlphaType alphaType)
    {
        using var source = MakeSource(colorType, alphaType);

        using var expected = (WriteableBitmap)source.ToAvaloniaBitmap();

        using var actual = new WriteableBitmap(
            new PixelSize(Width, Height), new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Premul);
        source.CopyToAvaloniaBitmap(actual);

        Assert.Equal(ReadPixels(expected), ReadPixels(actual));
    }

    [AvaloniaFact]
    public void CopyToAvaloniaBitmap_LeavesTargetAloneOnSizeMismatch()
    {
        using var source = MakeSource(SKColorType.Bgra8888, SKAlphaType.Premul);

        using var target = new WriteableBitmap(
            new PixelSize(Width + 1, Height), new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Premul);
        var before = ReadPixels(target);

        source.CopyToAvaloniaBitmap(target);

        Assert.Equal(before, ReadPixels(target));
    }

    [AvaloniaFact]
    public void CopyToAvaloniaBitmap_OverwritesEveryPixelOnReuse()
    {
        // The spectrogram cache reuses one surface across rebuilds, so a second copy must not
        // leave any pixel of the first behind.
        using var first = MakeSource(SKColorType.Bgra8888, SKAlphaType.Premul);
        using var second = MakeSource(SKColorType.Bgra8888, SKAlphaType.Premul, seed: 91);

        using var target = new WriteableBitmap(
            new PixelSize(Width, Height), new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Premul);

        first.CopyToAvaloniaBitmap(target);
        second.CopyToAvaloniaBitmap(target);

        using var expected = (WriteableBitmap)second.ToAvaloniaBitmap();
        Assert.Equal(ReadPixels(expected), ReadPixels(target));
    }

    /// <summary>
    /// A deterministic pattern that covers alpha 255 (the opaque fast path), alpha 0 (which the
    /// writer zeroes out) and the partial alphas in between.
    /// </summary>
    private static SKBitmap MakeSource(SKColorType colorType, SKAlphaType alphaType, int seed = 7)
    {
        var bitmap = new SKBitmap(Width, Height, colorType, alphaType);
        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                var i = y * Width + x + seed;
                var alpha = (i % 4) switch
                {
                    0 => (byte)255,
                    1 => (byte)0,
                    2 => (byte)128,
                    _ => (byte)37,
                };

                // Keep the channels below the alpha so the premultiplied inputs stay valid.
                var scale = alpha / 255.0;
                bitmap.SetPixel(x, y, new SKColor(
                    (byte)(i * 7 % 256 * scale),
                    (byte)(i * 13 % 256 * scale),
                    (byte)(i * 29 % 256 * scale),
                    alpha));
            }
        }

        return bitmap;
    }

    private static byte[] ReadPixels(WriteableBitmap bitmap)
    {
        using var locked = bitmap.Lock();
        var bytes = new byte[locked.RowBytes * bitmap.PixelSize.Height];
        System.Runtime.InteropServices.Marshal.Copy(locked.Address, bytes, 0, bytes.Length);
        return bytes;
    }
}
