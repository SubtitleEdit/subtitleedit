using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Tools.ImproveTimeCodes;

/// <summary>
/// Brings a subtitle that is seconds or minutes out of sync close enough for the forced
/// aligner, by finding its words in a transcription of the audio.
///
/// A subtitle can be out of sync in three ways: by a constant offset, by a drift (a frame
/// rate mismatch - the offset grows along the film), and by jumps (a scene cut from one
/// version and not the other). All three are an offset that changes slowly or in steps, so
/// that is what is measured: distinctive words and word pairs that occur in both the
/// subtitle and the transcription are paired up, kept only where they agree on the order
/// (a monotonic chain), and every line that has such pairs gives one offset. Offsets that
/// disagree with the lines round them are dropped as mismatches, and the rest are smoothed
/// with a running median - which follows a drift and keeps a jump sharp. Lines between
/// measured ones get an offset interpolated from them.
///
/// Matching text rather than speech and silence is what keeps this from piling lines up at
/// the start of the film when the offset is large, as purely acoustic sync can.
/// </summary>
public static class RoughSync
{
    public sealed record Result(double[] Offsets, int AnchorLines, int SpokenLines)
    {
        /// <summary>The largest correction applied to any line.</summary>
        public double MaxAbsOffset => Offsets.Length == 0 ? 0 : Offsets.Max(Math.Abs);
    }

    /// <summary>Only words at least this long can pair up on their own.</summary>
    private const int MinWordLength = 6;

    /// <summary>A word pair must have at least this many letters between the two words.</summary>
    private const int MinPairLength = 6;

    /// <summary>A word or pair heard more often than this is too common to tell one place from another.</summary>
    private const int MaxOccurrences = 3;

    /// <summary>Measured lines each side that the running median looks at.</summary>
    private const int Neighbours = 4;

    /// <summary>A line whose offset is this far from its neighbours' median is a mismatch.</summary>
    private const double OutlierSeconds = 1.0;

    /// <summary>
    /// Works out how far each line has to move. Returns null when too little of the subtitle
    /// was found in the transcription to say - a subtitle in another language, or for another
    /// cut of the film.
    /// </summary>
    public static Result? Measure(IReadOnlyList<SubtitleRetimer.Line> lines, IReadOnlyList<SpeechToTextCheck.HeardWord> heardWords)
    {
        var heard = heardWords
            .Select(w => (Word: SpeechToTextCheck.Normalize(w.Word), w.StartSeconds))
            .Where(w => w.Word.Length > 0)
            .OrderBy(w => w.StartSeconds)
            .ToList();

        var tokens = new List<(string Word, int Line, double Seconds)>();
        var spokenLines = 0;
        for (var i = 0; i < lines.Count; i++)
        {
            var words = SubtitleRetimer.GetSpokenText(lines[i].Text)
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(SpeechToTextCheck.Normalize)
                .Where(w => w.Length > 0)
                .ToList();
            if (words.Count == 0)
            {
                continue;
            }

            spokenLines++;
            var totalChars = words.Sum(w => w.Length + 1);
            var duration = Math.Max(0, lines[i].EndSeconds - lines[i].StartSeconds);
            var chars = 0;
            foreach (var word in words)
            {
                tokens.Add((word, i, lines[i].StartSeconds + (duration * chars / totalChars)));
                chars += word.Length + 1;
            }
        }

        var chain = FindAnchors(tokens, heard);

        // One offset per line: the median of its anchors, which evens out where in the line
        // each word was guessed to be.
        var perLine = chain
            .GroupBy(a => tokens[a.Token].Line)
            .Select(g => (Line: g.Key, Offset: Median(g.Select(a => heard[a.Heard].StartSeconds - tokens[a.Token].Seconds).ToList())))
            .OrderBy(a => a.Line)
            .ToList();

        // Mismatches first, against the unsmoothed neighbourhood - then the smoothing.
        // (A line just after a jump is outvoted too; the next ones, past the jump, are not.)
        var kept = perLine.Where((a, k) => Math.Abs(a.Offset - OthersMedian(perLine, k)) <= OutlierSeconds).ToList();
        var minAnchors = Math.Max(8, spokenLines / 10);
        if (kept.Count < minAnchors)
        {
            return null;
        }

        var smoothed = kept.Select((a, k) => (a.Line, Offset: NeighbourMedian(kept, k))).ToList();
        return new Result(Interpolate(lines, smoothed), kept.Count, spokenLines);
    }

