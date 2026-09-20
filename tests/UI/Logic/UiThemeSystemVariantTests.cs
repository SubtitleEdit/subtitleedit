using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Logic;

public class UiThemeSystemVariantTests
{
    /// <summary>
    /// With the "System" theme, SetCurrentTheme subscribes UiTheme to the application's
    /// ActualThemeVariantChanged event for the rest of the process - ApplySettings does this in
    /// every MainView test that applies settings. The handler used to re-enter SetCurrentTheme,
    /// which reset RequestedThemeVariant to Default, so any explicit variant set afterwards was
    /// undone on the spot and ThemeName reported the platform default (Light) instead of the
    /// variant that had just been requested. That is the order-dependent CI failure of
    /// SubtitleSyntaxThemeColorTests (expected #ff9cdcfe, actual #ff006c9b).
    /// </summary>
    [AvaloniaFact]
    public void SystemTheme_KeepsAnExplicitVariant_AfterSetCurrentThemeSubscribed()
    {
        var app = Application.Current!;
        var savedTheme = Se.Settings.Appearance.Theme;
        var savedVariant = app.RequestedThemeVariant;
        try
        {
            Se.Settings.Appearance.Theme = UiTheme.ThemeNameSystem;
            UiTheme.SetCurrentTheme();

            app.RequestedThemeVariant = ThemeVariant.Dark;
            Assert.Equal(ThemeVariant.Dark, app.RequestedThemeVariant);
            Assert.Equal(ThemeVariant.Dark, app.ActualThemeVariant);
            Assert.Equal(UiTheme.ThemeNameDark, UiTheme.ThemeName);

            app.RequestedThemeVariant = ThemeVariant.Light;
            Assert.Equal(ThemeVariant.Light, app.ActualThemeVariant);
            Assert.Equal(UiTheme.ThemeNameLight, UiTheme.ThemeName);
        }
        finally
        {
            Se.Settings.Appearance.Theme = savedTheme;
            app.RequestedThemeVariant = savedVariant;
        }
    }
}
