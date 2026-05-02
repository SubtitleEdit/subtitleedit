using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.UiLogic.Ocr;

internal static class NOcrLineGenerator
{
    private const int FgAlphaThreshold = 150;

    internal static void GenerateSkeletonDistance(int maxNumberOfLines, bool veryPrecise, NOcrChar nOcrChar, NikseBitmap2 bitmap)
    {
        var w = bitmap.Width;
        var h = bitmap.Height;
        if (w < 4 || h < 4)
        {
            NOcrChar.GenerateLineSegmentsRandom(maxNumberOfLines, veryPrecise, nOcrChar, bitmap);
            return;
        }

        var fg = BuildForegroundMask(bitmap);
        var skel = ZhangSuenThin((bool[,])fg.Clone(), w, h);

        // Scan-based FG runs first - guaranteed to catch axis-aligned strokes
        // (the verticals of M, the horizontal bar of T, etc.) where skeleton
        // tracing + Douglas-Peucker can drop a segment due to thinning wobble.
        AddScanBasedForegroundLines(fg, w, h, nOcrChar, bitmap, maxNumberOfLines);
        // Skeleton complements with curves and arbitrary-angle strokes.
        AddSkeletonForegroundLines(skel, w, h, nOcrChar, bitmap, maxNumberOfLines, veryPrecise);
        AddDistanceTransformBackgroundLines(fg, w, h, nOcrChar, bitmap, maxNumberOfLines, veryPrecise);

        NOcrChar.RemoveDuplicates(nOcrChar.LinesForeground);
        NOcrChar.RemoveDuplicates(nOcrChar.LinesBackground);
        RemoveSimilarLines(nOcrChar.LinesForeground, nOcrChar.Width, nOcrChar.Height);
        RemoveSimilarLines(nOcrChar.LinesBackground, nOcrChar.Width, nOcrChar.Height);
    }

    internal static void GenerateEdgeHough(int maxNumberOfLines, bool veryPrecise, NOcrChar nOcrChar, NikseBitmap2 bitmap)
    {
        var w = bitmap.Width;
        var h = bitmap.Height;
        if (w < 6 || h < 6)
        {
            NOcrChar.GenerateLineSegmentsRandom(maxNumberOfLines, veryPrecise, nOcrChar, bitmap);
            return;
        }

        var fg = BuildForegroundMask(bitmap);
        var edges = SobelEdgeMask(fg, w, h);
        var minRun = Math.Max(3, Math.Min(w, h) / 4);

        // FG lines: Hough peaks on FG pixels themselves -> stroke centerlines/diagonals.
        var fgPeaks = HoughPeaksOnMask(fg, edges, w, h, wantFg: true, maxPeaks: Math.Max(maxNumberOfLines * 3, 60));
        foreach (var peak in fgPeaks)
        {
            if (nOcrChar.LinesForeground.Count >= maxNumberOfLines)
            {
                break;
            }

            var seg = LongestRunAlongLine(peak, fg, w, h, wantFg: true, minRun: minRun);
            if (!seg.HasValue)
            {
                continue;
            }

            var line = MakeLine(seg.Value.X1, seg.Value.Y1, seg.Value.X2, seg.Value.Y2, nOcrChar, bitmap);
            if (line != null && IsFgCenterline(line, bitmap, nOcrChar))
            {
                nOcrChar.LinesForeground.Add(line);
            }
        }

        // BG lines: Hough peaks on BG pixels -> corridors through negative space.
        var bgPeaks = HoughPeaksOnMask(fg, edges, w, h, wantFg: false, maxPeaks: Math.Max(maxNumberOfLines * 3, 60));
        foreach (var peak in bgPeaks)
        {
            if (nOcrChar.LinesBackground.Count >= maxNumberOfLines)
            {
                break;
            }

            var seg = LongestRunAlongLine(peak, fg, w, h, wantFg: false, minRun: minRun);
            if (!seg.HasValue)
            {
                continue;
            }

            var line = MakeLine(seg.Value.X1, seg.Value.Y1, seg.Value.X2, seg.Value.Y2, nOcrChar, bitmap);
            if (line != null && IsBgCenterline(line, bitmap, nOcrChar))
            {
                nOcrChar.LinesBackground.Add(line);
            }
        }

        // Same fixed corner-cut probes as the Random algorithm uses for its first
        // BG iterations - cheap and helps disambiguate corners on letters with
        // sharp top serifs (T, F, etc.).
        AddCornerCuts(nOcrChar, bitmap, maxNumberOfLines);

        NOcrChar.RemoveDuplicates(nOcrChar.LinesForeground);
        NOcrChar.RemoveDuplicates(nOcrChar.LinesBackground);
        RemoveSimilarLines(nOcrChar.LinesForeground, nOcrChar.Width, nOcrChar.Height);
        RemoveSimilarLines(nOcrChar.LinesBackground, nOcrChar.Width, nOcrChar.Height);
    }

    // -------------------- Shared helpers --------------------

