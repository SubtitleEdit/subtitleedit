using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Controls.SyntaxTextEditorControl;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Main.MainHelpers;

internal static class RightToLeftHelper
{
    private static readonly AttachedProperty<FlowDirection?> TextDirectionOverrideProperty =
        AvaloniaProperty.RegisterAttached<TextBox, FlowDirection?>("TextDirectionOverride", typeof(RightToLeftHelper));

    internal static void SetRightToLeftForDataGridAndText(Window window)
    {
        var flowDirection = Se.Settings.Appearance.RightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        SetFlowDirectionRecursive(window, flowDirection);
    }

    /// <summary>
    /// A text control follows its content in either mode: text with right to left
    /// letters (Arabic, Hebrew, and so on) flows right to left, text in a left to
    /// right script (for example a Turkish or English original subtitle) flows
    /// left to right. The layout itself still follows only the mode toggle; this
    /// governs text direction inside a control. Empty controls keep the requested
    /// direction so typing starts on the side matching the active mode.
    /// </summary>
    internal static FlowDirection GetContentDirection(string? text, FlowDirection requested)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return requested;
        }

        return LanguageAutoDetect.ContainsRightToLeftLetter(text)
            ? FlowDirection.RightToLeft
            : FlowDirection.LeftToRight;
    }

    /// <summary>
    /// Follows the subtitle's content direction until the user overrides it with
    /// Ctrl+Left Shift (LTR) or Ctrl+Right Shift (RTL). The override belongs to the
    /// edit box and survives typing and switching subtitles, without changing text.
    /// Also corrects End navigation at a right-to-left line break (#16424).
    /// </summary>
    internal static void ConfigureTextBox(TextBox textBox)
    {
        textBox.Classes.Add("subtitleTextEditor");
        textBox.PropertyChanged += (_, e) =>
        {
            if (e.Property == TextBox.TextProperty)
            {
                var requested = Se.Settings.Appearance.RightToLeft
                    ? FlowDirection.RightToLeft
                    : FlowDirection.LeftToRight;
                textBox.FlowDirection = GetTextBoxDirection(textBox, requested);
            }
        };

        FlowDirection? pendingDirection = null;
        // Commit on modifier release so Ctrl+Shift+letter shortcuts do not also
        // switch direction. Observe handled keys too: the window may own that chord.
        textBox.AddHandler(InputElement.KeyDownEvent, (_, e) =>
        {
            pendingDirection = e.KeyModifiers == (KeyModifiers.Control | KeyModifiers.Shift)
                ? e.Key switch
                {
                    Key.LeftShift or Key.LeftCtrl => FlowDirection.LeftToRight,
                    Key.RightShift or Key.RightCtrl => FlowDirection.RightToLeft,
                    _ => null,
                }
                : null;

            if (!e.Handled)
            {
                HandleLineEnd(textBox, e);
            }
        }, RoutingStrategies.Tunnel, handledEventsToo: true);

        textBox.AddHandler(InputElement.KeyUpEvent, (_, e) =>
        {
            if (pendingDirection is { } direction &&
                e.Key is Key.LeftShift or Key.RightShift or Key.LeftCtrl or Key.RightCtrl)
            {
                textBox.SetValue(TextDirectionOverrideProperty, direction);
                textBox.FlowDirection = direction;
                e.Handled = true;
            }

            pendingDirection = null;
        }, RoutingStrategies.Tunnel);
        textBox.LostFocus += (_, _) => pendingDirection = null;
    }

    private static FlowDirection GetTextBoxDirection(TextBox textBox, FlowDirection requested)
        => textBox.GetValue(TextDirectionOverrideProperty) ?? GetContentDirection(textBox.Text, requested);

    private static void HandleLineEnd(TextBox textBox, KeyEventArgs e)
    {
        if (e.Key != Key.End || (e.KeyModifiers & ~KeyModifiers.Shift) != KeyModifiers.None ||
            !LanguageAutoDetect.ContainsRightToLeftLetter(textBox.Text ?? string.Empty))
        {
            return;
        }

        var presenter = textBox.GetVisualDescendants().OfType<SyntaxHighlightingTextPresenter>().FirstOrDefault();
        if (presenter == null || !string.IsNullOrEmpty(presenter.PreeditText))
        {
            return;
        }

        var layout = presenter.TextLayout;
        // The caret's visual line matters at a soft wrap, where one text offset
        // belongs to both the end of one line and the start of the next.
        var lineTop = 0.0;
        var line = layout.TextLines[0];
        foreach (var candidate in layout.TextLines)
        {
            line = candidate;
            if (presenter.CursorRectangle.Center.Y < lineTop + line.Height)
            {
                break;
            }

            lineTop += line.Height;
        }

        var target = line.FirstTextSourceIndex + line.Length - line.NewLineLength;
        var hit = new CharacterHit(target);
        if (target > line.FirstTextSourceIndex)
        {
            var previous = line.GetPreviousCaretCharacterHit(hit);
            hit = new CharacterHit(previous.FirstCharacterIndex, target - previous.FirstCharacterIndex);
        }

        var anchor = e.KeyModifiers.HasFlag(KeyModifiers.Shift) ? textBox.SelectionStart : target;
        textBox.CaretIndex = target;
        textBox.SelectionStart = anchor;
        textBox.SelectionEnd = target;
        // Restore the visual line affinity after the offset/selection bindings ran.
        presenter.MoveCaretToLineEnd(hit);
        e.Handled = true;
    }

    /// <summary>
    /// Mirrors the main text edit grid so the current and original text boxes keep
    /// matching the mirrored subtitle grid columns (issue #12249). Setting
    /// FlowDirection on a grid does not rearrange its children, so the columns are
    /// swapped for real: the column definitions are reversed (the original column's
    /// width binding travels with its definition object) and every child is
    /// remapped to the mirrored column index. Idempotent via the grid's Tag.
    /// </summary>
    private static void MirrorTextEditGrid(Grid grid, FlowDirection flowDirection)
    {
        var wantMirrored = flowDirection == FlowDirection.RightToLeft;
        var isMirrored = Equals(grid.Tag, "mirrored");
        if (wantMirrored == isMirrored)
        {
            return;
        }

        grid.Tag = wantMirrored ? "mirrored" : null;

        var definitions = grid.ColumnDefinitions.ToList();
        grid.ColumnDefinitions.Clear();
        definitions.Reverse();
        foreach (var definition in definitions)
        {
            grid.ColumnDefinitions.Add(definition);
        }

        var last = definitions.Count - 1;
        foreach (var child in grid.Children)
        {
            Grid.SetColumn(child, last - Grid.GetColumn(child));

            // Keep horizontal margins (the gap between the two text boxes) on the inner side.
            var margin = child.Margin;
            if (margin.Left != margin.Right)
            {
                child.Margin = new Thickness(margin.Right, margin.Top, margin.Left, margin.Bottom);
            }
        }
    }

    /// <summary>
    /// Re-runs the subtitle grid's cell bindings after the right to left mode
    /// changed: the per line flow direction converter reads the setting, and
    /// bindings do not re-evaluate on their own when a setting changes. Rebinding
    /// the items source re-creates the rows; selection and scroll position are
    /// restored.
    /// </summary>
    internal static void RefreshDataGridBindings(TableView? grid, System.Collections.IEnumerable? itemsSource, object? selected)
    {
        if (grid == null)
        {
            return;
        }

        grid.ItemsSource = null;
        grid.ItemsSource = itemsSource;
        if (selected != null)
        {
            grid.SelectedItem = selected;
            grid.ScrollIntoView(selected);
        }
    }

    private static void SetFlowDirectionRecursive(Visual visual, FlowDirection flowDirection)
    {
        if (visual is ComboBox)
        {
            return;
        }

        if (visual is NumericUpDown)
        {
            return;
        }

        if (visual is TimeCodeUpDown)
        {
            return;
        }

        if (visual is SecondsUpDown)
        {
            return;
        }

        if (visual is TableView tableView)
        {
            tableView.FlowDirection = flowDirection;
        }
        else if (visual is Grid grid && grid.Name == "SubtitleTextEditGrid")
        {
            MirrorTextEditGrid(grid, flowDirection);
        }
        else if (visual is TextBox textBox)
        {
            textBox.FlowDirection = GetTextBoxDirection(textBox, flowDirection);
        }
        else if (visual is SyntaxTextView sourceView)
        {
            sourceView.FlowDirection = GetContentDirection(sourceView.Document.GetLine(0), flowDirection);
        }

        foreach (var child in visual.GetVisualChildren())
        {
            SetFlowDirectionRecursive(child, flowDirection);
        }
    }
}
