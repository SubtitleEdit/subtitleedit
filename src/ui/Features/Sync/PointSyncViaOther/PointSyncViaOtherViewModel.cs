using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Shared.FindText;
using Nikse.SubtitleEdit.Features.Sync.PointSync.SetSyncPoint;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Sync.PointSyncViaOther;

public partial class PointSyncViaOtherViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<SubtitleLineViewModel> _subtitles;
    [ObservableProperty] private SubtitleLineViewModel? _selectedSubtitle;
    [ObservableProperty] private ObservableCollection<SubtitleLineViewModel> _othersubtitles;
    [ObservableProperty] private SubtitleLineViewModel? _selectedOtherSubtitle;
    [ObservableProperty] private string _fileName;
    [ObservableProperty] private string _fileNameOther;
    [ObservableProperty] private bool _isOkEnabled;

    public ObservableCollection<SyncPoint> SyncPoints { get; set; }
    [ObservableProperty] private SyncPoint? _selectedSyncPoint;

    public Window? Window { get; set; }
    public List<SubtitleLineViewModel> SyncedSubtitles { get; private set; }
    public bool OkPressed { get; private set; }
    public string WindowTitle { get; private set; }

    /// <summary>
    /// The video in use when the window closed - the caller adopts it, so a video opened from
    /// "Set sync point via video" also reaches the main window (issue #13341).
    /// </summary>
    public string VideoFileName => _videoFileName;

    /// <summary>Set by the window; scrolls the "other subtitle" grid to a line without selecting it.</summary>
    public Action<SubtitleLineViewModel>? ScrollOtherToLine { get; set; }

    private readonly IFileHelper _fileHelper;
    private readonly IWindowService _windowService;

    // Carried down to the "Set sync point" dialog, which opens its own player (issue #13995).
    private int _audioTrackId = -1;

    private string _videoFileName;
    private List<SubtitleLineViewModel> _originalSubtitles;

    // Only passed on to "Set sync point via video", which draws the subtitle on its video (#13767).
    private VideoPreviewSubtitleContext _previewContext = VideoPreviewSubtitleContext.Default;

    public PointSyncViaOtherViewModel(IFileHelper fileHelper, IWindowService windowService)
    {
        _fileHelper = fileHelper;
        _windowService = windowService;
        Subtitles = new ObservableCollection<SubtitleLineViewModel>();
        Othersubtitles = new ObservableCollection<SubtitleLineViewModel>();
        SyncPoints = new ObservableCollection<SyncPoint>();
        WindowTitle = string.Empty;
        FileName = string.Empty;
        FileNameOther = string.Empty;
        _videoFileName = string.Empty;
        SyncedSubtitles = new List<SubtitleLineViewModel>();
        _originalSubtitles = new List<SubtitleLineViewModel>();
    }

    public void Initialize(List<SubtitleLineViewModel> subtitles, string videoFileName, string fileName, VideoPreviewSubtitleContext previewContext, int audioTrackId = -1)
    {
        _audioTrackId = audioTrackId;
        Subtitles.Clear();
        Subtitles.AddRange(subtitles);
        _originalSubtitles = subtitles.Select(s => new SubtitleLineViewModel(s)).ToList();
        FileName = fileName;
        _videoFileName = videoFileName;
        _previewContext = previewContext;
        UpdateGaps(Subtitles);

        if (Subtitles.Count > 0)
        {
            SelectedSubtitle = Subtitles[0];
        }
    }

    /// <summary>
    /// Fills in the silence before each line's start, shown in the "Gap" column to point out
    /// likely sync points (issue #10175). Unlike the main grid's gap-to-next, the gap here is
    /// the one *before* a line - a line starting after silence is where two independently made
    /// subtitle files are most likely to truly align. The first line's gap is measured from
    /// 00:00, since it too starts after "silence".
    /// </summary>
    private static void UpdateGaps(IList<SubtitleLineViewModel> lines)
    {
        for (var i = 0; i < lines.Count; i++)
        {
            var previousEnd = i == 0 ? TimeSpan.Zero : lines[i - 1].EndTime;
            lines[i].PreviousGap = (lines[i].StartTime - previousEnd).TotalMilliseconds;
        }
    }

    private void Close()
    {
        Dispatcher.UIThread.Post(() => { Window?.Close(); });
    }

    // Clicking a line in the left grid scrolls the other grid to the matching time (#12529).
    // Only the view scrolls - the other grid's selection is the user's pick for the next sync
    // point, so it must not change. The time is offset by the nearest existing sync point so
    // the jump still lands right when the two files drift apart.
    partial void OnSelectedSubtitleChanged(SubtitleLineViewModel? value)
    {
        if (value == null || Othersubtitles.Count == 0)
        {
            return;
        }

        // Compare in original-time space: sync points store original left times, and the
        // grid line may show previewed times after "Apply" (#12942).
        var leftIndexInGrid = Subtitles.IndexOf(value);
        var leftTime = leftIndexInGrid >= 0 && leftIndexInGrid < _originalSubtitles.Count
            ? _originalSubtitles[leftIndexInGrid].StartTime
            : value.StartTime;
        SyncPoint? nearestPoint = null;
        var nearestDistance = TimeSpan.MaxValue;
        foreach (var syncPoint in SyncPoints)
        {
            var distance = (syncPoint.LeftStartTime - leftTime).Duration();
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestPoint = syncPoint;
            }
        }

        var targetTime = nearestPoint == null
            ? leftTime
            : leftTime + (nearestPoint.RightStartTime - nearestPoint.LeftStartTime);

        SubtitleLineViewModel? closest = null;
        var closestDiff = TimeSpan.MaxValue;
        foreach (var line in Othersubtitles)
        {
            var diff = (line.StartTime - targetTime).Duration();
            if (diff < closestDiff)
            {
                closestDiff = diff;
                closest = line;
            }
        }

        if (closest != null)
        {
            ScrollOtherToLine?.Invoke(closest);
        }
    }

    [RelayCommand]
    private async Task BrowseOther()
    {
        if (Window == null)
        {
            return;
        }

        var fileName = await _fileHelper.PickOpenSubtitleFile(Window, Se.Language.General.OpenSubtitleFileTitle);
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        var subtitle = Subtitle.Parse(fileName);
        if (subtitle == null)
        {
            foreach (var f in SubtitleFormat.GetBinaryFormats(false))
            {
                if (f.IsMine(null, fileName))
                {
                    subtitle = new Subtitle();
                    f.LoadSubtitle(subtitle, null, fileName);
                    subtitle.OriginalFormat = f;
                    break; // format found, exit the loop
                }
            }
        }

        if (subtitle == null)
        {
            await MessageBox.Show(Window, Se.Language.General.Error, Se.Language.General.UnknownSubtitleFormat);
            return;
        }

        FileNameOther = fileName;
        Othersubtitles.Clear();
        foreach (var p in subtitle.Paragraphs)
        {
            Othersubtitles.Add(new SubtitleLineViewModel(p, subtitle.OriginalFormat));
        }

        UpdateGaps(Othersubtitles);
        SelectedOtherSubtitle = Othersubtitles.FirstOrDefault();

        // Sync points reference lines in the replaced file, so they are no longer valid.
        SyncPoints.Clear();
        IsOkEnabled = false;
    }

    [RelayCommand]
    private void SetSyncPoint()
    {
        var left = SelectedSubtitle;
        var right = SelectedOtherSubtitle;

        if (left == null)
        {
            return;
        }

        if (right == null)
        {
            return;
        }

        // Sync points are applied to the original timings, so the left time must come from
        // the original line - the grid line may already show previewed times after "Apply",
        // which would bake the previous offset into the new point (#12942).
        var leftIndex = Subtitles.IndexOf(left);
        AddSyncPoint(new SyncPoint(
            _originalSubtitles[leftIndex], leftIndex,
            right, Othersubtitles.IndexOf(right)));
    }

    /// <summary>
    /// Sets a sync point from the video instead of from the other subtitle - for lines the other
    /// file has no counterpart for, and for when there is no other file at all yet (issue #13341).
    /// </summary>
    [RelayCommand]
    private async Task SetSyncPointViaVideo()
    {
        var left = SelectedSubtitle;
        if (left == null || Window == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<SetSyncPointWindow, SetSyncPointViewModel>(Window, vm =>
        {
            vm.Initialize(Subtitles.ToList(), left, _videoFileName, FileName, _previewContext, null, _audioTrackId);
        });

        // Keep a video opened (or found) in there - also when the dialog was cancelled.
        if (!string.IsNullOrEmpty(result.VideoFileName))
        {
            _videoFileName = result.VideoFileName;
        }

        if (!result.OkPressed)
        {
            return;
        }

        // As in SetSyncPoint: the left time has to come from the original line, since the grid may
        // already show previewed times after "Apply" (#12942). The right side is the video
        // position, carried on a copy of the line - there is no other-subtitle line for it.
        var leftIndex = Subtitles.IndexOf(left);
        if (leftIndex < 0 || leftIndex >= _originalSubtitles.Count)
        {
            return;
        }

        var right = new SubtitleLineViewModel(_originalSubtitles[leftIndex])
        {
            StartTime = TimeSpan.FromSeconds(result.SyncPosition),
        };

        AddSyncPoint(new SyncPoint(_originalSubtitles[leftIndex], leftIndex, right, leftIndex));
    }

    /// <summary>
    /// A line has at most one sync point (setting it again re-points it), and the list stays in
    /// chronological order (#12529).
    /// </summary>
    private void AddSyncPoint(SyncPoint syncPoint)
    {
        var existing = SyncPoints.FirstOrDefault(p => p.LeftIndex == syncPoint.LeftIndex);
        if (existing != null)
        {
            SyncPoints.Remove(existing);
        }

        var insertAt = 0;
        while (insertAt < SyncPoints.Count && SyncPoints[insertAt].LeftIndex < syncPoint.LeftIndex)
        {
            insertAt++;
        }

        SyncPoints.Insert(insertAt, syncPoint);
        SelectedSyncPoint = syncPoint;
        IsOkEnabled = true;
    }

    [RelayCommand]
    private void DeleteSelectedPointSync()
    {
        var selectedPointSync = SelectedSyncPoint;
        if (selectedPointSync == null)
        {
            return;
        }

        SyncPoints.Remove(selectedPointSync);
        IsOkEnabled = SyncPoints.Count > 0;
    }

    [RelayCommand]
    private void Apply()
    {
        if (SyncPoints.Count == 0)
        {
            return;
        }

        // Re-preview from the original timings so repeated Apply clicks don't stack.
        var synced = PointSyncer.PointSync(_originalSubtitles, SyncPoints.ToList());
        for (var i = 0; i < Subtitles.Count && i < synced.Count; i++)
        {
            Subtitles[i].StartTime = synced[i].StartTime;
            Subtitles[i].EndTime = synced[i].EndTime;
        }

        UpdateGaps(Subtitles);
    }

    [RelayCommand]
    private async Task FindTextLeft()
    {
        if (Window == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<FindTextWindow, FindTextViewModel>(Window, vm =>
        {
            vm.Initialize(Subtitles.ToList(), string.Format(Se.Language.General.FindTextX, FileName));
        });

        if (result.OkPressed && result.SelectedSubtitle != null)
        {
            SelectedSubtitle = result.SelectedSubtitle;
        }
    }

    [RelayCommand]
    private async Task FindTextOther()
    {
        if (Window == null || Othersubtitles.Count == 0)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<FindTextWindow, FindTextViewModel>(Window, vm =>
        {
            vm.Initialize(Othersubtitles.ToList(), string.Format(Se.Language.General.FindTextX, FileNameOther));
        });

        if (result.OkPressed && result.SelectedSubtitle != null)
        {
            SelectedOtherSubtitle = result.SelectedSubtitle;
        }
    }

    [RelayCommand]
    private void Ok()
    {
        SyncedSubtitles = PointSyncer.PointSync(_originalSubtitles, SyncPoints.ToList());
        OkPressed = true;
        Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Close();
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Cancel();
        }
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/point-sync-via-other");
        }
    }

}