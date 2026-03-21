using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Tools.ApplyDurationLimits;

public partial class ApplyDurationLimitsViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<ApplyDurationLimitItem> _fixes;
    [ObservableProperty] private ApplyDurationLimitItem? _selectedFix;

    [ObservableProperty] private ObservableCollection<SubtitleLineViewModel> _subtitles;
    [ObservableProperty] private SubtitleLineViewModel? _selectedSubtitle;

    [ObservableProperty] private int? _minDurationMs;
    [ObservableProperty] private bool _fixMinDurationMs;

    [ObservableProperty] private int? _maxDurationMs;
    [ObservableProperty] private bool _fixMaxDurationMs;

    [ObservableProperty] private bool _doNotGoPastShotChange;
    [ObservableProperty] private bool _isDoNotGoPastShotChangeVisible;

    [ObservableProperty] private string _fixesInfo;
    [ObservableProperty] private string _fixesSkippedInfo;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }
    public List<SubtitleLineViewModel> AllSubtitlesFixed { get; set; }

    private List<SubtitleLineViewModel> _allSubtitles;

    private readonly System.Timers.Timer _previewTimer;
    private bool _isDirty;
    private List<double> _shotChanges;

    public ApplyDurationLimitsViewModel()
    {
        Fixes = new ObservableCollection<ApplyDurationLimitItem>();
        Subtitles = new ObservableCollection<SubtitleLineViewModel>();
        _allSubtitles = new List<SubtitleLineViewModel>();
        _shotChanges = new List<double>();
        AllSubtitlesFixed = new List<SubtitleLineViewModel>();
        FixesInfo = string.Empty;
        FixesSkippedInfo = string.Empty;

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
        if (MinDurationMs == null || MaxDurationMs == null || _allSubtitles.Count == 0)
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            Subtitles.Clear();
            AllSubtitlesFixed.Clear();
            Fixes.Clear();
            if (MinDurationMs >= MaxDurationMs)
            {
                return;
            }

            var minMs = MinDurationMs.Value;
            var maxMs = MaxDurationMs.Value;
            var fixCount = 0;
            var improveCount = 0;
            var skipCount = 0;

            for (var index = 0; index < _allSubtitles.Count; index++)
            {
                var item = new SubtitleLineViewModel(_allSubtitles[index]);
                AllSubtitlesFixed.Add(item);

                var next = _allSubtitles.GetOrNull(index + 1);

                if (next == null)
                {
                    if (item.Duration.TotalMilliseconds > maxMs && FixMaxDurationMs)
                    {
                        var newEndTime = TimeSpan.FromMilliseconds(item.StartTime.TotalMilliseconds + maxMs);
                        Update(item, newEndTime);
                        fixCount++;
                    }

                    if (item.Duration.TotalMilliseconds < minMs && FixMinDurationMs)
                    {
                        var newEndTime = TimeSpan.FromMilliseconds(item.EndTime.TotalMilliseconds + minMs);
                        Update(item, newEndTime);
                        fixCount++;
                    }
                }
                else
                {
                    if (item.Duration.TotalMilliseconds > maxMs && FixMaxDurationMs)
                    {
                        var newEndTime = TimeSpan.FromMilliseconds(item.StartTime.TotalMilliseconds + maxMs);
                        Update(item, newEndTime);
                        fixCount++;
                    }

                    if (item.Duration.TotalMilliseconds < minMs && FixMinDurationMs)
                    {
                        var newEndTime = TimeSpan.FromMilliseconds(item.StartTime.TotalMilliseconds + minMs);
                        if (newEndTime > next.StartTime)
                        {
                            var cappedEndTime = TimeSpan.FromMilliseconds(next.StartTime.TotalMilliseconds - Se.Settings.General.MinimumMillisecondsBetweenLines);
                            if (cappedEndTime > item.EndTime)
                            {
                                // improved, but not fixed
                                Update(item, cappedEndTime,  Se.Language.Tools.ApplyDurationLimits.OnlyPartialFixed);
                                improveCount++;
                            }
                            else
                            {
                                // unfixable
                                Subtitles.Add(item);
                                skipCount++;
                            }
                        }
                        else
                        {
                            Update(item, newEndTime);
                            fixCount++;
                        }
                    }
                }
            }

            if (fixCount == 0 && improveCount == 0 && skipCount == 0)
            {
                FixesInfo = Se.Language.Tools.ApplyDurationLimits.NoChangesNeeded;
                FixesSkippedInfo = string.Empty;
                return;
            }

            if (improveCount == 0)
            {
                FixesInfo = string.Format(Se.Language.Tools.ApplyDurationLimits.FixedX, fixCount);
            }
            else
            {
                FixesInfo = string.Format(Se.Language.Tools.ApplyDurationLimits.FixedXImprovedY, fixCount, improveCount);
            }


            FixesSkippedInfo = skipCount > 0 ? string.Format(Se.Language.Tools.ApplyDurationLimits.UnfixableX, skipCount) : string.Empty;
        });
    }

    private void Update(SubtitleLineViewModel item, TimeSpan newEndTime, string? comment = null)
    {
        var before = new TimeCode(item.Duration).ToShortDisplayString();
        item.EndTime = newEndTime;
        var after = new TimeCode(item.Duration).ToShortDisplayString();

        var fixFormat = Se.Language.Tools.ApplyDurationLimits.ChangedDurationFromXToYCommentZ;
        var fix = string.Format(fixFormat, before, after, comment);

        Fixes.Add(new ApplyDurationLimitItem(true, item.Text, item.Number, fix, item));
    }

    private void LoadSettings()
    {
        FixMinDurationMs = true;
        MinDurationMs = Se.Settings.General.SubtitleMinimumDisplayMilliseconds;

        FixMaxDurationMs = true;
        MaxDurationMs = Se.Settings.General.SubtitleMaximumDisplayMilliseconds;

        DoNotGoPastShotChange = Se.Settings.Tools.ApplyDurationLimits.DoNotExtendPastShotChange;
    }

    private void SaveSettings()
    {
        Se.Settings.Tools.ApplyDurationLimits.DoNotExtendPastShotChange = DoNotGoPastShotChange;
        Se.SaveSettings();
    }

    [RelayCommand]
    private async Task Ok()
    {
        if (Window == null)
        {
            return;
        }

        SaveSettings();
        
        if (FixMinDurationMs && FixMaxDurationMs && MinDurationMs > MaxDurationMs)
        {
            var msg = Se.Language.Tools.ApplyDurationLimits.MaxDurationShouldBeHigherThanMinDuration;
            await MessageBox.Show(Window, Se.Language.General.Error, msg, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (FixMinDurationMs || FixMaxDurationMs)
        {
            OkPressed = true;
        }

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
        _shotChanges =  shotChanges;
        IsDoNotGoPastShotChangeVisible = shotChanges.Count > 0;
        IsDoNotGoPastShotChangeVisible = false; //TODO: not implemented
        _previewTimer.Start();
        _isDirty = true;
    }

    internal void SetChanged()
    {
        _isDirty = true;
    }
}