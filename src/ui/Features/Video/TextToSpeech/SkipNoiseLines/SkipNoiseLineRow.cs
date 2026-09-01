using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.SkipNoiseLines;

public class SkipNoiseLineRow : SelectLinesRowBase
{
    public Paragraph Paragraph { get; }

    public SkipNoiseLineRow(Paragraph paragraph)
        // Every detected line starts checked - the whole point of the dialog is skipping them,
        // unchecking is for the occasional false positive.
        : base(true, paragraph.Number, paragraph.StartTime.ToDisplayString(), paragraph.Text)
    {
        Paragraph = paragraph;
    }
}
