using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes;
using Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream;
using Nikse.SubtitleEdit.Core.VobSub;
using SkiaSharp;
using System.Text;

namespace SeConv.Core;

/// <summary>
/// Loads image-based subtitle sources as a stream of pre-rendered bitmaps, bypassing
/// OCR. Used by the image-to-image conversion path (e.g. .sup → BDN-XML, VobSub → .sup)
/// so the original source pixels reach the output handler intact instead of being
/// OCR'd to text and re-rasterised at seconv's default font.
/// </summary>
internal static class BitmapSubtitleLoader
{
    /// <summary>
    /// One bitmap subtitle event. <see cref="ScreenWidth"/> / <see cref="ScreenHeight"/>
    /// are the source-format's declared video frame dimensions when known — pass them
    /// to the output handler so e.g. Blu-Ray 1920x1080 stays at 1920x1080 rather than
    /// being squashed into the CLI's default <c>--resolution</c>. Null = caller picks.
    /// </summary>
    public sealed record BitmapSubtitleItem(
        TimeCode StartTime,
        TimeCode EndTime,
        SKBitmap Bitmap,
        int? ScreenWidth = null,
        int? ScreenHeight = null) : IDisposable
    {
        public void Dispose() => Bitmap.Dispose();
    }

    /// <summary>
    /// Blu-Ray .sup file → bitmap events. PcsData times are 90 kHz ticks; divide
    /// by 90 for milliseconds. The PCS header's Width/Height carry the source video
    /// dimensions (typically 1920x1080 or 1280x720).
    /// </summary>
    public static IReadOnlyList<BitmapSubtitleItem> LoadBluRaySup(string filePath)
    {
        var log = new StringBuilder();
        var pcsList = BluRaySupParser.ParseBluRaySup(filePath, log);
        if (pcsList.Count == 0)
        {
            throw new InvalidOperationException($"No Blu-Ray sup subtitles found in: {filePath}");
        }
        return PcsListToItems(pcsList);
    }

    /// <summary>
    /// MKV PGS track (S_HDMV/PGS) → bitmap events. PGS-in-MKV is the same PCS payload
    /// as a .sup file, just wrapped in Matroska block timing.
    /// </summary>
    public static IReadOnlyList<BitmapSubtitleItem> LoadMatroskaPgs(MatroskaFile matroska, MatroskaTrackInfo track)
    {
        var pcsList = BluRaySupParser.ParseBluRaySupFromMatroska(track, matroska);
        if (pcsList.Count == 0)
        {
            throw new InvalidOperationException($"No PGS subtitles in MKV track #{track.TrackNumber}.");
        }
        return PcsListToItems(pcsList);
    }

    /// <summary>
    /// VobSub <c>.sub</c> (+ optional <c>.idx</c>) → bitmap events. Uses
    /// <see cref="VobSubParser.OpenSubIdx"/>, which uses the .idx for timing + palette when
    /// present and otherwise parses the .sub's MPEG-PS stream directly (stream PTS timing +
    /// a default palette). The VobSub spec doesn't store a screen size in the index file, so
    /// we bake in the DVD-standard frame sizes (720x576 PAL, 720x480 NTSC) — otherwise the
    /// output writer would fall back to <c>--resolution</c> / 1920x1080, which is wrong
    /// metadata for DVD sources.
    /// </summary>
    public static IReadOnlyList<BitmapSubtitleItem> LoadVobSub(string subPath, string idxPath, bool isPal)
    {
        var parser = new VobSubParser(isPal);
        // OpenSubIdx falls back to parsing the .sub stream directly when the .idx is missing,
        // so an absent companion is not fatal — see IsBinaryVobSub for the caller's gate.
        parser.OpenSubIdx(subPath, idxPath);
        var packs = parser.MergeVobSubPacks();
        if (packs.Count == 0)
        {
            throw new InvalidOperationException($"No VobSub subtitle packs in: {subPath}");
        }

        var screenWidth = 720;
        var screenHeight = isPal ? 576 : 480;

        var items = new List<BitmapSubtitleItem>(packs.Count);
        foreach (var pack in packs)
        {
            var bmp = pack.GetBitmap();
            if (bmp is null)
            {
                continue;
            }
            items.Add(new BitmapSubtitleItem(pack.StartTimeCode, pack.EndTimeCode, bmp, screenWidth, screenHeight));
        }
        return items;
    }

    /// <summary>
    /// True if the file begins with an MPEG-2 pack header (<c>00 00 01 BA</c>), i.e. it's a
    /// binary VobSub subpicture stream rather than a text MicroDVD <c>.sub</c>. Used to decide
    /// whether a <c>.sub</c> without an <c>.idx</c> companion is a VobSub (read it directly,
    /// with a warning) or a plain text subtitle (fall through to the text loader).
    /// </summary>
    public static bool IsBinaryVobSub(string filePath)
    {
        try
        {
            var header = new byte[4];
            using var fs = File.OpenRead(filePath);
            return fs.Read(header, 0, 4) == 4 && VobSubParser.IsMpeg2PackHeader(header);
        }
        catch
        {
            // I/O race / permissions — let the text loader try rather than hard-failing here.
            return false;
        }
    }

