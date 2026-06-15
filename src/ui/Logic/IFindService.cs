using System.Collections.Generic;
using static Nikse.SubtitleEdit.Logic.FindService;

namespace Nikse.SubtitleEdit.Logic;

public interface IFindService
{
    string SearchText { get; set; }
    int CurrentLineNumber { get; set; }
    int CurrentTextIndex { get; set; }
    string CurrentTextFound { get; set; }
    bool WholeWord { get; set; }
    FindMode CurrentFindMode { get; set; }
    IReadOnlyList<string> SearchHistory { get; }

    void Initialize(List<string> textLines, int currentLineIndex, bool wholeWord, FindMode findMode);
    int FindNext(string searchText, List<string> textLines, int currentLineIndex, int startTextIndex);
    int FindPrevious(string searchText, List<string> textLines, int currentLineIndex, int startTextIndex);
    int ReplaceAll(string searchText, string replaceText);
    int Count(string searchText);
    int Count(string searchText, IReadOnlyList<string> textLines, bool wholeWord, FindMode findMode);
    List<(int LineIndex, int TextIndex, string FoundText)> FindAll(string searchText);
    void Reset();
    void RemoveFromSearchHistory(string searchText);
}