using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.UiLogic.MergeContinuationLines;

/// <summary>
/// The "Merge continuation lines" rules, shared by Tools → Merge continuation lines, batch
/// convert, text to speech and seconv so they all merge the same lines.
/// </summary>
public static class MergeContinuationLinesUtils
{
    /// <summary>
    /// Merges every line that continues into the next one - the same result as the
    /// Merge continuation lines tool with all candidates checked. The input is not changed.
    /// </summary>
    public static Subtitle MergeContinuationLinesInSubtitle(Subtitle subtitle, string? language, int maxGapMs, int maxTotalLength)
    {
        var lang = string.IsNullOrWhiteSpace(language) ? "en" : language!;
        var paragraphs = subtitle.Paragraphs;
        var result = new Subtitle();
        var i = 0;
        while (i < paragraphs.Count)
        {
            var current = new Paragraph(paragraphs[i]);
            var j = i;

            // Each pair is judged on its original texts, like the tool's candidate list.
            while (j + 1 < paragraphs.Count && CanMerge(paragraphs[j], paragraphs[j + 1], lang, maxGapMs, maxTotalLength))
            {
                var next = paragraphs[j + 1];
                current.Text = MergeText(current.Text, next.Text, lang);
                current.EndTime.TotalMilliseconds = next.EndTime.TotalMilliseconds;
                j++;
            }

            result.Paragraphs.Add(current);
            i = j + 1;
        }

        return result;
    }

    /// <summary>
    /// True when <paramref name="text"/> carries on in <paramref name="nextText"/>: the first line
    /// does not end a sentence, the gap and combined length are below the limits, and neither
    /// line is a dialog or music line.
    /// </summary>
    public static bool CanMerge(string? text, string? nextText, double gapMs, string language, int maxGapMs, int maxTotalLength)
    {
        if (!QualifiesForMerge(text, nextText, gapMs, language, maxGapMs, maxTotalLength))
        {
            return false;
        }

        // Skip dialog lines ("- A" / "- B") — these are two speakers, not continuation.
        return !StartsWithDashSpeaker(text) && !StartsWithDashSpeaker(nextText);
    }

    public static string MergeText(string? a, string? b, string language)
    {
        var combined = (a ?? string.Empty).TrimEnd() + " " + (b ?? string.Empty).TrimStart();
        return Utilities.AutoBreakLine(combined, language);
    }

    private static bool CanMerge(Paragraph current, Paragraph next, string language, int maxGapMs, int maxTotalLength)
    {
        var gapMs = next.StartTime.TotalMilliseconds - current.EndTime.TotalMilliseconds;
        return CanMerge(current.Text, next.Text, gapMs, language, maxGapMs, maxTotalLength);
    }

    private static bool QualifiesForMerge(string? text, string? next, double gapMs, string language, int maxGapMs, int maxTotalLength)
    {
        var s = HtmlUtil.RemoveHtmlTags((text ?? string.Empty).Trim(), true);
        var nextText = HtmlUtil.RemoveHtmlTags((next ?? string.Empty).Trim(), true);
        if (s.Length + nextText.Length >= maxTotalLength || gapMs >= maxGapMs)
        {
            return false;
        }

        if (s.Length == 0)
        {
            return true;
        }

        if (s.EndsWith('♪') || s.EndsWith('♫') || nextText.StartsWith('♪') || nextText.StartsWith('♫'))
        {
            return false;
        }

        return IsContinuation(s, language);
    }

    /// <summary>
    /// A line continues into the next one unless it ends a sentence - like SE4's "Merge sentences".
    /// A list of "continuation" end characters (letters, comma...) misses too much: words ending in
    /// letters outside the configured alphabet (ß, Š, Arabic, Hebrew, Thai...), numbers, colons,
    /// quotes and so on.
    /// </summary>
    private static bool IsContinuation(string text, string language)
    {
        var s = text.TrimEnd();
        if (s.Length == 0)
        {
            return true;
        }

        // A trailing ellipsis is an unfinished sentence that carries on in the next line.
        if (s.EndsWith("...", StringComparison.Ordinal) || s.EndsWith('…'))
        {
            return true;
        }

        var lastChar = s[s.Length - 1];
        if (lastChar == '！' || lastChar == '．' || lastChar == '｡' || lastChar == '♪' || lastChar == '♫')
        {
            return false;
        }

        return !s.HasSentenceEnding(language);
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
}
