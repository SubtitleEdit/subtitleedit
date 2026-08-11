using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Gives a <see cref="TableView"/>'s vertical scroll bar Windows-standard trough behavior:
/// press-and-hold on the trough pages toward the cursor and pauses when the thumb reaches
/// it (#12894), and shift + trough click jumps the thumb straight to the click position
/// (#12051/#12438).
///
/// Both behaviors shipped for the old DataGrid in DataGridScrollBarBehavior, which the
/// TableView migration retired with the grid itself (b24211a1b1) on the note that native
/// ScrollViewer paging replaced it. It only half did: a plain ScrollViewer does page a full
/// viewport per trough click, but the trough of Avalonia's Fluent ScrollBar is a
/// RepeatButton that keeps firing while IsPressed, and Button only re-evaluates IsPressed
/// on PointerMoved - so with a stationary cursor no event arrives as the thumb slides under
/// it and the paging runs right past the cursor to the end of the list. The shift+click
/// jump has no native counterpart at all.
///
/// Unlike the DataGrid version there is no reflection and no LargeChange bookkeeping here:
/// the ScrollViewer template binds the scroll bar's Value two-way to the scroll offset and
/// its LargeChange to the viewport, so a page is just a Value change.
/// </summary>
public static class TableViewScrollBarBehavior
{
    // Avalonia's RepeatButton defaults, so the trough repeats like every other scroll bar.
    private static readonly TimeSpan RepeatDelay = TimeSpan.FromMilliseconds(300);
    private static readonly TimeSpan RepeatInterval = TimeSpan.FromMilliseconds(100);

    /// <summary>
    /// Attach the trough paging and shift+click jump to a TableView. Set once, application
    /// wide, from a single TableView style in Styles.axaml so every grid gets the
    /// Windows-standard scroll bar behavior without each window wiring it by hand (#12438),
    /// including the grids built with plain new TableView() rather than
    /// <see cref="TableViewExtras.MakeTableView"/>.
    /// </summary>
    public static readonly AttachedProperty<bool> EnableTroughPagingProperty =
        AvaloniaProperty.RegisterAttached<TableView, bool>("EnableTroughPaging", typeof(TableViewScrollBarBehavior));

    // Marks a grid already wired so that setting the attached property to true more than once
    // does not stack a second set of pointer handlers.
    private static readonly AttachedProperty<bool> WiredProperty =
        AvaloniaProperty.RegisterAttached<TableView, bool>("TroughPagingWired", typeof(TableViewScrollBarBehavior));

    // Non-null only while the left button is held on the grid's vertical trough.
    private static readonly AttachedProperty<TroughHoldState?> HoldStateProperty =
        AvaloniaProperty.RegisterAttached<TableView, TroughHoldState?>("TroughHoldState", typeof(TableViewScrollBarBehavior));

    public static void SetEnableTroughPaging(TableView tableView, bool value) => tableView.SetValue(EnableTroughPagingProperty, value);

    public static bool GetEnableTroughPaging(TableView tableView) => tableView.GetValue(EnableTroughPagingProperty);

    static TableViewScrollBarBehavior()
    {
        EnableTroughPagingProperty.Changed.AddClassHandler<TableView>((tableView, e) =>
        {
            if (e.NewValue is true && !tableView.GetValue(WiredProperty))
            {
                tableView.SetValue(WiredProperty, true);
                Wire(tableView);
            }
        });
    }

    // All handlers sit on the grid, not the scroll bar: the scroll bar lives two templates
    // deep (grid template -> ScrollViewer template) and only exists once both have applied,
    // while a tunneling handler on the grid sees the press first regardless - before the
    // theme's trough repeat button can engage.
    private static void Wire(TableView tableView)
    {
        tableView.AddHandler(
            InputElement.PointerPressedEvent,
            (sender, e) =>
            {
                var grid = (TableView)sender!;
                if ((e.KeyModifiers & KeyModifiers.Shift) != 0)
                {
                    JumpToClickPosition(grid, e);
                }
                else
                {
                    StartTroughHoldPaging(grid, e);
                }
            },
            RoutingStrategies.Tunnel);

        tableView.PointerMoved += (sender, e) =>
        {
            var grid = (TableView)sender!;
            if (GetHoldState(grid) is { } state)
            {
                state.PointerPosition = e.GetPosition(grid);
            }
        };

        tableView.PointerReleased += (sender, e) =>
        {
            var grid = (TableView)sender!;
            if (GetHoldState(grid) != null && e.InitialPressMouseButton == MouseButton.Left)
            {
                StopTroughHoldPaging(grid);
                e.Pointer.Capture(null);
                e.Handled = true;
            }
        };

        // Covers a release outside the window and window deactivation mid-hold.
        tableView.PointerCaptureLost += (sender, _) => StopTroughHoldPaging((TableView)sender!);
    }

    // State for a press-and-hold on the trough. The direction is latched at press time
    // (Windows-style): re-deciding it per tick would ping-pong once a page overshoots.
    private sealed class TroughHoldState
    {
        public required DispatcherTimer Timer { get; init; }
        public required IPointer Pointer { get; init; }
        public required ScrollBar ScrollBar { get; init; }
        public required Track Track { get; init; }
        public required bool PageDown { get; init; }
        public Point PointerPosition { get; set; } // in grid coordinates
    }

