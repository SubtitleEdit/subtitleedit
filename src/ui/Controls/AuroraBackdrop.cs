using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Rendering.Composition;
using Avalonia.Skia;
using SkiaSharp;
using System;
using System.Diagnostics;

namespace Nikse.SubtitleEdit.Controls;

/// <summary>
/// An animated aurora (northern lights over a starry sky), drawn by a composition custom visual:
/// the animation runs on Avalonia's render thread, so it costs the UI thread nothing. The frame
/// loop only runs while the control is visible.
/// </summary>
public class AuroraBackdrop : Control
{
    private CompositionCustomVisual? _visual;

    public AuroraBackdrop()
    {
        IsHitTestVisible = false;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        var compositor = ElementComposition.GetElementVisual(this)?.Compositor;
        if (compositor == null)
        {
            return;
        }

        _visual = compositor.CreateCustomVisual(new AuroraHandler());
        ElementComposition.SetElementChildVisual(this, _visual);
        _visual.Size = new Vector(Bounds.Width, Bounds.Height);
        _visual.SendHandlerMessage(IsVisible);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _visual?.SendHandlerMessage(false);
        ElementComposition.SetElementChildVisual(this, null);
        _visual = null;
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        if (_visual != null)
        {
            _visual.Size = new Vector(e.NewSize.Width, e.NewSize.Height);
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsVisibleProperty)
        {
            _visual?.SendHandlerMessage(IsVisible);
        }
    }

    private sealed class AuroraHandler : CompositionCustomVisualHandler
    {
        private const int RibbonSteps = 48;

        private static readonly (SKColor Color, float Height, float Speed, float Offset)[] Ribbons =
        {
            (new SKColor(60, 255, 150), 0.42f, 0.23f, 0.0f),
            (new SKColor(40, 220, 255), 0.30f, 0.31f, 1.7f),
            (new SKColor(170, 90, 255), 0.22f, 0.19f, 3.1f),
            (new SKColor(80, 255, 200), 0.18f, 0.37f, 4.6f),
        };

        private static readonly float[] RibbonStops = { 0f, 0.35f, 1f };

        private readonly Stopwatch _clock = Stopwatch.StartNew();
        private readonly SKPaint _fillPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
        private readonly SKPaint _ribbonPaint = new()
        {
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            BlendMode = SKBlendMode.Plus,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 14),
        };

        private bool _running;
        private float _width;
        private float _height;
        private (float X, float Y, float Radius, float Speed)[] _stars = Array.Empty<(float, float, float, float)>();

        public override void OnMessage(object message)
        {
            if (message is bool running)
            {
                _running = running;
                if (running)
                {
                    RegisterForNextAnimationFrameUpdate();
                }
            }
        }

        public override void OnAnimationFrameUpdate()
        {
            if (!_running)
            {
                return;
            }

            var size = EffectiveSize;
            if ((float)size.X != _width || (float)size.Y != _height)
            {
                _width = (float)size.X;
                _height = (float)size.Y;
                MakeStars();
            }

            Invalidate();
            RegisterForNextAnimationFrameUpdate();
        }

        private void MakeStars()
        {
            var random = new Random(1234);
            var count = (int)Math.Clamp(_width * _height / 3000, 0, 1000);
            _stars = new (float, float, float, float)[count];
            for (var i = 0; i < count; i++)
            {
                _stars[i] = ((float)random.NextDouble() * _width,
                             (float)random.NextDouble() * _height * 0.8f,
                             0.6f + (float)random.NextDouble(),
                             1 + (float)random.NextDouble() * 2);
            }
        }

        public override void OnRender(ImmediateDrawingContext context)
        {
            if (_width < 1 || _height < 1)
            {
                return;
            }

            var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
            if (leaseFeature == null)
            {
                return;
            }

            using var lease = leaseFeature.Lease();
            var canvas = lease.SkCanvas;
            var t = (float)_clock.Elapsed.TotalSeconds;

            canvas.Save();
            canvas.ClipRect(new SKRect(0, 0, _width, _height));
            DrawSky(canvas, t);
            foreach (var ribbon in Ribbons)
            {
                DrawRibbon(canvas, t, ribbon.Color, ribbon.Height, ribbon.Speed, ribbon.Offset);
            }

            canvas.Restore();
        }

        private void DrawSky(SKCanvas canvas, float t)
        {
            using (var sky = SKShader.CreateLinearGradient(
                       new SKPoint(0, 0), new SKPoint(0, _height),
                       new[] { new SKColor(2, 6, 20, 235), new SKColor(6, 22, 40, 200), new SKColor(2, 10, 18, 235) },
                       null, SKShaderTileMode.Clamp))
            {
                _fillPaint.Shader = sky;
                canvas.DrawRect(0, 0, _width, _height, _fillPaint);
                _fillPaint.Shader = null;
            }

            for (var i = 0; i < _stars.Length; i++)
            {
                var (x, y, radius, speed) = _stars[i];
                var twinkle = 0.5f + 0.5f * MathF.Sin(t * speed + i);
                _fillPaint.Color = new SKColor(255, 255, 255, (byte)(40 + 140 * twinkle));
                canvas.DrawCircle(x, y, radius, _fillPaint);
            }
        }

        /// <summary>
        /// One curtain of light: a wavy lower edge with light rising from it and fading out upwards,
        /// its brightness rippling sideways like the real thing.
        /// </summary>
        private void DrawRibbon(SKCanvas canvas, float t, SKColor color, float heightFactor, float speed, float offset)
        {
            var step = _width / RibbonSteps;
            var curtain = _height * heightFactor;
            for (var i = 0; i < RibbonSteps; i++)
            {
                var x = i * step;
                var u = x / _width;
                var baseY = _height * (0.55f + 0.08f * MathF.Sin(u * 5f + t * speed * 2 + offset)
                                             + 0.05f * MathF.Sin(u * 11f - t * speed * 3 + offset * 2));
                var glow = 0.35f + 0.65f * MathF.Pow(0.5f + 0.5f * MathF.Sin(u * 17f - t * speed * 6 + offset), 2);
                var top = baseY - curtain * (0.7f + 0.3f * MathF.Sin(u * 7f + t * speed + offset));
                using var shader = SKShader.CreateLinearGradient(
                    new SKPoint(0, baseY), new SKPoint(0, top),
                    new[] { color.WithAlpha((byte)(150 * glow)), color.WithAlpha((byte)(60 * glow)), color.WithAlpha(0) },
                    RibbonStops, SKShaderTileMode.Clamp);
                _ribbonPaint.Shader = shader;
                canvas.DrawRect(x - 1, top, step + 2, baseY - top + 6, _ribbonPaint);
            }

            _ribbonPaint.Shader = null;
        }
    }
}
