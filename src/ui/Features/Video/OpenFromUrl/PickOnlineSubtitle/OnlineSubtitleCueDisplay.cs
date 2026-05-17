using System;

namespace Nikse.SubtitleEdit.Features.Video.OpenFromUrl.PickOnlineSubtitle;

/// <summary>
/// DataGrid row for the preview pane in the picker — one entry per parsed cue.
/// </summary>
public class OnlineSubtitleCueDisplay
{
    public int Number { get; set; }
    public TimeSpan Show { get; set; }
    public TimeSpan Duration { get; set; }
    public string Text { get; set; } = string.Empty;
}
