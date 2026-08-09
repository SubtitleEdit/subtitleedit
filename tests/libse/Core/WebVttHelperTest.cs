using Nikse.SubtitleEdit.Core.Common;
using SkiaSharp;

namespace LibSETests.Core;

public class WebVttHelperTest
{
    [Fact]
    public void RemoveColorTag1()
    {
        var styles = new List<WebVttStyle>
        {
            new WebVttStyle()
            {
                Name = ".Red",
                Color = SKColors.Red,
            },
        };

        var text = "<c.Red>Red</c>";
        var result = WebVttHelper.RemoveColorTag(text, SKColors.Red, styles);

        Assert.Equal("Red", result);
    }

    [Fact]
    public void RemoveColorTag2()
    {
        var styles = new List<WebVttStyle>
        {
            new WebVttStyle
            {
                Name = ".Red",
                Color = SKColors.Red,
            },
            new WebVttStyle
            {
                Name = ".Italic",
                Italic = true,
            },
        };

        var text = "<c.Red.Italic>Red</c>";
        var result = WebVttHelper.RemoveColorTag(text, SKColors.Red, styles);

        Assert.Equal("<c.Italic>Red</c>", result);
    }

    [Fact]
    public void RemoveColorTagMultiline()
    {
        var styles = new List<WebVttStyle>
        {
            new WebVttStyle
            {
                Name = ".yellow",
                Color = SKColors.Yellow,
            },
        };

        var text = "<c.yellow>-Qu'est-ce qu'on a ?</c>" + Environment.NewLine + "<c.yellow>-Adrien Dorval, 65 ans.</c>";
        var result = WebVttHelper.RemoveColorTag(text, SKColors.Yellow, styles);

        var expected = "-Qu'est-ce qu'on a ?" + Environment.NewLine + "-Adrien Dorval, 65 ans.";
        Assert.Equal(expected, result);
    }

    [Fact]
    public void RemoveDefaultColorClassesMultiline()
    {
        var text = "<c.yellow>-Qu'est-ce qu'on a ?</c>" + Environment.NewLine + "<c.yellow>-Adrien Dorval, 65 ans.</c>";
        var result = WebVttHelper.RemoveDefaultColorClasses(text);

        var expected = "-Qu'est-ce qu'on a ?" + Environment.NewLine + "-Adrien Dorval, 65 ans.";
        Assert.Equal(expected, result);
    }

    [Fact]
    public void RemoveDefaultColorClassesPreservesUnknownClasses()
    {
        var text = "<c.yellow.bold>Hi</c>";
        var result = WebVttHelper.RemoveDefaultColorClasses(text);

        Assert.Equal("<c.bold>Hi</c>", result);
    }

    [Fact]
    public void RemoveDefaultColorClassesNoChange()
    {
        var text = "Plain text";
        var result = WebVttHelper.RemoveDefaultColorClasses(text);

        Assert.Equal("Plain text", result);
    }

    [Fact]
    public void RemoveDefaultColorClassesGreen()
    {
        var text = "<c.green>Vert</c>" + Environment.NewLine + "<c.bg_green>Fond vert</c>";
        var result = WebVttHelper.RemoveDefaultColorClasses(text);

        var expected = "Vert" + Environment.NewLine + "Fond vert";
        Assert.Equal(expected, result);
    }

    [Fact]
    public void RemoveDefaultColorClassesContentWithDot()
    {
        var text = "<c.white>france.tv access</c>";
        var result = WebVttHelper.RemoveDefaultColorClasses(text);

        Assert.Equal("france.tv access", result);
    }

    [Fact]
    public void GetCssPropertiesWritesParsableTextShadow()
    {
        var style = new WebVttStyle
        {
            Name = ".shadow",
            ShadowColor = new SKColor(0x10, 0x10, 0x10),
            ShadowWidth = 3,
        };

        var css = WebVttHelper.GetCssProperties(style);

        Assert.Contains("text-shadow: #101010ff 3px", css);
    }

