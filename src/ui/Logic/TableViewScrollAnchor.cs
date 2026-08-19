using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using System;
using System.Linq;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Keeps a <see cref="TableView"/> looking at the same row when the virtualizing panel
/// re-estimates its pixel extent (issue #13619).
///
/// TableView virtualizes with Avalonia's VirtualizingStackPanel, which estimates the total
/// extent as "realized rows + remaining count x average realized row height". When a realized
/// row changes height - the subtitle grid's rows are one or two text lines, so breaking a line
/// grows one - the average goes up, the extent estimate grows with it, and the panel keeps the
/// pixel *offset* fixed instead of the row the user is looking at. The same offset then maps to
/// a much earlier row, and the grid scrolls away from the line being edited by roughly
/// index x (1 - extentBefore / extentAfter) rows - measured at 546 -> 512 for a single row
/// gaining a second line 546 rows down, and worse the further down the file you are. The user
/// keeps the selection and the edit box but the row is gone from the view (upstream:
/// AvaloniaUI/Avalonia #17831, the same estimator behind #13579 and PrePositionScroll).
///
/// So: remember the row at the top of the viewport (and how far it is scrolled into), and when
/// a scroll change reports an extent change that no offset change asked for - a re-estimate,
/// not a scroll - put that row back where it was. The restore works in row indices, not pixels,
/// because the pixels are exactly what became unreliable: it uses
/// <see cref="TableViewExtras.PrePositionScroll"/> plus the same measure-jump-remeasure settle
/// loop <see cref="TableViewIndexScrollBar"/> uses for its thumb.
/// </summary>
public sealed class TableViewScrollAnchor
{
    /// <summary>Refinement steps the restore is allowed to take, like PrePositionScroll.</summary>
    private const int SettlePasses = 3;

    private static readonly AttachedProperty<TableViewScrollAnchor?> InstanceProperty =
        AvaloniaProperty.RegisterAttached<TableView, TableViewScrollAnchor?>("ScrollAnchorInstance", typeof(TableViewScrollAnchor));

    private readonly TableView _tableView;
    private ScrollViewer? _scrollViewer;

    // The row at the top of the viewport: the item itself (so an insert above it does not
    // shift the view by a row), its index as a fallback, the item count that index was valid
    // for, and its top edge in viewport coordinates - negative while scrolled into.
    private object? _anchorItem;
    private int _anchorIndex = -1;
    private int _anchorItemCount = -1;
    private double _anchorTop;

    // Restoring the anchor scrolls the view, which reports back as another scroll change.
    private bool _restoring;

    // Someone else owns the offset for the moment (see Suspend).
    private int _suspendCount;

    private TableViewScrollAnchor(TableView tableView)
    {
        _tableView = tableView;

        if (tableView.IsLoaded)
        {
            HookScrollViewer();
        }

        tableView.Loaded += (_, _) => HookScrollViewer();
    }

    /// <summary>
    /// Starts anchoring <paramref name="tableView"/>. Attaching twice returns the first
    /// anchor instead of stacking a second set of handlers.
    /// </summary>
    public static TableViewScrollAnchor Attach(TableView tableView)
    {
        if (tableView.GetValue(InstanceProperty) is { } existing)
        {
            return existing;
        }

        var anchor = new TableViewScrollAnchor(tableView);
        tableView.SetValue(InstanceProperty, anchor);
        return anchor;
    }

    /// <summary>The anchor attached to <paramref name="tableView"/>, if any.</summary>
    public static TableViewScrollAnchor? GetFor(TableView tableView) => tableView.GetValue(InstanceProperty);

    /// <summary>
    /// Hands the offset to the caller until the returned scope is disposed: the anchor keeps
    /// following the view but never moves it. Deliberate multi-pass scrolling - the index
    /// scroll bar placing a row at the viewport top - re-estimates the extent as it realizes
    /// rows, and an anchor restore in the middle of that would fight it.
    /// </summary>
    public IDisposable Suspend()
    {
        _suspendCount++;
        return new SuspendScope(this);
    }

    private sealed class SuspendScope : IDisposable
    {
        private TableViewScrollAnchor? _anchor;

        public SuspendScope(TableViewScrollAnchor anchor) => _anchor = anchor;

        public void Dispose()
        {
            if (_anchor is { } anchor)
            {
                _anchor = null;
                anchor._suspendCount--;
            }
        }
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
            return;
        }

