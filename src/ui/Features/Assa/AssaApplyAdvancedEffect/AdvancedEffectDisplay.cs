using Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect.Effects;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Media;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect;

public interface IAdvancedEffectDisplay
{
    string Name { get; }
    string Description { get; }
    bool UsesAudio { get; }
    List<SubtitleLineViewModel> ApplyEffect(string header, List<SubtitleLineViewModel> subtitles, int width, int height, WavePeakData2? wavePeaks);
}

/// <summary>
/// An effect that needs extra styles in the script header (e.g. an opaque-box style).
/// </summary>
public interface IAdvancedEffectHeaderUpdater
{
    string UpdateHeader(string header, List<SubtitleLineViewModel> subtitles);
}

public static class AdvancedEffectDisplayFactory
{
    public static List<IAdvancedEffectDisplay> List()
    {
        return new List<IAdvancedEffectDisplay>
        {
            new AdvancedEffectAudioTextPulse(),
            new AdvancedEffectBlurInWords(),
            new AdvancedEffectBoldCaptions(),
            new AdvancedEffectBounceIn(),
            new AdvancedEffectCinematicTitle(),
            new AdvancedEffectConfetti(),
            new AdvancedEffectEndCreditsScroll(),
            new AdvancedEffectFadeIn(),
            new AdvancedEffectFadeInOut(),
            new AdvancedEffectFadeOut(),
            new AdvancedEffectFancyKaraoke(),
            new AdvancedEffectFireflies(),
            new AdvancedEffectGlitch(),
            new AdvancedEffectHearts(),
            new AdvancedEffectHighlightBox(),
            new AdvancedEffectKaraoke(),
            new AdvancedEffectLowerThird(),
            new AdvancedEffectMatrix(),
            new AdvancedEffectNeonBurst(),
            new AdvancedEffectOldMovie(),
            new AdvancedEffectRain(),
            new AdvancedEffectRainbowPulse(),
            new AdvancedEffectRollingCaptions(),
            new AdvancedEffectScrambleReveal(),
            new AdvancedEffectSingleWordPop(),
            new AdvancedEffectSlideInLeft(),
            new AdvancedEffectSlideInRight(),
            new AdvancedEffectSlowZoomIn(),
            new AdvancedEffectSmoothFill(),
            new AdvancedEffectSlowZoomOut(),
            new AdvancedEffectSnow(),
            new AdvancedEffectStarWarsScroll(),
            new AdvancedEffectStarfield(),
            new AdvancedEffectTvClose(),
            new AdvancedEffectTypewriter(),
            new AdvancedEffectTypewriterWithHighlight(),
            new AdvancedEffectWave(),
            new AdvancedEffectWaveBlue(),
            new AdvancedEffectWordByWord(),
            new AdvancedEffectWordFlip3D(),
            new AdvancedEffectWordSpacing(),
        }.OrderBy(p => p.Name).ToList();
    }
}
