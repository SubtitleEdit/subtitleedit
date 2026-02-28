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
}
