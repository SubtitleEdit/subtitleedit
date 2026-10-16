using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Features.Tools.FixCommonErrors;
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

    [AvaloniaFact]
    public void OpenWindow_SemanticColorOutsideThePalette_FollowsAThemeChange()
    {
        var application = Application.Current!;
        var old = application.RequestedThemeVariant;
        application.RequestedThemeVariant = ThemeVariant.Light;
        var vm = new FixCommonErrorsLogViewModel();
        vm.Initialize(new[] { "a" }, System.Array.Empty<string>(), 1);
        var window = new FixCommonErrorsLogWindow(vm);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            // The red "important messages" label was picked once when the window was built
            var label = window.GetLogicalDescendants().OfType<TextBlock>()
                .First(t => t.Text == vm.ImportantMessagesText);
            var brush = Assert.IsAssignableFrom<ISolidColorBrush>(label.Foreground);
            var light = brush.Color;

            application.RequestedThemeVariant = ThemeVariant.Dark;
            Dispatcher.UIThread.RunJobs();

            Assert.Same(brush, label.Foreground);
            Assert.NotEqual(light, brush.Color);
        }
        finally
        {
            window.Close();
            application.RequestedThemeVariant = old;
        }
    }
}
