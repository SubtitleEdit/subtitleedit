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
                    // Stripping tags can only ever shorten a line, so a line that already fits
                    // with its markup cannot be too long without it. Checking that first skips
                    // the string RemoveHtmlTags would allocate for every line of every
                    // paragraph — and on a clean file that is every line.
                    if (lines[li].Length <= maxLineLen)
                    {
                        continue;
                    }

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

                CountTags(p.Text, out var openItalic, out var closeItalic, out var openBold, out var closeBold);
                CheckTagBalance(openItalic, closeItalic, n, "<i>", "</i>", "italic", issues);
                CheckTagBalance(openBold, closeBold, n, "<b>", "</b>", "bold", issues);
            }

            // Duration checks
            var durMs = p.DurationTotalMilliseconds;
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
    /// Reports a mismatched-tag issue when the open and close counts differ. Counting is enough
    /// here because we only care that opens and closes balance — nesting is not validated.
    /// </summary>
    private static void CheckTagBalance(
        int opens,
        int closes,
        int paragraphNumber,
        string open,
        string close,
        string label,
        List<LintIssue> issues)
    {
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

    /// <summary>
    /// Counts <c>&lt;i&gt;</c>, <c>&lt;/i&gt;</c>, <c>&lt;b&gt;</c> and <c>&lt;/b&gt;</c> in one
    /// pass over <paramref name="text"/>. The previous shape ran four separate
    /// <c>IndexOf(string, StringComparison.OrdinalIgnoreCase)</c> scans per paragraph; hopping
    /// between '&lt;' positions on a span reads the text once and never allocates.
    /// </summary>
    private static void CountTags(string text, out int openItalic, out int closeItalic, out int openBold, out int closeBold)
    {
        openItalic = 0;
        closeItalic = 0;
        openBold = 0;
        closeBold = 0;

        var remaining = text.AsSpan();
        while (true)
        {
            var at = remaining.IndexOf('<');
            if (at < 0)
            {
                return;
            }

            remaining = remaining[at..];
            if (remaining.Length >= 3 && remaining[2] == '>')
            {
                // <i> / <b>
                if (remaining[1] is 'i' or 'I')
                {
                    openItalic++;
                }
                else if (remaining[1] is 'b' or 'B')
                {
                    openBold++;
                }
            }
            else if (remaining.Length >= 4 && remaining[1] == '/' && remaining[3] == '>')
            {
                // </i> / </b>
                if (remaining[2] is 'i' or 'I')
                {
                    closeItalic++;
                }
                else if (remaining[2] is 'b' or 'B')
                {
                    closeBold++;
                }
            }

            remaining = remaining[1..];
        }
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
