using Avalonia.Platform;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Initializers;

public interface  IOcrInitializer
{
    Task UpdateOcrDictionariesIfNeeded();
}

public class OcrInitializer : IOcrInitializer
{
    /// <summary>
    /// Records the hash of every file last unpacked from Ocr.zip. A file whose hash no longer
    /// matches was trained or optimized by the user and survives a version upgrade.
    /// </summary>
    private const string ShippedHashesFileName = "shipped-hashes.txt";

    public async Task UpdateOcrDictionariesIfNeeded()
    {
        if (await NeedsUpdate())
        {
            Unpack(onlyMissing: false);
            WriteNewVersionFile();
        }
        else
        {
            // Databases added in a later version (e.g. Cyrillic.nocr) still need to arrive.
            Unpack(onlyMissing: true);
        }
    }

    /// <summary>
    /// Unpacks Ocr.zip into the OCR folder. Existing files are left alone when
    /// <paramref name="onlyMissing"/> is set, or when the user changed them since they were
    /// unpacked (training, Optimize database), so an upgrade never discards that work.
    /// </summary>
    private static void Unpack(bool onlyMissing)
    {
        try
        {
            var outputDir = Se.OcrFolder;
            Directory.CreateDirectory(outputDir);
            var shippedHashes = ReadShippedHashes(outputDir);
            var changed = false;
            using var zipStream = AssetLoader.Open(new Uri("avares://SubtitleEdit/Assets/Ocr.zip"));
            using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);
            foreach (var entry in archive.Entries)
            {
                if (string.IsNullOrEmpty(entry.Name) || entry.FullName.Contains(".."))
                {
                    continue;
                }

                var fileName = Path.Combine(outputDir, entry.FullName);
                if (File.Exists(fileName) && (onlyMissing || IsUserModified(entry.FullName, fileName, shippedHashes)))
                {
                    continue;
                }

                Directory.CreateDirectory(Path.GetDirectoryName(fileName)!);
                entry.ExtractToFile(fileName, overwrite: true);
                shippedHashes[entry.FullName] = GetFileHash(fileName);
                changed = true;
            }

            if (changed)
            {
                WriteShippedHashes(outputDir, shippedHashes);
            }
        }
        catch (Exception exception)
        {
            Se.LogError(exception, "Could not unpack OCR files");
        }
    }

    private static bool IsUserModified(string entryName, string fileName, Dictionary<string, string> shippedHashes)
    {
        if (shippedHashes.TryGetValue(entryName, out var shippedHash))
        {
            return !string.Equals(GetFileHash(fileName), shippedHash, StringComparison.OrdinalIgnoreCase);
        }

        // Unpacked before hashes were recorded: a Latin.nocr well above the shipped size was trained.
        return entryName.Equals("Latin.nocr", StringComparison.OrdinalIgnoreCase) &&
               new FileInfo(fileName).Length > 1_700_000;
    }

    private static string GetFileHash(string fileName)
    {
        using var stream = File.OpenRead(fileName);
        return Convert.ToHexString(SHA256.HashData(stream));
    }

    private static Dictionary<string, string> ReadShippedHashes(string outputDir)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var fileName = Path.Combine(outputDir, ShippedHashesFileName);
        if (!File.Exists(fileName))
        {
            return result;
        }

        foreach (var line in File.ReadAllLines(fileName))
        {
            var parts = line.Split('\t');
            if (parts.Length == 2 && parts[0].Length > 0 && parts[1].Length > 0)
            {
                result[parts[0]] = parts[1];
            }
        }

        return result;
    }

    private static void WriteShippedHashes(string outputDir, Dictionary<string, string> shippedHashes)
    {
        var lines = shippedHashes.OrderBy(p => p.Key, StringComparer.OrdinalIgnoreCase).Select(p => p.Key + "\t" + p.Value);
        File.WriteAllLines(Path.Combine(outputDir, ShippedHashesFileName), lines);
    }

    private static void WriteNewVersionFile()
    {
        string outputDir = Se.OcrFolder;
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
        var outputDir = Se.OcrFolder;
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
}
