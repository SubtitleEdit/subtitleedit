using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.UiLogic.Translate;

public sealed class TranslationPostProcessDictionary
{
    public sealed record Rule(string? Replacement, IReadOnlyList<string> Suggestions, string? Note);
    public sealed record Entry(string SourceLanguage, string Source, string Draft, Rule Rule);

    private const string ResourcePrefix = "TranslationPostProcess/";
    private static readonly Regex Decorated = new(@"[<>\[\]{}♪♫\r\n\u2028\u2029]|^[-–—−]");
    private static readonly Lazy<Dictionary<string, string>> Resources = new(FindResources);
    private readonly Dictionary<(string Source, string Draft), Rule> _rules;

    private TranslationPostProcessDictionary(IEnumerable<Entry> entries)
    {
        _rules = entries.ToDictionary(entry => (Normalize(entry.Source), Normalize(entry.Draft)), entry => entry.Rule);
    }

    public static IEnumerable<string> Languages => Resources.Value.Keys.Order(StringComparer.Ordinal);

    public static TranslationPostProcessDictionary? Get(TranslationPair? source, TranslationPair? target, string? userFolder = null)
    {
        if (source == null || target == null)
        {
            return null;
        }

        var sourceCode = GetLanguage(source);
        var targetCode = GetLanguage(target);
        if (!Resources.Value.ContainsKey(targetCode))
        {
            return null;
        }

        IReadOnlyList<Entry> entries;
        try
        {
            entries = Read(targetCode, userFolder);
        }
        catch (Exception ex) when (userFolder != null && ex is (IOException or UnauthorizedAccessException or JsonException or InvalidDataException))
        {
            // A damaged or inaccessible user file should not interrupt translation.
            entries = Read(targetCode);
        }

        var rules = entries.Where(entry => entry.SourceLanguage.Equals(sourceCode, StringComparison.OrdinalIgnoreCase)).ToList();
        return rules.Count == 0 ? null : new TranslationPostProcessDictionary(rules);
    }

    public Rule? Match(string source, string draft)
    {
        return Decorated.IsMatch(source) || Decorated.IsMatch(draft)
            ? null
            : _rules.GetValueOrDefault((Normalize(source), Normalize(draft)));
    }

    public static IReadOnlyList<Entry> Read(string target, string? userFolder = null)
    {
        var resource = FindResource(target);
        var file = userFolder == null ? null : Path.Combine(userFolder, resource[ResourcePrefix.Length..]);
        using var stream = file != null && File.Exists(file)
            ? File.OpenRead(file)
            : typeof(TranslationPostProcessDictionary).Assembly.GetManifestResourceStream(resource)!;
        using var json = JsonDocument.Parse(stream);
        try
        {
            if (!string.Equals(json.RootElement.GetProperty("target_language").GetString(), target, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException("The dictionary target does not match the selected language.");
            }

            var entries = new List<Entry>();
            foreach (var source in json.RootElement.GetProperty("rules").EnumerateObject())
            {
                foreach (var item in source.Value.EnumerateArray())
                {
                    var replacement = item.TryGetProperty("replacement", out var value) ? value.GetString() : null;
                    var suggestions = item.TryGetProperty("suggestions", out var options)
                        ? options.EnumerateArray().Select(option => option.GetString() ?? string.Empty).ToArray()
                        : Array.Empty<string>();
                    var note = item.TryGetProperty("note", out var description) ? description.GetString() : null;
                    var rule = new Rule(replacement, suggestions, note);
                    entries.Add(new Entry(source.Name, item.GetProperty("source").GetString() ?? string.Empty,
                        item.GetProperty("draft").GetString() ?? string.Empty, rule));
                }
            }

            Validate(target, entries);
            return entries;
        }
        catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
        {
            throw new InvalidDataException("Invalid post-processing dictionary.", ex);
        }
    }

    public static void Save(string target, IReadOnlyList<Entry> entries, string userFolder)
    {
        var resource = FindResource(target);
        Validate(target, entries);
        Directory.CreateDirectory(userFolder);
        var file = Path.Combine(userFolder, resource[ResourcePrefix.Length..]);
        var temporaryFile = file + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            using (var stream = File.Create(temporaryFile))
            using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
            {
                writer.WriteStartObject();
                writer.WriteString("target_language", target);
                writer.WriteStartObject("rules");
                foreach (var group in entries.GroupBy(entry => entry.SourceLanguage, StringComparer.OrdinalIgnoreCase))
                {
                    writer.WriteStartArray(group.Key);
                    foreach (var entry in group)
                    {
                        writer.WriteStartObject();
                        writer.WriteString("source", entry.Source);
                        writer.WriteString("draft", entry.Draft);
                        if (entry.Rule.Replacement is { } replacement)
                        {
                            writer.WriteString("replacement", replacement);
                        }
                        else
                        {
                            writer.WriteStartArray("suggestions");
                            foreach (var suggestion in entry.Rule.Suggestions)
                            {
                                writer.WriteStringValue(suggestion);
                            }

                            writer.WriteEndArray();
                        }

                        if (!string.IsNullOrEmpty(entry.Rule.Note))
                        {
                            writer.WriteString("note", entry.Rule.Note);
                        }

                        writer.WriteEndObject();
                    }

                    writer.WriteEndArray();
                }

                writer.WriteEndObject();
                writer.WriteEndObject();
            }

            File.Move(temporaryFile, file, overwrite: true);
        }
        finally
        {
            File.Delete(temporaryFile);
        }
    }

