using Nikse.SubtitleEdit.Features.Main;

namespace Nikse.SubtitleEdit.Features.Shared.ErrorList;

public class ErrorListItem
{
    public int Number { get; set; }
    public string Text { get; set; }
    public string Error { get; set; }
    public SubtitleLineViewModel Subtitle { get; set; }

    public ErrorListItem(SubtitleLineViewModel subtitle, SubtitleLineViewModel? prev, SubtitleLineViewModel? next)
    {
        Subtitle = subtitle;
        Text = subtitle.Text;
        Number = subtitle.Number;
        Error = subtitle.GetErrors(prev, next);
    }
}
