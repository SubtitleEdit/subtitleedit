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

    /// <summary>
    /// Which columns the next find/replace covers. Only the replace window narrows it, and it sets
    /// it again on every action, so the scope lives for one replace window session: finding always
    /// covers both columns and resets this on the way in - the find window, and the find next /
    /// find previous shortcuts, which reach the service without opening a window. A find that
    /// silently skipped a column would have nothing on screen to say so.
    /// </summary>
    FindScope CurrentScope { get; set; }

    IReadOnlyList<string> SearchHistory { get; }

    void Initialize(List<string> textLines, int currentLineIndex, bool wholeWord, FindMode findMode, List<string>? originalTextLines = null);
    int FindNext(string searchText, List<string> textLines, int currentLineIndex, int startTextIndex, List<string>? originalTextLines = null, bool startInOriginal = false);
    int FindPrevious(string searchText, List<string> textLines, int currentLineIndex, int startTextIndex, List<string>? originalTextLines = null, bool startInOriginal = false);
    int ReplaceAll(string searchText, string replaceText);
    int Count(string searchText, IReadOnlyList<string> textLines, bool wholeWord, FindMode findMode, IReadOnlyList<string>? originalTextLines = null, FindScope scope = FindScope.TextAndOriginal);
    List<(int LineIndex, int TextIndex, string FoundText)> FindAll(string searchText);
    void Reset();
    void RemoveFromSearchHistory(string searchText);
}