    /// <summary>
    /// Collapse pairs of lines whose orientations and midpoints are close, keeping
    /// the longer one. Catches near-parallel duplicates that the exact-match
    /// <see cref="NOcrChar.RemoveDuplicates"/> misses (e.g. two Hough peaks on the
    /// same thick stroke at slightly different rho/theta).
    /// </summary>
    internal static void RemoveSimilarLines(List<NOcrLine> lines, int width, int height)
    {
        if (lines.Count < 2)
        {
            return;
        }

        const double angleThresholdRad = Math.PI * 5.0 / 180.0; // 5 degrees
        var midDistThreshold = Math.Max(2.0, Math.Min(width, height) / 8.0);
        var midDistThresholdSq = midDistThreshold * midDistThreshold;

        var angles = new double[lines.Count];
        var midX = new double[lines.Count];
        var midY = new double[lines.Count];
        var lengthSq = new int[lines.Count];
        for (var i = 0; i < lines.Count; i++)
        {
            angles[i] = LineAngleNormalized(lines[i]);
            midX[i] = (lines[i].Start.X + lines[i].End.X) / 2.0;
            midY[i] = (lines[i].Start.Y + lines[i].End.Y) / 2.0;
            lengthSq[i] = LineLengthSquared(lines[i]);
        }

        var toRemove = new HashSet<int>();
        for (var i = 0; i < lines.Count; i++)
        {
            if (toRemove.Contains(i))
            {
                continue;
            }

            for (var j = i + 1; j < lines.Count; j++)
            {
                if (toRemove.Contains(j))
                {
                    continue;
                }

                var dAng = Math.Abs(angles[i] - angles[j]);
                if (dAng > Math.PI / 2)
                {
                    dAng = Math.PI - dAng;
                }
                if (dAng > angleThresholdRad)
                {
                    continue;
                }

                var dx = midX[i] - midX[j];
                var dy = midY[i] - midY[j];
                if (dx * dx + dy * dy > midDistThresholdSq)
                {
                    continue;
                }

                // When the two lines are close in length, prefer the more
                // axis-aligned one (cleaner geometry usually means scan-based
                // hit it; the other is likely a 1-px wobble from the skeleton).
                var maxLen = Math.Max(lengthSq[i], lengthSq[j]);
                var lengthClose = Math.Abs(lengthSq[i] - lengthSq[j]) <= maxLen * 0.25;
                bool dropJ;
                if (lengthClose)
                {
                    var axisI = DistanceToNearestAxis(angles[i]);
                    var axisJ = DistanceToNearestAxis(angles[j]);
                    if (axisI != axisJ)
                    {
                        dropJ = axisI <= axisJ;
                    }
                    else
                    {
                        dropJ = lengthSq[j] <= lengthSq[i];
                    }
                }
                else
                {
                    dropJ = lengthSq[j] <= lengthSq[i];
                }

                if (dropJ)
                {
                    toRemove.Add(j);
                }
                else
                {
                    toRemove.Add(i);
                    break;
                }
            }
        }

        foreach (var idx in toRemove.OrderByDescending(p => p))
        {
            lines.RemoveAt(idx);
        }
    }

    private static double DistanceToNearestAxis(double angle)
    {
        // angle is normalized to [0, π). Axes of interest: 0 and π/2.
        // π itself is == 0 modulo direction, so handle the wrap.
        var d0 = Math.Min(angle, Math.PI - angle);
        var dHalf = Math.Abs(angle - Math.PI / 2);
        return Math.Min(d0, dHalf);
    }

    private static double LineAngleNormalized(NOcrLine line)
    {
        var dx = line.End.X - line.Start.X;
        var dy = line.End.Y - line.Start.Y;
        var a = Math.Atan2(dy, dx);
        // Direction-insensitive: a line going SE is the same as going NW.
        if (a < 0)
        {
            a += Math.PI;
        }
        if (a >= Math.PI)
        {
            a -= Math.PI;
        }
        return a;
    }

    private static int LineLengthSquared(NOcrLine line)
    {
        var dx = line.End.X - line.Start.X;
        var dy = line.End.Y - line.Start.Y;
        return dx * dx + dy * dy;
    }

    /// <summary>
    /// Centerline-only FG check that tolerates a small number of BG pixels
    /// along the line (anti-aliasing creates 1-px gaps mid-stroke on rendered
    /// glyphs). Allows ceil(N / 15) BG pixels capped at 2 - so a 30-px stroke
    /// may have up to 2 anti-aliased "off" pixels and still validate.
    /// </summary>
    private static bool IsFgCenterline(NOcrLine op, NikseBitmap2 nbmp, NOcrChar nOcrChar)
    {
        if (Math.Abs(op.Start.X - op.End.X) < 2 && Math.Abs(op.End.Y - op.Start.Y) < 2)
        {
            return false;
        }

        var total = 0;
        var bg = 0;
        foreach (var point in op.ScaledGetPoints(nOcrChar, nbmp.Width, nbmp.Height))
        {
            if (point.X < 0 || point.Y < 0 || point.X >= nbmp.Width || point.Y >= nbmp.Height)
            {
                continue;
            }
            total++;
            if (nbmp.GetAlpha(point.X, point.Y) <= FgAlphaThreshold)
            {
                bg++;
            }
        }
        if (total == 0)
        {
            return false;
        }
        var maxBgAllowed = Math.Min(2, total / 15);
        return bg <= maxBgAllowed;
    }

    /// <summary>
    /// Strict centerline-only BG check. NOcrChar.IsMatchPointBackGround unconditionally
    /// also checks the (x+1,y) neighbor, which rejects most BG lines running adjacent
    /// to a stroke - too aggressive for the structured algorithms that intentionally
    /// run lines along narrow corridors.
    /// </summary>
    private static bool IsBgCenterline(NOcrLine op, NikseBitmap2 nbmp, NOcrChar nOcrChar)
    {
        if (Math.Abs(op.Start.X - op.End.X) < 2 && Math.Abs(op.End.Y - op.Start.Y) < 2)
        {
            return false;
        }

        foreach (var point in op.ScaledGetPoints(nOcrChar, nbmp.Width, nbmp.Height))
        {
            if (point.X < 0 || point.Y < 0 || point.X >= nbmp.Width || point.Y >= nbmp.Height)
            {
                continue;
            }
            if (nbmp.GetAlpha(point.X, point.Y) > FgAlphaThreshold)
            {
                return false;
            }
        }
        return true;
    }

