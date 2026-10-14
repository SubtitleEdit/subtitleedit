using Avalonia;
using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect.Effects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect;

/// <summary>
/// Draws a looping, stylised approximation of an advanced effect with plain Avalonia drawing,
/// for the gallery tiles and for the preview when no video is loaded. Text is laid out per
/// text element so each letter/word can be animated on its own.
/// </summary>
public static class EffectPreviewRenderer
{
    public const double CycleSeconds = 3.6;

    private const string ScrambleChars = "ABCDEFGHJKLMNPQRSTUVWXYZ0123456789#$%&@";
    private const string MatrixChars = "01アイウエオカキクケコサシスセソタチツテト";

    private static readonly Typeface BoldTypeface = new(FontFamily.Default, FontStyle.Normal, FontWeight.Bold);
    private static readonly IBrush OutlineBrush = new SolidColorBrush(Color.FromArgb(0xE6, 0x10, 0x10, 0x14));
    private static readonly Dictionary<(string Text, double Size), (Geometry? Geometry, double Width, double Height)> GlyphCache = new();
    private static readonly Dictionary<(string Text, double Size, double MaxWidth, int MaxLines), PreviewTextLayout> LayoutCache = new();
    private static Geometry? _heartGeometry;

    private sealed class PreviewGlyph
    {
        public required string Text { get; init; }
        public Geometry? Geometry { get; init; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; init; }
        public double Height { get; init; }
        public int Index { get; init; }
        public int Word { get; init; }
        public int Line { get; set; }
        public int WordInLine { get; set; }
        public int WordsInLine { get; set; }
        public bool IsSpace { get; init; }
    }

    private sealed class PreviewTextLayout
    {
        public List<PreviewGlyph> Glyphs { get; } = new();
        public int WordCount { get; set; }
        public int VisibleCount { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double LineHeight { get; set; }
        public double FontSize { get; init; }
    }

    private readonly struct GlyphStyle
    {
        public IBrush Fill { get; init; }
        public double Opacity { get; init; }
        public double ScaleX { get; init; }
        public double ScaleY { get; init; }
        public double Dx { get; init; }
        public double Dy { get; init; }
        public Color? Glow { get; init; }
        public string? Replacement { get; init; }

        public static GlyphStyle Default(IBrush fill) => new() { Fill = fill, Opacity = 1, ScaleX = 1, ScaleY = 1 };
    }

    /// <summary>
    /// A representative moment of the loop for a still (non-animating) tile.
    /// </summary>
    public static double GetPosterTime(EffectPreviewKind kind)
    {
        var p = kind switch
        {
            EffectPreviewKind.ScreenFadeIn => 0.3,
            EffectPreviewKind.ScreenFadeOut => 0.75,
            EffectPreviewKind.TvClose => 0.35,
            EffectPreviewKind.Typewriter or EffectPreviewKind.TypewriterHighlight => 0.42,
            EffectPreviewKind.Karaoke or EffectPreviewKind.WordPop or EffectPreviewKind.ScrambleReveal => 0.45,
            EffectPreviewKind.HighlightBox or EffectPreviewKind.BoldCaptions => 0.35,
            EffectPreviewKind.SingleWordPop => 0.3,
            EffectPreviewKind.SmoothFill => 0.42,
            EffectPreviewKind.BlurInWords => 0.32,
            EffectPreviewKind.RollingCaptions => 0.7,
            EffectPreviewKind.WordByWord => 0.4,
            EffectPreviewKind.SlideInLeft or EffectPreviewKind.SlideInRight => 0.13,
            EffectPreviewKind.BounceIn => 0.13,
            EffectPreviewKind.WordFlip3D => 0.17,
            EffectPreviewKind.CinematicTitle => 0.3,
            EffectPreviewKind.StarWarsScroll or EffectPreviewKind.CreditsScroll => 0.45,
            EffectPreviewKind.Confetti => 0.16,
            EffectPreviewKind.Glitch => 0.08,
            _ => 0.6,
        };
        return p * CycleSeconds;
    }

