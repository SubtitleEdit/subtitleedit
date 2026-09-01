namespace Nikse.SubtitleEdit.Features.Main.AssistedSplit;

public class AssistedSplitCandidate
{
    public int Number { get; set; }
    public string Title { get; set; } = string.Empty;
    public int TextIndex { get; set; }
    public string FirstText { get; set; } = string.Empty;
    public string SecondText { get; set; } = string.Empty;
    public string FirstInfo { get; set; } = string.Empty;
    public string SecondInfo { get; set; } = string.Empty;
}
