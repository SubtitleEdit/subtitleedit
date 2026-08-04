namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageMergeLineswithSameTimeCodes
{
    public string MaxMsDifference { get; set; }
    public string MakeDialog { get; set; }

    public LanguageMergeLineswithSameTimeCodes()
    {
        MaxMsDifference = "Max difference (milliseconds)";
        MakeDialog = "Make dialogs";
    }
}