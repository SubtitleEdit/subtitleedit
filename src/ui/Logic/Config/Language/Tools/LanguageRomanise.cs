
namespace Nikse.SubtitleEdit.Logic.Config.Language.Tools;

public class LanguageRomanize
{
    public string Title { get; set; }
    public string TitleSettings { get; set; }
    public string TitleLanguages { get; set; }
    public string Korean { get; set; }
    public string Japanese { get; set; }
    public string Russian { get; set; }

    public LanguageRomanize()
    {
        Title = "Romanize";
        TitleSettings = "Toggle/select below to change settings for operation";
        TitleLanguages = "Toggle below to select languages";
        Korean = "Korean";
        Japanese = "Japanese (Kana)";
        Russian = "Russian";
    }
}
