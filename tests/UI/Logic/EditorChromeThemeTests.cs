using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Logic;

/// <summary>
/// The editor palette is one shared brush per color whose color follows the app theme, so windows
/// that are already open repaint when the theme changes instead of keeping the old palette.
/// </summary>
public class EditorChromeThemeTests
{
    [AvaloniaFact]
    public void PaletteBrush_IsShared_AndFollowsAThemeChange()
    {
        var application = Application.Current!;
        var old = application.RequestedThemeVariant;
        try
        {
            application.RequestedThemeVariant = ThemeVariant.Light;
            var brush = Assert.IsAssignableFrom<ISolidColorBrush>(EditorChrome.WindowBrush);
            var light = brush.Color;

            application.RequestedThemeVariant = ThemeVariant.Dark;

            // Same instance (what an open window holds), new color
            Assert.Same(brush, EditorChrome.WindowBrush);
            Assert.NotEqual(light, brush.Color);

            application.RequestedThemeVariant = ThemeVariant.Light;
            Assert.Equal(light, brush.Color);
        }
        finally
        {
            application.RequestedThemeVariant = old;
        }
    }
}
