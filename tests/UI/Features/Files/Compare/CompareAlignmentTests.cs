using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
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
/// Compare lines the two files up by content, not by position: a line missing on one side gets a
/// blank row and the identical lines after it pair up again, even when the two files are timed
/// differently - and the user can force a pair with a sync point (#15394).
/// </summary>
public class CompareAlignmentTests : IDisposable
{
    private readonly SeCompare _savedSettings = Se.Settings.File.Compare;

    public CompareAlignmentTests()
    {
        Se.Settings.File.Compare = new SeCompare();
    }

    public void Dispose()
    {
        Se.Settings.File.Compare = _savedSettings;
    }

    [Fact]
    public void Align_MissingLineInRetimedFile_PairsTheRestByText()
    {
        // Same texts, the right side shifted by 700 ms and missing "C" - the old matcher needed two
        // matching properties, so text alone never brought the two back in step.
        var left = Lines(("A", 0), ("B", 2000), ("C", 4000), ("D", 6000), ("E", 8000), ("F", 10000));
        var right = Lines(("A", 700), ("B", 2700), ("D", 6700), ("E", 8700), ("F", 10700));

        var pairs = CompareAligner.Align(left, right, TimeEqual);

        Assert.Equal(new[] { (0, 0), (1, 1), (2, -1), (3, 2), (4, 3), (5, 4) }, pairs.Select(p => (p.Left, p.Right)));
    }

    [Fact]
    public void Align_ExtraLineAmongRepeatedTexts_UsesFuzzyTextAndTiming()
    {
        // No unique anchors to lean on: repeated texts, a typo fix, and an extra line on the right.
        var left = Lines(("Yes.", 0), ("I told you so.", 2000), ("Yes.", 4000), ("No.", 6000));
        var right = Lines(("Yes.", 0), ("Wait!", 1000), ("I told ya so.", 2000), ("Yes.", 4000), ("No.", 6000));

        var pairs = CompareAligner.Align(left, right, TimeEqual);

        Assert.Equal(new[] { (0, 0), (-1, 1), (1, 2), (2, 3), (3, 4) }, pairs.Select(p => (p.Left, p.Right)));
    }

    [Fact]
    public void Align_AllDifferent_PairsByPosition()
    {
        var left = Lines(("One", 0), ("Two", 5000), ("Three", 10000));
        var right = Lines(("Uno", 20000), ("Dos", 25000), ("Tres", 30000));

        var pairs = CompareAligner.Align(left, right, TimeEqual);

        Assert.Equal(new[] { (0, 0), (1, 1), (2, 2) }, pairs.Select(p => (p.Left, p.Right)));
    }

    [Fact]
    public void Align_SyncPoint_ForcesThePairAndAlignsEachSideOfIt()
    {
        var left = Lines(("One", 0), ("Two", 5000), ("Three", 10000), ("Four", 15000));
        var right = Lines(("Uno", 20000), ("Dos", 25000), ("Tres", 30000), ("Four", 35000));

        // Without the sync point "Four" anchors and the rest pairs by position; with it, left
        // "One" is paired with right "Tres", leaving "Uno" and "Dos" alone above it.
        var pairs = CompareAligner.Align(left, right, TimeEqual, new[] { (0, 2) });

        Assert.Equal(new[] { (-1, 0), (-1, 1), (0, 2), (1, -1), (2, -1), (3, 3) }, pairs.Select(p => (p.Left, p.Right)));
    }

    [Fact]
    public void Align_LargeTranslationWithExtraLineAtTop_PairsByTiming()
    {
        // Original vs translation: no text in common, identical timings, one extra line on top.
        // Far too big for the dynamic program - it used to fall back to pairing by position.
        var left = Enumerable.Range(0, 3000).Select(i => Line("Original " + i, 2000 + i * 2000)).ToList();
        var right = new[] { Line("Extra", 0) }.Concat(Enumerable.Range(0, 3000).Select(i => Line("Translation " + i, 2000 + i * 2000))).ToList();

        var watch = System.Diagnostics.Stopwatch.StartNew();
        var pairs = CompareAligner.Align(left, right, TimeEqual);
        watch.Stop();

        Assert.Equal(new[] { (-1, 0) }.Concat(Enumerable.Range(0, 3000).Select(i => (i, i + 1))), pairs.Select(p => (p.Left, p.Right)));
        Assert.True(watch.ElapsedMilliseconds < 2000, $"took {watch.ElapsedMilliseconds} ms");
    }

