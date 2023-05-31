using Nikse.SubtitleEdit.Core.Common;
using System;
using System.IO;

namespace Nikse.SubtitleEdit.Core.AudioToText
{
    public static class WhisperHelper
    {
        public static IWhisperModel GetWhisperModel()
        {
            if (Configuration.Settings.Tools.WhisperChoice == WhisperChoice.Cpp ||
                Configuration.Settings.Tools.WhisperChoice == WhisperChoice.ConstMe)
            {
                return new WhisperCppModel();
            }

            if (Configuration.Settings.Tools.WhisperChoice == WhisperChoice.CTranslate2)
            {
                return new WhisperCTranslate2Model();
            }

            return new WhisperModel();
        }

        public static string ModelExtension()
        {
            if (Configuration.Settings.Tools.WhisperChoice == WhisperChoice.Cpp ||
                Configuration.Settings.Tools.WhisperChoice == WhisperChoice.ConstMe)
            {
                return ".bin";
            }

            if (Configuration.Settings.Tools.WhisperChoice == WhisperChoice.CTranslate2)
            {
                return string.Empty;
            }

            return ".pt";
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

            if (Configuration.Settings.Tools.WhisperChoice == WhisperChoice.ConstMe)
            {
                return "https://github.com/Const-me/Whisper";
            }

            if (Configuration.Settings.Tools.WhisperChoice == WhisperChoice.CTranslate2)
            {
                return "https://github.com/jordimas/whisper-ctranslate2";
            }

            if (Configuration.Settings.Tools.WhisperChoice == WhisperChoice.StableTs)
            {
                return "https://github.com/jianfch/stable-ts";
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
                if (Configuration.Settings.Tools.WhisperChoice == WhisperChoice.OpenAI)
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
                }

                if (Configuration.Settings.Tools.WhisperChoice == WhisperChoice.CTranslate2)
                {
                    var location = Configuration.Settings.Tools.WhisperCtranslate2Location;
                    if (!string.IsNullOrEmpty(location))
                    {
                        if (location.EndsWith("whisper-ctranslate2.exe", StringComparison.InvariantCultureIgnoreCase) && File.Exists(location))
                        {
                            return Path.GetDirectoryName(location);
                        }

                        if (Directory.Exists(location) && File.Exists(Path.Combine(location, "whisper-ctranslate2.exe")))
                        {
                            return location;
                        }
                    }
                }

                if (Configuration.Settings.Tools.WhisperChoice == WhisperChoice.Cpp)
                {
                    var path = Path.Combine(Configuration.DataDirectory, "Whisper");
                    return Directory.Exists(path) ? path : null;
                }

                if (Configuration.Settings.Tools.WhisperChoice == WhisperChoice.WhisperX && !string.IsNullOrEmpty(Configuration.Settings.Tools.WhisperXLocation))
                {
                    if (Configuration.Settings.Tools.WhisperXLocation.EndsWith("whisperx.exe", StringComparison.InvariantCultureIgnoreCase) && File.Exists(Configuration.Settings.Tools.WhisperXLocation))
                    {
                        return Path.GetDirectoryName(Configuration.Settings.Tools.WhisperXLocation);
                    }

                    if (Directory.Exists(Configuration.Settings.Tools.WhisperXLocation) && File.Exists(Path.Combine(Configuration.Settings.Tools.WhisperXLocation, "whisperx.exe")))
                    {
                        return Configuration.Settings.Tools.WhisperXLocation;
                    }
                }

                if (Configuration.Settings.Tools.WhisperChoice == WhisperChoice.WhisperX)
                {
                    var path = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                        "anaconda3", "envs", "whisperx", "Scripts", "whisperx.exe");
                    if (File.Exists(path))
                    {
                        return path;
                    }
                }

