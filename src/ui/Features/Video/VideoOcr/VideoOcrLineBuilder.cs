using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Video.VideoOcr;

/// <summary>
/// Turns OCR'ed frame groups into subtitle lines: merges consecutive groups whose text is
/// near-identical (OCR jitter), picks the text variant that was on screen the longest
/// (majority vote), and drops blips shorter than the minimum duration.
/// </summary>
public static class VideoOcrLineBuilder
{
    public class OcrLine
    {
        public double StartMs { get; set; }
        public double EndMs { get; set; }
        public string Text { get; set; } = string.Empty;
    }

    private class WorkLine
    {
        public double StartMs;
        public double EndMs;
        public readonly Dictionary<string, double> TextDurations = new();

        public void AddText(string text, double durationMs)
        {
            TextDurations.TryGetValue(text, out var current);
            TextDurations[text] = current + durationMs;
        }

        public string GetMajorityText()
        {
            return TextDurations.OrderByDescending(p => p.Value).First().Key;
        }
    }

    public static List<OcrLine> Build(
        IReadOnlyList<VideoOcrFrameGroup> groups,
        double framesPerSecond,
        int textSimilarityPercent,
        int maxGapMs,
        int minDurationMs)
    {
        var work = new List<WorkLine>();
        WorkLine? current = null;

        foreach (var group in groups.OrderBy(p => p.StartFrame))
        {
            var text = (group.Text ?? string.Empty).Trim();
            if (group.IsBlank || text.Length == 0)
            {
                // Empty stretches are just skipped: the max-gap check below bridges a
                // short flicker (e.g. one frame dipping below the brightness minimum)
                // while a longer empty stretch still ends the line.
                continue;
            }

            var startMs = group.GetStartMs(framesPerSecond);
            var endMs = group.GetEndMs(framesPerSecond);

            if (current != null &&
                startMs - current.EndMs <= maxGapMs &&
                GetTextSimilarityPercent(current.GetMajorityText(), text) >= textSimilarityPercent)
            {
                current.EndMs = endMs;
                current.AddText(text, endMs - startMs);
            }
            else
            {
                current = new WorkLine { StartMs = startMs, EndMs = endMs };
                current.AddText(text, endMs - startMs);
                work.Add(current);
            }
        }

        return work
            .Where(p => p.EndMs - p.StartMs >= minDurationMs)
            .Select(p => new OcrLine
            {
                StartMs = p.StartMs,
                EndMs = p.EndMs,
                Text = p.GetMajorityText(),
            })
            .ToList();
    }

    /// <summary>
    /// Cleans a raw OCR result for a video frame. Frames without subtitle text make
    /// vision LLMs hallucinate (markdown fences, prompt echoes, repeated symbol lines) -
    /// after cleaning, such results become empty, which correctly means "no subtitle".
    /// </summary>
    public static string CleanOcrResult(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var kept = new List<string>();
        string? lastLine = null;
        foreach (var raw in text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n'))
        {
            var line = raw.Trim();
            if (line.Length == 0 ||
                line.StartsWith("```", StringComparison.Ordinal) ||
                line.StartsWith("You are an OCR engine", StringComparison.OrdinalIgnoreCase) ||
                line == lastLine ||
                !line.Any(char.IsLetterOrDigit))
            {
                continue;
            }

            lastLine = line;
            kept.Add(line);

            if (kept.Count >= 4) // a subtitle is at most a few short lines
            {
                break;
            }
        }

        return string.Join("\n", kept);
    }

    /// <summary>
    /// Similarity of two texts as Levenshtein ratio in percent, ignoring case and
    /// whitespace so OCR spacing/line-break jitter does not split one subtitle in two.
    /// </summary>
    public static int GetTextSimilarityPercent(string a, string b)
    {
        var s1 = Normalize(a);
        var s2 = Normalize(b);

        if (s1.Length == 0 && s2.Length == 0)
        {
            return 100;
        }

        if (s1.Length == 0 || s2.Length == 0)
        {
            return 0;
        }

        var distance = GetLevenshteinDistance(s1, s2);
        var maxLength = Math.Max(s1.Length, s2.Length);
        return (int)Math.Round(100.0 * (maxLength - distance) / maxLength);
    }

    private static string Normalize(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        var sb = new System.Text.StringBuilder(text.Length);
        foreach (var ch in text)
        {
            if (!char.IsWhiteSpace(ch))
            {
                sb.Append(char.ToLowerInvariant(ch));
            }
        }

        return sb.ToString();
    }

    private static int GetLevenshteinDistance(string a, string b)
    {
        var previous = new int[b.Length + 1];
        var current = new int[b.Length + 1];

        for (var j = 0; j <= b.Length; j++)
        {
            previous[j] = j;
        }

        for (var i = 1; i <= a.Length; i++)
        {
            current[0] = i;
            for (var j = 1; j <= b.Length; j++)
            {
                var cost = a[i - 1] == b[j - 1] ? 0 : 1;
                current[j] = Math.Min(Math.Min(current[j - 1] + 1, previous[j] + 1), previous[j - 1] + cost);
            }

            (previous, current) = (current, previous);
        }

        return previous[b.Length];
    }

    /// <summary>
    /// Maps the center of the scan area to an ASSA alignment tag ("an1".."an9"), or an
    /// empty string for the default bottom-center position (no tag needed).
    /// </summary>
    public static string GetAssaAlignmentTag(double relativeX, double relativeY)
    {
        int horizontal; // 0=left, 1=center, 2=right
        if (relativeX < 0.33)
        {
            horizontal = 0;
        }
        else if (relativeX > 0.67)
        {
            horizontal = 2;
        }
        else
        {
            horizontal = 1;
        }

        int alignment;
        if (relativeY < 0.33)
        {
            alignment = 7 + horizontal; // top row: an7, an8, an9
        }
        else if (relativeY > 0.67)
        {
            alignment = 1 + horizontal; // bottom row: an1, an2, an3
        }
        else
        {
            alignment = 4 + horizontal; // middle row: an4, an5, an6
        }

        return alignment == 2 ? string.Empty : $"{{\\an{alignment}}}";
    }
}
