using System.Reflection;
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
/// The lines "Merge lines with same text" / "Merge lines with same time codes" return go back
/// onto the rows they came from, and the row that was current stays current - the grid used to
/// jump to row 0 after every merge, losing the user's place in the file.
/// </summary>
public class ApplyMergeDialogResultTests
{
    [AvaloniaFact]
    public async Task SelectedLineSurvives_StaysCurrentOnTheSameRow()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddLine(vm, "Same", 0, 1000);
            AddLine(vm, "Same", 1000, 2000);
            AddLine(vm, "Three", 2000, 3000);
            AddLine(vm, "Four", 3000, 4000);
            var rows = vm.Subtitles.ToList();
            SelectInGrid(vm, rows[2]);
            Settle(window);

            var selectedBefore = vm.SelectedSubtitle;
            var idsBefore = vm.Subtitles.Select(p => p.Id).ToList();
            var merged = MergeFirstTwo(rows);
            ApplyMergeDialogResult(vm, merged, selectedBefore, idsBefore);
            await SettleAsync(window);

            Assert.Equal(new[] { "Same", "Three", "Four" }, vm.Subtitles.Select(p => p.Text));
            Assert.Equal(2000, vm.Subtitles[0].EndTime.TotalMilliseconds);
            Assert.Equal(new[] { 1, 2, 3 }, vm.Subtitles.Select(p => p.Number));

            // Same row instances for every line that survived, and the current row is still "Three".
            Assert.Same(rows[0], vm.Subtitles[0]);
            Assert.Same(rows[2], vm.Subtitles[1]);
            Assert.Same(rows[3], vm.Subtitles[2]);
            Assert.Same(rows[2], vm.SelectedSubtitle);
            Assert.Same(rows[2], vm.SubtitleGrid.SelectedItem);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public async Task SelectedLineMergedAway_MakesTheMergedLineCurrent()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddLine(vm, "One", 0, 1000);
            AddLine(vm, "Same", 1000, 2000);
            AddLine(vm, "Same", 2000, 3000);
            AddLine(vm, "Four", 3000, 4000);
            var rows = vm.Subtitles.ToList();

            // The second of the two "Same" lines is current - it is consumed by the merge.
            SelectInGrid(vm, rows[2]);
            Settle(window);

            var selectedBefore = vm.SelectedSubtitle;
            var idsBefore = vm.Subtitles.Select(p => p.Id).ToList();
            var merged = new List<SubtitleLineViewModel>
            {
                new(rows[0]),
                new(rows[1]) { EndTime = rows[2].EndTime },
                new(rows[3]),
            };
            ApplyMergeDialogResult(vm, merged, selectedBefore, idsBefore);
            await SettleAsync(window);

            Assert.Equal(new[] { "One", "Same", "Four" }, vm.Subtitles.Select(p => p.Text));
            Assert.Same(rows[1], vm.Subtitles[1]);
            Assert.Same(rows[1], vm.SelectedSubtitle);
            Assert.Same(rows[1], vm.SubtitleGrid.SelectedItem);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public async Task NothingSelected_SelectsTheFirstRow()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddLine(vm, "Same", 0, 1000);
            AddLine(vm, "Same", 1000, 2000);
            AddLine(vm, "Three", 2000, 3000);
            var rows = vm.Subtitles.ToList();
            Settle(window);

            var idsBefore = vm.Subtitles.Select(p => p.Id).ToList();
            ApplyMergeDialogResult(vm, MergeFirstTwo(rows), null, idsBefore);
            await SettleAsync(window);

            Assert.Equal(2, vm.Subtitles.Count);
            Assert.Same(rows[0], vm.SelectedSubtitle);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    /// <summary>
    /// The view must not move either: merging lines far above the current row removes those
    /// rows in place, and the row the user was looking at stays on screen where it was.
    /// </summary>
    [AvaloniaFact]
    public async Task MergeAboveTheView_KeepsTheCurrentRowOnScreen()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            for (var i = 0; i < 300; i++)
            {
                AddLine(vm, i < 2 ? "Same" : $"Line {i + 1}", i * 2000, i * 2000 + 1500);
            }

            Settle(window);
            var rows = vm.Subtitles.ToList();
            vm.SelectAndScrollToSubtitle(rows[200]);
            await SettleAsync(window);
            Assert.True(TableViewExtras.IsRowFullyVisible(vm.SubtitleGrid, rows[200]));

