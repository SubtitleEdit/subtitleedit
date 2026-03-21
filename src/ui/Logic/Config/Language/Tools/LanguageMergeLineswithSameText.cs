namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageMergeLineswithSameText
{
    public string Title { get; set; }
    public string MaxMsBetweenLines { get; set; }
    public string IncludeIncrementingLines { get; set; }

    public LanguageMergeLineswithSameText()
    {
        Title = "Merge lines with same text";
        MaxMsBetweenLines = "Max milliseconds between lines";
        IncludeIncrementingLines = "Include lines with incrementing text";
    }
}