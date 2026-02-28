using Microsoft.Win32;
using System;
using System.IO;

namespace Nikse.SubtitleEdit.Logic.Platform.Windows;

internal static class FileTypeAssociationsHelper
{
    [System.Runtime.InteropServices.DllImport("Shell32.dll")]
    private static extern int SHChangeNotify(int eventId, int flags, nint item1, nint item2);

    internal static bool GetChecked(string ext, string appName)
    {
#pragma warning disable CA1416 // Validate platform compatibility
        using (var key = Registry.CurrentUser.OpenSubKey($"Software\\Classes\\{appName}{ext}"))
        {
            if (key == null)
            {
                return false;
            }

            var defaultIcon = key.OpenSubKey("DefaultIcon");

            var iconFileNameObject = defaultIcon?.GetValue("");
            if (!(iconFileNameObject is string iconFileName) || !File.Exists(iconFileName))
            {
                return false;
            }

            var cmd = key.OpenSubKey("shell\\open\\command");
            if (cmd == null)
            {
                return false;
            }

            var cmdObject = cmd.GetValue("");
            if (cmdObject is string cmdString)
            {
                var ix = cmdString.IndexOf("SubtitleEdit.exe", StringComparison.Ordinal);
                if (ix >= 0)
                {
                    var exeFileName = cmdString.Substring(0, ix + "SubtitleEdit.exe".Length).Trim('"');
                    return File.Exists(exeFileName);
                }
            }
        }
#pragma warning restore CA1416 // Validate platform compatibility

        return false;
    }

    internal static void SetFileAssociationViaRegistry(string ext, string exeFileName, string iconFileName, string appName)
    {
#pragma warning disable CA1416 // Validate platform compatibility
        Registry.SetValue($"HKEY_CURRENT_USER\\Software\\Classes\\{appName}{ext}", "", $"{GetFriendlyName(ext)} subtitle file");
        Registry.SetValue($"HKEY_CURRENT_USER\\Software\\Classes\\{appName}{ext}\\DefaultIcon", "", iconFileName);
        Registry.SetValue($"HKEY_CURRENT_USER\\Software\\Classes\\{appName}{ext}\\shell\\open\\command", "", $"\"{exeFileName.Trim('"')}\" \"%1\"");
        Registry.SetValue($"HKEY_CURRENT_USER\\Software\\Classes\\{ext}", "", $"{appName}{ext}");
#pragma warning restore CA1416 // Validate platform compatibility
    }

    private static string GetFriendlyName(string ext)
    {
        if (ext.Equals(".srt", StringComparison.OrdinalIgnoreCase))
        {
            return "SubRip";
        }

        if (ext.Equals(".ass", StringComparison.OrdinalIgnoreCase))
        {
            return "Advanced Sub Station Alpha";
        }

        if (ext.Equals(".dfxp", StringComparison.OrdinalIgnoreCase))
        {
            return "Distribution Format Exchange Profile";
        }

        if (ext.Equals(".ssa", StringComparison.OrdinalIgnoreCase))
        {
            return "Sub Station Alpha";
        }

        if (ext.Equals(".sup", StringComparison.OrdinalIgnoreCase))
        {
            return "Blu-ray PGS";
        }

        if (ext.Equals(".vtt", StringComparison.OrdinalIgnoreCase))
        {
            return "Web Video Text Tracks (WebVTT)";
        }

        if (ext.Equals(".smi", StringComparison.OrdinalIgnoreCase))
        {
            return "SAMI";
        }

        if (ext.Equals(".itt", StringComparison.OrdinalIgnoreCase))
        {
            return "iTunes Timed Text";
        }

        return $"{ext.TrimStart('.')}";
    }

    internal static void DeleteFileAssociationViaRegistry(string ext, string appName)
    {
#pragma warning disable CA1416 // Validate platform compatibility
        try
        {
            var appExtensionRegKey = $"{appName}{ext}";
            using (var registryKey = Registry.CurrentUser.OpenSubKey(@"Software\Classes\", true))
            {
                if (registryKey?.OpenSubKey(appExtensionRegKey) != null)
                {
                    registryKey.DeleteSubKeyTree($"{appName}{ext}");
                }
            }

            // (Default)
            const string defaultRegValueName = "";
            using (var registryKey = Registry.CurrentUser.OpenSubKey("Software\\Classes\\" + ext, true))
            {
                if (registryKey == null)
                {
                    return;
                }   

                var registryValue = registryKey.GetValue(defaultRegValueName);
                if (registryValue == null)
                {
                    return;
                }

                if (appExtensionRegKey.Equals((string)registryValue, StringComparison.Ordinal))
                {
                    registryKey.DeleteValue(defaultRegValueName);
                }
            }
        }
        catch (UnauthorizedAccessException)
        {
            throw;
        }
        catch (System.Security.SecurityException)
        {
            throw;
        }
#pragma warning restore CA1416 // Validate platform compatibility
    }

    internal static void Refresh()
    {
        //this call notifies Windows that it needs to redo the file associations and icons
        SHChangeNotify(0x08000000, 0x2000, nint.Zero, nint.Zero);
    }
}