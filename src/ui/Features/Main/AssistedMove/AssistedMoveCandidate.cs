namespace Nikse.SubtitleEdit.Features.Main.AssistedMove;

public enum AssistedMoveKind
{
    WithPrevious,
    WithNext,
    WithinSubtitle,
}

public class AssistedMoveCandidate
{
    public int Number { get; set; }
    public string Title { get; set; } = string.Empty;
    public AssistedMoveKind Kind { get; set; }

    public string NewCurrentText { get; set; } = string.Empty;
    public string NewOtherText { get; set; } = string.Empty;

    /// <summary>
    /// For cross-subtitle moves the boundary time moves with the text: the earlier
    /// subtitle's new end and the later subtitle's new start, split proportionally to the
    /// new text lengths (keeping the original gap). Null for within-subtitle moves.
    /// </summary>
    public System.TimeSpan? NewFirstEnd { get; set; }
    public System.TimeSpan? NewSecondStart { get; set; }

    public string FirstText { get; set; } = string.Empty;
    public string SecondText { get; set; } = string.Empty;
    public string FirstInfo { get; set; } = string.Empty;
    public string SecondInfo { get; set; } = string.Empty;
}
