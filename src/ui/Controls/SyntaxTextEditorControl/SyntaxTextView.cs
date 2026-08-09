using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Input.TextInput;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Media.TextFormatting;
using Avalonia.Threading;
using Avalonia.Utilities;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Controls.SyntaxTextEditorControl;

/// <summary>
/// The text surface of <see cref="SyntaxTextEditor"/>: it lays out and draws only the lines that
/// are on screen, so opening and scrolling a large subtitle source costs the same as a small one.
///
/// Everything expensive is per visible line - the syntax rules (<see cref="ISourceSyntaxHighlighter"/>
/// is per line by design), the text layout and the drawing. Layouts are cached until the document,
/// the font or the theme changes.
///
/// Word wrap is deliberately not supported: a wrapped line has no fixed height, so the scroll
/// extent could not be known without laying out the whole document. Long lines scroll sideways.
///
/// This is one file on purpose - the declarative-UI source generator emits one registration per
/// declaration of a Control, so a partial control class breaks the build.
/// </summary>
public class SyntaxTextView : Control
{
    public static readonly StyledProperty<IBrush?> ForegroundProperty =
        TextElement.ForegroundProperty.AddOwner<SyntaxTextView>();

    public static readonly StyledProperty<FontFamily> FontFamilyProperty =
        TextElement.FontFamilyProperty.AddOwner<SyntaxTextView>();

    public static readonly StyledProperty<double> FontSizeProperty =
        TextElement.FontSizeProperty.AddOwner<SyntaxTextView>();

    public static readonly StyledProperty<bool> IsReadOnlyProperty =
        AvaloniaProperty.Register<SyntaxTextView, bool>(nameof(IsReadOnly));

    public static readonly StyledProperty<IBrush?> SelectionBrushProperty =
        AvaloniaProperty.Register<SyntaxTextView, IBrush?>(nameof(SelectionBrush));

    public static readonly StyledProperty<IBrush?> CaretBrushProperty =
        AvaloniaProperty.Register<SyntaxTextView, IBrush?>(nameof(CaretBrush));

    public static readonly StyledProperty<IBrush?> CurrentLineBrushProperty =
        AvaloniaProperty.Register<SyntaxTextView, IBrush?>(nameof(CurrentLineBrush));

    private const double PaddingLeft = 4;
    private const int MaxCachedLayouts = 512;
    private const int MaxUndoEntries = 500;

    private static readonly ImmutableSolidColorBrush DefaultSelectionBrush = new(Colors.SteelBlue, 0.4);
    private static readonly char[] LineBreakChars = ['\r', '\n'];

    // Layouts of the lines drawn recently. Only the visible range is ever built and the cache is
    // trimmed to the lines around the viewport, so scrolling a huge file cannot grow it.
    private readonly Dictionary<int, TextLayout> _lineLayouts = new();
    private readonly List<int> _evictionScratch = new();

    private readonly SourceSyntaxLineStyler _styler = new();
    private readonly List<SourceSyntaxSpan> _spanScratch = new();
    private readonly Dictionary<(Color Color, bool Bold), GenericTextRunProperties> _runPropertiesCache = new();

    private readonly List<UndoEntry> _undoStack = new();
    private readonly List<UndoEntry> _redoStack = new();

    private SyntaxTextDocument _document = new();
    private ISourceSyntaxHighlighter? _sourceHighlighter;
    private int _cachedVersion = -1;

    private Typeface _typeface;
    private Typeface _boldTypeface;
    private GenericTextRunProperties? _defaultRunProperties;
    private double _measuredFontSize;
    private FontFamily? _measuredFontFamily;
    private bool _measuredDarkTheme;
    private double _lineHeight = 16;

    private Vector _scrollOffset;
    private double _maxLineWidth;
    private bool _maxLineWidthValid;

    private int _caretOffset;
    private int _selectionAnchor;

    // The column the caret keeps while moving up and down, so it comes back out in the right place
    // after passing through short lines.
    private double? _desiredCaretX;

    private readonly SyntaxTextViewInputMethodClient _inputMethodClient;

    private DispatcherTimer? _caretTimer;
    private bool _caretOn;
    private bool _isDragSelecting;
    private bool _allowUndoMerge;
    private bool _isRendering;
    private bool _scrollMetricsUpdateQueued;

    /// <summary>Number of line layouts built - the virtualization tests assert on this.</summary>
    internal int LayoutsCreated { get; private set; }

    static SyntaxTextView()
    {
        AffectsRender<SyntaxTextView>(
            ForegroundProperty,
            SelectionBrushProperty,
            CaretBrushProperty,
            CurrentLineBrushProperty);

        FocusableProperty.OverrideDefaultValue<SyntaxTextView>(true);
    }

    public SyntaxTextView()
    {
        _document.Changed += OnDocumentChanged;
        _inputMethodClient = new SyntaxTextViewInputMethodClient(this);
        AddHandler(TextInputMethodClientRequestedEvent, OnTextInputMethodClientRequested);
    }

    /// <summary>Raised when the extent, the viewport or the scroll offset changed.</summary>
    public event EventHandler? ScrollMetricsChanged;

    /// <summary>Raised after the text changed (typing, paste, undo, a new document).</summary>
    public event EventHandler? TextChanged;

    /// <summary>Raised when the caret moved or the selection changed.</summary>
    public event EventHandler? CaretChanged;

