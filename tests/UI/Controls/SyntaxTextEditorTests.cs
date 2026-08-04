using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Media;
using Nikse.SubtitleEdit.Controls.SyntaxTextEditorControl;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Controls;

/// <summary>
/// The virtualizing source editor. The point of the control is that the work per frame follows the
/// viewport, not the file size, so the first test here is the one that matters most.
/// </summary>
public class SyntaxTextEditorTests : IDisposable
{
    private const int WindowHeight = 400;

    // Every window opened by a test is closed again: an open window keeps its caret blink timer
    // ticking, and a stray timer trips over the next test's dispatcher.
    private readonly List<Window> _windows = new();

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            window.Close();
        }

        _windows.Clear();
    }

    private static string MakeSrt(int paragraphs)
    {
        var sb = new System.Text.StringBuilder();
        for (var i = 1; i <= paragraphs; i++)
        {
            var start = TimeSpan.FromSeconds(i * 2);
            var end = TimeSpan.FromSeconds(i * 2 + 1.5);
            sb.Append(i).Append("\r\n")
                .Append($"{start:hh\\:mm\\:ss},000 --> {end:hh\\:mm\\:ss},500").Append("\r\n")
                .Append("Lorem <i>ipsum</i> dolor sit amet, line ").Append(i).Append("\r\n")
                .Append("\r\n");
        }

        return sb.ToString();
    }

    private (Window Window, SyntaxTextEditor Editor) Show(string text, bool readOnly = false)
    {
        var editor = new SyntaxTextEditor
        {
            Text = text,
            FontFamily = new FontFamily("Courier New"),
            FontSize = 12,
            IsReadOnly = readOnly,
            SourceHighlighter = new SubRipSourceSyntaxHighlighting(),
        };

        var window = new Window { Content = editor, Width = 600, Height = WindowHeight };
        _windows.Add(window);
        window.Show();
        window.UpdateLayout();
        editor.View.Focus();
        return (window, editor);
    }

    private static void Press(Window window, Key key, RawInputModifiers modifiers = RawInputModifiers.None)
    {
        window.KeyPress(key, modifiers, PhysicalKey.None, string.Empty);
    }

    [AvaloniaFact]
    public void OnlyTheVisibleLinesAreLaidOut()
    {
        var (_, editor) = Show(MakeSrt(20_000)); // 4 lines per paragraph plus a trailing blank
        var view = editor.View;

        view.EnsureVisibleLayouts();

        var visible = view.GetVisibleLineRange();
        var visibleCount = visible.Last - visible.First + 1;

        Assert.Equal(80_001, view.Document.LineCount);
        Assert.True(visibleCount < 100, $"expected a screenful of lines, got {visibleCount}");

        // One layout per visible line, plus the one the width estimate measures.
        Assert.True(
            view.LayoutsCreated <= visibleCount + 2,
            $"{view.LayoutsCreated} layouts for {visibleCount} visible lines - virtualization is broken");
    }

    [AvaloniaFact]
    public void ScrollingBuildsOnlyTheNewlyVisibleLines()
    {
        var (_, editor) = Show(MakeSrt(20_000));
        var view = editor.View;

        view.EnsureVisibleLayouts();
        var afterFirstPaint = view.LayoutsCreated;

        // Jump far down: the lines already cached cannot help, so this is a full screen of work.
        view.ScrollOffset = new Vector(0, 100_000);
        view.EnsureVisibleLayouts();

        var visible = view.GetVisibleLineRange();
        var visibleCount = visible.Last - visible.First + 1;
        Assert.True(
            view.LayoutsCreated - afterFirstPaint <= visibleCount + 1,
            $"scrolling built {view.LayoutsCreated - afterFirstPaint} layouts for {visibleCount} lines");

        // Scrolling back to a place that is still cached should build nothing at all.
        var beforeReturn = view.LayoutsCreated;
        view.ScrollOffset = new Vector(0, 100_000 + view.LineHeight);
        view.EnsureVisibleLayouts();
        Assert.True(view.LayoutsCreated - beforeReturn <= 2);
    }

    [AvaloniaFact]
    public void ScrollOffsetIsClampedToTheExtent()
    {
        var (_, editor) = Show(MakeSrt(10));
        var view = editor.View;

        view.ScrollOffset = new Vector(-500, -500);
        Assert.Equal(new Vector(0, 0), view.ScrollOffset);

        view.ScrollOffset = new Vector(0, 10_000_000);
        Assert.True(view.ScrollOffset.Y <= view.Extent.Height);
    }

    [AvaloniaFact]
    public void GutterCountsAndFollowsTheView()
    {
        var (_, editor) = Show(MakeSrt(100));

        var gutter = Assert.IsType<LineNumberGutter>(
            ((Grid)((Decorator)editor).Child!).Children[0]);

        Assert.Equal(editor.Document.LineCount, gutter.LineCount);
        Assert.True(gutter.Bounds.Width > 0, "the gutter should size itself to the line count");

        editor.View.ScrollOffset = new Vector(0, 200);
        Assert.Equal(200, gutter.VerticalOffset);

        editor.CaretOffset = editor.Document.GetLineStartOffset(7);
        Assert.Equal(7, gutter.CurrentLine);
    }

    [AvaloniaFact]
    public void ShowLineNumbersHidesTheGutter()
    {
        var (_, editor) = Show(MakeSrt(10));
        var gutter = ((Grid)((Decorator)editor).Child!).Children[0];

        editor.ShowLineNumbers = false;
        Assert.False(gutter.IsVisible);
    }

    [AvaloniaFact]
    public void CaretMovesByCharacterAndLine()
    {
        var (window, editor) = Show("one\r\ntwo\r\nthree");
        var view = editor.View;

        view.CaretOffset = 0;
        Press(window, Key.Right);
        Assert.Equal(1, view.CaretOffset);

        Press(window, Key.End);
        Assert.Equal(3, view.CaretOffset);

        // Past the end of a line the caret lands on the next line's start, not inside the break.
        Press(window, Key.Right);
        Assert.Equal(5, view.CaretOffset);

        Press(window, Key.Down);
        Assert.Equal(2, view.CaretLine);

        // Left from the start of a line steps back over the whole break, onto the end of line 1.
        Press(window, Key.Left);
        Assert.Equal(1, view.CaretLine);
        Assert.Equal(8, view.CaretOffset);
    }

    [AvaloniaFact]
    public void ShiftArrowSelectsAndTypingReplacesTheSelection()
    {
        var (window, editor) = Show("hello world");
        var view = editor.View;

        view.CaretOffset = 0;
        for (var i = 0; i < 5; i++)
        {
            Press(window, Key.Right, RawInputModifiers.Shift);
        }

        Assert.Equal("hello", view.SelectedText);

        view.InsertText("bye");
        Assert.Equal("bye world", editor.Text);
        Assert.Equal(3, view.CaretOffset);
    }

    [AvaloniaFact]
    public void EnterSplitsTheLineAndBackspaceJoinsItBack()
    {
        var (window, editor) = Show("onetwo");
        var view = editor.View;

        view.CaretOffset = 3;
        Press(window, Key.Enter);

        Assert.Equal(2, view.Document.LineCount);
        Assert.Equal("one" + view.Document.NewLine + "two", editor.Text);

        Press(window, Key.Back);
        Assert.Equal(1, view.Document.LineCount);
        Assert.Equal("onetwo", editor.Text);
        Assert.Equal(3, view.CaretOffset);
    }

    [AvaloniaFact]
    public void TypedCharactersUndoAsOneStep()
    {
        var (_, editor) = Show("start");
        var view = editor.View;

        view.CaretOffset = 5;
        view.InsertText("a");
        view.InsertText("b");
        view.InsertText("c");
        Assert.Equal("startabc", editor.Text);

        view.Undo();
        Assert.Equal("start", editor.Text);
        Assert.False(view.CanUndo);

        view.Redo();
        Assert.Equal("startabc", editor.Text);
    }

    [AvaloniaFact]
    public void UndoRestoresDeletedTextAndTheCaret()
    {
        var (_, editor) = Show("one two three");
        var view = editor.View;

        view.Select(4, 3);
        view.DeleteSelection();
        Assert.Equal("one  three", editor.Text);

        view.Undo();
        Assert.Equal("one two three", editor.Text);
        Assert.Equal(4, view.SelectionStart);
        Assert.Equal(3, view.SelectionLength);
    }

    [AvaloniaFact]
    public void ReadOnlyRefusesEveryEdit()
    {
        var (window, editor) = Show("keep me", readOnly: true);
        var view = editor.View;

        view.CaretOffset = 0;
        view.InsertText("x");
        Press(window, Key.Delete);
        view.SelectAll();
        Press(window, Key.Back);

        Assert.Equal("keep me", editor.Text);
        Assert.False(view.CanUndo);
    }

    [AvaloniaFact]
    public void SelectAllCoversTheWholeDocument()
    {
        var text = MakeSrt(5);
        var (_, editor) = Show(text);

        editor.SelectAll();

        Assert.Equal(0, editor.SelectionStart);
        Assert.Equal(text.Length, editor.SelectionLength);
        Assert.Equal(text, editor.SelectedText);
    }

    [AvaloniaFact]
    public void SettingTextKeepsTheDocumentAndCaretConsistent()
    {
        var (_, editor) = Show("first");

        editor.Text = "a\r\nb\r\nc";

        Assert.Equal(3, editor.Document.LineCount);
        Assert.True(editor.CaretOffset <= editor.Document.TextLength);
        Assert.False(editor.View.CanUndo);
    }

    [AvaloniaFact]
    public void BringCaretIntoViewScrollsToTheCaret()
    {
        var (_, editor) = Show(MakeSrt(2_000));
        var view = editor.View;

        view.CaretOffset = view.Document.GetLineStartOffset(5_000);
        view.BringCaretIntoView();

        var visible = view.GetVisibleLineRange();
        Assert.InRange(5_000, visible.First, visible.Last);
    }

    // ----------------------------------------------------------------------------------------
    // Line commands
    // ----------------------------------------------------------------------------------------

    private static string Lines(params string[] lines) => string.Join("\r\n", lines);

    [AvaloniaFact]
    public void AltUpMovesTheCaretLineUpAndKeepsTheColumn()
    {
        var (window, editor) = Show(Lines("one", "two", "three"));
        var view = editor.View;

        view.CaretOffset = view.Document.GetOffset(1, 2); // inside "two"
        Press(window, Key.Up, RawInputModifiers.Alt);

        Assert.Equal(Lines("two", "one", "three"), editor.Text);
        Assert.Equal(new SyntaxTextPosition(0, 2), view.Document.GetPosition(view.CaretOffset));
    }

    [AvaloniaFact]
    public void AltDownMovesTheWholeSelectedBlock()
    {
        var (window, editor) = Show(Lines("a", "b", "c", "d"));
        var view = editor.View;

        // Select from inside line 0 to inside line 1 - both lines count as touched.
        view.Select(view.Document.GetOffset(0, 0), view.Document.GetOffset(1, 1));
        Press(window, Key.Down, RawInputModifiers.Alt);

        Assert.Equal(Lines("c", "a", "b", "d"), editor.Text);
        Assert.Equal("a\r\nb", view.SelectedText);
    }

    [AvaloniaFact]
    public void MovingLinesStaysInsideTheDocument()
    {
        var (window, editor) = Show(Lines("first", "last"));
        var view = editor.View;

        view.CaretOffset = 0;
        Press(window, Key.Up, RawInputModifiers.Alt);
        Assert.Equal(Lines("first", "last"), editor.Text);

        view.CaretOffset = view.Document.GetLineStartOffset(1);
        Press(window, Key.Down, RawInputModifiers.Alt);
        Assert.Equal(Lines("first", "last"), editor.Text);
        Assert.False(view.CanUndo);
    }

    [AvaloniaFact]
    public void MoveLineUndoesAsOneStep()
    {
        var (window, editor) = Show(Lines("one", "two", "three"));
        var view = editor.View;

        view.CaretOffset = view.Document.GetLineStartOffset(2);
        Press(window, Key.Up, RawInputModifiers.Alt);
        Assert.Equal(Lines("one", "three", "two"), editor.Text);

        view.Undo();
        Assert.Equal(Lines("one", "two", "three"), editor.Text);
        Assert.False(view.CanUndo);
    }

    [AvaloniaFact]
    public void DuplicateLineInsertsTheCopyBelowAndLandsOnIt()
    {
        var (window, editor) = Show(Lines("one", "two"));
        var view = editor.View;

        view.CaretOffset = view.Document.GetOffset(0, 1);
        Press(window, Key.D, ControlOrMeta);

        Assert.Equal(Lines("one", "one", "two"), editor.Text);
        Assert.Equal(new SyntaxTextPosition(1, 1), view.Document.GetPosition(view.CaretOffset));

        // Repeating it duplicates the copy, not the original.
        Press(window, Key.D, ControlOrMeta);
        Assert.Equal(Lines("one", "one", "one", "two"), editor.Text);
    }

    [AvaloniaFact]
    public void DeleteLineTakesTheLineBreakWithIt()
    {
        var (window, editor) = Show(Lines("one", "two", "three"));
        var view = editor.View;

        view.CaretOffset = view.Document.GetLineStartOffset(1);
        Press(window, Key.K, ControlOrMeta | RawInputModifiers.Shift);

        Assert.Equal(Lines("one", "three"), editor.Text);
        Assert.Equal(2, view.Document.LineCount);
    }

    [AvaloniaFact]
    public void DeletingTheLastLineTakesTheBreakAboveIt()
    {
        var (window, editor) = Show(Lines("one", "two"));
        var view = editor.View;

        view.CaretOffset = view.Document.GetLineStartOffset(1);
        Press(window, Key.K, ControlOrMeta | RawInputModifiers.Shift);

        Assert.Equal("one", editor.Text);
        Assert.Equal(1, view.Document.LineCount);
    }

    [AvaloniaFact]
    public void DeleteLineOnTheOnlyLineJustEmptiesIt()
    {
        var (window, editor) = Show("only");
        var view = editor.View;

        Press(window, Key.K, ControlOrMeta | RawInputModifiers.Shift);

        Assert.Equal(string.Empty, editor.Text);
        Assert.Equal(1, view.Document.LineCount);
    }

    [AvaloniaFact]
    public void DeleteWordLeftAndRightRemoveOneWord()
    {
        var (window, editor) = Show("one two three");
        var view = editor.View;

        view.CaretOffset = 7; // right after "two"
        Press(window, Key.Back, WordModifier);
        Assert.Equal("one  three", editor.Text);

        view.CaretOffset = 4;
        Press(window, Key.Delete, WordModifier);
        Assert.Equal("one ", editor.Text);
    }

    [AvaloniaFact]
    public void ReadOnlyRefusesTheLineCommands()
    {
        var text = Lines("one", "two", "three");
        var (window, editor) = Show(text, readOnly: true);

        editor.View.CaretOffset = editor.Document.GetLineStartOffset(1);
        Press(window, Key.Up, RawInputModifiers.Alt);
        Press(window, Key.D, ControlOrMeta);
        Press(window, Key.K, ControlOrMeta | RawInputModifiers.Shift);
        Press(window, Key.Back, WordModifier);

        Assert.Equal(text, editor.Text);
        Assert.False(editor.View.CanUndo);
    }

    [AvaloniaFact]
    public void ReplaceAllTextIsOneUndoStep()
    {
        var (_, editor) = Show(Lines("one", "two"));

        editor.ReplaceAllText(Lines("1", "2"));
        Assert.Equal(Lines("1", "2"), editor.Text);

        editor.Undo();
        Assert.Equal(Lines("one", "two"), editor.Text);
        Assert.False(editor.View.CanUndo);
    }

    // The editor follows the platform: Cmd on macOS, Ctrl elsewhere - and word steps use Option on
    // macOS, where Ctrl is not a text-editing modifier at all.
    private static RawInputModifiers ControlOrMeta =>
        OperatingSystem.IsMacOS() ? RawInputModifiers.Meta : RawInputModifiers.Control;

    private static RawInputModifiers WordModifier =>
        OperatingSystem.IsMacOS() ? RawInputModifiers.Alt : RawInputModifiers.Control;
}
