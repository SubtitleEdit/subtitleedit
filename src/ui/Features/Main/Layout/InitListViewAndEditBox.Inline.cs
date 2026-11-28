using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Main.Layout;

public static partial class InitListViewAndEditBox
{
    /// <summary>
    /// "Inline editing" text box position (prototype): the subtitle text box is not docked
    /// below/above the grid but floated over the Text cell of the selected row, so the text is
    /// edited right where it is shown. It is still the one and only <see cref="MainViewModel.EditTextBox"/>,
    /// so every command, shortcut and binding that works on the docked text box keeps working.
    /// The overlay follows the row on selection change, scrolling, column resize and row growth
    /// (all of which end in a layout pass), and hides when the row is not realized, scrolled out
    /// of view, or when several rows are selected.
    /// </summary>
    internal static void AttachInlineTextEditor(MainViewModel vm, Grid mainGrid, TableView subtitleGrid, Control textEditor)
    {
        // Canvas without a background: only the editor itself takes pointer input, the rest
        // of the grid underneath stays clickable.
        var overlay = new Canvas { ClipToBounds = true };
        Grid.SetRow(overlay, 0);
        mainGrid.Children.Add(overlay);

        if (textEditor is TextBox textBox)
        {
            // Same font size as the grid so the text does not jump when the editor appears.
            textBox.FontSize = Se.Settings.Appearance.SubtitleGridFontSize;
            textBox.MinHeight = 0;
            textBox.Padding = new Thickness(4, 1, 4, 1);
        }

        var host = new Border
        {
            Child = textEditor,
            IsVisible = false,
            BoxShadow = new BoxShadows(new BoxShadow { Blur = 6, OffsetY = 2, Color = Color.FromArgb(80, 0, 0, 0) }),
        };
        overlay.Children.Add(host);

        // The editor only takes the mouse while it has keyboard focus. Until then clicks fall
        // through to the grid row underneath, so the subtitle grid single/double-click actions
        // (go to video position, play, focus text box, ...) work on the Text cell too - and the
        // second click of a double-click is not swallowed by the editor that just moved onto
        // the newly selected row. The "focus text box" actions are what put it into edit mode.
        host.IsHitTestVisible = false;
        textEditor.PropertyChanged += (_, e) =>
        {
            if (e.Property == InputElement.IsKeyboardFocusWithinProperty)
            {
                host.IsHitTestVisible = textEditor.IsKeyboardFocusWithin;
            }
        };

        // The Fluent text box background is translucent, so the cell text under the editor is
        // hidden (not removed - the row keeps its height) while the editor covers it.
        TextBlock? hiddenCellText = null;
        void HideCellText(TextBlock? textBlock)
        {
            if (ReferenceEquals(hiddenCellText, textBlock))
            {
                return;
            }

            if (hiddenCellText != null)
            {
                hiddenCellText.Opacity = 1;
            }

            hiddenCellText = textBlock;
            if (hiddenCellText != null)
            {
                hiddenCellText.Opacity = 0;
            }
        }

        // Escape goes back to the grid, so arrow keys move between lines again.
        textEditor.AddHandler(InputElement.KeyDownEvent, (_, e) =>
        {
            if (e.Key == Key.Escape && e.KeyModifiers == KeyModifiers.None)
            {
                TableViewExtras.FocusRow(subtitleGrid);
                e.Handled = true;
            }
        }, RoutingStrategies.Tunnel);

        // A single click on the Text cell of any row selects it and starts editing there, caret
        // where the user clicked. The click waits out the double-click time so a double-click
        // still runs the grid double-click action. Whether the click hit the Text cell is decided
        // when the timer fires - by then the editor has moved onto the newly selected row, so
        // its bounds are that row's Text cell. Shift/Ctrl clicks (multi-select) never edit. The
        // keyboard way in is the existing Enter key action / "Focus text box" shortcuts.
        var pressedForEdit = false;
        Point pressPoint = default;
        IDisposable? beginEditTimer = null;
        var dropHost = vm.SubtitleGridDropHost!;
        dropHost.AddHandler(InputElement.PointerPressedEvent, (_, e) =>
        {
            beginEditTimer?.Dispose();
            pressPoint = e.GetPosition(overlay);
            pressedForEdit = !host.IsHitTestVisible &&
                             e.GetCurrentPoint(overlay).Properties.IsLeftButtonPressed &&
                             e.KeyModifiers == KeyModifiers.None;
        }, RoutingStrategies.Tunnel, handledEventsToo: true);
        dropHost.AddHandler(InputElement.TappedEvent, (_, _) =>
        {
            if (!pressedForEdit)
            {
                return;
            }

            pressedForEdit = false;
            var point = pressPoint;
            beginEditTimer?.Dispose();
            beginEditTimer = DispatcherTimer.RunOnce(() =>
            {
                if (host.IsVisible && host.Bounds.Contains(point))
                {
                    BeginInlineEdit(textEditor, overlay, point);
                }
            }, TimeSpan.FromMilliseconds(300));
        }, RoutingStrategies.Bubble, handledEventsToo: true);
        dropHost.AddHandler(InputElement.DoubleTappedEvent, (_, _) =>
        {
            pressedForEdit = false;
            beginEditTimer?.Dispose();
        }, RoutingStrategies.Tunnel, handledEventsToo: true);

        Rect? lastRect = null;

        void Update()
        {
            var rect = GetInlineEditorRect(subtitleGrid, overlay, out var cellText);
            HideCellText(cellText);
            if (rect == lastRect)
            {
                return;
            }

            lastRect = rect;
            if (rect is not { } r)
            {
                host.IsVisible = false;
                return;
            }

            Canvas.SetLeft(host, r.X);
            Canvas.SetTop(host, r.Y);
            host.Width = r.Width;
            host.MinHeight = r.Height;
            host.IsVisible = true;
        }

        // LayoutUpdated covers scrolling, column resize and the row growing while typing; it
        // is cheap here because nothing is written unless the rectangle changed.
        subtitleGrid.LayoutUpdated += (_, _) => Update();
        subtitleGrid.SelectionChanged += (_, _) => Dispatcher.UIThread.Post(Update, DispatcherPriority.Render);
    }

