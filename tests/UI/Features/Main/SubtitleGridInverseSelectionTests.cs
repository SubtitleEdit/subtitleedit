using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Main;

/// <summary>
/// "Inverse selection" used to throw the grid to the top of the file: the inverted selection is
/// handed to ApplyGridSelection, which made its first row current - and with a single row
/// selected that first row is line 1, so the view jumped to row 0 and the edit box showed line 1.
/// SE 4 flipped each row's Selected flag and never touched the focused row or the scroll offset.
/// </summary>
public class SubtitleGridInverseSelectionTests : IDisposable
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
    public async Task InverseSelection_FromTheMiddle_LeavesTheViewWhereItWas()
    {
        var (window, vm) = ShowMainWindowWithLines(300);
        vm.SelectAndScrollToSubtitle(vm.Subtitles[150]);
        await SettleAsync(window);

        var scrollViewer = vm.SubtitleGrid.GetVisualDescendants().OfType<ScrollViewer>().First();
        var before = scrollViewer.Offset.Y;
        Assert.True(before > 0);

        vm.InverseSelectionCommand.Execute(null);
        await SettleAsync(window);

        Assert.Equal(before, scrollViewer.Offset.Y);
        Assert.Equal("Line 152", vm.SelectedSubtitle?.Text);
        Assert.Equal(151, vm.SelectedSubtitleIndex);
    }

    [AvaloniaFact]
    public async Task InverseSelection_InvertsEveryRow()
    {
        var (window, vm) = ShowMainWindowWithLines(300);
        vm.SelectAndScrollToSubtitle(vm.Subtitles[150]);
        await SettleAsync(window);

        vm.InverseSelectionCommand.Execute(null);
        await SettleAsync(window);

        var selected = vm.SubtitleGridSelectedItems;
        Assert.Equal(299, selected.Count);
        Assert.DoesNotContain(vm.Subtitles[150], selected);
        Assert.Contains(vm.Subtitles[0], selected);
        Assert.Contains(vm.Subtitles[299], selected);
    }

    [AvaloniaFact]
    public async Task InverseSelection_OnTheLastRow_TakesThePreviousRowAsCurrent()
    {
        var (window, vm) = ShowMainWindowWithLines(300);
        vm.SelectAndScrollToSubtitle(vm.Subtitles[299]);
        await SettleAsync(window);

        vm.InverseSelectionCommand.Execute(null);
        await SettleAsync(window);

        Assert.Equal(298, vm.SelectedSubtitleIndex);
        Assert.True(TableViewExtras.IsRowFullyVisible(vm.SubtitleGrid, vm.Subtitles[298]));
    }

    [AvaloniaFact]
    public async Task InverseSelection_WithEverythingSelected_KeepsTheCurrentRow()
    {
        // The inverse of "everything" is nothing, but SelectionMode.AlwaysSelected would
        // re-pick row 0 and scroll to the top; collapse to the row the user is on instead.
        var (window, vm) = ShowMainWindowWithLines(300);
        vm.SelectAndScrollToSubtitle(vm.Subtitles[150]);
        await SettleAsync(window);
        var scrollViewer = vm.SubtitleGrid.GetVisualDescendants().OfType<ScrollViewer>().First();
        var before = scrollViewer.Offset.Y;

        vm.SelectAllLinesCommand.Execute(null);
        await SettleAsync(window);
        Assert.Equal(300, vm.SubtitleGridSelectedItems.Count);

        vm.InverseSelectionCommand.Execute(null);
        await SettleAsync(window);

        Assert.Equal(before, scrollViewer.Offset.Y);
        Assert.Equal(150, vm.SelectedSubtitleIndex);
        Assert.Single(vm.SubtitleGridSelectedItems);
    }

    [AvaloniaFact]
    public async Task InverseSelection_Twice_ComesBackToTheOriginalSelection()
    {
        var (window, vm) = ShowMainWindowWithLines(50);
        vm.SelectAndScrollToSubtitle(vm.Subtitles[10]);
        await SettleAsync(window);

        vm.InverseSelectionCommand.Execute(null);
        await SettleAsync(window);
        vm.InverseSelectionCommand.Execute(null);
        await SettleAsync(window);

        var selected = vm.SubtitleGridSelectedItems;
        Assert.Single(selected);
        Assert.Same(vm.Subtitles[10], selected[0]);
        Assert.Equal(10, vm.SelectedSubtitleIndex);
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

    private static async Task SettleAsync(Window window)
    {
        Settle(window);
        await Task.Delay(50);
        Settle(window);
    }
}
