using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect.Effects;

public class AdvancedEffectTvClose : IAdvancedEffectDisplay
{
    public string Name => Se.Language.Assa.AdvancedEffectTvClose;
    public string Description => Se.Language.Assa.AdvancedEffectTvCloseDescription;
    public bool UsesAudio => false;

    public override string ToString() => Name;

    public List<SubtitleLineViewModel> ApplyEffect(string header, List<SubtitleLineViewModel> subtitles, int width, int height, WavePeakData2? wavePeaks)
    {
        var result = new List<SubtitleLineViewModel>();
        if (subtitles.Count == 0)
        {
            return result;
        }

        var w = width > 0 ? width : 1280;
        var h = height > 0 ? height : 720;

        foreach (var sub in subtitles)
        {
            var durationMs = sub.Duration.TotalMilliseconds;
            var halfMs = (int)(durationMs / 2);
            // Ceiling division so the two bars always meet with no gap.
            var halfH = (h + 1) / 2;

            // Middle white band (layer 5): starts transparent and fades to white while shrinking to center.
            // It ends at halfMs exactly when the two black bars meet.
            result.Add(CreateWhiteOverlay(sub.StartTime, halfMs, w, h, halfH));

            // Top black bar (layer 10): grows down from screen top to center in first half.
            result.Add(CreateBar(sub.StartTime, durationMs, halfMs, w, h,
                startY: -h, endY: -(h - halfH)));

            // Bottom black bar (layer 10): grows up from screen bottom to center in first half.
            result.Add(CreateBar(sub.StartTime, durationMs, halfMs, w, h,
                startY: h, endY: halfH));

            result.Add(sub);
        }

        return result;
    }

    private static SubtitleLineViewModel CreateWhiteOverlay(TimeSpan start, int halfMs, int w, int h, int halfH)
    {
        var overlay = new SubtitleLineViewModel();
        overlay.StartTime = start;
        overlay.Layer = 5;
        // Ends at halfMs — cannot bleed into the solid-black second half.
        overlay.EndTime = start.Add(TimeSpan.FromMilliseconds(halfMs));

        string drawBox = $"m 0 0 l {w} 0 l {w} {h} l 0 {h}";

        // Middle band = dynamic clip between the two bar edges.
        // At t=0 clip is full frame; at t=halfMs it collapses to the center line.
        overlay.Text = $"{{\\p1\\an7\\pos(0,0)\\bord0\\shad0\\1c&HFFFFFF&\\alpha&HFF&" +
                       $"\\clip(0,0,{w},{h})" +
                       $"\\t(0,{halfMs},\\clip(0,{halfH},{w},{halfH}))" +
                       $"\\t(0,{halfMs},\\alpha&H00&)}}" + drawBox;

        return overlay;
    }

    private static SubtitleLineViewModel CreateBar(TimeSpan start, double durationMs, int halfMs,
        int w, int barH, int startY, int endY)
    {
        var bar = new SubtitleLineViewModel();
        bar.StartTime = start;
        bar.Layer = 10;
        bar.EndTime = start.Add(TimeSpan.FromMilliseconds(durationMs));

        string drawBox = $"m 0 0 l {w} 0 l {w} {barH} l 0 {barH}";
        // No \layer tag — insertion order keeps this on top of the white overlay.
        // \move(x1,y1,x2,y2,t1,t2): animates during [0,halfMs], stays at (x2,y2) afterward.
        bar.Text = $"{{\\p1\\an7\\move(0,{startY},0,{endY},0,{halfMs})\\bord0\\shad0\\1c&H000000&}}" + drawBox;

        return bar;
    }
}

