using System.Collections.Generic;

// Kept in the original UI namespace so existing references resolve unchanged.
namespace Nikse.SubtitleEdit.Features.SpellCheck;

/// <summary>
/// Minimal spell-checking surface the OCR-fix engine (libuilogic) needs, so it does not depend on the
/// interactive ISpellCheckManager (which is UI-bound via SubtitleLineViewModel). The UI's
/// SpellCheckManager implements this; seconv can supply its own implementation. (#11744)
/// </summary>
public interface ISpellChecker
{
    bool Initialize(string dictionaryFile, string twoLetterLanguageCode);
    bool IsWordCorrect(string word);
    List<string> GetSuggestions(string word);
}
