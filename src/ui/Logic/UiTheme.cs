using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Controls.SyntaxTextEditorControl;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

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
    public const string ThemeNameNord = "Nord";
    public const string ThemeNameMidnightCinema = "MidnightCinema";
    public const string ThemeNameSakura = "Sakura";

    /// <summary>Theme art in a theme's image folder, for the empty video area.</summary>
    public const string VideoBackgroundFileName = "VideoBackground.jpg";

    /// <summary>Theme art in a theme's image folder, tiled across the empty waveform.</summary>
    public const string WaveformBackgroundFileName = "WaveformBackground.png";

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

    /// <summary>
    /// True for every dark theme - Dark, Nord and Midnight Cinema. The last two are fixed-palette
    /// variants of Dark: same Fluent dark variant and dark syntax colors, their own colors.
    /// </summary>
    public static bool IsDarkThemeEnabled()
    {
        var themeName = ThemeName;
        return themeName == ThemeNameDark || themeName == ThemeNameNord || themeName == ThemeNameMidnightCinema;
    }

    /// <summary>
    /// Themes that ship an image folder of their own in Themes.zip, with icons and background art.
    /// </summary>
    private static bool HasThemeFolder(string themeName)
    {
        return themeName == ThemeNameMidnightCinema || themeName == ThemeNameSakura;
    }

    /// <summary>
    /// True when the toolbar and message box images come from the theme's own folder (Midnight
    /// Cinema, Sakura) - those are drawn in the theme's colors and must not be recolored.
    /// </summary>
    public static bool HasOwnIcons
    {
        get
        {
            var themeName = ThemeName;
            return HasThemeFolder(themeName) &&
                   string.Equals(ImageFolder, Path.Combine(Se.ThemesFolder, themeName), StringComparison.Ordinal);
        }
    }

    /// <summary>
    /// Folder holding the current theme's images. These are unpacked from Themes.zip into
    /// <see cref="Se.ThemesFolder"/> at start-up - they are not embedded assets, so they cannot
    /// be reached through an avares:// URI. A theme that ships no image folder of its own
    /// (Pastel, for instance) falls back to Light - or Dark for a dark theme, as the Dark glyphs
    /// are near-white and vanish on a light background. An explicit icon theme wins over both.
    /// </summary>
    public static string ImageFolder
    {
        get
        {
            var folder = Path.Combine(Se.ThemesFolder, ThemeName);
            if (!Directory.Exists(folder) && HasThemeFolder(ThemeName))
            {
                // A theme added after the folder was last unpacked (same version, so the start-up
                // check saw nothing to do) - unpack Themes.zip again.
                Initializers.ThemeInitializer.TryRepair(folder);
            }

            if (!Directory.Exists(folder))
            {
                folder = Path.Combine(Se.ThemesFolder, IsDarkThemeEnabled() ? ThemeNameDark : ThemeNameLight);
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
        else if (themeSetting == ThemeNameNord)
        {
            Application.Current!.RequestedThemeVariant = ThemeVariant.Dark;
            ApplyLighterDark();
            ApplyNordAccent();
        }
        else if (themeSetting == ThemeNameMidnightCinema)
        {
            Application.Current!.RequestedThemeVariant = ThemeVariant.Dark;
            ApplyLighterDark();
            ApplyMidnightCinemaAccent();
        }
        else if (themeSetting == ThemeNameClassic)
        {
            Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
            ApplyWindowsClassicGray();
        }
        else if (themeSetting == ThemeNameSakura)
        {
            Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
            ApplySakura();
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

        // The editor palette follows the theme, the dark background color and the accent set above
        EditorChrome.InvalidatePalette();

        ApplyFieldStyle();
        ApplyMenuScaleStyle(Se.Settings.Appearance.LayoutScale);
        ApplyTextSelectionStyle();
        ApplyProgressBarContrastStyle();
        ApplyLayoutScaleToAllWindows();
        ApplyScaleToDenseMenus();
    }

    public static Action? SystemThemeChangedCallback { get; set; }

    private static void OnActualThemeVariantChanged(object? sender, EventArgs e)
    {
        if (Se.Settings.Appearance.Theme != ThemeNameSystem || Application.Current == null)
        {
            return;
        }

        // Follow the variant that just became active; do not re-enter SetCurrentTheme here.
        // That reset RequestedThemeVariant to Default, which is a no-op while the OS drives
        // the variant but silently undoes any explicit RequestedThemeVariant assignment made
        // while this handler is subscribed (the headless tests do that - the subscription
        // outlives the ApplySettings call that made it and turned an unrelated theme test
        // order-dependent).
        RemoveLighterDark();
        if (ThemeName == ThemeNameDark)
        {
            ApplyLighterDark();
        }

        EditorChrome.InvalidatePalette();
        ApplyFieldStyle();
        ApplyTextSelectionStyle();
        ApplyProgressBarContrastStyle();
        SystemThemeChangedCallback?.Invoke();
    }

    public const double ScaleStep = 0.1;
    public const double MinScale = 0.5;
    public const double MaxScale = 2.0;

    /// <summary>
    /// Font scale (#14812): scales text only, leaving layout metrics alone, unlike
    /// <see cref="ApplyScaleToWindow"/>'s layout transform. Applied through the inherited
    /// window <see cref="TemplatedControl.FontSize"/>, so controls that set an explicit
    /// FontSize keep it. Kept to a modest range as fixed-width controls clip at large values.
    /// </summary>
    public const double MinFontScale = 0.8;
    public const double MaxFontScale = 1.5;

    /// <summary>
    /// The text size of every window at 100 % font scale (#16276): the main window, the dialogs'
    /// editor look (<see cref="EditorChrome.FieldFontSize"/>) and the menus share it, so text
    /// reads the same size everywhere. Fluent's own default is 14.
    /// </summary>
    public const double BaseFontSize = 13.0;

    public static double FontScale => Math.Clamp(Se.Settings.Appearance.FontScale <= 0 ? 1.0 : Se.Settings.Appearance.FontScale, MinFontScale, MaxFontScale);

    /// <summary>Font size for popup menu/combo items rendered outside the layout transform.</summary>
    private static double PopupFontSize(double layoutFactor) => BaseFontSize * layoutFactor * FontScale;

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

        // Font scale rides on FontSize inheritance: every control without an explicit FontSize
        // picks it up from the window.
        window.FontSize = BaseFontSize * FontScale;

        // Explicit sizes are baked in at creation; long-lived controls that opted in via
        // DesignFontSize get theirs re-derived here.
        foreach (var control in window.GetVisualDescendants().OfType<Control>())
        {
            if (control.IsSet(UiUtil.DesignFontSizeProperty))
            {
                control.SetValue(TextElement.FontSizeProperty, UiUtil.ScaledFontSize(control.GetValue(UiUtil.DesignFontSizeProperty)));
            }
        }

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
        ApplyScaleToDenseMenus();
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
        menuItemStyle.Setters.Add(new Setter(TemplatedControl.FontSizeProperty, PopupFontSize(factor)));
        menuItemStyle.Setters.Add(new Setter(Layoutable.MinHeightProperty, 32.0 * factor));
        styles.Add(menuItemStyle);

        // Reset top-level menu bar items (inside the LayoutTransformControl, already scaled
        // by the transform). Only direct Menu children: submenu and context menu items are
        // logical descendants of the LTC too, but render in popups outside the transform.
        var ltcMenuItemStyle = new Style(x => x.OfType<Menu>().Child().OfType<MenuItem>());
        ltcMenuItemStyle.Setters.Add(new Setter(TemplatedControl.FontSizeProperty, BaseFontSize * FontScale));
        ltcMenuItemStyle.Setters.Add(new Setter(Layoutable.MinHeightProperty, 32.0));
        styles.Add(ltcMenuItemStyle);

        // Scale ComboBox dropdown items too — the dropdown popup renders outside the
        // LayoutTransformControl, so the window scale transform never reaches it (#13010).
        // ComboBoxItems only ever appear inside that popup, so no LTC reset counterpart is
        // needed. Skipped at 100% so windows with locally restyled combos keep their look.
        if (Math.Abs(factor - 1.0) > 0.0001 || Math.Abs(FontScale - 1.0) > 0.0001)
        {
            var comboBoxItemStyle = new Style(x => x.OfType<ComboBoxItem>());
            comboBoxItemStyle.Setters.Add(new Setter(TemplatedControl.FontSizeProperty, PopupFontSize(factor)));
            styles.Add(comboBoxItemStyle);
        }

        // Font scale must not grow icons (#14812): an Icon sizes its glyph from the inherited
        // FontSize, so the scaled window font would enlarge every icon button. A style beats
        // inheritance, so re-derive the icon size from the nearest ancestor, undoing the scale.
        // Icons with their own explicit FontSize keep it (local value beats style).
        if (Math.Abs(FontScale - 1.0) > 0.0001)
        {
            var iconStyle = new Style(x => x.OfType<Optris.Icons.Avalonia.Icon>());
            iconStyle.Setters.Add(new Setter(TemplatedControl.FontSizeProperty, new Binding(nameof(TemplatedControl.FontSize))
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor) { AncestorType = typeof(Control) },
                Converter = new IconFontSizeConverter(FontScale),
            }));
            styles.Add(iconStyle);
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

    /// <summary>
    /// Divides an icon's inherited font size by the font scale: every explicit size goes through
    /// <see cref="UiUtil.ScaledFontSize"/> and the window default is scaled too, so this lands
    /// icons back on their design-time size.
    /// </summary>
    private sealed class IconFontSizeConverter(double fontScale) : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            if (value is not double size)
            {
                return value;
            }

            return size / fontScale;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
            => throw new NotSupportedException();
    }

    private static Styles? _scrollBarStyle;
    private static Styles? _fieldStyle;
    private static Styles? _textSelectionStyle;
    private static Styles? _progressBarContrastStyle;

    /// <summary>
    /// Fluent paints the progress bar fill with the plain accent color on a translucent
    /// track. With a gray Windows accent such as "Storm" (#4C4A48) the fill matches the dark
    /// track almost exactly and the bar looks like it never moves (#15836). When the accent is
    /// too close to the track, the fill is blended toward white (dark) or black (light) until
    /// it stands out; accents that already contrast are left alone.
    /// </summary>
    public static void ApplyProgressBarContrastStyle()
    {
        if (Application.Current == null)
        {
            return;
        }

        if (_progressBarContrastStyle != null)
        {
            Application.Current.Styles.Remove(_progressBarContrastStyle);
            _progressBarContrastStyle = null;
        }

        var variant = Application.Current.ActualThemeVariant;
        if (!Application.Current.TryGetResource("SystemAccentColor", variant, out var value) || value is not Color accent)
        {
            return;
        }

        var isDark = variant == ThemeVariant.Dark;
        var fill = GetProgressBarFillColor(accent, isDark);
        if (fill == accent)
        {
            return;
        }

        _progressBarContrastStyle = new Styles
        {
            new Style(x => x.Is<ProgressBar>())
            {
                Setters =
                {
                    new Setter(ProgressBar.ForegroundProperty, new SolidColorBrush(fill)),
                }
            },
        };

        Application.Current.Styles.Add(_progressBarContrastStyle);
    }

    // The default Windows blue (#0078D4) is about 1.8:1 against the dark track, so only accents
    // below this are treated as invisible; those are then lifted to a clearly visible fill.
    public const double MinProgressBarContrast = 1.5;
    public const double AdjustedProgressBarContrast = 2.5;

    /// <summary>
    /// Returns <paramref name="accent"/> when it contrasts enough with the Fluent progress bar
    /// track, otherwise the accent blended toward white (dark theme) or black (light theme)
    /// just far enough to reach <see cref="AdjustedProgressBarContrast"/>.
    /// </summary>
    internal static Color GetProgressBarFillColor(Color accent, bool isDark)
    {
        // The Fluent track is SystemBaseLowColor (20% white/black) over the window background.
        var track = isDark ? Color.FromRgb(77, 77, 77) : Color.FromRgb(204, 204, 204);
        if (GetContrastRatio(accent, track) >= MinProgressBarContrast)
        {
            return accent;
        }

        var target = isDark ? Colors.White : Colors.Black;
        for (var step = 1; step <= 10; step++)
        {
            var color = Blend(accent, target, step / 10.0);
            if (GetContrastRatio(color, track) >= AdjustedProgressBarContrast)
            {
                return color;
            }
        }

        return target;
    }

    private static Color Blend(Color from, Color to, double amount)
    {
        return Color.FromRgb(
            (byte)Math.Round(from.R + (to.R - from.R) * amount),
            (byte)Math.Round(from.G + (to.G - from.G) * amount),
            (byte)Math.Round(from.B + (to.B - from.B) * amount));
    }

    internal static double GetContrastRatio(Color a, Color b)
    {
        var la = GetRelativeLuminance(a);
        var lb = GetRelativeLuminance(b);
        return (Math.Max(la, lb) + 0.05) / (Math.Min(la, lb) + 0.05);
    }

    private static double GetRelativeLuminance(Color c)
    {
        static double Channel(byte v)
        {
            var s = v / 255.0;
            return s <= 0.03928 ? s / 12.92 : Math.Pow((s + 0.055) / 1.055, 2.4);
        }

        return 0.2126 * Channel(c.R) + 0.7152 * Channel(c.G) + 0.0722 * Channel(c.B);
    }


    public const int MinTextSelectionOpacity = 10;

    /// <summary>
    /// Text box selection highlight opacity (#14744). Fluent paints the selection as a solid
    /// accent block with white text, which some find tiring to read. Below 100% the accent is
    /// made translucent and the selected text keeps its normal (or syntax) color instead of
    /// turning white, which would vanish on a light highlight. At 100% nothing is overridden.
    /// </summary>
    public static void ApplyTextSelectionStyle()
    {
        if (Application.Current == null)
        {
            return;
        }

        if (_textSelectionStyle != null)
        {
            Application.Current.Styles.Remove(_textSelectionStyle);
            _textSelectionStyle = null;
        }

        var opacity = Math.Clamp(Se.Settings.Appearance.TextSelectionOpacity, MinTextSelectionOpacity, 100);
        if (opacity >= 100)
        {
            return;
        }

        var accent = Application.Current.TryGetResource("SystemAccentColor", Application.Current.ActualThemeVariant, out var value) && value is Color c
            ? c
            : Colors.SteelBlue;
        var brush = new SolidColorBrush(accent, opacity / 100.0);

        _textSelectionStyle = new Styles
        {
            new Style(x => x.Is<TextBox>())
            {
                Setters =
                {
                    new Setter(TextBox.SelectionBrushProperty, brush),
                    new Setter(TextBox.SelectionForegroundBrushProperty, null),
                }
            },
        };

        Application.Current.Styles.Add(_textSelectionStyle);
    }
    /// <summary>
    /// Text boxes, combo boxes, number fields and lists (list boxes, table views, trees) in one
    /// field look all over the app: the editor field color, a subtle border and rounded corners.
    /// Not for the Classic, Pastel and Sakura themes, which have their own field colors. Added after the
    /// theme override styles so it wins over them - call it again after re-adding those.
    /// </summary>
    public static void ApplyFieldStyle()
    {
        if (Application.Current == null)
        {
            return;
        }

        if (_fieldStyle != null)
        {
            Application.Current.Styles.Remove(_fieldStyle);
            _fieldStyle = null;
        }

        var theme = Se.Settings.Appearance.Theme;
        if (theme == ThemeNameClassic || theme == ThemeNamePastel || theme == ThemeNameSakura)
        {
            return;
        }

        var cornerRadius = new CornerRadius(6);
        _fieldStyle = new Styles();
        foreach (var fieldType in new[] { typeof(TextBox), typeof(ComboBox), typeof(NumericUpDown) })
        {
            _fieldStyle.Add(new Style(x => x.Is(fieldType))
            {
                Setters =
                {
                    new Setter(TemplatedControl.BackgroundProperty, EditorChrome.FieldBrush),
                    new Setter(TemplatedControl.BorderBrushProperty, EditorChrome.FieldBorderBrush),
                    new Setter(TemplatedControl.CornerRadiusProperty, cornerRadius),
                },
            });
        }

        foreach (var listType in new[] { typeof(ListBox), typeof(TableView), typeof(TreeView) })
        {
            _fieldStyle.Add(new Style(x => x.Is(listType))
            {
                Setters =
                {
                    new Setter(TemplatedControl.BackgroundProperty, EditorChrome.FieldBrush),
                    new Setter(TemplatedControl.BorderBrushProperty, EditorChrome.FieldBorderBrush),
                    new Setter(TemplatedControl.BorderThicknessProperty, new Thickness(1)),
                    new Setter(TemplatedControl.CornerRadiusProperty, cornerRadius),
                },
            });
        }

        // The combo box arrow in the same blue-gray as the field border, light enough to read;
        // brighter while hovered. Disabled keeps the theme's dimmed arrow.
        _fieldStyle.Add(new Style(x => x.OfType<ComboBox>().Not(y => y.Class(":disabled")).Template().OfType<PathIcon>().Name("DropDownGlyph"))
        {
            Setters = { new Setter(TemplatedControl.ForegroundProperty, EditorChrome.FaintTextBrush) },
        });
        _fieldStyle.Add(new Style(x => x.OfType<ComboBox>().Class(":pointerover").Not(y => y.Class(":disabled")).Template().OfType<PathIcon>().Name("DropDownGlyph"))
        {
            Setters = { new Setter(TemplatedControl.ForegroundProperty, EditorChrome.IconBrush) },
        });

        // A text box keeps its own background while hovered/focused - the theme paints its own
        // hover/focus colors there
        foreach (var state in new[] { ":pointerover", ":focus" })
        {
            _fieldStyle.Add(new Style(x => x.Is<TextBox>().Class(state).Template().OfType<Border>().Name("PART_BorderElement"))
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

        // ...except the text box of an editable combo box: it covers the combo's border, so an
        // opaque fill would wipe that out (see ApplyLighterDark)
        _fieldStyle.Add(new Style(x => x.OfType<ComboBox>().Template().OfType<TextBox>())
        {
            Setters = { new Setter(TemplatedControl.BackgroundProperty, Brushes.Transparent) },
        });
        foreach (var state in new[] { ":pointerover", ":focus" })
        {
            _fieldStyle.Add(new Style(x => x.OfType<ComboBox>().Template().OfType<TextBox>().Class(state).Template().OfType<Border>().Name("PART_BorderElement"))
            {
                Setters = { new Setter(Border.BackgroundProperty, Brushes.Transparent) },
            });
        }

        Application.Current.Styles.Add(_fieldStyle);
    }

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
            // The TextBox template binds its inner ScrollViewer's AllowAutoHide to the attached
            // property on the TextBox itself, and that template binding beats the ScrollViewer
            // style above - so the text box needs the value too, or its scrollbar overlays the
            // text (#15033).
            new Style(x => x.Is<TextBox>())
            {
                Setters = { new Setter(ScrollViewer.AllowAutoHideProperty, allowAutoHide) }
            },
            // Same template binding in the ListBox template - without this the list's scrollbar
            // overlays right-aligned item content like the command palette shortcuts (#15920).
            new Style(x => x.Is<ListBox>())
            {
                Setters = { new Setter(ScrollViewer.AllowAutoHideProperty, allowAutoHide) }
            },
        };


        Application.Current.Styles.Add(_scrollBarStyle);
    }


    /// <summary>
    /// Menus with their own dense style (the main menu) override the application-wide menu
    /// item style, so they need the new scale baked in again.
    /// </summary>
    private static void ApplyScaleToDenseMenus()
    {
        foreach (var (menu, baseFontSize) in DenseMenuFontSizes.ToList())
        {
            ApplyDenseMenuStyle(menu, baseFontSize.Value);
        }
    }

    private static readonly ConditionalWeakTable<Menu, StrongBox<double>> DenseMenuFontSizes = new();
    private static readonly ConditionalWeakTable<Menu, Styles> DenseMenuStyles = new();

    /// <summary>
    /// Smaller font and tighter padding for a menu with many entries (the main menu). The style
    /// lives on the menu, so it beats the application-wide menu item style for every item below
    /// it - including submenu items, which are logical descendants but render in popups outside
    /// the window's LayoutTransformControl. Those therefore get the UI scale baked in here, or
    /// they ignored it at startup (PR #14818 comment). Re-applied on every UI scale change.
    /// </summary>
    public static void ApplyDenseMenuStyle(Menu menu, double baseFontSize)
    {
        if (DenseMenuStyles.TryGetValue(menu, out var previous))
        {
            menu.Styles.Remove(previous);
        }

        var factor = Se.Settings.Appearance.LayoutScale;
        var fontSize = baseFontSize * FontScale;
        menu.FontSize = fontSize;

        // Popup items first, so the top-level rule below wins for the menu bar items.
        var popupItemStyle = new Style(x => x.OfType<MenuItem>());
        popupItemStyle.Setters.Add(new Setter(TemplatedControl.FontSizeProperty, fontSize * factor));
        popupItemStyle.Setters.Add(new Setter(TemplatedControl.PaddingProperty, new Thickness(10, 1)));
        popupItemStyle.Setters.Add(new Setter(Layoutable.MinHeightProperty, 23.0 * factor));

        // Top-level items sit inside the LayoutTransformControl, already scaled by the transform.
        var topLevelItemStyle = new Style(x => x.OfType<Menu>().Child().OfType<MenuItem>());
        topLevelItemStyle.Setters.Add(new Setter(TemplatedControl.FontSizeProperty, fontSize));
        topLevelItemStyle.Setters.Add(new Setter(Layoutable.MinHeightProperty, 23.0));

        var styles = new Styles { popupItemStyle, topLevelItemStyle };
        DenseMenuStyles.AddOrUpdate(menu, styles);
        DenseMenuFontSizes.AddOrUpdate(menu, new StrongBox<double>(baseFontSize));
        menu.Styles.Add(styles);
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
            // Window background. The Fluent palette's RegionColor (UpdateRegionColor) only
            // reaches windows when the theme variant changes, so switching between two dark
            // themes - Dark and Nord - left the old background behind until a restart.
            new Style(x => x.OfType<Window>())
            {
                Setters =
                {
                    new Setter(Window.BackgroundProperty, new SolidColorBrush(bgColor))
                }
            },

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

    /// <summary>
    /// Nord's frost blues for everything Fluent derives from the accent (selection, focus
    /// rings, check boxes, sliders, progress bars), and its polar-night shades for buttons and
    /// headers. Runs after <see cref="ApplyLighterDark"/>, which already set the Nord window,
    /// input and text colors through <see cref="GetDarkThemeBackgroundColor"/> - this adds to
    /// its resource dictionary and style collection so RemoveLighterDark removes both.
    /// </summary>
    private static void ApplyNordAccent()
    {
        if (Application.Current == null || _resourceOverrides == null || _themeOverrideStyle is not Styles styles)
        {
            return;
        }

        var button = Color.FromRgb(0x43, 0x4C, 0x5E); // nord2
        var buttonHover = Color.FromRgb(0x4C, 0x56, 0x6A); // nord3
        var buttonPressed = Color.FromRgb(0x56, 0x61, 0x77);
        var border = Color.FromRgb(0x4C, 0x56, 0x6A); // nord3
        var header = Color.FromRgb(0x3B, 0x42, 0x52); // nord1
        var frost = Color.FromRgb(0x88, 0xC0, 0xD0); // nord8

        // The dark Fluent variant fills accent controls with the Light1-3 shades and puts
        // dark text on them, so the brighter frost blues go there.
        _resourceOverrides["SystemAccentColor"] = Color.FromRgb(0x5E, 0x81, 0xAC); // nord10
        _resourceOverrides["SystemAccentColorLight1"] = Color.FromRgb(0x81, 0xA1, 0xC1); // nord9
        _resourceOverrides["SystemAccentColorLight2"] = frost;
        _resourceOverrides["SystemAccentColorLight3"] = Color.FromRgb(0x8F, 0xBC, 0xBB); // nord7
        _resourceOverrides["SystemAccentColorDark1"] = Color.FromRgb(0x4F, 0x6F, 0x96);
        _resourceOverrides["SystemAccentColorDark2"] = Color.FromRgb(0x42, 0x5E, 0x80);
        _resourceOverrides["SystemAccentColorDark3"] = Color.FromRgb(0x35, 0x4C, 0x6A);

        // Hover/pressed are template-level in Fluent, so a Background setter alone would
        // leave them Fluent gray.
        _resourceOverrides["ButtonBackground"] = new SolidColorBrush(button);
        _resourceOverrides["ButtonBackgroundPointerOver"] = new SolidColorBrush(buttonHover);
        _resourceOverrides["ButtonBackgroundPressed"] = new SolidColorBrush(buttonPressed);
        _resourceOverrides["ButtonBorderBrushPointerOver"] = new SolidColorBrush(frost);
        _resourceOverrides["ToggleButtonBackground"] = new SolidColorBrush(button);
        _resourceOverrides["ToggleButtonBackgroundPointerOver"] = new SolidColorBrush(buttonHover);
        _resourceOverrides["ToggleButtonBackgroundPressed"] = new SolidColorBrush(buttonPressed);

        styles.Add(new Style(x => x.OfType<TableViewColumnHeader>())
        {
            Setters =
            {
                new Setter(TableViewColumnHeader.BackgroundProperty, new SolidColorBrush(header)),
            }
        });
        styles.Add(new Style(x => x.OfType<ContextMenu>())
        {
            Setters =
            {
                new Setter(TemplatedControl.BackgroundProperty, new SolidColorBrush(header)),
                new Setter(TemplatedControl.BorderBrushProperty, new SolidColorBrush(border)),
            }
        });
        styles.Add(new Style(x => x.OfType<FlyoutPresenter>())
        {
            Setters =
            {
                new Setter(TemplatedControl.BackgroundProperty, new SolidColorBrush(header)),
                new Setter(TemplatedControl.BorderBrushProperty, new SolidColorBrush(border)),
            }
        });
    }

    /// <summary>
    /// Midnight Cinema: projector amber for everything Fluent derives from the accent, warm
    /// plum-black shades for buttons, headers and menus. Like <see cref="ApplyNordAccent"/> it
    /// runs after <see cref="ApplyLighterDark"/>, which already set the window, input and text
    /// colors through <see cref="GetDarkThemeBackgroundColor"/>.
    /// </summary>
    private static void ApplyMidnightCinemaAccent()
    {
        if (Application.Current == null || _resourceOverrides == null || _themeOverrideStyle is not Styles styles)
        {
            return;
        }

        var button = Color.FromRgb(0x2A, 0x26, 0x31);
        var buttonHover = Color.FromRgb(0x35, 0x30, 0x40);
        var buttonPressed = Color.FromRgb(0x40, 0x3A, 0x4C);
        var border = Color.FromRgb(0x3A, 0x34, 0x42);
        var amber = Color.FromRgb(0xE8, 0xA3, 0x3D);

        // The dark Fluent variant fills accent controls with the Light1-3 shades and puts
        // dark text on them.
        _resourceOverrides["SystemAccentColor"] = amber;
        _resourceOverrides["SystemAccentColorLight1"] = Color.FromRgb(0xED, 0xB4, 0x5E);
        _resourceOverrides["SystemAccentColorLight2"] = Color.FromRgb(0xF2, 0xC4, 0x7F);
        _resourceOverrides["SystemAccentColorLight3"] = Color.FromRgb(0xF7, 0xD6, 0xA3);
        _resourceOverrides["SystemAccentColorDark1"] = Color.FromRgb(0xC9, 0x8A, 0x2E);
        _resourceOverrides["SystemAccentColorDark2"] = Color.FromRgb(0xA8, 0x71, 0x24);
        _resourceOverrides["SystemAccentColorDark3"] = Color.FromRgb(0x87, 0x59, 0x1B);

        _resourceOverrides["ButtonBackground"] = new SolidColorBrush(button);
        _resourceOverrides["ButtonBackgroundPointerOver"] = new SolidColorBrush(buttonHover);
        _resourceOverrides["ButtonBackgroundPressed"] = new SolidColorBrush(buttonPressed);
        _resourceOverrides["ButtonBorderBrushPointerOver"] = new SolidColorBrush(amber);
        _resourceOverrides["ToggleButtonBackground"] = new SolidColorBrush(button);
        _resourceOverrides["ToggleButtonBackgroundPointerOver"] = new SolidColorBrush(buttonHover);
        _resourceOverrides["ToggleButtonBackgroundPressed"] = new SolidColorBrush(buttonPressed);

        styles.Add(new Style(x => x.OfType<TableViewColumnHeader>())
        {
            Setters =
            {
                new Setter(TableViewColumnHeader.BackgroundProperty, new SolidColorBrush(MidnightCinemaHeaderColor)),
            }
        });
        styles.Add(new Style(x => x.OfType<ContextMenu>())
        {
            Setters =
            {
                new Setter(TemplatedControl.BackgroundProperty, new SolidColorBrush(MidnightCinemaHeaderColor)),
                new Setter(TemplatedControl.BorderBrushProperty, new SolidColorBrush(border)),
            }
        });
        styles.Add(new Style(x => x.OfType<FlyoutPresenter>())
        {
            Setters =
            {
                new Setter(TemplatedControl.BackgroundProperty, new SolidColorBrush(MidnightCinemaHeaderColor)),
                new Setter(TemplatedControl.BorderBrushProperty, new SolidColorBrush(border)),
            }
        });
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

        // One lavender family for the chrome, with mint and sky as the secondary pastels and a
        // violet accent. Keep bgColor in sync with PastelBackgroundColor.
        var bgColor = PastelBackgroundColor; // Soft lavender
        var surface = Color.FromRgb(251, 249, 255); // Lilac-tinted white for text input
        var lilac = Color.FromRgb(227, 216, 250); // Buttons
        var lilacHover = Color.FromRgb(214, 199, 247);
        var lilacPressed = Color.FromRgb(198, 180, 241);
        var sky = Color.FromRgb(228, 241, 255); // Combo boxes
        var mint = Color.FromRgb(227, 246, 236); // Numeric / time code spinners
        var header = Color.FromRgb(232, 222, 252); // Grid header
        var borderColor = Color.FromRgb(196, 180, 232); // Lilac border
        var accent = Color.FromRgb(142, 111, 216); // Violet

        // Fluent derives selection, focus rings, check boxes, sliders, toggle buttons and
        // progress bars from the accent colors - without these they stay Windows blue.
        _resourceOverrides = new ResourceDictionary
        {
            ["SystemAccentColor"] = accent,
            ["SystemAccentColorLight1"] = Color.FromRgb(164, 138, 226),
            ["SystemAccentColorLight2"] = Color.FromRgb(189, 169, 236),
            ["SystemAccentColorLight3"] = Color.FromRgb(214, 200, 245),
            ["SystemAccentColorDark1"] = Color.FromRgb(122, 92, 196),
            ["SystemAccentColorDark2"] = Color.FromRgb(102, 74, 174),
            ["SystemAccentColorDark3"] = Color.FromRgb(82, 58, 150),

            // Hover/pressed are template-level in Fluent, so a Background setter alone would
            // leave them gray.
            ["ButtonBackgroundPointerOver"] = new SolidColorBrush(lilacHover),
            ["ButtonBackgroundPressed"] = new SolidColorBrush(lilacPressed),
            ["ButtonBorderBrushPointerOver"] = new SolidColorBrush(accent),
            ["ButtonBorderBrushPressed"] = new SolidColorBrush(accent),
            ["ToggleButtonBackground"] = new SolidColorBrush(lilac),
            ["ToggleButtonBackgroundPointerOver"] = new SolidColorBrush(lilacHover),
            ["ToggleButtonBackgroundPressed"] = new SolidColorBrush(lilacPressed),
            ["ToggleButtonBorderBrush"] = new SolidColorBrush(borderColor),

            ["ControlCornerRadius"] = new CornerRadius(8),
            ["OverlayCornerRadius"] = new CornerRadius(10),
        };
        Application.Current.Resources.MergedDictionaries.Add(_resourceOverrides);

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

            new Style(x => x.OfType<TextBox>())
            {
                Setters =
                {
                    new Setter(TextBox.BackgroundProperty, new SolidColorBrush(surface)),
                    new Setter(TextBox.BorderBrushProperty, new SolidColorBrush(borderColor)),
                    new Setter(TextBox.BorderThicknessProperty, new Thickness(1))
                }
            },

            new Style(x => x.OfType<Button>())
            {
                Setters =
                {
                    new Setter(Button.BackgroundProperty, new SolidColorBrush(lilac)),
                    new Setter(Button.BorderBrushProperty, new SolidColorBrush(borderColor))
                }
            },

            new Style(x => x.OfType<NumericUpDown>())
            {
                Setters =
                {
                    new Setter(NumericUpDown.BackgroundProperty, new SolidColorBrush(mint)),
                    new Setter(NumericUpDown.BorderBrushProperty, new SolidColorBrush(borderColor))
                }
            },

            new Style(x => x.OfType<ComboBox>())
            {
                Setters =
                {
                    new Setter(ComboBox.BackgroundProperty, new SolidColorBrush(sky)),
                    new Setter(ComboBox.BorderBrushProperty, new SolidColorBrush(borderColor))
                }
            },

            // Menus and flyouts pop up over the lavender window - plain white looks foreign
            new Style(x => x.OfType<ContextMenu>())
            {
                Setters =
                {
                    new Setter(TemplatedControl.BackgroundProperty, new SolidColorBrush(surface)),
                    new Setter(TemplatedControl.BorderBrushProperty, new SolidColorBrush(borderColor))
                }
            },
            new Style(x => x.OfType<FlyoutPresenter>())
            {
                Setters =
                {
                    new Setter(TemplatedControl.BackgroundProperty, new SolidColorBrush(surface)),
                    new Setter(TemplatedControl.BorderBrushProperty, new SolidColorBrush(borderColor))
                }
            },

            // TableView header
            new Style(x => x.OfType<TableViewColumnHeader>())
            {
                Setters =
                {
                    new Setter(TableViewColumnHeader.BackgroundProperty, new SolidColorBrush(header))
                }
            },

            // ButtonSpinner (used by TimeCodeUpDown)
            new Style(x => x.OfType<ButtonSpinner>())
            {
                Setters =
                {
                    new Setter(ButtonSpinner.BackgroundProperty, new SolidColorBrush(mint)),
                    new Setter(ButtonSpinner.BorderBrushProperty, new SolidColorBrush(borderColor))
                }
            },

            // SecondsUpDown - mint by default (external bindings will override when needed)
            new Style(x => x.OfType<Nikse.SubtitleEdit.Controls.SecondsUpDown>())
            {
                Setters =
                {
                    new Setter(Nikse.SubtitleEdit.Controls.SecondsUpDown.BackgroundProperty, new SolidColorBrush(mint))
                }
            },

            // TimeCodeUpDown - mint by default
            new Style(x => x.OfType<Nikse.SubtitleEdit.Controls.TimeCodeUpDown>())
            {
                Setters =
                {
                    new Setter(Nikse.SubtitleEdit.Controls.TimeCodeUpDown.BackgroundProperty, new SolidColorBrush(mint))
                }
            },

            // The source editor
            new Style(x => x.OfType<SyntaxTextEditor>())
            {
                Setters =
                {
                    new Setter(SyntaxTextEditor.BackgroundProperty, new SolidColorBrush(surface))
                }
            },
        };

        Application.Current.Styles.Add(_themeOverrideStyle);
    }

    /// <summary>
    /// Sakura: cherry blossom pinks on a blush window, plum text and a cherry accent. Built like
    /// <see cref="ApplyPastel"/>. Keep bgColor in sync with SakuraBackgroundColor.
    /// </summary>
    private static void ApplySakura()
    {
        if (Application.Current == null)
        {
            return;
        }

        var bgColor = SakuraBackgroundColor;
        var surface = Color.FromRgb(255, 252, 253); // Text input
        var blush = Color.FromRgb(250, 226, 234); // Buttons
        var blushHover = Color.FromRgb(246, 212, 224);
        var blushPressed = Color.FromRgb(240, 196, 212);
        var field = Color.FromRgb(253, 241, 245); // Combo boxes and spinners
        var header = Color.FromRgb(248, 228, 235); // Grid header
        var borderColor = Color.FromRgb(236, 196, 210);
        var accent = SakuraAccentColor;
        var text = SakuraForegroundColor;
        var textBrush = new SolidColorBrush(text);

        _resourceOverrides = new ResourceDictionary
        {
            ["SystemAccentColor"] = accent,
            ["SystemAccentColorLight1"] = Color.FromRgb(226, 104, 138),
            ["SystemAccentColorLight2"] = Color.FromRgb(235, 140, 166),
            ["SystemAccentColorLight3"] = Color.FromRgb(244, 180, 198),
            ["SystemAccentColorDark1"] = Color.FromRgb(194, 56, 95),
            ["SystemAccentColorDark2"] = Color.FromRgb(168, 44, 80),
            ["SystemAccentColorDark3"] = Color.FromRgb(140, 34, 66),

            // Hover/pressed are template-level in Fluent, so a Background setter alone would
            // leave them gray.
            ["ButtonBackgroundPointerOver"] = new SolidColorBrush(blushHover),
            ["ButtonBackgroundPressed"] = new SolidColorBrush(blushPressed),
            ["ButtonBorderBrushPointerOver"] = new SolidColorBrush(accent),
            ["ButtonBorderBrushPressed"] = new SolidColorBrush(accent),
            ["ButtonForegroundPointerOver"] = textBrush,
            ["ButtonForegroundPressed"] = textBrush,
            ["ToggleButtonBackground"] = new SolidColorBrush(blush),
            ["ToggleButtonBackgroundPointerOver"] = new SolidColorBrush(blushHover),
            ["ToggleButtonBackgroundPressed"] = new SolidColorBrush(blushPressed),
            ["ToggleButtonBorderBrush"] = new SolidColorBrush(borderColor),

            ["TextControlForeground"] = textBrush,
            ["TextControlForegroundPointerOver"] = textBrush,
            ["TextControlForegroundFocused"] = textBrush,

            ["ControlCornerRadius"] = new CornerRadius(8),
            ["OverlayCornerRadius"] = new CornerRadius(10),
        };
        Application.Current.Resources.MergedDictionaries.Add(_resourceOverrides);

        _themeOverrideStyle = new Styles
        {
            new Style(x => x.OfType<Window>())
            {
                Setters =
                {
                    new Setter(Window.BackgroundProperty, new SolidColorBrush(bgColor)),
                    new Setter(Window.ForegroundProperty, textBrush),
                }
            },

            // Plum text instead of black - set on the text elements too, as the Fluent
            // templates give many of them their own foreground
            new Style(x => x.OfType<TextBlock>())
            {
                Setters = { new Setter(TextBlock.ForegroundProperty, textBrush) }
            },
            new Style(x => x.OfType<Label>())
            {
                Setters = { new Setter(Label.ForegroundProperty, textBrush) }
            },
            new Style(x => x.OfType<Avalonia.Controls.MenuItem>())
            {
                Setters = { new Setter(Avalonia.Controls.MenuItem.ForegroundProperty, textBrush) }
            },
            new Style(x => x.OfType<Optris.Icons.Avalonia.Icon>())
            {
                Setters = { new Setter(Optris.Icons.Avalonia.Icon.ForegroundProperty, new SolidColorBrush(Color.FromRgb(107, 78, 91))) }
            },

            // Icons on a colored accent square keep their white glyph (#12717)
            new Style(x => x.OfType<ContentControl>().Class(IconOnAccentClassName)
                    .Descendant().OfType<Optris.Icons.Avalonia.Icon>())
            {
                Setters = { new Setter(Optris.Icons.Avalonia.Icon.ForegroundProperty, Brushes.White) }
            },

            new Style(x => x.OfType<TextBox>())
            {
                Setters =
                {
                    new Setter(TextBox.BackgroundProperty, new SolidColorBrush(surface)),
                    new Setter(TextBox.BorderBrushProperty, new SolidColorBrush(borderColor)),
                    new Setter(TextBox.BorderThicknessProperty, new Thickness(1))
                }
            },

            new Style(x => x.OfType<Button>())
            {
                Setters =
                {
                    new Setter(Button.BackgroundProperty, new SolidColorBrush(blush)),
                    new Setter(Button.BorderBrushProperty, new SolidColorBrush(borderColor))
                }
            },

            new Style(x => x.OfType<NumericUpDown>())
            {
                Setters =
                {
                    new Setter(NumericUpDown.BackgroundProperty, new SolidColorBrush(field)),
                    new Setter(NumericUpDown.BorderBrushProperty, new SolidColorBrush(borderColor))
                }
            },

            new Style(x => x.OfType<ComboBox>())
            {
                Setters =
                {
                    new Setter(ComboBox.BackgroundProperty, new SolidColorBrush(field)),
                    new Setter(ComboBox.BorderBrushProperty, new SolidColorBrush(borderColor))
                }
            },

            // Menus and flyouts pop up over the blush window - plain white looks foreign
            new Style(x => x.OfType<ContextMenu>())
            {
                Setters =
                {
                    new Setter(TemplatedControl.BackgroundProperty, new SolidColorBrush(surface)),
                    new Setter(TemplatedControl.BorderBrushProperty, new SolidColorBrush(borderColor))
                }
            },
            new Style(x => x.OfType<FlyoutPresenter>())
            {
                Setters =
                {
                    new Setter(TemplatedControl.BackgroundProperty, new SolidColorBrush(surface)),
                    new Setter(TemplatedControl.BorderBrushProperty, new SolidColorBrush(borderColor))
                }
            },

            new Style(x => x.OfType<TableViewColumnHeader>())
            {
                Setters =
                {
                    new Setter(TableViewColumnHeader.BackgroundProperty, new SolidColorBrush(header)),
                    new Setter(TableViewColumnHeader.ForegroundProperty, textBrush),
                }
            },

            new Style(x => x.OfType<ButtonSpinner>())
            {
                Setters =
                {
                    new Setter(ButtonSpinner.BackgroundProperty, new SolidColorBrush(field)),
                    new Setter(ButtonSpinner.BorderBrushProperty, new SolidColorBrush(borderColor))
                }
            },

            new Style(x => x.OfType<Nikse.SubtitleEdit.Controls.SecondsUpDown>())
            {
                Setters =
                {
                    new Setter(Nikse.SubtitleEdit.Controls.SecondsUpDown.BackgroundProperty, new SolidColorBrush(field))
                }
            },

            new Style(x => x.OfType<Nikse.SubtitleEdit.Controls.TimeCodeUpDown>())
            {
                Setters =
                {
                    new Setter(Nikse.SubtitleEdit.Controls.TimeCodeUpDown.BackgroundProperty, new SolidColorBrush(field))
                }
            },

            new Style(x => x.OfType<SyntaxTextEditor>())
            {
                Setters =
                {
                    new Setter(SyntaxTextEditor.BackgroundProperty, new SolidColorBrush(surface))
                }
            },
        };

        Application.Current.Styles.Add(_themeOverrideStyle);
    }

    /// <summary>
    /// Nord and Midnight Cinema ignore the user's dark-mode colors: their palette is the point
    /// of picking them.
    /// </summary>
    internal static Color GetDarkThemeBackgroundColor()
    {
        var theme = Se.Settings.Appearance.Theme;
        if (theme == ThemeNameNord)
        {
            return NordBackgroundColor;
        }

        if (theme == ThemeNameMidnightCinema)
        {
            return MidnightCinemaBackgroundColor;
        }

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

        return GetLightThemeBackgroundColor(Se.Settings.Appearance.Theme);
    }

    /// <summary>
    /// The window background of a light theme by its setting name: Classic, Pastel and Sakura
    /// have their own, every other light theme is white.
    /// </summary>
    internal static Color GetLightThemeBackgroundColor(string? themeSetting)
    {
        if (themeSetting == ThemeNameClassic)
        {
            return ClassicBackgroundColor;
        }

        if (themeSetting == ThemeNameSakura)
        {
            return SakuraBackgroundColor;
        }

        return themeSetting == ThemeNamePastel ? PastelBackgroundColor : Colors.White;
    }

    private static readonly Color ClassicBackgroundColor = Color.FromRgb(236, 233, 216);
    private static readonly Color PastelBackgroundColor = Color.FromRgb(240, 235, 255);
    internal static readonly Color NordBackgroundColor = Color.FromRgb(0x2E, 0x34, 0x40); // nord0
    internal static readonly Color NordForegroundColor = Color.FromRgb(0xE5, 0xE9, 0xF0); // nord5
    private static readonly Color NordAlternatingRowColor = Color.FromRgb(0x34, 0x3B, 0x48);
    internal static readonly Color MidnightCinemaBackgroundColor = Color.FromRgb(0x14, 0x12, 0x18);
    internal static readonly Color MidnightCinemaForegroundColor = Color.FromRgb(0xEC, 0xE6, 0xD9);
    internal static readonly Color MidnightCinemaHeaderColor = Color.FromRgb(0x1E, 0x1B, 0x24);
    internal static readonly Color MidnightCinemaAccentColor = Color.FromRgb(0xE8, 0xA3, 0x3D);
    private static readonly Color MidnightCinemaAlternatingRowColor = Color.FromRgb(0x1A, 0x17, 0x20);
    internal static readonly Color SakuraBackgroundColor = Color.FromRgb(0xFF, 0xF7, 0xF9);
    internal static readonly Color SakuraForegroundColor = Color.FromRgb(0x3A, 0x2A, 0x31);
    internal static readonly Color SakuraAccentColor = Color.FromRgb(0xD9, 0x46, 0x6F);
    private static readonly Color SakuraAlternatingRowColor = Color.FromRgb(0xFD, 0xEF, 0xF3);

    /// <summary>
    /// The subtitle grid's alternating row color. The fixed-palette themes have their own - the
    /// gray settings read as stray gray stripes on their tinted rows.
    /// </summary>
    public static string GetGridAlternatingRowColorHex(bool isDark)
    {
        var theme = Se.Settings.Appearance.Theme;
        if (!isDark)
        {
            return theme == ThemeNameSakura
                ? SakuraAlternatingRowColor.FromColorToHex()
                : Se.Settings.Appearance.GridAlternatingRowColor;
        }

        if (theme == ThemeNameNord)
        {
            return NordAlternatingRowColor.FromColorToHex();
        }

        return theme == ThemeNameMidnightCinema
            ? MidnightCinemaAlternatingRowColor.FromColorToHex()
            : Se.Settings.Appearance.GridAlternatingRowColorDark;
    }

    private static readonly WaveformColors MidnightCinemaWaveformColors = new(
        TextHex: MidnightCinemaForegroundColor.FromColorToHex(),
        Waveform: Color.FromArgb(230, 0xE8, 0xA3, 0x3D),
        Background: Color.FromRgb(0x0F, 0x0D, 0x12),
        Selected: Color.FromArgb(110, 0xE8, 0xA3, 0x3D),
        Cursor: Color.FromRgb(0xFF, 0x6B, 0x4A),
        ShotChange: Color.FromArgb(200, 0xEC, 0xE6, 0xD9),
        Grid: Color.FromArgb(55, 0xE8, 0xA3, 0x3D),
        ParagraphLeft: Color.FromArgb(120, 0xE8, 0xA3, 0x3D),
        ParagraphRight: Color.FromArgb(120, 0xFF, 0x6B, 0x4A),
        FancyHigh: Color.FromRgb(0xFF, 0xD2, 0x7A),
        ParagraphBackground: Color.FromArgb(130, 0x3A, 0x32, 0x42),
        ParagraphSelectedBackground: Color.FromArgb(150, 0x6E, 0x50, 0x26));

    private static readonly WaveformColors SakuraWaveformColors = new(
        TextHex: SakuraForegroundColor.FromColorToHex(),
        Waveform: Color.FromRgb(0xC2, 0x5A, 0x7C),
        Background: Color.FromRgb(0xFF, 0xF1, 0xF5),
        Selected: Color.FromArgb(80, 0xD9, 0x46, 0x6F),
        Cursor: Color.FromRgb(0x6B, 0x4B, 0xC2),
        ShotChange: Color.FromArgb(170, 0x6B, 0x4E, 0x5B),
        Grid: Color.FromArgb(45, 0xC2, 0x5A, 0x7C),
        ParagraphLeft: Color.FromArgb(130, 0xD9, 0x46, 0x6F),
        ParagraphRight: Color.FromArgb(130, 0x6B, 0x4B, 0xC2),
        FancyHigh: Color.FromRgb(0xD9, 0x46, 0x6F),
        ParagraphBackground: Color.FromArgb(70, 0xE8, 0xB4, 0xC6),
        ParagraphSelectedBackground: Color.FromArgb(110, 0xF0, 0x9C, 0xB6));

    /// <summary>
    /// The waveform colors of a theme with a palette of its own, or null to use the settings.
    /// </summary>
    internal static WaveformColors? GetThemeWaveformColors()
    {
        var theme = Se.Settings.Appearance.Theme;
        if (theme == ThemeNameMidnightCinema)
        {
            return MidnightCinemaWaveformColors;
        }

        return theme == ThemeNameSakura ? SakuraWaveformColors : null;
    }

    private static readonly System.Collections.Generic.Dictionary<string, Bitmap?> ThemeArtCache = new(StringComparer.Ordinal);

    /// <summary>
    /// A background image shipped in the current theme's folder (see
    /// <see cref="VideoBackgroundFileName"/>), or null when the theme has none.
    /// </summary>
    public static Bitmap? GetThemeArt(string fileName)
    {
        var theme = Se.Settings.Appearance.Theme;
        if (!HasThemeFolder(theme))
        {
            return null;
        }

        var path = Path.Combine(Se.ThemesFolder, theme, fileName);
        if (ThemeArtCache.TryGetValue(path, out var cached))
        {
            return cached;
        }

        if (!File.Exists(path) && !Initializers.ThemeInitializer.TryRepair(path))
        {
            return null;
        }

        try
        {
            var bitmap = new Bitmap(path);
            ThemeArtCache[path] = bitmap;
            return bitmap;
        }
        catch (Exception e)
        {
            Se.LogError(e, $"Could not load theme image \"{path}\"");
            ThemeArtCache[path] = null;
            return null;
        }
    }

    public static Color GetDarkThemeForegroundColor()
    {
        var theme = Se.Settings.Appearance.Theme;
        if (theme == ThemeNameNord)
        {
            return NordForegroundColor;
        }

        if (theme == ThemeNameMidnightCinema)
        {
            return MidnightCinemaForegroundColor;
        }

        return Se.Settings.Appearance.DarkModeForegroundColor.FromHexToColor();
    }
}