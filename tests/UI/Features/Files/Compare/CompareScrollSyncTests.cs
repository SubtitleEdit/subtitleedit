using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Files.Compare;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace UITests.Features.Files.Compare;

/// <summary>
/// Compare's two grids scroll as one: whichever side moves, the other lines up on the same row
/// again (#13504). SE4 did this by copying the active list view's TopItem; SE5's grids were
/// independent, so with larger files the sides drifted a page or more apart.
///
/// The sync has to work in row indices rather than pixels - the two sides hold different text,
/// so a row that wraps to two lines on one side is one line on the other, and equal pixel
/// offsets stop meaning equal rows. The unequal-heights test below is the one that fails for a
/// naive Offset.Y copy.
/// </summary>
public class CompareScrollSyncTests : IDisposable
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
    public void ScrollingTheLeftGrid_BringsTheRightGridToTheSameRow()
    {
        var (window, vm) = OpenCompare(200);

        ScrollTo(window, LeftScrollViewer(vm), 1500);

        Assert.Equal(TopRowIndex(vm.LeftGrid!), TopRowIndex(vm.RightGrid!));
    }

    [AvaloniaFact]
    public void ScrollingTheRightGrid_BringsTheLeftGridToTheSameRow()
    {
        var (window, vm) = OpenCompare(200);

        ScrollTo(window, RightScrollViewer(vm), 2200);

        Assert.Equal(TopRowIndex(vm.RightGrid!), TopRowIndex(vm.LeftGrid!));
    }

    // The right lines wrap to two rendered lines where the left ones do not, so the two grids
    // scale their pixels differently. Copying the offset across would land on a different row;
    // only the index-based sync stays aligned.
    [AvaloniaFact]
    public void ScrollingWithUnequalRowHeights_AlignsRowsRatherThanPixels()
    {
        var (window, vm) = OpenCompare(200, rightLineSuffix: Environment.NewLine + "second rendered line");

        var leftScrollViewer = LeftScrollViewer(vm);
        var rightScrollViewer = RightScrollViewer(vm);
        ScrollTo(window, leftScrollViewer, 1500);

        Assert.Equal(TopRowIndex(vm.LeftGrid!), TopRowIndex(vm.RightGrid!));
        Assert.True(Math.Abs(rightScrollViewer.Offset.Y - leftScrollViewer.Offset.Y) > 1,
            "the two sides ended on the same pixel offset, so this file does not exercise unequal row heights");
    }

    // Selecting a row already mirrored the selection, but scrolled the other side with
    // ScrollIntoView - which only promises the row is somewhere in view. With the taller rows on
    // the right that lands a different number of rows into the view, so the two sides could keep
    // the same row selected while showing different ranges.
    [AvaloniaFact]
    public void SelectingARowFarDown_LinesUpBothViews()
    {
        var (window, vm) = OpenCompare(200, rightLineSuffix: Environment.NewLine + "second rendered line");

        vm.LeftGrid!.SelectedIndex = 150;
        Settle(window);

        Assert.Equal(150, vm.RightGrid!.SelectedIndex);
        Assert.Equal(TopRowIndex(vm.LeftGrid!), TopRowIndex(vm.RightGrid!));
    }

    // The right-hand side is empty until a second file is loaded (Tools > Compare always opens
    // that way), so the sync has nothing to align to and must simply leave it alone.
    [AvaloniaFact]
    public void ScrollingWithAnEmptyRightGrid_DoesNotThrow()
    {
        var (window, vm) = OpenCompare(200, loadRight: false);

        ScrollTo(window, LeftScrollViewer(vm), 1500);

        Assert.Empty(vm.RightSubtitles);
        Assert.True(LeftScrollViewer(vm).Offset.Y > 0, "the left grid did not scroll");
    }

    private static int TopRowIndex(TableView tableView)
    {
        var scrollViewer = tableView.GetVisualDescendants().OfType<ScrollViewer>().First();
        var viewportOrigin = (Visual?)scrollViewer.Presenter ?? scrollViewer;

        var top = double.MaxValue;
        var index = -1;
        foreach (var row in tableView.GetRealizedContainers().OfType<TableViewRow>())
        {
            if (row.Bounds.Height <= 0 ||
                ((Visual)row).TranslatePoint(new Point(0, 0), viewportOrigin)?.Y is not { } rowTop ||
                rowTop + row.Bounds.Height <= 0.5 ||
                rowTop >= top)
            {
                continue;
            }

            top = rowTop;
            index = tableView.IndexFromContainer(row);
        }

        return index;
    }

    private static ScrollViewer LeftScrollViewer(CompareViewModel vm)
        => vm.LeftGrid!.GetVisualDescendants().OfType<ScrollViewer>().First();

    private static ScrollViewer RightScrollViewer(CompareViewModel vm)
        => vm.RightGrid!.GetVisualDescendants().OfType<ScrollViewer>().First();

    private static void ScrollTo(Window window, ScrollViewer scrollViewer, double offsetY)
    {
        scrollViewer.Offset = new Vector(scrollViewer.Offset.X, offsetY);
        Settle(window);
    }

    private (Window Window, CompareViewModel Vm) OpenCompare(
        int lineCount,
        string rightLineSuffix = "",
        bool loadRight = true)
    {
        var vm = new CompareViewModel(new FileHelper(), new FolderHelper());
        vm.Initialize(
            MakeLines(lineCount, string.Empty),
            "left.srt",
            loadRight ? MakeLines(lineCount, rightLineSuffix) : new ObservableCollection<SubtitleLineViewModel>(),
            loadRight ? "right.srt" : string.Empty,
            false);

        var window = new CompareWindow(vm);
        _windows.Add(window);
        window.Show();
        Settle(window);

        Assert.Equal(lineCount, vm.LeftSubtitles.Count);
        return (window, vm);
    }

    private static ObservableCollection<SubtitleLineViewModel> MakeLines(int count, string suffix)
    {
        var lines = new ObservableCollection<SubtitleLineViewModel>();
        for (var i = 0; i < count; i++)
        {
            lines.Add(new SubtitleLineViewModel(new Paragraph($"Line {i + 1}{suffix}", i * 2000, i * 2000 + 1500), null!)
            {
                Number = i + 1,
            });
        }

        return lines;
    }

    private static void Settle(Window window)
    {
        for (var pump = 0; pump < 12; pump++)
        {
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }
    }
}
