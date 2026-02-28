using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic;

public partial class FindService : IFindService
{
    public string SearchText { get; set; } = string.Empty;
    public int CurrentLineNumber { get; set; } = -1;
    public int CurrentTextIndex { get; set; } = -1;
    public string CurrentTextFound { get; set; } = string.Empty;
    public bool WholeWord { get; set; }
    public FindMode CurrentFindMode { get; set; } = FindMode.CaseInsensitive;

    private List<string> _textLines = new List<string>();
    private readonly List<string> _searchHistory = new List<string>();
    private const int MaxSearchHistoryItems = 10;

    public IReadOnlyList<string> SearchHistory => _searchHistory.AsReadOnly();

    public void Initialize(List<string> textLines, int currentLineNumber, bool wholeWord, FindMode findMode)
    {
        _textLines = textLines;
        CurrentLineNumber = Math.Max(-1, Math.Min(currentLineNumber, textLines.Count - 1));
        WholeWord = wholeWord;
        CurrentFindMode = findMode;
        ResetSearchState();
    }

    public int FindNext(string searchText, List<string> textLines, int startLineIndex, int startTextIndex)
    {
        if (string.IsNullOrEmpty(searchText) || _textLines.Count == 0)
        {
            ResetSearchState();
            return -1;
        }

        SearchText = searchText;
        _textLines = textLines;
        AddToSearchHistory(searchText);

        if (startLineIndex < 0)
        {
            startLineIndex = 0;
            startTextIndex = 0;
        }
        else
        {
            if (startLineIndex >= _textLines.Count)
            {
                return NotFound();
            }

            // If we've reached the end of current line, move to next line
            if (startTextIndex >= _textLines[startLineIndex].Length)
            {
                startLineIndex++;
                startTextIndex = 0;
            }

            if (startLineIndex >= _textLines.Count)
            {
                return NotFound();
            }
        }

        var result = FindInList(searchText, startLineIndex, startTextIndex);
        CurrentLineNumber = result.lineIndex;
        CurrentTextIndex = result.textIndex;
        CurrentTextFound = result.foundText;

        return CurrentLineNumber;
    }

    private int NotFound()
    {
        ResetSearchState();
        return -1;
    }

    public int FindPrevious(string searchText, List<string> textLines, int startLineIndex, int startTextIndex)
    {
        if (string.IsNullOrEmpty(searchText) || _textLines.Count == 0)
        {
            ResetSearchState();
            return -1;
        }

        SearchText = searchText;
        _textLines = textLines;
        AddToSearchHistory(searchText);

        if (startLineIndex < 0)
        {
            return NotFound();
        }
        else
        {
            if (startLineIndex >= _textLines.Count)
            {
                startLineIndex = _textLines.Count - 1;
                startTextIndex = _textLines[startLineIndex].Length - 1;
            }

            // If we've reached the beginning of current line, move to previous line
            if (startTextIndex < 0)
            {
                startLineIndex--;
                if (startLineIndex >= 0)
                {
                    startTextIndex = _textLines[startLineIndex].Length - 1;
                }
            }

            if (startLineIndex < 0)
            {
                return NotFound();
            }
        }

        var result = FindInListReverse(searchText, startLineIndex, startTextIndex);
        CurrentLineNumber = result.lineIndex;
        CurrentTextIndex = result.textIndex;
        CurrentTextFound = result.foundText;

        return CurrentLineNumber;
    }

    public int Count(string searchText)
    {
        if (string.IsNullOrEmpty(searchText) || _textLines.Count == 0)
        {
            return 0;
        }

        int count = 0;
        for (int i = 0; i < _textLines.Count; i++)
        {
            count += CountMatchesInLine(_textLines[i], searchText);
        }

        return count;
    }

    public List<(int LineIndex, int TextIndex, string FoundText)> FindAll(string searchText)
    {
        var results = new List<(int LineIndex, int TextIndex, string FoundText)>();

        if (string.IsNullOrEmpty(searchText) || _textLines.Count == 0)
        {
            return results;
        }

        for (int lineIndex = 0; lineIndex < _textLines.Count; lineIndex++)
        {
            var matches = GetAllMatchesInLine(_textLines[lineIndex], searchText);
            foreach (var match in matches)
            {
                results.Add((lineIndex, match.Index, match.Value));
            }
        }

        return results;
    }

