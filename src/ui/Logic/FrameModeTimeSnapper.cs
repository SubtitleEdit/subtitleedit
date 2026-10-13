using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Keeps edited time codes on frames while frame mode is on. Many edits produce times between
/// frames - the video position, a Shift+click in the waveform, a split point, "previous end + 1 ms"
/// - and frame mode hides that, so the stray milliseconds only show up later in gaps, durations
/// and saved files (users ran "Snap all times to frames" by hand to clean up).
/// </summary>
public static class FrameModeTimeSnapper
{
    /// <summary>
    /// The frame time the frame-mode display shows for <paramref name="milliseconds"/>, i.e. what
    /// typing the displayed HH:MM:SS:FF back in stores. This is whole seconds plus the frame within
    /// the second, the same grid frame-based formats save - not "frame number × frame duration",
    /// which drifts from it by up to a frame at 23.976/29.97.
    /// </summary>
    public static TimeSpan Snap(TimeSpan time)
    {
        if (time < TimeSpan.Zero || time.IsMaxTime())
        {
            return time;
        }

        // Frame numbers count frames from zero, which at 23.976/29.97 is not the same grid as
        // HH:MM:SS:FF - snap to the one the user sees, or a typed frame number would move.
        if (Config.Se.Settings.General.UseFrameNumbers)
        {
            return FrameNumbers.Snap(time);
        }

        // Already on a video frame (the video position, a frame step): keep it - near the edge
        // of a time code frame two video frames can show the same hh:mm:ss:ff.
        if (IsSnappingToVideoFrames() && FrameNumbers.Snap(time) == time)
        {
            return time;
        }

        var timeCode = new TimeCode(time.TotalMilliseconds);
        return ToVideoFrame(TimeSpan.FromMilliseconds(TimeCode.ParseHHMMSSFFToMilliseconds(timeCode.ToHHMMSSFF())));
    }

    /// <summary>
    /// With "snap times to video frames" on, moves a time on the hh:mm:ss:ff grid (whole seconds
    /// plus the frame within the second) to the video frame nearest to it (#16007), the frame a
    /// Blu-ray sup export puts it on. Every video frame keeps its own hh:mm:ss:ff, so typing the
    /// displayed time code back in lands on the same frame. Otherwise the time is returned as is.
    /// </summary>
    public static TimeSpan ToVideoFrame(TimeSpan time)
    {
        if (!IsSnappingToVideoFrames() || time < TimeSpan.Zero || time.IsMaxTime())
        {
            return time;
        }

        // The nearest video frame, unless that shows as another hh:mm:ss:ff - the time code's
        // whole milliseconds can sit right between two frames, and the neighbour is the one with
        // this time code.
        var timeCode = new TimeCode(time.TotalMilliseconds).ToHHMMSSFF();
        var nearest = FrameNumbers.FromMilliseconds(time.TotalMilliseconds);
        foreach (var frame in new[] { nearest, nearest - 1, nearest + 1 })
        {
            var frameMs = FrameNumbers.ToMilliseconds(frame);
            if (frame >= 0 && new TimeCode(frameMs).ToHHMMSSFF() == timeCode)
            {
                return TimeSpan.FromMilliseconds(frameMs);
            }
        }

        return TimeSpan.FromMilliseconds(FrameNumbers.ToMilliseconds(nearest));
    }

    private static bool IsSnappingToVideoFrames()
    {
        var general = Config.Se.Settings.General;
        return general.FrameModeSnapToVideoFrames && !general.UseFrameNumbers &&
               Configuration.Settings.General.CurrentFrameRate >= 1;
    }

    /// <summary>
    /// Snaps the times of the lines that are new or were re-timed since <paramref name="recorded"/>
    /// (the last undo snapshot). Lines the user did not touch keep their times, so opening a file
    /// never changes it. A line never collapses to zero frames by snapping.
    /// </summary>
    /// <returns>The number of lines changed.</returns>
    public static int SnapChangedLines(IReadOnlyList<SubtitleLineViewModel> lines, IReadOnlyList<SubtitleLineViewModel> recorded)
    {
        // The display (and so Snap) uses the libse frame rate; the toolbar keeps it in sync.
        var frameRate = Configuration.Settings.General.CurrentFrameRate;
        if (frameRate < 1)
        {
            return 0;
        }

        var recordedTimes = new Dictionary<Guid, (TimeSpan Start, TimeSpan End)>(recorded.Count);
        foreach (var p in recorded)
        {
            recordedTimes[p.Id] = (p.StartTime, p.EndTime);
        }

        var changed = 0;
        foreach (var line in lines)
        {
            if (line.IsReferenceOnly ||
                (recordedTimes.TryGetValue(line.Id, out var old) && old.Start == line.StartTime && old.End == line.EndTime))
            {
                continue;
            }

            var start = Snap(line.StartTime);
            var end = Snap(line.EndTime);
            if (end <= start && line.EndTime > line.StartTime)
            {
                end = Snap(start + TimeSpan.FromMilliseconds(1000.0 / frameRate));
            }

            if (start != line.StartTime || end != line.EndTime)
            {
                line.SetTimes(start, end);
                changed++;
            }
        }

        return changed;
    }
}
