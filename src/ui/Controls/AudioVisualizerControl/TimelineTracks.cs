using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace Nikse.SubtitleEdit.Controls.AudioVisualizerControl;

/// <summary>How <see cref="TimelineTracks"/> splits the subtitles over rows.</summary>
public enum TimelineTrackGrouping
{
    None,
    Layer,
    Actor,
    Style,
}

/// <summary>
/// The video and subtitle rows of the editor-style layout (layout 14): a filmstrip of video
/// frames and one block per subtitle, stacked above the waveform like the tracks of a video
/// editor's timeline. The subtitles can be split over several rows (by layer, actor or style),
/// and a loaded original subtitle gets a row of its own.
/// <para>
/// The control has no time axis of its own - zoom, scroll position, play-head, paragraphs and
/// colors all come from the <see cref="AudioVisualizer"/> below it, so the rows always line up.
/// It has no editing of its own either: pointer input is handed to the waveform, which sits in
/// the same column and so shares its x coordinates. Moving and resizing a block is therefore
/// the waveform's drag in every detail - snapping, minimum gap, multi-selection, undo.
/// </para>
/// </summary>
public class TimelineTracks : Control, IDisposable
{
    public const double VideoRowHeight = TimelineThumbnailCache.ThumbnailHeight;
    public const double SubtitleRowHeight = 30;
    public const double CompactSubtitleRowHeight = 24;
    public const double RowGap = 2;

    /// <summary>Height with a single subtitle row, which is what a new layout starts with.</summary>
    public const double TotalHeight = VideoRowHeight + RowGap + SubtitleRowHeight + RowGap;

    // More groups than this share the last row: a timeline taller than the waveform it sits on
    // has stopped being a timeline.
    internal const int MaxGroupTracks = 6;

    // 16:9 at the row height - how much room a frame needs before the next one may start.
    private const double ThumbnailSlotWidth = 96;

    // Distances between filmstrip frames in seconds. Fixed steps (rather than "one frame per
    // slot width") keep every frame at a time that survives scrolling and small zoom changes,
    // so the cache keeps hitting.
    private static readonly double[] FilmstripSteps =
    {
        0.1, 0.2, 0.25, 0.5, 1, 2, 3, 4, 5, 6, 8, 10, 12, 15, 20, 30, 45, 60, 90, 120, 180, 240, 300, 450, 600, 900, 1200, 1800, 3600,
    };

    private sealed record Track(string Key, string Label, bool IsOriginal);

    private readonly TimelineThumbnailCache _thumbnails = new();
    private readonly List<SubtitleLineViewModel> _paragraphs = new();
    private readonly Dictionary<SubtitleLineViewModel, (string Source, FormattedText Text)> _textCache = new();
    private readonly Dictionary<SubtitleLineViewModel, (string Source, FormattedText Text)> _originalTextCache = new();
    private readonly Dictionary<string, FormattedText> _labelCache = new();
    private readonly Typeface _typeface = new(UiUtil.GetDefaultFontName());

    private List<Track> _tracks = new() { new Track(string.Empty, string.Empty, false) };
    private readonly Dictionary<string, int> _trackIndexByKey = new(StringComparer.OrdinalIgnoreCase);
    private readonly DispatcherTimer _trackTimer;
    private TimelineTrackGrouping _grouping;
    private bool _showVideoRow = true;

    private AudioVisualizer? _source;
    private bool _hooked;
    private long _lastPropertyInvalidateTicks;
    private int _invalidatePosted; // also written from the thumbnail worker thread

    private IBrush _backgroundBrush = Brushes.Black;
    private IBrush _rowBrush = Brushes.Black;
    private IBrush _tileBrush = Brushes.Black;
    private IBrush _labelBrush = Brushes.Black;
    private IBrush _paragraphBrush = Brushes.Gray;
    private IBrush _paragraphSelectedBrush = Brushes.Gray;
    private IPen _paragraphPen = new Pen(Brushes.Gray, 1);
    private IPen _paragraphSelectedPen = new Pen(Brushes.White, 1);
    private IPen _cursorPen = new Pen(Brushes.Cyan, 1);
    private IPen _shotChangePen = new Pen(Brushes.AntiqueWhite, 1);
    private IBrush _textBrush = Brushes.White;
    private (Color, Color, Color, Color, Color, string) _colorKey;

