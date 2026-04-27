using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Shared.PickTsTrack;

public class TsTrackInfoDisplay
{
    public int TrackNumber { get; set; }
    public bool IsDefault { get; set; }
    public bool IsForced { get; set; }
    public string Codec { get; set; }
    public string Language { get; set; }
    public string Name { get; set; }
    public bool IsTeletext { get; set; }
    public List<Paragraph> Teletext { get; set; }

    public TsTrackInfoDisplay()
    {
        Codec = string.Empty;
        Language = string.Empty;
        Name = string.Empty;
        Teletext = new List<Paragraph>();
    }
}
