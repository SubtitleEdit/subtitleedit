using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect.Effects;

/// <summary>
/// Slow zoom-out: each subtitle starts at a large scale (derived from the style font size)
/// and smoothly eases down to 100% over the full subtitle duration.
/// </summary>
public class AdvancedEffectSlowZoomOut : IAdvancedEffectDisplay
{
    public string Name => Se.Language.Assa.AdvancedEffectSlowZoomOut;
    public string Description => Se.Language.Assa.AdvancedEffectSlowZoomOutDescription;
    public bool UsesAudio => false;

    public override string ToString() => Name;

    public List<SubtitleLineViewModel> ApplyEffect(string header, List<SubtitleLineViewModel> subtitles, int width, int height, WavePeakData2? wavePeaks)
    {
        var result = new List<SubtitleLineViewModel>();
        if (subtitles.Count == 0)
        {
            return result;
        }

        foreach (var sub in subtitles)
        {
            var newSub = new SubtitleLineViewModel(sub, generateNewId: true);
            int durationMs = (int)sub.Duration.TotalMilliseconds;
            newSub.Text = $"{{\\fscx110\\fscy110\\t(0,{durationMs},\\fscx100\\fscy100)}}" + sub.Text;
            result.Add(newSub);
        }

        return result;
    }
}

