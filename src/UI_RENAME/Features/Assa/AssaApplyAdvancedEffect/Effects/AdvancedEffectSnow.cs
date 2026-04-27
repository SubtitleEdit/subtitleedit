using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect.Effects;

public class AdvancedEffectSnow : IAdvancedEffectDisplay
{
    public string Name => Se.Language.Assa.AdvancedEffectSnow;
    public string Description => Se.Language.Assa.AdvancedEffectSnowDescription;
    public bool UsesAudio => false;
    public int FlakeCount { get; set; } = 200;

    public override string ToString() => Name;

    public List<SubtitleLineViewModel> ApplyEffect(string header, List<SubtitleLineViewModel> subtitles, int width, int height, WavePeakData2? wavePeaks)
    {
        var result = new List<SubtitleLineViewModel>();
        if (subtitles.Count == 0) return result;

        Random rng = new Random(subtitles[0].Text.GetHashCode());

        int screenWidth = width > 0 ? width : 1280;
        int screenHeight = height > 0 ? height : 720;

        var globalStart = subtitles.Min(s => s.StartTime);
        var globalEnd = subtitles.Max(s => s.EndTime);
        double totalVideoMs = (globalEnd - globalStart).TotalMilliseconds;

        for (int i = 0; i < FlakeCount; i++)
        {
            // INITIAL DELAY: This creates the "start slow" effect.
            // Higher index flakes start much later in the video.
            double currentTimeMs = rng.Next(0, 5000) + (i * 20);

            while (currentTimeMs < totalVideoMs)
            {
                var flake = new SubtitleLineViewModel();

                int layerRoll = rng.Next(0, 100);
                int fallDuration, size, alpha, xDrift;
                double glowBorder, glowBlur;

                if (layerRoll < 70) // BACKGROUND - tiny specks, subtle glow
                {
                    fallDuration = rng.Next(10000, 15000);
                    size = rng.Next(10, 25);
                    glowBorder = 0.8;
                    glowBlur = 1.5;
                    alpha = 180;
                    xDrift = rng.Next(-50, 50); // Gentle bidirectional drift
                }
                else if (layerRoll < 95) // MIDGROUND - gentle flakes, soft glow
                {
                    fallDuration = rng.Next(6000, 9000);
                    size = rng.Next(30, 50);
                    glowBorder = 1.5;
                    glowBlur = 2.5;
                    alpha = 100;
                    xDrift = rng.Next(-100, 100); // Moderate wind sway
                }
                else // FOREGROUND - large out-of-focus flakes, bright halo
                {
                    fallDuration = rng.Next(4000, 6000);
                    size = rng.Next(60, 110);
                    glowBorder = 3.0;
                    glowBlur = 6.0;
                    alpha = 60;
                    xDrift = rng.Next(-160, 160); // Strong gusting drift
                }

                flake.StartTime = globalStart.Add(TimeSpan.FromMilliseconds(currentTimeMs));
                flake.EndTime = flake.StartTime.Add(TimeSpan.FromMilliseconds(fallDuration));

                if (flake.StartTime >= globalEnd) break;

                int startX = rng.Next(-100, screenWidth + 100);
                int endX = startX + xDrift;

                // ROTATION: Wide tumble range for natural spinning
                int startRotation = rng.Next(0, 360);
                int endRotation = startRotation + rng.Next(-270, 270);

                // GLOW: white border (\bord + \3c) blurred outward creates a soft luminous halo
                string hexAlpha = alpha.ToString("X2");
                string tags = $@"\\an5\\bord{glowBorder:F1}\\shad0\\3c&HFFFFFF&\\blur{glowBlur:F1}\\1c&HFFFFFF&\\alpha&H{hexAlpha}&\\fad(1200,1200)" +
                              $@"\\fscx{size}\\fscy{size}\\frz{startRotation}\\t(0,{fallDuration},\\frz{endRotation})" +
                              $@"\\move({startX},-50,{endX},{screenHeight + 50})";

                flake.Text = "{" + tags + "}•";
                result.Add(flake);

                currentTimeMs += fallDuration + rng.Next(0, 2000);
            }
        }

        result.AddRange(subtitles);
        return result;
    }
}

