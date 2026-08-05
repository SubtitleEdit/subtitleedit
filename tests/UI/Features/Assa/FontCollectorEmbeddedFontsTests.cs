using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Features.Assa;

/// <summary>
/// Tests the [Fonts] footer parsing the font collector uses for embedded fonts.
/// The payload need not be a real font - the parser only splits and UU-decodes.
/// </summary>
public class FontCollectorEmbeddedFontsTests
{
    private static byte[] MakeBytes(int count, byte seed)
    {
        var bytes = new byte[count];
        for (var i = 0; i < count; i++)
        {
            bytes[i] = (byte)(seed + i);
        }

        return bytes;
    }

    [Fact]
    public void GetEmbeddedFonts_RoundTripsSingleFont()
    {
        var bytes = MakeBytes(300, 7);
        var footer = "[Fonts]\r\nfontname: MyFont_0.ttf\r\n" + UUEncoding.UUEncode(bytes);

        var fonts = AssaFontEmbedder.GetEmbeddedFonts(footer);

        Assert.Single(fonts);
        Assert.Equal("MyFont_0.ttf", fonts[0].FileName);
        Assert.Equal(bytes, fonts[0].Bytes);
    }

    [Fact]
    public void GetEmbeddedFonts_ReadsMultipleFontsAndStopsAtNextSection()
    {
        // UUDecode pads non-3-divisible payloads with trailing zeros (same as the
        // attachments window) - use 3-divisible sizes so the comparison is exact.
        var bytes1 = MakeBytes(99, 3);
        var bytes2 = MakeBytes(81, 90);
        var footer =
            "[Fonts]\r\n" +
            "fontname: one.ttf\r\n" + UUEncoding.UUEncode(bytes1) + "\r\n" +
            "fontname: two.otf\r\n" + UUEncoding.UUEncode(bytes2) + "\r\n" +
            "[Graphics]\r\n" +
            "filename: not-a-font.png\r\nAAAA\r\n";

        var fonts = AssaFontEmbedder.GetEmbeddedFonts(footer);

        Assert.Equal(2, fonts.Count);
        Assert.Equal("one.ttf", fonts[0].FileName);
        Assert.Equal(bytes1, fonts[0].Bytes);
        Assert.Equal("two.otf", fonts[1].FileName);
        Assert.Equal(bytes2, fonts[1].Bytes);
    }

    [Fact]
    public void GetEmbeddedFonts_EmptyOrFontlessFooter_ReturnsNothing()
    {
        Assert.Empty(AssaFontEmbedder.GetEmbeddedFonts(null));
        Assert.Empty(AssaFontEmbedder.GetEmbeddedFonts(string.Empty));
        Assert.Empty(AssaFontEmbedder.GetEmbeddedFonts("[Graphics]\r\nfilename: img.png\r\nAAAA\r\n"));
    }
}
