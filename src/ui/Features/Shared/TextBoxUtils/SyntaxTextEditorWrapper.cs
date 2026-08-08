using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Nikse.SubtitleEdit.Controls.SyntaxTextEditorControl;
using Nikse.SubtitleEdit.Features.SpellCheck;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Shared.TextBoxUtils;

/// <summary>
/// Adapts the virtualizing <see cref="SyntaxTextEditor"/> to <see cref="ITextBoxWrapper"/> so the
/// source view and batch convert ASSA can use it like any other editor here.
///
/// Live spell check is not wired up: it exists for the subtitle edit box, and the source views
/// never turn it on. The methods are the same no-ops a plain text box gets.
/// </summary>
public class SyntaxTextEditorWrapper : ITextBoxWrapper
{
    private readonly SyntaxTextEditor _editor;
    private readonly Control _contentControl;

    public SyntaxTextEditorWrapper(SyntaxTextEditor editor, Control? contentControl = null)
    {
        _editor = editor;
        _contentControl = contentControl ?? editor;
    }

    public bool HasFocus { get; set; }

    public string Text
    {
        get => _editor.Text;
        set => _editor.Text = value;
    }

    public string SelectedText
    {
        get => _editor.SelectedText;
        set => _editor.InsertText(value);
    }

    public int SelectionStart
    {
        get => _editor.SelectionStart;
        set => _editor.Select(value, SelectionLength);
    }

    public int SelectionLength
    {
        get => _editor.SelectionLength;
        set => _editor.Select(SelectionStart, value);
    }

    public int SelectionEnd
    {
        get => _editor.SelectionStart + _editor.SelectionLength;
        set => _editor.Select(SelectionStart, Math.Max(0, value - SelectionStart));
    }

    public void Select(int start, int length) => _editor.Select(start, length);

    public int CaretIndex
    {
        get => _editor.CaretOffset;
        set => _editor.CaretOffset = value;
    }

    public void Focus() => _editor.View.Focus();

    public Control TextControl => _editor.View;

    public Control ContentControl => _contentControl;

    public bool IsFocused => _editor.View.IsFocused;

    public void Cut() => _editor.Cut();

    public void Copy() => _editor.Copy();

    public void Paste() => _editor.Paste();

    public void SelectAll() => _editor.SelectAll();

    public void ClearSelection() => _editor.ClearSelection();

    public void DeleteForward() => _editor.View.DeleteForward();

    /// <summary>
    /// Source text is always left aligned - centering it would only make the time codes harder to
    /// read. The subtitle edit box is the one that follows this setting.
    /// </summary>
    public void SetAlignment(TextAlignment alignment)
    {
    }

    public void EnableSpellCheck(ISpellCheckManager spellCheckManager)
    {
    }

    public void DisableSpellCheck()
    {
    }

    public void RefreshSpellCheck()
    {
    }

    public bool IsSpellCheckEnabled => false;

    public SpellCheckWord? GetWordAtPosition(PointerEventArgs e) => null;

    public bool IsWordMisspelledAtOffset(int offset) => false;

    public List<string>? GetSuggestionsForWordAtOffset(int offset) => null;
}
