using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Features.Shared.TextBoxUtils;

namespace UITests.Features.Shared;

// Ctrl+I in the OCR window's text box (and the main window's edit box) toggles italic on
// the selected part of the text, or on the whole line when nothing is selected (issue #12393).
public class TextBoxTagTogglerTests
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
    public void ToggleTag_SelectedWord_WrapsSelectionInItalic()
    {
        var textBox = MakeTextBox("Hello world", 6, 11);

        TextBoxTagToggler.ToggleTag(new TextBoxWrapper(textBox), "i", isAssa: false);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal("Hello <i>world</i>", textBox.Text);
        Assert.Equal(6, textBox.SelectionStart);
        Assert.Equal("<i>world</i>".Length + 6, textBox.SelectionEnd);
    }

    [AvaloniaFact]
    public void ToggleTag_SelectedItalicWord_RemovesItalic()
    {
        var textBox = MakeTextBox("Hello <i>world</i>", 6, 18);

        TextBoxTagToggler.ToggleTag(new TextBoxWrapper(textBox), "i", isAssa: false);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal("Hello world", textBox.Text);
    }

    [AvaloniaFact]
    public void ToggleTag_NoSelection_TogglesWholeText()
    {
        var textBox = MakeTextBox("Hello world", 0, 0);

        TextBoxTagToggler.ToggleTag(new TextBoxWrapper(textBox), "i", isAssa: false);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal("<i>Hello world</i>", textBox.Text);
    }

    [AvaloniaFact]
    public void ToggleTag_SelectionWithSurroundingSpaces_KeepsSpacesOutsideTag()
    {
        var textBox = MakeTextBox("Hello world", 5, 11); // " world" selected

        TextBoxTagToggler.ToggleTag(new TextBoxWrapper(textBox), "i", isAssa: false);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal("Hello <i>world</i>", textBox.Text);
    }

    [AvaloniaFact]
    public void ToggleTag_Assa_UsesAssaTags()
    {
        var textBox = MakeTextBox("Hello world", 6, 11);

        TextBoxTagToggler.ToggleTag(new TextBoxWrapper(textBox), "i", isAssa: true);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal("Hello {\\i1}world{\\i0}", textBox.Text);
    }
}
