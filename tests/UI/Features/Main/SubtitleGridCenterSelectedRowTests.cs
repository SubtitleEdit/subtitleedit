using Avalonia;
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
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Main;

/// <summary>
/// "Subtitle grid, center when selecting prev/next row" (#14231). Centering used to be skipped
/// whenever the target row already was on screen - a guard meant for delete/insert, where moving
/// the rows out from under the user is wrong - so stepping down through the file left the
/// selection walking from the middle of the view to its bottom edge before anything scrolled,
/// and the view then jumped a half screen at a time. Plain arrow keys never centered at all:
/// they are handled by TableView's own list navigation, which scrolls only when the next row is
/// off screen.
/// </summary>
public class SubtitleGridCenterSelectedRowTests : IDisposable
{
    private readonly List<Window> _windows = new();

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            window.Close();
        }

        _windows.Clear();
        Se.Settings.General.SubtitleGridCenterSelectedRow = false;
        Se.Settings.General.PromptBeforeDelete = true;
    }

    [AvaloniaFact]
    public async Task GoToNextLine_Repeatedly_KeepsTheRowCentered()
    {
        Se.Settings.General.SubtitleGridCenterSelectedRow = true;
        var (window, vm) = ShowMainWindowWithLines(300);
        vm.SelectAndScrollToSubtitle(vm.Subtitles[100]);
        await SettleAsync(window);

        var offsets = await Step(window, vm, 8, () => vm.GoToNextLineCommand.Execute(null));

        AssertAllCentered(offsets);
    }

    [AvaloniaFact]
    public async Task GoToPreviousLine_Repeatedly_KeepsTheRowCentered()
    {
        Se.Settings.General.SubtitleGridCenterSelectedRow = true;
        var (window, vm) = ShowMainWindowWithLines(300);
        vm.SelectAndScrollToSubtitle(vm.Subtitles[200]);
        await SettleAsync(window);

        var offsets = await Step(window, vm, 8, () => vm.GoToPreviousLineCommand.Execute(null));

        AssertAllCentered(offsets);
    }

    [AvaloniaFact]
    public async Task ArrowDown_InTheGrid_KeepsTheRowCentered()
    {
        Se.Settings.General.SubtitleGridCenterSelectedRow = true;
        var (window, vm) = ShowMainWindowWithLines(300);
        await FocusRowInGrid(window, vm, 100);

        var offsets = await Step(window, vm, 8,
            () => window.KeyPressQwerty(PhysicalKey.ArrowDown, RawInputModifiers.None));

        Assert.Equal(108, vm.SelectedSubtitleIndex);
        AssertAllCentered(offsets);
    }

    [AvaloniaFact]
    public async Task ArrowUp_InTheGrid_KeepsTheRowCentered()
    {
        Se.Settings.General.SubtitleGridCenterSelectedRow = true;
        var (window, vm) = ShowMainWindowWithLines(300);
        await FocusRowInGrid(window, vm, 200);

        var offsets = await Step(window, vm, 8,
            () => window.KeyPressQwerty(PhysicalKey.ArrowUp, RawInputModifiers.None));

        Assert.Equal(192, vm.SelectedSubtitleIndex);
        AssertAllCentered(offsets);
    }

    [AvaloniaFact]
    public async Task ArrowDown_WithTheSettingOff_LeavesTheScrollOffsetAlone()
    {
        Se.Settings.General.SubtitleGridCenterSelectedRow = false;
        var (window, vm) = ShowMainWindowWithLines(300);
        await FocusRowInGrid(window, vm, 100);

        var scrollViewer = ScrollViewerOf(vm);
        var before = scrollViewer.Offset.Y;
        window.KeyPressQwerty(PhysicalKey.ArrowDown, RawInputModifiers.None);
        await SettleAsync(window);

        Assert.Equal(101, vm.SelectedSubtitleIndex);
        Assert.Equal(before, scrollViewer.Offset.Y);
    }

    [AvaloniaFact]
    public async Task Delete_WithTheSettingOn_StillLeavesTheScrollOffsetAlone()
    {
        // The already-visible guard stays in place for the commands that add or remove rows:
        // re-centering there moves the rows out from under the user (see the delete/insert tests).
        Se.Settings.General.SubtitleGridCenterSelectedRow = true;
        Se.Settings.General.PromptBeforeDelete = false;
        var (window, vm) = ShowMainWindowWithLines(300);
        vm.SelectAndScrollToSubtitle(vm.Subtitles[108]);
        await SettleAsync(window);

        var scrollViewer = ScrollViewerOf(vm);
        var before = scrollViewer.Offset.Y;
        Assert.True(before > 0);

        await vm.DeleteSelectedLinesCommand.ExecuteAsync(null);
        await SettleAsync(window);

        Assert.Equal("Line 110", vm.SelectedSubtitle?.Text);
        Assert.Equal(before, scrollViewer.Offset.Y);
    }

    private static async Task<List<double>> Step(Window window, MainViewModel vm, int steps, Action move)
    {
        var offsets = new List<double>();
        for (var i = 0; i < steps; i++)
        {
            move();
            await SettleAsync(window);
            offsets.Add(RowCenterOffsetFromViewportCenter(vm));
        }

        return offsets;
    }

    private static void AssertAllCentered(IReadOnlyList<double> offsets)
    {
        Assert.All(offsets, offset => Assert.True(Math.Abs(offset) < 5,
            "selected row drifted away from the middle of the view, per-step offsets: " +
            string.Join(", ", offsets.Select(o => o.ToString("0.0")))));
    }

    /// <summary>How far the selected row's middle sits from the middle of the viewport.</summary>
    private static double RowCenterOffsetFromViewportCenter(MainViewModel vm)
    {
        var scrollViewer = ScrollViewerOf(vm);
        var row = vm.SubtitleGrid.ContainerFromItem(vm.SelectedSubtitle!)!;
        var viewportOrigin = (Visual?)scrollViewer.Presenter ?? scrollViewer;
        var top = ((Visual)row).TranslatePoint(new Point(0, 0), viewportOrigin)!.Value.Y;
        return top + row.Bounds.Height / 2.0 - scrollViewer.Viewport.Height / 2.0;
    }

    private static ScrollViewer ScrollViewerOf(MainViewModel vm)
        => vm.SubtitleGrid.GetVisualDescendants().OfType<ScrollViewer>().First();

    private static async Task FocusRowInGrid(Window window, MainViewModel vm, int index)
    {
        vm.SelectAndScrollToSubtitle(vm.Subtitles[index]);
        await SettleAsync(window);
        TableViewExtras.FocusRow(vm.SubtitleGrid);
        await SettleAsync(window);
        Assert.True(vm.SubtitleGrid.IsKeyboardFocusWithin);
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
