using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Files.ImportPlainText;

public static class ScriptSyncService
{
    private sealed record WordTimestamp(string Word, double StartMs, double EndMs);

    private const int Lookahead = 60;
    private const double SimilarityThreshold = 0.45;

    public static void SyncScript(List<SubtitleLineViewModel> scriptLines, Subtitle whisperSubtitle)
    {
        var minDurationMs = (double)Se.Settings.General.SubtitleMinimumDisplayMilliseconds;
        var maxDurationMs = (double)Se.Settings.General.SubtitleMaximumDisplayMilliseconds;
        var minGapMs = (double)Se.Settings.General.MinimumMillisecondsBetweenLines;

        var whisperWords = ExtractWordTimestamps(whisperSubtitle);
        if (whisperWords.Count == 0)
        {
            return;
        }

        var scriptTokens = new List<(string Word, int LineIdx)>();
        for (var i = 0; i < scriptLines.Count; i++)
        {
            var rawText = HtmlUtil.RemoveHtmlTags(scriptLines[i].Text ?? string.Empty, true);
            var words = rawText.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var word in words)
            {
                scriptTokens.Add((word, i));
            }
        }

        if (scriptTokens.Count == 0)
        {
            return;
        }

        var alignments = AlignWords(scriptTokens.Select(t => t.Word).ToList(), whisperWords);

        var lineStartMs = new double[scriptLines.Count];
        var lineEndMs = new double[scriptLines.Count];
        var lineHasMatch = new bool[scriptLines.Count];
        for (var i = 0; i < lineStartMs.Length; i++)
        {
            lineStartMs[i] = -1;
            lineEndMs[i] = -1;
        }

        for (var i = 0; i < alignments.Count; i++)
        {
            int whisperIdx = alignments[i];
            if (whisperIdx < 0)
            {
                continue;
            }

            int lineIdx = scriptTokens[i].LineIdx;
            var wt = whisperWords[whisperIdx];
            if (!lineHasMatch[lineIdx])
            {
                lineStartMs[lineIdx] = wt.StartMs;
                lineEndMs[lineIdx] = wt.EndMs;
                lineHasMatch[lineIdx] = true;
            }
            else
            {
                lineEndMs[lineIdx] = wt.EndMs;
            }
        }

        InterpolateUnmatched(lineStartMs, lineEndMs, lineHasMatch, minDurationMs, minGapMs);

        // Apply min/max duration clamping
        for (int i = 0; i < scriptLines.Count; i++)
        {
            if (lineStartMs[i] < 0)
            {
                continue;
            }

            lineEndMs[i] = Math.Max(lineEndMs[i], lineStartMs[i] + minDurationMs);
            if (maxDurationMs > 0)
            {
                lineEndMs[i] = Math.Min(lineEndMs[i], lineStartMs[i] + maxDurationMs);
            }
        }

        // Overlap prevention: ensure each line starts at least minGapMs after the previous ends
        for (int i = 1; i < scriptLines.Count; i++)
        {
            if (lineStartMs[i] < 0 || lineStartMs[i - 1] < 0)
            {
                continue;
            }

            var minStart = lineEndMs[i - 1] + minGapMs;
            if (lineStartMs[i] < minStart)
            {
                lineStartMs[i] = minStart;
                lineEndMs[i] = Math.Max(lineEndMs[i], lineStartMs[i] + minDurationMs);
            }
        }

