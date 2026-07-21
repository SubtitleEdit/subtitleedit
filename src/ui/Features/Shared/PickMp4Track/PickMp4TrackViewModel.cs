using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Shared.PromptFileSaved;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.UiLogic.Export;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Shared.PickMp4Track;

public partial class PickMp4TrackViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<Mp4TrackInfoDisplay> _tracks;
    [ObservableProperty] private Mp4TrackInfoDisplay? _selectedTrack;
    [ObservableProperty] private ObservableCollection<Mp4SubtitleCueDisplay> _rows;
    [ObservableProperty] private string _subtitleCountText;

    public Window? Window { get; set; }
    public DataGrid TracksGrid { get; set; }
    public Mp4TrackInfoDisplay? SelectedMatroskaTrack { get; set; }
    public bool OkPressed { get; private set; }
    public string WindowTitle { get; private set; }

    private readonly IFileHelper _fileHelper;
    private readonly IWindowService _windowService;

    private List<Trak> _mp4Tracks;
    private string _fileName;

    public PickMp4TrackViewModel(IFileHelper fileHelper, IWindowService windowService)
    {
        _fileHelper = fileHelper;
        _windowService = windowService;
        Tracks = new ObservableCollection<Mp4TrackInfoDisplay>();
        TracksGrid = new DataGrid();
        WindowTitle = string.Empty;
        SubtitleCountText = string.Empty;
        Rows = new ObservableCollection<Mp4SubtitleCueDisplay>();
        _mp4Tracks = new List<Trak>();
        _fileName = string.Empty;
    }

    public void Initialize(List<Trak> mp4Tracks, string fileName)
    {
        _mp4Tracks = mp4Tracks;
        _fileName = fileName;
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

    /// <summary>
    /// Saves the selected track to a file: text tracks as SubRip, VobSub image
    /// tracks as Blu-ray .sup (same approach as the Matroska track picker).
    /// </summary>
    [RelayCommand]
    private async Task Export()
    {
        var selectedTrack = SelectedTrack;
        var track = selectedTrack?.Track;
        if (Window == null || track == null)
        {
            return;
        }

        var suggestedFileName = Utilities.GetPathAndFileNameWithoutExtension(_fileName);

        if (track.Mdia.IsVobSubSubtitle)
        {
            var fileName = await _fileHelper.PickSaveSubtitleFile(Window, ".sup", suggestedFileName, Se.Language.General.SaveFileAsTitle);
            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }

            var paragraphs = track.Mdia.Minf.Stbl.GetParagraphs();
            var subPictures = track.Mdia.Minf.Stbl.SubPictures;
            var count = Math.Min(paragraphs.Count, subPictures.Count);
            if (count == 0)
            {
                return;
            }

            // VobSub is DVD-resolution; the track header carries the real canvas
            // size, with NTSC DVD as the fallback when it's missing.
            var screenWidth = (int)track.Tkhd.Width;
            var screenHeight = (int)track.Tkhd.Height;
            if (screenWidth <= 0 || screenHeight <= 0)
            {
                screenWidth = 720;
                screenHeight = 480;
            }

            var exportHandler = new ExportHandlerBluRaySup();
            exportHandler.WriteHeader(fileName, new ImageParameter
            {
                ScreenWidth = screenWidth,
                ScreenHeight = screenHeight,
            });
            for (var i = 0; i < count; i++)
            {
                var subPicture = subPictures[i];
                var paragraph = paragraphs[i];
                exportHandler.WriteParagraph(new ImageParameter
                {
                    Bitmap = subPicture.GetBitmap(null, SKColors.Transparent, SKColors.Black, SKColors.White, SKColors.Black, false),
                    StartTime = TimeSpan.FromMilliseconds(paragraph.StartTime.TotalMilliseconds),
                    EndTime = TimeSpan.FromMilliseconds(paragraph.EndTime.TotalMilliseconds),
                    ScreenWidth = screenWidth,
                    ScreenHeight = screenHeight,
                    Index = i + 1,
                    OverridePosition = new SKPointI(subPicture.ImageDisplayArea.Left, subPicture.ImageDisplayArea.Top),
                });
            }

            exportHandler.WriteFooter();

            _ = await _windowService.ShowDialogAsync<PromptFileSavedWindow, PromptFileSavedViewModel>(Window,
                vm => { vm.Initialize(Se.Language.General.SubtitleFileSaved, string.Format(Se.Language.General.SubtitleFileSavedToX, fileName), fileName, true, true); });
        }
        else
        {
            var subtitle = new Subtitle();
            subtitle.Paragraphs.AddRange(track.Mdia.Minf.Stbl.GetParagraphs());
            subtitle.Renumber();
            var format = new SubRip();
            var rawText = format.ToText(subtitle, string.Empty);

            var fileName = await _fileHelper.PickSaveSubtitleFile(Window, format.Extension, suggestedFileName, Se.Language.General.SaveFileAsTitle);
            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }

            await File.WriteAllTextAsync(fileName, rawText, Encoding.UTF8);
            _ = await _windowService.ShowDialogAsync<PromptFileSavedWindow, PromptFileSavedViewModel>(Window,
                vm => { vm.Initialize(Se.Language.General.SubtitleFileSaved, string.Format(Se.Language.General.SubtitleFileSavedToX, fileName), fileName, true, true); });
        }
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
            SubtitleCountText = string.Empty;
            return false;
        }

        Rows.Clear();
        var trackinfo = selectedTrack.Track!;
        var subtitles = trackinfo.Mdia.Minf.Stbl.GetParagraphs();
        SubtitleCountText = string.Format(Se.Language.File.Import.NumberOfSubtitlesX, subtitles.Count);
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