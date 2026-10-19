using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Logic;

/// <summary>
/// The editor palette is derived from the theme settings (#16233): the dark version from the
/// user's dark background color, the light one from the theme's window color, and the accent from
/// the theme, so the redesigned windows match the rest of the app instead of a blue-tinted palette
/// of their own.
/// </summary>
public class EditorChromePaletteTests
{
    private static readonly Color DefaultDark = Color.FromRgb(33, 33, 33);

    [Fact]
    public void DefaultDarkPalette_IsNeutralGray_AndFlushWithTheMainWindow()
    {
        var palette = EditorChrome.EditorPalette.Derive(isDark: true, DefaultDark, EditorChrome.FallbackAccent);

        Assert.Equal(DefaultDark, palette.Window);
        foreach (var color in new[] { palette.Window, palette.Panel, palette.Status, palette.ButtonBar, palette.Line, palette.Separator, palette.Hover, palette.Pressed, palette.Field, palette.FieldBorder, palette.Menu, palette.MenuBorder, palette.DimText, palette.FaintText, palette.Icon, palette.Thumbnail, palette.ThumbnailMuted })
        {
            Assert.True(color.R == color.G && color.G == color.B, $"not neutral: {color}");
        }

        // Panels a step up from the window, fields a step down, hover above panel, pressed above hover
        Assert.True(palette.Panel.R > palette.Window.R);
        Assert.True(palette.Field.R < palette.Window.R);
        Assert.True(palette.Hover.R > palette.Panel.R);
        Assert.True(palette.Pressed.R > palette.Hover.R);
    }

    [Theory]
    [InlineData(33, 33, 33)]
    [InlineData(30, 31, 36)]
    [InlineData(0, 0, 0)]
    [InlineData(60, 50, 50)]
    public void DarkPalette_TextOnPanel_MeetsContrast(byte r, byte g, byte b)
    {
        var palette = EditorChrome.EditorPalette.Derive(isDark: true, Color.FromRgb(r, g, b), EditorChrome.FallbackAccent);

        Assert.True(UiTheme.GetContrastRatio(palette.DimText, palette.Panel) >= 4.5, "dim text");
        Assert.True(UiTheme.GetContrastRatio(palette.FaintText, palette.Panel) >= 3.0, "faint text");
        Assert.True(UiTheme.GetContrastRatio(palette.Icon, palette.Panel) >= 3.0, "icons");
        Assert.True(UiTheme.GetContrastRatio(palette.StrongText, palette.Hover) >= 4.5, "hover text");
    }

    [Theory]
    [InlineData(255, 255, 255)] // Light
    [InlineData(236, 233, 216)] // Classic
    [InlineData(240, 235, 255)] // Pastel
    public void LightPalette_TextOnPanel_MeetsContrast(byte r, byte g, byte b)
    {
        var palette = EditorChrome.EditorPalette.Derive(isDark: false, Color.FromRgb(r, g, b), EditorChrome.FallbackAccent);

        Assert.True(UiTheme.GetContrastRatio(palette.DimText, palette.Panel) >= 4.5, "dim text");
        Assert.True(UiTheme.GetContrastRatio(palette.FaintText, palette.Panel) >= 3.0, "faint text");
        Assert.True(UiTheme.GetContrastRatio(palette.Icon, palette.Panel) >= 3.0, "icons");
    }

    [Fact]
    public void TintedBase_KeepsItsTint()
    {
        var palette = EditorChrome.EditorPalette.Derive(isDark: true, Color.FromRgb(30, 31, 36), EditorChrome.FallbackAccent);

        // Each surface keeps the base's channel differences (bluish stays bluish)
        Assert.Equal(Color.FromRgb(30, 31, 36), palette.Window);
        Assert.Equal(Color.FromRgb(36, 37, 42), palette.Panel);
        Assert.True(palette.Hover.B > palette.Hover.R);
    }

