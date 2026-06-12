using Nikse.SubtitleEdit.Core.Common;

namespace SeConv.Core;

/// <summary>
/// Validates a subtitle file without writing. Reports overlaps, bad display
/// times, too-long/too-many lines, mismatched HTML tags, and a few other
/// common problems. Used by <c>seconv lint</c>.
///
/// Thresholds come from <see cref="Configuration.Settings"/> (the same defaults
/// the desktop UI uses), so a <c>--settings:foo.json</c> overlay will affect
/// lint thresholds too.
/// </summary>
internal static class SubtitleLinter
{
    public static LintReport Lint(string filePath)
    {
        var subtitle = LibSEIntegration.LoadSubtitle(filePath);
        var issues = new List<LintIssue>();

        var general = Configuration.Settings.General;
        var maxLineLen = general.SubtitleLineMaximumLength;
        var minDuration = general.SubtitleMinimumDisplayMilliseconds;
        var maxDuration = general.SubtitleMaximumDisplayMilliseconds;
        var maxLines = general.MaxNumberOfLines;
        var minGap = general.MinimumMillisecondsBetweenLines;

        for (var i = 0; i < subtitle.Paragraphs.Count; i++)
        {
            var p = subtitle.Paragraphs[i];
            var n = i + 1;

            // Text-level checks
            if (string.IsNullOrWhiteSpace(p.Text))
            {
                issues.Add(new LintIssue
                {
                    Type = "empty",
                    ParagraphNumber = n,
                    Message = "Paragraph has no text.",
                });
            }
            else
            {
                var lines = p.Text.SplitToLines();
                if (lines.Count > maxLines)
                {
                    issues.Add(new LintIssue
                    {
                        Type = "too-many-lines",
                        ParagraphNumber = n,
                        Message = $"{lines.Count} lines (max {maxLines}).",
                    });
                }
                for (var li = 0; li < lines.Count; li++)
                {
                    var stripped = HtmlUtil.RemoveHtmlTags(lines[li], true);
                    if (stripped.Length > maxLineLen)
                    {
                        issues.Add(new LintIssue
                        {
                            Type = "line-too-long",
                            ParagraphNumber = n,
                            Message = $"Line {li + 1} is {stripped.Length} chars (max {maxLineLen}).",
                        });
                    }
                }

                CheckTagBalance(p.Text, n, "<i>", "</i>", "italic", issues);
                CheckTagBalance(p.Text, n, "<b>", "</b>", "bold", issues);
            }

            // Duration checks
            var durMs = p.Duration.TotalMilliseconds;
            if (durMs < 0)
            {
                issues.Add(new LintIssue
                {
                    Type = "negative-duration",
                    ParagraphNumber = n,
                    Message = $"End time precedes start time ({durMs:0} ms).",
                });
            }
            else if (durMs == 0)
            {
                issues.Add(new LintIssue
                {
                    Type = "zero-duration",
                    ParagraphNumber = n,
                    Message = "Start and end time are equal.",
                });
            }
            else
            {
                if (durMs < minDuration)
                {
                    issues.Add(new LintIssue
                    {
                        Type = "display-time-too-short",
                        ParagraphNumber = n,
                        Message = $"Duration {durMs:0} ms (min {minDuration} ms).",
                    });
                }
                if (durMs > maxDuration)
                {
                    issues.Add(new LintIssue
                    {
                        Type = "display-time-too-long",
                        ParagraphNumber = n,
                        Message = $"Duration {durMs:0} ms (max {maxDuration} ms).",
                    });
                }
            }

            // Inter-paragraph checks
            if (i + 1 < subtitle.Paragraphs.Count)
            {
                var next = subtitle.Paragraphs[i + 1];
                var gap = next.StartTime.TotalMilliseconds - p.EndTime.TotalMilliseconds;
                if (gap < 0)
                {
                    issues.Add(new LintIssue
                    {
                        Type = "overlap",
                        ParagraphNumber = n,
                        Message = $"Overlaps next paragraph by {-gap:0} ms.",
                    });
                }
                else if (gap < minGap)
                {
                    issues.Add(new LintIssue
                    {
                        Type = "gap-too-short",
                        ParagraphNumber = n,
                        Message = $"Gap to next is {gap:0} ms (min {minGap} ms).",
                    });
                }
            }
        }

        return new LintReport
        {
            Path = filePath,
            Issues = issues,
        };
    }

    /// <summary>
    /// Reports a mismatched-tag issue when the count of <paramref name="open"/> and
    /// <paramref name="close"/> in <paramref name="text"/> differ. Naive substring
    /// counting is fine here because we only care that opens and closes balance —
    /// nesting is not validated.
    /// </summary>
    private static void CheckTagBalance(
        string text,
        int paragraphNumber,
        string open,
        string close,
        string label,
        List<LintIssue> issues)
    {
        var opens = CountOccurrences(text, open);
        var closes = CountOccurrences(text, close);
        if (opens != closes)
        {
            issues.Add(new LintIssue
            {
                Type = $"mismatched-{label}",
                ParagraphNumber = paragraphNumber,
                Message = $"{opens} '{open}' vs {closes} '{close}' tags.",
            });
        }
    }

    private static int CountOccurrences(string haystack, string needle)
    {
        var count = 0;
        var idx = 0;
        while ((idx = haystack.IndexOf(needle, idx, StringComparison.OrdinalIgnoreCase)) >= 0)
        {
            count++;
            idx += needle.Length;
        }
        return count;
    }
}

internal sealed record LintIssue
{
    public required string Type { get; init; }
    public required int ParagraphNumber { get; init; }
    public required string Message { get; init; }
}

internal sealed record LintReport
{
    public required string Path { get; init; }
    public required IReadOnlyList<LintIssue> Issues { get; init; }
    public bool IsClean => Issues.Count == 0;
}
