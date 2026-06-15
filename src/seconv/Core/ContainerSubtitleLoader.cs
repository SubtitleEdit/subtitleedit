using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using Nikse.SubtitleEdit.Core.ContainerFormats.MaterialExchangeFormat;
using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4;
using Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Spectre.Console;

namespace SeConv.Core;

/// <summary>
/// Extracts subtitle tracks from container files (.mkv/.mks/.mp4/.m4v/.m4s/.3gp/.mcc).
/// Each track becomes one <see cref="LoadedTrack"/>; image-codec tracks are skipped
/// with a stderr warning (deferred to Phase 5 OCR).
/// </summary>
internal static class ContainerSubtitleLoader
{
    public sealed record LoadedTrack(
        Subtitle Subtitle,
        SubtitleFormat Format,
        string LanguageCode,
        int? TrackNumber);

    /// <summary>
    /// Returns the list of tracks if <paramref name="filePath"/> is a recognised container,
    /// or <c>null</c> if it's a regular subtitle file (caller should fall back to
    /// <see cref="LibSEIntegration.LoadSubtitleWithFormat"/>).
    /// </summary>
    public static List<LoadedTrack>? TryLoadTracks(string filePath, ConversionOptions options)
    {
        var ext = Path.GetExtension(filePath).ToLowerInvariant();

        if (ext is ".mkv" or ".mks")
        {
            return LoadMatroska(filePath, options);
        }

        if (ext is ".mp4" or ".m4v" or ".m4s" or ".3gp")
        {
            // Old converter applied a 10 KB minimum. Below that, fall through to text loader.
            try
            {
                if (new FileInfo(filePath).Length > 10_000)
                {
                    return LoadMp4(filePath, options);
                }
            }
            catch
            {
                // Ignore I/O race; let the text loader try.
            }
        }

        if (ext == ".mcc")
        {
            return LoadMcc(filePath);
        }

        if (ext == ".sup")
        {
            return LoadBluRaySup(filePath, options);
        }

        if (ext == ".sub")
        {
            var idxPath = Path.ChangeExtension(filePath, ".idx");
            if (File.Exists(idxPath))
            {
                return LoadVobSub(filePath, idxPath, options);
            }

            // No .idx companion. A binary VobSub .sub can still be read — the MPEG-PS packets
            // carry their own PTS timing and a default palette is used — so read it (with a
            // note) rather than letting it fall through to the MicroDVD text loader, which
            // would misparse the binary and surface a confusing "no subtitles found" error. A
            // genuine text MicroDVD .sub starts with text, not the MPEG pack header, so it
            // returns null here and is handled by the text loader.
            if (BitmapSubtitleLoader.IsBinaryVobSub(filePath))
            {
                AnsiConsole.MarkupLine(
                    $"[yellow]Note: VobSub '.sub' has no '.idx' companion ({Path.GetFileName(idxPath).EscapeMarkup()}); "
                    + "reading timing from the stream and using a default color palette.[/]");
                return LoadVobSub(filePath, idxPath, options);
            }

            return null;
        }

        if (ext is ".ts" or ".m2ts" or ".mts")
        {
            return LoadTransportStream(filePath, options);
        }

        if (ext == ".mxf")
        {
            return LoadMxf(filePath, options);
        }

        return null;
    }

