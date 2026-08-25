using CommunityToolkit.Mvvm.ComponentModel;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.DetectSpeakers;

public partial class DetectSpeakersRow : ObservableObject
{
    [ObservableProperty] private bool _isSelected;

    public int Number { get; }
    public string Show { get; }
    public string Speaker { get; }
    public string Text { get; }
    public TextSpeakerCandidate Candidate { get; }

    public DetectSpeakersRow(TextSpeakerCandidate candidate)
    {
        // An SDH-style name (ALL CAPS, "Speaker 1") is confidently a speaker; a mixed-case
        // "Note:" is listed for the user to judge, but not pre-checked.
        IsSelected = TextSpeakerDetector.IsConfidentSpeakerName(candidate.Speaker);
        Number = candidate.Paragraph.Number;
        Show = candidate.Paragraph.StartTime.ToDisplayString();
        Speaker = candidate.Speaker;
        Text = candidate.Paragraph.Text;
        Candidate = candidate;
    }
}
