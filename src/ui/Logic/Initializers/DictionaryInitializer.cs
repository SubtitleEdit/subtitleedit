using Avalonia.Platform;
using Nikse.SubtitleEdit.Logic.Compression;
using Nikse.SubtitleEdit.Core.Dictionaries;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Initializers;

public interface IDictionaryInitializer
{
    Task UpdateDictionariesIfNeeded();
}

public class DictionaryInitializer(IZipUnpacker zipUnpacker) : IDictionaryInitializer
{
    public async Task UpdateDictionariesIfNeeded()
    {
        if (await NeedsUpdate())
        {
            var previousNoBreakAfterLists = SnapshotNoBreakAfterListsForMigration(Se.DictionariesFolder);
            await Unpack();
            MigrateNoBreakAfterListEdits(Se.DictionariesFolder, previousNoBreakAfterLists);
            WriteNewVersionFile();
        }
    }

    /// <summary>
    /// Marks a dictionaries folder whose do-not-break-after edits already live in the
    /// *_NoBreakAfterList_User.xml overlay files, so the one-time migration below runs once.
    /// </summary>
    internal const string NoBreakAfterMigrationMarkerFileName = "NoBreakAfterList_User.migrated";

    /// <summary>
    /// Before #16375 the do-not-break-after editor wrote into the shipped *_NoBreakAfterList.xml
    /// files, which the unpack overwrites. Reads those lists before the unpack, once per folder.
    /// </summary>
    internal static Dictionary<string, List<NoBreakAfterListFile.Entry>> SnapshotNoBreakAfterListsForMigration(string folder)
    {
        var result = new Dictionary<string, List<NoBreakAfterListFile.Entry>>(StringComparer.OrdinalIgnoreCase);
        try
        {
            if (!Directory.Exists(folder) || File.Exists(Path.Combine(folder, NoBreakAfterMigrationMarkerFileName)))
            {
                return result;
            }

            foreach (var fileName in Directory.GetFiles(folder, "*" + NoBreakAfterListFile.FileNameSuffix))
            {
                try
                {
                    result[Path.GetFileName(fileName)] = NoBreakAfterListFile.LoadItems(fileName);
                }
                catch
                {
                    // an unreadable list has no edits to keep
                }
            }
        }
        catch (Exception exception)
        {
            Se.LogError(exception, "Could not read the do-not-break-after lists before updating dictionaries");
        }

        return result;
    }

    /// <summary>
    /// After the unpack, keeps items the user had added to a shipped list as overlay additions.
    /// </summary>
    internal static void MigrateNoBreakAfterListEdits(string folder, Dictionary<string, List<NoBreakAfterListFile.Entry>> previousLists)
    {
        try
        {
            if (!Directory.Exists(folder))
            {
                return;
            }

            foreach (var pair in previousLists)
            {
                try
                {
                    NoBreakAfterListFile.PreserveAddedItems(Path.Combine(folder, pair.Key), pair.Value);
                }
                catch (Exception exception)
                {
                    Se.LogError(exception, $"Could not keep do-not-break-after edits for \"{pair.Key}\"");
                }
            }

            File.WriteAllText(Path.Combine(folder, NoBreakAfterMigrationMarkerFileName), Se.Version);
        }
        catch (Exception exception)
        {
            Se.LogError(exception, "Could not migrate the do-not-break-after lists");
        }
    }

    private static void WriteNewVersionFile()
    {
        string outputDir = Se.DictionariesFolder;
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
        string outputDir = Se.DictionariesFolder;
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
        var zipUri = new Uri("avares://SubtitleEdit/Assets/Dictionaries.zip");
        await using var zipStream = AssetLoader.Open(zipUri);
        zipUnpacker.UnpackZipStream(zipStream, Se.DictionariesFolder);
    }
}