    private static bool[,] BuildForegroundMask(NikseBitmap2 bitmap)
    {
        var w = bitmap.Width;
        var h = bitmap.Height;
        var fg = new bool[w, h];
        for (var y = 0; y < h; y++)
        {
            for (var x = 0; x < w; x++)
            {
                fg[x, y] = bitmap.GetAlpha(x, y) > FgAlphaThreshold;
            }
        }
        return fg;
    }

    private static NOcrLine? MakeLine(int x1, int y1, int x2, int y2, NOcrChar nOcrChar, NikseBitmap2 bitmap)
    {
        if (x1 == x2 && y1 == y2)
        {
            return null;
        }

        var s = BitmapToNOcr(x1, y1, nOcrChar, bitmap);
        var e = BitmapToNOcr(x2, y2, nOcrChar, bitmap);
        if (s.X == e.X && s.Y == e.Y)
        {
            return null;
        }

        return new NOcrLine(s, e);
    }

    private static OcrPoint BitmapToNOcr(int x, int y, NOcrChar nOcrChar, NikseBitmap2 bitmap)
    {
        if (bitmap.Width == nOcrChar.Width && bitmap.Height == nOcrChar.Height)
        {
            return new OcrPoint(x, y);
        }

        var nx = (int)Math.Round(x * nOcrChar.Width / (double)bitmap.Width);
        var ny = (int)Math.Round(y * nOcrChar.Height / (double)bitmap.Height);
        return new OcrPoint(nx, ny);
    }

    // -------------------- Zhang-Suen thinning --------------------

    private static bool[,] ZhangSuenThin(bool[,] image, int w, int h)
    {
        var changed = true;
        var toRemove = new List<(int X, int Y)>();
        var maxIter = 200;

        while (changed && maxIter-- > 0)
        {
            changed = false;

            for (var step = 0; step < 2; step++)
            {
                toRemove.Clear();

                for (var y = 1; y < h - 1; y++)
                {
                    for (var x = 1; x < w - 1; x++)
                    {
                        if (!image[x, y])
                        {
                            continue;
                        }

                        // Neighbors P2..P9 in clockwise order starting from N
                        var p2 = image[x, y - 1];
                        var p3 = image[x + 1, y - 1];
                        var p4 = image[x + 1, y];
                        var p5 = image[x + 1, y + 1];
                        var p6 = image[x, y + 1];
                        var p7 = image[x - 1, y + 1];
                        var p8 = image[x - 1, y];
                        var p9 = image[x - 1, y - 1];

                        var b = (p2 ? 1 : 0) + (p3 ? 1 : 0) + (p4 ? 1 : 0) + (p5 ? 1 : 0)
                              + (p6 ? 1 : 0) + (p7 ? 1 : 0) + (p8 ? 1 : 0) + (p9 ? 1 : 0);
                        if (b < 2 || b > 6)
                        {
                            continue;
                        }

                        var a = 0;
                        if (!p2 && p3) { a++; }
                        if (!p3 && p4) { a++; }
                        if (!p4 && p5) { a++; }
                        if (!p5 && p6) { a++; }
                        if (!p6 && p7) { a++; }
                        if (!p7 && p8) { a++; }
                        if (!p8 && p9) { a++; }
                        if (!p9 && p2) { a++; }
                        if (a != 1)
                        {
                            continue;
                        }

                        if (step == 0)
                        {
                            if (p2 && p4 && p6) { continue; }
                            if (p4 && p6 && p8) { continue; }
                        }
                        else
                        {
                            if (p2 && p4 && p8) { continue; }
                            if (p2 && p6 && p8) { continue; }
                        }

                        toRemove.Add((x, y));
                    }
                }

                if (toRemove.Count > 0)
                {
                    changed = true;
                    foreach (var p in toRemove)
                    {
                        image[p.X, p.Y] = false;
                    }
                }
            }
        }

        return image;
    }

    // -------------------- Skeleton tracing + Douglas-Peucker --------------------

    private static void AddSkeletonForegroundLines(bool[,] skel, int w, int h, NOcrChar nOcrChar, NikseBitmap2 bitmap, int maxNumberOfLines, bool veryPrecise)
    {
        var visited = new bool[w, h];
        var paths = new List<List<(int X, int Y)>>();

        // First pass: trace from endpoints (1 neighbor)
        for (var y = 0; y < h; y++)
        {
            for (var x = 0; x < w; x++)
            {
                if (skel[x, y] && !visited[x, y] && CountNeighbors(skel, x, y, w, h) == 1)
                {
                    var path = TraceFrom(skel, visited, x, y, w, h);
                    if (path.Count >= 2)
                    {
                        paths.Add(path);
                    }
                }
            }
        }

        // Second pass: any remaining skeleton pixels (loops or branches)
        for (var y = 0; y < h; y++)
        {
            for (var x = 0; x < w; x++)
            {
                if (skel[x, y] && !visited[x, y])
                {
                    var path = TraceFrom(skel, visited, x, y, w, h);
                    if (path.Count >= 2)
                    {
                        paths.Add(path);
                    }
                }
            }
        }

        foreach (var path in paths)
        {
            if (nOcrChar.LinesForeground.Count >= maxNumberOfLines)
            {
                break;
            }

            var keptIndices = DouglasPeuckerIndices(path, epsilon: 1.0);
            for (var i = 0; i + 1 < keptIndices.Count; i++)
            {
                if (nOcrChar.LinesForeground.Count >= maxNumberOfLines)
                {
                    break;
                }

                EmitSegmentRecursive(path, keptIndices[i], keptIndices[i + 1], nOcrChar, bitmap, maxNumberOfLines);
            }
        }
    }

