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

    public static readonly StyledProperty<Color> WaveformShotChangeColorProperty =
       AvaloniaProperty.Register<AudioVisualizer, Color>(nameof(WaveformShotChangeColor));

    public static readonly StyledProperty<Color> WaveformParagraphLeftColorProperty =
        AvaloniaProperty.Register<AudioVisualizer, Color>(nameof(WaveformParagraphLeftColor));

    public static readonly StyledProperty<Color> WaveformParagraphRightColorProperty =
        AvaloniaProperty.Register<AudioVisualizer, Color>(nameof(WaveformParagraphRightColor));

    public WavePeakData2? WavePeaks
    {
        get => GetValue(WavePeaksProperty);
        set => SetValue(WavePeaksProperty, value);
    }

    public double StartPositionSeconds
    {
        get => GetValue(StartPositionSecondsProperty);
        set => SetValue(StartPositionSecondsProperty, Math.Clamp(value, 0, MaxStartPositionSeconds));
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

    
         public Color WaveformShotChangeColor
    {
        get => GetValue(WaveformShotChangeColorProperty);
        set
        {
            _paintShotChangeThickPen = new Pen(new SolidColorBrush(value), 2);
            _paintShotChangeThinPen = new Pen(new SolidColorBrush(value), 1);
            SetValue(WaveformShotChangeColorProperty, value);
        }
    }

    public Color WaveformParagraphLeftColor
    {
        get => GetValue(WaveformParagraphLeftColorProperty);
        set
        {
            _paintLeft = new Pen(new SolidColorBrush(value), 1);
            SetValue(WaveformParagraphLeftColorProperty, value);
        }
    }


    public Color WaveformParagraphRightColor
    {
        get => GetValue(WaveformParagraphRightColorProperty);
        set
        {
            _paintRight = new Pen(new SolidColorBrush(value), 1);
            SetValue(WaveformParagraphRightColorProperty, value);
        }
    }


    public SubtitleLineViewModel? SelectedParagraph { get; set; }

    public double MinGapSeconds { get; set; } = 0.1;

    public double ShotChangeSnapSeconds { get; set; } = 0.05;
    public WaveformDrawStyle WaveformDrawStyle { get; set; } = WaveformDrawStyle.Classic;

    // Reads the user setting directly (like SnapToFrames) so the "Snap to shot changes"
    // checkbox actually takes effect, and does so live without re-wiring at every call site.
    public bool SnapToShotChanges => Se.Settings.Waveform.SnapToShotChanges;
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
    private Pen _paintShotChangeThickPen = new Pen(Brushes.AntiqueWhite, 2);
    private Pen _paintShotChangeThinPen = new Pen(Brushes.AntiqueWhite, 1);

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
    private Pen _paintLeft = new Pen(new SolidColorBrush(Color.FromArgb(60, 0, 255, 0)), 2);
    private Pen _paintRight = new Pen(new SolidColorBrush(Color.FromArgb(100, 255, 0, 0)), 2);
    private IBrush _paintText = new SolidColorBrush(Se.Settings.Waveform.WaveformTextColor.FromHexToColor());
    private Typeface _typeface = new Typeface(UiUtil.GetDefaultFontName(), FontStyle.Normal, Se.Settings.Waveform.WaveformTextFontBold ? FontWeight.Bold : FontWeight.Normal);
    private readonly Pen _paintShotChangeParagraphStartPen = new Pen(new SolidColorBrush(Color.FromArgb(175, 0, 100, 0)), 2, dashStyle: DashStyle.Dash);
    private readonly Pen _paintShotChangeParagraphEndPen = new Pen(new SolidColorBrush(Color.FromArgb(175, 110, 10, 10)), 2, dashStyle: DashStyle.Dash);
    private readonly Pen _centerLinePen = new Pen(Brushes.DarkGray, 0.5);
    private readonly HashSet<int> _paragraphStartPositions = new();
    private readonly HashSet<int> _paragraphEndPositions = new();
    private readonly HashSet<SubtitleLineViewModel> _selectedParagraphsRenderSet = new();
    private double _fontSize = Se.Settings.Waveform.WaveformTextFontSize;
    private static readonly Cursor _cursorArrow = new Cursor(StandardCursorType.Arrow);
    private static readonly Cursor _cursorHand = new Cursor(StandardCursorType.Hand);
    private static readonly Cursor _cursorSizeWestEast = new Cursor(StandardCursorType.SizeWestEast);

    private readonly List<SubtitleLineViewModel> _displayableParagraphs = new();
    private readonly IsSelectedHelper _isSelectedHelper = new();
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

    // Snapshot of every selected paragraph's start/duration captured when a bulk
    // (multi-selection) move starts, so the whole block can be shifted by one delta.
    private readonly List<(SubtitleLineViewModel Paragraph, double StartSeconds, double DurationSeconds)> _selectionMoveSnapshot = new();

    private bool _preventNextTap;
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
        MovingSelection,
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

    // Horizontal pixels the pointer must travel between press and release before an
    // interaction counts as a real drag (move/resize). Below this it is a plain
    // click, so the follow-up Tapped (select/seek) must NOT be suppressed.
    private const double ClickDragSlopPixels = 3.0;
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

    /// <summary>Raised when the user clicks the empty waveform to generate it on demand
    /// (shown only when auto-generate is off and there are no cached peaks).</summary>
    public event EventHandler? OnGenerateWaveformRequested;

    /// <summary>When true and there are no <see cref="WavePeaks"/>, a "click to generate"
    /// hint is drawn and a click raises <see cref="OnGenerateWaveformRequested"/>.</summary>
    public bool ShowClickToGenerateHint { get; set; }

    /// <summary>Localized hint text drawn over the empty waveform (see <see cref="ShowClickToGenerateHint"/>).</summary>
    public string ClickToGenerateText { get; set; } = string.Empty;

    public AudioVisualizer()
    {
        // A right to left UI language sets the whole window to RightToLeft, which
        // mirrors every visual, including this waveform (the peaks and the region
        // labels would render flipped). Pin the control to left to right so the
        // waveform is never mirrored regardless of the UI language.
        FlowDirection = Avalonia.Media.FlowDirection.LeftToRight;

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

        if (_preventNextTap)
        {
            _preventNextTap = false;
            return;
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
                firstSelected.SetStartTimeKeepDuration(TimeSpan.FromSeconds(seconds));
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
            var deltaAlt = e.Delta.Y;
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
        // Handle both vertical (Y) and horizontal (X) trackpad scrolling
        var scrollDelta = delta;
        
        // If Y delta is negligible but X delta is significant (trackpad horizontal scroll)
        if (Math.Abs(delta) < 0.1 && Math.Abs(e.Delta.X) > 0.1)
        {
            scrollDelta = InvertMouseWheel ? -e.Delta.X : e.Delta.X;
        }

        // SE 4 parity: seek the video position (step forward/back) instead of scrolling the view.
        // Ctrl/Meta still keep their scroll + set-position-at-cursor behavior below.
        if (Se.Settings.Waveform.MouseWheelSetsVideoPosition && !_isCtrlDown && !_isMetaDown)
        {
            // One "notch" per event; magnitude scales with the delta for fast wheels/trackpads.
            var notches = scrollDelta > 0 ? Math.Ceiling(scrollDelta) : Math.Floor(scrollDelta);
            var stepMs = Se.Settings.Waveform.MouseWheelVideoPositionStepMs;

            double newVideoPosition;
            var fps = Se.Settings.General.CurrentFrameRate;
            if (stepMs <= 0 && fps >= 1)
            {
                // Frame step: quantize to the frame grid and move whole frames.
                var currentFrame = Math.Round(CurrentVideoPositionSeconds * fps);
                newVideoPosition = (currentFrame + notches) / fps;
            }
            else
            {
                // Millisecond step (also the fallback when a frame step has no frame rate).
                var stepSeconds = stepMs > 0 ? stepMs / 1000.0 : 0.5;
                newVideoPosition = CurrentVideoPositionSeconds + notches * stepSeconds;
            }

            if (newVideoPosition < 0)
            {
                newVideoPosition = 0;
            }

            if (WavePeaks != null && newVideoPosition > WavePeaks.LengthInSeconds)
            {
                newVideoPosition = WavePeaks.LengthInSeconds;
            }

            // Follow the play-head: scroll the view only when it would leave the visible range.
            var visibleSeconds = EndPositionSeconds - StartPositionSeconds;
            if (visibleSeconds > 0 && (newVideoPosition < StartPositionSeconds || newVideoPosition > EndPositionSeconds))
            {
                var followStart = newVideoPosition - visibleSeconds / 2;
                StartPositionSeconds = followStart < 0 ? 0 : followStart;
                OnHorizontalScroll?.Invoke(this, new PositionEventArgs { PositionInSeconds = StartPositionSeconds });
            }

            // Update locally so rapid wheeling builds on the new position before the player reports back.
            CurrentVideoPositionSeconds = newVideoPosition;
            OnVideoPositionChanged?.Invoke(this, new PositionEventArgs { PositionInSeconds = newVideoPosition });

            InvalidateVisual();
            return;
        }

        var newStart = StartPositionSeconds + scrollDelta / 2;
        if (newStart < 0)
        {
            newStart = 0;
        }

        _audioVisualizerLastScroll = _lastMouseWheelScroll; // Update the last scroll time
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

        if (_interactionMode is InteractionMode.Moving or InteractionMode.MovingSelection)
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

        if (_interactionMode is
            InteractionMode.Moving or
            InteractionMode.MovingSelection or
            InteractionMode.ResizingLeft or
            InteractionMode.ResizingRight or
            InteractionMode.ResizingLeftOr or
            InteractionMode.ResizingRightOr or
            InteractionMode.ResizeLeftAnd or
            InteractionMode.ResizeRightAnd)
        {
            // Only suppress the follow-up Tapped (which selects/seeks) when the pointer
            // actually dragged. A plain click that merely started within ResizeMargin of a
            // paragraph edge is tagged as a resize on press but never moves; without this
            // guard _preventNextTap would swallow OnTapped and the paragraph would not get
            // selected (clicking near a boundary sometimes did not select).
            if (Math.Abs(pos.X - _startPointerPosition.X) > ClickDragSlopPixels)
            {
                _preventNextTap = true;
            }
        }

        _interactionMode = InteractionMode.None;
        _activeParagraph = null;
        _selectionMoveSnapshot.Clear();

        InvalidateVisual();
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _isCtrlDown = e.KeyModifiers.HasFlag(KeyModifiers.Control);
        _isShiftDown = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
        _isAltDown = e.KeyModifiers.HasFlag(KeyModifiers.Alt);
        _isMetaDown = e.KeyModifiers.HasFlag(KeyModifiers.Meta);

        _lastPointerPressed = Environment.TickCount64;
        _preventNextTap = false;
        e.Handled = true;
        var point = e.GetPosition(this);
        _startPointerPosition = point;

        // No waveform yet and auto-generate is off: a click generates it on demand.
        if (WavePeaks == null && ShowClickToGenerateHint &&
            e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            OnGenerateWaveformRequested?.Invoke(this, EventArgs.Empty);
            return;
        }

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

            // A plain drag on a line that is part of a multi-selection shifts the whole
            // selection together (SE4 parity for bulk time-offset in the waveform).
            if (AllSelectedParagraphs is { Count: > 1 } && AllSelectedParagraphs.Contains(p))
            {
                _selectionMoveSnapshot.Clear();
                foreach (var selected in AllSelectedParagraphs)
                {
                    _selectionMoveSnapshot.Add((selected, selected.StartTime.TotalSeconds, selected.Duration.TotalSeconds));
                }

                _interactionMode = InteractionMode.MovingSelection;
            }
            else
            {
                _interactionMode = InteractionMode.Moving;
            }
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

            // Block overlap with neighbouring subtitles by default, like SE4 and the resize/move
            // path: keep the dragged selection inside the empty gap around its anchor. Holding Shift
            // or enabling "allow overlap" in settings bypasses the clamp.
            if (!_isShiftDown && !Se.Settings.Waveform.AllowOverlap)
            {
                var lowerBound = 0.0;
                var upperBound = double.MaxValue;
                foreach (var p in _displayableParagraphs)
                {
                    if (p == newP)
                    {
                        continue;
                    }

                    var pEnd = p.EndTime.TotalSeconds;
                    var pStart = p.StartTime.TotalSeconds;
                    if (pEnd <= _newSelectionSeconds && pEnd + MinGapSeconds > lowerBound)
                    {
                        lowerBound = pEnd + MinGapSeconds;
                    }

                    if (pStart >= _newSelectionSeconds && pStart - MinGapSeconds < upperBound)
                    {
                        upperBound = pStart - MinGapSeconds;
                    }
                }

                if (upperBound >= lowerBound)
                {
                    seconds = Math.Clamp(seconds, lowerBound, upperBound);
                }
            }

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

                newStart = SnapToFrame(newStart);

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

                    // Clamp to available space to prevent overlap (frame-aligned when snap is on)
                    if (newStart < availableStart)
                    {
                        newStart = SnapToFrameCeil(availableStart);
                    }

                    if (newStart + _originalDurationSeconds > availableEnd)
                    {
                        newStart = SnapToFrameFloor(availableEnd - _originalDurationSeconds);
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
            case InteractionMode.MovingSelection:
            {
                // Shift every selected paragraph by the same delta. Snap is anchored on the
                // grabbed line so relative spacing is preserved, and the delta is clamped so
                // the earliest selected line never moves before zero. Overlap with unselected
                // neighbours is allowed - the user is repositioning the block as a whole.
                var shift = SnapToFrame(_originalStartSeconds + deltaSeconds - StartPositionSeconds) - _originalStartSeconds;

                var minOriginalStart = double.MaxValue;
                foreach (var item in _selectionMoveSnapshot)
                {
                    if (item.StartSeconds < minOriginalStart)
                    {
                        minOriginalStart = item.StartSeconds;
                    }
                }

                if (minOriginalStart + shift < 0)
                {
                    shift = -minOriginalStart;
                }

                foreach (var item in _selectionMoveSnapshot)
                {
                    var start = item.StartSeconds + shift;
                    item.Paragraph.StartTime = TimeSpan.FromSeconds(start);
                    item.Paragraph.EndTime = TimeSpan.FromSeconds(start + item.DurationSeconds);
                }

                break;
            }
            case InteractionMode.ResizeLeftAnd:
                newStart = _originalStartSeconds + deltaSeconds - StartPositionSeconds;
                var newPrevEnd = _originalPreviousEndSeconds + deltaSeconds - StartPositionSeconds;
                var snappedLeft = SnapToFrame(newStart);
                newPrevEnd += snappedLeft - newStart;
                newStart = snappedLeft;
                if (_activeParagraphPrevious != null)
                {
                    _activeParagraph.SetStartTimeOnly(TimeSpan.FromSeconds(newStart));
                    _activeParagraphPrevious.EndTime = TimeSpan.FromSeconds(newPrevEnd);
                }

                break;
            case InteractionMode.ResizeRightAnd:
                newEnd = _originalEndSeconds + deltaSeconds - StartPositionSeconds;
                var newNextStart = _originalNextStartSeconds + deltaSeconds - StartPositionSeconds;
                var snappedRight = SnapToFrame(newEnd);
                newNextStart += snappedRight - newEnd;
                newEnd = snappedRight;
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

                var snappedToShotLeft = false;
                if (SnapToShotChanges && !_isShiftDown)
                {
                    var nearestShotChange = ShotChangesHelper.GetClosestShotChange(_shotChanges, TimeCode.FromSeconds(newStart));
                    if (nearestShotChange != null)
                    {
                        var nearest = (double)nearestShotChange;
                        var snapSeconds = GetInCueSnapSeconds();
                        if (nearest != newStart && Math.Abs(newStart - nearest) < snapSeconds)
                        {
                            newStart = nearest;
                            snappedToShotLeft = true;
                        }
                    }
                }

                if (!snappedToShotLeft)
                {
                    newStart = SnapToFrame(newStart);
                }

                if (previous != null && newStart < previous.EndTime.TotalSeconds + MinGapSeconds)
                {
                    newStart = previous.EndTime.TotalSeconds + MinGapSeconds + 0.001;
                    newStart = SnapToFrameCeil(newStart);
                }

                if (newStart < _activeParagraph.EndTime.TotalSeconds - 0.1)
                {
                    _activeParagraph.SetStartTimeOnly(TimeSpan.FromSeconds(newStart));
                }

                break;
            case InteractionMode.ResizingRight:
                newEnd = _originalEndSeconds + deltaSeconds - StartPositionSeconds;

                var snappedToShotRight = false;
                if (SnapToShotChanges && !_isShiftDown)
                {
                    // OUT cues conventionally land one frame BEFORE the shot change so they
                    // don't bleed visually onto the next shot.
                    var fps = Se.Settings.General.CurrentFrameRate;
                    var oneFrameSeconds = fps >= 1 ? 1.0 / fps : 0.0;
                    var nearestShotChange = ShotChangesHelper.GetClosestShotChange(_shotChanges, TimeCode.FromSeconds(newEnd));
                    if (nearestShotChange != null)
                    {
                        var nearest = (double)nearestShotChange;
                        var snapSeconds = GetOutCueSnapSeconds();
                        if (nearest != newEnd && Math.Abs(newEnd - nearest + oneFrameSeconds) < snapSeconds)
                        {
                            newEnd = nearest - oneFrameSeconds;
                            snappedToShotRight = true;
                        }
                    }
                }

                if (!snappedToShotRight)
                {
                    newEnd = SnapToFrame(newEnd);
                }

                if (next != null && newEnd > next.StartTime.TotalSeconds - MinGapSeconds)
                {
                    newEnd = next.StartTime.TotalSeconds - 0.001 - MinGapSeconds;
                    newEnd = SnapToFrameFloor(newEnd);
                }

                if (newEnd > _activeParagraph.StartTime.TotalSeconds + 0.1)
                {
                    _activeParagraph.EndTime = TimeSpan.FromSeconds(newEnd);
                }

                break;
        }

        // SE 4 parity: scrub the video to the edge being dragged so the user sees the
        // exact frame at the new start/end while resizing (whole-paragraph moves are excluded).
        if (Se.Settings.Waveform.SetVideoPositionOnMoveStartEnd && OnVideoPositionChanged != null && _activeParagraph != null)
        {
            switch (_interactionMode)
            {
                case InteractionMode.ResizingLeft:
                case InteractionMode.ResizeLeftAnd:
                    OnVideoPositionChanged.Invoke(this, new PositionEventArgs { PositionInSeconds = _activeParagraph.StartTime.TotalSeconds });
                    break;
                case InteractionMode.ResizingRight:
                case InteractionMode.ResizeRightAnd:
                    OnVideoPositionChanged.Invoke(this, new PositionEventArgs { PositionInSeconds = _activeParagraph.EndTime.TotalSeconds });
                    break;
            }
        }

        InvalidateVisual();
    }

    private static double SnapToFrame(double seconds)
    {
        if (!TryGetFrameDuration(out var frameDur))
        {
            return seconds;
        }

        return Math.Round(seconds / frameDur, MidpointRounding.AwayFromZero) * frameDur;
    }

    private static double SnapToFrameCeil(double seconds)
    {
        if (!TryGetFrameDuration(out var frameDur))
        {
            return seconds;
        }

        return Math.Ceiling(seconds / frameDur) * frameDur;
    }

    private static double SnapToFrameFloor(double seconds)
    {
        if (!TryGetFrameDuration(out var frameDur))
        {
            return seconds;
        }

        return Math.Floor(seconds / frameDur) * frameDur;
    }

    private static bool TryGetFrameDuration(out double frameDur)
    {
        frameDur = 0;
        if (!Se.Settings.Waveform.SnapToFrames)
        {
            return false;
        }

        var fps = Se.Settings.General.CurrentFrameRate;
        if (fps < 1)
        {
            return false;
        }

        frameDur = 1.0 / fps;
        return true;
    }

    /// <summary>
    /// Snap distance (seconds) for a paragraph IN-cue near a shot change, derived from
    /// the BeautifyTimeCodes profile's InCues red zones. Falls back to <see cref="ShotChangeSnapSeconds"/>
    /// when no profile / fps is available.
    /// </summary>
    private double GetInCueSnapSeconds()
    {
        var fps = Se.Settings.General.CurrentFrameRate;
        if (fps < 1)
        {
            return ShotChangeSnapSeconds;
        }

        var profile = Nikse.SubtitleEdit.Core.Common.Configuration.Settings.BeautifyTimeCodes?.Profile;
        if (profile == null)
        {
            return ShotChangeSnapSeconds;
        }

        var frames = Math.Max(profile.InCuesLeftRedZone, profile.InCuesRightRedZone);
        return frames > 0 ? frames / fps : ShotChangeSnapSeconds;
    }

    /// <summary>Snap distance (seconds) for a paragraph OUT-cue, derived from OutCues red zones.</summary>
    private double GetOutCueSnapSeconds()
    {
        var fps = Se.Settings.General.CurrentFrameRate;
        if (fps < 1)
        {
            return ShotChangeSnapSeconds;
        }

        var profile = Nikse.SubtitleEdit.Core.Common.Configuration.Settings.BeautifyTimeCodes?.Profile;
        if (profile == null)
        {
            return ShotChangeSnapSeconds;
        }

        var frames = Math.Max(profile.OutCuesLeftRedZone, profile.OutCuesRightRedZone);
        return frames > 0 ? frames / fps : ShotChangeSnapSeconds;
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
                    Cursor = _cursorArrow;
                }
                else
                {
                    Cursor = _cursorSizeWestEast;
                }
            }
            else
            {
                Cursor = _cursorHand;
            }
        }
        else
        {
            Cursor = _cursorArrow;
        }
    }

    private SubtitleLineViewModel? HitTestParagraph(Point point)
    {
        var pointX = point.X;
        var startPosSeconds = StartPositionSeconds;

        // Runs per pointer move for every displayable paragraph; SecondsToXPosition reads the
        // WavePeaks/ZoomFactor StyledProperties on each call, so resolve the factor once here.
        var xFactor = WavePeaks == null ? 0.0 : WavePeaks.SampleRate * ZoomFactor;
        int ToX(double seconds) => (int)Math.Round(seconds * xFactor, MidpointRounding.AwayFromZero);

        // Check NewSelectionParagraph first as it's typically the active interaction target
        var newSelection = NewSelectionParagraph;
        if (newSelection != null)
        {
            var left = ToX(newSelection.StartTime.TotalSeconds - startPosSeconds);
            var right = ToX(newSelection.EndTime.TotalSeconds - startPosSeconds);

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
            var left = ToX(p.StartTime.TotalSeconds - startPosSeconds);
            var right = ToX(p.EndTime.TotalSeconds - startPosSeconds);

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
                var prevRight = ToX(prev.EndTime.TotalSeconds - startPosSeconds);
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
                var nextLeft = ToX(next.StartTime.TotalSeconds - startPosSeconds);
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
        var width = Bounds.Width;
        var height = Bounds.Height;
        if (width <= 0 || height <= 0)
        {
            return;
        }

        var boundsRect = new Rect(0, 0, width, height);
        context.DrawRectangle(_paintBackground, null, boundsRect);

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

            if (WavePeaks == null && ShowClickToGenerateHint && !string.IsNullOrEmpty(ClickToGenerateText))
            {
                var hint = new FormattedText(
                    ClickToGenerateText,
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    Typeface.Default,
                    14,
                    Brushes.Gainsboro);
                context.DrawText(hint, new Point((width - hint.Width) / 2, (height - hint.Height) / 2));
            }

            if (IsFocused)
            {
                context.DrawRectangle(null, _paintPenSelected, boundsRect);
            }
        }
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
        using var avaloniaBitmap = skBitmapCombined.ToAvaloniaBitmap();

        var destRectangle = new Rect(0, renderCtx.Height - displayHeight, renderCtx.Width, displayHeight);
        context.DrawImage(avaloniaBitmap, destRectangle);
    }

    // Pooled buffers and FormattedText cache for the timeline ruler.
    private readonly List<FancyLine> _timeLineMajorTicks = new(128);
    private readonly List<FancyLine> _timeLineMinorTicks = new(128);
    private readonly Dictionary<string, FormattedText> _timeLineTextCache = new(256);

    private FormattedText GetCachedTimeLineText(string text)
    {
        if (!_timeLineTextCache.TryGetValue(text, out var formatted))
        {
            // Same cap as the paragraph text caches - in frame mode at high zoom every
            // distinct label is a new entry, so without a bound this grows for hours.
            if (_timeLineTextCache.Count > 8000)
            {
                _timeLineTextCache.Clear();
            }

            formatted = new FormattedText(
                text,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                _typeface,
                _fontSize - 1,
                _paintText);
            _timeLineTextCache[text] = formatted;
        }
        return formatted;
    }

    private void DrawTimeLine(DrawingContext context, ref RenderContext renderCtx)
    {
        if (renderCtx.SampleRate == 0)
        {
            return;
        }

        var fps = Se.Settings.General.CurrentFrameRate;
        if (Se.Settings.General.UseFrameMode && fps >= 1)
        {
            DrawFrameAlignedTimeLine(context, ref renderCtx, fps);
            return;
        }

        // Hoist loop-invariants: renderCtx is a ref struct, and n (pixels per second) was being
        // recomputed every iteration. With sampleRate != 0 guaranteed above, SecondsToXPositionOptimized
        // reduces to Math.Round(seconds * n), so inline it.
        var startPosSeconds = renderCtx.StartPositionSeconds;
        var n = renderCtx.ZoomFactor * renderCtx.SampleRate;
        var imageHeight = renderCtx.Height;
        var width = renderCtx.Width;

        var seconds = Math.Ceiling(startPosSeconds) - startPosSeconds;
        var position = (int)Math.Round(seconds * n, MidpointRounding.AwayFromZero);

        var majorTicks = _timeLineMajorTicks;
        var minorTicks = _timeLineMinorTicks;
        majorTicks.Clear();
        minorTicks.Clear();

        var drawMinor = n > 64;

        // Collect ticks first; draw text directly with cached FormattedText (single
        // DrawText call per label is fine — the per-frame cost was the FormattedText
        // allocation, not the DrawText call).
        while (position < width)
        {
            if (n > 38 || (int)Math.Round(startPosSeconds + seconds) % 5 == 0)
            {
                majorTicks.Add(new FancyLine(position, imageHeight - 10, imageHeight));

                var timeText = GetDisplayTime(startPosSeconds + seconds);
                var formattedText = GetCachedTimeLineText(timeText);
                var textY = Math.Max(0, imageHeight - formattedText.Height - 2);
                context.DrawText(formattedText, new Point(position + 2, textY));
            }

            seconds += 0.5;
            position = (int)Math.Round(seconds * n, MidpointRounding.AwayFromZero);

            if (drawMinor)
            {
                minorTicks.Add(new FancyLine(position, imageHeight - 5, imageHeight));
            }

            seconds += 0.5;
            position = (int)Math.Round(seconds * n, MidpointRounding.AwayFromZero);
        }

        DrawVerticalLineBatch(context, _paintTimeLine, majorTicks);
        DrawVerticalLineBatch(context, _paintTimeLine, minorTicks);
    }

    private void DrawFrameAlignedTimeLine(DrawingContext context, ref RenderContext renderCtx, double fps)
    {
        var imageHeight = renderCtx.Height;
        var width = renderCtx.Width;
        var pixelsPerFrame = renderCtx.SampleRate * renderCtx.ZoomFactor / fps;
        if (pixelsPerFrame <= 0)
        {
            return;
        }

        // Mirror the time-mode density: majors >= 38 px apart, minors at half-step
        // (and only when the half-step lands on a whole frame and stays >= 24 px apart).
        var majorStepFrames = PickFramesPerStep(pixelsPerFrame, 38);
        var minorStepFrames = (majorStepFrames > 1 && majorStepFrames % 2 == 0 && pixelsPerFrame * (majorStepFrames / 2) >= 24)
            ? majorStepFrames / 2
            : 0;

        var majorTicks = _timeLineMajorTicks;
        var minorTicks = _timeLineMinorTicks;
        majorTicks.Clear();
        minorTicks.Clear();

        // First major boundary (absolute frame index) at-or-before the visible start.
        // Use integer-space floor: double-space Math.Floor near a frame boundary can
        // land on 24.9999... instead of 25, offsetting every tick by a full step.
        var startFrame = FloorWithEpsilon(renderCtx.StartPositionSeconds * fps);
        if (startFrame < 0)
        {
            startFrame = 0;
        }

        // The x position is linear in the frame index: x = absFrame * pixelsPerFrame -
        // startPixelOffset. Precompute the invariants so each tick is a multiply-subtract instead of
        // a per-tick division + SecondsToXPositionOptimized call (which re-reads the ref-struct
        // renderCtx fields). invFps is only needed for the visible labels' time value.
        var startPixelOffset = renderCtx.StartPositionSeconds * renderCtx.SampleRate * renderCtx.ZoomFactor;
        var invFps = 1.0 / fps;

        var firstAbsFrame = startFrame - startFrame % majorStepFrames;
        for (var absFrame = firstAbsFrame; ; absFrame += majorStepFrames)
        {
            var xPosition = (int)Math.Round(absFrame * pixelsPerFrame - startPixelOffset, MidpointRounding.AwayFromZero);
            if (xPosition >= width)
            {
                break;
            }

            if (xPosition >= 0)
            {
                majorTicks.Add(new FancyLine(xPosition, imageHeight - 10, imageHeight));

                var timeText = GetFrameDisplayTime(absFrame * invFps);
                var formattedText = GetCachedTimeLineText(timeText);
                var textY = Math.Max(0, imageHeight - formattedText.Height - 2);
                context.DrawText(formattedText, new Point(xPosition + 2, textY));
            }

            if (minorStepFrames > 0)
            {
                var minorX = (int)Math.Round((absFrame + minorStepFrames) * pixelsPerFrame - startPixelOffset, MidpointRounding.AwayFromZero);
                if (minorX >= 0 && minorX < width)
                {
                    minorTicks.Add(new FancyLine(minorX, imageHeight - 5, imageHeight));
                }
            }
        }

        DrawVerticalLineBatch(context, _paintTimeLine, majorTicks);
        DrawVerticalLineBatch(context, _paintTimeLine, minorTicks);
    }

    private static string GetFrameDisplayTime(double seconds)
    {
        // CurrentVideoOffsetInMs is signed; the previous `> 0.00001` guard silently
        // dropped negative offsets so the timeline labels were wrong whenever the
        // audio leads the video.
        if (Math.Abs(Se.Settings.General.CurrentVideoOffsetInMs) > 0.00001)
        {
            seconds = seconds + Se.Settings.General.CurrentVideoOffsetInMs / 1000.0;
        }

        return new TimeCode(seconds * 1000.0).ToShortStringHHMMSSFF();
    }

    private readonly Pen _paintTimeLine = new Pen(Brushes.Gray, 1);

    private static string GetDisplayTime(double seconds)
    {
        if (Math.Abs(Se.Settings.General.CurrentVideoOffsetInMs) > 0.00001)
        {
            seconds = seconds + Se.Settings.General.CurrentVideoOffsetInMs / 1000.0;
        }

        // SE 4 parity: zero-pad minutes/hours so the labels keep a stable width across the
        // ruler (e.g. "02:05" instead of "2:05", "01:23:45" instead of "1:23:45").
        //
        // Work in the absolute domain and prepend a single sign so a negative pan or
        // negative video offset produces "-01:23:45" rather than "-01:-23:-45"; and use
        // TotalHours to include the day component, otherwise a >24h video wraps to "00:…".
        var sign = seconds < 0 ? "-" : string.Empty;
        var abs = Math.Abs(seconds);
        var totalHours = (int)(abs / 3600);
        var ts = TimeSpan.FromSeconds(abs);
        var minutes = ts.Minutes;
        var secs = ts.Seconds;

        if (totalHours == 0 && minutes == 0)
        {
            return $"{sign}{secs.ToString(CultureInfo.InvariantCulture)}";
        }

        if (totalHours == 0)
        {
            return $"{sign}{minutes:00}:{secs:00}";
        }

        return $"{sign}{totalHours:00}:{minutes:00}:{secs:00}";
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

    // Pooled x positions for the grid, so the per-frame collection below doesn't allocate.
    private readonly List<double> _gridLineXPositions = new(256);

    private void DrawAllGridLines(DrawingContext context, ref RenderContext renderCtx)
    {
        if (!DrawGridLines)
        {
            return;
        }

        var width = renderCtx.Width;
        var height = renderCtx.Height;

        // Pixel spacing between adjacent vertical lines; reused for the horizontal lines so the
        // grid stays square, matching the look users are familiar with from SE4.
        double stepPixels;

        var xPositions = _gridLineXPositions;
        xPositions.Clear();

        if (renderCtx.SampleRate == 0)
        {
            stepPixels = 10;
            for (var i = 0d; i < width; i += stepPixels)
            {
                xPositions.Add(i);
            }
        }
        else if (Se.Settings.General.UseFrameMode && Se.Settings.General.CurrentFrameRate >= 1)
        {
            var fps = Se.Settings.General.CurrentFrameRate;
            var pixelsPerFrame = renderCtx.SampleRate * renderCtx.ZoomFactor / fps;
            if (pixelsPerFrame <= 0)
            {
                return;
            }

            var framesPerStep = PickFramesPerStep(pixelsPerFrame, minPixelGap: 8);
            stepPixels = framesPerStep * pixelsPerFrame;

            // First frame boundary (absolute frame index) at-or-before the visible start.
            // Compute the start frame in integer space — Math.Floor in double space can land
            // on the wrong side of a boundary when StartPositionSeconds * fps rounds to
            // 24.9999... instead of exactly 25.0, which would offset every gridline by a step.
            var startFrame = FloorWithEpsilon(renderCtx.StartPositionSeconds * fps);
            if (startFrame < 0)
            {
                startFrame = 0;
            }

            var firstAbsFrame = startFrame - startFrame % framesPerStep;
            for (var absFrame = firstAbsFrame; ; absFrame += framesPerStep)
            {
                var relSeconds = absFrame / fps - renderCtx.StartPositionSeconds;
                var xPosition = SecondsToXPositionOptimized(relSeconds, renderCtx.SampleRate, renderCtx.ZoomFactor);
                if (xPosition >= width)
                {
                    break;
                }

                if (xPosition >= 0)
                {
                    xPositions.Add(xPosition);
                }
            }
        }
        else
        {
            var seconds = Math.Ceiling(renderCtx.StartPositionSeconds) - renderCtx.StartPositionSeconds - 1;
            var xPosition = SecondsToXPositionOptimized(seconds, renderCtx.SampleRate, renderCtx.ZoomFactor);
            var interval = renderCtx.ZoomFactor >= 0.4d
                ? 0.1d
                : // a pixel is 0.1 second
                1.0d; // a pixel is 1.0 second
            stepPixels = interval * renderCtx.SampleRate * renderCtx.ZoomFactor;

            while (xPosition < width)
            {
                xPositions.Add(xPosition);
                seconds += interval;
                xPosition = SecondsToXPositionOptimized(seconds, renderCtx.SampleRate, renderCtx.ZoomFactor);
            }
        }

        // Batch the whole grid into one geometry - hundreds of individual DrawLine calls per
        // frame add real scene-graph overhead at 60 fps (same approach as the timeline ticks).
        var drawHorizontal = stepPixels >= 1;
        if (xPositions.Count == 0 && !drawHorizontal)
        {
            return;
        }

        var geom = new StreamGeometry();
        using (var gctx = geom.Open())
        {
            for (var i = 0; i < xPositions.Count; i++)
            {
                var x = xPositions[i];
                gctx.BeginFigure(new Point(x, 0), false);
                gctx.LineTo(new Point(x, height));
                gctx.EndFigure(false);
            }

            if (drawHorizontal)
            {
                for (var y = stepPixels; y < height; y += stepPixels)
                {
                    gctx.BeginFigure(new Point(0, y), false);
                    gctx.LineTo(new Point(width, y));
                    gctx.EndFigure(false);
                }
            }
        }

        context.DrawGeometry(null, _paintGridLines, geom);
    }

    private static readonly int[] FrameStepCandidates = { 1, 2, 5, 10, 25, 50, 100, 250, 500, 1000, 2500, 5000 };

    private static int PickFramesPerStep(double pixelsPerFrame, double minPixelGap)
    {
        foreach (var c in FrameStepCandidates)
        {
            if (pixelsPerFrame * c >= minPixelGap)
            {
                return c;
            }
        }
        return FrameStepCandidates[^1];
    }

    // floor(value) with a small positive epsilon so a value that should be an integer
    // but came out of double math as integer - 1e-12 still floors up correctly.
    private static long FloorWithEpsilon(double value) => (long)Math.Floor(value + 1e-6);

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

        // Rebuild the cached geometry only when the view actually changed; otherwise (e.g. the
        // cursor moved) just replay the existing draw ops. See _waveformCacheDraws.
        var key = BuildWaveformCacheKey(waveformHeight, ref renderCtx);
        if (!_waveformCacheValid || !ReferenceEquals(_waveformCachePeaks, WavePeaks) || !key.Equals(_waveformCacheKey))
        {
            _waveformCacheDraws.Clear();
            if (WaveformDrawStyle == WaveformDrawStyle.Classic)
            {
                BuildWaveFormClassic(waveformHeight, ref renderCtx);
            }
            else
            {
                BuildWaveFormFancy(waveformHeight, ref renderCtx);
            }

            _waveformCacheKey = key;
            _waveformCachePeaks = WavePeaks;
            _waveformCacheValid = true;
        }

        // The fancy style draws a center line; it depends only on width/height (already in the
        // key) so it's cheap to draw live each render rather than caching it.
        if (WaveformDrawStyle != WaveformDrawStyle.Classic)
        {
            var halfWaveformHeight = waveformHeight / 2;
            context.DrawLine(_centerLinePen, new Point(0, halfWaveformHeight), new Point(renderCtx.Width, halfWaveformHeight));
        }

        for (var i = 0; i < _waveformCacheDraws.Count; i++)
        {
            var draw = _waveformCacheDraws[i];
            context.DrawGeometry(null, draw.Pen, draw.Geometry);
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

    private sealed class FancyBatch
    {
        public Pen Pen;
        public readonly List<FancyLine> Lines = new(256);
        public FancyBatch(Pen pen) { Pen = pen; }
    }

    private readonly record struct FancyLine(double X, double YMax, double YMin);

    // Pooled across renders to avoid per-render dictionary/list allocation.
    private readonly Dictionary<int, FancyBatch> _fancyBatches = new(64);
    private readonly List<int> _fancyBatchKeysInUse = new(64);

    private void BuildWaveFormFancy(double waveformHeight, ref RenderContext renderCtx)
    {
        _isSelectedHelper.Reset(AllSelectedParagraphs, renderCtx.SampleRate);
        var isSelectedHelper = _isSelectedHelper;
        var halfWaveformHeight = waveformHeight / 2;
        var div = renderCtx.SampleRate * renderCtx.ZoomFactor;

        if (div <= 0 || WavePeaks == null)
        {
            return;
        }

        // Hot loop: get a Span over the peaks array so the per-pixel indexer skips
        // the IList<T> interface dispatch.
        var peaks = WavePeaks.AsSpan();
        var peaksCount = peaks.Length;
        var highestPeak = renderCtx.HighestPeak;

        // Hoist loop-invariant RenderContext fields into locals: it is a ref struct, so the JIT
        // cannot cache these reads across the loop. Also turn the per-pixel sample position into a
        // multiply (startSample + x * samplesPerPixel) instead of a division (x / div).
        var width = renderCtx.Width;
        var height = renderCtx.Height;
        var verticalZoomFactor = renderCtx.VerticalZoomFactor;
        var startSample = renderCtx.StartPositionSeconds * renderCtx.SampleRate;
        var samplesPerPixel = renderCtx.SampleRate / div;
        var yScaleHalf = verticalZoomFactor / highestPeak * halfWaveformHeight;

        // Calculate the threshold for color transitions (as a fraction of the highest peak)
        var lowThreshold = highestPeak * 0.3;
        var mediumThreshold = highestPeak * 0.6;
        // Reciprocals of the (loop-invariant) blend ranges, so the per-pixel colour math multiplies
        // instead of dividing.
        var invMediumMinusLow = 1.0 / (mediumThreshold - lowThreshold);
        var invHighMinusMedium = 1.0 / (highestPeak - mediumThreshold);

        // StyledProperty getters go through Avalonia's value store - keep them out of the
        // per-pixel loop below.
        var waveformColor = WaveformColor;
        var waveformSelectedColor = WaveformSelectedColor;
        var waveformFancyHighColor = WaveformFancyHighColor;

        // Reset pooled batches for this render
        var batches = _fancyBatches;
        var keysInUse = _fancyBatchKeysInUse;
        for (var i = 0; i < keysInUse.Count; i++)
        {
            batches[keysInUse[i]].Lines.Clear();
        }
        keysInUse.Clear();

        for (var x = 0; x < width; x++)
        {
            var pos = startSample + x * samplesPerPixel;
            var pos0 = (int)pos;
            var pos1 = pos0 + 1;

            if (pos1 >= peaksCount)
            {
                break;
            }

            var pos1Weight = pos - pos0;
            var pos0Weight = 1.0 - pos1Weight;
            var peak0 = peaks[pos0];
            var peak1 = peaks[pos1];
            var max = peak0.Max * pos0Weight + peak1.Max * pos1Weight;
            var min = peak0.Min * pos0Weight + peak1.Min * pos1Weight;

            var yMax = CalculateYOptimized(max, halfWaveformHeight, yScaleHalf, height);
            var yMin = CalculateYOptimized(min, halfWaveformHeight, yScaleHalf, height);

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
            var aMax = Math.Abs(max);
            var aMin = Math.Abs(min);
            var amplitude = aMax > aMin ? aMax : aMin;

            // Determine color based on amplitude and create a cache key
            Color color;
            int colorKey;

            // Determine base color (selected or normal)
            var baseColor = isSelected ? waveformSelectedColor : waveformColor;
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
                var blend = (amplitude - lowThreshold) * invMediumMinusLow;
                var highColor = waveformFancyHighColor;
                var r = (byte)(baseColor.R + blend * (highColor.R - baseColor.R));
                var g = (byte)(baseColor.G + blend * (highColor.G - baseColor.G));
                var b = (byte)(baseColor.B + blend * (highColor.B - baseColor.B));
                var a = (byte)(baseColor.A + blend * (highColor.A - baseColor.A));
                color = Color.FromArgb(a, r, g, b);
                // Quantize blend to 10 steps for caching
                colorKey = baseColorKeyOffset + 1 + (int)(blend * 10);
            }
            else
            {
                // High amplitude - use high color with increased opacity
                var blend = Math.Min(1.0, (amplitude - mediumThreshold) * invHighMinusMedium);
                var highColor = waveformFancyHighColor;
                var a = (byte)Math.Min(255, highColor.A + blend * (255 - highColor.A));
                color = Color.FromArgb(a, highColor.R, highColor.G, highColor.B);
                // Quantize blend to 10 steps for caching
                colorKey = baseColorKeyOffset + 12 + (int)(blend * 10);
            }

            // Append to per-pen bucket
            if (!batches.TryGetValue(colorKey, out var mainBatch))
            {
                mainBatch = new FancyBatch(GetCachedFancyWaveformPen(colorKey, color));
                batches[colorKey] = mainBatch;
            }
            if (mainBatch.Lines.Count == 0)
            {
                keysInUse.Add(colorKey);
            }
            mainBatch.Lines.Add(new FancyLine(x, yMax, yMin));

            // Add subtle glow for higher amplitudes
            if (amplitude > mediumThreshold)
            {
                var glowAlpha = (byte)(50 * ((amplitude - mediumThreshold) * invHighMinusMedium));
                var glowColor = Color.FromArgb(glowAlpha, color.R, color.G, color.B);
                // Quantize glow alpha to 5 steps for caching
                var glowKey = 1000 + colorKey * 10 + (glowAlpha / 10);
                if (!batches.TryGetValue(glowKey, out var glowBatch))
                {
                    glowBatch = new FancyBatch(GetCachedFancyWaveformGlowPen(glowKey, glowColor, 3.0));
                    batches[glowKey] = glowBatch;
                }
                if (glowBatch.Lines.Count == 0)
                {
                    keysInUse.Add(glowKey);
                }
                glowBatch.Lines.Add(new FancyLine(x, yMax - 0.5, yMin + 0.5));
            }
        }

        // One cached geometry per pen, instead of two DrawLines per column.
        // The gradient brush bounds become the entire waveform area instead of each
        // individual line, so the gradient fades across the waveform vertically rather
        // than per column.
        for (var i = 0; i < keysInUse.Count; i++)
        {
            var batch = batches[keysInUse[i]];
            AddLineBatchToCache(batch.Pen, batch.Lines);
        }
    }

    private void BuildWaveFormClassic(double waveformHeight, ref RenderContext renderCtx)
    {
        _isSelectedHelper.Reset(AllSelectedParagraphs, renderCtx.SampleRate);
        var isSelectedHelper = _isSelectedHelper;
        var halfWaveformHeight = waveformHeight / 2;
        var div = renderCtx.SampleRate * renderCtx.ZoomFactor;

        if (div <= 0 || WavePeaks == null)
        {
            return;
        }

        // See BuildWaveFormFancy: skip IList<T> interface dispatch in the per-pixel loop.
        var peaks = WavePeaks.AsSpan();
        var peaksCount = peaks.Length;

        // Hoist loop-invariant RenderContext fields (it is a ref struct) and replace the per-pixel
        // division for the sample position with a multiply. See BuildWaveFormFancy.
        var width = renderCtx.Width;
        var height = renderCtx.Height;
        var highestPeak = renderCtx.HighestPeak;
        var verticalZoomFactor = renderCtx.VerticalZoomFactor;
        var startSample = renderCtx.StartPositionSeconds * renderCtx.SampleRate;
        var samplesPerPixel = renderCtx.SampleRate / div;
        var yScaleHalf = verticalZoomFactor / highestPeak * halfWaveformHeight;

        var unselectedLines = _classicUnselectedLines;
        var selectedLines = _classicSelectedLines;
        unselectedLines.Clear();
        selectedLines.Clear();

        for (var x = 0; x < width; x++)
        {
            var pos = startSample + x * samplesPerPixel;
            var pos0 = (int)pos;
            var pos1 = pos0 + 1;

            if (pos1 >= peaksCount)
            {
                break;
            }

            var pos1Weight = pos - pos0;
            var pos0Weight = 1.0 - pos1Weight;
            var peak0 = peaks[pos0];
            var peak1 = peaks[pos1];
            var max = peak0.Max * pos0Weight + peak1.Max * pos1Weight;
            var min = peak0.Min * pos0Weight + peak1.Min * pos1Weight;

            var yMax = CalculateYOptimized(max, halfWaveformHeight, yScaleHalf, height);
            var yMin = CalculateYOptimized(min, halfWaveformHeight, yScaleHalf, height);

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

            var line = new FancyLine(x, yMax, yMin);
            if (isSelectedHelper.IsSelected(pos0))
            {
                selectedLines.Add(line);
            }
            else
            {
                unselectedLines.Add(line);
            }
        }

        AddLineBatchToCache(_paintWaveform, unselectedLines);
        AddLineBatchToCache(_paintPenSelected, selectedLines);
    }

    // Pooled buffers for the classic waveform's two pens.
    private readonly List<FancyLine> _classicUnselectedLines = new(2048);
    private readonly List<FancyLine> _classicSelectedLines = new(2048);

    // Cached waveform draw ops. Building the waveform is a per-pixel CPU loop that allocates
    // geometry every render; doing it on every cursor tick (CurrentVideoPositionSeconds has
    // AffectsRender) made playback stutter, worst over loud audio (extra glow geometry). We
    // build the geometry once per view-state and just replay the draw calls while only the
    // cursor moves. The key captures everything that changes the waveform pixels, so any real
    // change (scroll, zoom, resize, selection, peaks, colors, style) rebuilds automatically.
    private readonly List<(IPen Pen, Geometry Geometry)> _waveformCacheDraws = new(64);
    private WaveformCacheKey _waveformCacheKey;
    private bool _waveformCacheValid;
    private object? _waveformCachePeaks;

    private readonly struct WaveformCacheKey : IEquatable<WaveformCacheKey>
    {
        public readonly int Width;
        public readonly double Height;
        public readonly double WaveformHeight;
        public readonly double StartPositionSeconds;
        public readonly double ZoomFactor;
        public readonly double VerticalZoomFactor;
        public readonly double HighestPeak;
        public readonly int SampleRate;
        public readonly int DisplayMode;
        public readonly int DrawStyle;
        public readonly uint ColorMain;
        public readonly uint ColorSelected;
        public readonly uint ColorHigh;
        public readonly int SelectionCount;
        public readonly long SelectionHash;

        public WaveformCacheKey(int width, double height, double waveformHeight, double startPositionSeconds,
            double zoomFactor, double verticalZoomFactor, double highestPeak, int sampleRate, int displayMode,
            int drawStyle, uint colorMain, uint colorSelected, uint colorHigh, int selectionCount, long selectionHash)
        {
            Width = width;
            Height = height;
            WaveformHeight = waveformHeight;
            StartPositionSeconds = startPositionSeconds;
            ZoomFactor = zoomFactor;
            VerticalZoomFactor = verticalZoomFactor;
            HighestPeak = highestPeak;
            SampleRate = sampleRate;
            DisplayMode = displayMode;
            DrawStyle = drawStyle;
            ColorMain = colorMain;
            ColorSelected = colorSelected;
            ColorHigh = colorHigh;
            SelectionCount = selectionCount;
            SelectionHash = selectionHash;
        }

        public bool Equals(WaveformCacheKey other) =>
            Width == other.Width &&
            Height.Equals(other.Height) &&
            WaveformHeight.Equals(other.WaveformHeight) &&
            StartPositionSeconds.Equals(other.StartPositionSeconds) &&
            ZoomFactor.Equals(other.ZoomFactor) &&
            VerticalZoomFactor.Equals(other.VerticalZoomFactor) &&
            HighestPeak.Equals(other.HighestPeak) &&
            SampleRate == other.SampleRate &&
            DisplayMode == other.DisplayMode &&
            DrawStyle == other.DrawStyle &&
            ColorMain == other.ColorMain &&
            ColorSelected == other.ColorSelected &&
            ColorHigh == other.ColorHigh &&
            SelectionCount == other.SelectionCount &&
            SelectionHash == other.SelectionHash;

        public override bool Equals(object? obj) => obj is WaveformCacheKey other && Equals(other);
        public override int GetHashCode() => 0; // unused; cache does an exact Equals comparison
    }

    private static uint ToKeyColor(Color c) => ((uint)c.A << 24) | ((uint)c.R << 16) | ((uint)c.G << 8) | c.B;

    private WaveformCacheKey BuildWaveformCacheKey(double waveformHeight, ref RenderContext renderCtx)
    {
        long selectionHash = 17;
        var selection = AllSelectedParagraphs;
        for (var i = 0; i < selection.Count; i++)
        {
            var p = selection[i];
            selectionHash = selectionHash * 31 + p.StartTime.Ticks;
            selectionHash = selectionHash * 31 + p.EndTime.Ticks;
        }

        return new WaveformCacheKey(
            (int)Math.Ceiling(renderCtx.Width),
            renderCtx.Height,
            waveformHeight,
            renderCtx.StartPositionSeconds,
            renderCtx.ZoomFactor,
            renderCtx.VerticalZoomFactor,
            renderCtx.HighestPeak,
            renderCtx.SampleRate,
            (int)_displayMode,
            (int)WaveformDrawStyle,
            ToKeyColor(WaveformColor),
            ToKeyColor(WaveformSelectedColor),
            ToKeyColor(WaveformFancyHighColor),
            selection.Count,
            selectionHash);
    }

    private void AddLineBatchToCache(IPen pen, List<FancyLine> lines)
    {
        if (lines.Count == 0)
        {
            return;
        }

        var geom = new StreamGeometry();
        using (var gctx = geom.Open())
        {
            for (var i = 0; i < lines.Count; i++)
            {
                var l = lines[i];
                gctx.BeginFigure(new Point(l.X, l.YMax), false);
                gctx.LineTo(new Point(l.X, l.YMin));
                gctx.EndFigure(false);
            }
        }

        _waveformCacheDraws.Add((pen, geom));
    }

    private static void DrawVerticalLineBatch(DrawingContext context, IPen pen, List<FancyLine> lines)
    {
        if (lines.Count == 0)
        {
            return;
        }

        var geom = new StreamGeometry();
        using (var gctx = geom.Open())
        {
            for (var i = 0; i < lines.Count; i++)
            {
                var l = lines[i];
                gctx.BeginFigure(new Point(l.X, l.YMax), false);
                gctx.LineTo(new Point(l.X, l.YMin));
                gctx.EndFigure(false);
            }
        }
        context.DrawGeometry(null, pen, geom);
    }

    private void DrawParagraphs(DrawingContext context, ref RenderContext renderCtx)
    {
        var paragraphs = _displayableParagraphs;
        var startPositionMilliseconds = renderCtx.StartPositionSeconds * 1000.0;
        var endPositionMilliseconds = RelativeXPositionToSecondsOptimized(renderCtx.Width, renderCtx.SampleRate, renderCtx.StartPositionSeconds, renderCtx.ZoomFactor) * 1000.0;

        // List.Contains per visible paragraph is O(selection) - with "select all" on a large
        // subtitle that is millions of compares per frame, so probe a set instead.
        _selectedParagraphsRenderSet.Clear();
        var allSelected = AllSelectedParagraphs;
        if (allSelected != null)
        {
            foreach (var selected in allSelected)
            {
                _selectedParagraphsRenderSet.Add(selected);
            }
        }

        foreach (var p in paragraphs)
        {
            if (p.EndTime.TotalMilliseconds >= startPositionMilliseconds && p.StartTime.TotalMilliseconds <= endPositionMilliseconds)
            {
                DrawParagraph(p, context, ref renderCtx);
            }
        }
    }

    // Subtitle text is otherwise re-shaped on every frame. At 60 fps the FormattedText shaping
    // plus RemoveHtmlTags (regex) and SplitToLines for each visible paragraph churns enough
    // short-lived garbage to trigger GC pauses, which show up as the cursor briefly freezing and
    // then jumping. Cache the prepared text and the shaped FormattedText; both are cleared in
    // ResetCache() when the waveform font/colors change.
    private readonly Dictionary<string, FormattedText> _paragraphFormattedTextCache = new(512);
    private readonly Dictionary<string, (List<string> Lines, string Unwrapped)> _paragraphTextCache = new(512);

    private FormattedText GetCachedParagraphText(string text)
    {
        if (!_paragraphFormattedTextCache.TryGetValue(text, out var formatted))
        {
            if (_paragraphFormattedTextCache.Count > 8000)
            {
                _paragraphFormattedTextCache.Clear();
            }

            formatted = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                _typeface, _fontSize, _paintText);
            _paragraphFormattedTextCache[text] = formatted;
        }

        return formatted;
    }

    private (List<string> Lines, string Unwrapped) GetPreparedParagraphText(string rawText)
    {
        if (!_paragraphTextCache.TryGetValue(rawText, out var prepared))
        {
            var text = HtmlUtil.RemoveHtmlTags(rawText, true);
            if (text.Length > 200)
            {
                text = text.Substring(0, 100).TrimEnd() + "...";
            }

            var lines = text.SplitToLines();
            var unwrapped = string.Join("  ", lines);
            prepared = (lines, unwrapped);

            if (_paragraphTextCache.Count > 8000)
            {
                _paragraphTextCache.Clear();
            }

            _paragraphTextCache[rawText] = prepared;
        }

        return prepared;
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
        context.FillRectangle(_selectedParagraphsRenderSet.Contains(paragraph) ? _paintParagraphSelectedBackground : _paintParagraphBackground,
            new Rect(currentRegionLeft, 0, currentRegionWidth, height));

        // Draw left and right borders
        context.DrawLine(_paintLeft, new Point(currentRegionLeft, 0), new Point(currentRegionLeft, height));
        context.DrawLine(_paintRight, new Point(currentRegionRight - 1, 0), new Point(currentRegionRight - 1, height));

        // Draw clipped text (prepared text + shaped FormattedText are cached; see GetCachedParagraphText)
        var prepared = GetPreparedParagraphText(paragraph.Text);

        var textBounds = new Rect(currentRegionLeft + 1, 0, currentRegionWidth - 3, height);

        using (context.PushClip(textBounds))
        {
            if (Se.Settings.Waveform.WaveformUnwrapText)
            {
                var formattedText = GetCachedParagraphText(prepared.Unwrapped);
                context.DrawText(formattedText, new Point(currentRegionLeft + 3, 14));
            }
            else
            {
                double addY = 0;
                foreach (var line in prepared.Lines)
                {
                    var formattedText = GetCachedParagraphText(line);
                    context.DrawText(formattedText, new Point(currentRegionLeft + 3, 14 + addY));
                    addY += formattedText.Height;
                }
            }

            DrawParagraphFooter(context, paragraph, currentRegionLeft, currentRegionWidth, height, ref renderCtx);
        }
    }

    // SE 4 parity: small footer at the bottom-left of each paragraph rectangle with
    // up to three rows: characters-per-second (top), then "#NUMBER  DURATION".
    //
    // Zoom thresholds match SE 4 (n = samplesPerPixel = zoomFactor * sampleRate):
    //   n <= 15  → nothing (too zoomed out, the rectangle is barely visible)
    //   n <= 51  OR  "#N  Duration" wouldn't fit                       → just "#NUMBER"
    //   51 < n <= 99                                                   → "#NUMBER  DURATION"
    //   n > 99                                                         → add CPS line above
    private void DrawParagraphFooter(DrawingContext context, SubtitleLineViewModel paragraph,
        double currentRegionLeft, double currentRegionWidth, double height, ref RenderContext renderCtx)
    {
        if (!Se.Settings.Waveform.WaveformShowNumberAndDuration && !Se.Settings.Waveform.WaveformShowCps)
        {
            return;
        }

        var n = renderCtx.ZoomFactor * renderCtx.SampleRate;
        if (n <= 15)
        {
            return;
        }

        const double padding = 3;
        var availableWidth = currentRegionWidth - padding - 1;

        string? baseLine = null;
        if (Se.Settings.Waveform.WaveformShowNumberAndDuration)
        {
            // At narrow zoom we already know we're falling back to just "#N", so don't pay
            // for the FormattedText probe and TimeCode formatting that compute the wider
            // "#N  Duration" candidate.
            if (n <= 51)
            {
                baseLine = $"#{paragraph.Number}";
            }
            else
            {
                // ToShortDisplayString consults the libse UseTimeFormatHHMMSSFF flag, which SE 5
                // mirrors from Se.Settings.General.UseFrameMode (Se.cs:409). So flipping frame
                // mode on automatically switches this label between the time form ("2,500") and
                // the frame form ("00:00:02:12") without an explicit branch here.
                var durationText = new TimeCode(paragraph.Duration.TotalMilliseconds).ToShortDisplayString();
                var withDuration = $"#{paragraph.Number}  {durationText}";
                var probe = GetCachedParagraphText(withDuration);

                baseLine = probe.Width >= availableWidth
                    ? $"#{paragraph.Number}"
                    : withDuration;
            }
        }

        string? cpsLine = null;
        if (n > 99 && Se.Settings.Waveform.WaveformShowCps && paragraph.Duration.TotalMilliseconds > 0)
        {
            cpsLine = $"{paragraph.CharactersPerSecond:0.00}";
        }

        if (baseLine == null && cpsLine == null)
        {
            return;
        }

        // Layout from the bottom up so the optional CPS line stacks above the base line.
        var bottomY = height - 14;
        var x = currentRegionLeft + padding;

        if (baseLine != null)
        {
            var baseText = GetCachedParagraphText(baseLine);
            var baseY = bottomY - baseText.Height;
            context.DrawText(baseText, new Point(x, baseY));

            if (cpsLine != null)
            {
                var cpsText = GetCachedParagraphText(cpsLine);
                context.DrawText(cpsText, new Point(x, baseY - cpsText.Height));
            }
        }
        else if (cpsLine != null)
        {
            var cpsText = GetCachedParagraphText(cpsLine);
            context.DrawText(cpsText, new Point(x, bottomY - cpsText.Height));
        }
    }

    private void DrawShotChanges(DrawingContext context, ref RenderContext renderCtx)
    {
        if (_shotChanges.Count == 0)
        {
            return;
        }

        var currentPositionPos = SecondsToXPositionOptimized(renderCtx.CurrentVideoPositionSeconds - renderCtx.StartPositionSeconds, renderCtx.SampleRate, renderCtx.ZoomFactor);

        var startPositionMilliseconds = renderCtx.StartPositionSeconds * 1000.0;
        var endPositionMilliseconds = RelativeXPositionToSecondsOptimized(renderCtx.Width, renderCtx.SampleRate, renderCtx.StartPositionSeconds, renderCtx.ZoomFactor) * 1000.0;
        _paragraphStartPositions.Clear();
        _paragraphEndPositions.Clear();
        foreach (var p in _displayableParagraphs)
        {
            if (p.EndTime.TotalMilliseconds >= startPositionMilliseconds && p.StartTime.TotalMilliseconds <= endPositionMilliseconds)
            {
                _paragraphStartPositions.Add(SecondsToXPositionOptimized(p.StartTime.TotalSeconds - renderCtx.StartPositionSeconds, renderCtx.SampleRate, renderCtx.ZoomFactor));
                _paragraphEndPositions.Add(SecondsToXPositionOptimized(p.EndTime.TotalSeconds - renderCtx.StartPositionSeconds, renderCtx.SampleRate, renderCtx.ZoomFactor));
            }
        }

        // The list is sorted, so binary search the first shot change at/after the visible window
        // instead of walking all shot changes before it, and stop once past the right edge.
        var low = 0;
        var high = _shotChanges.Count;
        while (low < high)
        {
            var mid = low + (high - low) / 2;
            if (_shotChanges[mid] < renderCtx.StartPositionSeconds)
            {
                low = mid + 1;
            }
            else
            {
                high = mid;
            }
        }

        for (var index = low; index < _shotChanges.Count; index++)
        {
            var time = _shotChanges[index];
            var pos = SecondsToXPositionOptimized(time - renderCtx.StartPositionSeconds, renderCtx.SampleRate, renderCtx.ZoomFactor);

            if (pos >= renderCtx.Width)
            {
                break;
            }

            if (pos > 0)
            {
                if (currentPositionPos == pos)
                {
                    // shot change and current pos are the same
                    context.DrawLine(_paintShotChangeThickPen, new Point(pos, 0), new Point(pos, renderCtx.Height));
                    context.DrawLine(_paintPenCursor, new Point(pos, 0), new Point(pos, renderCtx.Height));
                }
                else if (_paragraphStartPositions.Contains(pos))
                {
                    context.DrawLine(_paintShotChangeThickPen, new Point(pos, 0), new Point(pos, renderCtx.Height));
                    context.DrawLine(_paintShotChangeParagraphStartPen, new Point(pos, 0), new Point(pos, renderCtx.Height));
                }
                else if (_paragraphEndPositions.Contains(pos))
                {
                    context.DrawLine(_paintShotChangeThickPen, new Point(pos, 0), new Point(pos, renderCtx.Height));
                    context.DrawLine(_paintShotChangeParagraphEndPen, new Point(pos, 0), new Point(pos, renderCtx.Height));
                }
                else
                {
                    context.DrawLine(_paintShotChangeThinPen, new Point(pos, 0), new Point(pos, renderCtx.Height));
                }
            }
        }
    }

    private static readonly Pen _paintPenCursorOnShotChange = new Pen(Brushes.LightCyan, 1.5)
    {
        DashStyle = DashStyle.Dash,
    };

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
            var pen = isOnShotChange ? _paintPenCursorOnShotChange : _paintPenCursor;
            context.DrawLine(pen,
                new Point(currentPositionPos, 0),
                new Point(currentPositionPos, renderCtx.Height));
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

            DrawNewParagraphDuration(context, currentRegionLeft, currentRegionWidth, renderCtx.Height);
        }
    }

    // SE 4 parity: while the user drags out a new selection, show its duration inside the
    // rectangle so the length is visible before the subtitle is created (issue #12034).
    private void DrawNewParagraphDuration(DrawingContext context, double currentRegionLeft, double currentRegionWidth, double height)
    {
        if (NewSelectionParagraph == null || currentRegionWidth <= 5)
        {
            return;
        }

        var durationMs = (NewSelectionParagraph.EndTime - NewSelectionParagraph.StartTime).TotalMilliseconds;
        if (durationMs < 10)
        {
            return;
        }

        // ToShortDisplayString mirrors Se.Settings.General.UseFrameMode, so this label follows the
        // same time/frame formatting as the existing paragraph footer (see DrawParagraphFooter).
        var durationText = GetCachedParagraphText(new TimeCode(durationMs).ToShortDisplayString());
        if (durationText.Width >= currentRegionWidth - 4)
        {
            return;
        }

        var textBounds = new Rect(currentRegionLeft + 1, 0, currentRegionWidth - 3, height);
        using (context.PushClip(textBounds))
        {
            var x = currentRegionLeft + (currentRegionWidth - durationText.Width) / 2;
            var y = height - 14 - durationText.Height;
            context.DrawText(durationText, new Point(x, y));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // yScaleHalf folds the loop-invariant verticalZoomFactor / highestPeak * halfWaveformHeight into
    // one factor the caller computes once, so the per-pixel call is a single multiply (no division).
    private static double CalculateYOptimized(double value, double halfWaveformHeight, double yScaleHalf, double boundsHeight)
    {
        var y = halfWaveformHeight - value * yScaleHalf;
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

    public void SetPosition(double startPositionSeconds, IReadOnlyList<SubtitleLineViewModel> subtitle, double currentVideoPositionSeconds, int subtitleIndex,
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

    private void LoadParagraphs(IReadOnlyList<SubtitleLineViewModel> subtitle, int primarySelectedIndex, List<SubtitleLineViewModel> selectedIndexes)
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
    private static int FindFirstIndexAfterTime(IReadOnlyList<SubtitleLineViewModel> subtitle, double timeMs)
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
        var shotChanges = ShotChanges;
        if (shotChanges == null || shotChanges.Count == 0)
        {
            return -1;
        }

        // The list is sorted, so binary search the insertion point and check the two neighbors
        // (called per frame from the render loop to pick the cursor pen).
        var low = 0;
        var high = shotChanges.Count;
        while (low < high)
        {
            var mid = low + (high - low) / 2;
            if (shotChanges[mid] < seconds)
            {
                low = mid + 1;
            }
            else
            {
                high = mid;
            }
        }

        var bestIndex = -1;
        var bestDistance = 0.04;
        if (low < shotChanges.Count && Math.Abs(shotChanges[low] - seconds) < bestDistance)
        {
            bestIndex = low;
            bestDistance = Math.Abs(shotChanges[low] - seconds);
        }

        if (low > 0 && Math.Abs(shotChanges[low - 1] - seconds) < bestDistance)
        {
            bestIndex = low - 1;
        }

        return bestIndex;
    }

    internal void SetSpectrogram(SpectrogramData2? spectrogram)
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

    internal SpectrogramData2? GetSpectrogram()
    {
        return _spectrogram;
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
        int minBetween = SecondsToSampleIndex(Se.Settings.General.MinimumBetweenLines.GetMilliseconds() / TimeCode.BaseUnit);
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
        _timeLineTextCache.Clear();
        _paragraphFormattedTextCache.Clear();
        _paragraphTextCache.Clear();
        _waveformCacheValid = false;

        _paintText = new SolidColorBrush(Se.Settings.Waveform.WaveformTextColor.FromHexToColor());
        _typeface = new Typeface(UiUtil.GetDefaultFontName(), FontStyle.Normal, Se.Settings.Waveform.WaveformTextFontBold ? FontWeight.Bold : FontWeight.Normal);
        _fontSize = Se.Settings.Waveform.WaveformTextFontSize;
    }

    internal void SetKeyModifiers(KeyEventArgs e)
    {
        _isCtrlDown = e.KeyModifiers.HasFlag(KeyModifiers.Control);
        _isShiftDown = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
        _isAltDown = e.KeyModifiers.HasFlag(KeyModifiers.Alt);
        _isMetaDown = e.KeyModifiers.HasFlag(KeyModifiers.Meta);
    }

    internal void UpdateTheme()
    {
        //_paintTimeText = UiUtil.GetTextColor();
    }
}