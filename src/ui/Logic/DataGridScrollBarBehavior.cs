using System;
using System.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
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
///
/// Press-and-hold is handled here too: the theme's trough RepeatButton repeats while
/// IsPressed, and Button only re-evaluates IsPressed on PointerMoved, so with a stationary
/// cursor it pages past the cursor to the end (issue #12894). This pages toward the cursor
/// and pauses when the thumb reaches it.
/// </summary>
public static class DataGridScrollBarBehavior
{
    private const string VerticalScrollBarPartName = "PART_VerticalScrollbar";

    // Avalonia's RepeatButton defaults, so the trough repeats like every other scroll bar.
    private static readonly TimeSpan RepeatDelay = TimeSpan.FromMilliseconds(300);
    private static readonly TimeSpan RepeatInterval = TimeSpan.FromMilliseconds(100);

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

    // Non-null only while the left button is held on that scroll bar's trough.
    private static readonly AttachedProperty<TroughHoldState?> HoldStateProperty =
        AvaloniaProperty.RegisterAttached<ScrollBar, TroughHoldState?>("TroughHoldState", typeof(DataGridScrollBarBehavior));

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

            // Tunnel so this runs before the theme's trough repeat button can engage.
            verticalScrollBar.AddHandler(
                InputElement.PointerPressedEvent,
                (_, args) =>
                {
                    if ((args.KeyModifiers & KeyModifiers.Shift) != 0)
                    {
                        JumpToClickPosition(grid, verticalScrollBar, args);
                    }
                    else
                    {
                        StartTroughHoldPaging(grid, verticalScrollBar, args);
                    }
                },
                RoutingStrategies.Tunnel);

            verticalScrollBar.PointerMoved += (_, args) =>
            {
                if (GetHoldState(verticalScrollBar) is { } state)
                {
                    state.PointerPosition = args.GetPosition(verticalScrollBar);
                }
            };

            verticalScrollBar.PointerReleased += (_, args) =>
            {
                if (GetHoldState(verticalScrollBar) != null && args.InitialPressMouseButton == MouseButton.Left)
                {
                    StopTroughHoldPaging(verticalScrollBar);
                    args.Pointer.Capture(null);
                    args.Handled = true;
                }
            };

