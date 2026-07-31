using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// A <see cref="TableViewColumn"/> with the extras SE's grids need beyond what the
/// control offers: a <see cref="Tag"/> for stable column keys (width persistence),
/// a <see cref="MinWidth"/> hint, and a bindable <see cref="IsVisible"/> property.
/// TableViewColumn has no visibility concept, so a <see cref="TableViewColumnManager"/>
/// watches <see cref="IsVisible"/> and adds/removes the column from the TableView,
/// keeping the original column order.
/// </summary>
public class SeTableViewColumn : TableViewColumn
{
    public static readonly StyledProperty<bool> IsVisibleProperty =
        AvaloniaProperty.Register<SeTableViewColumn, bool>(nameof(IsVisible), defaultValue: true);

    public bool IsVisible
    {
        get => GetValue(IsVisibleProperty);
        set => SetValue(IsVisibleProperty, value);
    }

    public object? Tag { get; set; }

    public double MinWidth { get; set; }
}

/// <summary>
/// Owns the full, ordered column list of a <see cref="TableView"/> and keeps the
/// control's live <see cref="TableView.Columns"/> in sync with each
/// <see cref="SeTableViewColumn.IsVisible"/>: hidden columns are removed from the
/// control (TableView renders every column it holds), visible ones are re-inserted
/// at their original position.
/// </summary>
public sealed class TableViewColumnManager
{
    private readonly TableView _tableView;
    private readonly List<TableViewColumn> _columns = new();

    public TableViewColumnManager(TableView tableView)
    {
        _tableView = tableView;
    }

    /// <summary>All managed columns in display order, including hidden ones.</summary>
    public IReadOnlyList<TableViewColumn> Columns => _columns;

    public void Add(TableViewColumn column)
    {
        _columns.Add(column);
        if (column is SeTableViewColumn seColumn)
        {
            seColumn.PropertyChanged += (_, e) =>
            {
                if (e.Property == SeTableViewColumn.IsVisibleProperty)
                {
                    Sync();
                }
            };
        }

        Sync();
    }

    private void Sync()
    {
        var target = _columns.Where(c => c is not SeTableViewColumn se || se.IsVisible).ToList();
        var live = _tableView.Columns;

        // Remove columns that should no longer be shown. After this, the live list is a
        // subsequence of the target list (both preserve the master order), so the missing
        // ones can simply be inserted at their target positions.
        for (var i = live.Count - 1; i >= 0; i--)
        {
            if (!target.Contains(live[i]))
            {
                live.RemoveAt(i);
            }
        }

        for (var i = 0; i < target.Count; i++)
        {
            if (i >= live.Count || !ReferenceEquals(live[i], target[i]))
            {
                live.Insert(i, target[i]);
            }
        }
    }
}

/// <summary>
/// Reusable behaviors for <see cref="TableView"/>-based grids: reliable scrolling
/// (center / fully-visible), row hit-testing, page size, batched selection updates and
/// per-row style bindings. These are the TableView counterparts of the hand-rolled
/// DataGrid machinery the main subtitle grid accumulated over time; use them for any
/// new TableView so the behavior stays consistent.
/// </summary>
public static class TableViewExtras
{
    /// <summary>
    /// Creates a TableView with SE's standard look and behavior (multi-select,
    /// resizable columns, tight row style).
    /// </summary>
    public static TableView MakeTableView(bool alwaysSelected = true)
    {
        var tableView = new TableView
        {
            SelectionMode = alwaysSelected
                ? SelectionMode.Multiple | SelectionMode.AlwaysSelected
                : SelectionMode.Multiple,
            CanUserResizeColumns = true,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,

            // The DataGrid had no background of its own, so SE's panel background showed
            // through; TableView's theme paints SystemControlBackgroundChromeMediumLowBrush,
            // which in dark mode is within a shade or two of the alternating-row tint
            // (#2D2D2D) and swallowed it completely. Transparent restores the DataGrid
            // backdrop so row tints contrast against the app background again.
            Background = Brushes.Transparent,
        };

        UiUtil.ApplyTableViewRowStyle(tableView);
        return tableView;
    }

    /// <summary>
    /// Adds a style that binds <paramref name="property"/> on every row to
    /// <paramref name="binding"/> (evaluated against the row's DataContext, i.e. the
    /// item). Used e.g. to collapse hidden rows or to expose an accessible name.
    /// </summary>
    public static void BindRowProperty(TableView tableView, AvaloniaProperty property, BindingBase binding)
    {
        tableView.Styles.Add(new Style(x => x.OfType<TableViewRow>())
        {
            Setters = { new Setter(property, binding) },
        });
    }

