namespace Nikse.SubtitleEdit.Logic.Config;

public class SeAdjustAllTimes
{
    public double Seconds { get; set; }
    public bool IsAllSelected { get; set; }
    public bool IsSelectedLinesSelected { get; set; }
    public bool IsSelectedLinesAndForwardSelected { get; set; }

    public SeAdjustAllTimes()
    {
        Seconds = 0.1d;
        IsAllSelected = true;
    }
}
