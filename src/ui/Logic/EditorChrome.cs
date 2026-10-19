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
using Avalonia.Styling;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// The editor look shared by the redesigned windows (ASSA draw and friends, burn-in, ...): a palette
/// with a dark and a light version that follows the app theme, flat tool buttons, compact fields,
/// section headers and accent menus.
/// </summary>
internal static partial class EditorChrome
{
    // The editor palette, derived from the theme and appearance settings (see EditorChromePalette.cs).
    // Each color is one shared brush whose color follows the app theme (also when it changes while
    // windows are open), so everything built from it repaints without rebuilding.
    internal static bool IsLightTheme => Application.Current?.ActualThemeVariant == ThemeVariant.Light;

    // The editor look's sizes are design values at 100 % that follow the "Font scale (%)" setting
    // (#16242): an explicit FontSize does not inherit the scaled window font, so every text size
    // goes through ScaledFont, and the fixed heights and widths that must fit the text (the 28 px
    // fields) through ScaledSize. Icons keep their size: an Icon with its own FontSize is left
    // alone, and one that inherits gets the scale undone by the icon style in UiTheme.

    /// <summary>The text size of the fields, labels and buttons: 12.5 at 100 %.</summary>
    internal static double FieldFontSize => ScaledFont(12.5);

    /// <summary>The height of the compact fields, combo boxes and buttons: 28 at 100 %.</summary>
    internal static double FieldHeight => ScaledSize(28);

    /// <summary>A text size of the editor look scaled by the "Font scale (%)" setting.</summary>
    internal static double ScaledFont(double designSize) => UiUtil.ScaledFontSize(designSize);

    /// <summary>A height or width of the editor look that holds text, scaled by the "Font scale (%)" setting.</summary>
    internal static double ScaledSize(double designSize) => Math.Round(designSize * UiTheme.FontScale);

    /// <summary>
    /// Text and icons on hover - white on the dark palette, near black on the light one.
    /// </summary>
    internal static IBrush StrongTextBrush => StrongTextColor.Brush;

    /// <summary>
    /// Shape thumbnails (shape library, box and bar styles): light gray on dark, dark gray on light.
    /// </summary>
    internal static IBrush ThumbnailBrush => ThumbnailColor.Brush;
    internal static IBrush ThumbnailMutedBrush => ThumbnailMutedColor.Brush;

    internal static IBrush WindowBrush => WindowColor.Brush;
    internal static IBrush PanelBrush => PanelColor.Brush;
    internal static IBrush StatusBrush => StatusColor.Brush;
    internal static IBrush ButtonBarBrush => ButtonBarColor.Brush;
    internal static IBrush LineBrush => LineColor.Brush;
    internal static IBrush SeparatorBrush => SeparatorColor.Brush;
    internal static IBrush DimTextBrush => DimTextColor.Brush;
    internal static IBrush FaintTextBrush => FaintTextColor.Brush;
    internal static IBrush IconBrush => IconColor.Brush;
    internal static IBrush HoverBrush => HoverColor.Brush;
    internal static IBrush PressedBrush => PressedColor.Brush;
    internal static IBrush AccentBrush => AccentColor.Brush;
    internal static IBrush AccentHoverBrush => AccentHoverColor.Brush;
    internal static IBrush AccentSoftBrush => AccentSoftColor.Brush;
    internal static IBrush FieldBrush => FieldColor.Brush;
    internal static IBrush FieldBorderBrush => FieldBorderColor.Brush;
    internal static IBrush MenuBrush => MenuColor.Brush;
    internal static IBrush MenuBorderBrush => MenuBorderColor.Brush;

    /// <summary>
    /// Text on an AccentSoftBrush badge (counts): white on dark, accent on light.
    /// </summary>
    internal static IBrush BadgeTextBrush => BadgeTextColor.Brush;

    /// <summary>
    /// Text and icons on an accent surface (active tool buttons, primary buttons): white, or
    /// black when the theme's accent is too light for white.
    /// </summary>
    internal static IBrush OnAccentBrush => OnAccentColor.Brush;

