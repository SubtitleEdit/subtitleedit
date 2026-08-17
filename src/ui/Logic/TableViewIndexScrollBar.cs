using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Hosts a <see cref="TableView"/> next to a vertical scroll bar that works in row indices
/// instead of pixels, so the thumb stays steady with variable-height rows (issue #13579).
///
/// TableView virtualizes with Avalonia's VirtualizingStackPanel, which estimates the total
/// extent as "realized rows + remaining count x average realized height". With rows of one
/// and two text lines the average changes with every wheel tick as the realized window moves,
/// the extent is recomputed, and the native thumb jumps around (upstream: AvaloniaUI/Avalonia
/// #17831 "More precise VirtualizingStackPanel"). The estimator is private and the native
/// scroll bar cannot be re-united: any ScrollBar whose TemplatedParent is a ScrollViewer
/// pushes every Value change straight into the pixel offset from its own PropertyChanged.
///
/// So the native vertical bar is hidden (ScrollViewer.VerticalScrollBarVisibility=Hidden on
/// the grid, template-bound into the inner ScrollViewer) and this control docks a standalone
/// ScrollBar - TemplatedParent null, so it never couples to any ScrollViewer - beside the
/// grid, mapped like the SE4 list view:
///   Value    = index of the first visible row (+ the fraction scrolled into it)
///   Maximum  = row count - fully visible rows
/// The scroll wheel, keyboard and ScrollIntoView still move the pixel offset; ScrollChanged
/// re-derives Value from the realized rows. Dragging the thumb (or the trough/step buttons)
/// changes Value; the row at floor(Value) is then placed at the viewport top via
/// <see cref="TableViewExtras.PrePositionScroll"/>, which realizes at most a few viewports
/// instead of walking the list.
///
/// The trough gets the same Windows-standard press-and-hold behavior the in-grid bars get
/// from <see cref="TableViewScrollBarBehavior"/> (#12894: page toward the cursor, pause when
/// the thumb reaches it; #12051/#12438: shift+click jumps to the position). That behavior
/// only wires bars *inside* a TableView, which this bar deliberately is not, so the small
/// press/hold state machine is replicated here in index units.
/// </summary>
public sealed class TableViewIndexScrollBar : Grid
{
    // Avalonia's RepeatButton defaults, same as TableViewScrollBarBehavior.
    private static readonly TimeSpan RepeatDelay = TimeSpan.FromMilliseconds(300);
    private static readonly TimeSpan RepeatInterval = TimeSpan.FromMilliseconds(100);

    private readonly TableView _tableView;
    private readonly ScrollBar _bar;
    private ScrollViewer? _scrollViewer;

    // Writing bar properties from the view state - ignore the resulting Value change.
    private bool _syncingBar;
    // Scrolling the view from a bar value - ignore the resulting ScrollChanged.
    private bool _applyingValue;
    // Latest user-driven Value not yet applied; coalesces a fast thumb drag to one
    // scroll per dispatcher pass instead of one per pointer move.
    private double? _pendingValue;
    // Left button held somewhere on the bar (thumb drag or trough hold). The pointer owns
    // Value for the duration, so the view->bar sync must not write it back mid-drag.
    private bool _barPressed;

    // Non-null only while the left button is held on the trough.
    private TroughHold? _troughHold;

    // Row to put back at the viewport top after a pending row-height change (PreserveTopRow).
    private double? _preservedRow;

    private sealed class TroughHold
    {
        public required DispatcherTimer Timer { get; init; }
        public required IPointer Pointer { get; init; }
        public required Track Track { get; init; }
        public required bool PageDown { get; init; }
        public Point PointerPosition { get; set; } // in bar coordinates
    }

