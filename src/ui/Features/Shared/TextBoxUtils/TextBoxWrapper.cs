using Avalonia.Controls;
using Avalonia.Input;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Features.SpellCheck;
using Nikse.SubtitleEdit.Logic;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Shared.TextBoxUtils;

public class TextBoxWrapper : ITextBoxWrapper
{
    private readonly TextBox _textBox;

    public TextBoxWrapper(TextBox textBox)
    {
        _textBox = textBox;
    }

    public string Text
    {
        get
        {
            if (_textBox.Text != null)
            {
                return _textBox.Text;
            }

            return string.Empty;
        }
        set => _textBox.Text = value;
    }

    public string SelectedText
    {
        get => _textBox.SelectedText;    
        set => _textBox.SelectedText = value;
    }

    public int SelectionStart
    {
        get => _textBox.SelectionStart;
        set => _textBox.SelectionStart = value;
    }

    public int SelectionLength
    {
        get => _textBox.SelectionEnd - _textBox.SelectionStart;
        set => _textBox.SelectionEnd = _textBox.SelectionStart + value;
    }
    
    public int SelectionEnd
    {
        get => _textBox.SelectionEnd;
        set => _textBox.SelectionEnd =  value;
    }

    public void Select(int start, int length)
    {
        _textBox.SelectionStart = start;
        _textBox.SelectionEnd = start + length;
    }

    public int CaretIndex
    {
        get => _textBox.CaretIndex;
        set => _textBox.CaretIndex = value;
    }

    public void Focus()
    {
        _textBox.Focus();
    }

    public Control TextControl => _textBox;
    public Control ContentControl => _textBox;

    public bool IsFocused => _textBox.IsFocused;
    public void Cut()
    {
        _textBox.Cut();
    }

    public void Copy()
    {
        _textBox.Copy();
    }

    public void Paste()
    {
        _textBox.Paste();
    }

    public void SelectAll()
    {
        _textBox.SelectAll();
    }

    public void ClearSelection()
    {
        _textBox.ClearSelection();
    }

    public void SetAlignment(Avalonia.Media.TextAlignment alignment)
    {
        _textBox.TextAlignment = alignment;
    }

    // Live spell check works when the wrapped text box is a SyntaxHighlightingTextBox (the
    // "Color tags" subtitle edit box); for a plain TextBox these are no-ops so the caller
    // does not have to care which editor is active.

    public void EnableSpellCheck(ISpellCheckManager spellCheckManager)
    {
        if (_textBox is SyntaxHighlightingTextBox box)
        {
            box.EnableSpellCheck(spellCheckManager);
        }
    }

    public void DisableSpellCheck()
    {
        if (_textBox is SyntaxHighlightingTextBox box)
        {
            box.DisableSpellCheck();
        }
    }

    public void RefreshSpellCheck()
    {
        if (_textBox is SyntaxHighlightingTextBox box)
        {
            box.RefreshSpellCheck();
        }
    }

    public bool IsSpellCheckEnabled => _textBox is SyntaxHighlightingTextBox { IsSpellCheckEnabled: true };

    public SpellCheckWord? GetWordAtOffset(int offset)
    {
        var text = Text;
        if (string.IsNullOrEmpty(text) || offset < 0 || offset > text.Length)
        {
            return null;
        }

        return SpellCheckWordLists.Split(text).FirstOrDefault(w => offset >= w.Index && offset < w.Index + w.Length);
    }

    public SpellCheckWord? GetWordAtPosition(PointerEventArgs e)
    {
        if (_textBox is not SyntaxHighlightingTextBox box)
        {
            return null;
        }

        var index = box.GetCharIndexAtPoint(e);
        return index == null ? null : GetWordAtOffset(index.Value);
    }

    public bool IsWordMisspelledAtOffset(int offset)
    {
        if (_textBox is not SyntaxHighlightingTextBox { IsSpellCheckEnabled: true, SpellCheckManager: { } spellCheckManager })
        {
            return false;
        }

        var word = GetWordAtOffset(offset);
        if (word == null || string.IsNullOrWhiteSpace(word.Text))
        {
            return false;
        }

        return SpellCheckUnderlineTransformer.IsWordMisspelled(word, Text, spellCheckManager);
    }

    public List<string>? GetSuggestionsForWordAtOffset(int offset)
    {
        if (_textBox is not SyntaxHighlightingTextBox { SpellCheckManager: { } spellCheckManager })
        {
            return null;
        }

        var word = GetWordAtOffset(offset);
        if (word == null || string.IsNullOrWhiteSpace(word.Text))
        {
            return null;
        }

        return spellCheckManager.GetSuggestions(word.Text);
    }
}