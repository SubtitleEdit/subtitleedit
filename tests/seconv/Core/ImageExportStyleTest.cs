using Nikse.SubtitleEdit.UiLogic.Export;
using SeConv.Core;
using Xunit;

namespace SeConvTests.Core;

public class ImageExportStyleTest
{
    [Theory]
    [InlineData("#FFFFFF", 255, 255, 255, 255)]
    [InlineData("FFFFFF", 255, 255, 255, 255)]
    [InlineData("#B4000000", 0, 0, 0, 180)]
    [InlineData("white", 255, 255, 255, 255)]
    [InlineData("Black", 0, 0, 0, 255)]
    [InlineData("YELLOW", 255, 255, 0, 255)]
    public void TryParseColor_ValidInputs(string input, byte r, byte g, byte b, byte a)
    {
        Assert.True(ImageExportStyle.TryParseColor(input, out var color));
        Assert.Equal(r, color.Red);
        Assert.Equal(g, color.Green);
        Assert.Equal(b, color.Blue);
        Assert.Equal(a, color.Alpha);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("notacolor")]
    [InlineData("#GGGGGG")]
    public void TryParseColor_InvalidInputs(string input)
    {
        Assert.False(ImageExportStyle.TryParseColor(input, out _));
    }

    [Theory]
    [InlineData("none", ExportBoxType.None)]
    [InlineData("one-box", ExportBoxType.OneBox)]
    [InlineData("OneBox", ExportBoxType.OneBox)]
    [InlineData("box-per-line", ExportBoxType.BoxPerLine)]
    [InlineData("box_per_line", ExportBoxType.BoxPerLine)]
    public void TryParseBoxType_ValidInputs(string input, ExportBoxType expected)
    {
        Assert.True(ImageExportStyle.TryParseBoxType(input, out var boxType));
        Assert.Equal(expected, boxType);
    }

    [Theory]
    [InlineData("nope")]
    [InlineData("7")]
    public void TryParseBoxType_InvalidInputs(string input)
    {
        Assert.False(ImageExportStyle.TryParseBoxType(input, out _));
    }

    [Theory]
    [InlineData("bottom-center", ExportAlignment.BottomCenter)]
    [InlineData("TopLeft", ExportAlignment.TopLeft)]
    [InlineData("middle-right", ExportAlignment.MiddleRight)]
    public void TryParseAlignment_ValidInputs(string input, ExportAlignment expected)
    {
        Assert.True(ImageExportStyle.TryParseAlignment(input, out var alignment));
        Assert.Equal(expected, alignment);
    }

    [Fact]
    public void EffectiveBoxType_DefaultsToNone()
    {
        var style = new ImageExportStyle();
        Assert.Equal(ExportBoxType.None, style.EffectiveBoxType);
    }

    [Fact]
    public void EffectiveBoxType_VisibleBackgroundImpliesOneBox()
    {
        var style = new ImageExportStyle();
        Assert.True(ImageExportStyle.TryParseColor("#B4000000", out var bg));
        style.BackgroundColor = bg;
        Assert.Equal(ExportBoxType.OneBox, style.EffectiveBoxType);
    }

    [Fact]
    public void EffectiveBoxType_ExplicitBoxTypeWinsOverBackground()
    {
        var style = new ImageExportStyle();
        Assert.True(ImageExportStyle.TryParseColor("black", out var bg));
        style.BackgroundColor = bg;
        style.BoxType = ExportBoxType.BoxPerLine;
        Assert.Equal(ExportBoxType.BoxPerLine, style.EffectiveBoxType);

        style.BoxType = ExportBoxType.None;
        Assert.Equal(ExportBoxType.None, style.EffectiveBoxType);
    }
}
