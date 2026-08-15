using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using SeConv.Commands;
using SeConv.Core;

namespace SeConv.Helpers;

/// <summary>
/// Machine-readable description of the command line, reflected off
/// <see cref="ConvertCommand.Settings"/> so it cannot drift from the options the parser
/// actually binds. Drives <c>seconv --help-json</c> and the "did you mean" suggestion
/// printed for an unknown option.
/// </summary>
internal static class CliSchema
{
    public const string Version = "5.0.0";

    /// <summary>Schema revision, bumped when the emitted JSON shape changes (not its contents).</summary>
    public const int SchemaVersion = 1;

    /// <summary>
    /// Subcommands that read valid values for an option at runtime. An agent that hits an
    /// "unknown value" error can follow this to the enumeration instead of guessing.
    /// </summary>
    private static readonly Dictionary<string, string> DiscoveryCommands = new(StringComparer.OrdinalIgnoreCase)
    {
        ["--format"] = "seconv formats --json",
        ["--encoding"] = "seconv list-encodings --json",
        ["--input-encoding-fallback"] = "seconv list-encodings --json",
        ["--pac-codepage"] = "seconv list-pac-codepages --json",
        ["--ocr-engine"] = "seconv list-ocr-engines --json",
        ["--fix-common-errors-rules"] = "seconv list-fce-rules --json",
        ["--remove-formatting-rules"] = "seconv list-rf-rules --json",
        ["--settings"] = "seconv dump-settings",
    };

    /// <summary>
    /// Closed value sets, for options whose validator rejects anything else. Kept here rather
    /// than parsed out of the description text; <c>CliSchemaTest</c> asserts every key still
    /// names a real option.
    /// </summary>
    private static readonly Dictionary<string, string[]> Choices = new(StringComparer.OrdinalIgnoreCase)
    {
        ["--ocr-engine"] = ["tesseract", "nocr", "binaryocr", "ollama", "llamacpp", "paddle"],
        ["--translate-engine"] = ["llamacpp", "ollama", "lmstudio", "libretranslate", "nllb-serve", "nllb-api"],
        ["--box-type"] = ["none", "one-box", "box-per-line"],
        ["--content-alignment"] = ["left", "center", "right"],
        ["--alignment"] =
        [
            "top-left", "top-center", "top-right",
            "middle-left", "middle-center", "middle-right",
            "bottom-left", "bottom-center", "bottom-right",
        ],
    };

    /// <summary>
    /// Value-carrying options that belong to the operations pipeline rather than to output
    /// configuration. The boolean operations are taken from
    /// <see cref="OperationOrderParser.ToggleOperations"/> so that half of the grouping
    /// cannot drift.
    /// </summary>
    private static readonly HashSet<string> ValueOperations = new(StringComparer.OrdinalIgnoreCase)
    {
        "--apply-min-gap",
        "--bridge-gaps",
        "--delete-first",
        "--delete-last",
        "--delete-contains",
        "--fix-common-errors-rules",
        "--fce-language",
        "--dictionary-folder",
        "--remove-formatting-rules",
    };

