using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Sync.PointSync.SetSyncPoint;
using Nikse.SubtitleEdit.Features.Sync.PointSyncViaOther;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Sync.PointSync;

public partial class PointSyncViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<SubtitleLineViewModel> _subtitles;
    [ObservableProperty] private SubtitleLineViewModel? _selectedSubtitle;
    [ObservableProperty] private string _fileName;
    [ObservableProperty] private string _fileNameOther;
    [ObservableProperty] private bool _isOkEnabled;

    public ObservableCollection<SyncPoint> SyncPoints { get; set; }
    [ObservableProperty] private SyncPoint? _selectedSyncPoint;

    public Window? Window { get; set; }
    public List<SubtitleLineViewModel> SyncedSubtitles { get; private set; }
    public bool OkPressed { get; private set; }
    public string WindowTitle { get; private set; }

    private readonly IFileHelper _fileHelper;
    private readonly IWindowService _windowService;

    private string _videoFileName;
    private AudioVisualizer? _audioVisualizer;

    public PointSyncViewModel(IFileHelper fileHelper, IWindowService windowService)
    {
        _fileHelper = fileHelper;
        _windowService = windowService;
        Subtitles = new ObservableCollection<SubtitleLineViewModel>();
        SyncPoints = new ObservableCollection<SyncPoint>();
        WindowTitle = string.Empty;
        FileName = string.Empty;
        FileNameOther = string.Empty;
        _videoFileName = string.Empty;
        SyncedSubtitles = new List<SubtitleLineViewModel>();
    }

    public void Initialize(
        List<SubtitleLineViewModel> subtitles,
        List<SubtitleLineViewModel> selectedSubtitles,
        string videoFileName, 
        string fileName, 
        AudioVisualizer? audioVisualizer)
    {
        Subtitles.Clear();
        Subtitles.AddRange(subtitles);
        FileName = fileName;
        _videoFileName = videoFileName;
        _audioVisualizer = audioVisualizer;

        if (Subtitles.Count > 0)
        {
            SelectedSubtitle = Subtitles[0];
        }
    }

    private void Close()
    {
        Dispatcher.UIThread.Post(() => { Window?.Close(); });
    }

    [RelayCommand]
    private async Task SetSyncPoint()
    {
        var selectedSubtitle = SelectedSubtitle;
        if (selectedSubtitle == null || Window == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<SetSyncPointWindow, SetSyncPointViewModel>(Window, vm =>
        {
            vm.Initialize(Subtitles.ToList(), SelectedSubtitle, _videoFileName, FileName, _audioVisualizer);
        });

        if (!result.OkPressed)
        {
            return;
        }

        var subtitleOther = new SubtitleLineViewModel(selectedSubtitle);
        subtitleOther.StartTime = TimeSpan.FromSeconds(result.SyncPosition);
        var syncPoint = new SyncPoint(
            selectedSubtitle, Subtitles.IndexOf(selectedSubtitle),
            subtitleOther, Subtitles.IndexOf(selectedSubtitle));
        SyncPoints.Add(syncPoint);
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
    private void Ok()
    {
        SyncedSubtitles = PointSyncer.PointSync(Subtitles.ToList(), SyncPoints.ToList());
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
            UiUtil.ShowHelp("features/point-sync");
        }
    }

    internal void PointSyncContextMenuOpening(object? sender, EventArgs e)
    {
        if (SyncPoints.Count == 0)
        {
            return;
        }

        DeleteSelectedPointSync();
    }
}