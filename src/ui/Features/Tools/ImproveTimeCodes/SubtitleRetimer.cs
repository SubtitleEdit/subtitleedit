using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Files.ImportPlainText;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Tools.ImproveTimeCodes;

/// <summary>
/// Tightens the time codes of a subtitle that is already roughly in sync, by running a
/// forced aligner over small batches of lines cut out round where they currently sit.
///
/// Unlike <see cref="ForcedAligner"/>, which has to find the script in the audio, this
/// starts from times that are nearly right. That allows short windows (so length is not
/// a limit, and neither is memory) and lets every result be checked against the time it
/// replaces: a line that would move further than the allowed shift keeps its old time.
///
/// A forced aligner only measures a boundary that has a spoken word on both sides of it.
/// The first cue of a window soaks up whatever speech precedes it and the last cue runs
/// on to the window edge, so a batch that was cut out of continuous dialogue is fed with
/// its neighbouring lines as sentinels - aligned, then thrown away - which turns the
/// batch's own first start and last end into measured, interior boundaries.
/// </summary>
public sealed partial class SubtitleRetimer
{
    public sealed class Options
    {
        /// <summary>Lines re-timed per aligner run, sentinels not counted.</summary>
        public int BatchLines { get; init; } = 8;

        /// <summary>A batch is closed early once it spans this much audio.</summary>
        public double MaxBatchSeconds { get; init; } = 45.0;

        /// <summary>
        /// A gap between lines longer than this ends the batch: the silence is a natural
        /// cut, and padding out into it cannot pick up a neighbour's speech.
        /// </summary>
        public double BreakGapSeconds { get; init; } = 4.0;

        /// <summary>The furthest a start or end may move; further is treated as a misfit.</summary>
        public double MaxShiftSeconds { get; init; } = 0.5;

        /// <summary>Audio kept beyond a sentinel, so a slightly early or late sentinel is not clipped.</summary>
        public double SentinelPaddingSeconds { get; init; } = 1.0;

        public bool AdjustStart { get; init; } = true;
        public bool AdjustEnd { get; init; } = true;

        /// <summary>Shortest duration a re-timed line is left with, room permitting.</summary>
        public double MinDurationSeconds { get; init; } = 1.0;

        /// <summary>
        /// Comfortable reading speed. Speech is often over before a line has been read, so a
        /// line is held for this long - though never past where it used to end. 0 turns it off.
        /// </summary>
        public double ReadingCharsPerSecond { get; init; } = 0;

        public double MinGapSeconds { get; init; } = 0.0;

        /// <summary>
        /// How far a line may move on its own. A subtitle that is out of sync is out of sync by
        /// much the same amount from one line to the next, so <see cref="MaxShiftSeconds"/> is
        /// really an allowance for that shared offset. A line that goes further than this from
        /// where its neighbours went has usually been pulled off by speech that is not in its
        /// text, and is moved along with the neighbours instead.
        /// </summary>
        public double MaxOwnShiftSeconds { get; init; } = 0.5;

        /// <summary>Lines looked at on each side when working out where the neighbours went.</summary>
        public int NeighbourLines { get; init; } = 4;

        /// <summary>Neighbours further away than this say nothing about the offset here.</summary>
        public double NeighbourSeconds { get; init; } = 45.0;

        /// <summary>The neighbours only count as agreeing when their shifts lie this close together.</summary>
        public double NeighbourSpreadSeconds { get; init; } = 0.25;
    }

    public enum LineStatus
    {
        /// <summary>Re-timed by the aligner.</summary>
        Retimed,

        /// <summary>
        /// The aligner's own answer stood apart from an otherwise consistent neighbourhood, so
        /// the line was moved by the same amount as the lines round it.
        /// </summary>
        MovedWithNeighbours,

        /// <summary>
        /// The aligner wants to move this line a long way while the lines round it stay put.
        /// That is either a line that really was mistimed or speech that is not in its text -
        /// the times are the aligner's, but it is for the user to confirm them.
        /// </summary>
        LargeMoveUnconfirmed,

        /// <summary>Aligned, but already where the aligner puts it.</summary>
        Unchanged,

        /// <summary>Nothing spoken to align (blank, music symbols, sound descriptions).</summary>
        NoSpeech,

