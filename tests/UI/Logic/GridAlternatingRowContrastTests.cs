using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Logic;

/// <summary>
/// The grid's alternating row tint is the theme background moved a percentage (4-20%) toward
/// the theme text color, so it is visible on every theme without hand-picked colors (#16779).
/// </summary>
public class GridAlternatingRowContrastTests
{
    [Fact]
    public void Light_Theme_Darkens_White_By_The_Percentage()
    {
        var color = UiTheme.BlendGridAlternatingRowColor(Colors.White, Colors.Black, 10);

        Assert.Equal(Color.FromRgb(230, 230, 230), color);
    }

    [Fact]
    public void Dark_Theme_Lightens_Toward_Foreground()
    {
        var background = Color.FromRgb(33, 33, 33);
        var foreground = Color.FromRgb(220, 220, 220);

        var color = UiTheme.BlendGridAlternatingRowColor(background, foreground, 20);

        Assert.True(color.R > background.R);
        Assert.Equal(Color.FromRgb(70, 70, 70), color);
    }

    [Theory]
    [InlineData(0, 4)]
    [InlineData(4, 4)]
    [InlineData(12, 12)]
    [InlineData(20, 20)]
    [InlineData(99, 20)]
    public void Percentage_Is_Clamped(int value, int expected)
    {
        Assert.Equal(expected, UiTheme.ClampGridAlternatingRowContrast(value));
    }

    [Fact]
    public void Default_Is_Visible_On_Light_Theme()
    {
        Assert.Equal(10, new SeAppearance().GridAlternatingRowContrast);
        var color = UiTheme.BlendGridAlternatingRowColor(Colors.White, Colors.Black, 10);

        // The old default was #F5F5F5, too faint to notice on many screens.
        Assert.Equal(Color.FromRgb(230, 230, 230), color);
    }
}
