using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

/// <summary>
/// The editor look (<see cref="EditorChrome"/>) for the Settings window and its dialogs (reset,
/// import/export, profiles): compact fields, combo boxes and buttons, switches for on/off options,
/// the category rows of the side panel and the label column of the setting rows.
/// </summary>
internal static class SettingsChrome
{
    /// <summary>Width of the label column, so the fields of every setting line up.</summary>
    internal const double LabelWidth = 250;

    /// <summary>Width of a compact number field.</summary>
    internal const double FieldWidth = 110;

    /// <summary>Class of the category rows in the side panel.</summary>
    internal const string NavClass = "settings-nav";

    /// <summary>Class of the selected category row.</summary>
    internal const string SelectedClass = "selected";

    private static readonly ConditionalWeakTable<Window, Styles> AppliedStyles = new();

    /// <summary>
    /// Window background and the editor styles. Called again when the app theme changes while the
    /// window is open (Settings > Apply), as the brushes are picked when the styles are made.
    /// </summary>
    internal static void Apply(Window window)
    {
        window.Background = EditorChrome.WindowBrush;
        if (AppliedStyles.TryGetValue(window, out var previous))
        {
            window.Styles.Remove(previous);
            AppliedStyles.Remove(window);
        }

        var styles = new Styles();
        EditorChrome.AddEditorStyles(styles);
        AddStyles(styles);
        window.Styles.Add(styles);
        AppliedStyles.Add(window, styles);
    }

    private static void AddStyles(Styles styles)
    {
        styles.Add(new Style(x => x.OfType<ComboBox>())
        {
            Setters =
            {
                new Setter(TemplatedControl.FontSizeProperty, 12.5),
                new Setter(TemplatedControl.MinHeightProperty, 28.0),
                new Setter(TemplatedControl.PaddingProperty, new Thickness(8, 2, 4, 2)),
                new Setter(TemplatedControl.CornerRadiusProperty, new CornerRadius(6)),
            },
        });
        styles.Add(new Style(x => x.OfType<TextBox>().Class("text"))
        {
            Setters =
            {
                new Setter(TemplatedControl.FontSizeProperty, 12.5),
                new Setter(TemplatedControl.MinHeightProperty, 28.0),
                new Setter(TemplatedControl.PaddingProperty, new Thickness(8, 4)),
                new Setter(TemplatedControl.CornerRadiusProperty, new CornerRadius(6)),
                new Setter(TemplatedControl.BackgroundProperty, EditorChrome.FieldBrush),
                new Setter(TemplatedControl.BorderBrushProperty, EditorChrome.FieldBorderBrush),
                new Setter(TextBox.VerticalContentAlignmentProperty, VerticalAlignment.Center),
            },
        });

        // Compact rows in the lists (favorite formats and languages)
        styles.Add(new Style(x => x.OfType<ListBoxItem>())
        {
            Setters =
            {
                new Setter(TemplatedControl.FontSizeProperty, 12.5),
                new Setter(Layoutable.MinHeightProperty, 28.0),
                new Setter(TemplatedControl.PaddingProperty, new Thickness(10, 5)),
            },
        });

        // Text buttons (Edit, Download, ...) in the size of the fields beside them
        styles.Add(new Style(x => x.OfType<Button>().Class("compact"))
        {
            Setters =
            {
                new Setter(TemplatedControl.FontSizeProperty, 12.5),
                new Setter(Layoutable.MinHeightProperty, 28.0),
                new Setter(TemplatedControl.PaddingProperty, new Thickness(12, 4)),
                new Setter(TemplatedControl.CornerRadiusProperty, new CornerRadius(6)),
                new Setter(ContentControl.VerticalContentAlignmentProperty, VerticalAlignment.Center),
            },
        });

        // The category rows of the side panel: flat, rounded, the selected one in the soft accent
        styles.Add(new Style(x => x.OfType<Button>().Class(NavClass))
        {
            Setters =
            {
                new Setter(TemplatedControl.BackgroundProperty, Brushes.Transparent),
                new Setter(TemplatedControl.BorderThicknessProperty, new Thickness(0)),
                new Setter(TemplatedControl.CornerRadiusProperty, new CornerRadius(7)),
                new Setter(TemplatedControl.PaddingProperty, new Thickness(8, 6)),
                new Setter(Layoutable.HorizontalAlignmentProperty, HorizontalAlignment.Stretch),
                new Setter(ContentControl.HorizontalContentAlignmentProperty, HorizontalAlignment.Left),
                new Setter(ContentControl.VerticalContentAlignmentProperty, VerticalAlignment.Center),
            },
        });
        styles.Add(NavPresenter(x => x.OfType<Button>().Class(NavClass).Class(":pointerover"),
            EditorChrome.HoverBrush, EditorChrome.StrongTextBrush));
        styles.Add(NavPresenter(x => x.OfType<Button>().Class(NavClass).Class(":pressed"),
            EditorChrome.PressedBrush, EditorChrome.StrongTextBrush));
        styles.Add(NavPresenter(x => x.OfType<Button>().Class(NavClass).Class(SelectedClass),
            EditorChrome.AccentSoftBrush, EditorChrome.StrongTextBrush));
        styles.Add(NavPresenter(x => x.OfType<Button>().Class(NavClass).Class(SelectedClass).Class(":pointerover"),
            EditorChrome.AccentSoftBrush, EditorChrome.StrongTextBrush));
        styles.Add(new Style(x => x.OfType<Button>().Class(NavClass).Class(SelectedClass))
        {
            Setters = { new Setter(TemplatedControl.FontWeightProperty, FontWeight.SemiBold) },
        });
    }

