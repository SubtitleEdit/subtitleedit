using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Controls.SyntaxTextEditorControl;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.IO;
using System.Linq;

namespace Nikse.SubtitleEdit.Logic;

public static class UiTheme
{
    private static IStyle? _themeOverrideStyle;
    private static IStyle? _layoutScaleMenuStyle;
    private static ResourceDictionary? _resourceOverrides;
    private static object? _themeChangeSubscription;

    /// <summary>
    /// Style class for a ContentControl hosting an icon that sits on a colored accent
    /// square - its glyph stays white in the dark theme instead of getting the custom
    /// dark-theme foreground (#12717).
    /// </summary>
    public const string IconOnAccentClassName = "icon-on-accent";

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
                // No Application in unit tests or at design time - fall back to dark.
                // ActualThemeVariant is UI-thread-affine, so an off-thread read (plain
                // xunit facts, worker threads) must fall back too instead of throwing.
                if (Application.Current == null || !Dispatcher.UIThread.CheckAccess())
                {
                    return ThemeNameDark;
                }

                if (Application.Current.ActualThemeVariant == ThemeVariant.Dark)
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

    /// <summary>
    /// Folder holding the current theme's images. These are unpacked from Themes.zip into
    /// <see cref="Se.ThemesFolder"/> at start-up - they are not embedded assets, so they cannot
    /// be reached through an avares:// URI. Falls back to Dark when the active theme ships no
    /// image folder of its own (Pastel, for instance), and an explicit icon theme wins over both.
    /// </summary>
    public static string ImageFolder
    {
        get
        {
            var folder = Path.Combine(Se.ThemesFolder, ThemeName);
            if (!Directory.Exists(folder))
            {
                folder = Path.Combine(Se.ThemesFolder, ThemeNameDark);
            }

            var iconTheme = Se.Settings.Appearance.IconTheme;
            if (!string.IsNullOrEmpty(iconTheme) && iconTheme != Se.Language.General.Auto)
            {
                var iconThemeFolder = Path.Combine(Se.ThemesFolder, iconTheme);
                if (Directory.Exists(iconThemeFolder))
                {
                    folder = iconThemeFolder;
                }
            }

            return folder;
        }
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

    public static Action? SystemThemeChangedCallback { get; set; }

    private static void OnActualThemeVariantChanged(object? sender, EventArgs e)
    {
        if (Se.Settings.Appearance.Theme == ThemeNameSystem && Application.Current != null)
        {
            SetCurrentTheme();
            SystemThemeChangedCallback?.Invoke();
        }
    }

    public const double ScaleStep = 0.1;
    public const double MinScale = 0.5;
    public const double MaxScale = 2.0;

    /// <summary>
    /// Returns the logical content of a window, unwrapping the
    /// <see cref="LayoutTransformControl"/> that <see cref="ApplyScaleToWindow"/>
    /// uses to host scaled content.
    /// </summary>
    public static Control? GetUnscaledContent(Window window)
    {
        return window.Content is LayoutTransformControl ltc
            ? ltc.Child as Control
            : window.Content as Control;
    }

    public static void ApplyScaleToWindow(Window window)
    {
        // Keep the OS-drawn title bar in sync with the app theme. Piggybacks on the universal
        // per-window hook so it covers both window-open (every caller does ApplyScaleToWindow)
        // and theme switches (ApplyLayoutScaleToAllWindows re-runs this for every open window).
        ApplyTitleBarTheme(window);

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

    /// <summary>
    /// Applies the Windows immersive dark-mode title bar to <paramref name="window"/> based on the
    /// current theme. The native window handle only exists once the window is shown, so when it
    /// isn't ready yet (e.g. ShowDialog applies chrome before the window opens) this defers to the
    /// window's Opened event. No-op on non-Windows platforms.
    /// </summary>
    public static void ApplyTitleBarTheme(Window window)
    {
        if (window == null || !OperatingSystem.IsWindows())
        {
            return;
        }

        var handle = window.TryGetPlatformHandle();
        if (handle != null && handle.Handle != IntPtr.Zero)
        {
            Platform.Windows.WindowsDarkMode.Apply(handle.Handle, IsDarkThemeEnabled());
            return;
        }

        void OnOpened(object? sender, EventArgs e)
        {
            window.Opened -= OnOpened;
            var openedHandle = window.TryGetPlatformHandle();
            if (openedHandle != null)
            {
                Platform.Windows.WindowsDarkMode.Apply(openedHandle.Handle, IsDarkThemeEnabled());
            }
        }

        window.Opened += OnOpened;
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

        // Scale ComboBox dropdown items too — the dropdown popup renders outside the
        // LayoutTransformControl, so the window scale transform never reaches it (#13010).
        // ComboBoxItems only ever appear inside that popup, so no LTC reset counterpart is
        // needed. Skipped at 100% so windows with locally restyled combos keep their look.
        if (Math.Abs(factor - 1.0) > 0.0001)
        {
            var comboBoxItemStyle = new Style(x => x.OfType<ComboBoxItem>());
            comboBoxItemStyle.Setters.Add(new Setter(TemplatedControl.FontSizeProperty, 14.0 * factor));
            styles.Add(comboBoxItemStyle);
        }

        _layoutScaleMenuStyle = styles;
        Application.Current.Styles.Add(styles);

        // The Fluent menu/context-menu popup templates cap their width at 456 via the
        // FlyoutThemeMaxWidth resource, which clips long localized menu items (e.g. Italian,
        // #13011) without an ellipsis. The cap sits on a Border inside the templates as a
        // DynamicResource, so a style cannot target it - overriding the resource is the only
        // lever, and it deliberately raises the cap for every flyout. It scales with the menu
        // font size above, so 150%+ layouts do not clip again; popups still size to their
        // content, so short menus are unaffected.
        Application.Current.Resources["FlyoutThemeMaxWidth"] = 680d * factor;
    }

    private static Styles? _scrollBarStyle;
    private static bool _scrollBarAllowAutoHide;
    /// <summary>
    /// Applies scrollbar visibility styles based on the OS preference.
    /// On macOS, reads "Show scroll bars" system setting. When set to "Always",
    /// forces always-expanded scrollbars. ListBox/ScrollViewer scrollbars respond to
    /// AllowAutoHide=false via the style system. Designed to be called once at startup.
    /// </summary>
    public static void ApplyScrollBarStyle()
    {
        if (Application.Current == null)
            return;

        if (_scrollBarStyle != null)
        {
            Application.Current.Styles.Remove(_scrollBarStyle);
            _scrollBarStyle = null;
        }

        var allowAutoHide = false;
        if (OperatingSystem.IsMacOS())
        {
            var pref = MacHelper.GetShowScrollBarsPreference();
            allowAutoHide = pref != "Always";
        }

        _scrollBarAllowAutoHide = allowAutoHide;

        _scrollBarStyle = new Styles
        {
            new Style(x => x.OfType<ScrollViewer>())
            {
                Setters = { new Setter(ScrollViewer.AllowAutoHideProperty, allowAutoHide) }
            },
            new Style(x => x.OfType<Avalonia.Controls.Primitives.ScrollBar>())
            {
                Setters = { new Setter(Avalonia.Controls.Primitives.ScrollBar.AllowAutoHideProperty, allowAutoHide) }
            },
        };


        Application.Current.Styles.Add(_scrollBarStyle);
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

        _themeOverrideStyle = new Styles
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

            // The two styles above paint every text box's inner PART_BorderElement with an
            // opaque fill on hover/focus. Inside an editable ComboBox the text box covers the
            // whole text column, on top of the combo's border, so that fill wipes the border
            // out on hover, while the drop-down is open, and after selecting (the box keeps
            // focus) - only the stretch around the arrow, which the text box does not cover,
            // survives. Fluent guards against exactly this with inline
            // TextControlBackgroundFocused/PointerOver=Transparent resources on the inner text
            // box, but the styles above set the border element directly, bypassing resources -
            // so undo them for text boxes hosted inside a ComboBox template. Later in the
            // collection = wins over the two styles above.
            new Style(x => x.OfType<ComboBox>().Template().OfType<TextBox>()
                .Class(":focus").Template().OfType<Border>().Name("PART_BorderElement"))
            {
                Setters =
                {
                    new Setter(Border.BackgroundProperty, Brushes.Transparent)
                }
            },
            new Style(x => x.OfType<ComboBox>().Template().OfType<TextBox>()
                .Class(":pointerover").Template().OfType<Border>().Name("PART_BorderElement"))
            {
                Setters =
                {
                    new Setter(Border.BackgroundProperty, Brushes.Transparent)
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

            // The source editor's text surface
            new Style(x => x.OfType<SyntaxTextView>())
            {
                Setters =
                {
                    new Setter(SyntaxTextView.ForegroundProperty, new SolidColorBrush(foreColor)),
                    new Setter(SyntaxTextView.CaretBrushProperty, new SolidColorBrush(foreColor)),

                    // The default translucent steel blue nearly disappears on a dark background -
                    // a brighter blue still lets the syntax colors read through on top.
                    new Setter(SyntaxTextView.SelectionBrushProperty, new SolidColorBrush(Color.FromArgb(0x99, 0x4C, 0x8D, 0xE0))),
                }
            },

            // ... and its line numbers
            new Style(x => x.OfType<LineNumberGutter>())
            {
                Setters =
                {
                    new Setter(LineNumberGutter.ForegroundProperty, new SolidColorBrush(foreColor)),
                }
            },

            // The source editor owns its text surface, so unlike a TextBox it does not inherit
            // the dark input background from Fluent.
            new Style(x => x.OfType<SyntaxTextEditor>())
            {
                Setters =
                {
                    new Setter(SyntaxTextEditor.BackgroundProperty, new SolidColorBrush(bgColor)),
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
            new Style(x => x.OfType<Optris.Icons.Avalonia.Icon>())
            {
                Setters =
                {
                    new Setter(Optris.Icons.Avalonia.Icon.ForegroundProperty, new SolidColorBrush(foreColor))
                }
            },

            // Icons on a colored accent square (settings sections, word lists, shortcut
            // groups) keep their white glyph: the blanket icon foreground above would wash
            // them out against the colored background (#12717). Hosts opt in by adding
            // IconOnAccentClassName; this must come after the blanket style so it wins.
            new Style(x => x.OfType<ContentControl>().Class(IconOnAccentClassName)
                    .Descendant().OfType<Optris.Icons.Avalonia.Icon>())
            {
                Setters =
                {
                    new Setter(Optris.Icons.Avalonia.Icon.ForegroundProperty, Brushes.White)
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


            // TableView header
            new Style(x => x.OfType<TableViewColumnHeader>())
            {
                Setters =
                {
                    new Setter(TableViewColumnHeader.BackgroundProperty, new SolidColorBrush(bgColorHeader)),
                    new Setter(TableViewColumnHeader.ForegroundProperty, new SolidColorBrush(foreColor))
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

        Application.Current.Styles.Add(_themeOverrideStyle);
    }

    private static void RemoveLighterDark()
    {
        if (_resourceOverrides != null)
        {
            Application.Current!.Resources.MergedDictionaries.Remove(_resourceOverrides);
            _resourceOverrides = null;
        }

        if (_themeOverrideStyle != null)
        {
            Application.Current!.Styles.Remove(_themeOverrideStyle);
            _themeOverrideStyle = null;
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

        _themeOverrideStyle = new Styles
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


            // TableView header
            new Style(x => x.OfType<TableViewColumnHeader>())
            {
                Setters =
                {
                    new Setter(TableViewColumnHeader.BackgroundProperty, new SolidColorBrush(headerColor))
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

            // The source editor - slightly off-white for consistency
            new Style(x => x.OfType<SyntaxTextEditor>())
            {
                Setters =
                {
                    new Setter(SyntaxTextEditor.BackgroundProperty, new SolidColorBrush(inputColor))
                }
            },
        };

        Application.Current.Styles.Add(_themeOverrideStyle);
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

        _themeOverrideStyle = new Styles
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


            // TableView header
            new Style(x => x.OfType<TableViewColumnHeader>())
            {
                Setters =
                {
                    new Setter(TableViewColumnHeader.BackgroundProperty, new SolidColorBrush(lightPurple))
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

            // The source editor with soft blue
            new Style(x => x.OfType<SyntaxTextEditor>())
            {
                Setters =
                {
                    new Setter(SyntaxTextEditor.BackgroundProperty, new SolidColorBrush(lightBlue))
                }
            },
        };

        Application.Current.Styles.Add(_themeOverrideStyle);
    }

    private static Color GetDarkThemeBackgroundColor()
    {
        return Se.Settings.Appearance.DarkModeBackgroundColor.FromHexToColor();
    }

    /// <summary>
    /// The window/grid background of the active theme. Used where something has to be drawn
    /// legibly against it (e.g. picking a readable ASSA color for the subtitle grid), so the
    /// light themes must not all be assumed to be white.
    /// </summary>
    public static Color GetThemeBackgroundColor()
    {
        try
        {
            if (IsDarkThemeEnabled())
            {
                return GetDarkThemeBackgroundColor();
            }
        }
        catch
        {
            // malformed DarkModeBackgroundColor in the settings file
            return Color.FromRgb(0x21, 0x21, 0x21);
        }

        var themeSetting = Se.Settings.Appearance.Theme;
        if (themeSetting == ThemeNameClassic)
        {
            return ClassicBackgroundColor;
        }

        return themeSetting == ThemeNamePastel ? PastelBackgroundColor : Colors.White;
    }

    private static readonly Color ClassicBackgroundColor = Color.FromRgb(236, 233, 216);
    private static readonly Color PastelBackgroundColor = Color.FromRgb(240, 235, 255);

    public static Color GetDarkThemeForegroundColor()
    {
        return Se.Settings.Appearance.DarkModeForegroundColor.FromHexToColor();
    }
}