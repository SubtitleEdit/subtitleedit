using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Media;

/// <summary>
/// Extracts a video's exact frame time codes with ffmpeg, for "Beautify time codes".
///
/// The time codes must be <em>presentation</em> time stamps: a subtitle cue is correct when it is
/// on screen at the right moment, which has nothing to do with the order frames are stored or
/// decoded in. With B-frames those two orders differ, and a container can additionally start at a
/// non-zero time stamp, so decode/packet-domain values are offset from what the viewer sees.
///
/// ffmpeg's "showinfo" filter reports frames as they leave the decoder - presentation order, and
/// normalized to a zero-based timeline (no -copyts) to match the subtitle timeline - which is
/// exactly the domain wanted here. It needs a full decode, hence the progress reporting,
/// cancellation, and the on-disk cache in <see cref="TimeCodesHelper"/>.
/// </summary>
public static class TimeCodesGenerator
{
    private const string PtsTimeKey = "pts_time:";

    /// <summary>
    /// Decodes the video and returns its frame presentation times in seconds, ascending.
    /// </summary>
    /// <param name="videoFileName">Video to read frame time codes from.</param>
    /// <param name="durationSeconds">Video length, used for the progress percentage. 0 if unknown.</param>
    /// <param name="progress">Receives 0-100 while decoding. Called on a background thread.</param>
    /// <param name="cancellationToken">Kills ffmpeg and returns what was gathered so far.</param>
    public static async Task<List<double>> ExtractAsync(
        string videoFileName,
        double durationSeconds,
        IProgress<int>? progress,
        CancellationToken cancellationToken)
    {
        var timeCodes = new List<double>();
        var tracker = durationSeconds > 0 ? new FfmpegProgressTracker(durationSeconds) : null;

        // "-map 0:v:0 -an -sn -dn": only the first video stream is decoded; audio/subtitle/data
        // streams would just cost time. Frames are discarded by the null muxer - the time codes
        // come from what showinfo logs on stderr, while progress arrives on stdout.
        var arguments =
            $"{FfmpegProgressTracker.ProgressArguments} -i \"{videoFileName}\" " +
            "-map 0:v:0 -an -sn -dn -vf showinfo -threads 0 -f null -";

        void OutputHandler(object sender, DataReceivedEventArgs outLine)
        {
            var line = outLine.Data;
            if (string.IsNullOrEmpty(line))
            {
                return;
            }

            if (tracker != null && tracker.TryGetNewPercent(line, out var percent))
            {
                progress?.Report(percent);
                return;
            }

            if (TryParsePtsTime(line, out var seconds))
            {
                timeCodes.Add(seconds);
            }
        }

        var process = FfmpegGenerator.GetProcess(arguments, OutputHandler);
#pragma warning disable CA1416 // Validate platform compatibility
        process.Start();
#pragma warning restore CA1416
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        try
        {
            await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

            // ffmpeg dying mid-decode (corrupt file, unsupported codec) still exits "normally"
            // here, leaving a partial list that looks complete when the duration is unknown.
            if (process.ExitCode != 0)
            {
                throw new Exception($"ffmpeg exited with code {process.ExitCode} while extracting time codes");
            }
        }
        catch (OperationCanceledException)
        {
            KillQuietly(process);
        }
        finally
        {
            process.Dispose();
        }

        return TimeCodesHelper.Normalize(timeCodes);
    }

    /// <summary>
    /// Reads the seconds value out of a showinfo line, e.g.
    /// "[Parsed_showinfo_0 @ 0x..] n: 1 pts: 40 pts_time:0.04 pos:5049 fmt:yuv420p ...".
    /// Hand-rolled rather than a regex: this runs once per frame of the video, so tens of
    /// thousands of times for a feature-length file.
    /// </summary>
    internal static bool TryParsePtsTime(string line, out double seconds)
    {
        seconds = 0;

        var start = line.IndexOf(PtsTimeKey, StringComparison.Ordinal);
        if (start < 0)
        {
            return false;
        }

        start += PtsTimeKey.Length;
        var end = start;
        while (end < line.Length && (char.IsAsciiDigit(line[end]) || line[end] == '.' || line[end] == '-'))
        {
            end++;
        }

        if (end == start)
        {
            return false; // "pts_time:N/A" - the frame carries no usable time stamp
        }

        return double.TryParse(
            line.AsSpan(start, end - start),
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out seconds);
    }

    private static void KillQuietly(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
#pragma warning disable CA1416 // Validate platform compatibility
                process.Kill(true);
#pragma warning restore CA1416
            }
        }
        catch
        {
            // already gone, or not ours to kill - nothing useful to do either way
        }
    }
}
