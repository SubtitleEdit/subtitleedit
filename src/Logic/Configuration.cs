using System.IO;
using System;

namespace Nikse.SubtitleEdit.Logic
{
    public class Configuration
    {
        static readonly Configuration Instance = new Configuration();
        string _baseDir;
        string _dataDir;
        Settings _settings;

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

        public static string TesseractDataFolder
        {
            get
            {
                if (Utilities.IsRunningOnLinux() || Utilities.IsRunningOnMac())
                {
                    if (System.IO.Directory.Exists("/usr/share/tesseract-ocr/tessdata"))
                        return "/usr/share/tesseract-ocr/tessdata";
                    else if (System.IO.Directory.Exists("/usr/share/tesseract/tessdata"))
                        return "/usr/share/tesseract/tessdata";
                    else if (System.IO.Directory.Exists("/usr/share/tessdata"))
                        return "/usr/share/tessdata";
                }
                return TesseractFolder + "tessdata";
            }
        }

        public static string TesseractFolder
        {
            get
            {
                return BaseDirectory + "Tesseract" + Path.DirectorySeparatorChar;
            }
        }

        public static string VobSubCompareFolder
        {
            get
            {
                return DataDirectory + "VobSub" + Path.DirectorySeparatorChar;
            }
        }

        public static string WaveFormsFolder
        {
            get
            {
                return DataDirectory + "WaveForms" + Path.DirectorySeparatorChar;
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

        public static string DataDirectory
        {
            get
            {
                if (Instance._dataDir == null)
                {
                    if (Utilities.IsRunningOnLinux() || Utilities.IsRunningOnMac())
                    {
                        Instance._dataDir = BaseDirectory;
                    }
                    else
                    {
                        string installerPath = GetInstallerPath();
                        string pf = System.Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles).TrimEnd('\\');
                        string appDataRoamingPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Subtitle Edit");
                        if (installerPath != null && BaseDirectory.ToLower().StartsWith(installerPath.ToLower().TrimEnd('\\')))
                        {
                            if (Directory.Exists(appDataRoamingPath))
                            {
                                Instance._dataDir = appDataRoamingPath + Path.DirectorySeparatorChar;
                            }
                            else
                            {
                                Instance._dataDir = BaseDirectory;
                                System.Windows.Forms.MessageBox.Show("Please re-install Subtitle Edit (installer version)");
                                System.Windows.Forms.Application.ExitThread();
                            }
                        }
                        else if (BaseDirectory.ToLower().StartsWith(pf.ToLower()) && Environment.OSVersion.Version.Major >= 6 ) // 6 == Vista/Win2008Server/Win7
                        { // windows vista and newer does not like programs writing to PF
                            Instance._dataDir = BaseDirectory;
                            //if (Configuration.Settings.General.ShowOriginalAsPreviewIfAvailable)
                            //{
                            //    System.Windows.Forms.MessageBox.Show("Warning: Subtitle Edit portable should not be installed in " + pf);
                            //    Configuration.Settings.General.ShowOriginalAsPreviewIfAvailable = false;
                            //}
                        }
                        else
                        {
                            Instance._dataDir = BaseDirectory;
                        }
                    }
                }
                return Instance._dataDir;
            }
        }

        private static string GetInstallerPath()
        {
            string installerPath = null;
            var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\SubtitleEdit_is1");
            if (key != null)
            {
                string temp = (string)key.GetValue("InstallLocation");
                if (temp != null && Directory.Exists(temp))
                    installerPath = temp;
            }
            if (installerPath == null)
            {
                key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\SubtitleEdit_is1");
                if (key != null)
                {
                    string temp = (string)key.GetValue("InstallLocation");
                    if (temp != null && Directory.Exists(temp))
                        installerPath = temp;
                }
            }
            return installerPath;
        }

        public static string BaseDirectory
        {
            get
            {
                if (Instance._baseDir == null)
                {
                    System.Reflection.Assembly a = System.Reflection.Assembly.GetEntryAssembly();
                    if (a != null)
                        Instance._baseDir = Path.GetDirectoryName(a.Location);
                    else
                        Instance._baseDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                    if (Instance._baseDir.EndsWith("Test\\bin\\Release"))
                        Instance._baseDir = Instance._baseDir.Replace("Test\\bin\\Release", "bin\\Release");
                    if (!Instance._baseDir.EndsWith(Path.DirectorySeparatorChar.ToString()))
                        Instance._baseDir += Path.DirectorySeparatorChar;
                }
                return Instance._baseDir;
            }
        }

        public static Settings Settings
        {
            get
            {
                if (Instance._settings == null)
                    Instance._settings = Settings.GetSettings();
                return Instance._settings;
            }
        }
    }
}
