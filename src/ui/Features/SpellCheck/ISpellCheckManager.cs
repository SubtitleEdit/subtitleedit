using Nikse.SubtitleEdit.Features.Main;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;

namespace Nikse.SubtitleEdit.Features.SpellCheck;

public interface ISpellCheckManager : ISpellChecker
{
    event SpellCheckManager.SpellCheckWordChangedHandler? OnWordChanged;
    bool IsWordCorrect(SpellCheckWord word, string allText);
    List<SpellCheckResult> CheckSpelling(ObservableCollection<SubtitleLineViewModel> subtitles, SpellCheckResult? startFrom = null, int? stopBeforeLineIndex = null);
    int NoOfChangedWords { get; set; }
    int NoOfSkippedWords { get; set; }
    int NoOfCorrectWords { get; set; }
    int NoOfNames { get; set; }
    int NoOfAddedWords { get; set; }
    void AddIgnoreWord(string word);
    void ChangeWord(string fromWord, string toWord, SpellCheckWord spellCheckWord, SubtitleLineViewModel p);
    void ChangeAllWord(string fromWord, string toWord, SpellCheckWord spellCheckWord, SubtitleLineViewModel p);
    void AddToNames(string currentWord);
    void AdToUserDictionary(string currentWord);
    void RemoveIgnoreWord(string word);
    void RemoveChangeAllWord(string fromWord);
    void RemoveFromNames(string word);
    void RemoveFromUserDictionary(string word);
    List<SpellCheckDictionaryDisplay> GetDictionaryLanguages(string dictionaryFolder);
    IWordSpellChecker? WordSpellChecker { get; set; }
}