    public TableViewIndexScrollBar(TableView tableView)
    {
        _tableView = tableView;

        ColumnDefinitions = new ColumnDefinitions("*,Auto");

        // Hide the native pixel-mapped vertical bar; the grid still scrolls, it just has
        // no bar of its own. (The horizontal bar is untouched.)
        ScrollViewer.SetVerticalScrollBarVisibility(tableView, ScrollBarVisibility.Hidden);

        _bar = new ScrollBar
        {
            Orientation = Orientation.Vertical,
            // Auto hides the bar when everything fits (Maximum stays 0), like the native one.
            Visibility = ScrollBarVisibility.Auto,
            Minimum = 0,
            Maximum = 0,
            SmallChange = 1,
        };

        SetColumn(tableView, 0);
        SetColumn(_bar, 1);
        Children.Add(tableView);
        Children.Add(_bar);

        tableView.Loaded += (_, _) => HookScrollViewer();
        tableView.PropertyChanged += (_, e) =>
        {
            if (e.Property == ItemsControl.ItemCountProperty)
            {
                SyncBarFromView();
            }
        };

        _bar.PropertyChanged += (_, e) =>
        {
            if (e.Property == RangeBase.ValueProperty && !_syncingBar)
            {
                QueueApply(_bar.Value);
            }
        };

        // The native bar sits inside the ScrollViewer, so wheeling over it scrolls the list;
        // keep that for this bar by forwarding the wheel as rows.
        _bar.PointerWheelChanged += (_, e) =>
        {
            if (_scrollViewer is { } scrollViewer)
            {
                var rowHeight = AverageRealizedRowHeight();
                if (rowHeight > 0)
                {
                    var newY = Math.Clamp(
                        scrollViewer.Offset.Y - e.Delta.Y * 3 * rowHeight,
                        0, Math.Max(0, scrollViewer.Extent.Height - scrollViewer.Viewport.Height));
                    scrollViewer.Offset = new Vector(scrollViewer.Offset.X, newY);
                }

                e.Handled = true;
            }
        };

        WireTroughBehavior();
    }

    private void HookScrollViewer()
    {
        if (_scrollViewer != null)
        {
            return;
        }

        _scrollViewer = _tableView.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
        if (_scrollViewer == null)
        {
            // Template not applied yet - try again after the layout pass that applies it.
            Dispatcher.UIThread.Post(HookScrollViewer, DispatcherPriority.Loaded);
            return;
        }

        _scrollViewer.ScrollChanged += (_, _) => SyncBarFromView();

        // Start the bar below the column header, like the native bar in the grid template.
        var header = _tableView.GetVisualDescendants().OfType<TableViewColumnHeadersPresenter>().FirstOrDefault();
        if (header != null && header.Bounds.Height > 0)
        {
            _bar.Margin = new Thickness(0, header.Bounds.Height, 0, 0);
        }

        SyncBarFromView();
    }

    /// <summary>
    /// The visual whose origin is the viewport's top-left corner. The TableView template
    /// keeps the column header *inside* the ScrollViewer (pinned above the rows), so the
    /// ScrollViewer's own origin sits a header height above the viewport - rows must be
    /// measured against the content presenter instead.
    /// </summary>
    private Visual? ViewportOrigin => (Visual?)_scrollViewer?.Presenter ?? _scrollViewer;

    /// <summary>All realized rows with a height, as (index, top, height) with the top in
    /// viewport coordinates, ordered by index.</summary>
    private List<(int Index, double Top, double Height)> GetRealizedRows()
    {
        var rows = new List<(int Index, double Top, double Height)>();
        if (ViewportOrigin is not { } viewportOrigin)
        {
            return rows;
        }

        foreach (var row in _tableView.GetRealizedContainers().OfType<TableViewRow>())
        {
            var height = row.Bounds.Height;
            if (height <= 0)
            {
                continue;
            }

            var index = _tableView.IndexFromContainer(row);
            if (index < 0)
            {
                continue;
            }

            var top = ((Visual)row).TranslatePoint(new Point(0, 0), viewportOrigin)?.Y;
            if (top == null)
            {
                continue;
            }

            rows.Add((index, top.Value, height));
        }

        rows.Sort((a, b) => a.Index.CompareTo(b.Index));
        return rows;
    }

    private double AverageRealizedRowHeight()
    {
        var rows = GetRealizedRows();
        return rows.Count == 0 ? 0 : rows.Average(r => r.Height);
    }

