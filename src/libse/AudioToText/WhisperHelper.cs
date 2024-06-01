using Nikse.SubtitleEdit.Core.Common;
using System;
using System.IO;

namespace Nikse.SubtitleEdit.Core.AudioToText
{
    public static class WhisperHelper
    {
        public static IWhisperModel GetWhisperModel()
        {
            return GetWhisperModel(Configuration.Settings.Tools.WhisperChoice);
        }

        public static IWhisperModel GetWhisperModel(string whisperChoice)
        {
            if (whisperChoice == WhisperChoice.Cpp || whisperChoice == WhisperChoice.CppCuBlas)
            {
                return new WhisperCppModel();
            }

            if (whisperChoice == WhisperChoice.ConstMe)
            {
                return new WhisperConstMeModel();
            }

            if (whisperChoice == WhisperChoice.CTranslate2)
            {
                return new WhisperCTranslate2Model();
            }

            if (whisperChoice == WhisperChoice.PurfviewFasterWhisper || whisperChoice == WhisperChoice.PurfviewFasterWhisperXXL)
            {
                return new WhisperPurfviewFasterWhisperModel();
            }

            return new WhisperModel();
        }

        public static string ModelExtension()
        {
            if (Configuration.Settings.Tools.WhisperChoice == WhisperChoice.Cpp || 
                Configuration.Settings.Tools.WhisperChoice == WhisperChoice.CppCuBlas ||
                Configuration.Settings.Tools.WhisperChoice == WhisperChoice.ConstMe)
            {
                return ".bin";
            }

            if (Configuration.Settings.Tools.WhisperChoice == WhisperChoice.CTranslate2 ||
                Configuration.Settings.Tools.WhisperChoice == WhisperChoice.PurfviewFasterWhisper ||
                Configuration.Settings.Tools.WhisperChoice == WhisperChoice.PurfviewFasterWhisperXXL)
            {
                return string.Empty;
            }

            return ".pt";
        }

