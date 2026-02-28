using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using SkiaSharp;
using System;

namespace Nikse.SubtitleEdit.Features.Shared.ColorPicker;

public class ColorWheelControl : Control
{
    public static readonly StyledProperty<Color> SelectedColorProperty =
        AvaloniaProperty.Register<ColorWheelControl, Color>(nameof(SelectedColor), Colors.White);

    public Color SelectedColor
    {
        get => GetValue(SelectedColorProperty);
        set => SetValue(SelectedColorProperty, value);
    }

    public event EventHandler<Color>? ColorChanged;

    private Point _center;
    private double _radius;
    private Point _selectedPoint;
    private bool _isDragging;
    private SKBitmap? _wheelBitmap;

    static ColorWheelControl()
    {
        AffectsRender<ColorWheelControl>(SelectedColorProperty);
    }

    public ColorWheelControl()
    {
        Width = 200;
        Height = 200;
        ClipToBounds = true;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SelectedColorProperty)
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
        if (angle < 0) angle += 360;

        // Convert to HSV
        var hue = (int)(angle / 360 * 255);
        var saturation = (int)(distance / _radius * 255);
        var value = 255; // Full brightness

        // Convert HSV to RGB
        var color = HsvToColor(SelectedColor.A, hue, saturation, value);
        SelectedColor = color;
        ColorChanged?.Invoke(this, color);
        InvalidateVisual();
    }

    private void UpdateSelectedPoint()
    {
        // Convert current color to HSV to get position
        var hsv = RgbToHsv(SelectedColor);

        // Calculate angle from hue
        var angle = (double)hsv.Hue / 255 * 360 * Math.PI / 180;

        // Calculate distance from saturation
        var distance = (double)hsv.Saturation / 255 * _radius;

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

        if (width <= 0 || height <= 0) return;

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
                    if (angle < 0) angle += 360;

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
            if (leaseFeature == null || _bitmap == null) return;

            using var lease = leaseFeature.Lease();
            var canvas = lease.SkCanvas;

            canvas.DrawBitmap(_bitmap, 0, 0);

            // Draw selection indicator
            var paint = new SKPaint
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

    private static Color HsvToColor(byte alpha, int hue, int saturation, int value)
    {
        double r = 0, g = 0, b = 0;

        var h = ((double)hue / 255 * 360) % 360;
        var s = (double)saturation / 255;
        var v = (double)value / 255;

        if (Math.Abs(s) < 0.01)
        {
            r = g = b = v;
        }
        else
        {
            var sectorPos = h / 60;
            var sectorNumber = (int)Math.Floor(sectorPos);
            var fractionalSector = sectorPos - sectorNumber;

            var p = v * (1 - s);
            var q = v * (1 - (s * fractionalSector));
            var t = v * (1 - (s * (1 - fractionalSector)));

            switch (sectorNumber)
            {
                case 0: r = v; g = t; b = p; break;
                case 1: r = q; g = v; b = p; break;
                case 2: r = p; g = v; b = t; break;
                case 3: r = p; g = q; b = v; break;
                case 4: r = t; g = p; b = v; break;
                case 5: r = v; g = p; b = q; break;
            }
        }

        return Color.FromArgb(alpha, (byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
    }

    private static (int Hue, int Saturation, int Value) RgbToHsv(Color color)
    {
        var r = (double)color.R / 255;
        var g = (double)color.G / 255;
        var b = (double)color.B / 255;

        var min = Math.Min(Math.Min(r, g), b);
        var max = Math.Max(Math.Max(r, g), b);

        double h, s;
        var v = max;
        var delta = max - min;

        if (Math.Abs(max) < 0.01 || Math.Abs(delta) < 0.01)
        {
            s = 0;
            h = 0;
        }
        else
        {
            s = delta / max;
            if (Math.Abs(r - max) < 0.01)
                h = (g - b) / delta;
            else if (Math.Abs(g - max) < 0.01)
                h = 2 + (b - r) / delta;
            else
                h = 4 + (r - g) / delta;
        }

        h *= 60;
        if (h < 0) h += 360;

        return ((int)(h / 360 * 255), (int)(s * 255), (int)(v * 255));
    }
}
