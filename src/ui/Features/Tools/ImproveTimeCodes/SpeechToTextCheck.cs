using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Features.Tools.ImproveTimeCodes;

/// <summary>
/// Checks the forced aligner's answers against an independent transcription of the audio.
///
/// A forced aligner places exactly the words it is given, so where the text is not what is
/// said it places them confidently in the wrong spot - and nothing in its own output shows
/// it. Speech-to-text is not told the text: it reports what it heard, and when. Matching the
/// subtitle's words to the heard words gives two things per line: how much of the line was
/// actually heard (text that differs from the speech shows up as a low share), and - when
/// the line's first words were heard - where the line really starts. That settles moves the
/// aligner and the neighbourhood check could not: a large move is confirmed when the speech
/// starts where the aligner put the line, and disputed when it starts where the line was.
///
/// Word times from speech-to-text are coarser than the aligner's, so they are only used as
/// a referee between two positions far enough apart to tell - never as the new time itself.
/// </summary>
public static class SpeechToTextCheck
{
    public readonly record struct HeardWord(string Word, double StartSeconds, double EndSeconds);

    /// <param name="Words">Words in the line's spoken text.</param>
    /// <param name="Heard">How many of them were matched to heard words.</param>
    /// <param name="AnchorSeconds">
    /// Where one of the line's first words was heard, when one was - see <see cref="AnchorFraction"/>.
    /// </param>
    /// <param name="AnchorFraction">
    /// How far into the line that word is, as a share of its characters: 0 for the first word.
    /// </param>
    public sealed record LineEvidence(int Words, int Heard, double? AnchorSeconds, double AnchorFraction = 0)
    {
        /// <summary>
        /// Where the line's last word was heard to end, when it was - and heard right after the
        /// word before it, so it is this line's and not a copy from elsewhere.
        /// </summary>
        public double? LastWordEndSeconds { get; init; }

        public double HeardRatio => Words == 0 ? 0 : Heard / (double)Words;
    }

    /// <summary>How far a heard word may be from where a position puts it and still count as agreeing.</summary>
    internal const double AgreeSeconds = 0.25;

    /// <summary>The other position has to be at least this much further from the heard word to lose.</summary>
    internal const double MarginSeconds = 0.3;

    /// <summary>Below this share of heard words a line's match is too thin to referee anything.</summary>
    internal const double MinHeardRatio = 0.5;

    /// <summary>A retimed line is only disputed when the aligner moved it at least this far from where it was heard.</summary>
    internal const double DisputeSeconds = 0.4;

    /// <summary>Only one of a line's first few words can anchor it; further in, its place in the line is too much of a guess.</summary>
    private const int MaxAnchorPosition = 2;

