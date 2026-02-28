using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Controls.AudioVisualizerControl;

public class AudioVisualizer : Control
{
    public static readonly StyledProperty<WavePeakData2?> WavePeaksProperty =
        AvaloniaProperty.Register<AudioVisualizer, WavePeakData2?>(nameof(WavePeaks));

    public static readonly StyledProperty<double> StartPositionSecondsProperty =
        AvaloniaProperty.Register<AudioVisualizer, double>(nameof(StartPositionSeconds));

    public static readonly StyledProperty<double> ZoomFactorProperty =
        AvaloniaProperty.Register<AudioVisualizer, double>(nameof(ZoomFactor), 1.0);

    public static readonly StyledProperty<double> VerticalZoomFactorProperty =
        AvaloniaProperty.Register<AudioVisualizer, double>(nameof(VerticalZoomFactor), 1.0);

    public static readonly StyledProperty<double> CurrentVideoPositionSecondsProperty =
        AvaloniaProperty.Register<AudioVisualizer, double>(nameof(CurrentVideoPositionSeconds));

    public static readonly StyledProperty<bool> DrawGridLinesProperty =
        AvaloniaProperty.Register<AudioVisualizer, bool>(nameof(DrawGridLines));

    public static readonly StyledProperty<bool> IsReadOnlyProperty =
        AvaloniaProperty.Register<AudioVisualizer, bool>(nameof(IsReadOnly));

    public static readonly StyledProperty<bool> InvertMouseWheelProperty =
        AvaloniaProperty.Register<AudioVisualizer, bool>(nameof(InvertMouseWheel));

    public static readonly StyledProperty<List<SubtitleLineViewModel>> AllSelectedParagraphsProperty =
        AvaloniaProperty.Register<AudioVisualizer, List<SubtitleLineViewModel>>(nameof(AllSelectedParagraphs));

    public static readonly StyledProperty<Color> WaveformColorProperty =
        AvaloniaProperty.Register<AudioVisualizer, Color>(nameof(WaveformColor));

    public static readonly StyledProperty<Color> WaveformBackgroundColorProperty =
        AvaloniaProperty.Register<AudioVisualizer, Color>(nameof(WaveformBackgroundColor));

    public static readonly StyledProperty<Color> WaveformSelectedColorProperty =
        AvaloniaProperty.Register<AudioVisualizer, Color>(nameof(WaveformSelectedColor));

    public static readonly StyledProperty<Color> WaveformCursorColorProperty =
        AvaloniaProperty.Register<AudioVisualizer, Color>(nameof(WaveformCursorColor));

    public WavePeakData2? WavePeaks
    {
        get => GetValue(WavePeaksProperty);
        set => SetValue(WavePeaksProperty, value);
    }

    public double StartPositionSeconds
    {
        get => GetValue(StartPositionSecondsProperty);
        set
        {
            var clampedValue = Math.Max(0, Math.Min(value, MaxStartPositionSeconds));
            SetValue(StartPositionSecondsProperty, clampedValue);
        }
    }

    public double ZoomFactor
    {
        get => GetValue(ZoomFactorProperty);
        set => SetValue(ZoomFactorProperty, value);
    }

    public double VerticalZoomFactor
    {
        get => GetValue(VerticalZoomFactorProperty);
        set => SetValue(VerticalZoomFactorProperty, value);
    }

    public double CurrentVideoPositionSeconds
    {
        get => GetValue(CurrentVideoPositionSecondsProperty);
        set => SetValue(CurrentVideoPositionSecondsProperty, value);
    }

    public bool InvertMouseWheel
    {
        get => GetValue(InvertMouseWheelProperty);
        set => SetValue(InvertMouseWheelProperty, value);
    }

    public bool DrawGridLines
    {
        get => GetValue(DrawGridLinesProperty);
        set => SetValue(DrawGridLinesProperty, value);
    }

