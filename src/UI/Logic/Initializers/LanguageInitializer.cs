using Avalonia.Platform;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Initializers;

public interface ILanguageInitializer
{
    Task UpdateLanguagesIfNeeded();
}

public class LanguageInitializer() : ILanguageInitializer
{
    // List of available language files in Assets/Languages folder
    private static readonly string[] LanguageFiles =
    [
        "Arabic",
        "Basque",
        "Bulgarian",
        "ChineseSimplified",
        "ChineseTraditional",
        "Czech",
        "Danish",
        "Dutch",
        "English",
        "Estonian",
        "Farsi",
        "Finnish",
        "French",
        "German",
        "Hebrew",
        "Hungarian",
        "Italian",
        "Japanese",
        "Korean",
        "Norwegian",
        "Polish",
        "Portuguese",
        "Romanian",
        "Russian",
        "Slovak",
        "Spanish",
        "Swedish",
        "Thai",
        "Turkish",
        "Ukrainian",
        "Vietnamese",
    ];

    public async Task UpdateLanguagesIfNeeded()
    {
        if (await NeedsUpdate())
        {
            await Unpack();
            WriteNewVersionFile();
        }
    }

    private static void WriteNewVersionFile()
    {
        string outputDir = Se.TranslationFolder;
        try
        {
            if (!Directory.Exists(outputDir))
            {
                return;
            }

            var versionFileName = Path.Combine(outputDir, "version.txt");
            File.Delete(versionFileName);
            File.WriteAllText(versionFileName, Se.Version);
        }
        catch
        {
            Se.LogError($"Could not write version file in \"{outputDir}\" folder.");
        }
    }

    private static async Task<bool> NeedsUpdate()
    {
        string outputDir = Se.TranslationFolder;
        if (!Directory.Exists(outputDir))
        {
            return true;
        }

        var versionFileName = Path.Combine(outputDir, "version.txt");
        if (!File.Exists(versionFileName))
        {
            return true;
        }

        var currentNormalizedVersion = new SemanticVersion(Se.Version);

        var version = await File.ReadAllTextAsync(versionFileName);
        var normalizedVersion = new SemanticVersion(version);

        if (normalizedVersion.IsLessThan(currentNormalizedVersion))
        {
            return true;
        }

        return false;
    }

    private async Task Unpack()
    {
        var outputDir = Se.TranslationFolder;
        if (!Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        foreach (var languageFile in LanguageFiles)
        {
            try
            {
                var uri = new Uri($"avares://SubtitleEdit/Assets/Languages/{languageFile}.json");
                await using var stream = AssetLoader.Open(uri);
                var outputPath = Path.Combine(outputDir, $"{languageFile}.json");
                await using var fileStream = File.Create(outputPath);
                await stream.CopyToAsync(fileStream);
            }
            catch
            {
                // Ignore
            }
        }
    }
}
