using System;
using System.Globalization;

namespace Nikse.SubtitleEdit.Logic.Media;

/// <summary>
/// Turns ffmpeg's machine-readable progress output into a monotonic whole-number percentage.
/// Prepend <see cref="ProgressArguments"/> to the ffmpeg command line (progress key/value lines
/// then arrive on stdout), feed every received line to <see cref="TryGetNewPercent"/>, and update
/// the UI only when it returns true. The percent only ever increases, so out-of-order or repeated
/// lines never make a progress bar jump backwards.
/// </summary>
public sealed class FfmpegProgressTracker
{
    /// <summary>
    /// Arguments to prepend to an ffmpeg command line: silences the human-readable stderr stats
    /// and emits "key=value" progress lines (out_time_us=..., speed=..., progress=...) on stdout.
    /// </summary>
    public const string ProgressArguments = "-nostats -progress pipe:1";

    private const string OutTimePrefix = "out_time_us=";

    // Every key ffmpeg emits in a "-progress" block; used to keep these lines out of user-facing logs.
    private static readonly string[] ProgressLinePrefixes =
    {
        "frame=", "fps=", "stream_", "bitrate=", "total_size=", "out_time",
        "dup_frames=", "drop_frames=", "speed=", "progress=",
    };

    private readonly double _totalDurationSeconds;
    private int _lastPercent = -1;

    public FfmpegProgressTracker(double totalDurationSeconds)
    {
        _totalDurationSeconds = totalDurationSeconds;
    }

    /// <summary>
    /// Parses one line of ffmpeg output. Returns true only when the line is an "out_time_us="
    /// progress line that advances to a new, higher whole percent (0-100).
    /// </summary>
    public bool TryGetNewPercent(string? line, out int percent)
    {
        percent = 0;
        if (string.IsNullOrEmpty(line) ||
            _totalDurationSeconds <= 0 ||
            !line.StartsWith(OutTimePrefix, StringComparison.Ordinal))
        {
            return false;
        }

        var value = line.Substring(OutTimePrefix.Length).Trim();
        if (!long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var microseconds) ||
            microseconds < 0)
        {
            return false;
        }

        var p = (int)Math.Round(microseconds / (_totalDurationSeconds * 10_000.0), MidpointRounding.AwayFromZero);
        p = Math.Clamp(p, 0, 100);
        if (p <= _lastPercent)
        {
            return false;
        }

        _lastPercent = p;
        percent = p;
        return true;
    }

    /// <summary>
    /// Parses the processed-frame count from a "frame=N" progress line. Also tolerates the
    /// human-readable stderr stats form ("frame=  123 fps= 25 ...") so callers still get
    /// progress if a custom command line disables the machine-readable channel.
    /// </summary>
    public static bool TryGetFrame(string? line, out long frame)
    {
        frame = 0;
        if (string.IsNullOrEmpty(line) || !line.StartsWith("frame=", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var i = "frame=".Length;
        while (i < line.Length && line[i] == ' ')
        {
            i++;
        }

        var start = i;
        while (i < line.Length && char.IsAsciiDigit(line[i]))
        {
            i++;
        }

        return i > start &&
               long.TryParse(line.AsSpan(start, i - start), NumberStyles.Integer, CultureInfo.InvariantCulture, out frame);
    }

    /// <summary>
    /// True when the line is part of a "-progress" key/value block (so callers can keep the
    /// twice-a-second progress spam out of logs shown to the user).
    /// </summary>
    public static bool IsProgressLine(string line)
    {
        foreach (var prefix in ProgressLinePrefixes)
        {
            if (line.StartsWith(prefix, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
