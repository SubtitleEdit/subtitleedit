using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.VobSub;
using SkiaSharp;
using Paragraph = Nikse.SubtitleEdit.Core.Common.Paragraph;

namespace SeConv.Core;

/// <summary>
/// Extracts VobSub subpicture streams from DVD .VOB files into one or more
/// <c>.sub</c> + <c>.idx</c> pairs. Issue #15 in subtitleedit-cli — the
/// regular text-subtitle pipeline trips the 33 MB size guard on VOB files
/// because they're MPEG-PS video streams, not subtitle text.
///
/// The VOBs are ripped with <see cref="DvdSubtitleRipper"/>, the resulting merged
/// packs are rendered to bitmaps and written via <see cref="VobSubWriter"/>.
/// One output pair is produced per DVD subtitle stream (one stream = one
/// language, e.g. English on 0x20, Spanish on 0x21, ...) so multi-language
/// DVDs don't have their tracks collapsed together.
/// </summary>
internal static class VobSubExtractor
{
    /// <summary>One output produced by a successful extraction.</summary>
    public sealed record StreamOutput(string Path, int StreamId, int Written);

    /// <summary>The outputs plus a one line note on what was read (for the console).</summary>
    public sealed record ExtractionResult(IReadOnlyList<StreamOutput> Outputs, string Source, int EncryptedPacks = 0, int TotalPacks = 0);

    /// <summary>
    /// Parse <paramref name="vobFiles"/> (treated as one logical title) and write
    /// one .sub + .idx pair per discovered subpicture stream. <paramref name="subOutputPath"/>
    /// must already end in <c>.sub</c>; when there's more than one stream, the
    /// stream index is inserted before the extension (<c>movie.sub</c> →
    /// <c>movie.0.sub</c>, <c>movie.1.sub</c>, …) and the matching .idx is
    /// written alongside each one.
    ///
    /// When the title set's IFO (VTS_xx_0.IFO) is next to the VOBs it supplies PAL/NTSC, the
    /// palette and the stream languages, and - when the VOBs are the title set's VOB files - one
    /// program chain (<paramref name="dvdTitleNumber"/>, else the one with the most playing time)
    /// is ripped cell by cell with exact time codes. Otherwise every VOB is read and the PTS
    /// restarts are stitched from the NAV packs.
    /// </summary>
    public static ExtractionResult Extract(IReadOnlyList<string> vobFiles, string subOutputPath, int? dvdTitleNumber = null)
    {
        if (vobFiles.Count == 0)
        {
            throw new InvalidOperationException("No VOB files supplied.");
        }

        if (!subOutputPath.EndsWith(".sub", StringComparison.OrdinalIgnoreCase))
        {
            // Guard: VobSubWriter derives the .idx path with a literal
            // Substring(0, Length - 3) + "idx" — pass anything other than ".sub"
            // here and the .idx ends up at the wrong path (e.g. movie → "vie"+"idx"
            // = "vieidx"). Caller normalises, but assert just in case.
            throw new ArgumentException("subOutputPath must end in '.sub'", nameof(subOutputPath));
        }

        var ifoFileName = IfoParser.GetIfoFileName(vobFiles[0]);
        var ifo = ifoFileName == null ? null : new IfoParser(ifoFileName);
        if (ifo is { Type: not IfoParser.IfoType.VideoTitleSet })
        {
            ifo = null;
        }

        var title = ifo == null ? null : PickTitle(ifoFileName!, vobFiles, dvdTitleNumber);
        List<VobSubPack> packs;
        string source;
        if (title != null)
        {
            packs = DvdSubtitleRipper.Rip(vobFiles, title.ProgramChain);
            source = $"{Path.GetFileName(ifoFileName)} title {title.ProgramChain.Number} ({title.ProgramChain.Duration:hh\\:mm\\:ss})";
        }
        else
        {
            if (dvdTitleNumber.HasValue)
            {
                throw new InvalidOperationException(
                    "--track-number selects a DVD title, which needs the title set's IFO (VTS_xx_0.IFO) next to all of its VOB files (VTS_xx_1.VOB, ...).");
            }

            packs = DvdSubtitleRipper.Rip(vobFiles);
            source = ifo != null ? $"{Path.GetFileName(ifoFileName)} (palette, languages)" : "no IFO";
        }

        var isPal = ifo?.IsPal ?? true;
        var parser = new VobSubParser(isPal);
        parser.VobSubPacks.AddRange(packs);
        var allPacks = parser.MergeVobSubPacks();
        if (allPacks.Count == 0)
        {
            throw new InvalidOperationException(
                "No VobSub subtitle packets found in the input VOB(s). "
                + "DVD menu chunks (VTS_xx_0.VOB) usually carry no movie subtitles — they live in VTS_xx_1.VOB and later.");
        }

        // With the DVD's palette the sub picture units are copied as is (lossless, and no
        // decode/re-encode); without it they are re-rendered in SE's default colors.
        var palette = title?.ProgramChain.Palette is { Count: > 0 } titlePalette ? titlePalette : ifo?.Palette;

        // Group by DVD subpicture stream ID (one stream ≙ one subtitle language).
        // Sorting by Key keeps the per-stream output indices stable across reruns.
        var streams = allPacks
            .GroupBy(p => p.StreamId)
            .OrderBy(g => g.Key)
            .ToList();

        var outputs = new List<StreamOutput>(streams.Count);
        for (var i = 0; i < streams.Count; i++)
        {
            var streamId = streams[i].Key;
            var streamPacks = streams[i].OrderBy(p => p.StartTime.Ticks).ToList();

            var outputPath = streams.Count == 1
                ? subOutputPath
                : InsertStreamIndex(subOutputPath, i);

            var languageCode = ifo?.GetLanguageCode(streamId);
            var language = string.IsNullOrEmpty(languageCode) ? null : DvdSubtitleLanguage.GetLanguageOrNull(languageCode);
            var written = palette is { Count: > 0 }
                ? CopyOneStream(streamPacks, outputPath, isPal, streamId, language ?? DvdSubtitleLanguage.English, palette)
                : WriteOneStream(streamPacks, outputPath, isPal, streamId, language ?? DvdSubtitleLanguage.English);
            outputs.Add(new StreamOutput(outputPath, streamId, written));
        }

        return new ExtractionResult(outputs, source, DvdSubtitleRipper.CountEncrypted(packs), packs.Count);
    }

