using Nikse.SubtitleEdit.Features.Main;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic.UndoRedo;

/// <summary>
/// Finds the row an undo/redo step changed, for "Undo/redo: go to changed line" (#15029). The
/// row index stored in the snapshot is no help here: it is whatever row was current when change
/// detection fired, not the row that was edited (#11308) - so the rows themselves are compared.
/// </summary>
public static class UndoRedoChangedRowFinder
{
    /// <summary>
    /// Index of the first row that differs between the two states, or -1 when the rows are the
    /// same. With lines inserted or removed at the end that is the first index past the shorter
    /// list, so the caller must clamp it to the current line count.
    /// </summary>
    public static int FindFirstChangedRowIndex(IReadOnlyList<SubtitleLineViewModel> before, IReadOnlyList<SubtitleLineViewModel> after)
    {
        var min = Math.Min(before.Count, after.Count);
        for (var i = 0; i < min; i++)
        {
            if (!IsSameContent(before[i], after[i]))
            {
                return i;
            }
        }

        return before.Count == after.Count ? -1 : min;
    }

    private static bool IsSameContent(SubtitleLineViewModel a, SubtitleLineViewModel b)
    {
        return a.StartTime == b.StartTime &&
               a.EndTime == b.EndTime &&
               string.Equals(a.Text ?? string.Empty, b.Text ?? string.Empty, StringComparison.Ordinal) &&
               string.Equals(a.OriginalText ?? string.Empty, b.OriginalText ?? string.Empty, StringComparison.Ordinal) &&
               string.Equals(a.Actor ?? string.Empty, b.Actor ?? string.Empty, StringComparison.Ordinal) &&
               string.Equals(a.Style ?? string.Empty, b.Style ?? string.Empty, StringComparison.Ordinal);
    }
}
