using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Features.Tools.MergeShortLines;

public partial class MergeShortLinesViewModel : ObservableObject
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
    private bool _isDirty;
    private List<double> _shotChanges;

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

            var gapThresholdMs = Se.Settings.Tools.BridgeGaps.BridgeGapsSmallerThanMs;
            var unbreakLinesShorterThan = Se.Settings.General.UnbreakLinesShorterThan;

            var mergeResult = new MergeShortLinesResult(new List<SubtitleLineViewModel>(), new List<MergeShortLinesItem>(), 0);
            if (HighLight)
            {
                mergeResult = MergeShortLinesHelper.MergeWithHighlights(
                    _allSubtitles,
                    _shotChanges,
                    SingleLineMaxLength,
                    MaxNumberOfLines,
                    gapThresholdMs,
                    unbreakLinesShorterThan);
            }
            else
            {
                mergeResult = MergeShortLinesHelper.Merge(
                    _allSubtitles,
                    _shotChanges,
                    SingleLineMaxLength,
                    MaxNumberOfLines,
                    gapThresholdMs,
                    unbreakLinesShorterThan);
            }
            

            AllSubtitlesFixed.AddRange(mergeResult.MergedSubtitles);

            foreach (var fix in mergeResult.Fixes)
            {
                Fixes.Add(fix);
            }

            if (mergeResult.MergeCount == 0)
            {
                FixesInfo = Se.Language.Tools.ApplyDurationLimits.NoChangesNeeded;
            }
            else
            {
                FixesInfo = $"Lines merged: {mergeResult.MergeCount}";
            }
        });
    }

    private void LoadSettings()
    {
        SingleLineMaxLength = Se.Settings.General.SubtitleLineMaximumLength;
        MaxNumberOfLines = Se.Settings.General.MaxNumberOfLines;
    }

    private void SaveSettings()
    {
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