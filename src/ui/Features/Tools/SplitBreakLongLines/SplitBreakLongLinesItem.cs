using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Features.Main;

namespace Nikse.SubtitleEdit.Features.Tools.SplitBreakLongLines;

public partial class SplitBreakLongLinesItem : ObservableObject
{
    public string Name { get; }
    public int Number { get; }
    public string Fix { get; }
    public SubtitleLineViewModel SubtitleLine { get; }

    /// <summary>
    /// Rebalance fixes are opt-out per row: the proposed text is applied while the row is
    /// checked and the original text restored when it is unchecked. Split fixes change the
    /// subtitle count and are always applied, so they show no checkbox.
    /// </summary>
    public bool IsSelectable { get; }

    private readonly string _originalText;
    private readonly string _proposedText;
    private readonly string _originalMarginV;
    private readonly string _proposedMarginV;

    [ObservableProperty] private bool _isSelected = true;

    public SplitBreakLongLinesItem(string name, int number, string fix, SubtitleLineViewModel subtitleLine)
        : this(name, number, fix, subtitleLine, null)
    {
    }

    /// <param name="proposedText">Rebalanced text, or null for a fix that is not optional.</param>
    /// <param name="proposedMarginV">Teletext row that goes with the proposed text (EBU STL only).</param>
    public SplitBreakLongLinesItem(string name, int number, string fix, SubtitleLineViewModel subtitleLine, string? proposedText, string? proposedMarginV = null)
    {
        Name = name;
        Number = number;
        Fix = fix;
        SubtitleLine = subtitleLine;

        _originalText = subtitleLine.Text ?? string.Empty;
        _proposedText = proposedText ?? _originalText;
        _originalMarginV = subtitleLine.MarginV;
        _proposedMarginV = proposedMarginV ?? _originalMarginV;
        IsSelectable = proposedText != null;

        ApplySelection();
    }

    partial void OnIsSelectedChanged(bool value)
    {
        ApplySelection();
    }

    private void ApplySelection()
    {
        if (IsSelectable)
        {
            SubtitleLine.Text = IsSelected ? _proposedText : _originalText;
            SubtitleLine.MarginV = IsSelected ? _proposedMarginV : _originalMarginV;
        }
    }
}
