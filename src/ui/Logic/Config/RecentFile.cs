namespace Nikse.SubtitleEdit.Logic.Config;

public class RecentFile
{
    public string SubtitleFileName { get; set; } = string.Empty;
    public string SubtitleFileNameOriginal { get; set; } = string.Empty;
    public string SubtitleFileNameSecondary { get; set; } = string.Empty;
    public string VideoFileName { get; set; } = string.Empty;
    public int SelectedLine { get; set; }
    public string Encoding { get; set; } = string.Empty;
    public long VideoOffsetInMs { get; set; }
    public bool VideoIsSmpte { get; set; }
    public int AudioTrack { get; set; } = -1;

    /// <summary>"Use empty waveform" was picked for this subtitle and video: reopening them shows
    /// the subtitles on the empty waveform again instead of the "Generate waveform" buttons.</summary>
    public bool UseEmptyWaveform { get; set; }

    /// <summary>The original did not line up 1:1 with the subtitle, and this was the "Show all original
    /// lines" choice made for it: reopening the pair applies it again instead of asking (#16752).
    /// Null when the question was never asked.</summary>
    public bool? OriginalShowAllLines { get; set; }

    /// <summary>The "Allow edit of original subtitle" choice that goes with <see cref="OriginalShowAllLines"/>.</summary>
    public bool? OriginalAllowEdit { get; set; }
}