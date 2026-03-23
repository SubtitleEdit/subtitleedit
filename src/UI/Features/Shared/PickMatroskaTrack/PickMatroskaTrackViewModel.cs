using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Files.ExportImageBased;
using Nikse.SubtitleEdit.Features.Shared.PromptFileSaved;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Shared.PickMatroskaTrack;

public partial class PickMatroskaTrackViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<MatroskaTrackInfoDisplay> _tracks;
    [ObservableProperty] private MatroskaTrackInfoDisplay? _selectedTrack;
    [ObservableProperty] private ObservableCollection<MatroskaSubtitleCueDisplay> _rows;

    public Window? Window { get; set; }
    public DataGrid TracksGrid { get; set; }
    public MatroskaTrackInfo? SelectedMatroskaTrack { get; set; }
    public bool OkPressed { get; private set; }
    public string WindowTitle { get; private set; }

    private readonly IFileHelper _fileHelper;
    private readonly IWindowService _windowService;

    private List<MatroskaTrackInfo> _matroskaTracks;
    private MatroskaFile? _matroskaFile;
    private string _fileName;

    public PickMatroskaTrackViewModel(IFileHelper fileHelper, IWindowService windowService)
    {
        _fileHelper = fileHelper;
        _windowService = windowService;
        Tracks = new ObservableCollection<MatroskaTrackInfoDisplay>();
        TracksGrid = new DataGrid();
        WindowTitle = string.Empty;
        Rows = new ObservableCollection<MatroskaSubtitleCueDisplay>();
        _matroskaTracks = new List<MatroskaTrackInfo>();
        _fileName = string.Empty;
    }

    public void Initialize(MatroskaFile matroskaFile, List<MatroskaTrackInfo> matroskaTracks, string fileName)
    {
        _matroskaFile = matroskaFile;
        _matroskaTracks = matroskaTracks;
        _fileName = fileName;
        WindowTitle = string.Format(Se.Language.File.PickMatroskaTrackX, fileName);
        foreach (var track in _matroskaTracks)
        {
            var display = new MatroskaTrackInfoDisplay
            {
                TrackNumber = track.TrackNumber,
                IsDefault = track.IsDefault,
                IsForced = track.IsForced,
                Codec = track.CodecId,
                Language = track.Language,
                Name = track.Name,
                MatroskaTrackInfo = track,
            };
            Tracks.Add(display);
        }
    }

    private void Close()
    {
        Dispatcher.UIThread.Post(() => { Window?.Close(); });
    }

    [RelayCommand]
    private async Task Export()
    {
        if (Window == null)
        {
            return;
        }

        var selectedTrack = SelectedTrack;
        if (selectedTrack == null)
        {
            return;
        }

        var trackInfo = selectedTrack.MatroskaTrackInfo!;
        var subtitles = _matroskaFile?.GetSubtitle(trackInfo.TrackNumber, null);
        if (trackInfo.CodecId == MatroskaTrackType.SubRip && subtitles != null)
        {
            await WriteTextSubtitleFile(Window, trackInfo, subtitles, new SubRip());
        }
        else if (trackInfo.CodecId is MatroskaTrackType.SubStationAlpha or MatroskaTrackType.SubStationAlpha2 && subtitles != null)
        {
            await WriteTextSubtitleFile(Window, trackInfo, subtitles, new SubStationAlpha());
        }
        else if (trackInfo.CodecId is MatroskaTrackType.AdvancedSubStationAlpha or MatroskaTrackType.AdvancedSubStationAlpha2 && subtitles != null)
        {
            await WriteTextSubtitleFile(Window, trackInfo, subtitles, new AdvancedSubStationAlpha());
        }
        else if (trackInfo.CodecId == MatroskaTrackType.BluRay && subtitles != null && _matroskaFile != null)
        {
            var suggestedFileName = Path.GetFileNameWithoutExtension(_fileName);
            var fileName = await _fileHelper.PickSaveSubtitleFile(Window, ".sup", suggestedFileName, Se.Language.General.SaveFileAsTitle);
            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }

            var pcsData = BluRaySupParser.ParseBluRaySupFromMatroska(trackInfo, _matroskaFile);
            if (pcsData.Count == 0)
            {
                return;
            }

            var exportHandler = new ExportHandlerBluRaySup();
            exportHandler.WriteHeader(fileName, new ImageParameter
            {
                ScreenWidth = (int)Math.Round(pcsData[0].GetScreenSize().Width, MidpointRounding.AwayFromZero),
                ScreenHeight = (int)Math.Round(pcsData[0].GetScreenSize().Height, MidpointRounding.AwayFromZero),
            });
            for (var i = 0; i < pcsData.Count; i++)
            {
                var item = pcsData[i];
                var ip = new ImageParameter
                {
                    Bitmap = item.GetBitmap(),
                    StartTime = TimeSpan.FromMilliseconds(item.StartTimeCode.TotalMilliseconds),
                    EndTime = TimeSpan.FromMilliseconds(item.EndTimeCode.TotalMilliseconds),
                    ScreenWidth = 1920,
                    ScreenHeight = 1080,
                    Index = i + 1,
                    OverridePosition = new SKPointI(item.GetPosition().Left, item.GetPosition().Top),
                };
                exportHandler.WriteParagraph(ip);
            }

            exportHandler.WriteFooter();

            _ = await _windowService.ShowDialogAsync<PromptFileSavedWindow, PromptFileSavedViewModel>(Window,
                vm => { vm.Initialize(Se.Language.General.SubtitleFileSaved, string.Format(Se.Language.General.SubtitleFileSavedToX, fileName), fileName, true, true); });
        }
        else if (trackInfo.CodecId == MatroskaTrackType.TextSt && subtitles != null && _matroskaFile != null)
        {
            var subtitle = new Subtitle();
            var sub = _matroskaFile.GetSubtitle(trackInfo.TrackNumber, null);
            Utilities.LoadMatroskaTextSubtitle(trackInfo, _matroskaFile, sub, subtitle);
            Utilities.ParseMatroskaTextSt(trackInfo, sub, subtitle);
            await WriteTextSubtitleFile(Window, trackInfo, subtitles, new SubRip());
        }
        else
        {
            await MessageBox.Show(Window, Se.Language.General.Error, "Format not supported: " + trackInfo.CodecId);
        }
    }

    private async Task WriteTextSubtitleFile(Window window, MatroskaTrackInfo trackInfo, List<MatroskaSubtitle> subtitles, SubtitleFormat format)
    {
        var sub = new Subtitle();
        Utilities.LoadMatroskaTextSubtitle(trackInfo, _matroskaFile, subtitles, sub);
        var rawText = format.ToText(sub, string.Empty);
        var suggestedFileName = Path.GetFileNameWithoutExtension(_fileName);
        var fileName = await _fileHelper.PickSaveSubtitleFile(window, format.Extension, suggestedFileName, Se.Language.General.SaveFileAsTitle);

        if (!string.IsNullOrEmpty(fileName))
        {
            await File.WriteAllTextAsync(fileName, rawText, Encoding.UTF8);
            _ = await _windowService.ShowDialogAsync<PromptFileSavedWindow, PromptFileSavedViewModel>(window,
                vm => { vm.Initialize(Se.Language.General.SubtitleFileSaved, string.Format(Se.Language.General.SubtitleFileSavedToX, fileName), fileName, true, true); });
        }
    }

    [RelayCommand]
    private void Ok()
    {
        SelectedMatroskaTrack = SelectedTrack?.MatroskaTrackInfo;
        OkPressed = true;
        Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Close();
    }

    internal void OnKeyDownHandler(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Cancel();
            e.Handled = true;
        }
        else if (e.Key == Key.Enter && TracksGrid.IsFocused)
        {
            Ok();
            e.Handled = true;
        }
    }

    internal void DataGridTracksSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        TrackChanged();
    }

    private bool TrackChanged()
    {
        var selectedTrack = SelectedTrack;
        if (selectedTrack == null || selectedTrack.MatroskaTrackInfo == null)
        {
            return false;
        }

        Rows.Clear();
        var trackInfo = selectedTrack.MatroskaTrackInfo!;
        var subtitles = _matroskaFile?.GetSubtitle(trackInfo.TrackNumber, null);
        if (trackInfo.CodecId == MatroskaTrackType.SubRip && subtitles != null)
        {
            AddTextContent(trackInfo, subtitles, new SubRip());
        }
        else if (trackInfo.CodecId is MatroskaTrackType.SubStationAlpha or MatroskaTrackType.SubStationAlpha2 && subtitles != null)
        {
            AddTextContent(trackInfo, subtitles, new SubStationAlpha());
        }
        else if (trackInfo.CodecId is MatroskaTrackType.AdvancedSubStationAlpha or MatroskaTrackType.AdvancedSubStationAlpha2 && subtitles != null)
        {
            AddTextContent(trackInfo, subtitles, new AdvancedSubStationAlpha());
        }
        else if (trackInfo.CodecId is MatroskaTrackType.WebVTT or MatroskaTrackType.WebVTT2 && subtitles != null)
        {
            AddTextContent(trackInfo, subtitles, new WebVTT());
        }
        else if (trackInfo.CodecId == MatroskaTrackType.BluRay && subtitles != null && _matroskaFile != null)
        {
            var pcsData = BluRaySupParser.ParseBluRaySupFromMatroska(trackInfo, _matroskaFile);
            for (var i = 0; i < 20 && i < pcsData.Count; i++)
            {
                var item = pcsData[i];
                var bitmap = item.GetBitmap();
                var cue = new MatroskaSubtitleCueDisplay()
                {
                    Number = i + 1,
                    Show = TimeSpan.FromMilliseconds(item.StartTimeCode.TotalMilliseconds),
                    Duration = TimeSpan.FromMilliseconds(item.EndTimeCode.TotalMilliseconds - item.StartTimeCode.TotalMilliseconds),
                    Image = new Image { Source = bitmap.ToAvaloniaBitmap() },
                };
                Rows.Add(cue);
            }
        }
        else if (trackInfo.CodecId == MatroskaTrackType.TextSt && subtitles != null && _matroskaFile != null)
        {
            var subtitle = new Subtitle();
            var sub = _matroskaFile.GetSubtitle(trackInfo.TrackNumber, null);
            Utilities.LoadMatroskaTextSubtitle(trackInfo, _matroskaFile, sub, subtitle);
            Utilities.ParseMatroskaTextSt(trackInfo, sub, subtitle);

            for (var i = 0; i < 20 && i < subtitle.Paragraphs.Count; i++)
            {
                var item = subtitle.Paragraphs[i];
                var cue = new MatroskaSubtitleCueDisplay()
                {
                    Number = i + 1,
                    Show = item.StartTime.TimeSpan,
                    Duration = TimeSpan.FromMilliseconds(item.EndTime.TotalMilliseconds - item.StartTime.TotalMilliseconds),
                    Text = item.Text,
                };
                Rows.Add(cue);
            }
        }

        return true;
    }

    private void AddTextContent(MatroskaTrackInfo trackInfo, List<MatroskaSubtitle> subtitles, SubtitleFormat format)
    {
        var sub = new Subtitle();
        Utilities.LoadMatroskaTextSubtitle(trackInfo, _matroskaFile, subtitles, sub);
        var raw = format.ToText(sub, string.Empty);
        for (var i = 0; i < sub.Paragraphs.Count; i++)
        {
            var p = sub.Paragraphs[i];
            var cue = new MatroskaSubtitleCueDisplay()
            {
                Number = p.Number,
                Text = p.Text,
                Show = TimeSpan.FromMilliseconds(p.StartTime.TotalMilliseconds),
                Duration = TimeSpan.FromMilliseconds(p.EndTime.TotalMilliseconds - p.StartTime.TotalMilliseconds),
            };
            Rows.Add(cue);
        }
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