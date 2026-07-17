using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using AvaloniaEdit;
using Nikse.SubtitleEdit.Logic;
using System.Runtime.InteropServices;

namespace UITests.Logic;

/// <summary>
/// Mouse hit-testing and selection rendering in the right-to-left subtitle text box, including
/// lines with formatting tags. Tags are rendered as atomic literal elements by
/// <see cref="RtlTagIsolationGenerator"/> (see its remarks for why); these tests pin down that
/// selection highlights land on the text and that clicks map to the correct document offsets -
/// both were broken before, with the highlight drawn beside the text ("opposite" selection).
/// </summary>
public class RightToLeftSelectionTests
{
    private const string Arabic = "مرحبا بالعالم هذا اختبار";
    private const string Tagged = "{\\an8}<i>هذا اختبار.</i>";
    private const int Width = 400;
    private const int Height = 100;

    private static (Window window, TextEditor editor) MakeWindow(string text)
    {
        var editor = new TextEditor
        {
            Text = text,
            WordWrap = true,
            FontSize = 20,
            Foreground = Brushes.White,
            Background = Brushes.Black,
        };
        // Matches the app: FlowDirection on the TextView (RightToLeftHelper) and the tag
        // isolation generator (TextEditorWrapper).
        editor.TextArea.TextView.FlowDirection = FlowDirection.RightToLeft;
        editor.TextArea.TextView.ElementGenerators.Add(new RtlTagIsolationGenerator(editor.TextArea));
        var window = new Window { Content = editor, Width = Width, Height = Height, Background = Brushes.Black };
        window.Show();
        window.Measure(new Size(Width, Height));
        window.Arrange(new Rect(0, 0, Width, Height));
        Dispatcher.UIThread.RunJobs();
        return (window, editor);
    }

    /// <summary>
    /// Horizontal extent of the selection highlight (blue-ish pixels). The headless frame may
    /// be RGBA or BGRA; a run of several pixels is required so single-pixel antialiasing
    /// fringes of glyphs don't count.
    /// </summary>
    private static (int MinX, int MaxX) MeasureSelectionInk(Window window)
    {
        window.Measure(new Size(Width, Height));
        window.Arrange(new Rect(0, 0, Width, Height));
        Dispatcher.UIThread.RunJobs();
        var frame = window.CaptureRenderedFrame();
        Assert.NotNull(frame);

        var rgba = frame!.Format == Avalonia.Platform.PixelFormats.Rgba8888;
        int minX = int.MaxValue, maxX = -1;
        using (var fb = frame.Lock())
        {
            var row = new int[frame.PixelSize.Width];
            for (var y = 0; y < frame.PixelSize.Height; y++)
            {
                Marshal.Copy(fb.Address + y * fb.RowBytes, row, 0, row.Length);
                var runStart = -1;
                for (var x = 0; x <= row.Length; x++)
                {
                    var isBlue = false;
                    if (x < row.Length)
                    {
                        var px = row[x];
                        var r = rgba ? px & 0xFF : (px >> 16) & 0xFF;
                        var g = (px >> 8) & 0xFF;
                        var b = rgba ? (px >> 16) & 0xFF : px & 0xFF;
                        isBlue = b > 100 && b > r + 40 && b > g + 40;
                    }

                    if (isBlue)
                    {
                        if (runStart < 0) runStart = x;
                    }
                    else if (runStart >= 0)
                    {
                        if (x - runStart >= 4)
                        {
                            if (runStart < minX) minX = runStart;
                            if (x - 1 > maxX) maxX = x - 1;
                        }

                        runStart = -1;
                    }
                }
            }
        }

        return (minX, maxX);
    }

    [AvaloniaFact]
    public void Click_NearRightEdge_PutsCaretAtDocumentStart()
    {
        var (window, editor) = MakeWindow(Arabic);

        // RTL: the first character is at the far right.
        window.MouseDown(new Point(Width - 4, 15), MouseButton.Left);
        window.MouseUp(new Point(Width - 4, 15), MouseButton.Left);
        Dispatcher.UIThread.RunJobs();

        Assert.True(editor.CaretOffset <= 1, $"caret at {editor.CaretOffset}, expected 0/1 (right edge = text start in RTL)");
    }

    [AvaloniaFact]
    public void Click_LeftOfText_PutsCaretAtDocumentEnd()
    {
        var (window, editor) = MakeWindow(Arabic);

        // RTL: left of the text block is past the end of the line.
        window.MouseDown(new Point(4, 15), MouseButton.Left);
        window.MouseUp(new Point(4, 15), MouseButton.Left);
        Dispatcher.UIThread.RunJobs();

        Assert.True(editor.CaretOffset >= editor.Text.Length - 1,
            $"caret at {editor.CaretOffset}, expected near {editor.Text.Length} (left side = text end in RTL)");
    }

    [AvaloniaFact]
    public void Drag_FromRightEdgeToMiddle_SelectsLeadingText()
    {
        var (window, editor) = MakeWindow(Arabic);

        window.MouseDown(new Point(Width - 4, 15), MouseButton.Left);
        window.MouseMove(new Point(Width / 2, 15));
        window.MouseUp(new Point(Width / 2, 15), MouseButton.Left);
        Dispatcher.UIThread.RunJobs();

        Assert.True(editor.SelectionLength > 0, "drag produced no selection");
        Assert.True(editor.SelectionStart <= 1,
            $"selection [{editor.SelectionStart}..{editor.SelectionStart + editor.SelectionLength}), expected to start at document start");
    }

