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

    // Alignment is anchor-based. Rare, reasonably long words that occur in both the
    // script and the transcription become candidate anchors, and the longest increasing
    // subsequence of those candidates gives a monotonic, mutually consistent backbone
    // that no single bad match can corrupt. The gaps between anchors are then filled
    // with a bounded search for the *nearest* good match.
    //
    // This replaces a greedy scan that took the best-scoring word anywhere in a 60..600
    // word lookahead with no preference for nearby matches. Because a common word
    // matching far ahead outscored a good match at the next position, a single jump
    // moved the cursor permanently: on a 2500-word script with a 5%-error transcription
    // it desynced at word ~23 and never recovered (SubtitleEdit #11746).
    private const int MinAnchorLength = 5;
    private const int MaxAnchorOccurrences = 3;
    private const int FillLookahead = 25;

    // Words shorter than this never place a line. Three-letter function words ("the",
    // "and", "was") recur constantly, so when a line's distinctive word is missing from
    // the transcription they will happily match the previous line's copy and pull the
    // line a second or more too early. Excluding them costs at most the length of a
    // leading short word and removes that whole failure mode.
    private const int MinFillLength = 4;
    private const double FillSimilarityThreshold = 0.85;

    /// <param name="UnmatchedLines">
    /// Lines with no direct word-level match against the transcription. These still get
    /// time codes interpolated from their neighbours, so this is a confidence signal
    /// rather than a count of lines left untimed.
    /// </param>
    public readonly record struct SyncResult(int TotalLines, int UnmatchedLines)
    {
        public int MatchedLines => TotalLines - UnmatchedLines;
    }

    public static SyncResult SyncScript(List<SubtitleLineViewModel> scriptLines, Subtitle whisperSubtitle)
    {
        var minDurationMs = (double)Se.Settings.General.SubtitleMinimumDisplayMilliseconds;
        var maxDurationMs = (double)Se.Settings.General.SubtitleMaximumDisplayMilliseconds;
        var minGapMs = (double)Se.Settings.General.MinimumBetweenLines.GetMilliseconds();

        var whisperWords = ExtractWordTimestamps(whisperSubtitle);
        if (whisperWords.Count == 0)
        {
            return new SyncResult(scriptLines.Count, scriptLines.Count);
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
            return new SyncResult(scriptLines.Count, scriptLines.Count);
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

        // Count lines with no direct word match rather than lines left without time codes:
        // interpolation gives almost every line a time code, so the latter was ~always
        // zero and the caller's "this transcription may be off" warning never fired.
        var unmatched = 0;
        for (int i = 0; i < scriptLines.Count; i++)
        {
            if (!lineHasMatch[i])
            {
                unmatched++;
            }

            if (lineStartMs[i] < 0)
            {
                continue;
            }

            scriptLines[i].StartTime = TimeSpan.FromMilliseconds(lineStartMs[i]);
            scriptLines[i].EndTime = TimeSpan.FromMilliseconds(lineEndMs[i]);
            scriptLines[i].UpdateDuration();
        }

        return new SyncResult(scriptLines.Count, unmatched);
    }

    private static List<int> AlignWords(List<string> scriptWords, List<WordTimestamp> whisperWords)
    {
        var result = new List<int>(scriptWords.Count);
        for (var i = 0; i < scriptWords.Count; i++)
        {
            result.Add(-1);
        }

        var anchors = FindAnchors(scriptWords, whisperWords);

        // Fill the stretches between consecutive anchors. Each stretch is bounded on both
        // sides, so a wrong match inside it cannot leak past the next anchor. With no
        // anchors at all this degrades to a single bounded pass over the whole script.
        var prevScript = -1;
        var prevWhisper = -1;
        foreach (var anchor in anchors)
        {
            result[anchor.ScriptIdx] = anchor.WhisperIdx;
            FillRange(result, scriptWords, whisperWords, prevScript + 1, anchor.ScriptIdx, prevWhisper + 1, anchor.WhisperIdx);
            prevScript = anchor.ScriptIdx;
            prevWhisper = anchor.WhisperIdx;
        }

        FillRange(result, scriptWords, whisperWords, prevScript + 1, scriptWords.Count, prevWhisper + 1, whisperWords.Count);

        return result;
    }

    /// <summary>
    /// Picks a monotonic set of high-confidence (script word, transcription word) pairs.
    /// Only words that are long enough to be distinctive and rare enough in the
    /// transcription to be unambiguous are considered, and the longest increasing
    /// subsequence then discards any candidate that contradicts the majority ordering.
    /// </summary>
    private static List<(int ScriptIdx, int WhisperIdx)> FindAnchors(List<string> scriptWords, List<WordTimestamp> whisperWords)
    {
        var whisperIndex = new Dictionary<string, List<int>>(StringComparer.Ordinal);
        for (var i = 0; i < whisperWords.Count; i++)
        {
            var normalized = NormalizeWord(whisperWords[i].Word);
            if (normalized.Length < MinAnchorLength)
            {
                continue;
            }

            if (!whisperIndex.TryGetValue(normalized, out var positions))
            {
                positions = new List<int>();
                whisperIndex[normalized] = positions;
            }

            positions.Add(i);
        }

        // Ordered by script index ascending and, for one script word with several
        // candidate positions, transcription index descending - so a strictly increasing
        // subsequence can never take two positions for the same word.
        var candidates = new List<(int ScriptIdx, int WhisperIdx)>();
        for (var i = 0; i < scriptWords.Count; i++)
        {
            var normalized = NormalizeWord(scriptWords[i]);
            if (normalized.Length < MinAnchorLength ||
                !whisperIndex.TryGetValue(normalized, out var positions) ||
                positions.Count > MaxAnchorOccurrences)
            {
                continue;
            }

            for (var p = positions.Count - 1; p >= 0; p--)
            {
                candidates.Add((i, positions[p]));
            }
        }

        return LongestIncreasingByWhisperIndex(candidates);
    }

    private static List<(int ScriptIdx, int WhisperIdx)> LongestIncreasingByWhisperIndex(List<(int ScriptIdx, int WhisperIdx)> candidates)
    {
        var result = new List<(int ScriptIdx, int WhisperIdx)>();
        if (candidates.Count == 0)
        {
            return result;
        }

        // Patience sorting: tailValues[n] is the smallest transcription index that can end
        // an increasing run of length n+1, and tailIndices[n] is the candidate it came from.
        var tailValues = new List<int>();
        var tailIndices = new List<int>();
        var previous = new int[candidates.Count];

        for (var k = 0; k < candidates.Count; k++)
        {
            var pos = LowerBound(tailValues, candidates[k].WhisperIdx);
            previous[k] = pos > 0 ? tailIndices[pos - 1] : -1;
            if (pos == tailValues.Count)
            {
                tailValues.Add(candidates[k].WhisperIdx);
                tailIndices.Add(k);
            }
            else
            {
                tailValues[pos] = candidates[k].WhisperIdx;
                tailIndices[pos] = k;
            }
        }

        for (var k = tailIndices[tailIndices.Count - 1]; k >= 0; k = previous[k])
        {
            result.Add(candidates[k]);
        }

        result.Reverse();
        return result;
    }

    private static int LowerBound(List<int> sortedValues, int value)
    {
        var lo = 0;
        var hi = sortedValues.Count;
        while (lo < hi)
        {
            var mid = lo + ((hi - lo) / 2);
            if (sortedValues[mid] < value)
            {
                lo = mid + 1;
            }
            else
            {
                hi = mid;
            }
        }

        return lo;
    }

    /// <summary>
    /// Matches script words to transcription words within one bounded stretch, taking the
    /// <em>nearest</em> match above a high similarity threshold rather than the best one
    /// anywhere in the window. Preferring near matches is what stops a repeated word from
    /// dragging the cursor forward.
    /// </summary>
    private static void FillRange(
        List<int> result,
        List<string> scriptWords,
        List<WordTimestamp> whisperWords,
        int scriptFrom,
        int scriptTo,
        int whisperFrom,
        int whisperTo)
    {
        var whisperPos = whisperFrom;
        for (var i = scriptFrom; i < scriptTo; i++)
        {
            // Short function words ("in"/"it", "of"/"or", "we"/"he") sit above any useful
            // similarity threshold for each other, so they are never used to place a line.
            if (NormalizeWord(scriptWords[i]).Length < MinFillLength)
            {
                continue;
            }

            var windowEnd = Math.Min(whisperTo, whisperPos + FillLookahead);
            for (var wp = whisperPos; wp < windowEnd; wp++)
            {
                if (WordSimilarity(scriptWords[i], whisperWords[wp].Word) >= FillSimilarityThreshold)
                {
                    result[i] = wp;
                    whisperPos = wp + 1;
                    break;
                }
            }
        }
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
                // Trailing lines, with no later match to interpolate towards: lay them out
                // back to back at the minimum display duration. (Using the minimum *gap*
                // as the duration here happened to come out the same, because the duration
                // clamp and overlap pass below both repair it - but say what we mean.)
                int offset = i - prevMatched;
                lineStartMs[i] = lineEndMs[prevMatched] + (offset * minGapMs) + ((offset - 1) * minDurationMs);
                lineEndMs[i] = lineStartMs[i] + minDurationMs;
            }
            else if (nextMatched >= 0)
            {
                // Leading lines, laid out backwards from the first match. These must leave
                // room for their own minimum duration: reserving only the minimum gap put
                // the line's end *at* the first matched line's start, the duration clamp
                // below then pushed its end past that start, and the overlap pass resolved
                // the collision by moving the correctly matched line later - losing the one
                // timing we were actually sure about.
                int offset = nextMatched - i;
                lineEndMs[i] = lineStartMs[nextMatched] - (offset * minGapMs) - ((offset - 1) * minDurationMs);
                lineStartMs[i] = Math.Max(0, lineEndMs[i] - minDurationMs);
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
