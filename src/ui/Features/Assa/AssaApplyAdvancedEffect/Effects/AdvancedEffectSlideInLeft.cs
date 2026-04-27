using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect.Effects;

/// <summary>
/// Slide-in from left + slide-out to left: subtitle flies in from off-screen left,
/// holds at its natural position, then exits back off-screen to the left.
/// </summary>
public class AdvancedEffectSlideInLeft : IAdvancedEffectDisplay
{
    public string Name => Se.Language.Assa.AdvancedEffectSlideInLeft;
    public string Description => Se.Language.Assa.AdvancedEffectSlideInLeftDescription;
    public bool UsesAudio => false;

    public override string ToString() => Name;

    public List<SubtitleLineViewModel> ApplyEffect(string header, List<SubtitleLineViewModel> subtitles, int width, int height, WavePeakData2? wavePeaks)
    {
        var result = new List<SubtitleLineViewModel>();
        if (subtitles.Count == 0)
        {
            return result;
        }

        int cx = width / 2;
        int cy = height - 60;

        foreach (var sub in subtitles)
        {
            int durationMs = (int)sub.Duration.TotalMilliseconds;
            int slideMs = (int)Math.Min(400, durationMs / 3.0);

            // Slide in from left
            var inSub = new SubtitleLineViewModel(sub, generateNewId: true);
            inSub.StartTime = sub.StartTime;
            inSub.EndTime = sub.StartTime + TimeSpan.FromMilliseconds(slideMs);
            inSub.Text = $"{{\\an2\\move(-{width},{cy},{cx},{cy},0,{slideMs})}}" + sub.Text;
            result.Add(inSub);

            // Hold at centre
            var holdSub = new SubtitleLineViewModel(sub, generateNewId: true);
            holdSub.StartTime = sub.StartTime + TimeSpan.FromMilliseconds(slideMs);
            holdSub.EndTime = sub.EndTime - TimeSpan.FromMilliseconds(slideMs);
            holdSub.Text = $"{{\\an2\\pos({cx},{cy})}}" + sub.Text;
            result.Add(holdSub);

            // Slide out to left
            var outSub = new SubtitleLineViewModel(sub, generateNewId: true);
            outSub.StartTime = sub.EndTime - TimeSpan.FromMilliseconds(slideMs);
            outSub.EndTime = sub.EndTime;
            outSub.Text = $"{{\\an2\\move({cx},{cy},-{width},{cy},0,{slideMs})}}" + sub.Text;
            result.Add(outSub);
        }

        return result;
    }
}
