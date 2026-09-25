using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Shared.ColorPicker;

public partial class ColorPickerViewModel : ObservableObject
{
    [ObservableProperty] private Color _selectedColor = Colors.White;

    [ObservableProperty] private byte _red = 255;
    [ObservableProperty] private byte _green = 255;
    [ObservableProperty] private byte _blue = 255;
    [ObservableProperty] private byte _alpha = 255;

    [ObservableProperty] private string _hexColor = "FFFFFFFF";

    [ObservableProperty] private Color _redGradientStart = Colors.Black;
    [ObservableProperty] private Color _redGradientEnd = Colors.Red;
    [ObservableProperty] private Color _greenGradientStart = Colors.Black;
    [ObservableProperty] private Color _greenGradientEnd = Colors.Green;
    [ObservableProperty] private Color _blueGradientStart = Colors.Black;
    [ObservableProperty] private Color _blueGradientEnd = Colors.Blue;
    [ObservableProperty] private Color _alphaGradientStart = Colors.Transparent;
    [ObservableProperty] private Color _alphaGradientEnd = Colors.White;

    [ObservableProperty] private bool _showAlpha = true;

    // HSV state for the color wheel (hue/saturation) and the brightness bar. Kept separately
    // from RGB so hue and saturation survive dragging the brightness down to black and back.
    [ObservableProperty] private double _hue;
    [ObservableProperty] private double _saturation;
    [ObservableProperty] private double _brightness = 1;
    [ObservableProperty] private Color _brightnessTopColor = Colors.White;

    [ObservableProperty] private Color _lastColorPickerColor;
    [ObservableProperty] private Color _lastColorPickerColor1;
    [ObservableProperty] private Color _lastColorPickerColor2;
    [ObservableProperty] private Color _lastColorPickerColor3;
    [ObservableProperty] private Color _lastColorPickerColor4;
    [ObservableProperty] private Color _lastColorPickerColor5;
    [ObservableProperty] private Color _lastColorPickerColor6;
    [ObservableProperty] private Color _lastColorPickerColor7;
    [ObservableProperty] private Color _lastColorPickerDropper;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    private bool _isUpdating;

    public ColorPickerViewModel()
    {
        LoadSettings();
    }

    public void Initialize(Color initialColor)
    {
        _isUpdating = true;
        SelectedColor = initialColor;
        UpdateFromColor(initialColor);
        _isUpdating = false;
    }

    partial void OnRedChanged(byte value)
    {
        if (!_isUpdating)
        {
            UpdateColorFromRgb();
        }
    }

    partial void OnGreenChanged(byte value)
    {
        if (!_isUpdating)
        {
            UpdateColorFromRgb();
        }
    }

    partial void OnBlueChanged(byte value)
    {
        if (!_isUpdating)
        {
            UpdateColorFromRgb();
        }
    }

    partial void OnAlphaChanged(byte value)
    {
        if (!_isUpdating)
        {
            UpdateColorFromRgb();
        }
    }

    partial void OnHexColorChanged(string value)
    {
        if (!_isUpdating && !string.IsNullOrWhiteSpace(value))
        {
            try
            {
                var hexValue = value.TrimStart('#');
                if (hexValue.Length == 6 || hexValue.Length == 8)
                {
                    var color = ("#" + hexValue).FromHexToColor();
                    _isUpdating = true;
                    SelectedColor = color;
                    UpdateFromColor(color);
                    _isUpdating = false;
                }
            }
            catch
            {
                // Invalid hex color, ignore
            }
        }
    }

    partial void OnHueChanged(double value)
    {
        UpdateColorFromHsv();
    }

    partial void OnSaturationChanged(double value)
    {
        UpdateColorFromHsv();
    }

    partial void OnBrightnessChanged(double value)
    {
        UpdateColorFromHsv();
    }

    private void UpdateColorFromHsv()
    {
        if (_isUpdating)
        {
            return;
        }

        _isUpdating = true;
        var color = HsvToColor(Alpha, Hue, Saturation, Brightness);
        SelectedColor = color;
        Red = color.R;
        Green = color.G;
        Blue = color.B;
        BrightnessTopColor = HsvToColor(255, Hue, Saturation, 1);
        UpdateHexColor();
        _isUpdating = false;
    }

