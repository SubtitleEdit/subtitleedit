using Avalonia.Headless.XUnit;
using Avalonia.Platform;
using System.Text.Json;

namespace UITests.Logic;

/// <summary>
/// Validates that every translation file shipped in <c>src/ui/Assets/Languages</c> is well-formed
/// JSON, so a broken language file is caught here rather than at application start-up.
/// </summary>
public class LanguageJsonFilesTests
{
    /// <summary>One theory case per <c>*.json</c> file in the Languages folder.</summary>
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
    public void LanguageFile_IsValidJson(string fileName)
    {
        var path = Path.Combine(GetLanguagesFolder(), fileName);

        // File.ReadAllText strips a UTF-8 BOM; the language files are saved with one.
        var json = File.ReadAllText(path);

        var exception = Record.Exception(() => JsonDocument.Parse(json).Dispose());

        Assert.True(exception is null, $"{fileName} is not valid JSON: {exception?.Message}");
    }

    [Fact]
    public void LanguagesFolder_ContainsLanguageFiles()
    {
        var files = Directory.GetFiles(GetLanguagesFolder(), "*.json");

        Assert.NotEmpty(files);
        Assert.Contains(files, f => Path.GetFileName(f) == "English.json");
    }

    /// <summary>
    /// Every file in the Languages folder must be embedded as an Avalonia resource and be
    /// discoverable by folder enumeration - that is what unpacks the translations at start-up.
    /// A file added to the folder but not shipped would silently never reach the language picker.
    /// </summary>
    [AvaloniaFact]
    public void EmbeddedLanguageAssets_MatchLanguagesFolder()
    {
        var onDisk = Directory.GetFiles(GetLanguagesFolder(), "*.json")
            .Select(Path.GetFileName)
            .OrderBy(f => f, StringComparer.Ordinal);

        var embedded = AssetLoader
            .GetAssets(new Uri("avares://SubtitleEdit/Assets/Languages"), null)
            .Select(uri => Path.GetFileName(Uri.UnescapeDataString(uri.AbsolutePath)))
            .Where(f => f.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            .OrderBy(f => f, StringComparer.Ordinal);

        Assert.Equal(onDisk, embedded);
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

        throw new DirectoryNotFoundException(
            $"Could not locate 'src/ui/Assets/Languages' walking up from '{AppContext.BaseDirectory}'.");
    }
}