    private static Style NavPresenter(System.Func<Selector?, Selector> button, IBrush background, IBrush foreground)
    {
        return new Style(x => button(x).Template().OfType<ContentPresenter>().Name("PART_ContentPresenter"))
        {
            Setters =
            {
                new Setter(ContentPresenter.BackgroundProperty, background),
                new Setter(ContentPresenter.ForegroundProperty, foreground),
            },
        };
    }

    /// <summary>
    /// A compact on/off switch bound to <paramref name="property"/> of <paramref name="source"/>.
    /// </summary>
    internal static ToggleSwitch MakeSwitch(object source, string property, string? accessibleName = null)
    {
        var toggle = new ToggleSwitch
        {
            OnContent = null,
            OffContent = null,
            MinWidth = 0,
            VerticalAlignment = VerticalAlignment.Center,
            [!ToggleButton.IsCheckedProperty] = new Binding(property) { Source = source, Mode = BindingMode.TwoWay },
        };

        if (!string.IsNullOrEmpty(accessibleName))
        {
            AutomationProperties.SetName(toggle, accessibleName);
        }

        return toggle;
    }

    /// <summary>
    /// A switch with its label beside it; the label wraps instead of being cut off.
    /// </summary>
    internal static Grid MakeSwitchRow(ToggleSwitch toggle, string text)
    {
        var label = new TextBlock
        {
            Text = text,
            FontSize = 12.5,
            VerticalAlignment = VerticalAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
        };
        AccessibleLabels.LinkToLabel(toggle, label);
        DimLabelWhenDisabled(toggle, label);

        var row = new Grid
        {
            ColumnSpacing = 8,
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(new GridLength(1, GridUnitType.Star)) },
        };
        row.Add(toggle, 0, 0);
        row.Add(label, 0, 1);
        return row;
    }

    /// <summary>
    /// Dims the label of a switch that is off limits (it depends on another setting) with it.
    /// </summary>
    internal static void DimLabelWhenDisabled(Control control, TextBlock label)
    {
        void Update() => label.Opacity = control.IsEnabled ? 1 : 0.5;
        control.PropertyChanged += (_, e) =>
        {
            if (e.Property == InputElement.IsEnabledProperty)
            {
                Update();
            }
        };
        Update();
    }

    /// <summary>
    /// Cuts a long text (a file path) with an ellipsis at the edge of the field column.
    /// </summary>
    internal static TextBlock WithEllipsis(this TextBlock textBlock)
    {
        textBlock.TextTrimming = TextTrimming.CharacterEllipsis;
        textBlock.HorizontalAlignment = HorizontalAlignment.Left;
        return textBlock;
    }

    /// <summary>
    /// The dim label of a setting row, in the label column.
    /// </summary>
    internal static TextBlock MakeLabel(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = 12.5,
            Foreground = EditorChrome.DimTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
        };
    }

    /// <summary>
    /// Gives the inputs of a setting (the control itself or the inputs of a wrapper panel) the
    /// compact editor look: number fields without spinners, compact text boxes and text buttons.
    /// </summary>
    internal static void Compact(Control control)
    {
        CompactOne(control);
        foreach (var child in control.GetLogicalDescendants())
        {
            if (child is Control c)
            {
                CompactOne(c);
            }
        }
    }

    private static void CompactOne(Control control)
    {
        switch (control)
        {
            case NumericUpDown numericUpDown when numericUpDown.TemplatedParent == null:
                numericUpDown.Classes.Add("field");
                numericUpDown.Width = FieldWidth;
                numericUpDown.HorizontalAlignment = HorizontalAlignment.Left;
                numericUpDown.VerticalAlignment = VerticalAlignment.Center;
                break;
            case TextBox textBox when textBox.TemplatedParent == null:
                textBox.Classes.Add("text");
                textBox.VerticalAlignment = VerticalAlignment.Center;
                break;
            case ComboBox comboBox:
                comboBox.VerticalAlignment = VerticalAlignment.Center;
                break;
            case Button button when button.GetType() == typeof(Button) && !button.Classes.Contains("tool") && !button.Classes.Contains("primary"):
                button.Classes.Add("compact");
                if (button.HorizontalAlignment is HorizontalAlignment.Stretch or HorizontalAlignment.Center)
                {
                    button.HorizontalAlignment = HorizontalAlignment.Left;
                }

                break;
        }
    }

    /// <summary>
    /// A list in a rounded field frame.
    /// </summary>
    internal static Border MakeListFrame(Control list)
    {
        return new Border
        {
            Child = list,
            Background = EditorChrome.FieldBrush,
            BorderBrush = EditorChrome.FieldBorderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            ClipToBounds = true,
        };
    }

    /// <summary>
    /// A group of sub-settings (the video preview font, the border style) in a rounded frame.
    /// </summary>
    internal static Border MakeGroupFrame(Control content)
    {
        return new Border
        {
            Child = content,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(12, 10),
            HorizontalAlignment = HorizontalAlignment.Left,
        };
    }

    /// <summary>
    /// The bottom bar: <paramref name="left"/> on the left, the buttons on the right.
    /// </summary>
    internal static Border MakeButtonBar(Control? left, params Control[] buttons)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10, VerticalAlignment = VerticalAlignment.Center };
        foreach (var button in buttons)
        {
            panel.Children.Add(button);
        }

        var grid = new Grid
        {
            ColumnSpacing = 12,
            ColumnDefinitions = { new ColumnDefinition(new GridLength(1, GridUnitType.Star)), new ColumnDefinition(GridLength.Auto) },
        };
        if (left != null)
        {
            left.VerticalAlignment = VerticalAlignment.Center;
            left.HorizontalAlignment = HorizontalAlignment.Left;
            grid.Add(left, 0, 0);
        }

        grid.Add(panel, 0, 1);

        return new Border
        {
            Child = grid,
            Background = EditorChrome.ButtonBarBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(16, 10),
        };
    }

    /// <summary>
    /// The panel color behind <paramref name="content"/>.
    /// </summary>
    internal static Border MakePanel(Control content)
    {
        return new Border
        {
            Child = content,
            Background = EditorChrome.PanelBrush,
        };
    }
}