    public static void Render(DrawingContext ctx, Rect bounds, IAdvancedEffectDisplay? effect, double time, string text)
    {
        if (bounds.Width < 4 || bounds.Height < 4)
        {
            return;
        }

        var kind = AdvancedEffectCatalog.GetPreviewKind(effect);
        var t = time % CycleSeconds;
        var p = t / CycleSeconds;

        DrawBackground(ctx, bounds, kind, time);

        // tiles get relatively big text so it reads at thumbnail size; the large preview uses subtitle-like proportions
        var small = bounds.Height < 120;
        var fontSize = Math.Clamp(Math.Min(bounds.Height * (small ? 0.19 : 0.11), bounds.Width * (small ? 0.1 : 0.06)), 8, 64);
        var maxLines = bounds.Height > 150 ? 3 : 2;
        if (kind is EffectPreviewKind.LowerThird or EffectPreviewKind.CreditsScroll)
        {
            fontSize *= 0.8;
        }

        if (effect is AdvancedEffectHighlightBox { UpperCase: true } or AdvancedEffectSingleWordPop { UpperCase: true } or
            AdvancedEffectBoldCaptions { UpperCase: true } or AdvancedEffectSmoothFill { UpperCase: true } or AdvancedEffectBlurInWords { UpperCase: true })
        {
            text = text.ToUpper(CultureInfo.CurrentCulture);
        }

        var layout = GetLayout(text, fontSize, bounds.Width * 0.86, maxLines);
        if (layout.Glyphs.Count == 0)
        {
            return;
        }

        var origin = new Point(
            bounds.X + (bounds.Width - layout.Width) / 2,
            bounds.Bottom - layout.Height - bounds.Height * 0.1);

        switch (kind)
        {
            case EffectPreviewKind.ScreenFadeIn:
                DrawText(ctx, layout, origin, g => GlyphStyle.Default(Brushes.White));
                FillRect(ctx, bounds, Colors.Black, 1 - EaseOutCubic(Clamp01(p / 0.6)));
                break;
            case EffectPreviewKind.ScreenFadeOut:
                DrawText(ctx, layout, origin, g => GlyphStyle.Default(Brushes.White));
                FillRect(ctx, bounds, Colors.Black, EaseInCubic(Clamp01((p - 0.35) / 0.55)));
                break;
            case EffectPreviewKind.TvClose:
                DrawTvClose(ctx, bounds, layout, origin, p);
                break;
            case EffectPreviewKind.FadeInOut:
            {
                var opacity = Math.Min(Clamp01(p / 0.2), Clamp01((1 - p) / 0.2));
                DrawText(ctx, layout, origin, g => GlyphStyle.Default(Brushes.White) with { Opacity = opacity });
                break;
            }
            case EffectPreviewKind.SlideInLeft:
            case EffectPreviewKind.SlideInRight:
            {
                var direction = kind == EffectPreviewKind.SlideInLeft ? -1 : 1;
                var into = EaseOutCubic(Clamp01(p / 0.25));
                var away = EaseInCubic(Clamp01((p - 0.8) / 0.18));
                var dx = direction * ((1 - into) + away) * (bounds.Width * 0.5 + layout.Width / 2);
                DrawText(ctx, layout, origin, g => GlyphStyle.Default(Brushes.White) with { Dx = dx });
                break;
            }
            case EffectPreviewKind.BounceIn:
                DrawText(ctx, layout, origin, g =>
                {
                    var local = Clamp01((p - g.Index * 0.03) / 0.2);
                    var s = EaseOutBack(local, 2.6);
                    return GlyphStyle.Default(Brushes.White) with { ScaleX = s, ScaleY = s, Opacity = local > 0 ? 1 : 0 };
                });
                break;
            case EffectPreviewKind.ZoomIn:
            case EffectPreviewKind.ZoomOut:
            {
                var e = EaseInOutSine(p);
                var s = kind == EffectPreviewKind.ZoomIn ? 1 + 0.12 * e : 1.12 - 0.12 * e;
                var center = new Point(origin.X + layout.Width / 2, origin.Y + layout.Height / 2);
                using (ctx.PushTransform(ScaleAbout(center, s, s)))
                {
                    DrawText(ctx, layout, origin, g => GlyphStyle.Default(Brushes.White));
                }
                break;
            }
            case EffectPreviewKind.Typewriter:
            case EffectPreviewKind.TypewriterHighlight:
            {
                var visible = Clamp01(p / 0.7) * layout.VisibleCount;
                var highlight = kind == EffectPreviewKind.TypewriterHighlight;
                var yellow = new SolidColorBrush(Color.FromRgb(0xFF, 0xE0, 0x5A));
                DrawText(ctx, layout, origin, g =>
                {
                    if (g.Index >= visible)
                    {
                        return GlyphStyle.Default(Brushes.White) with { Opacity = 0 };
                    }

                    var isCurrent = highlight && g.Index >= visible - 1 && p < 0.72;
                    return isCurrent
                        ? GlyphStyle.Default(yellow) with { Glow = Color.FromRgb(0xFF, 0xC4, 0x00), ScaleX = 1.15, ScaleY = 1.15 }
                        : GlyphStyle.Default(Brushes.White);
                });
                break;
            }
            case EffectPreviewKind.WordByWord:
            {
                var progress = Clamp01(p / 0.7) * layout.WordCount;
                DrawText(ctx, layout, origin, g =>
                {
                    var local = Clamp01((progress - g.Word) * 2.5);
                    return GlyphStyle.Default(Brushes.White) with
                    {
                        Opacity = local,
                        Dy = (1 - EaseOutCubic(local)) * layout.FontSize * 0.35,
                    };
                });
                break;
            }
            case EffectPreviewKind.Karaoke:
            {
                var rightToLeft = effect is AdvancedEffectKaraoke { RightToLeft: true };
                var progress = Clamp01(p / 0.8) * layout.VisibleCount;
                var sung = new SolidColorBrush(Color.FromRgb(0xFF, 0xD6, 0x0A));
                var unsung = new SolidColorBrush(Color.FromRgb(0xF2, 0xF2, 0xF2));
                DrawText(ctx, layout, origin, g =>
                {
                    var index = rightToLeft ? layout.VisibleCount - 1 - g.Index : g.Index;
                    return GlyphStyle.Default(index < progress ? sung : unsung);
                });
                break;
            }
            case EffectPreviewKind.WordPop:
                DrawWordPop(ctx, layout, origin, p, effect as AdvancedEffectFancyKaraoke);
                break;
            case EffectPreviewKind.HighlightBox:
                DrawHighlightBox(ctx, layout, origin, p, effect as AdvancedEffectHighlightBox);
                break;
            case EffectPreviewKind.SingleWordPop:
                DrawSingleWordPop(ctx, bounds, text, fontSize, p, effect as AdvancedEffectSingleWordPop);
                break;
            case EffectPreviewKind.BoldCaptions:
                DrawBoldCaptions(ctx, layout, origin, p, effect as AdvancedEffectBoldCaptions);
                break;
            case EffectPreviewKind.SmoothFill:
                DrawSmoothFill(ctx, layout, origin, p, effect as AdvancedEffectSmoothFill);
                break;
            case EffectPreviewKind.BlurInWords:
                DrawBlurInWords(ctx, layout, origin, p, effect as AdvancedEffectBlurInWords);
                break;
            case EffectPreviewKind.RollingCaptions:
                DrawRollingCaptions(ctx, bounds, text, fontSize, p);
                break;
            case EffectPreviewKind.ScrambleReveal:
            {
                var reveal = Clamp01(p / 0.7) * layout.VisibleCount;
                var frame = (int)(time * 14);
                var green = new SolidColorBrush(Color.FromRgb(0x7C, 0xFF, 0xB2));
                DrawText(ctx, layout, origin, g =>
                {
                    if (g.Index < reveal)
                    {
                        return GlyphStyle.Default(Brushes.White);
                    }

                    var c = ScrambleChars[(frame * 31 + g.Index * 17) % ScrambleChars.Length];
                    return GlyphStyle.Default(green) with { Replacement = c.ToString(), Opacity = 0.85 };
                });
                break;
            }
            case EffectPreviewKind.WordFlip3D:
                DrawText(ctx, layout, origin, g =>
                {
                    var local = Clamp01((p - g.Word * 0.12) / 0.16);
                    return GlyphStyle.Default(Brushes.White) with
                    {
                        ScaleY = EaseOutBack(local, 2.2),
                        ScaleX = 0.85 + 0.15 * local,
                        Opacity = local > 0 ? 1 : 0,
                    };
                });
                break;
            case EffectPreviewKind.WordSpacing:
            {
                var extra = layout.FontSize * 0.3 * EaseInOutSine(Clamp01(p / 0.45));
                DrawText(ctx, layout, origin, g => GlyphStyle.Default(Brushes.White) with
                {
                    Dx = (g.WordInLine - (g.WordsInLine - 1) / 2.0) * extra,
                });
                break;
            }
            case EffectPreviewKind.RainbowPulse:
                DrawText(ctx, layout, origin, g =>
                {
                    var hue = (time * 0.35 + g.Index * 0.07) % 1.0;
                    var s = 1 + 0.07 * Math.Sin(time * 5 + g.Index * 0.6);
                    return GlyphStyle.Default(new SolidColorBrush(FromHsv(hue, 0.75, 1))) with { ScaleX = s, ScaleY = s };
                });
                break;
            case EffectPreviewKind.Wave:
            case EffectPreviewKind.WaveBlue:
            {
                var blue = kind == EffectPreviewKind.WaveBlue;
                DrawText(ctx, layout, origin, g =>
                {
                    var wave = Math.Sin(time * 5 - g.Index * 0.55);
                    var fill = blue
                        ? new SolidColorBrush(Lerp(Color.FromRgb(0x3A, 0x7B, 0xFF), Color.FromRgb(0x5C, 0xF2, 0xFF), (wave + 1) / 2))
                        : (IBrush)Brushes.White;
                    return GlyphStyle.Default(fill) with { Dy = -wave * layout.FontSize * 0.22 };
                });
                break;
            }
            case EffectPreviewKind.Glitch:
                DrawGlitch(ctx, bounds, layout, origin, time);
                break;
            case EffectPreviewKind.NeonBurst:
            {
                var pop = p < 0.25 ? EaseOutBack(p / 0.25, 3) : 1;
                var s = 0.6 + 0.4 * pop;
                var pulse = 0.5 + 0.5 * Math.Sin(time * 6);
                var glow = Lerp(Color.FromRgb(0xFF, 0x2B, 0xD6), Color.FromRgb(0x2B, 0xE4, 0xFF), pulse);
                var center = new Point(origin.X + layout.Width / 2, origin.Y + layout.Height / 2);
                using (ctx.PushTransform(ScaleAbout(center, s, s)))
                {
                    DrawText(ctx, layout, origin, g => GlyphStyle.Default(new SolidColorBrush(Color.FromRgb(0xFF, 0xF0, 0xFF))) with { Glow = glow });
                }
                break;
            }
            case EffectPreviewKind.AudioPulse:
            {
                var beat = Math.Pow(Math.Max(0, Math.Sin(time * Math.PI * 2 * 1.4)), 4);
                var s = 1 + 0.12 * beat;
                var center = new Point(origin.X + layout.Width / 2, origin.Y + layout.Height / 2);
                using (ctx.PushTransform(ScaleAbout(center, s, s)))
                {
                    DrawText(ctx, layout, origin, g => GlyphStyle.Default(Brushes.White) with
                    {
                        Glow = beat > 0.1 ? Color.FromArgb((byte)(255 * beat), 0x30, 0xE0, 0xFF) : null,
                    });
                }
                break;
            }
            case EffectPreviewKind.Matrix:
            {
                DrawMatrixRain(ctx, bounds, time, layout.FontSize * 0.7);
                var visible = Clamp01(p / 0.6) * layout.VisibleCount;
                var green = new SolidColorBrush(Color.FromRgb(0xC8, 0xFF, 0xD0));
                DrawText(ctx, layout, origin, g => GlyphStyle.Default(green) with
                {
                    Opacity = g.Index < visible ? 1 : 0,
                    Glow = Color.FromRgb(0x00, 0xFF, 0x46),
                });
                break;
            }
            case EffectPreviewKind.OldMovie:
            {
                var frame = (int)(time * 12);
                var flicker = 0.85 + 0.15 * Hash(frame, 3);
                var cream = new SolidColorBrush(Color.FromRgb(0xF4, 0xE6, 0xC8));
                DrawText(ctx, layout, origin, g => GlyphStyle.Default(cream) with { Opacity = flicker });
                DrawOldMovieOverlay(ctx, bounds, frame);
                break;
            }
            case EffectPreviewKind.CinematicTitle:
                DrawCinematicTitle(ctx, bounds, layout, p);
                break;
            case EffectPreviewKind.LowerThird:
                DrawLowerThird(ctx, bounds, layout, p, effect as AdvancedEffectLowerThird);
                break;
            case EffectPreviewKind.StarWarsScroll:
                DrawStarWars(ctx, bounds, layout, p);
                break;
            case EffectPreviewKind.CreditsScroll:
            {
                var travel = bounds.Height + layout.Height;
                var y = bounds.Bottom - p * travel;
                DrawText(ctx, layout, new Point(origin.X, y), g => GlyphStyle.Default(Brushes.White));
                break;
            }
            default:
                // Scenes: the particles are part of the background, the text stays put
                DrawText(ctx, layout, origin, g => GlyphStyle.Default(Brushes.White));
                DrawSceneForeground(ctx, bounds, kind, time, effect);
                break;
        }
    }

