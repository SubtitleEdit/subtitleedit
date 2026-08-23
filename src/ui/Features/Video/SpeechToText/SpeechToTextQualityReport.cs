using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText;

public enum SpeechToTextQualityIssueType
{
    /// <summary>Display time below the minimum, or reading speed above the maximum.</summary>
    TooShort,

    /// <summary>Display time above the maximum, or a long span with very little text (typical hallucination).</summary>
    TooLong,

    /// <summary>End time is later than the next line's start time.</summary>
    Overlap,

    /// <summary>Text is only a sound/music description such as "[Music]" or "(waves crashing)".</summary>
    NonSpeech,

    /// <summary>Same text as the previous line (engine loop).</summary>
    Repeated,
}

public class SpeechToTextQualityIssue
{
    public SpeechToTextQualityIssueType Type { get; init; }
    public int Number { get; init; }
    public TimeCode StartTime { get; init; } = new TimeCode();
    public TimeCode EndTime { get; init; } = new TimeCode();
    public string Text { get; init; } = string.Empty;

    /// <summary>Short machine-independent detail, e.g. "0.2 s" or "31.5 cps".</summary>
    public string Detail { get; init; } = string.Empty;
}

/// <summary>
/// Result of checking a speech-to-text transcript for the defect classes users keep
/// running into (issue #13973): lines too short to read, suspiciously long lines,
/// overlaps, non-speech descriptions and repeated (looping) lines. Lines the
/// post-processor removed are listed with <see cref="Removed"/> set so the report
/// can tell "fixed" from "still there".
/// </summary>
public class SpeechToTextQualityReport
{
    public List<SpeechToTextQualityIssue> Issues { get; } = new();

    /// <summary>Lines that were dropped by post-processing (non-speech / repeats), for the report.</summary>
    public List<SpeechToTextQualityIssue> Removed { get; } = new();

    public int TotalLines { get; set; }

    public bool HasIssues => Issues.Count > 0 || Removed.Count > 0;

    public int Count(SpeechToTextQualityIssueType type)
    {
        return Issues.Count(p => p.Type == type);
    }

    public int RemovedCount(SpeechToTextQualityIssueType type)
    {
        return Removed.Count(p => p.Type == type);
    }

    /// <summary>
    /// Long line with very little text — below this many characters per second over
    /// <see cref="SparseMinDurationMs"/> or more is reported as "too long" even when
    /// the duration is within the configured maximum. Whisper hallucinations on
    /// silence typically look like 2-3 words stretched over 10-30 seconds.
    /// </summary>
    public const double SparseCharsPerSecond = 2.0;
    public const int SparseMinDurationMs = 4000;

    // "[Music]", "(laughs)", "*applause*", "♪ ♪", "[Música] (risas)" - the whole
    // line must be made of bracketed/parenthesised groups, music notes or
    // whitespace. A bracketed speaker tag followed by real speech is kept.
    private static readonly Regex NonSpeechRegex = new(@"^\s*(?:(?:\[[^\]]*\]|\([^)]*\)|\*[^*]*\*|<[^>]*>|[♪♫♬¶]+)[\s.,!?\-–—…]*)+$", RegexOptions.Compiled);

    public static bool IsNonSpeechLine(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var noHtml = HtmlUtil.RemoveHtmlTags(text, true).Trim();
        if (noHtml.Length == 0)
        {
            return false;
        }

        return NonSpeechRegex.IsMatch(noHtml);
    }

    public static string NormalizeForRepeat(string text)
    {
        var s = HtmlUtil.RemoveHtmlTags(text, true).ToLowerInvariant();
        var sb = new System.Text.StringBuilder(s.Length);
        foreach (var ch in s)
        {
            if (char.IsLetterOrDigit(ch))
            {
                sb.Append(ch);
            }
        }

        return sb.ToString();
    }

