using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Core.Common
{
    /// <summary>
    /// Splits a single-line text at its spaces into a fixed number of pieces, as evenly as
    /// possible. Filling piece by piece leaves whatever is left over to the last one; this picks
    /// all the breaks together by dynamic programming over the spaces.
    /// </summary>
    internal static class TextPartition
    {
        /// <summary>
        /// Returns the indices of the spaces to break at (ascending, <paramref name="parts"/> - 1
        /// of them), or null if the text cannot be split that way.
        /// </summary>
        /// <param name="text">Text without line breaks.</param>
        /// <param name="parts">Number of pieces wanted.</param>
        /// <param name="maxLength">Longest piece allowed, a single word excepted (it cannot be broken). Zero or less = no limit.</param>
        /// <param name="breakCost">Cost of breaking at the space with this index, added to the squared length deviations. Null = no cost.</param>
        internal static int[] Split(string text, int parts, int maxLength, Func<int, double> breakCost)
        {
            if (string.IsNullOrEmpty(text) || parts < 2)
            {
                return null;
            }

            // boundaries: -1 (before the text), every space between two words, text.Length
            var boundaries = new List<int> { -1 };
            for (var i = 1; i < text.Length - 1; i++)
            {
                if (text[i] == ' ' && text[i - 1] != ' ')
                {
                    boundaries.Add(i);
                }
            }

            boundaries.Add(text.Length);
            var last = boundaries.Count - 1;
            if (last < parts)
            {
                return null; // fewer words than parts
            }

            var costs = new double[last + 1];
            for (var i = 1; i < last; i++)
            {
                costs[i] = breakCost?.Invoke(boundaries[i]) ?? 0;
            }

            var target = (text.Length - (parts - 1)) / (double)parts;

            // best[k, end] = lowest cost of splitting the text before boundary "end" into k pieces,
            // from[k, end] = the boundary where the last of those pieces starts
            var best = new double[parts + 1, last + 1];
            var from = new int[parts + 1, last + 1];
            for (var k = 0; k <= parts; k++)
            {
                for (var end = 0; end <= last; end++)
                {
                    best[k, end] = double.PositiveInfinity;
                }
            }

            best[0, 0] = 0;
            for (var k = 1; k <= parts; k++)
            {
                // leave at least one word for each of the remaining pieces
                for (var end = k; end <= last - (parts - k); end++)
                {
                    for (var start = end - 1; start >= k - 1; start--)
                    {
                        var length = boundaries[end] - boundaries[start] - 1;
                        if (maxLength > 0 && length > maxLength && start < end - 1)
                        {
                            break; // only gets longer
                        }

                        var previous = best[k - 1, start];
                        if (double.IsPositiveInfinity(previous))
                        {
                            continue;
                        }

                        var deviation = length - target;
                        var cost = previous + deviation * deviation + costs[end];
                        if (cost < best[k, end])
                        {
                            best[k, end] = cost;
                            from[k, end] = start;
                        }
                    }
                }
            }

            if (double.IsPositiveInfinity(best[parts, last]))
            {
                return null;
            }

            var result = new int[parts - 1];
            var boundary = last;
            for (var k = parts; k > 1; k--)
            {
                boundary = from[k, boundary];
                result[k - 2] = boundaries[boundary];
            }

            return result;
        }
    }
}