        public static string GetWebSiteUrl()
        {
            if (Configuration.Settings.Tools.WhisperChoice == WhisperChoice.Cpp || Configuration.Settings.Tools.WhisperChoice == WhisperChoice.CppCuBlas)
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

            if (Configuration.Settings.Tools.WhisperChoice == WhisperChoice.PurfviewFasterWhisper ||
                Configuration.Settings.Tools.WhisperChoice == WhisperChoice.PurfviewFasterWhisperXXL)
            {
                return "https://github.com/Purfview/whisper-standalone-win";
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
            return GetWhisperFolder(Configuration.Settings.Tools.WhisperChoice);
        }

        public static string GetWhisperFolder(string whisperChoice)
        {
            if (Configuration.IsRunningOnLinux && whisperChoice == WhisperChoice.Cpp)
            {
                var path = Path.Combine(Configuration.DataDirectory, "Whisper", "Cpp");
                return Directory.Exists(path) ? path : null;
            }

            if (Configuration.Settings.Tools.WhisperChoice == WhisperChoice.CppCuBlas)
            {
                var path = Path.Combine(Configuration.DataDirectory, "Whisper", WhisperChoice.CppCuBlas);
                return Directory.Exists(path) ? path : null;
            }

            if (!Configuration.IsRunningOnWindows)
            {
                return null;
            }

            try
            {
                if (whisperChoice == WhisperChoice.OpenAi)
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

                if (whisperChoice == WhisperChoice.CTranslate2)
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

                if (whisperChoice == WhisperChoice.PurfviewFasterWhisper || whisperChoice  == WhisperChoice.PurfviewFasterWhisperXXL)
                {
                    var location = Configuration.Settings.Tools.WhisperCtranslate2Location;
                    if (!string.IsNullOrEmpty(location))
                    {
                        if (location.EndsWith("whisper-faster.exe", StringComparison.InvariantCultureIgnoreCase) && File.Exists(location))
                        {
                            return Path.GetDirectoryName(location);
                        }

                        if (Directory.Exists(location) && File.Exists(Path.Combine(location, "whisper-faster.exe")))
                        {
                            return location;
                        }
                    }

                    location = Path.Combine(Configuration.DataDirectory, "Whisper", "Purfview-Whisper-Faster");
                    return Directory.Exists(location) ? location : null;
                }

                if (whisperChoice == WhisperChoice.Cpp)
                {
                    var path = Path.Combine(Configuration.DataDirectory, "Whisper", "Cpp");
                    return Directory.Exists(path) ? path : null;
                }

                if (Configuration.Settings.Tools.WhisperChoice == WhisperChoice.CppCuBlas)
                {
                    var path = Path.Combine(Configuration.DataDirectory, "Whisper", WhisperChoice.CppCuBlas);
                    return Directory.Exists(path) ? path : null;
                }

                if (whisperChoice == WhisperChoice.WhisperX && !string.IsNullOrEmpty(Configuration.Settings.Tools.WhisperXLocation))
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

                if (whisperChoice == WhisperChoice.WhisperX)
                {
                    var path = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                        "anaconda3", "envs", "whisperx", "Scripts", "whisperx.exe");
                    if (File.Exists(path))
                    {
                        return path;
                    }
                }

                if (whisperChoice == WhisperChoice.StableTs && !string.IsNullOrEmpty(Configuration.Settings.Tools.WhisperStableTsLocation))
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

                if (whisperChoice == WhisperChoice.ConstMe)
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
                            if (whisperChoice == WhisperChoice.WhisperX)
                            {
                                var whisperXFullPath = Path.Combine(dir, "Scripts", "whisperx.exe");
                                if (File.Exists(whisperXFullPath))
                                {
                                    return Path.Combine(dir, "Scripts");
                                }

                                return null;
                            }

                            if (whisperChoice == WhisperChoice.CTranslate2)
                            {
                                var whisperCTranslate2FullPath = Path.Combine(dir, "Scripts", "whisper-ctranslate2.exe");
                                if (File.Exists(whisperCTranslate2FullPath))
                                {
                                    return Path.Combine(dir, "Scripts");
                                }

                                return null;
                            }

                            if (whisperChoice == WhisperChoice.StableTs)
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

        public static string GetExecutableFileName()
        {
            return GetExecutableFileName(Configuration.Settings.Tools.WhisperChoice);
        }

        public static string GetExecutableFileName(string whisperChoice)
        {
            var whisperFolder = GetWhisperFolder(whisperChoice);
            if (string.IsNullOrEmpty(whisperFolder))
            {
                if (whisperChoice == WhisperChoice.Cpp || Configuration.Settings.Tools.WhisperChoice == WhisperChoice.CppCuBlas)
                {
                    return "main";
                }

                if (whisperChoice == WhisperChoice.CTranslate2)
                {
                    return "whisper-ctranslate2";
                }

                if (whisperChoice == WhisperChoice.PurfviewFasterWhisper)
                {
                    return "whisper-faster.exe";
                }

                if (whisperChoice == WhisperChoice.PurfviewFasterWhisperXXL)
                {
                    return "faster-whisper-xxl.exe";
                }

                if (whisperChoice == WhisperChoice.WhisperX)
                {
                    return "whisperx";
                }

                if (whisperChoice == WhisperChoice.ConstMe)
                {
                    return "main";
                }

                if (whisperChoice == WhisperChoice.StableTs)
                {
                    return "stable-ts";
                }

                return "whisper";
            }

            if (Configuration.IsRunningOnWindows)
            {
                if (whisperChoice == WhisperChoice.Cpp || Configuration.Settings.Tools.WhisperChoice == WhisperChoice.CppCuBlas)
                {
                    return "main.exe";
                }

                if (whisperChoice == WhisperChoice.Cpp || Configuration.Settings.Tools.WhisperChoice == WhisperChoice.CppCuBlas)
                {
                    return "main.exe";
                }

                if (whisperChoice == WhisperChoice.WhisperX)
                {
                    return "whisperx.exe";
                }

                if (whisperChoice == WhisperChoice.CTranslate2)
                {
                    return "whisper-ctranslate2.exe";
                }

                if (whisperChoice == WhisperChoice.PurfviewFasterWhisper)
                {
                    return "whisper-faster.exe";
                }

                if (whisperChoice == WhisperChoice.PurfviewFasterWhisperXXL)
                {
                    return "faster-whisper-xxl.exe";
                }

                if (whisperChoice == WhisperChoice.StableTs)
                {
                    return "stable-ts.exe";
                }

                if (whisperChoice == WhisperChoice.ConstMe)
                {
                    return "main.exe";
                }

                return "whisper.exe";
            }

            if (Configuration.IsRunningOnLinux && whisperChoice == WhisperChoice.Cpp || Configuration.Settings.Tools.WhisperChoice == WhisperChoice.CppCuBlas)
            {
                return "main";
            }

            if (Configuration.IsRunningOnLinux && whisperChoice == WhisperChoice.WhisperX)
            {
                return "whisperx";
            }

            if (whisperChoice == WhisperChoice.CTranslate2)
            {
                return "whisper-ctranslate2";
            }

            if (whisperChoice == WhisperChoice.StableTs)
            {
                return "stable-ts";
            }

            return "whisper";
        }

        public static string GetWhisperPathAndFileName()
        {
            return GetWhisperPathAndFileName(Configuration.Settings.Tools.WhisperChoice);
        }

        /// <summary>
        /// Get installed path
        /// </summary>
        public static string GetWhisperPathAndFileName(string whisperChoice)
        {
            var fileNameOnly = GetExecutableFileName(whisperChoice);
            var whisperFolder = GetWhisperFolder(whisperChoice);
            if (string.IsNullOrEmpty(whisperFolder))
            {
                return fileNameOnly;
            }

            if (Configuration.IsRunningOnWindows)
            {
                if (whisperChoice == WhisperChoice.Cpp || whisperChoice == WhisperChoice.CppCuBlas)
                {
                    var f = Path.Combine(whisperFolder, fileNameOnly);
                    if (File.Exists(f))
                    {
                        return f;
                    }
                }
                else if (whisperChoice == WhisperChoice.WhisperX)
                {
                    var f = GetWhisperFolder(whisperChoice);
                    if (File.Exists(f))
                    {
                        return f;
                    }

                    f = Path.Combine(whisperFolder, fileNameOnly);
                    if (File.Exists(f))
                    {
                        return f;
                    }
                }
                else if (whisperChoice == WhisperChoice.CTranslate2)
                {
                    var f = Path.Combine(whisperFolder, fileNameOnly);
                    if (File.Exists(f))
                    {
                        return f;
                    }
                }
                else if (whisperChoice == WhisperChoice.PurfviewFasterWhisper || whisperChoice == WhisperChoice.PurfviewFasterWhisperXXL)
                {
                    var f = Path.Combine(whisperFolder, fileNameOnly);
                    if (File.Exists(f))
                    {
                        return f;
                    }
                }
                else if (whisperChoice == WhisperChoice.StableTs)
                {
                    var f = Path.Combine(whisperFolder, fileNameOnly);
                    if (File.Exists(f))
                    {
                        return f;
                    }
                }
                else if (whisperChoice == WhisperChoice.ConstMe)
                {
                    var f = Path.Combine(whisperFolder, fileNameOnly);
                    if (File.Exists(f))
                    {
                        return f;
                    }

                    return null;
                }
                else
                {
                    var f = Path.Combine(whisperFolder, fileNameOnly);
                    if (File.Exists(f))
                    {
                        return f;
                    }
                }
            }

            return fileNameOnly;
        }

        public static string GetWhisperModelForCmdLine(string model)
        {
            if (Configuration.Settings.Tools.WhisperChoice == WhisperChoice.Cpp || Configuration.Settings.Tools.WhisperChoice == WhisperChoice.CppCuBlas)
            {
                return Path.Combine(GetWhisperModel().ModelFolder, model + ModelExtension());
            }

            if (Configuration.Settings.Tools.WhisperChoice == WhisperChoice.ConstMe)
            {
                return Path.Combine(GetWhisperModel().ModelFolder, model + ModelExtension());
            }

            if (Configuration.Settings.Tools.WhisperChoice == WhisperChoice.WhisperX && model == "large-v3")
            {
                return "large";
            }

            return model;
        }

        public static string GetWhisperTranslateParameter()
        {
            return Configuration.Settings.Tools.WhisperChoice == WhisperChoice.Cpp ||
                   Configuration.Settings.Tools.WhisperChoice == WhisperChoice.CppCuBlas ||
                   Configuration.Settings.Tools.WhisperChoice == WhisperChoice.ConstMe
                ? "--translate "
                : "--task translate ";
        }
    }
}
