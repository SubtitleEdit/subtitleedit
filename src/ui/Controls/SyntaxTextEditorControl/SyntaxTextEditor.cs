using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using System;

namespace Nikse.SubtitleEdit.Controls.SyntaxTextEditorControl;

/// <summary>
/// A syntax colored text editor for subtitle source text: a <see cref="LineNumberGutter"/>, a
/// virtualizing <see cref="SyntaxTextView"/> and the two scroll bars.
///
/// Only the visible lines are tokenized, laid out and drawn, so a 25 MB source opens and scrolls
/// like a small one. Word wrap is not supported - long lines scroll sideways instead.
/// </summary>
public class SyntaxTextEditor : Decorator
{
    // White like every other text input; the dark and pastel themes set their own through a style
    // (see UiTheme), and a caller can still set it directly.
    public static readonly StyledProperty<IBrush?> BackgroundProperty =
        Border.BackgroundProperty.AddOwner<SyntaxTextEditor>(
            new StyledPropertyMetadata<IBrush?>(defaultValue: new Optional<IBrush?>(Brushes.White)));

    public static readonly StyledProperty<IBrush?> ForegroundProperty =
        TextElement.ForegroundProperty.AddOwner<SyntaxTextEditor>();

    public static readonly StyledProperty<FontFamily> FontFamilyProperty =
        TextElement.FontFamilyProperty.AddOwner<SyntaxTextEditor>();

    public static readonly StyledProperty<double> FontSizeProperty =
        TextElement.FontSizeProperty.AddOwner<SyntaxTextEditor>();

    public static readonly StyledProperty<bool> ShowLineNumbersProperty =
        AvaloniaProperty.Register<SyntaxTextEditor, bool>(nameof(ShowLineNumbers), true);

    public static readonly StyledProperty<bool> IsReadOnlyProperty =
        SyntaxTextView.IsReadOnlyProperty.AddOwner<SyntaxTextEditor>();

    private readonly Grid _grid;
    private readonly SyntaxTextView _view;
    private readonly LineNumberGutter _gutter;
    private readonly ScrollBar _verticalScrollBar;
    private readonly ScrollBar _horizontalScrollBar;

    private bool _syncingScrollBars;

