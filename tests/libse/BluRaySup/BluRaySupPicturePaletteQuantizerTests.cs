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
}
