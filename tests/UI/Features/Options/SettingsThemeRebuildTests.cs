using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Styling;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Options.Settings;

namespace UITests.Features.Options;

/// <summary>
/// Attaching the Settings page to its window raises ActualThemeVariantChanged, which used to
/// rebuild the shown category and re-apply the window styles a second time on every open.
/// Only a real change of the app theme (e.g. with Apply) should rebuild.
/// </summary>
public class SettingsThemeRebuildTests
{
    [AvaloniaFact]
    public void Opening_DoesNotRebuild_ThemeChange_Does()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        Locator.Services = services.BuildServiceProvider();

        var app = Application.Current!;
        var requestedBefore = app.RequestedThemeVariant;
        app.RequestedThemeVariant = ThemeVariant.Light;
        Dispatcher.UIThread.RunJobs();
        try
        {
            var vm = Locator.Services.GetRequiredService<SettingsViewModel>();
            var window = new SettingsWindow(vm);
            var builtPanel = vm.SelectedSection!.Panel;
            window.Show();
            Dispatcher.UIThread.RunJobs();
            Assert.Same(builtPanel, vm.SelectedSection!.Panel);

            app.RequestedThemeVariant = ThemeVariant.Dark;
            Dispatcher.UIThread.RunJobs();
            Assert.NotSame(builtPanel, vm.SelectedSection!.Panel);

            window.Close();
        }
        finally
        {
            app.RequestedThemeVariant = requestedBefore;
            Dispatcher.UIThread.RunJobs();
        }
    }
}
