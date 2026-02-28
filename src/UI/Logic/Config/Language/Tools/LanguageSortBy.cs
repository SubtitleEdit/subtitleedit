namespace Nikse.SubtitleEdit.Logic.Config.Language.Tools;

public class LanguageSortBy
{
    public string Title { get; set; }
    public string SortOrder { get; set; }

    public LanguageSortBy()
    {
        Title = "Sort subtitles";
        SortOrder = "Sort order";
    }
}