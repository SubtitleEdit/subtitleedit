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

    public sealed record Progress(int WindowIndex, int WindowCount, int LinesAligned, int LinesTotal);

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

        var texts = lines
            .Select(l => HtmlUtil.RemoveHtmlTags(l.Text ?? string.Empty, true).Replace('\r', ' ').Replace('\n', ' ').Trim())
            .ToList();

        if (texts.Count == 0 || _audio.TotalSeconds <= 0)
        {
            return new Result(texts.Count, 0);
        }

        var silences = await DetectSilenceSafelyAsync(cancellationToken).ConfigureAwait(false);

        // A fixed rate over the whole file, deliberately not re-estimated as we go. The
        // obvious refinement - learn the rate from each window's output - feeds back on
        // itself: over-feeding a window makes the aligner compress its cues, a compressed
        // window looks like faster speech, and the next window is over-fed harder still.
        var charsPerSecond = Math.Max(1.0, texts.Sum(t => t.Length) / _audio.TotalSeconds);

        var aligned = new List<(double Start, double End)>(texts.Count);
        var cursor = 0;
        var position = 0.0;
        var windowIndex = 0;

        // The window slides to wherever the last trusted cue ended rather than stepping
        // along a fixed grid. Only part of each window is trusted (the tail is where
        // overshoot text gets crammed), so a fixed grid would advance the audio by a full
        // window while advancing the script by less - the script would fall progressively
        // further behind the audio over a long file.
        var estimatedWindows = Math.Max(1, (int)Math.Ceiling(_audio.TotalSeconds / (_options.WindowSeconds * _options.AcceptFraction)));

        while (cursor < texts.Count && position < _audio.TotalSeconds - 0.25)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var windowEnd = SnapWindowEnd(position + _options.WindowSeconds, silences);
            var isLast = windowEnd >= _audio.TotalSeconds - 0.25;
            var windowLength = windowEnd - position;
            if (windowLength <= 0)
            {
                break;
            }

            var remaining = texts.Skip(cursor).ToList();
            var take = ForcedAlignPlanner.LinesForWindow(remaining, windowLength, charsPerSecond, _options, isLast);
            if (take == 0)
            {
                break;
            }

            var fed = remaining.Take(take).ToList();
            var cues = await AlignWindowAsync(position, windowLength, fed, cancellationToken).ConfigureAwait(false);
            var accept = ForcedAlignPlanner.AcceptCount(cues, windowLength, _options, isLast);

            if (accept == 0)
            {
                // Nothing usable came back. Step past this stretch of audio rather than
                // planning the identical window again forever.
                position += _options.WindowSeconds * _options.AcceptFraction;
                windowIndex++;
                continue;
            }

            for (var i = 0; i < accept; i++)
            {
                aligned.Add((position + cues[i].StartSeconds, position + cues[i].EndSeconds));
            }

            var consumedSeconds = cues[accept - 1].EndSeconds;
            cursor += accept;
            windowIndex++;

            var nextPosition = position + consumedSeconds;
            if (nextPosition <= position + 0.25)
            {
                // Guard against a window that consumed no meaningful audio.
                nextPosition = position + (_options.WindowSeconds * _options.AcceptFraction);
            }

            position = nextPosition;
            progress?.Report(new Progress(windowIndex, Math.Max(windowIndex, estimatedWindows), cursor, texts.Count));
        }

        ApplyTimeCodes(lines, aligned);
        return new Result(texts.Count, aligned.Count);
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

    private static void ApplyTimeCodes(IList<SubtitleLineViewModel> lines, List<(double Start, double End)> aligned)
    {
        var minGapMs = Se.Settings.General.MinimumBetweenLines.GetMilliseconds();
        var previousEndMs = double.NegativeInfinity;

        for (var i = 0; i < aligned.Count && i < lines.Count; i++)
        {
            var startMs = aligned[i].Start * 1000.0;
            var endMs = aligned[i].End * 1000.0;

            // Windows are aligned independently, so the first cue of a window can start a
            // hair before the last cue of the previous one ended. Nudge rather than
            // reorder - the alignment itself is monotonic, this is just rounding at seams.
            if (startMs < previousEndMs + minGapMs)
            {
                startMs = previousEndMs + minGapMs;
            }

            if (endMs <= startMs)
            {
                endMs = startMs + Se.Settings.General.SubtitleMinimumDisplayMilliseconds;
            }

            lines[i].StartTime = TimeSpan.FromMilliseconds(startMs);
            lines[i].EndTime = TimeSpan.FromMilliseconds(endMs);
            lines[i].UpdateDuration();
            previousEndMs = endMs;
        }
    }
}
