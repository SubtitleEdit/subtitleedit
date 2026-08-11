using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Logic;

/// <summary>
/// A press on the trough of a TableView's vertical scroll bar pages toward the cursor and
/// pauses when the thumb reaches it, and shift+click jumps the thumb to the click position,
/// instead of the theme's trough RepeatButton paging right past the cursor to the end of
/// the list (#12894 - fixed for the DataGrid, regressed when that behavior was retired with
/// the DataGrid itself). The headless dispatcher never fires a DispatcherTimer, so the
/// repeats are stepped by hand through TickTroughHoldPagingForTest.
/// </summary>
public class TableViewScrollBarTroughPagingTests : IDisposable
{
    // Every window opened by a test is closed again in Dispose: if a test stops early, an
    // unclosed window would outlive the test and race with the headless session teardown.
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

    /// <param name="viaAppStyles">
    /// Enable the behavior the way the app does - the TableView style in Styles.axaml - instead
    /// of setting the attached property on the grid. The wiring is what the DataGrid retirement
    /// dropped, so a selector or property rename must fail a test rather than ship silently.
    /// </param>
    private (Window window, TableView grid, ScrollBar scrollBar) BuildShownGrid(bool viaAppStyles = false)
    {
        var grid = new TableView
        {
            ItemsSource = Enumerable.Range(1, 200).Select(i => new Row(i, $"Line {i}")).ToList(),
            Columns =
            {
                new TableViewColumn { Header = "#", Binding = new Avalonia.Data.Binding(nameof(Row.Number)) },
                new TableViewColumn { Header = "Text", Binding = new Avalonia.Data.Binding(nameof(Row.Text)) },
            },
        };
        if (!viaAppStyles)
        {
            TableViewScrollBarBehavior.SetEnableTroughPaging(grid, true);
        }

        var window = new Window { Content = grid, Width = 400, Height = 300 };
        if (viaAppStyles)
        {
            window.Styles.Add((Styles)AvaloniaXamlLoader.Load(new Uri("avares://SubtitleEdit/Styles.axaml")));
        }

        // The app keeps scroll bars always expanded (UiTheme.ApplyScrollBarStyle, except on
        // macOS set to auto-hide); do the same here so the trough is hit-testable without the
        // hover-expansion that headless input never delivers.
        window.Styles.Add(new Style(x => x.OfType<ScrollBar>())
        {
            Setters = { new Setter(ScrollBar.AllowAutoHideProperty, false) },
        });

        _windows.Add(window);
        window.Show();
        window.UpdateLayout();
        AvaloniaHeadlessPlatform.ForceRenderTimerTick();
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        var scrollBar = grid.GetVisualDescendants().OfType<ScrollBar>()
            .First(s => s.Orientation == Orientation.Vertical);
        return (window, grid, scrollBar);
    }

    private static Track TrackOf(ScrollBar scrollBar) => scrollBar.GetVisualDescendants().OfType<Track>().First();

    // A point centered horizontally, at the given fraction down the track, in window coordinates.
    private static Point TrackPoint(Window window, ScrollBar scrollBar, double fraction)
    {
        var track = TrackOf(scrollBar);
        return track.TranslatePoint(new Point(track.Bounds.Width / 2, track.Bounds.Height * fraction), window)!.Value;
    }

    private static Point TroughPointBelowThumb(Window window, ScrollBar scrollBar)
    {
        var track = TrackOf(scrollBar);

        // Well below the thumb, which sits at the top while Value is 0.
        var yInTrack = (track.Thumb!.Bounds.Bottom + track.Bounds.Height) / 2;
        return track.TranslatePoint(new Point(track.Bounds.Width / 2, yInTrack), window)!.Value;
    }

    private static void Repeat(Window window, TableView grid, int ticks)
    {
        for (var i = 0; i < ticks; i++)
        {
            TableViewScrollBarBehavior.TickTroughHoldPagingForTest(grid);
            window.UpdateLayout();
        }
    }

    // True once the thumb has caught up with the pointer, so paging should have paused.
    private static bool ThumbReached(Window window, ScrollBar scrollBar, Point windowPoint)
    {
        var track = TrackOf(scrollBar);
        return track.Thumb!.Bounds.Bottom >= window.TranslatePoint(windowPoint, track)!.Value.Y;
    }

    /// <summary>
    /// The behavior only reaches users through the TableView style in Styles.axaml, and that
    /// wiring - not the behavior - is what went missing with the DataGrid. Every other test here
    /// sets the attached property by hand, so without this one a renamed property or a selector
    /// that stops matching would leave the whole file green and the grids unfixed.
    /// </summary>
    [AvaloniaFact]
    public void StylesAxaml_EnablesTroughPagingOnEveryGrid()
    {
        var (window, grid, scrollBar) = BuildShownGrid(viaAppStyles: true);

        Assert.True(TableViewScrollBarBehavior.GetEnableTroughPaging(grid),
            "the TableView style in Styles.axaml no longer sets EnableTroughPaging");

        var point = TroughPointBelowThumb(window, scrollBar);
        window.MouseDown(point, MouseButton.Left);
        Repeat(window, grid, 50);

        Assert.True(scrollBar.Value > 0, "a trough press on a style-wired grid did not page");
        Assert.True(scrollBar.Value < scrollBar.Maximum,
            $"paging ran past the pointer to the end ({scrollBar.Value} of {scrollBar.Maximum})");

        window.MouseUp(point, MouseButton.Left);
    }

