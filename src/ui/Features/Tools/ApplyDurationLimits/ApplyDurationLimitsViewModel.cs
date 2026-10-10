using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Tools.ApplyDurationLimits;

public partial class ApplyDurationLimitsViewModel : ObservableObject, IClosingCleanup
{
    [ObservableProperty] private ObservableCollection<ApplyDurationLimitItem> _fixes;
    [ObservableProperty] private ApplyDurationLimitItem? _selectedFix;

    [ObservableProperty] private ObservableCollection<SubtitleLineViewModel> _subtitles;
    [ObservableProperty] private SubtitleLineViewModel? _selectedSubtitle;

    [ObservableProperty] private int? _minDurationMsOrFrames;
    [ObservableProperty] private bool _fixMinDurationMs;

    [ObservableProperty] private int? _maxDurationMsOrFrames;
    [ObservableProperty] private bool _fixMaxDurationMs;

    [ObservableProperty] private bool _doNotGoPastShotChange;
    [ObservableProperty] private bool _isDoNotGoPastShotChangeVisible;

    [ObservableProperty] private string _fixesInfo;
    [ObservableProperty] private string _fixesSkippedInfo;

    /// <summary>Frame mode: both boxes hold frames, like Bridge gaps and Apply min gap.</summary>
    public bool IsFrameMode { get; }