    /// <summary>
    /// Matches every line's words to the heard words. Each heard word is used at most once and
    /// the matches keep the order of both texts, so a word cannot be borrowed from the line
    /// before or after. A subtitle word is only looked for within <paramref name="searchSeconds"/>
    /// of where the subtitle says it is. Returns null for lines with nothing spoken.
    /// </summary>
    public static LineEvidence?[] Match(IReadOnlyList<SubtitleRetimer.Line> lines, IReadOnlyList<HeardWord> heardWords, double searchSeconds)
    {
        var evidence = new LineEvidence?[lines.Count];

        var heard = heardWords
            .Select(w => (Word: Normalize(w.Word), w.StartSeconds, w.EndSeconds))
            .Where(w => w.Word.Length > 0)
            .OrderBy(w => w.StartSeconds)
            .ToList();
        var heardStarts = heard.Select(w => w.StartSeconds).ToArray();

        // Every spoken word of the subtitle, with where the subtitle puts it: spread over the
        // line by length, which is near enough for a search window of a few seconds.
        var tokens = new List<(string Word, int Line, int Position)>();
        var tokenTimes = new List<double>();
        var tokenFractions = new List<double>();
        var wordCounts = new int[lines.Count];
        for (var i = 0; i < lines.Count; i++)
        {
            var words = SubtitleRetimer.GetSpokenText(lines[i].Text)
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(Normalize)
                .Where(w => w.Length > 0)
                .ToList();
            wordCounts[i] = words.Count;
            if (words.Count == 0)
            {
                continue;
            }

            var totalChars = words.Sum(w => w.Length + 1);
            var duration = Math.Max(0, lines[i].EndSeconds - lines[i].StartSeconds);
            var chars = 0;
            for (var k = 0; k < words.Count; k++)
            {
                tokens.Add((words[k], i, k));
                tokenFractions.Add(chars / (double)totalChars);
                tokenTimes.Add(lines[i].StartSeconds + (duration * tokenFractions[^1]));
                chars += words[k].Length + 1;
            }
        }

        // Candidate pairs, ordered by token and - for one token - by heard index descending, so
        // a strictly increasing chain can never take two heard words for the same token.
        var candidates = new List<(int Token, int Heard, double Weight)>();
        for (var t = 0; t < tokens.Count; t++)
        {
            var from = LowerBound(heardStarts, tokenTimes[t] - searchSeconds);
            var to = LowerBound(heardStarts, tokenTimes[t] + searchSeconds);
            for (var h = to - 1; h >= from; h--)
            {
                if (IsSameWord(tokens[t].Word, heard[h].Word))
                {
                    // Long words say more than short ones; of two equal words the nearer wins.
                    var weight = Math.Min(tokens[t].Word.Length, 5) - (0.01 * Math.Abs(heardStarts[h] - tokenTimes[t]));
                    candidates.Add((t, h, weight));
                }
            }
        }

        var matchedHeard = new int[tokens.Count];
        Array.Fill(matchedHeard, -1);
        foreach (var (token, h) in HeaviestIncreasingChain(candidates, heard.Count))
        {
            matchedHeard[token] = h;
        }

        var heardCounts = new int[lines.Count];
        var lastWordEnds = new double?[lines.Count];
        var anchorToken = new int[lines.Count];
        Array.Fill(anchorToken, -1);
        for (var t = 0; t < tokens.Count; t++)
        {
            var line = tokens[t].Line;
            if (matchedHeard[t] < 0)
            {
                continue;
            }

            heardCounts[line]++;
            if (tokens[t].Position == wordCounts[line] - 1 && IsEnd(t, wordCounts[line], tokens, matchedHeard))
            {
                lastWordEnds[line] = heard[matchedHeard[t]].EndSeconds;
            }

            if (anchorToken[line] < 0 && tokens[t].Position <= MaxAnchorPosition && IsAnchor(t, wordCounts[line], tokens, matchedHeard))
            {
                anchorToken[line] = t;
            }
        }

        for (var i = 0; i < lines.Count; i++)
        {
            if (wordCounts[i] == 0)
            {
                continue;
            }

            var t = anchorToken[i];
            evidence[i] = (t < 0
                ? new LineEvidence(wordCounts[i], heardCounts[i], null)
                : new LineEvidence(wordCounts[i], heardCounts[i], heard[matchedHeard[t]].StartSeconds, tokenFractions[t]))
                with { LastWordEndSeconds = lastWordEnds[i] };
        }

        return evidence;
    }

    /// <summary>
    /// A heard word only marks where its line is when it is not a lone match: a short word like
    /// "I" or "you" can match a copy of itself in speech that is not this line's. So the line's
    /// next word must have been heard right after it - or, for a one-word line, the word must
    /// be long enough to be its own evidence.
    /// </summary>
    private static bool IsAnchor(int t, int wordCount, List<(string Word, int Line, int Position)> tokens, int[] matchedHeard)
    {
        if (wordCount == 1)
        {
            return tokens[t].Word.Length >= 3;
        }

        if (tokens[t].Position + 1 >= wordCount)
        {
            return false;
        }

        var next = matchedHeard[t + 1];
        return next > matchedHeard[t] && next - matchedHeard[t] <= 2;
    }

    /// <summary>The mirror of <see cref="IsAnchor"/> for a line's last word: the word before it must have been heard just before it.</summary>
    private static bool IsEnd(int t, int wordCount, List<(string Word, int Line, int Position)> tokens, int[] matchedHeard)
    {
        if (wordCount == 1)
        {
            return tokens[t].Word.Length >= 3;
        }

        var previous = matchedHeard[t - 1];
        return previous >= 0 && previous < matchedHeard[t] && matchedHeard[t] - previous <= 2;
    }

