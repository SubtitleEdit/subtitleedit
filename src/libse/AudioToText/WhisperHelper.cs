using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Nikse.SubtitleEdit.Core.AudioToText
{
    public static class WhisperHelper
    {
        public static bool IsWhisperInstalled()
        {
            if (Directory.Exists(WhisperModel.ModelFolder))
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
    }
}
