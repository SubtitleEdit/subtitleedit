using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic.Config;
using System;

namespace Nikse.SubtitleEdit.Features.Shared.ColorPicker;

/// <summary>
/// Vertical brightness (tone) bar next to the color wheel: top is the full-brightness
/// color of the current hue/saturation, bottom is black (issue #15296).
/// </summary>
public class ColorBrightnessBar : Control
{
    private const double MarkerWidth = 8;

    public static readonly StyledProperty<double> ValueProperty =
        AvaloniaProperty.Register<ColorBrightnessBar, double>(nameof(Value), 1, defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<Color> TopColorProperty =
        AvaloniaProperty.Register<ColorBrightnessBar, Color>(nameof(TopColor), Colors.White);

    /// <summary>
    /// Brightness (0-1), 1 at the top.
    /// </summary>
    public double Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public Color TopColor
    {
        get => GetValue(TopColorProperty);
        set => SetValue(TopColorProperty, value);
    }

    private bool _isDragging;

    static ColorBrightnessBar()
    {
        AffectsRender<ColorBrightnessBar>(ValueProperty, TopColorProperty);
        FocusableProperty.OverrideDefaultValue<ColorBrightnessBar>(true);
    }

    public ColorBrightnessBar()
    {
        Width = 20 + MarkerWidth;
        Height = 200;
        Cursor = new Cursor(StandardCursorType.Hand);
        AutomationProperties.SetName(this, Se.Language.Tools.ColorPickerBrightness);
    }

    private Rect BarRect => new Rect(0, 0, Math.Max(0, Bounds.Width - MarkerWidth), Bounds.Height);

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            _isDragging = true;
            e.Pointer.Capture(this);
            UpdateValueFromPoint(e.GetPosition(this));
            e.Handled = true;
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (_isDragging)
        {
            UpdateValueFromPoint(e.GetPosition(this));
            e.Handled = true;
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        _isDragging = false;
        e.Pointer.Capture(null);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        var step = 1.0 / 255;
        if (e.Key == Key.Up)
        {
            SetCurrentValue(ValueProperty, Math.Min(1, Value + step));
            e.Handled = true;
        }
        else if (e.Key == Key.Down)
        {
            SetCurrentValue(ValueProperty, Math.Max(0, Value - step));
            e.Handled = true;
        }
        else if (e.Key == Key.PageUp)
        {
            SetCurrentValue(ValueProperty, Math.Min(1, Value + step * 16));
            e.Handled = true;
        }
        else if (e.Key == Key.PageDown)
        {
            SetCurrentValue(ValueProperty, Math.Max(0, Value - step * 16));
            e.Handled = true;
        }
    }

    private void UpdateValueFromPoint(Point point)
    {
        var height = Bounds.Height;
        if (height <= 0)
        {
            return;
        }

        SetCurrentValue(ValueProperty, Math.Clamp(1 - point.Y / height, 0, 1));
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var bar = BarRect;
        var brush = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0.5, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(0.5, 1, RelativeUnit.Relative),
            GradientStops =
            {
                new GradientStop(Color.FromRgb(TopColor.R, TopColor.G, TopColor.B), 0),
                new GradientStop(Colors.Black, 1),
            },
        };
        context.DrawRectangle(brush, new Pen(Brushes.Gray), bar);

        // Triangle marker pointing at the bar, like the Subtitle Edit 4 color dialog
        var y = (1 - Math.Clamp(Value, 0, 1)) * Bounds.Height;
        var geometry = new StreamGeometry();
        using (var ctx = geometry.Open())
        {
            ctx.BeginFigure(new Point(bar.Right + 1, y), true);
            ctx.LineTo(new Point(Bounds.Width, y - MarkerWidth / 2));
            ctx.LineTo(new Point(Bounds.Width, y + MarkerWidth / 2));
            ctx.EndFigure(true);
        }

        var markerBrush = IsFocused ? Brushes.DodgerBlue : Brushes.Gray;
        context.DrawGeometry(markerBrush, null, geometry);

        // Thin line across the bar so the position is visible on both light and dark tones
        var lineBrush = Value > 0.5 && TopColor.R + TopColor.G + TopColor.B > 382 ? Brushes.Black : Brushes.White;
        context.DrawLine(new Pen(lineBrush, 1), new Point(bar.Left, y), new Point(bar.Right, y));
    }

    protected override void OnGotFocus(FocusChangedEventArgs e)
    {
        base.OnGotFocus(e);
        InvalidateVisual();
    }

    protected override void OnLostFocus(FocusChangedEventArgs e)
    {
        base.OnLostFocus(e);
        InvalidateVisual();
    }
}