    /// <summary>
    /// Try to emit a single straight segment from path[lo] to path[hi].
    /// If the validator rejects it (e.g. the skeleton wobbled and the straight
    /// Bresenham line clips a BG pixel), split at the midpoint and recurse.
    /// </summary>
    private static void EmitSegmentRecursive(List<(int X, int Y)> path, int lo, int hi, NOcrChar nOcrChar, NikseBitmap2 bitmap, int maxNumberOfLines)
    {
        if (lo >= hi || nOcrChar.LinesForeground.Count >= maxNumberOfLines)
        {
            return;
        }

        var a = path[lo];
        var b = path[hi];
        var line = MakeLine(a.X, a.Y, b.X, b.Y, nOcrChar, bitmap);
        if (line != null && IsFgCenterline(line, bitmap, nOcrChar))
        {
            nOcrChar.LinesForeground.Add(line);
            return;
        }

        if (hi - lo <= 1)
        {
            return;
        }

        var mid = (lo + hi) / 2;
        EmitSegmentRecursive(path, lo, mid, nOcrChar, bitmap, maxNumberOfLines);
        EmitSegmentRecursive(path, mid, hi, nOcrChar, bitmap, maxNumberOfLines);
    }

    private static int CountNeighbors(bool[,] m, int x, int y, int w, int h)
    {
        var count = 0;
        for (var dy = -1; dy <= 1; dy++)
        {
            for (var dx = -1; dx <= 1; dx++)
            {
                if (dx == 0 && dy == 0)
                {
                    continue;
                }

                var nx = x + dx;
                var ny = y + dy;
                if (nx >= 0 && ny >= 0 && nx < w && ny < h && m[nx, ny])
                {
                    count++;
                }
            }
        }
        return count;
    }

    private static readonly int[] _neighborDx = { 1, -1, 0, 0, 1, 1, -1, -1 };
    private static readonly int[] _neighborDy = { 0, 0, 1, -1, 1, -1, 1, -1 };

    private static List<(int X, int Y)> TraceFrom(bool[,] m, bool[,] visited, int sx, int sy, int w, int h)
    {
        var path = new List<(int X, int Y)>();
        var x = sx;
        var y = sy;
        path.Add((x, y));
        visited[x, y] = true;
        var lastDx = 0;
        var lastDy = 0;

        while (true)
        {
            // Score each unvisited neighbor by alignment with the last step
            // (so at a junction we prefer the continuation rather than an
            // arbitrary first-found branch). Tiebreak with the original
            // cardinal-first preference.
            var bestScore = int.MinValue;
            var bestDx = 0;
            var bestDy = 0;
            var found = false;

            for (var i = 0; i < 8; i++)
            {
                var dx = _neighborDx[i];
                var dy = _neighborDy[i];
                var nx = x + dx;
                var ny = y + dy;
                if (nx < 0 || ny < 0 || nx >= w || ny >= h)
                {
                    continue;
                }
                if (!m[nx, ny] || visited[nx, ny])
                {
                    continue;
                }

                int score;
                if (lastDx == 0 && lastDy == 0)
                {
                    // First step: cardinal-first preference.
                    score = -i;
                }
                else
                {
                    // dot * big + cardinal bonus + index tie-breaker
                    var dot = dx * lastDx + dy * lastDy;
                    var cardinalBonus = (dx == 0 || dy == 0) ? 1 : 0;
                    score = dot * 100 + cardinalBonus * 10 - i;
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    bestDx = dx;
                    bestDy = dy;
                    found = true;
                }
            }

            if (found)
            {
                x += bestDx;
                y += bestDy;
                visited[x, y] = true;
                path.Add((x, y));
                lastDx = bestDx;
                lastDy = bestDy;
            }
            else
            {
                break;
            }
        }

        return path;
    }

    private static List<int> DouglasPeuckerIndices(List<(int X, int Y)> path, double epsilon)
    {
        var result = new List<int>();
        if (path.Count == 0)
        {
            return result;
        }
        if (path.Count == 1)
        {
            result.Add(0);
            return result;
        }

        var keep = new bool[path.Count];
        keep[0] = true;
        keep[path.Count - 1] = true;
        if (path.Count >= 3)
        {
            DouglasPeuckerRecursive(path, 0, path.Count - 1, epsilon, keep);
        }

        for (var i = 0; i < path.Count; i++)
        {
            if (keep[i])
            {
                result.Add(i);
            }
        }
        return result;
    }

