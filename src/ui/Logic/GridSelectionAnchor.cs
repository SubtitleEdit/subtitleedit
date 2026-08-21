using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Where the selection should land after rows are removed from the subtitle grid.
/// <para>
/// Removing rows from a SelectionMode.AlwaysSelected grid makes the control pick a replacement
/// row on its own, and the view scrolls to wherever that lands - which is how "delete all empty
/// lines" threw the user to the end of the list (issue #13822). Deciding the row up front keeps
/// the user where they were working.
/// </para>
/// </summary>
public static class GridSelectionAnchor
{
    /// <summary>
    /// The index - into the list as it is now - of the row to select once the rows flagged in
    /// <paramref name="isRemoved"/> are gone: the selected row if it survives, else the nearest
    /// survivor after it, else the nearest one before it. -1 when every row is removed.
    /// </summary>
    public static int PickSurvivorIndex(IReadOnlyList<bool> isRemoved, int selectedIndex)
    {
        if (isRemoved.Count == 0)
        {
            return -1;
        }

        var start = selectedIndex < 0 ? 0 : selectedIndex;
        if (start >= isRemoved.Count)
        {
            start = isRemoved.Count - 1;
        }

        for (var i = start; i < isRemoved.Count; i++)
        {
            if (!isRemoved[i])
            {
                return i;
            }
        }

        for (var i = start - 1; i >= 0; i--)
        {
            if (!isRemoved[i])
            {
                return i;
            }
        }

        return -1;
    }
}