    private static void StartTroughHoldPaging(TableView tableView, PointerPressedEventArgs e)
    {
        if (GetHoldState(tableView) != null ||
            !e.GetCurrentPoint(tableView).Properties.IsLeftButtonPressed ||
            !TryGetTroughPress(tableView, e, out var scrollBar, out var track, out _, out var isBelowThumb))
        {
            return;
        }

        var timer = new DispatcherTimer { Interval = RepeatDelay };
        var state = new TroughHoldState
        {
            Timer = timer,
            Pointer = e.Pointer,
            ScrollBar = scrollBar,
            Track = track,
            PageDown = isBelowThumb,
            PointerPosition = e.GetPosition(tableView),
        };

        timer.Tick += (_, _) =>
        {
            timer.Interval = RepeatInterval;
            TickTroughHoldPaging(tableView, state);
        };

        tableView.SetValue(HoldStateProperty, state);
        e.Pointer.Capture(tableView);
        PageOnce(scrollBar, state.PageDown);
        timer.Start();

        // Keep the trough repeat button from setting IsPressed and starting its own repeat.
        e.Handled = true;
    }

    private static void TickTroughHoldPaging(TableView tableView, TroughHoldState state)
    {
        // A missed release, or a capture taken elsewhere, must not leave the timer paging.
        if (state.Pointer.Captured != tableView)
        {
            StopTroughHoldPaging(tableView);
            return;
        }

        var thumb = state.Track.Thumb;
        var posY = tableView.TranslatePoint(state.PointerPosition, state.Track)?.Y;
        if (thumb == null || posY == null)
        {
            return;
        }

        // Pause (not stop) once the thumb has reached the pointer: paging resumes if the
        // pointer moves further in the latched direction, like the Windows scroll bar.
        if (ShouldPage(posY.Value, thumb.Bounds, state.PageDown))
        {
            PageOnce(state.ScrollBar, state.PageDown);
        }
    }

    private static bool ShouldPage(double posY, Rect thumbBounds, bool pageDown)
    {
        return pageDown ? posY > thumbBounds.Bottom : posY < thumbBounds.Top;
    }

    private static void PageOnce(ScrollBar scrollBar, bool pageDown)
    {
        var delta = pageDown ? scrollBar.LargeChange : -scrollBar.LargeChange;
        var newValue = Math.Clamp(scrollBar.Value + delta, scrollBar.Minimum, scrollBar.Maximum);
        if (newValue != scrollBar.Value)
        {
            scrollBar.Value = newValue;
        }
    }

    private static TroughHoldState? GetHoldState(TableView tableView) => tableView.GetValue(HoldStateProperty);

    private static void StopTroughHoldPaging(TableView tableView)
    {
        if (GetHoldState(tableView) is { } state)
        {
            state.Timer.Stop();
            tableView.SetValue(HoldStateProperty, null);
        }
    }

    // The headless test dispatcher never fires a DispatcherTimer, so the tests step the
    // repeat by hand (TableViewScrollBarTroughPagingTests).
    internal static void TickTroughHoldPagingForTest(TableView tableView)
    {
        if (GetHoldState(tableView) is { } state)
        {
            TickTroughHoldPaging(tableView, state);
        }
    }

    private static void JumpToClickPosition(TableView tableView, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(tableView).Properties.IsLeftButtonPressed ||
            !TryGetTroughPress(tableView, e, out var scrollBar, out var track, out var posY, out _))
        {
            return;
        }

        // Center the thumb on the cursor: subtract half the thumb length and scale by the
        // travel the thumb actually has (track height minus thumb length).
        var range = scrollBar.Maximum - scrollBar.Minimum;
        var thumbLength = track.Thumb?.Bounds.Height ?? 0;
        var travel = Math.Max(1, track.Bounds.Height - thumbLength);
        var offset = posY - (thumbLength / 2.0);
        var fraction = Math.Clamp(offset / travel, 0.0, 1.0);

        scrollBar.Value = Math.Clamp(scrollBar.Minimum + (fraction * range), scrollBar.Minimum, scrollBar.Maximum);
        e.Handled = true;
    }

    /// <summary>
    /// Returns the scroll bar, its track and the press's track-relative Y for a genuine
    /// trough press, or false for presses elsewhere in the grid (rows, headers), outside the
    /// track (the line step arrows), on the thumb itself (which begins a normal drag), on a
    /// scroll bar that is not the grid's own vertical one (the horizontal bar, or one of a
    /// control hosted inside a cell), or when there is nothing to scroll. Shared by the
    /// shift+click jump and the hold paging so both agree on what counts as the trough.
    /// </summary>
    private static bool TryGetTroughPress(TableView tableView, PointerEventArgs e, out ScrollBar scrollBar, out Track track, out double posY, out bool isBelowThumb)
    {
        scrollBar = null!;
        track = null!;
        posY = 0;
        isBelowThumb = false;

        var foundScrollBar = (e.Source as Visual)?.FindAncestorOfType<ScrollBar>(includeSelf: true);
        if (foundScrollBar == null ||
            foundScrollBar.Orientation != Orientation.Vertical ||
            foundScrollBar.FindAncestorOfType<TableView>() != tableView)
        {
            return false;
        }

        var range = foundScrollBar.Maximum - foundScrollBar.Minimum;
        if (double.IsNaN(range) || range <= 0)
        {
            return false;
        }

        var foundTrack = foundScrollBar.GetVisualDescendants().OfType<Track>().FirstOrDefault();
        if (foundTrack == null || foundTrack.Bounds.Height <= 0)
        {
            return false;
        }

        var y = e.GetPosition(foundTrack).Y;
        if (y < 0 || y > foundTrack.Bounds.Height)
        {
            return false;
        }

        var thumb = foundTrack.Thumb;
        if (thumb != null)
        {
            if (y >= thumb.Bounds.Top && y <= thumb.Bounds.Bottom)
            {
                return false;
            }

            isBelowThumb = y > thumb.Bounds.Bottom;
        }
        else
        {
            isBelowThumb = true;
        }

        scrollBar = foundScrollBar;
        track = foundTrack;
        posY = y;
        return true;
    }
}
