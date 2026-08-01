using Nikse.SubtitleEdit.Features.Main;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.BatchErrorList;

public class BatchErrorListItem
{
    public string FileName { get; set; }
    public int Number { get; set; }
    public string Text { get; set; }
    public string Error { get; set; }
    public SubtitleLineViewModel Subtitle { get; set; }

    public BatchErrorListItem(string fileName, SubtitleLineViewModel subtitle, SubtitleLineViewModel? prev, SubtitleLineViewModel? next)
    {
        FileName = fileName;
        Subtitle = subtitle;
        Text = subtitle.Text;
        Number = subtitle.Number;
        Error = subtitle.GetErrors(prev, next);
    }
}