    public bool IsReadOnly
    {
        get => GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    public List<SubtitleLineViewModel> AllSelectedParagraphs
    {
        get => GetValue(AllSelectedParagraphsProperty);
        set => SetValue(AllSelectedParagraphsProperty, value);
    }

    public Color WaveformColor
    {
        get => GetValue(WaveformColorProperty);
        set
        {
            _paintWaveform = new Pen(new SolidColorBrush(value), 1);
            SetValue(WaveformColorProperty, value);
        }
    }

    public Color WaveformBackgroundColor
    {
        get => GetValue(WaveformBackgroundColorProperty);
        set
        {
            _paintBackground = new SolidColorBrush(value);
            SetValue(WaveformBackgroundColorProperty, value);
        }
    }

    public Color WaveformSelectedColor
    {
        get => GetValue(WaveformSelectedColorProperty);
        set
        {
            _paintPenSelected = new Pen(new SolidColorBrush(value), 1);
            SetValue(WaveformSelectedColorProperty, value);
        }
    }

    public Color WaveformCursorColor
    {
        get => GetValue(WaveformCursorColorProperty);
        set
        {
            _paintPenCursor = new Pen(new SolidColorBrush(value), 1);
            SetValue(WaveformCursorColorProperty, value);
        }
    }

    public SubtitleLineViewModel? SelectedParagraph { get; set; }

    public double MinGapSeconds { get; set; } = 0.1;

    public double ShotChangeSnapSeconds { get; set; } = 0.05;
    public WaveformDrawStyle WaveformDrawStyle { get; set; } = WaveformDrawStyle.Classic;

    public bool SnapToShotChanges { get; set; } = true;
    public bool FocusOnMouseOver { get; set; } = true;
    public int WaveformHeightPercentage { get; set; } = 50;
    public Color WaveformFancyHighColor { get; set; } = Colors.Orange;

    private Color _paragraphBackground = Color.FromArgb(90, 70, 70, 70);

    public Color ParagraphBackground
    {
        get => _paragraphBackground;
        set
        {
            _paragraphBackground = value;
            _paintParagraphBackground = new SolidColorBrush(_paragraphBackground);
        }
    }

    private Color _paragraphSelectedBackground = Color.FromArgb(90, 70, 70, 70);

    public Color ParagraphSelectedBackground
    {
        get => _paragraphSelectedBackground;
        set
        {
            _paragraphSelectedBackground = value;
            _paintParagraphSelectedBackground = new SolidColorBrush(_paragraphSelectedBackground);
        }
    }

    private List<double> _shotChanges = new List<double>();

    /// <summary>
    /// Shot changes (seconds)
    /// </summary>
    public List<double> ShotChanges
    {
        get => _shotChanges;
        set { _shotChanges = value; }
    }

    public void UseSmpteDropFrameTime()
    {
        if (WavePeaks != null)
        {
            var list = new List<WavePeak2>(WavePeaks.Peaks.Count);
            for (var i = 0; i < WavePeaks.Peaks.Count; i++)
            {
                if (i % 1001 != 0)
                {
                    list.Add(WavePeaks.Peaks[i]);
                }
            }

            WavePeaks = new WavePeakData2(WavePeaks.SampleRate, list);

            if (_shotChanges?.Count > 0)
            {
                _shotChanges = _shotChanges.Select(sc => Math.Round(sc /= 1.001, 3, MidpointRounding.AwayFromZero)).ToList();
            }
        }
    }

    private double MaxStartPositionSeconds
    {
        get
        {
            if (WavePeaks == null || Bounds.Width <= 0 || ZoomFactor <= 0)
            {
                return int.MaxValue;
            }

            // Calculate how many seconds are visible in the current view
            var visibleSeconds = Bounds.Width / (ZoomFactor * WavePeaks.SampleRate);

            // Maximum start position is total length minus visible length
            var maxStart = WavePeaks.LengthInSeconds - visibleSeconds;

            // Don't allow negative values (when zoomed out beyond waveform length)
            return Math.Max(0, maxStart + 1); // +1 to allow some extra space at end
        }
    }

    // Pens and brushes
    private Pen _paintWaveform = new Pen(new SolidColorBrush(Color.FromArgb(150, 144, 238, 144)), 1);
    private Pen _paintPenSelected = new Pen(new SolidColorBrush(Color.FromArgb(210, 254, 10, 10)), 1);
    private Pen _paintPenCursor = new Pen(Brushes.Cyan, 1);
    private readonly Pen _paintGridLines = new Pen(Brushes.DarkGray, 0.2);
    private readonly IBrush _mouseOverBrush = new SolidColorBrush(Color.FromArgb(50, 255, 255, 0));

    // Cached drawing resources for fancy waveform
    private readonly Dictionary<int, Pen> _fancyWaveformPenCache = new(20);
    private readonly Dictionary<int, LinearGradientBrush> _fancyWaveformGradientCache = new(20);
    private readonly Dictionary<int, Pen> _fancyWaveformGlowPenCache = new(10);

    // Paragraph painting
    private IBrush _paintBackground = new SolidColorBrush(Color.FromArgb(90, 70, 70, 70));
    private IBrush _paintParagraphBackground = new SolidColorBrush(Color.FromArgb(90, 70, 70, 70));
    private IBrush _paintParagraphSelectedBackground = new SolidColorBrush(Color.FromArgb(90, 70, 70, 70));
    private readonly Pen _paintLeft = new Pen(new SolidColorBrush(Color.FromArgb(60, 0, 255, 0)), 2);
    private readonly Pen _paintRight = new Pen(new SolidColorBrush(Color.FromArgb(100, 255, 0, 0)), 2);
    private readonly IBrush _paintText = Brushes.White;
    private readonly Typeface _typeface = new Typeface(UiUtil.GetDefaultFontName());
    private readonly double _fontSize = 12;

    private readonly List<SubtitleLineViewModel> _displayableParagraphs = new();
    private bool _isCtrlDown;
    private bool _isMetaDown;
    private bool _isAltDown;
    private bool _isShiftDown;
    private long _lastMouseWheelScroll = -1;
    private readonly Lock _lock = new();
    public SubtitleLineViewModel? NewSelectionParagraph { get; set; }
    public double _newSelectionSeconds { get; set; }
    private SubtitleLineViewModel? _activeParagraph;
    private SubtitleLineViewModel? _activeParagraphPrevious;
    private SubtitleLineViewModel? _activeParagraphNext;
    private Point _startPointerPosition;
    private double _originalStartSeconds;
    private double _originalEndSeconds;
    private double _originalDurationSeconds;
    private double _originalPreviousEndSeconds;
    private double _originalNextStartSeconds;
    private long _audioVisualizerLastScroll;
    private SubtitleLineViewModel? _cachedHitParagraph;
    private bool _cachedIsNearLeftEdge;
    private bool _cachedIsNearRightEdge;
    private long _lastPointerPressed = -1;
    private WaveformDisplayMode _displayMode = WaveformDisplayMode.OnlyWaveform;
    private SpectrogramData2? _spectrogram;

    private enum InteractionMode
    {
        None,
        Moving,
        ResizingLeft,
        ResizingLeftOr,
        ResizingRight,
        ResizingRightOr,
        ResizeLeftAnd,
        ResizeRightAnd,
        New,
    }

    private InteractionMode _interactionMode = InteractionMode.None;

    public readonly double ResizeMargin = 5.0; // Margin for resizing paragraphs
    public bool IsScrolling => (Environment.TickCount64 - _audioVisualizerLastScroll) < 100;

    public class PositionEventArgs : EventArgs
    {
        public double PositionInSeconds { get; set; }
    }

    public class ContextEventArgs : EventArgs
    {
        public double PositionInSeconds { get; set; }
        public SubtitleLineViewModel? NewParagraph { get; set; }
    }

    public delegate void PositionEventHandler(object sender, PositionEventArgs e);

    public delegate void ContextEventHandler(object sender, ContextEventArgs e);

    public delegate void ParagraphEventHandler(object sender, ParagraphEventArgs e);
    public delegate void ParagraphNullableEventHandler(object sender, ParagraphNullableEventArgs e);

    public event PositionEventHandler? OnVideoPositionChanged;
    public event ContextEventHandler? FlyoutMenuOpening;
    public event ParagraphEventHandler? OnToggleSelection;
    public event PositionEventHandler? OnHorizontalScroll;
    public event ParagraphEventHandler? OnNewSelectionInsert;
    public event ParagraphEventHandler? OnDeletePressed;
    public event ParagraphEventHandler? OnSelectRequested;
    public event ParagraphNullableEventHandler? OnPrimarySingleClicked;
    public event ParagraphNullableEventHandler? OnPrimaryDoubleClicked;
    public event PositionEventHandler? OnSetStartAndOffsetTheRest;

    public AudioVisualizer()
    {
        AllSelectedParagraphs = new List<SubtitleLineViewModel>();
        Focusable = true;
        IsHitTestVisible = true;
        ClipToBounds = true;
        MenuFlyout = new MenuFlyout();

        AffectsRender<AudioVisualizer>(
            WavePeaksProperty,
            StartPositionSecondsProperty,
            ZoomFactorProperty,
            VerticalZoomFactorProperty,
            CurrentVideoPositionSecondsProperty,
            AllSelectedParagraphsProperty);

        PointerMoved += OnPointerMoved;
        PointerEntered += OnPointerEntered;
        PointerExited += OnPointerExited;
        PointerPressed += OnPointerPressed;
        PointerReleased += OnPointerReleased;
        PointerWheelChanged += OnPointerWheelChanged;
        Tapped += OnTapped;
        DoubleTapped += (sender, e) =>
        {
            if (OnPrimaryDoubleClicked != null && e.Pointer.IsPrimary)
            {
                var point = e.GetPosition(this);
                var position = RelativeXPositionToSeconds(point.X);
                var p = HitTestParagraph(point);
                OnPrimaryDoubleClicked?.Invoke(this, new ParagraphNullableEventArgs(position, p));
            }
        };
        KeyDown += OnKeyDown;
        KeyUp += OnKeyUp;
        LostFocus += (sender, e) => { InvalidateVisual(); };
    }

    public void UpdateTheme()
    {
        _paintTimeText = UiUtil.GetTextColor();
    }

    private void OnKeyUp(object? sender, KeyEventArgs e)
    {
        _isCtrlDown = e.KeyModifiers.HasFlag(KeyModifiers.Control);
        _isShiftDown = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
        _isAltDown = e.KeyModifiers.HasFlag(KeyModifiers.Alt);
        _isMetaDown = e.KeyModifiers.HasFlag(KeyModifiers.Meta);
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        _isCtrlDown = e.KeyModifiers.HasFlag(KeyModifiers.Control);
        _isShiftDown = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
        _isAltDown = e.KeyModifiers.HasFlag(KeyModifiers.Alt);
        _isMetaDown = e.KeyModifiers.HasFlag(KeyModifiers.Meta);

        if (e.Key == Key.Escape)
        {
            _interactionMode = InteractionMode.None;
            NewSelectionParagraph = null;
            InvalidateVisual();
            e.Handled = true;
        }
        else if (e.Key == Key.Enter)
        {
            var newP = NewSelectionParagraph;
            if (newP != null && OnNewSelectionInsert != null)
            {
                OnNewSelectionInsert.Invoke(this, new ParagraphEventArgs(newP));
            }

            _interactionMode = InteractionMode.None;
            NewSelectionParagraph = null;
            InvalidateVisual();
            e.Handled = true;
        }
        else if (e.Key == Key.LeftAlt || e.Key == Key.RightAlt)
        {
            _isAltDown = true;
        }
        else if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
        {
            _isCtrlDown = true;
        }
        else if (e.Key == Key.LWin || e.Key == Key.RWin)
        {
            _isMetaDown = true;
        }
        else if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
        {
            _isShiftDown = true;
        }
        else if (e.Key == Key.Delete && !e.KeyModifiers.HasFlag(KeyModifiers.Control) && !e.KeyModifiers.HasFlag(KeyModifiers.Alt) &&
                 !e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            OnDeletePressed?.Invoke(this, new ParagraphEventArgs(0, _activeParagraph));
        }
    }

    private void OnTapped(object? sender, TappedEventArgs e)
    {
        _isCtrlDown = e.KeyModifiers.HasFlag(KeyModifiers.Control);
        _isShiftDown = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
        _isAltDown = e.KeyModifiers.HasFlag(KeyModifiers.Alt);
        _isMetaDown = e.KeyModifiers.HasFlag(KeyModifiers.Meta);

        if (OperatingSystem.IsMacOS())
        {
            _isCtrlDown = _isMetaDown;
        }

        var point = e.GetPosition(this);
        if (!_isCtrlDown && !_isAltDown && !_isShiftDown && e.Pointer.IsPrimary)
        {
            var p = HitTestParagraph(point);
            var position = RelativeXPositionToSeconds(e.GetPosition(this).X);

            if (OnVideoPositionChanged != null)
            {
                _audioVisualizerLastScroll = 0;
                OnPrimarySingleClicked?.Invoke(this, new ParagraphNullableEventArgs(position, p));
            }

            _activeParagraph = null;
            return;
        }

        if (_isShiftDown && _isCtrlDown && !_isAltDown && e.Pointer.IsPrimary)
        {
            var position = RelativeXPositionToSeconds(e.GetPosition(this).X);
            OnSetStartAndOffsetTheRest?.Invoke(this, new PositionEventArgs { PositionInSeconds = position });
            return;
        }

        if (_isShiftDown && !_isCtrlDown)
        {
            var firstSelected = AllSelectedParagraphs.FirstOrDefault();
            var seconds = RelativeXPositionToSeconds(point.X);
            if (firstSelected != null)
            {
                if (seconds < firstSelected.EndTime.TotalSeconds - 0.01)
                {
                    firstSelected.SetStartTimeOnly(TimeSpan.FromSeconds(seconds));
                }

                e.Handled = true;
                InvalidateVisual();
                return;
            }
        }

        if (_isCtrlDown || (OperatingSystem.IsMacOS() && _isShiftDown))
        {
            var firstSelected = AllSelectedParagraphs.FirstOrDefault();
            var seconds = RelativeXPositionToSeconds(point.X);
            if (firstSelected != null)
            {
                if (seconds > firstSelected.StartTime.TotalSeconds + 0.01)
                {
                    firstSelected.EndTime = TimeSpan.FromSeconds(seconds);
                }

                e.Handled = true;
                InvalidateVisual();
                return;
            }
        }

        if (_isAltDown)
        {
            var firstSelected = AllSelectedParagraphs.FirstOrDefault();
            var seconds = RelativeXPositionToSeconds(point.X);
            if (firstSelected != null)
            {
                firstSelected.StartTime = TimeSpan.FromSeconds(seconds);
                e.Handled = true;
                InvalidateVisual();
                return;
            }
        }
    }

    public const double MinZoomFactor = 0.1;
    public const double MaxZoomFactor = 20.0;

    private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        _lastMouseWheelScroll = Environment.TickCount64;

        var point = e.GetPosition(this);
        var properties = e.GetCurrentPoint(this).Properties;

        if (_isAltDown)
        {
            var deltaAlt = InvertMouseWheel ? -e.Delta.Y : e.Delta.Y;
            var newZoomFactor = ZoomFactor + deltaAlt / 10.0;

            if (newZoomFactor < MinZoomFactor)
            {
                newZoomFactor = MinZoomFactor;
            }

            if (newZoomFactor > MaxZoomFactor)
            {
                newZoomFactor = MaxZoomFactor;
            }

            ZoomFactor = newZoomFactor;

            InvalidateVisual();
            return;
        }

        if (_isShiftDown)
        {
            var deltaShift = e.Delta.Y;
            var newZoomFactor = VerticalZoomFactor + deltaShift / 10.0;

            if (newZoomFactor < MinZoomFactor)
            {
                newZoomFactor = MinZoomFactor;
            }

            if (newZoomFactor > MaxZoomFactor)
            {
                newZoomFactor = MaxZoomFactor;
            }

            VerticalZoomFactor = newZoomFactor;

            InvalidateVisual();
            return;
        }

        var delta = InvertMouseWheel ? -e.Delta.Y : e.Delta.Y;

        e.Handled = true;
        var newStart = StartPositionSeconds + delta / 2;
        if (newStart < 0)
        {
            newStart = 0;
        }

        _audioVisualizerLastScroll = Environment.TickCount64; // Update the last scroll time
        StartPositionSeconds = newStart;
        OnHorizontalScroll?.Invoke(this, new PositionEventArgs { PositionInSeconds = newStart });

        if (_isCtrlDown || _isMetaDown)
        {
            var videoPosition = RelativeXPositionToSeconds(point.X);
            if (Se.Settings.Waveform.CenterVideoPosition && WavePeaks != null)
            {
                var halfWidthInSeconds = (Bounds.Width / 2) / (WavePeaks.SampleRate * ZoomFactor);
                videoPosition = StartPositionSeconds + halfWidthInSeconds;
            }
            OnVideoPositionChanged?.Invoke(this, new PositionEventArgs { PositionInSeconds = videoPosition });
        }

        InvalidateVisual();
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _isCtrlDown = e.KeyModifiers.HasFlag(KeyModifiers.Control);
        _isShiftDown = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
        _isAltDown = e.KeyModifiers.HasFlag(KeyModifiers.Alt);
        _isMetaDown = e.KeyModifiers.HasFlag(KeyModifiers.Meta);

        var nsp = NewSelectionParagraph;
        if (nsp is { Duration.TotalMilliseconds: <= 1 })
        {
            nsp = null;
        }

        var pos = e.GetPosition(this);

        if (nsp == null && e.InitialPressMouseButton == MouseButton.Right && Se.Settings.Waveform.RightClickSelectsSubtitle)
        {
            var p = HitTestParagraph(pos);

            if (_isCtrlDown)
            {
                OnToggleSelection?.Invoke(this, new ParagraphEventArgs(RelativeXPositionToSeconds(pos.X), p));
                return;
            }
            else
            {
                OnSelectRequested?.Invoke(this, new ParagraphEventArgs(RelativeXPositionToSeconds(pos.X), p));
            }

            _interactionMode = InteractionMode.None;
            _activeParagraph = null;
        }

        var showContextMenu = false;
        if (e.InitialPressMouseButton == MouseButton.Left && OperatingSystem.IsMacOS())
        {
            if (_isCtrlDown && !_isShiftDown && !_isAltDown)
            {
                showContextMenu = true;
            }
        }
        else if (e.InitialPressMouseButton == MouseButton.Right)
        {
            showContextMenu = true;
        }

        if (showContextMenu)
        {
            _isAltDown = false;
            _isCtrlDown = false;
            _isShiftDown = false;
            _isMetaDown = false;
            _interactionMode = InteractionMode.None;
            nsp?.UpdateDuration();
            _audioVisualizerLastScroll = 0;
            e.Handled = true;
            var videoPosition = RelativeXPositionToSeconds(pos.X);
            OnVideoPositionChanged?.Invoke(this, new PositionEventArgs { PositionInSeconds = videoPosition });
            FlyoutMenuOpening?.Invoke(this, new ContextEventArgs { PositionInSeconds = videoPosition, NewParagraph = nsp });
            InvalidateVisual();
            MenuFlyout.ShowAt(this, true);
            return;
        }

        if (nsp is { Duration.TotalMilliseconds: > 1 })
        {
            nsp.UpdateDuration();
            if (nsp.Duration.TotalMilliseconds < 10)
            {
                NewSelectionParagraph = null;
            }
            else
            {
                _interactionMode = InteractionMode.None;
                _activeParagraph = null;
                return;
            }
        }

        if (_interactionMode is InteractionMode.None or InteractionMode.New)
        {
            // if (OnVideoPositionChanged != null)
            // {
            //     var videoPosition = RelativeXPositionToSeconds(pos.X);
            //     _audioVisualizerLastScroll = 0;
            //     OnVideoPositionChanged.Invoke(this, new PositionEventArgs { PositionInSeconds = videoPosition });
            // }

            _activeParagraph = null;
            return;
        }

        if (_interactionMode == InteractionMode.Moving)
        {
            // click on paragraph, but with no move
            var ms = Environment.TickCount64 - _lastPointerPressed;
            if (ms < 100 ||
                (
                    _activeParagraph != null &&
                    Math.Abs(_originalStartSeconds - _activeParagraph.StartTime.TotalSeconds) < 0.01))
            {
                if (_isCtrlDown && !OperatingSystem.IsMacOS() && _activeParagraph != null && OnToggleSelection != null)
                {
                    var videoPosition = RelativeXPositionToSeconds(pos.X);
                    _audioVisualizerLastScroll = 0;
                    OnToggleSelection.Invoke(this, new ParagraphEventArgs(videoPosition, _activeParagraph));
                }
                else if (_isShiftDown && OperatingSystem.IsMacOS() && _activeParagraph != null && OnToggleSelection != null)
                {
                    var videoPosition = RelativeXPositionToSeconds(pos.X);
                    _audioVisualizerLastScroll = 0;
                    OnToggleSelection.Invoke(this, new ParagraphEventArgs(videoPosition, _activeParagraph));
                }
                else if (OnVideoPositionChanged != null)
                {
                    var videoPosition = RelativeXPositionToSeconds(e.GetPosition(this).X);
                    _audioVisualizerLastScroll = 0;
                    OnVideoPositionChanged.Invoke(this, new PositionEventArgs { PositionInSeconds = videoPosition });
                }

                _activeParagraph = null;
                return;
            }
        }
        else if ((_interactionMode == InteractionMode.ResizingLeft || _interactionMode == InteractionMode.ResizingRight) && _activeParagraph != null &&
                 _activeParagraph.Duration.TotalMilliseconds < 10)
        {
            if (OnVideoPositionChanged != null)
            {
                var videoPosition = RelativeXPositionToSeconds(e.GetPosition(this).X);
                _audioVisualizerLastScroll = 0;
                OnVideoPositionChanged.Invoke(this, new PositionEventArgs { PositionInSeconds = videoPosition });
            }

            _interactionMode = InteractionMode.None;
            _activeParagraph = null;
            return;
        }

        _interactionMode = InteractionMode.None;
        _activeParagraph = null;

        InvalidateVisual();
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _isCtrlDown = e.KeyModifiers.HasFlag(KeyModifiers.Control);
        _isShiftDown = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
        _isAltDown = e.KeyModifiers.HasFlag(KeyModifiers.Alt);
        _isMetaDown = e.KeyModifiers.HasFlag(KeyModifiers.Meta);

        _lastPointerPressed = Environment.TickCount64;
        e.Handled = true;
        var point = e.GetPosition(this);
        _startPointerPosition = point;
        if (IsReadOnly)
        {
            InvalidateVisual();
            return;
        }

        var p = _cachedHitParagraph;
        if (p == null)
        {
            _interactionMode = InteractionMode.New;
            NewSelectionParagraph = new SubtitleLineViewModel();

            var deltaX = point.X; // - _startPointerPosition.X;
            var deltaSeconds = RelativeXPositionToSeconds(deltaX);
            _newSelectionSeconds = deltaSeconds;

            NewSelectionParagraph.StartTime = TimeSpan.FromSeconds(deltaSeconds);
            NewSelectionParagraph.EndTime = TimeSpan.FromSeconds(deltaSeconds);
            InvalidateVisual();
            return;
        }

        if (NewSelectionParagraph != null && NewSelectionParagraph != p)
        {
            NewSelectionParagraph = null;
        }

        _activeParagraph = p;
        _originalStartSeconds = p.StartTime.TotalSeconds;
        _originalEndSeconds = p.EndTime.TotalSeconds;
        _originalDurationSeconds = p.Duration.TotalSeconds;

        var displayableParagraphs = _displayableParagraphs;
        if (displayableParagraphs.Count == 0)
        {
            return;
        }

        // Determine which edge we're actually near based on distance
        double leftEdgePos = SecondsToXPosition(p.StartTime.TotalSeconds - StartPositionSeconds);
        double rightEdgePos = SecondsToXPosition(p.EndTime.TotalSeconds - StartPositionSeconds);
        double distToLeft = Math.Abs(point.X - leftEdgePos);
        double distToRight = Math.Abs(point.X - rightEdgePos);

        bool isActuallyNearLeft = _cachedIsNearLeftEdge && (!_cachedIsNearRightEdge || distToLeft <= distToRight);
        bool isActuallyNearRight = _cachedIsNearRightEdge && (!_cachedIsNearLeftEdge || distToRight < distToLeft);

        if (isActuallyNearLeft)
        {
            _interactionMode = InteractionMode.ResizingLeft;

            var idx = displayableParagraphs.IndexOf(p);
            if (idx > 0)
            {
                var prevParagraph = displayableParagraphs[idx - 1];
                double prevRightPos = SecondsToXPosition(prevParagraph.EndTime.TotalSeconds - StartPositionSeconds);

                // Only consider "Or" mode if the previous paragraph's right edge is very close
                if (Math.Abs(point.X - prevRightPos) <= ResizeMargin)
                {
                    _activeParagraphPrevious = prevParagraph;
                    _interactionMode = InteractionMode.ResizingLeftOr;
                }
            }

            if (_isAltDown && idx > 0)
            {
                var p2 = HitTestParagraph(point, displayableParagraphs, idx - 1, 100);
                if (p2 != null)
                {
                    _activeParagraphPrevious = p2;
                    _originalPreviousEndSeconds = p2.EndTime.TotalSeconds;
                    _interactionMode = InteractionMode.ResizeLeftAnd;
                }
            }
        }
        else if (isActuallyNearRight)
        {
            _interactionMode = InteractionMode.ResizingRight;

            var idx = displayableParagraphs.IndexOf(p);
            if (idx < displayableParagraphs.Count - 1)
            {
                var nextParagraph = displayableParagraphs[idx + 1];
                double nextLeftPos = SecondsToXPosition(nextParagraph.StartTime.TotalSeconds - StartPositionSeconds);

                // Only consider "Or" mode if the next paragraph's left edge is very close
                if (Math.Abs(point.X - nextLeftPos) <= ResizeMargin)
                {
                    _activeParagraphNext = nextParagraph;
                    _interactionMode = InteractionMode.ResizingRightOr;
                }
            }

            if (_isAltDown && idx < displayableParagraphs.Count - 1)
            {
                var p2 = HitTestParagraphRight(point, displayableParagraphs, idx + 1, 100);
                if (p2 != null)
                {
                    _activeParagraphNext = p2;
                    _originalNextStartSeconds = p2.StartTime.TotalSeconds;
                    _interactionMode = InteractionMode.ResizeRightAnd;
                }
            }
        }
        else
        {
            // Not near an edge, so it's a move operation
            if (_isCtrlDown || _isAltDown)
            {
                _interactionMode = InteractionMode.None;
                return;
            }

            _interactionMode = InteractionMode.Moving;
        }
    }