            var selectedBefore = vm.SelectedSubtitle;
            var idsBefore = vm.Subtitles.Select(p => p.Id).ToList();
            ApplyMergeDialogResult(vm, MergeFirstTwo(rows), selectedBefore, idsBefore);
            await SettleAsync(window);

            Assert.Equal(299, vm.Subtitles.Count);
            Assert.Same(rows[200], vm.SelectedSubtitle);
            Assert.Same(rows[200], vm.SubtitleGrid.SelectedItem);
            Assert.Equal(199, vm.SubtitleGrid.SelectedIndex);
            Assert.True(TableViewExtras.IsRowFullyVisible(vm.SubtitleGrid, rows[200]));
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    /// <summary>
    /// A line the grid has no row for (a fresh id) cannot be removed in place; the rows are then
    /// rebuilt, and the current row must still come back.
    /// </summary>
    [AvaloniaFact]
    public async Task UnknownLine_RebuildsTheRowsAndStillKeepsTheCurrentRow()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddLine(vm, "Same", 0, 1000);
            AddLine(vm, "Same", 1000, 2000);
            AddLine(vm, "Three", 2000, 3000);
            var rows = vm.Subtitles.ToList();
            SelectInGrid(vm, rows[2]);
            Settle(window);

            var selectedBefore = vm.SelectedSubtitle;
            var idsBefore = vm.Subtitles.Select(p => p.Id).ToList();
            var merged = new List<SubtitleLineViewModel>
            {
                new(rows[0], generateNewId: true) { EndTime = rows[1].EndTime },
                new(rows[2]),
            };
            ApplyMergeDialogResult(vm, merged, selectedBefore, idsBefore);
            await SettleAsync(window);

            Assert.Equal(new[] { "Same", "Three" }, vm.Subtitles.Select(p => p.Text));
            Assert.NotSame(rows[0], vm.Subtitles[0]);
            Assert.Same(rows[2], vm.Subtitles[1]);
            Assert.Same(rows[2], vm.SelectedSubtitle);
            Assert.Same(rows[2], vm.SubtitleGrid.SelectedItem);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    /// <summary>The shape the merge dialogs return: copies that keep the ids, the merged line built
    /// from the first line of its group with the group's last end time.</summary>
    private static List<SubtitleLineViewModel> MergeFirstTwo(List<SubtitleLineViewModel> rows)
    {
        var result = new List<SubtitleLineViewModel> { new(rows[0]) { EndTime = rows[1].EndTime } };
        result.AddRange(rows.Skip(2).Select(p => new SubtitleLineViewModel(p)));
        return result;
    }

    private static void ApplyMergeDialogResult(
        MainViewModel vm,
        List<SubtitleLineViewModel> mergedLines,
        SubtitleLineViewModel? selectedBefore,
        IReadOnlyList<Guid> idsBefore)
    {
        typeof(MainViewModel)
            .GetMethod("ApplyMergeDialogResult", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(vm, new object?[] { mergedLines, selectedBefore, idsBefore });
    }

    // Assigning SelectedSubtitle alone leaves SubtitleGrid.SelectedItems empty.
    private static void SelectInGrid(MainViewModel vm, SubtitleLineViewModel row)
    {
        vm.SubtitleGrid.SelectedItems?.Clear();
        vm.SubtitleGrid.SelectedItem = row;
    }

    private static void Settle(Window window)
    {
        for (var pump = 0; pump < 8; pump++)
        {
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }
    }

    // The selection is posted, so give the dispatcher a real tick as well.
    private static async Task SettleAsync(Window window)
    {
        Settle(window);
        await Task.Delay(50);
        Settle(window);
    }

    private static (Window Window, MainViewModel Vm) CreateMainViewModel()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        Locator.Services = services.BuildServiceProvider();

        var window = new Window { Width = 1200, Height = 800 };
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

    private static SubtitleLineViewModel AddLine(MainViewModel vm, string text, int startMs, int endMs)
    {
        var line = new SubtitleLineViewModel(new Paragraph(text, startMs, endMs), null!)
        {
            Number = vm.Subtitles.Count + 1,
        };

        vm.Subtitles.Add(line);
        return line;
    }

    private static void CloseWindow(Window window, MainViewModel vm)
    {
        foreach (var ownedWindow in window.OwnedWindows.ToArray())
        {
            ownedWindow.Close();
        }

        window.Closing -= vm.OnClosing;
        if (window.IsVisible)
        {
            window.Close();
        }
    }
}
