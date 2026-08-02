using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nikse.SubtitleEdit.Logic;

public enum ListMoveDirection
{
    Up,
    Down,
    Top,
    Bottom,
}

/// <summary>
/// Multi-selection aware "move up/down/to top/to bottom" over an
/// <see cref="ObservableCollection{T}"/>, for lists where the order is data rather than
/// presentation (ASSA/SSA styles are written to the file header in list order).
///
/// A selection keeps its internal order and its gaps: moving {0, 2, 3} up gives
/// {0, 1, 2} - row 0 is already at the top and pins the ones below it - so repeated
/// moves collapse a scattered selection against the edge instead of scrambling it.
/// </summary>
public static class ListReorder
{
    /// <summary>
    /// Moves the items at <paramref name="selectedIndices"/> one step in
    /// <paramref name="direction"/>, or all the way to the top/bottom. Indices outside
    /// the collection are ignored; the order they are given in does not matter.
    /// </summary>
    public static void Move<T>(ObservableCollection<T> items, IEnumerable<int> selectedIndices, ListMoveDirection direction)
    {
        var indices = selectedIndices
            .Where(i => i >= 0 && i < items.Count)
            .Distinct()
            .OrderBy(i => i)
            .ToList();

        if (indices.Count == 0 || items.Count < 2)
        {
            return;
        }

        switch (direction)
        {
            case ListMoveDirection.Up:
                MoveUp(items, indices);
                break;
            case ListMoveDirection.Down:
                MoveDown(items, indices);
                break;
            case ListMoveDirection.Top:
                MoveToTop(items, indices);
                break;
            case ListMoveDirection.Bottom:
                MoveToBottom(items, indices);
                break;
        }
    }

    private static void MoveUp<T>(ObservableCollection<T> items, List<int> indices)
    {
        // Rows already stacked at the very top cannot move and pin the boundary the rest
        // move against. Walking top-down means a swap only ever touches the row above the
        // one being moved, so the indices still to come stay valid.
        var boundary = 0;
        foreach (var index in indices)
        {
            if (index == boundary)
            {
                boundary++;
                continue;
            }

            items.Move(index, index - 1);
        }
    }

    private static void MoveDown<T>(ObservableCollection<T> items, List<int> indices)
    {
        var boundary = items.Count - 1;
        for (var i = indices.Count - 1; i >= 0; i--)
        {
            var index = indices[i];
            if (index == boundary)
            {
                boundary--;
                continue;
            }

            items.Move(index, index + 1);
        }
    }

    private static void MoveToTop<T>(ObservableCollection<T> items, List<int> indices)
    {
        // Snapshot the items before moving anything - the indices go stale on the first Move.
        // Filling the target slots front-to-back keeps every not-yet-moved item at or after
        // its target, so each Move only shifts rows that are still to the right of it.
        var selected = indices.Select(i => items[i]).ToList();
        for (var i = 0; i < selected.Count; i++)
        {
            items.Move(items.IndexOf(selected[i]), i);
        }
    }

    private static void MoveToBottom<T>(ObservableCollection<T> items, List<int> indices)
    {
        // Mirror of MoveToTop: fill the target slots back-to-front. Going front-to-back here
        // would drag each already-placed row one slot back towards the middle as the next one
        // is removed from in front of it, reversing the selection's order.
        var selected = indices.Select(i => items[i]).ToList();
        for (var i = selected.Count - 1; i >= 0; i--)
        {
            items.Move(items.IndexOf(selected[i]), items.Count - (selected.Count - i));
        }
    }
}
