using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Features.Main;

namespace Nikse.SubtitleEdit.Features.Tools.SplitBreakLongLines;

public partial class SplitBreakLongLinesItem : ObservableObject
{
    public string Name { get; set; }
    public int Number { get; set; }
    public string Fix { get; set; }
    public SubtitleLineViewModel SubtitleLine { get; set; }

    public bool IsSelectable { get; }
    public string OriginalText { get; }
    public string ProposedText { get; }

    [ObservableProperty]
    private bool _isSelected = true;

    public SplitBreakLongLinesItem(
        string name,
        int number,
        string fix,
        SubtitleLineViewModel subtitleLine,
        bool isSelectable = false,
        string? proposedText = null)
    {
        Name = name;
        Number = number;
        Fix = fix;
        SubtitleLine = subtitleLine;

        IsSelectable = isSelectable;
        OriginalText = subtitleLine.Text ?? string.Empty;
        ProposedText = proposedText ?? OriginalText;

        ApplySelection();
    }

    partial void OnIsSelectedChanged(bool value)
    {
        ApplySelection();
    }

    private void ApplySelection()
    {
        if (!IsSelectable)
        {
            return;
        }

        SubtitleLine.Text = IsSelected ? ProposedText : OriginalText;
    }
}