    private void UpdateHsvFromColor(Color color)
    {
        var (hue, saturation, value) = ColorToHsv(color);

        // Hue is undefined for grays and saturation is undefined for black - keep the
        // previous values so the wheel marker doesn't jump to the center/right.
        if (value > 0 && saturation > 0)
        {
            Hue = hue;
        }

        if (value > 0)
        {
            Saturation = saturation;
        }

        Brightness = value;
        BrightnessTopColor = HsvToColor(255, Hue, Saturation, 1);
    }

    internal static Color HsvToColor(byte alpha, double hue, double saturation, double value)
    {
        var h = (hue % 360 + 360) % 360 / 60;
        var s = Math.Clamp(saturation, 0, 1);
        var v = Math.Clamp(value, 0, 1);

        var c = v * s;
        var x = c * (1 - Math.Abs(h % 2 - 1));
        var m = v - c;

        var (r, g, b) = (int)h switch
        {
            0 => (c, x, 0.0),
            1 => (x, c, 0.0),
            2 => (0.0, c, x),
            3 => (0.0, x, c),
            4 => (x, 0.0, c),
            _ => (c, 0.0, x),
        };

        return Color.FromArgb(alpha, ToByte(r + m), ToByte(g + m), ToByte(b + m));
    }

    internal static (double Hue, double Saturation, double Value) ColorToHsv(Color color)
    {
        var r = color.R / 255.0;
        var g = color.G / 255.0;
        var b = color.B / 255.0;

        var max = Math.Max(r, Math.Max(g, b));
        var min = Math.Min(r, Math.Min(g, b));
        var delta = max - min;

        double hue = 0;
        if (delta > 0)
        {
            if (max == r)
            {
                hue = 60 * ((g - b) / delta % 6);
            }
            else if (max == g)
            {
                hue = 60 * ((b - r) / delta + 2);
            }
            else
            {
                hue = 60 * ((r - g) / delta + 4);
            }
        }

        if (hue < 0)
        {
            hue += 360;
        }

        var saturation = max > 0 ? delta / max : 0;
        return (hue, saturation, max);
    }

    private static byte ToByte(double value)
    {
        return (byte)Math.Clamp(Math.Round(value * 255), 0, 255);
    }

    public void SelectRecentColor(Color color)
    {
        if (!_isUpdating)
        {
            _isUpdating = true;
            SelectedColor = color;
            UpdateFromColor(color);
            _isUpdating = false;
        }
    }

    private void UpdateColorFromRgb()
    {
        _isUpdating = true;
        SelectedColor = Color.FromArgb(Alpha, Red, Green, Blue);
        OnPropertyChanged(nameof(SelectedColor));
        UpdateHsvFromColor(SelectedColor);
        UpdateHexColor();
        _isUpdating = false;
    }

    private void UpdateFromColor(Color color)
    {
        Red = color.R;
        OnPropertyChanged(nameof(Red));
        Green = color.G;
        OnPropertyChanged(nameof(Green));
        Blue = color.B;
        OnPropertyChanged(nameof(Blue));
        Alpha = color.A;
        OnPropertyChanged(nameof(Alpha));
        UpdateHsvFromColor(color);
        UpdateHexColor();
    }

    private void UpdateHexColor()
    {
        // Only include the alpha byte (AARRGGBB) when an alpha/opacity channel is shown;
        // otherwise the hex is a plain 6-char RRGGBB value. (#11342 follow-up)
        HexColor = SelectedColor.FromColorToHex(ShowAlpha).TrimStart('#');
        OnPropertyChanged(nameof(HexColor));
    }

    partial void OnShowAlphaChanged(bool value)
    {
        // ShowAlpha is set after Initialize(), so refresh the hex to match the channel count.
        UpdateHexColor();
    }

