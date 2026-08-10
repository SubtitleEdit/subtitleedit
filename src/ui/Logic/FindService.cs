using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic;

public partial class FindService : IFindService
{
    public string SearchText { get; set; } = string.Empty;
    public int CurrentLineNumber { get; set; } = -1;
    public int CurrentTextIndex { get; set; } = -1;
    public string CurrentTextFound { get; set; } = string.Empty;
    public bool CurrentMatchInOriginal { get; set; }
    public bool WholeWord { get; set; }
    public FindMode CurrentFindMode { get; set; } = FindMode.CaseInsensitive;

    private List<string> _textLines = new List<string>();
    private List<string>? _originalTextLines;
    private readonly List<string> _searchHistory = new List<string>();
    private const int MaxSearchHistoryItems = 10;

    // ReplaceAll/Count/FindAll call into regex mode once per line; caching the last-built Regex
    // avoids recompiling the same pattern for every line in the subtitle.
    private string? _cachedRegexPattern;
    private RegexOptions _cachedRegexOptions;
    private Regex? _cachedRegex;

    private Regex GetCachedRegex(string pattern, RegexOptions options = RegexOptions.None)
    {
        if (_cachedRegex == null || _cachedRegexPattern != pattern || _cachedRegexOptions != options)
        {
            _cachedRegex = new Regex(pattern, options);
            _cachedRegexPattern = pattern;
            _cachedRegexOptions = options;
        }

        return _cachedRegex;
    }

    public IReadOnlyList<string> SearchHistory => _searchHistory.AsReadOnly();

    public FindService()
    {
        foreach (var findHistory in Se.Settings.Tools.FindHistory)
        {
            _searchHistory.Add(findHistory);
        }
    }

    public void Initialize(List<string> textLines, int currentLineNumber, bool wholeWord, FindMode findMode, List<string>? originalTextLines = null)
    {
        _textLines = textLines;
        _originalTextLines = originalTextLines;
        CurrentLineNumber = Math.Max(-1, Math.Min(currentLineNumber, textLines.Count - 1));
        WholeWord = wholeWord;
        CurrentFindMode = findMode;
        ResetSearchState();
    }

    public int FindNext(string searchText, List<string> textLines, int startLineIndex, int startTextIndex, List<string>? originalTextLines = null, bool startInOriginal = false)
    {
        if (string.IsNullOrEmpty(searchText) || _textLines.Count == 0)
        {
            ResetSearchState();
            return -1;
        }

        SearchText = RegexUtils.EscapeNewLines(searchText);
        _textLines = textLines;
        _originalTextLines = originalTextLines;
        AddToSearchHistory(searchText);

        if (startLineIndex < 0)
        {
            startLineIndex = 0;
            startTextIndex = 0;
            startInOriginal = false;
        }
        else
        {
            if (startLineIndex >= _textLines.Count)
            {
                return NotFound();
            }

            // If we've reached the end of the current text, move on to the next column
            // (original text of the same line) or to the next line.
            if (startTextIndex >= GetLine(startLineIndex, startInOriginal).Length)
            {
                if (!startInOriginal && GetOriginalLine(startLineIndex) != null)
                {
                    startInOriginal = true;
                }
                else
                {
                    startLineIndex++;
                    startInOriginal = false;
                }

                startTextIndex = 0;
            }

            if (startLineIndex >= _textLines.Count)
            {
                return NotFound();
            }
        }

        var result = FindInList(searchText, startLineIndex, startTextIndex, startInOriginal);
        CurrentLineNumber = result.lineIndex;
        CurrentTextIndex = result.textIndex;
        CurrentTextFound = result.foundText;
        CurrentMatchInOriginal = result.inOriginal;

        return CurrentLineNumber;
    }

    /// <summary>
    /// Text of a line in the requested column - the main text, or the original text when
    /// an original subtitle is loaded (translator mode).
    /// </summary>
    private string GetLine(int lineIndex, bool original)
    {
        if (original)
        {
            return GetOriginalLine(lineIndex) ?? string.Empty;
        }

        return lineIndex >= 0 && lineIndex < _textLines.Count ? _textLines[lineIndex] : string.Empty;
    }

    /// <summary>
    /// Original text of a line, or null when no original subtitle is loaded.
    /// </summary>
    private string? GetOriginalLine(int lineIndex)
    {
        if (_originalTextLines == null || lineIndex < 0 || lineIndex >= _originalTextLines.Count)
        {
            return null;
        }

        return _originalTextLines[lineIndex];
    }

