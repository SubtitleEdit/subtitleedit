using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Core.Forms
{
    public static class DurationsBridgeGaps
    {
        public static int BridgeGaps(Subtitle subtitle, int minMsBetweenLines, bool divideEven, double maxMs, List<int> fixedIndexes, Dictionary<string, string> dic, bool useFrames)
        {
            int fixedCount = 0;
            if (minMsBetweenLines > maxMs)
            {
                string message = $"{nameof(DurationsBridgeGaps)}: {nameof(minMsBetweenLines)} cannot be larger than {nameof(maxMs)}!";
                SeLogger.Error(new InvalidOperationException(message), message);
                return 0;
            }

            int count = subtitle.Paragraphs.Count - 1;
            for (int i = 0; i < count; i++)
            {
                var cur = subtitle.Paragraphs[i];
                var next = subtitle.Paragraphs[i + 1];

                double currentGap = next.StartTime.TotalMilliseconds - cur.EndTime.TotalMilliseconds;

                // there shouldn't be adjustment if current gaps is shorter or equal than minimum gap or greater than maximum gaps
                if (currentGap <= minMsBetweenLines || currentGap > maxMs)
                {
                    continue;
                }

                // next paragraph start-time will be pull to try to meet the current paragraph
                if (divideEven)
                {
                    next.StartTime.TotalMilliseconds -= currentGap / 2.0;
                }

                cur.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - minMsBetweenLines;
                if (fixedIndexes != null)
                {
                    fixedIndexes.Add(i);
                    fixedIndexes.Add(i + 1);
                }
                fixedCount++;

                double newGap = next.StartTime.TotalMilliseconds - cur.EndTime.TotalMilliseconds;
                if (useFrames)
                {
                    dic?.Add(cur.Id, $"{SubtitleFormat.MillisecondsToFrames(currentGap)} => {SubtitleFormat.MillisecondsToFrames(newGap)}");
                }
                else
                {
                    dic?.Add(cur.Id, $"{currentGap / TimeCode.BaseUnit:0.000} => {newGap / TimeCode.BaseUnit:0.000}");
                }
            }

            return fixedCount;
        }
    }
}
