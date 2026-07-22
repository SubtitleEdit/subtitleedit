using Nikse.SubtitleEdit.Core.Interfaces;

namespace Nikse.SubtitleEdit.UiLogic.SpellCheck;

public record WordSpellCheckLanguage(string Name, int LanguageId);

/// <summary>
/// Abstraction over the optional MS Word spell-check backend so the spell-check manager (moving to
/// libuilogic) does not have to reference the Windows-COM <c>WordSpellCheck</c> type, which stays in
/// the UI project. The UI injects a concrete <c>WordSpellCheck</c> instance.
/// </summary>
public interface IWordSpellChecker : IDoSpell
{
    WordSpellCheckLanguage? CurrentLanguage { get; set; }
    bool Initialize();
    List<WordSpellCheckLanguage> GetInstalledLanguages();
    List<string> GetSuggestions(string word);
}
