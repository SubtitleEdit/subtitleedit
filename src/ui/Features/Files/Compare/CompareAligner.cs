using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Files.Compare;

/// <summary>
/// Lines up the two subtitles of Compare: which current line pairs with which reference line,
/// and where one side gets a blank row because the line only exists in the other (#15394).
/// <para>
/// The user's sync points split the two files into segments that are aligned independently, so
/// a forced pair is always a row of its own. Inside a segment, lines whose text is unique on both
/// sides and appears in the same order anchor the alignment (patience diff); the stretches between
/// anchors are aligned by a scored dynamic program that prefers pairing similar lines - by text,
/// by fuzzy text, or by timing - over leaving a line unpaired. Two unrelated lines still pair up
/// rather than becoming two blank rows, so files that differ throughout read like before.
/// </para>
/// </summary>
internal static class CompareAligner
{
    /// <summary>One output row; -1 on a side means a blank filler there.</summary>
    internal readonly record struct Pair(int Left, int Right);

    internal sealed class Line
    {
        public Line(string normalizedText, TimeSpan start, TimeSpan end)
        {
            NormalizedText = normalizedText;
            Start = start;
            End = end;
        }

        public string NormalizedText { get; }
        public TimeSpan Start { get; }
        public TimeSpan End { get; }
        internal int[]? Bigrams { get; set; }
    }

    // Leaving a line unpaired costs this; pairing costs PairWeight * similarity - PairOffset. Two
    // unrelated lines (similarity 0) score -1.5, better than two blanks at -2, so they pair up,
    // while a real match next door (similarity ~1, +1.5) easily pays for a blank row to reach it.
    private const double GapScore = -1.0;
    private const double PairWeight = 3.0;
    private const double PairOffset = 1.5;

    // The dynamic program is O(n*m) in time and memory; beyond this a stretch is paired by position.
    private const long MaxCells = 1_000_000;

    public static List<Pair> Align(
        IReadOnlyList<Line> left,
        IReadOnlyList<Line> right,
        Func<TimeSpan, TimeSpan, bool> isTimeEqual,
        IEnumerable<(int Left, int Right)>? syncPoints = null)
    {
        var aligner = new Context(left, right, isTimeEqual);
        var result = new List<Pair>(Math.Max(left.Count, right.Count));

        var fixedPairs = GetOrderedSyncPoints(syncPoints, left.Count, right.Count);
        var l = 0;
        var r = 0;
        foreach (var (sl, sr) in fixedPairs)
        {
            aligner.AlignSegment(l, sl, r, sr, result);
            result.Add(new Pair(sl, sr));
            l = sl + 1;
            r = sr + 1;
        }

        aligner.AlignSegment(l, left.Count, r, right.Count, result);
        return result;
    }

    /// <summary>The sync points in file order; any that would cross an earlier-sorted one are left out.</summary>
    internal static List<(int Left, int Right)> GetOrderedSyncPoints(IEnumerable<(int Left, int Right)>? syncPoints, int leftCount, int rightCount)
    {
        var result = new List<(int Left, int Right)>();
        if (syncPoints == null)
        {
            return result;
        }

        var sorted = new List<(int Left, int Right)>();
        foreach (var p in syncPoints)
        {
            if (p.Left >= 0 && p.Left < leftCount && p.Right >= 0 && p.Right < rightCount)
            {
                sorted.Add(p);
            }
        }

        sorted.Sort((a, b) => a.Left != b.Left ? a.Left.CompareTo(b.Left) : a.Right.CompareTo(b.Right));
        foreach (var p in sorted)
        {
            if (result.Count == 0 || (p.Left > result[^1].Left && p.Right > result[^1].Right))
            {
                result.Add(p);
            }
        }

        return result;
    }

    private sealed class Context
    {
        private readonly IReadOnlyList<Line> _left;
        private readonly IReadOnlyList<Line> _right;
        private readonly Func<TimeSpan, TimeSpan, bool> _isTimeEqual;

        public Context(IReadOnlyList<Line> left, IReadOnlyList<Line> right, Func<TimeSpan, TimeSpan, bool> isTimeEqual)
        {
            _left = left;
            _right = right;
            _isTimeEqual = isTimeEqual;
        }