        /// <summary>The aligner wanted to move it further than allowed, so it was left alone.</summary>
        ShiftTooLarge,

        /// <summary>The aligner failed on this line's batch.</summary>
        Failed,
    }

    public readonly record struct Line(string Text, double StartSeconds, double EndSeconds);

    public sealed record LineResult(double StartSeconds, double EndSeconds, LineStatus Status);

    public sealed record Progress(int BatchIndex, int BatchCount, double Percent);

    /// <summary>One aligner run: <c>First..Last</c> index the alignable lines, sentinels excluded.</summary>
    internal sealed record Batch(int First, int Last, bool LeadingSentinel, bool TrailingSentinel);

    private readonly ForcedAligner.IRunner _runner;
    private readonly ForcedAligner.IAudioSource _audio;
    private readonly Options _options;

    public SubtitleRetimer(ForcedAligner.IRunner runner, ForcedAligner.IAudioSource audio, Options? options = null)
    {
        _runner = runner;
        _audio = audio;
        _options = options ?? new Options();
    }

    /// <summary>
    /// Returns one result per input line, in input order. Lines must be sorted by start time.
    /// Throws <see cref="ForcedAlignerException"/> only when every batch failed - a single bad
    /// batch just leaves its lines as they were.
    /// </summary>
    public async Task<IReadOnlyList<LineResult>> RetimeAsync(
        IReadOnlyList<Line> lines,
        IProgress<Progress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(lines);

        var results = lines
            .Select(l => new LineResult(l.StartSeconds, l.EndSeconds, LineStatus.NoSpeech))
            .ToArray();

        var spoken = lines.Select(l => GetSpokenText(l.Text)).ToList();
        var alignable = new List<int>();
        for (var i = 0; i < lines.Count; i++)
        {
            if (spoken[i].Length > 0 && lines[i].EndSeconds > lines[i].StartSeconds)
            {
                alignable.Add(i);
            }
        }

        if (alignable.Count == 0 || _audio.TotalSeconds <= 0)
        {
            return results;
        }

        var batches = PlanBatches(alignable.Select(i => lines[i]).ToList(), _options);
        Exception? lastError = null;
        var failedBatches = 0;

        for (var b = 0; b < batches.Count; b++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var batch = batches[b];

            try
            {
                await RetimeBatchAsync(batch, lines, spoken, alignable, results, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                lastError = exception;
                failedBatches++;
                for (var k = batch.First; k <= batch.Last; k++)
                {
                    var index = alignable[k];
                    results[index] = new LineResult(lines[index].StartSeconds, lines[index].EndSeconds, LineStatus.Failed);
                }
            }

            progress?.Report(new Progress(b + 1, batches.Count, (b + 1) / (double)batches.Count * 100.0));
        }

        if (failedBatches == batches.Count && lastError != null)
        {
            throw lastError as ForcedAlignerException
                  ?? new ForcedAlignerException(lastError.Message, lastError.ToString());
        }

        FollowNeighbours(lines, results, _options);
        Tidy(lines, results, _options, _audio.TotalSeconds);
        return results;
    }

    private async Task RetimeBatchAsync(
        Batch batch,
        IReadOnlyList<Line> lines,
        IReadOnlyList<string> spoken,
        IReadOnlyList<int> alignable,
        LineResult[] results,
        CancellationToken cancellationToken)
    {
        var firstFed = batch.LeadingSentinel ? batch.First - 1 : batch.First;
        var lastFed = batch.TrailingSentinel ? batch.Last + 1 : batch.Last;

        // Beside a sentinel the padding only has to protect the sentinel; at a natural
        // break it is the whole search range, reaching out into the silence.
        var openPadding = _options.MaxShiftSeconds + 0.5;
        var windowStart = lines[alignable[firstFed]].StartSeconds -
                          (batch.LeadingSentinel ? _options.SentinelPaddingSeconds : openPadding);
        var windowEnd = lines[alignable[lastFed]].EndSeconds +
                        (batch.TrailingSentinel ? _options.SentinelPaddingSeconds : openPadding);

        // Never pad into the speech of a line that is not part of this run.
        if (firstFed > 0)
        {
            windowStart = Math.Max(windowStart, lines[alignable[firstFed - 1]].EndSeconds);
        }

        if (lastFed + 1 < alignable.Count)
        {
            windowEnd = Math.Min(windowEnd, lines[alignable[lastFed + 1]].StartSeconds);
        }

        windowStart = Math.Max(0, windowStart);
        windowEnd = Math.Min(_audio.TotalSeconds, windowEnd);
        if (windowEnd - windowStart < 0.5)
        {
            throw new ForcedAlignerException("The lines lie outside the audio.");
        }

        var fed = new List<string>();
        for (var k = firstFed; k <= lastFed; k++)
        {
            fed.Add(spoken[alignable[k]]);
        }

        var audioFileName = await _audio
            .ExtractWindowAsync(windowStart, windowEnd - windowStart, cancellationToken)
            .ConfigureAwait(false);
        var textFileName = Path.ChangeExtension(audioFileName, ".txt");
        await File.WriteAllLinesAsync(textFileName, fed, cancellationToken).ConfigureAwait(false);

        var srt = await _runner.AlignAsync(audioFileName, textFileName, cancellationToken).ConfigureAwait(false);
        var cues = ForcedAligner.ParseCues(srt);
        if (cues.Count != fed.Count)
        {
            // Cue N is line N only while the aligner returns one cue per line.
            throw new ForcedAlignerException($"The forced aligner returned {cues.Count} cues for {fed.Count} lines.");
        }

        const double edgeSeconds = 0.25;
        var windowLength = windowEnd - windowStart;

        for (var k = batch.First; k <= batch.Last; k++)
        {
            var index = alignable[k];
            var cue = cues[k - firstFed];
            var line = lines[index];

            // Without a sentinel the outermost boundaries are only trustworthy when they
            // come to rest inside the window; pinned to its edge they were never measured.
            var startMeasured = k > firstFed || cue.StartSeconds > edgeSeconds;
            var endMeasured = k < lastFed || cue.EndSeconds < windowLength - edgeSeconds;

            var newStart = windowStart + cue.StartSeconds;
            var newEnd = windowStart + cue.EndSeconds;
            // A start that wants to move too far means the aligner did not find this line where
            // the subtitle says it is - its text may not be what is said - so nothing of it is used.
            var start = line.StartSeconds;
            if (_options.AdjustStart && startMeasured)
            {
                if (Math.Abs(newStart - line.StartSeconds) > _options.MaxShiftSeconds)
                {
                    results[index] = new LineResult(line.StartSeconds, line.EndSeconds, LineStatus.ShiftTooLarge);
                    continue;
                }

                start = newStart;
            }

            // An end is different: subtitles are routinely held long after the last word so they
            // can be read, so an end far from the speech end is normal. Such an end is not pulled
            // in to the speech - it travels with the start, and the line keeps its duration.
            var end = line.EndSeconds;
            if (_options.AdjustEnd)
            {
                end = endMeasured && Math.Abs(newEnd - line.EndSeconds) <= _options.MaxShiftSeconds
                    ? newEnd
                    : line.EndSeconds + (start - line.StartSeconds);
            }

            if (end <= start)
            {
                results[index] = new LineResult(line.StartSeconds, line.EndSeconds, LineStatus.ShiftTooLarge);
                continue;
            }

            results[index] = new LineResult(start, end, LineStatus.Retimed);
        }
    }

    /// <summary>
    /// Cuts the alignable lines into aligner runs. A run ends at a long gap, or when it is
    /// full - and a run that ended only because it was full borrows the line on the far
    /// side of the cut as a sentinel.
    /// </summary>
    internal static List<Batch> PlanBatches(IReadOnlyList<Line> lines, Options options)
    {
        var batches = new List<Batch>();
        var first = 0;
        while (first < lines.Count)
        {
            var last = first;
            while (last + 1 < lines.Count &&
                   last + 1 - first < options.BatchLines &&
                   lines[last + 1].EndSeconds - lines[first].StartSeconds <= options.MaxBatchSeconds &&
                   !IsBreak(lines, last, options))
            {
                last++;
            }

            var leading = first > 0 && !IsBreak(lines, first - 1, options);
            var trailing = last + 1 < lines.Count && !IsBreak(lines, last, options);
            batches.Add(new Batch(first, last, leading, trailing));
            first = last + 1;
        }

        return batches;
    }

    /// <summary>
    /// Checks every large move against the lines round it.
    ///
    /// A forced aligner places exactly the words it is given. Where the text is not quite what
    /// is said - a condensed line whose speech opens with words the subtitle drops, a stray
    /// "you know" from the line before - it places them confidently in the wrong spot, and the
    /// error is the same on every run, so re-aligning cannot expose it. What can is the
    /// neighbourhood: when the surrounding lines all moved by about the same amount, that is
    /// the offset of the subtitle, and a line that went somewhere else by more than
    /// <see cref="Options.MaxOwnShiftSeconds"/> is given that offset instead - as is a line
    /// that was refused for wanting to go too far. When the neighbours stayed where they were,
    /// the lone large move is kept as an unconfirmed proposal. Where the neighbours do not agree
    /// among themselves there is nothing to measure against, and the aligner's answers stand.
    /// </summary>
    internal static void FollowNeighbours(IReadOnlyList<Line> lines, LineResult[] results, Options options)
    {
        if (!options.AdjustStart)
        {
            return;
        }

        // Taken before anything is changed, so one corrected line never vouches for the next.
        var shifts = new double?[results.Length];
        for (var i = 0; i < results.Length; i++)
        {
            if (results[i].Status == LineStatus.Retimed)
            {
                shifts[i] = results[i].StartSeconds - lines[i].StartSeconds;
            }
        }

        for (var i = 0; i < results.Length; i++)
        {
            var status = results[i].Status;
            if (status != LineStatus.Retimed && status != LineStatus.ShiftTooLarge)
            {
                continue;
            }

            var neighbours = new List<double>();
            for (var step = -1; step <= 1; step += 2)
            {
                var found = 0;
                for (var k = i + step; k >= 0 && k < results.Length && found < options.NeighbourLines; k += step)
                {
                    if (Math.Abs(lines[k].StartSeconds - lines[i].StartSeconds) > options.NeighbourSeconds)
                    {
                        break;
                    }

                    if (shifts[k] is { } shift)
                    {
                        neighbours.Add(shift);
                        found++;
                    }
                }
            }

            if (neighbours.Count < 3)
            {
                continue;
            }

            var offset = Median(neighbours);
            var spread = Median(neighbours.Select(n => Math.Abs(n - offset)).ToList());
            if (spread > options.NeighbourSpreadSeconds)
            {
                continue;
            }

            if (shifts[i] is { } own && Math.Abs(own - offset) <= options.MaxOwnShiftSeconds)
            {
                continue;
            }

            // An offset only exists when it stands clear of the scatter it was measured in.
            var isOffset = Math.Abs(offset) > Math.Max(0.15, spread * 3.0);
            if (isOffset)
            {
                // "Adjust end times" off: the end stays, only the start takes the offset
                results[i] = new LineResult(
                    Math.Max(0, lines[i].StartSeconds + offset),
                    options.AdjustEnd ? lines[i].EndSeconds + offset : lines[i].EndSeconds,
                    LineStatus.MovedWithNeighbours);
            }
            else if (status == LineStatus.Retimed)
            {
                // The neighbours are where they were, and this line alone wants to go far. Nothing
                // here can tell a mistimed line from one with unsubtitled speech beside it, so
                // the aligner's answer is kept as a proposal rather than applied or thrown away.
                results[i] = results[i] with { Status = LineStatus.LargeMoveUnconfirmed };
            }
        }
    }

    private static double Median(List<double> values)
    {
        var sorted = values.OrderBy(v => v).ToList();
        var middle = sorted.Count / 2;
        return sorted.Count % 2 == 1 ? sorted[middle] : (sorted[middle - 1] + sorted[middle]) / 2.0;
    }

    /// <summary>True when the gap after line <paramref name="index"/> is long enough to cut at.</summary>
    private static bool IsBreak(IReadOnlyList<Line> lines, int index, Options options)
        => lines[index + 1].StartSeconds - lines[index].EndSeconds > options.BreakGapSeconds;

    /// <summary>
    /// The aligner reports where speech starts and stops; a subtitle also has to stay up long
    /// enough to be read and must not collide with its neighbours. Only lines that were moved
    /// are touched, and a line is never shown for longer than it originally was unless it
    /// would otherwise fall below the minimum duration.
    /// </summary>
    internal static void Tidy(IReadOnlyList<Line> lines, LineResult[] results, Options options, double audioSeconds)
    {
        for (var i = 0; i < results.Length; i++)
        {
            var result = results[i];
            if (result.Status is not (LineStatus.Retimed or LineStatus.MovedWithNeighbours or LineStatus.LargeMoveUnconfirmed))
            {
                continue;
            }

            var start = result.StartSeconds;
            var end = result.EndSeconds;

            // Overlaps the user made on purpose are left alone; only ones introduced here are undone.
            if (i > 0 && lines[i].StartSeconds >= lines[i - 1].EndSeconds)
            {
                start = Math.Max(start, SettledEnd(lines, results, i - 1) + options.MinGapSeconds);
            }

            var room = double.MaxValue;
            if (i + 1 < results.Length && lines[i + 1].StartSeconds >= lines[i].EndSeconds)
            {
                room = SettledStart(lines, results, i + 1) - options.MinGapSeconds;
            }

            if (audioSeconds > 0)
            {
                room = Math.Min(room, audioSeconds);
            }

            var wanted = options.MinDurationSeconds;
            if (options.ReadingCharsPerSecond > 0)
            {
                wanted = Math.Max(wanted, GetSpokenText(lines[i].Text).Length / options.ReadingCharsPerSecond);
            }

            // Reading time may hold a line for as long as it used to be shown, never longer. With
            // "Adjust end times" off the end is not held at all - it only gives way to the next
            // line - or every line whose start moved later had its end moved along after all.
            var originalDuration = lines[i].EndSeconds - lines[i].StartSeconds;
            var readingEnd = start + Math.Min(wanted, Math.Max(originalDuration, options.MinDurationSeconds));
            end = Math.Min(options.AdjustEnd ? Math.Max(end, readingEnd) : end, room);

            if (end <= start)
            {
                results[i] = new LineResult(lines[i].StartSeconds, lines[i].EndSeconds, LineStatus.ShiftTooLarge);
                continue;
            }

            var moved = Math.Abs(start - lines[i].StartSeconds) >= 0.001 || Math.Abs(end - lines[i].EndSeconds) >= 0.001;
            results[i] = new LineResult(start, end, moved ? result.Status : LineStatus.Unchanged);
        }
    }

    // An unconfirmed line may end up at either of its two positions, so its neighbours keep
    // clear of both.
    private static double SettledStart(IReadOnlyList<Line> lines, LineResult[] results, int index)
        => results[index].Status == LineStatus.LargeMoveUnconfirmed
            ? Math.Min(results[index].StartSeconds, lines[index].StartSeconds)
            : results[index].StartSeconds;

    private static double SettledEnd(IReadOnlyList<Line> lines, LineResult[] results, int index)
        => results[index].Status == LineStatus.LargeMoveUnconfirmed
            ? Math.Max(results[index].EndSeconds, lines[index].EndSeconds)
            : results[index].EndSeconds;

    /// <summary>
    /// What is actually said in a line: no tags, no sound descriptions, no music symbols, no
    /// dialogue dashes. Empty when nothing is left to align.
    /// </summary>
    internal static string GetSpokenText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var s = HtmlUtil.RemoveHtmlTags(text, true);
        s = SoundDescriptionRegex().Replace(s, " ");

        var sb = new StringBuilder(s.Length);
        foreach (var rawLine in s.SplitToLines())
        {
            var line = rawLine.Trim().TrimStart('-', '‐', '–', '—').Trim();
            if (line.Length > 0)
            {
                sb.Append(line).Append(' ');
            }
        }

        s = sb.ToString().Replace("♪", " ").Replace("♫", " ").Replace("#", " ");
        s = WhitespaceRegex().Replace(s, " ").Trim();
        return s.Any(char.IsLetterOrDigit) ? s : string.Empty;
    }

    [GeneratedRegex(@"\[[^\]]*\]|\([^\)]*\)")]
    private static partial Regex SoundDescriptionRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}
