using System;
using System.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Gives a <see cref="DataGrid"/>'s vertical scroll bar Windows-standard trough behavior:
/// a plain trough click scrolls a full page, and shift + trough click jumps the thumb
/// straight to the click position (issue #12051 and its follow-up).
///
/// Avalonia's DataGrid sets its vertical scroll bar's Maximum and ViewportSize but never
/// its LargeChange, so it keeps RangeBase's default of 1. A trough click raises a large
/// increment that moves the value by a single pixel, which looks like a one line scroll.
/// The horizontal scroll bar is given a LargeChange equal to its viewport, and a plain
/// ScrollViewer binds LargeChange to its viewport too, which is why the Options window
/// pages correctly while the subtitle grid and the Shortcuts grid do not. This keeps the
/// vertical LargeChange in step with the viewport so a page always matches the visible
/// height.
///
/// The DataGrid hooks the scroll bar's Scroll event (not ValueChanged) to refresh its
/// rows, so a programmatic jump also invokes the grid's internal ProcessVerticalScroll.
/// </summary>
public static class DataGridScrollBarBehavior
{
    private const string VerticalScrollBarPartName = "PART_VerticalScrollbar";

    private static readonly MethodInfo? ProcessVerticalScrollMethod = typeof(DataGrid).GetMethod(
        "ProcessVerticalScroll",
        BindingFlags.NonPublic | BindingFlags.Instance);

    /// <summary>
    /// Attach the trough paging and shift+click jump to a DataGrid. Set once, application wide,
    /// from a single DataGrid style in Styles.axaml so every grid gets the Windows-standard
    /// scroll bar behavior without each window wiring it by hand (requested on #12438).
    /// </summary>
    public static readonly AttachedProperty<bool> EnableTroughPagingProperty =
        AvaloniaProperty.RegisterAttached<DataGrid, bool>("EnableTroughPaging", typeof(DataGridScrollBarBehavior));

    // Marks a grid already wired so that setting the attached property to true more than once does
    // not subscribe to TemplateApplied a second time and stack a second set of scroll bar handlers.
    // A normal re-template needs no guard: the single subscription re-runs and wires the fresh
    // scroll bar once, and the discarded template's scroll bar (with its handlers) is dropped.
    private static readonly AttachedProperty<bool> WiredProperty =
        AvaloniaProperty.RegisterAttached<DataGrid, bool>("TroughPagingWired", typeof(DataGridScrollBarBehavior));

    public static void SetEnableTroughPaging(DataGrid grid, bool value) => grid.SetValue(EnableTroughPagingProperty, value);

    public static bool GetEnableTroughPaging(DataGrid grid) => grid.GetValue(EnableTroughPagingProperty);

    static DataGridScrollBarBehavior()
    {
        EnableTroughPagingProperty.Changed.AddClassHandler<DataGrid>((grid, e) =>
        {
            if (e.NewValue is true && !grid.GetValue(WiredProperty))
            {
                grid.SetValue(WiredProperty, true);
                EnableTroughPageScroll(grid);
            }
        });
    }

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

            // Shift + trough click jumps to the click position, matching the Windows scroll
            // bar. Tunnel so this runs before the trough repeat button starts paging.
            verticalScrollBar.AddHandler(
                InputElement.PointerPressedEvent,
                (_, args) => JumpToClickPosition(grid, verticalScrollBar, args),
                RoutingStrategies.Tunnel);
        };
    }

    private static void JumpToClickPosition(DataGrid grid, ScrollBar verticalScrollBar, PointerPressedEventArgs e)
    {
        if ((e.KeyModifiers & KeyModifiers.Shift) == 0 ||
            !e.GetCurrentPoint(verticalScrollBar).Properties.IsLeftButtonPressed)
        {
            return;
        }

        var track = verticalScrollBar.GetVisualDescendants().OfType<Track>().FirstOrDefault();
        if (track == null || track.Bounds.Height <= 0)
        {
            return;
        }

        var range = verticalScrollBar.Maximum - verticalScrollBar.Minimum;
        if (double.IsNaN(range) || range <= 0)
        {
            return;
        }

        // Only a trough click should jump. Ignore clicks that land outside the track (the line
        // step arrows) or on the thumb itself (which begins a normal drag); without this guard a
        // Shift+click on an arrow or the thumb would fling the view to min or max. (#12438 review)
        var posY = e.GetPosition(track).Y;
        if (posY < 0 || posY > track.Bounds.Height)
        {
            return;
        }

        var thumb = track.Thumb;
        if (thumb != null)
        {
            var thumbTop = thumb.Bounds.Y;
            if (posY >= thumbTop && posY <= thumbTop + thumb.Bounds.Height)
            {
                return;
            }
        }

        // Center the thumb on the cursor: subtract half the thumb length and scale by the
        // travel the thumb actually has (track height minus thumb length).
        var thumbLength = thumb?.Bounds.Height ?? 0;
        var travel = Math.Max(1, track.Bounds.Height - thumbLength);
        var offset = posY - (thumbLength / 2.0);
        var fraction = Math.Clamp(offset / travel, 0.0, 1.0);

        var newValue = verticalScrollBar.Minimum + (fraction * range);
        verticalScrollBar.Value = Math.Clamp(newValue, verticalScrollBar.Minimum, verticalScrollBar.Maximum);

        ProcessVerticalScrollMethod?.Invoke(grid, new object[] { ScrollEventType.EndScroll });
        e.Handled = true;
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
