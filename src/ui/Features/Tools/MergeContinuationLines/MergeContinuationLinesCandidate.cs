using CommunityToolkit.Mvvm.ComponentModel;

namespace Nikse.SubtitleEdit.Features.Tools.MergeContinuationLines;

public partial class MergeContinuationLinesCandidate : ObservableObject
{
    [ObservableProperty] private bool _isSelected;

    public int Index { get; set; }
    public int Number { get; set; }
    public int NextNumber { get; set; }
    public string Text1 { get; set; } = string.Empty;
    public string Text2 { get; set; } = string.Empty;
    public string MergedText { get; set; } = string.Empty;
    public string MergedTextDisplay => (MergedText ?? string.Empty).Replace("\r\n", " ⏎ ").Replace("\n", " ⏎ ");
}
