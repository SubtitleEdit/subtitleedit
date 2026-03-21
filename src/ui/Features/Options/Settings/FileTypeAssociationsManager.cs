using Avalonia.Controls;
using Avalonia.Platform;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Platform.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

public static class FileTypeAssociationsManager
{
    public static void LoadFileTypeAssociations(IEnumerable<FileTypeAssociationViewModel> fileTypeAssociations)
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var folder = Path.Combine(Se.DataFolder, "FileTypes");
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        foreach (var item in fileTypeAssociations)
        {
            item.IsAssociated = FileTypeAssociationsHelper.IsDefault(item.Extension, "SubtitleEdit5");
        }
    }

    public static async Task SaveFileTypeAssociationsAsync(
        IEnumerable<FileTypeAssociationViewModel> fileTypeAssociations,
        Window? window)
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var exeFileName = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
        if (string.IsNullOrEmpty(exeFileName) || !File.Exists(exeFileName))
        {
            return;
        }

        try
        {
            var hasChanges = fileTypeAssociations.Any(item =>
                item.IsAssociated != FileTypeAssociationsHelper.IsDefault(item.Extension, "SubtitleEdit5"));

            if (!hasChanges)
            {
                return;
            }

            await SaveFileTypeAssociationsViaRegistryAsync(fileTypeAssociations, exeFileName);
        }
        catch (UnauthorizedAccessException ex)
        {
            Se.LogError(ex, "SaveFileTypeAssociations - Unauthorized access");
            await ShowFileAssociationErrorAsync(
                window,
                "Unable to modify file associations. Administrator privileges are required.\n\n" +
                "Would you like to open Windows Settings to manually set file associations?",
                openSettings: true);
        }
        catch (System.Security.SecurityException ex)
        {
            Se.LogError(ex, "SaveFileTypeAssociations - Security exception");
            await ShowFileAssociationErrorAsync(
                window,
                "Unable to modify file associations due to security restrictions.\n\n" +
                "Would you like to open Windows Settings to manually set file associations?",
                openSettings: true);
        }
        catch (Exception ex)
        {
            Se.LogError(ex, "SaveFileTypeAssociations - Unexpected error");
            await ShowFileAssociationErrorAsync(
                window,
                $"An unexpected error occurred: {ex.Message}",
                openSettings: false);
        }
    }

    private static async Task SaveFileTypeAssociationsViaRegistryAsync(
        IEnumerable<FileTypeAssociationViewModel> fileTypeAssociations,
        string exeFileName)
    {
        foreach (var item in fileTypeAssociations)
        {
            var ext = item.Extension;
            if (item.IsAssociated)
            {
                var iconFileName = GetIconPath(item);
                FileTypeAssociationsHelper.SetFileAssociation(ext, exeFileName, "SubtitleEdit5");
            }
            else
            {
                FileTypeAssociationsHelper.DeleteFileAssociation(ext, "SubtitleEdit5");
            }
        }

        FileTypeAssociationsHelper.Refresh();
        await Task.CompletedTask;
    }

    private static string GetIconPath(FileTypeAssociationViewModel item)
    {
        var folder = Path.Combine(Se.DataFolder, "FileTypes");
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        var iconFileName = Path.Combine(folder, item.Extension.TrimStart('.') + ".ico");

        if (!File.Exists(iconFileName))
        {
            try
            {
                var uri = new Uri(item.IconPath);
                using var stream = AssetLoader.Open(uri);
                if (stream != null && item.IconPath.EndsWith(".ico", StringComparison.OrdinalIgnoreCase))
                {
                    using var fileStream = new FileStream(iconFileName, FileMode.Create, FileAccess.Write);
                    stream.CopyTo(fileStream);
                }
            }
            catch (Exception ex)
            {
                Se.LogError(ex, "GetIconPath");
            }
        }

        return File.Exists(iconFileName) ? iconFileName : string.Empty;
    }

    private static async Task ShowFileAssociationErrorAsync(
        Window? window,
        string message,
        bool openSettings)
    {
        if (window == null || window.IsClosing())
        {
            return;
        }

        var buttons = openSettings ? MessageBoxButtons.YesNo : MessageBoxButtons.OK;
        var result = await MessageBox.Show(
            window,
            "File Association Error",
            message,
            buttons,
            MessageBoxIcon.Warning);

        if (result == MessageBoxResult.Yes && openSettings)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "ms-settings:defaultapps",
                    UseShellExecute = true,
                });
            }
            catch (Exception ex)
            {
                Se.LogError(ex, "ShowFileAssociationErrorAsync - OpenWindowsDefaultAppsSettings");
            }
        }
    }
}
