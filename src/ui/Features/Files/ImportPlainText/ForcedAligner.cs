using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.OpenAiCompatible;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Files.ImportPlainText;

/// <summary>
/// Aligns a script against audio with a real forced aligner, one bounded window at a
/// time, so length is not a limit. See <see cref="ForcedAlignPlanner"/> for why the work
/// is chunked rather than handed to the aligner in one pass.
/// </summary>
public sealed class ForcedAligner
{
    /// <summary>Runs the aligner over one window and returns its SRT output.</summary>
    public interface IRunner
    {
        Task<string> AlignAsync(string audioFileName, string textFileName, CancellationToken cancellationToken);
    }

    /// <summary>Cuts a window out of the extracted audio, and reports how long that audio is.</summary>
    public interface IAudioSource
    {
        double TotalSeconds { get; }

        Task<IReadOnlyList<OpenAiSttChunker.SilenceInterval>> DetectSilenceAsync(CancellationToken cancellationToken);

        Task<string> ExtractWindowAsync(double startSeconds, double durationSeconds, CancellationToken cancellationToken);
    }

    /// <param name="Percent">
    /// Overall completion, 0-100. The two passes share the bar: aligning fills the first
    /// 70%, refining the rest, roughly matching how long each takes.
    /// </param>
    public sealed record Progress(int WindowIndex, int WindowCount, int LinesAligned, int LinesTotal, double Percent);

    public sealed record Result(int TotalLines, int AlignedLines)
    {
        public int UnalignedLines => TotalLines - AlignedLines;
    }

    private readonly IRunner _runner;
    private readonly IAudioSource _audio;
    private readonly ForcedAlignPlanner.Options _options;

    public ForcedAligner(IRunner runner, IAudioSource audio, ForcedAlignPlanner.Options? options = null)
    {
        _runner = runner;
        _audio = audio;
        _options = options ?? new ForcedAlignPlanner.Options();
    }

