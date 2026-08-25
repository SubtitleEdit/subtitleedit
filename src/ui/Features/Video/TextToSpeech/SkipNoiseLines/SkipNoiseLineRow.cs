using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.SkipNoiseLines;

public partial class SkipNoiseLineRow : ObservableObject
{
    [ObservableProperty] private bool _isSelected;

    public int Number { get; }
    public string Show { get; }
    public string Text { get; }
    public Paragraph Paragraph { get; }

    public SkipNoiseLineRow(Paragraph paragraph)
    {
        // Every detected line starts checked - the whole point of the dialog is skipping them,
        // unchecking is for the occasional false positive.
        IsSelected = true;
        Number = paragraph.Number;
        Show = paragraph.StartTime.ToDisplayString();
        Text = paragraph.Text;
        Paragraph = paragraph;
    }
}