    private static void DouglasPeuckerRecursive(List<(int X, int Y)> path, int i, int j, double epsilon, bool[] keep)
    {
        if (j - i < 2)
        {
            return;
        }

        var a = path[i];
        var b = path[j];
        double maxDist = 0;
        var maxIndex = -1;

        var dx = b.X - a.X;
        var dy = b.Y - a.Y;
        var len = Math.Sqrt(dx * dx + dy * dy);

        for (var k = i + 1; k < j; k++)
        {
            var p = path[k];
            double d;
            if (len < 1e-9)
            {
                var ex = p.X - a.X;
                var ey = p.Y - a.Y;
                d = Math.Sqrt(ex * ex + ey * ey);
            }
            else
            {
                d = Math.Abs(dy * p.X - dx * p.Y + b.X * a.Y - b.Y * a.X) / len;
            }

            if (d > maxDist)
            {
                maxDist = d;
                maxIndex = k;
            }
        }

        if (maxDist > epsilon && maxIndex >= 0)
        {
            keep[maxIndex] = true;
            DouglasPeuckerRecursive(path, i, maxIndex, epsilon, keep);
            DouglasPeuckerRecursive(path, maxIndex, j, epsilon, keep);
        }
    }

    // -------------------- Scan-based FG (sort longest-first, all directions) --------------------

    private enum ScanKind { Row, Column, Diagonal, AntiDiagonal }

    private readonly struct ScanCandidate
    {
        public readonly ScanKind Kind;
        public readonly int Length;
        public readonly int X1;
        public readonly int Y1;
        public readonly int X2;
        public readonly int Y2;
        public readonly int Coord;
        public ScanCandidate(ScanKind kind, int length, int x1, int y1, int x2, int y2, int coord)
        {
            Kind = kind; Length = length;
            X1 = x1; Y1 = y1; X2 = x2; Y2 = y2;
            Coord = coord;
        }
    }

    private static void AddScanBasedForegroundLines(bool[,] fg, int w, int h, NOcrChar nOcrChar, NikseBitmap2 bitmap, int maxNumberOfLines)
    {
        var candidates = new List<ScanCandidate>();
        CollectFgRowCandidates(fg, w, h, candidates);
        CollectFgColumnCandidates(fg, w, h, candidates);
        CollectFgDiagonalCandidates(fg, w, h, antiDiagonal: false, candidates);
        CollectFgDiagonalCandidates(fg, w, h, antiDiagonal: true, candidates);

        EmitGreedyByLength(candidates, w, h, nOcrChar, bitmap, maxNumberOfLines, isForeground: true);
    }

    /// <summary>
    /// Sort candidates by length descending and greedy-pick respecting per-kind
    /// spacing. This is the order-fix: ALL directions compete on length so a
    /// long vertical (M's leg) wins over a short horizontal (M's apex pixel
    /// pattern) when the cap is small.
    /// </summary>
    private static void EmitGreedyByLength(List<ScanCandidate> candidates, int w, int h, NOcrChar nOcrChar, NikseBitmap2 bitmap, int cap, bool isForeground)
    {
        candidates.Sort((a, b) => b.Length.CompareTo(a.Length));

        var minSpaceX = Math.Max(2, w / 10);
        var minSpaceY = Math.Max(2, h / 10);
        var minSpaceDiag = Math.Max(2, Math.Min(w, h) / 6);

        var pickedRowYs = new List<int>();
        var pickedColXs = new List<int>();
        var pickedDiagOffsets = new List<int>();
        var pickedAntiDiagOffsets = new List<int>();

        var lines = isForeground ? nOcrChar.LinesForeground : nOcrChar.LinesBackground;

        foreach (var c in candidates)
        {
            if (lines.Count >= cap)
            {
                break;
            }

            int spacing;
            List<int> picked;
            switch (c.Kind)
            {
                case ScanKind.Row: picked = pickedRowYs; spacing = minSpaceY; break;
                case ScanKind.Column: picked = pickedColXs; spacing = minSpaceX; break;
                case ScanKind.Diagonal: picked = pickedDiagOffsets; spacing = minSpaceDiag; break;
                default: picked = pickedAntiDiagOffsets; spacing = minSpaceDiag; break;
            }

            var tooClose = false;
            foreach (var p in picked)
            {
                if (Math.Abs(p - c.Coord) < spacing)
                {
                    tooClose = true;
                    break;
                }
            }
            if (tooClose)
            {
                continue;
            }

            var line = MakeLine(c.X1, c.Y1, c.X2, c.Y2, nOcrChar, bitmap);
            if (line == null)
            {
                continue;
            }

            var ok = isForeground
                ? IsFgCenterline(line, bitmap, nOcrChar)
                : IsBgCenterline(line, bitmap, nOcrChar);
            if (ok)
            {
                lines.Add(line);
                picked.Add(c.Coord);
            }
        }
    }

    private static void CollectFgRowCandidates(bool[,] fg, int w, int h, List<ScanCandidate> candidates)
    {
        for (var y = 0; y < h; y++)
        {
            var firstFg = -1;
            var lastFg = -1;
            for (var x = 0; x < w; x++)
            {
                if (fg[x, y])
                {
                    if (firstFg < 0) { firstFg = x; }
                    lastFg = x;
                }
            }

            if (firstFg < 0)
            {
                continue;
            }

            // Use outer extent so anti-aliasing gaps don't artificially shorten
            // a stroke. The validator tolerates a small number of BG pixels.
            var span = lastFg - firstFg + 1;
            if (span >= 4)
            {
                candidates.Add(new ScanCandidate(ScanKind.Row, span, firstFg, y, lastFg, y, y));
            }
        }
    }

    private static void CollectFgColumnCandidates(bool[,] fg, int w, int h, List<ScanCandidate> candidates)
    {
        for (var x = 0; x < w; x++)
        {
            var firstFg = -1;
            var lastFg = -1;
            for (var y = 0; y < h; y++)
            {
                if (fg[x, y])
                {
                    if (firstFg < 0) { firstFg = y; }
                    lastFg = y;
                }
            }

            if (firstFg < 0)
            {
                continue;
            }

            var span = lastFg - firstFg + 1;
            if (span >= 4)
            {
                candidates.Add(new ScanCandidate(ScanKind.Column, span, x, firstFg, x, lastFg, x));
            }
        }
    }

