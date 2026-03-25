using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect.Effects;

public class AdvancedEffectFadeIn : IAdvancedEffectDisplay
{
    public string Name => Se.Language.Assa.AdvancedEffectFadeIn;
    public string Description => Se.Language.Assa.AdvancedEffectFadeInDescription;
    public bool UsesAudio => false;

    public List<SubtitleLineViewModel> ApplyEffect(string header, List<SubtitleLineViewModel> subtitles, int width, int height, WavePeakData2? wavePeaks)
    {
        var result = new List<SubtitleLineViewModel>();
        if (subtitles.Count == 0) return result;

        int w = width > 0 ? width : 1280;
        int h = height > 0 ? height : 720;

        foreach (var sub in subtitles)
        {
            result.Add(CreateLineFadeIn(sub.StartTime, sub.Duration.TotalMilliseconds, w, h));
            result.Add(sub);
        }

        return result;
    }

    private SubtitleLineViewModel CreateLineFadeIn(TimeSpan lineStart, double durationMs, int w, int h)
    {
        var fadeIn = new SubtitleLineViewModel();
        fadeIn.StartTime = lineStart;
        // The box only needs to exist for the duration of the fade
        fadeIn.EndTime = lineStart.Add(TimeSpan.FromMilliseconds(durationMs));

        // Vector rectangle covering the whole screen
        string drawBox = $"m 0 0 l {w} 0 l {w} {h} l 0 {h}";

        // \fad(0, durationMs) makes the black box go from Opaque (0) to Transparent (durationMs)
        fadeIn.Text = "{\\p1\\an7\\pos(0,0)\\bord0\\shad0\\1c&H000000&\\fad(0," + durationMs + ")}" + drawBox;

        return fadeIn;
    }
}

