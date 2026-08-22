using Avalonia.Controls;
using Avalonia.Input;
using Nikse.SubtitleEdit.Logic;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
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
    [ObservableProperty] private bool _applyMinimumGapToAllSubtitles;
    [ObservableProperty] private int _unbreakLinesShorterThan;

    [ObservableProperty] private string _fixesInfo;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }
    public List<SubtitleLineViewModel> AllSubtitlesFixed { get; set; }

    private List<SubtitleLineViewModel> _allSubtitles;
    private string _languageCode = "en";

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
            var gapCount = 0;
            var maxCharactersPerSubtitle = MaxNumberOfLines * SingleLineMaxLength;

            if (SplitLongLines)
            {
                for (var index = 0; index < _allSubtitles.Count; index++)
                {
                    var item = new SubtitleLineViewModel(_allSubtitles[index]);

                    var splitLines = Split(
                        item,
                        maxCharactersPerSubtitle,
                        SingleLineMaxLength,
                        _languageCode,
                        makeCompliant: true,
                        GetGeneralMinimumGapMs());

                    var textChanged = splitLines.Count == 1 && splitLines[0].Text != item.Text;
                    if (splitLines.Count > 1 || textChanged)
                    {
                        splitCount++;
                        var originalPreview = GetTextPreview(item.Text, 50);
                        string fixDescription;
                        if (splitLines.Count > 1)
                        {
                            var firstSplitPreview = GetTextPreview(splitLines[0].Text, 50);
                            fixDescription = string.Format(
                                Se.Language.Tools.SplitBreakLongLines.SplitIntoXLines,
                                splitLines.Count,
                                originalPreview,
                                firstSplitPreview);
                        }
                        else
                        {
                            var correctedPreview = GetTextPreview(splitLines[0].Text.Replace("\r\n", " · ").Replace("\n", " · "), 60);
                            fixDescription = $"'{originalPreview}' → '{correctedPreview}'";
                        }

                        var fixItem = new SplitBreakLongLinesItem(
                            Se.Language.Tools.SplitBreakLongLines.SplitLongLine,
                            index + 1,
                            fixDescription,
                            item);
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
                // AutoBreakLine keeps text on one line only when it is strictly shorter than the
                // unbreak threshold, so a threshold at or above the single line max length means
                // "keep any text that fits on one line" - and capping there also prevents merging
                // to a single line that would exceed the max length (#12910).
                var mergeLinesShorterThan = UnbreakLinesShorterThan >= SingleLineMaxLength
                    ? SingleLineMaxLength + 1
                    : UnbreakLinesShorterThan;

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
                        var fixItem = new SplitBreakLongLinesItem(
                            Se.Language.Tools.SplitBreakLongLines.RebalanceLongLine,
                            index + 1,
                            fixDescription,
                            item,
                            isSelectable: true,
                            proposedText: rebalancedText);
                        Fixes.Add(fixItem);
                    }
                }
            }

            if (ApplyMinimumGapToAllSubtitles)
            {
                var minimumGapMs = GetGeneralMinimumGapMs();
                const double toleranceMs = 10.0;

                for (var index = 0; index < AllSubtitlesFixed.Count - 1; index++)
                {
                    var current = AllSubtitlesFixed[index];
                    var next = AllSubtitlesFixed[index + 1];
                    var currentGapMs = next.StartTime.TotalMilliseconds - current.EndTime.TotalMilliseconds;

                    if (currentGapMs >= minimumGapMs - toleranceMs)
                    {
                        continue;
                    }

                    var newEndMs = next.StartTime.TotalMilliseconds - minimumGapMs;
                    if (newEndMs <= current.StartTime.TotalMilliseconds)
                    {
                        continue;
                    }

                    var before = new TimeCode(currentGapMs).ToShortDisplayString();
                    current.EndTime = TimeSpan.FromMilliseconds(newEndMs);
                    current.UpdateDuration();
                    var newGapMs = next.StartTime.TotalMilliseconds - current.EndTime.TotalMilliseconds;
                    var after = new TimeCode(newGapMs).ToShortDisplayString();
                    gapCount++;

                    Fixes.Add(new SplitBreakLongLinesItem(
                        Se.Language.Main.Menu.ApplyMinGap,
                        index + 1,
                        $"Gap: {before} → {after}",
                        current));
                }
            }

            // Splitting clones the source SubtitleLineViewModel, including its Number.
            // When at least one subtitle event was split, renumber the complete result once
            // at the end so newly created events do not keep duplicate numbers.
            if (splitCount > 0)
            {
                RenumberSubtitles(AllSubtitlesFixed);
            }

            if (splitCount == 0 && rebalanceCount == 0 && gapCount == 0)
            {
                FixesInfo = Se.Language.Tools.ApplyDurationLimits.NoChangesNeeded;
                return;
            }

            if (gapCount > 0)
            {
                FixesInfo = $"Split: {splitCount}, rebalanced: {rebalanceCount}, gaps corrected: {gapCount}";
            }
            else if (rebalanceCount == 0)
            {
                FixesInfo = string.Format(Se.Language.Tools.SplitBreakLongLines.LinesSplitX, splitCount);
            }
            else
            {
                FixesInfo = string.Format(Se.Language.Tools.SplitBreakLongLines.LinesSplitXLinesRebalancedY, splitCount, rebalanceCount);
            }
        });
    }

    public static void RenumberSubtitles(List<SubtitleLineViewModel> subtitles)
    {
        for (var index = 0; index < subtitles.Count; index++)
        {
            subtitles[index].Number = index + 1;
        }
    }

    private static double GetGeneralMinimumGapMs()
    {
        var general = Se.Settings.General;
        if (general.UseFrameMode)
        {
            return SubtitleFormat.FramesToMilliseconds(general.MinimumBetweenLines.Frames);
        }

        return general.MinimumBetweenLines.Milliseconds;
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

    public static List<SubtitleLineViewModel> Split(SubtitleLineViewModel item, int maxCharactersPerSubtitle, int singleLineMaxLength)
    {
        return Split(item, maxCharactersPerSubtitle, singleLineMaxLength, "en", makeCompliant: false);
    }

    public static List<SubtitleLineViewModel> Split(
        SubtitleLineViewModel item,
        int maxCharactersPerSubtitle,
        int singleLineMaxLength,
        string languageCode,
        bool makeCompliant)
    {
        return Split(
            item,
            maxCharactersPerSubtitle,
            singleLineMaxLength,
            languageCode,
            makeCompliant,
            minimumGapMs: 0);
    }

    public static List<SubtitleLineViewModel> Split(
        SubtitleLineViewModel item,
        int maxCharactersPerSubtitle,
        int singleLineMaxLength,
        string languageCode,
        bool makeCompliant,
        double minimumGapMs)
    {
        var lines = new List<SubtitleLineViewModel>();

        var originalText = item.Text ?? string.Empty;
        var originalStartMs = item.StartTime.TotalMilliseconds;
        var originalEndMs = item.EndTime.TotalMilliseconds;
        var originalDurationMs = Math.Max(0, originalEndMs - originalStartMs);

        var perLineMax = Math.Max(5, singleLineMaxLength);
        var perSubtitleMax = Math.Max(perLineMax, maxCharactersPerSubtitle);
        var maxNumberOfLines = Math.Max(1, (int)Math.Ceiling(perSubtitleMax / (double)perLineMax));

        var plainText = HtmlUtil.RemoveHtmlTags(originalText, true)
            .Replace("\r\n", " ")
            .Replace('\n', ' ')
            .Trim();

        if (string.IsNullOrWhiteSpace(plainText))
        {
            lines.Add(item);
            return lines;
        }

        var originalPlainLines = HtmlUtil.RemoveHtmlTags(originalText, true).SplitToLines();
        var hasTooManyLines = originalPlainLines.Count > maxNumberOfLines;
        var hasOverlongExistingLine = false;
        foreach (var originalLine in originalPlainLines)
        {
            if (originalLine.Length > perLineMax)
            {
                hasOverlongExistingLine = true;
                break;
            }
        }

        // Legacy SE5 behavior used by the existing three-argument overload:
        // split only when the total text exceeds the subtitle capacity, except that an
        // already multi-line subtitle with an overlong line is allowed to split at its
        // existing line boundary (SE4 parity regression fix).
        if (!makeCompliant)
        {
            if (plainText.Length <= perSubtitleMax && !hasTooManyLines)
            {
                if (!(originalPlainLines.Count > 1 && hasOverlongExistingLine))
                {
                    lines.Add(item);
                    return lines;
                }
            }
        }
        else
        {
            // Split-long-lines should be a complete correction pass. Already compliant
            // subtitles must remain byte-for-byte untouched, including intentionally
            // unbalanced line breaks.
            if (!hasTooManyLines && !hasOverlongExistingLine)
            {
                lines.Add(item);
                return lines;
            }

            // A too-long single-line subtitle that still fits inside one subtitle event
            // is locally balanced here. This is deliberately not the global "Rebalance
            // long lines" operation: only this non-compliant subtitle is changed.
            if (originalPlainLines.Count == 1 && plainText.Length <= perSubtitleMax)
            {
                var balanced = BalanceSegment(originalText);
                if (IsCompliant(balanced))
                {
                    var balancedItem = new SubtitleLineViewModel(item, true)
                    {
                        Text = balanced,
                        StartTime = item.StartTime,
                        EndTime = item.EndTime,
                    };

                    AdjustTeletextRowForLineCountChange(
                        balancedItem,
                        item.MarginV,
                        originalPlainLines.Count,
                        GetPlainLineCount(balanced));

                    balancedItem.UpdateDuration();
                    lines.Add(balancedItem);
                    return lines;
                }
            }
        }

        // Prepare event segments. For an existing multi-line subtitle with an overlong
        // line, keep its editorial line boundaries as event split points. This matches
        // the useful SE4 behavior: a sentence/line boundary is not silently rebalanced
        // across the whole subtitle just to avoid creating a new event.
        var segments = new List<string>();
        var remaining = originalText.Trim();

        if (plainText.Length <= perSubtitleMax && originalPlainLines.Count > 1 &&
            (hasOverlongExistingLine || hasTooManyLines))
        {
            foreach (var originalLine in originalText.SplitToLines())
            {
                if (!string.IsNullOrWhiteSpace(originalLine))
                {
                    segments.Add(originalLine.Trim());
                }
            }
            remaining = string.Empty;
        }

        while (!string.IsNullOrEmpty(remaining))
        {
            var remainingPlain = HtmlUtil.RemoveHtmlTags(remaining, true)
                .Replace("\r\n", " ")
                .Replace('\n', ' ')
                .Trim();

            if (remainingPlain.Length <= perSubtitleMax)
            {
                segments.Add(remaining.Trim());
                break;
            }

            var splitIdx = FindBestSplitIndexByPlainLength(remaining, perSubtitleMax);
            if (splitIdx <= 0)
            {
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

        // In one-pass correction mode, locally balance only the segments that need it.
        // This guarantees that Split long lines does not create new overlong single-line
        // subtitles that would require a second global Rebalance run.
        if (makeCompliant)
        {
            for (var i = 0; i < segments.Count; i++)
            {
                if (HasLineTooLong(segments[i], perLineMax, maxNumberOfLines))
                {
                    segments[i] = BalanceSegment(segments[i]);
                }
            }
        }

        var charCounts = new List<int>();
        var totalChars = 0;
        foreach (var seg in segments)
        {
            var cnt = HtmlUtil.RemoveHtmlTags(seg, true)
                .Replace("\r\n", " ")
                .Replace('\n', ' ')
                .Length;
            if (cnt <= 0)
            {
                cnt = 1;
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

        // Keep the original outer timecodes, but reserve the configured minimum gap
        // between newly created subtitle events. If the source duration is too short to
        // fit the requested gap, clamp it so durations never become negative.
        var gapMs = 0.0;
        if (segments.Count > 1)
        {
            gapMs = Math.Max(0, minimumGapMs);
            var maxGapThatFits = originalDurationMs / (segments.Count - 1);
            gapMs = Math.Min(gapMs, maxGapThatFits);
        }

        var totalGapMs = gapMs * Math.Max(0, segments.Count - 1);
        var availableTextDurationMs = Math.Max(0, originalDurationMs - totalGapMs);

        double accumulatedMs = originalStartMs;
        for (var i = 0; i < segments.Count; i++)
        {
            var segText = segments[i];

            double segDurationMs;
            if (i == segments.Count - 1)
            {
                segDurationMs = Math.Max(0, originalEndMs - accumulatedMs);
            }
            else
            {
                segDurationMs = availableTextDurationMs * (charCounts[i] / (double)totalChars);
                segDurationMs = Math.Max(
                    0,
                    Math.Min(
                        segDurationMs,
                        Math.Max(0, originalEndMs - accumulatedMs - gapMs)));
            }

            var newLine = new SubtitleLineViewModel(item, true)
            {
                Text = segText,
                StartTime = TimeSpan.FromMilliseconds(accumulatedMs),
                EndTime = TimeSpan.FromMilliseconds(accumulatedMs + segDurationMs),
            };

            AdjustTeletextRowForLineCountChange(
                newLine,
                item.MarginV,
                originalPlainLines.Count,
                GetPlainLineCount(segText));

            newLine.UpdateDuration();
            lines.Add(newLine);

            accumulatedMs += segDurationMs;
            if (i < segments.Count - 1)
            {
                accumulatedMs += gapMs;
            }
        }

        if (lines.Count > 0)
        {
            var last = lines[^1];
            last.EndTime = TimeSpan.FromMilliseconds(originalEndMs);
            last.UpdateDuration();
        }

        return lines;

        static int GetPlainLineCount(string text)
        {
            return HtmlUtil.RemoveHtmlTags(text ?? string.Empty, true).SplitToLines().Count;
        }

        static void AdjustTeletextRowForLineCountChange(
            SubtitleLineViewModel subtitle,
            string originalMarginV,
            int oldLineCount,
            int newLineCount)
        {
            if (oldLineCount == newLineCount || oldLineCount < 1 || newLineCount < 1)
            {
                return;
            }

            if (!int.TryParse(originalMarginV, out var originalRow))
            {
                return;
            }

            // Double-height Teletext uses two physical rows per text line.
            // Keep the subtitle at its existing vertical area and move the start row
            // only by the number of rows gained/lost through the line-count change.
            //
            // Examples:
            // 2 lines at 21 -> 1 line at 23
            // 2 lines at 19 -> 1 line at 21
            // 2 lines at 20 -> 1 line at 22
            // and the inverse for 1 -> 2.
            const int rowsPerTextLine = 2;
            var rowDelta = (oldLineCount - newLineCount) * rowsPerTextLine;
            var adjustedRow = originalRow + rowDelta;

            if (adjustedRow is >= 1 and <= TeletextRowHelper.BottomRow)
            {
                subtitle.MarginV = adjustedRow.ToString(CultureInfo.InvariantCulture);
            }
        }

        string BalanceSegment(string text)
        {
            // max+1 prevents AutoBreakLine from collapsing a compliant two-line result
            // back to one line while still allowing a long single line to be balanced.
            return Utilities.AutoBreakLine(text, perLineMax, perLineMax + 1, languageCode);
        }

        bool IsCompliant(string text)
        {
            return !HasLineTooLong(text, perLineMax, maxNumberOfLines);
        }

        static int FindBestSplitIndex(string text, int targetIndex)
        {
            if (string.IsNullOrEmpty(text))
            {
                return 0;
            }

            var lbBefore = text.LastIndexOf('\n', Math.Min(targetIndex, text.Length - 1));
            if (lbBefore >= 0 && lbBefore >= targetIndex - 10)
            {
                return lbBefore;
            }

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
                if (weakPunctuation.Contains(ch) || char.IsWhiteSpace(ch))
                {
                    return i;
                }

                if (ch == '-' && i + 1 < text.Length && text[i + 1] == ' ')
                {
                    return i;
                }
            }

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

            return Math.Min(targetIndex, text.Length - 1);
        }

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
            var lastVisibleIdx = -1;
            var bestIdx = -1;
            var bestPlainLen = -1;
            var bestAcceptableIdx = -1;
            var bestAcceptablePlain = -1;

            for (var i = 0; i < text.Length; i++)
            {
                var ch = text[i];
                if (ch == '<')
                {
                    inTag = true;
                }

                if (!inTag)
                {
                    if (ch == '\n')
                    {
                        return i > 0 ? i - 1 : 0;
                    }

                    if (ch != '\r')
                    {
                        plainLen++;
                        lastVisibleIdx = i;
                    }

                    var isBreak = strongPunctuation.Contains(ch) || weakPunctuation.Contains(ch) ||
                                  char.IsWhiteSpace(ch) ||
                                  (ch == '-' && i + 1 < text.Length && text[i + 1] == ' ');
                    if (isBreak && plainLen <= maxPlainLen)
                    {
                        if (plainLen >= bestPlainLen)
                        {
                            bestPlainLen = plainLen;
                            bestIdx = i;
                        }

                        var acceptable = !CausesOrphanSmallWords(text, i, strongPunctuation);
                        if (acceptable && plainLen >= bestAcceptablePlain)
                        {
                            bestAcceptablePlain = plainLen;
                            bestAcceptableIdx = i;
                        }
                    }

                    if (plainLen > maxPlainLen)
                    {
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

            return text.Length - 1;

            static bool CausesOrphanSmallWords(string s, int breakIdx, HashSet<char> strong)
            {
                if (breakIdx < 0 || breakIdx >= s.Length)
                {
                    return false;
                }

                var ch = s[breakIdx];

                static List<int> GetPrevWordLens(string str, int idx)
                {
                    var res = new List<int>(2);
                    var i = Math.Min(idx, str.Length - 1);
                    var closingSkip = new HashSet<char>(new[] { ')', '’', '”', '\'', '»', ']', '}', '"' });
                    while (i >= 0 && (char.IsWhiteSpace(str[i]) || closingSkip.Contains(str[i])))
                    {
                        i--;
                    }

                    for (var w = 0; w < 2 && i >= 0; w++)
                    {
                        var length = 0;
                        while (i >= 0 && char.IsLetterOrDigit(str[i]))
                        {
                            length++;
                            i--;
                        }

                        if (length > 0)
                        {
                            res.Add(length);
                        }

                        while (i >= 0 && !char.IsLetterOrDigit(str[i]))
                        {
                            i--;
                        }
                    }

                    return res;
                }

                static List<int> GetNextWordLens(string str, int idx)
                {
                    var res = new List<int>(2);
                    var i = Math.Min(idx + 1, str.Length - 1);
                    var openingSkip = new HashSet<char>(new[] { '(', '“', '‘', '\'', '«', '[', '{', '"' });
                    while (i < str.Length && (char.IsWhiteSpace(str[i]) || openingSkip.Contains(str[i])))
                    {
                        i++;
                    }

                    for (var w = 0; w < 2 && i < str.Length; w++)
                    {
                        var length = 0;
                        while (i < str.Length && char.IsLetterOrDigit(str[i]))
                        {
                            length++;
                            i++;
                        }

                        if (length > 0)
                        {
                            res.Add(length);
                        }

                        while (i < str.Length && !char.IsLetterOrDigit(str[i]))
                        {
                            i++;
                        }
                    }

                    return res;
                }

                if (strong.Contains(ch))
                {
                    var prevLensStrong = GetPrevWordLens(s, breakIdx - 1);
                    var prevTinyStrong = prevLensStrong.Count > 0 && prevLensStrong.Count <= 2 &&
                                         prevLensStrong.TrueForAll(l => l <= 2);

                    var nextLensStrong = GetNextWordLens(s, breakIdx);
                    var nextTinyStrong = nextLensStrong.Count > 0 && nextLensStrong.Count <= 2 &&
                                         nextLensStrong.TrueForAll(l => l <= 2);

                    if (prevTinyStrong || nextTinyStrong)
                    {
                        return true;
                    }
                }

                var j = breakIdx - 1;
                while (j >= 0 && !strong.Contains(s[j]))
                {
                    j--;
                }

                if (j >= 0)
                {
                    var i = j + 1;
                    var openingSkip = new HashSet<char>(new[] { '(', '“', '‘', '\'', '«', '[', '{', '"' });
                    while (i < breakIdx && (char.IsWhiteSpace(s[i]) || openingSkip.Contains(s[i])))
                    {
                        i++;
                    }

                    var lens = new List<int>(2);
                    for (var w = 0; w < 2 && i < breakIdx; w++)
                    {
                        var len = 0;
                        while (i < breakIdx && char.IsLetterOrDigit(s[i]))
                        {
                            len++;
                            i++;
                        }

                        if (len > 0)
                        {
                            lens.Add(len);
                        }

                        while (i < breakIdx && !char.IsLetterOrDigit(s[i]))
                        {
                            i++;
                        }
                    }

                    if (lens.Count > 0 && lens.Count <= 2 && lens.TrueForAll(l => l <= 2))
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
        ApplyMinimumGapToAllSubtitles = false;
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
    private void SelectAllRebalances()
    {
        foreach (var fix in Fixes)
        {
            if (fix.IsSelectable)
            {
                fix.IsSelected = true;
            }
        }
    }

    [RelayCommand]
    private void DeselectAllRebalances()
    {
        foreach (var fix in Fixes)
        {
            if (fix.IsSelectable)
            {
                fix.IsSelected = false;
            }
        }
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

    public void Initialize(List<SubtitleLineViewModel> toList)
    {
        _allSubtitles = toList;

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