    private static void CollectFgDiagonalCandidates(bool[,] fg, int w, int h, bool antiDiagonal, List<ScanCandidate> candidates)
    {
        var minOffset = antiDiagonal ? 0 : -(h - 1);
        var maxOffset = antiDiagonal ? (w + h - 2) : (w - 1);
        var kind = antiDiagonal ? ScanKind.AntiDiagonal : ScanKind.Diagonal;

        for (var c = minOffset; c <= maxOffset; c++)
        {
            int x, y;
            if (antiDiagonal)
            {
                x = Math.Max(0, c - (h - 1));
                y = c - x;
            }
            else
            {
                x = Math.Max(0, -c);
                y = x + c;
            }

            var bestLen = 0;
            var bestStartX = 0;
            var bestStartY = 0;
            var bestEndX = 0;
            var bestEndY = 0;

            var runLen = 0;
            var runStartX = 0;
            var runStartY = 0;

            while (x >= 0 && y >= 0 && x < w && y < h)
            {
                if (fg[x, y])
                {
                    if (runLen == 0)
                    {
                        runStartX = x;
                        runStartY = y;
                    }
                    runLen++;
                    if (runLen > bestLen)
                    {
                        bestLen = runLen;
                        bestStartX = runStartX;
                        bestStartY = runStartY;
                        bestEndX = x;
                        bestEndY = y;
                    }
                }
                else
                {
                    runLen = 0;
                }

                if (antiDiagonal)
                {
                    x++;
                    y--;
                }
                else
                {
                    x++;
                    y++;
                }
            }

            if (bestLen >= 5)
            {
                candidates.Add(new ScanCandidate(kind, bestLen, bestStartX, bestStartY, bestEndX, bestEndY, c));
            }
        }
    }

    private static void AddDistanceTransformBackgroundLines(bool[,] fg, int w, int h, NOcrChar nOcrChar, NikseBitmap2 bitmap, int maxNumberOfLines, bool veryPrecise)
    {
        var dt = ChamferDistanceTransform(fg, w, h);
        var candidates = new List<ScanCandidate>();
        CollectBgRowCandidates(fg, dt, w, h, candidates);
        CollectBgColumnCandidates(fg, dt, w, h, candidates);
        CollectBgDiagonalCandidates(fg, w, h, antiDiagonal: false, candidates);
        CollectBgDiagonalCandidates(fg, w, h, antiDiagonal: true, candidates);

        EmitGreedyByLength(candidates, w, h, nOcrChar, bitmap, maxNumberOfLines, isForeground: false);
        AddCornerCuts(nOcrChar, bitmap, maxNumberOfLines);
    }

    private static int[,] ChamferDistanceTransform(bool[,] fg, int w, int h)
    {
        const int big = 1_000_000;
        const int dOrtho = 3;
        const int dDiag = 4;

        var d = new int[w, h];
        for (var y = 0; y < h; y++)
        {
            for (var x = 0; x < w; x++)
            {
                d[x, y] = fg[x, y] ? 0 : big;
            }
        }

        // Forward pass
        for (var y = 0; y < h; y++)
        {
            for (var x = 0; x < w; x++)
            {
                if (d[x, y] == 0)
                {
                    continue;
                }

                var v = d[x, y];
                if (x > 0) { v = Math.Min(v, d[x - 1, y] + dOrtho); }
                if (y > 0) { v = Math.Min(v, d[x, y - 1] + dOrtho); }
                if (x > 0 && y > 0) { v = Math.Min(v, d[x - 1, y - 1] + dDiag); }
                if (x + 1 < w && y > 0) { v = Math.Min(v, d[x + 1, y - 1] + dDiag); }
                d[x, y] = v;
            }
        }

        // Backward pass
        for (var y = h - 1; y >= 0; y--)
        {
            for (var x = w - 1; x >= 0; x--)
            {
                if (d[x, y] == 0)
                {
                    continue;
                }

                var v = d[x, y];
                if (x + 1 < w) { v = Math.Min(v, d[x + 1, y] + dOrtho); }
                if (y + 1 < h) { v = Math.Min(v, d[x, y + 1] + dOrtho); }
                if (x + 1 < w && y + 1 < h) { v = Math.Min(v, d[x + 1, y + 1] + dDiag); }
                if (x > 0 && y + 1 < h) { v = Math.Min(v, d[x - 1, y + 1] + dDiag); }
                d[x, y] = v;
            }
        }

        return d;
    }

    private static void CollectBgRowCandidates(bool[,] fg, int[,] dt, int w, int h, List<ScanCandidate> candidates)
    {
        for (var y = 0; y < h; y++)
        {
            var bestStart = -1;
            var bestEnd = -1;
            var bestLen = 0;

            var x = 0;
            while (x < w)
            {
                if (fg[x, y])
                {
                    x++;
                    continue;
                }

                var start = x;
                while (x < w && !fg[x, y])
                {
                    x++;
                }
                var end = x - 1;
                var len = end - start + 1;
                if (len > bestLen)
                {
                    bestLen = len;
                    bestStart = start;
                    bestEnd = end;
                }
            }

            if (bestLen >= 4)
            {
                // Score = depth * length so deep wide corridors rank above
                // narrow ones, but the unit is comparable to length-only
                // FG candidates so they can be sorted together.
                var midDepth = dt[(bestStart + bestEnd) / 2, y];
                var score = Math.Max(bestLen, midDepth * bestLen);
                candidates.Add(new ScanCandidate(ScanKind.Row, score, bestStart, y, bestEnd, y, y));
            }
        }
    }