    /// <summary>The video the filmstrip shows; null or empty for none.</summary>
    public Func<string?>? GetVideoFileName { get; set; }

    /// <summary>All subtitles, not only the visible ones: the rows must not change with scrolling.</summary>
    public Func<IReadOnlyList<SubtitleLineViewModel>>? GetAllParagraphs { get; set; }

    /// <summary>Whether an original subtitle is loaded, which adds the original row.</summary>
    public Func<bool>? GetHasOriginal { get; set; }

    /// <summary>Label of the original row.</summary>
    public string OriginalLabel { get; set; } = string.Empty;

    /// <summary>Label of the text row when it has the original row for company.</summary>
    public string TextLabel { get; set; } = string.Empty;

    /// <summary>Prefix of the row labels when grouping by layer ("Layer" gives "Layer 0").</summary>
    public string LayerLabel { get; set; } = string.Empty;

    /// <summary>Label of the row for subtitles without an actor or style.</summary>
    public string NoneLabel { get; set; } = "-";

    /// <summary>
    /// Raised with the difference when the user changed the rows - regrouped them through
    /// <see cref="Grouping"/> or switched the video row through <see cref="ShowVideoRow"/> - so
    /// the layout can give the timeline that much more (or less) room. Rows that come and go
    /// with the data (a new actor, an original being loaded) do not raise it: a splitter the
    /// user has placed is not moved behind their back.
    /// </summary>
    public event Action<double>? HeightChangedByUser;

    public TimelineTracks()
    {
        Height = TotalHeight;
        ClipToBounds = true;
        FlowDirection = FlowDirection.LeftToRight;
        _thumbnails.ThumbnailReady += PostInvalidate;

        // The rows depend on the whole subtitle (a new actor, an original being loaded), which
        // nothing announces. Looking twice a second is far cheaper than looking per frame, and
        // unlike Render it is allowed to change the control's height.
        _trackTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        _trackTimer.Tick += (_, _) => UpdateTracks();

        PointerEntered += (_, e) => Forward(e, e.GetPosition(this));
        PointerMoved += (_, e) => Forward(e, e.GetPosition(this));
        PointerPressed += OnPointerPressed;
        PointerReleased += (_, e) => Forward(e, null);
        PointerCaptureLost += (_, e) => Forward(e, null);
        PointerWheelChanged += (_, e) => Forward(e, null);
        PointerExited += OnPointerExited;
        Tapped += (_, e) => Forward(e, e.GetPosition(this));
        DoubleTapped += (_, e) => Forward(e, e.GetPosition(this));
    }

    /// <summary>The waveform whose time axis, paragraphs and colors this control follows.</summary>
    public AudioVisualizer? Source
    {
        get => _source;
        set
        {
            if (ReferenceEquals(_source, value))
            {
                return;
            }

            Unhook();
            _source = value;
            if (this.IsAttachedToVisualTree())
            {
                Hook();
            }

            InvalidateVisual();
        }
    }

    /// <summary>How the subtitles are split over rows.</summary>
    public TimelineTrackGrouping Grouping
    {
        get => _grouping;
        set
        {
            if (_grouping == value)
            {
                return;
            }

            _grouping = value;
            var before = Height;
            UpdateTracks();
            if (Math.Abs(Height - before) > 0.5)
            {
                HeightChangedByUser?.Invoke(Height - before);
            }
        }
    }

    /// <summary>
    /// Whether the filmstrip row is there at all. Without it no frames are extracted, which
    /// matters on a slow disk or a network share, and the subtitle rows move up into its place.
    /// </summary>
    public bool ShowVideoRow
    {
        get => _showVideoRow;
        set
        {
            if (_showVideoRow == value)
            {
                return;
            }

            _showVideoRow = value;
            var before = Height;
            Height = GetTotalHeight(_tracks.Count, _showVideoRow);
            InvalidateVisual();
            if (Math.Abs(Height - before) > 0.5)
            {
                HeightChangedByUser?.Invoke(Height - before);
            }
        }
    }