    /// <summary>
    /// MXF (Material Exchange Format) container — broadcast / DCP workflows wrap timed
    /// text essences (TTML, SRT, etc.) inside KLV-packetised "essence elements". libse's
    /// <see cref="MxfParser"/> walks the KLV structure and extracts each candidate
    /// subtitle blob; we hand them to <see cref="Subtitle.ReloadLoadSubtitle"/> to
    /// auto-detect which SubtitleFormat each one actually is.
    ///
    /// Returns one <see cref="LoadedTrack"/> per parseable subtitle essence. Image
    /// essences (PNG payloads in MxfParser.GetImages) are *not* surfaced here —
    /// MxfParser collects the bitmaps but doesn't carry per-essence PTS, so there's no
    /// timing context for image output. Flagged as a warning instead.
    /// </summary>
    private static List<LoadedTrack>? LoadMxf(string filePath, ConversionOptions options)
    {
        // MxfParser walks the KLV structure inside its constructor, so any malformed
        // packet (zero-length essence, truncated BER length, etc.) surfaces here as a
        // low-level exception like IndexOutOfRangeException. Wrap it so the user sees
        // an MXF-contextual error instead of a stack-traceless "Index was outside the
        // bounds of the array".
        MxfParser parser;
        try
        {
            parser = new MxfParser(filePath);
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            throw new InvalidOperationException(
                $"Failed to parse MXF file '{filePath}': {ex.Message}", ex);
        }

        if (!parser.IsValid)
        {
            // Not a real MXF (no Header Partition Pack signature). Fall through to the
            // text loader — the file might just have a misleading extension.
            return null;
        }

        var subtitleTexts = parser.GetSubtitles();
        var images = parser.GetImages();

        if (subtitleTexts.Count == 0)
        {
            if (images.Count > 0)
            {
                throw new InvalidOperationException(
                    $"MXF contains {images.Count} image essence(s) but no text subtitles. "
                    + "Image-based MXF subtitles (PNG essences) aren't supported yet — "
                    + "the parser doesn't reconstruct per-essence PTS, so the bitmaps have no timing.");
            }
            throw new InvalidOperationException($"No subtitle essences found in MXF: {filePath}");
        }

        if (images.Count > 0)
        {
            AnsiConsole.MarkupLine(
                $"[yellow]Note: MXF also contains {images.Count} image essence(s) — skipped (no timing context).[/]");
        }

        var tracks = new List<LoadedTrack>();
        var trackNumber = 1;
        foreach (var subtitleText in subtitleTexts)
        {
            // Honour --track-number against the 1-based essence index — mirrors how
            // LoadMatroska / LoadMp4 filter their multi-track inputs (line ~96).
            if (options.TrackNumbers.Count > 0 && !options.TrackNumbers.Contains(trackNumber))
            {
                trackNumber++;
                continue;
            }

            var subtitle = new Subtitle();
            var lines = new List<string>(subtitleText.SplitToLines());
            // ReloadLoadSubtitle scans every SubtitleFormat's IsMine until one claims the text.
            // Passing null for all three "preferred format" hints means full auto-detect.
            var format = subtitle.ReloadLoadSubtitle(lines, null, null);
            if (format == null || subtitle.Paragraphs.Count == 0)
            {
                AnsiConsole.MarkupLine(
                    $"[yellow]Warning: MXF essence #{trackNumber} doesn't parse as a known subtitle format; skipped.[/]");
                trackNumber++;
                continue;
            }
            subtitle.Renumber();
            // Use "mxf_track{n}" as the language-suffix slot so multi-track MXFs produce
            // stably-named outputs (mirrors the teletext_<page> / dvb_pid<n> conventions).
            tracks.Add(new LoadedTrack(subtitle, format, $"mxf_track{trackNumber}", trackNumber));
            trackNumber++;
        }

        if (tracks.Count == 0)
        {
            if (options.TrackNumbers.Count > 0)
            {
                throw new InvalidOperationException(
                    $"MXF contained {subtitleTexts.Count} essence(s) but none matched --track-number ({string.Join(",", options.TrackNumbers)}): {filePath}");
            }
            throw new InvalidOperationException(
                $"MXF contained {subtitleTexts.Count} candidate subtitle essence(s) but none parsed as a known format: {filePath}");
        }

        return tracks;
    }