                if (Configuration.Settings.Tools.WhisperChoice == WhisperChoice.StableTs && !string.IsNullOrEmpty(Configuration.Settings.Tools.WhisperStableTsLocation))
                {
                    if (Configuration.Settings.Tools.WhisperStableTsLocation.EndsWith("stable-ts.exe", StringComparison.InvariantCultureIgnoreCase) && File.Exists(Configuration.Settings.Tools.WhisperStableTsLocation))
                    {
                        return Path.GetDirectoryName(Configuration.Settings.Tools.WhisperStableTsLocation);
                    }

                    if (Directory.Exists(Configuration.Settings.Tools.WhisperStableTsLocation) && File.Exists(Path.Combine(Configuration.Settings.Tools.WhisperStableTsLocation, "stable-ts.exe")))
                    {
                        return Configuration.Settings.Tools.WhisperStableTsLocation;
                    }
                }

                if (Configuration.Settings.Tools.WhisperChoice == WhisperChoice.ConstMe)
                {
                    var path = Path.Combine(Configuration.DataDirectory, "Whisper", "Const-me");
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
                            if (Configuration.Settings.Tools.WhisperChoice == WhisperChoice.WhisperX)
                            {
                                var whisperXFullPath = Path.Combine(dir, "Scripts", "whisperx.exe");
                                if (File.Exists(whisperXFullPath))
                                {
                                    return Path.Combine(dir, "Scripts");
                                }

                                return null;
                            }

                            if (Configuration.Settings.Tools.WhisperChoice == WhisperChoice.CTranslate2)
                            {
                                var whisperCTranslate2FullPath = Path.Combine(dir, "Scripts", "whisper-ctranslate2.exe");
                                if (File.Exists(whisperCTranslate2FullPath))
                                {
                                    return Path.Combine(dir, "Scripts");
                                }

                                return null;
                            }

                            if (Configuration.Settings.Tools.WhisperChoice == WhisperChoice.StableTs)
                            {
                                var whisperCTranslate2FullPath = Path.Combine(dir, "Scripts", "stable-ts.exe");
                                if (File.Exists(whisperCTranslate2FullPath))
                                {
                                    return Path.Combine(dir, "Scripts");
                                }

                                return null;
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

                if (Configuration.Settings.Tools.WhisperChoice == WhisperChoice.CTranslate2)
                {
                    return "whisper-ctranslate2";
                }

                if (Configuration.Settings.Tools.WhisperChoice == WhisperChoice.WhisperX)
                {
                    return "whisperx";
                }

                if (Configuration.Settings.Tools.WhisperChoice == WhisperChoice.ConstMe)
                {
                    return "main";
                }

                if (Configuration.Settings.Tools.WhisperChoice == WhisperChoice.StableTs)
                {
                    return "stable-ts";
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
                else if (Configuration.Settings.Tools.WhisperChoice == WhisperChoice.CTranslate2)
                {
                    var f = Path.Combine(whisperFolder, "whisper-ctranslate2.exe");
                    if (File.Exists(f))
                    {
                        return f;
                    }
                }
                else if (Configuration.Settings.Tools.WhisperChoice == WhisperChoice.StableTs)
                {
                    var f = Path.Combine(whisperFolder, "stable-ts.exe");
                    if (File.Exists(f))
                    {
                        return f;
                    }
                }
                else if (Configuration.Settings.Tools.WhisperChoice == WhisperChoice.ConstMe)
                {
                    var f = Path.Combine(whisperFolder, "main.exe");
                    if (File.Exists(f))
                    {
                        return f;
                    }

                    return null;
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
            else if (Configuration.Settings.Tools.WhisperChoice == WhisperChoice.CTranslate2)
            {
                var f = Path.Combine(whisperFolder, "whisper-ctranslate2");
                if (File.Exists(f))
                {
                    return f;
                }
            }
            else if (Configuration.Settings.Tools.WhisperChoice == WhisperChoice.StableTs)
            {
                var f = Path.Combine(whisperFolder, "stable-ts");
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

            if (Configuration.Settings.Tools.WhisperChoice == WhisperChoice.ConstMe)
            {
                return Path.Combine(GetWhisperModel().ModelFolder, model + ModelExtension());
            }

            return model;
        }

        public static string GetWhisperTranslateParameter()
        {
            return Configuration.Settings.Tools.WhisperChoice == WhisperChoice.Cpp ||
                   Configuration.Settings.Tools.WhisperChoice == WhisperChoice.ConstMe
                ? "--translate " 
                : "--task translate ";
        }
    }
}
