using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Logic;
using System;

namespace Nikse.SubtitleEdit.Features.Sync;

/// <summary>
/// A video player over a waveform with a drag handle between them, laid out the way the main
/// window lays out its video and waveform. The sync dialogs used to pin the waveform at 80 px,
/// which is too little to pick a precise sync point from once the window holds a player as well
/// (issue #14414).
/// </summary>
public sealed class VideoWaveformSplitGrid : Grid
{
    public const double DefaultWaveformHeight = 80;
    private const double MinWaveformHeight = 40;
    private const double MinVideoHeight = 120;

    private readonly RowDefinition _rowVideo;
    private readonly RowDefinition _rowWaveform;
    private readonly GridSplitter _splitter;
    private bool _isVideoVisible = true;
    private bool _isWaveformVisible;
    private double _waveformHeight;

    /// <summary>Raised while the handle is dragged, with the new waveform height in pixels.</summary>
    public event Action<double>? WaveformHeightChanged;

    /// <summary>The last dragged height - what to remember, never a collapsed or star-sized row.</summary>
    public double WaveformHeight => _waveformHeight;

    public VideoWaveformSplitGrid(Control videoPlayer, AudioVisualizer waveform, double waveformHeight)
    {
        _waveformHeight = SanitizeHeight(waveformHeight);

        _rowVideo = new RowDefinition { Height = new GridLength(1, GridUnitType.Star) };
        _rowWaveform = new RowDefinition { Height = new GridLength(_waveformHeight, GridUnitType.Pixel) };
        RowDefinitions.Add(_rowVideo);
        RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // drag handle
        RowDefinitions.Add(_rowWaveform);
        ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        HorizontalAlignment = HorizontalAlignment.Stretch;
        VerticalAlignment = VerticalAlignment.Stretch;

        _splitter = new GridSplitter
        {
            Height = UiUtil.SplitterWidthOrHeight,
            ResizeDirection = GridResizeDirection.Rows,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 2),
        };
        _splitter.DragDelta += (_, _) => OnDragged();
        _splitter.DragCompleted += (_, _) => OnDragged();

        // The row decides the height from here on.
        waveform.Height = double.NaN;
        waveform.VerticalAlignment = VerticalAlignment.Stretch;

        this.Add(videoPlayer, 0);
        this.Add(_splitter, 1);
        this.Add(waveform, 2);
        UpdateRows();
    }

    public bool IsVideoVisible
    {
        get => _isVideoVisible;
        set
        {
            _isVideoVisible = value;
            UpdateRows();
        }
    }

    public bool IsWaveformVisible
    {
        get => _isWaveformVisible;
        set
        {
            _isWaveformVisible = value;
            UpdateRows();
        }
    }

    /// <summary>Follows another pane's handle, so two side-by-side waveforms stay the same height.</summary>
    public void SetWaveformHeight(double height)
    {
        _waveformHeight = SanitizeHeight(height);
        UpdateRows();
    }

    private void OnDragged()
    {
        if (!_rowWaveform.Height.IsAbsolute)
        {
            return;
        }

        _waveformHeight = SanitizeHeight(_rowWaveform.Height.Value);
        WaveformHeightChanged?.Invoke(_waveformHeight);
    }

    private void UpdateRows()
    {
        // The handle only makes sense with something on both sides of it.
        var hasHandle = _isVideoVisible && _isWaveformVisible;
        _splitter.IsVisible = hasHandle;

        // A Star row keeps its share of the height when its child is merely hidden, so the rows
        // themselves collapse - and a MinHeight on a collapsed row would prop it open.
        _rowVideo.MinHeight = _isVideoVisible ? MinVideoHeight : 0;
        _rowVideo.Height = _isVideoVisible ? new GridLength(1, GridUnitType.Star) : new GridLength(0);

        _rowWaveform.MinHeight = hasHandle ? MinWaveformHeight : 0;
        if (!_isWaveformVisible)
        {
            _rowWaveform.Height = new GridLength(0);
        }
        else if (_isVideoVisible)
        {
            _rowWaveform.Height = new GridLength(_waveformHeight, GridUnitType.Pixel);
        }
        else
        {
            // No player to share with: the waveform takes whatever height the window has.
            _rowWaveform.Height = new GridLength(1, GridUnitType.Star);
        }
    }

    private static double SanitizeHeight(double height)
    {
        if (double.IsNaN(height) || double.IsInfinity(height) || height < MinWaveformHeight)
        {
            return DefaultWaveformHeight;
        }

        return Math.Round(height);
    }
}
