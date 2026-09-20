using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Features.Shared.TextBoxUtils;

namespace UITests.Features;

/// <summary>
/// Text drag-and-drop for the subtitle edit boxes (#14534): Avalonia's TextBox has none, so the
/// helper takes over the press inside a selection and handles drops itself.
/// </summary>
public class TextBoxTextDragDropTests
{
    private static (Window window, TextBox textBox) Show(string text)
    {
        var textBox = new TextBox { AcceptsReturn = true, Text = text, Width = 400, FontSize = 14 };
        TextBoxTextDragDrop.Attach(textBox);
        var window = new Window { Width = 500, Height = 200, Content = textBox };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return (window, textBox);
    }

    /// <summary>
    /// Window-space point just inside the left edge of character <paramref name="index"/> -
    /// the left edge, not the middle, so the hit test resolves to that index on every
    /// platform's font metrics (a mid-glyph point rounded to the trailing edge on CI).
    /// </summary>
    private static Point PointAt(Window window, TextBox textBox, int index)
    {
        var presenter = textBox.GetVisualDescendants().OfType<TextPresenter>().First();
        var rect = presenter.TextLayout.HitTestTextPosition(index);
        var local = new Point(rect.X + 1, rect.Y + rect.Height / 2);
        return presenter.TranslatePoint(local, window)!.Value;
    }

    // Every drop below is preceded by a DragOver: the headless platform (like the real ones)
    // routes a drop only to a target it has already dragged over.
    private static DataTransfer Text(string text)
    {
        var transfer = new DataTransfer();
        transfer.Add(DataTransferItem.CreateText(text));
        return transfer;
    }

