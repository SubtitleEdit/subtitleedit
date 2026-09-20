using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes;
using Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream;
using Nikse.SubtitleEdit.UiLogic.Ocr.Paddle;
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
            if (!options.Quiet)
            {
                AnsiConsole.MarkupLine($"[dim]Extracting time codes from {pcsList.Count} Blu-Ray sup image(s) (no OCR)...[/]");
            }

            return PcsListToSubtitle(pcsList, null);
        }

        using var ocr = OcrEngineFactory.Create(options);
        var isolationNote = options.PgsIsolateColors ? string.Empty : " (colour isolation off)";
        if (!options.Quiet)
        {
            AnsiConsole.MarkupLine($"[dim]Running {ocr.Name} OCR on {pcsList.Count} Blu-Ray sup image(s){isolationNote}...[/]");
        }

        return PcsListToSubtitle(pcsList, ocr, options.PgsIsolateColors, options.Quiet);
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
            if (!options.Quiet)
            {
                AnsiConsole.MarkupLine($"[dim]Extracting time codes from {pcsList.Count} MKV PGS image(s) (track #{track.TrackNumber}, no OCR)...[/]");
            }

            return PcsListToSubtitle(pcsList, null);
        }

        using var ocr = OcrEngineFactory.Create(options);
        var isolationNote = options.PgsIsolateColors ? string.Empty : " (colour isolation off)";
        if (!options.Quiet)
        {
            AnsiConsole.MarkupLine($"[dim]Running {ocr.Name} OCR on {pcsList.Count} MKV PGS image(s) (track #{track.TrackNumber}){isolationNote}...[/]");
        }

        return PcsListToSubtitle(pcsList, ocr, options.PgsIsolateColors, options.Quiet);
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

                if (!options.Quiet)
                {
                    AnsiConsole.MarkupLine(ocr is null
                        ? $"[dim]Extracting time codes from {dvbSubtitles.Count} DVB-sub image(s) (PID {pid}, no OCR)...[/]"
                        : $"[dim]Running {ocr.Name} OCR on {dvbSubtitles.Count} DVB-sub image(s) (PID {pid})...[/]");
                }

                // Recognition is the slow part, so report it per image (issue #14267).
                var showProgress = ocr is not null && !options.Quiet;
                var subtitle = new Subtitle();

                // ocr == null → time-codes-only: keep every entry that has an image, with
                // empty text, without decoding a bitmap for recognition.
                // Same antialiased binarisation as the PGS path (issue #12291).
                Func<SKBitmap, SKBitmap>? isolate =
                    options.PgsIsolateColors ? (b => VobSubColorIsolation.BinarizeForOcr(b)) : null;
                var texts = ocr is null
                    ? TimeCodesOnlyTexts(dvbSubtitles.Count, i => dvbSubtitles[i].GetBitmap())
                    : RecognizeAll(
                        ocr, dvbSubtitles.Count, i => dvbSubtitles[i].GetBitmap(),
                        callerOwnsBitmap: false, isolate, quiet: !showProgress);

                for (var i = 0; i < dvbSubtitles.Count; i++)
                {
                    var text = texts[i];
                    if (text is null || (ocr is not null && string.IsNullOrWhiteSpace(text)))
                    {
                        continue;
                    }

                    subtitle.Paragraphs.Add(new LibSeParagraph(
                        text, dvbSubtitles[i].StartMilliseconds, dvbSubtitles[i].EndMilliseconds));
                }

                if (showProgress)
                {
                    ProgressLine.Report("OCR", dvbSubtitles.Count, dvbSubtitles.Count);
                    ProgressLine.Finish();
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
    /// DVB subtitle MKV track (codec <c>S_DVBSUB</c>) → text via the configured OCR engine, or
    /// time codes only when <see cref="ConversionOptions.TimeCodesOnly"/> is set.
    /// </summary>
    public static Subtitle LoadMatroskaDvbSub(MatroskaFile matroska, MatroskaTrackInfo track, ConversionOptions options)
    {
        var items = BitmapSubtitleLoader.LoadMatroskaDvbSub(matroska, track);
        return OcrBitmapItems(items, options, $"{items.Count} MKV DVB-sub image(s) (track #{track.TrackNumber})");
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
    /// XSUB (".avi"/".divx" DivX subtitles) → text via the configured OCR engine, or time codes
    /// only when <see cref="ConversionOptions.TimeCodesOnly"/> is set. One Subtitle per subtitle
    /// stream in the file; the stream number is returned so the caller can name the outputs.
    /// </summary>
    public static List<(Subtitle Subtitle, int? StreamNumber)> LoadXSub(string filePath, ConversionOptions options)
    {
        var results = new List<(Subtitle, int?)>();
        foreach (var (items, streamNumber) in BitmapSubtitleLoader.LoadXSub(filePath))
        {
            var label = streamNumber.HasValue
                ? $"{items.Count} XSUB image(s) (stream #{streamNumber.Value})"
                : $"{items.Count} XSUB image(s)";
            var subtitle = OcrBitmapItems(items, options, label);
            if (subtitle.Paragraphs.Count > 0)
            {
                results.Add((subtitle, streamNumber));
            }
        }

        return results;
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
                if (!options.Quiet)
                {
                    AnsiConsole.MarkupLine($"[dim]Extracting time codes from {what} (no OCR)...[/]");
                }

                return BitmapItemsToSubtitle(items, null);
            }

            using var ocr = OcrEngineFactory.Create(options);
            var isolationNote = options.VobSubIsolateColors ? string.Empty : " (colour isolation off)";
            if (!options.Quiet)
            {
                AnsiConsole.MarkupLine($"[dim]Running {ocr.Name} OCR on {what}{isolationNote}...[/]");
            }

            return BitmapItemsToSubtitle(items, ocr, options.VobSubIsolateColors, options.Quiet);
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
    /// Recognises a whole subtitle's worth of images and returns the text per index -
    /// <c>null</c> for an index that had no usable bitmap, which the caller drops entirely.
    /// <para>
    /// Paddle OCR spends roughly twenty seconds loading its models per process, so the whole
    /// set goes through one batched run; other engines stay one call per image. Bitmaps are
    /// fetched one at a time and released immediately, so a feature-length subtitle never has
    /// more than one decoded frame in memory.
    /// </para>
    /// </summary>
    /// <param name="rentBitmap">Decodes the image for an index, or returns null if there is none.</param>
    /// <param name="callerOwnsBitmap">
    /// True when <paramref name="rentBitmap"/> hands back a bitmap owned by someone else (an
    /// item that is disposed later); false when it decodes a fresh one we have to release.
    /// </param>
    /// <param name="isolate">Optional colour isolation applied before recognition.</param>
    /// <param name="quiet">Suppresses progress and the engine's warnings.</param>
    private static string?[] RecognizeAll(
        IOcrEngine ocr,
        int count,
        Func<int, SKBitmap?> rentBitmap,
        bool callerOwnsBitmap,
        Func<SKBitmap, SKBitmap>? isolate,
        bool quiet)
    {
        var texts = new string?[count];
        var showProgress = !quiet;

        if (ocr is PaddleOcrEngine paddle)
        {
            // An index with no bitmap still needs a slot in the batch, or every later result
            // would be attributed to the wrong line; it is marked here and dropped afterwards.
            var missing = new bool[count];

            var results = paddle.RecognizeBatch(
                count,
                (index, path) =>
                {
                    var bitmap = rentBitmap(index);
                    if (bitmap is null)
                    {
                        missing[index] = true;
                        PaddleOcrImagePrep.WriteBlankPng(path);
                        return;
                    }

                    try
                    {
                        if (isolate is null)
                        {
                            PaddleOcrImagePrep.WritePreparedPng(bitmap, path);
                        }
                        else
                        {
                            using var isolated = isolate(bitmap);
                            PaddleOcrImagePrep.WritePreparedPng(isolated, path);
                        }
                    }
                    finally
                    {
                        if (!callerOwnsBitmap)
                        {
                            bitmap.Dispose();
                        }
                    }
                },
                // Stop one short of the total: each caller prints the closing 100% itself,
                // and in a redirected log a second one would show up as a duplicate line.
                showProgress
                    ? (done, total) => { if (done < total) { ProgressLine.Report("OCR", done, total); } }
                    : null);

            for (var i = 0; i < count; i++)
            {
                texts[i] = missing[i] ? null : results[i];
            }

            if (!quiet)
            {
                foreach (var warning in paddle.Warnings)
                {
                    AnsiConsole.MarkupLine($"[yellow]Note: {warning.EscapeMarkup()}[/]");
                }
            }

            return texts;
        }

        for (var i = 0; i < count; i++)
        {
            // Reported before the image is recognised, so the count is images *finished* -
            // 100% must not show while the last one is still running.
            if (showProgress)
            {
                ProgressLine.Report("OCR", i, count);
            }

            var bitmap = rentBitmap(i);
            if (bitmap is null)
            {
                continue;
            }

            try
            {
                if (isolate is null)
                {
                    texts[i] = ocr.Recognize(bitmap);
                }
                else
                {
                    using var isolated = isolate(bitmap);
                    texts[i] = ocr.Recognize(isolated);
                }
            }
            finally
            {
                if (!callerOwnsBitmap)
                {
                    bitmap.Dispose();
                }
            }
        }

        return texts;
    }

    /// <summary>
    /// Time-codes-only: empty text for every index that has an image, null for the rest, so
    /// the entries that survive are exactly the ones a real OCR run would have kept.
    /// </summary>
    private static string?[] TimeCodesOnlyTexts(int count, Func<int, SKBitmap?> rentBitmap)
    {
        var texts = new string?[count];
        for (var i = 0; i < count; i++)
        {
            using var bitmap = rentBitmap(i);
            if (bitmap is not null)
            {
                texts[i] = string.Empty;
            }
        }

        return texts;
    }

    /// <summary>
    /// Turns pre-decoded bitmap events into a Subtitle. <paramref name="ocr"/> null =
    /// time-codes-only (empty text kept); non-null = recognise each bitmap and drop blanks.
    /// When <paramref name="isolateColors"/> is set, each bitmap is binarised via
    /// <see cref="VobSubColorIsolation"/> before recognition (VobSub sources only, and never
    /// in time-codes-only mode since no recognition happens there).
    /// </summary>
    private static Subtitle BitmapItemsToSubtitle(
        IReadOnlyList<BitmapSubtitleLoader.BitmapSubtitleItem> items, IOcrEngine? ocr, bool isolateColors = false,
        bool quiet = false)
    {
        var subtitle = new Subtitle();
        var blankCount = 0;
        // Time-codes-only mode is instant, so only a real OCR run reports progress (#14267).
        var showProgress = ocr is not null && !quiet;

        Func<SKBitmap, SKBitmap>? isolate = isolateColors ? (b => VobSubColorIsolation.Isolate(b)) : null;
        var texts = ocr is null
            ? new string?[items.Count]
            : RecognizeAll(
                ocr, items.Count, i => items[i].Bitmap, callerOwnsBitmap: true, isolate, quiet: !showProgress);

        for (var i = 0; i < items.Count; i++)
        {
            var text = texts[i] ?? string.Empty;
            if (ocr is null || !string.IsNullOrWhiteSpace(text))
            {
                subtitle.Paragraphs.Add(new LibSeParagraph(
                    text, items[i].StartTime.TotalMilliseconds, items[i].EndTime.TotalMilliseconds));
            }
            else
            {
                blankCount++;
            }
        }

        if (showProgress)
        {
            ProgressLine.Report("OCR", items.Count, items.Count);
            ProgressLine.Finish();
        }

        if (blankCount > 0 && !quiet)
        {
            // Issue #12772: these used to vanish without a trace, making it look like the
            // source had fewer subtitles than the GUI sees.
            AnsiConsole.MarkupLine(
                $"[yellow]Note: {blankCount} image(s) produced no OCR text and were dropped.[/]");
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
    private static Subtitle PcsListToSubtitle(List<BluRaySupParser.PcsData> pcsList, IOcrEngine? ocr,
        bool isolateColors = false, bool quiet = false)
    {
        var subtitle = new Subtitle();
        // Time-codes-only mode is instant, so only a real OCR run reports progress (#14267).
        var showProgress = ocr is not null && !quiet;

        // PGS glyphs are white fill + black outline on transparency; binarise so the fill
        // survives the opaque white OCR canvas (issue #12291).
        Func<SKBitmap, SKBitmap>? isolate = isolateColors ? (b => VobSubColorIsolation.BinarizeForOcr(b)) : null;
        var texts = ocr is null
            ? TimeCodesOnlyTexts(pcsList.Count, i => pcsList[i].GetBitmap())
            : RecognizeAll(
                ocr, pcsList.Count, i => pcsList[i].GetBitmap(),
                callerOwnsBitmap: false, isolate, quiet: !showProgress);

        for (var i = 0; i < pcsList.Count; i++)
        {
            var text = texts[i];
            if (text is null || (ocr is not null && string.IsNullOrWhiteSpace(text)))
            {
                continue;
            }

            subtitle.Paragraphs.Add(new LibSeParagraph(text, pcsList[i].StartTime / 90.0, pcsList[i].EndTime / 90.0));
        }

        if (showProgress)
        {
            ProgressLine.Report("OCR", pcsList.Count, pcsList.Count);
            ProgressLine.Finish();
        }

        subtitle.Renumber();
        return subtitle;
    }
}
