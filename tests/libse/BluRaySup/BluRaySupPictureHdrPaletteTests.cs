using Nikse.SubtitleEdit.Core.BluRaySup;
using SkiaSharp;

namespace LibSETests.BluRaySup;

/// <summary>
/// The HDR palette of a UHD Blu-ray (issue #16550): BT.2020 + ST 2084 PQ, 8 bit, narrow range.
/// A PGS stream has no field for the colour space, so the palette values are the whole feature.
/// </summary>
public class BluRaySupPictureHdrPaletteTests
{
    private const byte Pds = 0x14;

    /// <summary>
    /// Y, Cb, Cr of the opaque entry holding <paramref name="color"/> - the only other entry of
    /// a solid image is the transparent one.
    /// </summary>
    private static (int Y, int Cb, int Cr) WrittenColor(SKColor color, int hdrWhiteNits)
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
            Width = 1920,
            Height = 1080,
            HdrWhiteNits = hdrWhiteNits,
        };
        var sup = BluRaySupPicture.CreateSupFrame(picture, bitmap, color, 25, 10, 10, BluRayContentAlignment.BottomCenter);

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
    [InlineData(0xFF, 0xFF, 0xFF, 203, 143, 128, 128)] // 58 % PQ, the BT.2408 graphics white
    [InlineData(0xFF, 0xFF, 0xFF, 500, 164, 128, 128)]
    [InlineData(0x80, 0x80, 0x80, 203, 107, 128, 128)]
    [InlineData(0x00, 0x00, 0x00, 203, 16, 128, 128)]
    [InlineData(0xCD, 0xA9, 0x65, 203, 122, 116, 132)] // the beige of the issue
    [InlineData(0xFF, 0x00, 0x00, 203, 98, 110, 152)]
    public void HdrPalette_IsBt2020Pq(byte r, byte g, byte b, int nits, int y, int cb, int cr)
    {
        Assert.Equal((y, cb, cr), WrittenColor(new SKColor(r, g, b), nits));
    }

    [Fact]
    public void NoNits_KeepsTheBt709Palette()
    {
        Assert.Equal((235, 128, 128), WrittenColor(SKColors.White, 0));
    }
}
