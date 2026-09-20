using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Files.ExportImageBased;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Shared.BinaryEdit.BinaryChangeResolution;

/// <summary>
/// Converts an image-based subtitle from one screen resolution to another (BDSup2Sub's
/// "set resolution"): every bitmap and every X/Y position is scaled by the width and height
/// ratios, so a 1080p SUP keeps its layout when re-targeted to 720p or PAL.
/// Only changing the screen size fields in the main window re-labels the canvas and leaves
/// the images oversized and off-screen.
/// </summary>
public partial class BinaryChangeResolutionViewModel : ObservableObject, IDisposable, IClosingCleanup
{
    [ObservableProperty] private ObservableCollection<ResolutionItem> _resolutions;
    [ObservableProperty] private ResolutionItem? _selectedResolution;
    [ObservableProperty] private int _newWidth;
    [ObservableProperty] private int _newHeight;
    [ObservableProperty] private Bitmap? _previewBitmap;
    [ObservableProperty] private string _sizeText;
    [ObservableProperty] private string _scaleText;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }
    public int OriginalWidth { get; private set; }
    public int OriginalHeight { get; private set; }

    private List<BinarySubtitleItem> _subtitles = new();
    private DispatcherTimer? _previewUpdateTimer;
    private bool _isDirty;
    private bool _isSyncingPreset;

    public BinaryChangeResolutionViewModel()
    {
        // The export list starts with "Pick resolution from video..." - a placeholder with no size.
        _resolutions = new ObservableCollection<ResolutionItem>(ResolutionItem.GetResolutions().Where(r => r.Width > 0 && r.Height > 0));
        _sizeText = string.Empty;
        _scaleText = string.Empty;
        InitializeTimer();
    }

    private void InitializeTimer()
    {
        _previewUpdateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(300)
        };
        _previewUpdateTimer.Tick += (_, _) =>
        {
            _previewUpdateTimer?.Stop();
            if (_isDirty)
            {
                _isDirty = false;
                UpdatePreview();
            }
        };
    }

    public void Initialize(List<BinarySubtitleItem> subtitles, int screenWidth, int screenHeight)
    {
        _subtitles = subtitles;
        OriginalWidth = screenWidth;
        OriginalHeight = screenHeight;

        // Start on the current size so OK without edits is a no-op.
        NewWidth = screenWidth;
        NewHeight = screenHeight;
        UpdatePreview();
    }

    partial void OnSelectedResolutionChanged(ResolutionItem? value)
    {
        if (value == null || _isSyncingPreset)
        {
            return;
        }

        _isSyncingPreset = true;
        NewWidth = value.Width;
        NewHeight = value.Height;
        _isSyncingPreset = false;
        SchedulePreviewUpdate();
    }

    partial void OnNewWidthChanged(int value)
    {
        SyncPresetToSize();
        SchedulePreviewUpdate();
    }

    partial void OnNewHeightChanged(int value)
    {
        SyncPresetToSize();
        SchedulePreviewUpdate();
    }

    /// <summary>
    /// Typing a size that matches a preset highlights it; any other size clears the selection
    /// (a custom resolution) without touching the typed numbers.
    /// </summary>
    private void SyncPresetToSize()
    {
        if (_isSyncingPreset)
        {
            return;
        }

        _isSyncingPreset = true;
        SelectedResolution = Resolutions.FirstOrDefault(r => r.Width == NewWidth && r.Height == NewHeight);
        _isSyncingPreset = false;
    }

    private void SchedulePreviewUpdate()
    {
        if (_previewUpdateTimer == null)
        {
            return;
        }

        _isDirty = true;
        _previewUpdateTimer.Stop();
        _previewUpdateTimer.Start();
    }

    private void UpdatePreview()
    {
        SizeText = string.Format(Se.Language.Tools.ImageBasedEdit.OriginalSizeXNewSizeY, OriginalWidth, OriginalHeight, NewWidth, NewHeight);

        var scaleX = GetScale(OriginalWidth, NewWidth);
        var scaleY = GetScale(OriginalHeight, NewHeight);
        ScaleText = $"{Se.Language.Tools.ImageBasedEdit.Percentage}: {Math.Round(scaleX * 100)}% × {Math.Round(scaleY * 100)}%";

        var first = _subtitles.FirstOrDefault(s => s.Bitmap != null);
        if (first == null)
        {
            return;
        }

        using var originalBitmap = first.Bitmap!.ToSkBitmap();
        using var resizedBitmap = ScaleBitmap(originalBitmap, scaleX, scaleY);
        var old = PreviewBitmap;
        PreviewBitmap = resizedBitmap.ToAvaloniaBitmap();
        old?.Dispose();
    }

    public static double GetScale(int from, int to)
    {
        return from <= 0 || to <= 0 ? 1.0 : (double)to / from;
    }

    /// <summary>
    /// Scales positions and bitmaps of <paramref name="subtitles"/> from
    /// <paramref name="fromWidth"/>×<paramref name="fromHeight"/> to
    /// <paramref name="toWidth"/>×<paramref name="toHeight"/>. Width and height scale
    /// independently so anamorphic targets (1920×1080 → 720×576) fill the frame the way
    /// BDSup2Sub does. The caller updates the screen size itself.
    /// </summary>
    public static void ApplyResolution(IEnumerable<BinarySubtitleItem> subtitles, int fromWidth, int fromHeight, int toWidth, int toHeight)
    {
        var scaleX = GetScale(fromWidth, toWidth);
        var scaleY = GetScale(fromHeight, toHeight);
        if (Math.Abs(scaleX - 1.0) < 0.000001 && Math.Abs(scaleY - 1.0) < 0.000001)
        {
            return;
        }

        foreach (var subtitle in subtitles)
        {
            subtitle.X = ScaleCoordinate(subtitle.X, scaleX);
            subtitle.Y = ScaleCoordinate(subtitle.Y, scaleY);

            if (subtitle.Bitmap == null)
            {
                continue;
            }

            using var originalBitmap = subtitle.Bitmap.ToSkBitmap();
            using var resizedBitmap = ScaleBitmap(originalBitmap, scaleX, scaleY);
            var old = subtitle.Bitmap;
            subtitle.Bitmap = resizedBitmap.ToAvaloniaBitmap();
            old?.Dispose();
        }
    }

    public static int ScaleCoordinate(int value, double scale)
    {
        return (int)Math.Round(value * scale, MidpointRounding.AwayFromZero);
    }

    private static SKBitmap ScaleBitmap(SKBitmap originalBitmap, double scaleX, double scaleY)
    {
        // Narrow or short images scale to a zero dimension, which SKCanvas rejects.
        var width = Math.Max(1, (int)Math.Round(originalBitmap.Width * scaleX));
        var height = Math.Max(1, (int)Math.Round(originalBitmap.Height * scaleY));

        var resizedBitmap = new SKBitmap(width, height, originalBitmap.ColorType, originalBitmap.AlphaType);
        using var canvas = new SKCanvas(resizedBitmap);
        canvas.Clear(SKColors.Transparent);
        using var image = SKImage.FromBitmap(originalBitmap);
        canvas.DrawImage(image, new SKRect(0, 0, width, height), new SKSamplingOptions(SKCubicResampler.Mitchell));
        return resizedBitmap;
    }

    [RelayCommand]
    private async Task Ok()
    {
        var msg = GetValidationError();
        if (!string.IsNullOrEmpty(msg))
        {
            await MessageBox.Show(Window!, Se.Language.General.Error, msg, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        ApplyResolution(_subtitles, OriginalWidth, OriginalHeight, NewWidth, NewHeight);
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    private string GetValidationError()
    {
        if (Window == null)
        {
            return "Window is null";
        }

        if (NewWidth <= 0)
        {
            return string.Format(Se.Language.General.PleaseEnterAValidValueForX, Se.Language.General.Width);
        }

        if (NewHeight <= 0)
        {
            return string.Format(Se.Language.General.PleaseEnterAValidValueForX, Se.Language.General.Height);
        }

        return string.Empty;
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }

    public void OnClosingCleanup()
    {
        Dispose();
    }

    public void Dispose()
    {
        _isDirty = false;
        _previewUpdateTimer?.Stop();
        _previewUpdateTimer = null;
        var old = PreviewBitmap;
        PreviewBitmap = null;
        old?.Dispose();
    }
}
