using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Shared;
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

    private readonly IFileHelper _fileHelper;
    private readonly IWindowService _windowService;

    private string _videoFileName;

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
    }

    public void Initialize(List<SubtitleLineViewModel> subtitles, string videoFileName, string fileName)
    {
        Subtitles.Clear();
        Subtitles.AddRange(subtitles);
        FileName = fileName;
        _videoFileName = videoFileName;

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
        foreach (var p in subtitle.Paragraphs)
        {
            Othersubtitles.Add(new SubtitleLineViewModel(p, subtitle.OriginalFormat));
        }
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

        var syncPoint = new SyncPoint(
            left, Subtitles.IndexOf(left),
            right, Othersubtitles.IndexOf(right));

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
            UiUtil.ShowHelp("features/point-sync-via-other");
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