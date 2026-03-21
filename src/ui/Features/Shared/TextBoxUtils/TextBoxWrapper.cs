using Avalonia.Controls;

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
}