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

            var pythonFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "AppData",
                "Local",
                "Programs",
                "Python");

            if (!Directory.Exists(pythonFolder))
            {
                return false;
            }

            foreach (var dir in Directory.GetDirectories(pythonFolder))
            {
                var dirName = Path.GetFileName(dir);
                if (dirName != null && dirName.StartsWith("Python3"))
                {
                    var whisperFolder = Path.Combine(dir, "Lib", "site-packages", "whisper");
                    if (Directory.Exists(whisperFolder))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static string GetWhisperFolder()
        {
            if (!Configuration.IsRunningOnWindows)
            {
                return null;
            }

            var pythonFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "AppData",
                "Local",
                "Programs",
                "Python");

            if (!Directory.Exists(pythonFolder))
            {
                return null;
            }

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

            return null;
        }
    }
}
