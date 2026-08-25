using SkiaSharp;

namespace Nikse.SubtitleEdit.UiLogic.Export;

/// <summary>
/// Advanced text formatting for image-based export. When <see cref="ImageParameter.TextEffects"/>
/// is set, <see cref="ImageRenderer"/> renders the subtitle through a layered pipeline instead of
/// the classic fill/outline/shadow path: the shaped glyphs become one combined SKPath, and every
/// effect is then a path operation - gradient fills are shaders bounded to the text, outlines are
/// strokes, shadows/glows are blurred fills. That is what lets the effects compose freely.
/// </summary>
public class TextEffects
{
    /// <summary>
    /// Fill for all text. Null means "use the per-segment colors" (font color and
    /// &lt;font color=..&gt; tags), like the classic renderer.
    /// </summary>
    public TextEffectFill? Fill { get; set; }

    /// <summary>Outline rings, innermost first. Replaces the classic single outline.</summary>
    public List<TextEffectStroke> Strokes { get; set; } = new();

    /// <summary>Drop shadows. Blur 0 gives the classic hard shadow.</summary>
    public List<TextEffectShadow> Shadows { get; set; } = new();

    public TextEffectGlow? Glow { get; set; }
    public TextEffectExtrude? Extrude { get; set; }
    public TextEffectBevel? Bevel { get; set; }

    /// <summary>Softens the glyph edges (like ASSA "\be").</summary>
    public float EdgeBlur { get; set; }

    /// <summary>Extra pixels between glyphs (like ASSA "\fsp").</summary>
    public float LetterSpacing { get; set; }

    /// <summary>
    /// Bends each line along a circular arc. -100..100; positive arches up (rainbow),
    /// negative curves down (smile), 0 is straight. The radius is derived from the widest
    /// line so the same value gives the same visual bend at any resolution.
    /// </summary>
    public float ArcBendPercent { get; set; }

    /// <summary>Sine-wave baseline offset in pixels; 0 is off.</summary>
    public float WaveAmplitude { get; set; }

    /// <summary>Wavelength of the baseline wave; 0 = pick from the font size.</summary>
    public float WaveLength { get; set; }

    /// <summary>
    /// True when any per-glyph geometry is active - those effects need each glyph as its
    /// own positioned path instead of a straight DrawShapedText run.
    /// </summary>
    public bool HasGlyphGeometry => LetterSpacing != 0 || ArcBendPercent != 0 || WaveAmplitude > 0;

    /// <summary>
    /// How far, in pixels, the effects can draw outside the glyph paths - used to pad the
    /// scratch canvas so blurred shadows and glows are never clipped. Blur sigma extends
    /// roughly 3 sigma.
    /// </summary>
    public float GetSafetyMargin()
    {
        var margin = 0f;

        foreach (var shadow in Shadows)
        {
            margin = Math.Max(margin, Math.Abs(shadow.Dx) + Math.Abs(shadow.Dy) + shadow.Blur * 3f);
        }

        if (Glow != null)
        {
            margin = Math.Max(margin, Glow.Radius * 3f);
        }

        if (Extrude != null)
        {
            margin = Math.Max(margin, Extrude.Depth * Math.Max(Math.Abs(Extrude.Dx), Math.Abs(Extrude.Dy)) + 2f);
        }

        var strokeTotal = 0f;
        var strokeBlur = 0f;
        foreach (var stroke in Strokes)
        {
            strokeTotal += stroke.Width;
            strokeBlur = Math.Max(strokeBlur, stroke.Blur);
        }

        margin = Math.Max(margin, strokeTotal + Math.Max(strokeBlur, EdgeBlur) * 3f);
        margin = Math.Max(margin, EdgeBlur * 3f);
        margin += WaveAmplitude;

        return margin + 4f;
    }
}

public enum TextEffectFillKind
{
    Solid,
    LinearGradient,
    RadialGradient,

    /// <summary>Perlin turbulence composed over the gradient - fire/marble/wood style texture.</summary>
    Turbulence,

