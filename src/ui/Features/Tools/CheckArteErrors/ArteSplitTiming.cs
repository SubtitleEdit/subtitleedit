using System;
using System.Collections.Generic;
using System.Linq;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Features.Tools.CheckArteErrors;

internal static class ArteSplitTiming
{
    // Allocate whole 25-fps frames, reserving five frames for each internal gap.
    internal static bool TryFit(IReadOnlyList<Paragraph> parts, double startMs, double endMs,
        double minimumMs, double maximumMs, double maxCps, out string error)
    {
        error = string.Empty;
        if (parts.Count == 0 || !double.IsFinite(startMs) || !double.IsFinite(endMs) || endMs <= startMs)
        {
            error = "The original time range is invalid.";
            return false;
        }
        // Same conversion as Fit selected subtitles to time range: round to whole frames.
        startMs = Math.Round(startMs / 40, MidpointRounding.AwayFromZero) * 40;
        endMs = Math.Round(endMs / 40, MidpointRounding.AwayFromZero) * 40;
        var available = (int)Math.Round((endMs - startMs) / 40) - 5 * (parts.Count - 1);
        var weights = parts.Select(p => Math.Max(1, HtmlUtil.RemoveHtmlTags(p.Text, true)
            .Count(c => c != '\r' && c != '\n'))).ToArray();
        var durations = weights.Select(count => (int)Math.Ceiling(Math.Max(40,
            Math.Max(minimumMs, maxCps > 0 ? count / maxCps * 1000 : 0)) / 40)).ToArray();
        var maximum = maximumMs > 0 ? (int)Math.Floor(maximumMs / 40) : Math.Max(1, available);
        if (available < parts.Count)
        {
            error = "Not enough frames for one frame per subtitle plus five-frame gaps.";
            return false;
        }
        var needsFit = durations.Any(d => d > maximum) || durations.Sum() > available ||
                       (long)maximum * parts.Count < available;
        if (needsFit)
        {
            // Fit-in-range fallback: distribute the available frames proportionally,
            // retaining at least one frame per subtitle and all five-frame gaps.
            var budget = available - parts.Count;
            var totalWeight = weights.Sum();
            var exact = weights.Select(w => budget * (double)w / totalWeight).ToArray();
            durations = exact.Select(value => 1 + (int)Math.Floor(value)).ToArray();
            var left = available - durations.Sum();
            foreach (var i in Enumerable.Range(0, parts.Count)
                         .OrderByDescending(i => exact[i] - Math.Floor(exact[i])).ThenBy(i => i).Take(left))
            {
                durations[i]++;
            }
            error = "Zeitlich knappe Darstellung (Fit in TC range).";
            maximum = available;
        }
        // Give each remaining frame to the part with the shortest duration per character,
        // without exceeding the maximum duration. Minimum durations are never compressed.
        for (var remaining = available - durations.Sum(); remaining > 0; remaining--)
        {
            var best = -1;
            for (var i = 0; i < parts.Count; i++)
            {
                if (durations[i] < maximum &&
                    (best < 0 || (double)durations[i] / weights[i] < (double)durations[best] / weights[best]))
                {
                    best = i;
                }
            }
            durations[best]++;
        }
        var cursor = startMs;
        for (var i = 0; i < parts.Count; i++)
        {
            parts[i].StartTime = new TimeCode(cursor);
            parts[i].EndTime = new TimeCode(cursor + durations[i] * 40);
            cursor += durations[i] * 40 + 200;
        }
        return true;
    }
}
