namespace Nikse.SubtitleEdit.Features.Video.OpenFromUrl.PickOnlineSubtitle;

/// <summary>
/// DataGrid row for a pre-downloaded subtitle file in the picker.
/// </summary>
public class OnlineSubtitleTrackDisplay
{
    public string Language { get; set; } = string.Empty;
    public string LanguageCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
}
