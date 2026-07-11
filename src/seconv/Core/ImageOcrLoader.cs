using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes;
using Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream;
using SkiaSharp;
using Spectre.Console;
using System.Text;
using LibSeParagraph = Nikse.SubtitleEdit.Core.Common.Paragraph;

namespace SeConv.Core;

/// <summary>
/// OCR-driven loaders for image-based subtitle sources. Each method returns a Subtitle
/// where each paragraph's text was recognised by Tesseract from the source's bitmap stream.
/// </summary>
internal static class ImageOcrLoader
{
    /// <summary>
    /// Blu-Ray .sup → text via the configured OCR engine. When
    /// <see cref="ConversionOptions.TimeCodesOnly"/> is set, OCR is skipped entirely and
    /// each entry keeps its timing with empty text — no OCR engine is even created.
    /// </summary>
    public static Subtitle LoadBluRaySup(string filePath, ConversionOptions options)
    {
        var log = new StringBuilder();
        var pcsList = BluRaySupParser.ParseBluRaySup(filePath, log);
        if (pcsList.Count == 0)
        {
            throw new InvalidOperationException($"No Blu-Ray sup subtitles found in: {filePath}");
        }

        if (options.TimeCodesOnly)
        {
            AnsiConsole.MarkupLine($"[dim]Extracting time codes from {pcsList.Count} Blu-Ray sup image(s) (no OCR)...[/]");
            return PcsListToSubtitle(pcsList, null);
        }

        using var ocr = OcrEngineFactory.Create(options);
        var isolationNote = options.PgsIsolateColors ? string.Empty : " (colour isolation off)";
        AnsiConsole.MarkupLine($"[dim]Running {ocr.Name} OCR on {pcsList.Count} Blu-Ray sup image(s){isolationNote}...[/]");
        return PcsListToSubtitle(pcsList, ocr, options.PgsIsolateColors);
    }

    /// <summary>
    /// MKV PGS track (S_HDMV/PGS) → text via the configured OCR engine, or time codes only
    /// when <see cref="ConversionOptions.TimeCodesOnly"/> is set.
    /// </summary>
    public static Subtitle LoadMatroskaPgs(MatroskaFile matroska, MatroskaTrackInfo track, ConversionOptions options)
    {
        var pcsList = BluRaySupParser.ParseBluRaySupFromMatroska(track, matroska);
        if (pcsList.Count == 0)
        {
            throw new InvalidOperationException($"No PGS subtitles in MKV track #{track.TrackNumber}.");
        }

        if (options.TimeCodesOnly)
        {
            AnsiConsole.MarkupLine($"[dim]Extracting time codes from {pcsList.Count} MKV PGS image(s) (track #{track.TrackNumber}, no OCR)...[/]");
            return PcsListToSubtitle(pcsList, null);
        }

        using var ocr = OcrEngineFactory.Create(options);
        var isolationNote = options.PgsIsolateColors ? string.Empty : " (colour isolation off)";
        AnsiConsole.MarkupLine($"[dim]Running {ocr.Name} OCR on {pcsList.Count} MKV PGS image(s) (track #{track.TrackNumber}){isolationNote}...[/]");
        return PcsListToSubtitle(pcsList, ocr, options.PgsIsolateColors);
    }

