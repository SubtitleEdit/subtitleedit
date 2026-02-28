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

namespace Nikse.SubtitleEdit.Features.Tools.MergeSubtitlesWithSameTimeCodes;

public partial class MergeSameTimeCodesViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<MergeDisplayItem> _mergeItems;
    [ObservableProperty] private MergeDisplayItem? _selectedMergeItem;
    [ObservableProperty] private int _maxMillisecondsDifference;
    [ObservableProperty] private bool _mergeDialog;
    [ObservableProperty] private bool _autoBreak;
    [ObservableProperty] private ObservableCollection<SubtitleLineViewModel> _mergeSubtitles;
    [ObservableProperty] private MergeDisplayItem? _selectedMergeSubtitle;
    [ObservableProperty] private bool _isOkEnabled;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }
    public List<SubtitleLineViewModel> ResultSubtitles { get; set; }
    public DataGrid SubtitleGrid { get; set; }

    private readonly System.Timers.Timer _timerUpdatePreview;
    private bool _dirty;
    private string _language;
    private List<SubtitleLineViewModel> _subtitles;

    public MergeSameTimeCodesViewModel()
    {
        MergeItems = new ObservableCollection<MergeDisplayItem>();
        MergeSubtitles = new ObservableCollection<SubtitleLineViewModel>();
        ResultSubtitles = new List<SubtitleLineViewModel>();
        SubtitleGrid = new DataGrid();

        LoadSettings();

        _language = "en";
        _subtitles = new List<SubtitleLineViewModel>();
        _timerUpdatePreview = new System.Timers.Timer(500);
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

    public void Initialize(List<SubtitleLineViewModel> subtitles, Subtitle subtitle)
    {
        _subtitles = subtitles;
        _language = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle);
        _dirty = true;
        _timerUpdatePreview.Start();
    }

    private void UpdatePreview()
    {
        MergeItems.Clear();
        MergeSubtitles.Clear();

        var mergedIndexes = new List<int>();
        var removed = new HashSet<int>();
        var makeDialog = MergeDialog;
        var reBreak = AutoBreak;
        var numberOfMerges = 0;
        SubtitleLineViewModel? p = null;
        MergeSubtitles.Clear();
        var singleMergeSubtitles = new List<SubtitleLineViewModel>();
        var mergedText = string.Empty;
        for (var i = 1; i < _subtitles.Count; i++)
        {
            p = _subtitles[i - 1];

            var next = _subtitles[i];
            if (QualifiesForMerge(p, next, MaxMillisecondsDifference) && IsFixAllowed(p))
            {
                if (!singleMergeSubtitles.Contains(p))
                {
                    singleMergeSubtitles.Add(p);
                }

                if (!singleMergeSubtitles.Contains(next))
                {
                    singleMergeSubtitles.Add(next);
                }

                var nextText = next.Text
                    .Replace("{\\an1}", string.Empty)
                    .Replace("{\\an2}", string.Empty)
                    .Replace("{\\an3}", string.Empty)
                    .Replace("{\\an4}", string.Empty)
                    .Replace("{\\an5}", string.Empty)
                    .Replace("{\\an6}", string.Empty)
                    .Replace("{\\an7}", string.Empty)
                    .Replace("{\\an8}", string.Empty)
                    .Replace("{\\an9}", string.Empty);

                if (singleMergeSubtitles.Count == 2)
                {
                    mergedText = p.Text;
                }
                if (mergedText.StartsWith("<i>", StringComparison.Ordinal) && mergedText.EndsWith("</i>", StringComparison.Ordinal) && nextText.StartsWith("<i>", StringComparison.Ordinal) && nextText.EndsWith("</i>", StringComparison.Ordinal))
                {
                    mergedText = GetMergedLines(mergedText.Remove(mergedText.Length - 4), nextText.Remove(0, 3), makeDialog);
                }
                else
                {
                    mergedText = GetMergedLines(mergedText, nextText, makeDialog);
                }

                if (reBreak)
                {
                    mergedText = Utilities.AutoBreakLine(mergedText, _language);
                }

                removed.Add(i);
                numberOfMerges++;
                if (!mergedIndexes.Contains(i))
                {
                    mergedIndexes.Add(i);
                }

                if (!mergedIndexes.Contains(i - 1))
                {
                    mergedIndexes.Add(i - 1);
                }
            }
            else
            {
                if (singleMergeSubtitles.Count > 0)
                {
                    var group = (MergeItems.Count + 1).ToString();
                    foreach (var svm in singleMergeSubtitles)
                    {
                        var preview = new SubtitleLineViewModel(svm)
                        {
                            Extra = group
                        };
                        MergeSubtitles.Add(preview);
                    }
                    MergeItems.Add(new MergeDisplayItem(true, singleMergeSubtitles, mergedText, group));
                    singleMergeSubtitles.Clear();
                    mergedText = string.Empty;
                }
            }
        }

        if (singleMergeSubtitles.Count > 0)
        {
            var group = (MergeItems.Count + 1).ToString();
            foreach (var svm in singleMergeSubtitles)
            {
                var preview = new SubtitleLineViewModel(svm)
                {
                    Extra = group
                };
                MergeSubtitles.Add(preview);
            }
            MergeItems.Add(new MergeDisplayItem(true, singleMergeSubtitles, mergedText, group));
        }

        IsOkEnabled = MergeItems.Count > 0;
    }

    public static string GetMergedLines(string line1, string line2, bool makeDialog)
    {
        if (makeDialog)
        {
            switch (Configuration.Settings.General.DialogStyle)
            {
                case Core.Enums.DialogType.DashBothLinesWithoutSpace:
                    return (line1.StartsWith("-") ? "" : "-") + line1 + Environment.NewLine + "-" + line2;
                case Core.Enums.DialogType.DashSecondLineWithSpace:
                    return line1 + Environment.NewLine + "- " + line2;
                case Core.Enums.DialogType.DashSecondLineWithoutSpace:
                    return line1 + Environment.NewLine + "-" + line2;
                default:
                    return (line1.StartsWith("- ") ? "" : "- ") + line1 + Environment.NewLine + "- " + line2;
            }
        }
        else
        {
            return line1 + Environment.NewLine + line2;
        }
    }

    public static bool QualifiesForMerge(SubtitleLineViewModel p, SubtitleLineViewModel next, int maxMsBetween)
    {
        if (p == null || next == null)
        {
            return false;
        }

        return Math.Abs(next.StartTime.TotalMilliseconds - p.StartTime.TotalMilliseconds) <= maxMsBetween &&
               Math.Abs(next.EndTime.TotalMilliseconds - p.EndTime.TotalMilliseconds) <= maxMsBetween;
    }

    private bool IsFixAllowed(SubtitleLineViewModel p)
    {
        foreach (var mi in MergeItems.Where(p => !p.Apply))
        {
            foreach (var line in mi.LinesToMerge)
            {
                if (line.Id == p.Id)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private void LoadSettings()
    {
        MaxMillisecondsDifference = Se.Settings.Tools.MergeSameTimeCode.MaxMillisecondsDifference;
        MergeDialog = Se.Settings.Tools.MergeSameTimeCode.MergeDialog;
        AutoBreak = Se.Settings.Tools.MergeSameTimeCode.AutoBreak;
    }

    private void SaveSettings()
    {
        Se.Settings.Tools.MergeSameTimeCode.MaxMillisecondsDifference = MaxMillisecondsDifference;
        Se.Settings.Tools.MergeSameTimeCode.MergeDialog = MergeDialog;
        Se.Settings.Tools.MergeSameTimeCode.AutoBreak = AutoBreak;

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

            var match = MergeItems.FirstOrDefault(p => p.Apply && p.LinesToMerge.Any(p => p.Id == s.Id));
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
            UiUtil.ShowHelp("features/merge-same-timecodes");
        }
    }
}