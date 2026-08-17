using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Chapters;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Logic.Media;

/// <summary>
/// Keeps edited chapters next to Subtitle Edit's data rather than in the video file, so chapter work
/// survives closing the video without the video itself having to be rewritten.
/// </summary>
/// <remarks>
/// Stored as Matroska chapter XML: the sidecar doubles as a file that can be handed straight to
/// mkvmerge.
/// </remarks>
public static class ChaptersHelper
{
    private const string Extension = ".chapters.xml";

    private static string GetChaptersFileName(string videoFileName)
    {
        var dir = Se.ChaptersFolder;
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        var videoFileNameWithoutExtension = Path.GetFileNameWithoutExtension(videoFileName)
            .Replace(".", string.Empty)
            .Replace("_", string.Empty);
        if (videoFileNameWithoutExtension.Length > 25)
        {
            videoFileNameWithoutExtension = videoFileNameWithoutExtension.Substring(0, 25);
        }

        return Path.Combine(dir, $"{MovieHasher.GenerateHash(videoFileName)}_{videoFileNameWithoutExtension}{Extension}");
    }

    public static List<Chapter> FromDisk(string videoFileName)
    {
        if (string.IsNullOrEmpty(videoFileName))
        {
            return new List<Chapter>();
        }

        try
        {
            var fileName = GetChaptersFileName(videoFileName);
            if (!File.Exists(fileName))
            {
                return new List<Chapter>();
            }

            return MatroskaChaptersXml.ParseChapters(File.ReadAllText(fileName));
        }
        catch
        {
            return new List<Chapter>();
        }
    }

    public static void SaveChapters(string videoFileName, IList<Chapter> chapters)
    {
        if (string.IsNullOrEmpty(videoFileName))
        {
            return;
        }

        try
        {
            File.WriteAllText(
                GetChaptersFileName(videoFileName),
                MatroskaChaptersXml.ToXml(chapters, "und"),
                new UTF8Encoding(false));
        }
        catch
        {
            // Losing the sidecar is not worth interrupting the user for; the chapters are still
            // in the session.
        }
    }

    public static void DeleteChapters(string videoFileName)
    {
        if (string.IsNullOrEmpty(videoFileName))
        {
            return;
        }

        try
        {
            var fileName = GetChaptersFileName(videoFileName);
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
        }
        catch
        {
            // Nothing useful to do if the file will not go away.
        }
    }
}