    [AvaloniaFact]
    public void TroughPress_PagesExactlyOneViewport()
    {
        var (window, grid, scrollBar) = BuildShownGrid();
        Assert.True(scrollBar.Maximum > 0, "grid should have enough rows to scroll");
        Assert.True(scrollBar.LargeChange > 1, "LargeChange should follow the viewport via the ScrollViewer template binding");
        Assert.Equal(0, scrollBar.Value);

        var point = TroughPointBelowThumb(window, scrollBar);
        window.MouseDown(point, MouseButton.Left);

        // One immediate page of one viewport; the repeat has not been stepped yet.
        Assert.Equal(scrollBar.LargeChange, scrollBar.Value, 3);

        // The rows follow: the scroll bar's Value is bound two-way to the scroll offset
        // (the old DataGrid needed its internal scroll processing invoked by reflection).
        var scrollViewer = grid.GetVisualDescendants().OfType<ScrollViewer>().First();
        Assert.Equal(scrollBar.Value, scrollViewer.Offset.Y, 3);

        window.MouseUp(point, MouseButton.Left);
    }

    [AvaloniaFact]
    public void TroughHold_PausesWhenThumbReachesPointer()
    {
        var (window, grid, scrollBar) = BuildShownGrid();

        var point = TroughPointBelowThumb(window, scrollBar);
        window.MouseDown(point, MouseButton.Left);
        Repeat(window, grid, 50);

        Assert.True(ThumbReached(window, scrollBar, point), "the thumb should have reached the pointer");
        Assert.True(scrollBar.Value > 0, "the hold should have paged");
        Assert.True(scrollBar.Value < scrollBar.Maximum, $"paging ran past the pointer to the end ({scrollBar.Value} of {scrollBar.Maximum})");

        window.MouseUp(point, MouseButton.Left);
    }

    [AvaloniaFact]
    public void TroughHold_ResumesWhenPointerMovesFurther()
    {
        var (window, grid, scrollBar) = BuildShownGrid();

        var point = TroughPointBelowThumb(window, scrollBar);
        window.MouseDown(point, MouseButton.Left);
        Repeat(window, grid, 50);
        var pausedValue = scrollBar.Value;

        var lower = TrackPoint(window, scrollBar, 0.95);
        window.MouseMove(lower);
        Repeat(window, grid, 50);

        Assert.True(scrollBar.Value > pausedValue, "paging should resume when the pointer moves further down");

        window.MouseUp(lower, MouseButton.Left);
    }

    [AvaloniaFact]
    public void TroughHold_DoesNotReverseWhenPointerMovesBack()
    {
        var (window, grid, scrollBar) = BuildShownGrid();

        var point = TroughPointBelowThumb(window, scrollBar);
        window.MouseDown(point, MouseButton.Left);
        Repeat(window, grid, 50);
        var pausedValue = scrollBar.Value;

        window.MouseMove(TrackPoint(window, scrollBar, 0.0));
        Repeat(window, grid, 10);

        Assert.Equal(pausedValue, scrollBar.Value);

        window.MouseUp(point, MouseButton.Left);
    }

    [AvaloniaFact]
    public void TroughRelease_StopsPaging()
    {
        var (window, grid, scrollBar) = BuildShownGrid();

        var point = TroughPointBelowThumb(window, scrollBar);
        window.MouseDown(point, MouseButton.Left);
        window.MouseUp(point, MouseButton.Left);

        var valueAfterRelease = scrollBar.Value;
        Repeat(window, grid, 10);

        Assert.Equal(valueAfterRelease, scrollBar.Value);
    }

    [AvaloniaFact]
    public void ThumbPress_DoesNotPage()
    {
        var (window, grid, scrollBar) = BuildShownGrid();

        var thumb = TrackOf(scrollBar).Thumb!;
        var thumbCenter = thumb.TranslatePoint(
            new Point(thumb.Bounds.Width / 2, thumb.Bounds.Height / 2), window)!.Value;

        window.MouseDown(thumbCenter, MouseButton.Left);
        Repeat(window, grid, 10);

        Assert.Equal(0, scrollBar.Value);

        window.MouseUp(thumbCenter, MouseButton.Left);
    }

    [AvaloniaFact]
    public void ShiftTroughPress_JumpsToClickPosition()
    {
        var (window, _, scrollBar) = BuildShownGrid();
        Assert.Equal(0, scrollBar.Value);

        var point = TrackPoint(window, scrollBar, 0.9);
        window.MouseDown(point, MouseButton.Left, RawInputModifiers.Shift);

        Assert.True(scrollBar.Value > scrollBar.Maximum * 0.5,
            $"shift+click should jump near the click position ({scrollBar.Value} of {scrollBar.Maximum})");

        window.MouseUp(point, MouseButton.Left, RawInputModifiers.Shift);
    }
}
