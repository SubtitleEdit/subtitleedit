using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect.Effects;

public class AdvancedEffectFadeOut : IAdvancedEffectDisplay
{
    public string Name => Se.Language.Assa.AdvancedEffectFadeOut;
    public string Description => Se.Language.Assa.AdvancedEffectFadeOutDescription;
    public bool UsesAudio => false;

    public override string ToString() => Name;

    public List<SubtitleLineViewModel> ApplyEffect(string header, List<SubtitleLineViewModel> subtitles, int width, int height, WavePeakData2? wavePeaks)
    {
        var result = new List<SubtitleLineViewModel>();
        if (subtitles.Count == 0) return result;

        int w = width > 0 ? width : 1280;
        int h = height > 0 ? height : 720;

        foreach (var sub in subtitles)
        {
            result.Add(CreateBlackOverlay(sub.StartTime, sub.Duration.TotalMilliseconds, w, h));
            result.Add(sub);
        }

        return result;
    }

    private SubtitleLineViewModel CreateBlackOverlay(TimeSpan lineStart, double durationMs, int w, int h)
    {
        var fadeOut = new SubtitleLineViewModel();
        fadeOut.StartTime = lineStart;
        fadeOut.EndTime = lineStart.Add(TimeSpan.FromMilliseconds(durationMs));

        // Vector rectangle covering the whole screen
        string drawBox = $"m 0 0 l {w} 0 l {w} {h} l 0 {h}";

        fadeOut.Text = "{\\p1\\an7\\pos(0,0)\\bord0\\shad0\\1c&H000000&\\layer10\\fad(" + durationMs + ",0)}" + drawBox;

        return fadeOut;
    }
}

