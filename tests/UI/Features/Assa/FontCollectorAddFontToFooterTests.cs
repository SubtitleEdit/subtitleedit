using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Features.Assa;

/// <summary>
/// Tests the [Fonts] footer writing used when a collected font is picked in the
/// ASSA styles dialog. The payload need not be a real font - only splitting and
/// UU-encoding are exercised (3-divisible sizes round-trip exactly).
/// </summary>
public class FontCollectorAddFontToFooterTests
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
    public void EmptyFooter_CreatesFontsSection()
    {
        var bytes = MakeBytes(300, 7);

        var footer = AssaFontEmbedder.AddFontToFooter(null, "/some/folder/MyFont.ttf", bytes);

        var fonts = AssaFontEmbedder.GetEmbeddedFonts(footer);
        Assert.Single(fonts);
        Assert.Equal("MyFont.ttf", fonts[0].FileName);
        Assert.Equal(bytes, fonts[0].Bytes);
        Assert.Contains("[Fonts]" + Environment.NewLine, footer); // the save gate in AdvancedSubStationAlpha.ToText
    }

    [Fact]
    public void ExistingFontsSection_AppendsAndKeepsExistingFont()
    {
        var bytes1 = MakeBytes(99, 3);
        var bytes2 = MakeBytes(81, 90);
        var footer = "[Fonts]\r\nfontname: one.ttf\r\n" + UUEncoding.UUEncode(bytes1);

        footer = AssaFontEmbedder.AddFontToFooter(footer, "two.otf", bytes2);

        var fonts = AssaFontEmbedder.GetEmbeddedFonts(footer);
        Assert.Equal(2, fonts.Count);
        Assert.Equal("one.ttf", fonts[0].FileName);
        Assert.Equal(bytes1, fonts[0].Bytes);
        Assert.Equal("two.otf", fonts[1].FileName);
        Assert.Equal(bytes2, fonts[1].Bytes);
    }

    [Fact]
    public void FontsSectionFollowedByGraphics_InsertsBeforeGraphics()
    {
        var bytes1 = MakeBytes(99, 3);
        var bytes2 = MakeBytes(81, 90);
        var footer =
            "[Fonts]\r\n" +
            "fontname: one.ttf\r\n" + UUEncoding.UUEncode(bytes1) + "\r\n" +
            "\r\n" +
            "[Graphics]\r\n" +
            "filename: img.png\r\nAAAA\r\n";

        footer = AssaFontEmbedder.AddFontToFooter(footer, "two.otf", bytes2);

        var fonts = AssaFontEmbedder.GetEmbeddedFonts(footer);
        Assert.Equal(2, fonts.Count);
        Assert.Equal("one.ttf", fonts[0].FileName);
        Assert.Equal("two.otf", fonts[1].FileName);
        Assert.True(footer.IndexOf("two.otf", StringComparison.Ordinal) < footer.IndexOf("[Graphics]", StringComparison.Ordinal));
        Assert.Contains("filename: img.png", footer);
    }

    [Fact]
    public void GraphicsOnlyFooter_AddsFontsSectionFirst()
    {
        var bytes = MakeBytes(300, 7);
        var footer = "[Graphics]\r\nfilename: img.png\r\nAAAA\r\n";

        footer = AssaFontEmbedder.AddFontToFooter(footer, "MyFont.ttf", bytes);

        var fonts = AssaFontEmbedder.GetEmbeddedFonts(footer);
        Assert.Single(fonts);
        Assert.Equal("MyFont.ttf", fonts[0].FileName);
        Assert.True(footer.IndexOf("[Fonts]", StringComparison.Ordinal) < footer.IndexOf("[Graphics]", StringComparison.Ordinal));
        Assert.Contains("filename: img.png", footer);
    }

    [Fact]
    public void SameFileName_IsNotAttachedTwice()
    {
        var bytes = MakeBytes(300, 7);
        var footer = AssaFontEmbedder.AddFontToFooter(null, "MyFont.ttf", bytes);

        var unchanged = AssaFontEmbedder.AddFontToFooter(footer, "/other/path/MyFont.ttf", MakeBytes(60, 1));

        Assert.Equal(footer, unchanged);
        Assert.Single(AssaFontEmbedder.GetEmbeddedFonts(unchanged));
    }
}
