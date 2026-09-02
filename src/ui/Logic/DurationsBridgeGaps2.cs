using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Forms;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Logic;

public static class DurationsBridgeGaps2
{
    public static int BridgeGaps(
        ObservableCollection<SubtitleLineViewModel> subtitles, 
        int minMsBetweenLines, 
        int percentageForPrevious, 
        double maxMs, 
        List<int> fixedIndexes, Dictionary<string, string> dic,
        bool useFrames)
    {
        var fixedCount = 0;
        if (minMsBetweenLines > maxMs)
        {
            var message = $"{nameof(DurationsBridgeGaps)}: {nameof(minMsBetweenLines)} cannot be larger than {nameof(maxMs)}!";
            Se.LogError(message);
            return 0;
        }

        var count = subtitles.Count - 1;
        for (var i = 0; i < count; i++)
        {
            var cur = subtitles[i];
            var next = subtitles[i + 1];

            var currentGap = next.StartTime.TotalMilliseconds - cur.EndTime.TotalMilliseconds;

            // there shouldn't be adjustment if current gaps is shorter or equal than minimum gap or greater than maximum gaps
            if (currentGap <= minMsBetweenLines || currentGap > maxMs)
            {
                continue;
            }

            // Calculate how much of the gap to distribute between subtitles
            var gapToBridge = currentGap - minMsBetweenLines;
            var previousExtension = (gapToBridge * percentageForPrevious) / 100.0;
            var nextReduction = gapToBridge - previousExtension;

            // Round to a whole millisecond at the producer (#14056). previousExtension is
            // fractional whenever the gap does not divide evenly - gap 101 ms, min 24, 50% for
            // the left gives an end time of X.5 ms, which the grid truncates and the format
            // writers then save a millisecond early.
            cur.EndTime = TimeSpanExtensions.FromMillisecondsWholeMilliseconds(cur.EndTime.TotalMilliseconds + previousExtension);

            // Move the next subtitle's start time backward by the calculated amount
            next.SetStartTimeOnly(TimeSpanExtensions.FromMillisecondsWholeMilliseconds(next.StartTime.TotalMilliseconds - nextReduction));

            if (fixedIndexes != null)
            {
                fixedIndexes.Add(i);
                fixedIndexes.Add(i + 1);
            }
            fixedCount++;

            var newGap = next.StartTime.TotalMilliseconds - cur.EndTime.TotalMilliseconds;
            if (useFrames)
            {
                dic?.Add(cur.Id.ToString(), $"{SubtitleFormat.MillisecondsToFrames(currentGap)} => {SubtitleFormat.MillisecondsToFrames(newGap)}");
            }
            else
            {
                dic?.Add(cur.Id.ToString(), $"{currentGap / TimeCode.BaseUnit:0.000} => {newGap / TimeCode.BaseUnit:0.000}");
            }
        }

        return fixedCount;
    }
}