    private void SyncBarFromView()
    {
        if (_applyingValue || _scrollViewer == null)
        {
            return;
        }

        var count = _tableView.ItemCount;
        var rows = GetRealizedRows();
        if (count == 0 || rows.Count == 0)
        {
            _syncingBar = true;
            _bar.Maximum = 0;
            _bar.Value = 0;
            _syncingBar = false;
            return;
        }

        var viewportHeight = _scrollViewer.Viewport.Height;

        // The row under the viewport top edge carries the value; the fraction scrolled into
        // it keeps the thumb gliding instead of stepping row by row.
        var first = rows.FirstOrDefault(r => r.Top + r.Height > 0.5, rows[^1]);
        var value = first.Index + Math.Clamp(-first.Top / first.Height, 0, 1);

        var fullyVisible = rows.Count(r => r.Top >= -0.5 && r.Top + r.Height <= viewportHeight + 0.5);
        var visibleRows = Math.Max(1, fullyVisible);
        var maximum = Math.Max(0, count - visibleRows);

        // Pin the ends: the pixel offset knows exactly when the list is at the top or the
        // bottom, and the thumb must sit hard against the end there.
        if (_scrollViewer.Offset.Y <= 0.5)
        {
            value = 0;
        }
        else if (_scrollViewer.Offset.Y >= _scrollViewer.Extent.Height - viewportHeight - 0.5)
        {
            value = maximum;
        }

        _syncingBar = true;
        _bar.Maximum = maximum;
        _bar.ViewportSize = visibleRows;
        _bar.LargeChange = Math.Max(1, visibleRows - 1);

        // While the pointer is down on the bar the drag owns Value; writing it back from
        // layout would make the thumb fight the hand holding it.
        if (!_barPressed && Math.Abs(_bar.Value - value) > 0.001)
        {
            _bar.Value = Math.Clamp(value, 0, maximum);
        }

        _syncingBar = false;
    }

    /// <summary>
    /// Keeps the row at the viewport top where it is across a change that re-measures every
    /// row - the OCR grid's Ctrl+plus/minus image zoom. The ScrollViewer preserves its
    /// *pixel* offset when the rows grow or shrink, so the user lands on a different row
    /// (zooming out far enough even pins the list to the end); in index units the same view
    /// is simply the same Value, so re-apply it once the new heights have been measured.
    /// Call it before making the change. Repeated calls while one is pending keep the first
    /// row, so holding the zoom key down does not drift.
    /// </summary>
    public void PreserveTopRow()
    {
        if (_preservedRow != null)
        {
            return;
        }

        _preservedRow = _bar.Value;

        // Loaded priority: after the layout pass that applies the new row heights (and so
        // after ScrollChanged has re-synced Maximum for the clamp below).
        Dispatcher.UIThread.Post(() =>
        {
            var preserved = _preservedRow;
            _preservedRow = null;
            if (preserved is { } value && _scrollViewer != null && _tableView.ItemCount > 0)
            {
                ApplyBarValue(Math.Clamp(value, 0, _bar.Maximum));
            }
        }, DispatcherPriority.Loaded);
    }

    private void QueueApply(double value)
    {
        var schedule = _pendingValue == null;
        _pendingValue = value;
        if (schedule)
        {
            // Render priority: one scroll per frame however fast the thumb moves.
            Dispatcher.UIThread.Post(() =>
            {
                if (_pendingValue is { } pending)
                {
                    _pendingValue = null;
                    ApplyBarValue(pending);
                }
            }, DispatcherPriority.Render);
        }
    }

