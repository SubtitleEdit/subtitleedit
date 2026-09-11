using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
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
/// read-only box stays untouched, and the seams get SE4's spacing: a space is added where the
/// dropped text would touch a word, none in front of punctuation or after a line break, and the
/// space a word leaves behind when it is moved out is removed.
///
/// While a drag hovers a box, a caret-shaped bar in the box's adorner layer marks where the text
/// will land (#14568). It is a separate visual rather than the real caret because moving the caret
/// in the source box would wipe the selection being dragged.
/// </summary>
public static class TextBoxTextDragDrop
{
    private const double DragThreshold = 4;

    /// <summary>Characters that never get a space in front of them when text is dropped or
    /// lifted out right before them (SE4's list, plus the comma).</summary>
    private const string NoSpaceBefore = ",:;]<.!?؟";

    /// <summary>The box a drag currently originates from, with the dragged range - so the
    /// drop handler can tell "move within the same box" from "copy from elsewhere".</summary>
    private static TextBox? _dragSource;
    private static int _dragSourceStart;
    private static int _dragSourceLength;

    private static readonly ConditionalWeakTable<TextBox, DropIndicator> Indicators = new();

    /// <summary>The indicator currently on screen, if any - hidden when the drag ends however
    /// it ends, so a cancelled drag (Escape) cannot leave a stray bar behind.</summary>
    private static DropIndicator? _shownIndicator;

    public static void Attach(TextBox textBox)
    {
        var session = new PressSession(textBox);
        textBox.AddHandler(InputElement.PointerPressedEvent, session.OnPointerPressed, RoutingStrategies.Tunnel);
        textBox.AddHandler(InputElement.PointerMovedEvent, session.OnPointerMoved, RoutingStrategies.Tunnel);
        textBox.AddHandler(InputElement.PointerReleasedEvent, session.OnPointerReleased, RoutingStrategies.Tunnel);
        textBox.AddHandler(InputElement.PointerCaptureLostEvent, session.OnPointerCaptureLost, RoutingStrategies.Tunnel);

        DragDrop.SetAllowDrop(textBox, true);
        textBox.AddHandler(DragDrop.DragEnterEvent, OnDragOver);
        textBox.AddHandler(DragDrop.DragOverEvent, OnDragOver);
        textBox.AddHandler(DragDrop.DragLeaveEvent, OnDragLeave);
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

            // Inclusive at the end, like SE4: the trailing half of the last selected character
            // hit-tests to selectionEnd, and a press there is still a press on the selection.
            var index = GetCharIndexAtPoint(_textBox, e);
            if (index == null || index < selectionStart || index > selectionEnd)
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

            // The box must not see any move while the press is pending, not just the one that
            // starts the drag: Avalonia captured the pointer to the presenter on the press even
            // though the box never handled it, and the box's own move handler then extends the
            // selection to the pointer - every sub-threshold jitter would shrink the selection
            // before the drag picks it up (#14568).
            e.Handled = true;

            var delta = e.GetPosition(_textBox) - _pressPoint;
            if (Math.Abs(delta.X) < DragThreshold && Math.Abs(delta.Y) < DragThreshold)
            {
                return;
            }

            _pending = null;
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
                HideIndicator();
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
            HideIndicator();
            return;
        }

        var isSameBox = ReferenceEquals(_dragSource, textBox);
        var index = GetCharIndexAtPoint(textBox, e);

        if (isSameBox && !e.KeyModifiers.HasFlag(KeyModifiers.Control) &&
            index != null && index >= _dragSourceStart && index <= _dragSourceStart + _dragSourceLength)
        {
            // Over the text being dragged: a drop here does nothing (see OnDrop), so say so
            // with the no-drop cursor and no target bar.
            e.DragEffects = DragDropEffects.None;
            HideIndicator();
            return;
        }

        e.DragEffects = isSameBox && !e.KeyModifiers.HasFlag(KeyModifiers.Control)
            ? DragDropEffects.Move
            : DragDropEffects.Copy;

        if (index == null)
        {
            HideIndicator();
            return;
        }

        // Caret follows the pointer too where it can - but not in the source box, where moving
        // the caret would wipe the selection being dragged.
        if (!isSameBox)
        {
            textBox.SelectionStart = index.Value;
            textBox.SelectionEnd = index.Value;
            textBox.CaretIndex = index.Value;
        }

