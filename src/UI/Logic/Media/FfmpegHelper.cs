using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;
using System.IO;
using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.Logic.Media;

public static class FfmpegHelper
{
    public static bool IsFfmpegInstalled()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return true;
        }

        Configuration.Settings.General.UseFFmpegForWaveExtraction = true;        
        return File.Exists(Se.Settings.General.FfmpegPath);
    }
}
