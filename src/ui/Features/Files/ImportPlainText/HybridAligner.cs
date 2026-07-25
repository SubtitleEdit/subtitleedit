using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Files.ImportPlainText;

/// <summary>
/// Aligns a script that does not match the audio exactly.
///
/// Forced alignment alone cannot cope with a script that omits something actually spoken:
/// it has to place every line it is given, so the lines after an omission get crammed onto
/// the missing passage's audio. Measured against ground truth with a contiguous 90 s
/// omission, every following line landed 100-121 s early.
///
/// So the transcription does the finding and the aligner does the timing. A speech
/// recogniser says what is actually spoken and when; <see cref="ScriptSyncService"/>
/// matches the script against that transcript, which tolerates text missing from either
/// side because it is a sequence match that may skip; and each run of confidently matched
/// lines is then forced-aligned against just its own stretch of audio, which is the part
/// that gets timings to a few tens of milliseconds.
/// </summary>
public sealed class HybridAligner
{
    /// <summary>A run of consecutive script lines and the audio they were matched to.</summary>
    public readonly record struct Region(int FirstLine, int LastLine, double StartSeconds, double EndSeconds)
    {
        public int LineCount => LastLine - FirstLine + 1;
        public double DurationSeconds => EndSeconds - StartSeconds;
    }

    /// <summary>
    /// Pads each region outwards, because the coarse match puts a line's start on its first
    /// matched word - anything before that word is outside the span and the aligner would
    /// have to squeeze it in.
    /// </summary>
    public const double RegionPaddingSeconds = 2.0;

    /// <summary>
    /// Longest region handed to the aligner in one piece. Beyond this the quadratic encoder
    /// cost stops being worth it and the region is split at its own line boundaries.
    /// </summary>
    public const double MaxRegionSeconds = 240.0;

    /// <summary>
    /// Groups lines into regions to refine. A region runs between directly-matched lines:
    /// those are the timings worth trusting, and the interpolated lines between them are
    /// what the aligner is being asked to place properly. Lines with no time codes at all,
    /// and stretches with no direct match anywhere, are left alone rather than guessed at.
    /// </summary>
    public static List<Region> PlanRegions(
        IReadOnlyList<double> startSeconds,
        IReadOnlyList<double> endSeconds,
        IReadOnlyList<bool> directMatches,
        double maxRegionSeconds = MaxRegionSeconds,
        double paddingSeconds = RegionPaddingSeconds)
    {
        ArgumentNullException.ThrowIfNull(startSeconds);
        ArgumentNullException.ThrowIfNull(endSeconds);
        ArgumentNullException.ThrowIfNull(directMatches);

        var regions = new List<Region>();
        var count = Math.Min(startSeconds.Count, Math.Min(endSeconds.Count, directMatches.Count));

        var i = 0;
        while (i < count)
        {
            if (!directMatches[i] || startSeconds[i] < 0)
            {
                i++;
                continue;
            }

            // Extend while the audio stays contiguous and the region stays within budget.
            var first = i;
            var last = i;
            for (var j = i + 1; j < count; j++)
            {
                if (startSeconds[j] < 0 || endSeconds[j] < startSeconds[j])
                {
                    break;
                }

                if (endSeconds[j] - startSeconds[first] > maxRegionSeconds)
                {
                    break;
                }

                last = j;
            }

            // A region has to end on a trustworthy line, or its tail is just interpolation
            // that the aligner would anchor to the wrong place.
            while (last > first && !directMatches[last])
            {
                last--;
            }

            if (last > first)
            {
                var start = Math.Max(0, startSeconds[first] - paddingSeconds);
                var end = endSeconds[last] + paddingSeconds;
                regions.Add(new Region(first, last, start, end));
                i = last + 1;
            }
            else
            {
                i++;
            }
        }

        return regions;
    }

    private readonly ForcedAligner.IRunner _runner;
    private readonly ForcedAligner.IAudioSource _audio;

