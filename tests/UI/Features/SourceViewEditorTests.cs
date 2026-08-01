using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Controls.SyntaxTextEditorControl;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Shared.SourceView;
using Nikse.SubtitleEdit.Features.Shared.TextBoxUtils;

namespace UITests.Features;

/// <summary>
/// The source view edits raw subtitle text in the virtualizing editor. Its text is deliberately
/// not mirrored into the view model while typing (that cost grows with the file), so the test that
/// matters most here is that Ok still picks up what was actually typed.
/// </summary>
public class SourceViewEditorTests
{
    private static (SourceViewViewModel Vm, Subtitle Subtitle, string Text) MakeSourceView()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("First line", 1000, 3000));
        subtitle.Paragraphs.Add(new Paragraph("Second line", 3500, 6000));

        var format = new SubRip();
        var text = subtitle.ToText(format);

        var vm = new SourceViewViewModel();
        vm.Initialize("Source view", text, format, subtitle, 1);
        return (vm, subtitle, text);
    }

    [AvaloniaFact]
    public void SourceIsShownInTheVirtualizingEditor()
    {
        var (vm, _, text) = MakeSourceView();

        Assert.IsType<SyntaxTextEditorWrapper>(vm.SourceViewTextBox);
        Assert.IsType<SyntaxTextView>(vm.SourceViewTextBox.TextControl);
        Assert.Equal(text, vm.SourceViewTextBox.Text);
    }

    [AvaloniaFact]
    public void CaretStartsOnTheSelectedParagraph()
    {
        var (vm, _, text) = MakeSourceView();
        vm.FocusEditor();

        var view = (SyntaxTextView)vm.SourceViewTextBox.TextControl;
        var caretLine = view.Document.GetPosition(view.CaretOffset).Line;

        // The caret lands on the text of the selected paragraph - line 6 here: number, time code,
        // text and a blank line for the first paragraph, then number and time code for the second.
        Assert.Equal(6, caretLine);
        Assert.Equal("Second line", view.Document.GetLine(caretLine));
    }

    [AvaloniaFact]
    public void OkReadsTheEditedTextBackFromTheEditor()
    {
        var (vm, subtitle, _) = MakeSourceView();
        var window = new Window { Content = new Border { Child = vm.SourceViewTextBox.ContentControl } };
        vm.Window = window;
        window.Show();
        window.UpdateLayout();

        var view = (SyntaxTextView)vm.SourceViewTextBox.TextControl;
        var offset = view.Document.Text.IndexOf("Second line", StringComparison.Ordinal);
        view.Select(offset, "Second line".Length);
        view.InsertText("Edited line");

        vm.OkCommand.Execute(null);

        Assert.True(vm.OkPressed);
        Assert.Equal("Edited line", vm.Subtitle.Paragraphs[1].Text);
        Assert.Equal(2, subtitle.Paragraphs.Count);

        window.Close();
    }
}
