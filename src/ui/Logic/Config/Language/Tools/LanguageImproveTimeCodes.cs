namespace Nikse.SubtitleEdit.Logic.Config.Language.Tools;

public class LanguageImproveTimeCodes
{
    public string Title { get; set; }
    public string Aligner { get; set; }
    public string AlignerHint { get; set; }
    public string MaxShift { get; set; }
    public string MaxShiftHint { get; set; }
    public string AdjustStartTimes { get; set; }
    public string AdjustEndTimes { get; set; }
    public string Align { get; set; }
    public string Original { get; set; }
    public string Aligned { get; set; }
    public string Intro { get; set; }
    public string ExtractingAudio { get; set; }
    public string AligningBatchXOfY { get; set; }
    public string ExtractAudioFailed { get; set; }
    public string ModelWillBeDownloaded { get; set; }
    public string SummaryXRetimedYKeptZSkipped { get; set; }
    public string MeanShiftX { get; set; }
    public string ChangeXOfY { get; set; }
    public string PreviousChange { get; set; }
    public string NextChange { get; set; }
    public string StartShift { get; set; }
    public string EndShift { get; set; }
    public string StatusRetimed { get; set; }
    public string StatusUnchanged { get; set; }
    public string StatusNoSpeech { get; set; }
    public string StatusShiftTooLarge { get; set; }
    public string StatusFailed { get; set; }
    public string PlayPause { get; set; }
    public string PlayOriginal { get; set; }
    public string PlayAligned { get; set; }
    public string PlayOriginalHint { get; set; }
    public string PlayAlignedHint { get; set; }
    public string Apply { get; set; }
    public string ApplyHint { get; set; }

    public LanguageImproveTimeCodes()
    {
        Title = "Improve time codes (forced alignment)";
        Aligner = "Aligner";
        AlignerHint = "The model that listens for each line in the audio. Best choices for the subtitle's language are listed first.";
        MaxShift = "Max shift (seconds)";
        MaxShiftHint = "A line whose start would move further than this is left as it is and flagged; an end that would move further is kept. The subtitle has to be roughly in sync already - use Synchronization first if it is not.";
        AdjustStartTimes = "Adjust start times";
        AdjustEndTimes = "Adjust end times";
        Align = "Align";
        Original = "Original";
        Aligned = "Aligned";
        Intro = "Pick an aligner and press \"Align\" to listen for every line in the audio and tighten its time codes.";
        ExtractingAudio = "Extracting audio...";
        AligningBatchXOfY = "Aligning... batch {0} of {1}";
        ExtractAudioFailed = "Could not extract the audio with ffmpeg.";
        ModelWillBeDownloaded = "{0} will be downloaded";
        SummaryXRetimedYKeptZSkipped = "Re-timed: {0}   ·   Kept: {1}   ·   No speech: {2}";
        MeanShiftX = "Mean shift: {0} ms";
        ChangeXOfY = "Change {0} of {1}";
        PreviousChange = "Previous change";
        NextChange = "Next change";
        StartShift = "Start";
        EndShift = "End";
        StatusRetimed = "Re-timed";
        StatusUnchanged = "Already in place";
        StatusNoSpeech = "No speech";
        StatusShiftTooLarge = "Kept - shift too large";
        StatusFailed = "Kept - aligner failed";
        PlayPause = "Play / pause";
        PlayOriginal = "Original";
        PlayAligned = "Aligned";
        PlayOriginalHint = "Play the selected line with its original time codes";
        PlayAlignedHint = "Play the selected line with its aligned time codes";
        Apply = "Apply";
        ApplyHint = "Untick a line to keep its original time codes. Double-click a line to play it.";
    }
}
