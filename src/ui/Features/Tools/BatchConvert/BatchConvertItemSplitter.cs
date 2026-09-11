using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream;
using Nikse.SubtitleEdit.Features.Ocr.OcrSubtitle;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.BatchConvert;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert;

public interface IBatchConvertItemSplitter
{
    List<BatchConvertItem> LoadTransportStream(BatchConvertItem item, CancellationToken cancellationToken);
}

public class BatchConvertTransportStreamSplitter : IBatchConvertItemSplitter
{
    public List<BatchConvertItem> LoadTransportStream(BatchConvertItem item, CancellationToken cancellationToken)
    {
        return LoadTransportStream(item, Se.Settings.Tools.BatchConvert.GetTransportStreamExportSettings(), cancellationToken);
    }

    public static List<BatchConvertItem> LoadTransportStream(BatchConvertItem item, TransportStreamExportSettings tsSettings, CancellationToken cancellationToken)
    {
        var result = new List<BatchConvertItem>();

        var tsParser = new TransportStreamParser();
        tsParser.Parse(item.FileName, null);

        var programMapTableParser = new ProgramMapTableParser();
        programMapTableParser.Parse(item.FileName); // get languages

        if (!tsSettings.OnlyTeletext)
        {
            foreach (var packetId in tsParser.SubtitlePacketIds)
            {
                var language = string.Empty;
                if (programMapTableParser.GetSubtitlePacketIds().Count > 0)
                {
                    language = programMapTableParser.GetSubtitleLanguage(packetId);
                }

                var subtitles = tsParser.GetDvbSubtitles(packetId);
                if (subtitles.Count > 0)
                {
                    result.Add(MakeTrackItem(item, tsSettings, language, packetId, packetId, subtitle: null, new OcrSubtitleTransportStream(subtitles)));
                }
            }
        }

        // One PID can carry several teletext subtitle pages (e.g. 888 + 889 for a second
        // language); every page is its own subtitle, so do not stop at the first one.
        foreach (var pidAndPages in tsParser.TeletextSubtitlesLookup)
        {
            var language = programMapTableParser.GetSubtitleLanguage(pidAndPages.Key) ?? string.Empty;
            foreach (var pageAndParagraphs in pidAndPages.Value)
            {
                if (pageAndParagraphs.Value.Count > 0)
                {
                    result.Add(MakeTrackItem(item, tsSettings, language, pidAndPages.Key, pageAndParagraphs.Key, new Subtitle(pageAndParagraphs.Value), imageSubtitle: null));
                }
            }
        }

        foreach (var aribPid in tsParser.AribSubtitlesLookup)
        {
            foreach (var language in aribPid.Value)
            {
                if (language.Value.Count == 0)
                {
                    continue;
                }

                var languageCode = string.Empty;
                if (tsParser.AribLanguageLookup.TryGetValue(aribPid.Key, out var languageCodes))
                {
                    languageCodes.TryGetValue(language.Key, out languageCode);
                }

                result.Add(MakeTrackItem(item, tsSettings, languageCode ?? string.Empty, aribPid.Key, language.Key, new Subtitle(language.Value), imageSubtitle: null));
            }
        }

        return result;
    }

    /// <summary>
    /// One batch item per extracted track. With a file name ending template the output name is
    /// fixed here ("video" + ".en" + ".ts" - the converter swaps the extension), and the
    /// converter's own language post fix is switched off so the token is not added twice.
    /// </summary>
    private static BatchConvertItem MakeTrackItem(BatchConvertItem source, TransportStreamExportSettings tsSettings, string language, int pid, int trackId, Subtitle? subtitle, OcrSubtitleTransportStream? imageSubtitle)
    {
        var trackItem = new BatchConvertItem
        {
            Size = source.Size,
            Format = source.Format,
            FileName = source.FileName,
            LanguageCode = language,
            TrackNumber = pid.ToString(System.Globalization.CultureInfo.InvariantCulture),
            Subtitle = subtitle,
            ImageSubtitle = imageSubtitle,
        };

        var ending = TransportStreamFileNameEnding.Format(tsSettings.FileNameAppend, language, trackId);
        if (ending.Length > 0)
        {
            var baseName = Path.GetFileNameWithoutExtension(source.FileName);
            trackItem.OutputFileName = baseName + ending + Path.GetExtension(source.FileName);
            trackItem.OutputFileNameIncludesLanguage = true;
        }

        return trackItem;
    }
}
