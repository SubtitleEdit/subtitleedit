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
/// Press-and-hold on the trough is also handled here rather than left to the theme's
/// trough RepeatButton. That button keeps repeating while IsPressed, and Button only
/// re-evaluates IsPressed on PointerMoved — with a stationary cursor no move event
/// arrives as the thumb slides under it, so it pages straight past the cursor to the
/// end. This class pages toward the cursor and pauses when the thumb reaches it,
/// resuming only if the pointer moves further along, like the Windows scroll bar.
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

            // One state instance per applied template: a re-template wires a fresh scroll
            // bar with fresh state, and the discarded template's timer dies with its capture.
            var holdState = new TroughHoldState();

            // Shift + trough click jumps to the click position, a plain trough press pages
            // and repeats toward the cursor, both matching the Windows scroll bar. Tunnel so
            // this runs before the theme's trough repeat button can engage.
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
                        StartTroughHoldPaging(grid, verticalScrollBar, holdState, args);
                    }
                },
                RoutingStrategies.Tunnel);

            verticalScrollBar.PointerMoved += (_, args) =>
            {
                if (holdState.IsActive)
                {
                    holdState.PointerPosition = args.GetPosition(verticalScrollBar);
                }
            };

            verticalScrollBar.PointerReleased += (_, args) =>
            {
                if (holdState.IsActive && args.InitialPressMouseButton == MouseButton.Left)
                {
                    StopTroughHoldPaging(holdState);
                    args.Pointer.Capture(null);
                    args.Handled = true;
                }
            };

            verticalScrollBar.PointerCaptureLost += (_, _) => StopTroughHoldPaging(holdState);
        };
    }

    // Per scroll bar state for a press-and-hold on the trough, captured by that scroll
    // bar's handlers. The direction is latched at press time (Windows-style) because
    // re-deciding it per tick would ping-pong around the cursor once a page overshoots.
    private sealed class TroughHoldState
    {
        public DispatcherTimer? Timer;
        public bool IsActive;
        public bool PageDown;
        public Point PointerPosition; // last pointer position, in scroll bar coordinates
    }

    private static void StartTroughHoldPaging(DataGrid grid, ScrollBar verticalScrollBar, TroughHoldState holdState, PointerPressedEventArgs e)
    {
        if (holdState.IsActive ||
            !e.GetCurrentPoint(verticalScrollBar).Properties.IsLeftButtonPressed ||
            !TryGetTroughPress(verticalScrollBar, e, out _, out _, out var isBelowThumb))
        {
            return;
        }

        holdState.IsActive = true;
        holdState.PageDown = isBelowThumb;
        holdState.PointerPosition = e.GetPosition(verticalScrollBar);
        e.Pointer.Capture(verticalScrollBar);

        PageOnce(grid, verticalScrollBar, isBelowThumb);

        // First repeat after the RepeatButton default delay, then its default rate, so the
        // grid's trough feels the same as every other Avalonia scroll bar in the app.
        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
        timer.Tick += (_, _) =>
        {
            timer.Interval = TimeSpan.FromMilliseconds(100);
            TickTroughHoldPaging(grid, verticalScrollBar, holdState);
        };
        holdState.Timer = timer;
        timer.Start();

        // Keep the trough repeat button from setting IsPressed and starting its own repeat.
        e.Handled = true;
    }

    private static void TickTroughHoldPaging(DataGrid grid, ScrollBar verticalScrollBar, TroughHoldState holdState)
    {
        var track = verticalScrollBar.GetVisualDescendants().OfType<Track>().FirstOrDefault();
        var thumb = track?.Thumb;
        if (track == null || thumb == null)
        {
            return;
        }

        var posY = verticalScrollBar.TranslatePoint(holdState.PointerPosition, track)?.Y;
        if (posY == null)
        {
            return;
        }

        // Pause (not stop) once the thumb has reached the pointer: paging resumes if the
        // pointer moves further in the latched direction, like the Windows scroll bar.
        var shouldPage = holdState.PageDown
            ? posY > thumb.Bounds.Y + thumb.Bounds.Height
            : posY < thumb.Bounds.Y;
        if (shouldPage)
        {
            PageOnce(grid, verticalScrollBar, holdState.PageDown);
        }
    }

    private static void PageOnce(DataGrid grid, ScrollBar verticalScrollBar, bool pageDown)
    {
        var delta = pageDown ? verticalScrollBar.LargeChange : -verticalScrollBar.LargeChange;
        verticalScrollBar.Value = Math.Clamp(verticalScrollBar.Value + delta, verticalScrollBar.Minimum, verticalScrollBar.Maximum);
        ProcessVerticalScrollMethod?.Invoke(grid, new object[] { ScrollEventType.EndScroll });
    }

    private static void StopTroughHoldPaging(TroughHoldState holdState)
    {
        holdState.Timer?.Stop();
        holdState.Timer = null;
        holdState.IsActive = false;
    }

    private static void JumpToClickPosition(DataGrid grid, ScrollBar verticalScrollBar, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(verticalScrollBar).Properties.IsLeftButtonPressed ||
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

        ProcessVerticalScrollMethod?.Invoke(grid, new object[] { ScrollEventType.EndScroll });
        e.Handled = true;
    }

    /// <summary>
    /// Returns the track and the press's track-relative Y for a genuine trough press, or
    /// false for presses outside the track (the line step arrows), on the thumb itself
    /// (which begins a normal drag), or when there is nothing to scroll; without this guard
    /// a press on an arrow or the thumb would jump or page unexpectedly. (#12438 review)
    /// Shared by the shift+click jump and the plain press hold-paging so both agree on
    /// what counts as the trough.
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
            var thumbTop = thumb.Bounds.Y;
            if (y >= thumbTop && y <= thumbTop + thumb.Bounds.Height)
            {
                return false;
            }

            isBelowThumb = y > thumbTop + thumb.Bounds.Height;
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
