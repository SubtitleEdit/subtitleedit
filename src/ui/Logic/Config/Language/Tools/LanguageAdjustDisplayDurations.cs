namespace Nikse.SubtitleEdit.Logic.Config.Language.Tools;

public class LanguageAdjustDisplayDurations
{
    public string Title { get; set; }
    public string AdjustVia { get; set; }
    public string Recalculate { get; set; }
    public string AddSeconds { get; set; }
    public string Note { get; set; }
    public string Fixed { get; set; }
    public string ExtendOnly { get; set; }
    public string RecalculateRequiresOcrNote { get; set; }

    public LanguageAdjustDisplayDurations()
    {
        Title = "Adjust durations";
        AdjustVia = "Adjust via";
        AddSeconds = "Add seconds";
        Recalculate = "Recalculate";
        Note = "Note: Display time will not overlap start time of next text";
        Fixed = "Fixed";
        ExtendOnly = "Extend only";
        RecalculateRequiresOcrNote = "Recalculate is unavailable — one or more subtitles have no OCR text. Run OCR first.";
    }
}