    [Fact]
    public void TextShadowRoundTrips()
    {
        var style = new WebVttStyle
        {
            Name = ".shadow",
            Color = SKColors.White,
            ShadowColor = new SKColor(0x10, 0x20, 0x30),
            ShadowWidth = 3,
        };

        var header = WebVttHelper.AddStyleToHeader(null, style);
        var readBack = WebVttHelper.GetStyles(header).Single();

        Assert.Equal(new SKColor(0x10, 0x20, 0x30), readBack.ShadowColor);
        Assert.Equal(3, readBack.ShadowWidth);
    }

    [Fact]
    public void TextShadowRoundTripsFractionalWidth()
    {
        var style = new WebVttStyle
        {
            Name = ".shadow",
            ShadowColor = SKColors.Black,
            ShadowWidth = 1.5m,
        };

        var header = WebVttHelper.AddStyleToHeader(null, style);
        var readBack = WebVttHelper.GetStyles(header).Single();

        Assert.Equal(1.5m, readBack.ShadowWidth);
    }

    [Theory]
    [InlineData("20px", 20)]
    [InlineData("20", 20)]
    [InlineData(" 18.5px ", 18.5)]
    public void FontSizeIsRead(string cssValue, decimal expected)
    {
        var header = "WEBVTT" + Environment.NewLine + Environment.NewLine +
                     "STYLE" + Environment.NewLine +
                     "::cue(.big) { font-size:" + cssValue + " }";

        var style = WebVttHelper.GetStyles(header).Single();

        Assert.Equal(expected, style.FontSize);
    }

    [Theory]
    [InlineData("1.5em")]
    [InlineData("larger")]
    public void RelativeFontSizeIsNotRead(string cssValue)
    {
        var header = "WEBVTT" + Environment.NewLine + Environment.NewLine +
                     "STYLE" + Environment.NewLine +
                     "::cue(.big) { font-size:" + cssValue + " }";

        var style = WebVttHelper.GetStyles(header).Single();

        Assert.Null(style.FontSize);
    }

    [Fact]
    public void FontAndDecorationsRoundTrip()
    {
        var style = new WebVttStyle
        {
            Name = ".fancy",
            FontName = "Arial",
            FontSize = 20,
            Bold = true,
            Italic = true,
            Underline = true,
            Color = SKColors.Red,
            BackgroundColor = SKColors.Blue,
        };

        var header = WebVttHelper.AddStyleToHeader(null, style);
        var readBack = WebVttHelper.GetStyles(header).Single();

        Assert.Equal(".fancy", readBack.Name);
        Assert.Equal("Arial", readBack.FontName);
        Assert.Equal(20, readBack.FontSize);
        Assert.True(readBack.Bold);
        Assert.True(readBack.Italic);
        Assert.True(readBack.Underline);
        Assert.Equal(SKColors.Red, readBack.Color);
        Assert.Equal(SKColors.Blue, readBack.BackgroundColor);
    }

    [Fact]
    public void GetParagraphStylesReadsAllClassesOfATag()
    {
        var styles = WebVttHelper.GetParagraphStyles("<c.loud.red>Hello</c>");

        Assert.Equal(new List<string> { ".loud", ".red" }, styles);
    }

    [Fact]
    public void GetParagraphStylesListsAClassUsedTwiceOnlyOnce()
    {
        var styles = WebVttHelper.GetParagraphStyles("<c.red>Hello</c>" + Environment.NewLine + "<c.red>world</c>");

        Assert.Equal(new List<string> { ".red" }, styles);
    }

    [Fact]
    public void GetParagraphStylesOfTextWithoutClasses()
    {
        Assert.Empty(WebVttHelper.GetParagraphStyles("<i>Hello</i>"));
        Assert.Empty(WebVttHelper.GetParagraphStyles(string.Empty));
    }
}
