using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect.Effects;

public class AdvancedEffectStarfield : IAdvancedEffectDisplay
{
    public string Name => Se.Language.Assa.AdvancedEffectStarfield;
    public string Description => Se.Language.Assa.AdvancedEffectStarfieldDescription;
    public bool UsesAudio => false;
    public int StarCount { get; set; } = 650;
    public decimal SpeedMultiplier { get; set; } = 1.0m;

    public override string ToString() => Name;

    public List<SubtitleLineViewModel> ApplyEffect(string header, List<SubtitleLineViewModel> subtitles, int width, int height, WavePeakData2? wavePeaks)
    {
        var result = new List<SubtitleLineViewModel>();
        if (subtitles.Count == 0) return result;

        Random rng = new Random(subtitles[0].Text.GetHashCode());

        int w = width > 0 ? width : 1280;
        int h = height > 0 ? height : 720;
        int cx = w / 2;
        int cy = h / 2;

        var globalStart = subtitles.Min(s => s.StartTime);
        var globalEnd = subtitles.Max(s => s.EndTime);
        double totalMs = (globalEnd - globalStart).TotalMilliseconds;

        for (int i = 0; i < StarCount; i++)
        {
            double startMs = rng.NextDouble() * totalMs;
            // INCREASED LIFESPAN: Ensures they stay on screen longer
            int life = (int)(rng.Next(5000, 19000) / (double)SpeedMultiplier);

            var star = new SubtitleLineViewModel();
            star.StartTime = globalStart.Add(TimeSpan.FromMilliseconds(startMs));
            star.EndTime = star.StartTime.Add(TimeSpan.FromMilliseconds(life));

            double angle = rng.NextDouble() * 2 * Math.PI;
            // Faster stars need to live shorter, slower stars live longer
            double speedMult = 0.4 + (rng.NextDouble() * 0.8);
            int maxDist = (int)(Math.Max(w, h) * 1.5); // Travel further off-screen

            int startX = cx + rng.Next(-3, 3);
            int startY = cy + rng.Next(-3, 3);
            int endX = cx + (int)(Math.Cos(angle) * maxDist * speedMult);
            int endY = cy + (int)(Math.Sin(angle) * maxDist * speedMult);

            string[] palette = { "&HFFFFFF&", "&HFFEBDA&", "&HDAEBFF&", "&HFFF0F5&", "&HFFFAF0&" };
            string color = palette[rng.Next(palette.Length)];

            string shape = "m 0 0 b 0 5 5 10 10 10 b 15 10 20 5 20 0 b 20 -5 15 -10 10 -10 b 5 -10 0 -5 0 0";

            double sizeFactor = Math.Pow(rng.NextDouble(), 2);
            int baseSize = 12 + (int)(sizeFactor * 14);

            int fadeInDuration = (int)(life * 0.3);
            int fadeOutStart = (int)(life * 0.8); // Start fading out at 80% of life

            // \fad(fadeIn, fadeOut) is the most reliable way to prevent "popping"
            string tags = $@"\\p1\\an5\\bord0\\shad0\\blur1.8\\1c{color}\\move({startX},{startY},{endX},{endY})\\fad(800,800)";

            // Initial setup
            tags += $@"\\fscx{baseSize / 2}\\fscy{baseSize / 2}";

            // Growth over full life
            tags += $@"\\t(0,{life},\\fscx{baseSize * 2.5}\\fscy{baseSize * 2.5})";

            // Twinkle (1 in 8)
            if (rng.Next(0, 8) == 0)
            {
                int tStep = life / 5;
                tags += $@"\\t({tStep},{tStep * 2},\\alpha&H66&)\\t({tStep * 2},{tStep * 3},\\alpha&H00&)";
            }

            star.Text = "{" + tags + "}" + shape;
            result.Add(star);
        }

        result.AddRange(subtitles);
        return result;
    }
}

