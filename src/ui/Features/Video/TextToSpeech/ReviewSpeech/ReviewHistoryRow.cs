using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.ReviewSpeech;

public partial class ReviewHistoryRow : ObservableObject
{
    [ObservableProperty] private bool _isPlaying;
    [ObservableProperty] private bool _isPlayingEnabled;

    public int Number { get; set; }
    public string FileName { get; set; }
    public string VoiceName { get; set; }
    public Voice? Voice { get; set; }
    public float Speed { get; set; }

    // Snapshot of the engine settings that produced this history entry, so picking it back from
    // the history dialog can restore the full left-panel state (not just the voice). Empty for
    // legacy history rows that pre-date this field.
    public string EngineName { get; set; }
    public string Model { get; set; }
    public string Instruction { get; set; }

    public ReviewHistoryRow()
    {
        // The history dialog's only writer of this is StopPlay, which first runs from a watchdog
        // timer tick - so every play button opened disabled and clicks in that window did nothing.
        IsPlayingEnabled = true;
        FileName = string.Empty;
        VoiceName = string.Empty;
        EngineName = string.Empty;
        Model = string.Empty;
        Instruction = string.Empty;
    }
}
