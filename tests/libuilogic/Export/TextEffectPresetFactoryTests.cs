using Nikse.SubtitleEdit.UiLogic.Export;
using SkiaSharp;

namespace LibUiLogicTests.Export;

/// <summary>
/// The settings-gated <see cref="TextEffectPresetFactory"/> overload: the one shared mapping
/// from the export-images settings (dialog state or a stored profile) to a
/// <see cref="TextEffects"/>, used by both the export dialog and batch convert - so a batch
/// convert renders the same effect the dialog's preview shows.
/// </summary>
public class TextEffectPresetFactoryTests
{
    private static TextEffects? Create(bool enabled, int strengthPercent, int letterSpacing = 0, int arcBend = 0, int wave = 0)
    {
        return TextEffectPresetFactory.Create(
            enabled,
            TextEffectPreset.SoftShadow,
            fontSize: 40,
            SKColors.White,
            SKColors.Black,
            SKColors.Black,
            strengthPercent,
            letterSpacing,
            arcBend,
            wave);
    }

    [Fact]
    public void Disabled_ReturnsNull()
    {
        Assert.Null(Create(enabled: false, strengthPercent: 100));
    }

    [Fact]
    public void Enabled_BuildsThePresetWithTheAdjustments()
    {
        var effects = Create(enabled: true, strengthPercent: 100, letterSpacing: 12, arcBend: -30, wave: 8);

        Assert.NotNull(effects);
        Assert.Equal(12f, effects!.LetterSpacing);
        Assert.Equal(-30f, effects.ArcBendPercent);
        Assert.Equal(8f, effects.WaveAmplitude);
    }

    [Fact]
    public void Strength_ScalesTheEffectSizes()
    {
        var neutral = Create(enabled: true, strengthPercent: 100)!;
        var doubled = Create(enabled: true, strengthPercent: 200)!;

        Assert.Equal(neutral.Shadows[0].Dy * 2, doubled.Shadows[0].Dy);
        Assert.Equal(neutral.Shadows[0].Blur * 2, doubled.Shadows[0].Blur);
    }

    /// <summary>
    /// A profile saved before the strength setting existed stores 0 - that means "never set",
    /// not "as weak as possible", so it renders like the neutral 100%.
    /// </summary>
    [Fact]
    public void NonPositiveStrength_IsTreatedAsNeutral()
    {
        var neutral = Create(enabled: true, strengthPercent: 100)!;
        var unset = Create(enabled: true, strengthPercent: 0)!;

        Assert.Equal(neutral.Shadows[0].Dy, unset.Shadows[0].Dy);
        Assert.Equal(neutral.Shadows[0].Blur, unset.Shadows[0].Blur);
    }

    /// <summary>"None" is a valid stored preset name and turns the effect off like the checkbox does.</summary>
    [Fact]
    public void NonePreset_ReturnsNull()
    {
        var effects = TextEffectPresetFactory.Create(
            enabled: true,
            TextEffectPreset.None,
            fontSize: 40,
            SKColors.White,
            SKColors.Black,
            SKColors.Black,
            strengthPercent: 100,
            letterSpacing: 0,
            arcBendPercent: 0,
            waveAmplitude: 0);

        Assert.Null(effects);
    }
}
