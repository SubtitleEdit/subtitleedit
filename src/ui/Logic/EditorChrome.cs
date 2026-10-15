using System;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// The editor look shared by the redesigned windows (ASSA draw and friends, burn-in, ...): a palette
/// with a dark and a light version that follows the app theme, flat tool buttons, compact fields,
/// section headers and accent menus.
/// </summary>
internal static class EditorChrome
{
    // The editor palette, in a dark and a light version: these windows follow the app theme
    // (picked when a window is built). Accent colors are the same in both.
    internal static bool IsLightTheme => Application.Current?.ActualThemeVariant == ThemeVariant.Light;

    /// <summary>
    /// Text and icons on hover - white on the dark palette, near black on the light one.
    /// </summary>
    internal static IBrush StrongTextBrush => IsLightTheme ? Brushes.Black : Brushes.White;

    /// <summary>
    /// Shape thumbnails (shape library, box and bar styles): light gray on dark, dark gray on light.
    /// </summary>
    internal static IBrush ThumbnailBrush => IsLightTheme ? LightThumbnail : DarkThumbnail;
    internal static IBrush ThumbnailMutedBrush => IsLightTheme ? LightThumbnailMuted : DarkThumbnailMuted;
    private static readonly IBrush DarkThumbnail = new ImmutableSolidColorBrush(Color.FromRgb(214, 218, 226));
    private static readonly IBrush LightThumbnail = new ImmutableSolidColorBrush(Color.FromRgb(72, 78, 92));
    private static readonly IBrush DarkThumbnailMuted = new ImmutableSolidColorBrush(Color.FromRgb(96, 102, 116));
    private static readonly IBrush LightThumbnailMuted = new ImmutableSolidColorBrush(Color.FromRgb(188, 193, 204));

