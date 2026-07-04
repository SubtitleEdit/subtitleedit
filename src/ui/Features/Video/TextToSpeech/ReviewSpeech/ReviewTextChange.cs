namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.ReviewSpeech;

/// <summary>
/// A text edit made in the review window, keyed by the time codes the line had when the review
/// opened (waveform drags may change the paragraph's times afterwards). The main window uses the
/// times to find the matching subtitle line and offer to apply the edit (#12093) - numbers and
/// indices can't be used because the TTS pipeline may have renumbered (empty-line removal) or
/// merged sentences.
/// </summary>
public class ReviewTextChange
{
    public double StartMs { get; }
    public double EndMs { get; }
    public string NewText { get; }

    public ReviewTextChange(double startMs, double endMs, string newText)
    {
        StartMs = startMs;
        EndMs = endMs;
        NewText = newText;
    }
}
