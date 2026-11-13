using Nikse.SubtitleEdit.Core.BluRaySup;
using SkiaSharp;
using System.Text;

namespace LibSETests.BluRaySup;

/// <summary>
/// A bitmap with more colours than a palette holds is reduced over the whole image (median
/// cut) - keeping the first colours met left the lower part of an image with none of its own.
/// </summary>
public class BluRaySupPicturePaletteQuantizerTests
{
    /// <summary>
    /// The top half has 400 colours far apart, the bottom half a smooth blue ramp.
    /// </summary>
    private static SKBitmap TopBusyBottomBlue()
    {
        var bitmap = new SKBitmap(new SKImageInfo(400, 40, SKColorType.Rgba8888, SKAlphaType.Unpremul));
        for (var x = 0; x < 400; x++)
        {
            var busy = new SKColor((byte)(x % 8 * 32), (byte)(x / 8 % 8 * 32 + 16), (byte)(x / 64 * 24), 255);
            var blue = new SKColor(0, 0, (byte)(55 + x / 2), 255);
            for (var y = 0; y < 20; y++)
            {
                bitmap.SetPixel(x, y, busy);
                bitmap.SetPixel(x, y + 20, blue);
            }
        }

        return bitmap;
    }

    [Fact]
    public void ManyColours_EveryPartOfTheImageKeepsItsColours()
    {
        using var bitmap = TopBusyBottomBlue();
        var picture = new BluRaySupPicture
        {
            StartTime = 1000,
            EndTime = 2000,
            Width = 1920,
            Height = 1080,
        };
        var sup = BluRaySupPicture.CreateSupFrame(picture, bitmap, SKColors.White, 25, 10, 10, BluRayContentAlignment.BottomCenter);
        using var stream = new MemoryStream(sup);
        var pcs = BluRaySupParser.ParseBluRaySup(stream, new StringBuilder(), false, new Dictionary<int, List<PaletteInfo>>(), new Dictionary<int, List<BluRaySupParser.OdsData>>())[0];
        using var decoded = pcs.GetBitmap();

        var worstBlue = 0;
        for (var x = 0; x < 400; x += 7)
        {
            var expected = bitmap.GetPixel(x, 30);
            var actual = decoded.GetPixel(x, 30);
            var difference = Math.Abs(expected.Red - actual.Red) + Math.Abs(expected.Green - actual.Green) + Math.Abs(expected.Blue - actual.Blue);
            worstBlue = Math.Max(worstBlue, difference);
        }

        Assert.InRange(worstBlue, 0, 16);
    }

    /// <summary>
    /// A colour sweep fading out towards its edge, like a glow or a soft shadow: thousands of
    /// colours, many of them faint.
    /// </summary>
    private static SKBitmap SoftColourSweep()
    {
        var bitmap = new SKBitmap(new SKImageInfo(300, 160, SKColorType.Rgba8888, SKAlphaType.Premul));
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.Transparent);
        using var sweep = new SKPaint
        {
            Shader = SKShader.CreateSweepGradient(new SKPoint(150, 80),
                new[] { SKColors.Red, SKColors.Yellow, SKColors.Lime, SKColors.Cyan, SKColors.Blue, SKColors.Magenta, SKColors.Red }),
        };
        canvas.DrawOval(new SKRect(5, 5, 295, 155), sweep);
        using var fade = new SKPaint
        {
            Shader = SKShader.CreateRadialGradient(new SKPoint(150, 80), 145,
                new[] { SKColors.Black.WithAlpha(0), SKColors.Black.WithAlpha(0), SKColors.Black },
                new[] { 0f, 0.6f, 1f }, SKShaderTileMode.Clamp),
            BlendMode = SKBlendMode.DstOut,
        };
        canvas.DrawRect(0, 0, 300, 160, fade);
        return bitmap;
    }

    /// <summary>
    /// The median cut leaves palette entries off-centre for gradients and glows, and a plain
    /// RGBA difference matched faint pixels by colours they barely show. With k-means passes
    /// and matching on premultiplied colours the picture over the video comes out closer.
    /// </summary>
    [Fact]
    public void SoftGradient_ComesOutCloseOverTheVideo()
    {
        using var bitmap = SoftColourSweep();
        var picture = new BluRaySupPicture
        {
            StartTime = 1000,
            EndTime = 2000,
            Width = 1920,
            Height = 1080,
        };
        var sup = BluRaySupPicture.CreateSupFrame(picture, bitmap, SKColors.White, 25, 10, 10, BluRayContentAlignment.BottomCenter);
        using var stream = new MemoryStream(sup);
        var pcs = BluRaySupParser.ParseBluRaySup(stream, new StringBuilder(), false, new Dictionary<int, List<PaletteInfo>>(), new Dictionary<int, List<BluRaySupParser.OdsData>>())[0];
        using var decoded = pcs.GetBitmap();

        // over black, what shows is each channel times alpha
        double sum = 0;
        var count = 0;
        for (var y = 0; y < bitmap.Height; y++)
        {
            for (var x = 0; x < bitmap.Width; x++)
            {
                var expected = bitmap.GetPixel(x, y);
                if (expected.Alpha == 0)
                {
                    continue;
                }

                var actual = decoded.GetPixel(x, y);
                foreach (var (e, a) in new[] { (expected.Red, actual.Red), (expected.Green, actual.Green), (expected.Blue, actual.Blue) })
                {
                    var difference = (e * expected.Alpha - a * actual.Alpha) / 255.0;
                    sum += difference * difference;
                    count++;
                }
            }
        }

        var rmse = Math.Sqrt(sum / count);
        // 6.0 before (median cut alone, RGBA matching), 3.9 with the k-means passes
        Assert.InRange(rmse, 0, 4.8);
    }
}
