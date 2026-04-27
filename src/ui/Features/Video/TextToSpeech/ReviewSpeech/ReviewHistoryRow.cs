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

    public ReviewHistoryRow()
    {
        FileName = string.Empty;
        VoiceName = string.Empty;
    }
}