    private static void DrawBackground(DrawingContext ctx, Rect bounds, EffectPreviewKind kind, double time)
    {
        (Color Top, Color Bottom) colors = kind switch
        {
            EffectPreviewKind.Matrix => (Color.FromRgb(0x01, 0x0B, 0x04), Color.FromRgb(0x00, 0x14, 0x06)),
            EffectPreviewKind.OldMovie => (Color.FromRgb(0x6B, 0x4E, 0x31), Color.FromRgb(0x2E, 0x20, 0x12)),
            EffectPreviewKind.Starfield or EffectPreviewKind.StarWarsScroll => (Color.FromRgb(0x02, 0x03, 0x0A), Color.FromRgb(0x08, 0x0A, 0x1C)),
            EffectPreviewKind.Snow => (Color.FromRgb(0x1D, 0x2B, 0x45), Color.FromRgb(0x3E, 0x52, 0x70)),
            EffectPreviewKind.Rain => (Color.FromRgb(0x14, 0x1C, 0x26), Color.FromRgb(0x27, 0x33, 0x40)),
            EffectPreviewKind.Fireflies => (Color.FromRgb(0x08, 0x14, 0x12), Color.FromRgb(0x14, 0x2A, 0x1E)),
            EffectPreviewKind.Hearts => (Color.FromRgb(0x3A, 0x10, 0x2A), Color.FromRgb(0x6A, 0x1F, 0x45)),
            EffectPreviewKind.Confetti => (Color.FromRgb(0x1C, 0x14, 0x3A), Color.FromRgb(0x38, 0x22, 0x5E)),
            _ => (Color.FromRgb(0x24, 0x2C, 0x48), Color.FromRgb(0x0D, 0x10, 0x1C)),
        };

        ctx.FillRectangle(new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(0.3, 1, RelativeUnit.Relative),
            GradientStops =
            {
                new GradientStop(colors.Top, 0),
                new GradientStop(colors.Bottom, 1),
            },
        }, bounds);

        switch (kind)
        {
            case EffectPreviewKind.Starfield:
                DrawStarfield(ctx, bounds, time);
                break;
            case EffectPreviewKind.StarWarsScroll:
                DrawStaticStars(ctx, bounds, 60);
                break;
            case EffectPreviewKind.Rain:
                DrawRain(ctx, bounds, time);
                break;
            default:
                if (kind is not (EffectPreviewKind.Matrix or EffectPreviewKind.OldMovie or EffectPreviewKind.Snow or
                    EffectPreviewKind.Fireflies or EffectPreviewKind.Hearts or EffectPreviewKind.Confetti))
                {
                    // a soft "scene" - a sun glow and a horizon - so it reads as video, not as a flat card
                    var glow = new RadialGradientBrush
                    {
                        Center = new RelativePoint(0.75, 0.3, RelativeUnit.Relative),
                        GradientOrigin = new RelativePoint(0.75, 0.3, RelativeUnit.Relative),
                        RadiusX = new RelativeScalar(0.6, RelativeUnit.Relative),
                        RadiusY = new RelativeScalar(0.6, RelativeUnit.Relative),
                        GradientStops =
                        {
                            new GradientStop(Color.FromArgb(0x55, 0xFF, 0x9A, 0x6B), 0),
                            new GradientStop(Color.FromArgb(0x00, 0xFF, 0x9A, 0x6B), 1),
                        },
                    };
                    ctx.FillRectangle(glow, bounds);
                    var hill = new StreamGeometry();
                    using (var g = hill.Open())
                    {
                        g.BeginFigure(new Point(bounds.Left, bounds.Bottom), true);
                        g.LineTo(new Point(bounds.Left, bounds.Top + bounds.Height * 0.72));
                        g.CubicBezierTo(
                            new Point(bounds.Left + bounds.Width * 0.3, bounds.Top + bounds.Height * 0.6),
                            new Point(bounds.Left + bounds.Width * 0.6, bounds.Top + bounds.Height * 0.82),
                            new Point(bounds.Right, bounds.Top + bounds.Height * 0.66));
                        g.LineTo(new Point(bounds.Right, bounds.Bottom));
                        g.EndFigure(true);
                    }

                    ctx.DrawGeometry(new SolidColorBrush(Color.FromArgb(0x70, 0x05, 0x07, 0x10)), null, hill);
                }
                break;
        }
    }