    public async Task<Result> AlignAsync(
        IList<SubtitleLineViewModel> lines,
        IProgress<Progress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(lines);

        var allTexts = lines
            .Select(l => HtmlUtil.RemoveHtmlTags(l.Text ?? string.Empty, true).Replace('\r', ' ').Replace('\n', ' ').Trim())
            .ToList();

        // Only non-blank lines are sent to the aligner, and we remember where each one
        // came from. crispasr silently drops blank and whitespace-only rows from its
        // --text-file, so feeding them and then matching cue N to line N positionally
        // shifted every following line by one, cumulatively, for the rest of the file.
        var alignable = new List<int>(allTexts.Count);
        for (var i = 0; i < allTexts.Count; i++)
        {
            if (!string.IsNullOrWhiteSpace(allTexts[i]))
            {
                alignable.Add(i);
            }
        }

        if (alignable.Count == 0 || _audio.TotalSeconds <= 0)
        {
            return new Result(allTexts.Count, 0);
        }

        var texts = alignable.Select(i => allTexts[i]).ToList();

        var silences = await DetectSilenceSafelyAsync(cancellationToken).ConfigureAwait(false);

        var optimalCps = Se.Settings.General.SubtitleOptimalCharactersPerSeconds;
        var readingCps = optimalCps > 0 ? optimalCps : 15.0;

        var aligned = new List<(double Start, double End)>(texts.Count);
        var estimated = new HashSet<int>();
        var cursor = 0;
        var position = 0.0;
        var windowIndex = 0;
        var estimatedChunks = Math.Max(1, (int)Math.Ceiling(texts.Count / (double)_options.LinesPerChunk));

        while (cursor < texts.Count && position < _audio.TotalSeconds - 0.25)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var windowEnd = SnapWindowEnd(position + _options.WindowSeconds, silences);
            var windowLength = Math.Min(windowEnd, _audio.TotalSeconds) - position;
            if (windowLength <= 0)
            {
                break;
            }

            var remaining = texts.Skip(cursor).ToList();
            var take = ForcedAlignPlanner.ChunkSize(remaining, windowLength, readingCps, _options);
            if (take == 0)
            {
                break;
            }

            var fed = remaining.Take(take).ToList();
            var cues = await AlignWindowAsync(position, windowLength, fed, cancellationToken).ConfigureAwait(false);
            if (cues.Count != fed.Count)
            {
                // Cue N is line N only while the aligner returns one cue per line. Anything
                // else and the mapping is guesswork, so step on rather than misplace lines.
                position += _options.SkipSeconds;
                windowIndex++;
                continue;
            }

            var readingSeconds = fed.Select(t => t.Length / readingCps).ToList();
            var accept = ForcedAlignPlanner.AcceptChunk(cues, readingSeconds);

            // The last cue accepted has no following word to stop at, so its end runs on;
            // leave that line for the next chunk unless this chunk holds the rest of the
            // script, in which case there is no next chunk to place it and its length is
            // clamped to reading time anyway.
            var isFinal = cursor + take >= texts.Count;
            if (!isFinal && accept > 1)
            {
                accept--;
            }

            if (accept == 0)
            {
                position += _options.SkipSeconds;
                windowIndex++;
                continue;
            }

            for (var i = 0; i < accept; i++)
            {
                var startSeconds = position + cues[i].StartSeconds;
                var endSeconds = position + cues[i].EndSeconds;

                // The first cue absorbs all the leading audio the script says nothing about,
                // so its start is meaningless while its end is sound. Take the end and work
                // backwards. Later cues in the chunk are bounded on both sides and exact.
                if (i == 0 && endSeconds - startSeconds > readingSeconds[0] * 1.5)
                {
                    startSeconds = Math.Max(position, endSeconds - readingSeconds[0]);
                }

                // A cue with no following word runs on to the end of the window, and the
                // audio cursor follows the last cue's end - so an unclamped end drives the
                // cursor to the end of the file and starves whatever script is still left.
                var cappedEnd = Math.Max(startSeconds + 0.05, Math.Min(endSeconds, startSeconds + (readingSeconds[i] * 2.0)));
                aligned.Add((startSeconds, cappedEnd));
            }

            cursor += accept;
            windowIndex++;

            var nextPosition = aligned[^1].End;
            if (nextPosition <= position + 0.25)
            {
                nextPosition = position + _options.SkipSeconds;
            }

            position = nextPosition;
            progress?.Report(new Progress(
                windowIndex,
                Math.Max(windowIndex, estimatedChunks),
                cursor,
                texts.Count,
                texts.Count > 0 ? cursor / (double)texts.Count * 70.0 : 0));
        }

