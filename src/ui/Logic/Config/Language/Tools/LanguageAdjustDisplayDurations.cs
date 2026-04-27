namespace Nikse.SubtitleEdit.Logic.Config.Language.Tools;

public class LanguageAdjustDisplayDurations
{
    public string Title { get; set; }
    public string AdjustVia { get; set; }
    public string Seconds { get; set; }
    public string Percent { get; set; }
    public string Recalculate { get; set; }
    public string AddSeconds { get; set; }
    public string SetAsPercent { get; set; }
    public string Note { get; set; }
    public string Fixed { get; set; }
    public string Milliseconds { get; set; }
    public string ExtendOnly { get; set; }
    public string EnforceDurationLimits { get; set; }
    public string CheckShotChanges { get; set; }
    public string BatchCheckShotChanges { get; set; }

    public LanguageAdjustDisplayDurations()
    {
        Title = "Adjust durations";
        AdjustVia = "Adjust via";
        AddSeconds = "Add seconds";
        SetAsPercent = "Set as percent of duration";
        Percent = "Percent";
        Recalculate = "Recalculate";
        Seconds = "Seconds";
        Note = "Note: Display time will not overlap start time of next text";
        Fixed = "Fixed";
        Milliseconds = "Milliseconds";
        ExtendOnly = "Extend only";
        EnforceDurationLimits = "Enforce minimum and maximum duration";
        CheckShotChanges = "Don't extend past shot changes";
        BatchCheckShotChanges = "Respect shot changes (if available)";
    }
}