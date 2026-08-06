namespace Nikse.SubtitleEdit.Logic.Config.Language.Tools;

public class LanguageBeautifyTimeCodes
{
    public string Title { get; set; }
    public string SnapToFrames { get; set; }
    public string MinDuration { get; set; }
    public string Original { get; set; }
    public string Beautified { get; set; }
    public string SubtitlesCount { get; set; }
    public string ChangedCount { get; set; }
    public string ShotChangesCount { get; set; }
    public string ChangeXOfY { get; set; }
    public string NoChanges { get; set; }
    public string SnappedToShotChange { get; set; }
    public string SnappedToFrame { get; set; }
    public string MinGapEnforced { get; set; }
    public string MinDurationEnforced { get; set; }
    public string MaxDurationEnforced { get; set; }
    public string NoReasonNote { get; set; }
    public string PreviousChange { get; set; }
    public string NextChange { get; set; }
    public string EditProfile { get; set; }
    public string BatchSnapToShotChanges { get; set; }
    public string BatchFrameRateFromVideo { get; set; }
    public string BatchFrameRateFixed { get; set; }
    public string BatchInfo { get; set; }

    public LanguageBeautifyTimeCodes()
    {
        Title = "Beautify time codes";
        SnapToFrames = "Snap to Frames";
        MinDuration = "Min Duration (ms)";
        Original = "Original";
        Beautified = "Beautified";
        SubtitlesCount = "Subtitles";
        ChangedCount = "Changed";
        ShotChangesCount = "Shot changes";
        ChangeXOfY = "Change {0} of {1}";
        NoChanges = "No changes — every cue is already on target";
        SnappedToShotChange = "snapped to shot change";
        SnappedToFrame = "snapped to frame";
        MinGapEnforced = "min. gap enforced";
        MinDurationEnforced = "min. duration enforced";
        MaxDurationEnforced = "max. duration enforced";
        NoReasonNote = "refined by profile rules";
        PreviousChange = "Previous change";
        NextChange = "Next change";
        EditProfile = "Edit profile...";
        BatchSnapToShotChanges = "Snap cues to shot changes (if available)";
        BatchFrameRateFromVideo = "Use frame rate from video file with same name as subtitle file";
        BatchFrameRateFixed = "Use fixed frame rate";
        BatchInfo = "Shot changes are read from a video file with the same name as the subtitle file, if one is found.";
    }
}
