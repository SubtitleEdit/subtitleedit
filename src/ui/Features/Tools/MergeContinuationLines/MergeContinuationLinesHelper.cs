using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.UiLogic.MergeContinuationLines;
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

            var gapMs = (next.StartTime - current.EndTime).TotalMilliseconds;
            if (!MergeContinuationLinesUtils.CanMerge(current.Text, next.Text, gapMs, lang, maxGapMs, maxTotalLength))
            {
                continue;
            }

            var merged = MergeContinuationLinesUtils.MergeText(current.Text, next.Text, lang);
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
                current.Text = MergeContinuationLinesUtils.MergeText(current.Text, next.Text, lang);
                current.EndTime = next.EndTime;
                current.UpdateDuration();
                j++;
            }
            result.Add(current);
            i = j + 1;
        }

        return result;
    }
}
