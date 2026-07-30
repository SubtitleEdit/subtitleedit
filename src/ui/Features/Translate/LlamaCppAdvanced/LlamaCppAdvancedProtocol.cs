using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.Json;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Translate.LlamaCppAdvanced;

/// <summary>
/// The wire format for the advanced llama.cpp engine: numbered lines in, a JSON object mapping
/// each line number to its translation out. The system prompt is deliberately ordered
/// stable-parts-first (instructions, synopsis, glossary) so llama-server's automatic prefix
/// KV-cache reuse (cache_prompt) only has to re-ingest the per-batch tail of each request.
/// </summary>
public static class LlamaCppAdvancedProtocol
{
    public const string DefaultPrompt =
        "You are a professional subtitle translator. Translate each subtitle line from {0} to {1}. " +
        "Preserve meaning, tone and style; do not censor the translation. " +
        "Keep formatting tags exactly as they are.";

    private const string ProtocolText =
        "\n\nThe user message is a JSON object. \"history\" holds recent already-translated lines (source and translation) - " +
        "use them only for context and consistency (names, pronouns, formality), do not re-translate them. " +
        "\"lines\" holds the lines to translate now, each with a line number \"n\" and its \"text\" " +
        "(a \"\\n\" inside a text is a line break inside that subtitle).\n" +
        "Answer with ONLY a JSON object mapping every line number to its translation, e.g. " +
        "{\"1\":\"...\",\"2\":\"...\"}. Include each line number exactly once and nothing else.";

    public record HistoryPair(string Source, string Target);
    public record BatchLine(int Number, string Text);

    /// <summary>
    /// Builds the system prompt. Placeholders are replaced (not string.Format'ed) so braces in
    /// subtitle tags or user-edited prompts cannot throw a FormatException.
    /// </summary>
    public static string BuildSystemPrompt(string sourceLanguage, string targetLanguage, SeLlamaCppAdvanced settings)
    {
        var instructions = string.IsNullOrWhiteSpace(settings.Prompt) ? DefaultPrompt : settings.Prompt;
        var sb = new StringBuilder(instructions.Replace("{0}", sourceLanguage).Replace("{1}", targetLanguage));

        if (settings.KeepLineBreaks)
        {
            sb.Append(" Keep the same number of line breaks in each translation as in its source line.");
        }

        if (settings.Formality == SeLlamaCppAdvanced.FormalityFormal)
        {
            sb.Append(" Use formal address (e.g. German \"Sie\", French \"vous\", Spanish \"usted\").");
        }
        else if (settings.Formality == SeLlamaCppAdvanced.FormalityInformal)
        {
            sb.Append(" Use informal address (e.g. German \"du\", French \"tu\", Spanish \"tú\").");
        }

        if (!string.IsNullOrWhiteSpace(settings.Synopsis))
        {
            sb.Append("\n\nAbout the content being translated:\n").Append(settings.Synopsis.Trim());
        }

        var glossary = ParseGlossary(settings.Glossary);
        if (glossary.Count > 0)
        {
            sb.Append("\n\nGlossary - always translate these terms exactly as given:");
            foreach (var pair in glossary)
            {
                sb.Append('\n').Append(pair.Key).Append(" = ").Append(pair.Value);
            }
        }

        sb.Append(ProtocolText);
        return sb.ToString();
    }

    /// <summary>Glossary text box format: one "source term = translation" per line.</summary>
    public static List<KeyValuePair<string, string>> ParseGlossary(string glossary)
    {
        var result = new List<KeyValuePair<string, string>>();
        foreach (var line in (glossary ?? string.Empty).SplitToLines())
        {
            var idx = line.IndexOf('=');
            if (idx <= 0)
            {
                continue;
            }

            var source = line.Substring(0, idx).Trim();
            var target = line.Substring(idx + 1).Trim();
            if (source.Length > 0 && target.Length > 0)
            {
                result.Add(new KeyValuePair<string, string>(source, target));
            }
        }

        return result;
    }

    public static string BuildUserContent(List<HistoryPair> history, List<BatchLine> lines)
    {
        using var stream = new System.IO.MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();

            writer.WriteStartArray("history");
            foreach (var pair in history)
            {
                writer.WriteStartObject();
                writer.WriteString("source", ToWireText(pair.Source));
                writer.WriteString("translation", ToWireText(pair.Target));
                writer.WriteEndObject();
            }
            writer.WriteEndArray();

            writer.WriteStartArray("lines");
            foreach (var line in lines)
            {
                writer.WriteStartObject();
                writer.WriteNumber("n", line.Number);
                writer.WriteString("text", ToWireText(line.Text));
                writer.WriteEndObject();
            }
            writer.WriteEndArray();

            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    /// <summary>
    /// The response_format value for llama-server: an OpenAI-style json_schema requiring exactly
    /// the batch's line numbers as keys. llama.cpp compiles this to a grammar, so the reply is
    /// guaranteed to be a parseable object with all keys present.
    /// </summary>
    public static string BuildResponseFormatJson(IReadOnlyList<BatchLine> lines)
    {
        using var stream = new System.IO.MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            writer.WriteString("type", "json_schema");
            writer.WriteStartObject("json_schema");
            writer.WriteString("name", "translations");
            writer.WriteBoolean("strict", true);
            writer.WriteStartObject("schema");
            writer.WriteString("type", "object");

            writer.WriteStartObject("properties");
            foreach (var line in lines)
            {
                writer.WriteStartObject(line.Number.ToString(CultureInfo.InvariantCulture));
                writer.WriteString("type", "string");
                writer.WriteEndObject();
            }
            writer.WriteEndObject();

            writer.WriteStartArray("required");
            foreach (var line in lines)
            {
                writer.WriteStringValue(line.Number.ToString(CultureInfo.InvariantCulture));
            }
            writer.WriteEndArray();

            writer.WriteBoolean("additionalProperties", false);
            writer.WriteEndObject(); // schema
            writer.WriteEndObject(); // json_schema
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    /// <summary>
    /// Parses the model reply into number → translation. Tolerates markdown fences and stray
    /// prose around the JSON object (the plain-text fallback path has no grammar guarantee).
    /// </summary>
    public static Dictionary<int, string> ParseTranslations(string responseText)
    {
        var result = new Dictionary<int, string>();
        var json = ExtractJsonObject(responseText);
        if (json == null)
        {
            return result;
        }

        using var doc = JsonDocument.Parse(json);
        if (doc.RootElement.ValueKind != JsonValueKind.Object)
        {
            return result;
        }

        foreach (var property in doc.RootElement.EnumerateObject())
        {
            if (property.Value.ValueKind == JsonValueKind.String &&
                int.TryParse(property.Name, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number))
            {
                result[number] = FromWireText(property.Value.GetString());
            }
        }

        return result;
    }

    private static string ToWireText(string text)
    {
        return (text ?? string.Empty).Replace(Environment.NewLine, "\n");
    }

    private static string FromWireText(string? text)
    {
        return (text ?? string.Empty).Replace("\r\n", "\n").Replace("\n", Environment.NewLine).Trim();
    }

    private static string? ExtractJsonObject(string text)
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
