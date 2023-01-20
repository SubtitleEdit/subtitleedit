using Nikse.SubtitleEdit.Core.Common;
using System;
using System.IO;

namespace Nikse.SubtitleEdit.Core.AudioToText
{
    public static class WhisperHelper
    {
        public static IWhisperModel GetWhisperModel()
        {
            return Configuration.Settings.Tools.WhisperChoice == WhisperChoice.Cpp ? (IWhisperModel)new WhisperCppModel() : new WhisperModel();
        }

        public static string ModelExtension()
        {
            return Configuration.Settings.Tools.WhisperChoice == WhisperChoice.Cpp ? ".bin" : ".pt";
        }

        public static string GetWebSiteUrl()
        {
            if (Configuration.Settings.Tools.WhisperChoice == WhisperChoice.Cpp)
            {
                return "https://github.com/ggerganov/whisper.cpp";
            }

            if (Configuration.Settings.Tools.WhisperChoice == WhisperChoice.WhisperX)
            {
                return "https://github.com/m-bain/whisperX";
            }

            return "https://github.com/openai/whisper";
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
            if (Configuration.IsRunningOnLinux && Configuration.Settings.Tools.WhisperChoice == WhisperChoice.Cpp)
            {
                var path = Path.Combine(Configuration.DataDirectory, "Whisper");
                return Directory.Exists(path) ? path : null;
            }

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

                if (Configuration.Settings.Tools.WhisperChoice == WhisperChoice.Cpp)
                {
                    var path = Path.Combine(Configuration.DataDirectory, "Whisper");
                    return Directory.Exists(path) ? path : null;
                }

                if (Configuration.Settings.Tools.WhisperChoice == WhisperChoice.WhisperX && !string.IsNullOrEmpty(Configuration.Settings.Tools.WhisperXLocation))
                {
                    if (Configuration.Settings.Tools.WhisperXLocation.EndsWith("whisperx.exe", StringComparison.InvariantCultureIgnoreCase) && File.Exists(location))
                    {
                        return Path.GetDirectoryName(Configuration.Settings.Tools.WhisperXLocation);
                    }

                    if (Directory.Exists(Configuration.Settings.Tools.WhisperXLocation) && File.Exists(Path.Combine(Configuration.Settings.Tools.WhisperXLocation, "whisperx.exe")))
                    {
                        return Configuration.Settings.Tools.WhisperXLocation;
                    }
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
                            if (Configuration.Settings.Tools.WhisperChoice == WhisperChoice.WhisperX)
                            {
                                var whisperXFullPath = Path.Combine(dir, "Scripts", "whisperx.exe");
                                if (File.Exists(whisperXFullPath))
                                {
                                    return Path.Combine(dir, "Scripts");
                                }
                            }

                            var whisperFullPath = Path.Combine(dir, "Scripts", "whisper.exe");
                            if (File.Exists(whisperFullPath))
                            {
                                return Path.Combine(dir, "Scripts");
                            }
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
            var whisperFolder = GetWhisperFolder();
            if (string.IsNullOrEmpty(whisperFolder))
            {
                if (Configuration.Settings.Tools.WhisperChoice == WhisperChoice.Cpp)
                {
                    return "main";
                }

                if (Configuration.Settings.Tools.WhisperChoice == WhisperChoice.WhisperX)
                {
                    return "whisperx";
                }

                return "whisper";
            }

            if (Configuration.IsRunningOnWindows)
            {
                if (Configuration.Settings.Tools.WhisperChoice == WhisperChoice.Cpp)
                {
                    var f = Path.Combine(whisperFolder, "main.exe");
                    if (File.Exists(f))
                    {
                        return f;
                    }
                }
                else if (Configuration.Settings.Tools.WhisperChoice == WhisperChoice.WhisperX)
                {
                    var f = Path.Combine(whisperFolder, "whisperx.exe");
                    if (File.Exists(f))
                    {
                        return f;
                    }
                }
                else
                {
                    var f = Path.Combine(whisperFolder, "whisper.exe");
                    if (File.Exists(f))
                    {
                        return f;
                    }
                }
            }

            if (Configuration.IsRunningOnLinux && Configuration.Settings.Tools.WhisperChoice == WhisperChoice.Cpp)
            {
                var f = Path.Combine(whisperFolder, "main");
                if (File.Exists(f))
                {
                    return f;
                }
            }
            else if (Configuration.IsRunningOnLinux && Configuration.Settings.Tools.WhisperChoice == WhisperChoice.WhisperX)
            {
                var f = Path.Combine(whisperFolder, "whisperx");
                if (File.Exists(f))
                {
                    return f;
                }
            }

            return "whisper";
        }

        public static string GetWhisperModelForCmdLine(string model)
        {
            if (Configuration.Settings.Tools.WhisperChoice == WhisperChoice.Cpp)
            {
                return Path.Combine(GetWhisperModel().ModelFolder, model + ModelExtension());
            }

            return model;
        }

        public static string GetWhisperTranslateParameter()
        {
            return Configuration.Settings.Tools.WhisperChoice == WhisperChoice.Cpp ? "--translate " : "--task translate ";
        }
    }
}
