using Avalonia.Platform;
using Nikse.SubtitleEdit.Logic.Compression;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Initializers;

public interface ILanguageInitializer
{
    Task UpdateLanguagesIfNeeded();
}

public class LanguageInitializer(IZipUnpacker zipUnpacker) : ILanguageInitializer
{
    private static readonly Uri LanguagesZipUri = new("avares://SubtitleEdit/Assets/Languages.zip");

    public async Task UpdateLanguagesIfNeeded()
    {
        if (await NeedsUpdate())
        {
            // Only stamp the version once everything actually landed. Stamping regardless would
            // mark a half-unpacked folder as current, and this version would then never retry.
            if (Unpack())
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
    private bool Unpack()
    {
        try
        {
            using var zipStream = AssetLoader.Open(LanguagesZipUri);
            zipUnpacker.UnpackZipStream(zipStream, Se.TranslationFolder);
            return true;
        }
        catch (Exception exception)
        {
            Se.LogError(exception, $"Could not unpack language files into \"{Se.TranslationFolder}\".");
            return false;
        }
    }
}
