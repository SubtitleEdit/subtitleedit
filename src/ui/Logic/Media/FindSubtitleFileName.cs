using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System.IO;
using System.Linq;

namespace Nikse.SubtitleEdit.Logic.Media;

public static class FindSubtitleFileName
{
    public static bool TryFindSubtitleFileName(string videoFileName, out string subtitleFileName)
    {
        subtitleFileName = string.Empty;

        if (string.IsNullOrEmpty(videoFileName))
        {
            return false;
        }

        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(videoFileName);
        foreach (var extension in SubtitleFormat.AllSubtitleFormats.Select(p => p.Extension).Distinct())
        {
            var fileName = fileNameWithoutExtension + extension;
            if (File.Exists(fileName))
            {
                subtitleFileName = fileName;
                return true;
            }
        }

        var index = fileNameWithoutExtension.LastIndexOf('.');
        if (index > 0 && TryFindSubtitleFileName(fileNameWithoutExtension.Remove(index), out subtitleFileName))
        {
            return true;
        }

        index = fileNameWithoutExtension.LastIndexOf('_');
        if (index > 0 && TryFindSubtitleFileName(fileNameWithoutExtension.Remove(index), out subtitleFileName))
        {
            return true;
        }

        return false;
    }
}
