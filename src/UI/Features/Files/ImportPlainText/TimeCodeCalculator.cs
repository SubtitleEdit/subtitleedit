using Nikse.SubtitleEdit.Features.Main;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Files.ImportPlainText;

public static class TimeCodeCalculator
{
    public static List<SubtitleLineViewModel> CalculateTimeCodes(
        List<SubtitleLineViewModel> subtitles,
        double subtitleOptimalCharactersPerSeconds,
        double subtitleMaximumCharactersPerSeconds,
        int minimumMillisecondsBetweenLines,
        int subtitleMinimumDisplayMilliseconds,
        int subtitleMaximumDisplayMilliseconds)
    {
        var currentTimeMs = 0;

        foreach (var subtitle in subtitles)
        {
            var characterCount = subtitle.Text.Length;

            // Calculate duration based on optimal reading speed (characters per second)
            var durationMs = (int)(characterCount / subtitleOptimalCharactersPerSeconds * 1000);

            // Clamp duration between minimum and maximum display time
            durationMs = Math.Max(subtitleMinimumDisplayMilliseconds, Math.Min(subtitleMaximumDisplayMilliseconds, durationMs));

            subtitle.StartTime = TimeSpan.FromMilliseconds(currentTimeMs);
            subtitle.EndTime = TimeSpan.FromMilliseconds(currentTimeMs + durationMs);

            // Move to next subtitle start time (current end time + gap)
            currentTimeMs += durationMs + minimumMillisecondsBetweenLines;
        }

        return subtitles;
    }
}