    [AvaloniaFact]
    public void DoubleClick_OnFirstWord_SelectsFirstWord()
    {
        var (window, editor) = MakeWindow(Arabic);

        var p = new Point(Width - 20, 15);
        window.MouseDown(p, MouseButton.Left);
        window.MouseUp(p, MouseButton.Left);
        window.MouseDown(p, MouseButton.Left);
        window.MouseUp(p, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();

        Assert.True(editor.SelectionLength > 0, "double-click produced no selection");
        Assert.True(editor.SelectionStart <= 1,
            $"selection [{editor.SelectionStart}..{editor.SelectionStart + editor.SelectionLength}), expected the first word (starts at 0)");
    }

    [AvaloniaFact]
    public void TaggedLine_TagsKeepLiteralLayout()
    {
        // "{\an8}" is the logical start, so it must sit at the far right; "</i>" is the logical
        // end, at the far left of the ink. Each tag is one atomic element (visual length 1).
        var (_, editor) = MakeWindow(Tagged);
        var visualLine = editor.TextArea.TextView.VisualLines[0];
        var textLine = visualLine.TextLines[0];

        var firstTag = textLine.GetTextBounds(0, 1)[0].Rectangle;
        var closingTag = textLine.GetTextBounds(visualLine.VisualLength - 1, 1)[0].Rectangle;

        Assert.True(firstTag.Right > Width - 10, $"{{\\an8}} ends at {firstTag.Right}, expected at the right edge");
        Assert.True(closingTag.Left < firstTag.Left, "</i> should be left of {\\an8}");
        Assert.True(System.Math.Abs(closingTag.Left - textLine.Start) < 1, "</i> should start at the line's left ink edge");
    }

    [AvaloniaFact]
    public void TaggedLine_DragFromRightEdge_SelectsFromDocumentStart()
    {
        var (window, editor) = MakeWindow(Tagged);

        window.MouseDown(new Point(Width - 4, 15), MouseButton.Left);
        window.MouseMove(new Point(Width / 2, 15));
        window.MouseUp(new Point(Width / 2, 15), MouseButton.Left);
        Dispatcher.UIThread.RunJobs();

        Assert.True(editor.SelectionLength > 0, "drag produced no selection");
        Assert.True(editor.SelectionStart <= 1,
            $"selection [{editor.SelectionStart}..{editor.SelectionStart + editor.SelectionLength}), " +
            $"selected text '{editor.SelectedText}'");
    }

    [AvaloniaFact]
    public void TaggedLine_ProgrammaticSelectionOfArabicWord_HighlightsWhereTheWordIs()
    {
        var (window, editor) = MakeWindow(Tagged);

        // Select the Arabic word "هذا" - visually right of center, after the tag island.
        editor.SelectionStart = Tagged.IndexOf('ه');
        editor.SelectionLength = 3;
        Dispatcher.UIThread.RunJobs();

        var (minX, maxX) = MeasureSelectionInk(window);
        Assert.True(maxX >= 0, "no selection highlight rendered");
        Assert.True(minX > Width / 2,
            $"selection highlight spans [{minX}..{maxX}], expected in the right half");
        Assert.True(maxX - minX < 80,
            $"selection highlight spans [{minX}..{maxX}], expected only the width of one short word");
    }

    [AvaloniaFact]
    public void TaggedLine_SelectAll_HighlightCoversWholeLine()
    {
        var (window, editor) = MakeWindow(Tagged);

        editor.SelectAll();
        Dispatcher.UIThread.RunJobs();

        var (minX, maxX) = MeasureSelectionInk(window);
        Assert.True(maxX >= 0, "no selection highlight rendered");
        Assert.True(maxX > Width - 30, $"selection highlight [{minX}..{maxX}] should reach the right edge");
        Assert.True(minX < Width / 2, $"selection highlight [{minX}..{maxX}] should cover the left-side </i> too");
    }

    [AvaloniaFact]
    public void TaggedLine_CaretInsideTag_OpensTagForEditing()
    {
        // A tag is atomic while the caret is elsewhere; placing the caret at or inside the tag
        // while the text area is focused must open it into plain per-character text so it can
        // be edited.
        var (_, editor) = MakeWindow(Tagged);
        var textView = editor.TextArea.TextView;

        // Unfocused: tags closed even though the caret (at 0) touches the first tag.
        var closed = textView.VisualLines[0];
        Assert.Equal(1, closed.GetVisualColumn(6) - closed.GetVisualColumn(0));

        editor.TextArea.Focus();
        editor.CaretOffset = 3; // inside {\an8}
        Dispatcher.UIThread.RunJobs();
        textView.EnsureVisualLines();

        // Open: the tag's six characters occupy six visual columns again.
        var open = textView.VisualLines[0];
        Assert.Equal(6, open.GetVisualColumn(6) - open.GetVisualColumn(0));

        editor.CaretOffset = 15; // back out into the Arabic text
        Dispatcher.UIThread.RunJobs();
        textView.EnsureVisualLines();

        var closedAgain = textView.VisualLines[0];
        Assert.Equal(1, closedAgain.GetVisualColumn(6) - closedAgain.GetVisualColumn(0));
    }

    [AvaloniaFact]
    public void TaggedLine_TypingInsideOpenedTag_InsertsAtCaret()
    {
        var (window, editor) = MakeWindow(Tagged);

        editor.TextArea.Focus();
        editor.CaretOffset = 4; // between "an" and "8" in {\an8}
        Dispatcher.UIThread.RunJobs();

        window.KeyTextInput("x");
        Dispatcher.UIThread.RunJobs();

        Assert.StartsWith("{\\anx8}", editor.Text);
    }
}
