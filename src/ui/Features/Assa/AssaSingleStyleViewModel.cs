using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Skia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using SkiaSharp;
using System;
using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Features.Assa;

public partial class AssaSingleStyleViewModel : ObservableObject
{
    [ObservableProperty] private StyleDisplay? _currentStyle;
    [ObservableProperty] private ObservableCollection<string> _fonts;
    [ObservableProperty] private ObservableCollection<BorderStyleItem> _borderTypes;
    [ObservableProperty] private BorderStyleItem _selectedBorderType;
    [ObservableProperty] private Bitmap? _imagePreview;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }
    private string? _previewFontName;

    public AssaSingleStyleViewModel()
    {
        Fonts = new ObservableCollection<string>(FontHelper.GetLibAssaFonts());
        BorderTypes = new ObservableCollection<BorderStyleItem>(BorderStyleItem.List());
        SelectedBorderType = BorderTypes[0];
    }

    public void Initialize(SsaStyle style)
    {
        CurrentStyle = new StyleDisplay(style);
        UpdatePreview();
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

    internal void BorderTypeChanged(object? sender, SelectionChangedEventArgs e)
    {
    }

    internal void PreviewFontName(string? fontName)
    {
        if (string.Equals(_previewFontName, fontName, StringComparison.Ordinal))
        {
            return;
        }

        _previewFontName = fontName;
        UpdatePreview();
    }

    internal void ClearPreviewFontName()
    {
        if (_previewFontName == null)
        {
            return;
        }

        _previewFontName = null;
        UpdatePreview();
    }

    private void UpdatePreview()
    {
        var style = CurrentStyle;
        if (style == null)
        {
            ImagePreview = new SKBitmap(1, 1, true).ToAvaloniaBitmap();
            return;
        }

        var text = "This is a test";
        var fontSize = (float)style.FontSize;
        var fontName = string.IsNullOrWhiteSpace(_previewFontName) ? style.FontName : _previewFontName;
        var libAssFontName = FontHelper.GetSkiaFontNameFromLibAssaFontName(fontName);
        SKBitmap bitmap;

        if (style.BorderStyle.Style == BorderStyleType.BoxPerLine)
        {
            bitmap = TextToImageGenerator.GenerateImageWithPadding(
                text,
                fontName,
                fontSize,
                style.Bold,
                style.ColorPrimary.ToSKColor(),
                style.ColorShadow.ToSKColor(),
                style.ColorOutline.ToSKColor(),
                style.ColorOutline.ToSKColor(),
                0,
                (float)style.ShadowWidth);

            if (style.ShadowWidth > 0)
            {
                bitmap = TextToImageGenerator.AddShadowToBitmap(
                    bitmap,
                    (int)Math.Round(style.ShadowWidth, MidpointRounding.AwayFromZero),
                    style.ColorShadow.ToSKColor());
            }
        }
        else if (style.BorderStyle.Style == BorderStyleType.OneBox)
        {
            bitmap = TextToImageGenerator.GenerateImageWithPadding(
                text,
                libAssFontName,
                fontSize,
                style.Bold,
                style.ColorPrimary.ToSKColor(),
                style.ColorOutline.ToSKColor(),
                SKColors.Red,
                style.ColorShadow.ToSKColor(),
                (float)style.OutlineWidth,
                0,
                1.0f,
                (int)Math.Round(style.ShadowWidth));
        }
        else
        {
            bitmap = TextToImageGenerator.GenerateImageWithPadding(
                text,
                libAssFontName,
                fontSize,
                style.Bold,
                style.ColorPrimary.ToSKColor(),
                style.ColorOutline.ToSKColor(),
                style.ColorShadow.ToSKColor(),
                SKColors.Transparent,
                (float)style.OutlineWidth,
                (float)style.ShadowWidth);
        }

        ImagePreview = bitmap.ToAvaloniaBitmap();
    }
}
