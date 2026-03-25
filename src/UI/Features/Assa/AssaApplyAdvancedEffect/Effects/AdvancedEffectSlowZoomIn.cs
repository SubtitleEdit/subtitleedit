using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect.Effects;

/// <summary>
/// Slow zoom-in: each subtitle starts at 100% scale and smoothly grows to 102%
/// over the full subtitle duration — a barely-noticeable creeping zoom.
/// </summary>
public class AdvancedEffectSlowZoomIn : IAdvancedEffectDisplay
{
    public string Name => Se.Language.Assa.AdvancedEffectSlowZoomIn;
    public string Description => Se.Language.Assa.AdvancedEffectSlowZoomInDescription;
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
            newSub.Text = $"{{\\fscx100\\fscy100\\t(0,{durationMs},\\fscx110\\fscy110)}}" + sub.Text;
            result.Add(newSub);
        }

        return result;
    }
}

