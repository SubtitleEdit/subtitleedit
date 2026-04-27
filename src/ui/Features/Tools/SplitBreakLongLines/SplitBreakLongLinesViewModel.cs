using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Features.Tools.SplitBreakLongLines;

public partial class SplitBreakLongLinesViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<SplitBreakLongLinesItem> _fixes;
    [ObservableProperty] private SplitBreakLongLinesItem? _selectedFix;

    [ObservableProperty] private ObservableCollection<SubtitleLineViewModel> _subtitles;
    [ObservableProperty] private SubtitleLineViewModel? _selectedSubtitle;

    [ObservableProperty] private bool _splitLongLines;
    [ObservableProperty] private int _singleLineMaxLength;
    [ObservableProperty] private int _maxNumberOfLines;

    [ObservableProperty] private bool _rebalanceLongLines;

    [ObservableProperty] private string _fixesInfo;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }
    public List<SubtitleLineViewModel> AllSubtitlesFixed { get; set; }

    private List<SubtitleLineViewModel> _allSubtitles;

    private readonly System.Timers.Timer _previewTimer;
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
        _previewTimer.Elapsed += (sender, args) =>
        {
            _previewTimer.Stop();

            if (_isDirty)
            {
                _isDirty = false;
                UpdatePreview();
            }

            _previewTimer.Start();
        };
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

            if (SplitLongLines)
            {
                for (var index = 0; index < _allSubtitles.Count; index++)
                {
                    var item = new SubtitleLineViewModel(_allSubtitles[index]);

                    var splitLines = Split(item, maxCharactersPerSubtitle, SingleLineMaxLength);
                    if (splitLines.Count > 1)
                    {
                        splitCount++;
                        var originalPreview = GetTextPreview(item.Text, 50);
                        var firstSplitPreview = GetTextPreview(splitLines[0].Text, 50);
                        var fixDescription = string.Format(Se.Language.Tools.SplitBreakLongLines.SplitIntoXLines, splitLines.Count, originalPreview, firstSplitPreview);
                        var fixItem = new SplitBreakLongLinesItem(Se.Language.Tools.SplitBreakLongLines.SplitLongLine, index + 1, fixDescription, item);
                        Fixes.Add(fixItem);
                    }
                    foreach (var s in splitLines)
                    {
                        AllSubtitlesFixed.Add(s);
                    }
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
                    var rebalancedText = Utilities.AutoBreakLine(item.Text, SingleLineMaxLength, Se.Settings.General.UnbreakLinesShorterThan, "en");
                    if (rebalancedText != item.Text)
                    {
                        rebalanceCount++;
                        var beforePreview = GetTextPreview(item.Text.Replace("\r\n", " ↵ ").Replace("\n", " ↵ "), 60);
                        var afterPreview = GetTextPreview(rebalancedText.Replace("\r\n", " ↵ ").Replace("\n", " ↵ "), 60);
                        var fixDescription = $"'{beforePreview}' → '{afterPreview}'";
                        var fixItem = new SplitBreakLongLinesItem(Se.Language.Tools.SplitBreakLongLines.RebalanceLongLine, index + 1, fixDescription, item);
                        Fixes.Add(fixItem);
                    }
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

    public static List<SubtitleLineViewModel> Split(SubtitleLineViewModel item, int maxCharactersPerSubtitle, int singleLineMaxLength)
    {
        var lines = new List<SubtitleLineViewModel>();

        // Guard clauses
        var originalText = item.Text ?? string.Empty;
        var originalStartMs = item.StartTime.TotalMilliseconds;
        var originalEndMs = item.EndTime.TotalMilliseconds;
        var originalDurationMs = Math.Max(0, originalEndMs - originalStartMs);

        // Per-line maximum for visual wrapping and per-subtitle maximum for splitting
        var perLineMax = Math.Max(5, singleLineMaxLength);
        var perSubtitleMax = Math.Max(perLineMax, maxCharactersPerSubtitle);

        // If already short enough, return as-is
        var plainText = HtmlUtil.RemoveHtmlTags(originalText, true).Replace("\r\n", " ").Replace('\n', ' ').Trim();
        if (plainText.Length <= perSubtitleMax || string.IsNullOrWhiteSpace(plainText))
        {
            lines.Add(item);
            return lines;
        }

        // Prepare segments trying to split at natural boundaries
        var remaining = originalText.Trim();
        var segments = new List<string>();

        while (!string.IsNullOrEmpty(remaining))
        {
            var remainingPlain = HtmlUtil.RemoveHtmlTags(remaining, true).Replace("\r\n", " ").Replace('\n', ' ').Trim();
            if (remainingPlain.Length <= perSubtitleMax)
            {
                segments.Add(remaining.Trim());
                break;
            }

            // Find best split position that keeps plain length <= perSubtitleMax
            var splitIdx = FindBestSplitIndexByPlainLength(remaining, perSubtitleMax);
            if (splitIdx <= 0)
            {
                // Fallback to previous logic using raw index near limit
                var approxCut = Math.Min(remaining.Length - 1, perSubtitleMax);
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

        if (segments.Count == 0)
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
        if (totalChars <= 0)
        {
            totalChars = segments.Count;
            charCounts.Clear();
            for (var i = 0; i < segments.Count; i++)
            {
                charCounts.Add(1);
            }
        }

        // Build new subtitle lines
        double accumulatedMs = originalStartMs;
        for (var i = 0; i < segments.Count; i++)
        {
            var segText = segments[i];

            // Calculate duration for this segment
            double segDurationMs;
            if (i == segments.Count - 1)
            {
                // last segment takes the rest (avoid rounding drift)
                segDurationMs = Math.Max(0, originalEndMs - accumulatedMs);
            }
            else
            {
                segDurationMs = originalDurationMs * (charCounts[i] / (double)totalChars);
                // Ensure we don't overrun
                segDurationMs = Math.Max(0, Math.Min(segDurationMs, Math.Max(0, originalEndMs - accumulatedMs)));
            }

            // Ensure resulting segment lines are wrapped to SingleLineMaxLength
            var wrappedSegText = Utilities.AutoBreakLine(segText, perLineMax, Se.Settings.General.UnbreakLinesShorterThan, item.Language ?? "en");

            var newLine = new SubtitleLineViewModel(item, true)
            {
                Text = wrappedSegText,
                StartTime = TimeSpan.FromMilliseconds(accumulatedMs),
                EndTime = TimeSpan.FromMilliseconds(accumulatedMs + segDurationMs)
            };
            newLine.UpdateDuration();
            lines.Add(newLine);

            accumulatedMs += segDurationMs;
        }

        // In rare rounding cases, force end of last to original end
        if (lines.Count > 0)
        {
            var last = lines[^1];
            last.EndTime = TimeSpan.FromMilliseconds(originalEndMs);
            last.UpdateDuration();
        }

        return lines;

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
        SingleLineMaxLength = Se.Settings.General.SubtitleLineMaximumLength;
        MaxNumberOfLines = Se.Settings.General.MaxNumberOfLines;
        SplitLongLines = Se.Settings.Tools.SplitRebalanceLongLinesSplit;
        RebalanceLongLines = Se.Settings.Tools.SplitRebalanceLongLinesRebalance;
    }

    private void SaveSettings()
    {
        Se.Settings.Tools.SplitRebalanceLongLinesSplit = SplitLongLines;
        Se.Settings.Tools.SplitRebalanceLongLinesRebalance = RebalanceLongLines;
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
    }

    public void Initialize(List<SubtitleLineViewModel> toList)
    {
        _allSubtitles = toList;
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