using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.VobSub;
using SkiaSharp;
using Paragraph = Nikse.SubtitleEdit.Core.Common.Paragraph;

namespace SeConv.Core;

/// <summary>
/// Extracts the VobSub subpicture stream from one or more DVD .VOB files into a
/// single <c>.sub</c> + <c>.idx</c> pair. Issue #15 in subtitleedit-cli — the
/// regular text-subtitle pipeline trips the 33 MB size guard on VOB files
/// because they're MPEG-PS video streams, not subtitle text.
///
/// Each VOB is parsed with <see cref="VobSubParser"/>, the resulting merged packs
/// are rendered to bitmaps, and <see cref="VobSubWriter"/> writes them back out.
/// The output is the same format the GUI's batch converter produces for the
/// "VobSub" target, so mkvtoolnix / mkvmerge consume it directly.
/// </summary>
internal static class VobSubExtractor
{
    /// <summary>
    /// Extract subpicture packets from <paramref name="vobFiles"/> into a single
    /// .sub/.idx pair at <paramref name="subOutputPath"/>. Returns the number of
    /// merged subtitle packs written.
    /// </summary>
    public static int Extract(IReadOnlyList<string> vobFiles, string subOutputPath, bool isPal)
    {
        if (vobFiles.Count == 0)
        {
            throw new InvalidOperationException("No VOB files supplied.");
        }

        // Parse every VOB and accumulate merged packs. DVDs assign continuous PTS
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

        var screenWidth = 720;
        var screenHeight = isPal ? 576 : 480;

        // SE's default subpicture colours when no .idx palette is available. The
        // bitmap is what determines on-screen colour, so internal consistency
        // matters more than matching the DVD's original palette here.
        var pattern = SKColors.Yellow;
        var emphasis = SKColors.Black;

        using var writer = new VobSubWriter(
            subOutputPath,
            screenWidth,
            screenHeight,
            bottomMargin: 0,
            leftRightMargin: 0,
            languageStreamId: 32,
            pattern,
            emphasis,
            useInnerAntiAliasing: true,
            DvdSubtitleLanguage.English);

        var written = 0;
        foreach (var pack in allPacks)
        {
            using var bmp = pack.GetBitmap();
            if (bmp is null)
            {
                continue;
            }

            var p = new Paragraph(pack.StartTimeCode, pack.EndTimeCode, string.Empty);
            writer.WriteParagraph(p, bmp, BluRayContentAlignment.BottomCenter);
            written++;
        }

        writer.WriteIdxFile();
        return written;
    }
}
