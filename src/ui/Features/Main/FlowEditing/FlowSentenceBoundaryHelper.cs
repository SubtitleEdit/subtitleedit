using System;
using System.Collections.Generic;
using System.Linq;
using Nikse.SubtitleEdit.Features.Tools.SplitBreakLongLines;

namespace Nikse.SubtitleEdit.Features.Main.FlowEditing;

internal static class FlowSentenceBoundaryHelper
{
    public static bool TrySplitAtPreferredBoundary(
        string? text,
        out string before,
        out string after)
    {
        before = string.Empty;
        after = string.Empty;

        var lines = (text ?? string.Empty)
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n');

        for (var index = 0; index < lines.Length - 1; index++)
        {
            if (!SplitBreakLongLinesViewModel.HasSentenceEndingLineBreak(
                    lines[index] + Environment.NewLine + lines[index + 1]))
            {
                continue;
            }

            before = string.Join(Environment.NewLine, lines.Take(index + 1)).Trim();
            after = string.Join(Environment.NewLine, lines.Skip(index + 1)).Trim();
            return before.Length > 0 && after.Length > 0;
        }

        return false;
    }

    public static bool IsPreferredBoundary(string? before, string? after)
    {
        return TrySplitAtPreferredBoundary(
            (before ?? string.Empty).TrimEnd() + Environment.NewLine +
            (after ?? string.Empty).TrimStart(),
            out _,
            out _);
    }

    public static int GetFirstCompleteSentenceWordCount(
        IReadOnlyList<string> words)
    {
        for (var count = 1; count < words.Count; count++)
        {
            var before = string.Join(" ", words.Take(count));
            var after = string.Join(" ", words.Skip(count));
            if (IsPreferredBoundary(before, after))
            {
                return count;
            }
        }

        return words.Count;
    }

    public static bool HasContentAfterFirstCompleteSentence(string? text)
    {
        var words = (text ?? string.Empty)
            .Split(
                new[] { ' ', '\t', '\r', '\n' },
                StringSplitOptions.RemoveEmptyEntries);

        return GetFirstCompleteSentenceWordCount(words) < words.Length;
    }
}