    public string FixMinDurationLabel { get; }
    public string FixMaxDurationLabel { get; }

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }
    public List<SubtitleLineViewModel> AllSubtitlesFixed { get; set; }

    private List<SubtitleLineViewModel> _allSubtitles;
    private ISet<Guid>? _onlyIds;

    private readonly System.Timers.Timer _previewTimer;
    private volatile bool _isClosing;
    private bool _isDirty;
    private List<double> _shotChanges;

    // The saved milliseconds the boxes were filled from - kept on save when the frame count is
    // unchanged, so a run in frame mode does not round the setting to whole frames.
    private int _loadedMinDurationMs;
    private int _loadedMaxDurationMs;

    public ApplyDurationLimitsViewModel()
    {
        IsFrameMode = Se.Settings.General.UseFrameMode;
        FixMinDurationLabel = IsFrameMode
            ? Se.Language.Tools.ApplyDurationLimits.FixMinDurationFrames
            : Se.Language.Tools.ApplyDurationLimits.FixMinDurationMs;
        FixMaxDurationLabel = IsFrameMode
            ? Se.Language.Tools.ApplyDurationLimits.FixMaxDurationFrames
            : Se.Language.Tools.ApplyDurationLimits.FixMaxDurationMs;
        Fixes = new ObservableCollection<ApplyDurationLimitItem>();
        Subtitles = new ObservableCollection<SubtitleLineViewModel>();
        _allSubtitles = new List<SubtitleLineViewModel>();
        _shotChanges = new List<double>();
        AllSubtitlesFixed = new List<SubtitleLineViewModel>();
        FixesInfo = string.Empty;
        FixesSkippedInfo = string.Empty;

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
            // OK may have built the result and closed the window before this queued tick ran.
            if (!_isClosing)
            {
                BuildPreview();
            }
        });
    }

    /// <summary>
    /// Rebuilds <see cref="AllSubtitlesFixed"/>, <see cref="Fixes"/> and the preview rows from the
    /// current settings. Must run on the UI thread; kept separate from <see cref="UpdatePreview"/>
    /// so <see cref="Ok"/> can build the result it hands to the caller rather than depending on the
    /// preview timer having ticked.
    /// </summary>
    private void BuildPreview()
    {
        if (MinDurationMsOrFrames == null || MaxDurationMsOrFrames == null || _allSubtitles.Count == 0)
        {
            return;
        }

        Subtitles.Clear();
        AllSubtitlesFixed.Clear();
        Fixes.Clear();

        // Only a conflict when both limits are actually applied - bailing out whenever the
        // numbers cross meant "shorten to 500 ms" with a stale 1000 ms minimum in the other
        // box silently did nothing at all.
        if (FixMinDurationMs && FixMaxDurationMs && MinDurationMsOrFrames >= MaxDurationMsOrFrames)
        {
            return;
        }

        var minMs = MsOrFrames.ToMilliseconds(MinDurationMsOrFrames.Value, IsFrameMode);
        var maxMs = MsOrFrames.ToMilliseconds(MaxDurationMsOrFrames.Value, IsFrameMode);
        var fixCount = 0;
        var improveCount = 0;
        var skipCount = 0;

        for (var index = 0; index < _allSubtitles.Count; index++)
        {
            var item = new SubtitleLineViewModel(_allSubtitles[index]);
            AllSubtitlesFixed.Add(item);
            if (_onlyIds != null && !_onlyIds.Contains(item.Id))
            {
                continue;
            }

            var next = _allSubtitles.GetOrNull(index + 1);

            // Compared in frames in frame mode, so a line exactly N frames long is not flagged
            // because its rounded milliseconds are one off the rounded limit.
            if (FixMaxDurationMs && MsOrFrames.IsAbove(item.Duration.TotalMilliseconds, MaxDurationMsOrFrames.Value, IsFrameMode))
            {
                // Shortening never runs into the next line or a shot change.
                var newEndTime = TimeSpan.FromMilliseconds(item.StartTime.TotalMilliseconds + maxMs);
                Update(item, newEndTime);
                fixCount++;
            }

            if (FixMinDurationMs && MsOrFrames.IsBelow(item.Duration.TotalMilliseconds, MinDurationMsOrFrames.Value, IsFrameMode))
            {
                var wantedEndTime = TimeSpan.FromMilliseconds(item.StartTime.TotalMilliseconds + minMs);
                var allowedEndTime = wantedEndTime;

                // Never overlap the next line.
                if (next != null && wantedEndTime > next.StartTime)
                {
                    allowedEndTime = TimeSpan.FromMilliseconds(next.StartTime.TotalMilliseconds - Se.Settings.General.MinimumBetweenLines.GetMilliseconds());
                }

                allowedEndTime = CapAtShotChange(item, allowedEndTime);

                if (allowedEndTime >= wantedEndTime)
                {
                    Update(item, wantedEndTime);
                    fixCount++;
                }
                else if (allowedEndTime > item.EndTime)
                {
                    // improved, but not fixed
                    Update(item, allowedEndTime, Se.Language.Tools.ApplyDurationLimits.OnlyPartialFixed);
                    improveCount++;
                }
                else
                {
                    // unfixable
                    Subtitles.Add(item);
                    skipCount++;
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
    }

    /// <summary>
    /// Caps an extended end time at the first shot change that falls inside the extension, so a line
    /// is never stretched across a cut. Returns <paramref name="newEndTime"/> unchanged when the
    /// option is off, when there are no shot changes, or when nothing is in the way.
    /// </summary>
    private TimeSpan CapAtShotChange(SubtitleLineViewModel item, TimeSpan newEndTime)
    {
        if (!DoNotGoPastShotChange || _shotChanges.Count == 0)
        {
            return newEndTime;
        }

        var currentEndMs = item.EndTime.TotalMilliseconds;
        var newEndMs = newEndTime.TotalMilliseconds;
        if (newEndMs <= currentEndMs)
        {
            return newEndTime;
        }

        // _shotChanges is sorted in Initialize, so the first hit is the earliest one.
        foreach (var shotChangeSeconds in _shotChanges)
        {
            var shotChangeMs = shotChangeSeconds * 1000.0;
            if (shotChangeMs >= newEndMs)
            {
                break;
            }

            if (shotChangeMs > currentEndMs)
            {
                return TimeSpan.FromMilliseconds(shotChangeMs);
            }
        }

        return newEndTime;
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
        // 0 means "not saved yet" - fall back to the general defaults (#13514 pattern). Saving the
        // dialog's own copy keeps a one-off run from rewriting the app-wide duration settings.
        // Stored in milliseconds; in frame mode the boxes show frames at the current frame rate.
        FixMinDurationMs = true;
        _loadedMinDurationMs = Se.Settings.Tools.ApplyDurationLimitsMinDurationMs > 0
            ? Se.Settings.Tools.ApplyDurationLimitsMinDurationMs
            : Se.Settings.General.SubtitleMinimumDisplayMilliseconds;
        MinDurationMsOrFrames = MsOrFrames.FromMilliseconds(_loadedMinDurationMs, IsFrameMode);

        FixMaxDurationMs = true;
        _loadedMaxDurationMs = Se.Settings.Tools.ApplyDurationLimitsMaxDurationMs > 0
            ? Se.Settings.Tools.ApplyDurationLimitsMaxDurationMs
            : Se.Settings.General.SubtitleMaximumDisplayMilliseconds;
        MaxDurationMsOrFrames = MsOrFrames.FromMilliseconds(_loadedMaxDurationMs, IsFrameMode);

        DoNotGoPastShotChange = Se.Settings.Tools.ApplyDurationLimits.DoNotExtendPastShotChange;
    }

    private void SaveSettings()
    {
        Se.Settings.Tools.ApplyDurationLimits.DoNotExtendPastShotChange = DoNotGoPastShotChange;
        Se.Settings.Tools.ApplyDurationLimitsMinDurationMs = MinDurationMsOrFrames is { } min
            ? MsOrFrames.ToMillisecondsForSave(min, IsFrameMode, _loadedMinDurationMs)
            : 0;
        Se.Settings.Tools.ApplyDurationLimitsMaxDurationMs = MaxDurationMsOrFrames is { } max
            ? MsOrFrames.ToMillisecondsForSave(max, IsFrameMode, _loadedMaxDurationMs)
            : 0;
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

        if (FixMinDurationMs && FixMaxDurationMs && MinDurationMsOrFrames >= MaxDurationMsOrFrames)
        {
            var msg = Se.Language.Tools.ApplyDurationLimits.MaxDurationShouldBeHigherThanMinDuration;
            await MessageBox.Show(Window, Se.Language.General.Error, msg, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // The preview timer fills AllSubtitlesFixed, so before its first tick - or right after a
        // value changed - OK either did nothing or applied the previous limits. Always build it
        // now: the timer clears _isDirty on its own thread and only posts the rebuild, so a tick
        // landing between a change and OK left the flag clear and the list still stale.
        // The rebuild below re-creates every fix as applied; remember which rows the user
        // unticked (rows line up with _allSubtitles by index) and put their timing back.
        var skipIndices = Fixes.Where(f => !f.Apply).Select(f => AllSubtitlesFixed.IndexOf(f.SubtitleLine)).Where(i => i >= 0).ToList();
        _isDirty = false;
        BuildPreview();
        foreach (var index in skipIndices)
        {
            if (index < AllSubtitlesFixed.Count && index < _allSubtitles.Count)
            {
                AllSubtitlesFixed[index].EndTime = _allSubtitles[index].EndTime;
            }
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
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/apply-duration-limits");
        }
    }

    /// <param name="onlyIds">When given, only lines with these ids are fixed; the rest travel
    /// through unchanged so a fix is still capped against the real next line.</param>
    public void Initialize(List<SubtitleLineViewModel> toList, List<double> shotChanges, ISet<Guid>? onlyIds = null)
    {
        _allSubtitles = toList;
        _onlyIds = onlyIds;
        _shotChanges = shotChanges.OrderBy(p => p).ToList();
        IsDoNotGoPastShotChangeVisible = _shotChanges.Count > 0;
        _previewTimer.Start();
        _isDirty = true;
    }

    internal void SetChanged()
    {
        _isDirty = true;
    }
}