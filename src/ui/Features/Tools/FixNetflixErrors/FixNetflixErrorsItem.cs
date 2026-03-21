using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.NetflixQualityCheck;

namespace Nikse.SubtitleEdit.Features.Tools.FixNetflixErrors;

public class FixNetflixErrorsItem
{
    public bool Apply { get; set; }
    public int Index { get; set; }
    public int IndexDisplay { get; set; }
    public string Before { get; set; }
    public string After { get; set; }
    public string Reason { get; set; }
    public Paragraph Paragraph { get; set; }
    public NetflixQualityController.Record Record { get; set; }
    public bool CanBeFixed { get; set; }

    public FixNetflixErrorsItem(bool apply, int index, string before, string after, Paragraph paragraph, NetflixQualityController.Record record)
    {
        Apply = apply;
        Index = index;
        IndexDisplay = index + 1;
        Before = before;
        After = after;
        Paragraph = paragraph;
        Record = record;
        Reason = record.Comment;
        CanBeFixed = record.CanBeFixed;
    }
}
