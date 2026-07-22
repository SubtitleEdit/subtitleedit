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
    private static readonly Uri LanguagesFolderUri = new("avares://SubtitleEdit/Assets/Languages");

    public async Task UpdateLanguagesIfNeeded()
    {
        if (await NeedsUpdate())
        {
            // Only stamp the version once everything actually landed. Stamping regardless would
            // mark a half-unpacked folder as current, and this version would then never retry.
            if (await Unpack())
            {
                WriteNewVersionFile();
            }
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

    /// <returns><see langword="true"/> when every language file was written.</returns>
    private async Task<bool> Unpack()
    {
        var outputDir = Se.TranslationFolder;
        if (!Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        var allWritten = true;

        foreach (var uri in AssetLoader.GetAssets(LanguagesFolderUri, null))
        {
            var fileName = Path.GetFileName(Uri.UnescapeDataString(uri.AbsolutePath));
            if (!fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            try
            {
                await using var stream = AssetLoader.Open(uri);
                var outputPath = Path.Combine(outputDir, fileName);
                await using var fileStream = File.Create(outputPath);
                await stream.CopyToAsync(fileStream);
            }
            catch
            {
                Se.LogError($"Could not unpack language file \"{fileName}\".");
                allWritten = false;
            }
        }

        return allWritten;
    }
}
