using Avalonia.Media;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.WebVtt;
using SkiaSharp;

namespace UITests.Features.WebVtt;

/// <summary>
/// A WebVTT cue style is CSS, where every property is optional. These tests pin down that the
/// editor's value+flag model does not invent properties a style never had, and that a style
/// survives a trip through the editor unchanged.
/// </summary>
public class WebVttStyleDisplayTests
{
    [Fact]
    public void EmptyStyleWritesNoProperties()
    {
        var display = new WebVttStyleDisplay { Name = "plain" };

        var style = display.ToWebVttStyle();

        Assert.Equal(".plain", style.Name);
        Assert.Null(style.FontName);
        Assert.Null(style.FontSize);
        Assert.Null(style.Bold);
        Assert.Null(style.Italic);
        Assert.Null(style.Underline);
        Assert.Null(style.StrikeThrough);
        Assert.Null(style.Color);
        Assert.Null(style.BackgroundColor);
        Assert.Null(style.ShadowColor);
        Assert.Equal(string.Empty, WebVttHelper.GetCssProperties(style));
    }

    [Fact]
    public void UncheckedColorIsNotWritten()
    {
        // The color value is still there (so re-checking the box restores it), but an
        // unchecked color must not end up in the CSS.
        var display = new WebVttStyleDisplay
        {
            Name = "x",
            Color = Colors.Red,
            UseColor = false,
        };

        Assert.Null(display.ToWebVttStyle().Color);
    }

    [Fact]
    public void CheckedColorIsWritten()
    {
        var display = new WebVttStyleDisplay
        {
            Name = "x",
            Color = Colors.Red,
            UseColor = true,
        };

        Assert.Equal(SKColors.Red, display.ToWebVttStyle().Color);
    }

    [Fact]
    public void ZeroFontSizeMeansNotSet()
    {
        var display = new WebVttStyleDisplay { Name = "x", FontSize = 0 };

        Assert.Null(display.ToWebVttStyle().FontSize);
        Assert.Equal("-", display.FontSizeDisplay);
    }

    [Fact]
    public void NameIsHeldWithoutTheCueSelectorDot()
    {
        var display = new WebVttStyleDisplay(new WebVttStyle { Name = ".red" });

        Assert.Equal("red", display.Name);
        Assert.Equal(".red", display.ToWebVttStyle().Name);
    }

    [Fact]
    public void RoundTripsAFullStyle()
    {
        var original = new WebVttStyle
        {
            Name = ".fancy",
            FontName = "Arial",
            FontSize = 22,
            Bold = true,
            Italic = true,
            Underline = true,
            StrikeThrough = true,
            Color = SKColors.Red,
            BackgroundColor = SKColors.Blue,
            ShadowColor = SKColors.Black,
            ShadowWidth = 2,
        };

        var result = new WebVttStyleDisplay(original).ToWebVttStyle();

        Assert.Equal(original.Name, result.Name);
        Assert.Equal(original.FontName, result.FontName);
        Assert.Equal(original.FontSize, result.FontSize);
        Assert.Equal(original.Bold, result.Bold);
        Assert.Equal(original.Italic, result.Italic);
        Assert.Equal(original.Underline, result.Underline);
        Assert.Equal(original.StrikeThrough, result.StrikeThrough);
        Assert.Equal(original.Color, result.Color);
        Assert.Equal(original.BackgroundColor, result.BackgroundColor);
        Assert.Equal(original.ShadowColor, result.ShadowColor);
        Assert.Equal(original.ShadowWidth, result.ShadowWidth);
    }

    [Fact]
    public void CopyConstructorCopiesEverything()
    {
        var source = new WebVttStyleDisplay
        {
            Name = "src",
            FontName = "Arial",
            FontSize = 12,
            Bold = true,
            UseColor = true,
            Color = Colors.Green,
            UseShadow = true,
            ShadowColor = Colors.Black,
            ShadowWidth = 3,
        };

        var copy = new WebVttStyleDisplay(source);

        Assert.Equal(source.Css, copy.Css);
    }

    [Fact]
    public void CssUpdatesWhenAPropertyChanges()
    {
        var display = new WebVttStyleDisplay { Name = "x" };
        var raised = false;
        display.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(WebVttStyleDisplay.Css))
            {
                raised = true;
            }
        };

        display.Italic = true;

        Assert.True(raised);
        Assert.Contains("font-style:italic", display.Css);
    }
}
