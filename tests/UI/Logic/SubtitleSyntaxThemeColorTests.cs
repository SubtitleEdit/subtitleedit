using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Logic;

public class SubtitleSyntaxThemeColorTests
{
    [AvaloniaFact]
    public void PastelsAreThemeStableButStyleColorFollowsTheme()
    {
        var savedTheme = Se.Settings.Appearance.Theme;
        var savedVariant = Application.Current!.RequestedThemeVariant;
        try
        {
            Se.Settings.Appearance.Theme = UiTheme.ThemeNameDark;
            var darkElement = SubtitleSyntaxTokenizer.ElementColor;
            var darkStyle = SubtitleSyntaxTokenizer.StyleColor;

            Se.Settings.Appearance.Theme = UiTheme.ThemeNameLight;

            // The pastels deliberately stay the same in light mode - tags must read as muted
            // markup, not as darker body text.
            Assert.Equal(darkElement, SubtitleSyntaxTokenizer.ElementColor);

            // Light cyan is illegible on white, so StyleColor is the one themed color.
            var lightStyle = SubtitleSyntaxTokenizer.StyleColor;
            Assert.NotEqual(darkStyle, lightStyle);

            // The "System" setting follows the actual Avalonia theme variant.
            Se.Settings.Appearance.Theme = UiTheme.ThemeNameSystem;
            Application.Current.RequestedThemeVariant = ThemeVariant.Dark;
            Assert.Equal(darkStyle, SubtitleSyntaxTokenizer.StyleColor);
            Application.Current.RequestedThemeVariant = ThemeVariant.Light;
            Assert.Equal(lightStyle, SubtitleSyntaxTokenizer.StyleColor);
        }
        finally
        {
            Se.Settings.Appearance.Theme = savedTheme;
            Application.Current.RequestedThemeVariant = savedVariant;
        }
    }

    [AvaloniaFact]
    public void TokenizerEmitsCurrentThemeStyleColor()
    {
        var savedTheme = Se.Settings.Appearance.Theme;
        try
        {
            // "font-style:italic" contains ':' and so gets StyleColor.
            const string text = "<span style=\"font-style:italic\">Hi</span>";

            Se.Settings.Appearance.Theme = UiTheme.ThemeNameLight;
            var lightRange = SubtitleSyntaxTokenizer.Tokenize(text)
                .Single(r => text.Substring(r.Start, r.Length) == "font-style:italic");

            Se.Settings.Appearance.Theme = UiTheme.ThemeNameDark;
            var darkRange = SubtitleSyntaxTokenizer.Tokenize(text)
                .Single(r => text.Substring(r.Start, r.Length) == "font-style:italic");

            Assert.NotEqual(darkRange.Color, lightRange.Color);
        }
        finally
        {
            Se.Settings.Appearance.Theme = savedTheme;
        }
    }
}
