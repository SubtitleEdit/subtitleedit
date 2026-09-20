using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech;

/// <summary>
/// Silence thresholds for the TTS audio pipeline, derived from the clip's own peak level.
/// </summary>
/// <remarks>
/// The silence trim, the VAD pause compression and the pro-chain noise gate all used a fixed
/// -40 dBFS threshold. Voice cloning engines reproduce the loudness of their reference, and a
/// reference cut from film dialogue often peaks at -20 to -30 dBFS - so the soft final consonant
/// of a quiet clone fell under the fixed threshold and was trimmed off as "silence", cutting the
/// last word of the line (#14480). Measured on a real clip: at a -21 dBFS peak the final "s" was
/// gone, at -33 dBFS the whole line was considered silence. Making the threshold relative to the
/// peak (40 dB below it) gives every clip the same trim a full-scale one gets.
/// <para>
/// Why 40 dB and not less: a word-final "s" peaks 20-25 dB under the clip's loudest vowel, and
/// "f"/"th" sit another ~10 dB lower, so a narrower window starts eating endings again. The
/// price is paid on clips whose noise floor is within 40 dB of the peak (some VibeVoice/Qwen3
/// output): up to a few hundred ms of hiss ~30 dB under the speech can stay at the ends. Over
/// ~250 real clips from eight engines the median difference to the old trim was 0 ms at both
/// ends, which is the point - loud clips are trimmed exactly as before.
/// </para>
/// </remarks>
public static partial class TtsSilenceThreshold
{
    /// <summary>How far below the clip's peak a sample still counts as speech.</summary>
    public const double BelowPeakDb = 40.0;

    /// <summary>
    /// Never listen below this: neural vocoders leave a noise floor around -60 dBFS, and a
    /// threshold under it would stop trimming trailing silence on very quiet clips.
    /// </summary>
    public const double FloorDbfs = -70.0;

    /// <summary>The threshold every stage used before the peak was measured: 0.01 = -40 dBFS.</summary>
    public const double LegacyThresholdDbfs = -40.0;

    /// <summary>
    /// Threshold in dBFS for a clip with the given peak. A null peak (ffmpeg could not measure
    /// the file) falls back to the legacy fixed threshold.
    /// </summary>
    public static double ThresholdDbfs(double? peakDbfs)
    {
        if (peakDbfs == null || double.IsNaN(peakDbfs.Value) || double.IsInfinity(peakDbfs.Value))
        {
            return LegacyThresholdDbfs;
        }

        // Float WAVs can peak above 0 dBFS; a threshold above the legacy one would trim more
        // aggressively than before, never less, so cap the peak at full scale.
        var peak = Math.Min(0.0, peakDbfs.Value);
        return Math.Max(FloorDbfs, peak - BelowPeakDb);
    }

    /// <summary>Threshold as a linear amplitude (0..1), the form silenceremove/agate take.</summary>
    public static double Amplitude(double? peakDbfs)
    {
        return Math.Pow(10.0, ThresholdDbfs(peakDbfs) / 20.0);
    }

    /// <summary>Threshold as an ffmpeg dB literal, e.g. "-52.3dB".</summary>
    public static string DbLiteral(double? peakDbfs)
    {
        return ThresholdDbfs(peakDbfs).ToString("0.0", CultureInfo.InvariantCulture) + "dB";
    }

    /// <summary>
    /// Peak level of an audio file in dBFS via ffmpeg's volumedetect, or null when ffmpeg failed
    /// or printed no peak (callers then fall back to the legacy threshold).
    /// </summary>
    public static async Task<double?> MeasurePeakDbfsAsync(string fileName, CancellationToken cancellationToken, TimeSpan? timeout = null)
    {
        var lines = new List<string>();
        var gate = new object();

        void OnLine(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            if (e.Data == null)
            {
                return;
            }

            lock (gate)
            {
                lines.Add(e.Data);
            }
        }

        try
        {
            using var process = FfmpegGenerator.MeasurePeakVolume(fileName, OnLine);
            if (timeout.HasValue)
            {
                await process.StartAndWaitAsync(cancellationToken, timeout.Value);
            }
            else
            {
                await process.StartAndWaitAsync(cancellationToken);
            }

            // Wait for the async stream readers to deliver everything before parsing.
            process.WaitForExit();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            return null;
        }

        lock (gate)
        {
            return ParsePeakDbfs(lines);
        }
    }

    /// <summary>Finds volumedetect's "max_volume: -2.8 dB" in ffmpeg's output.</summary>
    public static double? ParsePeakDbfs(IEnumerable<string> ffmpegOutputLines)
    {
        foreach (var line in ffmpegOutputLines)
        {
            var match = MaxVolumeRegex().Match(line);
            if (match.Success &&
                double.TryParse(match.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var peak))
            {
                return peak;
            }
        }

        return null;
    }

    [GeneratedRegex(@"max_volume:\s*(-?\d+(?:\.\d+)?)\s*dB", RegexOptions.IgnoreCase)]
    private static partial Regex MaxVolumeRegex();
}