    /// <summary>
    /// Settles the lines the aligner moved, using what was heard. Runs on the aligner's raw
    /// answers, before they are tidied, so the start compared is the aligner's own.
    /// - A large move the neighbourhood check could not decide is confirmed when the speech
    ///   is heard where the aligner put the line, and disputed when it is heard where the line was.
    /// - A move that was otherwise accepted is disputed when the speech is heard where the line
    ///   was and the aligner took it clearly away from there.
    /// Anything the heard words do not clearly decide is left as it was.
    /// </summary>
    public static void Apply(IReadOnlyList<SubtitleRetimer.Line> lines, SubtitleRetimer.LineResult[] results, IReadOnlyList<LineEvidence?> evidence)
    {
        var count = Math.Min(results.Length, evidence.Count);
        var bias = MeasureBias(lines, results, evidence, count);

        for (var i = 0; i < count; i++)
        {
            var e = evidence[i];
            if (e == null)
            {
                continue;
            }

            results[i] = results[i] with { HeardRatio = e.HeardRatio, HeardEndSeconds = e.LastWordEndSeconds };
            if (e.AnchorSeconds is not { } anchor || e.HeardRatio < MinHeardRatio)
            {
                continue;
            }

            var status = results[i].Status;
            if (status is not (SubtitleRetimer.LineStatus.Retimed
                or SubtitleRetimer.LineStatus.MovedWithNeighbours
                or SubtitleRetimer.LineStatus.LargeMoveUnconfirmed))
            {
                continue;
            }

            var heardAt = anchor - bias;
            var toNew = Math.Abs(ExpectedAt(results[i].StartSeconds, results[i].EndSeconds, e.AnchorFraction) - heardAt);
            var toOld = Math.Abs(ExpectedAt(lines[i].StartSeconds, lines[i].EndSeconds, e.AnchorFraction) - heardAt);

            // Moved with its neighbours, but heard where the aligner wanted it on its own: the
            // aligner was right and the neighbourhood rule overruled it.
            if (results[i].AlignerOwn is { } own)
            {
                var toOwn = Math.Abs(ExpectedAt(own.StartSeconds, own.EndSeconds, e.AnchorFraction) - heardAt);
                if (toOwn <= AgreeSeconds && toOwn + MarginSeconds <= Math.Min(toNew, toOld))
                {
                    results[i] = results[i] with
                    {
                        StartSeconds = own.StartSeconds,
                        EndSeconds = own.EndSeconds,
                        Status = SubtitleRetimer.LineStatus.ConfirmedBySpeech,
                    };
                    continue;
                }
            }

            if (status == SubtitleRetimer.LineStatus.LargeMoveUnconfirmed)
            {
                if (toNew <= AgreeSeconds && toNew + MarginSeconds <= toOld)
                {
                    results[i] = results[i] with { Status = SubtitleRetimer.LineStatus.ConfirmedBySpeech };
                }
                else if (toOld <= AgreeSeconds && toOld + MarginSeconds <= toNew)
                {
                    results[i] = results[i] with { Status = SubtitleRetimer.LineStatus.DisputedBySpeech };
                }
            }
            else if (toOld <= AgreeSeconds && toNew >= DisputeSeconds && toOld + MarginSeconds <= toNew)
            {
                results[i] = results[i] with { Status = SubtitleRetimer.LineStatus.DisputedBySpeech };
            }
        }
    }

    /// <summary>Where a line with these times would have its anchor word.</summary>
    private static double ExpectedAt(double start, double end, double fraction)
        => start + (Math.Max(0, end - start) * fraction);

    /// <summary>
    /// How much earlier or later speech-to-text puts a word than the aligner does. The two mark
    /// a word's start differently - Parakeet's run about 0.2 s early against a CTC aligner's -
    /// and that would eat most of <see cref="AgreeSeconds"/>. The lines the aligner re-timed
    /// without any doubt give the offset; with too few of them, none is assumed.
    /// </summary>
    internal static double MeasureBias(IReadOnlyList<SubtitleRetimer.Line> lines, SubtitleRetimer.LineResult[] results, IReadOnlyList<LineEvidence?> evidence, int count)
    {
        var offsets = new List<double>();
        for (var i = 0; i < count; i++)
        {
            if (results[i].Status == SubtitleRetimer.LineStatus.Retimed &&
                evidence[i] is { AnchorSeconds: { } anchor, AnchorFraction: 0 } e &&
                e.HeardRatio >= MinHeardRatio)
            {
                offsets.Add(anchor - results[i].StartSeconds);
            }
        }

        if (offsets.Count < 5)
        {
            return 0;
        }

        offsets.Sort();
        var middle = offsets.Count / 2;
        var median = offsets.Count % 2 == 1 ? offsets[middle] : (offsets[middle - 1] + offsets[middle]) / 2.0;

        // Only a word-marking difference, never a sync error of the subtitle - that is what the
        // aligner is there to measure.
        return Math.Clamp(median, -0.3, 0.3);
    }

