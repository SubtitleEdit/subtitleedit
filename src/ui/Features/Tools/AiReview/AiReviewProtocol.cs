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
        "Answer with ONLY a JSON object, no other text: {\"changes\":[{\"n\":<line number>,\"orig\":\"<that line's text, copied unchanged>\"," +
        "\"text\":\"<full corrected text>\",\"reason\":\"<short reason>\",\"category\":\"spelling|grammar|punctuation|casing|other\"}]}. " +
        "\"orig\" must be the exact original text of line \"n\" - it is used to verify the line number. " +
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
    /// Only changes for numbers in <paramref name="editableLines"/> (number -> current text) are
    /// returned. When the model echoes the original text ("orig"), the echo is checked against
    /// line "n": a change whose echo clearly belongs to a different editable line is remapped to
    /// that line, and a change whose echo matches no editable line is dropped - small models
    /// periodically shift their line numbers by one and would otherwise pair a correction with
    /// the wrong "before" line.
    /// </summary>
    public static List<AiReviewChange> ParseChanges(string responseText, IReadOnlyDictionary<int, string> editableLines)
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

        var usedNumbers = new HashSet<int>();
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

            if (!editableLines.ContainsKey(number))
            {
                continue; // hallucinated or context line
            }

            var text = (textElement.GetString() ?? string.Empty).Replace("\r\n", "\n").Replace("\n", Environment.NewLine).Trim();
            if (text.Length == 0)
            {
                continue;
            }

            var orig = element.TryGetProperty("orig", out var origElement) && origElement.ValueKind == JsonValueKind.String
                ? origElement.GetString() ?? string.Empty
                : string.Empty;
            number = VerifyNumberAgainstEcho(number, orig, text, editableLines);
            if (number < 0 || !usedNumbers.Add(number))
            {
                continue; // echo matches no editable line, or that line already has a change
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

    /// <summary>
    /// Checks the model's echo of the original text against line <paramref name="number"/>.
    /// Returns the number to use for the change, or -1 when the change should be dropped.
    /// An echo that just repeats the corrected text carries no information and is ignored.
    /// </summary>
    private static int VerifyNumberAgainstEcho(int number, string orig, string text, IReadOnlyDictionary<int, string> editableLines)
    {
        var origKey = NormalizeForMatch(orig);
        if (origKey.Length == 0 || origKey == NormalizeForMatch(text))
        {
            return number; // no usable echo - trust the model's line number
        }

        if (origKey == NormalizeForMatch(editableLines[number]))
        {
            return number; // echo confirms the line number
        }

        // the echo does not match line "n" - remap if it matches exactly one other editable line
        var match = -1;
        foreach (var line in editableLines)
        {
            if (NormalizeForMatch(line.Value) == origKey)
            {
                if (match >= 0)
                {
                    return -1; // ambiguous
                }

                match = line.Key;
            }
        }

        if (match >= 0)
        {
            return match;
        }

        // no exact match anywhere; accept a near-exact echo of line "n" (models often normalize
        // quotes or dashes when copying), otherwise the change points at an unknown line - drop it
        return GetSimilarityPercent(orig, editableLines[number]) >= 90 ? number : -1;
    }

    /// <summary>
    /// True when <paramref name="after"/> barely resembles <paramref name="before"/> but is a
    /// (near-)copy of one of <paramref name="neighborTexts"/> - the fingerprint of a model that
    /// shifted its line numbers, pairing line n's correction with line n±1.
    /// </summary>
    public static bool LooksMisaligned(string before, string after, IEnumerable<string> neighborTexts)
    {
        var own = GetSimilarityPercent(before, after);
        if (own >= 50)
        {
            return false;
        }

        foreach (var neighbor in neighborTexts)
        {
            var similarity = GetSimilarityPercent(after, neighbor);
            if (similarity >= 75 && similarity > own)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Levenshtein ratio in percent over tag-stripped, lower-cased, whitespace-free text.</summary>
    internal static int GetSimilarityPercent(string a, string b)
    {
        var s1 = NormalizeForMatch(a);
        var s2 = NormalizeForMatch(b);
        if (s1.Length == 0 && s2.Length == 0)
        {
            return 100;
        }

        if (s1.Length == 0 || s2.Length == 0)
        {
            return 0;
        }

        var maxLength = Math.Max(s1.Length, s2.Length);
        return (int)Math.Round(100.0 * (maxLength - GetLevenshteinDistance(s1, s2)) / maxLength);
    }

    private static string NormalizeForMatch(string text)
    {
        var s = TagRegex.Replace(text ?? string.Empty, string.Empty);
        var sb = new StringBuilder(s.Length);
        foreach (var ch in s)
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
