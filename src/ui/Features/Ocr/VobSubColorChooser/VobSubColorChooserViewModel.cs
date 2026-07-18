using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.VobSub;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Shared.ColorPicker;
using Nikse.SubtitleEdit.Logic;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Ocr.VobSubColorChooser;

public partial class VobSubColorChooserViewModel : ObservableObject
{
    [ObservableProperty] private Color _backgroundColor = Colors.Transparent;
    [ObservableProperty] private Color _patternColor = Colors.Black;
    [ObservableProperty] private Color _emphasis1Color = Colors.White;
    [ObservableProperty] private Color _emphasis2Color = Colors.Black;

    [ObservableProperty] private string _backgroundHex = "#00000000";
    [ObservableProperty] private string _patternHex = "#FF000000";
    [ObservableProperty] private string _emphasis1Hex = "#FFFFFFFF";
    [ObservableProperty] private string _emphasis2Hex = "#FF000000";

    [ObservableProperty] private Bitmap? _previewImage;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    public SKColor Background { get; private set; } = SKColors.Transparent;
    public SKColor Pattern { get; private set; } = SKColors.Black;
    public SKColor Emphasis1 { get; private set; } = SKColors.White;
    public SKColor Emphasis2 { get; private set; } = SKColors.Black;

    private readonly IWindowService _windowService;
    private SubPicture? _previewSubPicture;
    private List<SKColor>? _palette;

    public VobSubColorChooserViewModel(IWindowService windowService)
    {
        _windowService = windowService;
    }

    public void Initialize(SubPicture? previewSubPicture, List<SKColor>? palette, SKColor background, SKColor pattern, SKColor emphasis1, SKColor emphasis2)
    {
        _previewSubPicture = previewSubPicture;
        _palette = palette;

        Background = background;
        Pattern = pattern;
        Emphasis1 = emphasis1;
        Emphasis2 = emphasis2;

        BackgroundColor = background.ToAvaloniaColor();
        PatternColor = pattern.ToAvaloniaColor();
        Emphasis1Color = emphasis1.ToAvaloniaColor();
        Emphasis2Color = emphasis2.ToAvaloniaColor();

        UpdateHex();
        UpdatePreview();
    }

    partial void OnBackgroundColorChanged(Color value)
    {
        Background = value.ToSkColor();
        BackgroundHex = value.FromColorToHex();
        UpdatePreview();
    }

    partial void OnPatternColorChanged(Color value)
    {
        Pattern = value.ToSkColor();
        PatternHex = value.FromColorToHex();
        UpdatePreview();
    }

    partial void OnEmphasis1ColorChanged(Color value)
    {
        Emphasis1 = value.ToSkColor();
        Emphasis1Hex = value.FromColorToHex();
        UpdatePreview();
    }

    partial void OnEmphasis2ColorChanged(Color value)
    {
        Emphasis2 = value.ToSkColor();
        Emphasis2Hex = value.FromColorToHex();
        UpdatePreview();
    }

    private void UpdateHex()
    {
        BackgroundHex = BackgroundColor.FromColorToHex();
        PatternHex = PatternColor.FromColorToHex();
        Emphasis1Hex = Emphasis1Color.FromColorToHex();
        Emphasis2Hex = Emphasis2Color.FromColorToHex();
    }

    private void UpdatePreview()
    {
        if (_previewSubPicture == null)
        {
            return;
        }

        try
        {
            var bmp = _previewSubPicture.GetBitmap(_palette, Background, Pattern, Emphasis1, Emphasis2, true, true);
            if (bmp != null)
            {
                PreviewImage = bmp.ToAvaloniaBitmap();
            }
        }
        catch
        {
            // ignore preview render failures
        }
    }

    [RelayCommand]
    private async Task PickBackground()
    {
        var color = await PickColor(BackgroundColor);
        if (color.HasValue)
        {
            BackgroundColor = color.Value;
        }
    }

    [RelayCommand]
    private async Task PickPattern()
    {
        var color = await PickColor(PatternColor);
        if (color.HasValue)
        {
            PatternColor = color.Value;
        }
    }

    [RelayCommand]
    private async Task PickEmphasis1()
    {
        var color = await PickColor(Emphasis1Color);
        if (color.HasValue)
        {
            Emphasis1Color = color.Value;
        }
    }

    [RelayCommand]
    private async Task PickEmphasis2()
    {
        var color = await PickColor(Emphasis2Color);
        if (color.HasValue)
        {
            Emphasis2Color = color.Value;
        }
    }

    private async Task<Color?> PickColor(Color current)
    {
        if (Window == null)
        {
            return null;
        }

        var vm = await _windowService.ShowDialogAsync<ColorPickerWindow, ColorPickerViewModel>(
            Window, viewModel => viewModel.Initialize(current));

        return vm.OkPressed ? vm.SelectedColor : null;
    }

    [RelayCommand]
    private void ResetToDefaults()
    {
        BackgroundColor = Colors.Transparent;
        PatternColor = Colors.Black;
        Emphasis1Color = Colors.White;
        Emphasis2Color = Colors.Black;
    }

    [RelayCommand]
    private void InvertBackgroundForeground()
    {
        var bg = BackgroundColor;
        BackgroundColor = Emphasis1Color;
        Emphasis1Color = bg;
    }

    [RelayCommand]
    private void Ok()
    {
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
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }
}
