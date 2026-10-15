using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Sync;

/// <summary>
/// Small building blocks shared by the timing dialogs of the Sync menu (adjust all times, change
/// speed, change frame rate, manual sync) in the editor look (see <see cref="EditorChrome"/>).
/// </summary>
internal static class SyncTimingChrome
{
    /// <summary>
    /// Window background and the editor styles, plus compact combo boxes and time code fields.
    /// </summary>
    internal static void Apply(Window window)
    {
        window.Background = EditorChrome.WindowBrush;
        EditorChrome.AddEditorStyles(window.Styles);
        window.Styles.Add(new Style(x => x.OfType<ComboBox>())
        {
            Setters =
            {
                new Setter(TemplatedControl.FontSizeProperty, 12.5),
                new Setter(TemplatedControl.MinHeightProperty, 28.0),
                new Setter(TemplatedControl.PaddingProperty, new Thickness(8, 2, 4, 2)),
                new Setter(TemplatedControl.CornerRadiusProperty, new CornerRadius(6)),
            },
        });
        window.Styles.Add(new Style(x => x.OfType<TimeCodeUpDown>())
        {
            Setters =
            {
                new Setter(TemplatedControl.FontSizeProperty, 12.5),
                new Setter(TemplatedControl.MinHeightProperty, 28.0),
            },
        });
    }

    /// <summary>
    /// A dim label of fixed width and the control(s) after it, so the rows line up.
    /// </summary>
    internal static Grid Row(string label, Control content, double labelWidth)
    {
        var text = EditorChrome.MakeDimLabel(label, new Thickness(0));
        text.TextTrimming = TextTrimming.CharacterEllipsis;
        var grid = new Grid
        {
            ColumnSpacing = 8,
            ColumnDefinitions = { new ColumnDefinition(new GridLength(labelWidth)), new ColumnDefinition(new GridLength(1, GridUnitType.Star)) },
        };
        grid.Add(text, 0, 0);
        grid.Add(content, 0, 1);
        return grid;
    }

    /// <summary>
    /// "Adjust all / selected lines / selected lines and forward" - radio buttons in one parent,
    /// so they form one group.
    /// </summary>
    internal static StackPanel MakeApplyToRadioButtons(string adjustAll, string adjustSelectedLines, string adjustSelectedLinesAndForward, string? isSelectionAvailable = null)
    {
        RadioButton Make(string text, string property, bool needsSelection)
        {
            var radioButton = new RadioButton
            {
                Content = text,
                FontSize = 12.5,
                [!RadioButton.IsCheckedProperty] = new Binding(property),
            };
            if (needsSelection && isSelectionAvailable != null)
            {
                radioButton[!RadioButton.IsEnabledProperty] = new Binding(isSelectionAvailable);
            }

            return radioButton;
        }

        return new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 0,
            Children =
            {
                Make(Se.Language.Sync.AdjustAll, adjustAll, false),
                Make(Se.Language.Sync.AdjustSelectedLines, adjustSelectedLines, true),
                Make(Se.Language.Sync.AdjustSelectedLinesAndForward, adjustSelectedLinesAndForward, true),
            },
        };
    }

    /// <summary>
    /// The bottom button bar: optional status/summary on the left, the buttons on the right.
    /// </summary>
    internal static Border MakeButtonBar(Control? left, params Control[] buttons)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
        };
        foreach (var button in buttons)
        {
            panel.Children.Add(button);
        }

        var bar = new DockPanel();
        DockPanel.SetDock(panel, Dock.Right);
        bar.Children.Add(panel);
        if (left != null)
        {
            bar.Children.Add(left);
        }

        return new Border
        {
            Child = bar,
            Background = EditorChrome.ButtonBarBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(16, 10),
        };
    }

    /// <summary>
    /// The sections on the panel color above the button bar.
    /// </summary>
    internal static Grid MakeRoot(Control sections, Border buttonBar)
    {
        var root = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(new GridLength(1, GridUnitType.Star)),
                new RowDefinition(GridLength.Auto),
            },
        };
        root.Add(new Border { Background = EditorChrome.PanelBrush, Child = sections }, 0, 0);
        root.Add(buttonBar, 1, 0);
        return root;
    }

    /// <summary>
    /// Two sections side by side with a line between them.
    /// </summary>
    internal static Grid SideBySide(Control left, Control right)
    {
        var grid = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(new GridLength(1, GridUnitType.Star)) },
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
    /// A flat button with an icon and a short text, for actions that need their words.
    /// </summary>
    internal static Button MakeTextToolButton(string icon, string text, System.Windows.Input.ICommand? command)
    {
        var content = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6, VerticalAlignment = VerticalAlignment.Center };
        content.Children.Add(new Optris.Icons.Avalonia.Icon { Value = icon, FontSize = 12, VerticalAlignment = VerticalAlignment.Center });
        content.Children.Add(new TextBlock { Text = text, FontSize = 12.5, VerticalAlignment = VerticalAlignment.Center });
        var button = new Button
        {
            Content = content,
            Command = command,
            Height = 28,
            Padding = new Thickness(8, 0),
            VerticalContentAlignment = VerticalAlignment.Center,
        };
        button.Classes.Add("tool");
        Avalonia.Automation.AutomationProperties.SetName(button, text);
        return button;
    }
}
