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

        return margin + 4f;
    }
}

public enum TextEffectFillKind
{
    Solid,
    LinearGradient,
    RadialGradient,

    /// <summary>Perlin turbulence composed over the gradient - fire/marble style texture.</summary>
    Turbulence,
}

/// <summary>A paint source resolved against the rendered text's bounds.</summary>
public class TextEffectFill
{
    public TextEffectFillKind Kind { get; set; } = TextEffectFillKind.Solid;
    public SKColor[] Colors { get; set; } = { SKColors.White };
    public float[]? Stops { get; set; }

    /// <summary>Gradient direction in degrees; 90 = top-to-bottom.</summary>
    public float AngleDegrees { get; set; } = 90;

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
                using (var noise = SKShader.CreatePerlinNoiseTurbulence(0.012f, 0.035f, 3, 7f))
                {
                    paint.Shader = SKShader.CreateCompose(gradient, noise, SKBlendMode.Overlay);
                }

                break;

            default:
                paint.Color = Colors[0];
                break;
        }
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
