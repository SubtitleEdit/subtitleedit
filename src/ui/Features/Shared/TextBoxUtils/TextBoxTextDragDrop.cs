using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Shared.TextBoxUtils;

/// <summary>
/// Gives a subtitle text box the text drag-and-drop SE4's edit boxes had (#14534): a selection
/// can be dragged out with the mouse and dropped into another text box (copy) or elsewhere in
/// the same box (move; hold Ctrl to copy), and text dragged from anywhere - the other edit box,
/// another application - can be dropped at the pointer.
///
/// Avalonia's <see cref="TextBox"/> has neither half of this, so both are done here: the press
/// that lands inside the current selection is taken in the tunnel phase (otherwise the box would
/// collapse the selection to a caret), and a drag starts once the pointer moves a few pixels;
/// a release without movement replays the click the box did not see. Drops insert through the
/// same path as paste, so line breaks get normalized, the paragraph binding fires and a
/// read-only box stays untouched.
/// </summary>
public static class TextBoxTextDragDrop
{
    private const double DragThreshold = 4;

    /// <summary>The box a drag currently originates from, with the dragged range - so the
    /// drop handler can tell "move within the same box" from "copy from elsewhere".</summary>
    private static TextBox? _dragSource;
    private static int _dragSourceStart;
    private static int _dragSourceLength;

    public static void Attach(TextBox textBox)
    {
        var session = new PressSession(textBox);
        textBox.AddHandler(InputElement.PointerPressedEvent, session.OnPointerPressed, RoutingStrategies.Tunnel);
        textBox.AddHandler(InputElement.PointerMovedEvent, session.OnPointerMoved, RoutingStrategies.Tunnel);
        textBox.AddHandler(InputElement.PointerReleasedEvent, session.OnPointerReleased, RoutingStrategies.Tunnel);
        textBox.AddHandler(InputElement.PointerCaptureLostEvent, session.OnPointerCaptureLost, RoutingStrategies.Tunnel);

        DragDrop.SetAllowDrop(textBox, true);
        textBox.AddHandler(DragDrop.DragOverEvent, OnDragOver);
        textBox.AddHandler(DragDrop.DropEvent, OnDrop);
    }

    /// <summary>
    /// Tracks one left-button press that landed inside the selection, until it either turns
    /// into a drag or is released as a plain click.
    /// </summary>
    private sealed class PressSession
    {
        private readonly TextBox _textBox;
        private PointerPressedEventArgs? _pending;
        private Point _pressPoint;

        public PressSession(TextBox textBox)
        {
            _textBox = textBox;
        }

        public void OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            _pending = null;

            var point = e.GetCurrentPoint(_textBox);
            if (!point.Properties.IsLeftButtonPressed || e.ClickCount != 1)
            {
                return;
            }

            // Ctrl+click is the context menu gesture on macOS (see the main window's text box
            // setup); Shift extends the selection. Leave both to their owners.
            if (e.KeyModifiers.HasFlag(KeyModifiers.Shift) ||
                (OperatingSystem.IsMacOS() && e.KeyModifiers.HasFlag(KeyModifiers.Control)))
            {
                return;
            }

            var selectionStart = Math.Min(_textBox.SelectionStart, _textBox.SelectionEnd);
            var selectionEnd = Math.Max(_textBox.SelectionStart, _textBox.SelectionEnd);
            if (selectionEnd <= selectionStart)
            {
                return;
            }

            var index = GetCharIndexAtPoint(_textBox, e);
            if (index == null || index < selectionStart || index >= selectionEnd)
            {
                return;
            }

            _pending = e;
            _pressPoint = e.GetPosition(_textBox);
            e.Handled = true; // keep the TextBox from collapsing the selection
        }

        public void OnPointerMoved(object? sender, PointerEventArgs e)
        {
            var pending = _pending;
            if (pending == null)
            {
                return;
            }

            if (!e.GetCurrentPoint(_textBox).Properties.IsLeftButtonPressed)
            {
                _pending = null;
                return;
            }

            var delta = e.GetPosition(_textBox) - _pressPoint;
            if (Math.Abs(delta.X) < DragThreshold && Math.Abs(delta.Y) < DragThreshold)
            {
                return;
            }

            _pending = null;
            e.Handled = true;
            _ = StartDragAsync(pending);
        }

        public void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (_pending == null)
            {
                return;
            }

            _pending = null;