    /// <summary>
    /// Tints every other row with <paramref name="brush"/>. Applied per container from
    /// the ContainerPrepared/ContainerIndexChanged events, so the tint stays correct
    /// when rows are inserted, removed or recycled. Selection and hover still win:
    /// the row theme's :selected/:pointerover styles set the template Border's
    /// background directly, overriding the row's own Background.
    /// </summary>
    public static void ApplyAlternatingRows(TableView tableView, IBrush brush)
    {
        static void Apply(Control container, int index, IBrush alternatingBrush)
        {
            if (container is not TableViewRow row)
            {
                return;
            }

            if (index % 2 == 1)
            {
                row.Background = alternatingBrush;
            }
            else
            {
                row.ClearValue(TemplatedControl.BackgroundProperty);
            }
        }

        tableView.ContainerPrepared += (_, e) => Apply(e.Container, e.Index, brush);
        tableView.ContainerIndexChanged += (_, e) => Apply(e.Container, e.NewIndex, brush);
    }

    /// <summary>
    /// Moves keyboard focus to the selected row's container when it is realized, falling
    /// back to the TableView itself. Focusing the row (not the list control) is what makes
    /// the current line visible to screen readers via UI Automation (issue #13015).
    /// </summary>
    public static void FocusRow(TableView tableView)
    {
        if (tableView.SelectedItem is { } item &&
            tableView.ContainerFromItem(item) is { } container)
        {
            container.Focus();
            return;
        }

        tableView.Focus();
    }

    /// <summary>
    /// Returns the item index of the row under <paramref name="position"/> (relative to
    /// the TableView), or -1 when the point is not over a row.
    /// </summary>
    public static int GetRowIndexFromPoint(TableView tableView, Point position)
    {
        var current = tableView.InputHitTest(position) as Control;
        while (current != null)
        {
            if (current is TableViewRow row)
            {
                return tableView.IndexFromContainer(row);
            }

            current = current.Parent as Control;
        }

        return -1;
    }

    /// <summary>True when the visual (from a hit test) is inside a column header.</summary>
    public static bool IsInColumnHeader(Visual? visual)
    {
        return visual.FindAncestorOfType<TableViewColumnHeader>(includeSelf: true) != null;
    }

    /// <summary>True when the visual (from a hit test) is inside a scrollbar.</summary>
    public static bool IsInScrollBar(Visual? visual)
    {
        return visual.FindAncestorOfType<ScrollBar>(includeSelf: true) != null;
    }

    /// <summary>
    /// Number of rows that fit in the viewport (used as the PageUp/PageDown step).
    /// Counts realized rows, which works with variable row heights.
    /// </summary>
    public static int GetPageSize(TableView tableView)
    {
        var visibleRowCount = tableView.GetVisualDescendants()
            .OfType<TableViewRow>()
            .Count(r => r.IsVisible && r.Bounds.Height > 0);
        return Math.Max(1, visibleRowCount - 1);
    }

    /// <summary>
    /// Scrolls so <paramref name="item"/>'s row is vertically centered in the viewport.
    /// Posted at Loaded priority so the built-in ScrollIntoView (which realizes the row)
    /// has taken effect first.
    /// </summary>
    public static void CenterRow(TableView tableView, object item)
    {
        AdjustScrollForRow(tableView, item, (rowTop, rowHeight, viewportHeight) =>
            rowTop - (viewportHeight - rowHeight) / 2.0);
    }

    /// <summary>
    /// Nudges the scroll offset so <paramref name="item"/>'s row is fully on screen -
    /// the non-centering counterpart of <see cref="CenterRow"/> for variable-height rows
    /// that ScrollIntoView leaves clipped at an edge.
    /// </summary>
    public static void EnsureRowFullyVisible(TableView tableView, object item)
    {
        AdjustScrollForRow(tableView, item, (rowTop, rowHeight, viewportHeight) =>
        {
            var rowBottom = rowTop + rowHeight;
            if (rowBottom > viewportHeight)
            {
                return rowBottom - viewportHeight; // pokes out the bottom -> scroll down
            }

            if (rowTop < 0)
            {
                return rowTop; // pokes out the top -> scroll up (negative)
            }

            return 0;
        });
    }

    private static void AdjustScrollForRow(TableView tableView, object item, Func<double, double, double, double> computeDelta)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (tableView.ContainerFromItem(item) is not { } row || row.Bounds.Height <= 0)
            {
                return;
            }

            var scrollViewer = tableView.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
            if (scrollViewer == null || scrollViewer.Viewport.Height <= 0)
            {
                return;
            }

            // Row top in viewport coordinates.
            var rowTop = row.TranslatePoint(new Point(0, 0), scrollViewer)?.Y;
            if (rowTop == null)
            {
                return;
            }

            var delta = computeDelta(rowTop.Value, row.Bounds.Height, scrollViewer.Viewport.Height);
            if (Math.Abs(delta) < 1)
            {
                return;
            }

            var offset = scrollViewer.Offset;
            var newY = Math.Max(0, Math.Min(offset.Y + delta, scrollViewer.Extent.Height - scrollViewer.Viewport.Height));
            if (Math.Abs(newY - offset.Y) < 0.5)
            {
                return;
            }

            scrollViewer.Offset = new Vector(offset.X, newY);
        }, DispatcherPriority.Loaded);
    }
}

