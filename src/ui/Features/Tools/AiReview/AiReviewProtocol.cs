using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Features.Tools.AiReview;

public enum ReviewCategory
{
    Spelling,
    Grammar,
    Punctuation,
    Casing,
    Other,
}

public record AiReviewChange(int Number, string NewText, string Reason, ReviewCategory Category);

/// <summary>
/// The wire format between Subtitle Edit and the model: numbered lines in, a strict JSON object
/// with only the changed lines out. The protocol text is appended to the user's editable
/// instructions so prompt edits cannot break the parsing contract.
/// </summary>
public static class AiReviewProtocol
{
    private static readonly Regex TagRegex = new Regex(@"<[^>]*>|\{\\[^}]*\}", RegexOptions.Compiled);

    public const string ProtocolText =
        "\n\nThe user message is a JSON object. \"lines\" holds the subtitle lines to review, each with a line number \"n\" and its \"text\" " +
        "(a \"\\n\" inside a text is a line break inside that subtitle and must be kept). \"context_before\" and \"context_after\" are " +
        "read-only surrounding lines - never change or return those. A sentence may continue across several lines; correct it across " +
        "the lines, but never move words from one line to another.\n" +
        "Answer with ONLY a JSON object, no other text: {\"changes\":[{\"n\":<line number>,\"text\":\"<full corrected text>\"," +
        "\"reason\":\"<short reason>\",\"category\":\"spelling|grammar|punctuation|casing|other\"}]}. " +
        "Include only lines that actually need a correction; if none do, answer {\"changes\":[]}.";

    public static string BuildSystemPrompt(string instructions, string languageName)
    {
        var prompt = (instructions ?? string.Empty).Replace("{language}", languageName);
        return prompt + ProtocolText;
    }

    public static string BuildUserContent(ReviewChunk chunk)
    {
        using var stream = new System.IO.MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            WriteLines(writer, "context_before", chunk.ContextBefore);
            WriteLines(writer, "lines", chunk.Lines);
            WriteLines(writer, "context_after", chunk.ContextAfter);
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static void WriteLines(Utf8JsonWriter writer, string name, List<ReviewLine> lines)
    {
        writer.WriteStartArray(name);
        foreach (var line in lines)
        {
            writer.WriteStartObject();
            writer.WriteNumber("n", line.Number);
            writer.WriteString("text", line.Text.Replace(Environment.NewLine, "\n"));
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
    }

    /// <summary>
    /// Parses the model reply. Tolerates markdown fences and stray prose around the JSON object.
    /// Only changes for numbers in <paramref name="editableNumbers"/> are returned.
    /// </summary>
    public static List<AiReviewChange> ParseChanges(string responseText, HashSet<int> editableNumbers)
    {
        var changes = new List<AiReviewChange>();
        var json = ExtractJsonObject(responseText);
        if (json == null)
        {
            return changes;
        }

        using var doc = JsonDocument.Parse(json);
        if (!doc.RootElement.TryGetProperty("changes", out var array) || array.ValueKind != JsonValueKind.Array)
        {
            return changes;
        }

        foreach (var element in array.EnumerateArray())
        {
            if (element.ValueKind != JsonValueKind.Object ||
                !element.TryGetProperty("n", out var nElement) ||
                !element.TryGetProperty("text", out var textElement) ||
                textElement.ValueKind != JsonValueKind.String)
            {
                continue;
            }

            int number;
            if (nElement.ValueKind == JsonValueKind.Number && nElement.TryGetInt32(out var n))
            {
                number = n;
            }
            else if (nElement.ValueKind == JsonValueKind.String && int.TryParse(nElement.GetString(), out var ns))
            {
                number = ns;
            }
            else
            {
                continue;
            }

            if (!editableNumbers.Contains(number))
            {
                continue; // hallucinated or context line
            }

            var text = (textElement.GetString() ?? string.Empty).Replace("\r\n", "\n").Replace("\n", Environment.NewLine).Trim();
            if (text.Length == 0)
            {
                continue;
            }

            var reason = element.TryGetProperty("reason", out var reasonElement) && reasonElement.ValueKind == JsonValueKind.String
                ? reasonElement.GetString() ?? string.Empty
                : string.Empty;
            var category = ParseCategory(element.TryGetProperty("category", out var categoryElement) && categoryElement.ValueKind == JsonValueKind.String
                ? categoryElement.GetString()
                : null);

            changes.Add(new AiReviewChange(number, text, reason, category));
        }

        return changes;
    }

    /// <summary>True when both texts contain the same formatting tags in the same order.</summary>
    public static bool TagsMatch(string before, string after)
    {
        return GetTags(before) == GetTags(after);
    }

    private static string GetTags(string text)
    {
        var sb = new StringBuilder();
        foreach (Match match in TagRegex.Matches(text ?? string.Empty))
        {
            sb.Append(match.Value);
        }

        return sb.ToString();
    }

    private static ReviewCategory ParseCategory(string? value)
    {
        var s = (value ?? string.Empty).Trim().ToLowerInvariant();
        if (s.Contains("spell") || s.Contains("typo"))
        {
            return ReviewCategory.Spelling;
        }

        if (s.Contains("grammar") || s.Contains("tense") || s.Contains("agreement"))
        {
            return ReviewCategory.Grammar;
        }

        if (s.Contains("punct") || s.Contains("comma") || s.Contains("period"))
        {
            return ReviewCategory.Punctuation;
        }

        if (s.Contains("cas") || s.Contains("capital"))
        {
            return ReviewCategory.Casing;
        }

        return ReviewCategory.Other;
    }

    internal static string? ExtractJsonObject(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var start = text.IndexOf('{');
        var end = text.LastIndexOf('}');
        if (start < 0 || end <= start)
        {
            return null;
        }

        var candidate = text.Substring(start, end - start + 1);
        try
        {
            using var _ = JsonDocument.Parse(candidate);
            return candidate;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