    [Fact]
    public void LightPalette_FromWhite_MatchesTheOriginalLook()
    {
        var palette = EditorChrome.EditorPalette.Derive(isDark: false, Colors.White, EditorChrome.FallbackAccent);

        Assert.Equal(Color.FromRgb(243, 243, 243), palette.Window);
        Assert.Equal(Color.FromRgb(250, 250, 250), palette.Panel);
        Assert.Equal(Colors.White, palette.Field);
        Assert.Equal(Colors.White, palette.Menu);
        Assert.Equal(Colors.Black, palette.StrongText);
        Assert.Equal(EditorChrome.FallbackAccent, palette.BadgeText);
    }

    [Fact]
    public void Accent_IsTakenAsGiven_WithHoverSoftAndReadableTextOnIt()
    {
        var violet = Color.FromRgb(142, 111, 216);
        var dark = EditorChrome.EditorPalette.Derive(isDark: true, DefaultDark, violet);
        Assert.Equal(violet, dark.Accent);
        Assert.Equal(Color.FromRgb(156, 125, 230), dark.AccentHover);
        Assert.Equal(Color.FromArgb(70, 142, 111, 216), dark.AccentSoft);
        Assert.Equal(Colors.White, dark.OnAccent);

        // A light OS accent gets black text on it
        var yellow = Color.FromRgb(255, 220, 0);
        Assert.Equal(Colors.Black, EditorChrome.EditorPalette.Derive(isDark: true, DefaultDark, yellow).OnAccent);
    }

    [AvaloniaFact]
    public void PaletteBrushes_FollowTheDarkBackgroundSetting()
    {
        var application = Application.Current!;
        var appearance = Se.Settings.Appearance;
        var oldVariant = application.RequestedThemeVariant;
        var oldTheme = appearance.Theme;
        var oldBackground = appearance.DarkModeBackgroundColor;
        try
        {
            appearance.Theme = UiTheme.ThemeNameDark;
            application.RequestedThemeVariant = ThemeVariant.Dark;
            appearance.DarkModeBackgroundColor = "#FF2A2020";
            EditorChrome.InvalidatePalette();

            var window = Assert.IsAssignableFrom<ISolidColorBrush>(EditorChrome.WindowBrush);
            var panel = Assert.IsAssignableFrom<ISolidColorBrush>(EditorChrome.PanelBrush);
            Assert.Equal(Color.FromRgb(42, 32, 32), window.Color);
            Assert.Equal(Color.FromRgb(48, 38, 38), panel.Color);

            // A changed setting is picked up on the next read, without an explicit invalidate
            appearance.DarkModeBackgroundColor = "#FF212121";
            Assert.Equal(Color.FromRgb(33, 33, 33), Assert.IsAssignableFrom<ISolidColorBrush>(EditorChrome.WindowBrush).Color);

            // A malformed setting falls back to the default dark base
            appearance.DarkModeBackgroundColor = "nope";
            Assert.Equal(EditorChrome.DefaultDarkBase, Assert.IsAssignableFrom<ISolidColorBrush>(EditorChrome.WindowBrush).Color);
        }
        finally
        {
            appearance.Theme = oldTheme;
            appearance.DarkModeBackgroundColor = oldBackground;
            application.RequestedThemeVariant = oldVariant;
            EditorChrome.InvalidatePalette();
        }
    }

    [AvaloniaFact]
    public void PaletteBrushes_FollowTheLightThemeWindowColor()
    {
        var application = Application.Current!;
        var appearance = Se.Settings.Appearance;
        var oldVariant = application.RequestedThemeVariant;
        var oldTheme = appearance.Theme;
        try
        {
            application.RequestedThemeVariant = ThemeVariant.Light;

            appearance.Theme = UiTheme.ThemeNameLight;
            EditorChrome.InvalidatePalette();
            Assert.Equal(Colors.White, Assert.IsAssignableFrom<ISolidColorBrush>(EditorChrome.FieldBrush).Color);

            appearance.Theme = UiTheme.ThemeNameClassic;
            EditorChrome.InvalidatePalette();
            Assert.Equal(Color.FromRgb(236, 233, 216), Assert.IsAssignableFrom<ISolidColorBrush>(EditorChrome.FieldBrush).Color);
        }
        finally
        {
            appearance.Theme = oldTheme;
            application.RequestedThemeVariant = oldVariant;
            EditorChrome.InvalidatePalette();
        }
    }
}