    private static List<LoadedTrack> LoadMatroska(string filePath, ConversionOptions options)
    {
        var tracks = new List<LoadedTrack>();
        using var matroska = new MatroskaFile(filePath);
        if (!matroska.IsValid)
        {
            throw new InvalidOperationException($"Invalid Matroska file: {filePath}");
        }

        var subtitleTracks = matroska.GetTracks(true);
        if (subtitleTracks.Count == 0)
        {
            throw new InvalidOperationException($"No subtitle tracks in Matroska file: {filePath}");
        }

        foreach (var track in subtitleTracks)
        {
            if (options.ForcedOnly && !track.IsForced)
            {
                continue;
            }
            if (options.TrackNumbers.Count > 0 && !options.TrackNumbers.Contains(track.TrackNumber))
            {
                continue;
            }

            // Image-codec tracks: PGS goes through Tesseract OCR; VobSub deferred.
            if (track.CodecId.Equals("S_HDMV/PGS", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var pgsSub = ImageOcrLoader.LoadMatroskaPgs(matroska, track, options);
                    if (pgsSub.Paragraphs.Count > 0)
                    {
                        tracks.Add(new LoadedTrack(pgsSub, new SubRip(), SanitizeLang(track.Language), track.TrackNumber));
                    }
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[yellow]Warning: PGS OCR failed on MKV track #{track.TrackNumber}: {ex.Message.EscapeMarkup()}[/]");
                }
                continue;
            }

            if (track.CodecId.Equals("S_VOBSUB", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var vobSub = ImageOcrLoader.LoadMatroskaVobSub(matroska, track, options);
                    if (vobSub.Paragraphs.Count > 0)
                    {
                        tracks.Add(new LoadedTrack(vobSub, new SubRip(), SanitizeLang(track.Language), track.TrackNumber));
                    }
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[yellow]Warning: VobSub OCR failed on MKV track #{track.TrackNumber}: {ex.Message.EscapeMarkup()}[/]");
                }
                continue;
            }

            // Text-codec tracks: S_TEXT/UTF8, S_TEXT/SSA, S_TEXT/ASS, S_HDMV/TEXTST
            var subtitle = new Subtitle();
            var matroskaSubtitle = matroska.GetSubtitle(track.TrackNumber, null);
            var format = Utilities.LoadMatroskaTextSubtitle(track, matroska, matroskaSubtitle, subtitle);
            if (subtitle.Paragraphs.Count == 0 || format == null)
            {
                AnsiConsole.MarkupLine($"[yellow]Warning: track #{track.TrackNumber} produced no subtitles ({track.CodecId.EscapeMarkup()}).[/]");
                continue;
            }
            subtitle.Renumber();

            var lang = SanitizeLang(track.Language);
            tracks.Add(new LoadedTrack(subtitle, format, lang, track.TrackNumber));
        }

        return tracks;
    }

    private static List<LoadedTrack> LoadMp4(string filePath, ConversionOptions options)
    {
        var tracks = new List<LoadedTrack>();
        var parser = new MP4Parser(filePath);

        // VTTC sidecar track (some MP4s embed WebVTT cues alongside text tracks)
        if (parser.VttcSubtitle is { } vttc && vttc.Paragraphs.Count > 0)
        {
            vttc.Renumber();
            var lang = SanitizeLang(parser.VttcLanguage)
                ?? LanguageAutoDetect.AutoDetectGoogleLanguageOrNull(vttc)
                ?? string.Empty;
            tracks.Add(new LoadedTrack(vttc, new SubRip(), lang, null));
        }

        foreach (var trak in parser.GetSubtitleTracks())
        {
            var trackId = (int)trak.Tkhd.TrackId;
            if (options.TrackNumbers.Count > 0 && !options.TrackNumbers.Contains(trackId))
            {
                continue;
            }
            if (trak.Mdia.IsVobSubSubtitle)
            {
                try
                {
                    var vobSub = ImageOcrLoader.LoadMp4VobSub(trak, options);
                    if (vobSub.Paragraphs.Count > 0)
                    {
                        var vobLang = LanguageAutoDetect.AutoDetectGoogleLanguageOrNull(vobSub) ?? string.Empty;
                        tracks.Add(new LoadedTrack(vobSub, new SubRip(), vobLang, trackId));
                    }
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[yellow]Warning: VobSub OCR failed on MP4 track #{trackId}: {ex.Message.EscapeMarkup()}[/]");
                }
                continue;
            }

            var paragraphs = trak.Mdia.Minf.Stbl.GetParagraphs();
            if (paragraphs.Count == 0)
            {
                continue;
            }
            var subtitle = new Subtitle();
            subtitle.Paragraphs.AddRange(paragraphs);
            subtitle.Renumber();
            var lang = LanguageAutoDetect.AutoDetectGoogleLanguageOrNull(subtitle) ?? string.Empty;
            tracks.Add(new LoadedTrack(subtitle, new SubRip(), lang, trackId));
        }

        if (tracks.Count == 0)
        {
            throw new InvalidOperationException($"No subtitle tracks in MP4 file: {filePath}");
        }
        return tracks;
    }