    private void OnPointerExited(object? sender, PointerEventArgs e)
    {
        base.OnPointerExited(e);
        InvalidateVisual();
    }

    public bool SkipNextPointerEntered { get; set; }

    private void OnPointerEntered(object? sender, PointerEventArgs e)
    {
        base.OnPointerEntered(e);

        if (!FocusOnMouseOver)
        {
            return;
        }

        if (!IsFocused)
        {
            if (SkipNextPointerEntered)
            {
                SkipNextPointerEntered = false;
                return;
            }

            Focus();
        }

        InvalidateVisual();
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (IsReadOnly)
        {
            return;
        }

        var point = e.GetPosition(this);
        var properties = e.GetCurrentPoint(this).Properties;
        var newP = NewSelectionParagraph;
        if (_interactionMode == InteractionMode.New && newP != null && properties.IsLeftButtonPressed)
        {
            var seconds = RelativeXPositionToSeconds(point.X);
            if (seconds > _newSelectionSeconds)
            {
                newP.StartTime = TimeSpan.FromSeconds(_newSelectionSeconds);
                newP.EndTime = TimeSpan.FromSeconds(seconds);
            }
            else
            {
                newP.StartTime = TimeSpan.FromSeconds(seconds);
                newP.EndTime = TimeSpan.FromSeconds(_newSelectionSeconds);
            }

            InvalidateVisual();
            return;
        }

        if (_interactionMode == InteractionMode.None || _activeParagraph == null)
        {
            UpdateCursor(point);
            return;
        }

        var deltaX = point.X - _startPointerPosition.X;
        var deltaSeconds = RelativeXPositionToSeconds(deltaX);

        if (_interactionMode == InteractionMode.ResizingLeftOr && _activeParagraphPrevious != null)
        {
            if (Math.Abs(deltaX) < 3)
            {
                return;
            }

            if (_startPointerPosition.X < point.X)
            {
                _interactionMode = InteractionMode.ResizingLeft;
            }
            else
            {
                _activeParagraph = _activeParagraphPrevious;
                _originalStartSeconds = _activeParagraph.StartTime.TotalSeconds;
                _originalEndSeconds = _activeParagraph.EndTime.TotalSeconds;
                _interactionMode = InteractionMode.ResizingRight;
            }
        }
        else if (_interactionMode == InteractionMode.ResizingRightOr && _activeParagraphNext != null)
        {
            if (Math.Abs(deltaX) < 3)
            {
                return;
            }

            if (_startPointerPosition.X > point.X)
            {
                _interactionMode = InteractionMode.ResizingRight;
            }
            else
            {
                _activeParagraph = _activeParagraphNext;
                _originalStartSeconds = _activeParagraph.StartTime.TotalSeconds;
                _originalEndSeconds = _activeParagraph.EndTime.TotalSeconds;
                _interactionMode = InteractionMode.ResizingLeft;
            }
        }

        var newStart = _originalStartSeconds;
        var newEnd = _originalEndSeconds;

        var currentIndex = _displayableParagraphs.IndexOf(_activeParagraph);
        var previous = currentIndex > 0 ? _displayableParagraphs[currentIndex - 1] : null;
        var next = currentIndex < _displayableParagraphs.Count - 1 ? _displayableParagraphs[currentIndex + 1] : null;

        if (_isShiftDown || Se.Settings.Waveform.AllowOverlap)
        {
            previous = null;
            next = null;
        }

        if (NewSelectionParagraph == _activeParagraph)
        {
            previous = _displayableParagraphs.LastOrDefault(p => p.StartTime < _activeParagraph.StartTime);
            next = _displayableParagraphs.FirstOrDefault(p => p.StartTime > _activeParagraph.EndTime);
        }

        switch (_interactionMode)
        {
            case InteractionMode.Moving:
                newStart = _originalStartSeconds + deltaSeconds - StartPositionSeconds;
                newEnd = _originalEndSeconds + deltaSeconds - StartPositionSeconds;

                // Check if the paragraph already overlaps with neighbors
                bool alreadyOverlapping = false;
                if (_activeParagraph != null && currentIndex >= 0)
                {
                    var prevParagraph = currentIndex > 0 ? _displayableParagraphs[currentIndex - 1] : null;
                    var nextParagraph = currentIndex < _displayableParagraphs.Count - 1 ? _displayableParagraphs[currentIndex + 1] : null;

                    bool alreadyOverlapsPrevious = prevParagraph != null && _originalStartSeconds < prevParagraph.EndTime.TotalSeconds;
                    bool alreadyOverlapsNext = nextParagraph != null && _originalEndSeconds > nextParagraph.StartTime.TotalSeconds;
                    alreadyOverlapping = alreadyOverlapsPrevious || alreadyOverlapsNext;
                }

                // Allow overlap if shift key, setting, or already overlapping
                // (previous and next are already null if _isShiftDown or Se.Settings.Waveform.AllowOverlap)
                bool allowOverlap = (previous == null && next == null) || alreadyOverlapping;

                if (!allowOverlap && (previous != null || next != null))
                {
                    // Calculate available space considering MinGapSeconds
                    double availableStart = previous != null ? previous.EndTime.TotalSeconds + MinGapSeconds : 0;
                    double availableEnd = next != null ? next.StartTime.TotalSeconds - MinGapSeconds : double.MaxValue;
                    double availableSpace = availableEnd - availableStart;

                    // Check if there's enough room to move the paragraph
                    if (availableSpace < _originalDurationSeconds)
                    {
                        // Not enough space to move, keep original position
                        break;
                    }

                    // Clamp to available space to prevent overlap
                    if (newStart < availableStart)
                    {
                        newStart = availableStart;
                    }

                    if (newEnd > availableEnd)
                    {
                        newStart = availableEnd - _originalDurationSeconds;
                    }
                }

                // Ensure start time is non-negative
                if (newStart < 0)
                {
                    newStart = 0;
                }

                if (_activeParagraph != null)
                {
                    _activeParagraph.StartTime = TimeSpan.FromSeconds(newStart);
                    _activeParagraph.EndTime = TimeSpan.FromSeconds(newStart + _originalDurationSeconds);
                }
                break;
            case InteractionMode.ResizeLeftAnd:
                newStart = _originalStartSeconds + deltaSeconds - StartPositionSeconds;
                var newPrevEnd = _originalPreviousEndSeconds + deltaSeconds - StartPositionSeconds;
                if (_activeParagraphPrevious != null)
                {
                    _activeParagraph.SetStartTimeOnly(TimeSpan.FromSeconds(newStart));
                    _activeParagraphPrevious.EndTime = TimeSpan.FromSeconds(newPrevEnd);
                }

                break;
            case InteractionMode.ResizeRightAnd:
                newEnd = _originalEndSeconds + deltaSeconds - StartPositionSeconds;
                var newNextStart = _originalNextStartSeconds + deltaSeconds - StartPositionSeconds;
                if (_activeParagraphNext != null)
                {
                    _activeParagraph.EndTime = TimeSpan.FromSeconds(newEnd);
                    _activeParagraphNext.SetStartTimeOnly(TimeSpan.FromSeconds(newNextStart));
                }

                break;
            case InteractionMode.ResizingLeft:
                newStart = _originalStartSeconds + deltaSeconds - StartPositionSeconds;

                if (newStart < 0)
                {
                    newStart = 0;
                }

                if (SnapToShotChanges)
                {
                    var nearestShotChange = ShotChangesHelper.GetClosestShotChange(_shotChanges, TimeCode.FromSeconds(newStart));
                    if (nearestShotChange != null)
                    {
                        var nearest = (double)nearestShotChange;
                        if (nearest != newStart && Math.Abs(newStart - nearest) < ShotChangeSnapSeconds)
                        {
                            newStart = nearest;
                        }
                    }
                }

                if (previous != null && newStart < previous.EndTime.TotalSeconds + MinGapSeconds)
                {
                    newStart = previous.EndTime.TotalSeconds + MinGapSeconds + 0.001;
                }

                if (newStart < _activeParagraph.EndTime.TotalSeconds - 0.1)
                {
                    _activeParagraph.SetStartTimeOnly(TimeSpan.FromSeconds(newStart));
                }

                break;
            case InteractionMode.ResizingRight:
                newEnd = _originalEndSeconds + deltaSeconds - StartPositionSeconds;

                if (SnapToShotChanges)
                {
                    var oneFrameSeconds = 0; // Or should it be one frame before, like  0.042 (Get fps from video)
                    var nearestShotChange = ShotChangesHelper.GetClosestShotChange(_shotChanges, TimeCode.FromSeconds(newEnd));
                    if (nearestShotChange != null)
                    {
                        var nearest = (double)nearestShotChange;
                        if (nearest != newEnd && Math.Abs(newEnd - nearest + oneFrameSeconds) < ShotChangeSnapSeconds)
                        {
                            newEnd = nearest - oneFrameSeconds;
                        }
                    }
                }

                if (next != null && newEnd > next.StartTime.TotalSeconds - MinGapSeconds)
                {
                    newEnd = next.StartTime.TotalSeconds - 0.001 - MinGapSeconds;
                }

                if (newEnd > _activeParagraph.StartTime.TotalSeconds + 0.1)
                {
                    _activeParagraph.EndTime = TimeSpan.FromSeconds(newEnd);
                }

                break;
        }

        InvalidateVisual();
    }

