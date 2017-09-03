using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Core.Forms
{
    public static class DurationsBridgeGaps
    {
        public static int BridgeGaps(Subtitle fixedSubtitle, int minMsBetweenLines, bool divideEven, double maxMs, List<int> fixedIndexes, Dictionary<string, string> dic)
        {
            int fixedCount = 0;
            for (int i = 0; i < fixedSubtitle.Paragraphs.Count - 1; i++)
            {
                Paragraph cur = fixedSubtitle.Paragraphs[i];
                Paragraph next = fixedSubtitle.Paragraphs[i + 1];
                string before = null;
                var difMs = Math.Abs(cur.EndTime.TotalMilliseconds - next.StartTime.TotalMilliseconds);
                if (difMs < maxMs && difMs > minMsBetweenLines && maxMs > minMsBetweenLines)
                {
                    before = $"{(next.StartTime.TotalMilliseconds - cur.EndTime.TotalMilliseconds) / TimeCode.BaseUnit:0.000}";
                    if (divideEven && next.StartTime.TotalMilliseconds > cur.EndTime.TotalMilliseconds)
                    {
                        double half = (next.StartTime.TotalMilliseconds - cur.EndTime.TotalMilliseconds) / 2.0;
                        next.StartTime.TotalMilliseconds -= half;
                    }
                    cur.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - minMsBetweenLines;
                    if (fixedIndexes != null)
                    {
                        fixedIndexes.Add(i);
                        fixedIndexes.Add(i + 1);
                    }
                    fixedCount++;
                }
                var msToNext = next.StartTime.TotalMilliseconds - cur.EndTime.TotalMilliseconds;
                if (msToNext < 2000)
                {
                    string info;
                    if (!string.IsNullOrEmpty(before))
                        info = $"{before} => {msToNext / TimeCode.BaseUnit:0.000}";
                    else
                        info = $"{msToNext / TimeCode.BaseUnit:0.000}";
                    dic?.Add(cur.ID, info);
                }
            }
            return fixedCount;
        }
    }
}
