using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.UiLogic.Translate.LlamaCppAdvanced;

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
        "A sentence often continues from one line to the next: still translate each line on its own and " +
        "never move words to another line - each translation holds only the meaning of its own line.\n" +
        "Answer with ONLY a JSON object mapping every line number to its translation, e.g. " +
        "{\"1\":\"<translation of line 1>\",\"2\":\"<translation of line 2>\"}. Include each line number exactly once and nothing else.";

    /// <summary>
    /// Writer options for everything the model reads. The default encoder turns every non-ASCII
    /// character, and also quotes, apostrophes, angle brackets, ampersands and plus signs, into
    /// \uXXXX escapes; the server decodes only the request's outer JSON layer, so
    /// escapes inside the user message reach the model as literal backslash sequences and a
    /// Chinese line arrives as "\u4F60\u6628...". Relaxed escaping keeps the text readable and the
    /// JSON valid (quotes, backslashes and control characters are still escaped); "unsafe" only
    /// matters for JSON embedded in HTML, which this never is.
    /// </summary>
    internal static readonly JsonWriterOptions WireOptions = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

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
        using (var writer = new Utf8JsonWriter(stream, WireOptions))
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
        using (var writer = new Utf8JsonWriter(stream, WireOptions))
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
    /// <summary>
    /// True when a reply value is the "&lt;translation of line N&gt;" example from
    /// <see cref="ProtocolText"/> rather than a translation.
    /// </summary>
    public static bool IsExamplePlaceholder(string translation)
    {
        return ExamplePlaceholderRegex.IsMatch(translation ?? string.Empty);
    }

    /// <summary>
    /// True when a reply value is not a translation of <paramref name="source"/>: the prompt's
    /// example (<see cref="IsExamplePlaceholder"/>) or a value that swallowed protocol structure
    /// (<see cref="IsStructureLeak"/>).
    /// </summary>
    public static bool IsBrokenValue(string source, string translation)
    {
        return IsExamplePlaceholder(translation) || IsStructureLeak(source, translation);
    }

    /// <summary>
    /// True when a reply value holds more than its own line: the next lines' JSON
    /// (<c>2":"...</c>, <c>"}]}</c>) written inside the string with escaped quotes, or a runaway
    /// far longer than the source, e.g. the model's own notes ("Note: ... Let me correct that.").
    /// Index-Nailong 9B put four later lines and a comment into the value for "Come on up." (#16856).
    /// The schema only fixes the keys, so the parser accepts such a value as valid JSON.
    /// </summary>
    public static bool IsStructureLeak(string source, string translation)
    {
        if (string.IsNullOrEmpty(translation))
        {
            return false;
        }

        if (StructureLeakRegex.IsMatch(translation) && !StructureLeakRegex.IsMatch(source ?? string.Empty))
        {
            return true;
        }

        var sourceLength = (source ?? string.Empty).Trim().Length;
        return translation.Trim().Length > Math.Max(MinRunawayLength, RunawayFactor * sourceLength);
    }

    private const int MinRunawayLength = 80;
    private const int RunawayFactor = 4;

    // A line-number key with its colon and opening quote ("2":" / 2":"), or a JSON object/array end
    // right after a string ("}  "]}).
    private static readonly System.Text.RegularExpressions.Regex StructureLeakRegex =
        new(@"(?<![\d\p{L}])\d+""\s*:\s*""|""\s*\}|\}\s*\]\s*\}", System.Text.RegularExpressions.RegexOptions.CultureInvariant);

    private static readonly System.Text.RegularExpressions.Regex ExamplePlaceholderRegex =
        new(@"^\s*<\s*translation of line\s*\d*\s*>\s*$", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.CultureInvariant);

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
