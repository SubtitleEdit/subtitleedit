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
        /// A re-timed line is never held up longer than this by the reading-speed rules - it
        /// may still be longer when the speech itself is. 0 means no limit.
        /// </summary>
        public double MaxDurationSeconds { get; init; }

        /// <summary>
        /// Fastest reading speed allowed. Unlike <see cref="ReadingCharsPerSecond"/> this may
        /// hold a line for longer than it used to be shown, room permitting. 0 turns it off.
        /// </summary>
        public double MaxCharsPerSecond { get; init; }

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

        /// <summary>
        /// A large move the neighbourhood could not vouch for, confirmed by speech-to-text
        /// hearing the line start where the aligner put it.
        /// </summary>
        ConfirmedBySpeech,

        /// <summary>
        /// Speech-to-text heard the line start where it already was, not where the aligner
        /// moved it. The aligner's times are offered, unticked, for the user to check.
        /// </summary>
        DisputedBySpeech,

        /// <summary>
        /// The subtitle was out of sync by more than the max shift, so it was synced first
        /// (see <see cref="RoughSync"/>), and the aligner left this line where the sync put it.
        /// </summary>
        MovedWithSync,

        /// <summary>Moved or resized by the user in the waveform.</summary>
        AdjustedByHand,
    }

    public readonly record struct Line(string Text, double StartSeconds, double EndSeconds);

    public sealed record LineResult(double StartSeconds, double EndSeconds, LineStatus Status)
    {
        /// <summary>Share of the line's words speech-to-text heard; null when it was not run.</summary>
        public double? HeardRatio { get; init; }

        /// <summary>Where speech-to-text heard the line's last word end; null when it did not.</summary>
        public double? HeardEndSeconds { get; init; }

        /// <summary>
        /// For a line moved with its neighbours: where the aligner itself put it, which
        /// speech-to-text may yet show to be right.
        /// </summary>
        public (double StartSeconds, double EndSeconds)? AlignerOwn { get; init; }

        /// <summary>
        /// Where the line stays when this move is not taken; null for where it came in. Set once
        /// the whole subtitle was synced first - a line that is not re-timed still keeps the sync.
        /// </summary>
        public (double StartSeconds, double EndSeconds)? Fallback { get; init; }
    }

    public sealed record Progress(int BatchIndex, int BatchCount, double Percent);

    /// <summary>One aligner run: <c>First..Last</c> index the alignable lines, sentinels excluded.</summary>
    internal sealed record Batch(int First, int Last, bool LeadingSentinel, bool TrailingSentinel);

    /// <summary>How long a word goes on sounding after a CTC aligner has placed its end.</summary>
    internal const double EndTailSeconds = 0.2;

    /// <summary>
    /// The furthest an end is held on for speech that goes on. A CTC aligner stretches a line's
    /// last word over the silence after it, so this is more about the next line than the speech;
    /// further than this, it is more likely speech the line does not have.
    /// </summary>
    internal const double MaxEndExtensionSeconds = 2.0;

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
    /// <param name="heardWords">
    /// A word-level transcription of the same audio, when there is one: the aligner's moves are
    /// then checked against it (see <see cref="SpeechToTextCheck"/>).
    /// </param>
    public async Task<IReadOnlyList<LineResult>> RetimeAsync(
        IReadOnlyList<Line> lines,
        IProgress<Progress>? progress = null,
        CancellationToken cancellationToken = default,
        IReadOnlyList<SpeechToTextCheck.HeardWord>? heardWords = null)
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
        if (heardWords != null)
        {
            // Words are looked for as far away as a line may move, plus room for a word's place
            // in a long line being guessed from its length.
            var evidence = SpeechToTextCheck.Match(lines, heardWords, _options.MaxShiftSeconds + 3.0);
            SpeechToTextCheck.Apply(lines, results, evidence);
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

            // An end is different. Subtitles are held after the last word so they can be read, so
            // an end after the speech is normal and is not pulled in: it travels with the start,
            // and the line keeps its duration. And a CTC aligner's end is not where the sound
            // stops but where the last letter was recognised - a tenth to half a second before
            // the word has died away - so pulling ends in to it made lines stop too soon. What
            // the aligner's end is good for is the other way round: speech that goes on past
            // where the line would end. The line is then held until it is over.
            var end = line.EndSeconds;
            if (_options.AdjustEnd)
            {
                end = line.EndSeconds + (start - line.StartSeconds);
                var speechEnd = newEnd + EndTailSeconds;
                if (endMeasured && speechEnd > end && speechEnd - end <= MaxEndExtensionSeconds)
                {
                    end = speechEnd;
                }
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
                    LineStatus.MovedWithNeighbours)
                {
                    AlignerOwn = status == LineStatus.Retimed ? (results[i].StartSeconds, results[i].EndSeconds) : null,
                };
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
            if (!IsMove(result.Status))
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

            // With "Adjust end times" off the end is not held at all - it only gives way to the
            // next line - or every line whose start moved later had its end moved along after all.
            var readingEnd = start + GetHoldSeconds(lines[i], options);
            if (options.AdjustEnd)
            {
                end = Math.Max(end, readingEnd);

                // Speech-to-text heard the last word end later: the line is still being spoken.
                if (result.HeardEndSeconds is { } heardEnd && heardEnd > end && heardEnd > start &&
                    heardEnd - end <= MaxEndExtensionSeconds)
                {
                    end = heardEnd;
                }

                // Never held up longer than the maximum duration by the rules here - only when it
                // came in longer.
                var longest = Math.Max(options.MaxDurationSeconds, lines[i].EndSeconds - lines[i].StartSeconds);
                if (options.MaxDurationSeconds > 0 && end - start > longest)
                {
                    end = start + longest;
                }
            }

            end = Math.Min(end, room);

            if (end <= start)
            {
                results[i] = result with { StartSeconds = lines[i].StartSeconds, EndSeconds = lines[i].EndSeconds, Status = LineStatus.ShiftTooLarge };
                continue;
            }

            var moved = Math.Abs(start - lines[i].StartSeconds) >= 0.001 || Math.Abs(end - lines[i].EndSeconds) >= 0.001;
            results[i] = result with { StartSeconds = start, EndSeconds = end, Status = moved ? result.Status : LineStatus.Unchanged };
        }
    }

    /// <summary>
    /// How long a line has to stay up to be read. The minimum duration and the maximum reading
    /// speed are rules, and hold a line for as long as they need. The comfortable reading speed
    /// is only a wish: it holds a line for as long as it used to be shown, never longer.
    /// </summary>
    internal static double GetHoldSeconds(Line line, Options options)
    {
        var characters = CountCharacters(line.Text);
        var floor = options.MinDurationSeconds;
        if (options.MaxCharsPerSecond > 0)
        {
            floor = Math.Max(floor, characters / options.MaxCharsPerSecond);
        }

        if (options.MaxDurationSeconds > 0)
        {
            floor = Math.Min(floor, options.MaxDurationSeconds);
        }

        var wish = options.ReadingCharsPerSecond > 0 ? characters / options.ReadingCharsPerSecond : 0;
        var originalDuration = line.EndSeconds - line.StartSeconds;
        return Math.Max(floor, Math.Min(wish, originalDuration));
    }

    /// <summary>Characters as the rest of Subtitle Edit counts them for characters per second.</summary>
    private static double CountCharacters(string? text)
        => string.IsNullOrEmpty(text) ? 0 : (double)HtmlUtil.RemoveHtmlTags(text, true).CountCharacters(true);

    /// <summary>The line has new times to offer, ticked or not.</summary>
    public static bool IsMove(LineStatus status)
        => status is LineStatus.Retimed
            or LineStatus.MovedWithNeighbours
            or LineStatus.LargeMoveUnconfirmed
            or LineStatus.ConfirmedBySpeech
            or LineStatus.DisputedBySpeech
            or LineStatus.MovedWithSync
            or LineStatus.AdjustedByHand;

    /// <summary>The new times are offered unticked, for the user to check.</summary>
    public static bool IsUnconfirmed(LineStatus status)
        => status is LineStatus.LargeMoveUnconfirmed or LineStatus.DisputedBySpeech;

    // An unconfirmed line is offered unticked, so it stays where it was unless the user takes
    // the move - and its neighbours make room for where it stays. Keeping clear of both of its
    // positions cut a neighbour short for a move that is, as often as not, wrong; when the user
    // does take it, Settle makes room then.
    private static double SettledStart(IReadOnlyList<Line> lines, LineResult[] results, int index)
        => IsMove(results[index].Status) && !IsUnconfirmed(results[index].Status)
            ? results[index].StartSeconds
            : lines[index].StartSeconds;

    private static double SettledEnd(IReadOnlyList<Line> lines, LineResult[] results, int index)
        => IsMove(results[index].Status) && !IsUnconfirmed(results[index].Status)
            ? results[index].EndSeconds
            : lines[index].EndSeconds;

    /// <summary>One line's times on the final timeline, and whether they may still be changed.</summary>
    public record struct Placement(double StartSeconds, double EndSeconds, bool CanMove);

    /// <summary>
    /// Makes the lines as they will actually be saved - each one moved or not, as the user
    /// ticked it - keep clear of one another by at least <paramref name="minGapSeconds"/>.
    /// Lines that overlapped on the way in (two speakers at once) are left overlapping.
    ///
    /// Only lines that may move are changed. An end gives way first: it marks when the line
    /// comes down, which can be any time after the speech, while the start is where the speech
    /// begins. Only when the earlier line cannot give way, or would be left with almost nothing,
    /// does the later line start later.
    /// </summary>
    public static void Settle(IReadOnlyList<Line> original, Placement[] placements, double minGapSeconds)
    {
        const double shortestSeconds = 0.1;
        for (var i = 1; i < placements.Length; i++)
        {
            if (original[i].StartSeconds < original[i - 1].EndSeconds)
            {
                continue;
            }

            var previous = placements[i - 1];
            var current = placements[i];
            var latestEnd = current.StartSeconds - minGapSeconds;
            if (previous.EndSeconds <= latestEnd + 0.0005)
            {
                continue;
            }

            if (previous.CanMove && latestEnd >= previous.StartSeconds + shortestSeconds)
            {
                placements[i - 1] = previous with { EndSeconds = latestEnd };
            }
            else if (current.CanMove)
            {
                var start = previous.EndSeconds + minGapSeconds;
                var duration = current.EndSeconds - current.StartSeconds;
                placements[i] = current with
                {
                    StartSeconds = start,
                    EndSeconds = Math.Max(current.EndSeconds, start + Math.Max(shortestSeconds, Math.Min(duration, 1.0))),
                };
            }
            else if (previous.CanMove)
            {
                placements[i - 1] = previous with { EndSeconds = Math.Max(previous.StartSeconds + 0.001, latestEnd) };
            }
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
