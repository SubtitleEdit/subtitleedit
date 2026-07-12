using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Core.Common
{
    public static class ListSortUtil
    {
        /// <summary>
        /// Sorts <paramref name="list"/> with <paramref name="comparison"/> unless it is already
        /// in order. Intended for hot paths that re-sort a list that is almost always already
        /// sorted (e.g. the waveform subtitle buffer rebuilt on every position-timer tick): the
        /// already-sorted check is a single O(n) pass with early exit, versus the O(n log n)
        /// introsort List.Sort always runs. When the list is out of order the extra pass is a
        /// negligible constant on top of the sort.
        /// </summary>
        public static void SortIfNeeded<T>(List<T> list, Comparison<T> comparison)
        {
            if (IsSorted(list, comparison))
            {
                return;
            }

            list.Sort(comparison);
        }

        public static bool IsSorted<T>(List<T> list, Comparison<T> comparison)
        {
            for (var i = 1; i < list.Count; i++)
            {
                if (comparison(list[i - 1], list[i]) > 0)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
