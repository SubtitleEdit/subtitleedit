using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect.Effects;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect;

public enum AdvancedEffectCategory
{
    All,
    Words,
    InOut,
    TextFx,
    Scenes,
    Titles,
}

/// <summary>
/// How the gallery tile and the no-video preview animate an effect. The preview is a quick
/// Avalonia approximation of the look - the real libass rendering is shown on the video.
/// </summary>
public enum EffectPreviewKind
{
    None,
    ScreenFadeIn,
    ScreenFadeOut,
    FadeInOut,
    SlideInLeft,
    SlideInRight,
    BounceIn,
    ZoomIn,
    ZoomOut,
    TvClose,
    Typewriter,
    TypewriterHighlight,
    WordByWord,
    Karaoke,
    WordPop,
    HighlightBox,
    SingleWordPop,
    BoldCaptions,
    SmoothFill,
    BlurInWords,
    RollingCaptions,
    ScrambleReveal,
    WordFlip3D,
    WordSpacing,
    RainbowPulse,
    Wave,
    WaveBlue,
    Glitch,
    NeonBurst,
    AudioPulse,
    Snow,
    Rain,
    Starfield,
    Fireflies,
    Confetti,
    Hearts,
    Matrix,
    OldMovie,
    CinematicTitle,
    LowerThird,
    StarWarsScroll,
    CreditsScroll,
}

public class AdvancedEffectCategoryItem
{
    public AdvancedEffectCategory Category { get; }
    public string Name { get; }
    public string IconName { get; }
    public int Count { get; set; }
    public IBrush Brush { get; }

    public AdvancedEffectCategoryItem(AdvancedEffectCategory category, string name, string iconName, Color color)
    {
        Category = category;
        Name = name;
        IconName = iconName;
        Brush = new SolidColorBrush(color);
    }

    public override string ToString() => Name;
}

public static class AdvancedEffectCatalog
{
    public static AdvancedEffectCategory GetCategory(IAdvancedEffectDisplay effect)
    {
        return effect switch
        {
            AdvancedEffectKaraoke or AdvancedEffectFancyKaraoke or AdvancedEffectWordByWord or
                AdvancedEffectTypewriter or AdvancedEffectTypewriterWithHighlight or AdvancedEffectWordFlip3D or
                AdvancedEffectScrambleReveal or AdvancedEffectWordSpacing or AdvancedEffectHighlightBox or
                AdvancedEffectSingleWordPop or AdvancedEffectBoldCaptions or AdvancedEffectSmoothFill or
                AdvancedEffectBlurInWords => AdvancedEffectCategory.Words,
            AdvancedEffectFadeIn or AdvancedEffectFadeOut or AdvancedEffectFadeInOut or AdvancedEffectSlideInLeft or
                AdvancedEffectSlideInRight or AdvancedEffectBounceIn or AdvancedEffectSlowZoomIn or
                AdvancedEffectSlowZoomOut or AdvancedEffectTvClose or AdvancedEffectRollingCaptions => AdvancedEffectCategory.InOut,
            AdvancedEffectSnow or AdvancedEffectRain or AdvancedEffectStarfield or AdvancedEffectFireflies or
                AdvancedEffectConfetti or AdvancedEffectHearts or AdvancedEffectMatrix or
                AdvancedEffectOldMovie => AdvancedEffectCategory.Scenes,
            AdvancedEffectCinematicTitle or AdvancedEffectLowerThird or AdvancedEffectStarWarsScroll or
                AdvancedEffectEndCreditsScroll => AdvancedEffectCategory.Titles,
            _ => AdvancedEffectCategory.TextFx,
        };
    }

