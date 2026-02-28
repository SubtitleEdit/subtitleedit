using Nikse.SubtitleEdit.Features.Main;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Features.SpellCheck;

public interface ISpellCheckManager
{
    event SpellCheckManager.SpellCheckWordChangedHandler? OnWordChanged;
    bool IsWordCorrect(string word);
    List<SpellCheckResult> CheckSpelling(ObservableCollection<SubtitleLineViewModel> subtitles, SpellCheckResult? startFrom = null);
    int NoOfChangedWords { get; set; }
    int NoOfSkippedWords { get; set; }
    bool Initialize(string dictionaryFile, string twoLetterLanguageCode);
    void AddIgnoreWord(string word);
    void ChangeWord(string fromWord, string toWord, SpellCheckWord spellCheckWord, SubtitleLineViewModel p);
    void ChangeAllWord(string fromWord, string toWord, SpellCheckWord spellCheckWord, SubtitleLineViewModel p);
    void AddToNames(string currentWord);
    void AdToUserDictionary(string currentWord);
    List<SpellCheckDictionaryDisplay> GetDictionaryLanguages(string dictionaryFolder);
    List<string> GetSuggestions(string word);
}
