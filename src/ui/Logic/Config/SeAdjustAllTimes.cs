namespace Nikse.SubtitleEdit.Logic.Config;

public class SeAdjustAllTimes
{
    public double Seconds { get; set; }
    public double ExtendStartSeconds { get; set; }
    public double ExtendEndSeconds { get; set; }
    public bool IsAllSelected { get; set; }
    public bool IsSelectedLinesSelected { get; set; }
    public bool IsSelectedLinesAndForwardSelected { get; set; }

    public SeAdjustAllTimes()
    {
        Seconds = 0.1d;
        ExtendStartSeconds = 0.5d;
        ExtendEndSeconds = 0.5d;
        IsAllSelected = true;
    }
}
