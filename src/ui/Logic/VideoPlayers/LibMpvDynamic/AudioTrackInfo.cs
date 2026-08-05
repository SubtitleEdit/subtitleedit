using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;

public class AudioTrackInfo
{
    public int Id { get; set; }
    public string? Language { get; set; }
    public string? Title { get; set; }
    public bool IsSelected { get; set; }

    /// <summary>True when the container marks this track as the default track.</summary>
    public bool IsDefault { get; set; }
    public int? FfIndex { get; set; }

    /// <summary>Codec name as reported by mpv (e.g. "truehd", "ac3", "aac"), or null if unknown.</summary>
    public string? Codec { get; set; }

    /// <summary>Number of audio channels (e.g. 8 for 7.1, 6 for 5.1, 2 for stereo), or null if unknown.</summary>
    public int? Channels { get; set; }

    public override string ToString()
    {
        var parts = new List<string>();
        
        if (!string.IsNullOrWhiteSpace(Title))
        {
            parts.Add(Title);
        }
        
        if (!string.IsNullOrWhiteSpace(Language))
        {
            parts.Add($"[{Language}]");
        }
        
        parts.Add($"(ID: {Id})");
        
        if (IsSelected)
        {
            parts.Add("*");
        }
        
        return string.Join(" ", parts);
    }
}
