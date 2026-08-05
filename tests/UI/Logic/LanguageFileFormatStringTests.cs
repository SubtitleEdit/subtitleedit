using System.Text.Json;
using System.Text.RegularExpressions;
using Nikse.SubtitleEdit.Logic.Config.Language;

namespace UITests.Logic;

/// <summary>
/// Every shipped translation is fed to <c>string.Format</c> at the same call sites as the English
/// defaults, so a translated value with a mangled placeholder is not a cosmetic problem - it throws
/// FormatException and takes the window down. Bulgarian shipped 15 values whose opening brace had
/// been dropped ("Брой редове със субтитри: 0:#,##0}"), which threw on the first line of
/// File > Statistics, in batch convert and in export-as-images.
/// </summary>
public class LanguageFileFormatStringTests
{
    private static readonly Regex Placeholder = new(@"\{(\d+)(?:[,:][^}]*)?\}", RegexOptions.Compiled);

    private static string LanguagesFolder()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, "src", "ui", "Assets", "Languages");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate src/ui/Assets/Languages from " + AppContext.BaseDirectory);
    }

    /// <summary>Flattens a language object graph to "path -> string" using the JSON property names.</summary>
    private static Dictionary<string, string> Flatten(JsonElement element, string prefix = "")
    {
        var result = new Dictionary<string, string>();
        foreach (var property in element.EnumerateObject())
        {
            var path = prefix.Length == 0 ? property.Name : prefix + "." + property.Name;
            if (property.Value.ValueKind == JsonValueKind.Object)
            {
                foreach (var pair in Flatten(property.Value, path))
                {
                    result[pair.Key] = pair.Value;
                }
            }
            else if (property.Value.ValueKind == JsonValueKind.String)
            {
                result[path] = property.Value.GetString() ?? string.Empty;
            }
        }

        return result;
    }

    private static Dictionary<string, string> EnglishDefaults()
    {
        var json = JsonSerializer.Serialize(new SeLanguage(), new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        });

        using var document = JsonDocument.Parse(json);
        return Flatten(document.RootElement.Clone());
    }

    private static int ArgumentCount(string format)
    {
        var max = -1;
        foreach (Match match in Placeholder.Matches(format))
        {
            max = Math.Max(max, int.Parse(match.Groups[1].Value));
        }

        return max + 1;
    }

    [Fact]
    public void EveryTranslation_FormatsWithoutThrowing()
    {
        var english = EnglishDefaults();
        var failures = new List<string>();

        foreach (var file in Directory.EnumerateFiles(LanguagesFolder(), "*.json").OrderBy(p => p))
        {
            var name = Path.GetFileName(file);
            using var stream = File.OpenRead(file);
            using var document = JsonDocument.Parse(stream);
            foreach (var pair in Flatten(document.RootElement))
            {
                if (!english.TryGetValue(pair.Key, out var reference))
                {
                    continue; // stale key, not reachable from any call site
                }

                var argumentCount = ArgumentCount(reference);
                if (argumentCount == 0)
                {
                    // Not a format string: plain labels are shown verbatim, and several of them
                    // legitimately contain braces that string.Format would reject - the ASSA tag in
                    // "Add ASSA position tag (e.g. {\an8})", the hand-substituted {language} token in
                    // the AI prompt hints, "Curly brackets {...}".
                    continue;
                }

                // Call sites pass as many arguments as the English string has placeholders, so that
                // is exactly what the translation gets to work with.
                var args = new object[argumentCount];
                for (var i = 0; i < args.Length; i++)
                {
                    args[i] = 0;
                }

                try
                {
                    string.Format(pair.Value, args);
                }
                catch (FormatException exception)
                {
                    failures.Add($"{name} :: {pair.Key} :: \"{pair.Value}\" ({exception.Message})");
                }
            }
        }

        Assert.True(failures.Count == 0,
            $"{failures.Count} translated string(s) throw when formatted:{Environment.NewLine}{string.Join(Environment.NewLine, failures)}");
    }
}
