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

        // leaveOpen: true so callers retain control of the underlying stream — they typically need
        // to read it again afterwards (e.g. to hash the archive for an .installed.sha256 sidecar).
        using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read, leaveOpen: true);

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

            // Keep every entry inside the extraction folder, the way the 7-zip unpacker does.
            // This unpacker handles third-party plugin zips, and combining an unchecked entry
            // name let an archive write outside the target directory.
            var targetRoot = Path.GetFullPath(outputPath);
            var filePath = Path.GetFullPath(Path.Combine(targetRoot, entryFullName));
            var relativePath = Path.GetRelativePath(targetRoot, filePath);
            if (Path.IsPathRooted(entryFullName) ||
                relativePath.Equals("..", StringComparison.Ordinal) ||
                relativePath.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal) ||
                Path.IsPathRooted(relativePath))
            {
                throw new InvalidDataException("Archive entry is outside the extraction folder: " + entryFullName);
            }
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
            var isSymbolicLink = entry.EntryType is TarEntryType.SymbolicLink or TarEntryType.HardLink;
            if (entry.EntryType != TarEntryType.RegularFile && !isSymbolicLink)
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

            // Keep every entry inside the extraction folder, the way the 7-zip unpacker does.
            // This unpacker handles third-party plugin zips, and combining an unchecked entry
            // name let an archive write outside the target directory.
            var targetRoot = Path.GetFullPath(outputPath);
            var filePath = Path.GetFullPath(Path.Combine(targetRoot, entryFullName));
            var relativePath = Path.GetRelativePath(targetRoot, filePath);
            if (Path.IsPathRooted(entryFullName) ||
                relativePath.Equals("..", StringComparison.Ordinal) ||
                relativePath.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal) ||
                Path.IsPathRooted(relativePath))
            {
                throw new InvalidDataException("Archive entry is outside the extraction folder: " + entryFullName);
            }
            var directoryPath = Path.GetDirectoryName(filePath);

            if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            if (string.IsNullOrEmpty(Path.GetFileName(entryFullName)))
            {
                continue;
            }

            if (allowedExtensions.Count > 0)
            {
                var extension = Path.GetExtension(entryFullName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                {
                    continue;
                }
            }

            if (isSymbolicLink)
            {
                // Versioned native libraries (e.g. macOS/Linux llama.cpp builds) ship as symlink
                // chains; recreate them so loader @rpath references keep resolving.
                var linkTarget = entry.LinkName;
                if (allToOutputPath)
                {
                    linkTarget = Path.GetFileName(linkTarget);
                }

                if (string.IsNullOrEmpty(linkTarget))
                {
                    continue;
                }

                // The containment check above validates where the LINK is written, never where it
                // points. An archive shipping "lib -> /etc" (or "../..") followed by a regular
                // "lib/x" entry passes that check and then writes outside the extraction folder,
                // and the copy fallback below would pull an arbitrary host file in. Reject an
                // absolute target, and require a relative one to resolve inside the root.
                if (Path.IsPathRooted(linkTarget))
                {
                    continue;
                }

                var resolvedLinkTarget = Path.GetFullPath(Path.Combine(directoryPath ?? targetRoot, linkTarget));
                var linkTargetRelative = Path.GetRelativePath(targetRoot, resolvedLinkTarget);
                if (linkTargetRelative.Equals("..", StringComparison.Ordinal) ||
                    linkTargetRelative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal) ||
                    Path.IsPathRooted(linkTargetRelative))
                {
                    continue;
                }

                try
                {
                    if (File.Exists(filePath) || Directory.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }

                    File.CreateSymbolicLink(filePath, linkTarget);
                }
                catch
                {
                    // Symlinks may be unsupported (e.g. Windows without privileges) - fall back to
                    // copying the already-extracted target file when it is available.
                    if (File.Exists(resolvedLinkTarget) && !File.Exists(filePath))
                    {
                        File.Copy(resolvedLinkTarget, filePath);
                    }
                    else
                    {
                        continue;
                    }
                }
            }
            else
            {
                entry.ExtractToFile(filePath, overwrite: true);
            }

            outputFileNames?.Add(filePath);
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