            // The box never saw the press, so a plain click inside the selection would leave
            // the selection standing - replay what the box does on a click: caret to the point.
            var index = GetCharIndexAtPoint(_textBox, e);
            if (index != null)
            {
                _textBox.SelectionStart = index.Value;
                _textBox.SelectionEnd = index.Value;
                _textBox.CaretIndex = index.Value;
            }
        }

        public void OnPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
        {
            _pending = null;
        }

        private async Task StartDragAsync(PointerPressedEventArgs press)
        {
            var start = Math.Min(_textBox.SelectionStart, _textBox.SelectionEnd);
            var length = Math.Abs(_textBox.SelectionEnd - _textBox.SelectionStart);
            var text = _textBox.SelectedText;
            if (string.IsNullOrEmpty(text) || length == 0)
            {
                return;
            }

            _dragSource = _textBox;
            _dragSourceStart = start;
            _dragSourceLength = length;

            try
            {
                var transfer = new DataTransfer();
                transfer.Add(DataTransferItem.CreateText(text));
                await DragDrop.DoDragDropAsync(press, transfer, DragDropEffects.Copy | DragDropEffects.Move);
            }
            catch (Exception)
            {
                // Platform refused the drag (no window, headless) - nothing to undo, the
                // selection is still where it was.
            }
            finally
            {
                _dragSource = null;
            }
        }
    }

    /// <summary>
    /// Test seam: marks <paramref name="textBox"/> as the box a drag started from, as
    /// <see cref="PressSession"/> does before calling the platform - the headless platform has
    /// no drag source, so tests of the move path set it directly.
    /// </summary>
    internal static void SetDragSourceForTest(TextBox? textBox, int start, int length)
    {
        _dragSource = textBox;
        _dragSourceStart = start;
        _dragSourceLength = length;
    }

    private static void OnDragOver(object? sender, DragEventArgs e)
    {
        if (sender is not TextBox textBox)
        {
            return;
        }

        e.Handled = true;

        if (textBox.IsReadOnly || !e.DataTransfer.Contains(DataFormat.Text))
        {
            e.DragEffects = DragDropEffects.None;
            return;
        }

        var isSameBox = ReferenceEquals(_dragSource, textBox);
        e.DragEffects = isSameBox && !e.KeyModifiers.HasFlag(KeyModifiers.Control)
            ? DragDropEffects.Move
            : DragDropEffects.Copy;

        // Caret follows the pointer so the user can see where the text will land - but not
        // in the source box, where moving the caret would wipe the selection being dragged.
        if (!isSameBox)
        {
            var index = GetCharIndexAtPoint(textBox, e);
            if (index != null)
            {
                textBox.SelectionStart = index.Value;
                textBox.SelectionEnd = index.Value;
                textBox.CaretIndex = index.Value;
            }
        }
    }

    private static void OnDrop(object? sender, DragEventArgs e)
    {
        if (sender is not TextBox textBox || textBox.IsReadOnly)
        {
            return;
        }

        var text = e.DataTransfer.TryGetText();
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        e.Handled = true;

        var index = GetCharIndexAtPoint(textBox, e) ?? (textBox.Text?.Length ?? 0);
        var isMove = ReferenceEquals(_dragSource, textBox) && !e.KeyModifiers.HasFlag(KeyModifiers.Control);

        if (isMove)
        {
            var start = _dragSourceStart;
            var length = _dragSourceLength;
            if (index >= start && index <= start + length)
            {
                // Dropped back onto itself: nothing to move, just put the caret there.
                e.DragEffects = DragDropEffects.None;
                return;
            }

            // Remove the source range through the selection so it goes through the box's own
            // text-input path (same as paste), then re-aim the drop point past the hole.
            textBox.SelectionStart = start;
            textBox.SelectionEnd = start + length;
            textBox.SelectedText = string.Empty;
            if (index > start)
            {
                index -= length;
            }

            index = CollapseSeamSpace(textBox, start, index);
            e.DragEffects = DragDropEffects.Move;
        }
        else
        {
            e.DragEffects = DragDropEffects.Copy;
        }

        var current = textBox.Text ?? string.Empty;
        index = Math.Clamp(index, 0, current.Length);
        textBox.SelectionStart = index;
        textBox.SelectionEnd = index;
        textBox.CaretIndex = index;
        TextBoxPasteNormalizer.InsertNormalized(textBox, text);

        // Leave the dropped text selected, like SE4 - it shows what landed and lets a
        // second drag pick it straight up again.
        var insertedLength = (textBox.Text?.Length ?? 0) - current.Length;
        if (insertedLength > 0)
        {
            textBox.SelectionStart = index;
            textBox.SelectionEnd = index + insertedLength;
        }

        textBox.Focus();
    }

    /// <summary>
    /// Removing a word from the middle of a line leaves "a  b"; collapse the doubled space at
    /// <paramref name="seam"/> and shift <paramref name="dropIndex"/> if it sat past it.
    /// </summary>
    private static int CollapseSeamSpace(TextBox textBox, int seam, int dropIndex)
    {
        var text = textBox.Text ?? string.Empty;
        var doubleSpace = seam > 0 && seam < text.Length && text[seam - 1] == ' ' && text[seam] == ' ';
        var leadingSpace = seam == 0 && text.Length > 0 && text[0] == ' ';
        if (!doubleSpace && !leadingSpace)
        {
            return dropIndex;
        }

        textBox.SelectionStart = seam;
        textBox.SelectionEnd = seam + 1;
        textBox.SelectedText = string.Empty;
        return dropIndex > seam ? dropIndex - 1 : dropIndex;
    }

    private static int? GetCharIndexAtPoint(TextBox textBox, PointerEventArgs e)
    {
        var presenter = FindPresenter(textBox);
        return presenter == null ? null : HitTest(presenter, e.GetPosition(presenter));
    }

    private static int? GetCharIndexAtPoint(TextBox textBox, DragEventArgs e)
    {
        var presenter = FindPresenter(textBox);
        return presenter == null ? null : HitTest(presenter, e.GetPosition(presenter));
    }

    private static int HitTest(TextPresenter presenter, Point point)
    {
        var hit = presenter.TextLayout.HitTestPoint(point);
        var index = hit.TextPosition + (hit.IsTrailing ? 1 : 0);
        return Math.Clamp(index, 0, presenter.Text?.Length ?? 0);
    }

    private static TextPresenter? FindPresenter(TextBox textBox)
    {
        return textBox.GetVisualDescendants().OfType<TextPresenter>().FirstOrDefault();
    }
}
