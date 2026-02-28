using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using SkiaSharp;

namespace Nikse.SubtitleEdit.Features.Ocr;

public partial class PreProcessingViewModel : ObservableObject
{
    [ObservableProperty] private bool _cropTransparentColors;
    [ObservableProperty] private bool _inverseColors;
    [ObservableProperty] private bool _binarize;
    [ObservableProperty] private bool _removeBorders;
    [ObservableProperty] private int _borderSize = 2;
    [ObservableProperty] private bool _toOneColor;
    [ObservableProperty] private int _oneColorDarknessThreshold = 128;

    [ObservableProperty] private Bitmap _previewImage;

    public Bitmap SourceBitmap { get; set; }
    public Window? Window { get; set; }

    public PreProcessingSettings PreProcessingSettings { get; private set; }
    public bool OkPressed { get; private set; }

    private bool _dirty;
    private readonly System.Timers.Timer _timerUpdatePreview;

    public PreProcessingViewModel()
    {
        PreProcessingSettings = new PreProcessingSettings();
        PreviewImage = new SKBitmap(1,1, true).ToAvaloniaBitmap();  
        SourceBitmap = new SKBitmap(1,1, true).ToAvaloniaBitmap();
        _timerUpdatePreview = new System.Timers.Timer(250);
    }

    public void Initialize(PreProcessingSettings? preProcessingSettings, SKBitmap sourceBitmap)
    {
        if (preProcessingSettings == null)
        {
            preProcessingSettings = new PreProcessingSettings();
        }

        CropTransparentColors = preProcessingSettings.CropTransparentColors;
        InverseColors = preProcessingSettings.InverseColors;
        Binarize = preProcessingSettings.Binarize;
        RemoveBorders = preProcessingSettings.RemoveBorders;
        BorderSize = preProcessingSettings.BorderSize;
        ToOneColor = preProcessingSettings.ToOneColor;
        OneColorDarknessThreshold = preProcessingSettings.OneColorDarknessThreshold;

        SourceBitmap = sourceBitmap.ToAvaloniaBitmap();
        
        _timerUpdatePreview.Elapsed += (s, e) =>
        {
            _timerUpdatePreview.Stop();
            UpdatePreview();
            _timerUpdatePreview.Start();
        };
        _timerUpdatePreview.Start();
        _dirty = true;
    }

    private void UpdatePreview()
    {
        if (!_dirty)
        {
            return;
        }

        UpdateSettings();
        PreviewImage = PreProcessingSettings.PreProcess(SourceBitmap.ToSkBitmap()).ToAvaloniaBitmap();
        _dirty = false;
    }

    [RelayCommand]
    private void Ok()
    {
        UpdateSettings();

        OkPressed = true;
        Window?.Close();
    }

    private void UpdateSettings()
    {
        PreProcessingSettings = new PreProcessingSettings
        {
            CropTransparentColors = CropTransparentColors,
            InverseColors = InverseColors,
            Binarize = Binarize,
            RemoveBorders = RemoveBorders,
            BorderSize = BorderSize,
            ToOneColor = ToOneColor,
            OneColorDarknessThreshold = OneColorDarknessThreshold,
        };
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    internal void KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }

    internal void RequestPreviewUpdate()
    {
        _dirty = true;
    }
}