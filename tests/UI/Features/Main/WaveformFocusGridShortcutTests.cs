using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Options.Shortcuts;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Main;

/// <summary>
/// Selecting a line by clicking the waveform moves keyboard focus to the waveform, and the
/// "Subtitle list view &amp; text box" shortcuts (e.g. Toggle dialog dashes) were then only checked
/// when the grid or a text box had focus - so they did nothing until the user clicked the grid or
/// text box again (#16013). SE 4 ran them regardless, and they act on the grid selection.
/// </summary>
public class WaveformFocusGridShortcutTests
{
    private static readonly string[] Chord = ["Control", "Shift", nameof(Key.F11)];

    [AvaloniaFact]
    public void ToggleDialogDashes_Runs_WhenWaveformIsFocused()
    {
        WithShortcut(nameof(MainViewModel.ToggleDialogDashesCommand), Chord, () =>
        {
            var (window, vm) = ShowMainWindowWithOneLine("Hi!" + Environment.NewLine + "Hello.");
            try
            {
                Assert.NotNull(vm.AudioVisualizer);
                vm.AudioVisualizer!.Focus();
                Settle(window);
                Assert.True(vm.AudioVisualizer.IsFocused);

                window.KeyPressQwerty(PhysicalKey.F11, RawInputModifiers.Control | RawInputModifiers.Shift);
                Settle(window);

                Assert.StartsWith("-", vm.Subtitles[0].Text);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [AvaloniaFact]
    public void ToggleDialogDashes_Runs_WhenVideoPlayerControlHasFocus()
    {
        WithShortcut(nameof(MainViewModel.ToggleDialogDashesCommand), Chord, () =>
        {
            var (window, vm) = ShowMainWindowWithOneLine("Hi!" + Environment.NewLine + "Hello.");
            try
            {
                var videoPlayer = vm.GetVideoPlayerControl();
                Assert.NotNull(videoPlayer);
                var slider = videoPlayer!.GetVisualDescendants().OfType<Slider>().First(s => s.Focusable);
                slider.Focus();
                Settle(window);
                Assert.True(videoPlayer.IsKeyboardFocusWithin);

                window.KeyPressQwerty(PhysicalKey.F11, RawInputModifiers.Control | RawInputModifiers.Shift);
                Settle(window);

                Assert.StartsWith("-", vm.Subtitles[0].Text);
            }
            finally
            {
                window.Close();
            }
        });
    }

    private static void WithShortcut(string actionName, string[] keys, Action test)
    {
        var original = Se.Settings.Shortcuts.Where(s => s.ActionName == actionName).ToList();
        Se.Settings.Shortcuts.RemoveAll(s => s.ActionName == actionName);
        Se.Settings.Shortcuts.Add(new SeShortCut(actionName, [.. keys], ShortcutCategory.SubtitleGridAndTextBox.ToString()));
        try
        {
            test();
        }
        finally
        {
            Se.Settings.Shortcuts.RemoveAll(s => s.ActionName == actionName);
            Se.Settings.Shortcuts.AddRange(original);
        }
    }

    private static (Window Window, MainViewModel Vm) ShowMainWindowWithOneLine(string text)
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        Locator.Services = services.BuildServiceProvider();

        var window = new Window { Width = 1400, Height = 900 };
        MainView.NextHostWindow = window;
        var view = new MainView();
        window.Content = view;
        window.Show();
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();

        var vm = (MainViewModel)view.DataContext!;
        window.SuppressSaveChangesPromptOnClose(vm);
        vm.Subtitles.Add(new SubtitleLineViewModel(new Paragraph(text, 0, 2000), null!) { Number = 1 });
        Settle(window);

        vm.SelectedSubtitleIndex = 0;
        vm.SubtitleGrid.SelectedItem = vm.Subtitles[0];
        Settle(window);

        return (window, vm);
    }

    private static void Settle(Window window)
    {
        for (var pump = 0; pump < 5; pump++)
        {
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }
    }
}
