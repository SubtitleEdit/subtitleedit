using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Controls.SyntaxTextEditorControl;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Shared.SourceView;
using Nikse.SubtitleEdit.Features.Shared.TextBoxUtils;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Features;

/// <summary>
/// The source view edits raw subtitle text in the virtualizing editor. Its text is deliberately
/// not mirrored into the view model while typing (that cost grows with the file), so the test that
/// matters most here is that Ok still picks up what was actually typed.
/// </summary>
public class SourceViewEditorTests : IDisposable
{
    // Every window opened by a test is closed again in Dispose: if a test stops early, an
    // unclosed window would outlive the test and race with the headless session teardown.
    private readonly List<Window> _windows = new();

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            window.Close();
        }

        _windows.Clear();
    }

    /// <summary>Only Go to line opens a window, and none of these tests take that path.</summary>
    private sealed class NoWindowService : IWindowService
    {
        public T ShowWindow<T>(Window owner, Action<T>? configure = null) where T : Window
            => throw new NotSupportedException();

        public TViewModel ShowWindow<T, TViewModel>(Window owner, Action<T, TViewModel>? configure = null)
            where T : Window where TViewModel : class
            => throw new NotSupportedException();

        public TViewModel ShowIndependentWindow<T, TViewModel>(Action<T, TViewModel>? configure = null)
            where T : Window where TViewModel : class
            => throw new NotSupportedException();

        public Task<T> ShowDialogAsync<T>(Window owner, Action<T>? configure = null) where T : Window
            => throw new NotSupportedException();

        public Task<TViewModel> ShowDialogAsync<TWindow, TViewModel>(
            Window owner,
            Action<TViewModel>? configureViewModel = null,
            Action<TWindow>? configureWindow = null)
            where TWindow : Window where TViewModel : class
            => throw new NotSupportedException();
    }

    private static (SourceViewViewModel Vm, Subtitle Subtitle, string Text) MakeSourceView()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("First line", 1000, 3000));
        subtitle.Paragraphs.Add(new Paragraph("Second line", 3500, 6000));

        var format = new SubRip();
        var text = subtitle.ToText(format);

        var vm = new SourceViewViewModel(new NoWindowService());
        vm.Initialize("Source view", text, format, subtitle, 1);
        return (vm, subtitle, text);
    }

    private static SyntaxTextView ViewOf(SourceViewViewModel vm) => (SyntaxTextView)vm.SourceViewTextBox.TextControl;

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

        var view = ViewOf(vm);
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
        _windows.Add(window);
        vm.Window = window;
        window.Show();
        window.UpdateLayout();

        var view = ViewOf(vm);
        var offset = view.Document.Text.IndexOf("Second line", StringComparison.Ordinal);
        view.Select(offset, "Second line".Length);
        view.InsertText("Edited line");

        vm.OkCommand.Execute(null);

        Assert.True(vm.OkPressed);
        Assert.Equal("Edited line", vm.Subtitle.Paragraphs[1].Text);
        Assert.Equal(2, subtitle.Paragraphs.Count);

        window.Close();
    }

    // ----------------------------------------------------------------------------------------
    // Unsaved changes
    // ----------------------------------------------------------------------------------------

    [AvaloniaFact]
    public void AnUntouchedSourceClosesWithoutAsking()
    {
        var (vm, _, _) = MakeSourceView();

        // Loading the source is not an edit, and neither is only moving the caret.
        ViewOf(vm).CaretOffset = 3;

        Assert.False(vm.NeedsDiscardConfirmation);
    }

    [AvaloniaFact]
    public void EditingTheSourceAsksBeforeThrowingTheChangesAway()
    {
        var (vm, _, _) = MakeSourceView();

        ViewOf(vm).InsertText("x");

        Assert.True(vm.NeedsDiscardConfirmation);
    }

    [AvaloniaFact]
    public void OkNeverAsksAboutDiscarding()
    {
        var (vm, _, _) = MakeSourceView();
        var window = new Window { Content = new Border { Child = vm.SourceViewTextBox.ContentControl } };
        _windows.Add(window);
        vm.Window = window;
        window.Show();
        window.UpdateLayout();

        var view = ViewOf(vm);
        view.CaretOffset = view.Document.TextLength;
        view.InsertText("x");
        vm.OkCommand.Execute(null);

        Assert.True(vm.OkPressed);
        Assert.False(vm.NeedsDiscardConfirmation);

        window.Close();
    }

    // ----------------------------------------------------------------------------------------
    // Live validation
    // ----------------------------------------------------------------------------------------

    [AvaloniaFact]
    public void ValidationReportsTheLineAndSubtitleCount()
    {
        var (vm, _, _) = MakeSourceView();

        Assert.False(vm.IsValidationError);
        Assert.Contains("2 subtitles", vm.ValidationInfo);
    }

    [AvaloniaFact]
    public void ValidationFlagsASourceThatNoLongerParses()
    {
        var (vm, _, _) = MakeSourceView();

        ViewOf(vm).ReplaceAllText("this is not a subtitle at all");
        vm.Validate();

        Assert.True(vm.IsValidationError);
        Assert.Contains("SubRip", vm.ValidationInfo);
    }

    // ----------------------------------------------------------------------------------------
    // Find and replace
    // ----------------------------------------------------------------------------------------

    [AvaloniaFact]
    public void FindNextSelectsTheMatchAndWrapsAround()
    {
        var (vm, _, _) = MakeSourceView();
        var view = ViewOf(vm);

        vm.SearchText = "line";
        view.CaretOffset = 0;
        view.ClearSelection();

        vm.FindNextCommand.Execute(null);
        var first = view.SelectionStart;
        Assert.Equal("line", view.SelectedText);

        vm.FindNextCommand.Execute(null);
        var second = view.SelectionStart;
        Assert.True(second > first);

        // Only two matches, so the third search comes back around to the first one.
        vm.FindNextCommand.Execute(null);
        Assert.Equal(first, view.SelectionStart);
    }

    [AvaloniaFact]
    public void FindIsCaseInsensitiveUntilMatchCaseIsTurnedOn()
    {
        var (vm, _, _) = MakeSourceView();
        var view = ViewOf(vm);

        vm.SearchText = "FIRST";
        view.CaretOffset = 0;
        view.ClearSelection();

        vm.FindNextCommand.Execute(null);
        Assert.Equal("First", view.SelectedText);

        vm.MatchCase = true;
        view.CaretOffset = 0;
        view.ClearSelection();
        vm.FindNextCommand.Execute(null);

        Assert.Equal(0, view.SelectionLength);
        Assert.Contains("FIRST", vm.FindStatus);
    }

    [AvaloniaFact]
    public void WholeWordDoesNotMatchInsideAWord()
    {
        var (vm, _, _) = MakeSourceView();
        var view = ViewOf(vm);
        view.ReplaceAllText("1\r\n00:00:01,000 --> 00:00:03,000\r\nsub subtitle\r\n");

        vm.SearchText = "sub";
        vm.WholeWord = true;
        view.CaretOffset = 0;
        view.ClearSelection();

        vm.FindNextCommand.Execute(null);
        Assert.Equal("sub", view.SelectedText);

        // "subtitle" must not count, so the next search wraps back to the same "sub".
        var firstMatch = view.SelectionStart;
        vm.FindNextCommand.Execute(null);
        Assert.Equal(firstMatch, view.SelectionStart);
    }

    [AvaloniaFact]
    public void FindPreviousWalksBackwards()
    {
        var (vm, _, _) = MakeSourceView();
        var view = ViewOf(vm);

        vm.SearchText = "line";
        view.CaretOffset = view.Document.TextLength;
        view.ClearSelection();

        vm.FindPreviousCommand.Execute(null);
        var last = view.SelectionStart;

        vm.FindPreviousCommand.Execute(null);
        Assert.True(view.SelectionStart < last);
        Assert.Equal("line", view.SelectedText);
    }

    [AvaloniaFact]
    public void ReplaceAllReplacesEveryMatchAsOneUndoStep()
    {
        var (vm, _, text) = MakeSourceView();
        var view = ViewOf(vm);

        vm.SearchText = "line";
        vm.ReplaceText = "row";
        vm.ReplaceAllCommand.Execute(null);

        Assert.Contains("First row", view.Text);
        Assert.Contains("Second row", view.Text);
        Assert.Contains("2", vm.FindStatus);

        view.Undo();
        Assert.Equal(text, view.Text);
    }

    [AvaloniaFact]
    public void ReplaceAllKeepsADollarSignLiteralOutsideRegexMode()
    {
        var (vm, _, _) = MakeSourceView();
        var view = ViewOf(vm);

        vm.SearchText = "First";
        vm.ReplaceText = "$0 costs";
        vm.ReplaceAllCommand.Execute(null);

        Assert.Contains("$0 costs line", view.Text);
    }

    [AvaloniaFact]
    public void RegexModeSubstitutesGroups()
    {
        var (vm, _, _) = MakeSourceView();
        var view = ViewOf(vm);

        vm.UseRegularExpression = true;
        vm.SearchText = @"(\w+) line";
        vm.ReplaceText = "line $1";
        vm.ReplaceAllCommand.Execute(null);

        Assert.Contains("line First", view.Text);
        Assert.Contains("line Second", view.Text);
    }

    [AvaloniaFact]
    public void AnInvalidRegexIsReportedInsteadOfThrowing()
    {
        var (vm, _, text) = MakeSourceView();

        vm.UseRegularExpression = true;
        vm.SearchText = "([unclosed";
        vm.FindNextCommand.Execute(null);
        vm.ReplaceAllCommand.Execute(null);

        Assert.Equal(text, ViewOf(vm).Text);
        Assert.False(string.IsNullOrEmpty(vm.FindStatus));
    }

    [AvaloniaFact]
    public void ReplaceSwapsTheCurrentMatchAndMovesToTheNext()
    {
        var (vm, _, _) = MakeSourceView();
        var view = ViewOf(vm);

        vm.SearchText = "line";
        vm.ReplaceText = "row";
        view.CaretOffset = 0;
        view.ClearSelection();

        vm.FindNextCommand.Execute(null); // select the first "line"
        vm.ReplaceCommand.Execute(null);

        Assert.Contains("First row", view.Text);
        Assert.Contains("Second line", view.Text);
        Assert.Equal("line", view.SelectedText); // sitting on the next match
    }
}
