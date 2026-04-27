using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect.Effects;

public class AdvancedEffectOldMovie : IAdvancedEffectDisplay
{
    public string Name => Se.Language.Assa.AdvancedEffectOldMovie;
    public string Description => Se.Language.Assa.AdvancedEffectOldMovieDescription;
    public bool UsesAudio => false;

    public override string ToString() => Name;

    public List<SubtitleLineViewModel> ApplyEffect(string header, List<SubtitleLineViewModel> subtitles, int width, int height, WavePeakData2? wavePeaks)
    {
        var result = new List<SubtitleLineViewModel>();
        if (subtitles.Count == 0) return result;

        Random rng = new Random(subtitles[0].Text.GetHashCode());

        int w = width > 0 ? width : 1280;
        int h = height > 0 ? height : 720;

        var globalStart = subtitles.Min(s => s.StartTime);
        var globalEnd = subtitles.Max(s => s.EndTime);
        double totalMs = (globalEnd - globalStart).TotalMilliseconds;

        // --- 1. HEAVY FILM NOISE (GRAIN) ---
        // High particle count (40) with very short lifespans (33ms = 30fps)
        for (int i = 0; i < 40; i++)
        {
            double currentTimeMs = rng.Next(0, 1000);
            while (currentTimeMs < totalMs)
            {
                var grain = new SubtitleLineViewModel(); // Using requested constructor
                int life = 33;
                grain.StartTime = globalStart.Add(TimeSpan.FromMilliseconds(currentTimeMs));
                grain.EndTime = grain.StartTime.Add(TimeSpan.FromMilliseconds(life));

                int x = rng.Next(0, w);
                int y = rng.Next(0, h);
                // Mix of black and dark grey for "gritty" noise
                string color = rng.Next(0, 2) == 0 ? "&H000000&" : "&H333333&";

                grain.Text = "{\\an5\\pos(" + x + "," + y + ")\\bord0\\shad0\\1c" + color + "\\alpha&H90&\\fs" + rng.Next(2, 8) + "}·";

                result.Add(grain);
                currentTimeMs += life + rng.Next(10, 100);
            }
        }

        // --- 2. IMPERFECT/BROKEN SCRATCHES ---
        for (int i = 0; i < 6; i++)
        {
            double currentTimeMs = rng.Next(0, 5000);
            while (currentTimeMs < totalMs)
            {
                var scratch = new SubtitleLineViewModel();
                int life = rng.Next(50, 150);
                scratch.StartTime = globalStart.Add(TimeSpan.FromMilliseconds(currentTimeMs));
                scratch.EndTime = scratch.StartTime.Add(TimeSpan.FromMilliseconds(life));

                int x = rng.Next(50, w - 50);
                int startY = rng.Next(-100, h / 2);
                int endY = startY + rng.Next(h / 2, h + 200);

                // Slightly crooked vertical line
                string sDraw = $"m 0 0 l 1 0 l {rng.Next(0, 3)} {h} l {rng.Next(-2, 1)} {h}";
                scratch.Text = "{\\p1\\an7\\pos(" + x + "," + startY + ")\\bord0\\shad0\\1c&H666666&\\alpha&HA0&\\blur1}" + sDraw;

                result.Add(scratch);
                currentTimeMs += life + rng.Next(1000, 4000);
            }
        }

        // --- 3. SLOW GATE FLICKER ---
        double flickerTime = 0;
        while (flickerTime < totalMs)
        {
            var flicker = new SubtitleLineViewModel();
            int life = rng.Next(180, 400);
            flicker.StartTime = globalStart.Add(TimeSpan.FromMilliseconds(flickerTime));
            flicker.EndTime = flicker.StartTime.Add(TimeSpan.FromMilliseconds(life));

            string alpha = rng.Next(242, 252).ToString("X2");
            flicker.Text = "{\\p1\\an7\\pos(0,0)\\bord0\\shad0\\1c&HFFFFFF&\\alpha&H" + alpha + "}" + $"m 0 0 l {w} 0 l {w} {h} l 0 {h}";

            result.Add(flicker);
            flickerTime += life;
        }

        // --- 4. VIGNETTE ---
        var vignette = new SubtitleLineViewModel() { StartTime = globalStart, EndTime = globalEnd };
        string vDraw = $"m 0 0 l {w} 0 l {w} {h} l 0 {h} l 0 0 m 180 180 l {w - 180} 180 l {w - 180} {h - 180} l 180 {h - 180} l 180 180";
        vignette.Text = "{\\p1\\an7\\pos(0,0)\\bord0\\shad0\\1c&H000000&\\alpha&HA0&\\be90}" + vDraw;
        result.Add(vignette);

        result.AddRange(subtitles);
        return result;
    }
}

