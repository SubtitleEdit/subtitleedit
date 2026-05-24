using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText.OpenAiCompatible;

/// <summary>
/// Helpers for splitting an audio file into chunks below OpenAI's 25 MB STT
/// upload cap. The boundary-snapping and ffmpeg-output parsing pieces are
/// exposed as pure functions so they can be unit-tested without invoking
/// ffmpeg; the async detector wraps an actual ffmpeg run.
/// </summary>
public static class OpenAiSttChunker
{
    /// <summary>
    /// Maximum file size we'll upload in a single request — kept just under
    /// OpenAI's 25 MB hard cap so the multipart envelope overhead doesn't
    /// push us over.
    /// </summary>
    public const long DefaultThresholdBytes = 24L * 1024 * 1024;

    /// <summary>
    /// Target size for each chunk after splitting. 1 MB headroom below the
    /// threshold so an uneven bitrate doesn't push a single chunk over.
    /// </summary>
    public const long DefaultChunkSizeBytes = 23L * 1024 * 1024;

    public sealed record SilenceInterval(double StartSeconds, double EndSeconds)
    {
        public double Midpoint => (StartSeconds + EndSeconds) / 2.0;
        public double DurationSeconds => EndSeconds - StartSeconds;
    }

    public sealed record ChunkBoundary(double StartSeconds, double EndSeconds)
    {
        public double DurationSeconds => EndSeconds - StartSeconds;
    }

    /// <summary>
    /// Number of chunks needed to keep each piece below the target chunk
    /// size, rounded up. Always at least 1.
    /// </summary>
    public static int ComputeChunkCount(long fileSizeBytes, long chunkSizeBytes = DefaultChunkSizeBytes)
    {
        if (chunkSizeBytes <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(chunkSizeBytes), "Chunk size must be positive.");
        }

        if (fileSizeBytes <= 0)
        {
            return 1;
        }

