using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System.Text;

namespace Nikse.SubtitleEdit.UiLogic.SubtitleLoading;

/// <summary>
/// Loads the subtitle that File > Import > Time codes takes its time codes from. Only the
/// timings are used, so image-based files need no OCR: a Blu-ray sup gives the time codes of
/// its pictures, a BDN xml those of its png files (SE 4 read sup directly, SE 5 only tried the
/// text formats and said "Unknown subtitle format" - issue #17089).
/// </summary>
public static class TimeCodeImportLoader
{
    /// <returns>The subtitle with the time codes (texts may be empty or image file names), or null when the file is not a known format.</returns>
    public static Subtitle? TryLoad(string fileName)
    {
        if (!File.Exists(fileName))
        {
            return null;
        }

        var fileSize = new FileInfo(fileName).Length;
        var ext = Path.GetExtension(fileName).ToLowerInvariant();

        // by content too, like File > Open: a Blu-ray sup renamed to .sub is still a sup
        if ((ext == ".sup" && FileUtil.IsBluRaySup(fileName)) ||
            (ext != ".sup" && fileSize > 13 && FileUtil.IsBluRaySupByContent(fileName)))
        {
            var bluRaySubtitle = LoadBluRaySup(fileName);
            if (bluRaySubtitle != null)
            {
                return bluRaySubtitle;
            }
        }

        var subtitle = Subtitle.Parse(fileName);
        if (subtitle != null && subtitle.Paragraphs.Count > 0)
        {
            return subtitle;
        }

        if (ext == ".xml")
        {
            subtitle = ImageListSubtitleLoader.TryLoadImageListXml(fileName);
            if (subtitle != null)
            {
                return subtitle;
            }
        }

        subtitle = LoadBinaryFormat(fileName)
                   ?? NonRegisteredFormatLoader.TryLoadAribB36(fileName)
                   ?? LoadOnlyTextFormatLoader.TryLoad(fileName, LanguageAutoDetect.GetEncodingFromFile(fileName));

        return subtitle != null && subtitle.Paragraphs.Count > 0 ? subtitle : null;
    }

    private static Subtitle? LoadBluRaySup(string fileName)
    {
        var pcsData = BluRaySupParser.ParseBluRaySup(fileName, new StringBuilder());
        if (pcsData.Count == 0)
        {
            return null;
        }

        var subtitle = new Subtitle();
        foreach (var item in pcsData)
        {
            subtitle.Paragraphs.Add(new Paragraph(item.StartTimeCode, item.EndTimeCode, string.Empty));
        }

        return subtitle;
    }

    /// <summary>
    /// EBU STL, PAC, Cavena 890, Cheetah caption, ... - SE 4's import time codes tried these too.
    /// </summary>
    private static Subtitle? LoadBinaryFormat(string fileName)
    {
        foreach (var format in SubtitleFormat.GetBinaryFormats(true))
        {
            try
            {
                if (format.IsMine(null, fileName))
                {
                    var subtitle = new Subtitle();
                    format.LoadSubtitle(subtitle, null, fileName);
                    if (subtitle.Paragraphs.Count > 0)
                    {
                        subtitle.OriginalFormat = format;
                        return subtitle;
                    }
                }
            }
            catch
            {
                // not this format
            }
        }

        return null;
    }
}
