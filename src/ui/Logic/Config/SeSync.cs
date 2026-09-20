namespace Nikse.SubtitleEdit.Logic.Config;

public class SeSync
{
    public SeAdjustAllTimes AdjustAllTimes { get; set; } = new();
    public bool AdjustAllTimesRememberLineSelectionChoice { get; set; }
    public string AdjustAllTimesLineSelectionChoice { get; set; }

    public double ChangeFrameRateFrom { get; set; } = 23.976;
    public double ChangeFrameRateTo { get; set; } = 25.0;

    // Height of the drag-resizable waveform under the video in the sync dialogs (issue #14414).
    public double SetSyncPointWaveformHeight { get; set; } = 80;
    public double VisualSyncWaveformHeight { get; set; } = 80;

    public SeSync()
    {
        AdjustAllTimesLineSelectionChoice = string.Empty;
    }
}
