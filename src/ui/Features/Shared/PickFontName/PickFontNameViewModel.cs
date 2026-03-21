using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Features.Shared.PickFontName;

public partial class PickFontNameViewModel : ObservableObject
{
    [ObservableProperty] private string _searchText;
    [ObservableProperty] private ObservableCollection<string> _fontNames;
    [ObservableProperty] private string? _selectedFontName;
    [ObservableProperty] private Bitmap _imagePreview;
    [ObservableProperty] private decimal _fontSize;
    [ObservableProperty] private bool _isFontSizeVisible;
    [ObservableProperty] private bool _isFontBold;
    [ObservableProperty] private bool _isFontBoldVisible;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    private List<string> _allFontNames;
    private readonly System.Timers.Timer _timerUpdate;
    private bool _dirtySearch;
    private bool _dirtyPreview;

    public PickFontNameViewModel()
    {
        _allFontNames = FontHelper.GetSystemFonts();
        SearchText = string.Empty;
        FontNames = new ObservableCollection<string>(_allFontNames);
        SelectedFontName = FontNames.Count > 0 ? FontNames[0] : null;
        ImagePreview = new SKBitmap(1, 1, true).ToAvaloniaBitmap();

        _timerUpdate = new System.Timers.Timer(500);
        _timerUpdate.Elapsed += (s, e) =>
        {
            _timerUpdate.Stop();
            if (_dirtySearch)
            {
                _dirtySearch = false;
                UpdateSearch();
            }
            if (_dirtyPreview)
            {
                _dirtyPreview = false;
                UpdatePreview();
            }
            _timerUpdate.Start();
        };
    }

    internal void Initialize(bool isFontSizeVisible = false, bool isFontBoldVisible = false)
    {
        IsFontSizeVisible = isFontSizeVisible;
        IsFontBoldVisible = isFontBoldVisible;
        _timerUpdate.Start();
        _dirtyPreview = true;
    }

    private void UpdateSearch()
    {
        if (string.IsNullOrWhiteSpace(SearchText) && FontNames.Count == _allFontNames.Count)
        {
            return;
        }

        Dispatcher.UIThread.Invoke(() =>
        {
            FontNames.Clear();
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                FontNames.AddRange(_allFontNames);
                return;
            }

            foreach (var encoding in _allFontNames)
            {
                if (encoding.Contains(SearchText, StringComparison.InvariantCultureIgnoreCase))
                {
                    FontNames.Add(encoding);
                }
            }

            if (FontNames.Count > 0)
            {
                SelectedFontName = FontNames[0];
            }
        });
    }

    private void UpdatePreview()
    {
        if (string.IsNullOrWhiteSpace(SelectedFontName))
        {
            ImagePreview = new SKBitmap(1, 1, true).ToAvaloniaBitmap();
            return;
        }

        var previewWidth = 750; 
        var previewHeight = 200; 

        var skTypeface = SKTypeface.FromFamilyName(SelectedFontName);
        if (skTypeface == null)
        {
            ImagePreview = new SKBitmap(1, 1, true).ToAvaloniaBitmap();
            return;
        }

        ImagePreview?.Dispose();

        var imageInfo = new SKImageInfo(previewWidth, previewHeight, SKColorType.Bgra8888, SKAlphaType.Premul);
        using var surface = SKSurface.Create(imageInfo);
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.Transparent);
        using var font = new SKFont(skTypeface, 32);

        if (IsFontBold)
        { 
            font.Embolden = true;
        }

        using var paint = new SKPaint
        {
            Color = SKColors.Orange,
            IsAntialias = true
        };

        var text = $"{SelectedFontName}\nI know the quick brown fox jumps over the lazy dog.\n0123456789";
        var lines = text.SplitToLines() ?? [];
        float y = 25;
        foreach (var line in lines)
        {
            if (!string.IsNullOrEmpty(line))
            {
                canvas.DrawText(line, 12, y, font, paint);
            }
            y += font.Size + 5; // Line spacing using font.Size instead of paint.TextSize
        }

        // Convert to Avalonia bitmap
        using var skImage = surface.Snapshot();
        var skBitmap = SKBitmap.FromImage(skImage);
        ImagePreview = skBitmap.ToAvaloniaBitmap();
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

    internal void DataGridFontNameSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        _dirtyPreview = true;
    }

    internal void SearchTextChanged()
    {
        _dirtySearch = true;
    }

    internal void FontBoldChanged()
    {
        _dirtyPreview = true;
    }
}