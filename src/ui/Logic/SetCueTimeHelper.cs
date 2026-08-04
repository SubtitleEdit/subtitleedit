using Nikse.SubtitleEdit.Features.Main;
using System;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Stamping the video position onto a line's start/end cue ("set start time" / "set end time").
/// The commands used to bail out silently when the playhead was on the wrong side of the line's
/// other cue (issue #13066) - the normal case when spotting forward, since a freshly inserted
/// line sits right after the previous one, well behind the video position. SE4 always stamped
/// the time and left a red negative duration; here the time is always stamped too, but the
/// opposite cue moves along so the line stays valid.
/// </summary>
public static class SetCueTimeHelper
{
    /// <summary>
    /// Sets the start time, keeping the end fixed. If the new start is at or after the end,
    /// the end moves along so the line keeps its duration (or the minimum display duration
    /// when the line had none).
    /// </summary>
    public static void SetStart(SubtitleLineViewModel line, TimeSpan start, int minimumDurationMs)
    {
        if (start < line.EndTime)
        {
            line.SetStartTimeOnly(start);
            return;
        }

        line.SetTimes(start, start + GetDuration(line, minimumDurationMs));
    }

    /// <summary>
    /// Sets the end time, keeping the start fixed. If the new end is at or before the start,
    /// the start moves along so the line keeps its duration (or the minimum display duration
    /// when the line had none), clamped at zero.
    /// </summary>
    public static void SetEnd(SubtitleLineViewModel line, TimeSpan end, int minimumDurationMs)
    {
        if (end > line.StartTime)
        {
            line.EndTime = end;
            return;
        }

        var start = end - GetDuration(line, minimumDurationMs);
        if (start < TimeSpan.Zero)
        {
            start = TimeSpan.Zero;
        }

        line.SetTimes(start, end);
    }

    private static TimeSpan GetDuration(SubtitleLineViewModel line, int minimumDurationMs)
    {
        var duration = line.EndTime - line.StartTime;
        return duration > TimeSpan.Zero ? duration : TimeSpan.FromMilliseconds(minimumDurationMs);
    }
}
