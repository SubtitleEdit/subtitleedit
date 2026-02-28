using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes;

namespace Nikse.SubtitleEdit.Features.Shared.PickMp4Track;

public class Mp4TrackInfoDisplay
{
    public string HandlerType { get; set; }
    public Trak? Track { get; set; }
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
