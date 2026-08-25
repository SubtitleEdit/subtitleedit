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
