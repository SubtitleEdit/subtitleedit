using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Shared.BinaryEdit.SetText;

public partial class SetTextViewModel : ObservableObject
{
    [ObservableProperty] private string _text;
    [ObservableProperty] private ObservableCollection<string> _fontNames;
    [ObservableProperty] private string _selectedFontName;
    [ObservableProperty] private double? _fontSize;
    [ObservableProperty] private bool _fontIsBold;
    [ObservableProperty] private Color _fontColor;
    [ObservableProperty] private Color _outlineColor;
    [ObservableProperty] private Color _shadowColor;
    [ObservableProperty] private Color _backgroundColor;
    [ObservableProperty] private decimal? _outlineWidth;
    [ObservableProperty] private decimal? _shadowWidth;
    [ObservableProperty] private ObservableCollection<BoxTypeItem> _boxTypes;
    [ObservableProperty] private BoxTypeItem _selectedBoxType;
    [ObservableProperty] private Bitmap? _previewBitmap;
   
    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }
    public SKBitmap? ResultBitmap { get; private set; }

    public SetTextViewModel()
    {
        _text = string.Empty;
        FontNames = new ObservableCollection<string>(FontHelper.GetSystemFonts());
        SelectedFontName = FontNames.FirstOrDefault(p => p == Se.Settings.Video.BurnIn.FontName) ?? FontNames[0];
        FontSize = 48;
        FontIsBold = false;
        FontColor = Colors.White;
        OutlineColor = Colors.Black;
        ShadowColor = Colors.Black;
        BackgroundColor = Colors.Transparent;
        OutlineWidth = 2;
        ShadowWidth = 1;

        BoxTypes =
        [
            new BoxTypeItem(BoxType.None, Se.Language.General.None),
            new BoxTypeItem(BoxType.OneBox, "One box"),
            new BoxTypeItem(BoxType.BoxPerLine, "Box per line")
        ];
        SelectedBoxType = BoxTypes[0];

        LoadSettings();
    }

    private void LoadSettings()
    {
        SelectedFontName = FontNames.FirstOrDefault(p => p == Se.Settings.Tools.BinEditFontName) ?? FontNames[0];
        FontSize = Se.Settings.Tools.BinEditFontSize > 0 ? Se.Settings.Tools.BinEditFontSize : 48;
        FontIsBold = Se.Settings.Tools.BinEditIsBold;
        FontColor = Se.Settings.Tools.BinEditFontColor.FromHexToColor();
        OutlineColor = Se.Settings.Tools.BinEditOutlineColor.FromHexToColor();
        ShadowColor = Se.Settings.Tools.BinEditShadowColor.FromHexToColor();
        BackgroundColor = Se.Settings.Tools.BinEditBackgroundColor.FromHexToColor();
        OutlineWidth = Se.Settings.Tools.BinEditOutlineWidth;
        ShadowWidth = Se.Settings.Tools.BinEditShadowWidth;
    }

    private void SaveSettings()
    {
        Se.Settings.Tools.BinEditFontName = SelectedFontName;
        Se.Settings.Tools.BinEditFontSize = FontSize != null ? (int)FontSize.Value : 48;
        Se.Settings.Tools.BinEditIsBold = FontIsBold;
        Se.Settings.Tools.BinEditFontColor = FontColor.FromColorToHex();
        Se.Settings.Tools.BinEditOutlineColor = OutlineColor.FromColorToHex();
        Se.Settings.Tools.BinEditShadowColor = ShadowColor.FromColorToHex();
        Se.Settings.Tools.BinEditBackgroundColor = BackgroundColor.FromColorToHex();
        Se.Settings.Tools.BinEditOutlineWidth = OutlineWidth ?? 0;
        Se.Settings.Tools.BinEditShadowWidth = ShadowWidth ?? 0;
        Se.SaveSettings();
    }

    partial void OnTextChanged(string value)
    {
        UpdatePreview();
    }

    partial void OnSelectedFontNameChanged(string value)
    {
        UpdatePreview();
    }

    partial void OnFontSizeChanged(double? value)
    {
        UpdatePreview();
    }

    partial void OnFontIsBoldChanged(bool value)
    {
        UpdatePreview();
    }

    partial void OnFontColorChanged(Color value)
    {
        UpdatePreview();
    }

    partial void OnOutlineColorChanged(Color value)
    {
        UpdatePreview();
    }

    partial void OnShadowColorChanged(Color value)
    {
        UpdatePreview();
    }

    partial void OnBackgroundColorChanged(Color value)
    {
        UpdatePreview();
    }

    partial void OnOutlineWidthChanged(decimal? value)
    {
        UpdatePreview();
    }

    partial void OnShadowWidthChanged(decimal? value)
    {
        UpdatePreview();
    }

    partial void OnSelectedBoxTypeChanged(BoxTypeItem value)
    {
        UpdatePreview();
    }

    private void UpdatePreview()
    {
        if (string.IsNullOrWhiteSpace(Text) || FontSize == null || FontSize <= 0)
        {
            PreviewBitmap = null;
            return;
        }

        try
        {
            var skBitmap = GenerateBitmap();
            if (skBitmap != null)
            {
                PreviewBitmap = skBitmap.ToAvaloniaBitmap();
                skBitmap.Dispose();
            }
        }
        catch
        {
            PreviewBitmap = null;
        }
    }

    private SKBitmap? GenerateBitmap()
    {
        if (string.IsNullOrWhiteSpace(Text) || FontSize == null || FontSize <= 0)
        {
            return null;
        }

        var skTextColor = new SKColor(FontColor.R, FontColor.G, FontColor.B, FontColor.A);
        var skOutlineColor = new SKColor(OutlineColor.R, OutlineColor.G, OutlineColor.B, OutlineColor.A);
        var skShadowColor = new SKColor(ShadowColor.R, ShadowColor.G, ShadowColor.B, ShadowColor.A);
        var skBackgroundColor = new SKColor(BackgroundColor.R, BackgroundColor.G, BackgroundColor.B, BackgroundColor.A);

        var outlineWidthValue = (float)(OutlineWidth ?? 0);
        var shadowWidthValue = (float)(ShadowWidth ?? 0);

        if (SelectedBoxType.Type == BoxType.None)
        {
            return TextToImageGenerator.GenerateImageWithPadding(
                Text,
                SelectedFontName,
                (float)FontSize.Value,
                FontIsBold,
                skTextColor,
                skOutlineColor,
                skShadowColor,
                SKColors.Transparent,
                outlineWidthValue,
                shadowWidthValue,
                1.0f,
                10);
        }

        // For box types, we need to add background
        var bitmapWithoutBox = TextToImageGenerator.GenerateImageWithPadding(
            Text,
            SelectedFontName,
            (float)FontSize.Value,
            FontIsBold,
            skTextColor,
            skOutlineColor,
            skShadowColor,
            SKColors.Transparent,
            outlineWidthValue,
            shadowWidthValue,
            1.0f,
            10);

        // Create a new bitmap with background
        var finalBitmap = new SKBitmap(bitmapWithoutBox.Width, bitmapWithoutBox.Height);
        using (var canvas = new SKCanvas(finalBitmap))
        {
            canvas.Clear(skBackgroundColor);
            canvas.DrawBitmap(bitmapWithoutBox, 0, 0);
        }
        bitmapWithoutBox.Dispose();

        return finalBitmap;
    }

    [RelayCommand]
    private void Ok()
    {
        ResultBitmap = GenerateBitmap();
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    public void Closing()
    {
        SaveSettings();
    }
}

public enum BoxType
{
    None,
    OneBox,
    BoxPerLine
}

public class BoxTypeItem
{
    public BoxType Type { get; }
    public string Name { get; }

    public BoxTypeItem(BoxType type, string name)
    {
        Type = type;
        Name = name;
    }

    public override string ToString() => Name;
}

