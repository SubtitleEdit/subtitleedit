using Nikse.SubtitleEdit.Features.Main;
using System;

namespace Nikse.SubtitleEdit.Features.Edit.ModifySelection;

public class PreviewItem
{
    public bool Apply { get; set; }
    public int Number { get; set; }
    public TimeSpan Show { get; set; }
    public TimeSpan Duration { get; set; }
    public string Text { get; set; }
    public SubtitleLineViewModel Subtitle { get; set; }

    public PreviewItem(int number, bool apply, TimeSpan show, TimeSpan duration, string text, SubtitleLineViewModel subtitle)
    {
        Apply = apply;
        Number = number;
        Show = show;
        Duration = duration;
        Text = text;
        Subtitle = subtitle;
    }
}
