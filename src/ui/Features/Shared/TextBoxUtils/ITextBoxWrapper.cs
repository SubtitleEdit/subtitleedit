using Avalonia.Controls;
using Avalonia.Input;
using Nikse.SubtitleEdit.Features.SpellCheck;
using System.Collections.Generic;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;

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

    // Live spell check - a no-op for editors without underline support (plain TextBox).
    void EnableSpellCheck(ISpellCheckManager spellCheckManager);
    void DisableSpellCheck();
    void RefreshSpellCheck();
    bool IsSpellCheckEnabled { get; }
    SpellCheckWord? GetWordAtPosition(PointerEventArgs e);
    bool IsWordMisspelledAtOffset(int offset);
    List<string>? GetSuggestionsForWordAtOffset(int offset);
}
