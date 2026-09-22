using System;
using System.Collections.Generic;
using System.Linq;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Features.Tools.CheckArteErrors;

internal static class ArteSplitTiming
{
    // Allocate whole 25-fps frames, reserving the configured gap for each internal gap.
    internal static bool TryFit(IReadOnlyList<Paragraph> parts, double startMs, double endMs,
        double minimumMs, double maximumMs, double maxCps, int minimumGapFrames,
        double readingDurationTolerancePercent, bool acceptShortDurations, int shortMinimumFrames,
        out string error)
    {
        error = string.Empty;
        if (parts.Count == 0 || !double.IsFinite(startMs) || !double.IsFinite(endMs) || endMs <= startMs)
        {
            error = "The original time range is invalid.";
            return false;
        }

        startMs = Math.Round(startMs / 40, MidpointRounding.AwayFromZero) * 40;
        endMs = Math.Round(endMs / 40, MidpointRounding.AwayFromZero) * 40;
        minimumGapFrames = Math.Max(0, minimumGapFrames);
        shortMinimumFrames = Math.Max(1, shortMinimumFrames);
        readingDurationTolerancePercent = Math.Max(0, readingDurationTolerancePercent);

        var available = (int)Math.Round((endMs - startMs) / 40) - minimumGapFrames * (parts.Count - 1);
        var weights = parts.Select(p => Math.Max(1, HtmlUtil.RemoveHtmlTags(p.Text, true)
            .Count(c => c != '\r' && c != '\n'))).ToArray();
        var requiredFrames = weights.Select(count =>
        {
            var readingMinimum = maxCps > 0 ? count / maxCps * 1000.0 : 0;
            return Math.Max(1, (int)Math.Ceiling(Math.Max(minimumMs, readingMinimum) / 40.0));
        }).ToArray();
        var minimumAcceptedFrames = requiredFrames.Select(required => acceptShortDurations
            ? shortMinimumFrames
            : Math.Max(1, (int)Math.Ceiling(required * Math.Max(0, 1.0 - readingDurationTolerancePercent / 100.0))))
            .ToArray();

        if (available < minimumAcceptedFrames.Sum())
        {
            error = acceptShortDurations
                ? $"Not enough time for {shortMinimumFrames}-frame subtitle minimums plus {minimumGapFrames}-frame gaps."
                : $"Not enough time for the tolerated subtitle minimums plus {minimumGapFrames}-frame gaps.";
            return false;
        }

        var maximum = maximumMs > 0 ? Math.Max(1, (int)Math.Floor(maximumMs / 40.0)) : Math.Max(1, available);
        var durations = requiredFrames.Select(required => Math.Min(required, maximum)).ToArray();
        if (requiredFrames.Any(required => required > maximum) || durations.Sum() > available)
        {
            durations = minimumAcceptedFrames.ToArray();
            var remaining = available - durations.Sum();
            var desiredExtra = requiredFrames.Select((required, i) => Math.Max(0, Math.Min(required, maximum) - durations[i])).ToArray();
            while (remaining > 0 && desiredExtra.Any(extra => extra > 0))
            {
                var best = Enumerable.Range(0, parts.Count).Where(i => desiredExtra[i] > 0)
                    .OrderBy(i => (double)durations[i] / weights[i]).ThenBy(i => i).First();
                durations[best]++;
                desiredExtra[best]--;
                remaining--;
            }
            error = "Zeitlich knappe Darstellung (Fit in TC range).";
        }

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
            if (best < 0)
            {
                break;
            }
            durations[best]++;
        }

        var cursor = startMs;
        for (var i = 0; i < parts.Count; i++)
        {
            parts[i].StartTime = new TimeCode(cursor);
            parts[i].EndTime = new TimeCode(cursor + durations[i] * 40);
            cursor += durations[i] * 40 + minimumGapFrames * 40;
        }
        return true;
    }
}
