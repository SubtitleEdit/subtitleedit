using Nikse.SubtitleEdit.Core.Common;
using System;
using System.IO;

namespace Nikse.SubtitleEdit.Core.AudioToText
{
    public static class WhisperHelper
    {
        public static bool IsWhisperInstalled()
        {
            if (Directory.Exists(WhisperModel.ModelFolder) || Configuration.IsRunningOnLinux)
            {
                return true;
            }

            return !string.IsNullOrEmpty(GetWhisperFolder());
        }

        public static string GetWhisperFolder()
        {
            if (!Configuration.IsRunningOnWindows)
            {
                return null;
            }

            try
            {
                var location = Configuration.Settings.Tools.WhisperLocation;
                if (!string.IsNullOrEmpty(location))
                {
                    if (location.EndsWith("whisper.exe", StringComparison.InvariantCultureIgnoreCase) && File.Exists(location))
                    {
                        return Path.GetDirectoryName(location);
                    }

                    if (Directory.Exists(location) && File.Exists(Path.Combine(location, "whisper.exe")))
                    {
                        return location;
                    }
                }

                var pythonFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "AppData",
                    "Local",
                    "Programs",
                    "Python");
                if (Directory.Exists(pythonFolder))
                {
                    foreach (var dir in Directory.GetDirectories(pythonFolder))
                    {
                        var dirName = Path.GetFileName(dir);
                        if (dirName != null && dirName.StartsWith("Python3"))
                        {
                            var whisperFullPath = Path.Combine(dir, "Scripts", "whisper.exe");
                            if (File.Exists(whisperFullPath))
                            {
                                return Path.Combine(dir, "Scripts");
                            }
                        }
                    }
                }

                var programFilesFolder = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                foreach (var dir in Directory.GetDirectories(programFilesFolder))
                {
                    var dirName = Path.GetFileName(dir);
                    if (dirName != null && dirName.StartsWith("Python"))
                    {
                        var files = Directory.GetFiles(dir, "whisper.exe", SearchOption.AllDirectories);
                        if (files.Length > 0)
                        {
                            return Path.GetDirectoryName(files[0]);
                        }
                    }
                }

                var packagesFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "AppData",
                    "Local",
                    "Packages");
                if (Directory.Exists(packagesFolder))
                {
                    var files = Directory.GetFiles(packagesFolder, "whisper.exe", SearchOption.AllDirectories);
                    if (files.Length > 0)
                    {
                        return Path.GetDirectoryName(files[0]);
                    }
                }

                pythonFolder = "C:\\";
                foreach (var dir in Directory.GetDirectories(pythonFolder))
                {
                    var dirName = Path.GetFileName(dir);
                    if (dirName != null && dirName.StartsWith("Python"))
                    {
                        var files = Directory.GetFiles(dir, "whisper.exe", SearchOption.AllDirectories);
                        if (files.Length > 0)
                        {
                            return Path.GetDirectoryName(files[0]);
                        }
                    }
                }
            }
            catch
            {
                return null;
            }

            return null;
        }
    }
}
