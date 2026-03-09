using Microsoft.Win32;
using System;
using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.Logic.Platform.Windows;

internal static class FileTypeAssociationsHelper
{
    // Shell notification constants
    private const int SHCNE_ASSOCCHANGED = 0x08000000;
    private const uint SHCNF_IDLIST = 0x0000;
    private const uint SHCNF_FLUSH = 0x1000;

    [DllImport("Shell32.dll", SetLastError = true)]
    private static extern void SHChangeNotify(int eventId, uint flags, nint item1, nint item2);

    /// <summary>
    /// Checks if the app is currently the default handler for a specific extension.
    /// It checks both the ProgID and the "UserChoice" (the Windows 10+ standard).
    /// </summary>
    internal static bool IsDefault(string ext, string appName)
    {
#pragma warning disable CA1416
        try
        {
            string progId = $"{appName}{ext}";

            // 1. Check Modern "UserChoice" (Windows 10/11)
            // This key is read-only for apps, but we can read it to see who won the 'war'.
            using (var userChoice = Registry.CurrentUser.OpenSubKey($@"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\{ext}\UserChoice"))
            {
                var progIdValue = userChoice?.GetValue("Progid") as string;
                if (progIdValue == progId) return true;
            }

            // 2. Fallback: Check the old-school Software\Classes
            using (var key = Registry.CurrentUser.OpenSubKey($@"Software\Classes\{ext}"))
            {
                var val = key?.GetValue("") as string;
                return val == progId;
            }
        }
        catch
        {
            return false;
        }
#pragma warning restore CA1416
    }

    /// <summary>
    /// Registers the file association using both the legacy ProgID method 
    /// and the modern OpenWithProgids fallback.
    /// </summary>
    internal static void SetFileAssociation(string ext, string exePath, string appName)
    {
#pragma warning disable CA1416
        string progId = $"{appName}{ext}";
        string friendlyName = $"{GetFriendlyName(ext)} Subtitle File";

        // --- STEP 1: Old-School ProgID Registration ---
        // This defines WHAT your app is (Icon, Command, Name)
        using (var rootKey = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{progId}"))
        {
            rootKey.SetValue("", friendlyName);

            using (var iconKey = rootKey.CreateSubKey("DefaultIcon"))
            {
                iconKey.SetValue("", $"\"{exePath}\",0");
            }

            using (var cmdKey = rootKey.CreateSubKey(@"shell\open\command"))
            {
                cmdKey.SetValue("", $"\"{exePath}\" \"%1\"");
            }
        }

        // --- STEP 2: Extension Mapping ---
        using (var extKey = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{ext}"))
        {
            // Set as default (Old school - works on Win7 and fresh Win10 installs)
            extKey.SetValue("", progId);

            // --- STEP 3: Modern Fallback (OpenWithProgids) ---
            // This tells Windows 10/11: "I am capable of opening this." 
            // Even if Windows denies the "Default" status, you'll be in the 'Open With' menu.
            using (var openWith = extKey.CreateSubKey("OpenWithProgids"))
            {
                openWith.SetValue(progId, Array.Empty<byte>(), RegistryValueKind.Binary);
            }
        }

        Refresh();
#pragma warning restore CA1416
    }

    /// <summary>
    /// Cleans up registry entries for the specific app/extension pair.
    /// </summary>
    internal static void DeleteFileAssociation(string ext, string appName)
    {
#pragma warning disable CA1416
        string progId = $"{appName}{ext}";

        // Delete the ProgID definition
        Registry.CurrentUser.DeleteSubKeyTree($@"Software\Classes\{progId}", false);

        // Remove from the extension mapping
        using (var extKey = Registry.CurrentUser.OpenSubKey($@"Software\Classes\{ext}", true))
        {
            if (extKey != null)
            {
                if (extKey.GetValue("")?.ToString() == progId)
                    extKey.DeleteValue("");

                using (var openWith = extKey.OpenSubKey("OpenWithProgids", true))
                {
                    openWith?.DeleteValue(progId, false);
                }
            }
        }

        Refresh();
#pragma warning restore CA1416
    }

    private static string GetFriendlyName(string ext) => ext.ToLowerInvariant() switch
    {
        ".srt" => "SubRip",
        ".ass" => "Advanced Sub Station Alpha",
        ".ssa" => "Sub Station Alpha",
        ".vtt" => "WebVTT",
        ".sup" => "Blu-ray PGS",
        _ => ext.TrimStart('.').ToUpper()
    };

    internal static void Refresh()
    {
        // Flush the shell cache so icons change immediately
        SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_FLUSH | SHCNF_IDLIST, nint.Zero, nint.Zero);
    }
}