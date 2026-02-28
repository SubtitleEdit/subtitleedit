namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageMergeLineswithSameTimeCodes
{
    public string Title { get; set; }
    public string MaxMsDifference { get; set; }
    public string MakeDialog { get; set; }

    public LanguageMergeLineswithSameTimeCodes()
    {
        Title = "Merge lines with same time codes";
        MaxMsDifference = "Max difference (milliseconds)";
        MakeDialog = "Make dialogs";
    }
}