    private static void Validate(string target, IReadOnlyList<Entry> entries)
    {
        var keys = new HashSet<(string Language, string Source, string Draft)>();
        foreach (var entry in entries)
        {
            var language = entry.SourceLanguage.ToLowerInvariant();
            if (!Resources.Value.ContainsKey(language) || language == target.ToLowerInvariant())
            {
                throw new InvalidDataException("Choose a different source language.");
            }

            var replacement = entry.Rule.Replacement;
            var suggestions = entry.Rule.Suggestions;
            if (!IsPlainCue(entry.Source) || !IsPlainCue(entry.Draft) ||
                (replacement != null ? !IsPlainCue(replacement) || suggestions.Count != 0 : suggestions.Count == 0 || !suggestions.All(IsPlainCue)))
            {
                throw new InvalidDataException("Enter a plain, single-line source cue, engine translation, and replacement or suggestions.");
            }

            if (!keys.Add((language, Normalize(entry.Source), Normalize(entry.Draft))))
            {
                throw new InvalidDataException("A rule already exists for this source cue and engine translation.");
            }
        }
    }

    private static bool IsPlainCue(string text) => !string.IsNullOrWhiteSpace(text) && !Decorated.IsMatch(text);
    private static string Normalize(string text) => text.Normalize(NormalizationForm.FormC).Replace('’', '\'');

    private static string FindResource(string target) => Resources.Value.TryGetValue(target, out var resource)
        ? resource
        : throw new ArgumentException("Unsupported target language.", nameof(target));

    private static Dictionary<string, string> FindResources() => typeof(TranslationPostProcessDictionary).Assembly
        .GetManifestResourceNames()
        .Where(name => name.StartsWith(ResourcePrefix, StringComparison.Ordinal))
        .ToDictionary(name => Path.GetFileNameWithoutExtension(name[ResourcePrefix.Length..]), StringComparer.OrdinalIgnoreCase);

    public static string GetLanguage(TranslationPair language)
    {
        var code = language.Code.Replace('_', '-').ToLowerInvariant();
        var iso = language.TwoLetterIsoLanguageName.Replace('_', '-').ToLowerInvariant();
        if (code is "pt-br" or "pt-pt")
        {
            return code;
        }

        if (iso is "pt-br" or "pt-pt")
        {
            return iso;
        }

        if (iso.StartsWith("zh", StringComparison.Ordinal))
        {
            if (code is "zh-hans" or "zho-hans" or "zh-cn" or "zh-sg" || iso is "zh-hans" or "zh-cn" or "zh-sg" ||
                language.Name.Contains("Simplified", StringComparison.OrdinalIgnoreCase))
            {
                return "zh-hans";
            }

            if (code is "zh-hant" or "zho-hant" or "zh-tw" or "zh-hk" || iso is "zh-hant" or "zh-tw" or "zh-hk" ||
                language.Name.Contains("Traditional", StringComparison.OrdinalIgnoreCase))
            {
                return "zh-hant";
            }
        }

        var neutral = iso.Split('-')[0];
        return neutral == "no" ? "nb" : neutral;
    }
}
