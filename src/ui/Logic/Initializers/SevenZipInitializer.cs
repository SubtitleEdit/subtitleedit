using Avalonia.Platform;
using Nikse.SubtitleEdit.Logic.Compression;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Initializers;

public interface ISevenZipInitializer
{
    Task UpdateSevenZipIfNeeded();
}

public class SevenZipInitializer() : ISevenZipInitializer
{
    public async Task UpdateSevenZipIfNeeded()
    {
        string outputDir = Se.SevenZipFolder;

        if (OperatingSystem.IsWindows())
        {
            if (await NeedsUpdate(outputDir))
            {
                // Only stamp the version once the unpack succeeded - otherwise a failed unpack
                // is recorded as current and 7-Zip is never retried for this version.
                if (await UnpackWindows(outputDir))
                {
                    WriteNewVersionFile(outputDir);
                }
            }
        }

        if (OperatingSystem.IsLinux() && RuntimeInformation.ProcessArchitecture == Architecture.X64)
        {
            if (await NeedsUpdate(outputDir))
            {
                if (await UnpackLinux64(outputDir))
                {
                    WriteNewVersionFile(outputDir);
                }
            }
        }
    }

    private static void WriteNewVersionFile(string outputDir)
    {
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

    private static async Task<bool> NeedsUpdate(string outputDir)
    {
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

    /// <returns><see langword="true"/> when 7-Zip was unpacked.</returns>
    private static async Task<bool> UnpackWindows(string outputDir)
    {
        try
        {
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            var zipUri = new Uri("avares://SubtitleEdit/Assets/SevenZip/7zaWindows64.zip");
            await using var zipStream = AssetLoader.Open(zipUri);
            var zipUnpacker = new ZipUnpacker();
            zipUnpacker.UnpackZipStream(zipStream, outputDir);
            return true;
        }
        catch (Exception exception)
        {
            Se.LogError(exception, $"Could not unpack 7-Zip into \"{outputDir}\".");
            return false;
        }
    }

    /// <returns><see langword="true"/> when 7-Zip was unpacked.</returns>
    private static async Task<bool> UnpackLinux64(string outputDir)
    {
        try
        {
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            var zipUri = new Uri("avares://SubtitleEdit/Assets/SevenZip/7zrLinux64.zip");
            await using var zipStream = AssetLoader.Open(zipUri);
            var zipUnpacker = new ZipUnpacker();
            zipUnpacker.UnpackZipStream(zipStream, outputDir);
            return true;
        }
        catch (Exception exception)
        {
            Se.LogError(exception, $"Could not unpack 7-Zip into \"{outputDir}\".");
            return false;
        }
    }
}
