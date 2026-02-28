using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes;
using Nikse.SubtitleEdit.Logic;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Features.Shared.PickMp4Track;

public partial class PickMp4TrackViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<Mp4TrackInfoDisplay> _tracks;
    [ObservableProperty] private Mp4TrackInfoDisplay? _selectedTrack;
    [ObservableProperty] private ObservableCollection<Mp4SubtitleCueDisplay> _rows;

    public Window? Window { get; set; }
    public DataGrid TracksGrid { get; set; }
    public Mp4TrackInfoDisplay? SelectedMatroskaTrack { get; set; }
    public bool OkPressed { get; private set; }
    public string WindowTitle { get; private set; }

    private List<Trak> _mp4Tracks;

    public PickMp4TrackViewModel()
    {
        Tracks = new ObservableCollection<Mp4TrackInfoDisplay>();
        TracksGrid = new DataGrid();
        WindowTitle = string.Empty;
        Rows = new ObservableCollection<Mp4SubtitleCueDisplay>();
        _mp4Tracks = new List<Trak>();
    }

    public void Initialize(List<Trak> mp4Tracks, string fileName)
    {
        _mp4Tracks = mp4Tracks;
        WindowTitle = $"Pick MP4 track - {fileName}";
        foreach (var track in _mp4Tracks)
        {
            var display = new Mp4TrackInfoDisplay
            {
                HandlerType = track.Mdia.HandlerType,
                Name = track.Mdia.Name,
                StartPosition = track.Mdia.StartPosition,
                IsVobSubSubtitle = track.Mdia.IsVobSubSubtitle,
                Duration = track.Tkhd.Duration,
                Track = track,
            };
            Tracks.Add(display);
        }
    }

    private void Close()
    {
        Dispatcher.UIThread.Post(() =>
        {
            Window?.Close();
        });
    }

    [RelayCommand]
    private void Export()
    {
    }

    [RelayCommand]
    private void Ok()
    {
        SelectedMatroskaTrack = SelectedTrack;
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
    }

    internal void DataGridTracksSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        bool flowControl = TrackChanged();
        if (!flowControl)
        {
            return;
        }
    }

    private bool TrackChanged()
    {
        var selectedTrack = SelectedTrack;
        if (selectedTrack == null || selectedTrack.Track == null)
        {
            return false;
        }

        Rows.Clear();
        var trackinfo = selectedTrack.Track!;
        var subtitles = trackinfo.Mdia.Minf.Stbl.GetParagraphs();
        var i = 0;
        foreach (var item in subtitles)
        {
            i++;
            var cue = new Mp4SubtitleCueDisplay()
            {
                Number = i + 1,
                Show = item.StartTime.TimeSpan,
                Hide = item.EndTime.TimeSpan,
                Duration = TimeSpan.FromMilliseconds(item.EndTime.TotalMilliseconds - item.StartTime.TotalMilliseconds),
            };

            if (selectedTrack.IsVobSubSubtitle)
            {
                cue.Image = new Image { Source = trackinfo.Mdia.Minf.Stbl.SubPictures[i - 1].GetBitmap(null, SKColors.Transparent, SKColors.Black, SKColors.White, SKColors.Black, false).ToAvaloniaBitmap() };
            }
            else
            {
                cue.Text = item.Text;
            }

            Rows.Add(cue);
        }

        return true;
    }

    internal void SelectAndScrollToRow(int index)
    {
        if (index < 0 || index >= Tracks.Count)
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            TracksGrid.SelectedIndex = index;
            TracksGrid.ScrollIntoView(TracksGrid.SelectedItem, null);
            TrackChanged();
        }, DispatcherPriority.Background);
    }
}