    /// <summary>Where the subtitle rows start: below the video row, or at the top without one.</summary>
    internal double SubtitleRowsTop => _showVideoRow ? VideoRowHeight + RowGap : 0;

    /// <summary>Number of subtitle rows, the original row included.</summary>
    internal int TrackCount => _tracks.Count;

    internal string GetTrackLabel(int index) => _tracks[index].Label;

    private double RowHeight => GetRowHeight(_tracks.Count);

    private static double GetRowHeight(int trackCount) => trackCount > 2 ? CompactSubtitleRowHeight : SubtitleRowHeight;

    internal static double GetTotalHeight(int trackCount, bool showVideoRow = true)
    {
        return (showVideoRow ? VideoRowHeight + RowGap : 0) + trackCount * (GetRowHeight(trackCount) + RowGap);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Hook();
        UpdateTracks();
        _trackTimer.Start();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        // The waveform outlives every layout rebuild while this control does not, so a handler
        // left on it would keep each discarded instance alive.
        _trackTimer.Stop();
        Unhook();
        base.OnDetachedFromVisualTree(e);
    }

    public void Dispose()
    {
        _trackTimer.Stop();
        Unhook();
        _thumbnails.ThumbnailReady -= PostInvalidate;
    }

    private void Hook()
    {
        if (_hooked || _source == null)
        {
            return;
        }

        _source.PropertyChanged += OnSourcePropertyChanged;
        _source.Rendered += OnSourceRendered;
        _hooked = true;
    }

    private void Unhook()
    {
        if (!_hooked || _source == null)
        {
            return;
        }

        _source.PropertyChanged -= OnSourcePropertyChanged;
        _source.Rendered -= OnSourceRendered;
        _source.HitTestFilter = null;
        _hooked = false;
    }

