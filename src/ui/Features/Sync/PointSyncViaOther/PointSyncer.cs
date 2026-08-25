using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Sync.PointSyncViaOther;
public static class PointSyncer
{
    public static List<SubtitleLineViewModel> PointSync(List<SubtitleLineViewModel> subtitles, List<SyncPoint> syncPoints)
    {
        var result = subtitles.Select(s => new SubtitleLineViewModel(s)).ToList();

        if (syncPoints.Count == 1)
        {
            AdjustViaShowEarlierLater(result, syncPoints[0]);
            return result;
        }

        syncPoints = syncPoints.OrderBy(sp => sp.LeftIndex).ToList();

        var endIndex = -1;
        var minIndex = 0;

        for (var i = 0; i < syncPoints.Count; i++)
        {
            if (i == 0)
            {
                endIndex = syncPoints[i].LeftIndex;
            }
            else
            {
                var startIndex = endIndex;
                endIndex = syncPoints[i].LeftIndex;

                int maxIndex;
                if (i == syncPoints.Count - 1)
                {
                    maxIndex = subtitles.Count;
                }
                else
                {
                    maxIndex = syncPoints[i].LeftIndex;
                }

                Sync(
                    subtitles, 
                    result, 
                    startIndex, 
                    endIndex, 
                    minIndex, 
                    maxIndex, 
                    syncPoints[i - 1].RightStartTime.TotalMilliseconds / TimeCode.BaseUnit, 
                    syncPoints[i].RightStartTime.TotalMilliseconds / TimeCode.BaseUnit);

                minIndex = endIndex;
            }
        }

        return result;
    }

    private static void AdjustViaShowEarlierLater(List<SubtitleLineViewModel> result, SyncPoint syncPoint)
    {
        var diff = syncPoint.RightStartTime - syncPoint.LeftStartTime;
        for (var i = 0; i < result.Count; i++)
        {
            // Shift the whole line by the constant offset (start and end together).
            result[i].SetStartTimeKeepDuration(result[i].StartTime.Add(diff));
        }
    }

    private static void Sync(
        List<SubtitleLineViewModel> originalSubtitles,
        List<SubtitleLineViewModel> subtitles,
        int startIndex,
        int endIndex,
        int minIndex,
        int maxIndex,
        double startPos,
        double endPos)
    {
        if (endPos > startPos)
        {
            var subStart = originalSubtitles[startIndex].StartTime.TotalMilliseconds / TimeCode.BaseUnit;
            var subEnd = originalSubtitles[endIndex].StartTime.TotalMilliseconds / TimeCode.BaseUnit;

            var subDiff = subEnd - subStart;
            var realDiff = endPos - startPos;

            // speed factor
            var factor = Math.Abs(subDiff) < 0.001 ? 1 : realDiff / subDiff;

            // adjust to starting position
            var adjust = startPos - subStart * factor;

            for (var i = minIndex; i < subtitles.Count && i <= maxIndex; i++)
            {
                var p = subtitles[i];
                p.StartTime = originalSubtitles[i].StartTime;
                p.EndTime = originalSubtitles[i].EndTime;
                p.Adjust(factor, adjust);
            }

            // Adjust rounds start and duration to whole milliseconds separately, which can land
            // a line's end 1 ms past the next line's start, turning a clean join into an overlap
            // the source never had. Clip those back; source overlaps are left as they were.
            for (var i = minIndex; i + 1 < subtitles.Count && i + 1 <= maxIndex; i++)
            {
                var current = subtitles[i];
                var next = subtitles[i + 1];
                if (originalSubtitles[i].EndTime <= originalSubtitles[i + 1].StartTime &&
                    current.EndTime > next.StartTime)
                {
                    current.SetTimes(current.StartTime, next.StartTime);
                }
            }
        }
    }
}