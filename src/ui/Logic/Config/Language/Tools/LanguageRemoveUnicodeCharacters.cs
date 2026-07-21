namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageRemoveUnicodeCharacters
{
    public string Title { get; set; }
    public string Character { get; set; }
    public string ReplaceWith { get; set; }
    public string Lines { get; set; }
    public string CharactersFoundX { get; set; }
    public string NoCharactersFound { get; set; }

    public LanguageRemoveUnicodeCharacters()
    {
        Title = "Remove/replace Unicode characters";
        Character = "Character";
        ReplaceWith = "Replace with";
        Lines = "Lines";
        CharactersFoundX = "Unicode characters found: {0}";
        NoCharactersFound = "No Unicode characters found";
    }
}
