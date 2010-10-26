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

        public static string DataDirectory
        {
            get
            {
                if (Instance._dataDir == null)
                {
                    if (BaseDirectory.StartsWith(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + "Subtitle Edit";
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
                    Instance._baseDir = Path.GetDirectoryName(a.Location);
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
