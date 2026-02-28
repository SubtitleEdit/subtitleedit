using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Tools.MergeShortLines;

public class MergeShortLinesResult
{
    public List<SubtitleLineViewModel> MergedSubtitles { get; }
    public List<MergeShortLinesItem> Fixes { get; }
    public int MergeCount { get; }

    public MergeShortLinesResult(List<SubtitleLineViewModel> mergedSubtitles, List<MergeShortLinesItem> fixes, int mergeCount)
    {
        MergedSubtitles = mergedSubtitles;
        Fixes = fixes;
        MergeCount = mergeCount;
    }
}

public static class MergeShortLinesHelper
{
    public static MergeShortLinesResult Merge(
        List<SubtitleLineViewModel> subtitles,
        List<double> shotChanges,
        int singleLineMaxLength,
        int maxNumberOfLines,
        int gapThresholdMs,
        int unbreakLinesShorterThan)
    {
        var fixes = new List<MergeShortLinesItem>();
        var mergeCount = 0;
        var maxCharactersPerSubtitle = maxNumberOfLines * singleLineMaxLength;

        var result = new List<SubtitleLineViewModel>(subtitles.Count);

        for (var index = 0; index < subtitles.Count; index++)
        {
            var baseVm = subtitles[index];
            var current = new SubtitleLineViewModel(baseVm);

            var j = index + 1;
            while (j < subtitles.Count)
            {
                var next = subtitles[j];

                // stop if there is a shot change between current and next
                var hasShotChangeBetween = shotChanges != null && shotChanges.Any(s =>
                    s > current.EndTime.TotalSeconds && s < next.StartTime.TotalSeconds);
                if (hasShotChangeBetween)
                {
                    break;
                }

                // Check temporal proximity
                var gapMs = next.StartTime.TotalMilliseconds - current.EndTime.TotalMilliseconds;
                if (gapMs > gapThresholdMs)
                {
                    break;
                }

                // Check combined plain length limit
                var combinedPlain = HtmlUtil.RemoveHtmlTags((current.Text ?? string.Empty) + " " + (next.Text ?? string.Empty), true)
                    .Replace("\r\n", " ").Replace('\n', ' ').Trim();
                if (combinedPlain.Length > maxCharactersPerSubtitle)
                {
                    break;
                }

                // Try to wrap combined text within singleLineMaxLength and maxNumberOfLines
                var language = string.IsNullOrWhiteSpace(baseVm.Language) ? "en" : baseVm.Language;
                var combinedRaw = (current.Text ?? string.Empty).TrimEnd() + " " + (next.Text ?? string.Empty).TrimStart();
                var wrapped = Utilities.AutoBreakLine(combinedRaw, singleLineMaxLength, unbreakLinesShorterThan, language);
                var lines = wrapped.SplitToLines();
                if (lines.Count > maxNumberOfLines)
                {
                    break;
                }

                // Check that each line doesn't exceed singleLineMaxLength
                var anyLineTooLong = lines.Any(line => HtmlUtil.RemoveHtmlTags(line, true).Length > singleLineMaxLength);
                if (anyLineTooLong)
                {
                    break;
                }

                // Merge
                current.Text = wrapped;
                current.EndTime = next.EndTime;
                current.UpdateDuration();
                mergeCount++;

                // fix item for this merge step
                var fix = new MergeShortLinesItem(
                    Se.Language.Tools.MergeShortLines.Title,
                    index + 1,
                    $"Merged line {j + 1} into {index + 1} - {current.Text.Replace(Environment.NewLine, " ⏎ ")}",
                    new SubtitleLineViewModel(current));
                fixes.Add(fix);

                j++;
            }

            result.Add(current);
            // Skip the lines we merged into current
            index = j - 1;
        }

        return new MergeShortLinesResult(result, fixes, mergeCount);
    }