    /// <summary>
    /// Focuses the inline editor with the caret at <paramref name="overlayPoint"/> (the click
    /// position), or at the end of the text when there is no point.
    /// </summary>
    private static void BeginInlineEdit(Control textEditor, Visual overlay, Point? overlayPoint)
    {
        if (!textEditor.IsEffectivelyVisible || textEditor is not TextBox textBox)
        {
            return;
        }

        textBox.Focus();
        var length = textBox.Text?.Length ?? 0;
        var caret = length;
        if (overlayPoint is { } p &&
            textBox.GetVisualDescendants().OfType<TextPresenter>().FirstOrDefault() is { } presenter &&
            overlay.TranslatePoint(p, presenter) is { } local)
        {
            var hit = presenter.TextLayout.HitTestPoint(local);
            caret = Math.Clamp(hit.TextPosition + (hit.IsTrailing ? 1 : 0), 0, length);
        }

        textBox.SelectionStart = caret;
        textBox.SelectionEnd = caret;
        textBox.CaretIndex = caret;
    }

    private static Rect? GetInlineEditorRect(TableView subtitleGrid, Visual overlay, out TextBlock? cellText)
    {
        cellText = null;
        var item = subtitleGrid.SelectedItem;
        if (item == null || subtitleGrid.SelectedItems is { Count: > 1 })
        {
            return null;
        }

        if (subtitleGrid.ContainerFromItem(item) is not Control row || !row.IsEffectivelyVisible || row.Bounds.Height <= 0)
        {
            return null;
        }

        // The Text column's cell - its TextBlock is tagged with the column key.
        var textBlock = row.GetVisualDescendants()
            .OfType<TextBlock>()
            .FirstOrDefault(t => Equals(t.Tag, SubtitleGridColumnKeys.Text));
        var cell = textBlock?.Parent as Control ?? textBlock;
        if (cell == null || !cell.IsEffectivelyVisible || cell.Bounds.Width <= 0)
        {
            return null;
        }

        var topLeft = cell.TranslatePoint(new Point(0, 0), overlay);
        if (topLeft == null)
        {
            return null;
        }

        // Hide while the row is (partly) scrolled out of the rows viewport - otherwise the
        // editor would float over the column header or past the last visible row.
        var scrollViewer = subtitleGrid.GetRowsScrollViewer();
        if (scrollViewer != null)
        {
            var viewportTopLeft = scrollViewer.TranslatePoint(new Point(0, 0), overlay);
            if (viewportTopLeft is { } v)
            {
                var viewportBottom = v.Y + scrollViewer.Viewport.Height;
                if (topLeft.Value.Y < v.Y - 0.5 || topLeft.Value.Y + cell.Bounds.Height > viewportBottom + 0.5)
                {
                    return null;
                }
            }
        }

        cellText = textBlock;
        return new Rect(topLeft.Value, cell.Bounds.Size);
    }
}
