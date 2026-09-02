using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Features.Main;

/// <summary>
/// Applying settings rebuilds the layout - and with it the subtitle grid - and refreshes the
/// subtitle format list. The fresh grid silently auto-selects row 0, and every format-combo
/// SelectionChanged raised by the list refresh read that row 0 back and scrolled to it, so the
/// row the user was editing was lost on Apply and on OK after Apply (issue #14421).
/// </summary>
public class ApplySettingsKeepsSelectionTests : IDisposable
{
    private readonly List<Window> _windows = new();

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            window.Close();
        }

        _windows.Clear();
    }

    [AvaloniaFact]
    public async Task ApplySettings_KeepsTheSelectedRow()
    {
        var (window, vm) = ShowMainWindowWithLines(300);

        vm.SelectAndScrollToSubtitle(vm.Subtitles[100]);
        await SettleAsync(window);
        Assert.Equal("Line 101", vm.SelectedSubtitle?.Text);
        Assert.Equal(100, vm.SubtitleGrid.SelectedIndex);

        vm.ApplySettings();
        await SettleAsync(window);
        await SettleAsync(window);

        Assert.Equal("Line 101", vm.SelectedSubtitle?.Text);
        Assert.Equal(100, vm.SubtitleGrid.SelectedIndex);
        Assert.Equal("Line 101", (vm.SubtitleGrid.SelectedItem as SubtitleLineViewModel)?.Text);
    }

    [AvaloniaFact]
    public async Task ApplySettings_Twice_KeepsTheSelectedRow()
    {
        // Apply followed by OK with a further change runs the apply path twice in a row.
        var (window, vm) = ShowMainWindowWithLines(300);

        vm.SelectAndScrollToSubtitle(vm.Subtitles[42]);
        await SettleAsync(window);

        vm.ApplySettings();
        await SettleAsync(window);
        vm.ApplySettings();
        await SettleAsync(window);
        await SettleAsync(window);

        Assert.Equal("Line 43", vm.SelectedSubtitle?.Text);
        Assert.Equal(42, vm.SubtitleGrid.SelectedIndex);
    }

    private (Window Window, MainViewModel Vm) ShowMainWindowWithLines(int lineCount)
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        Locator.Services = services.BuildServiceProvider();

        var window = new Window { Width = 1400, Height = 900 };
        _windows.Add(window);
        MainView.NextHostWindow = window;
        var view = new MainView();
        window.Content = view;
        window.Show();
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();

        var vm = (MainViewModel)view.DataContext!;
        window.SuppressSaveChangesPromptOnClose(vm);
        vm.Menu.IsVisible = true;

        for (var i = 0; i < lineCount; i++)
        {
            vm.Subtitles.Add(new SubtitleLineViewModel(new Paragraph($"Line {i + 1}", i * 2000, i * 2000 + 1500), null!)
            {
                Number = i + 1,
            });
        }

        Settle(window);
        return (window, vm);
    }

    private static void Settle(Window window)
    {
        for (var pump = 0; pump < 8; pump++)
        {
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }
    }

    // The scroll/selection restore is posted, so give the dispatcher a real tick as well.
    private static async Task SettleAsync(Window window)
    {
        Settle(window);
        await Task.Delay(50);
        Settle(window);
    }
}
