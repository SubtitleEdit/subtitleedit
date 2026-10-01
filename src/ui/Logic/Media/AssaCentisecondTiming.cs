using Nikse.SubtitleEdit.Core.Common;
using System;

namespace Nikse.SubtitleEdit.Logic.Media;

/// <summary>
/// ASSA stores times in centiseconds and the writer rounds to the nearest one, so a
/// frame-aligned millisecond time like 0.167 s (frame 4 at 23.976 fps, pts 0.16683 s)
/// became 0.17 s - after the frame's timestamp, so libass skipped the first frame of
/// the subtitle. That hit about every other line at 23.976/24/29.97/59.94 fps (issue
/// #15520). Rounding both times down instead keeps the first frame (the start moves
/// less than 10 ms earlier, never back onto the previous frame below ~95 fps) and the
/// last one (the end frame stays excluded, the frame before it stays included).
/// Only for temp files handed to mpv/ffmpeg - never for a saved .ass.
/// </summary>
public static class AssaCentisecondTiming
{
    // Absorbs floating point noise like 1129.9999999 so it floors to 1130, not 1120.
    private const double Epsilon = 0.000001;

    /// <summary>
    /// A paragraph whose times change is replaced by a copy rather than edited: the merged
    /// secondary subtitle puts the caller's own paragraphs into the preview subtitle.
    /// </summary>
    public static void FloorToCentiseconds(Subtitle subtitle)
    {
        var paragraphs = subtitle.Paragraphs;
        for (var i = 0; i < paragraphs.Count; i++)
        {
            var paragraph = paragraphs[i];
            var start = FloorToCentiseconds(paragraph.StartTime);
            var end = FloorToCentiseconds(paragraph.EndTime);
            if (start == paragraph.StartTime.TotalMilliseconds && end == paragraph.EndTime.TotalMilliseconds)
            {
                continue;
            }

            var copy = new Paragraph(paragraph, false);
            copy.StartTime.TotalMilliseconds = start;
            copy.EndTime.TotalMilliseconds = end;
            paragraphs[i] = copy;
        }
    }

    private static double FloorToCentiseconds(TimeCode timeCode)
    {
        return timeCode.IsMaxTime ? timeCode.TotalMilliseconds : FloorToCentiseconds(timeCode.TotalMilliseconds);
    }

    public static double FloorToCentiseconds(double totalMilliseconds)
    {
        if (totalMilliseconds <= 0 || double.IsNaN(totalMilliseconds) || double.IsInfinity(totalMilliseconds))
        {
            return totalMilliseconds;
        }

        return Math.Floor(totalMilliseconds / 10.0 + Epsilon) * 10.0;
    }
}
