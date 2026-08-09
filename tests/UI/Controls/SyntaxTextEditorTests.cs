using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
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

    private async Task<(Window Window, SyntaxTextEditor Editor)> Show(string text, bool readOnly = false)
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

        // The key presses only reach the editor once the view holds focus; under CPU load the
        // Focus() call can lag a few frames (CI flake: Shift+Right selection missing). Wait for
        // it - the test's own state waits surface a real focus failure.
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        while (!ReferenceEquals(window.FocusManager?.GetFocusedElement(), editor.View))
        {
            if (stopwatch.ElapsedMilliseconds > 1000)
            {
                Assert.Fail("The editor did not get keyboard focus within 1 second — keys would land nowhere and the test result would be meaningless.");
            }

            Dispatcher.UIThread.RunJobs();
            await Task.Delay(10);
        }

        return (window, editor);
    }

    private static void Press(Window window, Key key, RawInputModifiers modifiers = RawInputModifiers.None)
    {
        window.KeyPress(key, modifiers, PhysicalKey.None, string.Empty);

        // Process the dispatcher queue synchronously so the editor digests the caret/text state
        // the test set directly before the key handler ran (under load that digestion can lag a
        // frame, and the command would then act on stale state - CI flake).
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
    }

    /// <summary>
    /// Waits for a state that a key press is expected to produce. The headless KeyPress dispatch
    /// rides the dispatcher queue, so under CPU load the editor state can lag a few frames behind
    /// the synchronous call; asserting instantly then fails nondeterministically (CI flake with a
    /// different victim test every run). Polling with a delay pumps the headless dispatcher until
    /// the expected state appears, or fails with a descriptive message on timeout.
    /// </summary>
    private static async Task WaitUntil(Func<bool> condition, string failureMessage, int timeoutMs = 500)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        while (!condition())
        {
            if (stopwatch.ElapsedMilliseconds > timeoutMs)
            {
                Assert.Fail(failureMessage);
            }

            // RunJobs pumps any queued dispatcher work the condition may depend on; Task.Delay
            // additionally yields the thread so the headless session can process input.
            Dispatcher.UIThread.RunJobs();
            await Task.Delay(10);
        }
    }

    [AvaloniaFact]
    public async Task OnlyTheVisibleLinesAreLaidOut()
    {
        var (_, editor) = await Show(MakeSrt(20_000)); // 4 lines per paragraph plus a trailing blank
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
    public async Task ScrollingBuildsOnlyTheNewlyVisibleLines()
    {
        var (_, editor) = await Show(MakeSrt(20_000));
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
    public async Task ScrollOffsetIsClampedToTheExtent()
    {
        var (_, editor) = await Show(MakeSrt(10));
        var view = editor.View;

        view.ScrollOffset = new Vector(-500, -500);
        Assert.Equal(new Vector(0, 0), view.ScrollOffset);

        view.ScrollOffset = new Vector(0, 10_000_000);
        Assert.True(view.ScrollOffset.Y <= view.Extent.Height);
    }

    [AvaloniaFact]
    public async Task GutterCountsAndFollowsTheView()
    {
        var (_, editor) = await Show(MakeSrt(100));

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
    public async Task ShowLineNumbersHidesTheGutter()
    {
        var (_, editor) = await Show(MakeSrt(10));
        var gutter = ((Grid)((Decorator)editor).Child!).Children[0];

        editor.ShowLineNumbers = false;
        Assert.False(gutter.IsVisible);
    }

    [AvaloniaFact]
    public async Task CaretMovesByCharacterAndLine()
    {
        var (window, editor) = await Show("one\r\ntwo\r\nthree");
        var view = editor.View;

        view.CaretOffset = 0;
        Press(window, Key.Right);
        await WaitUntil(() => view.CaretOffset == 1, "Right should move the caret to offset 1");

        Press(window, Key.End);
        await WaitUntil(() => view.CaretOffset == 3, "End should move the caret to the end of line 1");

        // Past the end of a line the caret lands on the next line's start, not inside the break.
        Press(window, Key.Right);
        await WaitUntil(() => view.CaretOffset == 5, "Right past the line end should land on line 2 start");

        Press(window, Key.Down);
        await WaitUntil(() => view.CaretLine == 2, "Down should move the caret to line 2");

        // Left from the start of a line steps back over the whole break, onto the end of line 1.
        Press(window, Key.Left);
        await WaitUntil(() => view.CaretLine == 1 && view.CaretOffset == 8, "Left should step back over the break");
    }

    [AvaloniaFact]
    public async Task ShiftArrowSelectsAndTypingReplacesTheSelection()
    {
        var (window, editor) = await Show("hello world");
        var view = editor.View;

        view.CaretOffset = 0;
        for (var i = 0; i < 5; i++)
        {
            Press(window, Key.Right, RawInputModifiers.Shift);
        }

        await WaitUntil(() => view.SelectedText == "hello", "Shift+Right x5 should select \"hello\"");

        view.InsertText("bye");
        await WaitUntil(() => editor.Text == "bye world", "typing over the selection should replace it");
        await WaitUntil(() => view.CaretOffset == 3, "caret should sit at the end of the inserted text");
    }

    [AvaloniaFact]
    public async Task EnterSplitsTheLineAndBackspaceJoinsItBack()
    {
        var (window, editor) = await Show("onetwo");
        var view = editor.View;

        view.CaretOffset = 3;
        Press(window, Key.Enter);
        await WaitUntil(() => view.Document.LineCount == 2, "Enter should split the line");
        await WaitUntil(() => editor.Text == "one" + view.Document.NewLine + "two", "Enter should split at the caret");

        Press(window, Key.Back);
        await WaitUntil(() => view.Document.LineCount == 1, "Backspace should join the lines");
        await WaitUntil(() => editor.Text == "onetwo" && view.CaretOffset == 3, "Backspace should restore the text and caret");
    }

    [AvaloniaFact]
    public async Task TypedCharactersUndoAsOneStep()
    {
        var (_, editor) = await Show("start");
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
    public async Task UndoRestoresDeletedTextAndTheCaret()
    {
        var (_, editor) = await Show("one two three");
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
    public async Task ReadOnlyRefusesEveryEdit()
    {
        var (window, editor) = await Show("keep me", readOnly: true);
        var view = editor.View;

        view.CaretOffset = 0;
        view.InsertText("x");
        Press(window, Key.Delete);
        view.SelectAll();
        Press(window, Key.Back);

        // These are no-transition assertions: waiting would pass before a delayed key was handled.
        // Press settles the dispatcher, so assert the unchanged state synchronously afterwards.
        Assert.Equal("keep me", editor.Text);
        Assert.False(view.CanUndo);
    }

    [AvaloniaFact]
    public async Task SelectAllCoversTheWholeDocument()
    {
        var text = MakeSrt(5);
        var (_, editor) = await Show(text);

        editor.SelectAll();

        Assert.Equal(0, editor.SelectionStart);
        Assert.Equal(text.Length, editor.SelectionLength);
        Assert.Equal(text, editor.SelectedText);
    }

    [AvaloniaFact]
    public async Task SettingTextKeepsTheDocumentAndCaretConsistent()
    {
        var (_, editor) = await Show("first");

        editor.Text = "a\r\nb\r\nc";

        Assert.Equal(3, editor.Document.LineCount);
        Assert.True(editor.CaretOffset <= editor.Document.TextLength);
        Assert.False(editor.View.CanUndo);
    }

    [AvaloniaFact]
    public async Task BringCaretIntoViewScrollsToTheCaret()
    {
        var (_, editor) = await Show(MakeSrt(2_000));
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
    public async Task AltUpMovesTheCaretLineUpAndKeepsTheColumn()
    {
        var (window, editor) = await Show(Lines("one", "two", "three"));
        var view = editor.View;

        view.CaretOffset = view.Document.GetOffset(1, 2); // inside "two"
        Press(window, Key.Up, RawInputModifiers.Alt);

        await WaitUntil(() => editor.Text == Lines("two", "one", "three"), "Alt+Up should move the caret line up");
        await WaitUntil(() => view.Document.GetPosition(view.CaretOffset) == new SyntaxTextPosition(0, 2), "the caret should keep its column");
    }

    [AvaloniaFact]
    public async Task AltDownMovesTheWholeSelectedBlock()
    {
        var (window, editor) = await Show(Lines("a", "b", "c", "d"));
        var view = editor.View;

        // Select from inside line 0 to inside line 1 - both lines count as touched.
        view.Select(view.Document.GetOffset(0, 0), view.Document.GetOffset(1, 1));
        Press(window, Key.Down, RawInputModifiers.Alt);

        await WaitUntil(() => editor.Text == Lines("c", "a", "b", "d"), "Alt+Down should move the selected block");
        await WaitUntil(() => view.SelectedText == "a\r\nb", "the selection should move with the block");
    }

    [AvaloniaFact]
    public async Task MovingLinesStaysInsideTheDocument()
    {
        var (window, editor) = await Show(Lines("first", "last"));
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
    public async Task MoveLineUndoesAsOneStep()
    {
        var (window, editor) = await Show(Lines("one", "two", "three"));
        var view = editor.View;

        view.CaretOffset = view.Document.GetLineStartOffset(2);
        Press(window, Key.Up, RawInputModifiers.Alt);
        await WaitUntil(() => editor.Text == Lines("one", "three", "two"), "Alt+Up should move line 3 above line 2");

        view.Undo();
        await WaitUntil(() => editor.Text == Lines("one", "two", "three"), "Undo should restore the original order");
        await WaitUntil(() => !view.CanUndo, "the move must undo as one step");
    }

    [AvaloniaFact]
    public async Task DuplicateLineInsertsTheCopyBelowAndLandsOnIt()
    {
        var (window, editor) = await Show(Lines("one", "two"));
        var view = editor.View;

        view.CaretOffset = view.Document.GetOffset(0, 1);
        Press(window, Key.D, ControlOrMeta);

        await WaitUntil(() => editor.Text == Lines("one", "one", "two"), "Ctrl+D should duplicate the line below");
        await WaitUntil(() => view.Document.GetPosition(view.CaretOffset) == new SyntaxTextPosition(1, 1), "the caret should land on the copy");

        // Repeating it duplicates the copy, not the original.
        Press(window, Key.D, ControlOrMeta);
        await WaitUntil(() => editor.Text == Lines("one", "one", "one", "two"), "repeating Ctrl+D should duplicate the copy");
    }

    [AvaloniaFact]
    public async Task DeleteLineTakesTheLineBreakWithIt()
    {
        var (window, editor) = await Show(Lines("one", "two", "three"));
        var view = editor.View;

        view.CaretOffset = view.Document.GetLineStartOffset(1);
        Press(window, Key.K, ControlOrMeta | RawInputModifiers.Shift);

        await WaitUntil(() => editor.Text == Lines("one", "three"), "Ctrl+Shift+K should delete the whole line");
        await WaitUntil(() => view.Document.LineCount == 2, "the line break must go with the line");
    }

    [AvaloniaFact]
    public async Task DeletingTheLastLineTakesTheBreakAboveIt()
    {
        var (window, editor) = await Show(Lines("one", "two"));
        var view = editor.View;

        view.CaretOffset = view.Document.GetLineStartOffset(1);
        Press(window, Key.K, ControlOrMeta | RawInputModifiers.Shift);

        await WaitUntil(() => editor.Text == "one", "deleting the last line must keep the first");
        await WaitUntil(() => view.Document.LineCount == 1, "the break above the deleted line must go too");
    }

    [AvaloniaFact]
    public async Task DeleteLineOnTheOnlyLineJustEmptiesIt()
    {
        var (window, editor) = await Show("only");
        var view = editor.View;

        Press(window, Key.K, ControlOrMeta | RawInputModifiers.Shift);

        await WaitUntil(() => editor.Text == string.Empty, "deleting the only line must empty the document");
        await WaitUntil(() => view.Document.LineCount == 1, "the only line must stay, just empty");
    }

    [AvaloniaFact]
    public async Task DeleteWordLeftAndRightRemoveOneWord()
    {
        var (window, editor) = await Show("one two three");
        var view = editor.View;

        view.CaretOffset = 7; // right after "two"
        Press(window, Key.Back, WordModifier);
        await WaitUntil(() => editor.Text == "one  three", "Ctrl+Backspace should remove the word to the left");

        view.CaretOffset = 4;
        Press(window, Key.Delete, WordModifier);
        await WaitUntil(() => editor.Text == "one ", "Ctrl+Delete should remove the word to the right");
    }

    [AvaloniaFact]
    public async Task ReadOnlyRefusesTheLineCommands()
    {
        var text = Lines("one", "two", "three");
        var (window, editor) = await Show(text, readOnly: true);

        editor.View.CaretOffset = editor.Document.GetLineStartOffset(1);
        Press(window, Key.Up, RawInputModifiers.Alt);
        Press(window, Key.D, ControlOrMeta);
        Press(window, Key.K, ControlOrMeta | RawInputModifiers.Shift);
        Press(window, Key.Back, WordModifier);

        // Each Press settles the dispatcher. Polling unchanged state would return before a delayed
        // command and could hide a regression, so check the no-op contract directly.
        Assert.Equal(text, editor.Text);
        Assert.False(editor.View.CanUndo);
    }

    [AvaloniaFact]
    public async Task ReplaceAllTextIsOneUndoStep()
    {
        var (_, editor) = await Show(Lines("one", "two"));

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
