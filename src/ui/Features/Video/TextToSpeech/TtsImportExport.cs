using System.Collections.Generic;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.ActorVoices;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech;

public class TtsImportExport
{
    public string VideoFileName { get; set; }
    public List<TtsImportExportItem> Items { get; set; }

    // Saved with the export so the cast travels with the audio. May be null/empty in older
    // exports — load paths must tolerate that.
    public List<ActorVoiceMapping> ActorVoiceMappings { get; set; }

    public TtsImportExport()
    {
        VideoFileName = string.Empty;
        Items = new List<TtsImportExportItem>();
        ActorVoiceMappings = new List<ActorVoiceMapping>();
    }
}
