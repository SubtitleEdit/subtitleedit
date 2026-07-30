namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageRemoveUnicodeCharacters
{
    public string Title { get; set; }
    public string CharactersFoundX { get; set; }
    public string NoCharactersFound { get; set; }

    public LanguageRemoveUnicodeCharacters()
    {
        Title = "Remove/replace Unicode characters";
        CharactersFoundX = "Unicode characters found: {0}";
        NoCharactersFound = "No Unicode characters found";
    }
}