    /// <summary>
    /// VobSub track inside an MKV (<c>S_VOBSUB</c>) → bitmap events. The subpicture packets
    /// live in the Matroska blocks; the per-pack timing comes from the block Start/End (not
    /// the SubPicture's own delay, which only applies to standalone <c>.sub</c>+<c>.idx</c>).
    /// Mirrors the desktop batch converter's <c>LoadVobSubFromMatroska</c>; the palette is
    /// left to <see cref="SubPicture"/>'s default, matching the GUI's OCR path.
    /// </summary>
    public static IReadOnlyList<BitmapSubtitleItem> LoadMatroskaVobSub(MatroskaFile matroska, MatroskaTrackInfo track)
    {
        if (track.ContentEncodingType == 1)
        {
            throw new InvalidOperationException(
                $"VobSub MKV track #{track.TrackNumber} is compressed (content encoding 1), which isn't supported.");
        }

        var sub = matroska.GetSubtitle(track.TrackNumber, null);
        var packs = new List<VobSubMergedPack>(sub.Count);
        foreach (var p in sub)
        {
            packs.Add(new VobSubMergedPack(p.GetData(track), TimeSpan.FromMilliseconds(p.Start), 32, null)
            {
                EndTime = TimeSpan.FromMilliseconds(p.End),
            });

            // Fix overlapping time codes (some Handbrake versions emit them) by clamping the
            // previous pack's end to just before this one's start.
            if (packs.Count > 1 && packs[^2].EndTime > packs[^1].StartTime)
            {
                packs[^2].EndTime = TimeSpan.FromMilliseconds(packs[^1].StartTime.TotalMilliseconds - 1);
            }
        }

        var items = new List<BitmapSubtitleItem>(packs.Count);
        foreach (var pack in packs)
        {
            var bmp = pack.GetBitmap();
            if (bmp is null)
            {
                continue;
            }
            // Use the block-derived Start/End (TimeSpan), not StartTimeCode/EndTimeCode which
            // are based on the SubPicture delay and only correct for .sub+.idx sources.
            items.Add(new BitmapSubtitleItem(
                new TimeCode(pack.StartTime.TotalMilliseconds),
                new TimeCode(pack.EndTime.TotalMilliseconds),
                bmp));
        }
        if (items.Count == 0)
        {
            throw new InvalidOperationException($"No VobSub subtitles in MKV track #{track.TrackNumber}.");
        }
        return items;
    }

    /// <summary>
    /// VobSub track inside an MP4 (handler type <c>subp</c>, e.g. produced by MP4Box) →
    /// bitmap events. The decoded subpictures and their timing are parsed by libse's
    /// <see cref="Stbl"/>; index <c>i</c> of <c>SubPictures</c> lines up with paragraph
    /// <c>i</c>. Mirrors the desktop <c>OcrSubtitleMp4VobSub</c> (palette left to default).
    /// </summary>
    public static IReadOnlyList<BitmapSubtitleItem> LoadMp4VobSub(Trak track)
    {
        var paragraphs = track.Mdia.Minf.Stbl.GetParagraphs();
        var subPictures = track.Mdia.Minf.Stbl.SubPictures;
        var count = Math.Min(paragraphs.Count, subPictures.Count);

        var items = new List<BitmapSubtitleItem>(count);
        for (var i = 0; i < count; i++)
        {
            var bmp = subPictures[i].GetBitmap(
                null, SKColors.Transparent, SKColors.Black, SKColors.White, SKColors.Black, false);
            if (bmp is null)
            {
                continue;
            }
            items.Add(new BitmapSubtitleItem(paragraphs[i].StartTime, paragraphs[i].EndTime, bmp));
        }
        if (items.Count == 0)
        {
            throw new InvalidOperationException("No VobSub subpictures found in MP4 track.");
        }
        return items;
    }

    /// <summary>
    /// Transport stream DVB-sub → one bitmap list per packet ID. Caller is responsible
    /// for routing each PID to its own output file (multiple subtitle streams =
    /// multiple languages, same as the OCR path in <see cref="ImageOcrLoader"/>).
    /// </summary>
    public static List<(IReadOnlyList<BitmapSubtitleItem> Items, int PacketId)> LoadTransportStreamDvbSub(string filePath)
    {
        var parser = new TransportStreamParser();
        parser.Parse(filePath, null);

        var results = new List<(IReadOnlyList<BitmapSubtitleItem>, int)>();
        if (parser.SubtitlePacketIds.Count == 0)
        {
            return results;
        }

        foreach (var pid in parser.SubtitlePacketIds)
        {
            var dvbSubtitles = parser.GetDvbSubtitles(pid);
            if (dvbSubtitles.Count == 0)
            {
                continue;
            }

            var items = new List<BitmapSubtitleItem>(dvbSubtitles.Count);
            foreach (var dvb in dvbSubtitles)
            {
                var bmp = dvb.GetBitmap();
                if (bmp is null)
                {
                    continue;
                }
                items.Add(new BitmapSubtitleItem(
                    new TimeCode(dvb.StartMilliseconds),
                    new TimeCode(dvb.EndMilliseconds),
                    bmp));
            }
            if (items.Count > 0)
            {
                results.Add((items, pid));
            }
        }
        return results;
    }

    private static IReadOnlyList<BitmapSubtitleItem> PcsListToItems(IReadOnlyList<BluRaySupParser.PcsData> pcsList)
    {
        // Take width/height from the first PCS that reports them. Blu-Ray streams
        // typically declare a single video resolution per epoch; the value is
        // baked into the PCS header so every entry should agree.
        var firstSize = pcsList[0].GetScreenSize();
        var screenWidth = firstSize.Width > 0 ? (int)firstSize.Width : (int?)null;
        var screenHeight = firstSize.Height > 0 ? (int)firstSize.Height : (int?)null;

        var items = new List<BitmapSubtitleItem>(pcsList.Count);
        foreach (var pcs in pcsList)
        {
            var bmp = pcs.GetBitmap();
            if (bmp is null)
            {
                continue;
            }
            items.Add(new BitmapSubtitleItem(pcs.StartTimeCode, pcs.EndTimeCode, bmp, screenWidth, screenHeight));
        }
        return items;
    }
}