    private void OnSourcePropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == AudioVisualizer.StartPositionSecondsProperty ||
            e.Property == AudioVisualizer.ZoomFactorProperty ||
            e.Property == AudioVisualizer.CurrentVideoPositionSecondsProperty ||
            e.Property == AudioVisualizer.WavePeaksProperty ||
            e.Property == AudioVisualizer.AllSelectedParagraphsProperty)
        {
            // Same frame as the waveform's own repaint, so the rows never trail it in a scroll.
            _lastPropertyInvalidateTicks = Environment.TickCount64;
            InvalidateVisual();
        }
    }

    private void OnSourceRendered(object? sender, EventArgs e)
    {
        // Everything that repaints the waveform without moving a property (a drag, an edited
        // text, a changed selection) arrives here. A repaint that a property change caused has
        // been handled above already; following it again would draw every frame twice.
        if (Environment.TickCount64 - _lastPropertyInvalidateTicks > 30)
        {
            PostInvalidate();
        }
    }

    private void PostInvalidate()
    {
        if (Interlocked.Exchange(ref _invalidatePosted, 1) == 1)
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            Interlocked.Exchange(ref _invalidatePosted, 0);
            InvalidateVisual();
        }, DispatcherPriority.Render);
    }

    // ------------------------------------------------------------------ rows

    /// <summary>
    /// Works out the rows from the whole subtitle and resizes the control when their number
    /// changed. Cheap enough for a timer: one pass collecting distinct keys.
    /// </summary>
    internal void UpdateTracks()
    {
        var tracks = new List<Track>();
        if (_grouping == TimelineTrackGrouping.None)
        {
            tracks.Add(new Track(string.Empty, string.Empty, false));
        }
        else
        {
            var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var all = GetAllParagraphs?.Invoke();
            if (all != null)
            {
                for (var i = 0; i < all.Count; i++)
                {
                    keys.Add(GetGroupKey(all[i]));
                }
            }

            if (keys.Count == 0)
            {
                keys.Add(_grouping == TimelineTrackGrouping.Layer ? "0" : string.Empty);
            }

            var sorted = _grouping == TimelineTrackGrouping.Layer
                ? keys.OrderBy(static k => int.TryParse(k, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) ? n : int.MaxValue).ToList()
                : keys.OrderBy(static k => k, StringComparer.OrdinalIgnoreCase).ToList();

            foreach (var key in sorted.Take(MaxGroupTracks))
            {
                tracks.Add(new Track(key, GetGroupLabel(key), false));
            }

            if (sorted.Count > MaxGroupTracks)
            {
                // The rest shares the last row rather than growing the timeline without limit.
                tracks[^1] = new Track(tracks[^1].Key, tracks[^1].Label + " ...", false);
            }
        }

        if (GetHasOriginal?.Invoke() == true)
        {
            if (tracks.Count == 1 && string.IsNullOrEmpty(tracks[0].Label))
            {
                tracks[0] = new Track(tracks[0].Key, TextLabel, false);
            }

            tracks.Add(new Track(string.Empty, OriginalLabel, true));
        }

        if (tracks.SequenceEqual(_tracks))
        {
            return;
        }

        _tracks = tracks;
        _trackIndexByKey.Clear();
        for (var i = 0; i < tracks.Count; i++)
        {
            if (!tracks[i].IsOriginal)
            {
                _trackIndexByKey[tracks[i].Key] = i;
            }
        }

        Height = GetTotalHeight(tracks.Count, _showVideoRow);
        InvalidateVisual();
    }

    private string GetGroupKey(SubtitleLineViewModel paragraph)
    {
        return _grouping switch
        {
            TimelineTrackGrouping.Layer => paragraph.Layer.ToString(CultureInfo.InvariantCulture),
            TimelineTrackGrouping.Actor => paragraph.Actor?.Trim() ?? string.Empty,
            TimelineTrackGrouping.Style => paragraph.Style?.Trim() ?? string.Empty,
            _ => string.Empty,
        };
    }

    private string GetGroupLabel(string key)
    {
        if (_grouping == TimelineTrackGrouping.Layer)
        {
            return (LayerLabel + " " + key).Trim();
        }

        return string.IsNullOrEmpty(key) ? NoneLabel : key;
    }

    /// <summary>The row a subtitle's own text is drawn in (never the original row).</summary>
    internal int GetGroupTrackIndex(SubtitleLineViewModel paragraph)
    {
        if (_grouping == TimelineTrackGrouping.None)
        {
            return 0;
        }

        if (_trackIndexByKey.TryGetValue(GetGroupKey(paragraph), out var index))
        {
            return index;
        }

        // Beyond MaxGroupTracks, or a key newer than the last look at the subtitle.
        var last = _tracks.Count - 1;
        return _tracks[last].IsOriginal ? Math.Max(0, last - 1) : last;
    }

    private Rect GetTrackRect(int index, double width)
    {
        var rowHeight = RowHeight;
        return new Rect(0, SubtitleRowsTop + index * (rowHeight + RowGap), width, rowHeight);
    }

    /// <summary>The subtitle row at a y coordinate, or -1 for the video row.</summary>
    internal int GetTrackIndexAt(double y)
    {
        var top = SubtitleRowsTop;
        if (y < top)
        {
            return -1;
        }

        return Math.Min(_tracks.Count - 1, (int)((y - top) / (RowHeight + RowGap)));
    }

    // ------------------------------------------------------------- filmstrip

    /// <summary>
    /// The distance in seconds between two filmstrip frames: the smallest fixed step that leaves
    /// each frame at least its own width.
    /// </summary>
    internal static double GetFilmstripStepSeconds(double pixelsPerSecond)
    {
        if (pixelsPerSecond <= 0)
        {
            return FilmstripSteps[^1];
        }

        var minimum = ThumbnailSlotWidth / pixelsPerSecond;
        foreach (var step in FilmstripSteps)
        {
            if (step >= minimum)
            {
                return step;
            }
        }

        return FilmstripSteps[^1];
    }

    /// <summary>
    /// The time a filmstrip tile takes its frame from. The very start of a video is black more
    /// often than not, so the first tile looks a little way in.
    /// </summary>
    internal static long GetFilmstripFrameMilliseconds(long tileIndex, double stepSeconds)
    {
        if (tileIndex <= 0)
        {
            return (long)Math.Round(Math.Min(stepSeconds / 2.0, 1.0) * 1000.0);
        }

        return (long)Math.Round(tileIndex * stepSeconds * 1000.0);
    }

    // ---------------------------------------------------------------- render

    public override void Render(DrawingContext context)
    {
        var width = Bounds.Width;
        var height = Bounds.Height;
        if (width <= 0 || height <= 0)
        {
            return;
        }

        var source = _source;
        UpdateBrushes(source);

        var bounds = new Rect(0, 0, width, height);
        var videoRow = new Rect(0, 0, width, VideoRowHeight);
        context.DrawRectangle(_backgroundBrush, null, bounds);
        if (_showVideoRow)
        {
            context.DrawRectangle(_rowBrush, null, videoRow);
        }

        for (var i = 0; i < _tracks.Count; i++)
        {
            context.DrawRectangle(_rowBrush, null, GetTrackRect(i, width));
        }

        var sampleRate = source?.SampleRate ?? 0;
        if (source == null || sampleRate == 0 || source.ZoomFactor <= 0)
        {
            DrawLabels(context, width);
            return;
        }

        var pixelsPerSecond = sampleRate * source.ZoomFactor;
        var startSeconds = source.StartPositionSeconds;
        var endSeconds = startSeconds + width / pixelsPerSecond;

        using (context.PushClip(bounds))
        {
            if (_showVideoRow)
            {
                DrawFilmstrip(context, source, videoRow, startSeconds, endSeconds, pixelsPerSecond);
            }

            DrawSubtitles(context, source, width, startSeconds, pixelsPerSecond);
            DrawLabels(context, width);

            var cursorX = Math.Round((source.CurrentVideoPositionSeconds - startSeconds) * pixelsPerSecond) + 0.5;
            if (cursorX >= 0 && cursorX <= width)
            {
                context.DrawLine(_cursorPen, new Point(cursorX, 0), new Point(cursorX, height));
            }
        }
    }

    private void DrawFilmstrip(DrawingContext context, AudioVisualizer source, Rect row, double startSeconds, double endSeconds, double pixelsPerSecond)
    {
        var videoFileName = GetVideoFileName?.Invoke();
        var step = GetFilmstripStepSeconds(pixelsPerSecond);
        var tileWidth = step * pixelsPerSecond;
        var firstTile = Math.Max(0, (long)Math.Floor(startSeconds / step));

        // No tiles past the end of the media (as far as the waveform knows it): there is no
        // picture there, only an ffmpeg run per tile to find that out.
        var lastTileSeconds = endSeconds;
        if (source.WavePeaks is { LengthInSeconds: > 0 } wavePeaks)
        {
            lastTileSeconds = Math.Min(endSeconds, wavePeaks.LengthInSeconds);
        }

        for (var tile = firstTile; tile * step < lastTileSeconds; tile++)
        {
            var x = Math.Round((tile * step - startSeconds) * pixelsPerSecond);
            var tileRect = new Rect(x, row.Y, Math.Max(1, tileWidth - 1), row.Height);
            context.DrawRectangle(_tileBrush, null, tileRect);

            var bitmap = _thumbnails.Get(videoFileName, GetFilmstripFrameMilliseconds(tile, step));
            if (bitmap == null || bitmap.Size.Height <= 0)
            {
                continue;
            }

            var bitmapWidth = bitmap.Size.Width * row.Height / bitmap.Size.Height;
            using (context.PushClip(tileRect))
            {
                context.DrawImage(bitmap, new Rect(x, row.Y, bitmapWidth, row.Height));
            }
        }

        foreach (var shotChange in source.ShotChanges)
        {
            if (shotChange < startSeconds || shotChange > endSeconds)
            {
                continue;
            }

            var x = Math.Round((shotChange - startSeconds) * pixelsPerSecond);
            context.DrawLine(_shotChangePen, new Point(x, row.Y), new Point(x, row.Bottom));
        }
    }

    private void DrawSubtitles(DrawingContext context, AudioVisualizer source, double width, double startSeconds, double pixelsPerSecond)
    {
        source.CopyDisplayableParagraphs(_paragraphs);

        var selected = source.AllSelectedParagraphs;
        var originalTrack = _tracks.FindIndex(static t => t.IsOriginal);
        foreach (var paragraph in _paragraphs)
        {
            var x1 = (paragraph.StartTime.TotalSeconds - startSeconds) * pixelsPerSecond;
            var x2 = (paragraph.EndTime.TotalSeconds - startSeconds) * pixelsPerSecond;
            if (x2 < 0 || x1 > width)
            {
                continue;
            }

            var isSelected = selected.Contains(paragraph);
            DrawBlock(context, paragraph, GetTrackRect(GetGroupTrackIndex(paragraph), width), x1, x2, isSelected, false);
            if (originalTrack >= 0)
            {
                DrawBlock(context, paragraph, GetTrackRect(originalTrack, width), x1, x2, isSelected, true);
            }
        }
    }

    private void DrawBlock(DrawingContext context, SubtitleLineViewModel paragraph, Rect row, double x1, double x2, bool isSelected, bool original)
    {
        var block = new Rect(Math.Round(x1) + 0.5, row.Y + 1.5, Math.Max(2, Math.Round(x2 - x1) - 1), row.Height - 3);
        context.DrawRectangle(
            isSelected ? _paragraphSelectedBrush : _paragraphBrush,
            isSelected ? _paragraphSelectedPen : _paragraphPen,
            block, 3, 3);

        if (block.Width < 14)
        {
            return;
        }

        var text = GetText(paragraph, original);
        using (context.PushClip(block.Deflate(new Thickness(4, 0, 3, 0))))
        {
            context.DrawText(text, new Point(block.X + 5, block.Y + (block.Height - text.Height) / 2));
        }
    }

    private void DrawLabels(DrawingContext context, double width)
    {
        if (_tracks.Count < 2)
        {
            return; // a single row needs no name
        }

        for (var i = 0; i < _tracks.Count; i++)
        {
            var label = _tracks[i].Label;
            if (string.IsNullOrEmpty(label))
            {
                continue;
            }

            if (!_labelCache.TryGetValue(label, out var text))
            {
                text = new FormattedText(label, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, _typeface, 10, _textBrush);
                _labelCache[label] = text;
            }

            var row = GetTrackRect(i, width);
            context.DrawRectangle(_labelBrush, null, new Rect(0, row.Y, Math.Min(width, text.Width + 8), row.Height));
            context.DrawText(text, new Point(4, row.Y + (row.Height - text.Height) / 2));
        }
    }

    private FormattedText GetText(SubtitleLineViewModel paragraph, bool original)
    {
        var cache = original ? _originalTextCache : _textCache;
        var source = (original ? paragraph.OriginalText : paragraph.Text) ?? string.Empty;
        if (cache.TryGetValue(paragraph, out var cached) && ReferenceEquals(cached.Source, source))
        {
            return cached.Text;
        }

        if (cache.Count > 1000)
        {
            cache.Clear();
        }

        var plain = HtmlUtil.RemoveHtmlTags(source, true).Replace("\r\n", " ").Replace('\n', ' ').Trim();
        var text = new FormattedText(plain, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, _typeface, 12, _textBrush);
        cache[paragraph] = (source, text);
        return text;
    }

    private void UpdateBrushes(AudioVisualizer? source)
    {
        var background = source?.WaveformBackgroundColor ?? Colors.Black;
        var key = (
            background,
            source?.ParagraphBackground ?? Colors.Gray,
            source?.WaveformSelectedColor ?? Colors.White,
            source?.WaveformCursorColor ?? Colors.Cyan,
            source?.WaveformShotChangeColor ?? Colors.AntiqueWhite,
            Se.Settings.Waveform.WaveformTextColor);
        if (key == _colorKey)
        {
            return;
        }

        _colorKey = key;

        // The rows are told apart from the gaps between them by a slight shift towards the
        // opposite of the background, which works for dark and light waveform themes alike.
        var isDark = background.R + background.G + background.B < 384;
        var contrast = isDark ? Colors.White : Colors.Black;
        _backgroundBrush = new SolidColorBrush(background);
        _rowBrush = new SolidColorBrush(Blend(background, contrast, 0.06));
        _tileBrush = new SolidColorBrush(Blend(background, contrast, 0.12));
        _labelBrush = new SolidColorBrush(Blend(background, contrast, 0.06), 0.85);

        // The waveform's paragraph colors are translucent overlays for a busy background; on a
        // plain row they are blended into it and drawn solid instead.
        var paragraph = Blend(background, Opaque(key.Item2), 0.75);
        var paragraphSelected = Blend(background, Opaque(key.Item3), 0.55);
        _paragraphBrush = new SolidColorBrush(paragraph);
        _paragraphSelectedBrush = new SolidColorBrush(paragraphSelected);
        _paragraphPen = new Pen(new SolidColorBrush(Blend(paragraph, contrast, 0.25)), 1);
        _paragraphSelectedPen = new Pen(new SolidColorBrush(Opaque(key.Item3)), 1);
        _cursorPen = new Pen(new SolidColorBrush(key.Item4), 1);
        _shotChangePen = new Pen(new SolidColorBrush(key.Item5), 2);
        _textBrush = new SolidColorBrush(key.Item6.FromHexToColor());
        _textCache.Clear();
        _originalTextCache.Clear();
        _labelCache.Clear();
    }

    private static Color Opaque(Color color) => Color.FromRgb(color.R, color.G, color.B);

    private static Color Blend(Color from, Color to, double amount)
    {
        return Color.FromRgb(
            (byte)Math.Round(from.R + (to.R - from.R) * amount),
            (byte)Math.Round(from.G + (to.G - from.G) * amount),
            (byte)Math.Round(from.B + (to.B - from.B) * amount));
    }

    // ----------------------------------------------------------------- input

    private bool TryGetSeconds(double x, out double seconds)
    {
        seconds = 0;
        var source = _source;
        var sampleRate = source?.SampleRate ?? 0;
        if (source == null || sampleRate == 0 || source.ZoomFactor <= 0)
        {
            return false;
        }

        seconds = source.StartPositionSeconds + x / (sampleRate * source.ZoomFactor);
        return true;
    }

    /// <summary>The subtitle whose block is at a point of this control, or null.</summary>
    internal SubtitleLineViewModel? HitTestParagraph(Point point)
    {
        var track = GetTrackIndexAt(point.Y);
        if (_source == null || track < 0 || !TryGetSeconds(point.X, out var seconds))
        {
            return null;
        }

        _source.CopyDisplayableParagraphs(_paragraphs);
        var milliseconds = seconds * 1000.0;
        foreach (var paragraph in _paragraphs)
        {
            if (milliseconds >= paragraph.StartTime.TotalMilliseconds &&
                milliseconds <= paragraph.EndTime.TotalMilliseconds &&
                IsInTrack(paragraph, track))
            {
                return paragraph;
            }
        }

        return null;
    }

    // Every subtitle has a block in the original row.
    private bool IsInTrack(SubtitleLineViewModel paragraph, int track) => _tracks[track].IsOriginal || GetGroupTrackIndex(paragraph) == track;

    /// <summary>
    /// Hands a pointer event to the waveform. With a position, the waveform's hit test is first
    /// narrowed to the row under the pointer, so of two overlapping subtitles the one whose
    /// block is pointed at gets picked; the video row picks none (a press there starts a new
    /// selection, like a press on empty waveform).
    /// </summary>
    private void Forward(RoutedEventArgs e, Point? position)
    {
        var source = _source;
        if (source == null)
        {
            return;
        }

        if (position != null)
        {
            var track = GetTrackIndexAt(position.Value.Y);
            if (track < 0)
            {
                source.HitTestFilter = static _ => false;
            }
            else if (_tracks.Count == 1 || _tracks[track].IsOriginal)
            {
                source.HitTestFilter = null;
            }
            else
            {
                source.HitTestFilter = p => GetGroupTrackIndex(p) == track;
            }
        }

        source.RaiseEvent(e);
        Cursor = source.Cursor;
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // A press focuses the focusable control it lands on; this one is not, and keyboard
        // shortcuts for the waveform should work after a click on its rows too.
        _source?.Focus();
        Forward(e, e.GetPosition(this));
    }

    private void OnPointerExited(object? sender, PointerEventArgs e)
    {
        Forward(e, null);
        if (_source != null)
        {
            // Back to x-only hit testing before the pointer reaches the waveform itself.
            _source.HitTestFilter = null;
        }
    }
}