    public int ReplaceNext(string searchText, string replaceText, List<string> textLines, int startLineIndex, int startTextIndex)
    {
        if (string.IsNullOrEmpty(searchText) || textLines == null || textLines.Count == 0)
        {
            ResetSearchState();
            return -1;
        }

        SearchText = searchText;
        _textLines = textLines;
        AddToSearchHistory(searchText);

        if (startLineIndex < 0)
        {
            startLineIndex = 0;
            startTextIndex = 0;
        }
        else
        {
            if (startLineIndex >= _textLines.Count)
            {
                return NotFound();
            }

            // If we've reached the end of current line, move to next line
            if (startTextIndex >= _textLines[startLineIndex].Length)
            {
                startLineIndex++;
                startTextIndex = 0;
            }

            if (startLineIndex >= _textLines.Count)
            {
                return NotFound();
            }
        }

        var result = FindInList(searchText, startLineIndex, startTextIndex);
        if (result.lineIndex == -1)
        {
            return NotFound();
        }

        // Perform the replacement
        var replacedText = ReplaceInLine(_textLines[result.lineIndex], searchText, replaceText, result.textIndex, 1);
        if (replacedText.replaced)
        {
            _textLines[result.lineIndex] = replacedText.newText;
            CurrentLineNumber = result.lineIndex;
            CurrentTextIndex = result.textIndex;
            CurrentTextFound = replaceText ?? string.Empty;
            return result.lineIndex;
        }

        return NotFound();
    }

    public int ReplaceAll(string searchText, string replaceText)
    {
        if (string.IsNullOrEmpty(searchText) || _textLines.Count == 0)
        {
            return 0;
        }

        int totalReplacements = 0;

        for (int lineIndex = 0; lineIndex < _textLines.Count; lineIndex++)
        {
            var replacedText = ReplaceInLine(_textLines[lineIndex], searchText, replaceText);
            if (replacedText.replaced)
            {
                _textLines[lineIndex] = replacedText.newText;
                totalReplacements += replacedText.replacementCount;
            }
        }

        if (totalReplacements > 0)
        {
            AddToSearchHistory(searchText);
            ResetSearchState();
        }

        return totalReplacements;
    }

    public void Reset()
    {
        ResetSearchState();
    }

    public void ClearSearchHistory()
    {
        _searchHistory.Clear();
    }

    public void RemoveFromSearchHistory(string searchText)
    {
        _searchHistory.Remove(searchText);
    }

    private void ResetSearchState()
    {
        CurrentLineNumber = -1;
        CurrentTextIndex = -1;
        CurrentTextFound = string.Empty;
    }

    private void AddToSearchHistory(string searchText)
    {
        if (string.IsNullOrEmpty(searchText))
        {
            return;
        }

        // Remove if already exists to move it to top
        _searchHistory.Remove(searchText);

        // Add to beginning of list (most recent first)
        _searchHistory.Insert(0, searchText);

        // Trim to max items
        while (_searchHistory.Count > MaxSearchHistoryItems)
        {
            _searchHistory.RemoveAt(_searchHistory.Count - 1);
        }
    }

    private (int lineIndex, int textIndex, string foundText) FindInList(string searchText, int startLineIndex, int startTextIndex = 0)
    {
        for (var i = startLineIndex; i < _textLines.Count; i++)
        {
            var textIndex = i == startLineIndex ? startTextIndex : 0;
            var match = FindInLine(_textLines[i], searchText, textIndex);

            if (match.found)
            {
                return (i, match.index, match.foundText);
            }
        }

        return (-1, -1, string.Empty);
    }

    private (int lineIndex, int textIndex, string foundText) FindInListReverse(string searchText, int startLineIndex, int startTextIndex)
    {
        for (var i = startLineIndex; i >= 0; i--)
        {
            if (i >= _textLines.Count)
            {
                continue;
            }

            var textIndex = i == startLineIndex ? startTextIndex : _textLines[i].Length - 1;
            var match = FindInLineReverse(_textLines[i], searchText, textIndex);

            if (match.found)
            {
                return (i, match.index, match.foundText);
            }
        }

        return (-1, -1, string.Empty);
    }

    private (bool found, int index, string foundText) FindInLine(string line, string searchText, int startIndex = 0)
    {
        if (string.IsNullOrEmpty(line) || startIndex >= line.Length)
        {
            return (false, -1, string.Empty);
        }

        switch (CurrentFindMode)
        {
            case FindMode.CaseSensitive:
                return FindWithStringComparison(line, searchText, StringComparison.Ordinal, startIndex);

            case FindMode.CaseInsensitive:
                return FindWithStringComparison(line, searchText, StringComparison.OrdinalIgnoreCase, startIndex);

            case FindMode.RegularExpression:
                return FindWithRegex(line, searchText, startIndex);

            default:
                return (false, -1, string.Empty);
        }
    }

    private (bool found, int index, string foundText) FindInLineReverse(string line, string searchText, int startIndex)
    {
        if (string.IsNullOrEmpty(line) || startIndex < 0)
        {
            return (false, -1, string.Empty);
        }

        switch (CurrentFindMode)
        {
            case FindMode.CaseSensitive:
                return FindWithStringComparisonReverse(line, searchText, StringComparison.Ordinal, startIndex);

            case FindMode.CaseInsensitive:
                return FindWithStringComparisonReverse(line, searchText, StringComparison.OrdinalIgnoreCase, startIndex);

            case FindMode.RegularExpression:
                return FindWithRegexReverse(line, searchText, startIndex);

            default:
                return (false, -1, string.Empty);
        }
    }