        _scrollViewer.ScrollChanged += OnScrollChanged;
        Remember();
    }

    /// <summary>
    /// The visual whose origin is the viewport's top-left corner. The TableView template keeps
    /// the column header *inside* the ScrollViewer (pinned above the rows), so the ScrollViewer's
    /// own origin sits a header height above the viewport.
    /// </summary>
    private Visual? ViewportOrigin => (Visual?)_scrollViewer?.Presenter ?? _scrollViewer;

    private void OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (_restoring || _scrollViewer == null)
        {
            return;
        }

        // An extent change with no offset change is the panel re-estimating under the view;
        // anything else is a scroll, which is the user's (or ScrollIntoView's) business.
        // At the very top there is nothing to correct - offset 0 always shows row 0.
        if (_suspendCount == 0 &&
            Math.Abs(e.ExtentDelta.Y) > 0.5 &&
            Math.Abs(e.OffsetDelta.Y) < 0.5 &&
            _scrollViewer.Offset.Y > 0.5)
        {
            Restore();
        }

        Remember();
    }

    private void Remember()
    {
        if (_scrollViewer == null || ViewportOrigin is not { } viewportOrigin)
        {
            return;
        }

        var top = double.MaxValue;
        Control? anchorRow = null;
        foreach (var row in _tableView.GetRealizedContainers().OfType<TableViewRow>())
        {
            if (row.Bounds.Height <= 0 ||
                ((Visual)row).TranslatePoint(new Point(0, 0), viewportOrigin)?.Y is not { } rowTop ||
                rowTop + row.Bounds.Height <= 0.5 ||
                rowTop >= top)
            {
                continue;
            }

            top = rowTop;
            anchorRow = row;
        }

        if (anchorRow == null)
        {
            _anchorItem = null;
            _anchorIndex = -1;
            _anchorItemCount = -1;
            return;
        }

        var index = _tableView.IndexFromContainer(anchorRow);
        if (index < 0)
        {
            return;
        }

        _anchorIndex = index;
        _anchorTop = top;
        _anchorItemCount = _tableView.ItemCount;
        _anchorItem = index < _tableView.ItemsView.Count ? _tableView.ItemsView[index] : null;
    }

    /// <summary>
    /// Where the anchored row lives now, or -1 when there is nothing safe to restore to.
    /// The item wins over the index so an insert above the anchor keeps the same content in
    /// view; the index is only trusted when the list is still the same length, so a reload
    /// (every item replaced) leaves the view alone instead of jumping to a stale row.
    /// </summary>
    private int ResolveAnchorIndex()
    {
        var count = _tableView.ItemCount;
        if (count == 0)
        {
            return -1;
        }

        if (_anchorItem != null)
        {
            var index = _tableView.ItemsView.IndexOf(_anchorItem);
            if (index >= 0)
            {
                return index;
            }
        }

        if (_anchorIndex >= 0 && _anchorIndex < count && count == _anchorItemCount)
        {
            return _anchorIndex;
        }

        return -1;
    }

    private void Restore()
    {
        if (_scrollViewer == null)
        {
            return;
        }

        var index = ResolveAnchorIndex();
        if (index < 0)
        {
            return;
        }

        _restoring = true;
        try
        {
            // The re-estimate usually leaves the anchor row outside the realized window - it
            // is what the view jumped away from - so bring it back the cheap way first.
            if (_tableView.ContainerFromIndex(index) is not { Bounds.Height: > 0 })
            {
                TableViewExtras.PrePositionScroll(_tableView, index);
                _tableView.ScrollIntoView(index);
                _tableView.UpdateLayout();
            }

            // Then put its top edge back where it was. Setting the offset makes the panel
            // re-estimate and re-anchor again, which can nudge the rows by a fraction of a
            // row - measure and correct until the row stays put.
            for (var pass = 0; pass < SettlePasses; pass++)
            {
                if (_tableView.ContainerFromIndex(index) is not { Bounds.Height: > 0 } row ||
                    ViewportOrigin is not { } viewportOrigin ||
                    ((Visual)row).TranslatePoint(new Point(0, 0), viewportOrigin)?.Y is not { } rowTop)
                {
                    break;
                }

                var newY = Math.Clamp(
                    _scrollViewer.Offset.Y + (rowTop - _anchorTop),
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
            _restoring = false;
        }
    }
}