    /// <summary>
    /// Transport stream DVB-sub → text via the configured OCR engine, or time codes only
    /// when <see cref="ConversionOptions.TimeCodesOnly"/> is set. Returns one Subtitle per
    /// packet ID that has subtitles. Teletext PIDs are not handled here (they're already
    /// text; see <see cref="ContainerSubtitleLoader"/>).
    /// </summary>
    public static List<(Subtitle Subtitle, int PacketId)> LoadTransportStreamDvbSub(string filePath, ConversionOptions options)
    {
        var parser = new TransportStreamParser();
        parser.Parse(filePath, null);

        var results = new List<(Subtitle, int)>();
        if (parser.SubtitlePacketIds.Count == 0)
        {
            return results;
        }

        // Time-codes-only needs no recognition, so don't create (or require) an OCR engine.
        IOcrEngine? ocr = options.TimeCodesOnly ? null : OcrEngineFactory.Create(options);
        try
        {
            foreach (var pid in parser.SubtitlePacketIds)
            {
                var dvbSubtitles = parser.GetDvbSubtitles(pid);
                if (dvbSubtitles.Count == 0)
                {
                    continue;
                }

                AnsiConsole.MarkupLine(ocr is null
                    ? $"[dim]Extracting time codes from {dvbSubtitles.Count} DVB-sub image(s) (PID {pid}, no OCR)...[/]"
                    : $"[dim]Running {ocr.Name} OCR on {dvbSubtitles.Count} DVB-sub image(s) (PID {pid})...[/]");
                var subtitle = new Subtitle();
                foreach (var dvb in dvbSubtitles)
                {
                    var bitmap = dvb.GetBitmap();
                    if (bitmap is null)
                    {
                        continue;
                    }
                    try
                    {
                        // ocr == null → time-codes-only: keep the entry with empty text.
                        string text;
                        if (ocr is null)
                        {
                            text = string.Empty;
                        }
                        else if (options.PgsIsolateColors)
                        {
                            // Same antialiased binarisation as the PGS path (issue #12291).
                            using var isolated = VobSubColorIsolation.BinarizeForOcr(bitmap);
                            text = ocr.Recognize(isolated);
                        }
                        else
                        {
                            text = ocr.Recognize(bitmap);
                        }
                        if (ocr is null || !string.IsNullOrWhiteSpace(text))
                        {
                            subtitle.Paragraphs.Add(new LibSeParagraph(text, (double)dvb.StartMilliseconds, (double)dvb.EndMilliseconds));
                        }
                    }
                    finally
                    {
                        bitmap.Dispose();
                    }
                }
                subtitle.Renumber();
                if (subtitle.Paragraphs.Count > 0)
                {
                    results.Add((subtitle, pid));
                }
            }
        }
        finally
        {
            ocr?.Dispose();
        }
        return results;
    }

    /// <summary>
    /// VobSub <c>.sub</c> + <c>.idx</c> pair → text via the configured OCR engine, or time
    /// codes only when <see cref="ConversionOptions.TimeCodesOnly"/> is set.
    /// </summary>
    public static Subtitle LoadVobSub(string subPath, string idxPath, ConversionOptions options)
    {
        // IsPal default mirrors BitmapSubtitleLoader / VobSubExtractor — a wrong guess only
        // affects timing scale, which doesn't matter for OCR/time-code extraction.
        var items = BitmapSubtitleLoader.LoadVobSub(subPath, idxPath, isPal: true);
        return OcrBitmapItems(items, options, $"{items.Count} VobSub image(s)");
    }

    /// <summary>
    /// VobSub MKV track (<c>S_VOBSUB</c>) → text via the configured OCR engine, or time
    /// codes only when <see cref="ConversionOptions.TimeCodesOnly"/> is set.
    /// </summary>
    public static Subtitle LoadMatroskaVobSub(MatroskaFile matroska, MatroskaTrackInfo track, ConversionOptions options)
    {
        var items = BitmapSubtitleLoader.LoadMatroskaVobSub(matroska, track);
        return OcrBitmapItems(items, options, $"{items.Count} MKV VobSub image(s) (track #{track.TrackNumber})");
    }

    /// <summary>
    /// VobSub MP4 track (handler <c>subp</c>) → text via the configured OCR engine, or time
    /// codes only when <see cref="ConversionOptions.TimeCodesOnly"/> is set.
    /// </summary>
    public static Subtitle LoadMp4VobSub(Trak track, ConversionOptions options)
    {
        var items = BitmapSubtitleLoader.LoadMp4VobSub(track);
        return OcrBitmapItems(items, options, $"{items.Count} MP4 VobSub image(s)");
    }

