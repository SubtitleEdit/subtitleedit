using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream;
using Nikse.SubtitleEdit.Features.Ocr.OcrSubtitle;
using Nikse.SubtitleEdit.UiLogic.BatchConvert;
using System.Collections.Generic;
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
        var result = new List<BatchConvertItem>();

        var tsParser = new TransportStreamParser();
        tsParser.Parse(item.FileName, null);

        var programMapTableParser = new ProgramMapTableParser();
        programMapTableParser.Parse(item.FileName); // get languages

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
                result.Add(new BatchConvertItem
                {
                    Size = item.Size,
                    Format = item.Format,
                    FileName = item.FileName,
                    LanguageCode = language,
                    ImageSubtitle = new OcrSubtitleTransportStream(tsParser, subtitles, item.FileName),
                });
            }
        }

        foreach (var i in tsParser.TeletextSubtitlesLookup.Keys)
        {
            var pid = tsParser.TeletextSubtitlesLookup[i];
            var paragraphs = pid.Values.First();
            if (paragraphs.Count > 0)
            {
                result.Add(new BatchConvertItem
                {
                    Size = item.Size,
                    Format = item.Format,
                    FileName = item.FileName,
                    Subtitle = new Subtitle(paragraphs),
                    LanguageCode = string.Empty,
                });
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

                result.Add(new BatchConvertItem
                {
                    Size = item.Size,
                    Format = item.Format,
                    FileName = item.FileName,
                    Subtitle = new Subtitle(language.Value),
                    LanguageCode = languageCode ?? string.Empty,
                });
            }
        }

        return result;
    }
}
