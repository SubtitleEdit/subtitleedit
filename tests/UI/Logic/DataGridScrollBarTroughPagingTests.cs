using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Logic;

/// <summary>
/// A press on the trough of a DataGrid's vertical scroll bar pages toward the cursor and pauses
/// when the thumb reaches it, instead of running to the end of the list like the theme's trough
/// RepeatButton did (#12894). The headless dispatcher never fires a DispatcherTimer, so the
/// repeats are stepped by hand through TickTroughHoldPagingForTest.
/// </summary>
public class DataGridScrollBarTroughPagingTests
{
    private sealed record Row(int Number, string Text);

    private static (Window window, DataGrid grid, ScrollBar scrollBar) BuildShownGrid()
    {
        // The TestApp only loads the base Fluent theme; the DataGrid template lives in its
        // own package theme, loaded the same way Program.cs does for the real app. Must be
        // in place before the grid attaches to a window - control themes resolve on attach.
        if (Application.Current!.Styles.OfType<StyleInclude>().All(s => s.Source?.ToString().Contains("DataGrid") != true))
        {
            Application.Current.Styles.Add(new StyleInclude(new Uri("avares://Avalonia.Controls.DataGrid/Themes/Fluent.xaml", UriKind.Absolute))
            {
                Source = new Uri("avares://Avalonia.Controls.DataGrid/Themes/Fluent.xaml")
            });
        }

        var grid = new DataGrid
        {
            ItemsSource = Enumerable.Range(1, 200).Select(i => new Row(i, $"Line {i}")).ToList(),
            Columns =
            {
                new DataGridTextColumn { Header = "#", Binding = new Avalonia.Data.Binding(nameof(Row.Number)) },
                new DataGridTextColumn { Header = "Text", Binding = new Avalonia.Data.Binding(nameof(Row.Text)) },
            },
        };
        DataGridScrollBarBehavior.SetEnableTroughPaging(grid, true);

        var window = new Window { Content = grid, Width = 400, Height = 300 };
        window.Show();
        window.UpdateLayout();
        AvaloniaHeadlessPlatform.ForceRenderTimerTick();
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        var scrollBar = grid.GetVisualDescendants().OfType<ScrollBar>()
            .First(s => s.Name == "PART_VerticalScrollbar");
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

    private static void Repeat(Window window, DataGrid grid, ScrollBar scrollBar, int ticks)
    {
        for (var i = 0; i < ticks; i++)
        {
            DataGridScrollBarBehavior.TickTroughHoldPagingForTest(grid, scrollBar);
            window.UpdateLayout();
        }
    }

    // True once the thumb has caught up with the pointer, so paging should have paused.
    private static bool ThumbReached(Window window, ScrollBar scrollBar, Point windowPoint)
    {
        var track = TrackOf(scrollBar);
        return track.Thumb!.Bounds.Bottom >= window.TranslatePoint(windowPoint, track)!.Value.Y;
    }

    [AvaloniaFact]
    public void TroughPress_PagesExactlyOneViewport()
    {
        var (window, _, scrollBar) = BuildShownGrid();
        Assert.True(scrollBar.Maximum > 0, "grid should have enough rows to scroll");
        Assert.True(scrollBar.LargeChange > 1, "LargeChange should follow the viewport, not RangeBase's default");
        Assert.Equal(0, scrollBar.Value);

        var point = TroughPointBelowThumb(window, scrollBar);
        window.MouseDown(point, MouseButton.Left);

        // One immediate page of one viewport; the repeat has not been stepped yet.
        Assert.Equal(scrollBar.LargeChange, scrollBar.Value, 3);

        window.MouseUp(point, MouseButton.Left);
    }

    [AvaloniaFact]
    public void TroughHold_PausesWhenThumbReachesPointer()
    {
        var (window, grid, scrollBar) = BuildShownGrid();

        var point = TroughPointBelowThumb(window, scrollBar);
        window.MouseDown(point, MouseButton.Left);
        Repeat(window, grid, scrollBar, 50);

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
        Repeat(window, grid, scrollBar, 50);
        var pausedValue = scrollBar.Value;

        var lower = TrackPoint(window, scrollBar, 0.95);
        window.MouseMove(lower);
        Repeat(window, grid, scrollBar, 50);

        Assert.True(scrollBar.Value > pausedValue, "paging should resume when the pointer moves further down");

        window.MouseUp(lower, MouseButton.Left);
    }

    [AvaloniaFact]
    public void TroughHold_DoesNotReverseWhenPointerMovesBack()
    {
        var (window, grid, scrollBar) = BuildShownGrid();

        var point = TroughPointBelowThumb(window, scrollBar);
        window.MouseDown(point, MouseButton.Left);
        Repeat(window, grid, scrollBar, 50);
        var pausedValue = scrollBar.Value;

        window.MouseMove(TrackPoint(window, scrollBar, 0.0));
        Repeat(window, grid, scrollBar, 10);

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
        Repeat(window, grid, scrollBar, 10);

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
        Repeat(window, grid, scrollBar, 10);

        Assert.Equal(0, scrollBar.Value);

        window.MouseUp(thumbCenter, MouseButton.Left);
    }
}
