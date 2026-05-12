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
}
