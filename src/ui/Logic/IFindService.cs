using System.Collections.Generic;
using static Nikse.SubtitleEdit.Logic.FindService;

namespace Nikse.SubtitleEdit.Logic;

public interface IFindService
{
    string SearchText { get; set; }
    int CurrentLineNumber { get; set; }
    int CurrentTextIndex { get; set; }
    string CurrentTextFound { get; set; }

    /// <summary>
    /// True when the last match was found in the original text column (translator mode).
    /// </summary>
    bool CurrentMatchInOriginal { get; set; }

    bool WholeWord { get; set; }
    FindMode CurrentFindMode { get; set; }
    IReadOnlyList<string> SearchHistory { get; }

    void Initialize(List<string> textLines, int currentLineIndex, bool wholeWord, FindMode findMode, List<string>? originalTextLines = null);
    int FindNext(string searchText, List<string> textLines, int currentLineIndex, int startTextIndex, List<string>? originalTextLines = null, bool startInOriginal = false);
    int FindPrevious(string searchText, List<string> textLines, int currentLineIndex, int startTextIndex, List<string>? originalTextLines = null, bool startInOriginal = false);
    int ReplaceAll(string searchText, string replaceText);
    int Count(string searchText, IReadOnlyList<string> textLines, bool wholeWord, FindMode findMode, IReadOnlyList<string>? originalTextLines = null);
    List<(int LineIndex, int TextIndex, string FoundText)> FindAll(string searchText);
    void Reset();
    void RemoveFromSearchHistory(string searchText);
}