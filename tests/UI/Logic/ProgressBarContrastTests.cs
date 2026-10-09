using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Logic;

/// <summary>
/// A gray Windows accent such as "Storm" (#4C4A48) made the progress bar fill match the dark
/// track, so the speech to text progress looked frozen (#15836).
/// </summary>
public class ProgressBarContrastTests
{
    private static readonly Color DarkTrack = Color.FromRgb(77, 77, 77);
    private static readonly Color LightTrack = Color.FromRgb(204, 204, 204);

    [Fact]
    public void Gray_Accent_On_Dark_Theme_Is_Lifted()
    {
        var storm = Color.FromRgb(0x4C, 0x4A, 0x48);
        var fill = UiTheme.GetProgressBarFillColor(storm, isDark: true);

        Assert.NotEqual(storm, fill);
        Assert.True(UiTheme.GetContrastRatio(fill, DarkTrack) >= UiTheme.AdjustedProgressBarContrast);
        Assert.True(fill.R > storm.R && fill.G > storm.G && fill.B > storm.B);
    }

    [Fact]
    public void Light_Gray_Accent_On_Light_Theme_Is_Darkened()
    {
        var accent = Color.FromRgb(200, 200, 200);
        var fill = UiTheme.GetProgressBarFillColor(accent, isDark: false);

        Assert.True(UiTheme.GetContrastRatio(fill, LightTrack) >= UiTheme.AdjustedProgressBarContrast);
        Assert.True(fill.R < accent.R);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Default_Windows_Blue_Is_Kept(bool isDark)
    {
        var blue = Color.FromRgb(0x00, 0x78, 0xD4);
        Assert.Equal(blue, UiTheme.GetProgressBarFillColor(blue, isDark));
    }
}