    private void UpdateCursor(Point point)
    {
        var p = HitTestParagraph(point);
        _cachedHitParagraph = p;
        _cachedIsNearLeftEdge = false;
        _cachedIsNearRightEdge = false;

        if (p != null)
        {
            double left = SecondsToXPosition(p.StartTime.TotalSeconds - StartPositionSeconds);
            double right = SecondsToXPosition(p.EndTime.TotalSeconds - StartPositionSeconds);
            var distToLeft = Math.Abs(point.X - left);
            var distToRight = Math.Abs(point.X - right);

            _cachedIsNearLeftEdge = distToLeft <= ResizeMargin;
            _cachedIsNearRightEdge = distToRight <= ResizeMargin;

            if ((_cachedIsNearLeftEdge && distToLeft <= distToRight) || (_cachedIsNearRightEdge && distToRight < distToLeft))
            {
                if (p == NewSelectionParagraph && NewSelectionParagraph?.Duration.TotalMilliseconds < 10)
                {
                    Cursor = new Cursor(StandardCursorType.Arrow);
                }
                else
                {
                    Cursor = new Cursor(StandardCursorType.SizeWestEast);
                }
            }
            else
            {
                Cursor = new Cursor(StandardCursorType.Hand);
            }
        }
        else
        {
            Cursor = new Cursor(StandardCursorType.Arrow);
        }
    }

