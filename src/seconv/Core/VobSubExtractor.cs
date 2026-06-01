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
/// Each VOB is parsed with <see cref="VobSubParser"/>, the resulting merged
/// packs are rendered to bitmaps and written via <see cref="VobSubWriter"/>.
/// One output pair is produced per DVD subtitle stream (one stream = one
/// language, e.g. English on 0x20, Spanish on 0x21, ...) so multi-language
/// DVDs don't have their tracks collapsed together.
/// </summary>
internal static class VobSubExtractor
{
    /// <summary>One output produced by a successful extraction.</summary>
    public sealed record StreamOutput(string Path, int StreamId, int Written);

    /// <summary>
    /// Parse <paramref name="vobFiles"/> (treated as one logical title) and write
    /// one .sub + .idx pair per discovered subpicture stream. <paramref name="subOutputPath"/>
    /// must already end in <c>.sub</c>; when there's more than one stream, the
    /// stream index is inserted before the extension (<c>movie.sub</c> →
    /// <c>movie.0.sub</c>, <c>movie.1.sub</c>, …) and the matching .idx is
    /// written alongside each one.
    /// </summary>
    public static IReadOnlyList<StreamOutput> Extract(IReadOnlyList<string> vobFiles, string subOutputPath, bool isPal)
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

        // Parse every VOB into per-stream merged packs. DVDs assign continuous PTS
        // across VOB chunks of the same title, so packs from later VOBs naturally
        // land after earlier ones in playback time.
        var allPacks = new List<VobSubMergedPack>();
        foreach (var vob in vobFiles)
        {
            var parser = new VobSubParser(isPal);
            parser.Open(vob);
            allPacks.AddRange(parser.MergeVobSubPacks());
        }

        if (allPacks.Count == 0)
        {
            throw new InvalidOperationException(
                "No VobSub subtitle packets found in the input VOB(s). "
                + "DVD audio-only chunks (VTS_xx_0.VOB) carry no subtitles — subtitles live in VTS_xx_1.VOB and later.");
        }

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

            var written = WriteOneStream(streamPacks, outputPath, isPal, streamId);
            outputs.Add(new StreamOutput(outputPath, streamId, written));
        }

        return outputs;
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

    private static int WriteOneStream(IReadOnlyList<VobSubMergedPack> packs, string outputPath, bool isPal, int streamId)
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
            DvdSubtitleLanguage.English);

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
