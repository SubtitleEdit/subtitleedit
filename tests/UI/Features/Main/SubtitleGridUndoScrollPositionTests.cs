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
using Nikse.SubtitleEdit.Logic.UndoRedo;
using System.Reflection;

namespace UITests.Features.Main;

/// <summary>
/// Undo/redo rebuild the whole grid from the snapshot's row copies (ReplaceSubtitles detaches the
/// ItemsSource on the way, which drops the scroll offset to 0). The follow-up scroll then found
/// the current row off screen and parked it at the top edge of the grid - a jump on every Undo,
/// worst when working near the bottom of the view (#14517). The restored row must come back at
/// the same viewport height it had before the command.
/// </summary>
public class SubtitleGridUndoScrollPositionTests : IDisposable
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
    public async Task Undo_KeepsTheCurrentRowAtTheSameViewportHeight()
    {
        Se.Settings.General.SubtitleGridCenterSelectedRow = false;
        var (window, vm) = ShowMainWindowWithLines(300);

        // The change-detection snapshot is taken with whatever row was current at the time -
        // here one far above the view the user then scrolls to.
        vm.SelectAndScrollToSubtitle(vm.Subtitles[5]);
        await SettleAsync(window);
        GetUndoRedoManager(vm).Do(vm.MakeUndoRedoObject("before edit"));

        // Scroll into the file, then pick a row near the bottom of the view.
        vm.SelectAndScrollToSubtitle(vm.Subtitles[100]);
        await SettleAsync(window);
        vm.SelectAndScrollToSubtitle(vm.Subtitles[112]);
        await SettleAsync(window);

        var scrollViewer = vm.SubtitleGrid.GetVisualDescendants().OfType<ScrollViewer>().First();
        var offsetBefore = scrollViewer.Offset.Y;
        Assert.True(offsetBefore > 0);
        var rowTopBefore = TableViewExtras.GetRowViewportTop(vm.SubtitleGrid, vm.Subtitles[112]);
        Assert.NotNull(rowTopBefore);
        Assert.True(rowTopBefore > scrollViewer.Viewport.Height / 2, $"row should sit in the lower half, was at {rowTopBefore}");

        // An unrecorded edit for Undo to revert.
        vm.Subtitles[112].Text = "edited";
        await SettleAsync(window);

        vm.UndoCommand.Execute(null);
        await SettleAsync(window);
        await SettleAsync(window);

        Assert.Equal("Line 113", vm.SelectedSubtitle?.Text);
        var rowTopAfter = TableViewExtras.GetRowViewportTop(vm.SubtitleGrid, vm.SelectedSubtitle!);
        Assert.NotNull(rowTopAfter);
        // The row's place on screen is what the user sees; the raw pixel offset legitimately
        // differs, as the rebuilt panel re-estimates its extent from scratch.
        Assert.InRange(rowTopAfter!.Value, rowTopBefore!.Value - 1, rowTopBefore.Value + 1);
    }

    [AvaloniaFact]
    public async Task Redo_KeepsTheCurrentRowAtTheSameViewportHeight()
    {
        Se.Settings.General.SubtitleGridCenterSelectedRow = false;
        var (window, vm) = ShowMainWindowWithLines(300);

        // The change-detection snapshot is taken with whatever row was current at the time -
        // here one far above the view the user then scrolls to.
        vm.SelectAndScrollToSubtitle(vm.Subtitles[5]);
        await SettleAsync(window);
        GetUndoRedoManager(vm).Do(vm.MakeUndoRedoObject("before edit"));

        vm.SelectAndScrollToSubtitle(vm.Subtitles[100]);
        await SettleAsync(window);
        vm.SelectAndScrollToSubtitle(vm.Subtitles[112]);
        await SettleAsync(window);

        vm.Subtitles[112].Text = "edited";
        await SettleAsync(window);
        vm.UndoCommand.Execute(null);
        await SettleAsync(window);
        await SettleAsync(window);

        var rowTopBefore = TableViewExtras.GetRowViewportTop(vm.SubtitleGrid, vm.SelectedSubtitle!);
        Assert.NotNull(rowTopBefore);

        vm.RedoCommand.Execute(null);
        await SettleAsync(window);
        await SettleAsync(window);

        Assert.Equal("edited", vm.SelectedSubtitle?.Text);
        var rowTopAfter = TableViewExtras.GetRowViewportTop(vm.SubtitleGrid, vm.SelectedSubtitle!);
        Assert.NotNull(rowTopAfter);
        Assert.InRange(rowTopAfter!.Value, rowTopBefore!.Value - 1, rowTopBefore.Value + 1);
    }

    private static IUndoRedoManager GetUndoRedoManager(MainViewModel vm)
    {
        var field = typeof(MainViewModel).GetField("_undoRedoManager", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?? throw new InvalidOperationException("_undoRedoManager not found");
        return (IUndoRedoManager)field.GetValue(vm)!;
    }

    private (Window Window, MainViewModel Vm) ShowMainWindowWithLines(int lineCount)
    {
        var (window, vm) = CreateMainViewModel();
        vm.Menu.IsVisible = true;
        for (var i = 0; i < lineCount; i++)
        {
            // Mixed one- and two-line rows: with uniform rows the rebuilt panel maps the old
            // pixel offset back to the same row by luck, and the drift never shows.
            var text = i % 3 == 0 ? $"Line {i + 1}\nsecond line" : $"Line {i + 1}";
            vm.Subtitles.Add(new SubtitleLineViewModel(new Paragraph(text, i * 2000, i * 2000 + 1500), null!)
            {
                Number = i + 1,
            });
        }

        Settle(window);
        return (window, vm);
    }

    private (Window Window, MainViewModel Vm) CreateMainViewModel()
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
        return (window, vm);
    }

    private static async Task SettleAsync(Window window)
    {
        Settle(window);
        await Task.Delay(50);
        Settle(window);
    }

    private static void Settle(Window window)
    {
        for (var pump = 0; pump < 8; pump++)
        {
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }
    }
}