    private static readonly IBrush DarkWindowBrush = new ImmutableSolidColorBrush(Color.FromRgb(30, 31, 36));
    private static readonly IBrush LightWindowBrush = new ImmutableSolidColorBrush(Color.FromRgb(243, 244, 247));
    internal static IBrush WindowBrush => IsLightTheme ? LightWindowBrush : DarkWindowBrush;
    private static readonly IBrush DarkPanelBrush = new ImmutableSolidColorBrush(Color.FromRgb(36, 38, 44));
    private static readonly IBrush LightPanelBrush = new ImmutableSolidColorBrush(Color.FromRgb(250, 250, 252));
    internal static IBrush PanelBrush => IsLightTheme ? LightPanelBrush : DarkPanelBrush;
    private static readonly IBrush DarkStatusBrush = new ImmutableSolidColorBrush(Color.FromRgb(27, 28, 33));
    private static readonly IBrush LightStatusBrush = new ImmutableSolidColorBrush(Color.FromRgb(236, 238, 242));
    internal static IBrush StatusBrush => IsLightTheme ? LightStatusBrush : DarkStatusBrush;
    private static readonly IBrush DarkButtonBarBrush = new ImmutableSolidColorBrush(Color.FromRgb(32, 34, 39));
    private static readonly IBrush LightButtonBarBrush = new ImmutableSolidColorBrush(Color.FromRgb(238, 240, 244));
    internal static IBrush ButtonBarBrush => IsLightTheme ? LightButtonBarBrush : DarkButtonBarBrush;
    private static readonly IBrush DarkLineBrush = new ImmutableSolidColorBrush(Color.FromRgb(47, 50, 58));
    private static readonly IBrush LightLineBrush = new ImmutableSolidColorBrush(Color.FromRgb(222, 225, 232));
    internal static IBrush LineBrush => IsLightTheme ? LightLineBrush : DarkLineBrush;
    private static readonly IBrush DarkSeparatorBrush = new ImmutableSolidColorBrush(Color.FromRgb(55, 58, 67));
    private static readonly IBrush LightSeparatorBrush = new ImmutableSolidColorBrush(Color.FromRgb(208, 212, 220));
    internal static IBrush SeparatorBrush => IsLightTheme ? LightSeparatorBrush : DarkSeparatorBrush;
    private static readonly IBrush DarkDimTextBrush = new ImmutableSolidColorBrush(Color.FromRgb(140, 146, 160));
    private static readonly IBrush LightDimTextBrush = new ImmutableSolidColorBrush(Color.FromRgb(92, 98, 112));
    internal static IBrush DimTextBrush => IsLightTheme ? LightDimTextBrush : DarkDimTextBrush;
    private static readonly IBrush DarkFaintTextBrush = new ImmutableSolidColorBrush(Color.FromRgb(111, 117, 131));
    private static readonly IBrush LightFaintTextBrush = new ImmutableSolidColorBrush(Color.FromRgb(122, 128, 142));
    internal static IBrush FaintTextBrush => IsLightTheme ? LightFaintTextBrush : DarkFaintTextBrush;
    private static readonly IBrush DarkIconBrush = new ImmutableSolidColorBrush(Color.FromRgb(174, 180, 194));
    private static readonly IBrush LightIconBrush = new ImmutableSolidColorBrush(Color.FromRgb(70, 76, 90));
    internal static IBrush IconBrush => IsLightTheme ? LightIconBrush : DarkIconBrush;
    private static readonly IBrush DarkHoverBrush = new ImmutableSolidColorBrush(Color.FromRgb(51, 54, 63));
    private static readonly IBrush LightHoverBrush = new ImmutableSolidColorBrush(Color.FromRgb(226, 229, 236));
    internal static IBrush HoverBrush => IsLightTheme ? LightHoverBrush : DarkHoverBrush;
    private static readonly IBrush DarkPressedBrush = new ImmutableSolidColorBrush(Color.FromRgb(59, 62, 71));
    private static readonly IBrush LightPressedBrush = new ImmutableSolidColorBrush(Color.FromRgb(214, 218, 227));
    internal static IBrush PressedBrush => IsLightTheme ? LightPressedBrush : DarkPressedBrush;
    private static readonly IBrush DarkAccentBrush = new ImmutableSolidColorBrush(Color.FromRgb(76, 110, 245));
    private static readonly IBrush LightAccentBrush = new ImmutableSolidColorBrush(Color.FromRgb(76, 110, 245));
    internal static IBrush AccentBrush => IsLightTheme ? LightAccentBrush : DarkAccentBrush;
    private static readonly IBrush DarkAccentHoverBrush = new ImmutableSolidColorBrush(Color.FromRgb(92, 124, 250));
    private static readonly IBrush LightAccentHoverBrush = new ImmutableSolidColorBrush(Color.FromRgb(92, 124, 250));
    internal static IBrush AccentHoverBrush => IsLightTheme ? LightAccentHoverBrush : DarkAccentHoverBrush;
    private static readonly IBrush DarkAccentSoftBrush = new ImmutableSolidColorBrush(Color.FromArgb(70, 76, 110, 245));
    private static readonly IBrush LightAccentSoftBrush = new ImmutableSolidColorBrush(Color.FromArgb(46, 76, 110, 245));
    internal static IBrush AccentSoftBrush => IsLightTheme ? LightAccentSoftBrush : DarkAccentSoftBrush;
    private static readonly IBrush DarkFieldBrush = new ImmutableSolidColorBrush(Color.FromRgb(27, 28, 33));
    private static readonly IBrush LightFieldBrush = new ImmutableSolidColorBrush(Color.FromRgb(255, 255, 255));
    internal static IBrush FieldBrush => IsLightTheme ? LightFieldBrush : DarkFieldBrush;
    private static readonly IBrush DarkFieldBorderBrush = new ImmutableSolidColorBrush(Color.FromRgb(54, 57, 68));
    private static readonly IBrush LightFieldBorderBrush = new ImmutableSolidColorBrush(Color.FromRgb(204, 209, 219));
    internal static IBrush FieldBorderBrush => IsLightTheme ? LightFieldBorderBrush : DarkFieldBorderBrush;
    private static readonly IBrush DarkMenuBrush = new ImmutableSolidColorBrush(Color.FromRgb(42, 44, 51));
    private static readonly IBrush LightMenuBrush = new ImmutableSolidColorBrush(Color.FromRgb(255, 255, 255));
    internal static IBrush MenuBrush => IsLightTheme ? LightMenuBrush : DarkMenuBrush;
    private static readonly IBrush DarkMenuBorderBrush = new ImmutableSolidColorBrush(Color.FromRgb(66, 69, 79));
    private static readonly IBrush LightMenuBorderBrush = new ImmutableSolidColorBrush(Color.FromRgb(208, 212, 220));
    internal static IBrush MenuBorderBrush => IsLightTheme ? LightMenuBorderBrush : DarkMenuBorderBrush;

