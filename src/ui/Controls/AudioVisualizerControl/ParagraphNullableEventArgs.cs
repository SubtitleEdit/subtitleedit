using Nikse.SubtitleEdit.Features.Main;

namespace Nikse.SubtitleEdit.Controls.AudioVisualizerControl;

public class ParagraphNullableEventArgs
{
    public SubtitleLineViewModel? Paragraph { get; }
    public double Seconds { get; }
    public MouseDownParagraphType MouseDownParagraphType { get; set; }
    public ParagraphNullableEventArgs(SubtitleLineViewModel? p)
    {
        if (p == null)
        {
            return;
        }

        Paragraph = new SubtitleLineViewModel(p);
    }

    public ParagraphNullableEventArgs(double seconds, SubtitleLineViewModel? p)
    {
        Seconds = seconds;
        if (p == null)
        {
            Paragraph = new SubtitleLineViewModel();
            return;
        }
        
        Paragraph = new SubtitleLineViewModel(p, false);
    }
}