        ShowIndicator(textBox, index.Value);
    }

    private static void OnDragLeave(object? sender, RoutedEventArgs e)
    {
        HideIndicator();
    }

    /// <summary>
    /// Puts the drop-target bar at the caret rectangle of <paramref name="index"/>, in the
    /// box's adorner layer so it sits above the text and outside the box's own rendering.
    /// </summary>
    private static void ShowIndicator(TextBox textBox, int index)
    {
        var presenter = FindPresenter(textBox);
        var layer = AdornerLayer.GetAdornerLayer(textBox);
        if (presenter == null || layer == null)
        {
            HideIndicator();
            return;
        }

        var rect = presenter.TextLayout.HitTestTextPosition(index);
        var top = presenter.TranslatePoint(rect.TopLeft, textBox);
        var bottom = presenter.TranslatePoint(rect.BottomLeft, textBox);
        if (top == null || bottom == null)
        {
            HideIndicator();
            return;
        }

        var indicator = Indicators.GetValue(textBox, static box => new DropIndicator(box));
        if (indicator.Parent != layer)
        {
            (indicator.Parent as AdornerLayer)?.Children.Remove(indicator);
            layer.Children.Add(indicator);
        }

        if (_shownIndicator != null && !ReferenceEquals(_shownIndicator, indicator))
        {
            HideIndicator();
        }

        _shownIndicator = indicator;
        indicator.Update(top.Value, bottom.Value, textBox.CaretBrush ?? textBox.Foreground);
    }

    private static void HideIndicator()
    {
        var indicator = _shownIndicator;
        _shownIndicator = null;
        if (indicator?.Parent is AdornerLayer layer)
        {
            layer.Children.Remove(indicator);
        }
    }

    /// <summary>
    /// Test seam: true while the drop-target bar is shown for <paramref name="textBox"/>.
    /// </summary>
    internal static bool IsIndicatorShownForTest(TextBox textBox)
    {
        return _shownIndicator != null && ReferenceEquals(_shownIndicator.TextBox, textBox) &&
               _shownIndicator.Parent is AdornerLayer;
    }

    /// <summary>
    /// The drop-target bar: a caret-width vertical line between two points in the adorned
    /// box's coordinates, drawn with the box's caret colour so it reads as "the caret will be
    /// here" in every theme.
    /// </summary>
    private sealed class DropIndicator : Control
    {
        private Point _top;
        private Point _bottom;
        private IBrush? _brush;

        public DropIndicator(TextBox textBox)
        {
            TextBox = textBox;
            IsHitTestVisible = false;
            AdornerLayer.SetAdornedElement(this, textBox);
        }

        public TextBox TextBox { get; }

        public void Update(Point top, Point bottom, IBrush? brush)
        {
            _top = top;
            _bottom = bottom;
            _brush = brush;
            InvalidateVisual();
        }

        public override void Render(DrawingContext context)
        {
            if (_bottom.Y <= _top.Y)
            {
                return;
            }

            // Two pixels wide, snapped to whole pixels so it does not blur into the glyphs.
            var x = Math.Round(_top.X) + 0.5;
            var pen = new Pen(_brush ?? Brushes.Gray, 2);
            context.DrawLine(pen, new Point(x, _top.Y), new Point(x, _bottom.Y));
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
        HideIndicator();

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
        var fitted = FitSpacing(current, index, text.NormalizeLineBreaks());
        textBox.SelectionStart = index;
        textBox.SelectionEnd = index;
        textBox.CaretIndex = index;
        TextBoxPasteNormalizer.InsertNormalized(textBox, fitted.Text);

        // Leave the dropped text selected, like SE4 - it shows what landed and lets a
        // second drag pick it straight up again. The padding spaces stay outside the
        // selection; they belong to the seams, not to the dragged text.
        var insertedLength = (textBox.Text?.Length ?? 0) - current.Length;
        var selectLength = Math.Min(fitted.Length, insertedLength - fitted.Skip);
        if (selectLength > 0)
        {
            textBox.SelectionStart = index + fitted.Skip;
            textBox.SelectionEnd = index + fitted.Skip + selectLength;
        }

        textBox.Focus();
    }

    /// <summary>
    /// SE4's drop spacing: the dropped text gets a space on each side that is missing one, unless
    /// it lands at a whitespace boundary (space, line break, start or end of the text) or right
    /// before closing punctuation; an outer space the text brings to such a boundary is dropped
    /// instead. Returns the string to insert, and where the dragged text sits inside it.
    /// </summary>
    internal static (string Text, int Skip, int Length) FitSpacing(string current, int index, string text)
    {
        var prev = index > 0 ? current[index - 1] : (char?)null;
        var next = index < current.Length ? current[index] : (char?)null;

        if (text.StartsWith(' ') && (prev == null || char.IsWhiteSpace(prev.Value)))
        {
            text = text.Substring(1);
        }

        if (text.EndsWith(' ') && (next == null || char.IsWhiteSpace(next.Value)))
        {
            text = text.Substring(0, text.Length - 1);
        }

        if (text.Length == 0)
        {
            return (string.Empty, 0, 0);
        }

        var prefix = prev != null && !char.IsWhiteSpace(prev.Value) && !char.IsWhiteSpace(text[0])
            ? " "
            : string.Empty;
        var suffix = next != null && !char.IsWhiteSpace(next.Value) && !NoSpaceBefore.Contains(next.Value) &&
                     !char.IsWhiteSpace(text[text.Length - 1])
            ? " "
            : string.Empty;

        return (prefix + text + suffix, prefix.Length, text.Length);
    }

    /// <summary>
    /// Lifting a word out leaves its spacing behind: "a  b", " b", "a .", "a " or "a \nb". Remove the
    /// stray space at <paramref name="seam"/> (SE4's rules, with a line break and the ends of the
    /// text counting as boundaries) and shift <paramref name="dropIndex"/> if it sat past it.
    /// </summary>
    private static int CollapseSeamSpace(TextBox textBox, int seam, int dropIndex)
    {
        var text = textBox.Text ?? string.Empty;
        var prev = seam > 0 ? text[seam - 1] : (char?)null;
        var next = seam < text.Length ? text[seam] : (char?)null;

        int removeAt;
        if (next == ' ' && (prev == null || char.IsWhiteSpace(prev.Value) ||
                            (seam + 1 < text.Length && NoSpaceBefore.Contains(text[seam + 1]))))
        {
            removeAt = seam;
        }
        else if (prev == ' ' && (next == null || char.IsWhiteSpace(next.Value) || NoSpaceBefore.Contains(next.Value)))
        {
            removeAt = seam - 1;
        }
        else
        {
            return dropIndex;
        }

        textBox.SelectionStart = removeAt;
        textBox.SelectionEnd = removeAt + 1;
        textBox.SelectedText = string.Empty;
        return dropIndex > removeAt ? dropIndex - 1 : dropIndex;
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
