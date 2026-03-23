using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Nikse.SubtitleEdit.Logic.Compression;

public class ZipUnpacker : IZipUnpacker
{
    public void UnpackZipStream(Stream zipStream, string outputPath)
    {
        UnpackZipStream(zipStream, outputPath, string.Empty, false, new List<string>(), null);
    }

    public void UnpackZipStream(
    Stream zipStream,
    string outputPath,
    string skipFolderLevel,
    bool allToOutputPath,
    List<string> allowedExtensions,
    List<string>? outputFileNames)
    {
        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }

        // detect if stream is ".tar.gz" header
        if (zipStream.CanSeek)
        {
            var header = new byte[2];
            zipStream.ReadExactly(header, 0, 2);
            zipStream.Seek(0, SeekOrigin.Begin);
            if (header[0] == 0x1F && header[1] == 0x8B)
            {
                UnpackTarGzStream(zipStream, outputPath, skipFolderLevel, allToOutputPath, allowedExtensions, outputFileNames);
                return;
            }            
        }

        allowedExtensions = allowedExtensions.Select(x => x.ToLowerInvariant()).ToList();

        using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);

        foreach (var entry in archive.Entries)
        {
            var entryFullName = entry.FullName;
            if (!string.IsNullOrEmpty(skipFolderLevel) && entryFullName.StartsWith(skipFolderLevel))
            {
                entryFullName = entryFullName[skipFolderLevel.Length..].Trim('/');
            }

            if (allToOutputPath)
            {
                entryFullName = Path.GetFileName(entryFullName);
            }

            var filePath = Path.Combine(outputPath, entryFullName);
            var directoryPath = Path.GetDirectoryName(filePath);

            if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            if (!string.IsNullOrEmpty(entry.Name))
            {

                if (allowedExtensions.Count > 0)
                {
                    var extension = Path.GetExtension(entry.Name).ToLowerInvariant();
                    if (!allowedExtensions.Contains(extension))
                    {
                        continue;
                    }
                }

                using var entryStream = entry.Open();
                using var fileStream = File.Create(filePath);
                entryStream.CopyTo(fileStream);
                outputFileNames?.Add(filePath);
            }
        }
    }

    private static void UnpackTarGzStream(Stream zipStream, string outputPath, string skipFolderLevel, bool allToOutputPath, List<string> allowedExtensions, List<string>? outputFileNames)
    {
        allowedExtensions = allowedExtensions.Select(x => x.ToLowerInvariant()).ToList();

        using var gzipStream = new GZipStream(zipStream, CompressionMode.Decompress);
        using var tarReader = new TarReader(gzipStream);

        TarEntry? entry;
        while ((entry = tarReader.GetNextEntry()) != null)
        {
            if (entry.EntryType != TarEntryType.RegularFile)
            {
                continue;
            }

            var entryFullName = entry.Name;
            if (!string.IsNullOrEmpty(skipFolderLevel) && entryFullName.StartsWith(skipFolderLevel))
            {
                entryFullName = entryFullName[skipFolderLevel.Length..].Trim('/');
            }

            if (allToOutputPath)
            {
                entryFullName = Path.GetFileName(entryFullName);
            }

            var filePath = Path.Combine(outputPath, entryFullName);
            var directoryPath = Path.GetDirectoryName(filePath);

            if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            if (!string.IsNullOrEmpty(Path.GetFileName(entryFullName)))
            {
                if (allowedExtensions.Count > 0)
                {
                    var extension = Path.GetExtension(entryFullName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(extension))
                    {
                        continue;
                    }
                }

                entry.ExtractToFile(filePath, overwrite: true);
                outputFileNames?.Add(filePath);
            }
        }
    }

    public string? UnpackTextFileFromZipStream(Stream zipStream, string fileName)
    {
        using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);

        foreach (var entry in archive.Entries)
        {
            if (entry.FullName != fileName)
            {
                continue;
            }

            using var entryStream = entry.Open();
            using var outStream = new MemoryStream();
            entryStream.CopyTo(outStream);

            return System.Text.Encoding.UTF8.GetString(outStream.ToArray());
        }

        return null;
    }
}