    internal sealed record OptionInfo(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("aliases")] IReadOnlyList<string> Aliases,
        [property: JsonPropertyName("type")] string Type,
        [property: JsonPropertyName("group")] string Group,
        [property: JsonPropertyName("description")] string Description,
        [property: JsonPropertyName("choices"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyList<string>? Choices,
        [property: JsonPropertyName("discover"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Discover);

    private static IReadOnlyList<OptionInfo>? _options;

    /// <summary>
    /// Every option the convert command binds, in declaration order. The first name in a
    /// Spectre template is the canonical one; the rest are back-compat aliases.
    /// </summary>
    public static IReadOnlyList<OptionInfo> Options => _options ??= BuildOptions();

    private static IReadOnlyList<OptionInfo> BuildOptions()
    {
        var toggleOperations = new HashSet<string>(
            OperationOrderParser.ToggleOperations.Select(o => "--" + o.ToLowerInvariant()),
            StringComparer.OrdinalIgnoreCase);

        var options = new List<OptionInfo>();
        foreach (var property in typeof(ConvertCommand.Settings).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var template = ReadAttributeTemplate(property, "CommandOptionAttribute");
            if (template is null)
            {
                continue;
            }

            var names = ParseTemplate(template);
            if (names.Count == 0)
            {
                continue;
            }

            var name = names[0];
            // A toggle operation's canonical flag is its PascalCase name hyphen-stripped, so
            // compare on the same footing (--remove-text-for-hi vs RemoveTextForHI).
            var isOperation = ValueOperations.Contains(name) ||
                              toggleOperations.Contains("--" + name.TrimStart('-').Replace("-", string.Empty));

            options.Add(new OptionInfo(
                Name: name,
                Aliases: names.Skip(1).ToArray(),
                Type: DescribeType(property.PropertyType),
                Group: isOperation ? "operation" : "option",
                Description: ReadDescription(property),
                Choices: Choices.TryGetValue(name, out var choices) ? choices : null,
                Discover: DiscoveryCommands.TryGetValue(name, out var discover) ? discover : null));
        }

        return options;
    }

    /// <summary>
    /// Every accepted option token, canonical names and aliases alike. Used to reject
    /// unknown options with a suggestion rather than a bare parser error.
    /// </summary>
    public static IReadOnlyCollection<string> AllTokens =>
        Options.SelectMany(o => new[] { o.Name }.Concat(o.Aliases)).ToArray();

    /// <summary>
    /// Best-matching known option for a mistyped one, or null when nothing is close enough.
    /// The distance budget scales with length so short flags don't match everything, and
    /// a token that is a prefix/suffix of a real option always matches (e.g. the common
    /// "--remove-formating" single-letter drop).
    /// </summary>
    public static string? SuggestClosest(string unknown)
    {
        var needle = "--" + unknown.TrimStart('-', '/');
        if (needle.Length <= 2)
        {
            return null;
        }

        var budget = Math.Max(2, needle.Length / 4);
        string? best = null;
        var bestDistance = int.MaxValue;

        foreach (var candidate in AllTokens)
        {
            var distance = Levenshtein(needle, candidate);
            if (distance < bestDistance && distance <= budget)
            {
                bestDistance = distance;
                best = candidate;
            }
        }

        return best;
    }

    private static int Levenshtein(string a, string b)
    {
        // Case-insensitive: the parser accepts --FixCommonErrors as well as --fix-common-errors,
        // so a casing-only difference should never read as a typo.
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
                var cost = char.ToLowerInvariant(a[i - 1]) == char.ToLowerInvariant(b[j - 1]) ? 0 : 1;
                current[j] = Math.Min(Math.Min(current[j - 1] + 1, previous[j] + 1), previous[j - 1] + cost);
            }

            (previous, current) = (current, previous);
        }

        return previous[b.Length];
    }

    /// <summary>
    /// Reads a Spectre attribute's template through <see cref="CustomAttributeData"/> rather
    /// than a typed property: the template is a constructor argument, and which properties
    /// the attribute exposes has changed between Spectre versions.
    /// </summary>
    private static string? ReadAttributeTemplate(PropertyInfo property, string attributeTypeName)
    {
        foreach (var data in property.GetCustomAttributesData())
        {
            if (data.AttributeType.Name != attributeTypeName)
            {
                continue;
            }

            foreach (var argument in data.ConstructorArguments)
            {
                if (argument.Value is string template)
                {
                    return template;
                }
            }
        }

        return null;
    }

    private static string ReadDescription(PropertyInfo property) =>
        property.GetCustomAttribute<DescriptionAttribute>()?.Description ?? string.Empty;

    /// <summary>
    /// Splits "--format|-f &lt;VALUE&gt;" into its option names, dropping any value
    /// placeholder Spectre allows after the names.
    /// </summary>
    private static List<string> ParseTemplate(string template)
    {
        var names = new List<string>();
        foreach (var part in template.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var name = part;
            var cut = name.IndexOfAny([' ', '<', '[']);
            if (cut > 0)
            {
                name = name[..cut].TrimEnd();
            }

            if (name.Length > 0 && name[0] == '-')
            {
                names.Add(name);
            }
        }

        return names;
    }

    private static string DescribeType(Type type)
    {
        var underlying = Nullable.GetUnderlyingType(type) ?? type;

        if (underlying == typeof(bool))
        {
            return "flag";
        }
        if (underlying == typeof(int) || underlying == typeof(long))
        {
            return "integer";
        }
        if (underlying == typeof(double) || underlying == typeof(float) || underlying == typeof(decimal))
        {
            return "number";
        }
        if (underlying == typeof(string[]))
        {
            return "string[]";
        }

        return "string";
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    /// <summary>
    /// The full command-line schema as JSON: usage, value syntax, exit codes, arguments,
    /// options and subcommands. Emitted by <c>seconv --help-json</c>.
    /// </summary>
    public static string ToJson()
    {
        var document = new
        {
            name = "seconv",
            version = Version,
            schemaVersion = SchemaVersion,
            description = "Subtitle Edit command line converter: batch convert subtitle files between 380+ formats.",
            usage = new[]
            {
                "seconv <pattern> <format> [options]",
                "seconv <pattern> --format <name> [options]",
                "seconv <subcommand> [args]",
            },
            valueSyntax = new
            {
                accepted = new[] { "--option:value", "--option=value", "--option value" },
                note = "All three forms are equivalent. Unknown options are an error, not a no-op.",
            },
            exitCodes = new[]
            {
                new { code = 0, meaning = "All matched files converted successfully (or the subcommand succeeded)" },
                new { code = 1, meaning = "Any failure: bad usage, unknown option, no files matched, or one or more files failed" },
            },
            arguments = new[]
            {
                new
                {
                    name = "pattern",
                    type = "string[]",
                    required = true,
                    description = "One or more file name patterns. Comma-separated in one argument, or several arguments. Relative to --input-folder when set.",
                },
                new
                {
                    name = "format",
                    type = "string",
                    required = false,
                    description = "Target format name without spaces, as the second positional argument. Equivalent to --format. Discover with: seconv formats --json",
                },
            },
            options = Options,
            subcommands = new[]
            {
                new { name = "formats", json = true, description = "List every subtitle format. The 'id' field is the exact token --format accepts." },
                new { name = "list-encodings", json = true, description = "List supported text encodings (--encoding values)." },
                new { name = "list-pac-codepages", json = true, description = "List PAC code pages (--pac-codepage values)." },
                new { name = "list-ocr-engines", json = true, description = "List OCR engines (--ocr-engine values) and their installation status." },
                new { name = "list-fce-rules", json = true, description = "List FixCommonErrors rule IDs (--fix-common-errors-rules values)." },
                new { name = "list-rf-rules", json = true, description = "List remove-formatting rule IDs (--remove-formatting-rules values)." },
                new { name = "dump-settings", json = true, description = "Print a full --settings JSON with libse defaults. Always JSON; redirect to a file." },
                new { name = "info", json = true, description = "Print format / encoding / duration / language for one file. Usage: seconv info <file>" },
                new { name = "lint", json = true, description = "Validate subtitles; exit 1 if any issues found. Usage: seconv lint <pattern>..." },
                new { name = "--help-json", json = true, description = "Print this schema." },
                new { name = "--version", json = false, description = "Print the seconv version and exit." },
            },
        };

        return JsonSerializer.Serialize(document, JsonOptions);
    }
}