        return Math.Max(1, (int)Math.Ceiling(fileSizeBytes / (double)chunkSizeBytes));
    }

    /// <summary>
    /// Split totalSeconds into chunkCount approximately equal windows, then
    /// snap each internal boundary to the midpoint of the nearest unused
    /// silence interval within ±maxOffsetSeconds of its target time
    /// (inclusive at the edge). Cuts stay strictly increasing — if snapping
    /// (or the even-time target itself) would land at or behind the previous
    /// cut, the cut is placed at <c>lastCut + (totalSeconds - lastCut) /
    /// remaining</c> so the remaining chunks spread evenly over what's left.
    /// </summary>
    public static IReadOnlyList<ChunkBoundary> ComputeAdjustedBoundaries(
        double totalSeconds,
        int chunkCount,
        IReadOnlyList<SilenceInterval> silenceIntervals,
        double maxOffsetSeconds = 10.0)
    {
        if (totalSeconds <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalSeconds), "Total seconds must be positive.");
        }

        if (chunkCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(chunkCount), "Chunk count must be positive.");
        }

        ArgumentNullException.ThrowIfNull(silenceIntervals);

        if (chunkCount == 1)
        {
            return new[] { new ChunkBoundary(0, totalSeconds) };
        }

        // Only silences whose midpoint falls strictly inside (0, totalSeconds)
        // can ever be cut points. Sort by midpoint so we can scan once.
        var sortedSilences = silenceIntervals
            .Where(s => s.Midpoint > 0 && s.Midpoint < totalSeconds)
            .OrderBy(s => s.Midpoint)
            .ToList();

        var cuts = new List<double>(chunkCount - 1);
        var lastCut = 0.0;
        var nextSilenceIdx = 0;

        for (var i = 0; i < chunkCount - 1; i++)
        {
            var target = totalSeconds * (i + 1) / chunkCount;
            var bestCut = target;
            var bestOffset = double.PositiveInfinity;
            var bestIdx = -1;

            // Skip silences we've already used or that fall before the
            // previous cut (can't reuse silence: that would collapse two
            // chunks into one big one above the size cap).
            while (nextSilenceIdx < sortedSilences.Count && sortedSilences[nextSilenceIdx].Midpoint <= lastCut)
            {
                nextSilenceIdx++;
            }

            for (var j = nextSilenceIdx; j < sortedSilences.Count; j++)
            {
                var mp = sortedSilences[j].Midpoint;
                if (mp > target + maxOffsetSeconds)
                {
                    break;
                }

                var offset = Math.Abs(mp - target);
                if (offset > maxOffsetSeconds)
                {
                    // Inside the sorted-scan upper bound but outside the
                    // window on the low side — ignore.
                    continue;
                }

                if (offset < bestOffset)
                {
                    bestOffset = offset;
                    bestCut = mp;
                    bestIdx = j;
                }
            }

            // If neither a silence nor the even-time target sits after the
            // previous cut (rare; only happens when snapping pushed an
            // earlier cut past a later target), fall back to a midpoint
            // between lastCut and totalSeconds so chunks stay non-empty.
            if (bestCut <= lastCut)
            {
                var remaining = chunkCount - i;
                bestCut = lastCut + (totalSeconds - lastCut) / remaining;
                bestIdx = -1;
            }

            cuts.Add(bestCut);
            lastCut = bestCut;
            if (bestIdx >= 0)
            {
                nextSilenceIdx = bestIdx + 1;
            }
        }

        var result = new List<ChunkBoundary>(chunkCount);
        var start = 0.0;
        foreach (var cut in cuts)
        {
            result.Add(new ChunkBoundary(start, cut));
            start = cut;
        }
        result.Add(new ChunkBoundary(start, totalSeconds));
        return result;
    }

    private static readonly Regex SilenceStartRegex = new(@"silence_start:\s*(-?\d+(?:\.\d+)?)", RegexOptions.Compiled);
    private static readonly Regex SilenceEndRegex = new(@"silence_end:\s*(-?\d+(?:\.\d+)?)", RegexOptions.Compiled);

    /// <summary>
    /// Parse the stderr text of an ffmpeg `silencedetect` pass into discrete
    /// silence intervals. A trailing silence_start with no matching
    /// silence_end (which happens when the file ends while still silent) is
    /// dropped — we can't use it for a cut point.
    /// </summary>
    public static IReadOnlyList<SilenceInterval> ParseSilenceIntervals(string ffmpegStderr)
    {
        if (string.IsNullOrEmpty(ffmpegStderr))
        {
            return Array.Empty<SilenceInterval>();
        }

        var intervals = new List<SilenceInterval>();
        double? pendingStart = null;
        foreach (var line in ffmpegStderr.Split('\n'))
        {
            var startMatch = SilenceStartRegex.Match(line);
            if (startMatch.Success)
            {
                pendingStart = double.Parse(startMatch.Groups[1].Value, CultureInfo.InvariantCulture);
                continue;
            }

            var endMatch = SilenceEndRegex.Match(line);
            if (endMatch.Success && pendingStart.HasValue)
            {
                var end = double.Parse(endMatch.Groups[1].Value, CultureInfo.InvariantCulture);
                if (end > pendingStart.Value)
                {
                    intervals.Add(new SilenceInterval(pendingStart.Value, end));
                }
                pendingStart = null;
            }
        }

        return intervals;
    }

    /// <summary>
    /// Extract a single time-window from an audio file into a new file via
    /// ffmpeg, using stream copy (no re-encoding) for speed. The output
    /// extension must match the input — `-c copy` can't change containers.
    /// Returns false if ffmpeg is missing or exits non-zero; the caller is
    /// expected to abort the chunked upload in that case.
    /// </summary>
    public static async Task<bool> ExtractChunkAsync(
        string ffmpegPath,
        string inputPath,
        string outputPath,
        double startSeconds,
        double durationSeconds,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(inputPath))
        {
            throw new FileNotFoundException("Input audio file not found", inputPath);
        }

        if (durationSeconds <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(durationSeconds), "Chunk duration must be positive.");
        }

        var start = startSeconds.ToString("0.###", CultureInfo.InvariantCulture);
        var duration = durationSeconds.ToString("0.###", CultureInfo.InvariantCulture);
        var args = $"-hide_banner -nostats -ss {start} -i \"{inputPath}\" -t {duration} -c copy -y \"{outputPath}\"";

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo(ffmpegPath, args)
            {
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            },
            EnableRaisingEvents = true,
        };

        try
        {
            process.Start();
        }
        catch (Win32Exception)
        {
            return false;
        }

        process.BeginErrorReadLine();
        process.BeginOutputReadLine();

        try
        {
            await process.WaitForExitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            try
            {
                if (!process.HasExited)
                {
                    process.Kill(entireProcessTree: true);
                }
            }
            catch
            {
                // best-effort cleanup
            }

            throw;
        }

        return process.ExitCode == 0 && File.Exists(outputPath) && new FileInfo(outputPath).Length > 0;
    }

    /// <summary>
    /// Run ffmpeg's `silencedetect` filter against the given audio file and
    /// return the parsed silence intervals. Defaults to -30 dB noise floor
    /// and 0.5 s minimum silence — reasonable for speech audio at 16 kHz
    /// mono. Returns an empty list when ffmpeg is unavailable or exits
    /// non-zero (the caller treats that as "no silences found" and falls
    /// back to even-time cuts).
    /// </summary>
    public static async Task<IReadOnlyList<SilenceInterval>> DetectSilenceIntervalsAsync(
        string ffmpegPath,
        string audioPath,
        double noiseFloorDb = -30.0,
        double minSilenceSeconds = 0.5,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(audioPath))
        {
            throw new FileNotFoundException("Audio file not found", audioPath);
        }

        var noise = noiseFloorDb.ToString("0.##", CultureInfo.InvariantCulture);
        var minDur = minSilenceSeconds.ToString("0.###", CultureInfo.InvariantCulture);
        var args = $"-hide_banner -nostats -i \"{audioPath}\" -af silencedetect=noise={noise}dB:d={minDur} -f null -";

        var stderr = new StringBuilder();
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo(ffmpegPath, args)
            {
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            },
            EnableRaisingEvents = true,
        };

        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data != null)
            {
                lock (stderr)
                {
                    stderr.AppendLine(e.Data);
                }
            }
        };

        try
        {
            process.Start();
        }
        catch (Win32Exception)
        {
            return Array.Empty<SilenceInterval>();
        }

        process.BeginErrorReadLine();
        process.BeginOutputReadLine();

        try
        {
            await process.WaitForExitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            try
            {
                if (!process.HasExited)
                {
                    process.Kill(entireProcessTree: true);
                }
            }
            catch
            {
                // best-effort cleanup
            }

            throw;
        }

        if (process.ExitCode != 0)
        {
            return Array.Empty<SilenceInterval>();
        }

        return ParseSilenceIntervals(stderr.ToString());
    }
}
