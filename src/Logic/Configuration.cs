using System.IO;

namespace Nikse.SubtitleEdit.Logic
{
    public class Configuration
    {
        static readonly Configuration Instance = new Configuration();
        string _baseDir;
        Settings _settings;

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
