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
/// A plain press on the trough of a DataGrid's vertical scroll bar must page exactly one
/// viewport toward the cursor and stop paging on release. The theme's trough RepeatButton
/// kept repeating past the cursor to the end because Button only re-evaluates IsPressed
/// on PointerMoved, so DataGridScrollBarBehavior handles the press itself (#12438 follow-up).
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

    private static Point TroughPointBelowThumb(Window window, ScrollBar scrollBar)
    {
        var track = scrollBar.GetVisualDescendants().OfType<Track>().First();
        var thumb = track.Thumb!;

        // A point centered horizontally, well below the thumb (Value is 0, thumb at top).
        var yInTrack = (thumb.Bounds.Y + thumb.Bounds.Height + track.Bounds.Height) / 2;
        return track.TranslatePoint(new Point(track.Bounds.Width / 2, yInTrack), window)!.Value;
    }

    [AvaloniaFact]
    public void TroughPress_PagesExactlyOneViewport()
    {
        var (window, _, scrollBar) = BuildShownGrid();
        Assert.True(scrollBar.Maximum > 0, "grid should have enough rows to scroll");
        Assert.Equal(0, scrollBar.Value);

        var point = TroughPointBelowThumb(window, scrollBar);
        window.MouseDown(point, MouseButton.Left);

        // One immediate page of one viewport; the repeat timer has not ticked yet.
        Assert.Equal(scrollBar.LargeChange, scrollBar.Value, 3);

        window.MouseUp(point, MouseButton.Left);
    }

    [AvaloniaFact]
    public void TroughRelease_StopsPaging()
    {
        var (window, _, scrollBar) = BuildShownGrid();

        var point = TroughPointBelowThumb(window, scrollBar);
        window.MouseDown(point, MouseButton.Left);
        window.MouseUp(point, MouseButton.Left);

        var valueAfterRelease = scrollBar.Value;
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        AvaloniaHeadlessPlatform.ForceRenderTimerTick();
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        Assert.Equal(valueAfterRelease, scrollBar.Value);
    }

    [AvaloniaFact]
    public void ThumbPress_DoesNotPage()
    {
        var (window, _, scrollBar) = BuildShownGrid();

        var track = scrollBar.GetVisualDescendants().OfType<Track>().First();
        var thumb = track.Thumb!;
        var thumbCenter = thumb.TranslatePoint(
            new Point(thumb.Bounds.Width / 2, thumb.Bounds.Height / 2), window)!.Value;

        window.MouseDown(thumbCenter, MouseButton.Left);
        Assert.Equal(0, scrollBar.Value);
        window.MouseUp(thumbCenter, MouseButton.Left);
    }
}
