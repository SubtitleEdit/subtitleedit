using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Tools.MergeContinuationLines;

public static class MergeContinuationLinesHelper
{
    public static List<MergeContinuationLinesCandidate> Detect(
        IReadOnlyList<SubtitleLineViewModel> subtitles,
        string? language,
        int maxGapMs,
        int maxTotalLength)
    {
        var result = new List<MergeContinuationLinesCandidate>();
        if (subtitles == null || subtitles.Count < 2)
        {
            return result;
        }

        var lang = string.IsNullOrWhiteSpace(language) ? "en" : language!;
        for (var i = 0; i < subtitles.Count - 1; i++)
        {
            var current = subtitles[i];
            var next = subtitles[i + 1];

            var p = current.ToParagraph();
            var nextP = next.ToParagraph();

            if (!Utilities.QualifiesForMerge(p, nextP, maxGapMs, maxTotalLength, onlyContinuationLines: true))
            {
                continue;
            }

            // Skip dialog lines ("- A" / "- B") — these are two speakers, not continuation.
            if (StartsWithDashSpeaker(current.Text) || StartsWithDashSpeaker(next.Text))
            {
                continue;
            }

            var merged = MergeText(current.Text, next.Text, lang);
            result.Add(new MergeContinuationLinesCandidate
            {
                Index = i,
                Number = i + 1,
                NextNumber = i + 2,
                Text1 = current.Text ?? string.Empty,
                Text2 = next.Text ?? string.Empty,
                MergedText = merged,
                IsSelected = true,
            });
        }

        return result;
    }

    public static List<SubtitleLineViewModel> Apply(
        IReadOnlyList<SubtitleLineViewModel> subtitles,
        IReadOnlyList<MergeContinuationLinesCandidate> candidates,
        string? language)
    {
        var lang = string.IsNullOrWhiteSpace(language) ? "en" : language!;
        var selectedByFirstIndex = new HashSet<int>();
        foreach (var c in candidates)
        {
            if (c.IsSelected)
            {
                selectedByFirstIndex.Add(c.Index);
            }
        }

        var result = new List<SubtitleLineViewModel>(subtitles.Count);
        var i = 0;
        while (i < subtitles.Count)
        {
            var current = new SubtitleLineViewModel(subtitles[i]);
            var j = i;
            while (j + 1 < subtitles.Count && selectedByFirstIndex.Contains(j))
            {
                var next = subtitles[j + 1];
                var combined = (current.Text ?? string.Empty).TrimEnd() + " " + (next.Text ?? string.Empty).TrimStart();
                current.Text = Utilities.AutoBreakLine(combined, lang);
                current.EndTime = next.EndTime;
                current.UpdateDuration();
                j++;
            }
            result.Add(current);
            i = j + 1;
        }

        return result;
    }

    private static bool StartsWithDashSpeaker(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return false;
        }

        var sanitized = HtmlUtil.RemoveHtmlTags(text!, true).TrimStart();
        if (sanitized.Length == 0)
        {
            return false;
        }

        var c = sanitized[0];
        return c == '-' || c == '‐' || c == '–' || c == '—';
    }

    private static string MergeText(string? a, string? b, string language)
    {
        var combined = (a ?? string.Empty).TrimEnd() + " " + (b ?? string.Empty).TrimStart();
        return Utilities.AutoBreakLine(combined, language);
    }
}
