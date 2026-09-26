using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Tools.AiReview;

/// <summary>Sends one request to the review engine: (system prompt, user content, want JSON object, token) -> reply.</summary>
public delegate Task<string> AiChatFunc(string systemPrompt, string userContent, bool jsonObject, CancellationToken cancellationToken);

/// <summary>
/// Drafts the review's reference context (names, terms, synopsis) from the subtitle itself
/// (issue #15290). The text is read in parts sized for a local model's context window; each part
/// yields names, terms and a short summary, names/terms the subtitle does not actually contain are
/// dropped (small models invent them), and the part summaries are merged into one synopsis.
/// </summary>
public static class AiReviewContextGenerator
{
    // llama.cpp is started with an 8192-token context - leave room for the prompt and the answer.
    public const int MaxTokensPerPart = 3000;
    private const int MaxNames = 60;
    private const int MaxTerms = 60;

    public const string ExtractPrompt =
        "You read part of the {language} subtitles of a film or TV episode and collect reference information for a proofreader. " +
        "Answer with ONLY a JSON object, no other text: {\"names\":[...],\"terms\":[...],\"summary\":\"...\"}.\n" +
        "\"names\": people, characters, places and organizations, spelled as they most often appear in the text.\n" +
        "\"terms\": unusual words, jargon, invented words and foreign words a proofreader must not \"correct\". Leave out ordinary words.\n" +
        "\"summary\": two or three sentences in {language} about what happens in this part.\n" +
        "Only include what actually appears in the text.";

    public const string SynopsisPrompt =
        "The user message holds summaries of consecutive parts of one film or TV episode. " +
        "Combine them into one synopsis of at most five sentences in {language}. Answer with the synopsis only, as plain text.";

    public record PartResult(List<string> Names, List<string> Terms, string Summary);

    /// <summary>Joins the lines into parts of at most <paramref name="maxTokens"/> estimated tokens, never splitting a line.</summary>
    public static List<string> BuildParts(IEnumerable<string> lines, int maxTokens = MaxTokensPerPart)
    {
        var parts = new List<string>();
        var sb = new StringBuilder();
        var tokens = 0;
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var lineTokens = EstimateTokens(line) + 1;
            if (tokens > 0 && tokens + lineTokens > maxTokens)
            {
                parts.Add(sb.ToString());
                sb.Clear();
                tokens = 0;
            }

            sb.Append(line).Append('\n');
            tokens += lineTokens;
        }

        if (sb.Length > 0)
        {
            parts.Add(sb.ToString());
        }

