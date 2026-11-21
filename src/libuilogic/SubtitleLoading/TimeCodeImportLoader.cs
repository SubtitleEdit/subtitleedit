using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Core.VobSub;
using SkiaSharp;
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

        var vobSub = TryLoadVobSub(fileName);
        if (vobSub != null)
        {
            // no language picker here - the main window asks when there are more streams
            return MakeSubtitle(vobSub.Streams[vobSub.LanguageStreamIds[0]]);
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

    /// <summary>
    /// The packs of a VobSub .sub/.idx, per stream (language).
    /// </summary>
    public sealed class VobSubStreams
    {
        public Dictionary<int, List<VobSubMergedPack>> Streams { get; } = new();

        /// <summary>Stream ids with real subtitles, in file order - more than one means the user picks a language.</summary>
        public List<int> LanguageStreamIds { get; } = new();

        public List<SKColor> Palette { get; set; } = new();
        public List<string> Languages { get; set; } = new();
    }

    /// <summary>
    /// Reads a VobSub .sub (with its .idx next to it) without OCR. Null when the file is not VobSub
    /// or has no subtitles.
    /// </summary>
    public static VobSubStreams? TryLoadVobSub(string fileName)
    {
        if (!Path.GetExtension(fileName).Equals(".sub", StringComparison.OrdinalIgnoreCase) || !FileUtil.IsVobSub(fileName))
        {
            return null;
        }

        var parser = new VobSubParser(true);
        parser.OpenSubIdx(fileName, Path.ChangeExtension(fileName, ".idx"));
        var packs = parser.MergeVobSubPacks();
        parser.VobSubPacks.Clear();

        var result = new VobSubStreams
        {
            Palette = parser.IdxPalette,
            Languages = parser.IdxLanguages,
        };

        // same stream rule as the main window's VobSub OCR import
        foreach (var pack in packs)
        {
            if (pack.SubPicture.Delay.TotalMilliseconds > 500 && !result.LanguageStreamIds.Contains(pack.StreamId))
            {
                result.LanguageStreamIds.Add(pack.StreamId);
            }

            if (!result.Streams.TryGetValue(pack.StreamId, out var list))
            {
                list = new List<VobSubMergedPack>();
                result.Streams.Add(pack.StreamId, list);
            }

            list.Add(pack);
        }

        return result.LanguageStreamIds.Count > 0 ? result : null;
    }

    public static Subtitle MakeSubtitle(List<VobSubMergedPack> packs)
    {
        var subtitle = new Subtitle();
        foreach (var pack in packs)
        {
            subtitle.Paragraphs.Add(new Paragraph(string.Empty, pack.StartTime.TotalMilliseconds, pack.EndTime.TotalMilliseconds));
        }

        return subtitle;
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
