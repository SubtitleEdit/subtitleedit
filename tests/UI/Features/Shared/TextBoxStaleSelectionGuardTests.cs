using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Features.Shared.TextBoxUtils;
using Fix = Nikse.SubtitleEdit.Features.Shared.TextBoxUtils.TextBoxStaleSelectionGuard.Fix;

namespace UITests.Features.Shared;

// A command that replaces the whole text (dialog dashes, surround, ...) must not leave the old
// selection range behind - the next "surround with" then lifted only part of the line (#17052).
public class TextBoxStaleSelectionGuardTests
{
    private static readonly string Line1 = "مرحبا بك";
    private static readonly string Line2 = "كيف حالك؟";

    private static TextBox MakeGuardedTextBox(string text, int selectionStart, int selectionEnd)
    {
        var textBox = new TextBox
        {
            AcceptsReturn = true,
            Text = text,
            SelectionStart = selectionStart,
            SelectionEnd = selectionEnd,
        };
        TextBoxStaleSelectionGuard.Attach(textBox);
        return textBox;
    }

    [Fact]
    public void Decide_NoSelection_None()
    {
        Assert.Equal(Fix.None, TextBoxStaleSelectionGuard.Decide("abc", "abcd", 2, 2));
    }

    [Fact]
    public void Decide_SameText_None()
    {
        Assert.Equal(Fix.None, TextBoxStaleSelectionGuard.Decide("abc", "abc", 0, 2));
    }

    [Fact]
    public void Decide_PartialSelection_Collapse()
    {
        Assert.Equal(Fix.Collapse, TextBoxStaleSelectionGuard.Decide("abc\ndef", "- abc\n- def", 4, 7));
    }

    [Fact]
    public void Decide_WholeTextSelectedTextGrew_SelectAll()
    {
        Assert.Equal(Fix.SelectAll, TextBoxStaleSelectionGuard.Decide("abc", "\"abc\"", 0, 3));
    }

    [Fact]
    public void Decide_WholeTextSelectedTextShrank_None()
    {
        // The text box already clamped the end to the (shorter) new text: still all selected.
        Assert.Equal(Fix.None, TextBoxStaleSelectionGuard.Decide("\"abc\"", "abc", 0, 3));
    }

    [AvaloniaFact]
    public void SurroundLine2_ThenDialogDashes_ThenLift_LiftsWholeText()
    {
        // #17052: quotes around the selected second line, dialog dashes, then F8 (add "\N\N\N" after).
        var text = Line1 + Environment.NewLine + Line2;
        var textBox = MakeGuardedTextBox(text, Line1.Length + Environment.NewLine.Length, text.Length);
        var wrapper = new TextBoxWrapper(textBox);

        Assert.True(TextBoxSurroundToggler.ToggleSelection(wrapper, "\"", "\""));
        Dispatcher.UIThread.RunJobs();
        Assert.Equal("\"" + Line2 + "\"", textBox.SelectedText);

        // Dialog dashes replace the whole text from the view model (through the binding).
        textBox.Text = "- " + Line1 + Environment.NewLine + "- \"" + Line2 + "\"";
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(textBox.SelectionStart, textBox.SelectionEnd);

        // No selection left: "surround with" falls back to the whole subtitle text.
        Assert.False(TextBoxSurroundToggler.ToggleSelection(wrapper, string.Empty, "\n<font size=23>\\N\\N\\N"));
    }

    [AvaloniaFact]
    public void WholeTextSelected_TextReplacedLonger_WholeTextSelectedAgain()
    {
        var textBox = MakeGuardedTextBox(Line2, 0, Line2.Length);

        textBox.Text = "\"" + Line2 + "\"";
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(0, textBox.SelectionStart);
        Assert.Equal(textBox.Text.Length, textBox.SelectionEnd);
    }

    [AvaloniaFact]
    public void TypingOverPartialSelection_CaretAfterTypedText()
    {
        var textBox = MakeGuardedTextBox("Hello world", 6, 11);

        textBox.SelectedText = "there";
        Dispatcher.UIThread.RunJobs();

        Assert.Equal("Hello there", textBox.Text);
        Assert.Equal(11, textBox.CaretIndex);
        Assert.Equal(textBox.SelectionStart, textBox.SelectionEnd);
    }

    [AvaloniaFact]
    public void TypingOverWholeSelection_NotSelectedAgain()
    {
        var textBox = MakeGuardedTextBox("Hello", 0, 5);

        textBox.SelectedText = "H";
        Dispatcher.UIThread.RunJobs();

        Assert.Equal("H", textBox.Text);
        Assert.Equal(1, textBox.CaretIndex);
        Assert.Equal(textBox.SelectionStart, textBox.SelectionEnd);
    }

    [AvaloniaFact]
    public void SurroundSelectedWord_KeepsSurroundedWordSelected()
    {
        var textBox = MakeGuardedTextBox("Hello world", 6, 11);

        Assert.True(TextBoxSurroundToggler.ToggleSelection(new TextBoxWrapper(textBox), "<i>", "</i>"));
        Dispatcher.UIThread.RunJobs();

        Assert.Equal("<i>world</i>", textBox.SelectedText);
    }
}