    private static void DrawSceneForeground(DrawingContext ctx, Rect bounds, EffectPreviewKind kind, double time, IAdvancedEffectDisplay? effect)
    {
        var area = bounds.Width * bounds.Height;
        var density = Math.Clamp(area / (128.0 * 72.0), 1, 12);
        switch (kind)
        {
            case EffectPreviewKind.Snow:
            {
                var count = (int)(30 * Math.Sqrt(density) * Math.Clamp((effect as AdvancedEffectSnow)?.FlakeCount / 200.0 ?? 1, 0.3, 4));
                for (var i = 0; i < count; i++)
                {
                    var depth = 0.4 + 0.6 * Hash(i, 2);
                    var y = Wrap(Hash(i, 1) + time * 0.08 * depth, -0.05, 1.05);
                    var x = Hash(i, 0) + Math.Sin(time * 1.2 + i) * 0.02;
                    var r = Math.Max(0.8, bounds.Height * 0.012 * depth);
                    ctx.DrawEllipse(new SolidColorBrush(Colors.White, 0.35 + 0.55 * depth), null,
                        new Point(bounds.X + x * bounds.Width, bounds.Y + y * bounds.Height), r, r);
                }
                break;
            }
            case EffectPreviewKind.Fireflies:
            {
                var count = (int)(12 * Math.Sqrt(density));
                for (var i = 0; i < count; i++)
                {
                    var x = Hash(i, 0) + Math.Sin(time * 0.6 + i * 1.7) * 0.05;
                    var y = Hash(i, 1) + Math.Cos(time * 0.5 + i * 2.3) * 0.05;
                    var pulse = 0.35 + 0.65 * Math.Pow(Math.Abs(Math.Sin(time * 1.5 + i)), 2);
                    var center = new Point(bounds.X + Wrap(x, 0, 1) * bounds.Width, bounds.Y + Wrap(y, 0, 1) * bounds.Height * 0.8);
                    var r = bounds.Height * 0.05;
                    var brush = new RadialGradientBrush
                    {
                        GradientStops =
                        {
                            new GradientStop(Color.FromArgb((byte)(230 * pulse), 0xFF, 0xE0, 0x6A), 0),
                            new GradientStop(Color.FromArgb((byte)(90 * pulse), 0xFF, 0xB0, 0x20), 0.25),
                            new GradientStop(Color.FromArgb(0, 0xFF, 0xA0, 0x10), 1),
                        },
                    };
                    ctx.DrawEllipse(brush, null, center, r, r);
                }
                break;
            }
            case EffectPreviewKind.Confetti:
                DrawConfetti(ctx, bounds, time, density);
                break;
            case EffectPreviewKind.Hearts:
                DrawHearts(ctx, bounds, time, density);
                break;
        }
    }

    private static void DrawStarfield(DrawingContext ctx, Rect bounds, double time)
    {
        var center = bounds.Center;
        var maxR = Math.Sqrt(bounds.Width * bounds.Width + bounds.Height * bounds.Height) / 2;
        var count = (int)Math.Clamp(bounds.Width * bounds.Height / 150, 40, 400);
        for (var i = 0; i < count; i++)
        {
            var angle = Hash(i, 0) * Math.PI * 2;
            var phase = Wrap(Hash(i, 1) + time * 0.25, 0, 1);
            var d = phase * phase * maxR;
            var d0 = Math.Max(0, d - maxR * 0.06 * phase);
            var dir = new Vector(Math.Cos(angle), Math.Sin(angle));
            var pen = new Pen(new SolidColorBrush(Colors.White, Math.Min(1, phase * 1.6)), Math.Max(0.6, phase * 2.2), lineCap: PenLineCap.Round);
            ctx.DrawLine(pen, center + dir * d0, center + dir * d);
        }
    }

    private static void DrawStaticStars(DrawingContext ctx, Rect bounds, int count)
    {
        for (var i = 0; i < count; i++)
        {
            var r = 0.5 + Hash(i, 2) * 0.9;
            ctx.DrawEllipse(new SolidColorBrush(Colors.White, 0.3 + 0.6 * Hash(i, 3)), null,
                new Point(bounds.X + Hash(i, 0) * bounds.Width, bounds.Y + Hash(i, 1) * bounds.Height), r, r);
        }
    }

    private static void DrawRain(DrawingContext ctx, Rect bounds, double time)
    {
        var count = (int)Math.Clamp(bounds.Width * bounds.Height / 300, 30, 300);
        for (var i = 0; i < count; i++)
        {
            var depth = 0.3 + 0.7 * Hash(i, 2);
            var y = Wrap(Hash(i, 1) + time * 1.1 * depth, -0.1, 1.1);
            var x = Hash(i, 0);
            var length = bounds.Height * 0.08 * depth;
            var start = new Point(bounds.X + x * bounds.Width, bounds.Y + y * bounds.Height);
            var pen = new Pen(new SolidColorBrush(Color.FromRgb(0xAE, 0xC8, 0xE8), 0.25 + 0.45 * depth), Math.Max(0.6, depth * 1.4));
            ctx.DrawLine(pen, start, start + new Vector(-length * 0.15, length));
        }
    }

    private static void DrawMatrixRain(DrawingContext ctx, Rect bounds, double time, double size)
    {
        size = Math.Max(5, size);
        var columns = (int)(bounds.Width / (size * 0.9));
        var trail = 7;
        for (var c = 0; c < columns; c++)
        {
            var speed = 0.25 + 0.35 * Hash(c, 1);
            var head = Wrap(Hash(c, 0) + time * speed, 0, 1.4) * bounds.Height;
            for (var k = 0; k < trail; k++)
            {
                var y = head - k * size * 1.05;
                if (y < bounds.Top - size || y > bounds.Bottom)
                {
                    continue;
                }

                var ch = MatrixChars[(int)(Hash(c * 31 + k, (int)(time * 6)) * MatrixChars.Length) % MatrixChars.Length].ToString();
                var glyph = GetGlyph(ch, size);
                if (glyph.Geometry == null)
                {
                    continue;
                }

                var color = k == 0 ? Color.FromRgb(0xD8, 0xFF, 0xE0) : Color.FromRgb(0x00, 0xE0, 0x41);
                using (ctx.PushTransform(Matrix.CreateTranslation(bounds.X + c * size * 0.9 + size * 0.2, y)))
                using (ctx.PushOpacity(0.85 * (1 - k / (double)trail)))
                {
                    ctx.DrawGeometry(new SolidColorBrush(color), null, glyph.Geometry);
                }
            }
        }
    }

    private static void DrawConfetti(DrawingContext ctx, Rect bounds, double time, double density)
    {
        var t = time % CycleSeconds;
        var count = (int)(36 * Math.Sqrt(density));
        Color[] palette =
        [
            Color.FromRgb(0xFF, 0x4D, 0x6D), Color.FromRgb(0xFF, 0xC4, 0x3D), Color.FromRgb(0x3D, 0xD6, 0xFF),
            Color.FromRgb(0x7C, 0xFF, 0x6B), Color.FromRgb(0xB3, 0x6B, 0xFF),
        ];
        var gravity = bounds.Height * 0.9;
        for (var i = 0; i < count; i++)
        {
            var fromLeft = i % 2 == 0;
            var angle = (fromLeft ? -70 : -110) + (Hash(i, 0) - 0.5) * 50;
            var speed = bounds.Height * (0.9 + Hash(i, 1) * 0.8);
            var rad = angle * Math.PI / 180;
            var x = (fromLeft ? bounds.Left : bounds.Right) + Math.Cos(rad) * speed * t;
            var y = bounds.Bottom + Math.Sin(rad) * speed * t + 0.5 * gravity * t * t;
            if (y > bounds.Bottom + 10)
            {
                continue;
            }

            var w = Math.Max(2, bounds.Height * 0.035);
            var spin = t * (4 + Hash(i, 2) * 6) + i;
            var m = Matrix.CreateTranslation(-w / 2, -w / 4) *
                    Matrix.CreateScale(1, Math.Abs(Math.Cos(spin * 1.3)) + 0.15) *
                    Matrix.CreateRotation(spin) *
                    Matrix.CreateTranslation(x, y);
            using (ctx.PushTransform(m))
            {
                ctx.FillRectangle(new SolidColorBrush(palette[i % palette.Length]), new Rect(0, 0, w, w / 2));
            }
        }
    }

