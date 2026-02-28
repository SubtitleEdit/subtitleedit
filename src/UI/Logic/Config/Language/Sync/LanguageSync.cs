using System;

namespace Nikse.SubtitleEdit.Logic.Config.Language.Sync;

public class LanguageSync
{
    public string VisualSync { get; set; }
    public string Sync { get; set; }
    public string StartScene { get; set; }
    public string EndScene { get; set; }
    public string PlayTwoSecondsAndBack { get; set; }
    public string FindText { get; set; }
    public string ResolutionXDurationYFrameRateZ { get; set; }
    public string StartSceneMustComeBeforeEndScene { get; set; }
    public string GoToSubPos { get; set; }
    public string SpeedInPercentage { get; set; }
    public string FromDropFrameValue { get; set; }
    public string ToDropFrameValue { get; set; }
    public string AdjustAll { get; set; }
    public string AdjustSelectedLines { get; set; }
    public string AdjustSelectedLinesAndForward { get; set; }
    public string ToFrameRate { get;  set; }
    public string FromFrameRate { get;  set; }
    public string AdjustAllTimes { get; set; }
    public string ShowEarlier { get; set; }
    public string ShowLater { get; set; }
    public string ChangeFrameRate { get; set; }
    public string SetSyncPoint { get; set; }
    public string SyncPoints { get; set; }
    public string PointSync { get; set; }
    public string PointSyncViaOther { get; set; }
    public string AdjustmentX { get; set; }
    public string AdjustAllShortcuts { get; set; }

    public LanguageSync()
    {
        VisualSync = "Visual Sync";
        Sync = "Sync";
        StartScene = "Start scene";
        EndScene = "End scene";
        PlayTwoSecondsAndBack = "Play 2 secs & back";
        FindText = "Find text";
        ResolutionXDurationYFrameRateZ = "Resolution: {0}, duration: {1}, frame rate: {2}";
        StartSceneMustComeBeforeEndScene = "Start scene must come before end scene";
        GoToSubPos = "Go to sub pos";
        SpeedInPercentage = "Speed in %";
        FromDropFrameValue = "From drop-frame value";
        ToDropFrameValue = "To drop-frame value";
        AdjustAll = "Adjust all";
        AdjustSelectedLines = "Adjust selected lines";
        AdjustSelectedLinesAndForward = "Adjust selected lines and forward";
        ToFrameRate = "To frame rate";
        FromFrameRate = "From frame rate";
        AdjustAllTimes = "Adjust all times (show earlier/later)";
        ShowEarlier = "Show earlier";
        ShowLater = "Show later";
        ChangeFrameRate = "Change frame rate";
        SetSyncPoint = "Set sync point";
        SyncPoints = "Sync points";
        PointSync = "Point sync";
        PointSyncViaOther = "Point sync via other subtitle";
        AdjustmentX = "Adjustment: {0}";
        AdjustAllShortcuts = "Keyboard shortcuts:\r\n\r\n• Shift + Left/Right: Move 10 ms\r\n• Ctrl + Left/Right: Move 100 ms\r\n• Alt + Left/Right: Move 500 ms";
    }
}