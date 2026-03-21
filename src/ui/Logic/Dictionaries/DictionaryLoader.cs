using System.IO;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.Logic.Compression;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Logic.Dictionaries;

public static class DictionaryLoader
{
    public static async Task UnpackIfNotFound()
    {
        if (!Directory.Exists(Se.DictionariesFolder))
        {
            Directory.CreateDirectory(Se.DictionariesFolder);
            await UnpackDictionaries();
        }
    }

    private static async Task UnpackDictionaries()
    {
        var zipFilePath = Path.Combine(Se.DataFolder, "Assets",  "Dictionaries.zip");
        await using var stream = new FileStream(zipFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);
    
        var zipUnpacker = new ZipUnpacker();
        zipUnpacker.UnpackZipStream(stream, Se.DictionariesFolder);
    }
}

