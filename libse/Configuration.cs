using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Core
{
    /// <summary>
    /// Configuration settings via Singleton pattern
    /// </summary>
    public class Configuration
    {
        private static readonly Lazy<Configuration> Instance = new Lazy<Configuration>(() => new Configuration());

        private readonly Lazy<Settings> _settings;
        private readonly IEnumerable<Encoding> _encodings;

        public static readonly string BaseDirectory = GetBaseDirectory();
        public static readonly string DataDirectory = GetDataDirectory();
        public static readonly string TesseractOriginalDirectory = BaseDirectory + "Tesseract302" + Path.DirectorySeparatorChar;
        public static readonly string DictionariesDirectory = DataDirectory + "Dictionaries" + Path.DirectorySeparatorChar;
        public static readonly string SpectrogramsDirectory = DataDirectory + "Spectrograms" + Path.DirectorySeparatorChar;
        public static readonly string SceneChangesDirectory = DataDirectory + "SceneChanges" + Path.DirectorySeparatorChar;
        public static readonly string AutoBackupDirectory = DataDirectory + "AutoBackup" + Path.DirectorySeparatorChar;
        public static readonly string VobSubCompareDirectory = DataDirectory + "VobSub" + Path.DirectorySeparatorChar;
        public static readonly string TesseractDirectory = DataDirectory + "Tesseract411" + Path.DirectorySeparatorChar;
        public static readonly string Tesseract302Directory = DataDirectory + "Tesseract302" + Path.DirectorySeparatorChar;
        public static readonly string WaveformsDirectory = DataDirectory + "Waveforms" + Path.DirectorySeparatorChar;
        public static readonly string PluginsDirectory = DataDirectory + "Plugins" + Path.DirectorySeparatorChar;
        public static readonly string IconsDirectory = BaseDirectory + "Icons" + Path.DirectorySeparatorChar;
        public static readonly string OcrDirectory = DataDirectory + "Ocr" + Path.DirectorySeparatorChar;
        public static readonly string SettingsFileName = DataDirectory + "Settings.xml";
        public static readonly string TesseractDataDirectory = GetTesseractDataDirectory();
        public static readonly string Tesseract302DataDirectory = GetTesseract302DataDirectory();

        public static readonly string DefaultLinuxFontName = "DejaVu Serif";

        private Configuration()
        {
            _encodings = GetAvailableEncodings();
            _settings = new Lazy<Settings>(Settings.GetSettings);
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

        public static Settings Settings => Instance.Value._settings.Value;

        public static IEnumerable<Encoding> AvailableEncodings => Instance.Value._encodings;

        private static string GetInstallerPath()
        {
            const string valueName = "InstallLocation";
            var value = RegistryUtil.GetValue(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\SubtitleEdit_is1", valueName);
            if (value != null && Directory.Exists(value))
            {
                return value;
            }

            value = RegistryUtil.GetValue(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\SubtitleEdit_is1", valueName);
            if (value != null && Directory.Exists(value))
            {
                return value;
            }

            return null;
        }

        private static string GetBaseDirectory()
        {
            var assembly = System.Reflection.Assembly.GetEntryAssembly() ?? System.Reflection.Assembly.GetExecutingAssembly();
            return Path.GetDirectoryName(assembly.Location) + Path.DirectorySeparatorChar;
        }

        private static string GetDataDirectory()
        {
            // hack for unit tests
            var assembly = System.Reflection.Assembly.GetEntryAssembly() ?? System.Reflection.Assembly.GetExecutingAssembly();
            var srcTestResultsIndex = assembly.Location.IndexOf(@"\src\TestResults", StringComparison.Ordinal);
            if (srcTestResultsIndex > 0)
            {
                var debugOrReleaseFolderName = "Release";
#if DEBUG
                debugOrReleaseFolderName = "Debug";
#endif
                return $@"{assembly.Location.Substring(0, srcTestResultsIndex)}\src\Test\bin\{debugOrReleaseFolderName}\";
            }

            var appDataRoamingPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Subtitle Edit");
            if (IsRunningOnLinux || IsRunningOnMac)
            {
                if (!Directory.Exists(appDataRoamingPath) && !File.Exists(Path.Combine(BaseDirectory, ".PACKAGE-MANAGER")))
                {
                    try
                    {
                        var path = Path.Combine(Directory.CreateDirectory(Path.Combine(BaseDirectory, "Dictionaries")).FullName, "not-a-word-list");
                        File.Create(path).Close();
                        File.Delete(path);
                        return BaseDirectory; // user installation
                    }
                    catch
                    {
                        // ignored
                    }
                }
                Directory.CreateDirectory(Path.Combine(appDataRoamingPath, "Dictionaries"));
                return appDataRoamingPath + Path.DirectorySeparatorChar; // system installation
            }

            var installerPath = GetInstallerPath();
            var hasUninstallFiles = Directory.GetFiles(BaseDirectory, "unins*.*").Length > 0;
            var hasDictionaryFolder = Directory.Exists(Path.Combine(BaseDirectory, "Dictionaries"));

            if ((installerPath == null || !installerPath.TrimEnd(Path.DirectorySeparatorChar).Equals(BaseDirectory.TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase))
                && !hasUninstallFiles && (hasDictionaryFolder || !Directory.Exists(Path.Combine(appDataRoamingPath, "Dictionaries"))))
            {
                return BaseDirectory;
            }

            if (Directory.Exists(appDataRoamingPath))
            {
                return appDataRoamingPath + Path.DirectorySeparatorChar;
            }

            try
            {
                Directory.CreateDirectory(appDataRoamingPath);
                Directory.CreateDirectory(Path.Combine(appDataRoamingPath, "Dictionaries"));
                return appDataRoamingPath + Path.DirectorySeparatorChar;
            }
            catch
            {
                throw new Exception("Please re-install Subtitle Edit (installer version)");
            }
        }

        private static string GetTesseractDataDirectory()
        {
            if (IsRunningOnLinux || IsRunningOnMac)
            {
                if (Directory.Exists("/usr/share/tesseract-ocr/4.00/tessdata"))
                {
                    return "/usr/share/tesseract-ocr/4.00/tessdata";
                }

                if (Directory.Exists("/usr/share/tesseract-ocr/tessdata"))
                {
                    return "/usr/share/tesseract-ocr/tessdata";
                }

                if (Directory.Exists("/usr/share/tessdata"))
                {
                    return "/usr/share/tessdata";
                }
            }
            return Path.Combine(TesseractDirectory, "tessdata");
        }

        private static string GetTesseract302DataDirectory()
        {
            if (IsRunningOnLinux || IsRunningOnMac)
            {
                if (Directory.Exists("/usr/share/tesseract-ocr/tessdata"))
                {
                    return "/usr/share/tesseract-ocr/tessdata";
                }

                if (Directory.Exists("/usr/share/tesseract/tessdata"))
                {
                    return "/usr/share/tesseract/tessdata";
                }

                if (Directory.Exists("/usr/share/tessdata"))
                {
                    return "/usr/share/tessdata";
                }
            }
            return Path.Combine(Tesseract302Directory, "tessdata");
        }

        private static IEnumerable<Encoding> GetAvailableEncodings()
        {
            var encodings = new List<Encoding>();
            foreach (var ei in Encoding.GetEncodings())
            {
                try
                {
                    encodings.Add(Encoding.GetEncoding(ei.CodePage));
                }
                catch
                {
                    // though advertised, this code page is not supported
                }
            }
            return encodings.AsEnumerable();
        }

    }
}
