using System;
using System.IO;

namespace Nikse.SubtitleEdit.Core.Common
{
    public class Configuration
    {
        public static string BaseDirectory = string.Empty;
        public static string DataDirectory = string.Empty;
        private static readonly Configuration Instance = new Configuration();
        private Settings.Settings _settings = new Settings.Settings();
        public static Settings.Settings Settings => Instance._settings;
        public static string DictionariesDirectory => Path.Combine(DataDirectory, "Dictionaries") + Path.DirectorySeparatorChar;
        public static readonly string DefaultLinuxFontName = "DejaVu Serif";

        private Configuration()
        {
        }

        private const int PlatformWindows = 1;
        private const int PlatformLinux = 2;
        private const int PlatformMac = 3;
        private static int _platform;

        public static bool IsRunningOnWindows
        {
            get
            {
                if (_platform == 0)
                {
                    _platform = GetPlatform();
                }
                return _platform == PlatformWindows;
            }
        }

        public static bool IsRunningOnLinux
        {
            get
            {
                if (_platform == 0)
                {
                    _platform = GetPlatform();
                }
                return _platform == PlatformLinux;
            }
        }

        public static bool IsRunningOnMac
        {
            get
            {
                if (_platform == 0)
                {
                    _platform = GetPlatform();
                }
                return _platform == PlatformMac;
            }
        }

        private static int GetPlatform()
        {
            // Current versions of Mono report MacOSX platform as Unix
            return Environment.OSVersion.Platform == PlatformID.MacOSX || (Environment.OSVersion.Platform == PlatformID.Unix && Directory.Exists("/Applications") && Directory.Exists("/System") && Directory.Exists("/Users"))
                 ? PlatformMac
                 : Environment.OSVersion.Platform == PlatformID.Unix
                 ? PlatformLinux
                 : PlatformWindows;
        }
    }
}
