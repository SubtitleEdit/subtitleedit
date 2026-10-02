using Avalonia;
﻿using Avalonia.Controls;
using Avalonia.Input;
using Nikse.SubtitleEdit.Logic;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Files.Compare;

public partial class CompareViewModel : ObservableObject
{
    public ObservableCollection<CompareItem> LeftSubtitles { get; } = new();
    public ObservableCollection<CompareItem> RightSubtitles { get; } = new();
    public ObservableCollection<CompareVisual> CompareVisuals { get; } = new();

    [ObservableProperty] private ObservableCollection<CompareRow> _rows = new();
    [ObservableProperty] private CompareRow? _selectedRow;
    [ObservableProperty] private bool _ignoreFormatting;
    [ObservableProperty] private bool _ignoreWhiteSpace;
    [ObservableProperty] private bool _ignoreNumbering;
    [ObservableProperty] private int _allCount;
    [ObservableProperty] private int _differenceCount;
    [ObservableProperty] private int _textDifferenceCount;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(LeftSideLabel))] private bool _isLeftEditable;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(HasPendingChanges), nameof(PendingChangesText), nameof(OkButtonText))] private int _pendingChangeCount;
    [ObservableProperty] private string _lastChangeText = string.Empty;
    [ObservableProperty] private bool _isReloadFromFileVisible;
    [ObservableProperty] private bool _isExportVisible;
    [ObservableProperty] private string _leftFileName = string.Empty;
    [ObservableProperty] private bool _leftFileNameHasChanges;
    [ObservableProperty] private string _rightFileName = string.Empty;
    [ObservableProperty] private string _statusText = string.Empty;
    [ObservableProperty] private CompareVisual _selectedCompareVisual;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(HasSyncPoints), nameof(ClearSyncPointsText), nameof(HasSyncBar))] private int _syncPointCount;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(HasSyncPointHint), nameof(HasSyncPointMessage), nameof(HasSyncBar))] private string _syncPointHint = string.Empty;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(HasSyncPointMessage), nameof(HasSyncBar))] private string _syncPointMessage = string.Empty;

    // The headers trim these to the space they have, keeping the start and the end (#15384).
    public string LeftFileNameDisplay => GetFileName(LeftFileName);
    public string RightFileNameDisplay => GetFileName(RightFileName);

    public bool HasPendingChanges => PendingChangeCount > 0;

    public string PendingChangesText => PendingChangeCount == 1
        ? Se.Language.File.CompareOnePendingChange
        : string.Format(Se.Language.File.CompareXPendingChanges, PendingChangeCount);

    public string OkButtonText => HasPendingChanges ? Se.Language.General.Apply : Se.Language.General.Ok;

    public bool HasSyncPoints => SyncPointCount > 0;
    public bool HasSyncPointHint => !string.IsNullOrEmpty(SyncPointHint);
    public string ClearSyncPointsText => string.Format(Se.Language.File.CompareClearXSyncPoints, SyncPointCount);
    public bool HasSyncPointMessage => !string.IsNullOrEmpty(SyncPointMessage) && !HasSyncPointHint;
    public bool HasSyncBar => HasSyncPoints || HasSyncPointHint || HasSyncPointMessage;

    // With one half picked, the menu item that completes the pair says so.
    public string PickSyncCurrentHeader => _pendingSyncRightId != null
        ? string.Format(Se.Language.File.CompareSyncWithReferenceX, _pendingSyncNumber)
        : Se.Language.File.CompareSyncPickCurrent;

    public string PickSyncReferenceHeader => _pendingSyncLeftId != null
        ? string.Format(Se.Language.File.CompareSyncWithCurrentX, _pendingSyncNumber)
        : Se.Language.File.CompareSyncPickReference;

    /// <summary>The selected row has the line the waiting half needs.</summary>
    public bool CanApplySync =>
        (_pendingSyncLeftId != null && SelectedRow?.Right.Line != null) ||
        (_pendingSyncRightId != null && SelectedRow?.Left.Line != null);

    public string LeftSideLabel => IsLeftEditable ? Se.Language.File.CompareEditable : Se.Language.File.CompareReadOnly;

    public Window? Window { get; internal set; }
    public bool OkPressed { get; private set; }

    /// <summary>The one list that shows both sides, a pair per row.</summary>
    public ListBox? RowsView { get; set; }

    /// <summary>Raised after the rows were rebuilt, so the overview ruler can redraw.</summary>
    public event EventHandler? RowsRebuilt;

    private IFileHelper _fileHelper;
    private IFolderHelper _folderHelper;
    private List<SubtitleLineViewModel> _leftLines = new();
    private List<SubtitleLineViewModel> _rightLines = new();
    private string _language = string.Empty;
    private bool _languageDirty = true;
    private bool _closeConfirmed;

    // Edits made in the merge view: every change pushes the state before it, so undo is a pop.
    private readonly Stack<EditState> _undoStack = new();
    private HashSet<Guid> _editedIds = new();
    private List<string> _changes = new();

    // Sync points: a current line and a reference line the user says belong together (#15394).
    // Kept by line id, so they survive edits and re-alignment; one half picked waits for the other.
    private readonly List<SyncPoint> _syncPoints = new();
    private Guid? _pendingSyncLeftId;
    private Guid? _pendingSyncRightId;
    private int _pendingSyncNumber;
    private HashSet<(Guid Left, Guid Right)> _alignedPairs = new();
    private DispatcherTimer? _syncMessageTimer;

    private sealed record SyncPoint(Guid LeftId, Guid RightId);

    private sealed record EditState(List<SubtitleLineViewModel> Lines, HashSet<Guid> EditedIds, List<string> Changes, List<SyncPoint> SyncPoints);

    // Theme aware - the light pastels are unreadable under the dark theme's near-white text (#13435).
    private static IBrush ListViewRed => CompareColors.OnlyInOneFileRow;
    private static IBrush ListViewGreen => CompareColors.TextOrTimeDifferenceRow;
    private static IBrush ListViewOrange => CompareColors.NumberDifferenceRow;
    private static readonly IBrush TransparentBrush = new ImmutableSolidColorBrush(Colors.Transparent);

    public CompareViewModel(IFileHelper fileHelper, IFolderHelper folderHelper)
    {
        _fileHelper = fileHelper;
        _folderHelper = folderHelper;

        CompareVisuals = new ObservableCollection<CompareVisual>(CompareVisual.GetCompareVisuals());
        SelectedCompareVisual = CompareVisuals[0];

        LoadSettings();
    }

    /// <summary>
    /// The "Show" choice and the two ignore options are remembered between sessions, like SE4
    /// did through Configuration.Settings.Compare (#14299).
    /// </summary>
    private void LoadSettings()
    {
        var settings = Se.Settings.File.Compare;

        if (Enum.TryParse<CompareVisualType>(settings.Show, out var show))
        {
            var visual = CompareVisuals.FirstOrDefault(p => p.Type == show);
            if (visual != null)
            {
                SelectedCompareVisual = visual;
            }
        }

        IgnoreWhiteSpace = settings.IgnoreWhitespace;
        IgnoreFormatting = settings.IgnoreFormatting;
        IgnoreNumbering = settings.IgnoreNumbering;
    }

    /// <summary>
    /// Only updates the in-memory settings, like Find/Replace do - the main window writes
    /// Settings.json when the application closes.
    /// </summary>
    internal void SaveSettings()
    {
        var settings = Se.Settings.File.Compare;
        settings.Show = SelectedCompareVisual.Type.ToString();
        settings.IgnoreWhitespace = IgnoreWhiteSpace;
        settings.IgnoreFormatting = IgnoreFormatting;
        settings.IgnoreNumbering = IgnoreNumbering;
    }

    internal void Initialize(
        ObservableCollection<SubtitleLineViewModel> left,
        string leftFileName,
        ObservableCollection<SubtitleLineViewModel> right,
        string rightFileName,
        bool hasChanges)
    {
        _leftLines.Clear();
        _leftLines.AddRange(left.Select(p => new SubtitleLineViewModel(p)));
        LeftFileName = leftFileName;

        // The left side is the editor's own subtitle, so it is the one that can be edited here -
        // the reference is only ever read, which keeps "what gets saved" a single subtitle.
        IsLeftEditable = true;
        ResetEdits();
        ResetSyncPoints();
        if (!string.IsNullOrEmpty(leftFileName) && hasChanges)
        {
            LeftFileNameHasChanges = true;
        }

        _rightLines.Clear();
        _rightLines.AddRange(right.Select(p => new SubtitleLineViewModel(p)));
        RightFileName = rightFileName;

        IsReloadFromFileVisible = !string.IsNullOrEmpty(LeftFileName);

        _languageDirty = true;
        Dispatcher.UIThread.Post(CompareAndSelectFirst);
    }

    private void Compare()
    {
        DetectLanguageIfNeeded();

        LeftSubtitles.Clear();
        foreach (var l in _leftLines)
        {
            LeftSubtitles.Add(new CompareItem(l));
        }

        RightSubtitles.Clear();
        foreach (var r in _rightLines)
        {
            RightSubtitles.Add(new CompareItem(r));
        }

        StatusText = string.Empty;
        InsertMissingLines();
        AddColoringAndCountDifferences();
        SetTextStackPanels();
        BuildRows();
        IsExportVisible = LeftSubtitles.Count > 0 && RightSubtitles.Count > 0;
    }

    private void CompareAndSelectFirst()
    {
        Compare();
        SelectRow(0);
    }

    private void BuildRows()
    {
        var max = Math.Max(LeftSubtitles.Count, RightSubtitles.Count);
        var rows = new List<CompareRow>(max);
        for (var i = 0; i < max; i++)
        {
            var left = i < LeftSubtitles.Count ? LeftSubtitles[i] : new CompareItem();
            var right = i < RightSubtitles.Count ? RightSubtitles[i] : new CompareItem();
            var isEdited = left.Line != null && _editedIds.Contains(left.Line.Id);
            rows.Add(new CompareRow(left, right, GetRowKind(left, right), isEdited, IsLeftEditable)
            {
                IsSyncPoint = IsSyncPoint(left, right),
            });
        }

        Rows = new ObservableCollection<CompareRow>(rows);
        UpdateSyncFlags();
        RowsRebuilt?.Invoke(this, EventArgs.Empty);
    }

    private static CompareRowKind GetRowKind(CompareItem left, CompareItem right)
    {
        if (!left.HasDifference)
        {
            return CompareRowKind.Same;
        }

        if (left.IsDefault)
        {
            return CompareRowKind.OnlyRight;
        }

        if (right.IsDefault)
        {
            return CompareRowKind.OnlyLeft;
        }

        return IsHighlighted(left.TextBackgroundBrush) || IsHighlighted(left.StartTimeBackgroundBrush) || IsHighlighted(left.EndTimeBackgroundBrush)
            ? CompareRowKind.Changed
            : CompareRowKind.NumberOnly;
    }

    private static bool IsHighlighted(IBrush brush) => !ReferenceEquals(brush, TransparentBrush);

    private void SetTextStackPanels()
    {
        if (LeftSubtitles.Count != RightSubtitles.Count)
        {
            return;
        }

        for (var i = 0; i < LeftSubtitles.Count; i++)
        {
            var left = LeftSubtitles[i];
            var right = RightSubtitles[i];
            // A pair the options count as equal gets no markup whatsoever - the same call the row
            // coloring makes, so the cell can never mark a difference the row has dropped (#14299).
            var (leftBlock, rightBlock) = AreTextsEqual(left, right)
                ? TextDiffHighlighter.MakePlainText(left.Text, right.Text)
                : TextDiffHighlighter.Compare(left.Text, right.Text, IgnoreWhiteSpace, IgnoreFormatting);
            left.TextPanel.Children.Clear();
            left.TextPanel.Children.Add(leftBlock);
            right.TextPanel.Children.Clear();
            right.TextPanel.Children.Add(rightBlock);
        }
    }

    private void AddColoringAndCountDifferences()
    {
        var differences = new List<int>();
        var totalWords = 0;
        var wordsChanged = 0;
        var min = Math.Min(LeftSubtitles.Count, RightSubtitles.Count);
        var onlyShowTextDiff = SelectedCompareVisual.Type == CompareVisualType.ShowOnlyDifferencesInText;
        var onlyShowDiff = SelectedCompareVisual.Type == CompareVisualType.ShowOnlyDifferences;

        ResetAllBackgroundColors();
        AllCount = Math.Max(LeftSubtitles.Count, RightSubtitles.Count);
        DifferenceCount = 0;
        TextDifferenceCount = 0;

        if (LeftSubtitles.Count == 0 || RightSubtitles.Count == 0)
        {
            return;
        }

        var differenceCount = 0;
        var textDifferenceCount = 0;
        for (var index = 0; index < min; index++)
        {
            var left = LeftSubtitles[index];
            var right = RightSubtitles[index];
            Utilities.GetTotalAndChangedWords(left.Text, right.Text, ref totalWords, ref wordsChanged, IgnoreWhiteSpace, IgnoreFormatting, ShouldBreakToLetter());

            bool isDifference;
            bool isTextDifference;
            if (left.IsDefault || right.IsDefault)
            {
                isDifference = true;
                isTextDifference = true;
                if (!right.IsDefault)
                {
                    SetItemBackgroundColor(index, false, ListViewRed, ItemColumn.All);
                }

                if (!left.IsDefault)
                {
                    SetItemBackgroundColor(index, true, ListViewRed, ItemColumn.All);
                }
            }
            else
            {
                var startMatch = IsTimeEqual(left.StartTime, right.StartTime);
                var endMatch = IsTimeEqual(left.EndTime, right.EndTime);
                var textsMatch = AreTextsEqual(left, right);
                var numbersMatch = IgnoreNumbering || left.Number == right.Number;
                isTextDifference = !textsMatch;
                isDifference = !startMatch || !endMatch || !textsMatch || !numbersMatch;

                if (!textsMatch)
                {
                    SetItemBackgroundColor(index, true, ListViewGreen, ItemColumn.Text);
                    SetItemBackgroundColor(index, false, ListViewGreen, ItemColumn.Text);
                }

                // "Only differences in text" leaves the timing and numbering unmarked, as it always did.
                if (!onlyShowTextDiff)
                {
                    if (!startMatch)
                    {
                        SetItemBackgroundColor(index, true, ListViewGreen, ItemColumn.StartTime);
                        SetItemBackgroundColor(index, false, ListViewGreen, ItemColumn.StartTime);
                    }

                    if (!endMatch)
                    {
                        SetItemBackgroundColor(index, true, ListViewGreen, ItemColumn.EndTime);
                        SetItemBackgroundColor(index, false, ListViewGreen, ItemColumn.EndTime);
                    }

                    if (!numbersMatch)
                    {
                        SetItemBackgroundColor(index, true, ListViewOrange, ItemColumn.Number);
                        SetItemBackgroundColor(index, false, ListViewOrange, ItemColumn.Number);
                    }
                }
            }

            // The tab counts are for the whole comparison, whichever view is showing.
            if (isDifference)
            {
                differenceCount++;
            }

            if (isTextDifference)
            {
                textDifferenceCount++;
            }

            if (onlyShowTextDiff ? isTextDifference : isDifference)
            {
                differences.Add(index);
            }
        }

        DifferenceCount = differenceCount;
        TextDifferenceCount = textDifferenceCount;

        foreach (var idx in differences)
        {
            LeftSubtitles[idx].HasDifference = true;
            RightSubtitles[idx].HasDifference = true;
        }

        // remove items not in differences
        if (onlyShowTextDiff || onlyShowDiff)
        {
            var differenceSet = new HashSet<int>(differences);
            var leftCount = LeftSubtitles.Count;
            var leftSurvivors = new List<CompareItem>(differenceSet.Count);
            var rightSurvivors = new List<CompareItem>(differenceSet.Count);
            for (var idx = 0; idx < leftCount; idx++)
            {
                if (differenceSet.Contains(idx))
                {
                    leftSurvivors.Add(LeftSubtitles[idx]);
                    rightSurvivors.Add(RightSubtitles[idx]);
                }
            }

            // Rows beyond the left count were never removed by the old per-row loop.
            for (var idx = leftCount; idx < RightSubtitles.Count; idx++)
            {
                rightSurvivors.Add(RightSubtitles[idx]);
            }

            // Rebuild both collections in one pass instead of one RemoveAt notification per row.
            LeftSubtitles.Clear();
            RightSubtitles.Clear();
            foreach (var item in leftSurvivors)
            {
                LeftSubtitles.Add(item);
            }

            foreach (var item in rightSurvivors)
            {
                RightSubtitles.Add(item);
            }
        }

        SetStatusText(differences, totalWords, wordsChanged, min);
    }

    private void SetStatusText(List<int> differences, int totalWords, int wordsChanged, int min)
    {
        if (differences.Count >= min)
        {
            StatusText = Se.Language.File.SubtitlesNotAlike;
        }
        else
        {
            if (wordsChanged != totalWords && wordsChanged > 0)
            {
                var formatString = Se.Language.File.XNumberOfDifferenceAndPercentChanged;
                if (ShouldBreakToLetter())
                {
                    formatString = Se.Language.File.XNumberOfDifferenceAndPercentLettersChanged;
                }

                StatusText = string.Format(formatString, differences.Count, wordsChanged * 100.00 / totalWords);
            }
            else
            {
                StatusText = string.Format(Se.Language.File.XNumberOfDifference, differences.Count);
            }
        }
    }

    private enum ItemColumn
    {
        All,
        Number,
        StartTime,
        EndTime,
        Text
    }

    private void SetItemBackgroundColor(int index, bool isLeft, IBrush brush, ItemColumn column)
    {
        var collection = isLeft ? LeftSubtitles : RightSubtitles;
        if (index >= collection.Count)
        {
            return;
        }

        var item = collection[index];

        switch (column)
        {
            case ItemColumn.All:
                item.NumberBackgroundBrush = brush;
                item.StartTimeBackgroundBrush = brush;
                item.EndTimeBackgroundBrush = brush;
                item.TextBackgroundBrush = brush;
                break;
            case ItemColumn.Number:
                item.NumberBackgroundBrush = brush;
                break;
            case ItemColumn.StartTime:
                item.StartTimeBackgroundBrush = brush;
                break;
            case ItemColumn.EndTime:
                item.EndTimeBackgroundBrush = brush;
                break;
            case ItemColumn.Text:
                item.TextBackgroundBrush = brush;
                break;
        }
    }

    private void ResetAllBackgroundColors()
    {
        foreach (var item in LeftSubtitles)
        {
            item.NumberBackgroundBrush = TransparentBrush;
            item.StartTimeBackgroundBrush = TransparentBrush;
            item.EndTimeBackgroundBrush = TransparentBrush;
            item.TextBackgroundBrush = TransparentBrush;
        }

        foreach (var item in RightSubtitles)
        {
            item.NumberBackgroundBrush = TransparentBrush;
            item.StartTimeBackgroundBrush = TransparentBrush;
            item.EndTimeBackgroundBrush = TransparentBrush;
            item.TextBackgroundBrush = TransparentBrush;
        }
    }

    private bool ShouldBreakToLetter() => _language != null && (_language == "ja" || _language == "zh");

    // The word/letter diff choice needs the subtitle language (letters for ja/zh). Detection
    // is whole-file work, so it only runs when one of the sides was (re)loaded.
    private void DetectLanguageIfNeeded()
    {
        if (!_languageDirty)
        {
            return;
        }

        _languageDirty = false;
        var lines = _leftLines.Count > 0 ? _leftLines : _rightLines;
        var subtitle = new Subtitle();
        foreach (var line in lines)
        {
            subtitle.Paragraphs.Add(new Paragraph(line.Text, line.StartTime.TotalMilliseconds, line.EndTime.TotalMilliseconds));
        }

        _language = subtitle.Paragraphs.Count > 0
            ? LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle)
            : string.Empty;
    }

    /// <summary>
    /// Lines the two sides up, with a blank row where a line exists on one side only - see
    /// <see cref="CompareAligner"/>. The user's sync points are honoured as forced pairs (#15394).
    /// </summary>
    private void InsertMissingLines()
    {
        if (LeftSubtitles.Count == 0 || RightSubtitles.Count == 0)
        {
            return;
        }

        var leftItems = LeftSubtitles.ToList();
        var rightItems = RightSubtitles.ToList();
        var pairs = CompareAligner.Align(
            leftItems.Select(ToAlignerLine).ToList(),
            rightItems.Select(ToAlignerLine).ToList(),
            IsTimeEqual,
            GetSyncPointIndexes(leftItems, rightItems));

        _alignedPairs = new HashSet<(Guid Left, Guid Right)>();
        LeftSubtitles.Clear();
        RightSubtitles.Clear();
        foreach (var pair in pairs)
        {
            if (pair.Left >= 0 && pair.Right >= 0 && leftItems[pair.Left].Line is { } l && rightItems[pair.Right].Line is { } r)
            {
                _alignedPairs.Add((l.Id, r.Id));
            }

            LeftSubtitles.Add(pair.Left >= 0 ? leftItems[pair.Left] : new CompareItem());
            RightSubtitles.Add(pair.Right >= 0 ? rightItems[pair.Right] : new CompareItem());
        }
    }

    private CompareAligner.Line ToAlignerLine(CompareItem item) => new(NormalizeForCompare(item.Text), item.StartTime, item.EndTime);

    private List<(int Left, int Right)> GetSyncPointIndexes(List<CompareItem> leftItems, List<CompareItem> rightItems)
    {
        var result = new List<(int Left, int Right)>();
        if (_syncPoints.Count == 0)
        {
            return result;
        }

        var leftIndexes = IndexById(leftItems);
        var rightIndexes = IndexById(rightItems);
        foreach (var sp in _syncPoints)
        {
            if (leftIndexes.TryGetValue(sp.LeftId, out var l) && rightIndexes.TryGetValue(sp.RightId, out var r))
            {
                result.Add((l, r));
            }
        }

        return result;
    }

    private static Dictionary<Guid, int> IndexById(List<CompareItem> items)
    {
        var result = new Dictionary<Guid, int>(items.Count);
        for (var i = 0; i < items.Count; i++)
        {
            if (items[i].Line is { } line)
            {
                result.TryAdd(line.Id, i);
            }
        }

        return result;
    }

    private bool AreTextsEqual(CompareItem p1, CompareItem p2)
    {
        return NormalizeForCompare(p1.Text) == NormalizeForCompare(p2.Text);
    }

    /// <summary>
    /// Strips whatever the two ignore options say to ignore, so one place decides what counts as
    /// a difference - the row coloring, the word statistics and the in-cell highlighting all go
    /// through this. RemoveHtmlTags must be told to take the ASSA tags too, or "Ignore formatting"
    /// does nothing at all on an .ass/.ssa file, where every tag is {\an8}-style (#14299).
    /// </summary>
    internal string NormalizeForCompare(string text)
    {
        if (IgnoreFormatting)
        {
            text = HtmlUtil.RemoveHtmlTags(text, true);
        }

        return IgnoreWhiteSpace ? RemoveWhiteSpace(text) : text.Trim();
    }

    public static string RemoveWhiteSpace(string text)
    {
        var sb = new System.Text.StringBuilder();
        foreach (var c in text)
        {
            if (!char.IsWhiteSpace(c))
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }

    private static bool IsTimeEqual(TimeSpan t1, TimeSpan t2)
    {
        if (Configuration.Settings.General.UseTimeFormatHHMMSSFF)
        {
            return new TimeCode(t1).ToDisplayString() == new TimeCode(t2).ToDisplayString();
        }

        const double tolerance = 0.1;
        return Math.Abs(t1.TotalMilliseconds - t2.TotalMilliseconds) < tolerance;
    }

    [RelayCommand]
    private async Task PickLeftSubtitleFile()
    {
        if (!await ConfirmDiscardChangesAsync())
        {
            return;
        }

        var fileName = await _fileHelper.PickOpenSubtitleFile(Window!, Se.Language.General.OpenSubtitleFileTitle);
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        var subtitle = Subtitle.Parse(fileName);
        if (subtitle == null)
        {
            return;
        }

        _leftLines.Clear();
        foreach (var line in subtitle.Paragraphs)
        {
            _leftLines.Add(new SubtitleLineViewModel(line, subtitle.OriginalFormat));
        }

        LeftFileNameHasChanges = false;
        LeftFileName = fileName;

        // Another file on the left is no longer the editor's subtitle - there is nothing to apply it to.
        IsLeftEditable = false;
        ResetEdits();
        ResetSyncPoints();

        _languageDirty = true;
        Dispatcher.UIThread.Post(CompareAndSelectFirst);
    }

    [RelayCommand]
    private async Task PickRightSubtitleFile()
    {
        var fileName = await _fileHelper.PickOpenSubtitleFile(Window!, Se.Language.General.OpenSubtitleFileTitle);
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        var subtitle = Subtitle.Parse(fileName);
        if (subtitle == null)
        {
            return;
        }

        ResetSyncPoints();

        _rightLines.Clear();
        foreach (var line in subtitle.Paragraphs)
        {
            _rightLines.Add(new SubtitleLineViewModel(line, subtitle.OriginalFormat));
        }

        RightFileName = fileName;
        IsReloadFromFileVisible = false;

        _languageDirty = true;
        Dispatcher.UIThread.Post(CompareAndSelectFirst);
    }

    [RelayCommand]
    private void ReloadRightFromFile()
    {
        var fileName = LeftFileName;
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        var subtitle = Subtitle.Parse(fileName);
        if (subtitle == null)
        {
            return;
        }

        ResetSyncPoints();

        _rightLines.Clear();
        foreach (var line in subtitle.Paragraphs)
        {
            _rightLines.Add(new SubtitleLineViewModel(line, subtitle.OriginalFormat));
        }

        RightFileName = fileName;
        IsReloadFromFileVisible = false;

        _languageDirty = true;
        Dispatcher.UIThread.Post(CompareAndSelectFirst);
    }

    /// <summary>Picks the row's current line as one half of a sync point; completes it when a reference line is waiting.</summary>
    [RelayCommand]
    private void PickSyncCurrent(CompareRow? row)
    {
        if (row?.Left.Line is not { } line)
        {
            return;
        }

        if (_pendingSyncRightId is { } rightId)
        {
            AddSyncPoint(line.Id, rightId);
            return;
        }

        ClearSyncPointMessage();
        _pendingSyncLeftId = line.Id;
        _pendingSyncNumber = line.Number;
        SyncPointHint = string.Format(Se.Language.File.CompareSyncCurrentPickedX, line.Number);
        UpdateSyncFlags();
    }

    /// <summary>Picks the row's reference line as one half of a sync point; completes it when a current line is waiting.</summary>
    [RelayCommand]
    private void PickSyncReference(CompareRow? row)
    {
        if (row?.Right.Line is not { } line)
        {
            return;
        }

        if (_pendingSyncLeftId is { } leftId)
        {
            AddSyncPoint(leftId, line.Id);
            return;
        }

        ClearSyncPointMessage();
        _pendingSyncRightId = line.Id;
        _pendingSyncNumber = line.Number;
        SyncPointHint = string.Format(Se.Language.File.CompareSyncReferencePickedX, line.Number);
        UpdateSyncFlags();
    }

    /// <summary>Completes the waiting sync point with the selected row's line on the other side.</summary>
    [RelayCommand]
    private void ApplySync()
    {
        if (_pendingSyncLeftId != null)
        {
            PickSyncReference(SelectedRow);
        }
        else if (_pendingSyncRightId != null)
        {
            PickSyncCurrent(SelectedRow);
        }
    }

    [RelayCommand]
    private void RemoveSyncPoint(CompareRow? row)
    {
        if (row == null || !row.IsSyncPoint)
        {
            return;
        }

        PushUndo();
        _syncPoints.RemoveAll(p => p.LeftId == row.Left.Line?.Id && p.RightId == row.Right.Line?.Id);
        UpdateSyncPointCount();
        CompareKeepingPlace(row.Left.Line?.Id, Rows.IndexOf(row));
    }

    [RelayCommand]
    private void ClearSyncPoints()
    {
        if (_syncPoints.Count == 0)
        {
            CancelSyncPick();
            return;
        }

        var lineId = SelectedRow?.Left.Line?.Id;
        PushUndo();
        ResetSyncPoints();
        CompareKeepingPlace(lineId);
    }

    [RelayCommand]
    private void CancelSyncPick()
    {
        _pendingSyncLeftId = null;
        _pendingSyncRightId = null;
        SyncPointHint = string.Empty;
        UpdateSyncFlags();
    }

    private bool IsSyncPickPending => _pendingSyncLeftId != null || _pendingSyncRightId != null;

    /// <summary>
    /// Adds the pair, dropping any sync point it contradicts - one that shares a line with it or
    /// would cross it - so the newest choice wins, then re-aligns. When the current side can be
    /// edited and the two start times differ, it also syncs the timing: the current line takes
    /// the reference line's start, and the lines after it move by the same amount, up to the
    /// next sync point - so each sync point sets the offset of its own stretch.
    /// </summary>
    private void AddSyncPoint(Guid leftId, Guid rightId)
    {
        _pendingSyncLeftId = null;
        _pendingSyncRightId = null;
        SyncPointHint = string.Empty;

        var leftIndex = _leftLines.FindIndex(p => p.Id == leftId);
        var rightIndex = _rightLines.FindIndex(p => p.Id == rightId);
        if (leftIndex < 0 || rightIndex < 0)
        {
            UpdateSyncFlags();
            return;
        }

        var leftLine = _leftLines[leftIndex];
        var rightLine = _rightLines[rightIndex];
        var offset = rightLine.StartTime - leftLine.StartTime;
        var shiftTiming = IsLeftEditable && !IsTimeEqual(leftLine.StartTime, rightLine.StartTime);

        // Already side by side with the same start: there is nothing to sync, so say so instead
        // of leaving a marker that looks like it did not work.
        if (!shiftTiming && _alignedPairs.Contains((leftId, rightId)))
        {
            ShowSyncPointMessage(string.Format(Se.Language.File.CompareSyncAlreadyPairedXY, leftLine.Number, rightLine.Number));
            UpdateSyncFlags();
            return;
        }

        // Every sync point change is undoable, as Undo restores the sync points with the lines.
        PushUndo();
        _syncPoints.RemoveAll(p =>
        {
            var l = _leftLines.FindIndex(x => x.Id == p.LeftId);
            var r = _rightLines.FindIndex(x => x.Id == p.RightId);
            return l < 0 || r < 0 || (long)(l - leftIndex) * (r - rightIndex) <= 0;
        });
        _syncPoints.Add(new SyncPoint(leftId, rightId));
        UpdateSyncPointCount();

        if (shiftTiming)
        {
            var endIndex = GetNextSyncPointLeftIndex(leftIndex);
            for (var i = leftIndex; i < endIndex; i++)
            {
                var line = _leftLines[i];
                SetTimes(line, line.StartTime + offset, line.EndTime + offset);
                _editedIds.Add(line.Id);
            }

            var lastNumber = _leftLines[endIndex - 1].Number;
            var offsetText = FormatOffset(offset);
            AddChange(string.Format(Se.Language.File.CompareChangeSyncShiftXYZ, leftLine.Number, lastNumber, offsetText));
            ShowSyncPointMessage(string.Format(Se.Language.File.CompareSyncPointShiftedXYZW, leftLine.Number, lastNumber, offsetText, rightLine.Number));
        }
        else
        {
            ShowSyncPointMessage(string.Format(Se.Language.File.CompareSyncPointSetXY, leftLine.Number, rightLine.Number));
        }

        CompareKeepingPlace(leftId);
    }

    /// <summary>Where the stretch that starts at <paramref name="leftIndex"/> ends: the next sync point's current line, or the end.</summary>
    private int GetNextSyncPointLeftIndex(int leftIndex)
    {
        var end = _leftLines.Count;
        foreach (var sp in _syncPoints)
        {
            var l = _leftLines.FindIndex(x => x.Id == sp.LeftId);
            if (l > leftIndex && l < end)
            {
                end = l;
            }
        }

        return end;
    }

    private static string FormatOffset(TimeSpan offset)
    {
        var sign = offset < TimeSpan.Zero ? "-" : "+";
        return sign + new TimeCode(offset.Duration()).ToDisplayString();
    }

    /// <summary>A short confirmation in the sync bar, gone after a few seconds or at the next pick.</summary>
    private void ShowSyncPointMessage(string message)
    {
        _syncMessageTimer?.Stop();
        SyncPointMessage = message;
        _syncMessageTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(6) };
        _syncMessageTimer.Tick += (_, _) =>
        {
            _syncMessageTimer?.Stop();
            SyncPointMessage = string.Empty;
        };
        _syncMessageTimer.Start();
    }

    private void ClearSyncPointMessage()
    {
        _syncMessageTimer?.Stop();
        SyncPointMessage = string.Empty;
    }

    private void ResetSyncPoints()
    {
        ClearSyncPointMessage();
        _syncPoints.Clear();
        _pendingSyncLeftId = null;
        _pendingSyncRightId = null;
        SyncPointCount = 0;
        SyncPointHint = string.Empty;
    }

    /// <summary>Counts only the sync points whose two lines still exist.</summary>
    private void UpdateSyncPointCount()
    {
        var count = 0;
        foreach (var sp in _syncPoints)
        {
            if (_leftLines.Exists(p => p.Id == sp.LeftId) && _rightLines.Exists(p => p.Id == sp.RightId))
            {
                count++;
            }
        }

        SyncPointCount = count;
    }

    private bool IsSyncPoint(CompareItem left, CompareItem right)
    {
        if (left.Line is not { } l || right.Line is not { } r)
        {
            return false;
        }

        foreach (var sp in _syncPoints)
        {
            if (sp.LeftId == l.Id && sp.RightId == r.Id)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Marks the half of a sync point that is waiting for its other half.</summary>
    private void UpdateSyncFlags()
    {
        foreach (var row in Rows)
        {
            row.IsLeftSyncPending = _pendingSyncLeftId != null && row.Left.Line?.Id == _pendingSyncLeftId;
            row.IsRightSyncPending = _pendingSyncRightId != null && row.Right.Line?.Id == _pendingSyncRightId;
        }

        OnPropertyChanged(nameof(PickSyncCurrentHeader));
        OnPropertyChanged(nameof(PickSyncReferenceHeader));
        OnPropertyChanged(nameof(CanApplySync));
    }

    partial void OnSelectedRowChanged(CompareRow? value)
    {
        OnPropertyChanged(nameof(CanApplySync));
    }

    [RelayCommand]
    private void PreviousDifference()
    {
        var idx = SelectedRow == null ? Rows.Count : Rows.IndexOf(SelectedRow);
        while (idx > 0)
        {
            idx--;
            if (Rows[idx].Kind != CompareRowKind.Same)
            {
                SelectRow(idx);
                return;
            }
        }
    }

    [RelayCommand]
    private void NextDifference()
    {
        var idx = SelectedRow == null ? -1 : Rows.IndexOf(SelectedRow);
        while (idx < Rows.Count - 1)
        {
            idx++;
            if (Rows[idx].Kind != CompareRowKind.Same)
            {
                SelectRow(idx);
                return;
            }
        }
    }

    [RelayCommand]
    private void ShowAll() => SetCompareVisual(CompareVisualType.All);

    [RelayCommand]
    private void ShowDifferences() => SetCompareVisual(CompareVisualType.ShowOnlyDifferences);

    [RelayCommand]
    private void ShowTextDifferences() => SetCompareVisual(CompareVisualType.ShowOnlyDifferencesInText);

    private void SetCompareVisual(CompareVisualType type)
    {
        var visual = CompareVisuals.FirstOrDefault(p => p.Type == type);
        if (visual == null || visual == SelectedCompareVisual)
        {
            // Clicking the active tab toggled it off; give it its checked state back.
            OnPropertyChanged(nameof(SelectedCompareVisual));
            return;
        }

        SelectedCompareVisual = visual;
        CompareKeepingPlace(SelectedRow?.Left.Line?.Id);
    }

    /// <summary>The gutter arrow: a differing pair takes the reference's text and timing, a reference-only line is inserted.</summary>
    [RelayCommand]
    private void TakeReference(CompareRow? row)
    {
        if (row == null || !row.CanTakeReference || row.Right.Line is not { } reference)
        {
            return;
        }

        if (row.Kind == CompareRowKind.OnlyRight)
        {
            var inserted = new SubtitleLineViewModel(reference, generateNewId: true);
            var index = _leftLines.FindIndex(p => p.StartTime > inserted.StartTime);
            ApplyEdit(inserted.Id, () =>
            {
                _leftLines.Insert(index < 0 ? _leftLines.Count : index, inserted);
                return inserted;
            }, Se.Language.File.CompareChangeInsertedX);
            return;
        }

        ApplyToLeftLine(row, line =>
        {
            line.Text = reference.Text;
            SetTimes(line, reference.StartTime, reference.EndTime);
        }, Se.Language.File.CompareChangeTextAndTimingX);
    }

    [RelayCommand]
    private void TakeReferenceText(CompareRow? row)
    {
        if (row?.Right.Line is { } reference && row.CanTakeFromPair)
        {
            ApplyToLeftLine(row, line => line.Text = reference.Text, Se.Language.File.CompareChangeTextX);
        }
    }

    [RelayCommand]
    private void TakeReferenceTiming(CompareRow? row)
    {
        if (row?.Right.Line is { } reference && row.CanTakeFromPair)
        {
            ApplyToLeftLine(row, line => SetTimes(line, reference.StartTime, reference.EndTime), Se.Language.File.CompareChangeTimingX);
        }
    }

    [RelayCommand]
    private void DeleteCurrentLine(CompareRow? row)
    {
        if (row == null || !row.CanEdit || row.Left.Line is not { } line)
        {
            return;
        }

        var number = line.Number;
        var rowIndex = Rows.IndexOf(row);
        PushUndo();
        _leftLines.Remove(line);
        _editedIds.Remove(line.Id);
        if (_syncPoints.RemoveAll(p => p.LeftId == line.Id) > 0)
        {
            UpdateSyncPointCount();
        }

        Renumber();
        AddChange(string.Format(Se.Language.File.CompareChangeDeletedX, number));
        CompareKeepingPlace(null, rowIndex);
    }

    [RelayCommand]
    private void BeginEdit(CompareRow? row)
    {
        if (row == null || !row.CanEdit)
        {
            return;
        }

        foreach (var other in Rows)
        {
            if (other.IsEditing && other != row)
            {
                other.IsEditing = false;
            }
        }

        SelectedRow = row;
        KeepRowInPlace(row, row.BeginEdit);
    }

    [RelayCommand]
    private void CommitEdit(CompareRow? row)
    {
        if (row == null || !row.IsEditing)
        {
            return;
        }

        row.IsEditing = false;
        var line = row.Left.Line;
        if (line == null)
        {
            return;
        }

        var end = row.EditEnd < row.EditStart ? row.EditStart : row.EditEnd;
        if (line.Text == row.EditText && IsTimeEqual(line.StartTime, row.EditStart) && IsTimeEqual(line.EndTime, end))
        {
            FocusRows();
            return;
        }

        ApplyToLeftLine(row, l =>
        {
            l.Text = row.EditText;
            SetTimes(l, row.EditStart, end);
        }, Se.Language.File.CompareChangeEditedX);
    }

    [RelayCommand]
    private void CancelEdit(CompareRow? row)
    {
        if (row != null)
        {
            KeepRowInPlace(row, () => row.IsEditing = false);
        }

        FocusRows();
    }

    /// <summary>
    /// Opening or closing the inline editor changes the row's height, and the virtualizing list
    /// then re-estimates where every row sits - with the scroll offset unchanged, the whole list
    /// jumped by about the height difference. Puts the row back where it was on screen.
    /// </summary>
    private void KeepRowInPlace(CompareRow row, Action change)
    {
        var scrollViewer = RowsView?.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
        var before = GetRowTop(row, scrollViewer);
        change();
        if (scrollViewer == null || before == null)
        {
            return;
        }

        RowsView!.UpdateLayout();
        if (GetRowTop(row, scrollViewer) is { } after && Math.Abs(after - before.Value) > 0.5)
        {
            scrollViewer.Offset = new Vector(scrollViewer.Offset.X, scrollViewer.Offset.Y + after - before.Value);
        }
    }

    private double? GetRowTop(CompareRow row, ScrollViewer? scrollViewer)
    {
        if (scrollViewer == null || RowsView?.ContainerFromItem(row) is not Control container)
        {
            return null;
        }

        return container.TranslatePoint(new Point(0, 0), scrollViewer)?.Y;
    }

    [RelayCommand]
    private void Undo()
    {
        if (_undoStack.Count == 0)
        {
            return;
        }

        var state = _undoStack.Pop();
        _leftLines = state.Lines;
        _editedIds = state.EditedIds;
        _changes = state.Changes;
        _syncPoints.Clear();
        _syncPoints.AddRange(state.SyncPoints);
        UpdateSyncPointCount();
        UpdatePendingChanges();
        CompareKeepingPlace(SelectedRow?.Left.Line?.Id, SelectedRow == null ? 0 : Rows.IndexOf(SelectedRow));
    }

    /// <summary>The current lines, edits included, for the main window to take over on Apply.</summary>
    public List<SubtitleLineViewModel> GetEditedLines() => _leftLines.Select(p => new SubtitleLineViewModel(p)).ToList();

    private void ApplyToLeftLine(CompareRow row, Action<SubtitleLineViewModel> edit, string changeFormat)
    {
        if (row.Left.Line is not { } line)
        {
            return;
        }

        ApplyEdit(line.Id, () =>
        {
            edit(line);
            return line;
        }, changeFormat);
    }

    private void ApplyEdit(Guid lineId, Func<SubtitleLineViewModel> edit, string changeFormat)
    {
        PushUndo();

        // The undo snapshot holds copies, so the live list can be changed in place from here.
        var line = edit();
        _editedIds.Add(lineId);
        Renumber();
        AddChange(string.Format(changeFormat, line.Number));
        CompareKeepingPlace(lineId);
    }

    private void PushUndo()
    {
        _undoStack.Push(new EditState(
            _leftLines.Select(p => new SubtitleLineViewModel(p)).ToList(),
            new HashSet<Guid>(_editedIds),
            new List<string>(_changes),
            new List<SyncPoint>(_syncPoints)));
    }

    private void AddChange(string description)
    {
        _changes.Add(description);
        UpdatePendingChanges();
    }

    private void UpdatePendingChanges()
    {
        PendingChangeCount = _changes.Count;
        LastChangeText = _changes.Count > 0 ? _changes[^1] : string.Empty;
    }

    private void ResetEdits()
    {
        _undoStack.Clear();
        _editedIds.Clear();
        _changes.Clear();
        UpdatePendingChanges();
    }

    /// <summary>Numbers follow the line order, as the main window will number them - reference-only rows take none (#13449).</summary>
    private void Renumber()
    {
        var number = 0;
        foreach (var line in _leftLines)
        {
            if (!line.IsReferenceOnly)
            {
                line.Number = ++number;
            }
        }
    }

    private static void SetTimes(SubtitleLineViewModel line, TimeSpan start, TimeSpan end)
    {
        line.StartTime = start;
        line.EndTime = end;
    }

    /// <summary>
    /// Re-runs the comparison after an edit without losing the user's place: the list keeps its
    /// scroll offset, and the row showing <paramref name="lineId"/> - or, for a line that is gone,
    /// the row now at <paramref name="fallbackIndex"/> - becomes the selected one.
    /// </summary>
    private void CompareKeepingPlace(Guid? lineId, int fallbackIndex = 0)
    {
        var scrollViewer = RowsView?.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
        var offset = scrollViewer?.Offset;

        Compare();

        var index = -1;
        if (lineId is { } id)
        {
            for (var i = 0; i < Rows.Count; i++)
            {
                if (Rows[i].Left.Line?.Id == id)
                {
                    index = i;
                    break;
                }
            }
        }

        if (index < 0)
        {
            index = Math.Clamp(fallbackIndex, 0, Math.Max(0, Rows.Count - 1));
        }

        Dispatcher.UIThread.Post(() =>
        {
            if (scrollViewer != null && offset is { } o)
            {
                scrollViewer.Offset = o;
            }

            SelectRow(index);
        }, DispatcherPriority.Loaded);
    }

    private async Task<bool> ConfirmDiscardChangesAsync()
    {
        if (!HasPendingChanges || Window == null)
        {
            return true;
        }

        var answer = await MessageBox.Show(
            Window,
            Se.Language.File.Compare,
            string.Format(Se.Language.File.CompareDiscardXChanges, PendingChangeCount),
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        return answer == MessageBoxResult.Yes;
    }

    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;
        Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Close();
    }

    /// <summary>Closing by Cancel, Escape or the title bar asks first when there are edits to lose.</summary>
    internal void WindowClosing(object? sender, WindowClosingEventArgs e)
    {
        if (OkPressed || _closeConfirmed || !HasPendingChanges)
        {
            return;
        }

        e.Cancel = true;
        Dispatcher.UIThread.Post(async () =>
        {
            if (await ConfirmDiscardChangesAsync())
            {
                _closeConfirmed = true;
                Window?.Close();
            }
        });
    }

    [RelayCommand]
    private async Task Export()
    {
        var targetFileName = string.IsNullOrEmpty(LeftFileName) ? "compare.html" : System.IO.Path.GetFileNameWithoutExtension(LeftFileName) + "-compare.html";
        var fileName = await _fileHelper.PickSaveFile(Window!, ".html", targetFileName, Se.Language.File.SaveCompareHtmlTitle);
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html>");
        sb.AppendLine("  <head>");
        sb.AppendLine("    <title>Subtitle Edit compare</title>");
        sb.AppendLine("  </head>");
        sb.AppendLine("  <style>");
        sb.AppendLine("    td { font-family: Tahoma, Verdana, 'Noto Sans', Ubuntu; padding: 8px; }");
        sb.AppendLine("  </style>");
        sb.AppendLine("  <body>");
        sb.AppendLine("    <h1>Subtitle Edit compare</h1>");
        sb.AppendLine("    <table>");
        sb.AppendLine("    <tr>");
        sb.AppendLine("      <th colspan='5' style='text-align:left'>" + GetFileName(LeftFileName) + "</th>");
        sb.AppendLine("      <th>&nbsp;</th>");
        sb.AppendLine("      <th colspan='5' style='text-align:left'>" + GetFileName(RightFileName) + "</th>");
        sb.AppendLine("    </tr>");
        // Belt and braces alongside the button guard: only the padded rows are pairs.
        var rowCount = Math.Min(LeftSubtitles.Count, RightSubtitles.Count);
        for (var i = 0; i < rowCount; i++)
        {
            var itemLeft = LeftSubtitles[i];
            var itemRight = RightSubtitles[i];
            var (leftTextHtml, rightTextHtml) = GetHtmlTextPair(itemLeft, itemRight);

            sb.AppendLine("    <tr>");
            sb.AppendLine("      <td" + GetHtmlBackgroundColor(itemLeft.NumberBackgroundBrush) + ">" + GetHtmlText(itemLeft, itemLeft.Number.ToString()) + "</td>");
            sb.AppendLine("      <td" + GetHtmlBackgroundColor(itemLeft.StartTimeBackgroundBrush) + ">" + GetHtmlText(itemLeft, new TimeCode(itemLeft.StartTime).ToDisplayString()) + "</td>");
            sb.AppendLine("      <td" + GetHtmlBackgroundColor(itemLeft.EndTimeBackgroundBrush) + ">" + GetHtmlText(itemLeft, new TimeCode(itemLeft.EndTime).ToDisplayString()) + "</td>");
            sb.AppendLine("      <td>" + HtmlUtil.EncodeNamed(CompareRow.FormatDuration(itemLeft)) + "</td>");
            sb.AppendLine("      <td" + GetHtmlBackgroundColor(itemLeft.TextBackgroundBrush) + ">" + leftTextHtml + "</td>");
            sb.AppendLine("      <td>&nbsp;</td>");
            sb.AppendLine("      <td" + GetHtmlBackgroundColor(itemRight.NumberBackgroundBrush) + ">" + GetHtmlText(itemRight, itemRight.Number.ToString()) + "</td>");
            sb.AppendLine("      <td" + GetHtmlBackgroundColor(itemRight.StartTimeBackgroundBrush) + ">" + GetHtmlText(itemRight, new TimeCode(itemRight.StartTime).ToDisplayString()) + "</td>");
            sb.AppendLine("      <td" + GetHtmlBackgroundColor(itemRight.EndTimeBackgroundBrush) + ">" + GetHtmlText(itemRight, new TimeCode(itemRight.EndTime).ToDisplayString()) + "</td>");
            sb.AppendLine("      <td>" + HtmlUtil.EncodeNamed(CompareRow.FormatDuration(itemRight)) + "</td>");
            sb.AppendLine("      <td" + GetHtmlBackgroundColor(itemRight.TextBackgroundBrush) + ">" + rightTextHtml + "</td>");
            sb.AppendLine("    </tr>");
        }
        sb.AppendLine("    <tr>");
        sb.AppendLine("      <td colspan='11' style='text-align:left'><br />" + StatusText + "</td>");
        sb.AppendLine("    </tr>");
        sb.AppendLine("    </table>");
        sb.AppendLine("  </body>");
        sb.AppendLine("</html>");
        await System.IO.File.WriteAllTextAsync(fileName, sb.ToString());
        await _folderHelper.OpenFolderWithFileSelected(Window!, fileName);
    }

    private static string GetFileName(string fileName)
    {
        try
        {
            return string.IsNullOrEmpty(fileName) ? string.Empty : System.IO.Path.GetFileName(fileName);
        }
        catch
        {
            return fileName;
        }
    }

    partial void OnLeftFileNameChanged(string value)
    {
        OnPropertyChanged(nameof(LeftFileNameDisplay));
    }

    partial void OnRightFileNameChanged(string value)
    {
        OnPropertyChanged(nameof(RightFileNameDisplay));
    }

    /// <summary>
    /// The text cells of an exported row carry the same word-level marking the window shows:
    /// the differing runs in red, the rest of a differing line on pale green. A pair the options
    /// count as equal is exported plain, exactly as the window leaves it unmarked.
    /// </summary>
    private (string left, string right) GetHtmlTextPair(CompareItem left, CompareItem right)
    {
        if (left.IsDefault || right.IsDefault || AreTextsEqual(left, right))
        {
            return (GetHtmlText(left, left.Text), GetHtmlText(right, right.Text));
        }

        return TextDiffHighlighter.CompareToHtml(left.Text, right.Text, IgnoreWhiteSpace, IgnoreFormatting);
    }

    private static string GetHtmlText(CompareItem p, string text)
    {
        return p.IsDefault ? string.Empty : HtmlUtil.EncodeNamed(text)
            .Replace("\r\n", "<br />")
            .Replace("\r", "<br />")
            .Replace("\n", "<br />");
    }

    private static string GetHtmlBackgroundColor(IBrush brush)
    {
        // The exported page is white with black text, so a highlight always exports as its
        // light pastel - the dark theme's row brushes would render as near-black cells.
        var exportColor = CompareColors.GetExportColor(brush);
        if (exportColor == null)
        {
            return string.Empty;
        }

        var c = exportColor.Value;
        var htmlColor = $"#{c.R:X2}{c.G:X2}{c.B:X2}";
        return $" style='background-color:{htmlColor}'";
    }

    private void Close()
    {
        Dispatcher.UIThread.Post(() =>
        {
            Window?.Close();
        });
    }

    internal void KeyDown(object? sender, KeyEventArgs e)
    {
        var editing = Rows.FirstOrDefault(p => p.IsEditing);
        var ctrl = e.KeyModifiers.HasFlag(KeyModifiers.Control) || e.KeyModifiers.HasFlag(KeyModifiers.Meta);

        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            if (editing != null)
            {
                CancelEdit(editing);
            }
            else if (IsSyncPickPending)
            {
                CancelSyncPick();
            }
            else
            {
                Close();
            }
        }
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/compare");
        }
        else if (editing != null)
        {
            if (e.Key == Key.Enter && ctrl)
            {
                e.Handled = true;
                CommitEdit(editing);
            }
        }
        else if (e.Key == Key.F2)
        {
            e.Handled = true;
            BeginEdit(SelectedRow);
        }
        else if (e.Key == Key.Z && ctrl)
        {
            e.Handled = true;
            Undo();
        }
        else if (e.Key == Key.Left && e.KeyModifiers == KeyModifiers.Alt)
        {
            e.Handled = true;
            TakeReference(SelectedRow);
        }
        else if (e.Key == Key.Delete && e.KeyModifiers == KeyModifiers.None && SelectedRow is { CanEdit: true })
        {
            e.Handled = true;
            DeleteCurrentLine(SelectedRow);
        }
        else if (e.Key == Key.F8)
        {
            e.Handled = true;
            if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
            {
                PreviousDifference();
            }
            else
            {
                NextDifference();
            }
        }
    }

    internal void SelectRow(int index)
    {
        if (index < 0 || index >= Rows.Count)
        {
            return;
        }

        SelectedRow = Rows[index];
        Dispatcher.UIThread.Post(() =>
        {
            if (RowsView != null && SelectedRow != null)
            {
                RowsView.ScrollIntoView(SelectedRow);
            }
        });
    }

    /// <summary>Scrolls so that row <paramref name="index"/> is in view, for the overview ruler.</summary>
    internal void ScrollToRow(int index)
    {
        if (RowsView != null && index >= 0 && index < Rows.Count)
        {
            RowsView.ScrollIntoView(index);
        }
    }

    private void FocusRows()
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (RowsView?.ContainerFromItem(SelectedRow!) is { } container)
            {
                container.Focus();
            }
            else
            {
                RowsView?.Focus();
            }
        });
    }

    internal void CheckBoxChanged(object? sender, RoutedEventArgs e)
    {
        Task.Delay(100).ContinueWith(_ =>
        {
            Dispatcher.UIThread.Post(CompareAndSelectFirst);
        });
    }

    internal void FileGridOnDragOver(object? sender, DragEventArgs e)
    {
        if (e.DataTransfer.Contains(DataFormat.File))
        {
            e.DragEffects = DragDropEffects.Copy; // show copy cursor
        }
        else
        {
            e.DragEffects = DragDropEffects.None;
        }

        e.Handled = true;
    }

    internal void FileGridOnDropLeft(object? sender, DragEventArgs e)
    {
        if (!e.DataTransfer.Contains(DataFormat.File))
        {
            return;
        }

        var files = e.DataTransfer.TryGetFiles();
        if (files != null)
        {
            Dispatcher.UIThread.Post(async () =>
            {
                if (!await ConfirmDiscardChangesAsync())
                {
                    return;
                }

                foreach (var file in files)
                {
                    var path = file.Path?.LocalPath;
                    var subtitle = Subtitle.Parse(path);
                    if (subtitle == null || path == null)
                    {
                        return;
                    }

                    _leftLines.Clear();
                    foreach (var line in subtitle.Paragraphs)
                    {
                        _leftLines.Add(new SubtitleLineViewModel(line, subtitle.OriginalFormat));
                    }

                    LeftFileNameHasChanges = false;
                    LeftFileName = path;
                    IsLeftEditable = false;
                    ResetEdits();
                    ResetSyncPoints();

                    _languageDirty = true;
                    Dispatcher.UIThread.Post(CompareAndSelectFirst);
                    break;
                }
            });
        }
    }

    internal void FileGridOnDropRight(object? sender, DragEventArgs e)
    {
        if (!e.DataTransfer.Contains(DataFormat.File))
        {
            return;
        }

        var files = e.DataTransfer.TryGetFiles();
        if (files != null)
        {
            Dispatcher.UIThread.Post(() =>
            {
                foreach (var file in files)
                {
                    var path = file.Path?.LocalPath;
                    var subtitle = Subtitle.Parse(path);
                    if (subtitle == null || path == null)
                    {
                        return;
                    }

                    ResetSyncPoints();

                    _rightLines.Clear();
                    foreach (var line in subtitle.Paragraphs)
                    {
                        _rightLines.Add(new SubtitleLineViewModel(line, subtitle.OriginalFormat));
                    }

                    RightFileName = path;
                    IsReloadFromFileVisible = false;

                    _languageDirty = true;
                    Dispatcher.UIThread.Post(CompareAndSelectFirst);
                    break;
                }
            });
        }
    }
}