    public static bool IsRepeatOf(string text, string previousText)
    {
        var a = NormalizeForRepeat(text);
        if (a.Length == 0)
        {
            return false;
        }

        return a == NormalizeForRepeat(previousText);
    }

    public static SpeechToTextQualityIssue MakeIssue(SpeechToTextQualityIssueType type, Paragraph p, int number, string detail)
    {
        return new SpeechToTextQualityIssue
        {
            Type = type,
            Number = number,
            StartTime = new TimeCode(p.StartTime.TotalMilliseconds),
            EndTime = new TimeCode(p.EndTime.TotalMilliseconds),
            Text = p.Text,
            Detail = detail,
        };
    }

    /// <summary>
    /// Scan the (final) subtitle and record what is still wrong. Uses the user's
    /// general timing settings for the thresholds, so the report agrees with what
    /// the main grid colors as errors.
    /// </summary>
    public void Analyze(Subtitle subtitle, int minDisplayMs, int maxDisplayMs, double maxCharsPerSecond)
    {
        TotalLines = subtitle.Paragraphs.Count;

        for (var i = 0; i < subtitle.Paragraphs.Count; i++)
        {
            var p = subtitle.Paragraphs[i];
            var number = i + 1;
            var durationMs = p.DurationTotalMilliseconds;
            var textNoHtml = HtmlUtil.RemoveHtmlTags(p.Text, true);
            var cps = p.GetCharactersPerSecond();

            if (IsNonSpeechLine(p.Text))
            {
                Issues.Add(MakeIssue(SpeechToTextQualityIssueType.NonSpeech, p, number, string.Empty));
            }
            else if (i > 0 && IsRepeatOf(p.Text, subtitle.Paragraphs[i - 1].Text))
            {
                Issues.Add(MakeIssue(SpeechToTextQualityIssueType.Repeated, p, number, $"= #{i}"));
            }

            if (durationMs < minDisplayMs)
            {
                Issues.Add(MakeIssue(SpeechToTextQualityIssueType.TooShort, p, number, FormatSeconds(durationMs)));
            }
            else if (cps > maxCharsPerSecond)
            {
                Issues.Add(MakeIssue(SpeechToTextQualityIssueType.TooShort, p, number, $"{cps:0.0} cps"));
            }

            if (durationMs > maxDisplayMs)
            {
                Issues.Add(MakeIssue(SpeechToTextQualityIssueType.TooLong, p, number, FormatSeconds(durationMs)));
            }
            else if (durationMs >= SparseMinDurationMs && textNoHtml.Trim().Length > 0 && cps < SparseCharsPerSecond)
            {
                Issues.Add(MakeIssue(SpeechToTextQualityIssueType.TooLong, p, number, $"{FormatSeconds(durationMs)}, {cps:0.0} cps"));
            }

            var next = subtitle.GetParagraphOrDefault(i + 1);
            if (next != null && p.EndTime.TotalMilliseconds > next.StartTime.TotalMilliseconds + 0.5)
            {
                var overlapMs = p.EndTime.TotalMilliseconds - next.StartTime.TotalMilliseconds;
                Issues.Add(MakeIssue(SpeechToTextQualityIssueType.Overlap, p, number, $"{overlapMs:0} ms"));
            }
        }
    }

    private static string FormatSeconds(double ms)
    {
        return (ms / 1000.0).ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + " s";
    }

    public string ToLogString()
    {
        var parts = new List<string>();
        foreach (var type in Enum.GetValues<SpeechToTextQualityIssueType>())
        {
            var n = Count(type);
            var r = RemovedCount(type);
            if (n > 0 || r > 0)
            {
                parts.Add(r > 0 ? $"{type}: {n} (removed {r})" : $"{type}: {n}");
            }
        }

        return parts.Count == 0
            ? $"Quality check: no issues found in {TotalLines} lines"
            : $"Quality check ({TotalLines} lines): {string.Join(", ", parts)}";
    }
}
