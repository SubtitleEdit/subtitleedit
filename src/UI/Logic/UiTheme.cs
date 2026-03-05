using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using Avalonia.VisualTree;
using AvaloniaEdit;
using AvaloniaEdit.Editing;
using Nikse.SubtitleEdit.Logic.Config;
using System;

namespace Nikse.SubtitleEdit.Logic;

public static class UiTheme
{
    private static IStyle? _lighterDarkStyle;
    private static IStyle? _layoutScaleMenuStyle;
    private static ResourceDictionary? _resourceOverrides;
    private static object? _themeChangeSubscription;

    public const string ThemeNameSystem = "System";
    public const string ThemeNameLight = "Light";
    public const string ThemeNameDark = "Dark";
    public const string ThemeNameClassic = "Classic";
    public const string ThemeNamePastel = "Pastel";

    public static FluentTheme? FluentTheme { get; set; }

    public static string ThemeName
    {
        get
        {
            var themeSetting = Se.Settings.Appearance.Theme;
            if (themeSetting == ThemeNameSystem)
            {
                if (Application.Current!.ActualThemeVariant == ThemeVariant.Dark)
                {
                    return ThemeNameDark;
                }

                return ThemeNameLight;
            }

            return themeSetting;
        }
    }

    public static bool IsDarkThemeEnabled()
    {
        return ThemeName == ThemeNameDark;
    }

    public static void SetCurrentTheme()
    {
        var themeSetting = Se.Settings.Appearance.Theme;

        // Unsubscribe from any previous theme change event
        if (_themeChangeSubscription != null)
        {
            Application.Current!.ActualThemeVariantChanged -= OnActualThemeVariantChanged;
            _themeChangeSubscription = null;
        }

        RemoveLighterDark();

        if (themeSetting == ThemeNameSystem)
        {
            // Let Avalonia track system theme automatically
            Application.Current!.RequestedThemeVariant = ThemeVariant.Default;
            if (ThemeName == ThemeNameDark)
            {
                ApplyLighterDark();
            }

            // Subscribe to theme changes
            Application.Current.ActualThemeVariantChanged += OnActualThemeVariantChanged;
            _themeChangeSubscription = new object(); // Mark as subscribed
        }
        else if (themeSetting == ThemeNameDark)
        {
            Application.Current!.RequestedThemeVariant = ThemeVariant.Dark;
            ApplyLighterDark();
        }
        else if (themeSetting == ThemeNameClassic)
        {
            Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
            ApplyWindowsClassicGray();
        }
        else if (themeSetting == ThemeNamePastel)
        {
            Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
            ApplyPastel();
        }
        else
        {
            Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        }

        ApplyMenuScaleStyle(Se.Settings.Appearance.LayoutScale);
        ApplyLayoutScaleToAllWindows();
    }

    private static void OnActualThemeVariantChanged(object? sender, EventArgs e)
    {
        if (Se.Settings.Appearance.Theme == ThemeNameSystem && Application.Current != null)
        {
            RemoveLighterDark();
            if (Application.Current.ActualThemeVariant == ThemeVariant.Dark)
            {
                ApplyLighterDark();
            }
        }
    }

    public const double ScaleStep = 0.1;
    public const double MinScale = 0.5;
    public const double MaxScale = 2.0;

    public static void ApplyScaleToWindow(Window window)
    {
        var factor = Se.Settings.Appearance.LayoutScale;

        if (window.Content is LayoutTransformControl ltc)
        {
            ltc.LayoutTransform = Math.Abs(factor - 1.0) < 0.0001
                ? null
                : new ScaleTransform(factor, factor);
            return;
        }

        if (Math.Abs(factor - 1.0) < 0.0001)
        {
            return;
        }

        if (window.Content is Control content)
        {
            window.Content = null;
            window.Content = new LayoutTransformControl
            {
                Child = content,
                LayoutTransform = new ScaleTransform(factor, factor)
            };
        }
    }

