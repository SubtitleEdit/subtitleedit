using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using System;
using System.Linq;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Keeps two side-by-side <see cref="TableView"/>s looking at the same row: scrolling either
/// one puts the row at the other one's viewport top too (issue #13504).
///
/// SE4's Compare did this with a timer that copied the active list view's TopItem index to the
/// other one (Compare.SelectLinesInBothListViews). SE5's two grids scroll independently, so
/// with larger files the sides drift a page or more apart and the comparison is unreadable.
///
/// The sync works in row *indices*, not pixels, and that is not a detail: the two grids hold
/// different text, a row wraps to one or two lines depending on its content, and TableView
/// virtualizes with a VirtualizingStackPanel whose extent is estimated from the average
/// realized row height (see <see cref="TableViewScrollAnchor"/> and
/// <see cref="TableViewExtras.PrePositionScroll"/>, upstream AvaloniaUI/Avalonia #17831). Two
/// grids with different content therefore produce different - and drifting - estimates, so
/// copying <c>Offset.Y</c> across would neither start nor stay aligned. Instead the row at the
/// source's viewport top is measured, and the same index is placed at the same top edge on the
/// other side with the pre-position + settle loop the other TableView helpers use.
///
/// Rows below the top can still diverge when one side wraps to two lines and its twin does not;
/// aligning those as well would mean giving matched rows a shared height, which is a change to
/// the compare rows themselves rather than to scrolling.
/// </summary>
public sealed class TableViewScrollSync
{
    /// <summary>Refinement steps the alignment is allowed to take, like PrePositionScroll.</summary>
    private const int SettlePasses = 3;

    private readonly TableView _left;
    private readonly TableView _right;
    private ScrollViewer? _leftScrollViewer;
    private ScrollViewer? _rightScrollViewer;

    // Aligning one side scrolls it, which reports back as another scroll change; without this
    // the two sides would push each other back and forth (and fight at the bottom, where the
    // shorter extent cannot follow the taller one).
    private bool _syncing;

    public TableViewScrollSync(TableView left, TableView right)
    {
        _left = left;
        _right = right;

        Hook(left, isLeft: true);
        Hook(right, isLeft: false);
    }

    /// <summary>
    /// Puts <paramref name="source"/>'s top row at the top of the other grid. Called on every
    /// scroll, and by callers that moved the selection instead of the offset - a mirrored
    /// selection only guarantees the row is somewhere in view, which is not the same as aligned.
    /// </summary>
    public void SyncFrom(TableView source)
    {
        if (_syncing)
        {
            return;
        }

        ScrollViewer? sourceScrollViewer;
        TableView target;
        ScrollViewer? targetScrollViewer;
        if (source == _left)
        {
            sourceScrollViewer = _leftScrollViewer;
            target = _right;
            targetScrollViewer = _rightScrollViewer;
        }
        else if (source == _right)
        {
            sourceScrollViewer = _rightScrollViewer;
            target = _left;
            targetScrollViewer = _leftScrollViewer;
        }
        else
        {
            return;
        }

        if (sourceScrollViewer == null || targetScrollViewer == null ||
            targetScrollViewer.Viewport.Height <= 0 || target.ItemCount == 0)
        {
            return;
        }

        // Set before the first layout pass, not just around the alignment: laying the source out
        // reports its own scroll change back here, and answering that would re-enter this method
        // for the same scroll until the stack runs out.
        _syncing = true;

        // Placing a row at the viewport top realizes rows as it goes, so the extent estimate
        // moves under the settle loop - an anchor restore in the middle of that would drag the
        // view back to where it just left (#13619).
        using var anchorSuspended = TableViewScrollAnchor.GetFor(target)?.Suspend();
        try
        {
            // The caller may have just scrolled the source with ScrollIntoView; measure what is
            // on screen now, not what was there before that layout pass.
            source.UpdateLayout();
            if (GetTopRow(source, sourceScrollViewer) is not { } topRow)
            {
                return;
            }

            Align(target, targetScrollViewer, topRow.Index, topRow.Top);
        }
        finally
        {
            _syncing = false;
        }
    }

