using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Media;

public interface IFolderHelper
{
    Task<string> PickFolderAsync(Window window, string title);
    Task OpenFolder(Window window, string folder);
    Task OpenFolderWithFileSelected(Window window, string selectedFile);
}

public class FolderHelper : IFolderHelper
{
    public async Task<string> PickFolderAsync(Window window, string title)
    {
        var storageProvider = window.StorageProvider;

        if (storageProvider.CanPickFolder)
        {
            var folders = await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = title,
                AllowMultiple = false
            });

            var selected = folders.Count > 0 ? folders[0] : null;
            return selected?.Path.LocalPath ?? string.Empty; 
        }

        return string.Empty;
    }

    public async Task OpenFolder(Window window, string folder)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
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

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
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
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
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

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = @"/select, " + selectedFile,
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