    private static void DrawHearts(DrawingContext ctx, Rect bounds, double time, double density)
    {
        var heart = GetHeartGeometry();
        var count = (int)(14 * Math.Sqrt(density));
        for (var i = 0; i < count; i++)
        {
            var depth = 0.5 + 0.5 * Hash(i, 2);
            var y = Wrap(Hash(i, 1) + time * 0.09 * depth, -0.1, 1.1);
            var x = Hash(i, 0) + Math.Sin(time + i) * 0.03;
            var size = bounds.Height * 0.08 * depth;
            var color = i % 3 == 0 ? Color.FromRgb(0xFF, 0x8F, 0xB8) : i % 3 == 1 ? Color.FromRgb(0xFF, 0x3D, 0x6E) : Color.FromRgb(0xE0, 0x1E, 0x55);
            var m = Matrix.CreateTranslation(-0.5, -0.5) *
                    Matrix.CreateRotation(Math.Sin(time * 1.5 + i) * 0.4) *
                    Matrix.CreateScale(size, size) *
                    Matrix.CreateTranslation(bounds.X + x * bounds.Width, bounds.Y + y * bounds.Height);
            using (ctx.PushTransform(m))
            {
                ctx.DrawGeometry(new SolidColorBrush(color, 0.85), null, heart);
            }
        }
    }

    private static Geometry GetHeartGeometry()
    {
        if (_heartGeometry != null)
        {
            return _heartGeometry;
        }

        var g = new StreamGeometry();
        using (var c = g.Open())
        {
            c.BeginFigure(new Point(0.5, 0.95), true);
            c.CubicBezierTo(new Point(0.1, 0.65), new Point(-0.05, 0.35), new Point(0.2, 0.15));
            c.CubicBezierTo(new Point(0.35, 0.03), new Point(0.48, 0.12), new Point(0.5, 0.25));
            c.CubicBezierTo(new Point(0.52, 0.12), new Point(0.65, 0.03), new Point(0.8, 0.15));
            c.CubicBezierTo(new Point(1.05, 0.35), new Point(0.9, 0.65), new Point(0.5, 0.95));
            c.EndFigure(true);
        }

        _heartGeometry = g;
        return g;
    }

    private static void DrawOldMovieOverlay(DrawingContext ctx, Rect bounds, int frame)
    {
        // scratches + dust, then a vignette
        for (var i = 0; i < 2; i++)
        {
            if (Hash(frame, 10 + i) < 0.55)
            {
                var x = bounds.X + Hash(frame, 20 + i) * bounds.Width;
                ctx.DrawLine(new Pen(new SolidColorBrush(Color.FromRgb(0xF0, 0xE6, 0xD2), 0.45), 1), new Point(x, bounds.Top), new Point(x + 2, bounds.Bottom));
            }
        }

        for (var i = 0; i < 6; i++)
        {
            var r = Math.Max(0.7, Hash(frame * 7 + i, 4) * bounds.Height * 0.012);
            ctx.DrawEllipse(new SolidColorBrush(Colors.Black, 0.6), null,
                new Point(bounds.X + Hash(frame * 7 + i, 5) * bounds.Width, bounds.Y + Hash(frame * 7 + i, 6) * bounds.Height), r, r);
        }

        ctx.FillRectangle(new RadialGradientBrush
        {
            RadiusX = new RelativeScalar(0.75, RelativeUnit.Relative),
            RadiusY = new RelativeScalar(0.75, RelativeUnit.Relative),
            GradientStops =
            {
                new GradientStop(Color.FromArgb(0, 0, 0, 0), 0.55),
                new GradientStop(Color.FromArgb(0xD0, 0x10, 0x08, 0x02), 1),
            },
        }, bounds);
    }

    private static void DrawTvClose(DrawingContext ctx, Rect bounds, PreviewTextLayout layout, Point origin, double p)
    {
        DrawText(ctx, layout, origin, g => GlyphStyle.Default(Brushes.White));
        if (p > 0.62)
        {
            FillRect(ctx, bounds, Colors.Black, 1);
            return;
        }

        var q = EaseInCubic(Clamp01(p / 0.6));
        FillRect(ctx, bounds, Colors.White, q * 0.9);
        var bar = q * bounds.Height / 2;
        ctx.FillRectangle(Brushes.Black, new Rect(bounds.X, bounds.Y, bounds.Width, bar));
        ctx.FillRectangle(Brushes.Black, new Rect(bounds.X, bounds.Bottom - bar, bounds.Width, bar));
    }

    private static void DrawWordPop(DrawingContext ctx, PreviewTextLayout layout, Point origin, double p, AdvancedEffectFancyKaraoke? effect)
    {
        var activeColor = effect?.ActiveWordColor ?? Colors.White;
        var inactiveColor = effect?.InactiveWordColor ?? Color.FromArgb(0xEE, 0xEE, 0xEE, 0xEE);
        var glow = effect == null || effect.ApplyGlow ? effect?.GlowColor ?? Color.FromRgb(0xFF, 0xDD, 0x00) : (Color?)null;
        var active = Math.Min(layout.WordCount - 1, (int)(Clamp01(p / 0.9) * layout.WordCount));
        var local = Clamp01(p / 0.9 * layout.WordCount - active);
        var pop = 1 + 0.18 * EaseOutBack(Clamp01(local * 4), 2.5) - 0.06 * Clamp01(local * 4 - 1);
        var activeBrush = new SolidColorBrush(activeColor);
        var inactiveBrush = new SolidColorBrush(Color.FromRgb(inactiveColor.R, inactiveColor.G, inactiveColor.B));
        var inactiveOpacity = Math.Max(0.35, inactiveColor.A / 255.0 * 0.7);
        DrawText(ctx, layout, origin, g => g.Word == active
            ? GlyphStyle.Default(activeBrush) with { ScaleX = pop, ScaleY = pop, Glow = glow }
            : GlyphStyle.Default(inactiveBrush) with { Opacity = inactiveOpacity });
    }

    private static (int Active, double SecondsIntoWord) GetActiveWord(PreviewTextLayout layout, double p)
    {
        var progress = Clamp01(p / 0.9) * layout.WordCount;
        var active = Math.Min(layout.WordCount - 1, (int)progress);
        return (active, (progress - active) * CycleSeconds * 0.9 / layout.WordCount);
    }

    private static (double Left, double Right, double Top, double Height) GetWordBounds(PreviewTextLayout layout, int word)
    {
        double left = double.MaxValue, right = double.MinValue, top = 0, height = 0;
        foreach (var g in layout.Glyphs)
        {
            if (g.Word == word && !g.IsSpace)
            {
                left = Math.Min(left, g.X);
                right = Math.Max(right, g.X + g.Width);
                top = g.Y;
                height = g.Height;
            }
        }

        return left > right ? (0, 0, 0, 0) : (left, right, top, height);
    }

    /// <summary>
    /// Horizontal shift for a glyph when word <paramref name="active"/> is scaled by <paramref name="scale"/>:
    /// the word grows around its center and pushes the rest of its line aside, like libass lays it out.
    /// </summary>
    private static double ScaledWordShift(PreviewGlyph g, int active, int activeLine, double wordLeft, double wordRight, double scale)
    {
        if (g.Line != activeLine)
        {
            return 0;
        }

        var extra = (scale - 1) * (wordRight - wordLeft) / 2;
        if (g.Word < active)
        {
            return -extra;
        }

        return g.Word > active ? extra : (g.X + g.Width / 2 - (wordLeft + wordRight) / 2) * (scale - 1);
    }

    private static int GetWordLine(PreviewTextLayout layout, int word)
    {
        foreach (var g in layout.Glyphs)
        {
            if (g.Word == word && !g.IsSpace)
            {
                return g.Line;
            }
        }

        return 0;
    }

