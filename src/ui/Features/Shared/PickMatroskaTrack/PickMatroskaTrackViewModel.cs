using Nikse.SubtitleEdit.UiLogic.Export;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
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
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Shared.PickMatroskaTrack;

public partial class PickMatroskaTrackViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<MatroskaTrackInfoDisplay> _tracks;
    [ObservableProperty] private MatroskaTrackInfoDisplay? _selectedTrack;
    [ObservableProperty] private ObservableCollection<MatroskaSubtitleCueDisplay> _rows;
    [ObservableProperty] private string _subtitleCountText;

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

    // MatroskaFile is not thread-safe (single shared FileStream), so preview parsing must be
    // serialized. The token lets a newer selection discard a stale preview still queued behind it.
    private readonly SemaphoreSlim _previewLock = new(1, 1);
    private int _trackChangeToken;

    // GetSubtitle reads (and caches) the whole cluster data on its first call; that is the only
    // slow part, so the progress window is only shown until that initial read has completed.
    private bool _clusterLoaded;

    // Only bother with the progress window for files large enough that the read is noticeable.
    private const long MatroskaProgressWindowMinFileSize = 25 * 1024 * 1024; // 25 MB

    public PickMatroskaTrackViewModel(IFileHelper fileHelper, IWindowService windowService)
    {
        _fileHelper = fileHelper;
        _windowService = windowService;
        Tracks = new ObservableCollection<MatroskaTrackInfoDisplay>();
        TracksGrid = new DataGrid();
        WindowTitle = string.Empty;
        SubtitleCountText = string.Empty;
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
            var suggestedFileName = Utilities.GetPathAndFileNameWithoutExtension(_fileName);
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
            Utilities.LoadMatroskaTextSubtitle(trackInfo, _matroskaFile, subtitles, subtitle);
            Utilities.ParseMatroskaTextSt(trackInfo, subtitles, subtitle);
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
        var suggestedFileName = Utilities.GetPathAndFileNameWithoutExtension(_fileName);
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
        _ = TrackChangedAsync();
    }

    /// <summary>
    /// Builds the preview for the selected track. The actual parsing (which, on the first call,
    /// reads the whole file's cluster data) runs off the UI thread with a progress window so the
    /// dialog no longer freezes when picking a track in a large multi-subtitle file (#12193).
    /// </summary>
    private async Task TrackChangedAsync()
    {
        var trackInfo = SelectedTrack?.MatroskaTrackInfo;
        var matroskaFile = _matroskaFile;
        var token = ++_trackChangeToken;

        Rows.Clear();
        if (trackInfo == null || matroskaFile == null)
        {
            SubtitleCountText = string.Empty;
            return;
        }

        await _previewLock.WaitAsync();
        try
        {
            // A newer selection arrived while we were waiting for the previous preview to finish;
            // let that newer call build the preview instead.
            if (token != _trackChangeToken)
            {
                return;
            }

            PleaseWaitViewModel? pleaseWaitVm = null;
            if (!_clusterLoaded)
            {
                long fileSize = 0;
                try
                {
                    fileSize = new FileInfo(matroskaFile.Path).Length;
                }
                catch
                {
                    // ignore - just means no size-based gating
                }

                if (fileSize >= MatroskaProgressWindowMinFileSize && Window != null)
                {
                    pleaseWaitVm = _windowService.ShowWindow<PleaseWaitWindow, PleaseWaitViewModel>(Window);
                    pleaseWaitVm.StatusText = Se.Language.Main.ParsingMatroskaFile;
                }
            }

            try
            {
                var vm = pleaseWaitVm;
                var preview = await Task.Run(() => BuildPreview(trackInfo, matroskaFile, vm));
                _clusterLoaded = true;

                // Discard the result if the user moved on to another track meanwhile.
                if (token != _trackChangeToken)
                {
                    return;
                }

                foreach (var cue in preview.Cues)
                {
                    Rows.Add(new MatroskaSubtitleCueDisplay
                    {
                        Number = cue.Number,
                        Show = cue.Show,
                        Duration = cue.Duration,
                        Text = cue.Text ?? string.Empty,
                        Image = cue.Image != null ? new Image { Source = cue.Image } : null,
                    });
                }

                SubtitleCountText = string.Format(Se.Language.File.Import.NumberOfSubtitlesX, preview.Count);
            }
            catch (Exception exception)
            {
                Se.LogError(exception, "Error building Matroska track preview");
                SubtitleCountText = string.Empty;
            }
            finally
            {
                pleaseWaitVm?.Close();
            }
        }
        finally
        {
            _previewLock.Release();
        }
    }

    /// <summary>
    /// Parses the selected track into plain preview data. Runs on a background thread, so it must
    /// not touch any UI controls - the Avalonia <see cref="Bitmap"/> objects it creates are decoded
    /// here, but the <see cref="Image"/> controls that host them are built on the UI thread.
    /// </summary>
    private static PreviewResult BuildPreview(MatroskaTrackInfo trackInfo, MatroskaFile matroskaFile, PleaseWaitViewModel? pleaseWaitVm)
    {
        var cues = new List<PreviewCueData>();
        var count = 0;

        MatroskaFile.LoadMatroskaCallback? callback =
            pleaseWaitVm != null ? (position, total) => pleaseWaitVm.ReportProgress(position, total) : null;
        var subtitles = matroskaFile.GetSubtitle(trackInfo.TrackNumber, callback);
        if (subtitles == null)
        {
            return new PreviewResult(0, cues);
        }

        if (trackInfo.CodecId is MatroskaTrackType.SubRip
            or MatroskaTrackType.SubStationAlpha or MatroskaTrackType.SubStationAlpha2
            or MatroskaTrackType.AdvancedSubStationAlpha or MatroskaTrackType.AdvancedSubStationAlpha2
            or MatroskaTrackType.WebVTT or MatroskaTrackType.WebVTT2)
        {
            var sub = new Subtitle();
            Utilities.LoadMatroskaTextSubtitle(trackInfo, matroskaFile, subtitles, sub);
            count = sub.Paragraphs.Count;
            foreach (var p in sub.Paragraphs)
            {
                cues.Add(new PreviewCueData
                {
                    Number = p.Number,
                    Text = p.Text,
                    Show = TimeSpan.FromMilliseconds(p.StartTime.TotalMilliseconds),
                    Duration = TimeSpan.FromMilliseconds(p.EndTime.TotalMilliseconds - p.StartTime.TotalMilliseconds),
                });
            }
        }
        else if (trackInfo.CodecId == MatroskaTrackType.BluRay)
        {
            var pcsData = BluRaySupParser.ParseBluRaySupFromMatroska(trackInfo, matroskaFile);
            count = pcsData.Count;
            for (var i = 0; i < 20 && i < pcsData.Count; i++)
            {
                var item = pcsData[i];
                cues.Add(new PreviewCueData
                {
                    Number = i + 1,
                    Show = TimeSpan.FromMilliseconds(item.StartTimeCode.TotalMilliseconds),
                    Duration = TimeSpan.FromMilliseconds(item.EndTimeCode.TotalMilliseconds - item.StartTimeCode.TotalMilliseconds),
                    Image = item.GetBitmap().ToAvaloniaBitmap(),
                });
            }
        }
        else if (trackInfo.CodecId == MatroskaTrackType.TextSt)
        {
            var subtitle = new Subtitle();
            Utilities.LoadMatroskaTextSubtitle(trackInfo, matroskaFile, subtitles, subtitle);
            Utilities.ParseMatroskaTextSt(trackInfo, subtitles, subtitle);
            count = subtitle.Paragraphs.Count;
            for (var i = 0; i < 20 && i < subtitle.Paragraphs.Count; i++)
            {
                var item = subtitle.Paragraphs[i];
                cues.Add(new PreviewCueData
                {
                    Number = i + 1,
                    Show = item.StartTime.TimeSpan,
                    Duration = TimeSpan.FromMilliseconds(item.EndTime.TotalMilliseconds - item.StartTime.TotalMilliseconds),
                    Text = item.Text,
                });
            }
        }

        return new PreviewResult(count, cues);
    }

    private sealed record PreviewResult(int Count, List<PreviewCueData> Cues);

    private sealed class PreviewCueData
    {
        public int Number { get; init; }
        public TimeSpan Show { get; init; }
        public TimeSpan Duration { get; init; }
        public string? Text { get; init; }
        public Bitmap? Image { get; init; }
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
            _ = TrackChangedAsync();
        }, DispatcherPriority.Background);
    }
}
