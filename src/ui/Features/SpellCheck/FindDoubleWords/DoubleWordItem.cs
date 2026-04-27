using Nikse.SubtitleEdit.Features.Main;

namespace Nikse.SubtitleEdit.Features.SpellCheck.FindDoubleWords;

public class DoubleWordItem  
{
    public int Number { get; set; }
    public string Text { get; set; }
    public string Hit { get; set; }
    public SubtitleLineViewModel Subtitle { get; set; }

    public DoubleWordItem(SubtitleLineViewModel subtitle, string hit)
    {
        Subtitle = subtitle;
        Text = subtitle.Text;
        Number = subtitle.Number;
        Hit = hit;
    }
}
