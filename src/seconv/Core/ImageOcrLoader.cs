using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
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
    /// Blu-Ray .sup → text via Tesseract.
    /// </summary>
    public static Subtitle LoadBluRaySup(string filePath, ConversionOptions options)
    {
        var log = new StringBuilder();
        var pcsList = BluRaySupParser.ParseBluRaySup(filePath, log);
        if (pcsList.Count == 0)
        {
            throw new InvalidOperationException($"No Blu-Ray sup subtitles found in: {filePath}");
        }

        using var ocr = OcrEngineFactory.Create(options);
        AnsiConsole.MarkupLine($"[dim]Running {ocr.Name} OCR on {pcsList.Count} Blu-Ray sup image(s)...[/]");
        return OcrPcsList(pcsList, ocr);
    }

    /// <summary>
    /// MKV PGS track (S_HDMV/PGS) → text via the configured OCR engine.
    /// </summary>
    public static Subtitle LoadMatroskaPgs(MatroskaFile matroska, MatroskaTrackInfo track, ConversionOptions options)
    {
        var pcsList = BluRaySupParser.ParseBluRaySupFromMatroska(track, matroska);
        if (pcsList.Count == 0)
        {
            throw new InvalidOperationException($"No PGS subtitles in MKV track #{track.TrackNumber}.");
        }
        using var ocr = OcrEngineFactory.Create(options);
        AnsiConsole.MarkupLine($"[dim]Running {ocr.Name} OCR on {pcsList.Count} MKV PGS image(s) (track #{track.TrackNumber})...[/]");
        return OcrPcsList(pcsList, ocr);
    }

    /// <summary>
    /// Transport stream DVB-sub → text via Tesseract. Returns one Subtitle per packet ID
    /// that has subtitles. Teletext PIDs are not handled here (they're already text;
    /// see <see cref="ContainerSubtitleLoader"/>).
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

        using var ocr = OcrEngineFactory.Create(options);

        foreach (var pid in parser.SubtitlePacketIds)
        {
            var dvbSubtitles = parser.GetDvbSubtitles(pid);
            if (dvbSubtitles.Count == 0)
            {
                continue;
            }

            AnsiConsole.MarkupLine($"[dim]Running {ocr.Name} OCR on {dvbSubtitles.Count} DVB-sub image(s) (PID {pid})...[/]");
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
                    var text = ocr.Recognize(bitmap);
                    if (!string.IsNullOrWhiteSpace(text))
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
        return results;
    }

    private static Subtitle OcrPcsList(List<BluRaySupParser.PcsData> pcsList, IOcrEngine ocr)
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
                var text = ocr.Recognize(bitmap);
                if (!string.IsNullOrWhiteSpace(text))
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