        for (int i = 0; i < scriptLines.Count; i++)
        {
            if (lineStartMs[i] < 0)
            {
                continue;
            }

            scriptLines[i].StartTime = TimeSpan.FromMilliseconds(lineStartMs[i]);
            scriptLines[i].EndTime = TimeSpan.FromMilliseconds(lineEndMs[i]);
            scriptLines[i].UpdateDuration();
        }
    }

    private static List<int> AlignWords(List<string> scriptWords, List<WordTimestamp> whisperWords)
    {
        var result = new List<int>(scriptWords.Count);
        int whisperPos = 0;

        for (int i = 0; i < scriptWords.Count; i++)
        {
            int windowEnd = Math.Min(whisperPos + Lookahead, whisperWords.Count);
            double bestScore = SimilarityThreshold;
            int bestPos = -1;

            for (int wp = whisperPos; wp < windowEnd; wp++)
            {
                double score = WordSimilarity(scriptWords[i], whisperWords[wp].Word);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestPos = wp;
                }
            }

            result.Add(bestPos);
            if (bestPos >= 0)
            {
                whisperPos = bestPos + 1;
            }
        }

        return result;
    }

    private static List<WordTimestamp> ExtractWordTimestamps(Subtitle subtitle)
    {
        var result = new List<WordTimestamp>();

        // Detect whisper word-level highlight output: each paragraph wraps its timed word in <u>…</u>
        bool isWordLevel = subtitle.Paragraphs.Any(p => p.Text.Contains("<u>", StringComparison.OrdinalIgnoreCase));

        if (isWordLevel)
        {
            foreach (var paragraph in subtitle.Paragraphs)
            {
                var text = paragraph.Text;
                var startTag = text.IndexOf("<u>", StringComparison.OrdinalIgnoreCase);
                var endTag = text.IndexOf("</u>", StringComparison.OrdinalIgnoreCase);
                if (startTag >= 0 && endTag > startTag + 2)
                {
                    var word = text.Substring(startTag + 3, endTag - startTag - 3).Trim();
                    if (!string.IsNullOrEmpty(word))
                    {
                        result.Add(new WordTimestamp(word,
                            paragraph.StartTime.TotalMilliseconds,
                            paragraph.EndTime.TotalMilliseconds));
                    }
                }
            }
        }
        else
        {
            foreach (var paragraph in subtitle.Paragraphs)
            {
                var text = HtmlUtil.RemoveHtmlTags(paragraph.Text, true);
                var words = text.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                if (words.Length == 0)
                {
                    continue;
                }

                double startMs = paragraph.StartTime.TotalMilliseconds;
                double totalMs = paragraph.EndTime.TotalMilliseconds - startMs;
                int totalChars = words.Sum(w => w.Length);

                double currentMs = startMs;
                foreach (var word in words)
                {
                    double proportion = totalChars > 0 ? (double)word.Length / totalChars : 1.0 / words.Length;
                    double durationMs = totalMs * proportion;
                    result.Add(new WordTimestamp(word, currentMs, currentMs + durationMs));
                    currentMs += durationMs;
                }
            }
        }

        return result;
    }

    private static void InterpolateUnmatched(double[] lineStartMs, double[] lineEndMs, bool[] lineHasMatch, double minDurationMs, double minGapMs)
    {
        int n = lineStartMs.Length;
        var matchedIndices = new List<int>();
        for (int i = 0; i < n; i++)
        {
            if (lineHasMatch[i])
            {
                matchedIndices.Add(i);
            }
        }

        if (matchedIndices.Count == 0)
        {
            return;
        }

        for (int i = 0; i < n; i++)
        {
            if (lineHasMatch[i])
            {
                continue;
            }

            int prevMatched = -1;
            int nextMatched = -1;

            for (int m = matchedIndices.Count - 1; m >= 0; m--)
            {
                if (matchedIndices[m] < i) { prevMatched = matchedIndices[m]; break; }
            }
            for (int m = 0; m < matchedIndices.Count; m++)
            {
                if (matchedIndices[m] > i) { nextMatched = matchedIndices[m]; break; }
            }

            if (prevMatched >= 0 && nextMatched >= 0)
            {
                double prevEnd = lineEndMs[prevMatched];
                double nextStart = lineStartMs[nextMatched];
                double totalGap = nextStart - prevEnd;
                int gapLines = nextMatched - prevMatched;
                double posInGap = i - prevMatched;
                lineStartMs[i] = prevEnd + totalGap * posInGap / gapLines;
                lineEndMs[i] = Math.Max(prevEnd + totalGap * (posInGap + 1) / gapLines, lineStartMs[i] + minDurationMs);
            }
            else if (prevMatched >= 0)
            {
                int offset = i - prevMatched;
                lineStartMs[i] = lineEndMs[prevMatched] + (offset - 1) * minGapMs;
                lineEndMs[i] = lineStartMs[i] + minGapMs;
            }
            else if (nextMatched >= 0)
            {
                int offset = nextMatched - i;
                lineEndMs[i] = lineStartMs[nextMatched] - (offset - 1) * minGapMs;
                lineStartMs[i] = Math.Max(0, lineEndMs[i] - minGapMs);
            }
        }
    }

    private static string NormalizeWord(string word)
        => new string(word.ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray());

    private static double WordSimilarity(string a, string b)
    {
        a = NormalizeWord(a);
        b = NormalizeWord(b);
        if (a.Length == 0 && b.Length == 0)
        {
            return 1.0;
        }

        if (a.Length == 0 || b.Length == 0)
        {
            return 0.0;
        }

        if (a == b)
        {
            return 1.0;
        }

        int dist = LevenshteinDistance(a, b);
        return 1.0 - (double)dist / Math.Max(a.Length, b.Length);
    }

    private static int LevenshteinDistance(string a, string b)
    {
        int m = a.Length, n = b.Length;
        var prev = new int[n + 1];
        var curr = new int[n + 1];
        for (int j = 0; j <= n; j++)
        {
            prev[j] = j;
        }

        for (int i = 1; i <= m; i++)
        {
            curr[0] = i;
            for (int j = 1; j <= n; j++)
            {
                int cost = a[i - 1] == b[j - 1] ? 0 : 1;
                curr[j] = Math.Min(Math.Min(prev[j] + 1, curr[j - 1] + 1), prev[j - 1] + cost);
            }
            Array.Copy(curr, prev, n + 1);
        }
        return prev[n];
    }
}
