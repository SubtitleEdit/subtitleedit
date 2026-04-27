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

public static class AdvancedEffectDisplayFactory
{
    public static List<IAdvancedEffectDisplay> List()
    {
        return new List<IAdvancedEffectDisplay>
        {
            new AdvancedEffectAudioTextPulse(),
            new AdvancedEffectBounceIn(),
            new AdvancedEffectConfetti(),
            new AdvancedEffectEndCreditsScroll(),
            new AdvancedEffectFadeIn(),
            new AdvancedEffectFadeInOut(),
            new AdvancedEffectFadeOut(),
            new AdvancedEffectFancyKaraoke(),
            new AdvancedEffectFireflies(),
            new AdvancedEffectGlitch(),
            new AdvancedEffectHearts(),
            new AdvancedEffectKaraoke(),
            new AdvancedEffectMatrix(),
            new AdvancedEffectNeonBurst(),
            new AdvancedEffectOldMovie(),
            new AdvancedEffectRain(),
            new AdvancedEffectRainbowPulse(),
            new AdvancedEffectScrambleReveal(),
            new AdvancedEffectSlideInLeft(),
            new AdvancedEffectSlideInRight(),
            new AdvancedEffectSlowZoomIn(),
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
            new AdvancedEffectWordSpacing(),
        }.OrderBy(p => p.Name).ToList();
    }
}