    public static EffectPreviewKind GetPreviewKind(IAdvancedEffectDisplay? effect)
    {
        return effect switch
        {
            AdvancedEffectFadeIn => EffectPreviewKind.ScreenFadeIn,
            AdvancedEffectFadeOut => EffectPreviewKind.ScreenFadeOut,
            AdvancedEffectFadeInOut => EffectPreviewKind.FadeInOut,
            AdvancedEffectSlideInLeft => EffectPreviewKind.SlideInLeft,
            AdvancedEffectSlideInRight => EffectPreviewKind.SlideInRight,
            AdvancedEffectBounceIn => EffectPreviewKind.BounceIn,
            AdvancedEffectSlowZoomIn => EffectPreviewKind.ZoomIn,
            AdvancedEffectSlowZoomOut => EffectPreviewKind.ZoomOut,
            AdvancedEffectTvClose => EffectPreviewKind.TvClose,
            AdvancedEffectTypewriter => EffectPreviewKind.Typewriter,
            AdvancedEffectTypewriterWithHighlight => EffectPreviewKind.TypewriterHighlight,
            AdvancedEffectWordByWord => EffectPreviewKind.WordByWord,
            AdvancedEffectKaraoke => EffectPreviewKind.Karaoke,
            AdvancedEffectFancyKaraoke => EffectPreviewKind.WordPop,
            AdvancedEffectHighlightBox => EffectPreviewKind.HighlightBox,
            AdvancedEffectSingleWordPop => EffectPreviewKind.SingleWordPop,
            AdvancedEffectBoldCaptions => EffectPreviewKind.BoldCaptions,
            AdvancedEffectSmoothFill => EffectPreviewKind.SmoothFill,
            AdvancedEffectBlurInWords => EffectPreviewKind.BlurInWords,
            AdvancedEffectRollingCaptions => EffectPreviewKind.RollingCaptions,
            AdvancedEffectScrambleReveal => EffectPreviewKind.ScrambleReveal,
            AdvancedEffectWordFlip3D => EffectPreviewKind.WordFlip3D,
            AdvancedEffectWordSpacing => EffectPreviewKind.WordSpacing,
            AdvancedEffectRainbowPulse => EffectPreviewKind.RainbowPulse,
            AdvancedEffectWave => EffectPreviewKind.Wave,
            AdvancedEffectWaveBlue => EffectPreviewKind.WaveBlue,
            AdvancedEffectGlitch => EffectPreviewKind.Glitch,
            AdvancedEffectNeonBurst => EffectPreviewKind.NeonBurst,
            AdvancedEffectAudioTextPulse => EffectPreviewKind.AudioPulse,
            AdvancedEffectSnow => EffectPreviewKind.Snow,
            AdvancedEffectRain => EffectPreviewKind.Rain,
            AdvancedEffectStarfield => EffectPreviewKind.Starfield,
            AdvancedEffectFireflies => EffectPreviewKind.Fireflies,
            AdvancedEffectConfetti => EffectPreviewKind.Confetti,
            AdvancedEffectHearts => EffectPreviewKind.Hearts,
            AdvancedEffectMatrix => EffectPreviewKind.Matrix,
            AdvancedEffectOldMovie => EffectPreviewKind.OldMovie,
            AdvancedEffectCinematicTitle => EffectPreviewKind.CinematicTitle,
            AdvancedEffectLowerThird => EffectPreviewKind.LowerThird,
            AdvancedEffectStarWarsScroll => EffectPreviewKind.StarWarsScroll,
            AdvancedEffectEndCreditsScroll => EffectPreviewKind.CreditsScroll,
            _ => EffectPreviewKind.None,
        };
    }

    public static AdvancedEffectCategoryItem[] MakeCategories()
    {
        var l = Se.Language.Assa;
        return
        [
            new AdvancedEffectCategoryItem(AdvancedEffectCategory.All, Se.Language.General.All, IconNames.ViewGrid, Color.FromRgb(0x60, 0x6B, 0x7D)),
            new AdvancedEffectCategoryItem(AdvancedEffectCategory.Words, l.AdvancedEffectCategoryWords, IconNames.MusicNote, Color.FromRgb(0xE0, 0x4F, 0x8B)),
            new AdvancedEffectCategoryItem(AdvancedEffectCategory.InOut, l.AdvancedEffectCategoryInOut, IconNames.AnimationPlay, Color.FromRgb(0x2F, 0x8F, 0xE0)),
            new AdvancedEffectCategoryItem(AdvancedEffectCategory.TextFx, l.AdvancedEffectCategoryTextFx, IconNames.Creation, Color.FromRgb(0x9B, 0x59, 0xD6)),
            new AdvancedEffectCategoryItem(AdvancedEffectCategory.Scenes, l.AdvancedEffectCategoryScenes, IconNames.Image, Color.FromRgb(0x1F, 0xA8, 0x7A)),
            new AdvancedEffectCategoryItem(AdvancedEffectCategory.Titles, l.AdvancedEffectCategoryTitles, IconNames.MovieOpenOutline, Color.FromRgb(0xE6, 0x8A, 0x1E)),
        ];
    }
}
