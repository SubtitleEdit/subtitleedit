using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
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

    public void UpdateFromColorWheel(Color color)
    {
        if (!_isUpdating)
        {
            _isUpdating = true;
            SelectedColor = Color.FromArgb(Alpha, color.R, color.G, color.B);
            UpdateFromColor(SelectedColor);
            _isUpdating = false;
        }
    }

    private void UpdateColorFromRgb()
    {
        _isUpdating = true;
        SelectedColor = Color.FromArgb(Alpha, Red, Green, Blue);
        OnPropertyChanged(nameof(SelectedColor));
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
        UpdateHexColor();
    }

    private void UpdateHexColor()
    {
        HexColor = SelectedColor.FromColorToHex(true).TrimStart('#');
        OnPropertyChanged(nameof(HexColor));
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
                        SelectedColor = color;
                        UpdateFromColor(color);
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
