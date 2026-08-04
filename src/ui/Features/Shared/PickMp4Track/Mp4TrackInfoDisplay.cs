using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4;
using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes;

namespace Nikse.SubtitleEdit.Features.Shared.PickMp4Track;

public class Mp4TrackInfoDisplay
{
    public string HandlerType { get; set; }

    /// <summary>Classic moov-based track (progressive MP4); null for fragmented tracks.</summary>
    public Trak? Track { get; set; }

    /// <summary>Fragmented (DASH/CMAF) track; null for moov-based tracks.</summary>
    public Mp4FragmentedSubtitleTrack? FragmentedTrack { get; set; }

    public string Name { get; internal set; }
    public ulong StartPosition { get; internal set; }
    public bool IsVobSubSubtitle { get; internal set; }
    public ulong Duration { get; internal set; }

    public Mp4TrackInfoDisplay()
    {
        HandlerType = string.Empty;
        Name = string.Empty;
    }
}