    private static readonly ThemedBrush StrongTextColor = new(p => p.StrongText);
    private static readonly ThemedBrush ThumbnailColor = new(p => p.Thumbnail);
    private static readonly ThemedBrush ThumbnailMutedColor = new(p => p.ThumbnailMuted);
    private static readonly ThemedBrush WindowColor = new(p => p.Window);
    private static readonly ThemedBrush PanelColor = new(p => p.Panel);
    private static readonly ThemedBrush StatusColor = new(p => p.Status);
    private static readonly ThemedBrush ButtonBarColor = new(p => p.ButtonBar);
    private static readonly ThemedBrush LineColor = new(p => p.Line);
    private static readonly ThemedBrush SeparatorColor = new(p => p.Separator);
    private static readonly ThemedBrush DimTextColor = new(p => p.DimText);
    private static readonly ThemedBrush FaintTextColor = new(p => p.FaintText);
    private static readonly ThemedBrush IconColor = new(p => p.Icon);
    private static readonly ThemedBrush HoverColor = new(p => p.Hover);
    private static readonly ThemedBrush PressedColor = new(p => p.Pressed);
    private static readonly ThemedBrush AccentColor = new(p => p.Accent);
    private static readonly ThemedBrush AccentHoverColor = new(p => p.AccentHover);
    private static readonly ThemedBrush AccentSoftColor = new(p => p.AccentSoft);
    private static readonly ThemedBrush FieldColor = new(p => p.Field);
    private static readonly ThemedBrush FieldBorderColor = new(p => p.FieldBorder);
    private static readonly ThemedBrush MenuColor = new(p => p.Menu);
    private static readonly ThemedBrush MenuBorderColor = new(p => p.MenuBorder);
    private static readonly ThemedBrush BadgeTextColor = new(p => p.BadgeText);
    private static readonly ThemedBrush OnAccentColor = new(p => p.OnAccent);

    /// <summary>
    /// A brush outside the palette that also follows the app theme (e.g. a status color with a
    /// darker light-theme version). Create it once - keep it in a static field - not per control.
    /// </summary>
    internal static IBrush Themed(Color dark, Color light) => new ThemedBrush(p => p.IsDark ? dark : light).Brush;

    /// <summary>
    /// One palette entry: a single mutable brush shared by every control that uses it. Its color is
    /// picked from the current palette on first use and again whenever the app theme changes or
    /// the palette is invalidated.
    /// </summary>
    private sealed class ThemedBrush
    {
        private static readonly System.Collections.Generic.List<ThemedBrush> All = new();
        private static Application? _hookedApplication;

        private readonly Func<EditorPalette, Color> _pick;
        private readonly SolidColorBrush _brush;

        public ThemedBrush(Func<EditorPalette, Color> pick)
        {
            _pick = pick;
            _brush = new SolidColorBrush(pick(_palette ?? EditorPalette.Default));
            lock (All)
            {
                All.Add(this);
            }
        }

        /// <summary>Repaint every palette brush from the current palette (UI thread).</summary>
        public static void RefreshAll()
        {
            ThemedBrush[] entries;
            lock (All)
            {
                entries = All.ToArray();
            }

            foreach (var entry in entries)
            {
                entry.Refresh();
            }
        }

        public IBrush Brush
        {
            get
            {
                // A brush may only be changed on the UI thread; a read from elsewhere (a view model
                // or test built off the UI thread) gets the brush as it is, and the UI thread
                // brings it up to date on its next read or theme change.
                if (Dispatcher.UIThread.CheckAccess())
                {
                    Hook();
                    Refresh();
                }

                return _brush;
            }
        }

        private void Refresh()
        {
            var color = _pick(Palette);
            if (_brush.Color != color)
            {
                _brush.Color = color;
            }
        }

