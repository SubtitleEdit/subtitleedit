using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;

namespace Nikse.SubtitleEdit.Features.Video.EmbeddedSubtitlesEdit;

public partial class EmbeddedTrack : ObservableObject
{
    [ObservableProperty] private string _format;
    [ObservableProperty] private string _languageOrTitle;
    [ObservableProperty] private string _name;
    [ObservableProperty] private bool _default;
    [ObservableProperty] private bool _forced;
    [ObservableProperty] private bool _deleted;

    public int Number { get; set; }
    public bool New { get; set; }
    public string FileName { get; set; } = string.Empty;
    public FfmpegTrackInfo? FfmpegTrackInfo { get; set; }
    public MatroskaTrackInfo? MatroskaTrackInfo { get; set; }

    public EmbeddedTrack()
    {
        Format = string.Empty;
        LanguageOrTitle = string.Empty;
        Name = string.Empty;
    }

    public EmbeddedTrack(EmbeddedTrack track)
    {
        Format = track.Format;
        LanguageOrTitle = track.LanguageOrTitle;
        Name = track.Name;
        Default = track.Default;
        Forced = track.Forced;
        Deleted = track.Deleted;
        New = track.New;
        FileName = track.FileName;
        FfmpegTrackInfo = track.FfmpegTrackInfo;
    }

    public override string ToString()
    {
        return Name;
    }
}
