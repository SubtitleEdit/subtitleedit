using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect.Effects;

/// <summary>
/// Fade-in / fade-out: subtitle fades in at the start and fades out at the end
/// using the ASS \fad tag applied directly to each subtitle line.
/// </summary>
public class AdvancedEffectFadeInOut : IAdvancedEffectDisplay
{
    public string Name => Se.Language.Assa.AdvancedEffectFadeInOut;
    public string Description => Se.Language.Assa.AdvancedEffectFadeInOutDescription;
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
            int durationMs = (int)sub.Duration.TotalMilliseconds;
            int fadeMs = (int)Math.Min(500, durationMs / 3.0);

            var fadeSub = new SubtitleLineViewModel(sub, generateNewId: true);
            fadeSub.Text = $"{{\\fad({fadeMs},{fadeMs})}}" + sub.Text;
            result.Add(fadeSub);
        }

        return result;
    }
}
