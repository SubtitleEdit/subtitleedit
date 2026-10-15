using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Features.Edit;
using Nikse.SubtitleEdit.Features.Sync;
using Nikse.SubtitleEdit.Features.Video;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Shared;

/// <summary>
/// The editor look (see <see cref="EditorChrome"/>) for the shared pickers, prompts and small
/// dialogs. Every picker uses the same layout: a list section on the panel color (small caps
/// header, search box on the header's right when the list can be searched, the list in a rounded
/// field frame), optional side/preview sections, and the button bar with Cancel and the accent
/// primary at the bottom.
/// </summary>
internal static class SharedDialogChrome
{
    /// <summary>
    /// Window background, the editor styles, compact combo boxes / time codes / text fields
    /// (class "text") and soft accent list box selection.
    /// </summary>
    internal static void Apply(Window window)
    {
        EditDialogChrome.Apply(window);

        var styles = window.Styles;
        styles.Add(new Style(x => x.OfType<ListBox>())
        {
            Setters =
            {
                new Setter(TemplatedControl.BackgroundProperty, Brushes.Transparent),
                new Setter(TemplatedControl.BorderThicknessProperty, new Thickness(0)),
                new Setter(TemplatedControl.PaddingProperty, new Thickness(4)),
            },
        });
        styles.Add(new Style(x => x.OfType<ListBoxItem>())
        {
            Setters =
            {
                new Setter(TemplatedControl.PaddingProperty, new Thickness(8, 4)),
                new Setter(TemplatedControl.CornerRadiusProperty, new CornerRadius(5)),
                new Setter(TemplatedControl.FontSizeProperty, 12.5),
                new Setter(Layoutable.MinHeightProperty, 26.0),
            },
        });
        foreach (var state in new[] { ":selected", ":selected:pointerover" })
        {
            styles.Add(new Style(x => (state.Contains("pointerover")
                    ? x.OfType<ListBoxItem>().Class(":selected").Class(":pointerover")
                    : x.OfType<ListBoxItem>().Class(":selected"))
                .Template().OfType<Avalonia.Controls.Presenters.ContentPresenter>().Name("PART_ContentPresenter"))
            {
                Setters =
                {
                    new Setter(Avalonia.Controls.Presenters.ContentPresenter.BackgroundProperty, EditorChrome.AccentSoftBrush),
                    new Setter(Avalonia.Controls.Presenters.ContentPresenter.ForegroundProperty, EditorChrome.StrongTextBrush),
                },
            });
        }

        // Compact number fields that keep their spinners (layer, line numbers, scan settings)
        styles.Add(new Style(x => x.OfType<NumericUpDown>().Not(y => y.Class("field")))
        {
            Setters =
            {
                new Setter(TemplatedControl.FontSizeProperty, 12.5),
                new Setter(Layoutable.MinHeightProperty, 28.0),
                new Setter(TemplatedControl.CornerRadiusProperty, new CornerRadius(6)),
            },
        });
        // Tabs: small headers instead of the big Fluent ones
        styles.Add(new Style(x => x.OfType<TabItem>())
        {
            Setters =
            {
                new Setter(TemplatedControl.FontSizeProperty, 13.0),
                new Setter(Layoutable.MinHeightProperty, 32.0),
                new Setter(TemplatedControl.PaddingProperty, new Thickness(12, 0)),
            },
        });
        styles.Add(new Style(x => x.OfType<CheckBox>())
        {
            Setters = { new Setter(TemplatedControl.FontSizeProperty, 12.5) },
        });
        styles.Add(new Style(x => x.OfType<RadioButton>())
        {
            Setters = { new Setter(TemplatedControl.FontSizeProperty, 12.5) },
        });
    }

    /// <summary>
    /// The content on the panel color above the button bar.
    /// </summary>
    internal static Grid MakeRoot(Control content, Border buttonBar) => SyncTimingChrome.MakeRoot(content, buttonBar);

    /// <summary>
    /// The bottom bar: optional status/summary on the left, the buttons on the right (pass them
    /// as Cancel, ..., primary).
    /// </summary>
    internal static Border MakeButtonBar(Control? left, params Control[] buttons) => VideoSubDialogChrome.MakeButtonBar(left, buttons);

    /// <summary>
    /// Marks <paramref name="button"/> as the dialog's accent primary.
    /// </summary>
    internal static Button Primary(Button button)
    {
        button.Classes.Add("primary");
        return button;
    }

