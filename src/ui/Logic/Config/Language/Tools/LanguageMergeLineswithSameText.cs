namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageMergeLineswithSameText
{
    public string MaxMsBetweenLines { get; set; }
    public string IncludeIncrementingLines { get; set; }

    public LanguageMergeLineswithSameText()
    {
        MaxMsBetweenLines = "Max milliseconds between lines";
        IncludeIncrementingLines = "Include lines with incrementing text";
    }
}