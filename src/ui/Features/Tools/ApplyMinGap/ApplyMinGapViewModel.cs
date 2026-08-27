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

public partial class ApplyMinGapViewModel : ObservableObject, IClosingCleanup
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
    private volatile bool _isClosing;
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
            _dirty = false;
            UpdatePreview();
        }

        // Guard the restart: OnClosingCleanup may have disposed the timer while this handler ran,
        // and Start() on a disposed timer throws ObjectDisposedException (no longer swallowed on
        // modern .NET), crashing the app from a thread-pool thread. (#12739)
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

    private void UpdatePreview()
    {
        var (fixedSubtitles, previewItems, fixedCount) = BuildFixedSubtitles();
        FixedSubtitles = fixedSubtitles;

        Dispatcher.UIThread.Post(() =>
        {
            Subtitles.Clear();
            foreach (var item in previewItems)
            {
                Subtitles.Add(item);
            }

            StatusText = string.Format(Se.Language.Tools.ApplyMinGaps.NumberOfGapsFixedX, fixedCount);
        });
    }

    /// <summary>
    /// Applies the current minimum gap to a copy of the subtitle and returns the result together
    /// with the preview rows. Kept synchronous so <see cref="Ok"/> can build the list it hands to
    /// the caller instead of depending on the preview timer having ticked.
    /// </summary>
    private (List<SubtitleLineViewModel> Fixed, List<ApplyMinGapItem> Preview, int FixedCount) BuildFixedSubtitles()
    {
        var minMsBetweenLines = MinGapMsOrFrames;
        if (Configuration.Settings.General.UseTimeFormatHHMMSSFF)
        {
            minMsBetweenLines = SubtitleFormat.FramesToMilliseconds(minMsBetweenLines);
        }

        var fixedSubtitles = _allSubtitles.Select(p => new SubtitleLineViewModel(p)).ToList();
        var previewItems = new List<ApplyMinGapItem>();
        var fixedCount = 0;

        for (var index = 0; index < fixedSubtitles.Count - 1; index++)
        {
            var current = fixedSubtitles[index];
            var next = fixedSubtitles[index + 1];
            var gapMs = next.StartTime.TotalMilliseconds - current.EndTime.TotalMilliseconds;
            if (gapMs >= minMsBetweenLines)
            {
                continue;
            }

            fixedCount++;

            var before = new TimeCode(gapMs).ToShortDisplayString();

            var newEndMs = next.StartTime.TotalMilliseconds - minMsBetweenLines;
            current.EndTime = TimeSpan.FromMilliseconds(newEndMs);
            var newGapMs = next.StartTime.TotalMilliseconds - current.EndTime.TotalMilliseconds;

            var after = new TimeCode(newGapMs).ToShortDisplayString();
            var fixFormat = Se.Language.Tools.ApplyMinGaps.ChangedGapFromXToYCommentZ;
            var comment = string.Empty;
            var info = string.Format(fixFormat, before, after, comment);

            previewItems.Add(new ApplyMinGapItem(current) { InfoText = info });
        }

        return (fixedSubtitles, previewItems, fixedCount);
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
        // Kept per unit: the box holds frames in frame mode and milliseconds otherwise, so one
        // shared number meant a gap saved as 10 ms came back as 10 frames (~400 ms) after the
        // user switched the time format. 0 means "not saved yet" - fall back to the general
        // minimum-gap setting, which is what the dialog used to open on every time because
        // nothing here was ever written back.
        if (Se.Settings.General.UseFrameMode)
        {
            var savedFrames = Se.Settings.Tools.ApplyMinGapFrames;
            MinGapMsOrFrames = savedFrames > 0 ? savedFrames : Se.Settings.General.MinimumBetweenLines.Frames;
        }
        else
        {
            var savedMs = Se.Settings.Tools.ApplyMinGapMilliseconds;
            MinGapMsOrFrames = savedMs > 0 ? savedMs : Se.Settings.General.MinimumBetweenLines.Milliseconds;
        }
    }

    private void SaveSettings()
    {
        if (Se.Settings.General.UseFrameMode)
        {
            Se.Settings.Tools.ApplyMinGapFrames = MinGapMsOrFrames;
        }
        else
        {
            Se.Settings.Tools.ApplyMinGapMilliseconds = MinGapMsOrFrames;
        }

        Se.SaveSettings();
    }

    [RelayCommand]
    private void Ok()
    {
        // FixedSubtitles was only ever built by the 500 ms preview timer, and the caller replaces
        // the whole subtitle with it - so pressing OK before the first tick wiped every line, and
        // pressing it right after changing the gap applied the previous value. Build it here.
        var (fixedSubtitles, _, _) = BuildFixedSubtitles();
        FixedSubtitles = fixedSubtitles;

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