    [AvaloniaFact]
    public void ClickInsideSelection_WithoutDragging_PlacesTheCaretLikeAPlainClick()
    {
        var (window, textBox) = Show("hello brave world");
        try
        {
            textBox.SelectionStart = 6;
            textBox.SelectionEnd = 11; // "brave"
            var point = PointAt(window, textBox, 8);

            window.MouseDown(point, MouseButton.Left);
            // Between press and release the selection must survive - that is what makes it draggable.
            Assert.Equal("brave", textBox.SelectedText);

            window.MouseUp(point, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(string.Empty, textBox.SelectedText);
            Assert.InRange(textBox.CaretIndex, 7, 9);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void SmallMoveInsideSelection_KeepsTheSelectionIntact()
    {
        // #14568: Avalonia captures the pointer to the presenter on the press even though the box
        // never handled it, and the box's move handler then drags SelectionEnd along with the
        // pointer - so a hand that wobbles before the drag threshold used to lose characters.
        var (window, textBox) = Show("hello brave world");
        try
        {
            textBox.SelectionStart = 6;
            textBox.SelectionEnd = 11; // "brave"
            var point = PointAt(window, textBox, 10);

            window.MouseDown(point, MouseButton.Left);
            window.MouseMove(new Point(point.X - 2, point.Y), RawInputModifiers.LeftMouseButton);
            Dispatcher.UIThread.RunJobs();
            Assert.Equal("brave", textBox.SelectedText);

            window.MouseMove(new Point(point.X - 30, point.Y), RawInputModifiers.LeftMouseButton);
            Dispatcher.UIThread.RunJobs();
            Assert.Equal("brave", textBox.SelectedText);

            window.MouseUp(new Point(point.X - 30, point.Y), MouseButton.Left);
            Dispatcher.UIThread.RunJobs();
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void PressAtTheEndOfTheSelection_CountsAsInside()
    {
        var (window, textBox) = Show("hello brave world");
        try
        {
            textBox.SelectionStart = 6;
            textBox.SelectionEnd = 11;
            var point = PointAt(window, textBox, 11); // hit-tests to selectionEnd

            window.MouseDown(point, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();
            Assert.Equal("brave", textBox.SelectedText);

            window.MouseUp(point, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void ClickOutsideSelection_IsLeftToTheTextBox()
    {
        var (window, textBox) = Show("hello brave world");
        try
        {
            textBox.SelectionStart = 6;
            textBox.SelectionEnd = 11;
            var point = PointAt(window, textBox, 2);

            window.MouseDown(point, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();

            // The box's own press handler ran and collapsed the selection to the click.
            Assert.Equal(string.Empty, textBox.SelectedText);
            Assert.InRange(textBox.CaretIndex, 1, 3);
            window.MouseUp(point, MouseButton.Left);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void DropFromElsewhere_InsertsAtThePointerAndSelectsIt()
    {
        var (window, textBox) = Show("hello world");
        try
        {
            TextBoxTextDragDrop.SetDragSourceForTest(null, 0, 0);
            var point = PointAt(window, textBox, 6); // before "world"

            window.DragDrop(point, RawDragEventType.DragOver, Text("big "), DragDropEffects.Copy | DragDropEffects.Move, RawInputModifiers.None);
            window.DragDrop(point, RawDragEventType.Drop, Text("big "), DragDropEffects.Copy | DragDropEffects.Move, RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();

            Assert.Equal("hello big world", textBox.Text);
            Assert.Equal("big ", textBox.SelectedText);
        }
        finally
        {
            window.Close();
        }
    }

    [Theory]
    // Between two words: a space is added on the side that lacks one.
    [InlineData("hello world", 5, "brave", "hello brave world", "brave")]
    [InlineData("hello world", 6, "brave", "hello brave world", "brave")]
    // Spaces the text carries are kept where needed and dropped at a boundary; what is kept
    // stays part of the selection, the added padding does not.
    [InlineData("hello world", 6, " brave ", "hello brave world", "brave ")]
    [InlineData("hello world", 11, " brave ", "hello world brave", " brave")]
    [InlineData("hello world", 0, " brave ", "brave hello world", "brave ")]
    // No space in front of closing punctuation (SE4's list plus the comma).
    [InlineData("hello.", 5, "brave", "hello brave.", "brave")]
    [InlineData("hello, world", 5, "brave", "hello brave, world", "brave")]
    [InlineData("hello<i>x</i>", 5, "brave", "hello brave<i>x</i>", "brave")]
    // A line break is a boundary: no space after it, none before it.
    [InlineData("hello\nworld", 6, "brave", "hello\nbrave world", "brave")]
    [InlineData("hello\nworld", 5, "brave", "hello brave\nworld", "brave")]
    [InlineData("hello\nworld", 6, " brave ", "hello\nbrave world", "brave ")]
    [InlineData("hello\nworld", 5, " brave ", "hello brave\nworld", " brave")]
    // Into an empty box: nothing to pad.
    [InlineData("", 0, "brave", "brave", "brave")]
    public void FitSpacing_FollowsSe4Rules(string current, int index, string dropped, string expected, string expectedSelected)
    {
        var fitted = TextBoxTextDragDrop.FitSpacing(current, index, dropped);

        Assert.Equal(expected, current.Insert(index, fitted.Text));
        Assert.Equal(expectedSelected, fitted.Text.Substring(fitted.Skip, fitted.Length));
    }

    [AvaloniaFact]
    public void DropFromElsewhere_BetweenTwoWords_AddsTheMissingSpace()
    {
        var (window, textBox) = Show("hello world");
        try
        {
            TextBoxTextDragDrop.SetDragSourceForTest(null, 0, 0);
            var point = PointAt(window, textBox, 5); // right after "hello"

            window.DragDrop(point, RawDragEventType.DragOver, Text("brave"), DragDropEffects.Copy, RawInputModifiers.None);
            window.DragDrop(point, RawDragEventType.Drop, Text("brave"), DragDropEffects.Copy, RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();

            Assert.Equal("hello brave world", textBox.Text);
            Assert.Equal("brave", textBox.SelectedText);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void DragOver_ShowsTheDropTargetBar_UntilTheDragLeavesOrDrops()
    {
        var (window, textBox) = Show("hello world");
        try
        {
            TextBoxTextDragDrop.SetDragSourceForTest(null, 0, 0);
            var point = PointAt(window, textBox, 5);

            window.DragDrop(point, RawDragEventType.DragOver, Text("brave"), DragDropEffects.Copy, RawInputModifiers.None);
            Assert.True(TextBoxTextDragDrop.IsIndicatorShownForTest(textBox));

            window.DragDrop(point, RawDragEventType.DragLeave, Text("brave"), DragDropEffects.Copy, RawInputModifiers.None);
            Assert.False(TextBoxTextDragDrop.IsIndicatorShownForTest(textBox));

            window.DragDrop(point, RawDragEventType.DragOver, Text("brave"), DragDropEffects.Copy, RawInputModifiers.None);
            Assert.True(TextBoxTextDragDrop.IsIndicatorShownForTest(textBox));

            window.DragDrop(point, RawDragEventType.Drop, Text("brave"), DragDropEffects.Copy, RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();
            Assert.False(TextBoxTextDragDrop.IsIndicatorShownForTest(textBox));
            Assert.Equal("hello brave world", textBox.Text);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void DragOver_TheDraggedRangeItself_RefusesTheDropAndShowsNoBar()
    {
        var (window, textBox) = Show("hello brave world");
        try
        {
            TextBoxTextDragDrop.SetDragSourceForTest(textBox, 6, 5);
            var inside = PointAt(window, textBox, 8);

            window.DragDrop(inside, RawDragEventType.DragOver, Text("brave"), DragDropEffects.Copy | DragDropEffects.Move, RawInputModifiers.None);
            Assert.False(TextBoxTextDragDrop.IsIndicatorShownForTest(textBox));

            var outside = PointAt(window, textBox, 2);
            window.DragDrop(outside, RawDragEventType.DragOver, Text("brave"), DragDropEffects.Copy | DragDropEffects.Move, RawInputModifiers.None);
            Assert.True(TextBoxTextDragDrop.IsIndicatorShownForTest(textBox));
        }
        finally
        {
            TextBoxTextDragDrop.SetDragSourceForTest(null, 0, 0);
            window.Close();
        }
    }

    [AvaloniaFact]
    public void DropWithinTheSameBox_LiftingAWordBeforePunctuation_LeavesNoStraySpace()
    {
        var (window, textBox) = Show("hello brave." + Environment.NewLine + "world");
        try
        {
            // Dragging "brave" (6..11) to the very end.
            textBox.SelectionStart = 6;
            textBox.SelectionEnd = 11;
            TextBoxTextDragDrop.SetDragSourceForTest(textBox, 6, 5);
            var end = textBox.Text!.Length;
            var point = PointAt(window, textBox, end);

            window.DragDrop(point, RawDragEventType.DragOver, Text("brave"), DragDropEffects.Copy | DragDropEffects.Move, RawInputModifiers.None);
            window.DragDrop(point, RawDragEventType.Drop, Text("brave"), DragDropEffects.Copy | DragDropEffects.Move, RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();

            Assert.Equal("hello." + Environment.NewLine + "world brave", textBox.Text);
            Assert.Equal("brave", textBox.SelectedText);
        }
        finally
        {
            TextBoxTextDragDrop.SetDragSourceForTest(null, 0, 0);
            window.Close();
        }
    }

    [AvaloniaFact]
    public void DropWithinTheSameBox_LiftingTheLastWordOfALine_LeavesNoTrailingSpace()
    {
        var (window, textBox) = Show("hello brave" + Environment.NewLine + "world");
        try
        {
            // Dragging "brave" (6..11) to the start of the text.
            textBox.SelectionStart = 6;
            textBox.SelectionEnd = 11;
            TextBoxTextDragDrop.SetDragSourceForTest(textBox, 6, 5);
            var point = PointAt(window, textBox, 0);

            window.DragDrop(point, RawDragEventType.DragOver, Text("brave"), DragDropEffects.Copy | DragDropEffects.Move, RawInputModifiers.None);
            window.DragDrop(point, RawDragEventType.Drop, Text("brave"), DragDropEffects.Copy | DragDropEffects.Move, RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();

            Assert.Equal("brave hello" + Environment.NewLine + "world", textBox.Text);
            Assert.Equal("brave", textBox.SelectedText);
        }
        finally
        {
            TextBoxTextDragDrop.SetDragSourceForTest(null, 0, 0);
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Drop_NormalizesLineBreaksLikePaste()
    {
        var (window, textBox) = Show("");
        try
        {
            TextBoxTextDragDrop.SetDragSourceForTest(null, 0, 0);
            var point = PointAt(window, textBox, 0);

            window.DragDrop(point, RawDragEventType.DragOver, Text("one\ntwo"), DragDropEffects.Copy, RawInputModifiers.None);
            window.DragDrop(point, RawDragEventType.Drop, Text("one\ntwo"), DragDropEffects.Copy, RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();

            Assert.Equal("one" + Environment.NewLine + "two", textBox.Text);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Drop_OnAReadOnlyBox_IsRefused()
    {
        var (window, textBox) = Show("untouched");
        try
        {
            textBox.IsReadOnly = true;
            TextBoxTextDragDrop.SetDragSourceForTest(null, 0, 0);
            var point = PointAt(window, textBox, 3);

            window.DragDrop(point, RawDragEventType.DragOver, Text("x"), DragDropEffects.Copy, RawInputModifiers.None);
            window.DragDrop(point, RawDragEventType.Drop, Text("x"), DragDropEffects.Copy, RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();
            Assert.Equal("untouched", textBox.Text);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void DropWithinTheSameBox_MovesTheTextAndCollapsesTheSeam()
    {
        var (window, textBox) = Show("hello brave world");
        try
        {
            // Dragging "brave " (6..12) to the end of the line.
            textBox.SelectionStart = 6;
            textBox.SelectionEnd = 12;
            TextBoxTextDragDrop.SetDragSourceForTest(textBox, 6, 6);
            var point = PointAt(window, textBox, 17);

            window.DragDrop(point, RawDragEventType.DragOver, Text("brave "), DragDropEffects.Copy | DragDropEffects.Move, RawInputModifiers.None);
            window.DragDrop(point, RawDragEventType.Drop, Text("brave "), DragDropEffects.Copy | DragDropEffects.Move, RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();
            // The trailing space it carried is dropped at the end of the text and a space is
            // put in front of it instead.
            Assert.Equal("hello world brave", textBox.Text);
            Assert.Equal("brave", textBox.SelectedText);
        }
        finally
        {
            TextBoxTextDragDrop.SetDragSourceForTest(null, 0, 0);
            window.Close();
        }
    }

    [AvaloniaFact]
    public void DropWithinTheSameBox_WithCtrl_Copies()
    {
        var (window, textBox) = Show("hello brave world");
        try
        {
            textBox.SelectionStart = 6;
            textBox.SelectionEnd = 12;
            TextBoxTextDragDrop.SetDragSourceForTest(textBox, 6, 6);
            var point = PointAt(window, textBox, 0);

            window.DragDrop(point, RawDragEventType.DragOver, Text("brave "), DragDropEffects.Copy | DragDropEffects.Move, RawInputModifiers.Control);
            window.DragDrop(point, RawDragEventType.Drop, Text("brave "), DragDropEffects.Copy | DragDropEffects.Move, RawInputModifiers.Control);
            Dispatcher.UIThread.RunJobs();
            Assert.Equal("brave hello brave world", textBox.Text);
        }
        finally
        {
            TextBoxTextDragDrop.SetDragSourceForTest(null, 0, 0);
            window.Close();
        }
    }

    [AvaloniaFact]
    public void DropOntoTheDraggedRangeItself_ChangesNothing()
    {
        var (window, textBox) = Show("hello brave world");
        try
        {
            textBox.SelectionStart = 6;
            textBox.SelectionEnd = 11;
            TextBoxTextDragDrop.SetDragSourceForTest(textBox, 6, 5);
            var point = PointAt(window, textBox, 8);

            window.DragDrop(point, RawDragEventType.DragOver, Text("brave"), DragDropEffects.Copy | DragDropEffects.Move, RawInputModifiers.None);
            window.DragDrop(point, RawDragEventType.Drop, Text("brave"), DragDropEffects.Copy | DragDropEffects.Move, RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();

            Assert.Equal("hello brave world", textBox.Text);
        }
        finally
        {
            TextBoxTextDragDrop.SetDragSourceForTest(null, 0, 0);
            window.Close();
        }
    }
}
