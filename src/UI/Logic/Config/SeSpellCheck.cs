namespace Nikse.SubtitleEdit.Logic.Config;

public class SeSpellCheck
{
    public const string SpellCheckHunspell = "hunspell";
    public const string SpellCheckMsWord = "msword";

    public string SpellCheckProvider { get; set; }

    public string? LastLanguageDictionaryFile { get; set; }
    public bool PromptForUnknownOneLetterWords { get; set; } = false;
    public bool TreatInQuoteASIng { get; set; } = true;

    public SeSpellCheck()
    {
        SpellCheckProvider = SpellCheckHunspell;
        
    }
}
