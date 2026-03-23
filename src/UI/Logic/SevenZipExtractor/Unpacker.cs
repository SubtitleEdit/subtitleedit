using Avalonia.Threading;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Initializers;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Readers;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace Nikse.SubtitleEdit.Logic.SevenZipExtractor;

public static class Unpacker
{
    public static void Extract7Zip(string tempFileName, string dir, string skipFolderLevel, CancellationTokenSource cancellationTokenSource, Action<string> updateProgressText)
    {
        Dispatcher.UIThread.Post(() =>
        {
            updateProgressText(Se.Language.General.Unpacking7ZipArchiveDotDotDot);
        });

        if (OperatingSystem.IsWindows())
        {
            Unpack7ZipVia77zExecutable(tempFileName, dir, skipFolderLevel, cancellationTokenSource, updateProgressText);
            return;
        }

        if (OperatingSystem.IsLinux() && RuntimeInformation.ProcessArchitecture == Architecture.X64)
        {
            Unpack7ZipVia77zExecutable(tempFileName, dir, skipFolderLevel, cancellationTokenSource, updateProgressText);
            return;
        }

        if (OperatingSystem.IsMacOS())
        {
            var macSevenZipPath = GetMacSevenZipPath();
            if (macSevenZipPath != null)
            {
                Unpack7ZipViaSystemExecutable(macSevenZipPath, tempFileName, dir, skipFolderLevel, cancellationTokenSource, updateProgressText);
                return;
            }
        }

        Extract7ZipSlow(tempFileName, dir, skipFolderLevel, cancellationTokenSource, updateProgressText);
    }

    private static string? GetMacSevenZipPath()
    {
        var paths = new[]
        {
            "/opt/homebrew/bin/7zz",
            "/usr/local/bin/7zz"
        };

        foreach (var path in paths)
        {
            if (File.Exists(path))
            {
                return path;
            }
        }

        return null;
    }