    private (bool replaced, string newText, int replacementCount) ReplaceInLine(string line, string searchText, string replaceText, int startIndex = 0, int maxReplacements = -1)
    {
        if (string.IsNullOrEmpty(line))
        {
            return (false, line, 0);
        }

        replaceText = replaceText ?? string.Empty;

        switch (CurrentFindMode)
        {
            case FindMode.RegularExpression:
                return ReplaceWithRegex(line, searchText, replaceText, startIndex, maxReplacements);

            default:
                return ReplaceWithStringComparison(line, searchText, replaceText, startIndex, maxReplacements);
        }
    }

    private (bool replaced, string newText, int replacementCount) ReplaceWithRegex(string line, string searchText, string replaceText, int startIndex, int maxReplacements)
    {
        try
        {
            var regex = new Regex(searchText);

            if (startIndex > 0 && maxReplacements == 1)
            {
                // For single replacement starting at a specific index
                var beforePart = line.Substring(0, startIndex);
                var afterPart = line.Substring(startIndex);

                var newAfterPart = regex.Replace(afterPart, replaceText, 1);
                var totalReplacements = newAfterPart != afterPart ? 1 : 0;

                return (totalReplacements > 0, beforePart + newAfterPart, totalReplacements);
            }
            else
            {
                // Replace all or limited occurrences
                var newText = maxReplacements == -1
                    ? regex.Replace(line, replaceText)
                    : regex.Replace(line, replaceText, maxReplacements);

                var totalReplacements = regex.Matches(line).Count;
                if (maxReplacements != -1 && totalReplacements > maxReplacements)
                {
                    totalReplacements = maxReplacements;
                }

                return (newText != line, newText, totalReplacements);
            }
        }
        catch (ArgumentException)
        {
            return (false, line, 0);
        }
    }

    private (bool replaced, string newText, int replacementCount) ReplaceWithStringComparison(string line, string searchText, string replaceText, int startIndex, int maxReplacements)
    {
        var comparison = CurrentFindMode == FindMode.CaseSensitive
            ? StringComparison.Ordinal
            : StringComparison.OrdinalIgnoreCase;

        if (WholeWord)
        {
            return ReplaceWholeWordWithStringComparison(line, searchText, replaceText, startIndex, maxReplacements, comparison);
        }

        string workingLine = line;
        int totalReplacements = 0;
        int currentIndex = startIndex;

        while (currentIndex < workingLine.Length && (maxReplacements == -1 || totalReplacements < maxReplacements))
        {
            var index = workingLine.IndexOf(searchText, currentIndex, comparison);
            if (index == -1)
            {
                break;
            }

            workingLine = workingLine.Substring(0, index) + replaceText + workingLine.Substring(index + searchText.Length);
            totalReplacements++;
            currentIndex = index + replaceText.Length;

            // For single replacement, break after first replacement
            if (maxReplacements == 1)
            {
                break;
            }
        }

        return (totalReplacements > 0, workingLine, totalReplacements);
    }

    private (bool replaced, string newText, int replacementCount) ReplaceWholeWordWithStringComparison(string line, string searchText, string replaceText, int startIndex, int maxReplacements, StringComparison comparison)
    {
        var pattern = $@"\b{Regex.Escape(searchText)}\b";
        var options = comparison == StringComparison.OrdinalIgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;

        try
        {
            var regex = new Regex(pattern, options);

            if (startIndex > 0 && maxReplacements == 1)
            {
                // For single replacement starting at a specific index
                var beforePart = line.Substring(0, startIndex);
                var afterPart = line.Substring(startIndex);

                var newAfterPart = regex.Replace(afterPart, replaceText, 1);
                var totalReplacements = newAfterPart != afterPart ? 1 : 0;

                return (totalReplacements > 0, beforePart + newAfterPart, totalReplacements);
            }
            else
            {
                // Replace all or limited occurrences
                var newText = maxReplacements == -1
                    ? regex.Replace(line, replaceText)
                    : regex.Replace(line, replaceText, maxReplacements);

                var totalReplacements = regex.Matches(line).Count;
                if (maxReplacements != -1 && totalReplacements > maxReplacements)
                {
                    totalReplacements = maxReplacements;
                }

                return (newText != line, newText, totalReplacements);
            }
        }
        catch (ArgumentException)
        {
            return (false, line, 0);
        }
    }

