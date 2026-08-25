using SkiaSharp;

namespace Nikse.SubtitleEdit.UiLogic.Export;

public enum TextEffectPreset
{
    None,
    SoftShadow,
    GradientGold,
    DoubleOutline,
    NeonGlow,
    Extrude3D,
    Chrome,
    Fire,
    Comic,
    Retro80s,
    Anaglyph3D,
    Ice,
    Emboss,
    Hollow,
    Marble,
    Wood,
    Lava,
    BrushedSteel,
    CandyCane,
    Rainbow,
    PolkaDots,
}

/// <summary>
/// User tweaks applied on top of a preset: overall strength scales every effect size
/// (stroke widths, blur radii, glow, extrude depth), and the geometry values pass straight
/// through to <see cref="TextEffects"/>.
/// </summary>
public class TextEffectAdjustments
{
    public int StrengthPercent { get; set; } = 100;
    public float LetterSpacing { get; set; }
    public float ArcBendPercent { get; set; }
    public float WaveAmplitude { get; set; }
}

/// <summary>
/// Builds a <see cref="TextEffects"/> from a named preset. All sizes scale with the font size
/// so a preset looks the same at 26 px and at 90 px. Presets use the dialog's colors where a
/// choice is natural (font color as fill, outline color as glow/ring color); the "signature"
/// looks (gold, chrome, fire) bring their own palette.
/// </summary>
public static class TextEffectPresetFactory
{
    public static TextEffects? Create(
        TextEffectPreset preset,
        float fontSize,
        SKColor fontColor,
        SKColor outlineColor,
        SKColor shadowColor,
        TextEffectAdjustments? adjustments = null)
    {
        var effects = CreateBase(preset, fontSize, fontColor, outlineColor, shadowColor);
        if (effects == null)
        {
            return null;
        }

        if (adjustments != null)
        {
            var strength = Math.Clamp(adjustments.StrengthPercent, 10, 400) / 100f;
            if (Math.Abs(strength - 1f) > 0.001f)
            {
                Scale(effects, strength);
            }

            effects.LetterSpacing = adjustments.LetterSpacing;
            effects.ArcBendPercent = adjustments.ArcBendPercent;
            effects.WaveAmplitude = adjustments.WaveAmplitude;
        }

        return effects;
    }

    private static void Scale(TextEffects effects, float factor)
    {
        foreach (var stroke in effects.Strokes)
        {
            stroke.Width *= factor;
            stroke.Blur *= factor;
        }

        foreach (var shadow in effects.Shadows)
        {
            shadow.Dx *= factor;
            shadow.Dy *= factor;
            shadow.Blur *= factor;
        }

        if (effects.Glow != null)
        {
            effects.Glow.Radius *= factor;
        }

        if (effects.Extrude != null)
        {
            effects.Extrude.Dx *= factor;
            effects.Extrude.Dy *= factor;
        }

        if (effects.Bevel != null)
        {
            effects.Bevel.Depth *= factor;
        }

        effects.EdgeBlur *= factor;
    }