    /// <summary>
    /// A compact search box with the magnifier and clear icons, bound to <paramref name="property"/>.
    /// </summary>
    internal static TextBox MakeSearchBox(object source, string property, string? placeholder, double width = 240)
    {
        var textBox = new TextBox
        {
            Width = width,
            PlaceholderText = placeholder,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Right,
            [!TextBox.TextProperty] = new Binding(property) { Source = source, Mode = BindingMode.TwoWay },
        }.WithSearchAndClearIcons();
        textBox.Classes.Add("text");
        AutomationProperties.SetName(textBox, Nikse.SubtitleEdit.Logic.Config.Se.Language.General.Search);
        return textBox;
    }

    /// <summary>
    /// A list section: the small caps header (with optional controls on its right) over the list
    /// in a rounded field frame that fills the rest of the section.
    /// </summary>
    internal static Border MakeListSection(string title, Control list, Control? headerRight = null, bool lineOnTop = false, Control? below = null)
    {
        var header = new DockPanel { MinHeight = 28, Margin = new Thickness(0, 0, 0, 8) };
        if (headerRight != null)
        {
            DockPanel.SetDock(headerRight, Dock.Right);
            header.Children.Add(headerRight);
        }

        header.Children.Add(EditorChrome.MakeSectionHeader(title));

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(new GridLength(1, GridUnitType.Star)),
                new RowDefinition(GridLength.Auto),
            },
        };
        grid.Add(header, 0, 0);
        grid.Add(EditDialogChrome.MakeListFrame(list), 1, 0);
        if (below != null)
        {
            below.Margin = new Thickness(0, 10, 0, 0);
            grid.Add(below, 2, 0);
        }

        return new Border
        {
            Child = grid,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, lineOnTop ? 1 : 0, 0, 0),
            Padding = new Thickness(14, 12, 14, 14),
        };
    }

    /// <summary>
    /// A small caps header over the content, with a line on top unless it is the first section.
    /// </summary>
    internal static Border MakeSection(string title, Control content, bool first = false, Control? headerRight = null)
    {
        return EditorChrome.MakeEditorSection(title, content, headerRight, first);
    }

    /// <summary>
    /// Two panes side by side with a line between them (the track pickers: tracks | preview).
    /// </summary>
    internal static Grid MakeTwoPanes(Control left, Control right)
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
            },
        };
        grid.Add(left, 0, 0);
        grid.Add(new Border
        {
            Child = right,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(1, 0, 0, 0),
        }, 0, 1);
        return grid;
    }

    /// <summary>
    /// A dim label of fixed width and the control after it, so the rows line up.
    /// </summary>
    internal static Grid MakeRow(string label, Control content, double labelWidth) => SyncTimingChrome.Row(label, content, labelWidth);

    /// <summary>
    /// Dim one-line text bound to <paramref name="property"/>.
    /// </summary>
    internal static TextBlock MakeHint(string property) => VideoSubDialogChrome.MakeHint(property);

    /// <summary>
    /// A flat button with an icon and a short text. The text keeps its access key ("_Find next"
    /// shows "Find next" with the F underlined and Alt+F clicks it).
    /// </summary>
    internal static Button MakeTextToolButton(string icon, string text, System.Windows.Input.ICommand? command)
    {
        var content = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6, VerticalAlignment = VerticalAlignment.Center };
        content.Children.Add(new Optris.Icons.Avalonia.Icon { Value = icon, FontSize = 12, VerticalAlignment = VerticalAlignment.Center });
        content.Children.Add(new AccessText { Text = text, FontSize = 12.5, VerticalAlignment = VerticalAlignment.Center });
        var button = new Button
        {
            Content = content,
            Command = command,
            Height = 28,
            Padding = new Thickness(8, 0),
            VerticalContentAlignment = VerticalAlignment.Center,
        };
        button.Classes.Add("tool");

        // Icon + label in a panel: a screen reader needs the name spelled out (#12087)
        AutomationProperties.SetName(button, text.Replace("_", string.Empty));
        return button;
    }

    /// <summary>
    /// A switch with its label beside it, bound to <paramref name="property"/>.
    /// </summary>
    internal static Grid MakeSwitchRow(string property, string text) =>
        VideoSubDialogChrome.MakeSwitchRow(VideoSubDialogChrome.MakeSwitch(property, text), text);
}