    /// <summary>Repeating two-color stripes (candy cane and friends).</summary>
    Stripes,

    /// <summary>Repeating polka dots, color 1 on a color 0 background.</summary>
    Dots,
}

/// <summary>A paint source resolved against the rendered text's bounds.</summary>
public class TextEffectFill
{
    public TextEffectFillKind Kind { get; set; } = TextEffectFillKind.Solid;
    public SKColor[] Colors { get; set; } = { SKColors.White };
    public float[]? Stops { get; set; }

    /// <summary>Gradient direction in degrees; 90 = top-to-bottom.</summary>
    public float AngleDegrees { get; set; } = 90;

    /// <summary>
    /// Turbulence tuning. The X/Y base frequencies are what make one texture read as fire,
    /// marble or wood grain: equal small values give blotches, a strongly anisotropic pair
    /// gives streaks/grain along the smaller-frequency axis.
    /// </summary>
    public float NoiseFrequencyX { get; set; } = 0.012f;

    public float NoiseFrequencyY { get; set; } = 0.035f;
    public int NoiseOctaves { get; set; } = 3;

    /// <summary>Stripe width or dot cell half-size, in pixels (set from the font size).</summary>
    public float TileSize { get; set; } = 12f;

    /// <summary>Stripe direction in degrees.</summary>
    public float TileAngleDegrees { get; set; } = 45f;

    public static TextEffectFill Solid(SKColor color)
    {
        return new TextEffectFill { Kind = TextEffectFillKind.Solid, Colors = new[] { color } };
    }

    public static TextEffectFill Linear(params SKColor[] colors)
    {
        return new TextEffectFill { Kind = TextEffectFillKind.LinearGradient, Colors = colors };
    }

    /// <summary>
    /// Configures the paint to draw with this fill across <paramref name="bounds"/>.
    /// Any shader is owned by the paint and freed with it.
    /// </summary>
    public void ApplyTo(SKPaint paint, SKRect bounds)
    {
        switch (Kind)
        {
            case TextEffectFillKind.LinearGradient:
                paint.Color = SKColors.White;
                paint.Shader = MakeLinearGradient(bounds);
                break;

            case TextEffectFillKind.RadialGradient:
                paint.Color = SKColors.White;
                paint.Shader = SKShader.CreateRadialGradient(
                    new SKPoint(bounds.MidX, bounds.MidY),
                    Math.Max(bounds.Width, bounds.Height) / 2f,
                    Colors, Stops, SKShaderTileMode.Clamp);
                break;

            case TextEffectFillKind.Turbulence:
                paint.Color = SKColors.White;
                using (var gradient = MakeLinearGradient(bounds))
                using (var noise = SKShader.CreatePerlinNoiseTurbulence(NoiseFrequencyX, NoiseFrequencyY, NoiseOctaves, 7f))
                {
                    paint.Shader = SKShader.CreateCompose(gradient, noise, SKBlendMode.Overlay);
                }

                break;

            case TextEffectFillKind.Stripes:
                paint.Color = SKColors.White;
                paint.Shader = MakeStripesShader();
                break;

            case TextEffectFillKind.Dots:
                paint.Color = SKColors.White;
                paint.Shader = MakeDotsShader();
                break;

            default:
                paint.Color = Colors[0];
                break;
        }
    }

    /// <summary>
    /// Repeating stripe texture as a bitmap tile shader. The tile is copied into an
    /// SKImage, and the shader keeps its own native reference, so nothing here has to
    /// outlive this call.
    /// </summary>
    private SKShader MakeStripesShader()
    {
        var stripe = Math.Max(2f, TileSize);
        var tileHeight = (int)Math.Ceiling(stripe * 2);
        using var tile = new SKBitmap(8, tileHeight);
        using (var canvas = new SKCanvas(tile))
        using (var p = new SKPaint())
        {
            p.Color = Colors[0];
            canvas.DrawRect(0, 0, 8, stripe, p);
            p.Color = Colors.Length > 1 ? Colors[1] : SKColors.White;
            canvas.DrawRect(0, stripe, 8, tileHeight - stripe, p);
        }

        using var image = SKImage.FromBitmap(tile);
        return image.ToShader(SKShaderTileMode.Repeat, SKShaderTileMode.Repeat,
            new SKSamplingOptions(SKFilterMode.Linear),
            SKMatrix.CreateRotationDegrees(TileAngleDegrees));
    }