    private static void CollectBgColumnCandidates(bool[,] fg, int[,] dt, int w, int h, List<ScanCandidate> candidates)
    {
        for (var x = 0; x < w; x++)
        {
            var bestStart = -1;
            var bestEnd = -1;
            var bestLen = 0;

            var y = 0;
            while (y < h)
            {
                if (fg[x, y])
                {
                    y++;
                    continue;
                }

                var start = y;
                while (y < h && !fg[x, y])
                {
                    y++;
                }
                var end = y - 1;
                var len = end - start + 1;
                if (len > bestLen)
                {
                    bestLen = len;
                    bestStart = start;
                    bestEnd = end;
                }
            }

            if (bestLen >= 4)
            {
                var midDepth = dt[x, (bestStart + bestEnd) / 2];
                var score = Math.Max(bestLen, midDepth * bestLen);
                candidates.Add(new ScanCandidate(ScanKind.Column, score, x, bestStart, x, bestEnd, x));
            }
        }
    }

    private static void CollectBgDiagonalCandidates(bool[,] fg, int w, int h, bool antiDiagonal, List<ScanCandidate> candidates)
    {
        var minOffset = antiDiagonal ? 0 : -(h - 1);
        var maxOffset = antiDiagonal ? (w + h - 2) : (w - 1);
        var kind = antiDiagonal ? ScanKind.AntiDiagonal : ScanKind.Diagonal;

        for (var c = minOffset; c <= maxOffset; c++)
        {
            int x, y;
            if (antiDiagonal)
            {
                x = Math.Max(0, c - (h - 1));
                y = c - x;
            }
            else
            {
                x = Math.Max(0, -c);
                y = x + c;
            }

            var bestLen = 0;
            var bestStartX = 0;
            var bestStartY = 0;
            var bestEndX = 0;
            var bestEndY = 0;

            var runLen = 0;
            var runStartX = 0;
            var runStartY = 0;

            while (x >= 0 && y >= 0 && x < w && y < h)
            {
                if (!fg[x, y])
                {
                    if (runLen == 0)
                    {
                        runStartX = x;
                        runStartY = y;
                    }
                    runLen++;
                    if (runLen > bestLen)
                    {
                        bestLen = runLen;
                        bestStartX = runStartX;
                        bestStartY = runStartY;
                        bestEndX = x;
                        bestEndY = y;
                    }
                }
                else
                {
                    runLen = 0;
                }

                if (antiDiagonal)
                {
                    x++;
                    y--;
                }
                else
                {
                    x++;
                    y++;
                }
            }

            if (bestLen >= 5)
            {
                candidates.Add(new ScanCandidate(kind, bestLen, bestStartX, bestStartY, bestEndX, bestEndY, c));
            }
        }
    }

    private static void AddCornerCuts(NOcrChar nOcrChar, NikseBitmap2 bitmap, int maxNumberOfLines)
    {
        var w = nOcrChar.Width;
        var h = nOcrChar.Height;
        if (w <= 4 || h <= 4)
        {
            return;
        }

        var cuts = new (int X1, int Y1, int X2, int Y2)[]
        {
            (0, 4, 4, 0),
            (0, 2, 2, 0),
            (w, 4, w - 4, 0),
            (w, 2, w - 2, 0),
            (0, h - 4, 4, h),
            (w, h - 4, w - 4, h),
        };

        foreach (var c in cuts)
        {
            if (nOcrChar.LinesBackground.Count >= maxNumberOfLines)
            {
                return;
            }

            var line = new NOcrLine(new OcrPoint(c.X1, c.Y1), new OcrPoint(c.X2, c.Y2));
            if (IsBgCenterline(line, bitmap, nOcrChar))
            {
                nOcrChar.LinesBackground.Add(line);
            }
        }
    }

    // -------------------- Sobel + Hough --------------------

    private static byte[,] SobelEdgeMask(bool[,] fg, int w, int h)
    {
        var mag = new byte[w, h];
        for (var y = 1; y < h - 1; y++)
        {
            for (var x = 1; x < w - 1; x++)
            {
                int v(int xx, int yy) => fg[xx, yy] ? 1 : 0;

                var gx = -v(x - 1, y - 1) - 2 * v(x - 1, y) - v(x - 1, y + 1)
                        + v(x + 1, y - 1) + 2 * v(x + 1, y) + v(x + 1, y + 1);
                var gy = -v(x - 1, y - 1) - 2 * v(x, y - 1) - v(x + 1, y - 1)
                        + v(x - 1, y + 1) + 2 * v(x, y + 1) + v(x + 1, y + 1);
                var m = Math.Abs(gx) + Math.Abs(gy);
                mag[x, y] = (byte)(m > 0 ? 1 : 0);
            }
        }
        return mag;
    }

    private readonly struct HoughPeak
    {
        public readonly double Theta;
        public readonly int Rho;
        public readonly int Votes;
        public HoughPeak(double theta, int rho, int votes)
        {
            Theta = theta;
            Rho = rho;
            Votes = votes;
        }
    }

