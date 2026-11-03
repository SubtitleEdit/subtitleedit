using Nikse.SubtitleEdit.Core.BluRaySup;
using SkiaSharp;
using System.Text;

namespace LibSETests.BluRaySup;

/// <summary>
/// The SD formats (480 and 576 lines) have their palette in BT.601, 720 and up in BT.709 - a
/// PGS stream has no field for the colour space, so the video size decides it.
/// </summary>
public class BluRaySupPictureSdPaletteTests
{
    private const byte Pds = 0x14;

    private static byte[] WriteSolid(SKColor color, int width, int height)
    {
        using var bitmap = new SKBitmap(100, 40);
        using (var canvas = new SKCanvas(bitmap))
        {
            canvas.Clear(color);
        }

        var picture = new BluRaySupPicture
        {
            StartTime = 1000,
            EndTime = 2000,
            Width = width,
            Height = height,
        };
        return BluRaySupPicture.CreateSupFrame(picture, bitmap, color, 25, 10, 10, BluRayContentAlignment.BottomCenter);
    }

    /// <summary>
    /// Y, Cb, Cr of the opaque entry - the only other entry of a solid image is the transparent one.
    /// </summary>
    private static (int Y, int Cb, int Cr) WrittenColor(byte[] sup)
    {
        var position = 0;
        while (position + 13 <= sup.Length)
        {
            var size = (sup[position + 11] << 8) + sup[position + 12];
            if (sup[position + 10] == Pds)
            {
                for (var entry = position + 13 + 2; entry < position + 13 + size; entry += 5)
                {
                    if (sup[entry + 4] == 255)
                    {
                        return (sup[entry + 1], sup[entry + 3], sup[entry + 2]);
                    }
                }
            }

            position += 13 + size;
        }

        throw new InvalidOperationException("No opaque palette entry");
    }

    [Theory]
    [InlineData(720, 576, 81, 90, 240)]
    [InlineData(720, 480, 81, 90, 240)]
    [InlineData(1280, 720, 63, 102, 240)]
    [InlineData(1920, 1080, 63, 102, 240)]
    public void Red_UsesTheColourModelOfTheVideoSize(int width, int height, int y, int cb, int cr)
    {
        Assert.Equal((y, cb, cr), WrittenColor(WriteSolid(SKColors.Red, width, height)));
    }

    [Theory]
    [InlineData(720, 576)]
    [InlineData(1920, 1080)]
    public void RoundTrip_KeepsTheColour(int width, int height)
    {
        var color = new SKColor(0xCD, 0xA9, 0x65);
        using var stream = new MemoryStream(WriteSolid(color, width, height));
        var pcsList = BluRaySupParser.ParseBluRaySup(stream, new StringBuilder(), false, new Dictionary<int, List<PaletteInfo>>(), new Dictionary<int, List<BluRaySupParser.OdsData>>());

        using var bitmap = pcsList[0].GetBitmap();
        var pixel = bitmap.GetPixel(bitmap.Width / 2, bitmap.Height / 2);
        Assert.InRange(Math.Abs(pixel.Red - color.Red), 0, 2);
        Assert.InRange(Math.Abs(pixel.Green - color.Green), 0, 2);
        Assert.InRange(Math.Abs(pixel.Blue - color.Blue), 0, 2);
    }
}
