namespace Nikse.SubtitleEdit.Logic.Config;

public class SeSpellCheck
{
    public string? LastLanguageDictionaryFile { get; set; }
    public bool PromptForUnknownOneLetterWords { get; set; } = false;
    public bool TreatInQuoteASIng { get; set; } = true;
}