    [Fact]
    public void Align_LargeStretchWithoutTimingAnchors_MergesByTime()
    {
        // Timings 50 ms apart - overlapping, but no exact match to anchor on.
        var left = Enumerable.Range(0, 600).Select(i => Line("Original " + i, 2000 + i * 2000)).ToList();
        var right = new[] { Line("Extra", 0) }.Concat(Enumerable.Range(0, 600).Select(i => Line("Translation " + i, 2050 + i * 2000))).ToList();

        var pairs = CompareAligner.Align(left, right, TimeEqual);

        Assert.Equal(new[] { (-1, 0) }.Concat(Enumerable.Range(0, 600).Select(i => (i, i + 1))), pairs.Select(p => (p.Left, p.Right)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    public void Align_UniqueTextFarAwayInTime_IsNotAnAnchor(int rightShiftMs)
    {
        // "Okay." is the only text in common, but at left #10 and right #150 - 280 s apart.
        var left = Enumerable.Range(0, 160).Select(i => Line(i == 10 ? "Okay." : "Original " + i, i * 2000)).ToList();
        var right = Enumerable.Range(0, 160).Select(i => Line(i == 150 ? "Okay." : "Translation " + i, i * 2000 + rightShiftMs)).ToList();

        var pairs = CompareAligner.Align(left, right, TimeEqual);

        Assert.Equal(Enumerable.Range(0, 160).Select(i => (i, i)), pairs.Select(p => (p.Left, p.Right)));
    }

    [Fact]
    public void Align_ConstantOffset_StillAnchorsByText()
    {
        // The reference runs a minute later throughout and lacks line #20.
        var texts = Enumerable.Range(0, 50).Select(i => $"Line {i}: {new string((char)('a' + i % 26), 1 + i % 7)}").ToList();
        var left = Enumerable.Range(0, 50).Select(i => Line(texts[i], i * 2000)).ToList();
        var right = Enumerable.Range(0, 50).Where(i => i != 20).Select(i => Line(texts[i], 60000 + i * 2000)).ToList();

        var pairs = CompareAligner.Align(left, right, TimeEqual);

        Assert.Equal(Enumerable.Range(0, 50).Select(i => (i, i < 20 ? i : i == 20 ? -1 : i - 1)), pairs.Select(p => (p.Left, p.Right)));
    }

    [Fact]
    public void GetOrderedSyncPoints_DropsCrossingAndOutOfRangePoints()
    {
        var result = CompareAligner.GetOrderedSyncPoints(new[] { (5, 1), (2, 3), (4, 9), (1, 1), (3, 2) }, 6, 5);

        Assert.Equal(new[] { (1, 1), (2, 3) }, result);
    }

    [AvaloniaFact]
    public void SyncPoint_PickedFromTwoRows_RealignsTheComparison()
    {
        var vm = Open(
            MakeLines(("Alpha", 0), ("Beta", 2000), ("Gamma", 4000)),
            MakeLines(("Intro", 0), ("Other", 1000), ("Alpha", 9000), ("Beta", 11000), ("Gamma", 13000)));

        // The identical texts already line up, the two right-only lines first.
        Assert.Equal(5, vm.Rows.Count);
        Assert.Equal("Alpha", vm.Rows[2].Left.Text);
        Assert.Equal("Alpha", vm.Rows[2].Right.Text);

        // Pair the current "Alpha" with the reference "Other" instead.
        vm.PickSyncCurrentCommand.Execute(vm.Rows[2]);
        Assert.True(vm.Rows[2].IsLeftSyncPending);
        Assert.True(vm.HasSyncPointHint);

        vm.PickSyncReferenceCommand.Execute(vm.Rows[1]);
        Settle();

        var syncRow = vm.Rows.Single(p => p.IsSyncPoint);
        Assert.Equal("Alpha", syncRow.Left.Text);
        Assert.Equal("Other", syncRow.Right.Text);
        Assert.Equal(1, vm.SyncPointCount);
        Assert.False(vm.HasSyncPointHint);
        Assert.DoesNotContain(vm.Rows, p => p.IsLeftSyncPending || p.IsRightSyncPending);

        vm.RemoveSyncPointCommand.Execute(syncRow);
        Settle();

        Assert.Equal(0, vm.SyncPointCount);
        Assert.DoesNotContain(vm.Rows, p => p.IsSyncPoint);
        Assert.Contains(vm.Rows, p => p.Left.Text == "Alpha" && p.Right.Text == "Alpha");
    }

    [AvaloniaFact]
    public void SyncPoint_NewPointReplacesACrossingOne()
    {
        var vm = Open(
            MakeLines(("A", 0), ("B", 2000), ("C", 4000)),
            MakeLines(("X", 50000), ("Y", 52000), ("Z", 54000)));

        AddSyncPoint(vm, "A", "Z");
        AddSyncPoint(vm, "C", "X"); // crosses A-Z, which gives way

        Assert.Equal(1, vm.SyncPointCount);
        var syncRow = vm.Rows.Single(p => p.IsSyncPoint);
        Assert.Equal("C", syncRow.Left.Text);
        Assert.Equal("X", syncRow.Right.Text);
    }

    [AvaloniaFact]
    public void RightClick_SelectsTheRowUnderThePointer_AndOpensTheSyncMenu()
    {
        var vm = Open(MakeLines(("A", 0), ("B", 2000), ("C", 4000)), MakeLines(("A", 0), ("X", 2000), ("C", 4000)));
        var window = new CompareWindow(vm) { Width = 1300, Height = 800 };
        window.Show();
        try
        {
            Settle(window);
            vm.SelectRow(0);
            Settle(window);

            var container = vm.RowsView!.GetRealizedContainers().ElementAt(2);
            var point = container.TranslatePoint(new Point(200, 20), window)!.Value;
            window.MouseDown(point, MouseButton.Right);
            window.MouseUp(point, MouseButton.Right);
            Settle(window);

            var flyout = Assert.IsType<MenuFlyout>(vm.RowsView!.ContextFlyout);
            Assert.True(flyout.IsOpen);
            Assert.Same(vm.Rows[2], vm.SelectedRow);
            var visible = flyout.Items.OfType<MenuItem>().Where(p => p.IsVisible).Select(p => p.Header).ToList();
            Assert.Equal(new object[] { Se.Language.File.CompareSyncPickCurrent, Se.Language.File.CompareSyncPickReference }, visible);
            flyout.Hide();
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void ApplySync_CompletesThePickWithTheSelectedRow_AndTheMenuNamesTheWaitingLine()
    {
        var vm = Open(MakeLines(("A", 0), ("B", 2000), ("C", 4000)), MakeLines(("X", 50000), ("Y", 52000), ("Z", 54000)));
        Assert.Equal(Se.Language.File.CompareSyncPickReference, vm.PickSyncReferenceHeader);

        vm.PickSyncCurrentCommand.Execute(vm.Rows[1]); // current "B", #2
        Assert.Equal(string.Format(Se.Language.File.CompareSyncWithCurrentX, 2), vm.PickSyncReferenceHeader);
        Assert.True(vm.HasSyncBar);

        vm.SelectedRow = vm.Rows.First(p => p.Right.Text == "Z");
        Assert.True(vm.CanApplySync);
        vm.ApplySyncCommand.Execute(null);
        Settle();

        var syncRow = vm.Rows.Single(p => p.IsSyncPoint);
        Assert.Equal("B", syncRow.Left.Text);
        Assert.Equal("Z", syncRow.Right.Text);
        Assert.False(vm.CanApplySync);
        Assert.Equal(Se.Language.File.CompareSyncPickReference, vm.PickSyncReferenceHeader);
    }

    [AvaloniaFact]
    public void SyncPoint_OnAPairAlreadyShownTogether_IsNotAdded_AndSaysSo()
    {
        var vm = Open(MakeLines(("A", 0), ("B", 2000)), MakeLines(("A", 0), ("X", 2000)));

        // Pick the current line, then Sync with the same row still selected.
        vm.PickSyncCurrentCommand.Execute(vm.Rows[1]);
        vm.SelectedRow = vm.Rows[1];
        vm.ApplySyncCommand.Execute(null);
        Settle();

        Assert.Equal(0, vm.SyncPointCount);
        Assert.False(vm.HasSyncPointHint);
        Assert.Equal(string.Format(Se.Language.File.CompareSyncAlreadyPairedXY, 2, 2), vm.SyncPointMessage);
        Assert.True(vm.HasSyncBar);
        Assert.DoesNotContain(vm.Rows, p => p.IsSyncPoint || p.IsLeftSyncPending);
    }

    [AvaloniaFact]
    public void SyncPoint_ShiftsTheCurrentLineToTheReferenceStart_AndTheRestWithIt()
    {
        // From "C" on, the reference runs 11 s later - the current file lost the sync there.
        var vm = Open(
            MakeLines(("A", 0), ("B", 2000), ("C", 4000), ("D", 6000)),
            MakeLines(("A", 0), ("B", 2000), ("C", 15000), ("D", 17000)));
        Assert.Equal(2, vm.DifferenceCount);

        // C is already beside C - a single sync point there is still a valid timing sync.
        AddSyncPoint(vm, "C", "C");

        var edited = vm.GetEditedLines();
        Assert.Equal(new[] { 0d, 2000, 15000, 17000 }, edited.Select(p => p.StartTime.TotalMilliseconds));
        Assert.Equal(new[] { 1500d, 3500, 16500, 18500 }, edited.Select(p => p.EndTime.TotalMilliseconds));
        Assert.Equal(0, vm.DifferenceCount);
        Assert.Equal(1, vm.PendingChangeCount);
        Assert.Equal(1, vm.SyncPointCount);
        Assert.Equal(string.Format(Se.Language.File.CompareSyncPointShiftedXYZW, 3, 4, "+" + new TimeCode(11000).ToDisplayString(), 3), vm.SyncPointMessage);

        // Undo takes back the timing and the sync point together.
        vm.UndoCommand.Execute(null);
        Settle();
        Assert.Equal(new[] { 0d, 2000, 4000, 6000 }, vm.GetEditedLines().Select(p => p.StartTime.TotalMilliseconds));
        Assert.Equal(0, vm.SyncPointCount);
        Assert.Equal(0, vm.PendingChangeCount);
    }

    [AvaloniaFact]
    public void SyncPoint_ShiftStopsAtTheNextSyncPoint()
    {
        var vm = Open(
            MakeLines(("A", 0), ("B", 2000), ("C", 4000), ("D", 6000)),
            MakeLines(("A", 1000), ("B", 3000), ("C", 9000), ("D", 11000)));

        AddSyncPoint(vm, "C", "C"); // C, D: +5 s
        AddSyncPoint(vm, "A", "A"); // A, B: +1 s - not past C

        Assert.Equal(new[] { 1000d, 3000, 9000, 11000 }, vm.GetEditedLines().Select(p => p.StartTime.TotalMilliseconds));
        Assert.Equal(2, vm.SyncPointCount);
        Assert.Equal(2, vm.PendingChangeCount);
    }

    [AvaloniaFact]
    public void SyncPoint_AddRemoveAndClear_AreEachUndoable()
    {
        var vm = Open(
            MakeLines(("A", 0), ("B", 2000), ("C", 4000)),
            MakeLines(("X", 0), ("Y", 2000), ("Z", 4000)));

        AddSyncPoint(vm, "B", "Z");
        Assert.Equal(1, vm.SyncPointCount);

        vm.RemoveSyncPointCommand.Execute(vm.Rows.Single(p => p.IsSyncPoint));
        Settle();
        Assert.Equal(0, vm.SyncPointCount);

        vm.UndoCommand.Execute(null); // the removal
        Settle();
        Assert.Equal(1, vm.SyncPointCount);

        vm.ClearSyncPointsCommand.Execute(null);
        Settle();
        Assert.Equal(0, vm.SyncPointCount);

        vm.UndoCommand.Execute(null); // the clear
        Settle();
        Assert.Equal(1, vm.SyncPointCount);
        Assert.Single(vm.Rows, p => p.IsSyncPoint);

        vm.UndoCommand.Execute(null); // the add, timing shift included
        Settle();
        Assert.Equal(0, vm.SyncPointCount);
        Assert.Equal(new[] { 0d, 2000, 4000 }, vm.GetEditedLines().Select(p => p.StartTime.TotalMilliseconds));
    }

    [AvaloniaFact]
    public void DeleteCurrentLine_RemovesItsSyncPoint_AndUndoBringsItBack()
    {
        var vm = Open(
            MakeLines(("A", 0), ("B", 2000), ("C", 4000)),
            MakeLines(("X", 0), ("Y", 2000), ("Z", 4000)));
        AddSyncPoint(vm, "B", "Z");
        Assert.Equal(1, vm.SyncPointCount);

        vm.DeleteCurrentLineCommand.Execute(vm.Rows.Single(p => p.IsSyncPoint));
        Settle();
        Assert.Equal(0, vm.SyncPointCount);
        Assert.False(vm.HasSyncPoints);
        Assert.DoesNotContain(vm.Rows, p => p.IsSyncPoint || p.Left.Text == "B");

        vm.UndoCommand.Execute(null);
        Settle();
        Assert.Equal(1, vm.SyncPointCount);
        var syncRow = vm.Rows.Single(p => p.IsSyncPoint);
        Assert.Equal("B", syncRow.Left.Text);
        Assert.Equal("Z", syncRow.Right.Text);
    }

    [AvaloniaFact]
    public void SyncBar_AtTheMinimumWidth_KeepsTheHintOnAFewLines()
    {
        var vm = Open(MakeLines(("A", 0), ("B", 2000)), MakeLines(("A", 0), ("X", 2000)));
        var window = new CompareWindow(vm) { Width = 900, Height = 500 };
        window.Show();
        try
        {
            Settle(window);
            vm.PickSyncCurrentCommand.Execute(vm.Rows[1]);
            Settle(window);

            var hint = window.GetVisualDescendants().OfType<TextBlock>().First(p => p.Text == vm.SyncPointHint);
            Assert.True(hint.IsEffectivelyVisible);
            Assert.True(hint.Bounds.Height < 60, $"hint is {hint.Bounds.Height}px tall");
        }
        finally
        {
            window.Close();
        }
    }

    private static void AddSyncPoint(CompareViewModel vm, string leftText, string rightText)
    {
        vm.PickSyncReferenceCommand.Execute(vm.Rows.First(p => p.Right.Text == rightText));
        vm.PickSyncCurrentCommand.Execute(vm.Rows.First(p => p.Left.Text == leftText));
        Settle();
    }

    private static bool TimeEqual(TimeSpan a, TimeSpan b) => Math.Abs((a - b).TotalMilliseconds) < 0.1;

    private static CompareAligner.Line Line(string text, int startMs) =>
        new(text, TimeSpan.FromMilliseconds(startMs), TimeSpan.FromMilliseconds(startMs + 1500));

    private static List<CompareAligner.Line> Lines(params (string Text, int StartMs)[] lines) =>
        lines.Select(p => new CompareAligner.Line(p.Text, TimeSpan.FromMilliseconds(p.StartMs), TimeSpan.FromMilliseconds(p.StartMs + 1500))).ToList();

    private static CompareViewModel Open(ObservableCollection<SubtitleLineViewModel> left, ObservableCollection<SubtitleLineViewModel> right)
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
            result.Add(new SubtitleLineViewModel(new Paragraph(lines[i].Text, lines[i].StartMs, lines[i].StartMs + 1500), null!)
            {
                Number = i + 1,
            });
        }

        return result;
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
