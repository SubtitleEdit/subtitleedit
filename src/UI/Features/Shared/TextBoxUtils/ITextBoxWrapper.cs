using Avalonia.Controls;

namespace Nikse.SubtitleEdit.Features.Shared.TextBoxUtils;

public interface ITextBoxWrapper
{
    string Text { get; set; }
    string SelectedText { get; set; }
    int SelectionStart { get; set; }
    int SelectionLength { get; set; }
    int SelectionEnd { get; set; }
    void Select(int start, int length);
    int CaretIndex { get; set; }
    void Focus();
    Control TextControl { get; }
    Control ContentControl { get; }
    bool IsFocused { get; }
    void Cut();
    void Copy();
    void Paste();
    void SelectAll();
    void ClearSelection();
    void SetAlignment(Avalonia.Media.TextAlignment alignment);
}