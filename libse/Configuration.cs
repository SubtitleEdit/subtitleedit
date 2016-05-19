using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Core
{
    /// <summary>
    /// Configuration settings via Singleton pattern
    /// </summary>
    public class Configuration
    {
        private static readonly Lazy<Configuration> Instance = new Lazy<Configuration>(() => new Configuration());

        private readonly string _baseDir;
        private readonly string _dataDir;
        private readonly Lazy<Settings> _settings;
        private readonly IEnumerable<Encoding> _encodings;

        private Configuration()
        {
            _baseDir = GetBaseDirectory();
            _dataDir = GetDataDirectory();
            _encodings = GetAvailableEncodings();
            _settings = new Lazy<Settings>(Settings.GetSettings);
        }

        public static string SettingsFileName
        {
            get
            {
                return DataDirectory + "Settings.xml";
            }
        }

        public static string DictionariesFolder
        {
            get
            {
                return DataDirectory + "Dictionaries" + Path.DirectorySeparatorChar;
            }
        }

        public static string IconsFolder
        {
            get
            {
                return BaseDirectory + "Icons" + Path.DirectorySeparatorChar;
            }
        }

        public static bool IsRunningOnLinux()
        {
            return Environment.OSVersion.Platform == PlatformID.Unix && !IsRunningOnMac();
        }

        public static bool IsRunningOnMac()
        {
            // Current versions of Mono report the platform as Unix
            return Environment.OSVersion.Platform == PlatformID.MacOSX ||
                (Environment.OSVersion.Platform == PlatformID.Unix &&
                 Directory.Exists("/Applications") &&
                 Directory.Exists("/System") &&
                 Directory.Exists("/Users"));
        }

        public static string TesseractDataFolder
        {
            get
            {
                if (IsRunningOnLinux() || IsRunningOnMac())
                {
                    if (Directory.Exists("/usr/share/tesseract-ocr/tessdata"))
                        return "/usr/share/tesseract-ocr/tessdata";
                    if (Directory.Exists("/usr/share/tesseract/tessdata"))
                        return "/usr/share/tesseract/tessdata";
                    if (Directory.Exists("/usr/share/tessdata"))
                        return "/usr/share/tessdata";
                }
                return TesseractFolder + "tessdata";
            }
        }

        public static string TesseractOriginalFolder
        {
            get
            {
                return BaseDirectory + "Tesseract" + Path.DirectorySeparatorChar;
            }
        }

        public static string TesseractFolder
        {
            get
            {
                return DataDirectory + "Tesseract" + Path.DirectorySeparatorChar;
            }
        }

        public static string VobSubCompareFolder
        {
            get
            {
                return DataDirectory + "VobSub" + Path.DirectorySeparatorChar;
            }
        }

        public static string OcrFolder
        {
            get
            {
                return DataDirectory + "Ocr" + Path.DirectorySeparatorChar;
            }
        }

        public static string WaveformsFolder
        {
            get
            {
                return DataDirectory + "Waveforms" + Path.DirectorySeparatorChar;
            }
        }

        public static string SpectrogramsFolder
        {
            get
            {
                return DataDirectory + "Spectrograms" + Path.DirectorySeparatorChar;
            }
        }

        public static string AutoBackupFolder
        {
            get
            {
                return DataDirectory + "AutoBackup" + Path.DirectorySeparatorChar;
            }
        }

        public static string PluginsDirectory
        {
            get
            {
                return Path.Combine(DataDirectory, "Plugins");
            }
        }

        public static string DataDirectory
        {
            get
            {
                return Instance.Value._dataDir;
            }
        }

        public static string BaseDirectory
        {
            get
            {
                return Instance.Value._baseDir;
            }
        }

        public static Settings Settings
        {
            get
            {
                return Instance.Value._settings.Value;
            }
        }

        public static IEnumerable<Encoding> AvailableEncodings
        {
            get
            {
                return Instance.Value._encodings;
            }
        }

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
            var assembly = System.Reflection.Assembly.GetEntryAssembly();
            var baseDirectory = Path.GetDirectoryName(assembly == null
                ? System.Reflection.Assembly.GetExecutingAssembly().Location
                : assembly.Location);

            return baseDirectory.EndsWith(Path.DirectorySeparatorChar)
                ? baseDirectory
                : baseDirectory + Path.DirectorySeparatorChar;
        }

        private string GetDataDirectory()
        {
            var appDataRoamingPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Subtitle Edit");

            if (IsRunningOnLinux() || IsRunningOnMac())
            {
                if (!Directory.Exists(appDataRoamingPath) && !File.Exists(Path.Combine(_baseDir, ".PACKAGE-MANAGER")))
                {
                    try
                    {
                        var path = Path.Combine(Directory.CreateDirectory(Path.Combine(_baseDir, "Dictionaries")).FullName, "not-a-word-list");
                        File.Create(path).Close();
                        File.Delete(path);
                        return _baseDir; // user installation
                    }
                    catch
                    {
                    }
                }
                Directory.CreateDirectory(Path.Combine(appDataRoamingPath, "Dictionaries"));
                return appDataRoamingPath + Path.DirectorySeparatorChar; // system installation
            }

            var installerPath = GetInstallerPath();
            var hasUninstallFiles = Directory.GetFiles(_baseDir, "unins*.*").Length > 0;
            var hasDictionaryFolder = Directory.Exists(Path.Combine(_baseDir, "Dictionaries"));

            if ((installerPath == null || !installerPath.TrimEnd(Path.DirectorySeparatorChar).Equals(_baseDir.TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase))
                && !hasUninstallFiles && (hasDictionaryFolder || !Directory.Exists(Path.Combine(appDataRoamingPath, "Dictionaries"))))
            {
                return _baseDir;
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
            return encodings.ToArray();
        }

    }
}
