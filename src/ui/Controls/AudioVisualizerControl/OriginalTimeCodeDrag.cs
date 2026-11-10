using Nikse.SubtitleEdit.Features.Main;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Controls.AudioVisualizerControl;

/// <summary>Which part of an original block a drag holds.</summary>
internal enum OriginalDragEdge
{
    Start,
    End,
    Move,
}

/// <summary>
/// Dragging the original subtitle's own time codes (<see cref="SubtitleLineViewModel.OriginalStartTime"/>),
/// shared by the two places that do it: the original row of the editor-style layout's
/// <see cref="TimelineTracks"/>, and the waveform itself while it edits the original
/// (<see cref="AudioVisualizer.EditOriginalTimeCodes"/>). Both work on rows - the original line a
/// row stands for - so a drag reaches the same place either way.
/// </summary>
internal static class OriginalTimeCodeDrag
{
    /// <summary>How close to a block's edge, in pixels, a press resizes instead of moves.</summary>
    public const double EdgeGrabPixels = 6;

    public static readonly TimeSpan MinDuration = TimeSpan.FromMilliseconds(100);

    /// <summary>
    /// The row whose original block is at <paramref name="seconds"/>, and which part of it. An
    /// edge within <paramref name="toleranceSeconds"/> wins over the middle of a block, and the
    /// nearest edge wins over a farther one, so two touching blocks can still be told apart.
    /// </summary>
    public static SubtitleLineViewModel? HitTest(
        IReadOnlyList<SubtitleLineViewModel> rows,
        Func<SubtitleLineViewModel, bool> hasOriginalLine,
        double seconds,
        double toleranceSeconds,
        out OriginalDragEdge edge)
    {
        edge = OriginalDragEdge.Move;
        SubtitleLineViewModel? best = null;
        var bestDistance = double.MaxValue;
        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            if (!hasOriginalLine(row))
            {
                continue;
            }

            var startSeconds = row.EffectiveOriginalStartTime.TotalSeconds;
            var endSeconds = row.EffectiveOriginalEndTime.TotalSeconds;
            if (seconds < startSeconds - toleranceSeconds || seconds > endSeconds + toleranceSeconds)
            {
                continue;
            }

            var toStart = Math.Abs(seconds - startSeconds);
            var toEnd = Math.Abs(seconds - endSeconds);
            OriginalDragEdge rowEdge;
            double distance;
            if (toStart <= toleranceSeconds && toStart <= toEnd)
            {
                rowEdge = OriginalDragEdge.Start;
                distance = toStart;
            }
            else if (toEnd <= toleranceSeconds)
            {
                rowEdge = OriginalDragEdge.End;
                distance = toEnd;
            }
            else
            {
                rowEdge = OriginalDragEdge.Move;
                distance = toleranceSeconds;
            }

            if (distance < bestDistance)
            {
                best = row;
                edge = rowEdge;
                bestDistance = distance;
            }
        }

        return best;
    }

    /// <summary>
    /// The time codes after dragging <paramref name="edge"/> of a block that started out at
    /// <paramref name="start"/>-<paramref name="end"/> by <paramref name="delta"/>: never before
    /// zero, and never shorter than <see cref="MinDuration"/>.
    /// </summary>
    public static (TimeSpan Start, TimeSpan End) Apply(TimeSpan start, TimeSpan end, TimeSpan delta, OriginalDragEdge edge)
    {
        switch (edge)
        {
            case OriginalDragEdge.Start:
                return (TimeSpan.FromTicks(Math.Max(0, Math.Min((start + delta).Ticks, (end - MinDuration).Ticks))), end);
            case OriginalDragEdge.End:
                return (start, TimeSpan.FromTicks(Math.Max((end + delta).Ticks, (start + MinDuration).Ticks)));
            default:
                if (start + delta < TimeSpan.Zero)
                {
                    delta = -start;
                }

                return (start + delta, end + delta);
        }
    }
}
