using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Tools.MergeSubtitlesWithSameText;

public partial class MergeSameTextViewModel : ObservableObject, IClosingCleanup
{
    [ObservableProperty] private ObservableCollection<MergeDisplayItem> _mergeItems;
    [ObservableProperty] private MergeDisplayItem? _selectedMergeItem;
    [ObservableProperty] private int _maxMillisecondsBetweenLines;
    [ObservableProperty] private bool _includeIncrementingLines;
    [ObservableProperty] private bool _includeRollUpCaptions;
    [ObservableProperty] private ObservableCollection<SubtitleLineViewModel> _mergeSubtitles;
    [ObservableProperty] private MergeDisplayItem? _selectedMergeSubtitle;
    [ObservableProperty] private bool _isOkEnabled;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }
    public List<SubtitleLineViewModel> ResultSubtitles { get; set; }
    public TableView SubtitleGrid { get; set; }

    private readonly System.Timers.Timer _timerUpdatePreview;
    private volatile bool _isClosing;
    private bool _dirty;
    private List<SubtitleLineViewModel> _subtitles;

    public MergeSameTextViewModel()
    {
        MergeItems = new ObservableCollection<MergeDisplayItem>();
        MergeSubtitles = new ObservableCollection<SubtitleLineViewModel>();
        ResultSubtitles = new List<SubtitleLineViewModel>();
        SubtitleGrid = new TableView();

        LoadSettings();

        _subtitles = new List<SubtitleLineViewModel>();
        _timerUpdatePreview = new System.Timers.Timer(250);
        _timerUpdatePreview.Elapsed += TimerUpdatePreviewElapsed;
    }

    private void TimerUpdatePreviewElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        if (_isClosing)
        {
            return;
        }

        _timerUpdatePreview.Stop();
        if (_dirty)
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                _dirty = false;
                UpdatePreview();
            });
        }

        // Guard the restart: OnClosingCleanup may have disposed the timer while this handler ran (#12739).
        if (!_isClosing)
        {
            _timerUpdatePreview.Start();
        }
    }

    public void OnClosingCleanup()
    {
        _isClosing = true;
        _timerUpdatePreview.StopAndDispose(TimerUpdatePreviewElapsed);
    }

    public void Initialize(List<SubtitleLineViewModel> subtitles)
    {
        _subtitles = subtitles;

        for (var i = 0; i < _subtitles.Count; i++)
        {
            _subtitles[i].Number = i + 1;
        }

        _dirty = true;
        _timerUpdatePreview.Start();
    }

    private void UpdatePreview()
    {
        MergeItems.Clear();
        MergeSubtitles.Clear();

        var mergedIndexes = new List<int>();
        var removed = new HashSet<int>();
        var maxMsBetween = MaxMillisecondsBetweenLines;
        var fixIncrementing = IncludeIncrementingLines;
        var numberOfMerges = 0;
        Paragraph? p = null;

        if (IncludeRollUpCaptions)
        {
            AddRollUpMerges(maxMsBetween, removed);
        }

        for (var i = 1; i < _subtitles.Count; i++)
        {
            if (removed.Contains(i - 1))
            {
                continue;
            }

            var s = _subtitles[i - 1];
            p = new Paragraph(s.Text, s.StartTime.TotalMilliseconds, s.EndTime.TotalMilliseconds)
            {
                Number = s.Number,
            };

            for (var j = i; j < _subtitles.Count; j++)
            {
                if (removed.Contains(j))
                {
                    continue;
                }

                var nextS = _subtitles[j];
                var next = new Paragraph(nextS.Text, nextS.StartTime.TotalMilliseconds, nextS.EndTime.TotalMilliseconds)
                {
                    Number = nextS.Number,
                };
                var incrementText = string.Empty;
                if ((MergeLinesSameTextUtils.QualifiesForMerge(p, next, maxMsBetween) || fixIncrementing && MergeLinesSameTextUtils.QualifiesForMergeIncrement(p, next, maxMsBetween, out incrementText)) && IsFixAllowed(p))
                {
                    p.Text = next.Text;
                    p.EndTime.TotalMilliseconds = next.EndTime.TotalMilliseconds;
                    if (!string.IsNullOrEmpty(incrementText))
                    {
                        p.Text = incrementText;
                    }

                    removed.Add(j);
                    numberOfMerges++;
                    if (!mergedIndexes.Contains(j))
                    {
                        mergedIndexes.Add(j);
                    }

                    if (!mergedIndexes.Contains(i - 1))
                    {
                        mergedIndexes.Add(i - 1);
                    }
                }
                else
                {
                    break;
                }
            }

            if (mergedIndexes.Count > 0)
            {
                var group = (MergeItems.Count + 1).ToString();

                var mergeSubtitles = new List<SubtitleLineViewModel>();
                foreach (var idx in mergedIndexes.OrderBy(p => p))
                {
                    mergeSubtitles.Add(_subtitles[idx]);

                    var s2 = new SubtitleLineViewModel(_subtitles[idx]) { Extra = group };
                    MergeSubtitles.Add(s2);
                }

                var mergeDisplayItem = new MergeDisplayItem(true, mergeSubtitles, p.Text, group);
                MergeItems.Add(mergeDisplayItem);

                mergedIndexes.Clear();
            }
        }

        IsOkEnabled = MergeItems.Count > 0;
    }

    /// <summary>
    /// Roll-up (scrolling) caption chains are claimed first, so the plain same-text /
    /// incrementing loop never sees their members (they are added to <paramref name="removed"/>).
    /// </summary>
    private void AddRollUpMerges(int maxMsBetween, HashSet<int> removed)
    {
        var paragraphs = _subtitles
            .Select(s => new Paragraph(s.Text, s.StartTime.TotalMilliseconds, s.EndTime.TotalMilliseconds) { Number = s.Number })
            .ToList();
        var maxLines = Math.Max(1, Configuration.Settings.General.MaxNumberOfLines);
        var i = 0;
        while (i < paragraphs.Count)
        {
            if (!MergeLinesSameTextUtils.TryGetRollUpChain(paragraphs, i, maxMsBetween, maxLines, out var endIndex, out var merged))
            {
                i++;
                continue;
            }

            var group = (MergeItems.Count + 1).ToString();
            var linesToMerge = new List<SubtitleLineViewModel>();
            for (var idx = i; idx <= endIndex; idx++)
            {
                linesToMerge.Add(_subtitles[idx]);
                MergeSubtitles.Add(new SubtitleLineViewModel(_subtitles[idx]) { Extra = group });
                removed.Add(idx);
            }

            var mergedText = string.Join(" | ", merged.Select(m => m.Text.Replace(Environment.NewLine, " / ")));
            MergeItems.Add(new MergeDisplayItem(true, linesToMerge, mergedText, group) { ResultParagraphs = merged });
            i = endIndex + 1;
        }
    }

    private bool IsFixAllowed(Paragraph p)
    {
        foreach (var mi in MergeItems.Where(p => !p.Apply))
        {
            foreach (var line in mi.LinesToMerge)
            {
                if (line.Number == p.Number)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private void LoadSettings()
    {
        MaxMillisecondsBetweenLines = Se.Settings.Tools.MergeSameText.MaxMillisecondsBetweenLines;
        IncludeIncrementingLines = Se.Settings.Tools.MergeSameText.IncludeIncrementingLines;
        IncludeRollUpCaptions = Se.Settings.Tools.MergeSameText.IncludeRollUpCaptions;
    }

    private void SaveSettings()
    {
        Se.Settings.Tools.MergeSameText.MaxMillisecondsBetweenLines = MaxMillisecondsBetweenLines;
        Se.Settings.Tools.MergeSameText.IncludeIncrementingLines = IncludeIncrementingLines;
        Se.Settings.Tools.MergeSameText.IncludeRollUpCaptions = IncludeRollUpCaptions;

        Se.SaveSettings();
    }

    private List<SubtitleLineViewModel> BuildResultSubtitles()
    {
        var result = new List<SubtitleLineViewModel>();
        var skipCount = 0;

        foreach (var s in _subtitles)
        {
            if (skipCount > 0)
            {
                skipCount--;
                continue;
            }

            var match = MergeItems.FirstOrDefault(p => p.Apply && p.LinesToMerge.Contains(s));
            if (match != null && match.ResultParagraphs != null)
            {
                for (var k = 0; k < match.ResultParagraphs.Count; k++)
                {
                    var rp = match.ResultParagraphs[k];
                    var merged = new SubtitleLineViewModel(s, generateNewId: k > 0);
                    merged.Text = rp.Text;
                    merged.StartTime = TimeSpan.FromMilliseconds(rp.StartTime.TotalMilliseconds);
                    merged.EndTime = TimeSpan.FromMilliseconds(rp.EndTime.TotalMilliseconds);
                    result.Add(merged);
                }

                skipCount += match.LinesToMerge.Count - 1;
                continue;
            }

            if (match != null)
            {
                var merged = new SubtitleLineViewModel(s);
                merged.EndTime = match.LinesToMerge.Max(p => p.EndTime);
                merged.Text = match.MergedText;
                result.Add(merged);

                skipCount += match.LinesToMerge.Count - 1;
                continue;
            }

            var copy = new SubtitleLineViewModel(s);
            result.Add(copy);
        }

        return result;
    }

    [RelayCommand]
    private void Ok()
    {
        SaveSettings();
        ResultSubtitles = BuildResultSubtitles();
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    internal void MergeItemChanged(object? sender, SelectionChangedEventArgs e)
    {
        var selected = SelectedMergeItem;
        var selectedItems = SubtitleGrid.SelectedItems;
        if (selected == null || selectedItems == null)
        {
            return;
        }

        selectedItems.Clear();
        foreach (var item in MergeSubtitles)
        {
            if (item is SubtitleLineViewModel svm && svm.Extra == selected.MergedGroup)
            {
                selectedItems.Add(item);
            }
        }
    }

    public void SetDirty()
    {
        _dirty = true;
    }

    internal void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/merge-same-text");
        }
    }
}