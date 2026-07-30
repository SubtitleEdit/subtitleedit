using Avalonia.Platform;
using Nikse.SubtitleEdit.Logic.Compression;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Initializers;

public interface IThemeInitializer
{
    Task UpdateThemesIfNeeded();
}

public class ThemeInitializer(IZipUnpacker zipUnpacker) : IThemeInitializer
{
    private static int _repairAttempted;

    /// <summary>
    /// Recovery path for a theme folder that claims to be current (version.txt matches) but is
    /// missing files - e.g. an icon added between releases without a version bump, or a user
    /// deleting files by hand. Re-unpacks Themes.zip over the existing folder. At most one
    /// attempt per process, so a zip that is itself missing the file cannot loop.
    /// </summary>
    public static bool TryRepair(string missingFileName)
    {
        if (System.Threading.Interlocked.Exchange(ref _repairAttempted, 1) == 1)
        {
            return false;
        }

        try
        {
            Se.LogError($"Theme image \"{missingFileName}\" missing - re-unpacking Themes.zip to \"{Se.ThemesFolder}\"");
            var zipUri = new Uri("avares://SubtitleEdit/Assets/Themes.zip");
            using var zipStream = AssetLoader.Open(zipUri);
            new Compression.ZipUnpacker().UnpackZipStream(zipStream, Se.ThemesFolder);
            WriteNewVersionFile();
            return true;
        }
        catch (Exception e)
        {
            Se.LogError(e, $"Re-unpacking Themes.zip to \"{Se.ThemesFolder}\" failed");
            return false;
        }
    }

    public async Task UpdateThemesIfNeeded()
    {
        if (await NeedsUpdate())
        {
            await Unpack();
            WriteNewVersionFile();
        }
    }

    private static void WriteNewVersionFile()
    {
        string outputDir = Se.ThemesFolder;
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
        string outputDir = Se.ThemesFolder;
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
        var zipUri = new Uri("avares://SubtitleEdit/Assets/Themes.zip");
        await using var zipStream = AssetLoader.Open(zipUri);
        zipUnpacker.UnpackZipStream(zipStream, Se.ThemesFolder);
    }
}