    /// <summary>
    /// Flat icon buttons with an accent "active" state, compact number fields and accent menus.
    /// </summary>
    internal static void AddEditorStyles(Styles styles)
    {
        static Style ToolPresenter(Func<Selector?, Selector> button, params (AvaloniaProperty Property, object Value)[] setters)
        {
            var style = new Style(x => button(x).Template().OfType<ContentPresenter>().Name("PART_ContentPresenter"));
            foreach (var (property, value) in setters)
            {
                style.Setters.Add(new Setter(property, value));
            }

            return style;
        }

        styles.Add(new Style(x => x.OfType<Button>().Class("tool"))
        {
            Setters =
            {
                new Setter(Button.BackgroundProperty, Brushes.Transparent),
                new Setter(Button.BorderThicknessProperty, new Thickness(0)),
                new Setter(Button.CornerRadiusProperty, new CornerRadius(7)),
                new Setter(Button.ForegroundProperty, IconBrush),
                new Setter(Button.PaddingProperty, new Thickness(0)),
            },
        });
        styles.Add(ToolPresenter(x => x.OfType<Button>().Class("tool").Class(":pointerover"),
            (ContentPresenter.BackgroundProperty, HoverBrush), (ContentPresenter.ForegroundProperty, StrongTextBrush)));
        styles.Add(ToolPresenter(x => x.OfType<Button>().Class("tool").Class(":pressed"),
            (ContentPresenter.BackgroundProperty, PressedBrush)));
        styles.Add(ToolPresenter(x => x.OfType<Button>().Class("tool").Class(":disabled"),
            (ContentPresenter.BackgroundProperty, Brushes.Transparent), (ContentPresenter.ForegroundProperty, SeparatorBrush)));
        styles.Add(ToolPresenter(x => x.OfType<Button>().Class("tool").Class("active"),
            (ContentPresenter.BackgroundProperty, AccentBrush), (ContentPresenter.ForegroundProperty, Brushes.White)));
        styles.Add(ToolPresenter(x => x.OfType<Button>().Class("tool").Class("active").Class(":pointerover"),
            (ContentPresenter.BackgroundProperty, AccentHoverBrush), (ContentPresenter.ForegroundProperty, Brushes.White)));

        // Primary button (OK) in the editor accent
        styles.Add(ToolPresenter(x => x.OfType<Button>().Class("primary"),
            (ContentPresenter.BackgroundProperty, AccentBrush), (ContentPresenter.ForegroundProperty, Brushes.White)));
        styles.Add(ToolPresenter(x => x.OfType<Button>().Class("primary").Class(":pointerover"),
            (ContentPresenter.BackgroundProperty, AccentHoverBrush), (ContentPresenter.ForegroundProperty, Brushes.White)));

        // Compact number fields
        styles.Add(new Style(x => x.OfType<NumericUpDown>().Class("field"))
        {
            Setters =
            {
                new Setter(NumericUpDown.ShowButtonSpinnerProperty, false),
                new Setter(NumericUpDown.MinHeightProperty, 28.0),
                new Setter(NumericUpDown.HeightProperty, 28.0),
                new Setter(NumericUpDown.FontSizeProperty, 12.5),
                new Setter(NumericUpDown.BackgroundProperty, FieldBrush),
                new Setter(NumericUpDown.BorderBrushProperty, FieldBorderBrush),
                new Setter(NumericUpDown.CornerRadiusProperty, new CornerRadius(6)),
            },
        });
        styles.Add(new Style(x => x.OfType<NumericUpDown>().Class("field").Template().OfType<TextBox>())
        {
            Setters =
            {
                new Setter(TextBox.MinHeightProperty, 0.0),
                new Setter(TextBox.HeightProperty, 28.0),
                new Setter(TextBox.PaddingProperty, new Thickness(6, 0)),
                new Setter(TextBox.VerticalContentAlignmentProperty, VerticalAlignment.Center),
                new Setter(TextBox.BackgroundProperty, FieldBrush),
            },
        });

        // Menus: rounded, darker, accent highlight
        foreach (var menuType in new[] { typeof(ContextMenu), typeof(MenuFlyoutPresenter) })
        {
            styles.Add(new Style(x => x.OfType(menuType))
            {
                Setters =
                {
                    new Setter(TemplatedControl.BackgroundProperty, MenuBrush),
                    new Setter(TemplatedControl.BorderBrushProperty, MenuBorderBrush),
                    new Setter(TemplatedControl.CornerRadiusProperty, new CornerRadius(9)),
                    new Setter(TemplatedControl.PaddingProperty, new Thickness(4)),
                },
            });
        }

        styles.Add(new Style(x => x.OfType<MenuItem>())
        {
            Setters = { new Setter(MenuItem.CornerRadiusProperty, new CornerRadius(5)) },
        });
        styles.Add(new Style(x => x.OfType<MenuItem>().Class(":pointerover").Template().OfType<Border>().Name("PART_LayoutRoot"))
        {
            Setters = { new Setter(Border.BackgroundProperty, AccentBrush) },
        });
        styles.Add(new Style(x => x.OfType<MenuItem>().Class(":open").Template().OfType<Border>().Name("PART_LayoutRoot"))
        {
            Setters = { new Setter(Border.BackgroundProperty, AccentBrush) },
        });

        // Tree: rounded rows, soft accent selection
        styles.Add(new Style(x => x.OfType<TreeViewItem>().Template().OfType<Border>().Name("PART_LayoutRoot"))
        {
            Setters =
            {
                new Setter(Border.CornerRadiusProperty, new CornerRadius(6)),
                new Setter(Border.MinHeightProperty, 28.0),
            },
        });
        styles.Add(new Style(x => x.OfType<TreeViewItem>().Class(":selected").Template().OfType<Border>().Name("PART_LayoutRoot"))
        {
            Setters = { new Setter(Border.BackgroundProperty, AccentSoftBrush) },
        });
        styles.Add(new Style(x => x.OfType<TreeViewItem>().Class(":selected").Class(":pointerover").Template().OfType<Border>().Name("PART_LayoutRoot"))
        {
            Setters = { new Setter(Border.BackgroundProperty, AccentSoftBrush) },
        });
    }