/// <summary>
/// Drag-to-select for a TableView: press on a row and drag to select the range, with
/// accelerating auto-scroll when the pointer nears the top/bottom edge. The host owns
/// how a range becomes a selection (it may batch, keep its own anchor bookkeeping and
/// raise its own changed notifications) via <paramref name="applyRange"/>; this class
/// owns the pointer/timer state machine.
/// </summary>
public sealed class TableViewDragSelect
{
    private const double AutoScrollEdgeSize = 28;
    private const double AutoScrollAccelerationPixels = 18;
    private const int AutoScrollMaxStep = 16;

    private readonly TableView _tableView;

    /// <summary>(anchorIndex, currentIndex) - replace the selection with that range.</summary>
    private readonly Action<int, int> _applyRange;

    private int _startIndex = -1;
    private int _lastIndex = -1;
    private int _autoScrollDirection;
    private int _autoScrollStep = 1;
    private DispatcherTimer? _autoScrollTimer;

    public TableViewDragSelect(TableView tableView, Action<int, int> applyRange)
    {
        _tableView = tableView;
        _applyRange = applyRange;
    }

    /// <summary>True once the pointer has moved to another row during the current press.</summary>
    public bool HasMoved { get; private set; }

    /// <summary>Arms a potential drag-select from a plain left press on the given row.</summary>
    public void Arm(int rowIndex)
    {
        _startIndex = rowIndex;
        _lastIndex = rowIndex;
    }

    /// <summary>Resets all state (e.g. on pointer press, before re-arming).</summary>
    public void Reset()
    {
        StopAutoScroll();
        _startIndex = -1;
        _lastIndex = -1;
        HasMoved = false;
    }

    public void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_startIndex < 0)
        {
            return;
        }

        if (!e.GetCurrentPoint(_tableView).Properties.IsLeftButtonPressed)
        {
            End(e);
            return;
        }

        var position = e.GetPosition(_tableView);
        UpdateAutoScroll(position);

        var currentIndex = TableViewExtras.GetRowIndexFromPoint(_tableView, position);
        if (currentIndex < 0)
        {
            return;
        }

        var wasDragging = HasMoved;
        DragTo(currentIndex);
        if (HasMoved)
        {
            if (!wasDragging && sender is Control control)
            {
                e.Pointer.Capture(control);
            }

            e.Handled = true;
        }
    }

    public void End(PointerEventArgs e)
    {
        StopAutoScroll();
        if (_startIndex >= 0)
        {
            e.Pointer.Capture(null);
        }

        _startIndex = -1;
        _lastIndex = -1;
    }

    private void DragTo(int currentIndex)
    {
        var itemCount = _tableView.ItemCount;
        if (_startIndex < 0 || currentIndex < 0 || currentIndex >= itemCount)
        {
            return;
        }

        _lastIndex = currentIndex;

        if (currentIndex == _startIndex && !HasMoved)
        {
            return;
        }

        HasMoved = true;
        _applyRange(_startIndex, currentIndex);
    }

    private void UpdateAutoScroll(Point position)
    {
        if (_tableView.Bounds.Height <= 0)
        {
            StopAutoScroll();
            return;
        }

        if (position.Y < AutoScrollEdgeSize)
        {
            StartAutoScroll(-1, AutoScrollEdgeSize - position.Y);
        }
        else if (position.Y > _tableView.Bounds.Height - AutoScrollEdgeSize)
        {
            StartAutoScroll(1, position.Y - (_tableView.Bounds.Height - AutoScrollEdgeSize));
        }
        else
        {
            StopAutoScroll();
        }
    }

    private void StartAutoScroll(int direction, double distanceFromEdge)
    {
        if (_lastIndex < 0 || _tableView.ItemCount == 0)
        {
            return;
        }

        _autoScrollDirection = direction;
        var step = 1 + (int)Math.Floor(Math.Max(0, distanceFromEdge) / AutoScrollAccelerationPixels);
        _autoScrollStep = Math.Clamp(step, 1, AutoScrollMaxStep);

        if (_autoScrollTimer != null)
        {
            if (!_autoScrollTimer.IsEnabled)
            {
                _autoScrollTimer.Start();
            }

            return;
        }

        _autoScrollTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(80),
        };
        _autoScrollTimer.Tick += (_, _) => AutoScrollTick();
        _autoScrollTimer.Start();
    }

    private void StopAutoScroll()
    {
        _autoScrollDirection = 0;
        _autoScrollStep = 1;
        _autoScrollTimer?.Stop();
    }

    private void AutoScrollTick()
    {
        if (_startIndex < 0 || _autoScrollDirection == 0)
        {
            StopAutoScroll();
            return;
        }

        var itemCount = _tableView.ItemCount;
        if (itemCount == 0)
        {
            StopAutoScroll();
            return;
        }

        var nextIndex = Math.Clamp(_lastIndex + _autoScrollDirection * _autoScrollStep, 0, itemCount - 1);
        if (nextIndex == _lastIndex)
        {
            StopAutoScroll();
            return;
        }

        DragTo(nextIndex);
        _tableView.ScrollIntoView(nextIndex);
    }
}