    private SubtitleLineViewModel? HitTestParagraph(Point point)
    {
        var pointX = point.X;
        var startPosSeconds = StartPositionSeconds;

        // Check NewSelectionParagraph first as it's typically the active interaction target
        var newSelection = NewSelectionParagraph;
        if (newSelection != null)
        {
            var left = SecondsToXPosition(newSelection.StartTime.TotalSeconds - startPosSeconds);
            var right = SecondsToXPosition(newSelection.EndTime.TotalSeconds - startPosSeconds);

            if (pointX >= left - ResizeMargin && pointX <= right + ResizeMargin)
            {
                return newSelection;
            }
        }

        // Early exit if no displayable paragraphs
        if (_displayableParagraphs.Count == 0)
        {
            return null;
        }

        // Single pass: Find closest edge or middle paragraph
        SubtitleLineViewModel? closestEdgeParagraph = null;
        SubtitleLineViewModel? middleParagraph = null;
        double closestEdgeDistance = double.MaxValue;
        bool isClosestEdgeLeft = false;
        int closestEdgeIndex = -1;

        for (var i = 0; i < _displayableParagraphs.Count; i++)
        {
            var p = _displayableParagraphs[i];
            var left = SecondsToXPosition(p.StartTime.TotalSeconds - startPosSeconds);
            var right = SecondsToXPosition(p.EndTime.TotalSeconds - startPosSeconds);

            // Check if in middle (not near edges)
            if (pointX >= left + ResizeMargin && pointX <= right - ResizeMargin)
            {
                middleParagraph = p;
                // Don't break - continue checking for closer edges
            }

            // Check left edge distance
            var distToLeft = Math.Abs(pointX - left);
            if (distToLeft <= ResizeMargin && distToLeft < closestEdgeDistance)
            {
                closestEdgeDistance = distToLeft;
                closestEdgeParagraph = p;
                closestEdgeIndex = i;
                isClosestEdgeLeft = true;
            }

            // Check right edge distance
            var distToRight = Math.Abs(pointX - right);
            if (distToRight <= ResizeMargin && distToRight < closestEdgeDistance)
            {
                closestEdgeDistance = distToRight;
                closestEdgeParagraph = p;
                closestEdgeIndex = i;
                isClosestEdgeLeft = false;
            }
        }

        // If we found an edge, check for adjacent paragraphs that might be closer
        if (closestEdgeParagraph != null)
        {
            if (isClosestEdgeLeft && closestEdgeIndex > 0)
            {
                // Check if previous paragraph's right edge is closer
                var prev = _displayableParagraphs[closestEdgeIndex - 1];
                var prevRight = SecondsToXPosition(prev.EndTime.TotalSeconds - startPosSeconds);
                var distToPrevRight = Math.Abs(pointX - prevRight);

                if (distToPrevRight <= ResizeMargin && distToPrevRight < closestEdgeDistance)
                {
                    return prev;
                }
            }
            else if (!isClosestEdgeLeft && closestEdgeIndex < _displayableParagraphs.Count - 1)
            {
                // Check if next paragraph's left edge is closer
                var next = _displayableParagraphs[closestEdgeIndex + 1];
                var nextLeft = SecondsToXPosition(next.StartTime.TotalSeconds - startPosSeconds);
                var distToNextLeft = Math.Abs(pointX - nextLeft);

                if (distToNextLeft <= ResizeMargin && distToNextLeft < closestEdgeDistance)
                {
                    return next;
                }
            }

            return closestEdgeParagraph;
        }

        // Return middle paragraph if found (no edges were close)
        return middleParagraph;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SubtitleLineViewModel? HitTestParagraph(Point point, List<SubtitleLineViewModel> subtitles, int index, double resizeMargin)
    {
        if (index < 0 || index >= subtitles.Count)
        {
            return null;
        }

        var p = subtitles[index];
        var left = SecondsToXPosition(p.StartTime.TotalSeconds - StartPositionSeconds);
        var right = SecondsToXPosition(p.EndTime.TotalSeconds - StartPositionSeconds);
        var pointX = point.X;

        return pointX >= left - ResizeMargin && pointX <= right + resizeMargin ? p : null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SubtitleLineViewModel? HitTestParagraphRight(Point point, List<SubtitleLineViewModel> subtitles, int index, double resizeMargin)
    {
        if (index < 0 || index >= subtitles.Count)
        {
            return null;
        }

        var p = subtitles[index];
        var leftEdge = SecondsToXPosition(p.StartTime.TotalSeconds - StartPositionSeconds);
        var pointX = point.X;

        return pointX >= leftEdge - resizeMargin && pointX <= leftEdge + resizeMargin ? p : null;
    }

    //Queue<double> _renderTimes = new Queue<double>();

    // Cached render values to avoid repeated calculations
    private struct RenderContext
    {
        public double Width;
        public double Height;
        public double StartPositionSeconds;
        public double ZoomFactor;
        public double VerticalZoomFactor;
        public double CurrentVideoPositionSeconds;
        public int SampleRate;
        public double HighestPeak;
        public Rect BoundsRect;

        public double WaveformHeight { get; internal set; }
        public double SpectrogramHeight { get; internal set; }
    }

    public override void Render(DrawingContext context)
    {
        // Early exit if no valid bounds
        var width = Bounds.Width;
        var height = Bounds.Height;
        if (width <= 0 || height <= 0)
        {
            return;
        }

        var boundsRect = new Rect(0, 0, width, height);
        context.DrawRectangle(_paintBackground, null, new Rect(Bounds.Size));

        //var ticks = DateTime.UtcNow.Ticks;

        // Pre-calculate commonly used values
        var waveformHeight = height * (WaveformHeightPercentage / 100.0);
        var renderCtx = new RenderContext
        {
            Width = width,
            Height = height,
            StartPositionSeconds = StartPositionSeconds,
            ZoomFactor = ZoomFactor,
            VerticalZoomFactor = VerticalZoomFactor,
            CurrentVideoPositionSeconds = CurrentVideoPositionSeconds,
            SampleRate = WavePeaks?.SampleRate ?? 0,
            HighestPeak = WavePeaks?.HighestPeak ?? 1.0,
            BoundsRect = boundsRect,
            WaveformHeight = waveformHeight,
            SpectrogramHeight = height - waveformHeight,
        };

        using (context.PushClip(boundsRect))
        {
            DrawAllGridLines(context, ref renderCtx);
            DrawWaveForm(context, ref renderCtx);
            DrawSpectrogram(context, ref renderCtx);
            DrawTimeLine(context, ref renderCtx);
            DrawParagraphs(context, ref renderCtx);
            DrawShotChanges(context, ref renderCtx);
            DrawCurrentVideoPosition(context, ref renderCtx);
            DrawNewParagraph(context, ref renderCtx);

            if (IsFocused)
            {
                context.DrawRectangle(null, _paintPenSelected, boundsRect);
            }
        }

        //var timeSpan = TimeSpan.FromTicks(DateTime.UtcNow.Ticks - ticks);
        //_renderTimes.Enqueue(timeSpan.TotalMilliseconds);
        //if (_renderTimes.Count > 100)
        //{
        //    _renderTimes.Dequeue();
        //}
        //var mean = _renderTimes.Average();
        //OnStatus?.Invoke(this,
        //    new ParagraphEventArgs(0, new SubtitleLineViewModel()
        //    {
        //        Text = $"Render time: {mean:0.00} ms"
        //    }, new SubtitleLineViewModel()));
    }

    private void DrawSpectrogram(DrawingContext context, ref RenderContext renderCtx)
    {
        if (_spectrogram == null || _displayMode == WaveformDisplayMode.OnlyWaveform)
        {
            return;
        }

        var height = renderCtx.Height;
        if (_displayMode == WaveformDisplayMode.WaveformAndSpectrogram)
        {
            height = renderCtx.SpectrogramHeight;
        }

        var endPositionSeconds = RelativeXPositionToSecondsOptimized(renderCtx.Width, renderCtx.SampleRate, renderCtx.StartPositionSeconds, renderCtx.ZoomFactor);
        var width = (int)Math.Round((endPositionSeconds - renderCtx.StartPositionSeconds) / _spectrogram.SampleDuration);
        if (width <= 0 || width > 4000)
        {
            return;
        }

        // Create a combined bitmap using SkiaSharp
        using var skBitmapCombined = new SKBitmap(width, _spectrogram.FftSize / 2);
        using var skCanvas = new SKCanvas(skBitmapCombined);

        var left = (int)Math.Round(StartPositionSeconds / _spectrogram.SampleDuration);
        var offset = 0;
        var imageIndex = left / _spectrogram.ImageWidth;

        while (offset < width && imageIndex < _spectrogram.Images.Count)
        {
            var x = (left + offset) % _spectrogram.ImageWidth;
            var w = Math.Min(_spectrogram.ImageWidth - x, width - offset);

            // Draw part of the spectrogram image
            var sourceRect = new SKRect(x, 0, x + w, skBitmapCombined.Height);
            var destRect = new SKRect(offset, 0, offset + w, skBitmapCombined.Height);
            skCanvas.DrawBitmap(_spectrogram.Images[imageIndex], sourceRect, destRect);

            offset += w;
            imageIndex++;
        }

        // Convert SKBitmap to Avalonia Bitmap and draw it
        var displayHeight = height;
        var avaloniaBitmap = skBitmapCombined.ToAvaloniaBitmap();

        var destRectangle = new Rect(0, renderCtx.Height - displayHeight, renderCtx.Width, displayHeight);
        context.DrawImage(avaloniaBitmap, destRectangle);
    }

    private void DrawTimeLine(DrawingContext context, ref RenderContext renderCtx)
    {
        if (renderCtx.SampleRate == 0)
        {
            return;
        }

        var seconds = Math.Ceiling(renderCtx.StartPositionSeconds) - renderCtx.StartPositionSeconds;
        var position = SecondsToXPositionOptimized(seconds, renderCtx.SampleRate, renderCtx.ZoomFactor);
        var imageHeight = renderCtx.Height;

        var pen = _paintTimeLine;
        var textBrush = _paintTimeText;

        while (position < renderCtx.Width)
        {
            var n = renderCtx.ZoomFactor * renderCtx.SampleRate;

            if (n > 38 || (int)Math.Round(renderCtx.StartPositionSeconds + seconds) % 5 == 0)
            {
                // Draw major tick lines (seconds)
                context.DrawLine(pen, new Point(position, imageHeight), new Point(position, imageHeight - 10));

                // Draw time text 
                var timeText = GetDisplayTime(renderCtx.StartPositionSeconds + seconds);
                var formattedText = new FormattedText(
                    timeText,
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    Typeface.Default, // Use default typeface instead of custom one
                    12, // Increased font size for better visibility
                    textBrush);

                // Try different Y positions - adjust these based on your control height
                var textY = Math.Max(0, imageHeight - formattedText.Height - 2); // Ensure text is within bounds
                context.DrawText(formattedText, new Point(position + 2, textY));
            }

            seconds += 0.5;
            position = SecondsToXPositionOptimized(seconds, renderCtx.SampleRate, renderCtx.ZoomFactor);

            if (n > 64)
            {
                // Draw minor tick line
                context.DrawLine(pen, new Point(position, imageHeight), new Point(position, imageHeight - 5));
            }

            seconds += 0.5;
            position = SecondsToXPositionOptimized(seconds, renderCtx.SampleRate, renderCtx.ZoomFactor);
        }
    }

    private readonly Pen _paintTimeLine = new Pen(Brushes.Gray, 1);
    private IBrush _paintTimeText = UiUtil.GetTextColor();

    private static string GetDisplayTime(double seconds)
    {
        if (Se.Settings.General.CurrentVideoOffsetInMs > 0.00001)
        {
            seconds = seconds + Se.Settings.General.CurrentVideoOffsetInMs / 1000.0;
        }

        var secs = (int)Math.Round(seconds, MidpointRounding.AwayFromZero);
        if (secs < 60)
        {
            return secs.ToString(CultureInfo.InvariantCulture);
        }

        var timeSpan = TimeSpan.FromSeconds(seconds);
        if (timeSpan.TotalHours >= 1)
        {
            return timeSpan.ToString(@"h\:mm\:ss");
        }

        return timeSpan.ToString(@"m\:ss");
    }

    public double EndPositionSeconds
    {
        get
        {
            if (WavePeaks == null)
            {
                return 0;
            }

            return RelativeXPositionToSeconds(Bounds.Width);
        }
    }

    public MenuFlyout MenuFlyout { get; set; }

    private void DrawAllGridLines(DrawingContext context, ref RenderContext renderCtx)
    {
        if (!DrawGridLines)
        {
            return;
        }

        var width = renderCtx.Width;
        var height = renderCtx.Height;

        if (renderCtx.SampleRate == 0)
        {
            for (var i = 0; i < width; i += 10)
            {
                context.DrawLine(_paintGridLines, new Point(i, 0), new Point(i, height));
                context.DrawLine(_paintGridLines, new Point(0, i), new Point(width, i));
            }
        }
        else
        {
            var seconds = Math.Ceiling(renderCtx.StartPositionSeconds) - renderCtx.StartPositionSeconds - 1;
            var xPosition = SecondsToXPositionOptimized(seconds, renderCtx.SampleRate, renderCtx.ZoomFactor);
            var yPosition = 0;
            var yCounter = 0d;
            var interval = renderCtx.ZoomFactor >= 0.4d
                ? 0.1d
                : // a pixel is 0.1 second
                1.0d; // a pixel is 1.0 second

            while (xPosition < width)
            {
                context.DrawLine(_paintGridLines, new Point(xPosition, 0), new Point(xPosition, height));
                seconds += interval;
                xPosition = SecondsToXPositionOptimized(seconds, renderCtx.SampleRate, renderCtx.ZoomFactor);
            }

            while (yPosition < height)
            {
                context.DrawLine(_paintGridLines, new Point(0, yPosition), new Point(width, yPosition));
                yCounter += interval;
                yPosition = Convert.ToInt32(yCounter * renderCtx.SampleRate * renderCtx.ZoomFactor);
            }
        }
    }

    private void DrawWaveForm(DrawingContext context, ref RenderContext renderCtx)
    {
        if (WavePeaks == null || _displayMode == WaveformDisplayMode.OnlySpectrogram)
        {
            return;
        }

        var waveformHeight = renderCtx.Height;
        if (_displayMode == WaveformDisplayMode.WaveformAndSpectrogram)
        {
            waveformHeight = renderCtx.WaveformHeight;
        }

        if (WaveformDrawStyle == WaveformDrawStyle.Classic)
        {
            DrawWaveFormClassic(context, waveformHeight, ref renderCtx);
        }
        else
        {
            DrawWaveFormFancy(context, waveformHeight, ref renderCtx);
        }
    }

    private Pen GetCachedFancyWaveformPen(int colorKey, Color color)
    {
        if (!_fancyWaveformPenCache.TryGetValue(colorKey, out var pen))
        {
            var gradient = GetCachedFancyWaveformGradient(colorKey, color);
            pen = new Pen(gradient, 1.5);
            _fancyWaveformPenCache[colorKey] = pen;
        }

        return pen;
    }

    private LinearGradientBrush GetCachedFancyWaveformGradient(int colorKey, Color color)
    {
        if (!_fancyWaveformGradientCache.TryGetValue(colorKey, out var gradient))
        {
            gradient = new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
                GradientStops = new GradientStops
                {
                    new GradientStop(Color.FromArgb((byte)(color.A * 0.3), color.R, color.G, color.B), 0.0),
                    new GradientStop(color, 0.5),
                    new GradientStop(Color.FromArgb((byte)(color.A * 0.3), color.R, color.G, color.B), 1.0),
                }
            };
            _fancyWaveformGradientCache[colorKey] = gradient;
        }

        return gradient;
    }

    private Pen GetCachedFancyWaveformGlowPen(int colorKey, Color color, double width)
    {
        if (!_fancyWaveformGlowPenCache.TryGetValue(colorKey, out var pen))
        {
            pen = new Pen(new SolidColorBrush(color), width);
            _fancyWaveformGlowPenCache[colorKey] = pen;
        }

        return pen;
    }

    private void DrawWaveFormFancy(DrawingContext context, double waveformHeight, ref RenderContext renderCtx)
    {
        var isSelectedHelper = new IsSelectedHelper(AllSelectedParagraphs, renderCtx.SampleRate);
        var halfWaveformHeight = waveformHeight / 2;
        var div = renderCtx.SampleRate * renderCtx.ZoomFactor;

        if (div <= 0 || WavePeaks == null)
        {
            return;
        }

        var peaks = WavePeaks.Peaks;
        var peaksCount = peaks.Count;
        var highestPeak = renderCtx.HighestPeak;

        // Draw center line first
        var centerLinePen = new Pen(Brushes.DarkGray, 0.5);
        context.DrawLine(centerLinePen, new Point(0, halfWaveformHeight), new Point(renderCtx.Width, halfWaveformHeight));

        // Calculate the threshold for color transitions (as a fraction of the highest peak)
        var lowThreshold = highestPeak * 0.3;
        var mediumThreshold = highestPeak * 0.6;

        for (var x = 0; x < renderCtx.Width; x++)
        {
            var pos = (renderCtx.StartPositionSeconds + x / div) * renderCtx.SampleRate;
            var pos0 = (int)pos;
            var pos1 = pos0 + 1;

            if (pos1 >= peaksCount || pos0 > peaksCount)
            {
                break;
            }

            var pos1Weight = pos - pos0;
            var pos0Weight = 1.0 - pos1Weight;
            var peak0 = peaks[pos0];
            var peak1 = peaks[pos1];
            var max = peak0.Max * pos0Weight + peak1.Max * pos1Weight;
            var min = peak0.Min * pos0Weight + peak1.Min * pos1Weight;

            var yMax = CalculateYOptimized(max, halfWaveformHeight, renderCtx.HighestPeak, renderCtx.VerticalZoomFactor, renderCtx.Height);
            var yMin = CalculateYOptimized(min, halfWaveformHeight, renderCtx.HighestPeak, renderCtx.VerticalZoomFactor, renderCtx.Height);

            if (yMin < yMax)
            {
                (yMin, yMax) = (yMax, yMin);
            }

            // Make sure there's at least a 1 pixel line
            if (Math.Abs(yMax - yMin) < 1)
            {
                yMin = yMax + 1;
            }

            var isSelected = isSelectedHelper.IsSelected(pos0);

            // Calculate amplitude for color determination
            var amplitude = Math.Abs(max) > Math.Abs(min) ? Math.Abs(max) : Math.Abs(min);

            // Determine color based on amplitude and create a cache key
            Color color;
            int colorKey;

            // Determine base color (selected or normal)
            var baseColor = isSelected ? WaveformSelectedColor : WaveformColor;
            var baseColorKeyOffset = isSelected ? 10000 : 0; // Use different cache key range for selected

            // Dynamic coloring based on amplitude - quantize to reduce cache variations
            if (amplitude < lowThreshold)
            {
                // Low amplitude - use base color
                color = baseColor;
                colorKey = baseColorKeyOffset + 0;
            }
            else if (amplitude < mediumThreshold)
            {
                // Medium amplitude - blend from base to high color
                var blend = (amplitude - lowThreshold) / (mediumThreshold - lowThreshold);
                var lowColor = baseColor;
                var highColor = WaveformFancyHighColor;
                var r = (byte)(lowColor.R + blend * (highColor.R - lowColor.R));
                var g = (byte)(lowColor.G + blend * (highColor.G - lowColor.G));
                var b = (byte)(lowColor.B + blend * (highColor.B - lowColor.B));
                var a = (byte)(lowColor.A + blend * (highColor.A - lowColor.A));
                color = Color.FromArgb(a, r, g, b);
                // Quantize blend to 10 steps for caching
                colorKey = baseColorKeyOffset + 1 + (int)(blend * 10);
            }
            else
            {
                // High amplitude - use high color with increased opacity
                var blend = Math.Min(1.0, (amplitude - mediumThreshold) / (highestPeak - mediumThreshold));
                var highColor = WaveformFancyHighColor;
                var a = (byte)Math.Min(255, highColor.A + blend * (255 - highColor.A));
                color = Color.FromArgb(a, highColor.R, highColor.G, highColor.B);
                // Quantize blend to 10 steps for caching
                colorKey = baseColorKeyOffset + 12 + (int)(blend * 10);
            }

            // Get cached pen with gradient
            var pen = GetCachedFancyWaveformPen(colorKey, color);
            context.DrawLine(pen, new Point(x, yMax), new Point(x, yMin));

            // Add subtle glow for higher amplitudes
            if (amplitude > mediumThreshold)
            {
                var glowAlpha = (byte)(50 * ((amplitude - mediumThreshold) / (highestPeak - mediumThreshold)));
                var glowColor = Color.FromArgb(glowAlpha, color.R, color.G, color.B);
                // Quantize glow alpha to 5 steps for caching
                var glowKey = 1000 + colorKey * 10 + (glowAlpha / 10);
                var glowPen = GetCachedFancyWaveformGlowPen(glowKey, glowColor, 3.0);
                context.DrawLine(glowPen, new Point(x, yMax - 0.5), new Point(x, yMin + 0.5));
            }
        }
    }

    private void DrawWaveFormClassic(DrawingContext context, double waveformHeight, ref RenderContext renderCtx)
    {
        var isSelectedHelper = new IsSelectedHelper(AllSelectedParagraphs, renderCtx.SampleRate);
        var halfWaveformHeight = waveformHeight / 2;
        var div = renderCtx.SampleRate * renderCtx.ZoomFactor;

        if (div <= 0 || WavePeaks == null)
        {
            return;
        }

        var peaks = WavePeaks.Peaks;
        var peaksCount = peaks.Count;

        for (var x = 0; x < renderCtx.Width; x++)
        {
            var pos = (renderCtx.StartPositionSeconds + x / div) * renderCtx.SampleRate;
            var pos0 = (int)pos;
            var pos1 = pos0 + 1;

            if (pos1 >= peaksCount || pos0 > peaksCount)
            {
                break;
            }

            var pos1Weight = pos - pos0;
            var pos0Weight = 1.0 - pos1Weight;
            var peak0 = peaks[pos0];
            var peak1 = peaks[pos1];
            var max = peak0.Max * pos0Weight + peak1.Max * pos1Weight;
            var min = peak0.Min * pos0Weight + peak1.Min * pos1Weight;

            var yMax = CalculateYOptimized(max, halfWaveformHeight, renderCtx.HighestPeak, renderCtx.VerticalZoomFactor, renderCtx.Height);
            var yMin = CalculateYOptimized(min, halfWaveformHeight, renderCtx.HighestPeak, renderCtx.VerticalZoomFactor, renderCtx.Height);

            // Ensure yMin is below yMax and both are within bounds
            if (yMin < yMax)
            {
                (yMin, yMax) = (yMax, yMin);
            }

            // Make sure there's at least a 1 pixel line
            if (Math.Abs(yMax - yMin) < 1)
            {
                yMin = yMax + 1;
            }

            var pen = isSelectedHelper.IsSelected(pos0) ? _paintPenSelected : _paintWaveform;
            context.DrawLine(pen, new Point(x, yMax), new Point(x, yMin));
        }
    }

    private void DrawParagraphs(DrawingContext context, ref RenderContext renderCtx)
    {
        var paragraphs = _displayableParagraphs;
        var startPositionMilliseconds = renderCtx.StartPositionSeconds * 1000.0;
        var endPositionMilliseconds = RelativeXPositionToSecondsOptimized(renderCtx.Width, renderCtx.SampleRate, renderCtx.StartPositionSeconds, renderCtx.ZoomFactor) * 1000.0;

        foreach (var p in paragraphs)
        {
            if (p.EndTime.TotalMilliseconds >= startPositionMilliseconds && p.StartTime.TotalMilliseconds <= endPositionMilliseconds)
            {
                DrawParagraph(p, context, ref renderCtx);
            }
        }
    }

    private void DrawParagraph(SubtitleLineViewModel paragraph, DrawingContext context, ref RenderContext renderCtx)
    {
        var currentRegionLeft = SecondsToXPositionOptimized(paragraph.StartTime.TotalSeconds - renderCtx.StartPositionSeconds, renderCtx.SampleRate, renderCtx.ZoomFactor);
        var currentRegionRight = SecondsToXPositionOptimized(paragraph.EndTime.TotalSeconds - renderCtx.StartPositionSeconds, renderCtx.SampleRate, renderCtx.ZoomFactor);
        var currentRegionWidth = currentRegionRight - currentRegionLeft;

        if (currentRegionWidth <= 5)
        {
            return;
        }

        var height = renderCtx.Height;

        // Draw background rectangle
        context.FillRectangle(AllSelectedParagraphs.Contains(paragraph) ? _paintParagraphSelectedBackground : _paintParagraphBackground,
            new Rect(currentRegionLeft, 0, currentRegionWidth, height));

        // Draw left and right borders
        context.DrawLine(_paintLeft, new Point(currentRegionLeft, 0), new Point(currentRegionLeft, height));
        context.DrawLine(_paintRight, new Point(currentRegionRight - 1, 0), new Point(currentRegionRight - 1, height));

        // Draw clipped text
        var text = HtmlUtil.RemoveHtmlTags(paragraph.Text, true);
        if (text.Length > 200)
        {
            text = text.Substring(0, 100).TrimEnd() + "...";
        }

        var textBounds = new Rect(currentRegionLeft + 1, 0, currentRegionWidth - 3, height);

        using (context.PushClip(textBounds))
        {
            var arr = text.SplitToLines();
            if (Configuration.Settings.VideoControls.WaveformUnwrapText)
            {
                text = string.Join("  ", arr);
                var formattedText = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                    _typeface, _fontSize, _paintText);
                context.DrawText(formattedText, new Point(currentRegionLeft + 3, 14));
            }
            else
            {
                double addY = 0;
                foreach (var line in arr)
                {
                    var formattedText = new FormattedText(line, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                        _typeface, _fontSize, _paintText);
                    context.DrawText(formattedText, new Point(currentRegionLeft + 3, 14 + addY));
                    addY += formattedText.Height;
                }
            }
        }
    }

