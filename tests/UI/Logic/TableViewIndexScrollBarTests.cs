using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Logic;

/// <summary>
/// The index-mapped scrollbar for the main subtitle grid (issue #13579): the grid's rows are
/// variable height (one or two text lines), and the virtualizing panel re-estimates the
/// pixel extent from the average realized height on every scroll, which made the native
/// thumb jitter. TableViewIndexScrollBar hides the native vertical bar and maps its own bar
/// to row indices, so the thumb moves monotonically no matter how the row heights are mixed.
/// </summary>
public class TableViewIndexScrollBarTests : IDisposable
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

    private sealed record Row(int Number, string Text);

    private const int RowCount = 500;

    private (Window window, TableView grid, TableViewIndexScrollBar wrapper, ScrollBar bar, ScrollViewer scrollViewer) BuildShownGrid()
    {
        // Every third row has two text lines, so realized row heights vary the way the
        // subtitle grid's do - the very mix that makes the native pixel extent jitter.
        var grid = new TableView
        {
            ItemsSource = Enumerable.Range(1, RowCount)
                .Select(i => new Row(i, i % 3 == 0 ? $"Line {i}\nsecond line" : $"Line {i}"))
                .ToList(),
            Columns =
            {
                new TableViewColumn { Header = "#", Binding = new Avalonia.Data.Binding(nameof(Row.Number)) },
                new TableViewColumn { Header = "Text", Binding = new Avalonia.Data.Binding(nameof(Row.Text)) },
            },
        };

        var wrapper = new TableViewIndexScrollBar(grid);
        var window = new Window { Content = wrapper, Width = 400, Height = 300 };

        // Like the app (UiTheme.ApplyScrollBarStyle): keep the bar expanded so its trough
        // is hit-testable without hover expansion, which headless input never delivers.
        window.Styles.Add(new Style(x => x.OfType<ScrollBar>())
        {
            Setters = { new Setter(ScrollBar.AllowAutoHideProperty, false) },
        });

        _windows.Add(window);
        window.Show();
        window.UpdateLayout();
        AvaloniaHeadlessPlatform.ForceRenderTimerTick();
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        var bar = wrapper.BarForTest;
        var scrollViewer = grid.GetVisualDescendants().OfType<ScrollViewer>().First();
        return (window, grid, wrapper, bar, scrollViewer);
    }

    // The TableView template pins the column header inside the ScrollViewer, so the true
    // viewport origin is the content presenter, a header height below the ScrollViewer's own.
    private static Visual ViewportOrigin(ScrollViewer scrollViewer) => (Visual?)scrollViewer.Presenter ?? scrollViewer;

    private static int FirstVisibleIndex(TableView grid, ScrollViewer scrollViewer)
    {
        var best = -1;
        var bestTop = double.MaxValue;
        foreach (var row in grid.GetRealizedContainers().OfType<TableViewRow>())
        {
            var top = ((Visual)row).TranslatePoint(new Point(0, 0), ViewportOrigin(scrollViewer))?.Y;
            if (top == null || row.Bounds.Height <= 0 || top.Value + row.Bounds.Height <= 0.5)
            {
                continue;
            }

            if (top.Value < bestTop)
            {
                bestTop = top.Value;
                best = grid.IndexFromContainer(row);
            }
        }

        return best;
    }

    [AvaloniaFact]
    public void RowsHaveVariableHeights()
    {
        // Guard for the whole file: if a template change ever made all rows the same
        // height, these tests would no longer exercise the jitter scenario at all.
        var (_, grid, _, _, _) = BuildShownGrid();

        var heights = grid.GetRealizedContainers().OfType<TableViewRow>()
            .Select(r => r.Bounds.Height)
            .Where(h => h > 0)
            .Distinct()
            .ToList();

        Assert.True(heights.Count > 1, "expected a mix of one and two line rows");
    }

    [AvaloniaFact]
    public void NativeVerticalScrollBar_IsHidden()
    {
        var (_, grid, _, bar, _) = BuildShownGrid();

        var nativeBar = grid.GetVisualDescendants().OfType<ScrollBar>()
            .First(s => s.Orientation == Orientation.Vertical && !ReferenceEquals(s, bar));

        Assert.False(nativeBar.IsVisible, "the native pixel-mapped vertical bar should be hidden");
        Assert.True(bar.IsVisible, "the index-mapped bar should be visible");
    }

    [AvaloniaFact]
    public void Maximum_IsRowBased()
    {
        var (_, _, _, bar, scrollViewer) = BuildShownGrid();

        // Row units, not pixels: the viewport shows a handful of rows out of 500, so the
        // maximum sits just below the row count - while the pixel extent is thousands.
        Assert.InRange(bar.Maximum, RowCount - 30, RowCount - 1);
        Assert.True(scrollViewer.Extent.Height - scrollViewer.Viewport.Height > bar.Maximum * 2,
            "sanity: the pixel extent should be far larger than the row-based maximum");
        Assert.Equal(0, bar.Value);
    }

    [AvaloniaFact]
    public void WheelScrollingDown_MovesValueMonotonically_AndMaximumStaysRowStable()
    {
        var (window, grid, _, bar, scrollViewer) = BuildShownGrid();

        var previousValue = bar.Value;
        var maxSeen = bar.Maximum;
        var minSeen = bar.Maximum;

        while (scrollViewer.Offset.Y < scrollViewer.Extent.Height - scrollViewer.Viewport.Height - 0.5)
        {
            // A wheel notch's worth of pixels; drives the same ScrollChanged path as the wheel.
            scrollViewer.Offset = new Vector(0, scrollViewer.Offset.Y + 120);
            window.UpdateLayout();

            Assert.True(bar.Value >= previousValue - 0.001,
                $"thumb moved backwards while scrolling down: {previousValue} -> {bar.Value} at offset {scrollViewer.Offset.Y}");
            previousValue = bar.Value;
            maxSeen = Math.Max(maxSeen, bar.Maximum);
            minSeen = Math.Min(minSeen, bar.Maximum);
        }

        // The native bar's Maximum (pixel extent) swings by whole percents as the average
        // row height is re-estimated; the index bar's varies only with the handful of rows
        // that fit the viewport.
        Assert.True(maxSeen - minSeen <= 6,
            $"row-based maximum should be stable, varied {minSeen}..{maxSeen}");

        Assert.Equal(bar.Maximum, bar.Value, 3); // pinned hard to the end at the bottom
    }

    [AvaloniaFact]
    public void ScrollingToTop_PinsValueToZero()
    {
        var (window, _, wrapper, bar, scrollViewer) = BuildShownGrid();

        bar.Value = bar.Maximum;
        wrapper.ApplyPendingForTest();
        window.UpdateLayout();
        Assert.True(scrollViewer.Offset.Y > 0);

        scrollViewer.Offset = new Vector(0, 0);
        window.UpdateLayout();

        Assert.Equal(0, bar.Value, 3);
    }

    [AvaloniaFact]
    public void SettingValue_ScrollsThatRowToViewportTop()
    {
        var (window, grid, wrapper, bar, scrollViewer) = BuildShownGrid();

        bar.Value = 250;
        wrapper.ApplyPendingForTest();
        window.UpdateLayout();

        Assert.Equal(250, FirstVisibleIndex(grid, scrollViewer));

        var row = grid.ContainerFromIndex(250)!;
        var top = ((Visual)row).TranslatePoint(new Point(0, 0), ViewportOrigin(scrollViewer))!.Value.Y;
        Assert.InRange(top, -1, 1);
    }

    [AvaloniaFact]
    public void SettingValueToMaximum_ShowsTheLastRow()
    {
        var (window, grid, wrapper, bar, scrollViewer) = BuildShownGrid();

        bar.Value = bar.Maximum;
        wrapper.ApplyPendingForTest();
        window.UpdateLayout();

        var lastRow = grid.ContainerFromIndex(RowCount - 1);
        Assert.NotNull(lastRow);
        var bottom = ((Visual)lastRow!).TranslatePoint(new Point(0, lastRow.Bounds.Height), ViewportOrigin(scrollViewer))!.Value.Y;
        Assert.InRange(bottom, scrollViewer.Viewport.Height - 2, scrollViewer.Viewport.Height + 2);
    }

    // ---- Trough behavior in index units --------------------------------------------------

    private static Track TrackOf(ScrollBar scrollBar) => scrollBar.GetVisualDescendants().OfType<Track>().First();

    private static Point TroughPointBelowThumb(Window window, ScrollBar scrollBar)
    {
        var track = TrackOf(scrollBar);
        var yInTrack = (track.Thumb!.Bounds.Bottom + track.Bounds.Height) / 2;
        return track.TranslatePoint(new Point(track.Bounds.Width / 2, yInTrack), window)!.Value;
    }

    [AvaloniaFact]
    public void TroughPress_PagesOneViewportOfRows()
    {
        var (window, _, wrapper, bar, _) = BuildShownGrid();
        Assert.True(bar.LargeChange > 1, "LargeChange should be the visible row count minus one");

        var point = TroughPointBelowThumb(window, bar);
        window.MouseDown(point, MouseButton.Left);

        Assert.Equal(bar.LargeChange, bar.Value, 3);

        // The rows follow the value.
        wrapper.ApplyPendingForTest();
        window.UpdateLayout();
        var (grid, scrollViewer) = (wrapper.Children.OfType<TableView>().First(),
            wrapper.Children.OfType<TableView>().First().GetVisualDescendants().OfType<ScrollViewer>().First());
        Assert.Equal((int)bar.Value, FirstVisibleIndex(grid, scrollViewer));

        window.MouseUp(point, MouseButton.Left);
    }

    [AvaloniaFact]
    public void TroughHold_PausesWhenThumbReachesPointer()
    {
        var (window, _, wrapper, bar, _) = BuildShownGrid();

        var point = TroughPointBelowThumb(window, bar);
        window.MouseDown(point, MouseButton.Left);
        for (var i = 0; i < 50; i++)
        {
            wrapper.TickTroughHoldForTest();
            wrapper.ApplyPendingForTest();
            window.UpdateLayout();
        }

        Assert.True(bar.Value > 0, "the hold should have paged");
        Assert.True(bar.Value < bar.Maximum,
            $"paging ran past the pointer to the end ({bar.Value} of {bar.Maximum})");

        window.MouseUp(point, MouseButton.Left);
    }

    [AvaloniaFact]
    public void ShiftTroughPress_JumpsToClickPosition()
    {
        var (window, _, wrapper, bar, _) = BuildShownGrid();
        Assert.Equal(0, bar.Value);

        var track = TrackOf(bar);
        var point = track.TranslatePoint(new Point(track.Bounds.Width / 2, track.Bounds.Height * 0.9), window)!.Value;
        window.MouseDown(point, MouseButton.Left, RawInputModifiers.Shift);

        Assert.True(bar.Value > bar.Maximum * 0.5,
            $"shift+click should jump near the click position ({bar.Value} of {bar.Maximum})");

        wrapper.ApplyPendingForTest();
        window.MouseUp(point, MouseButton.Left, RawInputModifiers.Shift);
    }

    [AvaloniaFact]
    public void ThumbPress_DoesNotPage()
    {
        var (window, _, _, bar, _) = BuildShownGrid();

        var thumb = TrackOf(bar).Thumb!;
        var thumbCenter = thumb.TranslatePoint(new Point(thumb.Bounds.Width / 2, thumb.Bounds.Height / 2), window)!.Value;

        window.MouseDown(thumbCenter, MouseButton.Left);
        Assert.Equal(0, bar.Value);
        window.MouseUp(thumbCenter, MouseButton.Left);
    }
}