    internal static MenuItem MakeMenuItem(string header, string? icon, System.Windows.Input.ICommand? command, KeyGesture? gesture = null)
    {
        var item = new MenuItem
        {
            Header = header,
            Command = command,
            InputGesture = gesture,
        };

        if (icon != null)
        {
            item.Icon = new Optris.Icons.Avalonia.Icon { Value = icon, FontSize = 13 };
        }

        return item;
    }

    internal static TextBlock MakeDimLabel(string text, Thickness margin)
    {
        return new TextBlock
        {
            Text = text,
            Foreground = DimTextBrush,
            FontSize = 12,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = margin,
        };
    }

    /// <summary>
    /// Compact number field without spinner buttons, optionally with a dim prefix like "X".
    /// </summary>
    internal static NumericUpDown MakeField(string property, double minimum, double maximum, double width = double.NaN, string? prefix = null, string format = "0")
    {
        var field = new NumericUpDown
        {
            Minimum = (decimal)minimum,
            Maximum = (decimal)maximum,
            Increment = 1,
            Width = width,
            FormatString = format,
            VerticalAlignment = VerticalAlignment.Center,
            [!NumericUpDown.ValueProperty] = new Binding(property) { Mode = BindingMode.TwoWay },
        };
        field.Classes.Add("field");
        if (prefix != null)
        {
            field.InnerLeftContent = new TextBlock
            {
                Text = prefix,
                Foreground = FaintTextBrush,
                FontSize = 11,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(8, 0, 0, 0),
            };
        }

        return field;
    }

