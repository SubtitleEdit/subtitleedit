using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Threading;
using AvaloniaEdit;

namespace UITests.Logic;

/// <summary>
/// The subtitle text box (with color tags on) is an AvaloniaEdit TextEditor. In a right-to-left
/// editor the Left key must move the caret visually left, which is logically forward. Avalonia's
/// own TextPresenter.MoveCaretHorizontal flips the logical direction for a RightToLeft control,
/// so the editor has to do the same or its arrows run backwards against every other text box in
/// the app (follow-up to the RTL rendering fix, #12386 / #12434).
///
/// These drive the same routed commands that the Left/Right key bindings invoke.
/// </summary>
public class RightToLeftCaretNavigationTests
{
    private const string Arabic = "مرحبا بالعالم";
    private const string English = "hello world";

    private static TextEditor ShowEditor(string text, FlowDirection flowDirection)
    {
        var editor = new TextEditor
        {
            FlowDirection = flowDirection,
            Text = text,
        };

        // A full window layout pass is what materializes the editor's template, so the TextArea
        // and its TextView actually enter the visual tree and inherit FlowDirection.
        var window = new Window { Content = editor, Width = 400, Height = 200 };
        window.Show();
        window.Measure(new Avalonia.Size(400, 200));
        window.Arrange(new Avalonia.Rect(0, 0, 400, 200));
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(flowDirection, editor.TextArea.TextView.FlowDirection);

        return editor;
    }

    [AvaloniaFact]
    public void RightToLeft_MoveLeft_MovesCaretLogicallyForward()
    {
        var editor = ShowEditor(Arabic, FlowDirection.RightToLeft);
        editor.CaretOffset = 5;

        EditingCommands.MoveLeftByCharacter.Execute(null, editor.TextArea);

        // Visually left on a right-to-left line is the next character logically.
        Assert.Equal(6, editor.CaretOffset);
    }

    [AvaloniaFact]
    public void RightToLeft_MoveRight_MovesCaretLogicallyBackward()
    {
        var editor = ShowEditor(Arabic, FlowDirection.RightToLeft);
        editor.CaretOffset = 5;

        EditingCommands.MoveRightByCharacter.Execute(null, editor.TextArea);

        Assert.Equal(4, editor.CaretOffset);
    }

    [AvaloniaFact]
    public void LeftToRight_MoveLeft_StillMovesCaretBackward()
    {
        var editor = ShowEditor(English, FlowDirection.LeftToRight);
        editor.CaretOffset = 5;

        EditingCommands.MoveLeftByCharacter.Execute(null, editor.TextArea);

        // Left-to-right must be untouched by the flip.
        Assert.Equal(4, editor.CaretOffset);
    }

    [AvaloniaFact]
    public void LeftToRight_MoveRight_StillMovesCaretForward()
    {
        var editor = ShowEditor(English, FlowDirection.LeftToRight);
        editor.CaretOffset = 5;

        EditingCommands.MoveRightByCharacter.Execute(null, editor.TextArea);

        Assert.Equal(6, editor.CaretOffset);
    }
}
