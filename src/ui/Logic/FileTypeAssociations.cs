using Microsoft.Win32;
using System;
using System.IO;

namespace Nikse.SubtitleEdit.Logic
{
    internal static class FileTypeAssociations
    {
        [System.Runtime.InteropServices.DllImport("Shell32.dll")]
        private static extern int SHChangeNotify(int eventId, int flags, IntPtr item1, IntPtr item2);

        internal static bool GetChecked(string ext, string appName)
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey($"Software\\Classes\\{appName}{ext}"))
            {
                if (key == null)
                {
                    return false;
                }

                var defaultIcon = key.OpenSubKey("DefaultIcon");
                if (defaultIcon == null)
                {
                    return false;
                }

                var iconFileNameObject = defaultIcon.GetValue("");
                if (iconFileNameObject == null || !(iconFileNameObject is string iconFileName) || !File.Exists(iconFileName))
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
                    var ix = cmdString.IndexOf("SubtitleEdit.exe");
                    if (ix >= 0)
                    {
                        var exeFileName = cmdString.Substring(0, ix + "SubtitleEdit.exe".Length).Trim('"');
                        return File.Exists(exeFileName);
                    }
                }
            }

            return false;
        }

        internal static void SetFileAssociationViaRegistry(string ext, string exeFileName, string iconFileName, string appName)
        {
            Registry.SetValue($"HKEY_CURRENT_USER\\Software\\Classes\\{appName}{ext}", "", $"{GetFriendlyName(ext)} subtitle file");
            Registry.SetValue($"HKEY_CURRENT_USER\\Software\\Classes\\{appName}{ext}\\DefaultIcon", "", iconFileName);
            Registry.SetValue($"HKEY_CURRENT_USER\\Software\\Classes\\{appName}{ext}\\shell\\open\\command", "", $"\"{exeFileName.Trim('"')}\" \"%1\"");
            Registry.SetValue($"HKEY_CURRENT_USER\\Software\\Classes\\{ext}", "", $"{appName}{ext}");
        }

        private static string GetFriendlyName(string ext)
        {
            if (ext.Equals(".srt", StringComparison.OrdinalIgnoreCase))
            {
                return "SubRip subtitle file";
            }

            if (ext.Equals(".ass", StringComparison.OrdinalIgnoreCase))
            {
                return "Advanced Sub Station Alpha subtitle file";
            }

            if (ext.Equals(".dfxp", StringComparison.OrdinalIgnoreCase))
            {
                return "Distribution Format Exchange Profile subtitle file";
            }

            if (ext.Equals(".ssa", StringComparison.OrdinalIgnoreCase))
            {
                return "Sub Station Alpha subtitle file";
            }

            if (ext.Equals(".sup", StringComparison.OrdinalIgnoreCase))
            {
                return "Blu-ray PGS subtitle file";
            }

            if (ext.Equals(".vtt", StringComparison.OrdinalIgnoreCase))
            {
                return "Web Video Text Tracks (WebVTT) subtitle file";
            }

            if (ext.Equals(".smi", StringComparison.OrdinalIgnoreCase))
            {
                return "SAMI subtitle file";
            }

            if (ext.Equals(".itt", StringComparison.OrdinalIgnoreCase))
            {
                return "iTunes Timed Text subtitle file";
            }

            return $"{ext.TrimStart('.')} subtitle file";
        }

        internal static void DeleteFileAssociationViaRegistry(string ext, string appName)
        {
            using (var registryKey = Registry.CurrentUser.OpenSubKey(@"Software\Classes\", true))
            {
                if (registryKey?.OpenSubKey($"{appName}{ext}") != null)
                {
                    registryKey.DeleteSubKeyTree($"{appName}{ext}");
                }
            }
        }

        internal static void Refresh()
        {
            //this call notifies Windows that it needs to redo the file associations and icons
            SHChangeNotify(0x08000000, 0x2000, IntPtr.Zero, IntPtr.Zero);
        }
    }
}
