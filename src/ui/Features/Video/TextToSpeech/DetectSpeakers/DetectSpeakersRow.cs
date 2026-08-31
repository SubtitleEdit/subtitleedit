namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.DetectSpeakers;

public class DetectSpeakersRow : SelectLinesRowBase
{
    public string Speaker { get; }
    public TextSpeakerCandidate Candidate { get; }

    public DetectSpeakersRow(TextSpeakerCandidate candidate)
        // An SDH-style name (ALL CAPS, "Speaker 1") is confidently a speaker; a mixed-case
        // "Note:" is listed for the user to judge, but not pre-checked.
        : base(TextSpeakerDetector.IsConfidentSpeakerName(candidate.Speaker),
            candidate.Paragraph.Number,
            candidate.Paragraph.StartTime.ToDisplayString(),
            candidate.Paragraph.Text)
    {
        Speaker = candidate.Speaker;
        Candidate = candidate;
    }
}
