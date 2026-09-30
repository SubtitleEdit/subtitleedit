using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Cea608;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.ProgramStream;
using Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream;
using Nikse.SubtitleEdit.Features.Ocr;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Shared.PickTsTrack;

public partial class PickTsTrackViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<TsTrackInfoDisplay> _tracks;
    [ObservableProperty] private TsTrackInfoDisplay? _selectedTrack;
    [ObservableProperty] private ObservableCollection<TsSubtitleCueDisplay> _rows;
    [ObservableProperty] private string _subtitleCountText;

    public Window? Window { get; set; }
    public TableView TracksGrid { get; set; }
    public bool OkPressed { get; private set; }
    public string WindowTitle { get; private set; }
    public Subtitle TeletextSubtitle { get; private set; }

    private string _fileName = string.Empty;
    private TransportStreamParser? _tsParser;
    private SortedDictionary<int, List<UmdVideoSubtitle>>? _umdTracks;

    public PickTsTrackViewModel()
    {
        Tracks = new ObservableCollection<TsTrackInfoDisplay>();
        TracksGrid = new TableView();
        WindowTitle = string.Empty;
        SubtitleCountText = string.Empty;
        Rows = new ObservableCollection<TsSubtitleCueDisplay>();
        TeletextSubtitle = new Subtitle();
    }

    internal void Initialize(TransportStreamParser tsParser, string fileName)
    {
        _tsParser = tsParser;
        _fileName = fileName;
        WindowTitle = UiUtil.FormatTitleWithFileName(Se.Language.File.PickTransportStreamTrackX, fileName);

        var programMapTableParser = new ProgramMapTableParser();
        programMapTableParser.Parse(fileName); // get languages

        for (var i = 0; i < tsParser.SubtitlePacketIds.Count; i++)
        {
            var pid = tsParser.SubtitlePacketIds[i];

            var language = string.Empty;
            if (programMapTableParser.GetSubtitlePacketIds().Count > 0)
            {
                language = programMapTableParser.GetSubtitleLanguage(pid);
            }

            var display = new TsTrackInfoDisplay
            {
                TrackNumber = pid,
                IsDefault = false,
                IsForced = false,
                Language = language,
                IsTeletext = false,
            };
            Tracks.Add(display);
        }


        foreach (var i in tsParser.TeletextSubtitlesLookup.Keys)
        {
            var pid = tsParser.TeletextSubtitlesLookup[i];
            var display = new TsTrackInfoDisplay
            {
                TrackNumber = i,
                Teletext = pid.Values.First(),
                IsDefault = false,
                IsForced = false,
                Codec = "Teletext",
                IsTeletext = true,
            };
            Tracks.Add(display);
        }

        foreach (var aribPid in tsParser.AribSubtitlesLookup)
        {
            foreach (var language in aribPid.Value)
            {
                var languageCode = string.Empty;
                if (tsParser.AribLanguageLookup.TryGetValue(aribPid.Key, out var languageCodes))
                {
                    languageCodes.TryGetValue(language.Key, out languageCode);
                }

                var display = new TsTrackInfoDisplay
                {
                    TrackNumber = aribPid.Key,
                    Teletext = language.Value,
                    IsDefault = false,
                    IsForced = false,
                    Language = languageCode ?? string.Empty,
                    Codec = "ARIB caption",
                    IsTeletext = true, // text track - same preview/open handling as teletext
                };
                Tracks.Add(display);
            }
        }

        foreach (var videoPid in tsParser.ClosedCaptionSubtitlesLookup)
        {
            foreach (var track in videoPid.Value)
            {
                var display = new TsTrackInfoDisplay
                {
                    TrackNumber = videoPid.Key,
                    Teletext = track.Value,
                    IsDefault = false,
                    IsForced = false,
                    Codec = ClosedCaptionExtractor.GetTrackName(track.Key),
                    IsTeletext = true, // text track - same preview/open handling as teletext
                };
                Tracks.Add(display);
            }
        }
    }

    /// <summary>
    /// Teletext pages from a Manzanita dump, which is a single elementary stream and so has no
    /// program map table to take packet ids or languages from.
    /// </summary>
    internal void Initialize(Dictionary<int, List<Paragraph>> teletextPages, string fileName)
    {
        _fileName = fileName;
        WindowTitle = UiUtil.FormatTitleWithFileName(Se.Language.File.PickTransportStreamTrackX, fileName);

        foreach (var page in teletextPages)
        {
            Tracks.Add(new TsTrackInfoDisplay
            {
                TrackNumber = page.Key,
                Teletext = page.Value,
                Codec = "Teletext",
                IsTeletext = true,
            });
        }
    }

    /// <summary>
    /// CEA-608/708 closed caption tracks read from the video track of an .mp4 or .mkv file.
    /// </summary>
    /// <param name="captionTracks">Paragraphs per track key (see <see cref="ClosedCaptionDecoder"/>)</param>
    /// <param name="trackNumber">Video track the captions came from</param>
    /// <param name="windowTitle">Window title</param>
    internal void InitializeClosedCaptions(SortedDictionary<int, List<Paragraph>> captionTracks, int trackNumber, string windowTitle)
    {
        WindowTitle = windowTitle;
        foreach (var track in captionTracks)
        {
            Tracks.Add(new TsTrackInfoDisplay
            {
                TrackNumber = trackNumber,
                Teletext = track.Value,
                Codec = ClosedCaptionDecoder.GetTrackName(track.Key),
                IsTeletext = true, // text track - same preview/open handling as teletext
            });
        }
    }

    /// <summary>
    /// PSP UMD Video: one image subtitle stream per sub-stream id (0x80 = the first).
    /// </summary>
    internal void InitializeUmdVideo(SortedDictionary<int, List<UmdVideoSubtitle>> tracks, string fileName)
    {
        _umdTracks = tracks;
        _fileName = fileName;
        WindowTitle = UiUtil.FormatTitleWithFileName(Se.Language.File.PickMpegTrackX, fileName);
        foreach (var track in tracks)
        {
            Tracks.Add(new TsTrackInfoDisplay
            {
                TrackNumber = track.Key,
                Codec = "PNG",
                Name = "#" + (track.Key - 0x80 + 1),
            });
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

    internal void TracksGridSelectionChanged(object? sender, SelectionChangedEventArgs e)
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
        if (selectedTrack == null)
        {
            SubtitleCountText = string.Empty;
            return false;
        }

        Rows.Clear();

        if (selectedTrack.IsTeletext)
        {
           var subtitle = new Subtitle(selectedTrack.Teletext);
            subtitle.Renumber();
            TeletextSubtitle = subtitle;
            SubtitleCountText = string.Format(Se.Language.File.Import.NumberOfSubtitlesX, subtitle.Paragraphs.Count.ToString("N0"));
            foreach (var p in subtitle.Paragraphs.Take(20))
            {
                var cue = new TsSubtitleCueDisplay()
                {
                    Number = p.Number,
                    Show = p.StartTime.TimeSpan,
                    Hide = p.EndTime.TimeSpan,
                    Duration = p.Duration.TimeSpan,
                    Text = p.Text,
                };
                Rows.Add(cue);
            }

            return true;
        }

        if (_umdTracks != null && _umdTracks.TryGetValue(selectedTrack.TrackNumber, out var pictures))
        {
            SubtitleCountText = string.Format(Se.Language.File.Import.NumberOfSubtitlesX, pictures.Count.ToString("N0"));
            for (var i = 0; i < 20 && i < pictures.Count; i++)
            {
                var picture = pictures[i];
                using var bitmap = picture.GetBitmap();
                Rows.Add(new TsSubtitleCueDisplay
                {
                    Number = i + 1,
                    Show = picture.StartTime,
                    Hide = picture.EndTime,
                    Duration = picture.EndTime - picture.StartTime,
                    Image = new Image { Source = bitmap.ToAvaloniaBitmap() },
                });
            }

            return true;
        }

        if (_tsParser == null)
        {
            SubtitleCountText = string.Empty;
            return false;
        }

        // GetDvbSubtitles returns null for a packet id it decoded no images for - a subtitle PID
        // announced by the stream is not a guarantee that anything came out of it.
        var subtitles = _tsParser.GetDvbSubtitles(selectedTrack.TrackNumber);
        if (subtitles == null)
        {
            SubtitleCountText = string.Empty;
            return false;
        }

        SubtitleCountText = string.Format(Se.Language.File.Import.NumberOfSubtitlesX, subtitles.Count.ToString("N0"));
        for (var i = 0; i < 20 && i < subtitles.Count; i++)
        {
            var item = subtitles[i];
            var cue = new TsSubtitleCueDisplay()
            {
                Number = i + 1,
                Show = TimeSpan.FromMilliseconds(item.StartMilliseconds),
                Hide = TimeSpan.FromMilliseconds(item.EndMilliseconds),
                Duration = TimeSpan.FromMilliseconds(item.EndMilliseconds - item.StartMilliseconds),
                Image = new Image { Source = PreProcessingSettings.CropTransparent(item.GetBitmap()).ToAvaloniaBitmap() },
            };
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
            // Select via the view model, not just the grid index - see the same fix in
            // PickMatroskaTrackViewModel: AlwaysSelected has already put the grid on row 0, so
            // re-assigning the index raises no SelectionChanged and SelectedTrack stayed null,
            // which left the preview empty and made OK return no track at all.
            SelectedTrack = Tracks[index];
            TracksGrid.SelectedIndex = index;
            if (TracksGrid.SelectedItem is { } selectedItem)
            {
                TracksGrid.ScrollIntoView(selectedItem);
            }

            TrackChanged();
        }, DispatcherPriority.Background);
    }
}