    private (bool found, int index, string foundText) FindWithStringComparison(string line, string searchText, StringComparison comparison, int startIndex)
    {
        var searchLine = startIndex > 0 ? line.Substring(startIndex) : line;

        if (WholeWord)
        {
            var pattern = $@"\b{Regex.Escape(searchText)}\b";
            var options = comparison == StringComparison.OrdinalIgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;

            try
            {
                var match = Regex.Match(searchLine, pattern, options);
                if (match.Success)
                {
                    return (true, startIndex + match.Index, match.Value);
                }
            }
            catch (ArgumentException)
            {
                return (false, -1, string.Empty);
            }
        }
        else
        {
            var index = searchLine.IndexOf(searchText, comparison);
            if (index >= 0)
            {
                return (true, startIndex + index, searchText);
            }
        }

        return (false, -1, string.Empty);
    }

    private (bool found, int index, string foundText) FindWithStringComparisonReverse(string line, string searchText, StringComparison comparison, int startIndex)
    {
        var searchLine = line.Substring(0, Math.Min(startIndex + 1, line.Length));

        if (WholeWord)
        {
            var pattern = $@"\b{Regex.Escape(searchText)}\b";
            var options = comparison == StringComparison.OrdinalIgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;

            try
            {
                var matches = Regex.Matches(searchLine, pattern, options);
                if (matches.Count > 0)
                {
                    var lastMatch = matches[matches.Count - 1];
                    return (true, lastMatch.Index, lastMatch.Value);
                }
            }
            catch (ArgumentException)
            {
                return (false, -1, string.Empty);
            }
        }
        else
        {
            var index = searchLine.LastIndexOf(searchText, comparison);
            if (index >= 0)
            {
                return (true, index, searchText);
            }
        }

        return (false, -1, string.Empty);
    }

    private (bool found, int index, string foundText) FindWithRegex(string line, string searchText, int startIndex)
    {
        try
        {
            var searchLine = startIndex > 0 ? line.Substring(startIndex) : line;
            var match = Regex.Match(searchLine, searchText);

            if (match.Success)
            {
                return (true, startIndex + match.Index, match.Value);
            }
        }
        catch (ArgumentException)
        {
            return (false, -1, string.Empty);
        }

        return (false, -1, string.Empty);
    }

    private (bool found, int index, string foundText) FindWithRegexReverse(string line, string searchText, int startIndex)
    {
        try
        {
            var searchLine = line.Substring(0, Math.Min(startIndex + 1, line.Length));
            var matches = Regex.Matches(searchLine, searchText);

            if (matches.Count > 0)
            {
                var lastMatch = matches[matches.Count - 1];
                return (true, lastMatch.Index, lastMatch.Value);
            }
        }
        catch (ArgumentException)
        {
            return (false, -1, string.Empty);
        }

        return (false, -1, string.Empty);
    }

    private int CountMatchesInLine(string line, string searchText)
    {
        if (string.IsNullOrEmpty(line))
        {
            return 0;
        }

        switch (CurrentFindMode)
        {
            case FindMode.RegularExpression:
                try
                {
                    return Regex.Matches(line, searchText).Count;
                }
                catch (ArgumentException)
                {
                    return 0;
                }

            default:
                var matches = GetAllMatchesInLine(line, searchText);
                return matches.Count;
        }
    }

    private List<FindMatch> GetAllMatchesInLine(string line, string searchText)
    {
        var matches = new List<FindMatch>();

        if (string.IsNullOrEmpty(line))
        {
            return matches;
        }

        switch (CurrentFindMode)
        {
            case FindMode.RegularExpression:
                try
                {
                    var regexMatches = Regex.Matches(line, searchText);
                    foreach (Match match in regexMatches)
                    {
                        matches.Add(new FindMatch(match.Index, match.Value));
                    }
                }
                catch (ArgumentException)
                {
                    // Invalid regex pattern
                }
                break;

            default:
                var comparison = CurrentFindMode == FindMode.CaseSensitive
                    ? StringComparison.Ordinal
                    : StringComparison.OrdinalIgnoreCase;

                if (WholeWord)
                {
                    var pattern = $@"\b{Regex.Escape(searchText)}\b";
                    var options = CurrentFindMode == FindMode.CaseInsensitive ? RegexOptions.IgnoreCase : RegexOptions.None;

                    try
                    {
                        var regexMatches = Regex.Matches(line, pattern, options);
                        foreach (Match match in regexMatches)
                        {
                            matches.Add(new FindMatch(match.Index, match.Value));
                        }
                    }
                    catch (ArgumentException)
                    {
                        // Invalid regex pattern
                    }
                }
                else
                {
                    int startIndex = 0;
                    while (startIndex < line.Length)
                    {
                        var index = line.IndexOf(searchText, startIndex, comparison);
                        if (index == -1)
                        {
                            break;
                        }

                        matches.Add(new FindMatch(index, searchText));
                        startIndex = index + 1;
                    }
                }
                break;
        }

        return matches;
    }
}