    /// <summary>
    /// Pairs of (subtitle word, heard word) that are distinctive enough to trust and agree on
    /// the order: rare word pairs, and rare long words, chained by heaviest increasing
    /// subsequence so a single coincidence cannot pull the chain off course.
    /// </summary>
    private static List<(int Token, int Heard)> FindAnchors(
        List<(string Word, int Line, double Seconds)> tokens,
        List<(string Word, double StartSeconds)> heard)
    {
        var heardWords = new Dictionary<string, List<int>>(StringComparer.Ordinal);
        var heardPairs = new Dictionary<string, List<int>>(StringComparer.Ordinal);
        for (var h = 0; h < heard.Count; h++)
        {
            Add(heardWords, heard[h].Word, h);
            if (h + 1 < heard.Count)
            {
                Add(heardPairs, heard[h].Word + " " + heard[h + 1].Word, h);
            }
        }

        var subtitleWords = tokens.GroupBy(t => t.Word).ToDictionary(g => g.Key, g => g.Count(), StringComparer.Ordinal);

        var candidates = new List<(int Token, int Heard, double Weight)>();
        for (var t = 0; t < tokens.Count; t++)
        {
            var found = new List<(int Heard, double Weight)>();
            if (t + 1 < tokens.Count && tokens[t].Word.Length + tokens[t + 1].Word.Length >= MinPairLength &&
                heardPairs.TryGetValue(tokens[t].Word + " " + tokens[t + 1].Word, out var pairs) && pairs.Count <= MaxOccurrences)
            {
                found.AddRange(pairs.Select(h => (h, 2.0)));
            }

            if (tokens[t].Word.Length >= MinWordLength && subtitleWords[tokens[t].Word] <= MaxOccurrences &&
                heardWords.TryGetValue(tokens[t].Word, out var singles) && singles.Count <= MaxOccurrences)
            {
                found.AddRange(singles.Where(h => found.All(f => f.Heard != h)).Select(h => (h, 1.0)));
            }

            // Descending heard index within a token, as the chain requires.
            candidates.AddRange(found.OrderByDescending(f => f.Heard).Select(f => (t, f.Heard, f.Weight)));
        }

        return SpeechToTextCheck.HeaviestIncreasingChain(candidates, heard.Count);
    }

    /// <summary>
    /// The median offset of the measured lines nearest <paramref name="index"/>, itself left out
    /// - lopsided at the ends, but a wrong match at the very start must still be outvoted.
    /// </summary>
    private static double OthersMedian(List<(int Line, double Offset)> anchors, int index)
    {
        var from = Math.Max(0, Math.Min(index - Neighbours, anchors.Count - 1 - (2 * Neighbours)));
        var to = Math.Min(anchors.Count - 1, from + (2 * Neighbours));
        var others = Enumerable.Range(from, to - from + 1).Where(k => k != index).Select(k => anchors[k].Offset).ToList();
        return others.Count == 0 ? anchors[index].Offset : Median(others);
    }

    /// <summary>
    /// The median offset of the measured lines round <paramref name="index"/>, itself included.
    /// The window is as wide on both sides, narrowing towards either end - one reaching further
    /// ahead than back would pull the first lines of a drifting subtitle towards later offsets.
    /// </summary>
    private static double NeighbourMedian(List<(int Line, double Offset)> anchors, int index)
    {
        var reach = Math.Min(Neighbours, Math.Min(index, anchors.Count - 1 - index));
        return Median(anchors.Skip(index - reach).Take((2 * reach) + 1).Select(a => a.Offset).ToList());
    }

