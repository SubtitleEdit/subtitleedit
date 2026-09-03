namespace Nikse.SubtitleEdit.Features.Files.ExportDvbTeletext;

/// <summary>
/// One entry of the "Subtitle type" drop-down: plain subtitles or subtitles for the hearing
/// impaired, the two DVB teletext descriptor types a subtitle page can be announced as.
/// </summary>
public class DvbTeletextSubtitleTypeItem
{
    public string Name { get; }
    public bool IsHearingImpaired { get; }

    public DvbTeletextSubtitleTypeItem(string name, bool isHearingImpaired)
    {
        Name = name;
        IsHearingImpaired = isHearingImpaired;
    }

    public override string ToString()
    {
        return Name;
    }
}