    private void DrawShotChanges(DrawingContext context, ref RenderContext renderCtx)
    {
        var index = 0;
        var currentPositionPos = SecondsToXPositionOptimized(renderCtx.CurrentVideoPositionSeconds - renderCtx.StartPositionSeconds, renderCtx.SampleRate, renderCtx.ZoomFactor);

        var startPositionMilliseconds = renderCtx.StartPositionSeconds * 1000.0;
        var endPositionMilliseconds = RelativeXPositionToSecondsOptimized(renderCtx.Width, renderCtx.SampleRate, renderCtx.StartPositionSeconds, renderCtx.ZoomFactor) * 1000.0;
        var paragraphStartList = new List<int>();
        var paragraphEndList = new List<int>();
        foreach (var p in _displayableParagraphs)
        {
            if (p.EndTime.TotalMilliseconds >= startPositionMilliseconds && p.StartTime.TotalMilliseconds <= endPositionMilliseconds)
            {
                paragraphStartList.Add(SecondsToXPositionOptimized(p.StartTime.TotalSeconds - renderCtx.StartPositionSeconds, renderCtx.SampleRate, renderCtx.ZoomFactor));
                paragraphEndList.Add(SecondsToXPositionOptimized(p.EndTime.TotalSeconds - renderCtx.StartPositionSeconds, renderCtx.SampleRate, renderCtx.ZoomFactor));
            }
        }

        while (index < _shotChanges.Count)
        {
            int pos;
            try
            {
                var time = _shotChanges[index++];
                pos = SecondsToXPositionOptimized(time - renderCtx.StartPositionSeconds, renderCtx.SampleRate, renderCtx.ZoomFactor);
            }
            catch
            {
                pos = -1;
            }

            if (pos > 0 && pos < renderCtx.Width)
            {
                if (currentPositionPos == pos)
                {
                    // shot change and current pos are the same
                    var pen1 = new Pen(Brushes.AntiqueWhite, 2);
                    context.DrawLine(pen1, new Point(pos, 0), new Point(pos, renderCtx.Height));
                    context.DrawLine(_paintPenCursor, new Point(pos, 0), new Point(pos, renderCtx.Height));
                }
                else if (paragraphStartList.Contains(pos))
                {
                    var pen1 = new Pen(Brushes.AntiqueWhite, 2);
                    context.DrawLine(pen1, new Point(pos, 0), new Point(pos, renderCtx.Height));

                    var brush = new SolidColorBrush(Color.FromArgb(175, 0, 100, 0));
                    var pen2 = new Pen(brush, 2, dashStyle: DashStyle.Dash);
                    context.DrawLine(pen2, new Point(pos, 0), new Point(pos, renderCtx.Height));
                }
                else if (paragraphEndList.Contains(pos))
                {
                    var pen1 = new Pen(Brushes.AntiqueWhite, 2);
                    context.DrawLine(pen1, new Point(pos, 0), new Point(pos, renderCtx.Height));

                    var brush = new SolidColorBrush(Color.FromArgb(175, 110, 10, 10));
                    var pen2 = new Pen(brush, 2, dashStyle: DashStyle.Dash);
                    context.DrawLine(pen2, new Point(pos, 0), new Point(pos, renderCtx.Height));
                }
                else
                {
                    var pen = new Pen(Brushes.AntiqueWhite, 1);
                    context.DrawLine(pen, new Point(pos, 0), new Point(pos, renderCtx.Height));
                }
            }
        }
    }