    private static TextEffects? CreateBase(
        TextEffectPreset preset,
        float fontSize,
        SKColor fontColor,
        SKColor outlineColor,
        SKColor shadowColor)
    {
        var s = fontSize; // effect sizes below are fractions of the font size

        switch (preset)
        {
            case TextEffectPreset.SoftShadow:
                return new TextEffects
                {
                    Fill = TextEffectFill.Solid(fontColor),
                    Shadows =
                    {
                        new TextEffectShadow { Dx = 0, Dy = s * 0.08f, Blur = s * 0.16f, Color = shadowColor },
                    },
                };

            case TextEffectPreset.GradientGold:
                return new TextEffects
                {
                    Fill = new TextEffectFill
                    {
                        Kind = TextEffectFillKind.LinearGradient,
                        Colors = new[]
                        {
                            new SKColor(255, 245, 200), new SKColor(255, 200, 60),
                            new SKColor(180, 110, 10), new SKColor(255, 220, 120),
                        },
                        Stops = new[] { 0f, 0.45f, 0.75f, 1f },
                    },
                    Strokes = { new TextEffectStroke { Width = s * 0.032f, Fill = TextEffectFill.Solid(new SKColor(70, 40, 0)) } },
                    Shadows = { new TextEffectShadow { Dx = s * 0.04f, Dy = s * 0.05f, Blur = s * 0.1f, Color = new SKColor(0, 0, 0, 200) } },
                };

            case TextEffectPreset.DoubleOutline:
                return new TextEffects
                {
                    Fill = TextEffectFill.Solid(fontColor),
                    Strokes =
                    {
                        new TextEffectStroke { Width = s * 0.04f, Fill = TextEffectFill.Solid(outlineColor) },
                        new TextEffectStroke { Width = s * 0.065f, Fill = TextEffectFill.Solid(shadowColor) },
                    },
                    Shadows = { new TextEffectShadow { Dx = s * 0.05f, Dy = s * 0.05f, Blur = s * 0.08f, Color = new SKColor(0, 0, 0, 160) } },
                };

            case TextEffectPreset.NeonGlow:
                return new TextEffects
                {
                    Fill = TextEffectFill.Solid(fontColor),
                    Strokes = { new TextEffectStroke { Width = s * 0.02f, Fill = TextEffectFill.Solid(outlineColor) } },
                    Glow = new TextEffectGlow { Color = outlineColor, Radius = s * 0.16f, Passes = 4 },
                };

            case TextEffectPreset.Extrude3D:
                return new TextEffects
                {
                    Fill = new TextEffectFill
                    {
                        Kind = TextEffectFillKind.LinearGradient,
                        Colors = new[] { fontColor, Darken(fontColor, 0.3f) },
                    },
                    Strokes = { new TextEffectStroke { Width = s * 0.015f, Fill = TextEffectFill.Solid(Darken(fontColor, 0.8f)) } },
                    Extrude = new TextEffectExtrude
                    {
                        Depth = Math.Max(4, (int)(s * 0.12f)),
                        Dx = Math.Max(1f, s * 0.017f),
                        Dy = Math.Max(1f, s * 0.017f),
                        NearColor = Darken(fontColor, 0.45f),
                        FarColor = Darken(fontColor, 0.75f),
                    },
                    Shadows = { new TextEffectShadow { Dx = s * 0.23f, Dy = s * 0.23f, Blur = s * 0.14f, Color = new SKColor(0, 0, 0, 120) } },
                };

            case TextEffectPreset.Chrome:
                return new TextEffects
                {
                    Fill = new TextEffectFill
                    {
                        Kind = TextEffectFillKind.LinearGradient,
                        Colors = new[]
                        {
                            new SKColor(235, 245, 255), new SKColor(140, 170, 200), new SKColor(250, 252, 255),
                            new SKColor(90, 110, 140), new SKColor(200, 220, 240),
                        },
                        Stops = new[] { 0f, 0.42f, 0.5f, 0.58f, 1f },
                    },
                    Strokes = { new TextEffectStroke { Width = s * 0.023f, Fill = TextEffectFill.Solid(new SKColor(30, 40, 60)) } },
                    Bevel = new TextEffectBevel { Depth = Math.Max(1.5f, s * 0.031f) },
                    Shadows = { new TextEffectShadow { Dx = 0, Dy = s * 0.06f, Blur = s * 0.08f, Color = new SKColor(0, 0, 0, 180) } },
                };

            case TextEffectPreset.Fire:
                return new TextEffects
                {
                    Fill = new TextEffectFill
                    {
                        Kind = TextEffectFillKind.Turbulence,
                        Colors = new[] { new SKColor(255, 240, 120), new SKColor(255, 120, 0), new SKColor(180, 20, 0) },
                    },
                    Strokes = { new TextEffectStroke { Width = s * 0.023f, Fill = TextEffectFill.Solid(new SKColor(60, 10, 0)) } },
                    Glow = new TextEffectGlow { Color = new SKColor(255, 100, 0), Radius = s * 0.11f, Passes = 2 },
                };

            case TextEffectPreset.Comic:
                return new TextEffects
                {
                    Fill = TextEffectFill.Solid(fontColor),
                    Strokes = { new TextEffectStroke { Width = s * 0.05f, Fill = TextEffectFill.Solid(SKColors.Black) } },
                    Shadows = { new TextEffectShadow { Dx = s * 0.09f, Dy = s * 0.09f, Blur = 0, Color = SKColors.Black } },
                };

            case TextEffectPreset.Retro80s:
                return new TextEffects
                {
                    Fill = new TextEffectFill
                    {
                        Kind = TextEffectFillKind.LinearGradient,
                        Colors = new[] { new SKColor(255, 90, 200), new SKColor(180, 70, 255), new SKColor(60, 200, 255) },
                    },
                    Strokes = { new TextEffectStroke { Width = s * 0.015f, Fill = TextEffectFill.Solid(new SKColor(255, 240, 255)) } },
                    Glow = new TextEffectGlow { Color = new SKColor(200, 60, 255), Radius = s * 0.12f, Passes = 3 },
                    Extrude = new TextEffectExtrude
                    {
                        Depth = Math.Max(3, (int)(s * 0.06f)),
                        Dx = Math.Max(1f, s * 0.015f),
                        Dy = Math.Max(1f, s * 0.015f),
                        NearColor = new SKColor(90, 20, 130),
                        FarColor = new SKColor(30, 5, 50),
                    },
                };

            case TextEffectPreset.Anaglyph3D:
                return new TextEffects
                {
                    Fill = TextEffectFill.Solid(fontColor),
                    Shadows =
                    {
                        new TextEffectShadow { Dx = -s * 0.06f, Dy = 0, Blur = 0, Color = new SKColor(255, 0, 60, 200) },
                        new TextEffectShadow { Dx = s * 0.06f, Dy = 0, Blur = 0, Color = new SKColor(0, 230, 255, 200) },
                    },
                };

            case TextEffectPreset.Ice:
                return new TextEffects
                {
                    Fill = new TextEffectFill
                    {
                        Kind = TextEffectFillKind.LinearGradient,
                        Colors = new[] { SKColors.White, new SKColor(200, 230, 255), new SKColor(110, 175, 230) },
                    },
                    Strokes = { new TextEffectStroke { Width = s * 0.02f, Fill = TextEffectFill.Solid(new SKColor(40, 80, 140)) } },
                    Glow = new TextEffectGlow { Color = new SKColor(170, 220, 255), Radius = s * 0.1f, Passes = 2 },
                };

            case TextEffectPreset.Emboss:
                return new TextEffects
                {
                    Fill = TextEffectFill.Solid(fontColor),
                    Bevel = new TextEffectBevel { Depth = Math.Max(1.5f, s * 0.05f) },
                    Shadows = { new TextEffectShadow { Dx = 0, Dy = s * 0.04f, Blur = s * 0.06f, Color = new SKColor(0, 0, 0, 140) } },
                };

            case TextEffectPreset.Hollow:
                return new TextEffects
                {
                    Fill = TextEffectFill.Solid(SKColors.Transparent),
                    Strokes = { new TextEffectStroke { Width = s * 0.028f, Fill = TextEffectFill.Solid(fontColor) } },
                    Shadows = { new TextEffectShadow { Dx = s * 0.04f, Dy = s * 0.04f, Blur = s * 0.08f, Color = new SKColor(0, 0, 0, 150) } },
                };

            case TextEffectPreset.Marble:
                return new TextEffects
                {
                    Fill = new TextEffectFill
                    {
                        Kind = TextEffectFillKind.Turbulence,
                        Colors = new[] { SKColors.White, new SKColor(205, 210, 220), new SKColor(130, 140, 160) },
                        NoiseFrequencyX = 0.008f,
                        NoiseFrequencyY = 0.008f,
                        NoiseOctaves = 4,
                    },
                    Strokes = { new TextEffectStroke { Width = s * 0.025f, Fill = TextEffectFill.Solid(new SKColor(60, 65, 80)) } },
                    Shadows = { new TextEffectShadow { Dx = 0, Dy = s * 0.05f, Blur = s * 0.08f, Color = new SKColor(0, 0, 0, 170) } },
                };

            case TextEffectPreset.Wood:
                return new TextEffects
                {
                    Fill = new TextEffectFill
                    {
                        Kind = TextEffectFillKind.Turbulence,
                        Colors = new[] { new SKColor(210, 160, 100), new SKColor(160, 105, 55), new SKColor(100, 60, 30) },
                        // Strongly anisotropic frequencies read as horizontal grain.
                        NoiseFrequencyX = 0.004f,
                        NoiseFrequencyY = 0.09f,
                        NoiseOctaves = 4,
                    },
                    Strokes = { new TextEffectStroke { Width = s * 0.028f, Fill = TextEffectFill.Solid(new SKColor(55, 30, 12)) } },
                    Shadows = { new TextEffectShadow { Dx = s * 0.04f, Dy = s * 0.05f, Blur = s * 0.07f, Color = new SKColor(0, 0, 0, 170) } },
                };

            case TextEffectPreset.Lava:
                return new TextEffects
                {
                    Fill = new TextEffectFill
                    {
                        Kind = TextEffectFillKind.Turbulence,
                        Colors = new[] { new SKColor(255, 235, 100), new SKColor(240, 80, 10), new SKColor(70, 5, 5) },
                        NoiseFrequencyX = 0.02f,
                        NoiseFrequencyY = 0.02f,
                        NoiseOctaves = 4,
                    },
                    Strokes = { new TextEffectStroke { Width = s * 0.025f, Fill = TextEffectFill.Solid(new SKColor(35, 5, 5)) } },
                    Glow = new TextEffectGlow { Color = new SKColor(255, 60, 0), Radius = s * 0.1f, Passes = 2 },
                };

            case TextEffectPreset.BrushedSteel:
                return new TextEffects
                {
                    Fill = new TextEffectFill
                    {
                        Kind = TextEffectFillKind.Turbulence,
                        Colors = new[] { new SKColor(225, 230, 238), new SKColor(150, 158, 170), new SKColor(200, 208, 218) },
                        // Fine horizontal streaks over the silver gradient = brushed metal.
                        NoiseFrequencyX = 0.002f,
                        NoiseFrequencyY = 0.35f,
                        NoiseOctaves = 2,
                    },
                    Strokes = { new TextEffectStroke { Width = s * 0.023f, Fill = TextEffectFill.Solid(new SKColor(45, 52, 65)) } },
                    Bevel = new TextEffectBevel { Depth = Math.Max(1.5f, s * 0.028f) },
                    Shadows = { new TextEffectShadow { Dx = 0, Dy = s * 0.06f, Blur = s * 0.08f, Color = new SKColor(0, 0, 0, 170) } },
                };

            case TextEffectPreset.CandyCane:
                return new TextEffects
                {
                    Fill = new TextEffectFill
                    {
                        Kind = TextEffectFillKind.Stripes,
                        Colors = new[] { new SKColor(220, 30, 40), SKColors.White },
                        TileSize = Math.Max(3f, s * 0.14f),
                        TileAngleDegrees = 45f,
                    },
                    Strokes = { new TextEffectStroke { Width = s * 0.028f, Fill = TextEffectFill.Solid(new SKColor(120, 10, 20)) } },
                    Shadows = { new TextEffectShadow { Dx = s * 0.04f, Dy = s * 0.05f, Blur = s * 0.08f, Color = new SKColor(0, 0, 0, 160) } },
                };

            case TextEffectPreset.Rainbow:
                return new TextEffects
                {
                    Fill = new TextEffectFill
                    {
                        Kind = TextEffectFillKind.LinearGradient,
                        AngleDegrees = 0,
                        Colors = new[]
                        {
                            new SKColor(228, 30, 40), new SKColor(255, 150, 30), new SKColor(255, 220, 50),
                            new SKColor(70, 190, 90), new SKColor(50, 120, 230), new SKColor(150, 70, 200),
                        },
                    },
                    Strokes = { new TextEffectStroke { Width = s * 0.028f, Fill = TextEffectFill.Solid(new SKColor(25, 25, 35)) } },
                    Shadows = { new TextEffectShadow { Dx = 0, Dy = s * 0.05f, Blur = s * 0.09f, Color = new SKColor(0, 0, 0, 170) } },
                };

            case TextEffectPreset.PolkaDots:
                return new TextEffects
                {
                    Fill = new TextEffectFill
                    {
                        Kind = TextEffectFillKind.Dots,
                        Colors = new[] { new SKColor(230, 60, 120), SKColors.White },
                        TileSize = Math.Max(3f, s * 0.11f),
                    },
                    Strokes = { new TextEffectStroke { Width = s * 0.032f, Fill = TextEffectFill.Solid(new SKColor(90, 15, 45)) } },
                    Shadows = { new TextEffectShadow { Dx = s * 0.05f, Dy = s * 0.05f, Blur = 0, Color = new SKColor(40, 5, 20, 200) } },
                };

            default:
                return null;
        }
    }

    private static SKColor Darken(SKColor color, float amount)
    {
        var keep = 1f - Math.Clamp(amount, 0f, 1f);
        return new SKColor(
            (byte)(color.Red * keep),
            (byte)(color.Green * keep),
            (byte)(color.Blue * keep),
            color.Alpha);
    }
}
