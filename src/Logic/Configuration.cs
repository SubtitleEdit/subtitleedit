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
                    string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + "Subtitle Edit";
                    bool useApplicationData = BaseDirectory.StartsWith(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), StringComparison.OrdinalIgnoreCase);

                    if (!useApplicationData)
                        useApplicationData = !Directory.Exists(BaseDirectory + "Dictionaries") && Directory.Exists(Path.Combine(path, "Directonries"));

                    if (useApplicationData)
                    {
                        try
                        {
                            if (!Directory.Exists(path))
                                Directory.CreateDirectory(path);

                            Instance._dataDir = path + Path.DirectorySeparatorChar;
                        }
                        catch
                        {
                            Instance._dataDir = BaseDirectory;
                        }
                    }
                    else
                    {
                        Instance._dataDir = BaseDirectory;
                    }
                }
                return Instance._dataDir;
            }
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