        // The main loop stops when the audio cursor reaches the end, which can leave the
        // last few lines untimed - the cursor is driven by where cues land, so it can run
        // out slightly ahead of the script. Give whatever is left one pass over the closing
        // stretch of audio rather than shipping lines with no time codes at all.
        if (cursor < texts.Count)
        {
            // The closing lines are the one place the normal loop cannot give the aligner
            // slack: the window can only run forward, and at the end of a file there is no
            // forward left. Aligning them against just the audio that remains is the
            // "filled window" case again, and it placed them ~15 s late.
            //
            // So the last chunk takes its slack backwards instead. A few already-placed
            // lines are re-aligned along with the unplaced ones, over a window reaching
            // back from the end of the recording - the aligner gets room on the leading
            // side, which is the side it tolerates, and the overlap keeps the seam honest.
            // One line of overlap only. Reaching further back drags that speech into the
            // window too, which fills it up and removes the very slack this needs - the
            // window has to be long relative to the text in it, not just long.
            var rejoin = Math.Max(0, cursor - 1);
            var fed = texts.Skip(rejoin).ToList();

            var needed = fed.Sum(t => t.Length) / readingCps;
            var tailStart = Math.Max(0, _audio.TotalSeconds - Math.Max(needed * 3.0, _options.WindowSeconds / 2.0));

            // Never reach back past what is already settled.
            if (rejoin > 0 && rejoin - 1 < aligned.Count)
            {
                tailStart = Math.Max(tailStart, aligned[rejoin - 1].End);
            }

            var tailLength = _audio.TotalSeconds - tailStart;

            if (tailLength > 0.5)
            {
                try
                {
                    var cues = await AlignWindowAsync(tailStart, tailLength, fed, cancellationToken)
                        .ConfigureAwait(false);

                    // No overlap to check against, so sanity-check the shape instead:
                    // one cue per line, in order, and inside the window.
                    var sane = cues.Count == fed.Count;
                    for (var k = 1; sane && k < cues.Count; k++)
                    {
                        sane = cues[k].StartSeconds >= cues[k - 1].StartSeconds;
                    }

                    if (sane)
                    {
                        aligned.RemoveRange(rejoin, aligned.Count - rejoin);

                        for (var k = 0; k < cues.Count; k++)
                        {
                            var cueStart = tailStart + cues[k].StartSeconds;
                            var cueEnd = tailStart + cues[k].EndSeconds;
                            var reading = texts[rejoin + k].Length / readingCps;

                            // Same leading-absorb correction the main loop applies: this
                            // window deliberately starts well before the text it holds.
                            if (k == 0 && cueEnd - cueStart > reading * 1.5)
                            {
                                cueStart = Math.Max(tailStart, cueEnd - reading);
                            }

                            aligned.Add((cueStart, Math.Max(cueStart + 0.05, cueEnd)));
                        }

                        cursor = texts.Count;
                    }
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch
                {
                    // Fall through to the deterministic layout below.
                }
            }

            // Whatever is still unplaced gets laid out by reading time from the last cue.
            // By this point the remaining audio is too short to give the aligner any slack -
            // it has to cram, and a crammed guess is worse than an even spread. Better an
            // approximate last few lines than lines with no time codes at all.
            if (cursor < texts.Count)
            {
                var at = aligned.Count > 0 ? aligned[^1].End : 0;
                var wanted = 0.0;
                for (var i = cursor; i < texts.Count; i++)
                {
                    wanted += Math.Max(0.5, texts[i].Length / readingCps);
                }

                // Squeeze them into the audio that is actually left rather than running on
                // past the end of the file. Times beyond the recording are not just wrong,
                // they are unrefinable - the second pass clamps their window to nothing.
                var available = Math.Max(0.5, _audio.TotalSeconds - at);
                var scale = wanted > available ? available / wanted : 1.0;

                for (var i = cursor; i < texts.Count; i++)
                {
                    var seconds = Math.Max(0.5, texts[i].Length / readingCps) * scale;
                    estimated.Add(aligned.Count);
                    aligned.Add((at, Math.Min(_audio.TotalSeconds, at + seconds)));
                    at += seconds;
                }

                cursor = texts.Count;
            }
        }

        aligned = await RefineAsync(aligned, texts, readingCps, estimated, progress, cancellationToken).ConfigureAwait(false);

        ApplyTimeCodes(lines, aligned, texts, alignable, _audio.TotalSeconds);
        return new Result(allTexts.Count, aligned.Count);
    }

