using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Tools.ApplyMinGap;

public partial class ApplyMinGapViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<ApplyMinGapItem> _subtitles;
    [ObservableProperty] private ApplyMinGapItem? _selectedSubtitle;
    [ObservableProperty] private string _minXBetweenLines;
    [ObservableProperty] private int _minGapMsOrFrames;
    [ObservableProperty] private string _statusText;
    
    public List<SubtitleLineViewModel> FixedSubtitles{ get; set; }
    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    private readonly System.Timers.Timer _timerUpdatePreview;
    private bool _dirty;
    private readonly List<SubtitleLineViewModel> _allSubtitles;

    public ApplyMinGapViewModel()
    {
        Subtitles = new ObservableCollection<ApplyMinGapItem>();
        FixedSubtitles = new List<SubtitleLineViewModel>();
        MinGapMsOrFrames = 10;
        StatusText = string.Empty;

        if (Se.Settings.General.UseFrameMode)
        {
            MinXBetweenLines = Se.Language.Tools.ApplyMinGaps.MinFramesBetweenLines;
        }
        else
        {
            MinXBetweenLines = Se.Language.Tools.ApplyMinGaps.MinMsBetweenLines;
        }

        LoadSettings();

        _allSubtitles = new List<SubtitleLineViewModel>();  
        _timerUpdatePreview = new System.Timers.Timer(500);
        _timerUpdatePreview.Elapsed += (s, e) =>
        {
            _timerUpdatePreview.Stop();
            if (_dirty)
            {
                _dirty = false;
                UpdatePreview();
            }
            _timerUpdatePreview.Start();
        };
    }

    private void UpdatePreview()
    {
        var minMsBetweenLines = MinGapMsOrFrames;
        if (Configuration.Settings.General.UseTimeFormatHHMMSSFF)
        {
            minMsBetweenLines = SubtitleFormat.FramesToMilliseconds(minMsBetweenLines);
        }

        FixedSubtitles.Clear();
        FixedSubtitles = new List<SubtitleLineViewModel>(_allSubtitles.Select(p => new SubtitleLineViewModel(p)));

        Dispatcher.UIThread.Post(() =>
        {
            Subtitles.Clear();
            var fixedCount = 0;
            for (var index = 0; index < FixedSubtitles.Count-1; index++)
            {
                var current = FixedSubtitles[index];
                var next = FixedSubtitles[index + 1];
                var gapMs = next.StartTime.TotalMilliseconds - current.EndTime.TotalMilliseconds;
                if (gapMs < minMsBetweenLines)
                {
                    fixedCount++;
                    
                    var before = new TimeCode(gapMs).ToShortDisplayString();
                    
                    var newEndMs = next.StartTime.TotalMilliseconds  - minMsBetweenLines;
                    current.EndTime = TimeSpan.FromMilliseconds(newEndMs);
                    var newGapMs = next.StartTime.TotalMilliseconds - current.EndTime.TotalMilliseconds;

                    var after = new TimeCode(newGapMs).ToShortDisplayString();
                    var fixFormat = Se.Language.Tools.ApplyMinGaps.ChangedGapFromXToYCommentZ;
                    var comment = string.Empty;
                    var info = string.Format(fixFormat, before, after, comment);

                    var vm = new ApplyMinGapItem(current);
                    vm.InfoText = info; 
                    Subtitles.Add(vm);
                }
            }

            StatusText = string.Format(Se.Language.Tools.ApplyMinGaps.NumberOfGapsFixedX, fixedCount);
        });
    }

    public void Initialize(List<SubtitleLineViewModel> subtitles)
    {
        _allSubtitles.Clear();
        _allSubtitles.AddRange(subtitles.Select(p => new SubtitleLineViewModel(p)));
        _dirty = true;
        _timerUpdatePreview.Start();
    }

    private void LoadSettings()
    {
        MinGapMsOrFrames = Se.Settings.General.UseFrameMode 
            ? SubtitleFormat.MillisecondsToFrames(Se.Settings.General.MinimumMillisecondsBetweenLines) 
            : Se.Settings.General.MinimumMillisecondsBetweenLines;
    }

    private void SaveSettings()
    {
        Se.SaveSettings();
    }

    [RelayCommand]
    private void Ok()
    {
        SaveSettings();
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/apply-min-gap");
        }
    }

    internal void ValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        _dirty = true;
    }
}