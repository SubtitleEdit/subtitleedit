using System;
using System.IO;

namespace Nikse.SubtitleEdit.Logic
{
    /// <summary>
    /// Configuration settings via Singleton pattern (note: Do not call Utilities from this class)
    /// </summary>
    public class Configuration
    {
        private static readonly Lazy<Configuration> _instance = new Lazy<Configuration>(() => new Configuration());

        private readonly string _baseDir;
        private readonly string _dataDir;
        private readonly Lazy<Settings> _settings;

        private Configuration()
        {
            _baseDir = GetBaseDirectory();
            _dataDir = GetDataDirectory();
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
            return Environment.OSVersion.Platform == PlatformID.Unix;
        }

        public static bool IsRunningOnMac()
        {
            return Environment.OSVersion.Platform == PlatformID.MacOSX;
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
                return _instance.Value._dataDir;
            }
        }

        public static string BaseDirectory
        {
            get
            {
                return _instance.Value._baseDir;
            }
        }

        public static Settings Settings
        {
            get
            {
                return _instance.Value._settings.Value;
            }
        }

        /// <summary>
        /// Retrieves the specified registry subkey value.
        /// </summary>
        /// <param name="keyName">The path of the subkey to open.</param>
        /// <param name="valueName">The name of the value to retrieve.</param>
        /// <returns>The value of the subkey requested, or <b>null</b> if the operation failed.</returns>
        public static string GetRegistryValue(string keyName, string valueName)
        {
            Microsoft.Win32.RegistryKey key = null;
            try
            {
                key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(keyName);
                if (key != null)
                {
                    var value = key.GetValue(valueName);
                    if (value != null)
                    {
                        return (string)value;
                    }
                }
            }
            catch (System.Security.SecurityException)
            {
                // The user does not have the permissions required to read the registry key.
            }
            finally
            {
                if (key != null)
                {
                    key.Dispose();
                }
            }
            return null;
        }

        private static string GetInstallerPath()
        {
            var value = GetRegistryValue(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\SubtitleEdit_is1", "InstallLocation");
            if (value != null && Directory.Exists(value))
            {
                return value;
            }

            value = GetRegistryValue(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\SubtitleEdit_is1", "InstallLocation");
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
            if (IsRunningOnLinux() || IsRunningOnMac())
            {
                return _baseDir;
            }

            var installerPath = GetInstallerPath();
            var hasUninstallFiles = Directory.GetFiles(_baseDir, "unins*.*").Length > 0;
            var hasDictionaryFolder = Directory.Exists(Path.Combine(_baseDir, "Dictionaries"));
            var appDataRoamingPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Subtitle Edit");

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
                System.Windows.Forms.MessageBox.Show("Please re-install Subtitle Edit (installer version)");
                System.Windows.Forms.Application.ExitThread();
                return _baseDir;
            }
        }
    }
}