    /// <summary>
    /// The chain of candidates with the greatest total weight whose token and heard indexes
    /// both strictly increase - a weighted longest increasing subsequence, via a Fenwick tree
    /// of prefix maxima over the heard index.
    /// </summary>
    internal static List<(int Token, int Heard)> HeaviestIncreasingChain(List<(int Token, int Heard, double Weight)> candidates, int heardCount)
    {
        var result = new List<(int Token, int Heard)>();
        if (candidates.Count == 0)
        {
            return result;
        }

        var treeValue = new double[heardCount + 1];
        var treeIndex = new int[heardCount + 1];
        Array.Fill(treeIndex, -1);
        var best = new double[candidates.Count];
        var previous = new int[candidates.Count];

        // Candidates of one token must not chain onto each other; they are all read before any
        // of them is written, which the descending heard order within a token already ensures.
        for (var k = 0; k < candidates.Count; k++)
        {
            var (_, h, weight) = candidates[k];
            var (priorValue, priorIndex) = PrefixMax(treeValue, treeIndex, h); // heard indexes < h
            best[k] = priorValue + weight;
            previous[k] = priorIndex;
            Update(treeValue, treeIndex, h + 1, best[k], k);
        }

        var end = 0;
        for (var k = 1; k < candidates.Count; k++)
        {
            if (best[k] > best[end])
            {
                end = k;
            }
        }

        for (var k = end; k >= 0; k = previous[k])
        {
            result.Add((candidates[k].Token, candidates[k].Heard));
        }

        result.Reverse();
        return result;
    }

    private static (double Value, int Index) PrefixMax(double[] treeValue, int[] treeIndex, int count)
    {
        var value = 0.0;
        var index = -1;
        for (var i = count; i > 0; i -= i & -i)
        {
            if (treeIndex[i] >= 0 && treeValue[i] > value)
            {
                value = treeValue[i];
                index = treeIndex[i];
            }
        }

        return (value, index);
    }

    private static void Update(double[] treeValue, int[] treeIndex, int position, double value, int index)
    {
        for (var i = position; i < treeValue.Length; i += i & -i)
        {
            if (treeIndex[i] < 0 || value > treeValue[i])
            {
                treeValue[i] = value;
                treeIndex[i] = index;
            }
        }
    }

    private static int LowerBound(double[] sorted, double value)
    {
        var lo = 0;
        var hi = sorted.Length;
        while (lo < hi)
        {
            var mid = lo + ((hi - lo) / 2);
            if (sorted[mid] < value)
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

    /// <summary>Lower case letters and digits only, so "Don't," and "don't" are the same word.</summary>
    internal static string Normalize(string word)
    {
        var sb = new StringBuilder(word.Length);
        foreach (var c in word)
        {
            if (char.IsLetterOrDigit(c))
            {
                sb.Append(char.ToLowerInvariant(c));
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Equal, or - for words long enough that one wrong letter still leaves them recognisable -
    /// one edit apart per five letters ("colour"/"color", "gonna"/"gona").
    /// </summary>
    private static bool IsSameWord(string a, string b)
    {
        if (a == b)
        {
            return true;
        }

        var longest = Math.Max(a.Length, b.Length);
        if (Math.Min(a.Length, b.Length) < 4 || Math.Abs(a.Length - b.Length) > longest / 5)
        {
            return false;
        }

        return LevenshteinDistance(a, b) <= longest / 5;
    }

    private static int LevenshteinDistance(string a, string b)
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
                current[j] = Math.Min(Math.Min(previous[j] + 1, current[j - 1] + 1), previous[j - 1] + cost);
            }

            (previous, current) = (current, previous);
        }

        return previous[b.Length];
    }
}