    public SyntaxTextEditor()
    {
        _view = new SyntaxTextView
        {
            [Grid.RowProperty] = 0,
            [Grid.ColumnProperty] = 1,
        };

        _gutter = new LineNumberGutter
        {
            [Grid.RowProperty] = 0,
            [Grid.ColumnProperty] = 0,
        };

        _verticalScrollBar = new ScrollBar
        {
            Orientation = Orientation.Vertical,
            [Grid.RowProperty] = 0,
            [Grid.ColumnProperty] = 2,
        };

        _horizontalScrollBar = new ScrollBar
        {
            Orientation = Orientation.Horizontal,
            [Grid.RowProperty] = 1,
            [Grid.ColumnProperty] = 0,
            [Grid.ColumnSpanProperty] = 2,
        };

        _grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
            },
            RowDefinitions =
            {
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto),
            },
            Children = { _gutter, _view, _verticalScrollBar, _horizontalScrollBar },
            Background = Background,
        };

        Child = _grid;

        _view.ScrollMetricsChanged += (_, _) => SyncScrollBars();
        _view.CaretChanged += (_, _) =>
        {
            _gutter.CurrentLine = _view.CaretLine;
            CaretChanged?.Invoke(this, EventArgs.Empty);
        };
        _view.TextChanged += (_, _) =>
        {
            SyncGutter();
            TextChanged?.Invoke(this, EventArgs.Empty);
        };

        _verticalScrollBar.Scroll += (_, _) => OnScrollBarMoved();
        _horizontalScrollBar.Scroll += (_, _) => OnScrollBarMoved();
        _verticalScrollBar.PropertyChanged += OnScrollBarPropertyChanged;
        _horizontalScrollBar.PropertyChanged += OnScrollBarPropertyChanged;

        SyncGutter();
    }

    /// <summary>Raised after the text changed.</summary>
    public event EventHandler? TextChanged;

    /// <summary>Raised when the caret moved or the selection changed.</summary>
    public event EventHandler? CaretChanged;

    /// <summary>The text surface - exposed for hit testing and focus.</summary>
    public SyntaxTextView View => _view;

    public IBrush? Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
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

    public bool ShowLineNumbers
    {
        get => GetValue(ShowLineNumbersProperty);
        set => SetValue(ShowLineNumbersProperty, value);
    }

    public bool IsReadOnly
    {
        get => GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    public string Text
    {
        get => _view.Text;
        set => _view.Text = value;
    }

    public SyntaxTextDocument Document => _view.Document;

    /// <summary>The syntax rules; null means no coloring.</summary>
    public ISourceSyntaxHighlighter? SourceHighlighter
    {
        get => _view.SourceHighlighter;
        set => _view.SourceHighlighter = value;
    }

    public int CaretOffset
    {
        get => _view.CaretOffset;
        set
        {
            _view.CaretOffset = value;
            _view.BringCaretIntoView();
        }
    }

    public int SelectionStart => _view.SelectionStart;

    public int SelectionLength => _view.SelectionLength;

    public string SelectedText => _view.SelectedText;

    public void Select(int start, int length) => _view.Select(start, length);

    public void SelectAll() => _view.SelectAll();

    public void ClearSelection() => _view.ClearSelection();

    public void Copy() => _view.Copy();

    public void Cut() => _view.Cut();

    public void Paste() => _view.Paste();

    public void Undo() => _view.Undo();

    public void Redo() => _view.Redo();

    public void InsertText(string text) => _view.InsertText(text);

    public void BringCaretIntoView() => _view.BringCaretIntoView();

    /// <summary>Moves the lines the selection touches one line up (-1) or down (+1).</summary>
    public void MoveSelectedLines(int lineDelta) => _view.MoveSelectedLines(lineDelta);

    public void DuplicateSelectedLines() => _view.DuplicateSelectedLines();

    public void DeleteSelectedLines() => _view.DeleteSelectedLines();

    public void DeleteWordLeft() => _view.DeleteWordLeft();

    public void DeleteWordRight() => _view.DeleteWordRight();

    /// <summary>Replaces the whole text as one undoable edit - see the note on the view.</summary>
    public void ReplaceAllText(string newText) => _view.ReplaceAllText(newText);

    public bool CanUndo => _view.CanUndo;

    public bool CanRedo => _view.CanRedo;

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ShowLineNumbersProperty)
        {
            _gutter.IsVisible = ShowLineNumbers;
        }
        else if (change.Property == BackgroundProperty)
        {
            _grid.Background = Background;
        }
        else if (change.Property == IsReadOnlyProperty)
        {
            _view.IsReadOnly = IsReadOnly;
        }
    }

    protected override void OnGotFocus(FocusChangedEventArgs e)
    {
        base.OnGotFocus(e);
        _view.Focus();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        // Clicking the gutter or the padding still puts the caret where the user pointed.
        if (!_view.IsFocused && !e.Handled)
        {
            _view.Focus();
        }
    }

    private void OnScrollBarPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == RangeBase.ValueProperty)
        {
            OnScrollBarMoved();
        }
    }

    private void OnScrollBarMoved()
    {
        if (_syncingScrollBars)
        {
            return;
        }

        _view.ScrollOffset = new Vector(_horizontalScrollBar.Value, _verticalScrollBar.Value);
        _gutter.VerticalOffset = _view.ScrollOffset.Y;
    }

    private void SyncScrollBars()
    {
        if (_syncingScrollBars)
        {
            return;
        }

        _syncingScrollBars = true;
        try
        {
            var extent = _view.Extent;
            var viewport = _view.Viewport;
            var offset = _view.ScrollOffset;
            var lineHeight = _view.LineHeight;

            var maxVertical = Math.Max(0, extent.Height - viewport.Height);
            _verticalScrollBar.Maximum = maxVertical;
            _verticalScrollBar.ViewportSize = viewport.Height;
            _verticalScrollBar.SmallChange = lineHeight;
            _verticalScrollBar.LargeChange = Math.Max(lineHeight, viewport.Height - lineHeight);
            _verticalScrollBar.Value = Math.Min(offset.Y, maxVertical);
            _verticalScrollBar.IsVisible = maxVertical > 0.5;

            var maxHorizontal = Math.Max(0, extent.Width - viewport.Width);
            _horizontalScrollBar.Maximum = maxHorizontal;
            _horizontalScrollBar.ViewportSize = viewport.Width;
            _horizontalScrollBar.SmallChange = lineHeight;
            _horizontalScrollBar.LargeChange = Math.Max(lineHeight, viewport.Width - lineHeight);
            _horizontalScrollBar.Value = Math.Min(offset.X, maxHorizontal);
            _horizontalScrollBar.IsVisible = maxHorizontal > 0.5;

            _gutter.VerticalOffset = offset.Y;
            _gutter.LineHeight = lineHeight;
        }
        finally
        {
            _syncingScrollBars = false;
        }
    }

    private void SyncGutter()
    {
        _gutter.LineCount = _view.Document.LineCount;
        _gutter.LineHeight = _view.LineHeight;
        _gutter.CurrentLine = _view.CaretLine;
        _gutter.VerticalOffset = _view.ScrollOffset.Y;
    }
}
