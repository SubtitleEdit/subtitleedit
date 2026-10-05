using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Features.Shared.TextBoxUtils;

namespace UITests.Features.Shared;

// "Surround with ♪ ♪" (Ctrl+M) should only surround the selected text when part of the text
// is selected in the edit box - like SE 4 does (issue #12873).
public class TextBoxSurroundTogglerTests
{
    private static TextBox MakeTextBox(string text, int selectionStart, int selectionEnd)
    {
        return new TextBox
        {
            Text = text,
            SelectionStart = selectionStart,
            SelectionEnd = selectionEnd,
        };
    }

    [AvaloniaFact]
    public void ToggleSelection_FirstOfTwoLinesSelected_OnlyFirstLineGetsMusicSymbols()
    {
        var text = "Will never be my fool" + Environment.NewLine + "FIETSEN VERBODEN";
        var textBox = MakeTextBox(text, 0, "Will never be my fool".Length);

        var result = TextBoxSurroundToggler.ToggleSelection(new TextBoxWrapper(textBox), "♪", "♪");
        Dispatcher.UIThread.RunJobs();

        Assert.True(result);
        Assert.Equal("♪ Will never be my fool ♪" + Environment.NewLine + "FIETSEN VERBODEN", textBox.Text);
    }

    [AvaloniaFact]
    public void ToggleSelection_ItalicLineSelected_KeepsMusicSymbolsInsideItalicTags()
    {
        var text = "<i>Will never be my fool</i>" + Environment.NewLine + "FIETSEN VERBODEN";
        var textBox = MakeTextBox(text, 0, "<i>Will never be my fool</i>".Length);

        var result = TextBoxSurroundToggler.ToggleSelection(new TextBoxWrapper(textBox), "♪", "♪");
        Dispatcher.UIThread.RunJobs();

        Assert.True(result);
        Assert.Equal("<i>♪ Will never be my fool ♪</i>" + Environment.NewLine + "FIETSEN VERBODEN", textBox.Text);
    }

    [AvaloniaFact]
    public void ToggleSelection_SelectionAlreadyHasMusicSymbols_RemovesThem()
    {
        var text = "♪ Will never be my fool ♪" + Environment.NewLine + "FIETSEN VERBODEN";
        var textBox = MakeTextBox(text, 0, "♪ Will never be my fool ♪".Length);

        var result = TextBoxSurroundToggler.ToggleSelection(new TextBoxWrapper(textBox), "♪", "♪");
        Dispatcher.UIThread.RunJobs();

        Assert.True(result);
        Assert.Equal("Will never be my fool" + Environment.NewLine + "FIETSEN VERBODEN", textBox.Text);
    }

    [AvaloniaFact]
    public void ToggleSelection_SelectionKeepsTrailingNewLineOutsideSymbols()
    {
        var text = "Line one" + Environment.NewLine + "Line two";
        var textBox = MakeTextBox(text, 0, ("Line one" + Environment.NewLine).Length);

        var result = TextBoxSurroundToggler.ToggleSelection(new TextBoxWrapper(textBox), "♪", "♪");
        Dispatcher.UIThread.RunJobs();

        Assert.True(result);
        Assert.Equal("♪ Line one ♪" + Environment.NewLine + "Line two", textBox.Text);
    }

    [AvaloniaFact]
    public void ToggleSelection_SelectionIsRestoredAfterToggle()
    {
        var textBox = MakeTextBox("Hello world", 6, 11);

        TextBoxSurroundToggler.ToggleSelection(new TextBoxWrapper(textBox), "♪", "♪");
        Dispatcher.UIThread.RunJobs();

        Assert.Equal("Hello ♪ world ♪", textBox.Text);
        Assert.Equal(6, textBox.SelectionStart);
        Assert.Equal("♪ world ♪".Length + 6, textBox.SelectionEnd);
    }

    [AvaloniaFact]
    public void ToggleSelection_NoSelection_ReturnsFalseAndKeepsText()
    {
        var textBox = MakeTextBox("Hello world", 3, 3);

        var result = TextBoxSurroundToggler.ToggleSelection(new TextBoxWrapper(textBox), "♪", "♪");
        Dispatcher.UIThread.RunJobs();

        Assert.False(result);
        Assert.Equal("Hello world", textBox.Text);
    }

    [AvaloniaFact]
    public void ToggleSelection_WholeTextSelected_ReturnsFalseAndKeepsText()
    {
        var textBox = MakeTextBox("Hello world", 0, "Hello world".Length);

        var result = TextBoxSurroundToggler.ToggleSelection(new TextBoxWrapper(textBox), "♪", "♪");
        Dispatcher.UIThread.RunJobs();

        Assert.False(result);
        Assert.Equal("Hello world", textBox.Text);
    }

    [AvaloniaFact]
    public void IsWholeTextSelected_DetectsFullSelectionOnly()
    {
        Assert.True(TextBoxSurroundToggler.IsWholeTextSelected(new TextBoxWrapper(MakeTextBox("Hello?", 0, 6))));
        Assert.True(TextBoxSurroundToggler.IsWholeTextSelected(new TextBoxWrapper(MakeTextBox("Hello?", 6, 0))));
        Assert.False(TextBoxSurroundToggler.IsWholeTextSelected(new TextBoxWrapper(MakeTextBox("Hello?", 0, 5))));
        Assert.False(TextBoxSurroundToggler.IsWholeTextSelected(new TextBoxWrapper(MakeTextBox("Hello?", 3, 3))));
        Assert.False(TextBoxSurroundToggler.IsWholeTextSelected(new TextBoxWrapper(MakeTextBox(string.Empty, 0, 0))));
        Assert.False(TextBoxSurroundToggler.IsWholeTextSelected(null));
    }

    // #15693: surrounding a fully selected line updates the subtitle text, and the text box kept the
    // old selection range - so the closing symbol and the last character were no longer selected.
    [AvaloniaFact]
    public void SelectAllAfterTextChange_WholeTextSelected_SelectionCoversLongerText()
    {
        var textBox = MakeTextBox("Hello?", 0, 6);
        var wrapper = new TextBoxWrapper(textBox);
        Assert.False(TextBoxSurroundToggler.ToggleSelection(wrapper, "\"", "\""));

        textBox.Text = "\"Hello?\"";
        Assert.Equal(6, textBox.SelectionEnd); // stale range: misses "?\""

        TextBoxSurroundToggler.SelectAllAfterTextChange(wrapper);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(0, textBox.SelectionStart);
        Assert.Equal(textBox.Text.Length, textBox.SelectionEnd);

        // The next surround (like a lift with "\N") now works on the whole text again.
        Assert.True(TextBoxSurroundToggler.IsWholeTextSelected(wrapper));
    }

    [AvaloniaFact]
    public void ToggleSelection_OnlyWhiteSpaceSelected_ReturnsFalseAndKeepsText()
    {
        var textBox = MakeTextBox("Hello world", 5, 6);

        var result = TextBoxSurroundToggler.ToggleSelection(new TextBoxWrapper(textBox), "♪", "♪");
        Dispatcher.UIThread.RunJobs();

        Assert.False(result);
        Assert.Equal("Hello world", textBox.Text);
    }

    [AvaloniaFact]
    public void ToggleSelection_NonMusicSurround_WrapsSelection()
    {
        var textBox = MakeTextBox("Hello world", 6, 11);

        var result = TextBoxSurroundToggler.ToggleSelection(new TextBoxWrapper(textBox), "(", ")");
        Dispatcher.UIThread.RunJobs();

        Assert.True(result);
        Assert.Equal("Hello (world)", textBox.Text);
    }
}