        /// <summary>Aligns left[l0..l1) with right[r0..r1), appending the rows to <paramref name="result"/>.</summary>
        public void AlignSegment(int l0, int l1, int r0, int r1, List<Pair> result)
        {
            var l = l0;
            var r = r0;
            foreach (var (al, ar) in FindUniqueTextAnchors(l0, l1, r0, r1))
            {
                AlignStretch(l, al, r, ar, result);
                result.Add(new Pair(al, ar));
                l = al + 1;
                r = ar + 1;
            }

            AlignStretch(l, l1, r, r1, result);
        }

        /// <summary>
        /// Lines whose text occurs exactly once on each side of the segment, kept only where they
        /// appear in the same order on both (the longest increasing run) - the patience diff anchors.
        /// </summary>
        private List<(int Left, int Right)> FindUniqueTextAnchors(int l0, int l1, int r0, int r1)
        {
            var leftCounts = CountTexts(_left, l0, l1);
            var rightCounts = CountTexts(_right, r0, r1);

            var candidates = new List<(int Left, int Right)>();
            for (var i = l0; i < l1; i++)
            {
                var text = _left[i].NormalizedText;
                if (text.Length > 0 &&
                    leftCounts.TryGetValue(text, out var lc) && lc.Count == 1 &&
                    rightCounts.TryGetValue(text, out var rc) && rc.Count == 1)
                {
                    candidates.Add((i, rc.Index));
                }
            }

            return LongestIncreasingByRight(candidates);
        }

        private static Dictionary<string, (int Count, int Index)> CountTexts(IReadOnlyList<Line> lines, int from, int to)
        {
            var counts = new Dictionary<string, (int Count, int Index)>(StringComparer.Ordinal);
            for (var i = from; i < to; i++)
            {
                var text = lines[i].NormalizedText;
                counts[text] = counts.TryGetValue(text, out var c) ? (c.Count + 1, c.Index) : (1, i);
            }

            return counts;
        }

        /// <summary>Candidates come sorted by left index; keeps the longest subsequence increasing in right index too.</summary>
        private static List<(int Left, int Right)> LongestIncreasingByRight(List<(int Left, int Right)> candidates)
        {
            var result = new List<(int Left, int Right)>();
            if (candidates.Count == 0)
            {
                return result;
            }

            var tailIndexes = new List<int>(); // index into candidates of the smallest tail for each length
            var previous = new int[candidates.Count];
            for (var i = 0; i < candidates.Count; i++)
            {
                var value = candidates[i].Right;
                int lo = 0, hi = tailIndexes.Count;
                while (lo < hi)
                {
                    var mid = (lo + hi) / 2;
                    if (candidates[tailIndexes[mid]].Right < value)
                    {
                        lo = mid + 1;
                    }
                    else
                    {
                        hi = mid;
                    }
                }

                previous[i] = lo > 0 ? tailIndexes[lo - 1] : -1;
                if (lo == tailIndexes.Count)
                {
                    tailIndexes.Add(i);
                }
                else
                {
                    tailIndexes[lo] = i;
                }
            }

            for (var i = tailIndexes[^1]; i >= 0; i = previous[i])
            {
                result.Add(candidates[i]);
            }

            result.Reverse();
            return result;
        }

        /// <summary>The scored alignment of a stretch with no anchors inside.</summary>
        private void AlignStretch(int l0, int l1, int r0, int r1, List<Pair> result)
        {
            var n = l1 - l0;
            var m = r1 - r0;
            if (n == 0 || m == 0 || (long)(n + 1) * (m + 1) > MaxCells)
            {
                AlignByPosition(l0, l1, r0, r1, result);
                return;
            }

            // score[i, j] = best score of left[l0..l0+i) against right[r0..r0+j).
            var width = m + 1;
            var score = new double[(n + 1) * width];
            var move = new byte[(n + 1) * width]; // 0 = pair, 1 = left only, 2 = right only
            for (var i = 1; i <= n; i++)
            {
                score[i * width] = i * GapScore;
                move[i * width] = 1;
            }

            for (var j = 1; j <= m; j++)
            {
                score[j] = j * GapScore;
                move[j] = 2;
            }

            for (var i = 1; i <= n; i++)
            {
                var leftLine = _left[l0 + i - 1];
                for (var j = 1; j <= m; j++)
                {
                    var pair = score[(i - 1) * width + j - 1] + PairWeight * Similarity(leftLine, _right[r0 + j - 1]) - PairOffset;
                    var leftOnly = score[(i - 1) * width + j] + GapScore;
                    var rightOnly = score[i * width + j - 1] + GapScore;

                    // Ties go to the pair, so equal-length stretches keep lining up by position.
                    var best = pair;
                    byte bestMove = 0;
                    if (leftOnly > best)
                    {
                        best = leftOnly;
                        bestMove = 1;
                    }

                    if (rightOnly > best)
                    {
                        best = rightOnly;
                        bestMove = 2;
                    }

                    score[i * width + j] = best;
                    move[i * width + j] = bestMove;
                }
            }

            var rows = new List<Pair>(n + m);
            int a = n, b = m;
            while (a > 0 || b > 0)
            {
                switch (move[a * width + b])
                {
                    case 0:
                        rows.Add(new Pair(l0 + a - 1, r0 + b - 1));
                        a--;
                        b--;
                        break;
                    case 1:
                        rows.Add(new Pair(l0 + a - 1, -1));
                        a--;
                        break;
                    default:
                        rows.Add(new Pair(-1, r0 + b - 1));
                        b--;
                        break;
                }
            }

            rows.Reverse();
            result.AddRange(rows);
        }