    /// <summary>
    /// The program chain to rip - only when the VOBs given are the title set's own VOB files, as
    /// cell sector numbers count from the start of VTS_xx_1.VOB.
    /// </summary>
    private static DvdTitle? PickTitle(string ifoFileName, IReadOnlyList<string> vobFiles, int? dvdTitleNumber)
    {
        var titleSetVobs = IfoParser.GetTitleVobFiles(Path.ChangeExtension(ifoFileName, ".IFO"));
        if (titleSetVobs.Count != vobFiles.Count ||
            !titleSetVobs.Select(Path.GetFullPath).SequenceEqual(vobFiles.Select(Path.GetFullPath), StringComparer.OrdinalIgnoreCase))
        {
            return null;
        }

        var titles = DvdTitle.Find(ifoFileName).Where(p => p.AvailableShare > 0).ToList();
        if (dvdTitleNumber.HasValue)
        {
            return titles.FirstOrDefault(p => p.ProgramChain.Number == dvdTitleNumber.Value) ??
                   throw new InvalidOperationException(
                       $"DVD title {dvdTitleNumber.Value} not found - {Path.GetFileName(ifoFileName)} has title(s): "
                       + string.Join(", ", titles.Select(p => $"{p.ProgramChain.Number} ({p.ProgramChain.Duration:hh\\:mm\\:ss})")));
        }

        return DvdTitle.GetDefault(titles);
    }
    /// <summary>
    /// Inserts <paramref name="index"/> before the <c>.sub</c> extension —
    /// <c>movie.sub</c> with index 2 → <c>movie.2.sub</c>.
    /// </summary>
    private static string InsertStreamIndex(string subOutputPath, int index)
    {
        var dir = Path.GetDirectoryName(subOutputPath) ?? string.Empty;
        var stem = Path.GetFileNameWithoutExtension(subOutputPath);
        return Path.Combine(dir, $"{stem}.{index}.sub");
    }

    private static int CopyOneStream(IReadOnlyList<VobSubMergedPack> packs, string outputPath, bool isPal, int streamId, DvdSubtitleLanguage language, IReadOnlyList<SKColor> palette)
    {
        using var writer = new VobSubWriter(outputPath, 720, isPal ? 576 : 480, streamId, language, palette);
        foreach (var pack in packs)
        {
            // the merged PES payloads can run past the unit (padding) - its first word is its size
            var data = pack.SubPictureData;
            var size = pack.SubPicture.SubPictureDateSize;
            if (size > 0 && size < data.Length)
            {
                data = data.AsSpan(0, size).ToArray();
            }

            writer.WriteSubPictureUnit(pack.StartTimeCode, data);
        }

        writer.WriteIdxFile();
        return packs.Count;
    }

    private static int WriteOneStream(IReadOnlyList<VobSubMergedPack> packs, string outputPath, bool isPal, int streamId, DvdSubtitleLanguage language)
    {
        var screenWidth = 720;
        var screenHeight = isPal ? 576 : 480;

        // SE's default subpicture colours when no .idx palette is available. The
        // bitmap is what determines on-screen colour, so internal consistency
        // matters more than matching the DVD's original palette here.
        var pattern = SKColors.Yellow;
        var emphasis = SKColors.Black;

        using var writer = new VobSubWriter(
            outputPath,
            screenWidth,
            screenHeight,
            bottomMargin: 0,
            leftRightMargin: 0,
            languageStreamId: streamId,
            pattern,
            emphasis,
            useInnerAntiAliasing: true,
            language);

        var written = 0;
        foreach (var pack in packs)
        {
            using var bmp = pack.GetBitmap();
            if (bmp is null)
            {
                continue;
            }

            // Preserve the original DVD display position so non-bottom-centered
            // cues (signs, top-positioned overlays) land where the disc placed
            // them — passing only BottomCenter would discard the SubPicture's
            // ImageDisplayArea and re-anchor every cue at the bottom.
            var pos = pack.GetPosition();
            var p = new Paragraph(pack.StartTimeCode, pack.EndTimeCode, string.Empty);
            writer.WriteParagraph(p, bmp, BluRayContentAlignment.BottomCenter, new SKPoint(pos.Left, pos.Top));
            written++;
        }

        writer.WriteIdxFile();
        return written;
    }
}
