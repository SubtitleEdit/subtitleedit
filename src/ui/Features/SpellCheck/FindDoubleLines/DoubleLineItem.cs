using Nikse.SubtitleEdit.Features.Main;

namespace Nikse.SubtitleEdit.Features.SpellCheck.FindDoubleLines;

public class DoubleLineItem
{
    public int Number { get; set; }
    public string Text { get; set; }
    public SubtitleLineViewModel Subtitle { get; set; }

    public DoubleLineItem(SubtitleLineViewModel subtitle)
    {
        Subtitle = subtitle;
        Text = subtitle.Text;
        Number = subtitle.Number;
    }
}
