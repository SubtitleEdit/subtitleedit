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
    public string IsolateSpeech { get; set; }
    public string IsolateSpeechHint { get; set; }
    public string IsolatingSpeech { get; set; }
    public string IsolateSpeechFailed { get; set; }
    public string ShowSpeechOnly { get; set; }
    public string ShowSpeechOnlyHint { get; set; }
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
    public string ToCheckX { get; set; }
    public string LargeMovesToCheckX { get; set; }
    public string ChangeXOfY { get; set; }
    public string PreviousChange { get; set; }
    public string NextChange { get; set; }
    public string StartShift { get; set; }
    public string EndShift { get; set; }
    public string StatusRetimed { get; set; }
    public string StatusMovedWithNeighbours { get; set; }
    public string StatusLargeMoveUnconfirmed { get; set; }
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
        MaxShiftHint = "How far the subtitle may be out of sync. A line whose start would move further is left as it is and flagged. Whatever the value, a single line that wants to move more than half a second further than the lines round it is not moved blindly: it follows its neighbours when they all moved, and is otherwise offered unticked for you to check.";
        AdjustStartTimes = "Adjust start times";
        AdjustEndTimes = "Adjust end times";
        IsolateSpeech = "Isolate speech first (slow)";
        IsolateSpeechHint = "Removes music and sound effects before aligning, so the aligner only hears the dialogue. Helps on lines spoken over loud music or action, but takes about as long as the audio itself with a GPU - and many times longer without one.";
        IsolatingSpeech = "Isolating speech... (this takes a while)";
        IsolateSpeechFailed = "Could not isolate the speech - aligned against the original audio instead.";
        ShowSpeechOnly = "Show speech only";
        ShowSpeechOnlyHint = "Draw the aligned waveform from the audio with music and sound effects removed; the original waveform stays as it is, for comparison. Available once the speech has been isolated - here, or with \"Show speech only\" in the main window's waveform.";
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
        ToCheckX = "To check: {0}";
        LargeMovesToCheckX = "{0} line(s) would move a long way on their own and are left unticked - play Original / Aligned and tick the ones that are right.";
        ChangeXOfY = "Change {0} of {1}";
        PreviousChange = "Previous change";
        NextChange = "Next change";
        StartShift = "Start";
        EndShift = "End";
        StatusRetimed = "Re-timed";
        StatusMovedWithNeighbours = "Moved with its neighbours";
        StatusLargeMoveUnconfirmed = "Large move - listen, tick to apply";
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
