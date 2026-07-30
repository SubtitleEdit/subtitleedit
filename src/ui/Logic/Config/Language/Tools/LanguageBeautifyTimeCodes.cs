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
    }
}
