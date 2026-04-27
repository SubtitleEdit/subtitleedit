using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
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

        if (ext is ".ts" or ".m2ts" or ".mts")
        {
            return LoadTransportStream(filePath, options);
        }

        return null;
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
                AnsiConsole.MarkupLine($"[yellow]Warning: skipping VobSub MKV track #{track.TrackNumber} — VobSub OCR not yet supported in seconv. Use Subtitle Edit (UI) for now.[/]");
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
                AnsiConsole.MarkupLine($"[yellow]Warning: skipping VobSub MP4 track #{trackId} — OCR is not yet supported.[/]");
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

    private static string SanitizeLang(string? lang)
    {
        if (string.IsNullOrWhiteSpace(lang))
        {
            return string.Empty;
        }
        return lang.RemoveChar('?').RemoveChar('!').RemoveChar('*').RemoveChar(',').RemoveChar('/').Trim();
    }
}