    private static List<LoadedTrack> LoadMcc(string filePath)
    {
        var mcc = new MacCaption10();
        if (!mcc.IsMine(null, filePath))
        {
            throw new InvalidOperationException($"File is not a valid MCC subtitle: {filePath}");
        }
        var subtitle = new Subtitle();
        mcc.LoadSubtitle(subtitle, null, filePath);
        subtitle.Renumber();
        return [new LoadedTrack(subtitle, mcc, string.Empty, null)];
    }

    private static List<LoadedTrack> LoadBluRaySup(string filePath, ConversionOptions options)
    {
        var subtitle = ImageOcrLoader.LoadBluRaySup(filePath, options);
        if (subtitle.Paragraphs.Count == 0)
        {
            throw new InvalidOperationException($"No subtitles recognised in Blu-Ray sup file: {filePath}");
        }
        return [new LoadedTrack(subtitle, new SubRip(), string.Empty, null)];
    }

    private static List<LoadedTrack> LoadVobSub(string subPath, string idxPath, ConversionOptions options)
    {
        var subtitle = ImageOcrLoader.LoadVobSub(subPath, idxPath, options);
        if (subtitle.Paragraphs.Count == 0)
        {
            throw new InvalidOperationException($"No subtitles recognised in VobSub file: {subPath}");
        }
        return [new LoadedTrack(subtitle, new SubRip(), string.Empty, null)];
    }

    private static List<LoadedTrack> LoadTransportStream(string filePath, ConversionOptions options)
    {
        var tracks = new List<LoadedTrack>();

        // 1. Teletext — already text, no OCR needed
        if (!options.SkipTeletext)
        {
            var parser = new TransportStreamParser();
            parser.Parse(filePath, null);
            foreach (var pidEntry in parser.TeletextSubtitlesLookup)
            {
                foreach (var pageEntry in pidEntry.Value)
                {
                    if (options.TeletextOnlyPage.HasValue && pageEntry.Key != options.TeletextOnlyPage.Value)
                    {
                        continue;
                    }
                    var paragraphs = pageEntry.Value;
                    if (paragraphs.Count == 0)
                    {
                        continue;
                    }
                    var subtitle = new Subtitle();
                    subtitle.Paragraphs.AddRange(paragraphs);
                    subtitle.Renumber();
                    tracks.Add(new LoadedTrack(subtitle, new SubRip(), $"teletext_{pidEntry.Key}_p{pageEntry.Key}", pidEntry.Key));
                }
            }
        }

        // 2. DVB-sub (image) — runs through Tesseract
        if (!options.TeletextOnly)
        {
            try
            {
                var dvbSubs = ImageOcrLoader.LoadTransportStreamDvbSub(filePath, options);
                foreach (var (subtitle, pid) in dvbSubs)
                {
                    tracks.Add(new LoadedTrack(subtitle, new SubRip(), $"dvb_pid{pid}", pid));
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[yellow]Warning: DVB-sub OCR failed: {ex.Message.EscapeMarkup()}[/]");
            }
        }

        if (tracks.Count == 0)
        {
            throw new InvalidOperationException($"No subtitles found in transport stream: {filePath}");
        }
        return tracks;
    }

    /// <summary>
    /// Cleans a language tag for use in an output filename. Empty/whitespace → empty
    /// string (caller treats as "no suffix"). Otherwise strips characters that are
    /// problematic in filenames on Windows. Note: "und" (ISO 639 "undetermined") is
    /// kept, so MKV tracks tagged as such still get a distinct suffix.
    /// </summary>
    internal static string SanitizeLang(string? lang)
    {
        if (string.IsNullOrWhiteSpace(lang))
        {
            return string.Empty;
        }
        return lang.RemoveChar('?').RemoveChar('!').RemoveChar('*').RemoveChar(',').RemoveChar('/').Trim();
    }
}