        private static void Hook()
        {
            var application = Application.Current;
            if (application == null || ReferenceEquals(application, _hookedApplication))
            {
                return;
            }

            _hookedApplication = application;
            application.ActualThemeVariantChanged += (_, _) => RefreshAll();
        }
    }

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
            (ContentPresenter.BackgroundProperty, AccentBrush), (ContentPresenter.ForegroundProperty, OnAccentBrush)));
        styles.Add(ToolPresenter(x => x.OfType<Button>().Class("tool").Class("active").Class(":pointerover"),
            (ContentPresenter.BackgroundProperty, AccentHoverBrush), (ContentPresenter.ForegroundProperty, OnAccentBrush)));

        // Primary button (OK) in the editor accent
        styles.Add(ToolPresenter(x => x.OfType<Button>().Class("primary"),
            (ContentPresenter.BackgroundProperty, AccentBrush), (ContentPresenter.ForegroundProperty, OnAccentBrush)));
        styles.Add(ToolPresenter(x => x.OfType<Button>().Class("primary").Class(":pointerover"),
            (ContentPresenter.BackgroundProperty, AccentHoverBrush), (ContentPresenter.ForegroundProperty, OnAccentBrush)));

        // Compact number fields
        styles.Add(new Style(x => x.OfType<NumericUpDown>().Class("field"))
        {
            Setters =
            {
                new Setter(NumericUpDown.ShowButtonSpinnerProperty, false),
                new Setter(NumericUpDown.MinHeightProperty, FieldHeight),
                new Setter(NumericUpDown.HeightProperty, FieldHeight),
                new Setter(NumericUpDown.FontSizeProperty, FieldFontSize),
                new Setter(NumericUpDown.BackgroundProperty, FieldBrush),
                new Setter(NumericUpDown.BorderBrushProperty, FieldBorderBrush),
                new Setter(NumericUpDown.CornerRadiusProperty, new CornerRadius(6)),
            },
        });
        // Remove Fluent's 32px minimum to fit compact fields.
        styles.Add(new Style(x => x.OfType<NumericUpDown>().Class("field").Template().OfType<ButtonSpinner>())
        {
            Setters = { new Setter(ButtonSpinner.MinHeightProperty, 0.0) },
        });
        styles.Add(new Style(x => x.OfType<NumericUpDown>().Class("field").Template().OfType<TextBox>())
        {
            Setters =
            {
                new Setter(TextBox.MinHeightProperty, 0.0),
                new Setter(TextBox.PaddingProperty, new Thickness(6, 0)),
                new Setter(TextBox.VerticalContentAlignmentProperty, VerticalAlignment.Center),
                new Setter(TextBox.BackgroundProperty, Brushes.Transparent),
                new Setter(TextBox.CornerRadiusProperty, new Binding(nameof(NumericUpDown.CornerRadius))
                {
                    RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent),
                }),
            },
        });

        // Text boxes keep their own background while hovered/focused - the theme turns a focused
        // box black (dark) or white (light), which reads as a different control on the field color.
        foreach (var state in new[] { ":pointerover", ":focus" })
        {
            styles.Add(new Style(x => x.OfType<TextBox>().Class(state).Template().OfType<Border>().Name("PART_BorderElement"))
            {
                Setters =
                {
                    new Setter(Border.BackgroundProperty, new Binding(nameof(TextBox.Background))
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent),
                    }),
                },
            });
        }

        // A switch without on/off texts and header is just the 40px knob track - the theme keeps a
        // 12px content column after it, which put a wide gap between a switch and its label.
        styles.Add(new Style(x => x.OfType<ToggleSwitch>()
            .PropertyEquals(ContentControl.ContentProperty, null)
            .PropertyEquals(ToggleSwitch.OnContentProperty, null)
            .PropertyEquals(ToggleSwitch.OffContentProperty, null))
        {
            Setters =
            {
                new Setter(Layoutable.WidthProperty, 40.0),
                new Setter(Layoutable.MinWidthProperty, 0.0),
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
                new Setter(Border.MinHeightProperty, FieldHeight),
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
            FontSize = ScaledFont(12),
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
                FontSize = ScaledFont(11),
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
        row.Children.Add(new TextBlock { Text = text, FontSize = FieldFontSize, VerticalAlignment = VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap });
        return row;
    }

    internal static TextBlock MakeSectionHeader(string text)
    {
        return new TextBlock
        {
            Text = text.ToUpperInvariant(),
            FontSize = ScaledFont(11),
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
