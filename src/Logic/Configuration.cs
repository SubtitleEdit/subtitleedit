﻿using System.IO;
using System;

namespace Nikse.SubtitleEdit.Logic
{

    /// <summary>
    /// Configuration settings via Singleton pattern
    /// </summary>
    public class Configuration
    {
        string _baseDir;
        string _dataDir;
        Settings _settings;

        public static Configuration Instance
        {
            get { return Nested.instance; }
        }

        private Configuration()
        {
        }

        private class Nested
        {
            static Nested() { }
            internal static readonly Configuration instance = new Configuration();
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

        public static string PluginsDirectory
        {
            get
            {
                return Path.Combine(Configuration.DataDirectory, "Plugins");
            }
        }

        public static string DataDirectory
        {
            get
            {
                if (Instance._dataDir == null)
                {
                    if (IsRunningOnLinux() || IsRunningOnMac())
                    {
                        Instance._dataDir = BaseDirectory;
                    }
                    else
                    {
                        string installerPath = GetInstallerPath();
                        string pf = System.Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles).TrimEnd(Path.DirectorySeparatorChar);
                        string appDataRoamingPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Subtitle Edit");
                        if (installerPath != null && BaseDirectory.ToLower().StartsWith(installerPath.ToLower().TrimEnd(Path.DirectorySeparatorChar)))
                        {
                            if (Directory.Exists(appDataRoamingPath))
                            {
                                Instance._dataDir = appDataRoamingPath + Path.DirectorySeparatorChar;
                            }
                            else
                            {
                                try
                                {
                                    System.IO.Directory.CreateDirectory(appDataRoamingPath);
                                    System.IO.Directory.CreateDirectory(Path.Combine(appDataRoamingPath, "Dictionaries"));
                                    Instance._dataDir = appDataRoamingPath + Path.DirectorySeparatorChar;
                                }
                                catch
                                {
                                    Instance._dataDir = BaseDirectory;
                                    System.Windows.Forms.MessageBox.Show("Please re-install Subtitle Edit (installer version)");
                                    System.Windows.Forms.Application.ExitThread();
                                }
                            }
                        }
                        else if (BaseDirectory.ToLower().StartsWith(pf.ToLower()) && Environment.OSVersion.Version.Major >= 6 ) // 6 == Vista/Win2008Server/Win7
                        { // windows vista and newer does not like programs writing to PF (nor does WinXp with non admin...)
                            try
                            {
                                System.IO.Directory.CreateDirectory(appDataRoamingPath);
                                System.IO.Directory.CreateDirectory(Path.Combine(appDataRoamingPath, "Dictionaries"));
                                Instance._dataDir = appDataRoamingPath + Path.DirectorySeparatorChar;
                            }
                            catch
                            {
                                Instance._dataDir = BaseDirectory;
                                System.Windows.Forms.MessageBox.Show("Please do not install portable version in 'Program Files' folder.");
                                System.Windows.Forms.Application.ExitThread();
                            }
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

                    // C:\Data\subtitleedit\subtitleedit\src\TestResults
                    string tag = @"\SubtitleEdit\src\TestResults\";
                    int start = Instance._baseDir.ToLower().IndexOf(tag.ToLower());
                    if (start > 0)
                    {
                        string s = Instance._baseDir.Substring(0, start + tag.Length - 12) + "bin\\Release";
                        Instance._baseDir = s;
                    }


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
