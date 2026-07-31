using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using SkiaSharp;
using Xunit;

namespace LibSETests.Common;

/// <summary>
/// ColorUtils.FromArgb used to have an extra overload taking (int alpha, byte blue, byte
/// green, byte red) - reversed. Byte arguments bound to it in preference to the (int alpha,
/// int red, int green, int blue) overload, so four call sites silently swapped red and blue:
/// VobSub subpicture decoding, .idx palettes written with alpha, SSA numeric colours and
/// TextST palettes. These tests pin the ARGB order down.
/// </summary>
public class ColorUtilsTest
{
    [Fact]
    public void FromArgb_ByteArguments_KeepArgbOrder()
    {
        // Explicit bytes - the argument types that used to pick the reversed overload
        byte r = 255;
        byte g = 128;
        byte b = 0;

        var color = ColorUtils.FromArgb(200, r, g, b);

        Assert.Equal(200, color.Alpha);
        Assert.Equal(255, color.Red);
        Assert.Equal(128, color.Green);
        Assert.Equal(0, color.Blue);
    }

    [Fact]
    public void GetSsaColor_DecimalNumber_IsBgrNotRgb()
    {
        // SSA numeric colours are decimal &HBBGGRR: 255 is red, 16711680 is blue
        var red = AdvancedSubStationAlpha.GetSsaColor("255", SKColors.Yellow);
        Assert.Equal(255, red.Red);
        Assert.Equal(0, red.Green);
        Assert.Equal(0, red.Blue);

        var blue = AdvancedSubStationAlpha.GetSsaColor("16711680", SKColors.Yellow);
        Assert.Equal(0, blue.Red);
        Assert.Equal(0, blue.Green);
        Assert.Equal(255, blue.Blue);
    }

    [Fact]
    public void TextStPalette_YCbCr_MapsToRgbNotBgr()
    {
        // Y=81, Cb=90, Cr=240 is red in studio-range BT.709
        var palette = new TextST.Palette { Y = 81, Cb = 90, Cr = 240, T = 255 };

        var color = palette.Color;

        Assert.True(color.Red > 200, $"expected a red-dominant colour, got {color}");
        Assert.True(color.Blue < 60, $"expected little blue, got {color}");
    }

    [Fact]
    public void IdxPalette_EightDigitHex_KeepsChannelOrder()
    {
        // "aarrggbb" - the .idx palette variant that carries alpha
        var idx = new Nikse.SubtitleEdit.Core.VobSub.Idx(new List<string> { "palette: 80ff8000, 000000" });

        Assert.Equal(2, idx.Palette.Count);
        Assert.Equal(0x80, idx.Palette[0].Alpha);
        Assert.Equal(255, idx.Palette[0].Red);
        Assert.Equal(0x80, idx.Palette[0].Green);
        Assert.Equal(0, idx.Palette[0].Blue);
    }
}
