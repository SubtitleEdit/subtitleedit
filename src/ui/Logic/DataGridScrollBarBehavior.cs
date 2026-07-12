using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Gives a <see cref="DataGrid"/>'s vertical scroll bar a working page size so that
/// clicking the scroll bar trough (the channel above or below the thumb) scrolls a full
/// page, as it does everywhere else and on other platforms (issue #12051).
///
/// Avalonia's DataGrid sets its vertical scroll bar's Maximum and ViewportSize but never
/// its LargeChange, so it keeps RangeBase's default of 1. A trough click raises a large
/// increment that moves the value by a single pixel, which looks like a one line scroll.
/// The horizontal scroll bar is given a LargeChange equal to its viewport, and a plain
/// ScrollViewer binds LargeChange to its viewport too, which is why the Options window
/// pages correctly while the subtitle grid and the Shortcuts grid do not. This keeps the
/// vertical LargeChange in step with the viewport so a page always matches the visible
/// height.
/// </summary>
internal static class DataGridScrollBarBehavior
{
    private const string VerticalScrollBarPartName = "PART_VerticalScrollbar";

    public static void EnableTroughPageScroll(DataGrid grid)
    {
        grid.TemplateApplied += (_, e) =>
        {
            var verticalScrollBar = e.NameScope.Find<ScrollBar>(VerticalScrollBarPartName);
            if (verticalScrollBar == null)
            {
                return;
            }

            SyncLargeChange(verticalScrollBar);

            // The DataGrid updates ViewportSize (and Maximum) as the grid is resized or its
            // rows change, so follow it and keep a page equal to the visible height.
            verticalScrollBar.PropertyChanged += (_, args) =>
            {
                if (args.Property == ScrollBar.ViewportSizeProperty ||
                    args.Property == ScrollBar.MaximumProperty)
                {
                    SyncLargeChange(verticalScrollBar);
                }
            };
        };
    }

    private static void SyncLargeChange(ScrollBar verticalScrollBar)
    {
        var viewport = verticalScrollBar.ViewportSize;
        if (!double.IsNaN(viewport) && viewport > 0 && verticalScrollBar.LargeChange != viewport)
        {
            verticalScrollBar.LargeChange = viewport;
        }
    }
}
