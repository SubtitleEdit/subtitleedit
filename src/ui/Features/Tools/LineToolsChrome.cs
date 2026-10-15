using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Edit;
using Nikse.SubtitleEdit.Features.Sync;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Tools;

/// <summary>
/// The shared layout of the Tools menu's line tools (split/merge lines, convert actors, change
/// formatting, sort, remove Unicode characters, renumber) in the editor look (see
/// <see cref="EditorChrome"/>): the options on top, the preview list(s) below, a summary plus
/// Cancel and OK in the button bar.
/// </summary>
internal static class LineToolsChrome
{
    /// <summary>
    /// Window background, the editor styles, compact combo boxes and text fields.
    /// </summary>
    internal static void Apply(Window window)
    {
        EditDialogChrome.Apply(window);
    }

    /// <summary>
    /// A number field from <see cref="UiUtil"/> made compact (no spinner), named for screen readers.
    /// </summary>
    internal static NumericUpDown MakeCompact(NumericUpDown field, string accessibleName, double width = 100)
    {
        field.Classes.Add("field");
        field.Width = width;
        field.HorizontalAlignment = HorizontalAlignment.Left;
        field.VerticalAlignment = VerticalAlignment.Center;
        AutomationProperties.SetName(field, accessibleName.TrimEnd(':'));
        return field;
    }

    /// <summary>
    /// "Label  [field]" rows with the labels in a column sized to the longest one (so
    /// translations do not get clipped).
    /// </summary>
    internal static Grid MakeFieldGrid(params (string Label, Control Field)[] rows)
    {
        var grid = new Grid
        {
            ColumnSpacing = 12,
            RowSpacing = 8,
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto) },
        };
        for (var i = 0; i < rows.Length; i++)
        {
            grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            grid.Add(EditorChrome.MakeDimLabel(rows[i].Label.TrimEnd(':'), new Thickness(0)), i, 0);
            grid.Add(rows[i].Field, i, 1);
        }

        return grid;
    }

    /// <summary>
    /// An on/off switch with its text, bound to <paramref name="property"/> of the inherited data context.
    /// </summary>
    internal static StackPanel MakeSwitchRow(string property, string text, out ToggleSwitch toggle)
    {
        var row = EditorChrome.MakeToggleRow(property, text);
        toggle = row.Children.OfType<ToggleSwitch>().First();
        return row;
    }

    /// <summary>
    /// Icon actions for a list section header (select all / none / invert).
    /// </summary>
    internal static StackPanel MakeListActions(params Control[] buttons)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 2,
            VerticalAlignment = VerticalAlignment.Center,
        };
        foreach (var button in buttons)
        {
            panel.Children.Add(button);
        }

        return panel;
    }

    internal static Button MakeSelectAllButton(System.Windows.Input.ICommand command) =>
        EditorChrome.CreateToolButton("fa-solid fa-square-check", Se.Language.General.SelectAll, command, 26);

    internal static Button MakeSelectNoneButton(System.Windows.Input.ICommand command) =>
        EditorChrome.CreateToolButton("fa-regular fa-square", Se.Language.General.SelectNone, command, 26);

    internal static Button MakeInvertSelectionButton(System.Windows.Input.ICommand command) =>
        EditorChrome.CreateToolButton("fa-solid fa-right-left", Se.Language.General.InvertSelection, command, 26);

    /// <summary>
    /// A section whose list fills the rest of the window: the small caps header (with optional
    /// actions on the right) above the list in a rounded frame.
    /// </summary>
    internal static Border MakeListSection(string title, Control list, Control? headerRight = null, bool first = false)
    {
        var header = new DockPanel { MinHeight = 26 };
        if (headerRight != null)
        {
            DockPanel.SetDock(headerRight, Dock.Right);
            header.Children.Add(headerRight);
        }

        header.Children.Add(EditorChrome.MakeSectionHeader(title));

        var content = new Grid
        {
            RowSpacing = 8,
            RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(new GridLength(1, GridUnitType.Star)) },
        };
        content.Add(header, 0, 0);
        content.Add(EditDialogChrome.MakeListFrame(list), 1, 0);
        return new Border
        {
            Child = content,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, first ? 0 : 1, 0, 0),
            Padding = new Thickness(14, 12, 14, 14),
        };
    }

    /// <summary>
    /// The options (auto height) above one or more list sections that share the rest.
    /// </summary>
    internal static Grid MakeSections(Control? options, params Control[] lists)
    {
        var grid = new Grid();
        var row = 0;
        if (options != null)
        {
            grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            grid.Add(options, row++, 0);
        }

        foreach (var list in lists)
        {
            grid.RowDefinitions.Add(new RowDefinition(new GridLength(1, GridUnitType.Star)) { MinHeight = 120 });
            grid.Add(list, row++, 0);
        }

        return grid;
    }

    /// <summary>
    /// A summary text (the fixes found, lines changed, ...) for the left side of the button bar.
    /// </summary>
    internal static TextBlock MakeStatusText(object vm, string property)
    {
        return new TextBlock
        {
            DataContext = vm,
            Foreground = EditorChrome.DimTextBrush,
            FontSize = 12.5,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            Margin = new Thickness(0, 0, 12, 0),
            [!TextBlock.TextProperty] = new Binding(property),
        };
    }

    /// <summary>
    /// The sections on the panel color above the button bar with the summary, Cancel and the
    /// accent OK.
    /// </summary>
    internal static Grid MakeRoot(Control sections, Control? status, Button buttonCancel, Button buttonOk)
    {
        buttonOk.Classes.Add("primary");
        return SyncTimingChrome.MakeRoot(sections, SyncTimingChrome.MakeButtonBar(status, buttonCancel, buttonOk));
    }
}
