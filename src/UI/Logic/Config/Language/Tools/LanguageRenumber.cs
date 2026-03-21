namespace Nikse.SubtitleEdit.Logic.Config.Language.Tools;

public class LanguageRenumber
{
    public string Title { get; set; }
    public string StartFromNumber { get; set; }

    public LanguageRenumber()
    {
        Title = "Renumber";
        StartFromNumber = "Start from number:";
    }
}