    public HybridAligner(ForcedAligner.IRunner runner, ForcedAligner.IAudioSource audio)
    {
        _runner = runner;
        _audio = audio;
    }

    public sealed record Result(int TotalLines, int CoarseMatched, int Refined);

    /// <summary>
    /// Refines coarse time codes region by region. The lines must already carry the coarse
    /// alignment from <see cref="ScriptSyncService"/>; anything that cannot be refined keeps
    /// the coarse timing rather than being dropped.
    /// </summary>
    public async Task<Result> RefineAsync(
        IList<SubtitleLineViewModel> lines,
        IReadOnlyList<bool> directMatches,
        IProgress<ForcedAligner.Progress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(lines);
        ArgumentNullException.ThrowIfNull(directMatches);

        var starts = lines.Select(l => l.StartTime.TotalSeconds).ToList();
        var ends = lines.Select(l => l.EndTime.TotalSeconds).ToList();
        var regions = PlanRegions(starts, ends, directMatches);

        var refined = 0;
        for (var r = 0; r < regions.Count; r++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var region = regions[r];
            var end = Math.Min(region.EndSeconds, _audio.TotalSeconds);
            var length = end - region.StartSeconds;
            if (length <= 0.5)
            {
                continue;
            }

            var texts = new List<string>(region.LineCount);
            var lineIndices = new List<int>(region.LineCount);
            for (var i = region.FirstLine; i <= region.LastLine && i < lines.Count; i++)
            {
                var text = HtmlUtil.RemoveHtmlTags(lines[i].Text ?? string.Empty, true)
                    .Replace('\r', ' ')
                    .Replace('\n', ' ')
                    .Trim();

                if (string.IsNullOrWhiteSpace(text))
                {
                    continue;
                }

                texts.Add(text);
                lineIndices.Add(i);
            }

            if (texts.Count == 0)
            {
                continue;
            }

            IReadOnlyList<ForcedAlignPlanner.Cue> cues;
            try
            {
                cues = await AlignRegionAsync(region.StartSeconds, length, texts, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            {
                // A region that will not align keeps its coarse timing. One bad stretch of
                // audio is not a reason to fail the whole run.
                continue;
            }

            if (cues.Count != texts.Count)
            {
                continue;
            }

            for (var k = 0; k < cues.Count; k++)
            {
                var lineIndex = lineIndices[k];
                lines[lineIndex].StartTime = TimeSpan.FromSeconds(region.StartSeconds + cues[k].StartSeconds);
                lines[lineIndex].EndTime = TimeSpan.FromSeconds(region.StartSeconds + cues[k].EndSeconds);
                lines[lineIndex].UpdateDuration();
                refined++;
            }

            progress?.Report(new ForcedAligner.Progress(r + 1, regions.Count, refined, lines.Count));
        }

        return new Result(lines.Count, directMatches.Count(m => m), refined);
    }

    private async Task<IReadOnlyList<ForcedAlignPlanner.Cue>> AlignRegionAsync(
        double startSeconds,
        double durationSeconds,
        IReadOnlyList<string> texts,
        CancellationToken cancellationToken)
    {
        var audioFileName = await _audio
            .ExtractWindowAsync(startSeconds, durationSeconds, cancellationToken)
            .ConfigureAwait(false);

        var textFileName = System.IO.Path.Combine(
            System.IO.Path.GetDirectoryName(audioFileName) ?? System.IO.Path.GetTempPath(),
            System.IO.Path.GetFileNameWithoutExtension(audioFileName) + ".txt");

        await System.IO.File.WriteAllLinesAsync(textFileName, texts, cancellationToken).ConfigureAwait(false);

        var srt = await _runner.AlignAsync(audioFileName, textFileName, cancellationToken).ConfigureAwait(false);
        return ForcedAligner.ParseCues(srt);
    }
}