    private static void Unpack7ZipViaSystemExecutable(string sevenZipPath, string tempFileName, string dir, string skipFolderLevel, CancellationTokenSource cancellationTokenSource, Action<string> updateProgressText)
    {
        // Extract to a temporary directory if we need to skip folder levels
        var extractPath = string.IsNullOrEmpty(skipFolderLevel) ? dir : Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        try
        {
            if (!Directory.Exists(extractPath))
            {
                Directory.CreateDirectory(extractPath);
            }

            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = sevenZipPath,
                    Arguments = $"x \"{tempFileName}\" -o\"{extractPath}\" -y",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();

            while (!process.StandardOutput.EndOfStream)
            {
                if (cancellationTokenSource.IsCancellationRequested)
                {
                    try
                    {
                        process.Kill();
                    }
                    catch
                    {
                        // Ignore if already exited
                    }
                    return;
                }

                var line = process.StandardOutput.ReadLine();
                if (!string.IsNullOrWhiteSpace(line) && (line.Contains("Extracting") || line.Contains("Unpacking")))
                {
                    var displayLine = line.Length > 50 ? "..." + line.Substring(line.Length - 46) : line;
                    Dispatcher.UIThread.Post(() =>
                    {
                        updateProgressText(displayLine);
                    });
                }
            }

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                var error = process.StandardError.ReadToEnd();
                throw new Exception($"7zz extraction failed with exit code {process.ExitCode}: {error}");
            }

            // If we need to skip folder levels, move files from temp to final destination
            if (!string.IsNullOrEmpty(skipFolderLevel))
            {
                MoveFilesSkippingFolderLevel(extractPath, dir, skipFolderLevel, cancellationTokenSource, updateProgressText);
            }
        }
        finally
        {
            // Clean up temporary directory if we used one
            if (!string.IsNullOrEmpty(skipFolderLevel) && Directory.Exists(extractPath))
            {
                try
                {
                    Directory.Delete(extractPath, true);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }
    }

    private static void Unpack7ZipVia77zExecutable(string tempFileName, string dir, string skipFolderLevel, CancellationTokenSource cancellationTokenSource, Action<string> updateProgressText)
    {
        new SevenZipInitializer().UpdateSevenZipIfNeeded().Wait();


        var sevenZipPath = Path.Combine(Se.SevenZipFolder, "7zr"); // 7zr is the standalone version for 7zip archives
        if (OperatingSystem.IsWindows())
        {
            sevenZipPath = Path.Combine(Se.SevenZipFolder, "7za.exe"); // 7za.exe is the windows executable for 7zip archives
        }

        if (!File.Exists(sevenZipPath))
        {
            throw new FileNotFoundException($"7-zip executable not found at {sevenZipPath}");
        }

        // Make sure 7zr is executable on Linux
        if (OperatingSystem.IsLinux())
        {
            LinuxHelper.MakeExecutable(sevenZipPath);
        }

        // Extract to a temporary directory if we need to skip folder levels
        var extractPath = string.IsNullOrEmpty(skipFolderLevel) ? dir : Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        try
        {
            if (!Directory.Exists(extractPath))
            {
                Directory.CreateDirectory(extractPath);
            }

            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = sevenZipPath,
                    Arguments = $"x \"{tempFileName}\" -o\"{extractPath}\" -y",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();

            while (!process.StandardOutput.EndOfStream)
            {
                if (cancellationTokenSource.IsCancellationRequested)
                {
                    try
                    {
                        process.Kill();
                    }
                    catch
                    {
                        // Ignore if already exited
                    }
                    return;
                }

                var line = process.StandardOutput.ReadLine();
                if (!string.IsNullOrWhiteSpace(line) && line.Contains("Extracting"))
                {
                    var displayLine = line.Length > 50 ? "..." + line.Substring(line.Length - 46) : line;
                    Dispatcher.UIThread.Post(() =>
                    {
                        updateProgressText(displayLine);
                    });
                }
            }

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                var error = process.StandardError.ReadToEnd();
                throw new Exception($"7zip extraction failed with exit code {process.ExitCode}: {error}");
            }

            // If we need to skip folder levels, move files from temp to final destination
            if (!string.IsNullOrEmpty(skipFolderLevel))
            {
                MoveFilesSkippingFolderLevel(extractPath, dir, skipFolderLevel, cancellationTokenSource, updateProgressText);
            }
        }
        finally
        {
            // Clean up temporary directory if we used one
            if (!string.IsNullOrEmpty(skipFolderLevel) && Directory.Exists(extractPath))
            {
                try
                {
                    Directory.Delete(extractPath, true);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }
    }

    private static void MoveFilesSkippingFolderLevel(string sourceDir, string targetDir, string skipFolderLevel, CancellationTokenSource cancellationTokenSource, Action<string> updateProgressText)
    {
        var skipPath = Path.Combine(sourceDir, skipFolderLevel.Replace('/', Path.DirectorySeparatorChar));

        if (!Directory.Exists(skipPath))
        {
            // If the skip path doesn't exist, just move everything
            skipPath = sourceDir;
        }

        MoveDirectoryContents(skipPath, targetDir, cancellationTokenSource, updateProgressText);
    }

    private static void MoveDirectoryContents(string sourceDir, string targetDir, CancellationTokenSource cancellationTokenSource, Action<string> updateProgressText)
    {
        if (!Directory.Exists(targetDir))
        {
            Directory.CreateDirectory(targetDir);
        }

        // Move all files
        foreach (var file in Directory.GetFiles(sourceDir))
        {
            if (cancellationTokenSource.IsCancellationRequested)
            {
                return;
            }

            var fileName = Path.GetFileName(file);
            var destFile = Path.Combine(targetDir, fileName);

            var displayName = fileName;
            if (displayName.Length > 30)
            {
                displayName = "..." + displayName.Substring(displayName.Length - 26).Trim();
            }

            Dispatcher.UIThread.Post(() =>
            {
                updateProgressText(string.Format(Se.Language.General.UnpackingX, displayName));
            });

            File.Move(file, destFile, true);
        }

        // Move all subdirectories recursively
        foreach (var subDir in Directory.GetDirectories(sourceDir))
        {
            if (cancellationTokenSource.IsCancellationRequested)
            {
                return;
            }

            var dirName = Path.GetFileName(subDir);
            var destSubDir = Path.Combine(targetDir, dirName);
            MoveDirectoryContents(subDir, destSubDir, cancellationTokenSource, updateProgressText);
        }
    }

    public static void Extract7ZipSlow(string tempFileName, string dir, string skipFolderLevel, CancellationTokenSource cancellationTokenSource, Action<string> updateProgressText)
    {
        using Stream stream = File.OpenRead(tempFileName);
        using var archive = SevenZipArchive.OpenArchive(stream);
        double totalSize = archive.TotalUncompressedSize;
        double unpackedSize = 0;

        var reader = archive.ExtractAllEntries();
        while (reader.MoveToNextEntry())
        {
            if (cancellationTokenSource.IsCancellationRequested)
            {
                return;
            }

            if (!string.IsNullOrEmpty(reader.Entry.Key))
            {
                var entryFullName = reader.Entry.Key;
                if (!string.IsNullOrEmpty(skipFolderLevel) && entryFullName.StartsWith(skipFolderLevel))
                {
                    entryFullName = entryFullName[skipFolderLevel.Length..];
                }

                entryFullName = entryFullName.Replace('/', Path.DirectorySeparatorChar);
                entryFullName = entryFullName.TrimStart(Path.DirectorySeparatorChar);

                var fullFileName = Path.Combine(dir, entryFullName);

                if (reader.Entry.IsDirectory)
                {
                    if (!Directory.Exists(fullFileName))
                    {
                        Directory.CreateDirectory(fullFileName);
                    }

                    continue;
                }

                var fullPath = Path.GetDirectoryName(fullFileName);
                if (fullPath == null)
                {
                    continue;
                }

                var displayName = entryFullName;
                if (displayName.Length > 30)
                {
                    displayName = "..." + displayName.Remove(0, displayName.Length - 26).Trim();
                }
                Dispatcher.UIThread.Post(() =>
                {
                    updateProgressText(string.Format(Se.Language.General.UnpackingX, displayName));
                });

                reader.WriteEntryToDirectory(fullPath);
                unpackedSize += reader.Entry.Size;
            }
        }
    }
}