        private static void AlignByPosition(int l0, int l1, int r0, int r1, List<Pair> result)
        {
            var n = l1 - l0;
            var m = r1 - r0;
            for (var k = 0; k < Math.Max(n, m); k++)
            {
                result.Add(new Pair(k < n ? l0 + k : -1, k < m ? r0 + k : -1));
            }
        }

        /// <summary>
        /// 0..1.1: the better of the text and the timing likeness, plus a little of the other, so
        /// a repeated "Yes." still lines up with the one at the same time.
        /// </summary>
        private double Similarity(Line a, Line b)
        {
            var text = TextSimilarity(a, b);
            var time = TimeSimilarity(a, b);
            return Math.Max(text, time) + 0.1 * Math.Min(text, time);
        }

        private static double TextSimilarity(Line a, Line b)
        {
            if (a.NormalizedText == b.NormalizedText)
            {
                return 1;
            }

            if (a.NormalizedText.Length == 0 || b.NormalizedText.Length == 0)
            {
                return 0;
            }

            // A fixed typo or a reworded line still reads as the same line: Dice over letter pairs,
            // squared so only close matches count for much.
            var dice = Dice(GetBigrams(a), GetBigrams(b));
            return dice * dice;
        }

        private double TimeSimilarity(Line a, Line b)
        {
            var startEqual = _isTimeEqual(a.Start, b.Start);
            var endEqual = _isTimeEqual(a.End, b.End);
            if (startEqual && endEqual)
            {
                return 1;
            }

            var overlap = (Math.Min(a.End.Ticks, b.End.Ticks) - Math.Max(a.Start.Ticks, b.Start.Ticks));
            var union = (Math.Max(a.End.Ticks, b.End.Ticks) - Math.Min(a.Start.Ticks, b.Start.Ticks));
            var iou = overlap > 0 && union > 0 ? (double)overlap / union : 0;
            return startEqual || endEqual ? Math.Max(0.5, iou) : iou;
        }

        private static int[] GetBigrams(Line line)
        {
            if (line.Bigrams != null)
            {
                return line.Bigrams;
            }

            var text = line.NormalizedText.ToLowerInvariant();
            var bigrams = new int[Math.Max(0, text.Length - 1)];
            for (var i = 0; i < bigrams.Length; i++)
            {
                bigrams[i] = (text[i] << 16) | text[i + 1];
            }

            if (bigrams.Length == 0 && text.Length == 1)
            {
                bigrams = [text[0]];
            }

            Array.Sort(bigrams);
            line.Bigrams = bigrams;
            return bigrams;
        }

        /// <summary>Dice coefficient of two sorted multisets.</summary>
        private static double Dice(int[] a, int[] b)
        {
            if (a.Length + b.Length == 0)
            {
                return 0;
            }

            int i = 0, j = 0, common = 0;
            while (i < a.Length && j < b.Length)
            {
                if (a[i] == b[j])
                {
                    common++;
                    i++;
                    j++;
                }
                else if (a[i] < b[j])
                {
                    i++;
                }
                else
                {
                    j++;
                }
            }

            return 2.0 * common / (a.Length + b.Length);
        }
    }
}
