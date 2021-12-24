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
                if (cmdObject != null && cmdObject is string cmdString)
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
            Registry.SetValue($"HKEY_CURRENT_USER\\Software\\Classes\\{appName}{ext}", "", $"{ext.TrimStart('.')} subtitle file");
            Registry.SetValue($"HKEY_CURRENT_USER\\Software\\Classes\\{appName}{ext}\\DefaultIcon", "", iconFileName);
            //            Registry.SetValue($"HKEY_CURRENT_USER\\Software\\Classes\\{appName}{ext}", "FriendlyTypeName", "My Friendly Type Name");
            Registry.SetValue($"HKEY_CURRENT_USER\\Software\\Classes\\{appName}{ext}\\shell\\open\\command", "", $"\"{exeFileName.Trim('"')}\" \"%1\"");
            Registry.SetValue($"HKEY_CURRENT_USER\\Software\\Classes\\{ext}", "", $"{appName}{ext}");
        }

        internal static void DeleteFileAssociationViaRegistry(string ext, string appName)
        {
            using (RegistryKey regkey = Registry.CurrentUser.OpenSubKey(@"Software\Classes\", true))
            {
                if (regkey != null && regkey.OpenSubKey($"{appName}{ext}") != null)
                {
                    regkey.DeleteSubKeyTree($"{appName}{ext}");
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
