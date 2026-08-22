using Avalonia.Controls;
using Avalonia.Input;
using Nikse.SubtitleEdit.Logic;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Nikse.SubtitleEdit.Features.Tools.SplitBreakLongLines;

public partial class SplitBreakLongLinesViewModel : ObservableObject, IClosingCleanup
{
    [ObservableProperty] private ObservableCollection<SplitBreakLongLinesItem> _fixes;
    [ObservableProperty] private SplitBreakLongLinesItem? _selectedFix;

    [ObservableProperty] private ObservableCollection<SubtitleLineViewModel> _subtitles;
    [ObservableProperty] private SubtitleLineViewModel? _selectedSubtitle;

    [ObservableProperty] private bool _splitLongLines;
    [ObservableProperty] private int _singleLineMaxLength;
    [ObservableProperty] private int _maxNumberOfLines;

    [ObservableProperty] private bool _rebalanceLongLines;
    [ObservableProperty] private bool _rebalanceOnlyLinesTooLong;
    [ObservableProperty] private int _unbreakLinesShorterThan;

    [ObservableProperty] private string _fixesInfo;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }
    public List<SubtitleLineViewModel> AllSubtitlesFixed { get; set; }

    private List<SubtitleLineViewModel> _allSubtitles;
    private string _languageCode = "en";
    private bool _isFormatEbu;

    private readonly System.Timers.Timer _previewTimer;
    private volatile bool _isClosing;
    private bool _isDirty;

    public SplitBreakLongLinesViewModel()
    {
        Fixes = new ObservableCollection<SplitBreakLongLinesItem>();
        Subtitles = new ObservableCollection<SubtitleLineViewModel>();
        _allSubtitles = new List<SubtitleLineViewModel>();
        AllSubtitlesFixed = new List<SubtitleLineViewModel>();
        FixesInfo = string.Empty;

        LoadSettings();

        _previewTimer = new System.Timers.Timer(250);
        _previewTimer.Elapsed += PreviewTimerElapsed;
    }

    private void PreviewTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        if (_isClosing)
        {
            return;
        }

        _previewTimer.Stop();

        if (_isDirty)
        {
            _isDirty = false;
            UpdatePreview();
        }

        // Guard the restart: OnClosingCleanup may have disposed the timer while this handler ran,
        // and Start() on a disposed timer throws ObjectDisposedException (no longer swallowed on
        // modern .NET), crashing the app from a thread-pool thread. (#12739)
        if (!_isClosing)
        {
            _previewTimer.Start();
        }
    }

    public void OnClosingCleanup()
    {
        _isClosing = true;
        _previewTimer.StopAndDispose(PreviewTimerElapsed);
    }

    private void UpdatePreview()
    {
        Dispatcher.UIThread.Post(() =>
        {
            Subtitles.Clear();
            AllSubtitlesFixed.Clear();
            Fixes.Clear();

            var splitCount = 0;
            var rebalanceCount = 0;
            var maxCharactersPerSubtitle = MaxNumberOfLines * SingleLineMaxLength;

            // AutoBreakLine keeps text on one line only when it is strictly shorter than the
            // unbreak threshold, so a threshold at or above the single line max length means
            // "keep any text that fits on one line" - and capping there also prevents merging
            // to a single line that would exceed the max length (#12910).
            var mergeLinesShorterThan = UnbreakLinesShorterThan >= SingleLineMaxLength
                ? SingleLineMaxLength + 1
                : UnbreakLinesShorterThan;

            if (SplitLongLines)
            {
                var options = new SplitOptions
                {
                    MinimumGapMs = Se.Settings.General.MinimumBetweenLines.GetMilliseconds(),
                    AdjustTeletextRows = _isFormatEbu,
                    TeletextDoubleHeight = Configuration.Settings.SubtitleSettings.EbuStlTeletextUseDoubleHeight,
                };

                for (var index = 0; index < _allSubtitles.Count; index++)
                {
                    var item = new SubtitleLineViewModel(_allSubtitles[index]);

                    // Like SE4: a subtitle is not cut into several events when re-wrapping its
                    // lines is enough to make it fit - the rebalance pass below does that.
                    if (RebalanceLongLines && CanBeFixedByRebalancing(item.Text, mergeLinesShorterThan))
                    {
                        AllSubtitlesFixed.Add(item);
                        continue;
                    }

                    var splitLines = Split(item, maxCharactersPerSubtitle, SingleLineMaxLength, options);
                    if (splitLines.Count > 1)
                    {
                        splitCount++;
                        var originalPreview = GetTextPreview(item.Text, 50);
                        var firstSplitPreview = GetTextPreview(splitLines[0].Text, 50);
                        var fixDescription = string.Format(Se.Language.Tools.SplitBreakLongLines.SplitIntoXLines, splitLines.Count, originalPreview, firstSplitPreview);
                        var fixItem = new SplitBreakLongLinesItem(Se.Language.Tools.SplitBreakLongLines.SplitLongLine, index + 1, fixDescription, item);
                        Fixes.Add(fixItem);
                    }

                    AllSubtitlesFixed.AddRange(splitLines);
                }
            }
            else
            {
                // If not splitting, use original subtitles for rebalancing
                foreach (var subtitle in _allSubtitles)
                {
                    AllSubtitlesFixed.Add(new SubtitleLineViewModel(subtitle));
                }
            }

            if (RebalanceLongLines)
            {
                for (var index = 0; index < AllSubtitlesFixed.Count; index++)
                {
                    var item = AllSubtitlesFixed[index];
                    if (RebalanceOnlyLinesTooLong && !HasLineTooLong(item.Text, SingleLineMaxLength, MaxNumberOfLines))
                    {
                        // An intentionally unbalanced subtitle can be editorially correct - when
                        // every line already fits, its existing line breaks are kept.
                        continue;
                    }

                    var rebalancedText = Utilities.AutoBreakLine(item.Text, SingleLineMaxLength, mergeLinesShorterThan, _languageCode);
                    if (rebalancedText != item.Text)
                    {
                        rebalanceCount++;
                        var beforePreview = GetTextPreview(item.Text.Replace("\r\n", " · ").Replace("\n", " · "), 60);
                        var afterPreview = GetTextPreview(rebalancedText.Replace("\r\n", " · ").Replace("\n", " · "), 60);
                        var fixDescription = $"'{beforePreview}' → '{afterPreview}'";

                        string? rebalancedMarginV = null;
                        if (_isFormatEbu)
                        {
                            rebalancedMarginV = TeletextRowHelper.GetRowKeepingBottomEdge(
                                    item.MarginV,
                                    GetPlainLineCount(item.Text),
                                    GetPlainLineCount(rebalancedText),
                                    Configuration.Settings.SubtitleSettings.EbuStlTeletextUseDoubleHeight)
                                ?.ToString(CultureInfo.InvariantCulture);
                        }

                        // The item applies the rebalanced text (and row) itself, and puts the
                        // original back when the user unchecks the row.
                        var fixItem = new SplitBreakLongLinesItem(Se.Language.Tools.SplitBreakLongLines.RebalanceLongLine, index + 1, fixDescription, item, rebalancedText, rebalancedMarginV);
                        Fixes.Add(fixItem);
                    }
                }
            }

            // Split events are clones of their source and keep its number - renumber so the
            // preview (and the grid after OK) shows 12, 13, 14 rather than 12, 12, 13.
            if (splitCount > 0)
            {
                for (var index = 0; index < AllSubtitlesFixed.Count; index++)
                {
                    AllSubtitlesFixed[index].Number = index + 1;
                }
            }

            if (splitCount == 0 && rebalanceCount == 0)
            {
                FixesInfo = Se.Language.Tools.ApplyDurationLimits.NoChangesNeeded;
                return;
            }

            if (rebalanceCount == 0)
            {
                FixesInfo = string.Format(Se.Language.Tools.SplitBreakLongLines.LinesSplitX, splitCount);
            }
            else
            {
                FixesInfo = string.Format(Se.Language.Tools.SplitBreakLongLines.LinesSplitXLinesRebalancedY, splitCount, rebalanceCount);
            }
        });
    }

    [RelayCommand]
    private void SelectAll()
    {
        SetSelectableFixes(true);
    }

    [RelayCommand]
    private void SelectNone()
    {
        SetSelectableFixes(false);
    }

    private void SetSelectableFixes(bool isSelected)
    {
        foreach (var fix in Fixes)
        {
            if (fix.IsSelectable)
            {
                fix.IsSelected = isSelected;
            }
        }
    }

    private bool CanBeFixedByRebalancing(string? text, int mergeLinesShorterThan)
    {
        if (!HasLineTooLong(text, SingleLineMaxLength, MaxNumberOfLines))
        {
            return true;
        }

        var rebalanced = Utilities.AutoBreakLine(text ?? string.Empty, SingleLineMaxLength, mergeLinesShorterThan, _languageCode);
        return !HasLineTooLong(rebalanced, SingleLineMaxLength, MaxNumberOfLines);
    }

    private static int GetPlainLineCount(string? text)
    {
        return HtmlUtil.RemoveHtmlTags(text ?? string.Empty, true).SplitToLines().Count;
    }

    public static bool HasLineTooLong(string? text, int singleLineMaxLength, int maxNumberOfLines)
    {
        var lines = HtmlUtil.RemoveHtmlTags(text ?? string.Empty, true).SplitToLines();
        if (lines.Count > maxNumberOfLines)
        {
            return true;
        }

        foreach (var line in lines)
        {
            if (line.Length > singleLineMaxLength)
            {
                return true;
            }
        }

        return false;
    }

    public sealed class SplitOptions
    {
        /// <summary>Gap reserved between the events a subtitle is split into.</summary>
        public double MinimumGapMs { get; init; }

        /// <summary>Move teletext rows (EBU STL MarginV) so the bottom edge of the text stays put.</summary>
        public bool AdjustTeletextRows { get; init; }

        public bool TeletextDoubleHeight { get; init; }
    }

    public static List<SubtitleLineViewModel> Split(SubtitleLineViewModel item, int maxCharactersPerSubtitle, int singleLineMaxLength)
    {
        return Split(item, maxCharactersPerSubtitle, singleLineMaxLength, new SplitOptions());
    }

    /// <summary>
    /// Cuts a subtitle that does not fit its limits into several events. Text is never re-wrapped
    /// here (#10959): split-only keeps each event's text as-is, and auto-wrapping stays with the
    /// opt-in rebalance step.
    /// </summary>
    public static List<SubtitleLineViewModel> Split(SubtitleLineViewModel item, int maxCharactersPerSubtitle, int singleLineMaxLength, SplitOptions options)
    {
        var lines = new List<SubtitleLineViewModel>();

        var originalText = item.Text ?? string.Empty;
        var originalStartMs = item.StartTime.TotalMilliseconds;
        var originalEndMs = item.EndTime.TotalMilliseconds;
        var originalDurationMs = Math.Max(0, originalEndMs - originalStartMs);

        // Per-line maximum for visual wrapping and per-subtitle maximum for splitting
        var perLineMax = Math.Max(5, singleLineMaxLength);
        var perSubtitleMax = Math.Max(perLineMax, maxCharactersPerSubtitle);
        var maxNumberOfLines = Math.Max(1, perSubtitleMax / perLineMax);

        var plainText = HtmlUtil.RemoveHtmlTags(originalText, true).Replace("\r\n", " ").Replace('\n', ' ').Trim();
        var originalLineCount = GetPlainLineCount(originalText);

        // Same rule as SE4's QualifiesForSplit: a subtitle that fits in total can still be
        // unusable because one of its lines is too long or it has too many lines.
        var fitsInTotal = plainText.Length <= perSubtitleMax;
        if (string.IsNullOrWhiteSpace(plainText) ||
            (fitsInTotal && !HasLineTooLong(originalText, perLineMax, maxNumberOfLines)))
        {
            lines.Add(item);
            return lines;
        }

        List<string> segments;
        if (fitsInTotal && originalLineCount > 1)
        {
            // Only a line is too long or there are too many lines: the author's line breaks
            // are the natural event boundaries, so cut there instead of re-flowing the text.
            segments = SplitAtLineBreaks(originalText, perLineMax, maxNumberOfLines);
        }
        else
        {
            // A single line that fits in total must still be cut so each event fits on one
            // line; a text over the total limit is cut by the subtitle limit as before.
            var limit = fitsInTotal ? perLineMax : perSubtitleMax;
            segments = SplitByLength(originalText.Trim(), limit);
        }

        if (segments.Count <= 1)
        {
            lines.Add(item);
            return lines;
        }

        // Distribute time proportional to character counts per segment
        var charCounts = new List<int>();
        var totalChars = 0;
        foreach (var seg in segments)
        {
            var cnt = HtmlUtil.RemoveHtmlTags(seg, true).Replace("\r\n", " ").Replace('\n', ' ').Length;
            if (cnt <= 0)
            {
                cnt = 1; // avoid zero to ensure some time
            }

            charCounts.Add(cnt);
            totalChars += cnt;
        }

        // Reserve the minimum gap between the new events, like SE4 did; the gaps may take at
        // most half the duration so a short subtitle still leaves time for its text.
        var gapCount = segments.Count - 1;
        var gapMs = Math.Min(Math.Max(0, options.MinimumGapMs), originalDurationMs / (2.0 * gapCount));
        var textDurationMs = originalDurationMs - gapMs * gapCount;

        // Build new subtitle lines
        var accumulatedMs = originalStartMs;
        for (var i = 0; i < segments.Count; i++)
        {
            var segText = segments[i];

            double segDurationMs;
            if (i == segments.Count - 1)
            {
                // last segment takes the rest (avoid rounding drift)
                segDurationMs = Math.Max(0, originalEndMs - accumulatedMs);
            }
            else
            {
                segDurationMs = textDurationMs * (charCounts[i] / (double)totalChars);
                segDurationMs = Math.Max(0, Math.Min(segDurationMs, originalEndMs - accumulatedMs - gapMs));
            }

            var newLine = new SubtitleLineViewModel(item, true)
            {
                Text = segText,
                StartTime = TimeSpan.FromMilliseconds(accumulatedMs),
                EndTime = TimeSpan.FromMilliseconds(accumulatedMs + segDurationMs)
            };
            newLine.UpdateDuration();

            if (options.AdjustTeletextRows)
            {
                var newRow = TeletextRowHelper.GetRowKeepingBottomEdge(item.MarginV, originalLineCount, GetPlainLineCount(segText), options.TeletextDoubleHeight);
                if (newRow.HasValue)
                {
                    newLine.MarginV = newRow.Value.ToString(CultureInfo.InvariantCulture);
                }
            }

            lines.Add(newLine);
            accumulatedMs += segDurationMs + gapMs;
        }

        // In rare rounding cases, force end of last to original end
        var last = lines[^1];
        last.EndTime = TimeSpan.FromMilliseconds(originalEndMs);
        last.UpdateDuration();

        return lines;

        // Local helper: groups the existing lines into events of at most maxLines lines. A line
        // over the per-line limit becomes an event of its own and is kept whole (SE4 did the
        // same): the author's line is a better event than a cut in the middle of it, and the
        // rebalance step can still wrap it.
        static List<string> SplitAtLineBreaks(string text, int perLineMax, int maxLines)
        {
            var segments = new List<string>();
            var group = new List<string>();
            foreach (var line in text.SplitToLines())
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var trimmed = line.Trim();
                if (HtmlUtil.RemoveHtmlTags(trimmed, true).Length > perLineMax)
                {
                    FlushGroup();
                    segments.Add(trimmed);
                    continue;
                }

                if (group.Count == maxLines)
                {
                    FlushGroup();
                }

                group.Add(trimmed);
            }

            FlushGroup();
            return segments;

            void FlushGroup()
            {
                if (group.Count > 0)
                {
                    segments.Add(string.Join(Environment.NewLine, group));
                    group.Clear();
                }
            }
        }

        // Local helper: cuts text at natural boundaries so each piece has at most maxPlainLen
        // visible characters.
        static List<string> SplitByLength(string text, int maxPlainLen)
        {
            var segments = new List<string>();
            var remaining = text;

            while (!string.IsNullOrEmpty(remaining))
            {
                var remainingPlain = HtmlUtil.RemoveHtmlTags(remaining, true).Replace("\r\n", " ").Replace('\n', ' ').Trim();
                if (remainingPlain.Length <= maxPlainLen)
                {
                    segments.Add(remaining.Trim());
                    break;
                }

                // Find best split position that keeps plain length <= maxPlainLen
                var splitIdx = FindBestSplitIndexByPlainLength(remaining, maxPlainLen);
                if (splitIdx <= 0)
                {
                    // Fallback to previous logic using raw index near limit
                    var approxCut = Math.Min(remaining.Length - 1, maxPlainLen);
                    splitIdx = FindBestSplitIndex(remaining, approxCut);
                }

                var part = remaining.Substring(0, splitIdx + 1).Trim();
                if (!string.IsNullOrWhiteSpace(part))
                {
                    segments.Add(part);
                }

                remaining = splitIdx + 1 < remaining.Length
                    ? remaining.Substring(splitIdx + 1).Trim()
                    : string.Empty;
            }

            return segments;
        }

        // Local helper: find the best split index near a target index (raw length based)
        static int FindBestSplitIndex(string text, int targetIndex)
        {
            if (string.IsNullOrEmpty(text))
            {
                return 0;
            }

            // Prefer line breaks if present before target
            var lbBefore = text.LastIndexOf('\n', Math.Min(targetIndex, text.Length - 1));
            if (lbBefore >= 0 && lbBefore >= targetIndex - 10)
            {
                return lbBefore;
            }

            // Search backwards from target for strong punctuation, then commas/spaces
            var strongPunctuation = new HashSet<char> { '.', '!', '?', '…', '。', '！', '？' };
            var weakPunctuation = new HashSet<char> { ';', ':', ',', '，', '；', '：' };

            for (var i = Math.Min(targetIndex, text.Length - 1); i >= 0 && i >= targetIndex - 40; i--)
            {
                var ch = text[i];
                if (strongPunctuation.Contains(ch))
                {
                    return i;
                }
            }
            for (var i = Math.Min(targetIndex, text.Length - 1); i >= 0 && i >= targetIndex - 30; i--)
            {
                var ch = text[i];
                if (weakPunctuation.Contains(ch))
                {
                    return i;
                }

                if (char.IsWhiteSpace(ch))
                {
                    return i;
                }

                if (ch == '-' && i + 1 < text.Length && text[i + 1] == ' ')
                {
                    return i; // split at "- "
                }
            }

            // If none found backwards, try small lookahead window
            for (var i = Math.Min(targetIndex + 1, text.Length - 1); i < text.Length && i <= targetIndex + 30; i++)
            {
                var ch = text[i];
                if (strongPunctuation.Contains(ch) || weakPunctuation.Contains(ch) || char.IsWhiteSpace(ch))
                {
                    return i;
                }

                if (ch == '-' && i + 1 < text.Length && text[i + 1] == ' ')
                {
                    return i;
                }
            }

            // Default to target index
            return Math.Min(targetIndex, text.Length - 1);
        }

        // Local helper: find split index ensuring plain (html-stripped) length <= maxPlainLen
        static int FindBestSplitIndexByPlainLength(string text, int maxPlainLen)
        {
            if (string.IsNullOrEmpty(text))
            {
                return 0;
            }

            var strongPunctuation = new HashSet<char> { '.', '!', '?', '…', '。', '！', '？' };
            var weakPunctuation = new HashSet<char> { ';', ':', ',', '，', '；', '：' };

            var plainLen = 0;
            var inTag = false;
            var lastVisibleIdx = -1; // last non-tag index
            var bestIdx = -1;             // best candidate index under the limit (any)
            var bestPlainLen = -1;        // plain length at bestIdx
            var bestAcceptableIdx = -1;   // best acceptable candidate index under the limit (no orphan small words)
            var bestAcceptablePlain = -1; // plain length at bestAcceptableIdx

            for (var i = 0; i < text.Length; i++)
            {
                var ch = text[i];
                if (ch == '<')
                {
                    inTag = true;
                }

                if (!inTag)
                {
                    // newline indicates strong boundary, do not include it
                    if (ch == '\n')
                    {
                        // Prefer splitting just before newline if within limit
                        return i > 0 ? i - 1 : 0;
                    }

                    // Count visible characters (skip CR)
                    if (ch != '\r')
                    {
                        plainLen++;
                        lastVisibleIdx = i;
                    }

                    // Any break candidate we see under or equal to the limit can be considered
                    var isBreak = strongPunctuation.Contains(ch) || weakPunctuation.Contains(ch) || char.IsWhiteSpace(ch) || (ch == '-' && i + 1 < text.Length && text[i + 1] == ' ');
                    if (isBreak && plainLen <= maxPlainLen)
                    {
                        if (plainLen >= bestPlainLen)
                        {
                            bestPlainLen = plainLen;
                            bestIdx = i;
                        }

                        // Prefer breakpoints that do not orphan one or two small words around strong punctuation
                        var acceptable = !CausesOrphanSmallWords(text, i, strongPunctuation);
                        if (acceptable && plainLen >= bestAcceptablePlain)
                        {
                            bestAcceptablePlain = plainLen;
                            bestAcceptableIdx = i;
                        }
                    }

                    if (plainLen > maxPlainLen)
                    {
                        // Prefer the acceptable candidate closest to the limit
                        if (bestAcceptableIdx >= 0)
                        {
                            return bestAcceptableIdx;
                        }

                        if (bestIdx >= 0)
                        {
                            return bestIdx;
                        }

                        return lastVisibleIdx >= 0 ? lastVisibleIdx : Math.Min(text.Length - 1, maxPlainLen);
                    }
                }

                if (ch == '>')
                {
                    inTag = false;
                }
            }

            // Entire text within limit
            return text.Length - 1;

            // Local: Avoid breakpoints that leave one or two small words alone around sentence-ending punctuation
            static bool CausesOrphanSmallWords(string s, int breakIdx, HashSet<char> strong)
            {
                if (breakIdx < 0 || breakIdx >= s.Length)
                {
                    return false;
                }

                var ch = s[breakIdx];

                // Helper to get up to two word lengths backward from index-1
                static List<int> GetPrevWordLens(string str, int idx)
                {
                    var res = new List<int>(2);
                    var i = Math.Min(idx, str.Length - 1);
                    // skip spaces and closing quotes/parens
                    var closingSkip = new HashSet<char>(new[] { ')', '’', '”', '\'', '»', ']', '}', '"' });
                    while (i >= 0 && (char.IsWhiteSpace(str[i]) || closingSkip.Contains(str[i])))
                    {
                        i--;
                    }

                    for (int w = 0; w < 2 && i >= 0; w++)
                    {
                        var length = 0;
                        while (i >= 0 && char.IsLetterOrDigit(str[i])) { length++; i--; }
                        if (length > 0)
                        {
                            res.Add(length);
                        }
                        // skip separators before next word
                        while (i >= 0 && !char.IsLetterOrDigit(str[i]))
                        {
                            i--;
                        }
                    }
                    return res;
                }

                // Helper to get up to two word lengths forward from index+1
                static List<int> GetNextWordLens(string str, int idx)
                {
                    var res = new List<int>(2);
                    var i = Math.Min(idx + 1, str.Length - 1);
                    // skip spaces and opening quotes/parens
                    var openingSkip = new HashSet<char>(new[] { '(', '“', '‘', '\'', '«', '[', '{', '"' });
                    while (i < str.Length && (char.IsWhiteSpace(str[i]) || openingSkip.Contains(str[i])))
                    {
                        i++;
                    }

                    for (int w = 0; w < 2 && i < str.Length; w++)
                    {
                        var length = 0;
                        while (i < str.Length && char.IsLetterOrDigit(str[i])) { length++; i++; }
                        if (length > 0)
                        {
                            res.Add(length);
                        }
                        // skip separators before next word
                        while (i < str.Length && !char.IsLetterOrDigit(str[i]))
                        {
                            i++;
                        }
                    }
                    return res;
                }

                // If candidate itself is a strong punctuation, avoid tiny words just before or just after it
                if (strong.Contains(ch))
                {
                    var prevLensStrong = GetPrevWordLens(s, breakIdx - 1);
                    var prevTinyStrong = prevLensStrong.Count > 0 && prevLensStrong.Count <= 2 && prevLensStrong.TrueForAll(l => l <= 2);

                    var nextLensStrong = GetNextWordLens(s, breakIdx);
                    var nextTinyStrong = nextLensStrong.Count > 0 && nextLensStrong.Count <= 2 && nextLensStrong.TrueForAll(l => l <= 2);

                    if (prevTinyStrong || nextTinyStrong)
                    {
                        return true;
                    }
                }

                // If candidate is NOT strong, check whether we are breaking shortly AFTER a strong punctuation
                // and the words since that strong punctuation are tiny (one or two words of length <= 2), e.g. "... overtime. So,"
                int j = breakIdx - 1;
                while (j >= 0 && !strong.Contains(s[j]))
                {
                    j--;
                }

                if (j >= 0)
                {
                    // We found previous strong punctuation at index j
                    // Count up to 2 word lengths between (j, breakIdx)
                    var tinyBetween = false;
                    {
                        int i = j + 1;
                        // skip whitespace and opening quotes/parens
                        var openingSkip = new HashSet<char>(new[] { '(', '“', '‘', '\'', '«', '[', '{', '"' });
                        while (i < breakIdx && (char.IsWhiteSpace(s[i]) || openingSkip.Contains(s[i])))
                        {
                            i++;
                        }

                        var lens = new List<int>(2);
                        for (int w = 0; w < 2 && i < breakIdx; w++)
                        {
                            int len = 0;
                            while (i < breakIdx && char.IsLetterOrDigit(s[i])) { len++; i++; }
                            if (len > 0)
                            {
                                lens.Add(len);
                            }

                            while (i < breakIdx && !char.IsLetterOrDigit(s[i]))
                            {
                                i++;
                            }
                        }
                        tinyBetween = lens.Count > 0 && lens.Count <= 2 && lens.TrueForAll(l => l <= 2);
                    }
                    if (tinyBetween)
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }

    private void LoadSettings()
    {
        // Values of 0 mean "not saved yet" - fall back to the general settings
        SingleLineMaxLength = Se.Settings.Tools.SplitRebalanceLongLinesSingleLineMaxLength > 0
            ? Se.Settings.Tools.SplitRebalanceLongLinesSingleLineMaxLength
            : Se.Settings.General.SubtitleLineMaximumLength;
        MaxNumberOfLines = Se.Settings.Tools.SplitRebalanceLongLinesMaxNumberOfLines > 0
            ? Se.Settings.Tools.SplitRebalanceLongLinesMaxNumberOfLines
            : Se.Settings.General.MaxNumberOfLines;
        UnbreakLinesShorterThan = Se.Settings.Tools.SplitRebalanceLongLinesUnbreakShorterThan > 0
            ? Se.Settings.Tools.SplitRebalanceLongLinesUnbreakShorterThan
            : Se.Settings.General.UnbreakLinesShorterThan;
        SplitLongLines = Se.Settings.Tools.SplitRebalanceLongLinesSplit;
        RebalanceLongLines = Se.Settings.Tools.SplitRebalanceLongLinesRebalance;
        RebalanceOnlyLinesTooLong = Se.Settings.Tools.SplitRebalanceLongLinesRebalanceOnlyTooLong;
    }

    private void SaveSettings()
    {
        Se.Settings.Tools.SplitRebalanceLongLinesSplit = SplitLongLines;
        Se.Settings.Tools.SplitRebalanceLongLinesRebalance = RebalanceLongLines;
        Se.Settings.Tools.SplitRebalanceLongLinesRebalanceOnlyTooLong = RebalanceOnlyLinesTooLong;
        Se.Settings.Tools.SplitRebalanceLongLinesSingleLineMaxLength = SingleLineMaxLength;
        Se.Settings.Tools.SplitRebalanceLongLinesMaxNumberOfLines = MaxNumberOfLines;
        Se.Settings.Tools.SplitRebalanceLongLinesUnbreakShorterThan = UnbreakLinesShorterThan;
        Se.SaveSettings();
    }

    [RelayCommand]
    private void Ok()
    {
        if (Window == null)
        {
            return;
        }

        SaveSettings();
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    internal void KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/split-break-long-lines");
        }
    }

    public void Initialize(List<SubtitleLineViewModel> toList, bool isFormatEbu = false)
    {
        _allSubtitles = toList;
        _isFormatEbu = isFormatEbu;

        var subtitle = new Subtitle();
        foreach (var line in toList)
        {
            subtitle.Paragraphs.Add(new Paragraph(line.Text, line.StartTime.TotalMilliseconds, line.EndTime.TotalMilliseconds));
        }
        _languageCode = LanguageAutoDetect.AutoDetectGoogleLanguageOrNull(subtitle) ?? "en";

        _previewTimer.Start();
    }

    internal void SetChanged()
    {
        _isDirty = true;
    }

    internal void Loaded()
    {
        _isDirty = true;
    }

    private static string GetTextPreview(string? text, int maxLength)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        var preview = HtmlUtil.RemoveHtmlTags(text, true).Replace("\r\n", " ").Replace("\n", " ").Trim();
        if (preview.Length <= maxLength)
        {
            return preview;
        }

        return preview.Substring(0, maxLength) + "…";
    }
}