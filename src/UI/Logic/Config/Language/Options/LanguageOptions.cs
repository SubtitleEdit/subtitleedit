namespace Nikse.SubtitleEdit.Logic.Config.Language.Options;

public class LanguageOptions
{
    public LanguageSettings Settings { get; set; } = new();
    public LanguageSettingsShortcuts Shortcuts { get; set; } = new();
    public LanguageSettingsWordLists WordLists { get; set; } = new();
    public LanguageChooseLanguage ChooseLanguage { get; set; } = new();

    public LanguageOptions()
    {
    }
}