    private void ApplyBarValue(double value)
    {
        if (_scrollViewer == null)
        {
            return;
        }

        var count = _tableView.ItemCount;
        if (count == 0)
        {
            return;
        }

        _applyingValue = true;
        // Placing a row at the viewport top realizes rows as it goes, so the extent estimate
        // moves under the loop below - an anchor restore in the middle of that would drag the
        // view back to where the thumb just left (#13619).
        using var anchorSuspended = TableViewScrollAnchor.GetFor(_tableView)?.Suspend();
        try
        {
            if (value <= 0.001)
            {
                _scrollViewer.Offset = new Vector(_scrollViewer.Offset.X, 0);
                _tableView.UpdateLayout();
                return;
            }

            if (value >= _bar.Maximum - 0.001)
            {
                ScrollToEnd(count);
                return;
            }

            var index = Math.Clamp((int)value, 0, count - 1);
            var fraction = value - index;

            // Jump the offset near the target first so ScrollIntoView never row-walks.
            TableViewExtras.PrePositionScroll(_tableView, index);
            _tableView.ScrollIntoView(index);
            _tableView.UpdateLayout();

            // Put the row's top at the viewport top, plus the fractional part. Setting the
            // offset makes the panel re-estimate and re-anchor, which can nudge the rows by
            // a fraction of a row (or recycle the target's container entirely) - measure
            // and correct again until the row stays put.
            for (var pass = 0; pass < 3; pass++)
            {
                if (_tableView.ContainerFromIndex(index) is not { } row || row.Bounds.Height <= 0)
                {
                    // The re-anchor derealized the target; bring it back and try again.
                    _tableView.ScrollIntoView(index);
                    _tableView.UpdateLayout();
                    if (_tableView.ContainerFromIndex(index) is not { } retried || retried.Bounds.Height <= 0)
                    {
                        break;
                    }

                    row = retried;
                }

                if (ViewportOrigin is not { } viewportOrigin ||
                    row.TranslatePoint(new Point(0, 0), viewportOrigin)?.Y is not { } rowTop)
                {
                    break;
                }

                var delta = rowTop + fraction * row.Bounds.Height;
                var newY = Math.Clamp(
                    _scrollViewer.Offset.Y + delta,
                    0, Math.Max(0, _scrollViewer.Extent.Height - _scrollViewer.Viewport.Height));
                if (Math.Abs(newY - _scrollViewer.Offset.Y) < 0.5)
                {
                    break;
                }

                _scrollViewer.Offset = new Vector(_scrollViewer.Offset.X, newY);
                _tableView.UpdateLayout();
            }
        }
        finally
        {
            _applyingValue = false;
        }

        SyncBarFromView();
    }

    private void ScrollToEnd(int count)
    {
        if (_scrollViewer == null)
        {
            return;
        }

        TableViewExtras.PrePositionScroll(_tableView, count - 1);
        _tableView.ScrollIntoView(count - 1);
        _tableView.UpdateLayout();

        // The extent is re-estimated as rows realize near the end, so clamping once is not
        // enough - settle until the offset stops moving.
        for (var i = 0; i < 3; i++)
        {
            var maxY = Math.Max(0, _scrollViewer.Extent.Height - _scrollViewer.Viewport.Height);
            if (Math.Abs(_scrollViewer.Offset.Y - maxY) < 0.5)
            {
                break;
            }

            _scrollViewer.Offset = new Vector(_scrollViewer.Offset.X, maxY);
            _tableView.UpdateLayout();
        }
    }

    // ---- Trough press-and-hold / shift+click, in index units ----------------------------

    private void WireTroughBehavior()
    {
        _bar.AddHandler(PointerPressedEvent, (_, e) =>
        {
            if (!e.GetCurrentPoint(_bar).Properties.IsLeftButtonPressed)
            {
                return;
            }

            _barPressed = true;

            if ((e.KeyModifiers & KeyModifiers.Shift) != 0)
            {
                JumpToClickPosition(e);
            }
            else
            {
                StartTroughHold(e);
            }
        }, RoutingStrategies.Tunnel);

        _bar.PointerMoved += (_, e) =>
        {
            if (_troughHold is { } hold)
            {
                hold.PointerPosition = e.GetPosition(_bar);
            }
        };

        _bar.AddHandler(PointerReleasedEvent, (_, e) =>
        {
            if (e.InitialPressMouseButton == MouseButton.Left)
            {
                _barPressed = false;
                if (_troughHold != null)
                {
                    StopTroughHold();
                    e.Pointer.Capture(null);
                    e.Handled = true;
                }
            }
        }, RoutingStrategies.Tunnel, handledEventsToo: true);

        _bar.PointerCaptureLost += (_, _) =>
        {
            _barPressed = false;
            StopTroughHold();
        };
    }

