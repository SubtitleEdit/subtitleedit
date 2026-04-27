using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech;

public class TtsImportExport
{
    public string VideoFileName { get; set; }
    public List<TtsImportExportItem> Items { get; set; }

    public TtsImportExport()
    {
        VideoFileName = string.Empty;
        Items = new List<TtsImportExportItem>();
    }
}
