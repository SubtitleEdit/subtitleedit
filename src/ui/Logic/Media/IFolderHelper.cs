using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Media;

public interface IFolderHelper
{
    Task<string> PickFolderAsync(Window window, string title, string? suggestedStartFolder = null);
    Task OpenFolder(Window window, string folder);
    Task OpenFolderWithFileSelected(Window window, string selectedFile);
}

public class FolderHelper : IFolderHelper
{
    public async Task<string> PickFolderAsync(Window window, string title, string? suggestedStartFolder = null)
    {
        var storageProvider = window.StorageProvider;

        if (storageProvider.CanPickFolder)
        {
            var options = new FolderPickerOpenOptions
            {
                Title = title,
                AllowMultiple = false
            };

            // Open in the caller's suggested folder (e.g. the loaded subtitle's own folder)
            // instead of wherever the picker was last used. Best effort - a missing folder or
            // a provider that can't resolve paths just falls back to the picker's default.
            if (!string.IsNullOrEmpty(suggestedStartFolder) && Directory.Exists(suggestedStartFolder))
            {
                try
                {
                    var startFolder = await storageProvider.TryGetFolderFromPathAsync(suggestedStartFolder);
                    if (startFolder != null)
                    {
                        options.SuggestedStartLocation = startFolder;
                    }
                }
                catch
                {
                    // ignore - the picker falls back to its default folder
                }
            }

            var folders = await NativePickers.OpenFolderPickerAsync(window, options);

            var selected = folders.Count > 0 ? folders[0] : null;
            return selected?.Path.LocalPath ?? string.Empty; 
        }

        return string.Empty;
    }

    public async Task OpenFolder(Window window, string folder)
    {
        if (OperatingSystem.IsMacOS())
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "open",
                Arguments = $"\"{folder}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            return;
        }

        if (OperatingSystem.IsWindows())
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "explorer",
                Arguments = $"\"{folder}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            return;
        }

        var dirInfo = new DirectoryInfo(folder);
        await window.Launcher.LaunchDirectoryInfoAsync(dirInfo);
    }
    
    public async Task OpenFolderWithFileSelected(Window window, string selectedFile)
    {
        if (OperatingSystem.IsMacOS())
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "open",
                Arguments = $"-R \"{selectedFile}\"", // -R flag reveals the file in Finder
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            return;
        }

        if (OperatingSystem.IsWindows())
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"/select,\"{selectedFile}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            return;
        }

        var folder = Path.GetDirectoryName(selectedFile);
        if (folder == null)
        {
            return;
        }

        var dirInfo = new DirectoryInfo(folder);
        await window.Launcher.LaunchDirectoryInfoAsync(dirInfo);
    }
}