using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream;
using Nikse.SubtitleEdit.Features.Ocr;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Shared.PickTsTrack;

public partial class PickTsTrackViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<TsTrackInfoDisplay> _tracks;
    [ObservableProperty] private TsTrackInfoDisplay? _selectedTrack;
    [ObservableProperty] private ObservableCollection<TsSubtitleCueDisplay> _rows;

    public Window? Window { get; set; }
    public DataGrid TracksGrid { get; set; }
    public bool OkPressed { get; private set; }
    public string WindowTitle { get; private set; }
    public Subtitle TeletextSubtitle { get; private set; }

    private string _fileName = string.Empty;
    private TransportStreamParser? _tsParser;

    public PickTsTrackViewModel()
    {
        Tracks = new ObservableCollection<TsTrackInfoDisplay>();
        TracksGrid = new DataGrid();
        WindowTitle = string.Empty;
        Rows = new ObservableCollection<TsSubtitleCueDisplay>();
        TeletextSubtitle = new Subtitle();
    }

    internal void Initialize(TransportStreamParser tsParser, string fileName)
    {
        _tsParser = tsParser;

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
        if (selectedTrack == null || _tsParser == null)
        {
            return false;
        }

        Rows.Clear();

        if (selectedTrack.IsTeletext)
        {
           var subtitle = new Subtitle(selectedTrack.Teletext);          
            subtitle.Renumber();
            TeletextSubtitle = subtitle;
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

        var subtitles = _tsParser.GetDvbSubtitles(selectedTrack.TrackNumber);
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
            TracksGrid.SelectedIndex = index;
            TracksGrid.ScrollIntoView(TracksGrid.SelectedItem, null);
            TrackChanged();
        }, DispatcherPriority.Background);
    }
}