    // a quick overshoot when a word becomes active: 1.12 -> 1 over ~0.12 s
    private static double WordPopScale(double secondsIntoWord, double from = 1.12) =>
        secondsIntoWord >= 0.12 ? 1 : from + (1 - from) * EaseOutCubic(secondsIntoWord / 0.12);

    private static void DrawHighlightBox(DrawingContext ctx, PreviewTextLayout layout, Point origin, double p, AdvancedEffectHighlightBox? effect)
    {
        var boxColor = effect?.BoxColor ?? Color.FromRgb(0x7C, 0x3A, 0xED);
        var activeBrush = new SolidColorBrush(effect?.ActiveTextColor ?? Colors.White);
        var (active, seconds) = GetActiveWord(layout, p);
        var (left, right, top, height) = GetWordBounds(layout, active);
        var s = WordPopScale(seconds, 1.1);
        var center = new Point(origin.X + (left + right) / 2, origin.Y + top + height / 2);
        var pad = layout.FontSize * 0.16;
        var line = GetWordLine(layout, active);
        using (ctx.PushTransform(ScaleAbout(center, s, s)))
        {
            var box = new Rect(origin.X + left - pad, origin.Y + top + height * 0.08, right - left + pad * 2, height * 0.92);
            ctx.DrawRectangle(new SolidColorBrush(boxColor), null, box, layout.FontSize * 0.06, layout.FontSize * 0.06);
        }

        DrawText(ctx, layout, origin, g => g.Word == active
            ? GlyphStyle.Default(activeBrush) with { ScaleX = s, ScaleY = s, Dx = ScaledWordShift(g, active, line, left, right, s) }
            : GlyphStyle.Default(Brushes.White) with { Dx = ScaledWordShift(g, active, line, left, right, s) });
    }

    private static void DrawSingleWordPop(DrawingContext ctx, Rect bounds, string text, double fontSize, double p, AdvancedEffectSingleWordPop? effect)
    {
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0)
        {
            return;
        }