    /// <summary>Repeating polka-dot texture: color 1 dots on a color 0 background.</summary>
    private SKShader MakeDotsShader()
    {
        var cell = (int)Math.Ceiling(Math.Max(4f, TileSize * 2));
        using var tile = new SKBitmap(cell, cell);
        using (var canvas = new SKCanvas(tile))
        using (var p = new SKPaint { IsAntialias = true })
        {
            canvas.Clear(Colors[0]);
            p.Color = Colors.Length > 1 ? Colors[1] : SKColors.White;
            var radius = cell * 0.28f;
            canvas.DrawCircle(cell / 2f, cell / 2f, radius, p);
            // quarter dots in the corners give the offset "polka" packing when tiled
            canvas.DrawCircle(0, 0, radius, p);
            canvas.DrawCircle(cell, 0, radius, p);
            canvas.DrawCircle(0, cell, radius, p);
            canvas.DrawCircle(cell, cell, radius, p);
        }

        using var image = SKImage.FromBitmap(tile);
        return image.ToShader(SKShaderTileMode.Repeat, SKShaderTileMode.Repeat,
            new SKSamplingOptions(SKFilterMode.Linear), SKMatrix.CreateIdentity());
    }

    private SKShader MakeLinearGradient(SKRect bounds)
    {
        var radians = AngleDegrees * MathF.PI / 180f;
        var dx = MathF.Cos(radians);
        var dy = MathF.Sin(radians);

        // Project the bounds onto the gradient direction so the gradient spans the text
        // exactly, whatever the angle.
        var half = (Math.Abs(dx) * bounds.Width + Math.Abs(dy) * bounds.Height) / 2f;
        var from = new SKPoint(bounds.MidX - dx * half, bounds.MidY - dy * half);
        var to = new SKPoint(bounds.MidX + dx * half, bounds.MidY + dy * half);
        return SKShader.CreateLinearGradient(from, to, Colors, Stops, SKShaderTileMode.Clamp);
    }
}

/// <summary>One outline ring.</summary>
public class TextEffectStroke
{
    public float Width { get; set; } = 2f;
    public TextEffectFill Fill { get; set; } = TextEffectFill.Solid(SKColors.Black);

    /// <summary>Softens just this ring.</summary>
    public float Blur { get; set; }
}

public class TextEffectShadow
{
    public float Dx { get; set; } = 3;
    public float Dy { get; set; } = 3;

    /// <summary>0 = hard classic shadow; larger = the soft "streaming" look.</summary>
    public float Blur { get; set; }

    public SKColor Color { get; set; } = new SKColor(0, 0, 0, 200);
}

/// <summary>Neon-style halo: repeated blurred draws that saturate around the glyphs.</summary>
public class TextEffectGlow
{
    public SKColor Color { get; set; } = SKColors.Cyan;
    public float Radius { get; set; } = 14;
    public int Passes { get; set; } = 3;
}

/// <summary>Fake 3D depth: the glyph path re-filled Depth times, stepping (Dx, Dy) each time.</summary>
public class TextEffectExtrude
{
    public int Depth { get; set; } = 8;
    public float Dx { get; set; } = 1.5f;
    public float Dy { get; set; } = 1.5f;
    public SKColor NearColor { get; set; } = new SKColor(120, 60, 0);
    public SKColor FarColor { get; set; } = new SKColor(40, 20, 0);
}

/// <summary>Inner bevel: blurred light/dark strokes clipped inside the glyphs.</summary>
public class TextEffectBevel
{
    public SKColor Highlight { get; set; } = new SKColor(255, 255, 255, 180);
    public SKColor Shade { get; set; } = new SKColor(0, 0, 0, 160);
    public float Depth { get; set; } = 3;
}
