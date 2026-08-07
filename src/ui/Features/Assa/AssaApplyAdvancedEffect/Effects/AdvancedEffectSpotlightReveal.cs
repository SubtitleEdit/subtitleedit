using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect.Effects;

/// <summary>
/// Spotlight reveal: the screen dims under a translucent overlay while a soft light spot
/// sweeps across the (assumed bottom-center) text band; the text itself is revealed by an
/// animated rectangular \clip that follows the sweep, then stays visible.
/// The three events per line sit on distinct ASSA layers so they always stack correctly.
/// </summary>
public class AdvancedEffectSpotlightReveal : IAdvancedEffectDisplay
{
    public string Name => Se.Language.Assa.AdvancedEffectSpotlightReveal;
    public string Description => Se.Language.Assa.AdvancedEffectSpotlightRevealDescription;
    public bool UsesAudio => false;

    public override string ToString() => Name;

    public List<SubtitleLineViewModel> ApplyEffect(
        string header, List<SubtitleLineViewModel> subtitles, int width, int height, WavePeakData2? wavePeaks)
    {
        var result = new List<SubtitleLineViewModel>();
        // \clip, \move and the drawing coordinates are in SCRIPT space (PlayRes), not
        // video pixels - with a mismatched header the beam would render off-screen and
        // the clip sweep would finish almost instantly.
        var (w, h) = AdvancedEffectUtil.GetScriptResolution(header, width, height);

        foreach (var sub in subtitles)
        {
            int durationMs = (int)sub.Duration.TotalMilliseconds;
            if (durationMs <= 0)
            {
                result.Add(AdvancedEffectUtil.PassThrough(sub));
                continue;
            }

            int sweepMs = Math.Max(1, Math.Min(600, durationMs / 3));

            // 1. Dim overlay above the video for the whole line (Layer is a Dialogue field,
            // not an override tag - see FadeOut)
            var overlay = new SubtitleLineViewModel
            {
                StartTime = sub.StartTime,
                EndTime = sub.EndTime,
                Layer = sub.Layer + 1,
                Text = "{\\p1\\an7\\pos(0,0)\\bord0\\shad0\\1c&H000000&\\alpha&H60&\\fad(150,150)}" +
                       $"m 0 0 l {w} 0 l {w} {h} l 0 {h}",
            };
            result.Add(overlay);

            // 2. Soft light spot sweeping across the text band during the reveal
            // (proportional so it stays on the text in small script spaces)
            int beamY = (int)(h * 0.88);
            var beamEnd = sub.StartTime.Add(TimeSpan.FromMilliseconds(Math.Min(durationMs, sweepMs + 300)));
            var beam = new SubtitleLineViewModel
            {
                StartTime = sub.StartTime,
                EndTime = beamEnd,
                Layer = sub.Layer + 2,
                Text = "{\\p1\\an5\\bord0\\shad0\\1c&HFFFFFF&\\alpha&HA8&\\blur28" +
                       $"\\move(-120,{beamY},{w + 120},{beamY},0,{sweepMs})\\fad(80,200)}}" +
                       "m -90 0 b -90 -50 90 -50 90 0 b 90 50 -90 50 -90 0",
            };
            result.Add(beam);

            // 3. The text, revealed left-to-right by an animated rectangular clip
            var text = new SubtitleLineViewModel(sub, generateNewId: true);
            text.Layer = sub.Layer + 3;
            text.Text = $"{{\\clip(0,0,0,{h})\\t(0,{sweepMs},\\clip(0,0,{w},{h}))}}" + sub.Text;
            result.Add(text);
        }

        return result;
    }
}
