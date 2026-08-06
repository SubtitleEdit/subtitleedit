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
    /// <para>
    /// <see cref="Position"/> is the bitmap's top-left corner in the source frame. Image
    /// sources carry it per event (a top-positioned caption, a sign at the left edge), and
    /// without it every re-export was re-centred at the bottom of the frame.
    /// </para>
    /// </summary>
    public sealed record BitmapSubtitleItem(
        TimeCode StartTime,
        TimeCode EndTime,
        SKBitmap Bitmap,
        int? ScreenWidth = null,
        int? ScreenHeight = null,
        SKPointI? Position = null) : IDisposable
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
    /// a default palette). The frame size comes from the .idx's <c>size:</c> line when it has
    /// one — VobSub is not always DVD-sized (Subtitle Edit's own export writes whatever the
    /// source was, e.g. <c>size: 1920x1080</c>) and squeezing a 1920x1080 stream into a DVD
    /// frame moves every subpicture. Without that line, fall back to the DVD standards
    /// (720x576 PAL, 720x480 NTSC) rather than <c>--resolution</c> / 1920x1080.
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

        var (screenWidth, screenHeight) = TryGetVobSubIdxScreenSize(ReadIdxText(idxPath))
                                          ?? (720, isPal ? 576 : 480);

        var items = new List<BitmapSubtitleItem>(packs.Count);
        var skipped = 0;
        foreach (var pack in packs)
        {
            // Without the .idx CLUT, SubPicture.GetBitmap skips both the SetColor and
            // SetContrast (per-plane alpha) display commands, so normally-transparent
            // outline/anti-alias planes render opaque and fuse into the glyphs — the
            // garbled OCR in issue #12772 ('-'→'=', 'S'→'$'). Mirrors the GUI, which
            // always passes VobSubParser.IdxPalette.
            if (parser.IdxPalette.Count > 0)
            {
                pack.Palette = parser.IdxPalette;
            }

            var bmp = pack.GetBitmap();
            if (bmp is null)
            {
                skipped++;
                continue;
            }

            // Use the EndTime *property*, not EndTimeCode: many discs omit the subpicture's
            // own delay command, so EndTimeCode is start + 0 (issue #12772 part 3: 740 of
            // 1115 zero-duration cues in the .sup export). MergeVobSubPacks has already
            // repaired EndTime for missing/negative/over-long delays — the same times the
            // GUI shows.
            // GetPosition reads SubPicture.ImageDisplayArea, which is only filled in while
            // decoding - so after GetBitmap above, never before.
            var position = pack.GetPosition();
            items.Add(new BitmapSubtitleItem(
                new TimeCode(pack.StartTime.TotalMilliseconds),
                new TimeCode(pack.EndTime.TotalMilliseconds),
                bmp,
                screenWidth,
                screenHeight,
                new SKPointI(position.Left, position.Top)));
        }
        WarnSkippedBitmaps(skipped, "VobSub");
        return items;
    }

    /// <summary>
    /// Issue #12772 part 3: undecodable subpictures used to vanish silently, making the
    /// output look like it had fewer subtitles than the GUI sees. Surface the count.
    /// </summary>
    private static void WarnSkippedBitmaps(int skipped, string what)
    {
        if (skipped > 0)
        {
            Spectre.Console.AnsiConsole.MarkupLine(
                $"[yellow]Note: {skipped} {what} subpicture(s) could not be decoded to a bitmap and were dropped.[/]");
        }
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

        // The track's CodecPrivate holds the .idx text, including the CLUT palette. Without
        // it SubPicture.GetBitmap skips SetColor/SetContrast and renders normally-transparent
        // planes opaque (issue #12772) — same fix as LoadVobSub; mirrors the GUI MKV path.
        var codecPrivate = track.GetCodecPrivate();
        var palette = GetVobSubIdxPalette(codecPrivate);
        var (screenWidth, screenHeight) = GetVobSubIdxScreenSize(codecPrivate);

        var sub = matroska.GetSubtitle(track.TrackNumber, null);
        var packs = new List<VobSubMergedPack>(sub.Count);
        foreach (var p in sub)
        {
            packs.Add(new VobSubMergedPack(p.GetData(track), TimeSpan.FromMilliseconds(p.Start), 32, null)
            {
                EndTime = TimeSpan.FromMilliseconds(p.End),
                Palette = palette,
            });

            // Fix overlapping time codes (some Handbrake versions emit them) by clamping the
            // previous pack's end to just before this one's start.
            if (packs.Count > 1 && packs[^2].EndTime > packs[^1].StartTime)
            {
                packs[^2].EndTime = TimeSpan.FromMilliseconds(packs[^1].StartTime.TotalMilliseconds - 1);
            }
        }

        var items = new List<BitmapSubtitleItem>(packs.Count);
        var skipped = 0;
        foreach (var pack in packs)
        {
            var bmp = pack.GetBitmap();
            if (bmp is null)
            {
                skipped++;
                continue;
            }
            // Use the block-derived Start/End (TimeSpan), not StartTimeCode/EndTimeCode which
            // are based on the SubPicture delay and only correct for .sub+.idx sources.
            // Screen size comes from the CodecPrivate "size:" line so an image output
            // (e.g. bluraysup) keeps the DVD frame instead of defaulting to 1920x1080.
            var position = pack.GetPosition();
            items.Add(new BitmapSubtitleItem(
                new TimeCode(pack.StartTime.TotalMilliseconds),
                new TimeCode(pack.EndTime.TotalMilliseconds),
                bmp,
                screenWidth,
                screenHeight,
                new SKPointI(position.Left, position.Top)));
        }
        WarnSkippedBitmaps(skipped, "MKV VobSub");
        if (items.Count == 0)
        {
            throw new InvalidOperationException($"No VobSub subtitles in MKV track #{track.TrackNumber}.");
        }
        return items;
    }

    /// <summary>
    /// Video frame size from a Matroska VobSub track's CodecPrivate .idx text ("size: 720x576"),
    /// or the DVD PAL default when the line is missing or malformed.
    /// </summary>
    internal static (int Width, int Height) GetVobSubIdxScreenSize(string? codecPrivate)
    {
        return TryGetVobSubIdxScreenSize(codecPrivate) ?? (720, 576);
    }

    /// <summary>
    /// Frame size from .idx text ("size: 720x576"), or null when it carries no usable
    /// <c>size:</c> line — the caller then picks its own default (DVD PAL vs NTSC differ,
    /// so there is no single right fallback here).
    /// </summary>
    internal static (int Width, int Height)? TryGetVobSubIdxScreenSize(string? idxText)
    {
        if (string.IsNullOrWhiteSpace(idxText))
        {
            return null;
        }

        foreach (var line in idxText.SplitToLines())
        {
            if (line.StartsWith("size:", StringComparison.OrdinalIgnoreCase))
            {
                var parts = line.Substring(5).Trim().Split('x');
                if (parts.Length == 2 &&
                    int.TryParse(parts[0].Trim(), out var w) && w > 0 &&
                    int.TryParse(parts[1].Trim(), out var h) && h > 0)
                {
                    return (w, h);
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Text of a standalone <c>.idx</c>, or null when there is none to read — an absent or
    /// unreadable companion is normal (<see cref="VobSubParser.OpenSubIdx"/> copes), so it
    /// must not fail the conversion.
    /// </summary>
    private static string? ReadIdxText(string? idxPath)
    {
        if (string.IsNullOrEmpty(idxPath) || !File.Exists(idxPath))
        {
            return null;
        }

        try
        {
            return File.ReadAllText(idxPath);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Palette (CLUT) from a Matroska VobSub track's CodecPrivate .idx text, or null when the
    /// track carries none — <see cref="SubPicture.GetBitmap"/> then falls back to its defaults.
    /// </summary>
    internal static List<SKColor>? GetVobSubIdxPalette(string? codecPrivate)
    {
        if (string.IsNullOrWhiteSpace(codecPrivate))
        {
            return null;
        }

        var palette = new Idx(codecPrivate.SplitToLines()).Palette;
        return palette.Count > 0 ? palette : null;
    }

    /// <summary>
    /// VobSub track inside an MP4 (handler type <c>subp</c>, e.g. produced by MP4Box) →
    /// bitmap events. The decoded subpictures and their timing are parsed by libse's
    /// <see cref="Stbl"/>; index <c>i</c> of <c>SubPictures</c> lines up with paragraph
    /// <c>i</c>. Mirrors the desktop <c>OcrSubtitleMp4VobSub</c>, including the CLUT from
    /// the sample entry when the track has one.
    /// </summary>
    public static IReadOnlyList<BitmapSubtitleItem> LoadMp4VobSub(Trak track)
    {
        var paragraphs = track.Mdia.Minf.Stbl.GetParagraphs();
        var subPictures = track.Mdia.Minf.Stbl.SubPictures;
        var palette = track.Mdia.Minf.Stbl.VobSubPalette;
        var count = Math.Min(paragraphs.Count, subPictures.Count);

        var items = new List<BitmapSubtitleItem>(count);
        for (var i = 0; i < count; i++)
        {
            var bmp = subPictures[i].GetBitmap(
                palette, SKColors.Transparent, SKColors.Black, SKColors.White, SKColors.Black, false);
            if (bmp is null)
            {
                continue;
            }
            // ImageDisplayArea is filled in by GetBitmap above.
            var displayArea = subPictures[i].ImageDisplayArea;
            items.Add(new BitmapSubtitleItem(paragraphs[i].StartTime, paragraphs[i].EndTime, bmp,
                Position: new SKPointI(displayArea.Left, displayArea.Top)));
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
                var position = dvb.GetPosition();
                items.Add(new BitmapSubtitleItem(
                    new TimeCode(dvb.StartMilliseconds),
                    new TimeCode(dvb.EndMilliseconds),
                    bmp,
                    Position: new SKPointI(position.Left, position.Top)));
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
            var position = pcs.GetPosition();
            items.Add(new BitmapSubtitleItem(pcs.StartTimeCode, pcs.EndTimeCode, bmp, screenWidth, screenHeight,
                new SKPointI(position.Left, position.Top)));
        }
        return items;
    }
}
