using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Logic.Media;

/// <summary>
/// Re-times a subtitle to match a video that has been cut with ffmpeg's
/// trim+concat filters (see FfmpegGenerator.GetMergeSegmentsParameters /
/// GetRemoveSegmentsParameters): lines are clipped to the kept ranges and
/// shifted onto the concatenated output timeline.
/// </summary>
public static class SubtitleSegmentCutter
{
    /// <summary>
    /// Keeps only the given ranges (the "merge segments" cut type): the output video is
    /// the segments concatenated, so each kept line is shifted by the summed duration of
    /// the segments before it. Lines spanning a segment edge are clipped to the segment.
    /// </summary>
    public static Subtitle KeepSegments(Subtitle subtitle, IReadOnlyList<(double StartSeconds, double EndSeconds)> segments)
    {
        var ordered = segments.Where(s => s.EndSeconds > s.StartSeconds).OrderBy(s => s.StartSeconds).ToList();
        var result = new Subtitle { Header = subtitle.Header, Footer = subtitle.Footer };

        var offsetSeconds = 0d;
        foreach (var (segmentStart, segmentEnd) in ordered)
        {
            foreach (var paragraph in subtitle.Paragraphs)
            {
                var clippedStart = Math.Max(paragraph.StartTime.TotalSeconds, segmentStart);
                var clippedEnd = Math.Min(paragraph.EndTime.TotalSeconds, segmentEnd);
                if (clippedEnd - clippedStart < 0.001)
                {
                    continue;
                }

                var p = new Paragraph(paragraph);
                p.StartTime.TotalMilliseconds = (offsetSeconds + clippedStart - segmentStart) * 1000.0;
                p.EndTime.TotalMilliseconds = (offsetSeconds + clippedEnd - segmentStart) * 1000.0;
                result.Paragraphs.Add(p);
            }

            offsetSeconds += segmentEnd - segmentStart;
        }

        result.Paragraphs.Sort((a, b) => a.StartTime.TotalMilliseconds.CompareTo(b.StartTime.TotalMilliseconds));
        result.Renumber();
        return result;
    }

    /// <summary>
    /// Removes the given ranges (the "cut segments" cut type): the output video is
    /// everything outside the segments, so this is <see cref="KeepSegments"/> over the
    /// complement of the ranges. <paramref name="totalDurationSeconds"/> caps the final
    /// kept range; when unknown (0) the last line's end time is used instead.
    /// </summary>
    public static Subtitle RemoveSegments(Subtitle subtitle, IReadOnlyList<(double StartSeconds, double EndSeconds)> segments, double totalDurationSeconds)
    {
        var ordered = segments.Where(s => s.EndSeconds > s.StartSeconds).OrderBy(s => s.StartSeconds).ToList();

        var lastParagraphEnd = subtitle.Paragraphs.Count > 0
            ? subtitle.Paragraphs.Max(p => p.EndTime.TotalSeconds)
            : 0;
        var endCap = Math.Max(totalDurationSeconds, lastParagraphEnd);

        var kept = new List<(double StartSeconds, double EndSeconds)>();
        var position = 0d;
        foreach (var (segmentStart, segmentEnd) in ordered)
        {
            if (segmentStart > position)
            {
                kept.Add((position, segmentStart));
            }

            position = Math.Max(position, segmentEnd);
        }

        if (endCap > position)
        {
            kept.Add((position, endCap));
        }

        return KeepSegments(subtitle, kept);
    }
}
