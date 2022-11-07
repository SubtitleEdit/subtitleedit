using Nikse.SubtitleEdit.Core.Common;
using System;
using System.IO;

namespace Nikse.SubtitleEdit.Core.AudioToText
{
    public static class WhisperHelper
    {
        public static IWhisperModel GetWhisperModel()
        {
            return Configuration.Settings.Tools.UseWhisperCpp ? (IWhisperModel)new WhisperCppModel() : new WhisperModel();
        }

        public static string ModelExtension()
        {
            return Configuration.Settings.Tools.UseWhisperCpp ? ".bin" : ".pt";
        }

        public static string GetWebSiteUrl()
        {
            return Configuration.Settings.Tools.UseWhisperCpp ? "https://github.com/ggerganov/whisper.cpp" : "https://github.com/openai/whisper";
        }

        public static bool IsWhisperInstalled()
        {
            if (Directory.Exists(GetWhisperModel().ModelFolder) || Configuration.IsRunningOnLinux)
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

                if (Configuration.Settings.Tools.UseWhisperCpp)
                {
                    var path = Path.Combine(Configuration.DataDirectory, "Whisper");
                    return Directory.Exists(path) ? path : null;
                }

                var pythonFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
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

                var pyEnvFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".pyenv");
                if (Directory.Exists(pyEnvFolder))
                {
                    var files = Directory.GetFiles(pyEnvFolder, "whisper.exe", SearchOption.AllDirectories);
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

                var appDataLocal = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData", "Local");
                if (Directory.Exists(appDataLocal))
                {
                    var files = Directory.GetFiles(appDataLocal, "whisper.exe", SearchOption.AllDirectories);
                    if (files.Length > 0)
                    {
                        return Path.GetDirectoryName(files[0]);
                    }
                }

                pythonFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData");
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

        public static string GetWhisperPathAndFileName()
        {
            if (Configuration.IsRunningOnWindows && Configuration.Settings.Tools.UseWhisperCpp)
            {
                return Path.Combine(GetWhisperFolder(), "whisper.exe");
            }

            return "whisper";
        }

        public static string GetWhisperModelForCmdLine(string model)
        {
            if (Configuration.IsRunningOnWindows && Configuration.Settings.Tools.UseWhisperCpp)
            {
                return Path.Combine(GetWhisperModel().ModelFolder, model + ModelExtension());
            }

            return model;
        }
    }
}