        return parts;
    }

    /// <summary>Rough token count: CJK and similar scripts are about one token per character, others about four characters per token.</summary>
    internal static int EstimateTokens(string text)
    {
        var wide = 0;
        var narrow = 0;
        foreach (var ch in text)
        {
            if (ch >= 0x2E80)
            {
                wide++;
            }
            else
            {
                narrow++;
            }
        }

        return wide + (narrow + 3) / 4;
    }

    public static PartResult? ParsePart(string reply)
    {
        var json = AiReviewProtocol.ExtractJsonObject(reply);
        if (json == null)
        {
            return null;
        }

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        if (root.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var summary = root.TryGetProperty("summary", out var s) && s.ValueKind == JsonValueKind.String
            ? (s.GetString() ?? string.Empty).Trim()
            : string.Empty;
        return new PartResult(ReadStrings(root, "names"), ReadStrings(root, "terms"), summary);
    }

    private static List<string> ReadStrings(JsonElement root, string name)
    {
        var list = new List<string>();
        if (root.TryGetProperty(name, out var array) && array.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in array.EnumerateArray())
            {
                var value = item.ValueKind == JsonValueKind.String ? item.GetString()?.Trim() : null;
                if (!string.IsNullOrEmpty(value))
                {
                    list.Add(value);
                }
            }
        }

        return list;
    }

    /// <summary>
    /// Distinct entries in first-seen order that really occur in <paramref name="fullText"/>
    /// (case-insensitive); per case-insensitive key the spelling found most often in the text wins.
    /// </summary>
    public static List<string> MergeEntries(IEnumerable<string> entries, string fullText, int max)
    {
        var normalizedText = NormalizeWhitespace(fullText);
        var order = new List<string>();
        var variants = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in entries)
        {
            var value = NormalizeWhitespace(entry);
            if (value.Length < 2 || !normalizedText.Contains(value, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!variants.TryGetValue(value, out var list))
            {
                list = new List<string>();
                variants[value] = list;
                order.Add(value);
            }

            if (!list.Contains(value, StringComparer.Ordinal))
            {
                list.Add(value);
            }
        }

        return order
            .Select(key => variants[key].OrderByDescending(v => CountOrdinal(normalizedText, v)).First())
            .Take(max)
            .ToList();
    }

    private static int CountOrdinal(string text, string value)
    {
        var count = 0;
        var index = text.IndexOf(value, StringComparison.Ordinal);
        while (index >= 0)
        {
            count++;
            index = text.IndexOf(value, index + value.Length, StringComparison.Ordinal);
        }

        return count;
    }

    private static string NormalizeWhitespace(string text)
    {
        return string.Join(' ', (text ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }

    public static string Format(List<string> names, List<string> terms, string synopsis)
    {
        var sb = new StringBuilder();
        if (names.Count > 0)
        {
            sb.AppendLine("Names: " + string.Join(", ", names));
        }

        if (terms.Count > 0)
        {
            sb.AppendLine("Terms: " + string.Join(", ", terms));
        }

        if (!string.IsNullOrWhiteSpace(synopsis))
        {
            sb.AppendLine("Synopsis: " + synopsis.Trim());
        }

        return sb.ToString().Trim();
    }

    /// <summary>
    /// Runs the whole generation. <paramref name="progress"/> gets (part number, part count), and
    /// (0, part count) while the synopsis is written. Returns an empty string when the engine gave
    /// nothing usable. Engine errors (HttpRequestException) and cancellation propagate.
    /// </summary>
    public static async Task<string> GenerateAsync(
        IReadOnlyList<string> lines,
        string languageName,
        AiChatFunc chat,
        Action<int, int>? progress,
        CancellationToken cancellationToken,
        Action<string>? log = null,
        int maxTokensPerPart = MaxTokensPerPart)
    {
        var parts = BuildParts(lines, maxTokensPerPart);
        if (parts.Count == 0)
        {
            return string.Empty;
        }

        var extractPrompt = ExtractPrompt.Replace("{language}", languageName);
        var names = new List<string>();
        var terms = new List<string>();
        var summaries = new List<string>();
        for (var i = 0; i < parts.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Invoke(i + 1, parts.Count);
            var reply = await chat(extractPrompt, parts[i], true, cancellationToken);
            log?.Invoke($"AI review context: reply for part {i + 1}/{parts.Count}: {reply}");
            PartResult? result = null;
            try
            {
                result = ParsePart(reply);
            }
            catch (JsonException)
            {
                // ignore - handled as an unusable reply below
            }

            if (result == null)
            {
                log?.Invoke($"AI review context: part {i + 1}/{parts.Count} gave no usable JSON - skipped");
                continue;
            }

            names.AddRange(result.Names);
            terms.AddRange(result.Terms);
            if (result.Summary.Length > 0)
            {
                summaries.Add(result.Summary);
            }
        }

        var fullText = string.Join('\n', parts);
        var mergedNames = MergeEntries(names, fullText, MaxNames);
        var mergedTerms = MergeEntries(terms, fullText, MaxTerms)
            .Where(t => !mergedNames.Contains(t, StringComparer.OrdinalIgnoreCase))
            .ToList();

        var synopsis = summaries.Count == 1 ? summaries[0] : string.Empty;
        if (summaries.Count > 1)
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Invoke(0, parts.Count);
            try
            {
                synopsis = (await chat(SynopsisPrompt.Replace("{language}", languageName), string.Join("\n\n", summaries), false, cancellationToken)).Trim();
            }
            catch (HttpRequestException e)
            {
                // the names/terms are the valuable part - keep them even when the last call fails
                log?.Invoke("AI review context: synopsis request failed: " + e.Message);
                synopsis = string.Empty;
            }
        }

        return Format(mergedNames, mergedTerms, synopsis);
    }
}
