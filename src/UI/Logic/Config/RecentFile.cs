namespace Nikse.SubtitleEdit.Logic.Config;

public class RecentFile
{
    public string SubtitleFileName { get; set; } = string.Empty;
    public string SubtitleFileNameOriginal { get; set; } = string.Empty;
    public string VideoFileName { get; set; } = string.Empty;
    public int SelectedLine { get; set; }
    public string Encoding { get; set; } = string.Empty;
    public long VideoOffsetInMs { get; set; }
    public bool VideoIsSmpte { get; set; }
    public int AudioTrack { get; set; } = -1;
}