        var progress = Clamp01(p / 0.9) * words.Length;
        var active = Math.Min(words.Length - 1, (int)progress);
        var seconds = (progress - active) * CycleSeconds * 0.9 / words.Length;
        var size = Math.Clamp((effect?.TextSizePercent ?? 150) / 100.0, 0.5, 2.2);
        var layout = GetLayout(words[active], fontSize * size, bounds.Width * 0.9, 1);
        var origin = new Point(bounds.X + (bounds.Width - layout.Width) / 2, bounds.Bottom - layout.Height - bounds.Height * 0.1);
        var center = new Point(origin.X + layout.Width / 2, origin.Y + layout.Height / 2);
        var s = seconds < 0.09
            ? 0.55 + (1.12 - 0.55) * EaseOutCubic(seconds / 0.09)
            : WordPopScale(seconds - 0.09);
        var fill = new SolidColorBrush(effect?.TextColor ?? Colors.White);
        using (ctx.PushTransform(ScaleAbout(center, s, s)))
        {
            DrawText(ctx, layout, origin, g => GlyphStyle.Default(fill));
        }
    }

    private static void DrawBoldCaptions(DrawingContext ctx, PreviewTextLayout layout, Point origin, double p, AdvancedEffectBoldCaptions? effect)
    {
        var highlight = new SolidColorBrush(effect?.HighlightColor ?? Color.FromRgb(0xFF, 0xD6, 0x0A));
        var (active, seconds) = GetActiveWord(layout, p);
        var (left, right, _, _) = GetWordBounds(layout, active);
        var line = GetWordLine(layout, active);
        var s = 1.15 * WordPopScale(seconds, 1.09);
        DrawText(ctx, layout, origin, g => g.Word == active
            ? GlyphStyle.Default(highlight) with { ScaleX = s, ScaleY = s, Dx = ScaledWordShift(g, active, line, left, right, s) }
            : GlyphStyle.Default(Brushes.White) with { Dx = ScaledWordShift(g, active, line, left, right, s) }, outlineScale: 1.6);
    }

    private static void DrawSmoothFill(DrawingContext ctx, PreviewTextLayout layout, Point origin, double p, AdvancedEffectSmoothFill? effect)
    {
        var fill = new SolidColorBrush(effect?.FillColor ?? Color.FromRgb(0xFF, 0xD6, 0x0A));
        var baseBrush = new SolidColorBrush(effect?.BaseColor ?? Colors.White);
        var progress = Clamp01(p / 0.9) * layout.WordCount;
        var active = Math.Min(layout.WordCount - 1, (int)progress);
        var fraction = progress - active;
        DrawText(ctx, layout, origin, g => GlyphStyle.Default(g.Word < active ? fill : baseBrush));

        // the active word: the fill sweeps across it left to right
        var (left, right, top, height) = GetWordBounds(layout, active);
        using (ctx.PushClip(new Rect(origin.X + left - layout.FontSize * 0.2, origin.Y + top - layout.FontSize * 0.2, (right - left) * fraction + layout.FontSize * 0.2, height + layout.FontSize * 0.4)))
        {
            DrawText(ctx, layout, origin, g => GlyphStyle.Default(fill) with { Opacity = g.Word == active ? 1 : 0 });
        }
    }

    private static void DrawBlurInWords(DrawingContext ctx, PreviewTextLayout layout, Point origin, double p, AdvancedEffectBlurInWords? effect)
    {
        var rise = effect?.Rise ?? true;
        var seconds = p * CycleSeconds;
        var perWord = CycleSeconds * 0.8 / layout.WordCount;
        DrawText(ctx, layout, origin, g =>
        {
            var local = seconds - g.Word * perWord;
            if (local <= 0)
            {
                return GlyphStyle.Default(Brushes.White) with { Opacity = 0 };
            }

            var reveal = Clamp01(local / 0.25);
            var sy = !rise ? 1 : local < 0.25 ? 0.4 + (1.12 - 0.4) * EaseOutCubic(reveal) : WordPopScale(local - 0.25);
            return GlyphStyle.Default(Brushes.White) with
            {
                Opacity = reveal,
                ScaleY = sy,
                Dy = (1 - sy) * g.Height * 0.25, // grow from the baseline, not the middle
                Glow = reveal < 1 ? Color.FromArgb((byte)(200 * (1 - reveal)), 0xFF, 0xFF, 0xFF) : null,
            };
        });
    }

    private static void DrawRollingCaptions(DrawingContext ctx, Rect bounds, string text, double fontSize, double p)
    {
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var half = Math.Max(1, (words.Length + 1) / 2);
        var first = GetLayout(string.Join(' ', words.Take(half)), fontSize, bounds.Width * 0.9, 1);
        var second = GetLayout(words.Length > half ? string.Join(' ', words.Skip(half)) : string.Join(' ', words), fontSize, bounds.Width * 0.9, 1);
        var bottom = bounds.Bottom - bounds.Height * 0.1;
        var slide = first.LineHeight * 0.8;

        // first line slides in, then the second slides in and pushes it up
        var firstIn = EaseOutCubic(Clamp01(p / 0.08));
        var secondIn = EaseOutCubic(Clamp01((p - 0.45) / 0.08));
        var fadeOut = Clamp01((1 - p) / 0.06);
        var firstY = bottom - first.Height + (1 - firstIn) * slide - secondIn * second.LineHeight;
        var firstOpacity = Math.Min(firstIn, 1 - 0.45 * secondIn) * fadeOut;
        DrawText(ctx, first, new Point(bounds.X + (bounds.Width - first.Width) / 2, firstY), g => GlyphStyle.Default(Brushes.White) with { Opacity = firstOpacity });
        if (p >= 0.45)
        {
            var secondY = bottom - second.Height + (1 - secondIn) * slide;
            DrawText(ctx, second, new Point(bounds.X + (bounds.Width - second.Width) / 2, secondY), g => GlyphStyle.Default(Brushes.White) with { Opacity = secondIn * fadeOut });
        }
    }

    private static void DrawGlitch(DrawingContext ctx, Rect bounds, PreviewTextLayout layout, Point origin, double time)
    {
        var frame = (int)(time * 12);
        var glitching = Hash(frame, 1) < 0.4;
        var offset = glitching ? layout.FontSize * (0.06 + 0.1 * Hash(frame, 2)) : layout.FontSize * 0.02;
        var red = new SolidColorBrush(Color.FromRgb(0xFF, 0x20, 0x50));
        var cyan = new SolidColorBrush(Color.FromRgb(0x20, 0xF0, 0xFF));
        DrawText(ctx, layout, origin, g => GlyphStyle.Default(red) with { Dx = -offset, Opacity = 0.75 }, outline: false);
        DrawText(ctx, layout, origin, g => GlyphStyle.Default(cyan) with { Dx = offset, Opacity = 0.75 }, outline: false);
        DrawText(ctx, layout, origin, g => GlyphStyle.Default(Brushes.White) with
        {
            Dx = glitching && Hash(frame * 13 + g.Line, 3) < 0.5 ? (Hash(frame, g.Line + 5) - 0.5) * layout.FontSize * 0.5 : 0,
        });

        if (glitching && Hash(frame, 4) < 0.4)
        {
            var y = bounds.Y + Hash(frame, 6) * bounds.Height;
            FillRect(ctx, new Rect(bounds.X, y, bounds.Width, bounds.Height * 0.04), Colors.White, 0.5);
        }

        if (Hash(frame, 7) < 0.06)
        {
            FillRect(ctx, bounds, Colors.White, 0.35);
        }
    }

    private static void DrawCinematicTitle(DrawingContext ctx, Rect bounds, PreviewTextLayout layout, double p)
    {
        var bar = bounds.Height * 0.12;
        var origin = new Point(bounds.X + (bounds.Width - layout.Width) / 2, bounds.Y + (bounds.Height - layout.Height) / 2);
        var e = EaseOutCubic(Clamp01(p / 0.65));
        var opacity = Math.Min(Clamp01(p / 0.35), Clamp01((1 - p) / 0.12));
        var tracking = layout.FontSize * 0.45 * (1 - e);
        var gold = new SolidColorBrush(Color.FromRgb(0xF6, 0xEB, 0xD0));
        DrawText(ctx, layout, origin, g => GlyphStyle.Default(gold) with
        {
            Dx = (g.Index - (layout.VisibleCount - 1) / 2.0) * tracking,
            Opacity = opacity,
            Glow = e < 0.95 ? Color.FromArgb((byte)(160 * (1 - e)), 0xFF, 0xF4, 0xDC) : null,
        }, outline: false);
        ctx.FillRectangle(Brushes.Black, new Rect(bounds.X, bounds.Y, bounds.Width, bar));
        ctx.FillRectangle(Brushes.Black, new Rect(bounds.X, bounds.Bottom - bar, bounds.Width, bar));
    }

    private static void DrawLowerThird(DrawingContext ctx, Rect bounds, PreviewTextLayout layout, double p, AdvancedEffectLowerThird? effect)
    {
        var accent = effect?.AccentColor ?? Color.FromRgb(0x00, 0xB4, 0xFF);
        var x = bounds.X + bounds.Width * 0.06;
        var y = bounds.Bottom - bounds.Height * 0.14 - layout.Height;
        var barIn = EaseOutCubic(Clamp01(p / 0.18));
        var panelIn = EaseOutCubic(Clamp01((p - 0.1) / 0.25));
        var textIn = Clamp01((p - 0.2) / 0.2);
        var outFade = Clamp01((1 - p) / 0.1);
        var pad = layout.FontSize * 0.4;
        var barWidth = Math.Max(2, layout.FontSize * 0.22);

        using (ctx.PushOpacity(outFade))
        {
            var panelWidth = (layout.Width + pad * 2) * panelIn;
            ctx.FillRectangle(new SolidColorBrush(Color.FromArgb(0xB8, 0x0C, 0x10, 0x18)), new Rect(x + barWidth, y - pad, panelWidth, layout.Height + pad * 2));
            ctx.FillRectangle(new SolidColorBrush(accent), new Rect(x - (1 - barIn) * bounds.Width * 0.2, y - pad, barWidth, layout.Height + pad * 2));
            using (ctx.PushClip(new Rect(x + barWidth, bounds.Y, panelWidth, bounds.Height)))
            {
                DrawText(ctx, layout, new Point(x + barWidth + pad, y), g => GlyphStyle.Default(Brushes.White) with
                {
                    Opacity = textIn,
                    Dx = (1 - textIn) * -layout.FontSize,
                }, outline: false);
            }
        }
    }

    private static void DrawStarWars(DrawingContext ctx, Rect bounds, PreviewTextLayout layout, double p)
    {
        var yellow = new SolidColorBrush(Color.FromRgb(0xFF, 0xE8, 0x1F));
        var centerX = bounds.X + bounds.Width / 2;
        var top = bounds.Bottom - p * (bounds.Height + layout.Height) * 1.1;
        var textOrigin = new Point(centerX - layout.Width / 2, top);
        DrawText(ctx, layout, textOrigin, g =>
        {
            var y = top + g.Y + g.Height / 2;
            var depth = Clamp01((y - bounds.Y) / bounds.Height); // 1 near bottom, 0 at top
            var s = 0.25 + 0.85 * depth;
            var glyphCenterX = textOrigin.X + g.X + g.Width / 2;
            return GlyphStyle.Default(yellow) with
            {
                ScaleX = s,
                ScaleY = s,
                Dx = (glyphCenterX - centerX) * (s - 1),
                Opacity = Clamp01(depth * 2.5),
            };
        }, outline: false);
    }

    private static void DrawText(DrawingContext ctx, PreviewTextLayout layout, Point origin, Func<PreviewGlyph, GlyphStyle> styleFor, bool outline = true, double outlineScale = 1)
    {
        var outlineWidth = Math.Max(1, layout.FontSize * 0.12 * outlineScale);
        foreach (var glyph in layout.Glyphs)
        {
            if (glyph.IsSpace)
            {
                continue;
            }

            var style = styleFor(glyph);
            if (style.Opacity <= 0.01 || Math.Abs(style.ScaleX) < 0.01 || Math.Abs(style.ScaleY) < 0.01)
            {
                continue;
            }

            var geometry = glyph.Geometry;
            var width = glyph.Width;
            if (style.Replacement != null)
            {
                var replacement = GetGlyph(style.Replacement, layout.FontSize);
                geometry = replacement.Geometry;
                width = replacement.Width;
            }

            if (geometry == null)
            {
                continue;
            }

            var cx = glyph.Width / 2;
            var cy = glyph.Height / 2;
            var m = Matrix.CreateTranslation(-cx + (glyph.Width - width) / 2, -cy) *
                    Matrix.CreateScale(style.ScaleX, style.ScaleY) *
                    Matrix.CreateTranslation(origin.X + glyph.X + cx + style.Dx, origin.Y + glyph.Y + cy + style.Dy);

            using (ctx.PushTransform(m))
            using (ctx.PushOpacity(Math.Min(1, style.Opacity)))
            {
                if (style.Glow is { } glow)
                {
                    for (var k = 0; k < 3; k++)
                    {
                        var pen = new Pen(new SolidColorBrush(glow, glow.A / 255.0 * 0.16), layout.FontSize * (0.28 - k * 0.08), lineJoin: PenLineJoin.Round);
                        ctx.DrawGeometry(null, pen, geometry);
                    }
                }

                if (outline)
                {
                    ctx.DrawGeometry(null, new Pen(OutlineBrush, outlineWidth, lineJoin: PenLineJoin.Round), geometry);
                }

                ctx.DrawGeometry(style.Fill, null, geometry);
            }
        }
    }

    private static PreviewTextLayout GetLayout(string text, double fontSize, double maxWidth, int maxLines)
    {
        fontSize = Math.Round(fontSize * 2) / 2;
        maxWidth = Math.Round(maxWidth);
        var key = (text, fontSize, maxWidth, maxLines);
        if (LayoutCache.TryGetValue(key, out var cached))
        {
            return cached;
        }

        if (LayoutCache.Count > 200)
        {
            LayoutCache.Clear();
        }

        var layout = BuildLayout(text, fontSize, maxWidth, maxLines);
        LayoutCache[key] = layout;
        return layout;
    }

    private static PreviewTextLayout BuildLayout(string text, double fontSize, double maxWidth, int maxLines)
    {
        var layout = new PreviewTextLayout { FontSize = fontSize };
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var spaceWidth = GetGlyph(" ", fontSize).Width;
        var lineHeight = GetGlyph("Hg", fontSize).Height;
        layout.LineHeight = lineHeight;

        // greedy word wrap into lines of (word, width)
        var lines = new List<List<(string Word, double Width)>>();
        var current = new List<(string Word, double Width)>();
        var currentWidth = 0.0;
        foreach (var word in words)
        {
            var w = MeasureWord(word, fontSize);
            var needed = current.Count == 0 ? w : currentWidth + spaceWidth + w;
            if (current.Count > 0 && needed > maxWidth)
            {
                lines.Add(current);
                current = new List<(string Word, double Width)>();
                needed = w;
            }

            current.Add((word, w));
            currentWidth = needed;
        }

        if (current.Count > 0)
        {
            lines.Add(current);
        }

        if (lines.Count > maxLines)
        {
            lines.RemoveRange(maxLines, lines.Count - maxLines);
            var last = lines[^1];
            last[^1] = (last[^1].Word + "…", MeasureWord(last[^1].Word + "…", fontSize));
        }

        var index = 0;
        var wordIndex = 0;
        var maxLineWidth = 0.0;
        var lineWidths = new List<double>();
        foreach (var line in lines)
        {
            var width = 0.0;
            for (var i = 0; i < line.Count; i++)
            {
                width += line[i].Width + (i > 0 ? spaceWidth : 0);
            }

            lineWidths.Add(width);
            maxLineWidth = Math.Max(maxLineWidth, width);
        }

        for (var lineIndex = 0; lineIndex < lines.Count; lineIndex++)
        {
            var line = lines[lineIndex];
            var x = (maxLineWidth - lineWidths[lineIndex]) / 2;
            var y = lineIndex * lineHeight;
            for (var w = 0; w < line.Count; w++)
            {
                if (w > 0)
                {
                    layout.Glyphs.Add(new PreviewGlyph { Text = " ", X = x, Y = y, Width = spaceWidth, Height = lineHeight, Index = -1, Word = wordIndex, IsSpace = true, Line = lineIndex });
                    x += spaceWidth;
                }

                var enumerator = StringInfo.GetTextElementEnumerator(line[w].Word);
                while (enumerator.MoveNext())
                {
                    var element = enumerator.GetTextElement();
                    var glyph = GetGlyph(element, fontSize);
                    layout.Glyphs.Add(new PreviewGlyph
                    {
                        Text = element,
                        Geometry = glyph.Geometry,
                        X = x,
                        Y = y,
                        Width = glyph.Width,
                        Height = lineHeight,
                        Index = index++,
                        Word = wordIndex,
                        Line = lineIndex,
                        WordInLine = w,
                        WordsInLine = line.Count,
                    });
                    x += glyph.Width;
                }

                wordIndex++;
            }
        }

        layout.WordCount = Math.Max(1, wordIndex);
        layout.VisibleCount = Math.Max(1, index);
        layout.Width = maxLineWidth;
        layout.Height = lines.Count * lineHeight;
        return layout;
    }

    private static double MeasureWord(string word, double fontSize)
    {
        var width = 0.0;
        var enumerator = StringInfo.GetTextElementEnumerator(word);
        while (enumerator.MoveNext())
        {
            width += GetGlyph(enumerator.GetTextElement(), fontSize).Width;
        }

        return width;
    }

    private static (Geometry? Geometry, double Width, double Height) GetGlyph(string text, double fontSize)
    {
        var key = (text, fontSize);
        if (GlyphCache.TryGetValue(key, out var cached))
        {
            return cached;
        }

        if (GlyphCache.Count > 4000)
        {
            GlyphCache.Clear();
        }

        var formatted = new FormattedText(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, BoldTypeface, fontSize, Brushes.White);
        var result = (string.IsNullOrWhiteSpace(text) ? null : formatted.BuildGeometry(new Point(0, 0)),
            formatted.WidthIncludingTrailingWhitespace, formatted.Height);
        GlyphCache[key] = result;
        return result;
    }

    private static void FillRect(DrawingContext ctx, Rect rect, Color color, double opacity)
    {
        if (opacity > 0.005)
        {
            ctx.FillRectangle(new SolidColorBrush(color, Math.Min(1, opacity)), rect);
        }
    }

    private static Matrix ScaleAbout(Point center, double sx, double sy) =>
        Matrix.CreateTranslation(-center.X, -center.Y) * Matrix.CreateScale(sx, sy) * Matrix.CreateTranslation(center.X, center.Y);

    private static double Hash(int i, int salt)
    {
        unchecked
        {
            var h = (uint)(i * 374761393 + salt * 668265263);
            h = (h ^ (h >> 13)) * 1274126177;
            h ^= h >> 16;
            return (h & 0xFFFFFF) / (double)0x1000000;
        }
    }

    private static double Wrap(double value, double min, double max)
    {
        var range = max - min;
        var v = (value - min) % range;
        return (v < 0 ? v + range : v) + min;
    }

    private static double Clamp01(double v) => v < 0 ? 0 : v > 1 ? 1 : v;
    private static double EaseOutCubic(double x) => 1 - Math.Pow(1 - x, 3);
    private static double EaseInCubic(double x) => x * x * x;
    private static double EaseInOutSine(double x) => -(Math.Cos(Math.PI * x) - 1) / 2;

    private static double EaseOutBack(double x, double overshoot = 1.70158)
    {
        var c3 = overshoot + 1;
        return 1 + c3 * Math.Pow(x - 1, 3) + overshoot * Math.Pow(x - 1, 2);
    }

    private static Color Lerp(Color a, Color b, double t) => Color.FromArgb(
        (byte)(a.A + (b.A - a.A) * t),
        (byte)(a.R + (b.R - a.R) * t),
        (byte)(a.G + (b.G - a.G) * t),
        (byte)(a.B + (b.B - a.B) * t));

    private static Color FromHsv(double h, double s, double v)
    {
        var i = (int)(h * 6) % 6;
        var f = h * 6 - Math.Floor(h * 6);
        var p = v * (1 - s);
        var q = v * (1 - f * s);
        var t = v * (1 - (1 - f) * s);
        var (r, g, b) = i switch
        {
            0 => (v, t, p),
            1 => (q, v, p),
            2 => (p, v, t),
            3 => (p, q, v),
            4 => (t, p, v),
            _ => (v, p, q),
        };
        return Color.FromRgb((byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
    }
}
