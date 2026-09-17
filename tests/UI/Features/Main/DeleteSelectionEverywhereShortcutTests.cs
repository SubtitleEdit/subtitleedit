using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Options.Shortcuts;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Main;

/// <summary>
/// "Delete selection (everywhere)" (#14966): the plain delete as a shortcut that is not limited to
/// the subtitle grid, so it can be used with the waveform or video player focused. Before this
/// only "Ripple delete selection" could be bound everywhere, which also retimes the following lines.
/// </summary>
public class DeleteSelectionEverywhereShortcutTests
{
    [AvaloniaFact]
    public void IsRegisteredAsAnEverywhereShortcut()
    {
        var (window, vm) = ShowMainWindowWithLines();
        try
        {
            var all = ShortcutsMain.GetAllShortcuts(vm);
            var everywhere = all.Single(s => s.Name == nameof(MainViewModel.DeleteSelectedLinesEverywhereCommand));
            var grid = all.Single(s => s.Name == nameof(MainViewModel.DeleteSelectedLinesCommand));

            Assert.Equal(ShortcutCategory.General, everywhere.Category);
            Assert.Equal(ShortcutCategory.SubtitleGrid, grid.Category);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void WaveformFocused_DeletesTheSelectedLineWithoutRetimingTheRest()
    {
        WithShortcut(nameof(MainViewModel.DeleteSelectedLinesEverywhereCommand), ["Control", "Shift", nameof(Key.F9)], () =>
        {
            var (window, vm) = ShowMainWindowWithLines();
            try
            {
                FocusWaveform(window, vm);

                window.KeyPressQwerty(PhysicalKey.F9, RawInputModifiers.Control | RawInputModifiers.Shift);
                Settle(window);

                Assert.Equal(["Line 2", "Line 3"], vm.Subtitles.Select(p => p.Text));
                Assert.Equal(6000, vm.Subtitles[0].StartTime.TotalMilliseconds);
                Assert.Equal(11000, vm.Subtitles[1].StartTime.TotalMilliseconds);
            }
            finally
            {
                window.Close();
            }
        });
    }

    /// <summary>
    /// The waveform has its own hardcoded Delete handling. With bare Delete bound here as well,
    /// the window's tunnelling shortcut dispatch handles the key first, so only one line goes.
    /// </summary>
    [AvaloniaFact]
    public void BareDelete_WaveformFocused_DeletesOnlyOneLine()
    {
        WithShortcut(nameof(MainViewModel.DeleteSelectedLinesEverywhereCommand), [nameof(Key.Delete)], () =>
        {
            var (window, vm) = ShowMainWindowWithLines();
            try
            {
                FocusWaveform(window, vm);

                window.KeyPressQwerty(PhysicalKey.Delete, RawInputModifiers.None);
                Settle(window);

                Assert.Equal(["Line 2", "Line 3"], vm.Subtitles.Select(p => p.Text));
            }
            finally
            {
                window.Close();
            }
        });
    }

    [AvaloniaFact]
    public void BareDelete_TextBoxFocused_StillEditsTheText()
    {
        WithShortcut(nameof(MainViewModel.DeleteSelectedLinesEverywhereCommand), [nameof(Key.Delete)], () =>
        {
            var (window, vm) = ShowMainWindowWithLines();
            try
            {
                vm.EditTextBox.Focus();
                vm.EditTextBox.CaretIndex = 0;
                Settle(window);

                window.KeyPressQwerty(PhysicalKey.Delete, RawInputModifiers.None);
                Settle(window);

                Assert.Equal(3, vm.Subtitles.Count);
                Assert.Equal("ine 1", vm.Subtitles[0].Text);
            }
            finally
            {
                window.Close();
            }
        });
    }

    private static void FocusWaveform(Window window, MainViewModel vm)
    {
        var av = vm.AudioVisualizer!;
        av.WavePeaks = MakePeaks(126, 60);
        av.SetPosition(0, vm.Subtitles, 0, 0, new List<SubtitleLineViewModel> { vm.Subtitles[0] });
        Settle(window);

        av.Focus();
        Settle(window);
        Assert.True(av.IsFocused);
    }

    private static WavePeakData2 MakePeaks(int sampleRate, int seconds)
    {
        var peaks = new WavePeak2[sampleRate * seconds];
        for (var i = 0; i < peaks.Length; i++)
        {
            peaks[i] = new WavePeak2(8000, -8000);
        }

        return new WavePeakData2(sampleRate, peaks);
    }

    /// <summary>
    /// The settings singleton is shared across the whole test run, so give the action exactly one
    /// binding - the one under test - and put the original bindings back afterwards.
    /// </summary>
    private static void WithShortcut(string actionName, string[] keys, Action test)
    {
        var promptBeforeDelete = Se.Settings.General.PromptBeforeDelete;
        Se.Settings.General.PromptBeforeDelete = false;
        var original = Se.Settings.Shortcuts.Where(s => s.ActionName == actionName).ToList();
        Se.Settings.Shortcuts.RemoveAll(s => s.ActionName == actionName);
        Se.Settings.Shortcuts.Add(new SeShortCut(actionName, [.. keys]));
        try
        {
            test();
        }
        finally
        {
            Se.Settings.Shortcuts.RemoveAll(s => s.ActionName == actionName);
            Se.Settings.Shortcuts.AddRange(original);
            Se.Settings.General.PromptBeforeDelete = promptBeforeDelete;
        }
    }

    private static (Window Window, MainViewModel Vm) ShowMainWindowWithLines()
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
        for (var i = 0; i < 3; i++)
        {
            vm.Subtitles.Add(new SubtitleLineViewModel(new Paragraph($"Line {i + 1}", i * 5000 + 1000, i * 5000 + 3000), null!)
            {
                Number = i + 1,
            });
        }

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
