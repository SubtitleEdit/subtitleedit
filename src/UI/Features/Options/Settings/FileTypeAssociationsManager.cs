using Avalonia.Controls;
using Avalonia.Platform;
using Microsoft.Win32;
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
            item.IsAssociated = FileTypeAssociationsHelper.GetChecked(item.Extension, "SubtitleEdit5");
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
                item.IsAssociated != FileTypeAssociationsHelper.GetChecked(item.Extension, "SubtitleEdit5"));

            if (!hasChanges)
            {
                return;
            }

            // For Windows 10+, use the modern approach
            if (Environment.OSVersion.Version.Major >= 10)
            {
                await SaveFileTypeAssociationsModernAsync(fileTypeAssociations, window);
            }
            else
            {
                // Legacy registry approach for older Windows versions
                await SaveFileTypeAssociationsLegacyAsync(fileTypeAssociations, exeFileName);
            }
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

    private static async Task SaveFileTypeAssociationsModernAsync(
        IEnumerable<FileTypeAssociationViewModel> fileTypeAssociations,
        Window? window)
    {
        // On Windows 10+, register the app but let user choose associations via Settings
        var exeFileName = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
        if (string.IsNullOrEmpty(exeFileName))
        {
            return;
        }

        var needsAssociation = fileTypeAssociations.Where(item => item.IsAssociated).ToList();

        if (needsAssociation.Count == 0)
        {
            return;
        }

        // Register the application capabilities (doesn't require admin)
        RegisterApplicationCapabilities(exeFileName, needsAssociation);

        // Prompt user to open Settings
        if (window != null && !window.IsClosing())
        {
            var result = await MessageBox.Show(
                window,
                "File Associations",
                $"To set file associations, please open Windows Settings.\n\n" +
                $"Would you like to open the Default Apps settings now?",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == MessageBoxResult.Yes)
            {
                OpenWindowsDefaultAppsSettings();
            }
        }
    }
    private static void RegisterApplicationCapabilities(
        string exeFileName,
        List<FileTypeAssociationViewModel> associations)
    {
        try
        {
            // Register under HKEY_CURRENT_USER (doesn't require admin)
#pragma warning disable CA1416 // Validate platform compatibility
            using var capabilitiesKey = Registry.CurrentUser.CreateSubKey(
                @"Software\SubtitleEdit5\Capabilities");

            if (capabilitiesKey != null)
            {
                capabilitiesKey.SetValue("ApplicationName", "Subtitle Edit");
                capabilitiesKey.SetValue("ApplicationDescription", "Subtitle editor");

                using var fileAssocsKey = capabilitiesKey.CreateSubKey("FileAssociations");
                if (fileAssocsKey != null)
                {
                    foreach (var item in associations)
                    {
                        fileAssocsKey.SetValue(item.Extension, $"SubtitleEdit5{item.Extension}");
                    }
                }
            }

            // Register the ProgId under HKEY_CURRENT_USER
            foreach (var item in associations)
            {
                var progId = $"SubtitleEdit5{item.Extension}";
                using var progIdKey = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{progId}");
                if (progIdKey != null)
                {
                    progIdKey.SetValue("", $"Subtitle Edit {item.Extension.ToUpper()} File");

                    using var iconKey = progIdKey.CreateSubKey("DefaultIcon");
                    if (iconKey != null)
                    {
                        var iconPath = GetIconPath(item);
                        if (!string.IsNullOrEmpty(iconPath))
                        {
                            iconKey.SetValue("", iconPath);
                        }
                    }

                    using var commandKey = progIdKey.CreateSubKey(@"shell\open\command");
                    if (commandKey != null)
                    {
                        commandKey.SetValue("", $"\"{exeFileName}\" \"%1\"");
                    }
                }
            }

#pragma warning restore CA1416 // Validate platform compatibility
        }
        catch (Exception ex)
        {
            Se.LogError(ex, "RegisterApplicationCapabilities");
            throw;
        }
    }

    private static void OpenWindowsDefaultAppsSettings()
    {
        try
        {
            // Open Default Apps settings in Windows 10/11
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "ms-settings:defaultapps",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            Se.LogError(ex, "OpenWindowsDefaultAppsSettings");
        }
    }

    private static async Task SaveFileTypeAssociationsLegacyAsync(
        IEnumerable<FileTypeAssociationViewModel> fileTypeAssociations,
        string exeFileName)
    {
        // Original registry-based approach for older Windows versions
        foreach (var item in fileTypeAssociations)
        {
            var ext = item.Extension;
            if (item.IsAssociated)
            {
                var iconFileName = GetIconPath(item);
                FileTypeAssociationsHelper.SetFileAssociationViaRegistry(ext, exeFileName, iconFileName, "SubtitleEdit5");
            }
            else
            {
                FileTypeAssociationsHelper.DeleteFileAssociationViaRegistry(ext, "SubtitleEdit5");
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
            OpenWindowsDefaultAppsSettings();
        }
    }
}
