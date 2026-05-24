using Avalonia.Media;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Logic.Plugins;

/// <summary>
/// Builds <see cref="PluginThemeColors"/> for the currently active Subtitle Edit theme.
/// Dark colors come from <see cref="SeAppearance"/>; Classic/Pastel/Light use the same
/// hard-coded palettes as <see cref="UiTheme"/>.
/// </summary>
internal static class PluginThemeColorsFactory
{
    public static PluginThemeColors Build()
    {
        var appearance = Se.Settings.Appearance;
        var themeName = UiTheme.ThemeName;
        var isDark = themeName == UiTheme.ThemeNameDark;

        Color background;
        Color foreground;

        if (isDark)
        {
            background = appearance.DarkModeBackgroundColor.FromHexToColor();
            foreground = appearance.DarkModeForegroundColor.FromHexToColor();
        }
        else if (themeName == UiTheme.ThemeNameClassic)
        {
            background = Color.FromRgb(236, 233, 216);
            foreground = Color.FromRgb(0, 0, 0);
        }
        else if (themeName == UiTheme.ThemeNamePastel)
        {
            background = Color.FromRgb(240, 235, 255);
            foreground = Color.FromRgb(0, 0, 0);
        }
        else
        {
            background = Color.FromRgb(255, 255, 255);
            foreground = Color.FromRgb(0, 0, 0);
        }

        return new PluginThemeColors
        {
            IsDark = isDark,
            BackgroundColor = background.FromColorToHex(),
            ForegroundColor = foreground.FromColorToHex(),
            AccentColor = appearance.FocusedButtonBackgroundColor,
            BackgroundColorLighter = UiUtil.LightenColor(background, 5).FromColorToHex(),
            BackgroundColorHeader = UiUtil.LightenColor(background, 15).FromColorToHex(),
            BookmarkColor = appearance.BookmarkColor,
        };
    }
}