    /// <summary>
    /// An offset for every line: measured lines keep theirs, lines between two measured ones
    /// get one in proportion to where they fall in time, and lines before the first or after
    /// the last take the nearest one.
    /// </summary>
    private static double[] Interpolate(IReadOnlyList<SubtitleRetimer.Line> lines, List<(int Line, double Offset)> anchors)
    {
        var offsets = new double[lines.Count];
        var next = 0;
        for (var i = 0; i < lines.Count; i++)
        {
            while (next < anchors.Count && anchors[next].Line < i)
            {
                next++;
            }

            if (next < anchors.Count && anchors[next].Line == i)
            {
                offsets[i] = anchors[next].Offset;
            }
            else if (next == 0)
            {
                offsets[i] = anchors[0].Offset;
            }
            else if (next == anchors.Count)
            {
                offsets[i] = anchors[^1].Offset;
            }
            else
            {
                var before = anchors[next - 1];
                var after = anchors[next];
                var t0 = lines[before.Line].StartSeconds;
                var t1 = lines[after.Line].StartSeconds;
                var f = t1 > t0 ? Math.Clamp((lines[i].StartSeconds - t0) / (t1 - t0), 0, 1) : 0.5;
                offsets[i] = before.Offset + ((after.Offset - before.Offset) * f);
            }

            // Never before the start of the audio.
            offsets[i] = Math.Max(offsets[i], -lines[i].StartSeconds);
        }

        return offsets;
    }

    /// <summary>
    /// The lines moved by the measured offsets. Neighbouring lines can get slightly different
    /// offsets (a drift, the smoothing), and a line moved further than the next would run into
    /// it - so where two lines did not overlap before, the earlier one ends in time to keep the
    /// gap there was between them. Overlaps made here would otherwise pass for overlaps the
    /// user made on purpose, and be kept.
    /// </summary>
    public static List<SubtitleRetimer.Line> Apply(IReadOnlyList<SubtitleRetimer.Line> lines, Result sync)
    {
        var synced = lines.Select((l, i) => l with { StartSeconds = l.StartSeconds + sync.Offsets[i], EndSeconds = l.EndSeconds + sync.Offsets[i] }).ToList();
        for (var i = 0; i + 1 < synced.Count; i++)
        {
            var gap = lines[i + 1].StartSeconds - lines[i].EndSeconds;
            if (gap < 0)
            {
                continue;
            }

            var latestEnd = synced[i + 1].StartSeconds - gap;
            if (synced[i].EndSeconds > latestEnd)
            {
                synced[i] = synced[i] with { EndSeconds = Math.Max(latestEnd, synced[i].StartSeconds + 0.1) };
            }
        }

        return synced;
    }

    /// <summary>
    /// Folds the sync into the aligner's results. The aligner worked on the synced lines, so a
    /// line it left alone - already in place, no speech, refused, failed - still has to take
    /// the sync's move, or it would fall back to where it was before. Such lines are marked
    /// <see cref="SubtitleRetimer.LineStatus.MovedWithSync"/>. A line the aligner did move
    /// falls back to its synced place, not its old one, when the user does not take the move.
    /// </summary>
    public static void Merge(IReadOnlyList<SubtitleRetimer.Line> original, IReadOnlyList<SubtitleRetimer.Line> synced, SubtitleRetimer.LineResult[] results)
    {
        for (var i = 0; i < results.Length; i++)
        {
            if (SubtitleRetimer.IsMove(results[i].Status))
            {
                results[i] = results[i] with { Fallback = (synced[i].StartSeconds, synced[i].EndSeconds) };
                continue;
            }

            var moved = Math.Abs(synced[i].StartSeconds - original[i].StartSeconds) >= 0.001;
            if (moved)
            {
                results[i] = results[i] with
                {
                    StartSeconds = synced[i].StartSeconds,
                    EndSeconds = synced[i].EndSeconds,
                    Status = SubtitleRetimer.LineStatus.MovedWithSync,
                };
            }
        }
    }

    private static void Add(Dictionary<string, List<int>> index, string key, int position)
    {
        if (!index.TryGetValue(key, out var positions))
        {
            positions = new List<int>();
            index[key] = positions;
        }

        positions.Add(position);
    }

    private static double Median(List<double> values)
    {
        values.Sort();
        var middle = values.Count / 2;
        return values.Count % 2 == 1 ? values[middle] : (values[middle - 1] + values[middle]) / 2.0;
    }
}