    /// <summary>
    /// Shared driver for the VobSub sources: recognises each pre-decoded bitmap to text
    /// (or keeps timing with empty text in time-codes-only mode), disposing the bitmaps
    /// afterwards. The OCR engine is only created when recognition is actually needed.
    /// </summary>
    private static Subtitle OcrBitmapItems(
        IReadOnlyList<BitmapSubtitleLoader.BitmapSubtitleItem> items, ConversionOptions options, string what)
    {
        try
        {
            if (options.TimeCodesOnly)
            {
                AnsiConsole.MarkupLine($"[dim]Extracting time codes from {what} (no OCR)...[/]");
                return BitmapItemsToSubtitle(items, null);
            }

            using var ocr = OcrEngineFactory.Create(options);
            var isolationNote = options.VobSubIsolateColors ? string.Empty : " (colour isolation off)";
            AnsiConsole.MarkupLine($"[dim]Running {ocr.Name} OCR on {what}{isolationNote}...[/]");
            return BitmapItemsToSubtitle(items, ocr, options.VobSubIsolateColors);
        }
        finally
        {
            foreach (var item in items)
            {
                item.Dispose();
            }
        }
    }

    /// <summary>
    /// Turns pre-decoded bitmap events into a Subtitle. <paramref name="ocr"/> null =
    /// time-codes-only (empty text kept); non-null = recognise each bitmap and drop blanks.
    /// When <paramref name="isolateColors"/> is set, each bitmap is binarised via
    /// <see cref="VobSubColorIsolation"/> before recognition (VobSub sources only, and never
    /// in time-codes-only mode since no recognition happens there).
    /// </summary>
    private static Subtitle BitmapItemsToSubtitle(
        IReadOnlyList<BitmapSubtitleLoader.BitmapSubtitleItem> items, IOcrEngine? ocr, bool isolateColors = false)
    {
        var subtitle = new Subtitle();
        foreach (var item in items)
        {
            string text;
            if (ocr is null)
            {
                text = string.Empty;
            }
            else if (isolateColors)
            {
                using var isolated = VobSubColorIsolation.Isolate(item.Bitmap);
                text = ocr.Recognize(isolated);
            }
            else
            {
                text = ocr.Recognize(item.Bitmap);
            }

            if (ocr is null || !string.IsNullOrWhiteSpace(text))
            {
                subtitle.Paragraphs.Add(new LibSeParagraph(
                    text, item.StartTime.TotalMilliseconds, item.EndTime.TotalMilliseconds));
            }
        }
        subtitle.Renumber();
        return subtitle;
    }

    /// <summary>
    /// Turns a PCS list into a Subtitle. When <paramref name="ocr"/> is non-null each
    /// bitmap is recognised to text; when it's null (time-codes-only mode) every entry is
    /// kept with empty text so the output carries timing but no recognised characters.
    /// Entries whose bitmap is null (e.g. clear-screen commands) are skipped in both modes.
    /// </summary>
    private static Subtitle PcsListToSubtitle(List<BluRaySupParser.PcsData> pcsList, IOcrEngine? ocr, bool isolateColors = false)
    {
        var subtitle = new Subtitle();

        foreach (var pcs in pcsList)
        {
            var bitmap = pcs.GetBitmap();
            if (bitmap is null)
            {
                continue;
            }
            try
            {
                string text;
                if (ocr is null)
                {
                    text = string.Empty;
                }
                else if (isolateColors)
                {
                    // PGS glyphs are white fill + black outline on transparency; binarise so
                    // the fill survives the opaque white OCR canvas (issue #12291).
                    using var isolated = VobSubColorIsolation.BinarizeForOcr(bitmap);
                    text = ocr.Recognize(isolated);
                }
                else
                {
                    text = ocr.Recognize(bitmap);
                }
                if (ocr is null || !string.IsNullOrWhiteSpace(text))
                {
                    subtitle.Paragraphs.Add(new LibSeParagraph(text, pcs.StartTime / 90.0, pcs.EndTime / 90.0));
                }
            }
            finally
            {
                bitmap.Dispose();
            }
        }
        subtitle.Renumber();
        return subtitle;
    }
}