    /// <summary>
    /// Second pass: re-align each small batch of lines against a window drawn tightly
    /// round where the first pass put them.
    ///
    /// The first pass has to keep its windows loose, because it does not yet know where
    /// anything is - which costs precision at every chunk edge, and leaves the closing
    /// lines laid out by reading time. Once every line has an approximate position that
    /// constraint is gone: a batch's window can be a few seconds of audio holding exactly
    /// that batch's speech, which is the condition a forced aligner is best at.
    ///
    /// A refined time is only taken when it agrees with the first pass to within
    /// <see cref="ForcedAlignPlanner.Options.MaxRefineShiftSeconds"/>. Anchored on a bad
    /// first-pass guess the aligner would re-fit the batch to the wrong audio just as
    /// confidently, so disagreement is treated as a reason to keep what we had.
    /// </summary>
    private async Task<List<(double Start, double End)>> RefineAsync(
        List<(double Start, double End)> aligned,
        IReadOnlyList<string> texts,
        double readingCps,
        IReadOnlySet<int> estimated,
        IProgress<Progress>? progress,
        CancellationToken cancellationToken)
    {
        if (aligned.Count == 0)
        {
            return aligned;
        }

        var refined = new List<(double Start, double End)>(aligned);
        var batches = (int)Math.Ceiling(aligned.Count / (double)_options.RefineBatchLines);

        for (var b = 0; b < batches; b++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var first = b * _options.RefineBatchLines;
            var last = Math.Min(first + _options.RefineBatchLines, aligned.Count) - 1;
            if (last <= first)
            {
                continue;
            }

            var start = Math.Max(0, aligned[first].Start - _options.RefinePaddingSeconds);
            var end = Math.Min(_audio.TotalSeconds, aligned[last].End + _options.RefinePaddingSeconds);
            var length = end - start;
            if (length <= 0.5)
            {
                continue;
            }

            var fed = new List<string>();
            for (var i = first; i <= last; i++)
            {
                fed.Add(texts[i]);
            }

            IReadOnlyList<ForcedAlignPlanner.Cue> cues;
            try
            {
                cues = await AlignWindowAsync(start, length, fed, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            {
                continue;
            }

            if (cues.Count != fed.Count)
            {
                continue;
            }

            for (var k = 0; k < cues.Count; k++)
            {
                var index = first + k;
                var newStart = start + cues[k].StartSeconds;
                var newEnd = start + cues[k].EndSeconds;

                // Lines the first pass could only estimate - the closing ones, laid out by
                // reading time once the audio ran short of window - have no measured time to
                // disagree with, so the guard would only preserve a guess.
                if (!estimated.Contains(index) &&
                    Math.Abs(newStart - aligned[index].Start) > _options.MaxRefineShiftSeconds)
                {
                    continue;
                }

                // The batch's last cue has no following word to stop at, so its end still
                // runs to the window edge; keep it honest against reading time.
                var cappedEnd = Math.Max(
                    newStart + 0.05,
                    Math.Min(newEnd, newStart + (texts[index].Length / readingCps * 2.0)));

                refined[index] = (newStart, cappedEnd);
            }

            progress?.Report(new Progress(
                b + 1, batches, aligned.Count, texts.Count, 70.0 + ((b + 1) / (double)batches * 30.0)));
        }

        return refined;
    }

    private async Task<IReadOnlyList<OpenAiSttChunker.SilenceInterval>> DetectSilenceSafelyAsync(CancellationToken cancellationToken)
    {
        if (_audio.TotalSeconds <= _options.WindowSeconds)
        {
            return Array.Empty<OpenAiSttChunker.SilenceInterval>();
        }

        try
        {
            return await _audio.DetectSilenceAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            // Silence detection is an optimisation, not a requirement - fixed-length
            // windows still align, they just risk cutting through a word.
            return Array.Empty<OpenAiSttChunker.SilenceInterval>();
        }
    }

    /// <summary>
    /// Moves a window edge onto the nearest silence so the cut does not fall in the middle
    /// of a word - a word split across two windows is lost to both of them.
    /// </summary>
    private double SnapWindowEnd(double target, IReadOnlyList<OpenAiSttChunker.SilenceInterval> silences)
    {
        if (target >= _audio.TotalSeconds)
        {
            return _audio.TotalSeconds;
        }

        const double maxOffsetSeconds = 10.0;
        var best = target;
        var bestDistance = double.MaxValue;

        foreach (var silence in silences)
        {
            var distance = Math.Abs(silence.Midpoint - target);
            if (distance < bestDistance && distance <= maxOffsetSeconds)
            {
                bestDistance = distance;
                best = silence.Midpoint;
            }
        }

        return Math.Min(best, _audio.TotalSeconds);
    }

    private async Task<IReadOnlyList<ForcedAlignPlanner.Cue>> AlignWindowAsync(
        double startSeconds,
        double durationSeconds,
        IReadOnlyList<string> fedLines,
        CancellationToken cancellationToken)
    {
        var audioFileName = await _audio
            .ExtractWindowAsync(startSeconds, durationSeconds, cancellationToken)
            .ConfigureAwait(false);

        var textFileName = Path.Combine(
            Path.GetDirectoryName(audioFileName) ?? Path.GetTempPath(),
            Path.GetFileNameWithoutExtension(audioFileName) + ".txt");

        await File.WriteAllLinesAsync(textFileName, fedLines, cancellationToken).ConfigureAwait(false);

        var srt = await _runner.AlignAsync(audioFileName, textFileName, cancellationToken).ConfigureAwait(false);
        return ParseCues(srt);
    }

    /// <summary>
    /// Reads the aligner's SRT output. One cue per fed line, in order - the aligner is
    /// told to emit segment granularity precisely so this mapping holds.
    /// </summary>
    public static IReadOnlyList<ForcedAlignPlanner.Cue> ParseCues(string srtText)
    {
        if (string.IsNullOrWhiteSpace(srtText))
        {
            return Array.Empty<ForcedAlignPlanner.Cue>();
        }

        var subtitle = new Subtitle();
        new SubRip().LoadSubtitle(subtitle, srtText.Replace("\r\n", "\n").Split('\n').ToList(), string.Empty);

        return subtitle.Paragraphs
            .Select(p => new ForcedAlignPlanner.Cue(p.StartTime.TotalSeconds, p.EndTime.TotalSeconds))
            .ToList();
    }

    /// <summary>
    /// Writes the aligned times onto the lines.
    ///
    /// Starts are used as the aligner reported them - measured against per-line ground
    /// truth they land within ~34 ms. Ends are not: a forced aligner ends each segment
    /// where the next one starts, so the last line before a stretch of music or action
    /// absorbs all of it. On a recording that is 56% speech this produced 40 cues over
    /// ten seconds long, one of them 16.8 s, against a true maximum of 4.4 s. Durations
    /// are therefore recomputed the same way the rest of this window does it
    /// (see <see cref="TimeCodeCalculator"/>): reading time for the text, clamped to the
    /// configured minimum and maximum display duration.
    /// </summary>
    private static void ApplyTimeCodes(
        IList<SubtitleLineViewModel> lines,
        List<(double Start, double End)> aligned,
        IReadOnlyList<string> texts,
        IReadOnlyList<int> alignable,
        double audioSeconds)
    {
        var minGapMs = Se.Settings.General.MinimumBetweenLines.GetMilliseconds();
        var minDurationMs = (double)Se.Settings.General.SubtitleMinimumDisplayMilliseconds;
        var maxDurationMs = (double)Se.Settings.General.SubtitleMaximumDisplayMilliseconds;
        var optimalCps = Se.Settings.General.SubtitleOptimalCharactersPerSeconds;
        var previousEndMs = double.NegativeInfinity;

        for (var i = 0; i < aligned.Count && i < alignable.Count; i++)
        {
            var lineIndex = alignable[i];
            var startMs = aligned[i].Start * 1000.0;

            // Windows are aligned independently, so the first cue of a window can start a
            // hair before the last cue of the previous one ended. Nudge rather than
            // reorder - the alignment itself is monotonic, this is just rounding at seams.
            if (startMs < previousEndMs + minGapMs)
            {
                startMs = previousEndMs + minGapMs;
            }

            var spokenMs = (aligned[i].End * 1000.0) - startMs;
            var readingMs = optimalCps > 0
                ? texts[i].Length / optimalCps * 1000.0
                : minDurationMs;

            // Whichever is shorter: how long the line actually took, or how long it takes
            // to read. A cue must never outlast the silence the aligner stretched it over.
            var durationMs = Math.Min(spokenMs > 0 ? spokenMs : readingMs, readingMs);
            durationMs = Math.Clamp(durationMs, minDurationMs, maxDurationMs > 0 ? maxDurationMs : durationMs);

            // Never run into the next line's speech.
            if (i + 1 < aligned.Count)
            {
                var nextStartMs = aligned[i + 1].Start * 1000.0;
                var roomMs = nextStartMs - minGapMs - startMs;
                if (roomMs > 0 && durationMs > roomMs)
                {
                    durationMs = roomMs;
                }
            }

            var endMs = startMs + Math.Max(durationMs, 1);

            // Nothing may run past the end of the recording. A cue placed near the end can
            // otherwise have reading time added on top and finish after the video does.
            var audioMs = audioSeconds * 1000.0;
            if (audioMs > 0 && endMs > audioMs)
            {
                endMs = audioMs;
                startMs = Math.Min(startMs, Math.Max(0, endMs - minDurationMs));
            }

            lines[lineIndex].StartTime = TimeSpan.FromMilliseconds(startMs);
            lines[lineIndex].EndTime = TimeSpan.FromMilliseconds(endMs);
            lines[lineIndex].UpdateDuration();
            previousEndMs = endMs;
        }
    }
}
