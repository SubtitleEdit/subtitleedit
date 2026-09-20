using Avalonia.Controls;
using Avalonia.Input;
using Nikse.SubtitleEdit.Logic;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Nikse.SubtitleEdit.Features.Tools.MergeShortLines;

public partial class MergeShortLinesViewModel : ObservableObject, IClosingCleanup
{
    [ObservableProperty] private ObservableCollection<MergeShortLinesItem> _fixes;
    [ObservableProperty] private MergeShortLinesItem? _selectedFix;

    [ObservableProperty] private ObservableCollection<SubtitleLineViewModel> _subtitles;
    [ObservableProperty] private SubtitleLineViewModel? _selectedSubtitle;

    [ObservableProperty] private int _singleLineMaxLength;
    [ObservableProperty] private int _maxNumberOfLines;
    [ObservableProperty] private bool _highLight;

    [ObservableProperty] private string _fixesInfo;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }
    public List<SubtitleLineViewModel> AllSubtitlesFixed { get; set; }

    private List<SubtitleLineViewModel> _allSubtitles;

    private readonly System.Timers.Timer _previewTimer;
    private volatile bool _isClosing;
    private bool _isDirty;
    private List<double> _shotChanges;

    // Lines the user unticked in the "Apply" column. This, not the preview list, is what OK
    // applies: the preview is filled by the 250 ms timer, so it is empty right after opening
    // and stale right after a settings change, while this set is always current.
    private readonly HashSet<Guid> _excludedLineIds = new();

    public MergeShortLinesViewModel()
    {
        Fixes = new ObservableCollection<MergeShortLinesItem>();
        Subtitles = new ObservableCollection<SubtitleLineViewModel>();
        _allSubtitles = new List<SubtitleLineViewModel>();
        _shotChanges = new List<double>();
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

        // Guard the restart: OnClosingCleanup may have disposed the timer while this handler ran (#12739).
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

    private MergeShortLinesResult RunMerge(bool highlight)
    {
        var gapThresholdMs = Se.Settings.Tools.BridgeGaps.BridgeGapsSmallerThanMs;
        var unbreakLinesShorterThan = Se.Settings.General.UnbreakLinesShorterThan;

        return highlight
            ? MergeShortLinesHelper.MergeWithHighlights(_allSubtitles, _shotChanges, SingleLineMaxLength, MaxNumberOfLines, gapThresholdMs, unbreakLinesShorterThan, _excludedLineIds)
            : MergeShortLinesHelper.Merge(_allSubtitles, _shotChanges, SingleLineMaxLength, MaxNumberOfLines, gapThresholdMs, unbreakLinesShorterThan, _excludedLineIds);
    }

    private void UpdatePreview()
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (_isClosing)
            {
                return; // must not overwrite the final result Ok() just computed
            }

            Subtitles.Clear();
            AllSubtitlesFixed.Clear();

            var mergeResult = RunMerge(HighLight);

            AllSubtitlesFixed.AddRange(mergeResult.MergedSubtitles);

            // The preview re-runs when a checkbox is toggled, so only the rows that actually
            // changed are replaced - a full clear would throw away the grid's scroll position
            // and selection under the user's pointer.
            var replaced = ReplaceChangedRows(Fixes, mergeResult.Fixes);
            foreach (var fix in replaced)
            {
                fix.PropertyChanged += FixPropertyChanged;
            }

            FixesInfo = Fixes.Count == 0
                ? Se.Language.Tools.ApplyDurationLimits.NoChangesNeeded
                : string.Format(Se.Language.Tools.MergeShortLines.LinesMergedX, mergeResult.MergeCount);
        });
    }

    /// <summary>
    /// Makes <paramref name="target"/> equal to <paramref name="fresh"/> by replacing only the
    /// rows between the unchanged prefix and suffix. Returns the rows that were inserted.
    /// </summary>
    internal static List<MergeShortLinesItem> ReplaceChangedRows(ObservableCollection<MergeShortLinesItem> target, List<MergeShortLinesItem> fresh)
    {
        var prefix = 0;
        while (prefix < target.Count && prefix < fresh.Count && IsSameRow(target[prefix], fresh[prefix]))
        {
            prefix++;
        }

        var suffix = 0;
        while (suffix < target.Count - prefix && suffix < fresh.Count - prefix &&
               IsSameRow(target[target.Count - 1 - suffix], fresh[fresh.Count - 1 - suffix]))
        {
            suffix++;
        }

        for (var i = target.Count - 1 - suffix; i >= prefix; i--)
        {
            target.RemoveAt(i);
        }

        var inserted = new List<MergeShortLinesItem>();
        for (var i = prefix; i < fresh.Count - suffix; i++)
        {
            target.Insert(i, fresh[i]);
            inserted.Add(fresh[i]);
        }

        return inserted;
    }

    private static bool IsSameRow(MergeShortLinesItem a, MergeShortLinesItem b)
    {
        return a.SourceLineId == b.SourceLineId &&
               a.CanToggle == b.CanToggle &&
               a.Apply == b.Apply &&
               a.Number == b.Number &&
               a.Fix == b.Fix;
    }

    private void FixPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(MergeShortLinesItem.Apply) || sender is not MergeShortLinesItem fix || !fix.CanToggle)
        {
            return;
        }

        if (fix.Apply)
        {
            _excludedLineIds.Remove(fix.SourceLineId);
        }
        else
        {
            _excludedLineIds.Add(fix.SourceLineId);
        }

        // Refusing a merge can make the line head its own group, so the candidates after it
        // change: let the preview timer re-run the merge (it coalesces bulk toggles).
        SetChanged();
    }

    private void LoadSettings()
    {
        // 0 means "not saved yet" - fall back to the general defaults (#13514 pattern).
        SingleLineMaxLength = Se.Settings.Tools.MergeShortLinesSingleLineMaxLength > 0
            ? Se.Settings.Tools.MergeShortLinesSingleLineMaxLength
            : Se.Settings.General.SubtitleLineMaximumLength;
        MaxNumberOfLines = Se.Settings.Tools.MergeShortLinesMaxNumberOfLines > 0
            ? Se.Settings.Tools.MergeShortLinesMaxNumberOfLines
            : Se.Settings.General.MaxNumberOfLines;
    }

    private void SaveSettings()
    {
        Se.Settings.Tools.MergeShortLinesSingleLineMaxLength = SingleLineMaxLength;
        Se.Settings.Tools.MergeShortLinesMaxNumberOfLines = MaxNumberOfLines;
        Se.SaveSettings();
    }

    [RelayCommand]
    private void SelectAll()
    {
        foreach (var fix in Fixes)
        {
            if (fix.CanToggle)
            {
                fix.Apply = true;
            }
        }
    }

    [RelayCommand]
    private void SelectNone()
    {
        foreach (var fix in Fixes)
        {
            if (fix.CanToggle)
            {
                fix.Apply = false;
            }
        }
    }

    [RelayCommand]
    private void InvertSelection()
    {
        foreach (var fix in Fixes)
        {
            if (fix.CanToggle)
            {
                fix.Apply = !fix.Apply;
            }
        }
    }

    /// <summary>
    /// The gestures advertised by the fixes grid context menu: tick all, untick all and invert
    /// the "Apply" column. Called both from the window (focus sits on a button) and from a
    /// tunneling handler on the grid, which would otherwise swallow Ctrl+A as "select all rows".
    /// </summary>
    internal bool HandleFixesSelectionKey(KeyEventArgs e)
    {
        var isCommand = e.KeyModifiers.HasFlag(KeyModifiers.Control) || e.KeyModifiers.HasFlag(KeyModifiers.Meta);
        if (!isCommand || e.KeyModifiers.HasFlag(KeyModifiers.Alt))
        {
            return false;
        }

        var isShift = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
        if (e.Key == Key.A && !isShift)
        {
            SelectAll();
        }
        else if (e.Key == Key.D && !isShift)
        {
            SelectNone();
        }
        else if (e.Key == Key.I && isShift)
        {
            InvertSelection();
        }
        else
        {
            return false;
        }

        return true;
    }

    [RelayCommand]
    private void Ok()
    {
        if (Window == null)
        {
            return;
        }

        // Always recompute the real merge from the current settings instead of handing back
        // what the 250 ms preview timer last produced: the preview is empty when OK comes
        // right after opening, stale when it comes right after a settings change, and in
        // "highlight parts" mode it is not the real merge at all. The user's unticks are
        // carried by _excludedLineIds, which does not depend on the preview.
        var mergeResult = RunMerge(highlight: false);
        AllSubtitlesFixed.Clear();
        AllSubtitlesFixed.AddRange(mergeResult.MergedSubtitles);

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
            UiUtil.ShowHelp("features/merge-short-lines");
        }
        else if (HandleFixesSelectionKey(e))
        {
            e.Handled = true;
        }
    }

    public void Initialize(List<SubtitleLineViewModel> toList, List<double> shotChanges)
    {
        _allSubtitles = toList;
        _shotChanges = shotChanges;
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
}
