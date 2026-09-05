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

    /// <summary>Window-space point at the middle of character <paramref name="index"/>.</summary>
    private static Point PointAt(Window window, TextBox textBox, int index)
    {
        var presenter = textBox.GetVisualDescendants().OfType<TextPresenter>().First();
        var rect = presenter.TextLayout.HitTestTextPosition(index);
        var local = new Point(rect.X + Math.Max(rect.Width / 2, 1), rect.Y + rect.Height / 2);
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
            Assert.Equal("hello worldbrave ", textBox.Text);
            Assert.Equal("brave ", textBox.SelectedText);
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
