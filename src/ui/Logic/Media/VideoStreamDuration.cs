using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech;

namespace Nikse.SubtitleEdit.Logic.Media;

/// <summary>
/// Measures how long the first video stream of a file really is, by stream-copying it to the
/// null muxer and reading the last "out_time_us" progress value.
/// </summary>
/// <remarks>
/// The container duration ffmpeg prints is the longest stream, so a file whose video stopped
/// after a minute but whose audio runs to the end still reports the full length (#15265).
/// Copying to the null muxer only reads packets - no decoding - so it is fast even for a film.
/// </remarks>
public static class VideoStreamDuration
{
    private const string OutTimePrefix = "out_time_us=";

    /// <summary>Returns the video stream's length in seconds, or null if it could not be measured.</summary>
    public static async Task<double?> GetSecondsAsync(string fileName, CancellationToken cancellationToken)
    {
        long maxMicroseconds = -1;
        var parameters = $"-nostdin {FfmpegProgressTracker.ProgressArguments} -i \"{fileName}\" -map 0:v:0 -c copy -f null -";
        using var process = FfmpegGenerator.GetProcess(parameters, (_, e) =>
        {
            if (TryParseOutTime(e.Data, out var microseconds))
            {
                Interlocked.Exchange(ref maxMicroseconds, Math.Max(Interlocked.Read(ref maxMicroseconds), microseconds));
            }
        });

        try
        {
            await process.StartAndWaitAsync(cancellationToken, TimeSpan.FromMinutes(10));
        }
        catch (TimeoutException)
        {
            return null;
        }

        var result = Interlocked.Read(ref maxMicroseconds);
        if (process.ExitCode != 0 || result <= 0)
        {
            return null;
        }

        return result / 1_000_000.0;
    }

    internal static bool TryParseOutTime(string? line, out long microseconds)
    {
        microseconds = 0;
        if (string.IsNullOrEmpty(line) || !line.StartsWith(OutTimePrefix, StringComparison.Ordinal))
        {
            return false;
        }

        // ffmpeg prints "N/A" before the first packet.
        return long.TryParse(line.AsSpan(OutTimePrefix.Length).Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out microseconds) &&
               microseconds >= 0;
    }

    /// <summary>
    /// True when the output's video is clearly shorter than the source's: more than 5 seconds
    /// and more than 2% short. Stream copy can drift by a frame or two at the ends, and a source
    /// with a leading gap may lose a little, so only a real loss counts.
    /// </summary>
    public static bool IsTruncated(double sourceSeconds, double outputSeconds)
    {
        if (sourceSeconds <= 0 || outputSeconds < 0)
        {
            return false;
        }

        var missing = sourceSeconds - outputSeconds;
        return missing > 5 && missing > sourceSeconds * 0.02;
    }
}
