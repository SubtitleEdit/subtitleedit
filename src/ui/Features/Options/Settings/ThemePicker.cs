using System.ComponentModel;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

/// <summary>
/// The theme setting as a row of cards, each a small window drawn in that theme's colors,
/// instead of a combo box of names.
/// </summary>
internal static class ThemePicker
{
    private sealed record Palette(Color Window, Color Header, Color Text, Color Button, Color Accent);

    internal static Control Make(SettingsViewModel vm)
    {
        var panel = new WrapPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top, // In line with the label
        };

        // Themes holds the translated names in the order of the theme setting names below
        string[] themeNames =
        [
            UiTheme.ThemeNameSystem, UiTheme.ThemeNameLight, UiTheme.ThemeNameDark,
            UiTheme.ThemeNameClassic, UiTheme.ThemeNamePastel, UiTheme.ThemeNameNord,
            UiTheme.ThemeNameMidnightCinema, UiTheme.ThemeNameSakura,
        ];

        for (var i = 0; i < vm.Themes.Count && i < themeNames.Length; i++)
        {
            panel.Children.Add(MakeCard(vm, vm.Themes[i], themeNames[i]));
        }

        void UpdateSelection()
        {
            foreach (var child in panel.Children)
            {
                if (child is Button button)
                {
                    if (button.Content is StackPanel { Children: [Border frame, TextBlock name] })
                    {
                        var isSelected = Equals(button.Tag, vm.SelectedTheme);
                        frame.BorderBrush = isSelected ? EditorChrome.AccentBrush : EditorChrome.LineBrush;
                        frame.BorderThickness = new Thickness(isSelected ? 2 : 1);
                        frame.Margin = new Thickness(isSelected ? 0 : 1);
                        name.FontWeight = isSelected ? FontWeight.SemiBold : FontWeight.Normal;
                        name.Foreground = isSelected ? EditorChrome.StrongTextBrush : EditorChrome.DimTextBrush;
                    }
                }
            }
        }

