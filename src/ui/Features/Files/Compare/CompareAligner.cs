using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Files.Compare;

/// <summary>
/// Lines up the two subtitles of Compare: which current line pairs with which reference line,
/// and where one side gets a blank row because the line only exists in the other (#15394).
/// <para>
/// The user's sync points split the two files into segments that are aligned independently, so
/// a forced pair is always a row of its own. Inside a segment, lines whose text is unique on both
/// sides, and lines whose start and end times both match one line only on the other side, anchor
/// the alignment where they appear in the same order (patience diff); the stretches between
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

    // The dynamic program is O(n*m) in time and memory, and every edit re-runs it; beyond this a
    // stretch is merged by time instead (see AlignByTime).
    private const long MaxCells = 250_000;

    // A line pair whose start and end differ by no more than this (or that isTimeEqual calls equal)
    // has the same timing - covers the rounding of formats with centisecond or frame precision.
    private static readonly long SameTimeToleranceTicks = TimeSpan.FromMilliseconds(10).Ticks;
    private static readonly long SameTimeSearchTicks = TimeSpan.FromMilliseconds(100).Ticks;

    // A unique text whose two lines are further apart in time than this is only trusted when a
    // neighbouring text anchor has about the same offset - a file shifted as a whole still anchors,
    // a lone "Okay." far away from its namesake does not.
    private static readonly long FarApartTicks = TimeSpan.FromSeconds(10).Ticks;
    private static readonly long SameOffsetTicks = TimeSpan.FromSeconds(3).Ticks;

    // Where text anchors and timing anchors disagree, the same text is the stronger evidence.
    private const int TextAnchorWeight = 2;
    private const int TimingAnchorWeight = 1;

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
            foreach (var (al, ar) in FindAnchors(l0, l1, r0, r1))
            {
                AlignStretch(l, al, r, ar, result);
                result.Add(new Pair(al, ar));
                l = al + 1;
                r = ar + 1;
            }

            AlignStretch(l, l1, r, r1, result);
        }

        /// <summary>
        /// The patience diff anchors: lines whose text occurs exactly once on each side of the
        /// segment, and lines whose timing matches exactly one line on the other side and vice
        /// versa, kept only where they appear in the same order on both. Where the two kinds
        /// disagree, the heaviest run wins, a text match weighing twice a timing match.
        /// </summary>
        private List<(int Left, int Right)> FindAnchors(int l0, int l1, int r0, int r1)
        {
            var candidates = new List<(int Left, int Right, int Weight)>();
            foreach (var (l, r) in FindTextCandidates(l0, l1, r0, r1))
            {
                candidates.Add((l, r, TextAnchorWeight));
            }

            foreach (var (l, r) in FindTimingCandidates(l0, l1, r0, r1))
            {
                candidates.Add((l, r, TimingAnchorWeight));
            }

            // By left, and by right descending for the same left, so the strictly increasing run
            // can never take two pairs of one left line; the same pair found twice adds up.
            candidates.Sort((a, b) => a.Left != b.Left ? a.Left.CompareTo(b.Left) : b.Right.CompareTo(a.Right));
            var merged = new List<(int Left, int Right, int Weight)>(candidates.Count);
            foreach (var c in candidates)
            {
                if (merged.Count > 0 && merged[^1].Left == c.Left && merged[^1].Right == c.Right)
                {
                    merged[^1] = (c.Left, c.Right, merged[^1].Weight + c.Weight);
                }
                else
                {
                    merged.Add(c);
                }
            }

            return HeaviestIncreasingByRight(merged, r0, r1);
        }

        private List<(int Left, int Right)> FindTextCandidates(int l0, int l1, int r0, int r1)
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

            // A pair far apart in time needs a neighbour with about the same offset to vouch for it.
            var result = new List<(int Left, int Right)>(candidates.Count);
            for (var k = 0; k < candidates.Count; k++)
            {
                var (cl, cr) = candidates[k];
                if (!IsFarApart(_left[cl], _right[cr]) ||
                    (k > 0 && IsSameOffset(candidates[k - 1], candidates[k])) ||
                    (k + 1 < candidates.Count && IsSameOffset(candidates[k + 1], candidates[k])))
                {
                    result.Add(candidates[k]);
                }
            }

            return result;
        }

        private static bool IsFarApart(Line a, Line b) =>
            !Overlaps(a, b) && Math.Abs(a.Start.Ticks - b.Start.Ticks) > FarApartTicks;

        private bool IsSameOffset((int Left, int Right) a, (int Left, int Right) b)
        {
            var offsetA = _right[a.Right].Start.Ticks - _left[a.Left].Start.Ticks;
            var offsetB = _right[b.Right].Start.Ticks - _left[b.Left].Start.Ticks;
            return Math.Abs(offsetA - offsetB) <= SameOffsetTicks;
        }

        private static bool Overlaps(Line a, Line b) =>
            Math.Min(a.End.Ticks, b.End.Ticks) > Math.Max(a.Start.Ticks, b.Start.Ticks);

        /// <summary>Pairs whose start and end both match, where neither line matches any other line that way.</summary>
        private List<(int Left, int Right)> FindTimingCandidates(int l0, int l1, int r0, int r1)
        {
            var result = new List<(int Left, int Right)>();
            if (l0 >= l1 || r0 >= r1)
            {
                return result;
            }

            var rightByStart = new int[r1 - r0];
            for (var j = 0; j < rightByStart.Length; j++)
            {
                rightByStart[j] = r0 + j;
            }

            Array.Sort(rightByStart, (a, b) => _right[a].Start.CompareTo(_right[b].Start));

            var rightMatchCount = new int[r1 - r0];
            var leftMatch = new int[l1 - l0];
            for (var i = l0; i < l1; i++)
            {
                var line = _left[i];
                var match = -1;
                var count = 0;
                for (var k = LowerBoundByStart(rightByStart, line.Start.Ticks - SameTimeSearchTicks); k < rightByStart.Length; k++)
                {
                    var other = _right[rightByStart[k]];
                    if (other.Start.Ticks > line.Start.Ticks + SameTimeSearchTicks)
                    {
                        break;
                    }

                    if (IsSameTime(line.Start, other.Start) && IsSameTime(line.End, other.End))
                    {
                        match = rightByStart[k];
                        count++;
                        rightMatchCount[match - r0]++;
                    }
                }

                leftMatch[i - l0] = count == 1 ? match : -1;
            }

            for (var i = l0; i < l1; i++)
            {
                var match = leftMatch[i - l0];
                if (match >= 0 && rightMatchCount[match - r0] == 1)
                {
                    result.Add((i, match));
                }
            }

            return result;
        }

        private int LowerBoundByStart(int[] rightByStart, long ticks)
        {
            int lo = 0, hi = rightByStart.Length;
            while (lo < hi)
            {
                var mid = (lo + hi) / 2;
                if (_right[rightByStart[mid]].Start.Ticks < ticks)
                {
                    lo = mid + 1;
                }
                else
                {
                    hi = mid;
                }
            }

            return lo;
        }

        private bool IsSameTime(TimeSpan a, TimeSpan b) =>
            Math.Abs(a.Ticks - b.Ticks) <= SameTimeToleranceTicks || _isTimeEqual(a, b);

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

        /// <summary>
        /// Candidates come sorted by left index (right descending within one); keeps the heaviest
        /// subsequence strictly increasing in right index too - a Fenwick tree of the best chain
        /// ending at or before each right index makes it O(n log n).
        /// </summary>
        private static List<(int Left, int Right)> HeaviestIncreasingByRight(List<(int Left, int Right, int Weight)> candidates, int r0, int r1)
        {
            var result = new List<(int Left, int Right)>();
            if (candidates.Count == 0)
            {
                return result;
            }

            var size = r1 - r0;
            var treeWeight = new long[size + 1];
            var treeIndex = new int[size + 1];
            var previous = new int[candidates.Count];
            long bestWeight = 0;
            var bestIndex = -1;
            for (var i = 0; i < candidates.Count; i++)
            {
                var position = candidates[i].Right - r0; // chains ending at a right index < Right
                long before = 0;
                var beforeIndex = -1;
                for (var k = position; k > 0; k -= k & -k)
                {
                    if (treeWeight[k] > before)
                    {
                        before = treeWeight[k];
                        beforeIndex = treeIndex[k];
                    }
                }

                previous[i] = beforeIndex;
                var weight = before + candidates[i].Weight;
                if (weight > bestWeight)
                {
                    bestWeight = weight;
                    bestIndex = i;
                }

                for (var k = position + 1; k <= size; k += k & -k)
                {
                    if (weight > treeWeight[k])
                    {
                        treeWeight[k] = weight;
                        treeIndex[k] = i;
                    }
                }
            }

            for (var i = bestIndex; i >= 0; i = previous[i])
            {
                result.Add((candidates[i].Left, candidates[i].Right));
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
                AlignByTime(l0, l1, r0, r1, result);
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

        /// <summary>
        /// The cheap alignment of a stretch too big for the dynamic program: walks both sides in
        /// step, pairing lines that overlap in time; a line is left alone only when it comes first
        /// and the next line on its own side overlaps the other side's line instead - otherwise the
        /// two pair by position, as unrelated timings did before.
        /// </summary>
        private void AlignByTime(int l0, int l1, int r0, int r1, List<Pair> result)
        {
            int i = l0, j = r0;
            while (i < l1 && j < r1)
            {
                var a = _left[i];
                var b = _right[j];
                if (!Overlaps(a, b))
                {
                    if (a.Start < b.Start && i + 1 < l1 && Overlaps(_left[i + 1], b))
                    {
                        result.Add(new Pair(i++, -1));
                        continue;
                    }

                    if (b.Start < a.Start && j + 1 < r1 && Overlaps(a, _right[j + 1]))
                    {
                        result.Add(new Pair(-1, j++));
                        continue;
                    }
                }

                result.Add(new Pair(i++, j++));
            }

            while (i < l1)
            {
                result.Add(new Pair(i++, -1));
            }

            while (j < r1)
            {
                result.Add(new Pair(-1, j++));
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
