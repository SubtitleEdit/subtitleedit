using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Shared.PickMatroskaTrack;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Features.Video.EmbeddedSubtitlesEdit;

public partial class EmbedTrackPreviewViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<MatroskaSubtitleCueDisplay> _rows;

    public Window? Window { get; set; }
    public MatroskaTrackInfo? SelectedMatroskaTrack { get; set; }
    public bool OkPressed { get; private set; }
    public string WindowTitle { get; private set; }

    private MatroskaTrackInfo? _matroskaTrack;
    private MatroskaFile? _matroskaFile;
    private string _videoFileName;
    private string _subtitleFileName;

    public EmbedTrackPreviewViewModel()
    {
        WindowTitle = string.Empty;
        Rows = new ObservableCollection<MatroskaSubtitleCueDisplay>();
        _matroskaTrack = new MatroskaTrackInfo();
        _videoFileName = string.Empty;
        _subtitleFileName = string.Empty;
    }

    public void Initialize(MatroskaFile matroskaFile, MatroskaTrackInfo? matroskaTrack, string videoFileName, string subtitleFileName)
    {
        _matroskaFile = matroskaFile;
        _matroskaTrack = matroskaTrack;
        _videoFileName = videoFileName;
        _subtitleFileName = subtitleFileName;
        WindowTitle = string.Format(Se.Language.Video.ViewMatroskaTrackX, videoFileName);
    }

    private void Close()
    {
        Dispatcher.UIThread.Post(() => { Window?.Close(); });
    }

    [RelayCommand]
    private void Ok()
    {
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
    }

    private bool InitTrack()
    {
        Rows.Clear();

        if (!string.IsNullOrEmpty(_subtitleFileName))
        {
            if (_subtitleFileName.EndsWith(".sup", StringComparison.OrdinalIgnoreCase) && FileUtil.IsBluRaySup(_subtitleFileName))
            {
                var pcsData = BluRaySupParser.ParseBluRaySup(_subtitleFileName, new System.Text.StringBuilder());
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

                return pcsData.Count > 0;
            }

            var subtitle = Subtitle.Parse(_subtitleFileName);
            if (subtitle != null)
            {
                for (var i = 0; i < subtitle.Paragraphs.Count; i++)
                {
                    var item = subtitle.Paragraphs[i];
                    var cue = new MatroskaSubtitleCueDisplay()
                    {
                        Number = i + 1,
                        Show = TimeSpan.FromMilliseconds(item.StartTime.TotalMilliseconds),
                        Duration = TimeSpan.FromMilliseconds(item.EndTime.TotalMilliseconds - item.StartTime.TotalMilliseconds),
                        Text = item.Text,
                    };
                    Rows.Add(cue);
                }
                return true;
            }

            return false;
        }

        var selectedTrack = _matroskaTrack;
        if (selectedTrack == null)
        {
            return false;
        }

        var trackInfo = selectedTrack;
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

    internal void Loaded()
    {
        InitTrack();
    }
}