    public static MergeShortLinesResult MergeWithHighlights(
        List<SubtitleLineViewModel> subtitles,
        List<double> shotChanges,
        int singleLineMaxLength,
        int maxNumberOfLines,
        int gapThresholdMs,
        int unbreakLinesShorterThan)
    {
        var fixes = new List<MergeShortLinesItem>();
        var mergeCount = 0;
        var maxCharactersPerSubtitle = maxNumberOfLines * singleLineMaxLength;

        var result = new List<SubtitleLineViewModel>(subtitles.Count);

        for (var index = 0; index < subtitles.Count; index++)
        {
            var baseVm = subtitles[index];

            // Collect all lines that can be merged together
            var mergeGroup = new List<SubtitleLineViewModel> { new SubtitleLineViewModel(baseVm) };
            var combinedText = (baseVm.Text ?? string.Empty).TrimEnd();

            var j = index + 1;
            while (j < subtitles.Count)
            {
                var next = subtitles[j];

                // stop if there is a shot change between current and next
                var lastInGroup = mergeGroup[^1];
                var hasShotChangeBetween = shotChanges != null && shotChanges.Any(s =>
                    s > lastInGroup.EndTime.TotalSeconds && s < next.StartTime.TotalSeconds);
                if (hasShotChangeBetween)
                {
                    break;
                }

                // Check temporal proximity
                var gapMs = next.StartTime.TotalMilliseconds - lastInGroup.EndTime.TotalMilliseconds;
                if (gapMs > gapThresholdMs)
                {
                    break;
                }

                // Check combined plain length limit
                var testCombinedPlain = HtmlUtil.RemoveHtmlTags(combinedText + " " + (next.Text ?? string.Empty), true)
                    .Replace("\r\n", " ").Replace('\n', ' ').Trim();
                if (testCombinedPlain.Length > maxCharactersPerSubtitle)
                {
                    break;
                }

                // Try to wrap combined text within singleLineMaxLength and maxNumberOfLines
                var language = string.IsNullOrWhiteSpace(baseVm.Language) ? "en" : baseVm.Language;
                var testCombinedRaw = combinedText + " " + (next.Text ?? string.Empty).TrimStart();
                var wrapped = Utilities.AutoBreakLine(testCombinedRaw, singleLineMaxLength, unbreakLinesShorterThan, language);
                var lines = wrapped.SplitToLines();
                if (lines.Count > maxNumberOfLines)
                {
                    break;
                }

                // Check that each line doesn't exceed singleLineMaxLength
                var anyLineTooLong = lines.Any(line => HtmlUtil.RemoveHtmlTags(line, true).Length > singleLineMaxLength);
                if (anyLineTooLong)
                {
                    break;
                }

                // Add to merge group
                mergeGroup.Add(new SubtitleLineViewModel(next));
                combinedText = testCombinedRaw;
                j++;
            }

            // If we have a merge group with more than one line, create highlighted versions
            if (mergeGroup.Count > 1)
            {
                mergeCount += mergeGroup.Count - 1;

                // Build the text parts for highlighting
                var textParts = mergeGroup.Select(vm => (vm.Text ?? string.Empty).Trim()).ToList();

                // Create a highlighted line for each original subtitle in the group
                for (var k = 0; k < mergeGroup.Count; k++)
                {
                    var originalVm = mergeGroup[k];

                    // Build highlighted text: all parts joined, with the k-th part wrapped in <u>
                    var highlightedParts = new List<string>();
                    for (var p = 0; p < textParts.Count; p++)
                    {
                        if (p == k)
                        {
                            highlightedParts.Add("<u>" + textParts[p] + "</u>");
                        }
                        else
                        {
                            highlightedParts.Add(textParts[p]);
                        }
                    }

                    var highlightedText = string.Join(" ", highlightedParts);
                    var language = string.IsNullOrWhiteSpace(originalVm.Language) ? "en" : originalVm.Language;
                    var wrappedHighlighted = Utilities.AutoBreakLine(highlightedText, singleLineMaxLength, unbreakLinesShorterThan, language);

                    // Create view model with original timecodes but highlighted text
                    var highlightedVm = new SubtitleLineViewModel(originalVm)
                    {
                        Text = wrappedHighlighted
                    };

                    result.Add(highlightedVm);

                    fixes.Add(new MergeShortLinesItem(
                        Se.Language.Tools.MergeShortLines.Title,
                        index + k + 1,
                        $"Line {index + k + 1} - {highlightedVm.Text.Replace(Environment.NewLine, " ⏎ ")}",
                        highlightedVm));
                }
            }
            else
            {
                // No merge, just add the original line
                result.Add(mergeGroup[0]);
            }

            // Skip the lines we processed
            index = j - 1;
        }

        return new MergeShortLinesResult(result, fixes, mergeCount);
    }
}
