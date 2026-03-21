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
    }
}
