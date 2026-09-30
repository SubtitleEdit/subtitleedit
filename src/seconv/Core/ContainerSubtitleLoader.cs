using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using Nikse.SubtitleEdit.Core.ContainerFormats.MaterialExchangeFormat;
using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4;
using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes;
using Nikse.SubtitleEdit.Core.ContainerFormats.ProgramStream;
using Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Core.VobSub;
using Nikse.SubtitleEdit.UiLogic.SubtitleLoading;
using Spectre.Console;

namespace SeConv.Core;

/// <summary>
/// Extracts subtitle tracks from container files (.mkv/.mks/.mp4/.m4v/.m4s/.3gp/.mcc/.avi).
/// Each track becomes one <see cref="LoadedTrack"/>; image-codec tracks are skipped
/// with a stderr warning (deferred to Phase 5 OCR).
/// </summary>
internal static class ContainerSubtitleLoader
{
    public sealed record LoadedTrack(
        Subtitle Subtitle,
        SubtitleFormat Format,
        string LanguageCode,
        int? TrackNumber,
        bool IsForced = false);

    /// <summary>
    /// Returns the list of tracks if <paramref name="filePath"/> is a recognised container,
    /// or <c>null</c> if it's a regular subtitle file (caller should fall back to
    /// <see cref="LibSEIntegration.LoadSubtitleWithFormat"/>).
    /// </summary>
    public static List<LoadedTrack>? TryLoadTracks(string filePath, ConversionOptions options)
    {
        var ext = Path.GetExtension(filePath).ToLowerInvariant();

        // A Manzanita "private_stream_1" dump can have any extension (.dvbttx, .stl, even .idx,
        // which the VobSub branch below would take). Teletext dumps are text (the DVB Teletext
        // format reads them); bitmap dumps need OCR.
        if (IsManzanitaDvbSubtitle(filePath))
        {
            return LoadManzanitaDvbSub(filePath, options);
        }

        if (IsManzanita(filePath))
        {
            return null;
        }

        // A transport stream saved under another video extension (e.g. an HLS web rip named
        // .mp4) - the MP4/Matroska parsers below would find nothing in it.
        if (FileUtil.IsTransportStreamWithOtherVideoExtension(filePath))
        {
            return LoadTransportStream(filePath, options);
        }

        // Content over extension, as the GUI opens them: a Matroska file named .mp4 or .sup, a
        // Blu-ray .sup named .sub - the loader picked by the extension found nothing in them.
        if (ext is not (".mkv" or ".mks" or ".webm") && FileUtil.IsMatroskaFileFast(filePath) && FileUtil.IsMatroskaFile(filePath))
        {
            return LoadMatroska(filePath, options);
        }

        if (ext != ".sup" && FileUtil.IsBluRaySupByContent(filePath))
        {
            return LoadBluRaySup(filePath, options);
        }

        // DVB recorder extensions, and transport streams saved as .mpg/.mpeg (the GUI opens these
        // as TS too); a recorder header before the first packet is fine, IsTransportStream looks
        // past it. The program stream reader below wants a pack header, so a .mpeg transport
        // stream fell through to the text loader - minutes of reading a video as lines.
        if (ext is ".tsv" or ".tts" or ".rec" or ".mpg" or ".mpeg" &&
            (FileUtil.IsTransportStream(filePath) || FileUtil.IsM2TransportStream(filePath)))
        {
            return LoadTransportStream(filePath, options);
        }

        // .webm is Matroska too - a WebVTT track muxed into one was falling through to the
        // text loader, which then failed to detect a format at all.
        if (ext is ".mkv" or ".mks" or ".webm")
        {
            return LoadMatroska(filePath, options);
        }

        if (ext is ".mp4" or ".m4v" or ".m4s" or ".3gp" or ".mov" or ".m4a" or ".m4b" or ".cmaf")
        {
            long fileLength;
            try
            {
                fileLength = new FileInfo(filePath).Length;
            }
            catch
            {
                // Ignore I/O race; let the text loader try.
                fileLength = 0;
            }

            // A real video: "no subtitle tracks" is the answer. Swallowing it sent the whole
            // movie through the text loader, which read gigabytes as lines only to report
            // "Unable to determine subtitle format".
            if (fileLength > 10_000)
            {
                return LoadMp4(filePath, options);
            }

            // Subtitle-only DASH/CMAF files (an init segment plus a few m4s fragments)
            // are typically just a few KB. Try the MP4 parser, but on failure fall
            // through to the text loader as the old 10 KB minimum did.
            if (fileLength > 100)
            {
                try
                {
                    return LoadMp4(filePath, options);
                }
                catch (InvalidOperationException)
                {
                    // No tracks found; let the text loader try.
                }
            }
        }

        // PSP UMD Video (.MPS), PSP movies (.PMF) and ".subs" dumps of their subtitles: png images,
        // one track per subtitle stream. A video without subtitles goes on to the video check.
        if (ext is ".mps" or ".pmf" or ".subs")
        {
            var umdTracks = LoadUmdVideo(filePath, options);
            if (umdTracks != null)
            {
                return umdTracks;
            }
        }

        if (ext == ".mcc")
        {
            return LoadMcc(filePath);
        }

        if (ext == ".sup")
        {
            if (HdDvdSupParser.IsHdDvdSup(filePath))
            {
                return LoadHdDvdSup(filePath, options);
            }

            if (FileUtil.IsSpDvdSup(filePath))
            {
                return LoadSpDvdSup(filePath, options);
            }

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

        if (ext == ".idx")
        {
            // 5.0.0 accepted the .idx of a VobSub pair as the input file; keep that working
            // by redirecting to the companion .sub (which holds the actual subpictures) and
            // using the given .idx for timing + palette (issue #12772).
            var subPath = Path.ChangeExtension(filePath, ".sub");
            if (!File.Exists(subPath))
            {
                throw new InvalidOperationException(
                    $"VobSub '.idx' input has no companion '.sub' ({Path.GetFileName(subPath)}) — "
                    + "the .idx only holds timing and palette; the subtitle images live in the .sub.");
            }

            return LoadVobSub(subPath, filePath, options);
        }

        if (ext is ".ts" or ".m2ts" or ".mts")
        {
            return LoadTransportStream(filePath, options);
        }

        if (ext is ".avi" or ".divx")
        {
            return LoadXSub(filePath, options);
        }

        if (ext == ".mxf")
        {
            return LoadMxf(filePath, options);
        }

        if (ext is ".vob" or ".mpg" or ".mpeg" or ".m2p" && ProgramStreamClosedCaptionReader.IsProgramStream(filePath) ||
            ext is ".m2v" or ".m1v" or ".mpv" && ProgramStreamClosedCaptionReader.IsVideoElementaryStream(filePath))
        {
            return LoadProgramStreamClosedCaptions(filePath, options);
        }

        // A video no container reader took has no subtitles SE can read - the text loader would
        // spend minutes trying every format on it as lines, then fail anyway.
        if (Utilities.VideoFileExtensions.Contains(ext) && new FileInfo(filePath).Length > 20_000_000)
        {
            throw new InvalidOperationException($"No subtitles found in video file: {filePath}");
        }

        // Image-list files (BDN xml, SON, DOST, SubRip with png names, ...) name an image per
        // cue. As text they would convert to the file names, so OCR them like the GUI does.
        var imageList = TryLoadImageList(filePath, ext);
        if (imageList != null)
        {
            var subtitle = ImageOcrLoader.LoadImageList(imageList, filePath, options);
            if (subtitle.Paragraphs.Count == 0)
            {
                throw new InvalidOperationException($"No subtitles recognised in image-list file: {filePath}");
            }

            return [new LoadedTrack(subtitle, new SubRip(), string.Empty, null)];
        }

        return null;
    }

    /// <summary>
    /// The subtitle streams of a PSP UMD Video file, null if it has none. The track number is the
    /// sub-stream id (0x80 = 128 for the first stream), the name "umd1", "umd2", ...
    /// </summary>
    private static List<LoadedTrack>? LoadUmdVideo(string filePath, ConversionOptions options)
    {
        var umdTracks = UmdVideoSubtitleReader.Read(filePath);
        if (umdTracks.Count == 0)
        {
            return null;
        }

        var tracks = new List<LoadedTrack>();
        foreach (var track in umdTracks)
        {
            if (options.TrackNumbers.Count > 0 && !options.TrackNumbers.Contains(track.Key))
            {
                continue;
            }

            var subtitle = ImageOcrLoader.LoadUmdVideo(track.Value, track.Key, options);
            tracks.Add(new LoadedTrack(subtitle, new SubRip(), "umd" + (track.Key - 0x80 + 1), track.Key));
        }

        if (tracks.Count == 0)
        {
            throw new InvalidOperationException($"No PSP UMD Video subtitle stream matches the track number(s) in: {filePath}");
        }

        return tracks;
    }

    private static readonly string[] ImageFileExtensions = [".png", ".bmp", ".jpg", ".tif"];

    /// <summary>
    /// The image-list subtitle in <paramref name="filePath"/> (cue text = image file names), or
    /// null for anything else. Same detection as the GUI's File > Open.
    /// </summary>
    private static Subtitle? TryLoadImageList(string filePath, string ext)
    {
        if (ext == ".xml")
        {
            var imageListXml = ImageListSubtitleLoader.TryLoadImageListXml(filePath);
            if (imageListXml != null)
            {
                return imageListXml;
            }
        }

        // Only a file that mentions an image file at all is parsed a second time - an
        // ordinary subtitle never pays for it. Image lists are small; skip big files.
        string text;
        try
        {
            if (new FileInfo(filePath).Length > 20_000_000)
            {
                return null;
            }

            text = System.Text.Encoding.Latin1.GetString(File.ReadAllBytes(filePath));
        }
        catch (IOException)
        {
            return null;
        }

        if (!ImageFileExtensions.Any(e => text.Contains(e, StringComparison.OrdinalIgnoreCase)))
        {
            return null;
        }

        var encoding = LanguageAutoDetect.GetEncodingFromFile(filePath);
        if (ext is ".ttml" or ".xml" or ".dfxp")
        {
            // IMSC image profile: smpte:backgroundImage names png files next to the document.
            var lines = FileUtil.ReadAllLinesShared(filePath, encoding);
            var timedTextImage = new TimedTextImage();
            if (timedTextImage.IsMine(lines, filePath))
            {
                var subtitle = new Subtitle();
                timedTextImage.LoadSubtitle(subtitle, lines, filePath);
                if (subtitle.Paragraphs.Count > 0)
                {
                    return subtitle;
                }
            }
        }

        return ImageListSubtitleLoader.TryLoad(filePath, encoding, Subtitle.Parse(filePath, encoding));
    }

    /// <summary>
    /// MPEG program stream (DVD .vob, .mpg): CEA-608 closed captions from the video - DVD style
    /// Line 21 captions, ATSC A/53 or SCTE 20 user data. The track number is the caption channel
    /// (1-4 = CC1-CC4).
    /// </summary>
    private static List<LoadedTrack>? LoadProgramStreamClosedCaptions(string filePath, ConversionOptions options)
    {
        var tracks = new List<LoadedTrack>();
        foreach (var captionTrack in ProgramStreamClosedCaptionReader.Read(filePath, ProgramStreamClosedCaptionReader.DefaultProbeMilliseconds, null))
        {
            if (options.TrackNumbers.Count > 0 && !options.TrackNumbers.Contains(captionTrack.Key))
            {
                continue;
            }

            var subtitle = new Subtitle();
            subtitle.Paragraphs.AddRange(captionTrack.Value);
            subtitle.Renumber();
            tracks.Add(new LoadedTrack(subtitle, new SubRip(), $"cea608_cc{captionTrack.Key}", captionTrack.Key));
        }

        return tracks.Count > 0 ? tracks : null; // null: let the other loaders have a go
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

        // CEA-608/708 closed captions from a SMPTE 436M ANC track (broadcast MXF). The track
        // number is the caption track key: 1-4 = CC1-CC4, 100 + n = CEA-708 service n.
        if (subtitleTexts.Count == 0 && parser.ClosedCaptionTracks.Count > 0)
        {
            var captionTracks = GetClosedCaptionTracks(parser.ClosedCaptionTracks, options);
            if (captionTracks.Count > 0)
            {
                return captionTracks;
            }
        }

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
            // CEA-608/708 closed captions inside the video track (e.g. a broadcast recording remuxed to .mkv)
            var videoTrack = MatroskaClosedCaptionReader.GetVideoTrack(matroska);
            if (videoTrack != null && (options.TrackNumbers.Count == 0 || options.TrackNumbers.Contains(videoTrack.TrackNumber)))
            {
                foreach (var captionTrack in MatroskaClosedCaptionReader.Read(matroska, MatroskaClosedCaptionReader.DefaultProbeMilliseconds, null))
                {
                    var subtitle = new Subtitle();
                    subtitle.Paragraphs.AddRange(captionTrack.Value);
                    subtitle.Renumber();
                    var trackName = captionTrack.Key > ClosedCaptionExtractor.Cea708TrackKeyOffset
                        ? $"cea708_{videoTrack.TrackNumber}_s{captionTrack.Key - ClosedCaptionExtractor.Cea708TrackKeyOffset}"
                        : $"cea608_{videoTrack.TrackNumber}_cc{captionTrack.Key}";
                    tracks.Add(new LoadedTrack(subtitle, new SubRip(), trackName, videoTrack.TrackNumber));
                }
            }

            if (tracks.Count > 0)
            {
                return tracks;
            }

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
                        tracks.Add(new LoadedTrack(pgsSub, new SubRip(), SanitizeLang(track.Language), track.TrackNumber, track.IsForced));
                    }
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLineInterpolated($"[yellow]Warning: PGS OCR failed on MKV track #{track.TrackNumber}: {ex.Message}[/]");
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
                        tracks.Add(new LoadedTrack(vobSub, new SubRip(), SanitizeLang(track.Language), track.TrackNumber, track.IsForced));
                    }
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLineInterpolated($"[yellow]Warning: VobSub OCR failed on MKV track #{track.TrackNumber}: {ex.Message}[/]");
                }
                continue;
            }

            if (track.CodecId.Equals("S_DVBSUB", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var dvbSub = ImageOcrLoader.LoadMatroskaDvbSub(matroska, track, options);
                    if (dvbSub.Paragraphs.Count > 0)
                    {
                        tracks.Add(new LoadedTrack(dvbSub, new SubRip(), SanitizeLang(track.Language), track.TrackNumber, track.IsForced));
                    }
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLineInterpolated($"[yellow]Warning: DVB-sub decode failed on MKV track #{track.TrackNumber}: {ex.Message}[/]");
                }
                continue;
            }

            // Text-codec tracks: S_TEXT/UTF8, S_TEXT/SSA, S_TEXT/ASS, S_HDMV/TEXTST
            var subtitle = new Subtitle();
            var matroskaSubtitle = matroska.GetSubtitle(track.TrackNumber, null);
            var format = Utilities.LoadMatroskaTextSubtitle(track, matroska, matroskaSubtitle, subtitle);
            if (subtitle.Paragraphs.Count == 0 || format == null)
            {
                AnsiConsole.MarkupLineInterpolated($"[yellow]Warning: track #{track.TrackNumber} produced no subtitles ({track.CodecId}).[/]");
                continue;
            }
            subtitle.Renumber();

            // Same fallback as the MP4 VTTC path: an MKV track without a declared language
            // gets auto-detected instead of an empty language.
            var lang = SanitizeLang(track.Language);
            lang = IsUndeclaredLanguage(lang) ? LanguageAutoDetect.AutoDetectGoogleLanguageOrNull(subtitle) ?? lang : lang;
            tracks.Add(new LoadedTrack(subtitle, format, lang, track.TrackNumber, track.IsForced));
        }

        // The file has subtitle tracks, but the filters (or OCR/decode failures) excluded
        // every one of them. Fail loudly like the MP4/TS/MXF loaders do - a silent empty
        // list would report a successful conversion of zero files.
        if (tracks.Count == 0)
        {
            throw new InvalidOperationException(
                $"No subtitle tracks matched in Matroska file: {filePath}" +
                (options.TrackNumbers.Count > 0 ? $" (--track-number {string.Join(",", options.TrackNumbers)})" : string.Empty) +
                (options.ForcedOnly ? " (--forced-only)" : string.Empty));
        }

        return tracks;
    }

    private static List<LoadedTrack> LoadMp4(string filePath, ConversionOptions options)
    {
        var tracks = new List<LoadedTrack>();
        var parser = new MP4Parser(filePath);

        // Fragmented (DASH/CMAF) text tracks - cues extracted from moof/traf/trun samples
        foreach (var fragmentedTrack in parser.FragmentedSubtitleTracks)
        {
            var trackNumber = (int?)fragmentedTrack.TrackId;
            if (trackNumber != null && options.TrackNumbers.Count > 0 && !options.TrackNumbers.Contains(trackNumber.Value))
            {
                continue;
            }

            var fragmentedSubtitle = fragmentedTrack.Subtitle;
            fragmentedSubtitle.Renumber();
            var fragmentedLang = SanitizeLang(fragmentedTrack.Language);
            fragmentedLang = IsUndeclaredLanguage(fragmentedLang) ? LanguageAutoDetect.AutoDetectGoogleLanguageOrNull(fragmentedSubtitle) ?? fragmentedLang : fragmentedLang;
            tracks.Add(new LoadedTrack(fragmentedSubtitle, new SubRip(), fragmentedLang, trackNumber));
        }

        foreach (var track in parser.GetSubtitleTracks())
        {
            var trackId = (int)track.Tkhd.TrackId;
            if (options.TrackNumbers.Count > 0 && !options.TrackNumbers.Contains(trackId))
            {
                continue;
            }

            // tx3g displayFlags is how QuickTime/AVFoundation marks a forced track
            var isForced = track.Mdia.Minf?.Stbl?.Stsd?.IsForcedSubtitle == true;
            if (options.ForcedOnly && !isForced)
            {
                continue;
            }

            if (track.Mdia.IsVobSubSubtitle)
            {
                try
                {
                    var vobSub = ImageOcrLoader.LoadMp4VobSub(track, options);
                    if (vobSub.Paragraphs.Count > 0)
                    {
                        tracks.Add(new LoadedTrack(vobSub, new SubRip(), GetMp4TrackLanguage(track, vobSub), trackId, isForced));
                    }
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLineInterpolated($"[yellow]Warning: VobSub OCR failed on MP4 track #{trackId}: {ex.Message}[/]");
                }
                continue;
            }

            var paragraphs = track.Mdia.Minf?.Stbl?.GetParagraphs();
            if (paragraphs == null || paragraphs.Count == 0)
            {
                continue;
            }
            var subtitle = new Subtitle();
            subtitle.Paragraphs.AddRange(paragraphs);
            subtitle.Renumber();
            tracks.Add(new LoadedTrack(subtitle, new SubRip(), GetMp4TrackLanguage(track, subtitle), trackId, isForced));
        }

        // CEA-608/708 closed captions in the video track's SEI (or a QuickTime c608 track) -
        // the parser only decodes them when the file has no subtitle track, as the GUI does.
        if (tracks.Count == 0 && parser.ClosedCaptionTracks.Count > 0 && !options.ForcedOnly)
        {
            tracks.AddRange(GetClosedCaptionTracks(parser.ClosedCaptionTracks, options));
        }

        if (tracks.Count == 0)
        {
            throw new InvalidOperationException(
                $"No subtitle tracks in MP4 file: {filePath}. Subtitles burned into the picture are not a track and cannot be extracted.");
        }
        return tracks;
    }

    /// <summary>
    /// One track per decoded closed caption channel. The key is the track number: 1-4 =
    /// CC1-CC4, 100 + n = CEA-708 service n (see <see cref="ClosedCaptionExtractor"/>).
    /// </summary>
    private static List<LoadedTrack> GetClosedCaptionTracks(SortedDictionary<int, List<Nikse.SubtitleEdit.Core.Common.Paragraph>> closedCaptionTracks, ConversionOptions options)
    {
        var captionTracks = new List<LoadedTrack>();
        foreach (var captionTrack in closedCaptionTracks)
        {
            if (options.TrackNumbers.Count > 0 && !options.TrackNumbers.Contains(captionTrack.Key))
            {
                continue;
            }

            var subtitle = new Subtitle();
            subtitle.Paragraphs.AddRange(captionTrack.Value);
            subtitle.Renumber();
            var trackName = captionTrack.Key > ClosedCaptionExtractor.Cea708TrackKeyOffset
                ? $"cea708_s{captionTrack.Key - ClosedCaptionExtractor.Cea708TrackKeyOffset}"
                : $"cea608_cc{captionTrack.Key}";
            captionTracks.Add(new LoadedTrack(subtitle, new SubRip(), trackName, captionTrack.Key));
        }

        return captionTracks;
    }

    /// <summary>
    /// The language the track declares in its media header, auto-detected from the text only
    /// when there is none. Auto-detecting regardless labelled every track of a multi-language
    /// file the same, and the per-track output names then collided - a three track eng/fre/deu
    /// file wrote one "*.en.srt" that the last track won.
    /// </summary>
    private static string GetMp4TrackLanguage(Trak track, Subtitle subtitle)
    {
        var lang = SanitizeLang(track.Mdia?.Mdhd?.Iso639ThreeLetterCode);
        return IsUndeclaredLanguage(lang)
            ? LanguageAutoDetect.AutoDetectGoogleLanguageOrNull(subtitle) ?? string.Empty
            : lang;
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

    private static List<LoadedTrack> LoadHdDvdSup(string filePath, ConversionOptions options)
    {
        var subtitle = ImageOcrLoader.LoadHdDvdSup(filePath, options);
        if (subtitle.Paragraphs.Count == 0)
        {
            throw new InvalidOperationException($"No subtitles recognised in HD-DVD sup file: {filePath}");
        }
        return [new LoadedTrack(subtitle, new SubRip(), string.Empty, null)];
    }

    private static List<LoadedTrack> LoadSpDvdSup(string filePath, ConversionOptions options)
    {
        var subtitle = ImageOcrLoader.LoadSpDvdSup(filePath, options);
        if (subtitle.Paragraphs.Count == 0)
        {
            throw new InvalidOperationException($"No subtitles recognised in DVD sup file: {filePath}");
        }
        return [new LoadedTrack(subtitle, new SubRip(), string.Empty, null)];
    }

    private static bool IsManzanita(string filePath)
    {
        try
        {
            return FileUtil.IsManzanita(filePath);
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }

    private static bool IsManzanitaDvbSubtitle(string filePath)
    {
        return IsManzanita(filePath) &&
               ManzanitaTransportStreamParser.GetStreamType(filePath) == ManzanitaTransportStreamParser.DvbSubtitleStreamType;
    }

    private static List<LoadedTrack> LoadManzanitaDvbSub(string filePath, ConversionOptions options)
    {
        var subtitle = ImageOcrLoader.LoadManzanitaDvbSub(filePath, options);
        if (subtitle.Paragraphs.Count == 0)
        {
            throw new InvalidOperationException($"No subtitles recognised in Manzanita DVB subtitle file: {filePath}");
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

    /// <summary>
    /// .avi/.divx with XSUB ("DivX") subtitles → one OCR'd track per subtitle stream. An AVI
    /// stream header carries no language, so multi-stream files are told apart by an
    /// "xsub_track&lt;n&gt;" suffix (the stream number); the common single-stream file keeps the
    /// plain output name.
    /// </summary>
    private static List<LoadedTrack> LoadXSub(string filePath, ConversionOptions options)
    {
        var streams = ImageOcrLoader.LoadXSub(filePath, options);
        if (streams.Count == 0)
        {
            throw new InvalidOperationException($"No XSUB (DivX) subtitles found in: {filePath}");
        }

        var tracks = new List<LoadedTrack>();
        foreach (var (subtitle, streamNumber) in streams)
        {
            if (options.TrackNumbers.Count > 0 &&
                (!streamNumber.HasValue || !options.TrackNumbers.Contains(streamNumber.Value)))
            {
                continue;
            }

            var languageSuffix = streams.Count > 1 && streamNumber.HasValue ? $"xsub_track{streamNumber.Value}" : string.Empty;
            tracks.Add(new LoadedTrack(subtitle, new SubRip(), languageSuffix, streamNumber));
        }

        if (tracks.Count == 0)
        {
            throw new InvalidOperationException(
                $"XSUB file has {streams.Count} subtitle stream(s) but none matched --track-number ({string.Join(",", options.TrackNumbers)}): {filePath}");
        }

        return tracks;
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
                // Every other container path honours --track-number; the transport-stream path
                // did not, so the filter was accepted and then silently ignored and SE wrote one
                // output per teletext page, per ARIB language and per DVB PID.
                if (options.TrackNumbers.Count > 0 && !options.TrackNumbers.Contains(pidEntry.Key))
                {
                    continue;
                }

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

            // ARIB STD-B24 captions (ISDB broadcasts) — also text
            foreach (var pidEntry in parser.AribSubtitlesLookup)
            {
                if (options.TrackNumbers.Count > 0 && !options.TrackNumbers.Contains(pidEntry.Key))
                {
                    continue;
                }

                foreach (var languageEntry in pidEntry.Value)
                {
                    if (languageEntry.Value.Count == 0)
                    {
                        continue;
                    }

                    var languageCode = string.Empty;
                    if (parser.AribLanguageLookup.TryGetValue(pidEntry.Key, out var languageCodes))
                    {
                        languageCodes.TryGetValue(languageEntry.Key, out languageCode);
                    }

                    var subtitle = new Subtitle();
                    subtitle.Paragraphs.AddRange(languageEntry.Value);
                    subtitle.Renumber();
                    var trackName = string.IsNullOrEmpty(languageCode)
                        ? $"arib_{pidEntry.Key}"
                        : $"arib_{pidEntry.Key}_{languageCode}";
                    tracks.Add(new LoadedTrack(subtitle, new SubRip(), trackName, pidEntry.Key));
                }
            }

            // CEA-608/708 closed captions from the video stream (ATSC/cable broadcasts) — also text
            foreach (var pidEntry in parser.ClosedCaptionSubtitlesLookup)
            {
                if (options.TrackNumbers.Count > 0 && !options.TrackNumbers.Contains(pidEntry.Key))
                {
                    continue;
                }

                foreach (var trackEntry in pidEntry.Value)
                {
                    if (trackEntry.Value.Count == 0)
                    {
                        continue;
                    }

                    var subtitle = new Subtitle();
                    subtitle.Paragraphs.AddRange(trackEntry.Value);
                    subtitle.Renumber();
                    var trackName = trackEntry.Key > ClosedCaptionExtractor.Cea708TrackKeyOffset
                        ? $"cea708_{pidEntry.Key}_s{trackEntry.Key - ClosedCaptionExtractor.Cea708TrackKeyOffset}"
                        : $"cea608_{pidEntry.Key}_cc{trackEntry.Key}";
                    tracks.Add(new LoadedTrack(subtitle, new SubRip(), trackName, pidEntry.Key));
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
                    if (options.TrackNumbers.Count > 0 && !options.TrackNumbers.Contains(pid))
                    {
                        continue;
                    }

                    tracks.Add(new LoadedTrack(subtitle, new SubRip(), $"dvb_pid{pid}", pid));
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLineInterpolated($"[yellow]Warning: DVB-sub OCR failed: {ex.Message}[/]");
            }
        }

        if (tracks.Count == 0)
        {
            if (options.TrackNumbers.Count > 0)
            {
                throw new InvalidOperationException(
                    $"Transport stream contained no subtitle stream matching --track-number ({string.Join(",", options.TrackNumbers)}): {filePath}");
            }

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
    /// <summary>
    /// True when a sanitized track language carries no usable information: empty, or the
    /// ISO 639-2 "und" (undetermined) code that muxers like ffmpeg write when no language
    /// was declared.
    /// </summary>
    internal static bool IsUndeclaredLanguage(string? lang) =>
        string.IsNullOrEmpty(lang) || lang.Equals("und", StringComparison.OrdinalIgnoreCase);

    internal static string SanitizeLang(string? lang)
    {
        if (string.IsNullOrWhiteSpace(lang))
        {
            return string.Empty;
        }
        return lang.RemoveChar('?').RemoveChar('!').RemoveChar('*').RemoveChar(',').RemoveChar('/').Trim();
    }
}
