using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Core.Forms
{
    public static class DurationsBridgeGaps
    {
        public static int BridgeGaps(Subtitle subtitle, int minMsBetweenLines, bool divideEven, double maxMs, List<int> fixedIndexes, Dictionary<string, string> dic)
        {
            int fixedCount = 0;
            if (minMsBetweenLines > maxMs)
            {
                string message = $"{nameof(DurationsBridgeGaps)}: {nameof(minMsBetweenLines)} cannot be smaller than {nameof(maxMs)}!";
                SeLogger.Error(new InvalidOperationException(message), message);
                return 0;
            }

            int count = subtitle.Paragraphs.Count - 1;
            for (int i = 0; i < count; i++)
            {
                var cur = subtitle.Paragraphs[i];
                var next = subtitle.Paragraphs[i + 1];

                double currentGaps = next.StartTime.TotalMilliseconds - cur.EndTime.TotalMilliseconds;

                // there shouldn't be adjustment if current gaps is shorter than minimum gaps or greater than maximum gaps
                if (currentGaps < minMsBetweenLines || currentGaps > maxMs)
                {
                    continue;
                }

                // next paragraph start-time will be pull to try to meet the current paragraph
                if (divideEven)
                {
                    next.StartTime.TotalMilliseconds -= currentGaps / 2.0;
                }

                cur.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - minMsBetweenLines;
                if (fixedIndexes != null)
                {
                    fixedIndexes.Add(i);
                    fixedIndexes.Add(i + 1);
                }
                fixedCount++;

                double newGaps = next.StartTime.TotalMilliseconds - cur.EndTime.TotalMilliseconds;
                dic?.Add(cur.ID, $"{currentGaps / TimeCode.BaseUnit:0.000} => {newGaps / TimeCode.BaseUnit:0.000}");
            }

            return fixedCount;
        }
    }
}
