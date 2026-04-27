using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Tools.MergeSubtitlesWithSameText;

public partial class MergeSameTextViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<MergeDisplayItem> _mergeItems;
    [ObservableProperty] private MergeDisplayItem? _selectedMergeItem;
    [ObservableProperty] private int _maxMillisecondsBetweenLines;
    [ObservableProperty] private bool _includeIncrementingLines;
    [ObservableProperty] private ObservableCollection<SubtitleLineViewModel> _mergeSubtitles;
    [ObservableProperty] private MergeDisplayItem? _selectedMergeSubtitle;
    [ObservableProperty] private bool _isOkEnabled;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }
    public List<SubtitleLineViewModel> ResultSubtitles { get; set; }
    public DataGrid SubtitleGrid { get; set; }

    private readonly System.Timers.Timer _timerUpdatePreview;
    private bool _dirty;
    private List<SubtitleLineViewModel> _subtitles;

    public MergeSameTextViewModel()
    {
        MergeItems = new ObservableCollection<MergeDisplayItem>();
        MergeSubtitles = new ObservableCollection<SubtitleLineViewModel>();
        ResultSubtitles = new List<SubtitleLineViewModel>();
        SubtitleGrid = new DataGrid();

        LoadSettings();

        _subtitles = new List<SubtitleLineViewModel>();
        _timerUpdatePreview = new System.Timers.Timer(250);
        _timerUpdatePreview.Elapsed += (s, e) =>
        {
            _timerUpdatePreview.Stop();
            if (_dirty)
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    _dirty = false;
                    UpdatePreview();
                });
            }
            _timerUpdatePreview.Start();
        };
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
        var lineNumbers = new List<int>();
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

                    if (lineNumbers.Count > 0)
                    {
                        lineNumbers.Add(next.Number);
                    }
                    else
                    {
                        lineNumbers.Add(p.Number);
                        lineNumbers.Add(next.Number);
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
    }

    private void SaveSettings()
    {
        Se.Settings.Tools.MergeSameText.MaxMillisecondsBetweenLines = MaxMillisecondsBetweenLines;
        Se.Settings.Tools.MergeSameText.IncludeIncrementingLines = IncludeIncrementingLines;

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

    internal void DataGridMergeItemChanged(object? sender, SelectionChangedEventArgs e)
    {
        var selected = SelectedMergeItem;
        if (selected == null)
        {
            return;
        }

        SubtitleGrid.SelectedItems.Clear();
        foreach (var item in MergeSubtitles)
        {
            if (item is SubtitleLineViewModel svm && svm.Extra == selected.MergedGroup)
            {
                SubtitleGrid.SelectedItems.Add(item);
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