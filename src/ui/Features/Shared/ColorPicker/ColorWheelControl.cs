using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Nikse.SubtitleEdit.Logic.Config;
using SkiaSharp;
using System;

namespace Nikse.SubtitleEdit.Features.Shared.ColorPicker;

public class ColorWheelControl : Control
{
    public static readonly StyledProperty<double> HueProperty =
        AvaloniaProperty.Register<ColorWheelControl, double>(nameof(Hue), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<double> SaturationProperty =
        AvaloniaProperty.Register<ColorWheelControl, double>(nameof(Saturation), defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    /// Hue in degrees (0-360).
    /// </summary>
    public double Hue
    {
        get => GetValue(HueProperty);
        set => SetValue(HueProperty, value);
    }

    /// <summary>
    /// Saturation (0-1), the distance from the wheel center.
    /// </summary>
    public double Saturation
    {
        get => GetValue(SaturationProperty);
        set => SetValue(SaturationProperty, value);
    }

    private Point _center;
    private double _radius;
    private Point _selectedPoint;
    private bool _isDragging;
    private SKBitmap? _wheelBitmap;

    static ColorWheelControl()
    {
        AffectsRender<ColorWheelControl>(HueProperty, SaturationProperty);
    }

    public ColorWheelControl()
    {
        Width = 200;
        Height = 200;
        ClipToBounds = true;

        // Custom-drawn control with no automation peer of its own; give it a name
        // so screen readers can announce it (issue #11553).
        AutomationProperties.SetName(this, Se.Language.General.Color);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == HueProperty || change.Property == SaturationProperty)
        {
            UpdateSelectedPoint();
            InvalidateVisual();
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        var point = e.GetPosition(this);
        if (IsPointInWheel(point))
        {
            _isDragging = true;
            UpdateColorFromPoint(point);
            e.Handled = true;
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (_isDragging)
        {
            var point = e.GetPosition(this);
            UpdateColorFromPoint(point);
            e.Handled = true;
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        _isDragging = false;
    }

    private bool IsPointInWheel(Point point)
    {
        var dx = point.X - _center.X;
        var dy = point.Y - _center.Y;
        var distance = Math.Sqrt(dx * dx + dy * dy);
        return distance <= _radius;
    }

    private void UpdateColorFromPoint(Point point)
    {
        var dx = point.X - _center.X;
        var dy = point.Y - _center.Y;
        var distance = Math.Sqrt(dx * dx + dy * dy);

        // Clamp to wheel radius
        if (distance > _radius)
        {
            distance = _radius;
        }

        // Calculate angle in degrees (0-360)
        var angle = Math.Atan2(dy, dx) * 180 / Math.PI;
        if (angle < 0)
        {
            angle += 360;
        }

        SetCurrentValue(HueProperty, angle);
        SetCurrentValue(SaturationProperty, distance / _radius);
        InvalidateVisual();
    }

    private void UpdateSelectedPoint()
    {
        var angle = Hue * Math.PI / 180;
        var distance = Math.Clamp(Saturation, 0, 1) * _radius;

        // Calculate point
        _selectedPoint = new Point(
            _center.X + distance * Math.Cos(angle),
            _center.Y + distance * Math.Sin(angle)
        );
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        _center = new Point(Bounds.Width / 2, Bounds.Height / 2);
        _radius = Math.Min(Bounds.Width, Bounds.Height) / 2 - 5;
        _wheelBitmap?.Dispose();
        _wheelBitmap = null;
        UpdateSelectedPoint();
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        _center = new Point(Bounds.Width / 2, Bounds.Height / 2);
        _radius = Math.Min(Bounds.Width, Bounds.Height) / 2 - 5;

        if (_wheelBitmap == null || _wheelBitmap.Width != (int)Bounds.Width || _wheelBitmap.Height != (int)Bounds.Height)
        {
            CreateWheelBitmap();
        }

        context.Custom(new ColorWheelDrawOperation(new Rect(0, 0, Bounds.Width, Bounds.Height), _wheelBitmap, _selectedPoint));
    }

    private void CreateWheelBitmap()
    {
        var width = (int)Bounds.Width;
        var height = (int)Bounds.Height;

        if (width <= 0 || height <= 0)
        {
            return;
        }

        _wheelBitmap?.Dispose();
        _wheelBitmap = new SKBitmap(width, height);

        using var canvas = new SKCanvas(_wheelBitmap);
        canvas.Clear(SKColors.Transparent);

        var center = new SKPoint((float)_center.X, (float)_center.Y);
        var radius = (float)_radius;

        // Draw color wheel with radial and angular gradients
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var dx = x - center.X;
                var dy = y - center.Y;
                var distance = Math.Sqrt(dx * dx + dy * dy);

                if (distance <= radius)
                {
                    var angle = Math.Atan2(dy, dx) * 180 / Math.PI;
                    if (angle < 0)
                    {
                        angle += 360;
                    }

                    var hue = (float)angle;
                    var saturation = (float)(distance / radius * 100);

                    var color = SKColor.FromHsv(hue, saturation, 100);
                    _wheelBitmap.SetPixel(x, y, color);
                }
            }
        }
    }

    private class ColorWheelDrawOperation : ICustomDrawOperation
    {
        private readonly Rect _bounds;
        private readonly SKBitmap? _bitmap;
        private readonly Point _selectedPoint;

        public ColorWheelDrawOperation(Rect bounds, SKBitmap? bitmap, Point selectedPoint)
        {
            _bounds = bounds;
            _bitmap = bitmap;
            _selectedPoint = selectedPoint;
        }

        public void Dispose() { }

        public Rect Bounds => _bounds;

        public bool HitTest(Point p) => _bounds.Contains(p);

        public bool Equals(ICustomDrawOperation? other) => false;

        public void Render(ImmediateDrawingContext context)
        {
            var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
            if (leaseFeature == null || _bitmap == null)
            {
                return;
            }

            using var lease = leaseFeature.Lease();
            var canvas = lease.SkCanvas;

            canvas.DrawBitmap(_bitmap, 0, 0);

            // Draw selection indicator - disposed, since Render runs on every frame and an
            // undisposed SKPaint holds native memory until a GC finalizer gets to it.
            using var paint = new SKPaint
            {
                Color = SKColors.White,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 2,
                IsAntialias = true
            };

            canvas.DrawCircle((float)_selectedPoint.X, (float)_selectedPoint.Y, 5, paint);

            paint.Color = SKColors.Black;
            paint.StrokeWidth = 1;
            canvas.DrawCircle((float)_selectedPoint.X, (float)_selectedPoint.Y, 6, paint);
        }
    }
}
