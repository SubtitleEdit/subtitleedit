
namespace Nikse.SubtitleEdit.Logic.Config.Language.Tools;

public class LanguageRomanize
{
    public string Title { get; set; }
    public string ToggleTitle { get; set; }
    public string Korean { get; set; }
    public string Japanese { get; set; }
    public string Russian { get; set; }
    public string OperationsTitle { get; set; }

    public LanguageRomanize()
    {
        Title = "Romanize";
        ToggleTitle = "Toggle below to select languages";
        Korean = "Korean";
        Japanese = "Japanese (Kana)";
        Russian = "Russian";
        OperationsTitle = "Toggle below to apply global alterations";
    }
}
