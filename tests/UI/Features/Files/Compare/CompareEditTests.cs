using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Files.Compare;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace UITests.Features.Files.Compare;

/// <summary>
/// Compare's merge view edits the current (left) subtitle only: a differing pair can take the
/// reference's text and timing, a reference-only line can be inserted, an extra line deleted,
/// and all of it undone - and Apply hands the lines back with their row ids intact (#14358).
/// </summary>
public class CompareEditTests : IDisposable
{
    private readonly List<Window> _windows = new();
    private readonly SeCompare _savedSettings = Se.Settings.File.Compare;

    // Closing a CompareWindow saves its size and the next one opens at it (#15393), so a test's
    // own Width/Height lost to whatever window an earlier test closed - a 500 px tall one left
    // no room for the inline editor. Each test starts with no saved positions and leaves none.
    private readonly SettingsScope _windowPositions = new("General.WindowPositions");

    public CompareEditTests()
    {
        Se.Settings.File.Compare = new SeCompare();
        Se.Settings.General.WindowPositions = new List<SeWindowPosition>();
    }

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            window.Close();
        }

        _windows.Clear();
        Se.Settings.File.Compare = _savedSettings;
        _windowPositions.Dispose();
    }

    [AvaloniaFact]
    public void TakeReference_CopiesTextAndTiming_AndKeepsTheLineId()
    {
        var left = MakeLines(("One", 0), ("Too", 2000), ("Three", 4000));
        var right = MakeLines(("One", 0), ("Two", 2100), ("Three", 4000));
        var vm = Open(left, right);
        var row = vm.Rows.Single(p => p.Kind == CompareRowKind.Changed);

        vm.TakeReferenceCommand.Execute(row);
        Settle();

        var edited = vm.GetEditedLines();
        Assert.Equal("Two", edited[1].Text);
        Assert.Equal(2100, edited[1].StartTime.TotalMilliseconds);
        Assert.Equal(left[1].Id, edited[1].Id);
        Assert.Equal(1, vm.PendingChangeCount);
        Assert.All(vm.Rows, p => Assert.Equal(CompareRowKind.Same, p.Kind));
        Assert.True(vm.Rows[1].IsEdited);

        // The editor's own rows are untouched until Apply.
        Assert.Equal("Too", left[1].Text);
    }

    [AvaloniaFact]
    public void TakeReferenceText_LeavesTheTimingAlone()
    {
        var vm = Open(MakeLines(("One", 0), ("Too", 2000)), MakeLines(("One", 0), ("Two", 2500)));
        var row = vm.Rows.Single(p => p.Kind == CompareRowKind.Changed);

        vm.TakeReferenceTextCommand.Execute(row);
        Settle();

        var edited = vm.GetEditedLines();
        Assert.Equal("Two", edited[1].Text);
        Assert.Equal(2000, edited[1].StartTime.TotalMilliseconds);
    }

    [AvaloniaFact]
    public void TakeReference_OnAReferenceOnlyLine_InsertsItInTimeOrderAndRenumbers()
    {
        var vm = Open(
            MakeLines(("One", 0), ("Three", 4000)),
            MakeLines(("One", 0), ("Two", 2000), ("Three", 4000)));
        var row = vm.Rows.Single(p => p.Kind == CompareRowKind.OnlyRight);

        vm.TakeReferenceCommand.Execute(row);
        Settle();

        var edited = vm.GetEditedLines();
        Assert.Equal(new[] { "One", "Two", "Three" }, edited.Select(p => p.Text));
        Assert.Equal(new[] { 1, 2, 3 }, edited.Select(p => p.Number));
        Assert.All(vm.Rows, p => Assert.Equal(CompareRowKind.Same, p.Kind));
    }

    [AvaloniaFact]
    public void DeleteCurrentLine_RemovesAnExtraLine()
    {
        var vm = Open(
            MakeLines(("One", 0), ("[DOOR SLAMS]", 2000), ("Three", 4000)),
            MakeLines(("One", 0), ("Three", 4000)));
        var row = vm.Rows.Single(p => p.Kind == CompareRowKind.OnlyLeft);
        Assert.True(row.CanDeleteCurrent);

        vm.DeleteCurrentLineCommand.Execute(row);
        Settle();

        Assert.Equal(new[] { "One", "Three" }, vm.GetEditedLines().Select(p => p.Text));
        Assert.All(vm.Rows, p => Assert.Equal(CompareRowKind.Same, p.Kind));
    }

    [AvaloniaFact]
    public void CommitEdit_WritesTheInlineEditorBack()
    {
        var vm = Open(MakeLines(("One", 0), ("Two", 2000)), MakeLines(("One", 0), ("Two", 2000)));
        var row = vm.Rows[1];

        vm.BeginEditCommand.Execute(row);
        row.EditText = "Two, changed";
        row.EditEnd = TimeSpan.FromMilliseconds(3000);
        vm.CommitEditCommand.Execute(row);
        Settle();

        var edited = vm.GetEditedLines()[1];
        Assert.Equal("Two, changed", edited.Text);
        Assert.Equal(3000, edited.EndTime.TotalMilliseconds);
        Assert.Equal(1, vm.PendingChangeCount);
    }

    [AvaloniaFact]
    public void CommitEdit_WithNothingChanged_IsNotAChange()
    {
        var vm = Open(MakeLines(("One", 0)), MakeLines(("One", 0)));
        var row = vm.Rows[0];

        vm.BeginEditCommand.Execute(row);
        vm.CommitEditCommand.Execute(row);
        Settle();

        Assert.Equal(0, vm.PendingChangeCount);
        Assert.False(vm.HasPendingChanges);
    }

    [AvaloniaFact]
    public void Undo_StepsBackOneChangeAtATime()
    {
        var vm = Open(
            MakeLines(("One", 0), ("Too", 2000), ("Tree", 4000)),
            MakeLines(("One", 0), ("Two", 2000), ("Three", 4000)));

        vm.TakeReferenceCommand.Execute(vm.Rows[1]);
        Settle();
        vm.TakeReferenceCommand.Execute(vm.Rows[2]);
        Settle();
        Assert.Equal(2, vm.PendingChangeCount);

        vm.UndoCommand.Execute(null);
        Settle();

        Assert.Equal(new[] { "One", "Two", "Tree" }, vm.GetEditedLines().Select(p => p.Text));
        Assert.Equal(1, vm.PendingChangeCount);
        Assert.False(vm.Rows[2].IsEdited);

        vm.UndoCommand.Execute(null);
        Settle();

        Assert.Equal(new[] { "One", "Too", "Tree" }, vm.GetEditedLines().Select(p => p.Text));
        Assert.False(vm.HasPendingChanges);
    }

    [AvaloniaFact]
    public void IgnoreNumbering_DropsNumberOnlyDifferences()
    {
        var left = MakeLines(("One", 0), ("Two", 2000));
        var right = MakeLines(("One", 0), ("Two", 2000));
        right[1].Number = 7;

        var vm = Open(left, right);
        Assert.Equal(CompareRowKind.NumberOnly, vm.Rows[1].Kind);

        vm.IgnoreNumbering = true;
        vm.Initialize(left, "left.srt", right, "right.srt", false);
        Settle();

        Assert.All(vm.Rows, p => Assert.Equal(CompareRowKind.Same, p.Kind));
    }

    [AvaloniaFact]
    public void Counts_CoverTheWholeComparison_WhateverTheView()
    {
        var vm = Open(
            MakeLines(("One", 0), ("Too", 2000), ("Three", 4000)),
            MakeLines(("One", 0), ("Two", 2000), ("Three", 4500)));

        Assert.Equal(3, vm.AllCount);
        Assert.Equal(2, vm.DifferenceCount);
        Assert.Equal(1, vm.TextDifferenceCount);

        vm.ShowTextDifferencesCommand.Execute(null);
        Settle();

        Assert.Single(vm.Rows);
        Assert.Equal(2, vm.DifferenceCount);
    }

    [AvaloniaFact]
    public void OnlyTheCurrentSideCanBeEdited()
    {
        var vm = Open(MakeLines(("Too", 0)), MakeLines(("Two", 0)));
        var row = vm.Rows[0];

        Assert.True(row.CanEdit);
        Assert.True(row.CanTakeReference);
        Assert.True(vm.IsLeftEditable);
        Assert.Equal(row.Left.Line!.Id, vm.GetEditedLines()[0].Id);
    }

    // The rows share one list, so a pair can no longer drift apart; scrolling far down recycles
    // row containers, which moves each item's text panel into a new host - that must not throw.
    [AvaloniaFact]
    public void Window_ScrollingRecyclesRowsWithoutLosingTheText()
    {
        var left = new ObservableCollection<SubtitleLineViewModel>();
        var right = new ObservableCollection<SubtitleLineViewModel>();
        for (var i = 0; i < 200; i++)
        {
            left.Add(MakeLine($"Line {i + 1}", i * 2000, i + 1));
            right.Add(MakeLine($"Line {i + 1}" + Environment.NewLine + "second line", i * 2000, i + 1));
        }

        var vm = new CompareViewModel(new FileHelper(), new FolderHelper());
        vm.Initialize(left, "left.srt", right, "right.srt", false);
        var window = new CompareWindow(vm);
        _windows.Add(window);
        window.Show();
        Settle(window);

        var scrollViewer = vm.RowsView!.GetVisualDescendants().OfType<ScrollViewer>().First();
        scrollViewer.Offset = new Vector(0, 5000);
        Settle(window);
        scrollViewer.Offset = new Vector(0, 0);
        Settle(window);

        Assert.Equal(200, vm.Rows.Count);
        var realized = vm.RowsView!.GetRealizedContainers().ToList();
        Assert.NotEmpty(realized);
        Assert.All(vm.Rows.Take(3), row => Assert.NotNull(row.Left.TextPanel.Parent));
    }

    [AvaloniaFact]
    public void OpeningAndClosingTheEditor_DoesNotMoveTheList()
    {
        var left = new ObservableCollection<SubtitleLineViewModel>(Enumerable.Range(0, 60).Select(i => MakeLine("Line " + i, i * 2000, i + 1)));
        var right = new ObservableCollection<SubtitleLineViewModel>(Enumerable.Range(0, 60).Select(i => MakeLine("Line " + i, i * 2000 + 300, i + 1)));
        var vm = Open(left, right);
        var window = new CompareWindow(vm) { Width = 1300, Height = 800 };
        _windows.Add(window);
        window.Show();
        Settle(window);

        var scrollViewer = vm.RowsView!.GetVisualDescendants().OfType<ScrollViewer>().First();
        scrollViewer.Offset = new Vector(0, 600);
        Settle(window);

        var containers = vm.RowsView.GetRealizedContainers().ToList();
        // Near the top, so the opened editor fits in the view - focusing it must not need a scroll
        // (rows are taller with the Linux fonts on CI, and a lower row's editor would overflow).
        var row = (CompareRow)containers[2].DataContext!;
        double RowTop() => vm.RowsView.ContainerFromItem(row)!.TranslatePoint(new Point(0, 0), window)!.Value.Y;
        var top = RowTop();

        vm.BeginEditCommand.Execute(row);
        Settle(window);
        Assert.True(row.IsEditing);
        Assert.Equal(top, RowTop(), 1);

        vm.CancelEditCommand.Execute(row);
        Settle(window);
        Assert.False(row.IsEditing);
        Assert.Equal(top, RowTop(), 1);
    }

    // Wheel scrolling leaves keyboard focus on the selected row, out of view; the virtualizing list
    // keeps it at an estimated position, and focus going back to it scrolled the list there -
    // hundreds of rows off with these mixed row heights (#15843).
    [AvaloniaFact]
    public void ReactivatingTheWindow_KeepsTheView_WhenTheFocusedRowIsScrolledAway()
    {
        var (vm, window, scrollViewer) = OpenScrolledAwayFromFocusedRow();
        var first = FirstVisibleRow(vm, scrollViewer);

        var other = new Window { Width = 200, Height = 200 };
        _windows.Add(other);
        other.Show();
        other.Activate();
        Settle(other);
        window.Activate();
        Settle(window);

        Assert.Equal(first, FirstVisibleRow(vm, scrollViewer));
    }

    [AvaloniaFact]
    public void ClosingTheEditor_KeepsTheView_WhenTheFocusedRowIsScrolledAway()
    {
        var (vm, window, scrollViewer) = OpenScrolledAwayFromFocusedRow();
        var row = vm.Rows[FirstVisibleRow(vm, scrollViewer) + 2];

        vm.BeginEditCommand.Execute(row);
        Settle(window);
        var first = FirstVisibleRow(vm, scrollViewer); // the editor's text box may nudge the list to show it
        vm.CancelEditCommand.Execute(row);
        Settle(window);
        Assert.Equal(first, FirstVisibleRow(vm, scrollViewer));

        vm.BeginEditCommand.Execute(row);
        Settle(window);
        first = FirstVisibleRow(vm, scrollViewer);
        vm.CommitEditCommand.Execute(row);
        Settle(window);
        Assert.Equal(first, FirstVisibleRow(vm, scrollViewer));
    }

    private (CompareViewModel Vm, CompareWindow Window, ScrollViewer ScrollViewer) OpenScrolledAwayFromFocusedRow()
    {
        var left = new ObservableCollection<SubtitleLineViewModel>();
        var right = new ObservableCollection<SubtitleLineViewModel>();
        for (var i = 0; i < 1000; i++)
        {
            var text = i < 450 ? "Line " + i : "Line " + i + Environment.NewLine + "second" + Environment.NewLine + "third";
            left.Add(MakeLine(text, i * 2000, i + 1));
            right.Add(MakeLine(i % 3 == 0 ? text + " x" : text, i * 2000, i + 1));
        }

        var vm = Open(left, right);
        var window = new CompareWindow(vm) { Width = 1300, Height = 800 };
        _windows.Add(window);
        window.Show();
        window.Activate();
        Settle(window);

        var scrollViewer = vm.RowsView!.GetVisualDescendants().OfType<ScrollViewer>().First();
        vm.SelectRow(420);
        Settle(window);
        ((Control)vm.RowsView.ContainerFromIndex(420)!).Focus();
        Settle(window);
        while (FirstVisibleRow(vm, scrollViewer) < 500)
        {
            scrollViewer.Offset = new Vector(0, scrollViewer.Offset.Y + 300);
            Settle(window);
        }

        return (vm, window, scrollViewer);
    }

    private static int FirstVisibleRow(CompareViewModel vm, ScrollViewer scrollViewer)
    {
        return vm.RowsView!.GetRealizedContainers()
            .Select(c => (Container: c, Top: c.TranslatePoint(new Point(0, 0), scrollViewer)!.Value.Y))
            .Where(p => p.Top + p.Container.Bounds.Height > 0 && p.Top < scrollViewer.Viewport.Height)
            .OrderBy(p => p.Top)
            .Select(p => vm.RowsView.IndexFromContainer(p.Container))
            .First();
    }

    private CompareViewModel Open(ObservableCollection<SubtitleLineViewModel> left, ObservableCollection<SubtitleLineViewModel> right)
    {
        var vm = new CompareViewModel(new FileHelper(), new FolderHelper());
        vm.Initialize(left, "left.srt", right, "right.srt", false);
        Settle();
        return vm;
    }

    private static ObservableCollection<SubtitleLineViewModel> MakeLines(params (string Text, int StartMs)[] lines)
    {
        var result = new ObservableCollection<SubtitleLineViewModel>();
        for (var i = 0; i < lines.Length; i++)
        {
            result.Add(MakeLine(lines[i].Text, lines[i].StartMs, i + 1));
        }

        return result;
    }

    private static SubtitleLineViewModel MakeLine(string text, int startMs, int number)
    {
        return new SubtitleLineViewModel(new Paragraph(text, startMs, startMs + 1500), null!)
        {
            Number = number,
        };
    }

    private static void Settle(Window? window = null)
    {
        for (var pump = 0; pump < 12; pump++)
        {
            Dispatcher.UIThread.RunJobs();
            window?.UpdateLayout();
        }
    }
}