        void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SettingsViewModel.SelectedTheme))
            {
                UpdateSelection();
            }
        }

        panel.AttachedToVisualTree += (_, _) => vm.PropertyChanged += OnPropertyChanged;
        panel.DetachedFromVisualTree += (_, _) => vm.PropertyChanged -= OnPropertyChanged;
        UpdateSelection();

        return panel;
    }

    private static Button MakeCard(SettingsViewModel vm, string displayName, string themeName)
    {
        var width = EditorChrome.ScaledSize(96);
        var height = EditorChrome.ScaledSize(70);

        Control preview;
        if (themeName == UiTheme.ThemeNameSystem)
        {
            // Light on the left, dark on the right - it follows the operating system
            var split = new Grid
            {
                ColumnDefinitions = { new ColumnDefinition(1, GridUnitType.Star), new ColumnDefinition(1, GridUnitType.Star) },
            };
            var light = MakeWindow(GetPalette(UiTheme.ThemeNameLight), width);
            var dark = MakeWindow(GetPalette(UiTheme.ThemeNameDark), width);
            light.HorizontalAlignment = HorizontalAlignment.Left;
            dark.HorizontalAlignment = HorizontalAlignment.Right;
            split.Children.Add(new Border { ClipToBounds = true, Child = light, [Grid.ColumnProperty] = 0 });
            split.Children.Add(new Border { ClipToBounds = true, Child = dark, [Grid.ColumnProperty] = 1 });
            preview = split;
        }
        else
        {
            preview = MakeWindow(GetPalette(themeName), width);
        }

        var frame = new Border
        {
            Width = width + 4,
            Height = height + 4,
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(1),
            Child = new Border
            {
                CornerRadius = new CornerRadius(6),
                ClipToBounds = true,
                Child = preview,
            },
        };

        var name = new TextBlock
        {
            Text = displayName,
            FontSize = EditorChrome.FieldFontSize,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 5, 0, 0),
        };

        var button = new Button
        {
            Tag = displayName,
            Content = new StackPanel { Children = { frame, name } },
            Command = new CommunityToolkit.Mvvm.Input.RelayCommand(() => vm.SelectedTheme = displayName),
            Padding = new Thickness(5),
            Margin = new Thickness(0, 0, 6, 6),
        };
        button.Classes.Add("tool");
        AutomationProperties.SetName(button, displayName);
        ToolTip.SetTip(button, displayName);
        return button;
    }

    /// <summary>
    /// A miniature window: title bar, a few lines of text, a selected row in the accent color
    /// and a button.
    /// </summary>
    private static Control MakeWindow(Palette palette, double width)
    {
        IBrush Brush(Color color, double opacity = 1) => new SolidColorBrush(color, opacity);
        Border Line(double lineWidth, double opacity) => new()
        {
            Width = lineWidth,
            Height = 4,
            CornerRadius = new CornerRadius(2),
            Background = Brush(palette.Text, opacity),
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(8, 0, 0, 5),
        };

        var titleBar = new Border
        {
            Height = 12,
            Background = Brush(palette.Header),
            Child = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 3,
                Margin = new Thickness(6, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Children =
                {
                    new Ellipse { Width = 4, Height = 4, Fill = Brush(palette.Text, 0.35) },
                    new Ellipse { Width = 4, Height = 4, Fill = Brush(palette.Text, 0.35) },
                    new Ellipse { Width = 4, Height = 4, Fill = Brush(palette.Text, 0.35) },
                },
            },
        };

        var selectedRow = new Border
        {
            Height = 9,
            Margin = new Thickness(4, 0, 4, 5),
            CornerRadius = new CornerRadius(2),
            Background = Brush(palette.Accent),
        };

        var button = new Border
        {
            Width = 26,
            Height = 10,
            CornerRadius = new CornerRadius(3),
            Background = Brush(palette.Button),
            BorderBrush = Brush(palette.Text, 0.2),
            BorderThickness = new Thickness(1),
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 0, 6, 0),
        };

        return new Border
        {
            Width = width,
            Background = Brush(palette.Window),
            Child = new StackPanel
            {
                Children =
                {
                    titleBar,
                    new Border { Height = 6 },
                    Line(width * 0.6, 0.55),
                    selectedRow,
                    Line(width * 0.45, 0.35),
                    button,
                },
            },
        };
    }

    private static Palette GetPalette(string themeName)
    {
        var text = Color.FromRgb(0x1F, 0x1F, 0x1F);
        switch (themeName)
        {
            case UiTheme.ThemeNameDark:
                var appearance = Se.Settings.Appearance;
                var background = TryParse(appearance.DarkModeBackgroundColor, EditorChrome.DefaultDarkBase);
                var foreground = TryParse(appearance.DarkModeForegroundColor, Color.FromRgb(220, 220, 220));
                return new Palette(background, Lighten(background, 0.08), foreground, Lighten(background, 0.16), EditorChrome.FallbackAccent);
            case UiTheme.ThemeNameClassic:
                return new Palette(Color.FromRgb(236, 233, 216), Color.FromRgb(192, 192, 192), Colors.Black, Color.FromRgb(212, 208, 200), Color.FromRgb(10, 36, 106));
            case UiTheme.ThemeNamePastel:
                return new Palette(Color.FromRgb(240, 235, 255), Color.FromRgb(232, 222, 252), Color.FromRgb(0x3A, 0x30, 0x55), Color.FromRgb(227, 216, 250), Color.FromRgb(142, 111, 216));
            case UiTheme.ThemeNameNord:
                return new Palette(UiTheme.NordBackgroundColor, Color.FromRgb(0x3B, 0x42, 0x52), UiTheme.NordForegroundColor, Color.FromRgb(0x43, 0x4C, 0x5E), Color.FromRgb(0x88, 0xC0, 0xD0));
            case UiTheme.ThemeNameMidnightCinema:
                return new Palette(UiTheme.MidnightCinemaBackgroundColor, UiTheme.MidnightCinemaHeaderColor, UiTheme.MidnightCinemaForegroundColor, Color.FromRgb(0x2A, 0x26, 0x31), UiTheme.MidnightCinemaAccentColor);
            case UiTheme.ThemeNameSakura:
                return new Palette(UiTheme.SakuraBackgroundColor, Color.FromRgb(248, 228, 235), UiTheme.SakuraForegroundColor, Color.FromRgb(250, 226, 234), UiTheme.SakuraAccentColor);
            default:
                return new Palette(Colors.White, Color.FromRgb(243, 243, 243), text, Color.FromRgb(240, 240, 240), EditorChrome.FallbackAccent);
        }
    }

    private static Color TryParse(string? hex, Color fallback)
    {
        if (string.IsNullOrWhiteSpace(hex))
        {
            return fallback;
        }

        try
        {
            return hex.FromHexToColor();
        }
        catch
        {
            return fallback;
        }
    }

    private static Color Lighten(Color color, double amount)
    {
        byte Mix(byte channel) => (byte)(channel + (255 - channel) * amount);
        return Color.FromRgb(Mix(color.R), Mix(color.G), Mix(color.B));
    }
}
