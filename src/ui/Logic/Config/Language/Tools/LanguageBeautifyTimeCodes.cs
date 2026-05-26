namespace Nikse.SubtitleEdit.Logic.Config.Language.Tools;

public class LanguageBeautifyTimeCodes
{
    public string Title { get; set; }
    public string BeautifySettings { get; set; }
    public string SnapToFrames { get; set; }
    public string FrameGap { get; set; }
    public string MinDuration { get; set; }
    public string ShotChangeThreshold { get; set; }
    public string ShotChangeOffset { get; set; }
    public string Original { get; set; }
    public string Beautified { get; set; }
    public string SubtitlesCount { get; set; }
    public string ChangedCount { get; set; }
    public string ShotChangesCount { get; set; }
    public string ChangeXOfY { get; set; }
    public string NoChanges { get; set; }
    public string SnappedToShotChange { get; set; }
    public string PreviousChange { get; set; }
    public string NextChange { get; set; }

    public LanguageBeautifyTimeCodes()
    {
        Title = "Beautify time codes";
        BeautifySettings = "Beautify Settings";
        SnapToFrames = "Snap to Frames";
        FrameGap = "Frame Gap";
        MinDuration = "Min Duration (ms)";
        ShotChangeThreshold = "Shot Change Threshold (ms)";
        ShotChangeOffset = "Shot Change Offset (frames)";
        Original = "Original";
        Beautified = "Beautified";
        SubtitlesCount = "Subtitles";
        ChangedCount = "Changed";
        ShotChangesCount = "Shot changes";
        ChangeXOfY = "Change {0} of {1}";
        NoChanges = "No changes — every cue is already on target";
        SnappedToShotChange = "snapped to shot change";
        PreviousChange = "Previous change";
        NextChange = "Next change";
    }
}
