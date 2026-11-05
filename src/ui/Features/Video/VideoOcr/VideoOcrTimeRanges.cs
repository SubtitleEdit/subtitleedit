using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Features.Video.VideoOcr;

/// <summary>A part of the video to scan, in seconds from the start of the video.</summary>
public readonly record struct VideoOcrTimeRange(double StartSeconds, double EndSeconds)
{
    public double LengthSeconds => EndSeconds - StartSeconds;
}

/// <summary>
/// Parses the "Time ranges" field of the burned-in OCR window (#16334), e.g.
/// "16:00-32:00; 36:00-41:00". A time is seconds, m:ss or h:mm:ss with optional
/// milliseconds (".500" or ",500"); a side left empty means the start/end of the video
/// ("36:00-" scans to the end). Ranges are separated by ';', ',', spaces or new lines.
/// </summary>
public static class VideoOcrTimeRanges
{
    // The ',' millisecond separator is only taken when no digit or ':' follows its 1-3
    // digits, so "16:00-32:00,36:00-41:00" is two ranges and not 32:00.36.
    private const string TimePattern = @"\d+(?::\d{1,2}){0,2}(?:[.,]\d{1,3}(?![\d:]))?";

    private static readonly Regex RangeRegex = new(
        $@"(?<start>{TimePattern})?\s*[-–]\s*(?<end>{TimePattern})?",
        RegexOptions.Compiled);

    private static readonly Regex SeparatorRegex = new(@"^[\s;,]*$", RegexOptions.Compiled);

    /// <summary>
    /// Returns the ranges sorted, with overlapping ones joined and each clamped to the video
    /// duration. An empty text is the whole video. Returns null and sets
    /// <paramref name="invalidPart"/> to the offending text when the input cannot be used.
    /// </summary>
    public static List<VideoOcrTimeRange>? Parse(string? text, double durationSeconds, out string invalidPart)
    {
        invalidPart = string.Empty;
        if (string.IsNullOrWhiteSpace(text))
        {
            return new List<VideoOcrTimeRange> { new(0, durationSeconds) };
        }

        var ranges = new List<VideoOcrTimeRange>();
        var position = 0;
        foreach (Match match in RangeRegex.Matches(text))
        {
            var gap = text.Substring(position, match.Index - position);
            if (!SeparatorRegex.IsMatch(gap))
            {
                invalidPart = gap.Trim(' ', ';', ',');
                return null;
            }

            position = match.Index + match.Length;

            var startGroup = match.Groups["start"];
            var endGroup = match.Groups["end"];
            if (!startGroup.Success && !endGroup.Success)
            {
                invalidPart = match.Value.Trim();
                return null;
            }

            var start = startGroup.Success ? ParseTime(startGroup.Value) : 0;
            var end = endGroup.Success ? ParseTime(endGroup.Value) : durationSeconds;
            if (durationSeconds > 0)
            {
                end = Math.Min(end, durationSeconds);
            }

            if (start >= end)
            {
                invalidPart = match.Value.Trim();
                return null;
            }

            ranges.Add(new VideoOcrTimeRange(start, end));
        }

        var rest = text.Substring(position);
        if (!SeparatorRegex.IsMatch(rest))
        {
            invalidPart = rest.Trim(' ', ';', ',');
            return null;
        }

        if (ranges.Count == 0)
        {
            invalidPart = text.Trim();
            return null;
        }

        var merged = new List<VideoOcrTimeRange>();
        foreach (var range in ranges.OrderBy(p => p.StartSeconds))
        {
            if (merged.Count > 0 && range.StartSeconds <= merged[^1].EndSeconds)
            {
                var last = merged[^1];
                merged[^1] = last with { EndSeconds = Math.Max(last.EndSeconds, range.EndSeconds) };
            }
            else
            {
                merged.Add(range);
            }
        }

        return merged;
    }

    /// <summary>
    /// Adds a time to the field: it ends an open range ("16:00-" becomes "16:00-32:00"),
    /// otherwise it starts a new one ("16:00-32:00; 36:00-").
    /// </summary>
    public static string AppendTime(string? text, double seconds)
    {
        var time = FormatTime(seconds);
        var trimmed = (text ?? string.Empty).TrimEnd();
        if (trimmed.Length == 0)
        {
            return time + "-";
        }

        if (trimmed.EndsWith('-') || trimmed.EndsWith('–'))
        {
            return trimmed + time;
        }

        return trimmed.TrimEnd(';', ',', ' ') + "; " + time + "-";
    }

    public static string FormatTime(double seconds)
    {
        var timeSpan = TimeSpan.FromSeconds(Math.Max(0, Math.Round(seconds)));
        return timeSpan.TotalHours >= 1
            ? timeSpan.ToString(@"h\:mm\:ss", CultureInfo.InvariantCulture)
            : timeSpan.ToString(@"m\:ss", CultureInfo.InvariantCulture);
    }

    private static double ParseTime(string value)
    {
        var fraction = 0.0;
        var fractionIndex = value.IndexOfAny(new[] { '.', ',' });
        if (fractionIndex >= 0)
        {
            var digits = value.Substring(fractionIndex + 1);
            fraction = int.Parse(digits, CultureInfo.InvariantCulture) / Math.Pow(10, digits.Length);
            value = value.Substring(0, fractionIndex);
        }

        var seconds = 0.0;
        foreach (var part in value.Split(':'))
        {
            seconds = seconds * 60 + double.Parse(part, CultureInfo.InvariantCulture);
        }

        return seconds + fraction;
    }
}