    private void Hook(TableView tableView, bool isLeft)
    {
        if (tableView.IsLoaded)
        {
            HookScrollViewer(tableView, isLeft);
        }

        tableView.Loaded += (_, _) => HookScrollViewer(tableView, isLeft);
    }

    private void HookScrollViewer(TableView tableView, bool isLeft)
    {
        if ((isLeft ? _leftScrollViewer : _rightScrollViewer) != null)
        {
            return;
        }

        var scrollViewer = tableView.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
        if (scrollViewer == null)
        {
            return;
        }

        if (isLeft)
        {
            _leftScrollViewer = scrollViewer;
        }
        else
        {
            _rightScrollViewer = scrollViewer;
        }

        scrollViewer.ScrollChanged += (_, e) =>
        {
            // An extent change with no offset change is the panel re-estimating under the view,
            // not a scroll - the other side has no reason to move for it.
            if (_syncing || Math.Abs(e.OffsetDelta.Y) < 0.5)
            {
                return;
            }

            SyncFrom(tableView);
        };
    }

    /// <summary>
    /// The visual whose origin is the viewport's top-left corner. The TableView template keeps
    /// the column header <i>inside</i> the ScrollViewer (pinned above the rows), so the
    /// ScrollViewer's own origin sits a header height above the viewport. Same helper as
    /// <see cref="TableViewScrollAnchor"/> and TableViewExtras.
    /// </summary>
    private static Visual ViewportOrigin(ScrollViewer scrollViewer) => (Visual?)scrollViewer.Presenter ?? scrollViewer;

    /// <summary>
    /// The topmost row still showing in the viewport, with its top edge in viewport
    /// coordinates - negative while the row is scrolled into.
    /// </summary>
    private static (int Index, double Top)? GetTopRow(TableView tableView, ScrollViewer scrollViewer)
    {
        var top = double.MaxValue;
        Control? topRow = null;
        foreach (var row in tableView.GetRealizedContainers().OfType<TableViewRow>())
        {
            if (row.Bounds.Height <= 0 ||
                ((Visual)row).TranslatePoint(new Point(0, 0), ViewportOrigin(scrollViewer))?.Y is not { } rowTop ||
                rowTop + row.Bounds.Height <= 0.5 ||
                rowTop >= top)
            {
                continue;
            }

            top = rowTop;
            topRow = row;
        }

        if (topRow == null)
        {
            return null;
        }

        var index = tableView.IndexFromContainer(topRow);
        return index < 0 ? null : (index, top);
    }

    /// <summary>
    /// Puts row <paramref name="index"/> of <paramref name="tableView"/> at <paramref name="top"/>
    /// in viewport coordinates.
    /// </summary>
    private static void Align(TableView tableView, ScrollViewer scrollViewer, int index, double top)
    {
        index = Math.Clamp(index, 0, tableView.ItemCount - 1);

        // The target row is usually outside the realized window - that is the whole point of
        // the sync - so bring it back the cheap way first instead of letting the panel walk.
        if (tableView.ContainerFromIndex(index) is not { Bounds.Height: > 0 })
        {
            TableViewExtras.PrePositionScroll(tableView, index);
            tableView.ScrollIntoView(index);
            tableView.UpdateLayout();
        }

        // Then put its top edge where the other side's is. Setting the offset makes the panel
        // re-estimate and re-anchor, which can nudge the rows by a fraction of a row - measure
        // and correct until the row stays put.
        for (var pass = 0; pass < SettlePasses; pass++)
        {
            if (tableView.ContainerFromIndex(index) is not { Bounds.Height: > 0 } row ||
                ((Visual)row).TranslatePoint(new Point(0, 0), ViewportOrigin(scrollViewer))?.Y is not { } rowTop)
            {
                return;
            }

            var newY = Math.Clamp(
                scrollViewer.Offset.Y + (rowTop - top),
                0, Math.Max(0, scrollViewer.Extent.Height - scrollViewer.Viewport.Height));
            if (Math.Abs(newY - scrollViewer.Offset.Y) < 0.5)
            {
                return;
            }

            scrollViewer.Offset = new Vector(scrollViewer.Offset.X, newY);
            tableView.UpdateLayout();
        }
    }
}
