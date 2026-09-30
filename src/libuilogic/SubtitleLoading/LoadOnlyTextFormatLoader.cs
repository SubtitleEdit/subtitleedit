using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.UiLogic.SubtitleLoading;

/// <summary>
/// Opens the load-only text formats (Captionate, WSB, FTE, the JSON "load only" variants, ...).
/// They live in <see cref="SubtitleFormat.GetTextOtherFormats"/>, which Subtitle.Parse never
/// tries, so without this fallback File > Open reported them as unknown - SE 4 tried them last.
/// </summary>
public static class LoadOnlyTextFormatLoader
{
    public static Subtitle? TryLoad(string fileName, Encoding encoding)
    {
        List<string> lines;
        try
        {
            lines = FileUtil.ReadAllLinesShared(fileName, encoding);
        }
        catch
        {
            return null;
        }

        return TryLoad(lines, fileName);
    }

    public static Subtitle? TryLoad(List<string> lines, string fileName)
    {
        foreach (var format in SubtitleFormat.GetTextOtherFormats())
        {
            // Image-list formats need OCR; loaded as text they fill the grid with png file names.
            if (IsImageListFormat(format))
            {
                continue;
            }

            // These parsers only ever ran as SE 4's last resort; one that throws on a file it
            // does not understand must not abort File > Open.
            try
            {
                if (format.IsMine(lines, fileName))
                {
                    var subtitle = new Subtitle();
                    format.LoadSubtitle(subtitle, lines, fileName);
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

    /// <summary>
    /// Formats whose cues name image files: they need OCR, never a text load.
    /// </summary>
    public static bool IsImageListFormat(SubtitleFormat format)
    {
        return format is BdnXml or Dost or FinalCutProImage or SeImageHtmlIndex or SpuImage or TimedImagesXml or TimedTextImage;
    }
}