    /// <summary>The press's Y in track coordinates for a genuine trough press, or null for
    /// presses on the thumb (a normal drag) or outside the track (the step arrows).</summary>
    private (Track Track, double PosY, bool IsBelowThumb)? GetTroughPress(PointerEventArgs e)
    {
        if (_bar.Maximum <= 0)
        {
            return null;
        }

        var track = _bar.GetVisualDescendants().OfType<Track>().FirstOrDefault();
        if (track == null || track.Bounds.Height <= 0)
        {
            return null;
        }

        var y = e.GetPosition(track).Y;
        if (y < 0 || y > track.Bounds.Height)
        {
            return null;
        }

        if (track.Thumb is { } thumb && y >= thumb.Bounds.Top && y <= thumb.Bounds.Bottom)
        {
            return null;
        }

        return (track, y, track.Thumb == null || y > track.Thumb.Bounds.Bottom);
    }

    private void StartTroughHold(PointerPressedEventArgs e)
    {
        if (_troughHold != null || GetTroughPress(e) is not { } press)
        {
            return;
        }

        var timer = new DispatcherTimer { Interval = RepeatDelay };
        var hold = new TroughHold
        {
            Timer = timer,
            Pointer = e.Pointer,
            Track = press.Track,
            PageDown = press.IsBelowThumb,
            PointerPosition = e.GetPosition(_bar),
        };

        timer.Tick += (_, _) =>
        {
            timer.Interval = RepeatInterval;
            TickTroughHold(hold);
        };

        _troughHold = hold;
        e.Pointer.Capture(_bar);
        PageOnce(hold.PageDown);
        timer.Start();

        // Keep the theme's trough repeat button from starting its own (runaway) repeat.
        e.Handled = true;
    }

    private void TickTroughHold(TroughHold hold)
    {
        if (hold.Pointer.Captured != _bar)
        {
            StopTroughHold();
            return;
        }

        var thumb = hold.Track.Thumb;
        var posY = _bar.TranslatePoint(hold.PointerPosition, hold.Track)?.Y;
        if (thumb == null || posY == null)
        {
            return;
        }

        // Pause (not stop) once the thumb has reached the pointer, like the Windows bar.
        var shouldPage = hold.PageDown ? posY.Value > thumb.Bounds.Bottom : posY.Value < thumb.Bounds.Top;
        if (shouldPage)
        {
            PageOnce(hold.PageDown);
        }
    }

    private void PageOnce(bool pageDown)
    {
        var delta = pageDown ? _bar.LargeChange : -_bar.LargeChange;
        var newValue = Math.Clamp(_bar.Value + delta, _bar.Minimum, _bar.Maximum);
        if (Math.Abs(newValue - _bar.Value) > 0.001)
        {
            _bar.Value = newValue;
        }
    }

    private void StopTroughHold()
    {
        if (_troughHold is { } hold)
        {
            hold.Timer.Stop();
            _troughHold = null;
        }
    }

    private void JumpToClickPosition(PointerPressedEventArgs e)
    {
        if (GetTroughPress(e) is not { } press)
        {
            return;
        }

        // Center the thumb on the cursor, scaled by the travel the thumb actually has.
        var thumbLength = press.Track.Thumb?.Bounds.Height ?? 0;
        var travel = Math.Max(1, press.Track.Bounds.Height - thumbLength);
        var fraction = Math.Clamp((press.PosY - thumbLength / 2.0) / travel, 0.0, 1.0);
        _bar.Value = Math.Clamp(fraction * _bar.Maximum, _bar.Minimum, _bar.Maximum);
        e.Handled = true;
    }

    // The headless test dispatcher never fires a DispatcherTimer; the tests step the repeat
    // by hand, like TableViewScrollBarBehavior.TickTroughHoldPagingForTest.
    internal void TickTroughHoldForTest()
    {
        if (_troughHold is { } hold)
        {
            TickTroughHold(hold);
        }
    }

    // The tests drive the bar->view mapping synchronously instead of waiting for the posted
    // dispatcher job.
    internal void ApplyPendingForTest()
    {
        if (_pendingValue is { } pending)
        {
            _pendingValue = null;
            ApplyBarValue(pending);
        }
    }

    internal ScrollBar BarForTest => _bar;
}