    private void DrawCurrentVideoPosition(DrawingContext context, ref RenderContext renderCtx)
    {
        if (renderCtx.CurrentVideoPositionSeconds <= 0)
        {
            return;
        }

        var currentPositionPos = SecondsToXPositionOptimized(renderCtx.CurrentVideoPositionSeconds - renderCtx.StartPositionSeconds, renderCtx.SampleRate, renderCtx.ZoomFactor);
        if (currentPositionPos > 0 && currentPositionPos < renderCtx.Width)
        {
            var isOnShotChange = GetShotChangeIndex(renderCtx.CurrentVideoPositionSeconds) >= 0;

            if (isOnShotChange)
            {
                var paintCurrentPositionOverlap = new Pen(Brushes.LightCyan, 1.5)
                {
                    DashStyle = DashStyle.Dash,
                };

                context.DrawLine(paintCurrentPositionOverlap,
                    new Point(currentPositionPos, 0),
                    new Point(currentPositionPos, renderCtx.Height));
            }
            else
            {
                context.DrawLine(_paintPenCursor,
                    new Point(currentPositionPos, 0),
                    new Point(currentPositionPos, renderCtx.Height));
            }
        }
    }

    private void DrawNewParagraph(DrawingContext context, ref RenderContext renderCtx)
    {
        if (NewSelectionParagraph == null)
        {
            return;
        }

        double currentRegionLeft =
            SecondsToXPositionOptimized(NewSelectionParagraph.StartTime.TotalSeconds - renderCtx.StartPositionSeconds, renderCtx.SampleRate, renderCtx.ZoomFactor);
        double currentRegionRight =
            SecondsToXPositionOptimized(NewSelectionParagraph.EndTime.TotalSeconds - renderCtx.StartPositionSeconds, renderCtx.SampleRate, renderCtx.ZoomFactor);
        var currentRegionWidth = currentRegionRight - currentRegionLeft;

        if (currentRegionRight >= 0 && currentRegionLeft <= renderCtx.Width)
        {
            var rect = new Rect(currentRegionLeft, 0, currentRegionWidth, renderCtx.Height);
            context.FillRectangle(_paintParagraphBackground, rect);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double CalculateYOptimized(double value, double halfWaveformHeight, double highestPeak, double verticalZoomFactor, double boundsHeight)
    {
        // Normalize the value to the control's height
        var normalizedValue = value / highestPeak * verticalZoomFactor;
        var yOffset = normalizedValue * halfWaveformHeight;

        // Ensure Y stays within bounds
        var y = halfWaveformHeight - yOffset;
        return Math.Max(0, Math.Min(boundsHeight, y));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private double RelativeXPositionToSeconds(int x)
    {
        if (WavePeaks == null)
        {
            return 0;
        }

        return StartPositionSeconds + (double)x / WavePeaks.SampleRate / ZoomFactor;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private double RelativeXPositionToSeconds(double x)
    {
        if (WavePeaks == null)
        {
            return 0;
        }

        return StartPositionSeconds + x / WavePeaks.SampleRate / ZoomFactor;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double RelativeXPositionToSecondsOptimized(double x, int sampleRate, double startPositionSeconds, double zoomFactor)
    {
        if (sampleRate == 0)
        {
            return 0;
        }

        return startPositionSeconds + x / sampleRate / zoomFactor;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int SecondsToXPosition(double seconds)
    {
        if (WavePeaks == null)
        {
            return 0;
        }

        return (int)Math.Round(seconds * WavePeaks.SampleRate * ZoomFactor, MidpointRounding.AwayFromZero);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int SecondsToXPositionOptimized(double seconds, int sampleRate, double zoomFactor)
    {
        if (sampleRate == 0)
        {
            return 0;
        }

        return (int)Math.Round(seconds * sampleRate * zoomFactor, MidpointRounding.AwayFromZero);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int SecondsToSampleIndex(double seconds)
    {
        if (WavePeaks == null)
        {
            return 0;
        }

        return (int)Math.Round(seconds * WavePeaks.SampleRate, MidpointRounding.AwayFromZero);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private double SampleIndexToSeconds(int index)
    {
        if (WavePeaks == null)
        {
            return 0;
        }

        return (double)index / WavePeaks.SampleRate;
    }

    public void SetPosition(double startPositionSeconds, ObservableCollection<SubtitleLineViewModel> subtitle, double currentVideoPositionSeconds, int subtitleIndex,
        List<SubtitleLineViewModel> selectedIndexes)
    {
        if (TimeSpan.FromMilliseconds(Environment.TickCount64 - _lastMouseWheelScroll).TotalSeconds > 0.25)
        {
            // don't set start position when scrolling with mouse wheel as it will make a bad (jumping back) forward scrolling
            StartPositionSeconds = startPositionSeconds;
        }

        CurrentVideoPositionSeconds = currentVideoPositionSeconds;
        LoadParagraphs(subtitle, subtitleIndex, selectedIndexes);
    }

    private void LoadParagraphs(ObservableCollection<SubtitleLineViewModel> subtitle, int primarySelectedIndex, List<SubtitleLineViewModel> selectedIndexes)
    {
        lock (_lock)
        {
            _displayableParagraphs.Clear();
            SelectedParagraph = null;
            AllSelectedParagraphs.Clear();

            if (WavePeaks == null || subtitle.Count == 0)
            {
                return;
            }

            const double additionalSeconds = 15.0;
            var startThreshold = (StartPositionSeconds - additionalSeconds) * TimeCode.BaseUnit;
            var endThreshold = (EndPositionSeconds + additionalSeconds) * TimeCode.BaseUnit;
            var maxTime = TimeCode.MaxTimeTotalMilliseconds;

            // 1. Use Binary Search to find the first potential subtitle in the time range O(log N)
            var startIndex = FindFirstIndexAfterTime(subtitle, startThreshold);

            var lastStartTime = -1d;
            var count = 0;

            // 2. Linear scan only the relevant window
            for (var i = startIndex; i < subtitle.Count; i++)
            {
                var p = subtitle[i];
                var pStart = p.StartTime.TotalMilliseconds;

                // Since it's sorted, if we exceed the end threshold or max time, we can stop entirely
                if (pStart > endThreshold || pStart >= maxTime)
                {
                    break;
                }

                // Skip subtitles that end before our window starts
                if (p.EndTime.TotalMilliseconds < startThreshold)
                {
                    continue;
                }

                // 3. Apply filtering logic immediately to avoid second loop
                var isTooShortOrDense = count > 200 && (p.Duration.TotalMilliseconds < 0.01 || pStart - lastStartTime < 90);

                if (!isTooShortOrDense)
                {
                    _displayableParagraphs.Add(p);
                    lastStartTime = pStart;
                    count++;
                }

                if (count >= 250)
                {
                    break;
                }
            }

            // 4. Optimized Selection Handling
            var primaryParagraph = (primarySelectedIndex >= 0 && primarySelectedIndex < subtitle.Count)
                ? subtitle[primarySelectedIndex]
                : null;

            if (primaryParagraph != null && !primaryParagraph.StartTime.IsMaxTime())
            {
                SelectedParagraph = primaryParagraph;
                AllSelectedParagraphs.Add(primaryParagraph);
            }

            foreach (var p in selectedIndexes)
            {
                if (p != null && !p.StartTime.IsMaxTime() && p != primaryParagraph)
                {
                    AllSelectedParagraphs.Add(p);
                }
            }
        }
    }

    // Helper for Binary Search
    private static int FindFirstIndexAfterTime(ObservableCollection<SubtitleLineViewModel> subtitle, double timeMs)
    {
        int low = 0, high = subtitle.Count - 1;
        var result = 0;

        while (low <= high)
        {
            int mid = low + (high - low) / 2;
            if (subtitle[mid].EndTime.TotalMilliseconds >= timeMs)
            {
                result = mid;
                high = mid - 1;
            }
            else
            {
                low = mid + 1;
            }
        }

        return result;
    }

    internal int GetShotChangeIndex(double seconds)
    {
        if (ShotChanges == null)
        {
            return -1;
        }

        try
        {
            for (var index = 0; index < ShotChanges.Count; index++)
            {
                var shotChange = ShotChanges[index];
                if (Math.Abs(shotChange - seconds) < 0.04)
                {
                    return index;
                }
            }
        }
        catch
        {
            // ignored
        }

        return -1;
    }

    internal void SetSpectrogram(SpectrogramData2 spectrogram)
    {
        if (_spectrogram != null)
        {
            _spectrogram.Dispose();
            _spectrogram = null;
        }

        if (spectrogram == null)
        {
            return;
        }

        if (spectrogram.IsLoaded)
        {
            InitializeSpectrogramInternal(spectrogram);
        }
        else
        {
            Task.Factory.StartNew(() =>
            {
                spectrogram.Load();
                Dispatcher.UIThread.Post(() => { InitializeSpectrogramInternal(spectrogram); });
            });
        }
    }

    public bool HasSpectrogram()
    {
        return _spectrogram != null &&
               _spectrogram.Images != null &&
               _spectrogram.Images.Count > 0;
    }

    private void InitializeSpectrogramInternal(SpectrogramData2 spectrogram)
    {
        if (_spectrogram != null)
        {
            return;
        }

        _spectrogram = spectrogram;
    }

    public double FindDataBelowThreshold(double thresholdPercent, double durationInSeconds)
    {
        if (WavePeaks == null || WavePeaks.Peaks.Count == 0)
        {
            return -1;
        }

        var begin = SecondsToSampleIndex(CurrentVideoPositionSeconds + 1);
        var length = SecondsToSampleIndex(durationInSeconds);
        var threshold = thresholdPercent / 100.0 * WavePeaks.HighestPeak;
        var hitCount = 0;
        for (var i = Math.Max(0, begin); i < WavePeaks.Peaks.Count; i++)
        {
            if (WavePeaks.Peaks[i].Abs <= threshold)
            {
                hitCount++;
            }
            else
            {
                hitCount = 0;
            }

            if (hitCount > length)
            {
                var seconds = RelativeXPositionToSeconds(i - (length / 2));
                if (seconds >= 0)
                {
                    StartPositionSeconds = seconds;
                    if (StartPositionSeconds > 1)
                    {
                        StartPositionSeconds -= 1;
                    }
                }

                return seconds;
            }
        }

        return -1;
    }

    /// <returns>video position in seconds, -1 if not found</returns>
    public double FindDataBelowThresholdBack(double thresholdPercent, double durationInSeconds)
    {
        if (WavePeaks == null || WavePeaks.Peaks.Count == 0)
        {
            return -1;
        }

        var begin = SecondsToSampleIndex(CurrentVideoPositionSeconds - 1);
        var length = SecondsToSampleIndex(durationInSeconds);
        var threshold = thresholdPercent / 100.0 * WavePeaks.HighestPeak;
        var hitCount = 0;
        for (var i = begin; i > 0; i--)
        {
            if (i < WavePeaks.Peaks.Count && WavePeaks.Peaks[i].Abs <= threshold)
            {
                hitCount++;
                if (hitCount > length)
                {
                    var seconds = RelativeXPositionToSeconds(i + length / 2);
                    if (seconds >= 0)
                    {
                        StartPositionSeconds = seconds;
                        if (StartPositionSeconds > 1)
                        {
                            StartPositionSeconds -= 1;
                        }
                        else
                        {
                            StartPositionSeconds = 0;
                        }
                    }

                    return seconds;
                }
            }
            else
            {
                hitCount = 0;
            }
        }

        return -1;
    }

    /// <summary>
    /// Seeks silence in volume
    /// </summary>
    /// <returns>video position in seconds, -1 if not found</returns>
    public double FindDataBelowThresholdBackForStart(double thresholdPercent, double durationInSeconds, double startSeconds)
    {
        if (WavePeaks == null || WavePeaks.Peaks.Count == 0)
        {
            return -1;
        }

        var min = Math.Max(0, SecondsToSampleIndex(startSeconds - 1));
        var maxShort = Math.Min(WavePeaks.Peaks.Count, SecondsToSampleIndex(startSeconds + durationInSeconds + 0.01));
        var max = Math.Min(WavePeaks.Peaks.Count, SecondsToSampleIndex(startSeconds + durationInSeconds + 0.8));
        var length = SecondsToSampleIndex(durationInSeconds);
        var threshold = thresholdPercent / 100.0 * WavePeaks.HighestPeak;

        var minMax = GetMinAndMax(min, max);
        const int lowPeakDifference = 4_000;
        if (minMax.Max - minMax.Min < lowPeakDifference)
        {
            return -1; // all audio about the same
        }

        // look for start silence in the beginning of subtitle
        min = SecondsToSampleIndex(startSeconds);
        var hitCount = 0;
        int index;
        for (index = min; index < max; index++)
        {
            if (index > 0 && index < WavePeaks.Peaks.Count && WavePeaks.Peaks[index].Abs <= threshold)
            {
                hitCount++;
            }
            else
            {
                minMax = GetMinAndMax(min, index);
                var currentMinMax = GetMinAndMax(SecondsToSampleIndex(startSeconds), SecondsToSampleIndex(startSeconds + 0.8));
                if (currentMinMax.Avg > minMax.Avg + 300 || currentMinMax.Avg < 1000 && minMax.Avg < 1000 && Math.Abs(currentMinMax.Avg - minMax.Avg) < 500)
                {
                    break;
                }

                hitCount = length / 2;
            }
        }

        if (hitCount > length)
        {
            minMax = GetMinAndMax(min, index);
            var currentMinMax = GetMinAndMax(SecondsToSampleIndex(startSeconds), SecondsToSampleIndex(startSeconds + 0.8));
            if (currentMinMax.Avg > minMax.Avg + 300 || currentMinMax.Avg < 1000 && minMax.Avg < 1000 && Math.Abs(currentMinMax.Avg - minMax.Avg) < 500)
            {
                return Math.Max(0, SampleIndexToSeconds(index - 1) - 0.01);
            }
        }

        // move start left?
        min = SecondsToSampleIndex(startSeconds - 1);
        hitCount = 0;
        for (index = maxShort; index > min; index--)
        {
            if (index > 0 && index < WavePeaks.Peaks.Count && WavePeaks.Peaks[index].Abs <= threshold)
            {
                hitCount++;
                if (hitCount > length)
                {
                    return Math.Max(0, SampleIndexToSeconds(index + length) - 0.01);
                }
            }
            else
            {
                hitCount = 0;
            }
        }

        return -1;
    }

    private MinMax GetMinAndMax(int startIndex, int endIndex)
    {
        if (WavePeaks == null || WavePeaks.Peaks.Count == 0)
        {
            return new MinMax { Min = 0, Max = 0, Avg = 0 };
        }

        var minPeak = int.MaxValue;
        var maxPeak = int.MinValue;
        double total = 0;
        for (var i = startIndex; i < endIndex; i++)
        {
            var v = WavePeaks.Peaks[i].Abs;
            total += v;
            if (v < minPeak)
            {
                minPeak = v;
            }

            if (v > maxPeak)
            {
                maxPeak = v;
            }
        }

        return new MinMax { Min = minPeak, Max = maxPeak, Avg = total / (endIndex - startIndex) };
    }

    internal void GenerateTimeCodes(Subtitle subtitle, double startFromSeconds, int blockSizeMilliseconds, int minimumVolumePercent, int maximumVolumePercent,
        int defaultMilliseconds)
    {
        if (WavePeaks == null || WavePeaks.Peaks.Count == 0)
        {
            return;
        }

        var begin = SecondsToSampleIndex(startFromSeconds);

        double average = 0;
        for (int k = begin; k < WavePeaks.Peaks.Count; k++)
        {
            average += WavePeaks.Peaks[k].Abs;
        }

        average /= WavePeaks.Peaks.Count - begin;

        var maxThreshold = (int)(WavePeaks.HighestPeak * (maximumVolumePercent / 100.0));
        var silenceThreshold = (int)(average * (minimumVolumePercent / 100.0));

        int length50Ms = SecondsToSampleIndex(0.050);
        double secondsPerParagraph = defaultMilliseconds / TimeCode.BaseUnit;
        int minBetween = SecondsToSampleIndex(Se.Settings.General.MinimumMillisecondsBetweenLines / TimeCode.BaseUnit);
        bool subtitleOn = false;
        int i = begin;
        while (i < WavePeaks.Peaks.Count)
        {
            if (subtitleOn)
            {
                var currentLengthInSeconds = SampleIndexToSeconds(i - begin);
                if (currentLengthInSeconds > 1.0)
                {
                    subtitleOn = EndParagraphDueToLowVolume(subtitle, blockSizeMilliseconds, silenceThreshold, begin, true, i);
                    if (!subtitleOn)
                    {
                        begin = i + minBetween;
                        i = begin;
                    }
                }

                if (subtitleOn && currentLengthInSeconds >= secondsPerParagraph)
                {
                    for (int j = 0; j < 20; j++)
                    {
                        subtitleOn = EndParagraphDueToLowVolume(subtitle, blockSizeMilliseconds, silenceThreshold, begin, true, i + (j * length50Ms));
                        if (!subtitleOn)
                        {
                            i += (j * length50Ms);
                            begin = i + minBetween;
                            i = begin;
                            break;
                        }
                    }

                    if (subtitleOn) // force break
                    {
                        var p = new Paragraph(string.Empty, SampleIndexToSeconds(begin) * TimeCode.BaseUnit, SampleIndexToSeconds(i) * TimeCode.BaseUnit);
                        subtitle.Paragraphs.Add(p);
                        begin = i + minBetween;
                        i = begin;
                    }
                }
            }
            else
            {
                double avgVol = GetAverageVolumeForNextMilliseconds(i, blockSizeMilliseconds);
                if (avgVol > silenceThreshold && avgVol < maxThreshold)
                {
                    subtitleOn = true;
                    begin = i;
                }
            }

            i++;
        }

        subtitle.Renumber();
    }

    private bool EndParagraphDueToLowVolume(Subtitle subtitle, int blockSizeMilliseconds, double silenceThreshold, int begin, bool subtitleOn, int i)
    {
        var avgVol = GetAverageVolumeForNextMilliseconds(i, blockSizeMilliseconds);
        if (avgVol < silenceThreshold)
        {
            var p = new Paragraph(string.Empty, SampleIndexToSeconds(begin) * TimeCode.BaseUnit, SampleIndexToSeconds(i) * TimeCode.BaseUnit);
            subtitle.Paragraphs.Add(p);
            subtitleOn = false;
        }

        return subtitleOn;
    }

    private double GetAverageVolumeForNextMilliseconds(int sampleIndex, int milliseconds)
    {
        if (WavePeaks == null)
        {
            return 0;
        }

        // length cannot be less than 9
        var length = Math.Max(SecondsToSampleIndex(milliseconds / TimeCode.BaseUnit), 9);
        var max = Math.Min(sampleIndex + length, WavePeaks.Peaks.Count);
        var from = Math.Max(sampleIndex, 1);

        if (from >= max)
        {
            return 0;
        }

        double v = 0;
        for (var i = from; i < max; i++)
        {
            v += WavePeaks.Peaks[i].Abs;
        }

        return v / (max - from);
    }

    internal void CenterOnPosition(double position)
    {
        if (WavePeaks == null)
        {
            return;
        }

        var halfWidthInSeconds = (Bounds.Width / 2) / (WavePeaks.SampleRate * ZoomFactor);
        StartPositionSeconds = Math.Max(0, position - halfWidthInSeconds);
    }

    internal void CenterOnPosition(SubtitleLineViewModel line)
    {
        if (WavePeaks == null)
        {
            return;
        }

        var halfWidthInSeconds = (Bounds.Width / 2.0) / (WavePeaks.SampleRate * ZoomFactor) - (line.Duration.TotalSeconds / 2.0);
        StartPositionSeconds = Math.Max(0, line.StartTime.TotalSeconds - halfWidthInSeconds);
    }

    internal void SetDisplayMode(WaveformDisplayMode displayMode)
    {
        _displayMode = displayMode;
    }

    internal WaveformDisplayMode GetDisplayMode()
    {
        return _displayMode;
    }

    internal void ResetCache()
    {
        _fancyWaveformPenCache.Clear();
        _fancyWaveformGlowPenCache.Clear();
        _fancyWaveformGradientCache.Clear();
    }

    internal void SetKeyModifiers(KeyEventArgs e)
    {
        _isCtrlDown = e.KeyModifiers.HasFlag(KeyModifiers.Control);
        _isShiftDown = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
        _isAltDown = e.KeyModifiers.HasFlag(KeyModifiers.Alt);
        _isMetaDown = e.KeyModifiers.HasFlag(KeyModifiers.Meta);
    }
}