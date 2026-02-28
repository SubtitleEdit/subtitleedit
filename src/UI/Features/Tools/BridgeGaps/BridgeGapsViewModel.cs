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

namespace Nikse.SubtitleEdit.Features.Tools.BridgeGaps;

public partial class BridgeGapsViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<BridgeGapDisplayItem> _subtitles;
    [ObservableProperty] private BridgeGapDisplayItem? _selectedSubtitle;
    [ObservableProperty] private int _bridgeGapsSmallerThanMs;
    [ObservableProperty] private int _minGapMs;
    [ObservableProperty] private int _percentForLeft;
    [ObservableProperty] private string _statusText;
    [ObservableProperty] private ObservableCollection<SubtitleLineViewModel> _allSubtitles;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    private readonly System.Timers.Timer _timerUpdatePreview;
    private bool _dirty;
    private Dictionary<string, string> _dic;


    public BridgeGapsViewModel()
    {
        Subtitles = new ObservableCollection<BridgeGapDisplayItem>();
        AllSubtitles = new ObservableCollection<SubtitleLineViewModel>();
        BridgeGapsSmallerThanMs = 500;
        MinGapMs = 10;
        StatusText = string.Empty;
        _dic = new Dictionary<string, string>();

        LoadSettings();

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
        _dic = new Dictionary<string, string>();
        var fixedIndexes = new List<int>(Subtitles.Count);
        var minMsBetweenLines = MinGapMs;
        var maxMs = BridgeGapsSmallerThanMs;
        if (Configuration.Settings.General.UseTimeFormatHHMMSSFF)
        {
            minMsBetweenLines = SubtitleFormat.FramesToMilliseconds(minMsBetweenLines);
            maxMs = SubtitleFormat.FramesToMilliseconds(maxMs);
        }

        var allSubtitles = new ObservableCollection<SubtitleLineViewModel>(AllSubtitles.Select(p => new SubtitleLineViewModel(p)));
        var fixedCount = DurationsBridgeGaps2.BridgeGaps(allSubtitles, minMsBetweenLines, PercentForLeft, maxMs, fixedIndexes, _dic, Configuration.Settings.General.UseTimeFormatHHMMSSFF);

        Dispatcher.UIThread.Post(() =>
        {
            Subtitles.Clear();
            foreach (var v in allSubtitles)
            {
                var vm = new BridgeGapDisplayItem(v);
                Subtitles.Add(vm);
            }

            for (var i = 0; i < Subtitles.Count - 1; i++)
            {
                var cur = Subtitles[i];
                if (_dic.ContainsKey(cur.SubtitleLineViewModel.Id.ToString()))
                {
                    cur.InfoText = _dic[cur.SubtitleLineViewModel.Id.ToString()];
                }
                else
                {
                    var info = string.Empty;
                    var next = Subtitles[i + 1];
                    if (next != null)
                    {
                        var gap = next.StartTime.TotalMilliseconds - cur.EndTime.TotalMilliseconds;
                        info = $"{gap / TimeCode.BaseUnit:0.000}";
                        if (Configuration.Settings.General.UseTimeFormatHHMMSSFF)
                        {
                            info = $"{SubtitleFormat.MillisecondsToFrames(gap)}";
                        }
                    }
                }

            }

            StatusText = string.Format(Se.Language.Tools.BridgeGaps.NumberOfSmallGapsBridgedX, fixedCount);
        });
    }

    public void Initialize(List<SubtitleLineViewModel> subtitles)
    {
        AllSubtitles.Clear();
        AllSubtitles.AddRange(subtitles.Select(p => new SubtitleLineViewModel(p)));
        _dirty = true;
        _timerUpdatePreview.Start();
    }

    private void LoadSettings()
    {
        BridgeGapsSmallerThanMs = Se.Settings.Tools.BridgeGaps.BridgeGapsSmallerThanMs;
        MinGapMs = Se.Settings.Tools.BridgeGaps.MinGapMs;
        PercentForLeft = Se.Settings.Tools.BridgeGaps.PercentForLeft;
    }

    private void SaveSettings()
    {
        Se.Settings.Tools.BridgeGaps.BridgeGapsSmallerThanMs = BridgeGapsSmallerThanMs;
        Se.Settings.Tools.BridgeGaps.MinGapMs = MinGapMs;
        Se.Settings.Tools.BridgeGaps.PercentForLeft = PercentForLeft;

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
            UiUtil.ShowHelp("features/bridge-gaps");
        }
    }

    internal void ValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        _dirty = true;
    }
}