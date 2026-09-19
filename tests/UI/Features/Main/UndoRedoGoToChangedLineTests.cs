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
/// "Undo/redo: go to changed line" (#15029): undo/redo keep the current row by default (#11308),
/// but when syncing with "move text after cursor to next, go to next and play" the user wants
/// Ctrl+Z to land back on the line the text came from.
/// </summary>
public class UndoRedoGoToChangedLineTests : IDisposable
{
    private readonly List<Window> _windows = new();

    public void Dispose()
    {
        Se.Settings.Tools.UndoRedoGoToChangedLine = false;
        foreach (var window in _windows)
        {
            window.Close();
        }

        _windows.Clear();
    }

    [AvaloniaFact]
    public async Task Undo_OptionOn_SelectsFirstChangedRow()
    {
        Se.Settings.Tools.UndoRedoGoToChangedLine = true;
        var (window, vm) = ShowMainWindowWithLines(50);
        await MoveTextFromLine11ToLine12AndGoToLine12(window, vm);

        vm.UndoCommand.Execute(null);
        await SettleAsync(window);

        Assert.Equal(10, vm.SelectedSubtitleIndex);
        Assert.Equal("Line 11", vm.SelectedSubtitle?.Text);

        vm.RedoCommand.Execute(null);
        await SettleAsync(window);

        // Rows 10 and 11 both change again; the first one is already current.
        Assert.Equal(10, vm.SelectedSubtitleIndex);
        Assert.Equal("Line", vm.SelectedSubtitle?.Text);
    }

    [AvaloniaFact]
    public async Task Undo_OptionOff_KeepsCurrentRow()
    {
        Se.Settings.Tools.UndoRedoGoToChangedLine = false;
        var (window, vm) = ShowMainWindowWithLines(50);
        await MoveTextFromLine11ToLine12AndGoToLine12(window, vm);

        vm.UndoCommand.Execute(null);
        await SettleAsync(window);

        Assert.Equal(11, vm.SelectedSubtitleIndex);
        Assert.Equal("Line 12", vm.SelectedSubtitle?.Text);
    }

    [Fact]
    public void FindFirstChangedRowIndex_FindsTextTimeAndCountChanges()
    {
        var before = MakeRows("a", "b", "c");

        Assert.Equal(-1, UndoRedoChangedRowFinder.FindFirstChangedRowIndex(before, MakeRows("a", "b", "c")));
        Assert.Equal(1, UndoRedoChangedRowFinder.FindFirstChangedRowIndex(before, MakeRows("a", "x", "y")));
        Assert.Equal(2, UndoRedoChangedRowFinder.FindFirstChangedRowIndex(before, MakeRows("a", "b")));
        Assert.Equal(3, UndoRedoChangedRowFinder.FindFirstChangedRowIndex(before, MakeRows("a", "b", "c", "d")));
        Assert.Equal(0, UndoRedoChangedRowFinder.FindFirstChangedRowIndex(before, MakeRows("b", "c")));

        var retimed = MakeRows("a", "b", "c");
        retimed[2].EndTime = retimed[2].EndTime.Add(TimeSpan.FromMilliseconds(100));
        Assert.Equal(2, UndoRedoChangedRowFinder.FindFirstChangedRowIndex(before, retimed));
    }

    private static SubtitleLineViewModel[] MakeRows(params string[] texts)
    {
        return texts
            .Select((text, i) => new SubtitleLineViewModel(new Paragraph(text, i * 2000, i * 2000 + 1500), null!))
            .ToArray();
    }

    /// <summary>
    /// What "move text after cursor to next subtitle, go to next" leaves behind: the snapshot
    /// taken before the edit, rows 10 and 11 rewritten, and row 11 current.
    /// </summary>
    private static async Task MoveTextFromLine11ToLine12AndGoToLine12(Window window, MainViewModel vm)
    {
        vm.SelectAndScrollToSubtitle(vm.Subtitles[10]);
        await SettleAsync(window);
        GetUndoRedoManager(vm).Do(vm.MakeUndoRedoObject("before edit"));

        vm.Subtitles[10].Text = "Line";
        vm.Subtitles[11].Text = "11\nLine 12";
        vm.SelectAndScrollToSubtitle(vm.Subtitles[11]);
        await SettleAsync(window);
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
            var text = $"Line {i + 1}";
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
