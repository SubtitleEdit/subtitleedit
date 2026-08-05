using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using System;
using System.Globalization;

namespace Nikse.SubtitleEdit.Controls.SyntaxTextEditorControl;

/// <summary>
/// Draws line numbers for the lines a text view currently shows. Like the view it only touches the
/// visible range, so the cost per frame does not grow with the file size.
///
/// It is a standalone control: give it <see cref="LineCount"/>, <see cref="LineHeight"/> and the
/// current <see cref="VerticalOffset"/> and it draws itself - <see cref="SyntaxTextEditor"/> keeps
/// those in sync with its view.
/// </summary>
public class LineNumberGutter : Control
{
    public static readonly StyledProperty<int> LineCountProperty =
        AvaloniaProperty.Register<LineNumberGutter, int>(nameof(LineCount), 1);

    public static readonly StyledProperty<double> LineHeightProperty =
        AvaloniaProperty.Register<LineNumberGutter, double>(nameof(LineHeight), 16);

    public static readonly StyledProperty<double> VerticalOffsetProperty =
        AvaloniaProperty.Register<LineNumberGutter, double>(nameof(VerticalOffset));

    /// <summary>The caret's line, drawn brighter - zero based, negative for "none".</summary>
    public static readonly StyledProperty<int> CurrentLineProperty =
        AvaloniaProperty.Register<LineNumberGutter, int>(nameof(CurrentLine), -1);

    public static readonly StyledProperty<IBrush?> ForegroundProperty =
        TextElement.ForegroundProperty.AddOwner<LineNumberGutter>();

    public static readonly StyledProperty<FontFamily> FontFamilyProperty =
        TextElement.FontFamilyProperty.AddOwner<LineNumberGutter>();

    public static readonly StyledProperty<double> FontSizeProperty =
        TextElement.FontSizeProperty.AddOwner<LineNumberGutter>();

    public static readonly StyledProperty<IBrush?> BackgroundProperty =
        Border.BackgroundProperty.AddOwner<LineNumberGutter>();

    private const double PaddingLeft = 6;
    private const double PaddingRight = 8;

    private Typeface _typeface;
    private double _digitWidth;
    private double _measuredFontSize;
    private FontFamily? _measuredFontFamily;

    static LineNumberGutter()
    {
        AffectsRender<LineNumberGutter>(
            LineCountProperty,
            LineHeightProperty,
            VerticalOffsetProperty,
            CurrentLineProperty,
            ForegroundProperty,
            BackgroundProperty);

        AffectsMeasure<LineNumberGutter>(LineCountProperty, FontFamilyProperty, FontSizeProperty);
    }

    public int LineCount
    {
        get => GetValue(LineCountProperty);
        set => SetValue(LineCountProperty, value);
    }

    public double LineHeight
    {
        get => GetValue(LineHeightProperty);
        set => SetValue(LineHeightProperty, value);
    }

    public double VerticalOffset
    {
        get => GetValue(VerticalOffsetProperty);
        set => SetValue(VerticalOffsetProperty, value);
    }

    public int CurrentLine
    {
        get => GetValue(CurrentLineProperty);
        set => SetValue(CurrentLineProperty, value);
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

    public IBrush? Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    private void EnsureFontMetrics()
    {
        if (_measuredFontFamily == FontFamily && Math.Abs(_measuredFontSize - FontSize) < 0.01)
        {
            return;
        }

        _measuredFontFamily = FontFamily;
        _measuredFontSize = FontSize;
        _typeface = new Typeface(FontFamily);

        // Digits are the same width in the fonts used here, and close enough elsewhere: the
        // gutter is sized from the widest digit so the numbers never clip.
        var widest = 0.0;
        for (var digit = '0'; digit <= '9'; digit++)
        {
            var layout = new TextLayout(digit.ToString(), _typeface, FontSize, Brushes.Black);
            widest = Math.Max(widest, layout.WidthIncludingTrailingWhitespace);
        }

        _digitWidth = widest;
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        EnsureFontMetrics();
        var digits = Math.Max(2, LineCount.ToString(CultureInfo.InvariantCulture).Length);
        return new Size(Math.Ceiling(digits * _digitWidth + PaddingLeft + PaddingRight), 0);
    }

    public override void Render(DrawingContext context)
    {
        var height = Bounds.Height;
        var width = Bounds.Width;
        if (width <= 0 || height <= 0)
        {
            return;
        }

        if (Background != null)
        {
            context.FillRectangle(Background, new Rect(0, 0, width, height));
        }

        var lineHeight = LineHeight;
        if (lineHeight <= 0)
        {
            return;
        }

        EnsureFontMetrics();

        var foreground = Foreground ?? Brushes.Gray;
        var firstLine = Math.Max(0, (int)(VerticalOffset / lineHeight));
        var lastLine = Math.Min(LineCount - 1, (int)((VerticalOffset + height) / lineHeight));
        var currentLine = CurrentLine;

        // The first and last visible lines are usually only partially scrolled in - clip so
        // their numbers do not spill above or below the gutter (the view clips the same way).
        using (context.PushClip(new Rect(0, 0, width, height)))
        {
            for (var line = firstLine; line <= lastLine; line++)
            {
                var y = line * lineHeight - VerticalOffset;
                var text = (line + 1).ToString(CultureInfo.InvariantCulture);
                var layout = new TextLayout(text, _typeface, FontSize, foreground);

                // Right aligned, like every other editor gutter.
                var x = width - PaddingRight - layout.WidthIncludingTrailingWhitespace;
                if (line == currentLine)
                {
                    layout.Draw(context, new Point(x, y));
                }
                else
                {
                    // Everything but the caret's line is dimmed.
                    using (context.PushOpacity(0.55))
                    {
                        layout.Draw(context, new Point(x, y));
                    }
                }
            }
        }
    }
}
