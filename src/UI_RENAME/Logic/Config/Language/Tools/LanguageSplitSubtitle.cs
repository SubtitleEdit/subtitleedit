namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageSplitSubtitle
{
    public string Title { get; set; }
    public string NumberOfEqualParts { get; set; }
    public string SaveSplitParts { get; set; }
    public string SubtitleSplitIntoXParts { get; set; }
    public string XPartsSavedInFormatYToFolder { get; set; }

    public LanguageSplitSubtitle()
    {
        Title = "Split subtitle";
        SaveSplitParts = "_Save split parts";
        NumberOfEqualParts = "Number of equal parts";
        SubtitleSplitIntoXParts = "Subtitle split into {0} parts.";
        XPartsSavedInFormatYToFolder = "{0} parts saved in format {1} to folder:";
    }
}