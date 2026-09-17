using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Options.Settings;
using Nikse.SubtitleEdit.Features.Options.Settings.WaveformToolbarItems;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Xunit;

namespace UITests.Features.Options;

/// <summary>
/// Closing a dialog opened from Settings (e.g. Waveform > Toolbar items) sent keyboard focus back
/// to the top of Settings instead of the button that opened it (#12087). On Windows the owner is
/// re-activated with focus on the bare window; the tests put it there when the dialog closes.
/// </summary>
public class SettingsSubDialogFocusTests : IDisposable
{
    public SettingsSubDialogFocusTests()
    {
        SettingsViewModel.AnimateScrollToSection = false;
    }

    public void Dispose()
    {
        SettingsViewModel.AnimateScrollToSection = true;
    }

    [AvaloniaFact]
    public void ClosingASubDialog_WithFocusLost_ReturnsFocusToTheButtonThatOpenedIt()
    {
        var (window, vm, button) = OpenSettingsWithToolbarItemsButtonFocused();
        try
        {
            var dialog = OpenToolbarItemsDialog(window);
            // What Windows leaves behind. Posted: headless Avalonia restores the owner's focus
            // itself while the dialog closes, which the real Windows re-activation does not.
            dialog.Closed += (_, _) => Dispatcher.UIThread.Post(() =>
            {
                window.Focusable = true;
                Assert.True(window.Focus());
            });

            dialog.KeyPressQwerty(PhysicalKey.Escape, RawInputModifiers.None);

            Assert.True(PumpUntil(() => window.FocusManager?.GetFocusedElement() == button),
                $"Focus is on {window.FocusManager?.GetFocusedElement()}");
            Assert.True(button.Classes.Contains(":focus-visible"), "Opened from the keyboard, so the focus rectangle should come back too");
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [AvaloniaFact]
    public void ClosingASubDialog_DoesNotMoveFocusThatLandedOnALiveControl()
    {
        var (window, vm, _) = OpenSettingsWithToolbarItemsButtonFocused();
        try
        {
            var searchBox = window.GetVisualDescendants().OfType<TextBox>().First();
            var dialog = OpenToolbarItemsDialog(window);
            dialog.Closed += (_, _) => Dispatcher.UIThread.Post(() => searchBox.Focus());

            dialog.KeyPressQwerty(PhysicalKey.Escape, RawInputModifiers.None);
            Assert.True(PumpUntil(() => !window.OwnedWindows.Contains(dialog)));
            for (var i = 0; i < 20; i++)
            {
                Dispatcher.UIThread.RunJobs();
                Thread.Sleep(10);
            }

            Assert.Same(searchBox, window.FocusManager?.GetFocusedElement());
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static (SettingsWindow Window, SettingsViewModel Vm, Button Button) OpenSettingsWithToolbarItemsButtonFocused()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        Locator.Services = services.BuildServiceProvider();

        var vm = Locator.Services.GetRequiredService<SettingsViewModel>();
        var window = new SettingsWindow(vm);
        window.Show();
        Dispatcher.UIThread.RunJobs();

        Button? button = null;
        foreach (var section in vm.Sections)
        {
            vm.SelectedSection = section;
            Dispatcher.UIThread.RunJobs();
            button = section.Panel!.GetVisualDescendants().OfType<Button>()
                .FirstOrDefault(b => b.Command == vm.EditWaveformToolbarPropertiesCommand);
            if (button != null)
            {
                break;
            }
        }

        Assert.NotNull(button);
        Assert.True(button!.Focus(NavigationMethod.Tab));
        Dispatcher.UIThread.RunJobs();
        return (window, vm, button);
    }

    private static WaveformToolbarItemsWindow OpenToolbarItemsDialog(SettingsWindow window)
    {
        window.KeyPressQwerty(PhysicalKey.Enter, RawInputModifiers.None);
        WaveformToolbarItemsWindow? dialog = null;
        Assert.True(PumpUntil(() => (dialog = window.OwnedWindows.OfType<WaveformToolbarItemsWindow>().FirstOrDefault()) != null));
        Dispatcher.UIThread.RunJobs();
        return dialog!;
    }

    private static bool PumpUntil(Func<bool> condition)
    {
        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.Elapsed < TimeSpan.FromSeconds(5))
        {
            Dispatcher.UIThread.RunJobs();
            if (condition())
            {
                return true;
            }

            Thread.Sleep(10);
        }

        return false;
    }
}