    public SyntaxTextDocument Document
    {
        get => _document;
        set
        {
            if (ReferenceEquals(_document, value))
            {
                return;
            }

            _document.Changed -= OnDocumentChanged;
            _document = value;
            _document.Changed += OnDocumentChanged;
            ClearUndo();
            ResetCaches();
            _caretOffset = 0;
            _selectionAnchor = 0;
            InvalidateScrollMetrics();
            InvalidateVisual();
            TextChanged?.Invoke(this, EventArgs.Empty);
            CaretChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>The syntax rules; null means no coloring.</summary>
    public ISourceSyntaxHighlighter? SourceHighlighter
    {
        get => _sourceHighlighter;
        set
        {
            if (ReferenceEquals(_sourceHighlighter, value))
            {
                return;
            }

            _sourceHighlighter = value;
            ResetCaches();
            InvalidateVisual();
        }
    }

    public bool IsReadOnly
    {
        get => GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    public IBrush? Foreground
    {
        get => GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    public FontFamily FontFamily
    {
        get => GetValue(FontFamilyProperty);
        set => SetValue(FontFamilyProperty, value);
    }

    public double FontSize
    {
        get => GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public IBrush? SelectionBrush
    {
        get => GetValue(SelectionBrushProperty);
        set => SetValue(SelectionBrushProperty, value);
    }

    public IBrush? CaretBrush
    {
        get => GetValue(CaretBrushProperty);
        set => SetValue(CaretBrushProperty, value);
    }

    /// <summary>Highlight behind the caret's line; null draws none.</summary>
    public IBrush? CurrentLineBrush
    {
        get => GetValue(CurrentLineBrushProperty);
        set => SetValue(CurrentLineBrushProperty, value);
    }

    public string Text
    {
        get => _document.Text;
        set
        {
            var text = value ?? string.Empty;
            if (_document.Text == text)
            {
                return;
            }

            _document.Text = text;
            ClearUndo();
            SetCaret(Math.Min(_caretOffset, _document.TextLength), extendSelection: false);
        }
    }

    /// <summary>Height of one line in pixels, rounded to whole pixels so text stays crisp.</summary>
    public double LineHeight
    {
        get
        {
            EnsureFontMetrics();
            return _lineHeight;
        }
    }

    /// <summary>The full size of the text - what the scroll bars scroll over.</summary>
    public Size Extent
    {
        get
        {
            EnsureFontMetrics();
            return new Size(GetMaxLineWidth() + PaddingLeft * 2, _document.LineCount * _lineHeight);
        }
    }

    public Size Viewport => Bounds.Size;

    public Vector ScrollOffset
    {
        get => _scrollOffset;
        set
        {
            var clamped = ClampScrollOffset(value);
            if (clamped.NearlyEquals(_scrollOffset))
            {
                return;
            }

            _scrollOffset = clamped;
            InvalidateVisual();
            RaiseScrollMetricsChanged();
        }
    }

    private Vector ClampScrollOffset(Vector offset)
    {
        var extent = Extent;
        var maxX = Math.Max(0, extent.Width - Bounds.Width);
        var maxY = Math.Max(0, extent.Height - Bounds.Height);
        return new Vector(Math.Clamp(offset.X, 0, maxX), Math.Clamp(offset.Y, 0, maxY));
    }

    private void OnDocumentChanged(object? sender, SyntaxTextDocumentChangedEventArgs e)
    {
        if (e.WholeDocument)
        {
            ResetCaches();
        }
        else
        {
            // Typing must not throw away a screenful of layouts. An edit inside one line only
            // invalidates that line; one that adds or removes lines shifts everything below it.
            if (e.LineCountDelta == 0)
            {
                _lineLayouts.Remove(e.StartLine);
            }
            else
            {
                DropCachedLayoutsFrom(e.StartLine);
            }

            // The horizontal extent is only grown from the lines that get laid out (see
            // GetLineLayout): rescanning every line for the longest one on each keystroke costs
            // more than the slightly-too-wide scroll range it would save after a long line is cut.
            _cachedVersion = _document.Version;
        }

        InvalidateScrollMetrics();
        InvalidateVisual();
        TextChanged?.Invoke(this, EventArgs.Empty);
    }

    private void DropCachedLayoutsFrom(int firstLine)
    {
        _evictionScratch.Clear();
        foreach (var cachedLine in _lineLayouts.Keys)
        {
            if (cachedLine >= firstLine)
            {
                _evictionScratch.Add(cachedLine);
            }
        }

        foreach (var cachedLine in _evictionScratch)
        {
            _lineLayouts.Remove(cachedLine);
        }

        _evictionScratch.Clear();
    }

    private void ResetCaches()
    {
        _lineLayouts.Clear();
        _cachedVersion = _document.Version;
        _maxLineWidthValid = false;
    }

    internal void InvalidateScrollMetrics()
    {
        _scrollOffset = ClampScrollOffset(_scrollOffset);
        RaiseScrollMetricsChanged();
    }

    /// <summary>
    /// Tells the host that the extent or the offset moved, so it can resize its scroll bars.
    ///
    /// Laying out a line can widen the extent (see <see cref="GetLineLayout"/>), and that happens
    /// while drawing - where the host's reaction, setting scroll bar properties, invalidates a
    /// visual and makes Avalonia throw "Visual was invalidated during the render pass". While a
    /// paint is running the notification is therefore posted to the next dispatcher turn.
    /// </summary>
    private void RaiseScrollMetricsChanged()
    {
        if (!_isRendering)
        {
            ScrollMetricsChanged?.Invoke(this, EventArgs.Empty);
            return;
        }

        if (_scrollMetricsUpdateQueued)
        {
            return;
        }

        _scrollMetricsUpdateQueued = true;
        Dispatcher.UIThread.Post(
            () =>
            {
                _scrollMetricsUpdateQueued = false;
                ScrollMetricsChanged?.Invoke(this, EventArgs.Empty);
            },
            DispatcherPriority.Background);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == FontFamilyProperty ||
            change.Property == FontSizeProperty ||
            change.Property == ForegroundProperty ||
            change.Property == FlowDirectionProperty)
        {
            _measuredFontFamily = null;
            ResetCaches();
            InvalidateScrollMetrics();
            InvalidateVisual();
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        EnsureFontMetrics();
        return availableSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var result = base.ArrangeOverride(finalSize);
        InvalidateScrollMetrics();
        return result;
    }

    private void EnsureFontMetrics()
    {
        var isDarkTheme = UiTheme.IsDarkThemeEnabled();
        if (_measuredFontFamily == FontFamily &&
            Math.Abs(_measuredFontSize - FontSize) < 0.01 &&
            isDarkTheme == _measuredDarkTheme)
        {
            return;
        }

        _measuredFontFamily = FontFamily;
        _measuredFontSize = FontSize;
        _measuredDarkTheme = isDarkTheme;
        _typeface = new Typeface(FontFamily);
        _boldTypeface = new Typeface(FontFamily, FontStyle.Normal, FontWeight.Bold);

        // The syntax colors are theme dependent, so the cached layouts and run properties have to
        // go with the theme.
        _defaultRunProperties = null;
        _runPropertiesCache.Clear();
        _lineLayouts.Clear();
        _maxLineWidthValid = false;

        var sample = new TextLayout("0", _typeface, FontSize, Foreground ?? Brushes.Black);
        _lineHeight = Math.Max(1, Math.Ceiling(sample.Height));
    }

    /// <summary>
    /// The width used for the horizontal extent. Measuring every line would defeat the point of
    /// virtualizing, so the longest line by character count is measured for real and every line
    /// laid out afterwards can push the value up.
    /// </summary>
    private double GetMaxLineWidth()
    {
        if (_maxLineWidthValid)
        {
            return _maxLineWidth;
        }

        var longest = 0;
        var longestLength = 0;
        for (var line = 0; line < _document.LineCount; line++)
        {
            var length = _document.GetLineLength(line);
            if (length > longestLength)
            {
                longestLength = length;
                longest = line;
            }
        }

        _maxLineWidth = longestLength == 0
            ? 0
            : new TextLayout(_document.GetLine(longest), _typeface, FontSize, Foreground ?? Brushes.Black)
                .WidthIncludingTrailingWhitespace;
        _maxLineWidthValid = true;
        return _maxLineWidth;
    }

    internal TextLayout GetLineLayout(int line)
    {
        EnsureFontMetrics();

        if (_cachedVersion != _document.Version)
        {
            _lineLayouts.Clear();
            _cachedVersion = _document.Version;
        }

        if (_lineLayouts.TryGetValue(line, out var cached))
        {
            return cached;
        }

        var text = _document.GetLine(line);
        var layout = new TextLayout(
            text,
            _typeface,
            FontSize,
            Foreground ?? Brushes.Black,
            flowDirection: FlowDirection,
            textStyleOverrides: BuildLineSpans(text));

        _lineLayouts[line] = layout;
        LayoutsCreated++;

        var width = layout.WidthIncludingTrailingWhitespace;
        if (width > _maxLineWidth)
        {
            _maxLineWidth = width;
            RaiseScrollMetricsChanged();
        }

        if (_lineLayouts.Count > MaxCachedLayouts)
        {
            TrimLayoutCache(line);
        }

        return layout;
    }

    /// <summary>The lines the viewport currently shows - the only ones ever laid out or drawn.</summary>
    internal (int First, int Last) GetVisibleLineRange()
    {
        EnsureFontMetrics();

        var first = Math.Max(0, (int)(_scrollOffset.Y / _lineHeight));
        var last = Math.Min(_document.LineCount - 1, (int)((_scrollOffset.Y + Math.Max(0, Bounds.Height)) / _lineHeight));
        return (first, Math.Max(first, last));
    }

    /// <summary>
    /// Builds the layouts for the visible lines - what <see cref="Render"/> does before it draws.
    /// Rendering cannot run in the headless test host, so the tests drive virtualization here.
    /// </summary>
    internal void EnsureVisibleLayouts()
    {
        var (first, last) = GetVisibleLineRange();
        for (var line = first; line <= last; line++)
        {
            GetLineLayout(line);
        }
    }

    /// <summary>Drops the cached layouts furthest from the line that was just needed.</summary>
    private void TrimLayoutCache(int aroundLine)
    {
        _evictionScratch.Clear();
        foreach (var cachedLine in _lineLayouts.Keys)
        {
            if (Math.Abs(cachedLine - aroundLine) > MaxCachedLayouts / 4)
            {
                _evictionScratch.Add(cachedLine);
            }
        }

        foreach (var cachedLine in _evictionScratch)
        {
            _lineLayouts.Remove(cachedLine);
        }

        _evictionScratch.Clear();
    }

    private GenericTextRunProperties GetDefaultRunProperties()
    {
        return _defaultRunProperties ??= new GenericTextRunProperties(
            _typeface,
            FontSize,
            foregroundBrush: Foreground ?? Brushes.Black);
    }

    private GenericTextRunProperties GetRunProperties(Color color, bool bold)
    {
        if (_runPropertiesCache.TryGetValue((color, bold), out var properties))
        {
            return properties;
        }

        properties = new GenericTextRunProperties(
            bold ? _boldTypeface : _typeface,
            FontSize,
            foregroundBrush: new ImmutableSolidColorBrush(color));
        _runPropertiesCache[(color, bold)] = properties;
        return properties;
    }

    /// <summary>
    /// Sorted, non-overlapping style spans covering the line. The gaps between colored tokens have
    /// to be filled with the default style, otherwise a token's color bleeds into the text after it.
    /// </summary>
    private IReadOnlyList<ValueSpan<TextRunProperties>>? BuildLineSpans(string lineText)
    {
        if (_sourceHighlighter == null || lineText.Length == 0)
        {
            return null;
        }

        _styler.Reset(lineText.Length);
        _sourceHighlighter.HighlightLine(lineText, _styler);
        _spanScratch.Clear();
        _styler.Flatten(0, _spanScratch);

        if (_spanScratch.Count == 0)
        {
            return null;
        }

        var defaultProperties = GetDefaultRunProperties();
        var spans = new List<ValueSpan<TextRunProperties>>(_spanScratch.Count * 2 + 1);
        var position = 0;

        foreach (var span in _spanScratch)
        {
            if (span.Start > position)
            {
                spans.Add(new ValueSpan<TextRunProperties>(position, span.Start - position, defaultProperties));
            }

            spans.Add(new ValueSpan<TextRunProperties>(span.Start, span.Length, GetRunProperties(span.Color, span.Bold)));
            position = span.Start + span.Length;
        }

        if (position < lineText.Length)
        {
            spans.Add(new ValueSpan<TextRunProperties>(position, lineText.Length - position, defaultProperties));
        }

        return spans;
    }

    public override void Render(DrawingContext context)
    {
        var width = Bounds.Width;
        var height = Bounds.Height;
        if (width <= 0 || height <= 0)
        {
            return;
        }

        EnsureFontMetrics();

        // Nothing done from here on may invalidate a visual - see RaiseScrollMetricsChanged.
        _isRendering = true;
        try
        {
            // Hit testing follows rendered geometry, so fill the whole surface: without this,
            // clicks in the empty space beside or below the text fall through to the parent and
            // never place the caret or start a drag selection.
            context.FillRectangle(Brushes.Transparent, new Rect(0, 0, width, height));

            var lineHeight = _lineHeight;
            var (firstLine, lastLine) = GetVisibleLineRange();
            var selectionStart = SelectionStart;
            var selectionEnd = SelectionEnd;
            var caretPosition = _document.GetPosition(_caretOffset);
            var selectionBrush = SelectionBrush ?? DefaultSelectionBrush;

            using (context.PushClip(new Rect(0, 0, width, height)))
            {
                // The caret's line, so it is easy to find in a wall of source text.
                if (CurrentLineBrush is { } currentLineBrush && selectionStart == selectionEnd)
                {
                    var y = caretPosition.Line * lineHeight - _scrollOffset.Y;
                    context.FillRectangle(currentLineBrush, new Rect(0, y, width, lineHeight));
                }

                for (var line = firstLine; line <= lastLine; line++)
                {
                    var y = line * lineHeight - _scrollOffset.Y;
                    var x = PaddingLeft - _scrollOffset.X;
                    var layout = GetLineLayout(line);

                    DrawSelection(context, line, y, layout, selectionStart, selectionEnd, selectionBrush);
                    layout.Draw(context, new Point(x, y));
                }

                DrawCaret(context, caretPosition, lineHeight);
            }
        }
        finally
        {
            _isRendering = false;
        }
    }

    private void DrawSelection(
        DrawingContext context,
        int line,
        double y,
        TextLayout layout,
        int selectionStart,
        int selectionEnd,
        IBrush selectionBrush)
    {
        if (selectionStart == selectionEnd)
        {
            return;
        }

        var lineStart = _document.GetLineStartOffset(line);
        var lineEnd = lineStart + _document.GetLineLength(line);
        if (selectionEnd <= lineStart || selectionStart > lineEnd)
        {
            return;
        }

        var startColumn = Math.Max(0, selectionStart - lineStart);
        var endColumn = Math.Min(lineEnd - lineStart, selectionEnd - lineStart);
        var x = PaddingLeft - _scrollOffset.X;

        if (endColumn > startColumn)
        {
            foreach (var rect in layout.HitTestTextRange(startColumn, endColumn - startColumn))
            {
                context.FillRectangle(selectionBrush, new Rect(x + rect.X, y, rect.Width, _lineHeight));
            }
        }

        // A selection running past the end of a line covers its line break as a small stub, the way
        // every editor shows "this whole line is selected".
        if (selectionEnd > lineEnd)
        {
            var caretRect = layout.HitTestTextPosition(lineEnd - lineStart);
            context.FillRectangle(selectionBrush, new Rect(x + caretRect.X, y, _lineHeight / 2, _lineHeight));
        }
    }

    private void DrawCaret(DrawingContext context, SyntaxTextPosition caretPosition, double lineHeight)
    {
        if (!_caretOn || !IsFocused || IsReadOnly)
        {
            return;
        }

        var layout = GetLineLayout(caretPosition.Line);
        var rect = layout.HitTestTextPosition(caretPosition.Column);
        var x = Math.Floor(PaddingLeft + rect.X - _scrollOffset.X) + 0.5;
        var y = caretPosition.Line * lineHeight - _scrollOffset.Y;
        var brush = CaretBrush ?? Foreground ?? Brushes.Black;
        context.DrawLine(new Pen(brush, 1), new Point(x, y), new Point(x, y + lineHeight));
    }

    protected override void OnGotFocus(FocusChangedEventArgs e)
    {
        base.OnGotFocus(e);
        StartCaretBlink();
    }

    protected override void OnLostFocus(FocusChangedEventArgs e)
    {
        base.OnLostFocus(e);
        StopCaretBlink();
        InvalidateVisual();
    }

    private void StartCaretBlink()
    {
        if (IsReadOnly)
        {
            return;
        }

        _caretOn = true;
        _caretTimer ??= new DispatcherTimer(TimeSpan.FromMilliseconds(500), DispatcherPriority.Background, (_, _) =>
        {
            _caretOn = !_caretOn;
            InvalidateVisual();
        });

        _caretTimer.Start();
        InvalidateVisual();
    }

    private void StopCaretBlink()
    {
        _caretTimer?.Stop();
        _caretOn = false;
    }

    /// <summary>Restarts the blink so the caret stays solid while it is being moved.</summary>
    private void ResetCaretBlink()
    {
        if (!IsFocused)
        {
            return;
        }

        _caretTimer?.Stop();
        StartCaretBlink();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        StopCaretBlink();
    }

    // ----------------------------------------------------------------------------------------
    // Caret and selection
    // ----------------------------------------------------------------------------------------

    public int CaretOffset
    {
        get => _caretOffset;
        set => SetCaret(value, extendSelection: false);
    }

    public int SelectionAnchor
    {
        get => _selectionAnchor;
        set
        {
            var clamped = Math.Clamp(value, 0, _document.TextLength);
            if (_selectionAnchor == clamped)
            {
                return;
            }

            _selectionAnchor = clamped;
            InvalidateVisual();
            CaretChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public int SelectionStart => Math.Min(_caretOffset, _selectionAnchor);

    public int SelectionEnd => Math.Max(_caretOffset, _selectionAnchor);

    public int SelectionLength => SelectionEnd - SelectionStart;

    public string SelectedText => _document.GetText(SelectionStart, SelectionLength);

    /// <summary>The caret's line, zero based - the gutter highlights it.</summary>
    public int CaretLine => _document.GetPosition(_caretOffset).Line;

    /// <summary>Moves the caret, optionally dragging the selection along with it.</summary>
    public void SetCaret(int offset, bool extendSelection)
    {
        var clamped = Math.Clamp(offset, 0, _document.TextLength);
        var changed = _caretOffset != clamped;
        _caretOffset = clamped;

        if (!extendSelection)
        {
            changed |= _selectionAnchor != clamped;
            _selectionAnchor = clamped;
        }

        _desiredCaretX = null;

        if (changed)
        {
            ResetCaretBlink();
            InvalidateVisual();
            _inputMethodClient.NotifyCaretMoved();
            CaretChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public void Select(int start, int length)
    {
        var textLength = _document.TextLength;
        var from = Math.Clamp(start, 0, textLength);
        var to = Math.Clamp(start + Math.Max(0, length), from, textLength);

        _selectionAnchor = from;
        _caretOffset = to;
        _desiredCaretX = null;
        InvalidateVisual();
        CaretChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SelectAll() => Select(0, _document.TextLength);

    public void ClearSelection() => SelectionAnchor = _caretOffset;

    /// <summary>The offset under a point in view coordinates.</summary>
    public int GetOffsetFromPoint(Point point)
    {
        EnsureFontMetrics();

        var line = Math.Clamp(
            (int)((point.Y + _scrollOffset.Y) / _lineHeight),
            0,
            _document.LineCount - 1);

        var layout = GetLineLayout(line);
        var x = point.X + _scrollOffset.X - PaddingLeft;
        var hit = layout.HitTestPoint(new Point(Math.Max(0, x), 0));
        var column = hit.TextPosition + (hit.IsTrailing ? 1 : 0);

        return _document.GetOffset(line, column);
    }

    /// <summary>Where the caret is drawn for an offset, in view coordinates.</summary>
    public Point GetPointFromOffset(int offset)
    {
        EnsureFontMetrics();

        var position = _document.GetPosition(offset);
        var layout = GetLineLayout(position.Line);
        var rect = layout.HitTestTextPosition(position.Column);
        return new Point(
            PaddingLeft + rect.X - _scrollOffset.X,
            position.Line * _lineHeight - _scrollOffset.Y);
    }

    /// <summary>Scrolls the caret into view, keeping a little context around it.</summary>
    public void BringCaretIntoView()
    {
        if (Bounds.Width <= 0 || Bounds.Height <= 0)
        {
            return;
        }

        EnsureFontMetrics();

        var position = _document.GetPosition(_caretOffset);
        var offset = _scrollOffset;

        var caretTop = position.Line * _lineHeight;
        var caretBottom = caretTop + _lineHeight;
        if (caretTop < offset.Y)
        {
            offset = offset.WithY(caretTop);
        }
        else if (caretBottom > offset.Y + Bounds.Height)
        {
            offset = offset.WithY(caretBottom - Bounds.Height);
        }

        var caretX = GetLineLayout(position.Line).HitTestTextPosition(position.Column).X;
        const double horizontalMargin = 24;
        if (caretX < offset.X)
        {
            offset = offset.WithX(Math.Max(0, caretX - horizontalMargin));
        }
        else if (caretX + PaddingLeft * 2 > offset.X + Bounds.Width)
        {
            offset = offset.WithX(caretX + PaddingLeft * 2 + horizontalMargin - Bounds.Width);
        }

        ScrollOffset = offset;
    }

    /// <summary>Moves the caret up or down, keeping the column where the lines allow it.</summary>
    private void MoveCaretByLines(int delta, bool extendSelection)
    {
        EnsureFontMetrics();

        var position = _document.GetPosition(_caretOffset);
        var targetLine = Math.Clamp(position.Line + delta, 0, _document.LineCount - 1);
        if (targetLine == position.Line)
        {
            // Already on the first/last line: go to its start or end, like other editors.
            SetCaret(delta < 0 ? 0 : _document.TextLength, extendSelection);
            return;
        }

        var desiredX = _desiredCaretX ?? GetLineLayout(position.Line).HitTestTextPosition(position.Column).X;
        var hit = GetLineLayout(targetLine).HitTestPoint(new Point(desiredX, 0));
        var column = hit.TextPosition + (hit.IsTrailing ? 1 : 0);

        SetCaret(_document.GetOffset(targetLine, column), extendSelection);
        _desiredCaretX = desiredX;
    }

    private int GetPageLineCount()
    {
        EnsureFontMetrics();
        return Math.Max(1, (int)(Bounds.Height / _lineHeight) - 1);
    }

    // ----------------------------------------------------------------------------------------
    // Mouse and keyboard
    // ----------------------------------------------------------------------------------------

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            return;
        }

        Focus();

        var offset = GetOffsetFromPoint(e.GetPosition(this));

        if (e.ClickCount == 2)
        {
            SelectWordAt(offset);
        }
        else if (e.ClickCount >= 3)
        {
            SelectLineAt(offset);
        }
        else
        {
            SetCaret(offset, (e.KeyModifiers & KeyModifiers.Shift) != 0);
            _isDragSelecting = true;
            e.Pointer.Capture(this);
        }

        _allowUndoMerge = false;
        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (!_isDragSelecting)
        {
            return;
        }

        var point = e.GetPosition(this);
        SetCaret(GetOffsetFromPoint(point), extendSelection: true);

        // Dragging past the edge keeps scrolling, the way a text box does.
        if (point.Y < 0 || point.Y > Bounds.Height || point.X < 0 || point.X > Bounds.Width)
        {
            BringCaretIntoView();
        }

        e.Handled = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (_isDragSelecting)
        {
            _isDragSelecting = false;
            e.Pointer.Capture(null);
        }
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);

        EnsureFontMetrics();

        var step = e.Delta.Y * _lineHeight * 3;

        // Shift+wheel scrolls sideways, like everywhere else.
        var offset = (e.KeyModifiers & KeyModifiers.Shift) != 0
            ? ScrollOffset.WithX(ScrollOffset.X - step)
            : ScrollOffset.WithY(ScrollOffset.Y - step);

        var before = ScrollOffset;
        ScrollOffset = offset;
        e.Handled = ScrollOffset != before;
    }

    /// <summary>
    /// Lets the platform IME (Chinese, Japanese, Korean input) place its candidate window next to
    /// the caret. Composition itself stays in the IME window - the committed text arrives through
    /// <see cref="OnTextInput"/> like any other typing.
    /// </summary>
    private sealed class SyntaxTextViewInputMethodClient : TextInputMethodClient
    {
        private readonly SyntaxTextView _view;

        public SyntaxTextViewInputMethodClient(SyntaxTextView view)
        {
            _view = view;
        }

        public override Visual TextViewVisual => _view;

        public override bool SupportsPreedit => false;

        public override bool SupportsSurroundingText => false;

        public override string SurroundingText => string.Empty;

        public override Rect CursorRectangle
        {
            get
            {
                var point = _view.GetPointFromOffset(_view.CaretOffset);
                return new Rect(point.X, point.Y, 1, _view.LineHeight);
            }
        }

        public override TextSelection Selection
        {
            get => new(_view.SelectionStart, _view.SelectionEnd);
            set => _view.Select(value.Start, value.End - value.Start);
        }

        public override void SetPreeditText(string? preeditText)
        {
            // Nothing to do: SupportsPreedit is false, so the IME draws its own composition.
        }

        internal void NotifyCaretMoved() => RaiseCursorRectangleChanged();
    }

    private void OnTextInputMethodClientRequested(object? sender, TextInputMethodClientRequestedEventArgs e)
    {
        if (!IsReadOnly)
        {
            e.Client = _inputMethodClient;
        }
    }

    protected override void OnTextInput(TextInputEventArgs e)
    {
        base.OnTextInput(e);

        if (IsReadOnly || string.IsNullOrEmpty(e.Text))
        {
            return;
        }

        InsertText(e.Text);
        e.Handled = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Handled)
        {
            return;
        }

        var shift = (e.KeyModifiers & KeyModifiers.Shift) != 0;

        // macOS uses Command for the shortcuts and Option+Arrow for word navigation.
        var commandModifier = OperatingSystem.IsMacOS()
            ? (e.KeyModifiers & KeyModifiers.Meta) != 0
            : (e.KeyModifiers & KeyModifiers.Control) != 0;
        var wordModifier = OperatingSystem.IsMacOS()
            ? (e.KeyModifiers & KeyModifiers.Alt) != 0
            : (e.KeyModifiers & KeyModifiers.Control) != 0;

        // Alt+Up/Down moves lines everywhere, including macOS - the same binding VS Code uses. The
        // guarded cases must come before the plain ones below or the compiler calls them subsumed.
        var altModifier = (e.KeyModifiers & KeyModifiers.Alt) != 0;

        var isNavigation = true;

        switch (e.Key)
        {
            case Key.Up when altModifier && !shift:
                isNavigation = false;
                MoveSelectedLines(-1);
                break;
            case Key.Down when altModifier && !shift:
                isNavigation = false;
                MoveSelectedLines(1);
                break;
            case Key.D when commandModifier && !shift:
                isNavigation = false;
                DuplicateSelectedLines();
                break;
            case Key.K when commandModifier && shift:
                isNavigation = false;
                DeleteSelectedLines();
                break;
            case Key.Back when wordModifier:
                isNavigation = false;
                DeleteWordLeft();
                break;
            case Key.Delete when wordModifier:
                isNavigation = false;
                DeleteWordRight();
                break;

            case Key.Left:
                SetCaret(wordModifier ? GetWordLeftOffset(_caretOffset) : GetPreviousCaretStop(_caretOffset), shift);
                break;
            case Key.Right:
                SetCaret(wordModifier ? GetWordRightOffset(_caretOffset) : GetNextCaretStop(_caretOffset), shift);
                break;
            case Key.Up:
                MoveCaretByLines(-1, shift);
                break;
            case Key.Down:
                MoveCaretByLines(1, shift);
                break;
            case Key.PageUp:
                MoveCaretByLines(-GetPageLineCount(), shift);
                break;
            case Key.PageDown:
                MoveCaretByLines(GetPageLineCount(), shift);
                break;
            case Key.Home:
                SetCaret(commandModifier ? 0 : GetLineStartForCaret(), shift);
                break;
            case Key.End:
                SetCaret(commandModifier ? _document.TextLength : GetLineEndForCaret(), shift);
                break;
            case Key.A when commandModifier:
                SelectAll();
                break;
            case Key.C when commandModifier:
                Copy();
                break;

            case Key.Back:
                isNavigation = false;
                Backspace();
                break;
            case Key.Delete:
                isNavigation = false;
                DeleteForward();
                break;
            case Key.Enter:
                isNavigation = false;
                if (IsReadOnly)
                {
                    return;
                }

                _allowUndoMerge = false;
                InsertText(_document.NewLine);
                _allowUndoMerge = false;
                break;
            case Key.Tab:
                if (IsReadOnly)
                {
                    return; // let Tab move the focus out of a read-only view
                }

                isNavigation = false;
                InsertText("\t");
                break;
            case Key.X when commandModifier:
                isNavigation = false;
                Cut();
                break;
            case Key.V when commandModifier:
                isNavigation = false;
                Paste();
                break;
            case Key.Z when commandModifier && !shift:
                isNavigation = false;
                Undo();
                break;
            case Key.Z when commandModifier && shift:
            case Key.Y when commandModifier:
                isNavigation = false;
                Redo();
                break;

            default:
                return;
        }

        if (isNavigation)
        {
            // Moving the caret ends the run of typed characters that undo takes back in one step.
            _allowUndoMerge = false;
            BringCaretIntoView();
        }

        e.Handled = true;
    }

    private int GetLineStartForCaret()
    {
        var position = _document.GetPosition(_caretOffset);
        var line = _document.GetLine(position.Line);

        var indent = 0;
        while (indent < line.Length && char.IsWhiteSpace(line[indent]))
        {
            indent++;
        }

        // Home goes to the first non-blank character, and to column 0 when already there.
        var lineStart = _document.GetLineStartOffset(position.Line);
        return _caretOffset == lineStart + indent ? lineStart : lineStart + indent;
    }

    private int GetLineEndForCaret()
    {
        return _document.GetLineEndOffset(_document.GetPosition(_caretOffset).Line);
    }

    private static bool IsWordChar(char c) => char.IsLetterOrDigit(c) || c == '_';

    private static bool IsNonNewlineWhiteSpace(char c) => c != '\n' && char.IsWhiteSpace(c);

    /// <summary>Same word stepping as the other SE editors: whitespace, then one character class.</summary>
    private int GetWordRightOffset(int offset)
    {
        var length = _document.TextLength;
        if (offset >= length)
        {
            return length;
        }

        while (offset < length && IsNonNewlineWhiteSpace(_document.GetCharAt(offset)))
        {
            offset++;
        }

        if (offset >= length)
        {
            return length;
        }

        if (_document.GetCharAt(offset) == '\n')
        {
            return GetNextCaretStop(offset);
        }

        var wordChar = IsWordChar(_document.GetCharAt(offset));
        while (offset < length &&
               !char.IsWhiteSpace(_document.GetCharAt(offset)) &&
               IsWordChar(_document.GetCharAt(offset)) == wordChar)
        {
            offset++;
        }

        return offset;
    }

    private int GetWordLeftOffset(int offset)
    {
        if (offset <= 0)
        {
            return 0;
        }

        while (offset > 0 && IsNonNewlineWhiteSpace(_document.GetCharAt(offset - 1)))
        {
            offset--;
        }

        if (offset == 0)
        {
            return 0;
        }

        if (_document.GetCharAt(offset - 1) == '\n')
        {
            return GetPreviousCaretStop(offset);
        }

        var wordChar = IsWordChar(_document.GetCharAt(offset - 1));
        while (offset > 0 &&
               !char.IsWhiteSpace(_document.GetCharAt(offset - 1)) &&
               IsWordChar(_document.GetCharAt(offset - 1)) == wordChar)
        {
            offset--;
        }

        return offset;
    }

    private void SelectWordAt(int offset)
    {
        var position = _document.GetPosition(offset);
        var line = _document.GetLine(position.Line);
        if (line.Length == 0)
        {
            SetCaret(offset, extendSelection: false);
            return;
        }

        var column = Math.Min(position.Column, line.Length - 1);
        var start = column;
        var end = column + 1;

        if (IsWordChar(line[column]))
        {
            while (start > 0 && IsWordChar(line[start - 1]))
            {
                start--;
            }

            end = column;
            while (end < line.Length && IsWordChar(line[end]))
            {
                end++;
            }
        }

        var lineStart = _document.GetLineStartOffset(position.Line);
        Select(lineStart + start, end - start);
    }

    private void SelectLineAt(int offset)
    {
        var position = _document.GetPosition(offset);
        var start = _document.GetLineStartOffset(position.Line);
        var end = position.Line < _document.LineCount - 1
            ? _document.GetLineStartOffset(position.Line + 1)
            : _document.GetLineEndOffset(position.Line);
        Select(start, end - start);
    }

    // ----------------------------------------------------------------------------------------
    // Editing and undo
    // ----------------------------------------------------------------------------------------

    private sealed class UndoEntry
    {
        public UndoEntry(int offset, string removed, string inserted, int caretBefore, int anchorBefore)
        {
            Offset = offset;
            Removed = removed;
            Inserted = inserted;
            CaretBefore = caretBefore;
            AnchorBefore = anchorBefore;
        }

        public int Offset { get; }

        public string Removed { get; }

        /// <summary>Grows while characters are typed on, so one undo takes back a whole word.</summary>
        public string Inserted { get; set; }

        public int CaretBefore { get; }

        public int AnchorBefore { get; }
    }

    public bool CanUndo => _undoStack.Count > 0;

    public bool CanRedo => _redoStack.Count > 0;

    /// <summary>
    /// Replaces <paramref name="removeLength"/> characters at <paramref name="offset"/> with
    /// <paramref name="insertText"/>, records undo and leaves the caret after the inserted text.
    /// Every change goes through here, so nothing can edit behind the undo stack's back.
    /// </summary>
    private void ApplyEdit(int offset, int removeLength, string insertText)
    {
        if (IsReadOnly)
        {
            return;
        }

        var textLength = _document.TextLength;
        offset = Math.Clamp(offset, 0, textLength);
        removeLength = Math.Clamp(removeLength, 0, textLength - offset);
        if (removeLength == 0 && string.IsNullOrEmpty(insertText))
        {
            return;
        }

        var removed = removeLength > 0 ? _document.GetText(offset, removeLength) : string.Empty;
        var caretBefore = _caretOffset;
        var anchorBefore = _selectionAnchor;

        if (removeLength > 0)
        {
            _document.Remove(offset, removeLength);
        }

        if (!string.IsNullOrEmpty(insertText))
        {
            _document.Insert(offset, insertText);
        }

        PushUndo(new UndoEntry(offset, removed, insertText ?? string.Empty, caretBefore, anchorBefore));

        SetCaret(offset + (insertText?.Length ?? 0), extendSelection: false);
        BringCaretIntoView();
    }

    private void PushUndo(UndoEntry entry)
    {
        _redoStack.Clear();

        // Typing merges into one undo step until the caret moves or a line break is typed - undoing
        // one character at a time is nobody's idea of a good time.
        if (_allowUndoMerge &&
            _undoStack.Count > 0 &&
            entry.Removed.Length == 0 &&
            entry.Inserted.Length > 0 &&
            entry.Inserted.IndexOfAny(LineBreakChars) < 0)
        {
            var last = _undoStack[^1];
            if (last.Removed.Length == 0 &&
                last.Inserted.Length > 0 &&
                last.Inserted.IndexOfAny(LineBreakChars) < 0 &&
                last.Offset + last.Inserted.Length == entry.Offset)
            {
                last.Inserted += entry.Inserted;
                return;
            }
        }

        _undoStack.Add(entry);
        _allowUndoMerge = true;

        if (_undoStack.Count > MaxUndoEntries)
        {
            _undoStack.RemoveAt(0);
        }
    }

    public void Undo()
    {
        if (IsReadOnly || _undoStack.Count == 0)
        {
            return;
        }

        var entry = _undoStack[^1];
        _undoStack.RemoveAt(_undoStack.Count - 1);

        if (entry.Inserted.Length > 0)
        {
            _document.Remove(entry.Offset, entry.Inserted.Length);
        }

        if (entry.Removed.Length > 0)
        {
            _document.Insert(entry.Offset, entry.Removed);
        }

        _redoStack.Add(entry);
        _allowUndoMerge = false;

        SetCaret(entry.CaretBefore, extendSelection: false);
        SelectionAnchor = entry.AnchorBefore;
        BringCaretIntoView();
    }

    public void Redo()
    {
        if (IsReadOnly || _redoStack.Count == 0)
        {
            return;
        }

        var entry = _redoStack[^1];
        _redoStack.RemoveAt(_redoStack.Count - 1);

        if (entry.Removed.Length > 0)
        {
            _document.Remove(entry.Offset, entry.Removed.Length);
        }

        if (entry.Inserted.Length > 0)
        {
            _document.Insert(entry.Offset, entry.Inserted);
        }

        _undoStack.Add(entry);
        _allowUndoMerge = false;

        SetCaret(entry.Offset + entry.Inserted.Length, extendSelection: false);
        BringCaretIntoView();
    }

    internal void ClearUndo()
    {
        _undoStack.Clear();
        _redoStack.Clear();
        _allowUndoMerge = false;
    }

    /// <summary>Replaces the selection (or inserts at the caret) with <paramref name="text"/>.</summary>
    public void InsertText(string text)
    {
        if (IsReadOnly || string.IsNullOrEmpty(text))
        {
            return;
        }

        ApplyEdit(SelectionStart, SelectionLength, text);
    }

    public void DeleteSelection()
    {
        if (IsReadOnly || SelectionLength == 0)
        {
            return;
        }

        _allowUndoMerge = false;
        ApplyEdit(SelectionStart, SelectionLength, string.Empty);
    }

    private void Backspace()
    {
        if (IsReadOnly)
        {
            return;
        }

        if (SelectionLength > 0)
        {
            DeleteSelection();
            return;
        }

        if (_caretOffset == 0)
        {
            return;
        }

        // A line break counts as one step even when it is two characters.
        var start = GetPreviousCaretStop(_caretOffset);
        _allowUndoMerge = false;
        ApplyEdit(start, _caretOffset - start, string.Empty);
    }

    /// <summary>Deletes the selection, or the character after the caret (the Delete key).</summary>
    public void DeleteForward()
    {
        if (IsReadOnly)
        {
            return;
        }

        if (SelectionLength > 0)
        {
            DeleteSelection();
            return;
        }

        var end = GetNextCaretStop(_caretOffset);
        if (end == _caretOffset)
        {
            return;
        }

        _allowUndoMerge = false;
        ApplyEdit(_caretOffset, end - _caretOffset, string.Empty);
    }

    /// <summary>Deletes from the caret back to the start of the word (Ctrl/Option+Backspace).</summary>
    public void DeleteWordLeft()
    {
        if (IsReadOnly)
        {
            return;
        }

        if (SelectionLength > 0)
        {
            DeleteSelection();
            return;
        }

        var start = GetWordLeftOffset(_caretOffset);
        if (start == _caretOffset)
        {
            return;
        }

        _allowUndoMerge = false;
        ApplyEdit(start, _caretOffset - start, string.Empty);
    }

    /// <summary>Deletes from the caret forward to the end of the word (Ctrl/Option+Delete).</summary>
    public void DeleteWordRight()
    {
        if (IsReadOnly)
        {
            return;
        }

        if (SelectionLength > 0)
        {
            DeleteSelection();
            return;
        }

        var end = GetWordRightOffset(_caretOffset);
        if (end == _caretOffset)
        {
            return;
        }

        _allowUndoMerge = false;
        ApplyEdit(_caretOffset, end - _caretOffset, string.Empty);
    }

    /// <summary>
    /// The first and last line the selection touches. The line commands work on whole lines, so a
    /// caret anywhere in a line is enough to include it.
    /// </summary>
    private (int First, int Last) GetSelectedLineRange()
    {
        var first = _document.GetPosition(SelectionStart).Line;
        var endPosition = _document.GetPosition(SelectionEnd);
        var last = endPosition.Line;

        // A selection that stops exactly at the start of a line does not really reach into it.
        if (last > first && endPosition.Column == 0)
        {
            last--;
        }

        return (first, last);
    }

    private string GetLinesText(int firstLine, int lastLine)
    {
        var start = _document.GetLineStartOffset(firstLine);
        return _document.GetText(start, _document.GetLineEndOffset(lastLine) - start);
    }

    /// <summary>
    /// Puts the caret and the selection back on text that moved <paramref name="lineDelta"/> lines,
    /// so the block stays selected and the command can be repeated.
    /// </summary>
    private void RestoreSelectionShiftedByLines(
        SyntaxTextPosition caretBefore,
        SyntaxTextPosition anchorBefore,
        int lineDelta)
    {
        _selectionAnchor = _document.GetOffset(anchorBefore.Line + lineDelta, anchorBefore.Column);
        SetCaret(_document.GetOffset(caretBefore.Line + lineDelta, caretBefore.Column), extendSelection: true);
        InvalidateVisual();
        BringCaretIntoView();
    }

    /// <summary>Moves the lines the selection touches one line up (-1) or down (+1).</summary>
    public void MoveSelectedLines(int lineDelta)
    {
        if (IsReadOnly || lineDelta == 0)
        {
            return;
        }

        var (first, last) = GetSelectedLineRange();
        if ((lineDelta < 0 && first == 0) || (lineDelta > 0 && last >= _document.LineCount - 1))
        {
            return;
        }

        var block = GetLinesText(first, last);
        var neighborLine = lineDelta < 0 ? first - 1 : last + 1;
        var neighbor = _document.GetLine(neighborLine);

        // Rewrite the block and the line it swaps with in one edit: one undo step, and the lines
        // below it never move, so the view keeps its cached layouts.
        var replacement = lineDelta < 0
            ? block + _document.NewLine + neighbor
            : neighbor + _document.NewLine + block;

        var caretBefore = _document.GetPosition(_caretOffset);
        var anchorBefore = _document.GetPosition(_selectionAnchor);

        var regionStart = _document.GetLineStartOffset(Math.Min(first, neighborLine));
        var regionEnd = _document.GetLineEndOffset(Math.Max(last, neighborLine));

        _allowUndoMerge = false;
        ApplyEdit(regionStart, regionEnd - regionStart, replacement);
        _allowUndoMerge = false;

        RestoreSelectionShiftedByLines(caretBefore, anchorBefore, lineDelta);
    }

    /// <summary>Inserts a copy of the lines the selection touches below them.</summary>
    public void DuplicateSelectedLines()
    {
        if (IsReadOnly)
        {
            return;
        }

        var (first, last) = GetSelectedLineRange();
        var block = GetLinesText(first, last);

        var caretBefore = _document.GetPosition(_caretOffset);
        var anchorBefore = _document.GetPosition(_selectionAnchor);

        _allowUndoMerge = false;
        ApplyEdit(_document.GetLineEndOffset(last), 0, _document.NewLine + block);
        _allowUndoMerge = false;

        // Land on the copy, so pressing it again duplicates the copy rather than the original.
        RestoreSelectionShiftedByLines(caretBefore, anchorBefore, last - first + 1);
    }

    /// <summary>Removes the lines the selection touches, line break and all.</summary>
    public void DeleteSelectedLines()
    {
        if (IsReadOnly)
        {
            return;
        }

        var (first, last) = GetSelectedLineRange();
        var start = _document.GetLineStartOffset(first);
        int end;

        if (last < _document.LineCount - 1)
        {
            end = _document.GetLineStartOffset(last + 1); // take the line break below with it
        }
        else if (first > 0)
        {
            start = _document.GetLineEndOffset(first - 1); // last line: take the break above instead
            end = _document.GetLineEndOffset(last);
        }
        else
        {
            end = _document.GetLineEndOffset(last); // nothing above to take a break from: empty it
        }

        _allowUndoMerge = false;
        ApplyEdit(start, end - start, string.Empty);
        _allowUndoMerge = false;
    }

    /// <summary>
    /// Replaces the whole text as one undoable edit. Assigning <see cref="Text"/> would go behind
    /// the undo stack's back, so replace-all uses this.
    /// </summary>
    public void ReplaceAllText(string newText)
    {
        if (IsReadOnly)
        {
            return;
        }

        _allowUndoMerge = false;
        ApplyEdit(0, _document.TextLength, newText ?? string.Empty);
        _allowUndoMerge = false;
    }

    /// <summary>The offset one character (or one whole line break) before <paramref name="offset"/>.</summary>
    private int GetPreviousCaretStop(int offset)
    {
        var position = _document.GetPosition(offset);
        if (position.Column > 0)
        {
            return offset - 1;
        }

        return position.Line > 0 ? _document.GetLineEndOffset(position.Line - 1) : 0;
    }

    private int GetNextCaretStop(int offset)
    {
        var position = _document.GetPosition(offset);
        if (position.Column < _document.GetLineLength(position.Line))
        {
            return offset + 1;
        }

        return position.Line < _document.LineCount - 1
            ? _document.GetLineStartOffset(position.Line + 1)
            : offset;
    }

    public void Copy() => _ = CopyAsync();

    public void Cut() => _ = CutAsync();

    public void Paste() => _ = PasteAsync();

    private async Task CopyAsync()
    {
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard == null || SelectionLength == 0)
        {
            return;
        }

        await clipboard.SetTextAsync(SelectedText);
    }

    private async Task CutAsync()
    {
        if (IsReadOnly || SelectionLength == 0)
        {
            return;
        }

        await CopyAsync();
        DeleteSelection();
    }

    private async Task PasteAsync()
    {
        if (IsReadOnly)
        {
            return;
        }

        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard == null)
        {
            return;
        }

        var text = await clipboard.TryGetTextAsync();
        if (!string.IsNullOrEmpty(text))
        {
            _allowUndoMerge = false;
            InsertText(text);
            _allowUndoMerge = false;
        }
    }
}
