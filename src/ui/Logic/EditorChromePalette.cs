using System;
using Avalonia;
using Avalonia.Media;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Logic;

internal static partial class EditorChrome
{
    /// <summary>
    /// The accent when the theme has none to offer (no Application, e.g. in a view model test).
    /// </summary>
    internal static Color FallbackAccent => EditorPalette.FallbackAccent;

    /// <summary>
    /// The dark base when the setting is missing or malformed - the default dark background.
    /// </summary>
    internal static Color DefaultDarkBase => EditorPalette.DefaultDarkBase;

    // Static initializer order across the partial files is unspecified, so a ThemedBrush built
    // while this field is still null starts from EditorPalette.Default (the nested class has its
    // own initializer).
    private static EditorPalette? _palette;
    private static (bool IsDark, string Theme, string DarkBackground) _paletteKey;
    private static bool _paletteDirty = true;

    /// <summary>
    /// The editor palette for the current theme and appearance settings (#16233): the dark
    /// version is built from the user's dark background color, the light one from the Light,
    /// Classic or Pastel window color, and the accent is the theme's system accent, so the
    /// redesigned windows match the main window and the grid selection instead of carrying a
    /// palette of their own. Off the UI thread the palette is whatever was last computed.
    /// </summary>
    internal static EditorPalette Palette
    {
        get
        {
            if (Dispatcher.UIThread.CheckAccess())
            {
                EnsurePalette();
            }

            return _palette ?? EditorPalette.Default;
        }
    }

    /// <summary>
    /// The current accent as a color, for drawing code that paints with Skia/DrawingContext
    /// rather than a brush (the ASSA draw canvas).
    /// </summary>
    internal static Color AccentColorValue => Palette.Accent;

    /// <summary>
    /// Recompute the palette from the settings and repaint every palette brush. Called when a
    /// theme is applied (also from Settings OK/Apply) and when the OS switches light/dark; a
    /// brush read also picks up a changed theme or dark background setting by itself.
    /// </summary>
    internal static void InvalidatePalette()
    {
        _paletteDirty = true;
        if (Dispatcher.UIThread.CheckAccess())
        {
            ThemedBrush.RefreshAll();
        }
    }

    private static void EnsurePalette()
    {
        var isDark = !IsLightTheme;
        var appearance = Se.Settings?.Appearance;
        var key = (isDark, appearance?.Theme ?? string.Empty, appearance?.DarkModeBackgroundColor ?? string.Empty);
        if (!_paletteDirty && key == _paletteKey)
        {
            return;
        }

        _paletteKey = key;
        _paletteDirty = false;
        _palette = EditorPalette.Derive(isDark, GetBaseColor(isDark, appearance), GetAccentColor());
    }

    private static Color GetBaseColor(bool isDark, SeAppearance? appearance)
    {
        if (isDark)
        {
            if (appearance?.Theme == UiTheme.ThemeNameNord)
            {
                return UiTheme.NordBackgroundColor;
            }

            return appearance != null && Color.TryParse(appearance.DarkModeBackgroundColor, out var dark)
                ? Color.FromRgb(dark.R, dark.G, dark.B)
                : DefaultDarkBase;
        }

        return UiTheme.GetLightThemeBackgroundColor(appearance?.Theme);
    }

    private static Color GetAccentColor()
    {
        var application = Application.Current;
        if (application != null &&
            application.TryGetResource("SystemAccentColor", application.ActualThemeVariant, out var value) &&
            value is Color accent && accent.A > 0)
        {
            return Color.FromRgb(accent.R, accent.G, accent.B);
        }

        return FallbackAccent;
    }

    /// <summary>
    /// One set of editor colors, derived from a base (window) color and an accent. Surfaces are
    /// small per-channel steps from the base, so a tinted base keeps its tint; text and icon
    /// grays are neutral, offset from the base lightness so contrast holds for any base.
    /// </summary>
    internal sealed class EditorPalette
    {
        public static readonly Color FallbackAccent = Color.FromRgb(76, 110, 245);
        public static readonly Color DefaultDarkBase = Color.FromRgb(33, 33, 33);

        /// <summary>The palette before any theme is known: default dark, fallback accent.</summary>
        public static EditorPalette Default { get; } = Derive(isDark: true, DefaultDarkBase, FallbackAccent);

        public bool IsDark { get; private init; }
        public Color Base { get; private init; }
        public Color Accent { get; private init; }

