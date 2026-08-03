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
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Shared.PickFontName;

public partial class PickFontNameViewModel : ObservableObject, IClosingCleanup
{
    [ObservableProperty] private string _searchText;
    [ObservableProperty] private ObservableCollection<string> _fontNames;
    [ObservableProperty] private string? _selectedFontName;
    [ObservableProperty] private ObservableCollection<string> _collectedFontNames;
    [ObservableProperty] private string? _selectedCollectedFontName;
    [ObservableProperty] private int _selectedTabIndex;
    [ObservableProperty] private Bitmap _imagePreview;
    [ObservableProperty] private decimal _fontSize;
    [ObservableProperty] private bool _isFontSizeVisible;
    [ObservableProperty] private bool _isFontBold;
    [ObservableProperty] private bool _isFontBoldVisible;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    /// <summary>Set when the pick came from the "Collected fonts" tab - such a font has a file callers can embed.</summary>
    public CollectedFont? SelectedCollectedFont { get; private set; }

    private List<string> _allFontNames;
    private readonly List<CollectedFont> _allCollectedFonts;
    private readonly System.Timers.Timer _timerUpdate;
    private volatile bool _isClosing;
    private bool _dirtySearch;
    private bool _dirtyPreview;

    public PickFontNameViewModel()
    {
        _allFontNames = FontHelper.GetSystemFonts();
        _allCollectedFonts = FontHelper.GetFontsFolderFonts();
        SearchText = string.Empty;
        FontNames = new ObservableCollection<string>(_allFontNames);
        CollectedFontNames = new ObservableCollection<string>(_allCollectedFonts.Select(f => f.Name));
        SelectedFontName = FontNames.Count > 0 ? FontNames[0] : null;
        SelectedCollectedFontName = CollectedFontNames.Count > 0 ? CollectedFontNames[0] : null;
        ImagePreview = new SKBitmap(1, 1, true).ToAvaloniaBitmap();

        _timerUpdate = new System.Timers.Timer(500);
        _timerUpdate.Elapsed += TimerUpdateElapsed;
    }

    partial void OnSelectedTabIndexChanged(int value) => _dirtyPreview = true;

    partial void OnSelectedCollectedFontNameChanged(string? value) => _dirtyPreview = true;

    private void TimerUpdateElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        if (_isClosing)
        {
            return;
        }

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

        // Guard the restart: OnClosingCleanup may have disposed the timer while this handler ran (#12739).
        if (!_isClosing)
        {
            _timerUpdate.Start();
        }
    }

    public void OnClosingCleanup()
    {
        _isClosing = true;
        _timerUpdate.StopAndDispose(TimerUpdateElapsed);
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
        if (string.IsNullOrWhiteSpace(SearchText) &&
            FontNames.Count == _allFontNames.Count &&
            CollectedFontNames.Count == _allCollectedFonts.Count)
        {
            return;
        }

        Dispatcher.UIThread.Invoke(() =>
        {
            FontNames.Clear();
            CollectedFontNames.Clear();
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                FontNames.AddRange(_allFontNames);
                CollectedFontNames.AddRange(_allCollectedFonts.Select(f => f.Name));
                return;
            }

            foreach (var fontName in _allFontNames)
            {
                if (fontName.Contains(SearchText, StringComparison.InvariantCultureIgnoreCase))
                {
                    FontNames.Add(fontName);
                }
            }

            foreach (var collected in _allCollectedFonts)
            {
                if (collected.Name.Contains(SearchText, StringComparison.InvariantCultureIgnoreCase))
                {
                    CollectedFontNames.Add(collected.Name);
                }
            }

            if (FontNames.Count > 0)
            {
                SelectedFontName = FontNames[0];
            }

            if (CollectedFontNames.Count > 0)
            {
                SelectedCollectedFontName = CollectedFontNames[0];
            }
        });
    }

    /// <summary>The selection of the active tab - installed fonts or collected fonts.</summary>
    private string? GetActiveFontName() => SelectedTabIndex == 1 ? SelectedCollectedFontName : SelectedFontName;

    private void UpdatePreview()
    {
        var fontName = GetActiveFontName();
        if (string.IsNullOrWhiteSpace(fontName))
        {
            ImagePreview = new SKBitmap(1, 1, true).ToAvaloniaBitmap();
            return;
        }

        var previewWidth = 750;
        var previewHeight = 200;

        // A collected font need not be installed, so it is rendered from its file.
        SKTypeface? skTypeface = null;
        if (SelectedTabIndex == 1)
        {
            var collected = _allCollectedFonts.Find(f => f.Name.Equals(fontName, StringComparison.OrdinalIgnoreCase));
            if (collected != null)
            {
                skTypeface = SKTypeface.FromFile(collected.FilePath, collected.FaceIndex);
            }
        }

        skTypeface ??= SKTypeface.FromFamilyName(fontName);
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

        var text = $"{fontName}\nI know the quick brown fox jumps over the lazy dog.\n0123456789";
        var lines = text.SplitToLines() ?? [];
        float y = 25;
        foreach (var line in lines)
        {
            if (!string.IsNullOrEmpty(line))
            {
                canvas.DrawText(line, 12, y, SKTextAlign.Left, font, paint);
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
        // Callers read SelectedFontName, so a pick on the collected-fonts tab is promoted to it.
        var fontName = GetActiveFontName();
        if (!string.IsNullOrEmpty(fontName))
        {
            SelectedFontName = fontName;

            if (SelectedTabIndex == 1)
            {
                SelectedCollectedFont = _allCollectedFonts.Find(f => f.Name.Equals(fontName, StringComparison.OrdinalIgnoreCase));
            }
        }

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

    internal void FontNameGridSelectionChanged(object? sender, SelectionChangedEventArgs e)
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