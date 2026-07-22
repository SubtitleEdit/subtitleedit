using System.Collections;
using System.Reflection;
using System.Text.Json;
using Nikse.SubtitleEdit.Logic.Config.Language;

namespace UITests.Logic.Config;

/// <summary>
/// <c>Se.LoadLanguage</c> deserializes translations through the source-generated
/// <see cref="SeLanguageJsonContext"/> rather than the reflection-based serializer, because the
/// reflection path cost ~315 ms on the start-up path (the graph is ~66 classes / ~3300 properties
/// and each language file is ~185 KB).
///
/// Source generation only maps properties it can see: a property that the generator skips — a
/// missing setter, a non-public type, a new nested section without a parameterless constructor —
/// deserializes as null instead of failing, so the UI would silently fall back to English for
/// those strings. These tests compare the generated context against the reflection serializer for
/// every shipped translation so such a regression fails the build instead of shipping.
/// </summary>
public class SeLanguageJsonContextTests
{
    public static TheoryData<string> LanguageFileNames()
    {
        var data = new TheoryData<string>();
        foreach (var path in Directory.GetFiles(GetLanguagesFolder(), "*.json"))
        {
            data.Add(Path.GetFileName(path));
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(LanguageFileNames))]
    public void SourceGeneratedContext_MatchesReflectionSerializer(string fileName)
    {
        var json = File.ReadAllText(Path.Combine(GetLanguagesFolder(), fileName));

        var viaReflection = JsonSerializer.Deserialize<SeLanguage>(
            json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        var viaSourceGen = JsonSerializer.Deserialize(json, SeLanguageJsonContext.Default.SeLanguage);

        Assert.NotNull(viaReflection);
        Assert.NotNull(viaSourceGen);

        var differences = new List<string>();
        Compare(viaReflection, viaSourceGen, "SeLanguage", differences);

        Assert.True(
            differences.Count == 0,
            $"{fileName}: source-generated context differs from the reflection serializer at "
            + $"{differences.Count} place(s):{Environment.NewLine}"
            + string.Join(Environment.NewLine, differences.Take(20)));
    }

    /// <summary>
    /// A translated string that survives the round trip proves the comparison above is actually
    /// reading translated content, not two identically-empty graphs.
    /// </summary>
    [Fact]
    public void SourceGeneratedContext_ReadsTranslatedStrings()
    {
        var json = File.ReadAllText(Path.Combine(GetLanguagesFolder(), "Danish.json"));

        var language = JsonSerializer.Deserialize(json, SeLanguageJsonContext.Default.SeLanguage);

        Assert.NotNull(language);
        Assert.False(string.IsNullOrWhiteSpace(language.General.Ok));
        Assert.False(string.IsNullOrWhiteSpace(language.General.Cancel));
    }

    private static void Compare(object? expected, object? actual, string path, List<string> differences)
    {
        if (differences.Count > 50)
        {
            return;
        }

        if (expected is null && actual is null)
        {
            return;
        }

        if (expected is null || actual is null)
        {
            differences.Add($"{path}: reflection={Describe(expected)} sourceGen={Describe(actual)}");
            return;
        }

        var type = expected.GetType();
        if (type == typeof(string) || type.IsPrimitive || type.IsEnum || type == typeof(decimal))
        {
            if (!Equals(expected, actual))
            {
                differences.Add($"{path}: reflection={Describe(expected)} sourceGen={Describe(actual)}");
            }

            return;
        }

        if (expected is IList expectedList && actual is IList actualList)
        {
            if (expectedList.Count != actualList.Count)
            {
                differences.Add($"{path}.Count: reflection={expectedList.Count} sourceGen={actualList.Count}");
                return;
            }

            for (var i = 0; i < expectedList.Count; i++)
            {
                Compare(expectedList[i], actualList[i], $"{path}[{i}]", differences);
            }

            return;
        }

        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!property.CanRead || property.GetIndexParameters().Length > 0)
            {
                continue;
            }

            Compare(property.GetValue(expected), property.GetValue(actual), $"{path}.{property.Name}", differences);
        }
    }

    private static string Describe(object? value)
    {
        if (value is null)
        {
            return "<null>";
        }

        var text = value.ToString() ?? string.Empty;
        return text.Length > 80 ? text[..80] + "..." : text;
    }

    /// <summary>
    /// Walks up from the test output directory to the repository root and returns the
    /// <c>src/ui/Assets/Languages</c> folder. Throws when it cannot be found.
    /// </summary>
    private static string GetLanguagesFolder()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
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
}