        public Color Window { get; private init; }
        public Color Panel { get; private init; }
        public Color Status { get; private init; }
        public Color ButtonBar { get; private init; }
        public Color Line { get; private init; }
        public Color Separator { get; private init; }
        public Color Hover { get; private init; }
        public Color Pressed { get; private init; }
        public Color Field { get; private init; }
        public Color FieldBorder { get; private init; }
        public Color Menu { get; private init; }
        public Color MenuBorder { get; private init; }

        public Color StrongText { get; private init; }
        public Color DimText { get; private init; }
        public Color FaintText { get; private init; }
        public Color Icon { get; private init; }
        public Color Thumbnail { get; private init; }
        public Color ThumbnailMuted { get; private init; }

        public Color AccentHover { get; private init; }
        public Color AccentSoft { get; private init; }
        public Color BadgeText { get; private init; }

        /// <summary>
        /// Text and icons on an accent surface: white unless the accent is so light that black
        /// reads better (a yellow or pastel OS accent).
        /// </summary>
        public Color OnAccent { get; private init; }

        public static EditorPalette Derive(bool isDark, Color baseColor, Color accent)
        {
            baseColor = Color.FromRgb(baseColor.R, baseColor.G, baseColor.B);
            accent = Color.FromRgb(accent.R, accent.G, accent.B);
            var lightness = (baseColor.R + baseColor.G + baseColor.B) / 3;
            var onAccent = UiTheme.GetContrastRatio(accent, Colors.White) >= 3.0 ? Colors.White : Colors.Black;

            if (isDark)
            {
                return new EditorPalette
                {
                    IsDark = true,
                    Base = baseColor,
                    Accent = accent,
                    Window = baseColor,
                    Panel = Shift(baseColor, 6),
                    Status = Shift(baseColor, -3),
                    ButtonBar = Shift(baseColor, 2),
                    Line = Shift(baseColor, 17),
                    Separator = Shift(baseColor, 25),
                    Hover = Shift(baseColor, 21),
                    Pressed = Shift(baseColor, 29),
                    Field = Shift(baseColor, -3),
                    FieldBorder = Shift(baseColor, 24),
                    Menu = Shift(baseColor, 12),
                    MenuBorder = Shift(baseColor, 36),
                    StrongText = Colors.White,
                    DimText = Gray(lightness + 122),
                    FaintText = Gray(lightness + 95),
                    Icon = Gray(lightness + 151),
                    Thumbnail = Gray(lightness + 187),
                    ThumbnailMuted = Gray(lightness + 73),
                    AccentHover = Shift(accent, 14),
                    AccentSoft = Color.FromArgb(70, accent.R, accent.G, accent.B),
                    BadgeText = Colors.White,
                    OnAccent = onAccent,
                };
            }

            return new EditorPalette
            {
                IsDark = false,
                Base = baseColor,
                Accent = accent,
                Window = Shift(baseColor, -12),
                Panel = Shift(baseColor, -5),
                Status = Shift(baseColor, -19),
                ButtonBar = Shift(baseColor, -17),
                Line = Shift(baseColor, -33),
                Separator = Shift(baseColor, -47),
                Hover = Shift(baseColor, -29),
                Pressed = Shift(baseColor, -41),
                Field = baseColor,
                FieldBorder = Shift(baseColor, -51),
                Menu = baseColor,
                MenuBorder = Shift(baseColor, -47),
                StrongText = Colors.Black,
                DimText = Gray(lightness - 157),
                FaintText = Gray(lightness - 127),
                Icon = Gray(lightness - 179),
                Thumbnail = Gray(lightness - 177),
                ThumbnailMuted = Gray(lightness - 59),
                AccentHover = Shift(accent, 14),
                AccentSoft = Color.FromArgb(46, accent.R, accent.G, accent.B),
                BadgeText = accent,
                OnAccent = onAccent,
            };
        }

        /// <summary>Each channel moved by the same amount (clamped), so the color keeps its tint.</summary>
        internal static Color Shift(Color color, int amount)
        {
            return Color.FromRgb(Clamp(color.R + amount), Clamp(color.G + amount), Clamp(color.B + amount));
        }

        private static Color Gray(int value)
        {
            var v = Clamp(value);
            return Color.FromRgb(v, v, v);
        }

        private static byte Clamp(int value) => (byte)Math.Clamp(value, 0, 255);
    }
}
