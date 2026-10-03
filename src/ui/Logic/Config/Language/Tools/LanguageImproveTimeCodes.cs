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
    public string CheckWithSpeechToText { get; set; }
    public string CheckWithSpeechToTextHint { get; set; }
    public string CheckWithSpeechToTextNotAvailableX { get; set; }
    public string TranscribingToCheck { get; set; }
    public string SpeechToTextFailed { get; set; }
    public string SpeechToTextX { get; set; }
    public string SpeechToTextConfirmedXDisputedY { get; set; }
    public string HardlyHeardX { get; set; }
    public string Heard { get; set; }
    public string HeardHint { get; set; }
    public string StatusConfirmedBySpeech { get; set; }
    public string StatusDisputedBySpeech { get; set; }
    public string StatusMovedWithSync { get; set; }
    public string SyncedFirstXY { get; set; }
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
    public string LinesToCheckX { get; set; }
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
    public string StatusAdjustedByHand { get; set; }
    public string AlignedWaveformHint { get; set; }
    public string PlayLine { get; set; }
    public string UndoAdjustment { get; set; }
    public string PlayPause { get; set; }
    public string PlayAligned { get; set; }
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
        CheckWithSpeechToText = "Check with speech-to-text";
        CheckWithSpeechToTextHint = "Transcribes the audio as well, and checks every move against where the words were actually heard. Large moves the speech confirms are ticked for you, and moves away from where a line is heard are left unticked for you to check. Also shows how much of each line was heard - a low share means the text differs from what is said. A subtitle that is further out of sync than the max shift - a constant offset, a drift from a frame rate mismatch, or jumps where scenes were cut - is synced by the heard words first. Uses Crisp ASR Parakeet, which takes a few minutes for a feature film with a GPU.";
        CheckWithSpeechToTextNotAvailableX = "Speech-to-text check is not available for {0}";
        TranscribingToCheck = "Transcribing to check the alignment...";
        SpeechToTextFailed = "Speech-to-text failed - the time codes were not checked against it.";
        SpeechToTextX = "Speech-to-text: {0}";
        SpeechToTextConfirmedXDisputedY = "Speech: {0} confirmed, {1} disputed";
        HardlyHeardX = "Speech-to-text heard only {0}% of the subtitle's words. Is the subtitle in the language that is spoken? The aligner cannot place a translation.";
        Heard = "Heard";
        HeardHint = "How much of the line speech-to-text heard. A low share means the text differs from what is said - where the aligner is most likely to be wrong.";
        StatusConfirmedBySpeech = "Confirmed by speech";
        StatusDisputedBySpeech = "Heard where it was";
        StatusMovedWithSync = "Moved with the sync";
        SyncedFirstXY = "Synced first: {0} s at the start, {1} s at the end";
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
        LargeMovesToCheckX = "{0} line(s) would move a long way on their own and are left unticked - listen to them (F5 plays the aligned line, Shift+F5 the original) and tick the ones that are right.";
        LinesToCheckX = "{0} line(s) are left unticked to check - listen to them (F5 plays the aligned line, Shift+F5 the original) and tick the ones that are right.";
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
        StatusAdjustedByHand = "Adjusted by hand";
        AlignedWaveformHint = "Drag a line to move it, or drag its edges to change its start or end.";
        PlayLine = "Play line";
        UndoAdjustment = "Undo adjustment";
        PlayPause = "Play / pause";
        PlayAligned = "Aligned";
        PlayAlignedHint = "Play the selected line with its aligned time codes";
        Apply = "Apply";
        ApplyHint = "Untick a line to keep its original time codes. Double-click a line to play it. Lines can also be moved or resized in the aligned waveform.";
    }
}