            // Covers a release outside the window, window deactivation and a re-template mid-hold.
            verticalScrollBar.PointerCaptureLost += (_, _) => StopTroughHoldPaging(verticalScrollBar);
        };
    }

    // State for a press-and-hold on one scroll bar's trough. The direction is latched at press
    // time (Windows-style): re-deciding it per tick would ping-pong once a page overshoots.
    private sealed class TroughHoldState
    {
        public required DispatcherTimer Timer { get; init; }
        public required IPointer Pointer { get; init; }
        public required Track Track { get; init; }
        public required bool PageDown { get; init; }
        public Point PointerPosition { get; set; } // in scroll bar coordinates
    }

    private static void StartTroughHoldPaging(DataGrid grid, ScrollBar verticalScrollBar, PointerPressedEventArgs e)
    {
        // Without ProcessVerticalScroll a page would move the thumb but not the rows, so leave
        // the press to the theme's repeat button instead of swallowing it.
        if (ProcessVerticalScrollMethod == null ||
            GetHoldState(verticalScrollBar) != null ||
            !e.GetCurrentPoint(verticalScrollBar).Properties.IsLeftButtonPressed ||
            !TryGetTroughPress(verticalScrollBar, e, out var track, out _, out var isBelowThumb))
        {
            return;
        }

        var timer = new DispatcherTimer { Interval = RepeatDelay };
        var state = new TroughHoldState
        {
            Timer = timer,
            Pointer = e.Pointer,
            Track = track,
            PageDown = isBelowThumb,
            PointerPosition = e.GetPosition(verticalScrollBar),
        };

        timer.Tick += (_, _) =>
        {
            timer.Interval = RepeatInterval;
            TickTroughHoldPaging(grid, verticalScrollBar, state);
        };

        verticalScrollBar.SetValue(HoldStateProperty, state);
        e.Pointer.Capture(verticalScrollBar);
        PageOnce(grid, verticalScrollBar, state.PageDown);
        timer.Start();

        // Keep the trough repeat button from setting IsPressed and starting its own repeat.
        e.Handled = true;
    }

    private static void TickTroughHoldPaging(DataGrid grid, ScrollBar verticalScrollBar, TroughHoldState state)
    {
        // A missed release, or a capture taken elsewhere, must not leave the timer paging.
        if (state.Pointer.Captured != verticalScrollBar)
        {
            StopTroughHoldPaging(verticalScrollBar);
            return;
        }

        var thumb = state.Track.Thumb;
        var posY = verticalScrollBar.TranslatePoint(state.PointerPosition, state.Track)?.Y;
        if (thumb == null || posY == null)
        {
            return;
        }

        // Pause (not stop) once the thumb has reached the pointer: paging resumes if the
        // pointer moves further in the latched direction, like the Windows scroll bar.
        if (ShouldPage(posY.Value, thumb.Bounds, state.PageDown))
        {
            PageOnce(grid, verticalScrollBar, state.PageDown);
        }
    }

    private static bool ShouldPage(double posY, Rect thumbBounds, bool pageDown)
    {
        return pageDown ? posY > thumbBounds.Bottom : posY < thumbBounds.Top;
    }

    private static void PageOnce(DataGrid grid, ScrollBar verticalScrollBar, bool pageDown)
    {
        var delta = pageDown ? verticalScrollBar.LargeChange : -verticalScrollBar.LargeChange;
        var newValue = Math.Clamp(verticalScrollBar.Value + delta, verticalScrollBar.Minimum, verticalScrollBar.Maximum);
        if (newValue == verticalScrollBar.Value)
        {
            return;
        }

        verticalScrollBar.Value = newValue;
        ProcessVerticalScrollMethod?.Invoke(grid, new object[] { ScrollEventType.EndScroll });
    }

    private static TroughHoldState? GetHoldState(ScrollBar verticalScrollBar) => verticalScrollBar.GetValue(HoldStateProperty);

    private static void StopTroughHoldPaging(ScrollBar verticalScrollBar)
    {
        if (GetHoldState(verticalScrollBar) is { } state)
        {
            state.Timer.Stop();
            verticalScrollBar.SetValue(HoldStateProperty, null);
        }
    }

    // The headless test dispatcher never fires a DispatcherTimer, so the tests step the repeat
    // by hand (DataGridScrollBarTroughPagingTests).
    internal static void TickTroughHoldPagingForTest(DataGrid grid, ScrollBar verticalScrollBar)
    {
        if (GetHoldState(verticalScrollBar) is { } state)
        {
            TickTroughHoldPaging(grid, verticalScrollBar, state);
        }
    }

    private static void JumpToClickPosition(DataGrid grid, ScrollBar verticalScrollBar, PointerPressedEventArgs e)
    {
        if (ProcessVerticalScrollMethod == null ||
            !e.GetCurrentPoint(verticalScrollBar).Properties.IsLeftButtonPressed ||
            !TryGetTroughPress(verticalScrollBar, e, out var track, out var posY, out _))
        {
            return;
        }

        // Center the thumb on the cursor: subtract half the thumb length and scale by the
        // travel the thumb actually has (track height minus thumb length).
        var range = verticalScrollBar.Maximum - verticalScrollBar.Minimum;
        var thumbLength = track.Thumb?.Bounds.Height ?? 0;
        var travel = Math.Max(1, track.Bounds.Height - thumbLength);
        var offset = posY - (thumbLength / 2.0);
        var fraction = Math.Clamp(offset / travel, 0.0, 1.0);

        var newValue = verticalScrollBar.Minimum + (fraction * range);
        verticalScrollBar.Value = Math.Clamp(newValue, verticalScrollBar.Minimum, verticalScrollBar.Maximum);

        ProcessVerticalScrollMethod.Invoke(grid, new object[] { ScrollEventType.EndScroll });
        e.Handled = true;
    }

    /// <summary>
    /// Returns the track and the press's track-relative Y for a genuine trough press, or false
    /// for presses outside the track (the line step arrows), on the thumb itself (which begins a
    /// normal drag), or when there is nothing to scroll (#12438 review). Shared by the shift+click
    /// jump and the hold paging so both agree on what counts as the trough.
    /// </summary>
    private static bool TryGetTroughPress(ScrollBar verticalScrollBar, PointerEventArgs e, out Track track, out double posY, out bool isBelowThumb)
    {
        track = null!;
        posY = 0;
        isBelowThumb = false;

        var foundTrack = verticalScrollBar.GetVisualDescendants().OfType<Track>().FirstOrDefault();
        if (foundTrack == null || foundTrack.Bounds.Height <= 0)
        {
            return false;
        }

        var range = verticalScrollBar.Maximum - verticalScrollBar.Minimum;
        if (double.IsNaN(range) || range <= 0)
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

        track = foundTrack;
        posY = y;
        return true;
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