    private static List<HoughPeak> HoughPeaksOnMask(bool[,] fg, byte[,] edges, int w, int h, bool wantFg, int maxPeaks)
    {
        const int thetaSteps = 90; // 2 deg resolution over 0..180
        var diag = (int)Math.Ceiling(Math.Sqrt(w * w + h * h));
        var rhoCount = 2 * diag + 1;

        var sin = new double[thetaSteps];
        var cos = new double[thetaSteps];
        for (var t = 0; t < thetaSteps; t++)
        {
            var angle = Math.PI * t / thetaSteps;
            sin[t] = Math.Sin(angle);
            cos[t] = Math.Cos(angle);
        }

        // Each pixel of the requested polarity casts +1 vote; pixels that are also
        // edges cast a small additional weight (uses Sobel mask), nudging peaks
        // toward stroke boundaries.
        var acc = new int[thetaSteps, rhoCount];
        for (var y = 0; y < h; y++)
        {
            for (var x = 0; x < w; x++)
            {
                var pixelMatches = fg[x, y] == wantFg;
                if (!pixelMatches)
                {
                    continue;
                }

                var weight = 1 + (edges[x, y] != 0 ? 1 : 0);
                for (var t = 0; t < thetaSteps; t++)
                {
                    var rho = (int)Math.Round(x * cos[t] + y * sin[t]) + diag;
                    if (rho >= 0 && rho < rhoCount)
                    {
                        acc[t, rho] += weight;
                    }
                }
            }
        }

        // Wider non-max suppression so a single thick stroke doesn't yield several
        // near-parallel peaks. Suppression window: +/- 4 deg in theta, +/- 2 px in rho.
        const int nmsTheta = 2;
        const int nmsRho = 2;
        var peaks = new List<HoughPeak>();
        var minVotes = Math.Max(4, Math.Min(w, h) / 3);
        for (var t = 0; t < thetaSteps; t++)
        {
            for (var r = 1; r < rhoCount - 1; r++)
            {
                var v = acc[t, r];
                if (v < minVotes)
                {
                    continue;
                }

                var isPeak = true;
                for (var dt = -nmsTheta; dt <= nmsTheta && isPeak; dt++)
                {
                    for (var dr = -nmsRho; dr <= nmsRho && isPeak; dr++)
                    {
                        if (dt == 0 && dr == 0)
                        {
                            continue;
                        }

                        var tt = (t + dt + thetaSteps) % thetaSteps;
                        var rr = r + dr;
                        if (rr < 0 || rr >= rhoCount)
                        {
                            continue;
                        }

                        if (acc[tt, rr] > v)
                        {
                            isPeak = false;
                        }
                        else if (acc[tt, rr] == v && (tt < t || (tt == t && rr < r)))
                        {
                            // tie-break: only the smaller (t, r) survives
                            isPeak = false;
                        }
                    }
                }

                if (isPeak)
                {
                    peaks.Add(new HoughPeak(Math.PI * t / thetaSteps, r - diag, v));
                }
            }
        }

        peaks.Sort((a, b) => b.Votes.CompareTo(a.Votes));
        if (peaks.Count > maxPeaks)
        {
            peaks.RemoveRange(maxPeaks, peaks.Count - maxPeaks);
        }
        return peaks;
    }

    private readonly struct HoughSegment
    {
        public readonly int X1, Y1, X2, Y2;
        public readonly int Length;
        public HoughSegment(int x1, int y1, int x2, int y2, int length)
        {
            X1 = x1; Y1 = y1; X2 = x2; Y2 = y2; Length = length;
        }
    }

    private static HoughSegment? LongestRunAlongLine(HoughPeak peak, bool[,] fg, int w, int h, bool wantFg, int minRun)
    {
        var cosT = Math.Cos(peak.Theta);
        var sinT = Math.Sin(peak.Theta);
        var diag = (int)Math.Ceiling(Math.Sqrt(w * w + h * h));

        var bestLen = 0;
        var bestStartX = 0;
        var bestStartY = 0;
        var bestEndX = 0;
        var bestEndY = 0;

        var inRun = false;
        var runLen = 0;
        var runStartX = 0;
        var runStartY = 0;
        var runEndX = 0;
        var runEndY = 0;
        var lastInside = false;

        for (var t = -diag; t <= diag; t++)
        {
            var x = (int)Math.Round(peak.Rho * cosT - t * sinT);
            var y = (int)Math.Round(peak.Rho * sinT + t * cosT);
            var inside = x >= 0 && y >= 0 && x < w && y < h;

            // Drop duplicate adjacent samples (rounding can repeat the same pixel).
            if (inside && lastInside && x == runEndX && y == runEndY && inRun)
            {
                continue;
            }

            if (inside && fg[x, y] == wantFg)
            {
                if (!inRun)
                {
                    inRun = true;
                    runLen = 1;
                    runStartX = x;
                    runStartY = y;
                }
                else
                {
                    runLen++;
                }
                runEndX = x;
                runEndY = y;
            }
            else
            {
                if (inRun && runLen > bestLen)
                {
                    bestLen = runLen;
                    bestStartX = runStartX;
                    bestStartY = runStartY;
                    bestEndX = runEndX;
                    bestEndY = runEndY;
                }
                inRun = false;
                runLen = 0;
            }
            lastInside = inside;
        }

        if (inRun && runLen > bestLen)
        {
            bestLen = runLen;
            bestStartX = runStartX;
            bestStartY = runStartY;
            bestEndX = runEndX;
            bestEndY = runEndY;
        }

        if (bestLen < minRun)
        {
            return null;
        }

        return new HoughSegment(bestStartX, bestStartY, bestEndX, bestEndY, bestLen);
    }
}
