namespace Nikse.SubtitleEdit.Logic.Config;

public class SeSync
{
    public SeAdjustAllTimes AdjustAllTimes { get; set; } = new();
    public bool AdjustAllTimesRememberLineSelectionChoice { get; set; }
    public string AdjustAllTimesLineSelectionChoice { get; set; }

    public SeSync()
    {
        AdjustAllTimesLineSelectionChoice = string.Empty;
    }
}