    private void LoadSettings()
    {
        LastColorPickerColor = Se.Settings.Tools.LastColorPickerColor.FromHexToColor();
        LastColorPickerColor1 = Se.Settings.Tools.LastColorPickerColor1.FromHexToColor();
        LastColorPickerColor2 = Se.Settings.Tools.LastColorPickerColor2.FromHexToColor();
        LastColorPickerColor3 = Se.Settings.Tools.LastColorPickerColor3.FromHexToColor();
        LastColorPickerColor4 = Se.Settings.Tools.LastColorPickerColor4.FromHexToColor();
        LastColorPickerColor5 = Se.Settings.Tools.LastColorPickerColor5.FromHexToColor();
        LastColorPickerColor6 = Se.Settings.Tools.LastColorPickerColor6.FromHexToColor();
        LastColorPickerColor7 = Se.Settings.Tools.LastColorPickerColor7.FromHexToColor();
    }

    private void SaveSettings()
    {
        var color = SelectedColor.FromColorToHex();
        var colorList = new List<string>
        {
            Se.Settings.Tools.LastColorPickerColor,
            Se.Settings.Tools.LastColorPickerColor1,
            Se.Settings.Tools.LastColorPickerColor2,
            Se.Settings.Tools.LastColorPickerColor3,
            Se.Settings.Tools.LastColorPickerColor4,
            Se.Settings.Tools.LastColorPickerColor5,
            Se.Settings.Tools.LastColorPickerColor6,
            Se.Settings.Tools.LastColorPickerColor7,
        };

        colorList = colorList.Where(c => c != color).ToList();
        var random = new Random();
        while (colorList.Count < 7)
        {
            colorList.Add(
                new Color(
                    255,
                    (byte)random.Next(256),
                    (byte)random.Next(256),
                    (byte)random.Next(256)
                ).FromColorToHex()
            );
        }

        Se.Settings.Tools.LastColorPickerColor = color;
        Se.Settings.Tools.LastColorPickerColor1 = colorList[0];
        Se.Settings.Tools.LastColorPickerColor2 = colorList[1];
        Se.Settings.Tools.LastColorPickerColor3 = colorList[2];
        Se.Settings.Tools.LastColorPickerColor4 = colorList[3];
        Se.Settings.Tools.LastColorPickerColor5 = colorList[4];
        Se.Settings.Tools.LastColorPickerColor6 = colorList[5];
        Se.Settings.Tools.LastColorPickerColor7 = colorList[6];

        Se.SaveSettings();
    }

    [RelayCommand]
    private void Ok()
    {
        // Shift the last colors
        LastColorPickerColor7 = LastColorPickerColor6;
        LastColorPickerColor6 = LastColorPickerColor5;
        LastColorPickerColor5 = LastColorPickerColor4;
        LastColorPickerColor4 = LastColorPickerColor3;
        LastColorPickerColor3 = LastColorPickerColor2;
        LastColorPickerColor2 = LastColorPickerColor1;
        LastColorPickerColor1 = LastColorPickerColor;
        LastColorPickerColor = SelectedColor;

        SaveSettings();
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.C && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            e.Handled = true;
            var hexColor = "#" + SelectedColor.FromColorToHex(true);
            Dispatcher.UIThread.Post(async () =>
            {
                if (Window == null || Window.Clipboard == null)
                {
                    return;
                }

                await ClipboardHelper.SetTextAsync(Window, hexColor);
            });
        }
        else if (e.Key == Key.V && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            e.Handled = true;
            Dispatcher.UIThread.Post(async () =>
            {
                if (Window == null || Window.Clipboard == null)
                {
                    return;
                }

                var clipboardText = await ClipboardHelper.GetTextAsync(Window);
                if (!string.IsNullOrWhiteSpace(clipboardText))
                {
                    try
                    {
                        var color = clipboardText.FromHexToColor();
                        _isUpdating = true;
                        SelectedColor = color;
                        UpdateFromColor(color);
                        _isUpdating = false;
                    }
                    catch
                    {
                        // Invalid hex color, ignore
                    }
                }
            });
        }
        else if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }
}