    private int NotFound()
    {
        ResetSearchState();
        return -1;
    }

    public int FindPrevious(string searchText, List<string> textLines, int startLineIndex, int startTextIndex, List<string>? originalTextLines = null, bool startInOriginal = false)
    {
        if (string.IsNullOrEmpty(searchText) || _textLines.Count == 0)
        {
            ResetSearchState();
            return -1;
        }

        SearchText = RegexUtils.EscapeNewLines(searchText);
        _textLines = textLines;
        _originalTextLines = originalTextLines;
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
                startInOriginal = GetOriginalLine(startLineIndex) != null;
                startTextIndex = GetLine(startLineIndex, startInOriginal).Length - 1;
            }

            // If we've reached the beginning of the current text, move back to the previous
            // column (main text of the same line) or to the previous line.
            if (startTextIndex < 0)
            {
                if (startInOriginal)
                {
                    startInOriginal = false;
                }
                else
                {
                    startLineIndex--;
                    startInOriginal = startLineIndex >= 0 && GetOriginalLine(startLineIndex) != null;
                }

                if (startLineIndex >= 0)
                {
                    startTextIndex = GetLine(startLineIndex, startInOriginal).Length - 1;
                }
            }

            if (startLineIndex < 0)
            {
                return NotFound();
            }
        }

        var result = FindInListReverse(searchText, startLineIndex, startTextIndex, startInOriginal);
        CurrentLineNumber = result.lineIndex;
        CurrentTextIndex = result.textIndex;
        CurrentTextFound = result.foundText;
        CurrentMatchInOriginal = result.inOriginal;

        return CurrentLineNumber;
    }

    public int Count(string searchText, IReadOnlyList<string> textLines, bool wholeWord, FindMode findMode, IReadOnlyList<string>? originalTextLines = null)
    {
        if (string.IsNullOrEmpty(searchText) || textLines == null || textLines.Count == 0)
        {
            return 0;
        }

        var total = 0;
        foreach (var line in textLines)
        {
            total += CountMatchesInLine(line, searchText, wholeWord, findMode);
        }

        if (originalTextLines != null)
        {
            foreach (var line in originalTextLines)
            {
                total += CountMatchesInLine(line, searchText, wholeWord, findMode);
            }
        }

        return total;
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
            var matches = GetAllMatchesInLine(_textLines[lineIndex], searchText, WholeWord, CurrentFindMode);
            foreach (var match in matches)
            {
                results.Add((lineIndex, match.Index, match.Value));
            }
        }

        return results;
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

        if (_originalTextLines != null)
        {
            for (int lineIndex = 0; lineIndex < _originalTextLines.Count; lineIndex++)
            {
                var replacedText = ReplaceInLine(_originalTextLines[lineIndex], searchText, replaceText);
                if (replacedText.replaced)
                {
                    _originalTextLines[lineIndex] = replacedText.newText;
                    totalReplacements += replacedText.replacementCount;
                }
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

    public void RemoveFromSearchHistory(string searchText)
    {
        _searchHistory.Remove(searchText);
    }

    private void ResetSearchState()
    {
        CurrentLineNumber = -1;
        CurrentTextIndex = -1;
        CurrentTextFound = string.Empty;
        CurrentMatchInOriginal = false;
    }

    private void AddToSearchHistory(string searchText)
    {
        if (string.IsNullOrEmpty(searchText))
        {
            return;
        }

        searchText = RegexUtils.EscapeNewLines(searchText);

        // Remove if already exists to move it to top
        _searchHistory.Remove(searchText);

        // Add to beginning of list (most recent first)
        _searchHistory.Insert(0, searchText);

        // Trim to max items
        while (_searchHistory.Count > MaxSearchHistoryItems)
        {
            _searchHistory.RemoveAt(_searchHistory.Count - 1);
        }

        Se.Settings.Tools.FindHistory = _searchHistory;
    }

    // Within a line the main text is searched before the original text, so a search resuming
    // from a match in the original column skips the main text of that line - SE 4 did the same
    // via its "match in original" flag (issue #13053).
    private (int lineIndex, int textIndex, string foundText, bool inOriginal) FindInList(string searchText, int startLineIndex, int startTextIndex, bool startInOriginal)
    {
        for (var i = startLineIndex; i < _textLines.Count; i++)
        {
            var first = i == startLineIndex;
            var textIndex = first ? startTextIndex : 0;

            if (!(first && startInOriginal))
            {
                var match = FindInLine(_textLines[i], searchText, textIndex);
                if (match.found)
                {
                    return (i, match.index, match.foundText, false);
                }

                textIndex = 0;
            }

            var original = GetOriginalLine(i);
            if (original != null)
            {
                var match = FindInLine(original, searchText, textIndex);
                if (match.found)
                {
                    return (i, match.index, match.foundText, true);
                }
            }
        }

        return (-1, -1, string.Empty, false);
    }

    private (int lineIndex, int textIndex, string foundText, bool inOriginal) FindInListReverse(string searchText, int startLineIndex, int startTextIndex, bool startInOriginal)
    {
        for (var i = startLineIndex; i >= 0; i--)
        {
            if (i >= _textLines.Count)
            {
                continue;
            }

            var first = i == startLineIndex;
            var original = GetOriginalLine(i);

            // Reverse of the forward order: original text first, then the main text.
            if (original != null && (!first || startInOriginal))
            {
                var textIndex = first ? startTextIndex : original.Length - 1;
                var match = FindInLineReverse(original, searchText, textIndex);
                if (match.found)
                {
                    return (i, match.index, match.foundText, true);
                }
            }

            var mainTextIndex = first && !startInOriginal ? startTextIndex : _textLines[i].Length - 1;
            var mainMatch = FindInLineReverse(_textLines[i], searchText, mainTextIndex);
            if (mainMatch.found)
            {
                return (i, mainMatch.index, mainMatch.foundText, false);
            }
        }

        return (-1, -1, string.Empty, false);
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
        replaceText = RegexUtils.FixNewLine(replaceText);
        searchText = RegexUtils.FixNewLine(searchText); // \r\n / \r in the pattern -> \n to match the text (#11956)
        try
        {
            var regex = GetCachedRegex(searchText);

            if (startIndex > 0 && maxReplacements == 1)
            {
                // For single replacement starting at a specific index
                var beforePart = line.Substring(0, startIndex);
                var afterPart = line.Substring(startIndex);

                // Match/replace line-feed-normalized so a \n pattern matches the whole \r\n break,
                // not just the \n (which would leave a dangling \r). See below (#12484).
                var newAfterPart = RegexUtils.ReplaceNewLineSafe(regex, afterPart, replaceText, 1, 0);
                var totalReplacements = RegexUtils.CountNewLineSafe(regex, afterPart) > 0 ? 1 : 0;

                return (totalReplacements > 0, beforePart + newAfterPart, totalReplacements);
            }
            else
            {
                // Count against line-feed-normalized text so a \n pattern matches the whole \r\n
                // break. Replacing the raw line instead left the \r behind, turning "A\r\nB" into
                // "A\r B" when replacing \n with a space (#12484). Bail on no match so lines that
                // don't match keep their original text (and line endings) untouched.
                var totalReplacements = RegexUtils.CountNewLineSafe(regex, line);
                if (totalReplacements == 0)
                {
                    return (false, line, 0);
                }

                var newText = maxReplacements == -1
                    ? RegexUtils.ReplaceNewLineSafe(regex, line, replaceText)
                    : RegexUtils.ReplaceNewLineSafe(regex, line, replaceText, maxReplacements, 0);

                if (maxReplacements != -1 && totalReplacements > maxReplacements)
                {
                    totalReplacements = maxReplacements;
                }

                return (true, newText, totalReplacements);
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
        var pattern = RegexUtils.BuildWholeWordPattern(searchText);
        var options = comparison == StringComparison.OrdinalIgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;

        try
        {
            var regex = GetCachedRegex(pattern, options);

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
            var pattern = RegexUtils.BuildWholeWordPattern(searchText);
            var options = comparison == StringComparison.OrdinalIgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;

            try
            {
                var match = GetCachedRegex(pattern, options).Match(searchLine);
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
            var pattern = RegexUtils.BuildWholeWordPattern(searchText);
            var options = comparison == StringComparison.OrdinalIgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;

            try
            {
                var matches = GetCachedRegex(pattern, options).Matches(searchLine);
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
            var originalLength = searchLine.Length;
            searchLine = NormalizeLineEndingsForRegex(searchLine, out var indexMap);
            var match = GetCachedRegex(RegexUtils.FixNewLine(searchText)).Match(searchLine);

            if (match.Success)
            {
                return (true, startIndex + MapNormalizedIndex(indexMap, match.Index, originalLength), match.Value);
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
            var originalLength = searchLine.Length;
            searchLine = NormalizeLineEndingsForRegex(searchLine, out var indexMap);

            // Advance by 1 after each match so overlapping matches are found
            // (e.g. two long lines sharing the \n between them).
            var regex = GetCachedRegex(RegexUtils.FixNewLine(searchText));
            Match? lastMatch = null;
            var pos = 0;
            while (pos < searchLine.Length)
            {
                var m = regex.Match(searchLine, pos);
                if (!m.Success) break;
                lastMatch = m;
                pos = m.Index + 1;
            }

            if (lastMatch != null)
            {
                return (true, MapNormalizedIndex(indexMap, lastMatch.Index, originalLength), lastMatch.Value);
            }
        }
        catch (ArgumentException)
        {
            return (false, -1, string.Empty);
        }

        return (false, -1, string.Empty);
    }

    private int CountMatchesInLine(string line, string searchText, bool wholeWord, FindMode findMode)
    {
        if (string.IsNullOrEmpty(line))
        {
            return 0;
        }

        switch (findMode)
        {
            case FindMode.RegularExpression:
                try
                {
                    var searchLine = NormalizeLineEndingsForRegex(line);
                    return GetCachedRegex(RegexUtils.FixNewLine(searchText)).Matches(searchLine).Count;
                }
                catch (ArgumentException)
                {
                    return 0;
                }

            default:
                var matches = GetAllMatchesInLine(line, searchText, wholeWord, findMode);
                return matches.Count;
        }
    }

    private List<FindMatch> GetAllMatchesInLine(string line, string searchText, bool wholeWord, FindMode findMode)
    {
        var matches = new List<FindMatch>();

        if (string.IsNullOrEmpty(line))
        {
            return matches;
        }

        switch (findMode)
        {
            case FindMode.RegularExpression:
                try
                {
                    var searchLine = NormalizeLineEndingsForRegex(line, out var indexMap);
                    var regexMatches = GetCachedRegex(RegexUtils.FixNewLine(searchText)).Matches(searchLine);
                    foreach (Match match in regexMatches)
                    {
                        matches.Add(new FindMatch(MapNormalizedIndex(indexMap, match.Index, line.Length), match.Value));
                    }
                }
                catch (ArgumentException)
                {
                    // Invalid regex pattern
                }
                break;

            default:
                var comparison = findMode == FindMode.CaseSensitive
                    ? StringComparison.Ordinal
                    : StringComparison.OrdinalIgnoreCase;

                if (wholeWord)
                {
                    var pattern = RegexUtils.BuildWholeWordPattern(searchText);
                    var options = findMode == FindMode.CaseInsensitive ? RegexOptions.IgnoreCase : RegexOptions.None;

                    try
                    {
                        var regexMatches = GetCachedRegex(pattern, options).Matches(line);
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
                        startIndex = index + Math.Max(1, searchText.Length);
                    }
                }
                break;
        }

        return matches;
    }

    // A null indexMap means the line had no '\r', i.e. normalization was the identity mapping.
    private static int MapNormalizedIndex(List<int>? indexMap, int normalizedIndex, int originalLength)
    {
        if (indexMap == null)
        {
            return normalizedIndex < originalLength ? normalizedIndex : originalLength;
        }

        return normalizedIndex < indexMap.Count ? indexMap[normalizedIndex] : originalLength;
    }

    private static string NormalizeLineEndingsForRegex(string line)
    {
        if (!line.Contains('\r'))
        {
            return line;
        }

        var normalized = new StringBuilder(line.Length);
        for (var i = 0; i < line.Length; i++)
        {
            if (line[i] == '\r')
            {
                normalized.Append('\n');
                if (i + 1 < line.Length && line[i + 1] == '\n')
                {
                    i++;
                }
            }
            else
            {
                normalized.Append(line[i]);
            }
        }

        return normalized.ToString();
    }

    private static string NormalizeLineEndingsForRegex(string line, out List<int>? indexMap)
    {
        if (!line.Contains('\r'))
        {
            // The common case: nothing to normalize, so the mapping is the identity - a null
            // map signals that to MapNormalizedIndex without allocating a per-line List.
            indexMap = null;
            return line;
        }

        indexMap = new List<int>(line.Length);

        var normalized = new StringBuilder(line.Length);
        for (var i = 0; i < line.Length; i++)
        {
            if (line[i] == '\r')
            {
                normalized.Append('\n');
                indexMap.Add(i);
                if (i + 1 < line.Length && line[i + 1] == '\n')
                {
                    i++;
                }
            }
            else
            {
                normalized.Append(line[i]);
                indexMap.Add(i);
            }
        }

        return normalized.ToString();
    }
}