    internal static Border MakeToolbarSeparator()
    {
        return new Border
        {
            Width = 1,
            Height = 22,
            Background = SeparatorBrush,
            Margin = new Thickness(8, 0),
            VerticalAlignment = VerticalAlignment.Center,
        };
    }

    internal static Button CreateToolButton(string icon, string tooltip, System.Windows.Input.ICommand? command, double size = 30, string? shortcut = null)
    {
        Control content = new Optris.Icons.Avalonia.Icon { Value = icon, FontSize = size > 34 ? 15 : 13 };
        if (shortcut != null)
        {
            // Shortcut letter in the corner, like the tool palettes of drawing programs
            var grid = new Grid { Width = size, Height = size };
            content.HorizontalAlignment = HorizontalAlignment.Center;
            content.VerticalAlignment = VerticalAlignment.Center;
            grid.Children.Add(content);
            grid.Children.Add(new TextBlock
            {
                Text = shortcut,
                FontSize = 8,
                Opacity = 0.6,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(0, 0, 4, 2),
            });
            content = grid;
        }

        var button = new Button
        {
            Content = content,
            Width = size,
            Height = size,
            Command = command,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
        };
        button.Classes.Add("tool");
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(button, tooltip);
        }

        // Icon-only, so the tool tip text is the only text there is for a screen reader (#12087).
        AutomationProperties.SetName(button, tooltip);
        return button;
    }

    /// <summary>
    /// A properties section like ASSA draw's: small caps header (with optional text on the right), line on top.
    /// </summary>
    internal static Border MakeEditorSection(string title, Control content, Control? headerRight = null, bool first = false)
    {
        var header = new DockPanel();
        if (headerRight != null)
        {
            DockPanel.SetDock(headerRight, Dock.Right);
            header.Children.Add(headerRight);
        }

        header.Children.Add(MakeSectionHeader(title));
        var panel = new StackPanel { Spacing = 10 };
        panel.Children.Add(header);
        panel.Children.Add(content);
        return new Border
        {
            Child = panel,
            BorderBrush = LineBrush,
            BorderThickness = new Thickness(0, first ? 0 : 1, 0, 0),
            Padding = new Thickness(14, 12, 14, 14),
        };
    }

    /// <summary>
    /// A compact on/off switch with its label to the right.
    /// </summary>
    internal static StackPanel MakeToggleRow(string property, string text)
    {
        var toggle = new ToggleSwitch
        {
            OnContent = null,
            OffContent = null,
            MinWidth = 0,
            VerticalAlignment = VerticalAlignment.Center,
            [!ToggleSwitch.IsCheckedProperty] = new Binding(property) { Mode = BindingMode.TwoWay },
        };
        AutomationProperties.SetName(toggle, text);
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        row.Children.Add(toggle);
        row.Children.Add(new TextBlock { Text = text, FontSize = 12.5, VerticalAlignment = VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap });
        return row;
    }

    internal static TextBlock MakeSectionHeader(string text)
    {
        return new TextBlock
        {
            Text = text.ToUpperInvariant(),
            FontSize = 11,
            FontWeight = FontWeight.SemiBold,
            LetterSpacing = 0.6,
            Foreground = DimTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
        };
    }

    /// <summary>
    /// A "Label  [field] [field]" row of the properties panel.
    /// </summary>
    internal static Grid MakePropertyRow(string label, params Control[] fields)
    {
        var grid = new Grid { ColumnSpacing = 6 };
        grid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(70)));
        grid.Children.Add(MakeDimLabel(label, new Thickness(0)));
        for (var i = 0; i < fields.Length; i++)
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            Grid.SetColumn(fields[i], i + 1);
            grid.Children.Add(fields[i]);
        }

        return grid;
    }
}
