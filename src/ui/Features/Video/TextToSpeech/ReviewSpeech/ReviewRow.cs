using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.ReviewSpeech;

public partial class ReviewRow : ObservableObject
{
    [ObservableProperty] private bool _include;
    [ObservableProperty] private int _number;
    [ObservableProperty] private string _voice;
    [ObservableProperty] private string _cps;
    [ObservableProperty] private string _speed;
    [ObservableProperty] private Color _speedBackgroundColor;
    [ObservableProperty] private string _text;
    [ObservableProperty] private bool _hasHistory;
    [ObservableProperty] private bool _isPlaying;
    [ObservableProperty] private bool _isPlayingEnabled;

    public TtsStepResult StepResult { get; set; }
    public List<ReviewHistoryRow> HistoryItems { get; set; }

    public ReviewRow()
    {
        Include = true;
        Number = 0;
        Voice = string.Empty;
        Cps = string.Empty;
        Speed = string.Empty;
        SpeedBackgroundColor = Colors.AliceBlue; //TODO: (Color)Application.Current!.Resources[ThemeNames.BackgroundColor];
        Text = string.Empty;
        StepResult = new TtsStepResult();
        HistoryItems = new List<ReviewHistoryRow>();
        HasHistory = false;
    }

    internal void StartHistory()
    {
        HistoryItems.Add(new ReviewHistoryRow
        {
            Number = 1,
            FileName = StepResult.CurrentFileName,
            VoiceName = StepResult.Voice?.Name ?? string.Empty,
            Voice = StepResult.Voice,
            Speed = StepResult.SpeedFactor,
        });
    }

    internal void AddHistory(Voices.Voice voice, TtsResult speakResult)
    {
        HistoryItems.Add(new ReviewHistoryRow
        {
            Number = HistoryItems.Count + 1,
            FileName = speakResult.FileName,
            VoiceName = voice.Name,
            Voice = voice,
            Speed = StepResult.SpeedFactor,
        });

        HasHistory = true;
    }
}