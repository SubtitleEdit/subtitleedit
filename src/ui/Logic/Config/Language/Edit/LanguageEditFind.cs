namespace Nikse.SubtitleEdit.Logic.Config.Language.Edit;

public class LanguageEditFind
{
    public string SearchTextWatermark { get; set; }
    public string WholeWord { get; set; }
    public string FindPrevious { get; set; }
    public string FindNext { get; set; }
    public string ReplaceAndFindNext { get; set; }
    public string ReplaceAll { get; set; }
    public string ReplaceTextWatermark { get; set; }

    public LanguageEditFind()
    {
        SearchTextWatermark = "Search text...";
        ReplaceTextWatermark = "Replace with...";
        WholeWord = "Whole word";
        FindPrevious = "Find _previous";
        FindNext = "_Find next";
        ReplaceAndFindNext = "_Replace & find next";
        ReplaceAll = "Replace _all";
    }
}