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
using Nikse.SubtitleEdit.Features.Main.MainHelpers;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Reflection;

namespace UITests.Features.Main;

/// <summary>
/// What the subtitle grid does with focus, the scroll offset and the selection right after a
/// command deletes or inserts rows. Three things used to go wrong on a plain Delete alone:
/// removing the focused row's container dropped keyboard focus to null (the next arrow key then
/// walked into the menu bar - the #13182/#13111 pattern), SelectionMode.AlwaysSelected re-picked
/// row 0 and posted Avalonia's own scroll-to-top ahead of ours (the grid jumped even though the
/// next line was fully visible), and that row-0 re-pick leaked into SelectedSubtitle through the
/// TwoWay binding. The rest covers the reference-row edge cases of the merge/insert/duplicate
/// commands found in the same review.
/// </summary>
public class SubtitleGridDeleteInsertFocusScrollTests : IDisposable
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
    public async Task Delete_FocusedRow_KeepsKeyboardFocusInGrid()
    {
        Se.Settings.General.PromptBeforeDelete = false;
        var (window, vm) = ShowMainWindowWithLines(300);

        vm.SelectAndScrollToSubtitle(vm.Subtitles[10]);
        Settle(window);
        TableViewExtras.FocusRow(vm.SubtitleGrid);
        Settle(window);
        Assert.True(vm.SubtitleGrid.IsKeyboardFocusWithin);

        await vm.DeleteSelectedLinesCommand.ExecuteAsync(null);
        await SettleAsync(window);

        Assert.True(vm.SubtitleGrid.IsKeyboardFocusWithin,
            $"focus left the grid after delete: {window.FocusManager?.GetFocusedElement()?.GetType().Name ?? "<null>"}");
        Assert.Equal("Line 12", vm.SelectedSubtitle?.Text);

        window.KeyPressQwerty(PhysicalKey.ArrowDown, RawInputModifiers.None);
        await SettleAsync(window);

        Assert.False(window.FocusManager?.GetFocusedElement() is MenuItem, "the arrow key went to the menu bar");
        Assert.Equal("Line 13", vm.SelectedSubtitle?.Text);
    }

    [AvaloniaFact]
    public async Task Delete_MiddleRow_DoesNotMoveTheScrollOffset()
    {
        Se.Settings.General.PromptBeforeDelete = false;
        Se.Settings.General.SubtitleGridCenterSelectedRow = false;
        var (window, vm) = ShowMainWindowWithLines(300);

        vm.SelectAndScrollToSubtitle(vm.Subtitles[100]);
        await SettleAsync(window);
        vm.SelectAndScrollToSubtitle(vm.Subtitles[108]);
        await SettleAsync(window);
        var scrollViewer = vm.SubtitleGrid.GetVisualDescendants().OfType<ScrollViewer>().First();
        var offsetBefore = scrollViewer.Offset.Y;
        Assert.True(offsetBefore > 0);
        Assert.True(TableViewExtras.IsRowFullyVisible(vm.SubtitleGrid, vm.Subtitles[109]));

        await vm.DeleteSelectedLinesCommand.ExecuteAsync(null);
        await SettleAsync(window);

        Assert.Equal("Line 110", vm.SelectedSubtitle?.Text);
        Assert.Equal(offsetBefore, scrollViewer.Offset.Y);
    }

    [AvaloniaFact]
    public async Task Delete_NeverShowsRowZeroAsSelectedInBetween()
    {
        Se.Settings.General.PromptBeforeDelete = false;
        var (window, vm) = ShowMainWindowWithLines(50);

        vm.SelectAndScrollToSubtitle(vm.Subtitles[20]);
        await SettleAsync(window);

        var selectedDuringRemoval = new List<string?>();
        vm.Subtitles.CollectionChanged += (_, _) => selectedDuringRemoval.Add(vm.SelectedSubtitle?.Text);

        await vm.DeleteSelectedLinesCommand.ExecuteAsync(null);
        await SettleAsync(window);

        Assert.DoesNotContain("Line 1", selectedDuringRemoval);
        Assert.Equal("Line 22", vm.SelectedSubtitle?.Text);
        Assert.Equal(49, vm.Subtitles.Count);
    }

    [AvaloniaFact]
    public async Task Delete_LastRow_SelectsThePreviousOne()
    {
        Se.Settings.General.PromptBeforeDelete = false;
        var (window, vm) = ShowMainWindowWithLines(5);

        vm.SelectAndScrollToSubtitle(vm.Subtitles[4]);
        await SettleAsync(window);

        await vm.DeleteSelectedLinesCommand.ExecuteAsync(null);
        await SettleAsync(window);

        Assert.Equal("Line 4", vm.SelectedSubtitle?.Text);
        Assert.Equal(4, vm.Subtitles.Count);
    }

    [AvaloniaFact]
    public async Task Delete_EveryRow_KeepsFocusOnTheEmptyGrid()
    {
        Se.Settings.General.PromptBeforeDelete = false;
        var (window, vm) = ShowMainWindowWithLines(3);

        vm.SelectAndScrollToSubtitle(vm.Subtitles[0]);
        await SettleAsync(window);
        TableViewExtras.FocusRow(vm.SubtitleGrid);
        Settle(window);
        vm.SubtitleGrid.SelectAll();
        Settle(window);

        await vm.DeleteSelectedLinesCommand.ExecuteAsync(null);
        await SettleAsync(window);

        Assert.Empty(vm.Subtitles);
        Assert.Null(vm.SelectedSubtitle);
        Assert.True(vm.SubtitleGrid.IsKeyboardFocusWithin || ReferenceEquals(window.FocusManager?.GetFocusedElement(), vm.SubtitleGrid));
    }

    [AvaloniaFact]
    public async Task Delete_WithReferenceRowNext_SkipsItAndSelectsTheNextWorkingLine()
    {
        Se.Settings.General.PromptBeforeDelete = false;
        var (window, vm) = CreateWithSampleReference();

        // Rows: Translated one | (reference only) | Translated two
        vm.SelectAndScrollToSubtitle(vm.Subtitles[0]);
        await SettleAsync(window);

        await vm.DeleteSelectedLinesCommand.ExecuteAsync(null);
        await SettleAsync(window);

        Assert.Equal("Translated two", vm.SelectedSubtitle?.Text);
        Assert.False(vm.SelectedSubtitle!.IsReferenceOnly);
    }

    [AvaloniaFact]
    public async Task MergeWithLineAfter_FromReferenceOnlyRow_DoesNothing()
    {
        var (window, vm) = CreateWithSampleReference();
        var referenceRow = vm.Subtitles.Single(p => p.IsReferenceOnly);
        vm.SelectAndScrollToSubtitle(referenceRow);
        await SettleAsync(window);
        var textsBefore = vm.Subtitles.Select(p => p.Text).ToList();

        // Used to throw ArgumentOutOfRangeException (IndexOf -1 on the detached row).
        vm.MergeWithLineAfterCommand.Execute(null);
        vm.MergeWithLineAfterKeepBreaksCommand.Execute(null);
        await SettleAsync(window);

        Assert.Equal(textsBefore, vm.Subtitles.Select(p => p.Text).ToList());
    }

    [AvaloniaFact]
    public async Task MergeWithLineAfterAsDialog_FromReferenceOnlyRow_DoesNotDeleteLineOne()
    {
        var (window, vm) = CreateWithSampleReference();
        var referenceRow = vm.Subtitles.Single(p => p.IsReferenceOnly);
        vm.SelectAndScrollToSubtitle(referenceRow);
        await SettleAsync(window);

        vm.MergeWithLineAfterAsDialogCommand.Execute(null);
        await SettleAsync(window);

        Assert.Equal(new[] { "Translated one", "", "Translated two" }, vm.Subtitles.Select(p => p.Text));
        Assert.True(vm.Subtitles[1].IsReferenceOnly);
    }

    [AvaloniaFact]
    public async Task MergeSelectedLinesBilingual_WithReferenceRowAbove_KeepsTheMergedLineSelected()
    {
        var (window, vm) = CreateMainViewModel();
        AddLine(vm, "One", string.Empty, 0, 1000);
        AddLine(vm, "Two", string.Empty, 4000, 5000);
        AddLine(vm, "Three", string.Empty, 6000, 7000);
        var reference = new Subtitle();
        reference.Paragraphs.Add(new Paragraph("Ref one", 0, 1000));
        reference.Paragraphs.Add(new Paragraph("Ref only", 2000, 3000));
        reference.Paragraphs.Add(new Paragraph("Ref two", 4000, 5000));
        reference.Paragraphs.Add(new Paragraph("Ref three", 6000, 7000));
        ImportReference(vm, reference);
        Settle(window);
        Assert.True(vm.Subtitles[1].IsReferenceOnly);

        // Current row first: assigning SelectedSubtitle replaces the whole selection.
        var two = vm.Subtitles[2];
        vm.SelectedSubtitle = two;
        vm.SubtitleGrid.SelectedItems!.Add(vm.Subtitles[3]);
        Settle(window);

        Assert.Equal(2, vm.SubtitleGridSelectedItems.Count);
        vm.MergeSelectedLinesBilingualCommand.Execute(null);
        await SettleAsync(window);

        // Two working rows remain (the merged-away line's original comes back as a reference row,
        // #13594), and the merged row stays current - by row, not by an index captured while the
        // reference row was detached, which landed one row above it (#13962).
        Assert.Equal(new[] { "One", "Two Three" }, vm.Subtitles.Where(p => !p.IsReferenceOnly).Select(p => p.Text));
        Assert.Same(two, vm.SelectedSubtitle);
    }

    [AvaloniaFact]
    public void RemovingARowAboveAMultiSelection_KeepsTheSelection()
    {
        var (window, vm) = ShowMainWindowWithLines(10);
        vm.SelectedSubtitle = vm.Subtitles[5];
        vm.SubtitleGrid.SelectedItems!.Add(vm.Subtitles[6]);
        vm.SubtitleGrid.SelectedItems!.Add(vm.Subtitles[7]);
        Settle(window);
        Assert.Equal(3, vm.SubtitleGridSelectedItems.Count);

        // The TwoWay SelectedIndex binding used to echo the shifted index back into the grid,
        // which is Clear()+Select() on the selection model - one row left selected.
        vm.Subtitles.RemoveAt(2);
        Settle(window);

        Assert.Equal(new[] { "Line 6", "Line 7", "Line 8" }, vm.SubtitleGridSelectedItems.Select(p => p.Text));
        Assert.Equal("Line 6", vm.SelectedSubtitle?.Text);
        Assert.Equal(4, vm.SelectedSubtitleIndex);
    }

    [AvaloniaFact]
    public async Task MergeSelectedLines_NonContiguous_KeepsTheSelection()
    {
        var (window, vm) = ShowMainWindowWithLines(6);
        vm.SelectedSubtitle = vm.Subtitles[1];
        vm.SubtitleGrid.SelectedItems!.Add(vm.Subtitles[3]);
        Settle(window);

        vm.MergeSelectedLinesCommand.Execute(null);
        await SettleAsync(window);

        Assert.Equal(6, vm.Subtitles.Count);
        Assert.Equal(2, vm.SubtitleGridSelectedItems.Count);
    }

    [AvaloniaFact]
    public async Task Duplicate_SkipsReferenceRowsAndSelectsTheCopies()
    {
        var (window, vm) = CreateWithSampleReference();
        var referenceRow = vm.Subtitles.Single(p => p.IsReferenceOnly);
        vm.SelectedSubtitle = vm.Subtitles[0];
        vm.SubtitleGrid.SelectedItems!.Add(referenceRow);
        Settle(window);

        vm.DuplicateSelectedLinesCommand.Execute(null);
        await SettleAsync(window);

        Assert.Equal(4, vm.Subtitles.Count);
        Assert.Equal(1, vm.Subtitles.Count(p => p.IsReferenceOnly));
        var copy = vm.Subtitles[1];
        Assert.Equal("Translated one", copy.Text);
        Assert.False(copy.IsReferenceOnly);
        Assert.Same(copy, vm.SelectedSubtitle);
    }

    [AvaloniaFact]
    public async Task InsertBefore_ShiftSelectionDownwards_InsertsAboveTheTopRow()
    {
        var (window, vm) = ShowMainWindowWithLines(10);
        // Select 5..8 "downwards": row 8 is the current row (the moving end of the selection).
        vm.SelectedSubtitle = vm.Subtitles[7];
        for (var i = 4; i <= 6; i++)
        {
            vm.SubtitleGrid.SelectedItems!.Add(vm.Subtitles[i]);
        }

        Settle(window);
        Assert.Same(vm.Subtitles[7], vm.SubtitleGrid.SelectedItem);
        Assert.Equal(4, vm.SubtitleGridSelectedItems.Count);

        vm.InsertLineBeforeCommand.Execute(null);
        await SettleAsync(window);

        Assert.Equal(11, vm.Subtitles.Count);
        Assert.Equal(string.Empty, vm.Subtitles[4].Text);
        Assert.Equal("Line 5", vm.Subtitles[5].Text);
        Assert.Same(vm.Subtitles[4], vm.SelectedSubtitle);
    }

    [AvaloniaFact]
    public async Task Split_OnTheBottomVisibleRow_RevealsTheNewHalf()
    {
        Se.Settings.General.SubtitleGridCenterSelectedRow = false;
        var (window, vm) = ShowMainWindowWithLines(300, text: "Some words to split in two halves");
        vm.SelectAndScrollToSubtitle(vm.Subtitles[100]);
        await SettleAsync(window);

        // Find the last fully visible row and make it current without scrolling.
        var last = vm.Subtitles.Skip(100).TakeWhile(p => TableViewExtras.IsRowFullyVisible(vm.SubtitleGrid, p)).Last();
        vm.SubtitleGrid.SelectedItem = last;
        await SettleAsync(window);
        Assert.True(TableViewExtras.IsRowFullyVisible(vm.SubtitleGrid, last));

        vm.SplitCommand.Execute(null);
        await SettleAsync(window);

        var newHalf = vm.Subtitles[vm.Subtitles.IndexOf(last) + 1];
        Assert.Same(last, vm.SelectedSubtitle);
        Assert.True(TableViewExtras.IsRowFullyVisible(vm.SubtitleGrid, newHalf), "the second half is off screen");
    }

    private (Window Window, MainViewModel Vm) ShowMainWindowWithLines(int lineCount, string text = "Line {0}")
    {
        var (window, vm) = CreateMainViewModel();
        vm.Menu.IsVisible = true;
        for (var i = 0; i < lineCount; i++)
        {
            var lineText = text.Contains("{0}") ? string.Format(text, i + 1) : text;
            vm.Subtitles.Add(new SubtitleLineViewModel(new Paragraph(lineText, i * 2000, i * 2000 + 1500), null!)
            {
                Number = i + 1,
            });
        }

        Settle(window);
        return (window, vm);
    }

    private (Window Window, MainViewModel Vm) CreateWithSampleReference()
    {
        var (window, vm) = CreateMainViewModel();
        AddLine(vm, "Translated one", string.Empty, 0, 2000);
        AddLine(vm, "Translated two", string.Empty, 4000, 6000);
        var reference = new Subtitle();
        reference.Paragraphs.Add(new Paragraph("Reference one", 0, 2000));
        reference.Paragraphs.Add(new Paragraph("Reference only - no translation", 2000, 4000));
        reference.Paragraphs.Add(new Paragraph("Reference two", 4000, 6000));
        ImportReference(vm, reference);
        Settle(window);
        Assert.Equal(new[] { false, true, false }, vm.Subtitles.Select(p => p.IsReferenceOnly));
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

    private static SubtitleLineViewModel AddLine(MainViewModel vm, string text, string originalText, int startMs, int endMs)
    {
        var line = new SubtitleLineViewModel(new Paragraph(text, startMs, endMs), null!)
        {
            OriginalText = originalText,
            Number = vm.Subtitles.Count + 1,
        };

        vm.Subtitles.Add(line);
        return line;
    }

    private static void ImportReference(MainViewModel vm, Subtitle reference)
    {
        var match = ImportOriginalHelper.MatchOriginalLines(vm.Subtitles, reference);
        var method = typeof(MainViewModel).GetMethod("ImportOriginalSubtitle", BindingFlags.Instance | BindingFlags.NonPublic)
                     ?? throw new InvalidOperationException("ImportOriginalSubtitle not found");
        method.Invoke(vm, new object?[] { 0, "reference.srt", reference, match, true });
    }

    private static void Settle(Window window)
    {
        for (var pump = 0; pump < 8; pump++)
        {
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }
    }

    // The scroll/focus work is posted (some of it at Background priority and behind a short
    // delay), so give the dispatcher a real tick as well.
    private static async Task SettleAsync(Window window)
    {
        Settle(window);
        await Task.Delay(50);
        Settle(window);
    }
}
