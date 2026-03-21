using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.BatchErrorList;

public class BatchErrorListItem
{
    public string FileName { get; set; }
    public int Number { get; set; }
    public string Text { get; set; }
    public string Error { get; set; }
    public SubtitleLineViewModel Subtitle { get; set; }

    public BatchErrorListItem(string fileName, SubtitleLineViewModel subtitle, Paragraph? prev, Paragraph? next)
    {
        FileName = fileName;
        Subtitle = subtitle;
        Text = subtitle.Text;
        Number = subtitle.Number;
        var format = new SubRip();
        var previousSubtitleLineViewModel = prev != null ? new SubtitleLineViewModel(prev, format) : null;
        var nextSubtitleLineViewModel = next != null ? new SubtitleLineViewModel(next, format) : null;
        Error = subtitle.GetErrors(previousSubtitleLineViewModel, nextSubtitleLineViewModel);
    }
}