    public static void ApplyLayoutScaleToAllWindows()
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            return;
        }

        foreach (var window in desktop.Windows)
        {
            ApplyScaleToWindow(window);
        }
    }

    public static void SetLayoutScale(double factor)
    {
        Se.Settings.Appearance.LayoutScale = factor;
        ApplyMenuScaleStyle(factor);
        ApplyLayoutScaleToAllWindows();
        ApplyScaleToExistingMenus(factor);
    }

    private static void ApplyMenuScaleStyle(double factor)
    {
        if (Application.Current == null)
        {
            return;
        }

        // Remove previous style to force Avalonia to re-evaluate all styled controls
        if (_layoutScaleMenuStyle != null)
        {
            Application.Current.Styles.Remove(_layoutScaleMenuStyle);
            _layoutScaleMenuStyle = null;
        }

        // Create a new style each time with the scaled values baked in.
        // This forces re-evaluation on all controls including popup menus.
        var styles = new Styles();

        // Scale MenuItems in popups/context menus (outside LayoutTransformControl)
        var menuItemStyle = new Style(x => x.OfType<MenuItem>());
        menuItemStyle.Setters.Add(new Setter(TemplatedControl.FontSizeProperty, 14.0 * factor));
        menuItemStyle.Setters.Add(new Setter(Layoutable.MinHeightProperty, 32.0 * factor));
        styles.Add(menuItemStyle);

        // Reset MenuItems inside LayoutTransformControl (already scaled by transform)
        var ltcMenuItemStyle = new Style(x => x.OfType<LayoutTransformControl>().Descendant().OfType<MenuItem>());
        ltcMenuItemStyle.Setters.Add(new Setter(TemplatedControl.FontSizeProperty, 14.0));
        ltcMenuItemStyle.Setters.Add(new Setter(Layoutable.MinHeightProperty, 32.0));
        styles.Add(ltcMenuItemStyle);

        _layoutScaleMenuStyle = styles;
        Application.Current.Styles.Add(styles);
    }

    /// <summary>
    /// Walk all open windows, find Menu, ContextMenu, and MenuFlyout instances,
    /// and directly set FontSize/MinHeight on their items. Also register Opened
    /// handlers so dynamic items get scaled when the menu opens.
    /// </summary>
    private static void ApplyScaleToExistingMenus(double factor)
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            return;
        }

        foreach (var window in desktop.Windows)
        {
            foreach (var visual in window.GetVisualDescendants())
            {
                if (visual is Menu menu)
                {
                    // Scale submenu items only (top-level items are inside LTC and already scaled)
                    foreach (var obj in menu.Items)
                    {
                        if (obj is MenuItem topItem)
                        {
                            ScaleChildMenuItems(topItem, factor);
                        }
                    }
                }

                if (visual is not Control control)
                {
                    continue;
                }

                if (control.ContextMenu is { } contextMenu)
                {
                    ScaleMenuItems(contextMenu, factor);
                    contextMenu.Opened -= OnContextMenuOpened;
                    contextMenu.Opened += OnContextMenuOpened;
                }

                if (control.ContextFlyout is MenuFlyout menuFlyout)
                {
                    ScaleMenuFlyoutItems(menuFlyout, factor);
                    menuFlyout.Opened -= OnMenuFlyoutOpened;
                    menuFlyout.Opened += OnMenuFlyoutOpened;
                }
            }
        }
    }

    private static void ScaleChildMenuItems(MenuItem parent, double factor)
    {
        foreach (var obj in parent.Items)
        {
            if (obj is MenuItem item)
            {
                item.FontSize = 14.0 * factor;
                item.MinHeight = 32.0 * factor;
                ScaleChildMenuItems(item, factor);
            }
        }
    }

    private static void ScaleMenuItems(ItemsControl parent, double factor)
    {
        foreach (var obj in parent.Items)
        {
            if (obj is MenuItem item)
            {
                item.FontSize = 14.0 * factor;
                item.MinHeight = 32.0 * factor;
                ScaleMenuItems(item, factor);
            }
        }
    }

    private static void ScaleMenuFlyoutItems(MenuFlyout flyout, double factor)
    {
        foreach (var obj in flyout.Items)
        {
            if (obj is MenuItem item)
            {
                item.FontSize = 14.0 * factor;
                item.MinHeight = 32.0 * factor;
                ScaleMenuItems(item, factor);
            }
        }
    }

    private static void OnContextMenuOpened(object? sender, EventArgs e)
    {
        if (sender is ContextMenu cm)
        {
            var factor = Se.Settings.Appearance.LayoutScale;
            ScaleMenuItems(cm, factor);
        }
    }

    private static void OnMenuFlyoutOpened(object? sender, EventArgs e)
    {
        if (sender is MenuFlyout flyout)
        {
            var factor = Se.Settings.Appearance.LayoutScale;
            ScaleMenuFlyoutItems(flyout, factor);
        }
    }

    public static void UpdateRegionColor()
    {
        if (FluentTheme == null)
        {
            return;
        }

        if (FluentTheme.Palettes.TryGetValue(ThemeVariant.Dark, out var palette))
        {
            palette.RegionColor = GetDarkThemeBackgroundColor();
        }
    }

    private static void ApplyLighterDark()
    {
        if (Application.Current == null)
        {
            return;
        }

        UpdateRegionColor();

        var foreColor = GetDarkThemeForegroundColor();
        var bgColor = GetDarkThemeBackgroundColor();
        var bgColorLighter = UiUtil.LightenColor(bgColor, 5);
        var bgColorHeader = UiUtil.LightenColor(bgColor, 15);
        var foregroundBrush = new SolidColorBrush(foreColor);

        // Override Fluent theme resources for text controls to prevent white foreground on focus/hover
        _resourceOverrides = new ResourceDictionary
        {
            ["TextControlForeground"] = foregroundBrush,
            ["TextControlForegroundPointerOver"] = foregroundBrush,
            ["TextControlForegroundFocused"] = foregroundBrush,
            ["TextControlForegroundDisabled"] = foregroundBrush,
        };
        Application.Current.Resources.MergedDictionaries.Add(_resourceOverrides);

        _lighterDarkStyle = new Styles
        {
            // TextBox
            new Style(x => x.OfType<TextBox>())
            {
                Setters =
                {
                    new Setter(TextBox.BackgroundProperty, new SolidColorBrush(bgColor)),
                    new Setter(TextBox.ForegroundProperty, new SolidColorBrush(foreColor))
                }
            },
            new Style(x => x.OfType<TextBox>().Class(":focus").Template().OfType<Border>().Name("PART_BorderElement"))
            {
                Setters =
                {
                    new Setter(Border.BackgroundProperty, new SolidColorBrush(bgColor)) // focused color
                }
            },
            new Style(x =>
                x.OfType<TextBox>().Class(":pointerover").Template().OfType<Border>().Name("PART_BorderElement"))
            {
                Setters =
                {
                    new Setter(Border.BackgroundProperty, new SolidColorBrush(bgColorLighter)) // mouse over color
                }
            },

            // Button
            new Style(x => x.OfType<Button>())
            {
                Setters =
                {
                    new Setter(Button.ForegroundProperty, new SolidColorBrush(foreColor))
                }
            },

            // NumericUpDown
            new Style(x => x.OfType<NumericUpDown>())
            {
                Setters =
                {
                    new Setter(NumericUpDown.BackgroundProperty, new SolidColorBrush(bgColor)),
                    new Setter(NumericUpDown.ForegroundProperty, new SolidColorBrush(foreColor))
                }
            },

            // ComboBox
            new Style(x => x.OfType<ComboBox>())
            {
                Setters =
                {
                    new Setter(ComboBox.ForegroundProperty, new SolidColorBrush(foreColor))
                }
            },

            // RadioButton
            new Style(x => x.OfType<RadioButton>())
            {
                Setters =
                {
                    new Setter(RadioButton.ForegroundProperty, new SolidColorBrush(foreColor))
                }
            },

            // CheckBox
            new Style(x => x.OfType<CheckBox>())
            {
                Setters =
                {
                    new Setter(CheckBox.ForegroundProperty, new SolidColorBrush(foreColor))
                }
            },

            // ListBox
            new Style(x => x.OfType<ListBox>())
            {
                Setters =
                {
                    new Setter(ListBox.ForegroundProperty, new SolidColorBrush(foreColor))
                }
            },

            // DataGrid
            new Style(x => x.OfType<DataGrid>())
            {
                Setters =
                {
                    new Setter(DataGrid.ForegroundProperty, new SolidColorBrush(foreColor))
                }
            },

            // Label
            new Style(x => x.OfType<Label>())
            {
                Setters =
                {
                    new Setter(Label.ForegroundProperty, new SolidColorBrush(foreColor))
                }
            },

            // TextBlock
            new Style(x => x.OfType<TextBlock>())
            {
                Setters =
                {
                    new Setter(TextBlock.ForegroundProperty, new SolidColorBrush(foreColor))
                }
            },

            // TextArea
            new Style(x => x.OfType<TextArea>())
            {
                Setters =
                {
                    new Setter(TextArea.ForegroundProperty, new SolidColorBrush(foreColor)),
                    new Setter(TextArea.CaretBrushProperty, new SolidColorBrush(foreColor)),
                }
            },
            
            // TextArea when focused
            new Style(x => x.OfType<TextArea>().Class(":focus"))
            {
                Setters =
                {
                    new Setter(TextArea.ForegroundProperty, new SolidColorBrush(foreColor)),
                }
            },
            
            // TextArea when pointer is over
            new Style(x => x.OfType<TextArea>().Class(":pointerover"))
            {
                Setters =
                {
                    new Setter(TextArea.ForegroundProperty, new SolidColorBrush(foreColor)),
                }
            },
            
            // TextArea when both focused and pointer over (combined state)
            new Style(x => x.OfType<TextArea>().Class(":focus").Class(":pointerover"))
            {
                Setters =
                {
                    new Setter(TextArea.ForegroundProperty, new SolidColorBrush(foreColor)),
                }
            },

            // MenuItem
            new Style(x => x.OfType<Avalonia.Controls.MenuItem>())
            {
                Setters =
                {
                    new Setter(Avalonia.Controls.MenuItem.ForegroundProperty, new SolidColorBrush(foreColor))
                }
            },

            // Icon
            new Style(x => x.OfType<Projektanker.Icons.Avalonia.Icon>())
            {
                Setters =
                {
                    new Setter(Projektanker.Icons.Avalonia.Icon.ForegroundProperty, new SolidColorBrush(foreColor))
                }
            },

            // Menu / ContextMenu
            new Style(x => x.OfType<ContextMenu>())
            {
                Setters =
                {
                    new Setter(TemplatedControl.BackgroundProperty, new SolidColorBrush(bgColor)),
                    new Setter(TemplatedControl.ForegroundProperty, new SolidColorBrush(foreColor))
                }
            },

            // Flyout
            new Style(x => x.OfType<FlyoutPresenter>())
            {
                Setters =
                {
                    new Setter(TemplatedControl.BackgroundProperty, new SolidColorBrush(bgColor)),
                    new Setter(TemplatedControl.ForegroundProperty, new SolidColorBrush(foreColor))
                }
            },

            // DataGrid header
            new Style(x => x.OfType<DataGridColumnHeader>())
            {
                Setters =
                {
                    new Setter(DataGridColumnHeader.BackgroundProperty, new SolidColorBrush(bgColorHeader)),
                    new Setter(DataGridColumnHeader.ForegroundProperty, new SolidColorBrush(foreColor))
                }
            },

            // ButtonSpinner
            new Style(x => x.OfType<ButtonSpinner>())
            {
                Setters =
                {
                    new Setter(ButtonSpinner.BackgroundProperty, new SolidColorBrush(bgColor)),
                    new Setter(ButtonSpinner.ForegroundProperty, new SolidColorBrush(foreColor))
                }
            },
        };

        Application.Current.Styles.Add(_lighterDarkStyle);
    }

    private static void RemoveLighterDark()
    {
        if (_resourceOverrides != null)
        {
            Application.Current!.Resources.MergedDictionaries.Remove(_resourceOverrides);
            _resourceOverrides = null;
        }

        if (_lighterDarkStyle != null)
        {
            Application.Current!.Styles.Remove(_lighterDarkStyle);
            _lighterDarkStyle = null;
        }
    }

    private static void ApplyWindowsClassicGray()
    {
        if (Application.Current == null)
        {
            return;
        }

        // Windows Classic colors inspired by old WinForms
        var bgColor = Color.FromRgb(236, 233, 216); // Classic Windows control gray
        var buttonColor = Color.FromRgb(212, 208, 200); // Classic button gray
        var borderColor = Color.FromRgb(172, 168, 153); // Classic border
        var headerColor = Color.FromRgb(192, 192, 192); // Classic silver header
        var inputColor = Color.FromRgb(255, 255, 250); // Slightly off-white (ivory) for input controls

        _lighterDarkStyle = new Styles
        {
            // Window background
            new Style(x => x.OfType<Window>())
            {
                Setters =
                {
                    new Setter(Window.BackgroundProperty, new SolidColorBrush(bgColor))
                }
            },

            // TextBox - slightly off-white for all input controls
            new Style(x => x.OfType<TextBox>())
            {
                Setters =
                {
                    new Setter(TextBox.BackgroundProperty, new SolidColorBrush(inputColor)),
                    new Setter(TextBox.BorderBrushProperty, new SolidColorBrush(borderColor)),
                    new Setter(TextBox.BorderThicknessProperty, new Thickness(1))
                }
            },

            // Button
            new Style(x => x.OfType<Button>())
            {
                Setters =
                {
                    new Setter(Button.BackgroundProperty, new SolidColorBrush(buttonColor)),
                    new Setter(Button.BorderBrushProperty, new SolidColorBrush(borderColor)),
                    new Setter(Button.BorderThicknessProperty, new Thickness(1))
                }
            },

            // NumericUpDown - slightly off-white for consistency
            new Style(x => x.OfType<NumericUpDown>())
            {
                Setters =
                {
                    new Setter(NumericUpDown.BackgroundProperty, new SolidColorBrush(inputColor)),
                    new Setter(NumericUpDown.BorderBrushProperty, new SolidColorBrush(borderColor))
                }
            },

            // ComboBox - slightly off-white for consistency
            new Style(x => x.OfType<ComboBox>())
            {
                Setters =
                {
                    new Setter(ComboBox.BackgroundProperty, new SolidColorBrush(inputColor)),
                    new Setter(ComboBox.BorderBrushProperty, new SolidColorBrush(borderColor))
                }
            },

            // DataGrid header
            new Style(x => x.OfType<DataGridColumnHeader>())
            {
                Setters =
                {
                    new Setter(DataGridColumnHeader.BackgroundProperty, new SolidColorBrush(headerColor))
                }
            },

            // ButtonSpinner - slightly off-white for consistency (used by TimeCodeUpDown and SecondsUpDown)
            new Style(x => x.OfType<ButtonSpinner>())
            {
                Setters =
                {
                    new Setter(ButtonSpinner.BackgroundProperty, new SolidColorBrush(inputColor)),
                    new Setter(ButtonSpinner.BorderBrushProperty, new SolidColorBrush(borderColor))
                }
            },

            // SecondsUpDown - set default off-white background (will be overridden by duration color bindings when needed)
            new Style(x => x.OfType<Nikse.SubtitleEdit.Controls.SecondsUpDown>())
            {
                Setters =
                {
                    new Setter(Nikse.SubtitleEdit.Controls.SecondsUpDown.BackgroundProperty, new SolidColorBrush(inputColor))
                }
            },

            // TimeCodeUpDown - slightly off-white by default
            new Style(x => x.OfType<Nikse.SubtitleEdit.Controls.TimeCodeUpDown>())
            {
                Setters =
                {
                    new Setter(Nikse.SubtitleEdit.Controls.TimeCodeUpDown.BackgroundProperty, new SolidColorBrush(inputColor))
                }
            },

            // AvaloniaEdit TextEditor - slightly off-white for consistency
            new Style(x => x.OfType<TextEditor>())
            {
                Setters =
                {
                    new Setter(TextEditor.BackgroundProperty, new SolidColorBrush(inputColor))
                }
            },
        };

        Application.Current.Styles.Add(_lighterDarkStyle);
    }

    private static void ApplyPastel()
    {
        if (Application.Current == null)
        {
            return;
        }

        // Soft pastel colors with a lavender background
        var bgColor = Color.FromRgb(240, 235, 255); // Soft lavender
        var lightPink = Color.FromRgb(255, 228, 225); // Misty rose
        var lightBlue = Color.FromRgb(230, 245, 255); // Light azure
        var lightGreen = Color.FromRgb(240, 255, 240); // Honeydew
        var lightPurple = Color.FromRgb(245, 240, 255); // Lavender
        var borderColor = Color.FromRgb(200, 180, 200); // Soft lavender border

        _lighterDarkStyle = new Styles
        {
            // Window background with soft lavender
            new Style(x => x.OfType<Window>())
            {
                Setters =
                {
                    new Setter(Window.BackgroundProperty, new SolidColorBrush(bgColor))
                }
            },

            // TextBox with soft colors
            new Style(x => x.OfType<TextBox>())
            {
                Setters =
                {
                    new Setter(TextBox.BackgroundProperty, new SolidColorBrush(lightBlue)),
                    new Setter(TextBox.BorderBrushProperty, new SolidColorBrush(borderColor)),
                    new Setter(TextBox.BorderThicknessProperty, new Thickness(1))
                }
            },

            // Button with pastel colors
            new Style(x => x.OfType<Button>())
            {
                Setters =
                {
                    new Setter(Button.BackgroundProperty, new SolidColorBrush(lightPink)),
                    new Setter(Button.BorderBrushProperty, new SolidColorBrush(borderColor))
                }
            },

            // NumericUpDown
            new Style(x => x.OfType<NumericUpDown>())
            {
                Setters =
                {
                    new Setter(NumericUpDown.BackgroundProperty, new SolidColorBrush(lightGreen))
                }
            },

            // ComboBox
            new Style(x => x.OfType<ComboBox>())
            {
                Setters =
                {
                    new Setter(ComboBox.BackgroundProperty, new SolidColorBrush(lightGreen))
                }
            },

            // DataGrid header with pastel purple
            new Style(x => x.OfType<DataGridColumnHeader>())
            {
                Setters =
                {
                    new Setter(DataGridColumnHeader.BackgroundProperty, new SolidColorBrush(lightPurple))
                }
            },

            // ButtonSpinner (used by TimeCodeUpDown) with soft pink
            new Style(x => x.OfType<ButtonSpinner>())
            {
                Setters =
                {
                    new Setter(ButtonSpinner.BackgroundProperty, new SolidColorBrush(lightPink))
                }
            },

            // SecondsUpDown - soft pink by default (external bindings will override when needed)
            new Style(x => x.OfType<Nikse.SubtitleEdit.Controls.SecondsUpDown>())
            {
                Setters =
                {
                    new Setter(Nikse.SubtitleEdit.Controls.SecondsUpDown.BackgroundProperty, new SolidColorBrush(lightPink))
                }
            },

            // TimeCodeUpDown - soft pink by default
            new Style(x => x.OfType<Nikse.SubtitleEdit.Controls.TimeCodeUpDown>())
            {
                Setters =
                {
                    new Setter(Nikse.SubtitleEdit.Controls.TimeCodeUpDown.BackgroundProperty, new SolidColorBrush(lightPink))
                }
            },

            // AvaloniaEdit TextEditor with soft blue
            new Style(x => x.OfType<TextEditor>())
            {
                Setters =
                {
                    new Setter(TextEditor.BackgroundProperty, new SolidColorBrush(lightBlue))
                }
            },
        };

        Application.Current.Styles.Add(_lighterDarkStyle);
    }

    private static Color GetDarkThemeBackgroundColor()
    {
        return Se.Settings.Appearance.DarkModeBackgroundColor.FromHexToColor();
    }

    public static Color GetDarkThemeForegroundColor()
    {
        return Se.Settings.Appearance.DarkModeForegroundColor.FromHexToColor();
    }
}