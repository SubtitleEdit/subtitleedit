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
    }

    public enum LineStatus
    {
        /// <summary>Re-timed by the aligner.</summary>
        Retimed,

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
            // can be read, so an end far from the speech end is normal. It is simply left alone.
            var end = line.EndSeconds;
            if (_options.AdjustEnd && endMeasured && Math.Abs(newEnd - line.EndSeconds) <= _options.MaxShiftSeconds)
            {
                end = newEnd;
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

    /// <summary>True when the gap after line <paramref name="index"/> is long enough to cut at.</summary>
    private static bool IsBreak(IReadOnlyList<Line> lines, int index, Options options)
        => lines[index + 1].StartSeconds - lines[index].EndSeconds > options.BreakGapSeconds;

    /// <summary>
    /// The aligner reports where speech starts and stops; a subtitle also has to stay up long
    /// enough to be read and must not collide with its neighbours. Only lines the aligner
    /// moved are touched, and a line is never extended past where it originally ended unless
    /// it would otherwise fall below the minimum duration.
    /// </summary>
    internal static void Tidy(IReadOnlyList<Line> lines, LineResult[] results, Options options, double audioSeconds)
    {
        for (var i = 0; i < results.Length; i++)
        {
            var result = results[i];
            if (result.Status != LineStatus.Retimed)
            {
                continue;
            }

            var start = result.StartSeconds;
            var end = result.EndSeconds;

            // Overlaps the user made on purpose are left alone; only ones introduced here are undone.
            if (i > 0 && lines[i].StartSeconds >= lines[i - 1].EndSeconds)
            {
                start = Math.Max(start, results[i - 1].EndSeconds + options.MinGapSeconds);
            }

            var room = double.MaxValue;
            if (i + 1 < results.Length && lines[i + 1].StartSeconds >= lines[i].EndSeconds)
            {
                room = results[i + 1].StartSeconds - options.MinGapSeconds;
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

            // Reading time may hold a line up to where it used to end, never beyond.
            var readingEnd = Math.Min(start + wanted, Math.Max(lines[i].EndSeconds, start + options.MinDurationSeconds));
            end = Math.Min(Math.Max(end, readingEnd), room);

            if (end <= start)
            {
                results[i] = new LineResult(lines[i].StartSeconds, lines[i].EndSeconds, LineStatus.ShiftTooLarge);
                continue;
            }

            var moved = Math.Abs(start - lines[i].StartSeconds) >= 0.001 || Math.Abs(end - lines[i].EndSeconds) >= 0.001;
            results[i] = new LineResult(start, end, moved ? LineStatus.Retimed : LineStatus.Unchanged);
        }
    }

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
