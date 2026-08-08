using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Features.Shared.TextBoxUtils;

namespace UITests.Features;

/// <summary>
/// DeleteForward emulates the Delete key on the plain TextBox wrapper (the syntax editor has its
/// own native implementation): delete the selection, or the whole text element after the caret -
/// CRLF and surrogate pairs must never be split in half.
/// </summary>
public class TextBoxWrapperDeleteForwardTests
{
    private static TextBoxWrapper MakeWrapper(string text, int caretIndex, out TextBox textBox)
    {
        textBox = new TextBox { Text = text, CaretIndex = caretIndex };
        return new TextBoxWrapper(textBox);
    }

    [AvaloniaFact]
    public void DeletesCharacterAfterCaret()
    {
        var wrapper = MakeWrapper("abc", 1, out var textBox);

        wrapper.DeleteForward();

        Assert.Equal("ac", textBox.Text);
        Assert.Equal(1, textBox.CaretIndex);
    }

    [AvaloniaFact]
    public void AtEndOfTextDoesNothing()
    {
        var wrapper = MakeWrapper("abc", 3, out var textBox);

        wrapper.DeleteForward();

        Assert.Equal("abc", textBox.Text);
        Assert.Equal(3, textBox.CaretIndex);
    }

    [AvaloniaFact]
    public void DeletesSelectionInsteadOfCharacter()
    {
        var wrapper = MakeWrapper("abcdef", 0, out var textBox);
        textBox.SelectionStart = 1;
        textBox.SelectionEnd = 4;

        wrapper.DeleteForward();

        Assert.Equal("aef", textBox.Text);
    }

    [AvaloniaFact]
    public void DeletesCrLfAsOneStep()
    {
        var wrapper = MakeWrapper("a\r\nb", 1, out var textBox);

        wrapper.DeleteForward();

        Assert.Equal("ab", textBox.Text);
    }

    [AvaloniaFact]
    public void DeletesSurrogatePairAsOneStep()
    {
        var wrapper = MakeWrapper("a\U0001F600b", 1, out var textBox);

        wrapper.DeleteForward();

        Assert.Equal("ab", textBox.Text);
    }
}
