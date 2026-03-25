using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect.Effects;

/// <summary>
/// Fireflies effect: warm-yellow glowing dots that drift organically across the screen,
/// pulsing their glow and fading between segments like real fireflies blinking.
/// </summary>
public class AdvancedEffectFireflies : IAdvancedEffectDisplay
{
    public string Name => Se.Language.Assa.AdvancedEffectFireflies;
    public string Description => Se.Language.Assa.AdvancedEffectFirefliesDescription;
    public bool UsesAudio => false;
    public int FireflyCount { get; set; } = 60;

    public override string ToString() => Name;

    // Warm yellow-amber palette (ASS BGR: B=0, G=varies, R=255 = yellow-to-amber range)
    string[] coreColors = { "&H00FFFF&", "&H00E8FF&", "&H00D7FF&", "&H00C0FF&" };

    public List<SubtitleLineViewModel> ApplyEffect(string header, List<SubtitleLineViewModel> subtitles, int width, int height, WavePeakData2? wavePeaks)
    {
        var result = new List<SubtitleLineViewModel>();
        if (subtitles.Count == 0) return result;

        Random rng = new Random(subtitles[0].Text.GetHashCode());

        int w = width > 0 ? width : 1280;
        int h = height > 0 ? height : 720;

        var globalStart = subtitles.Min(s => s.StartTime);
        var globalEnd = subtitles.Max(s => s.EndTime);
        double totalVideoMs = (globalEnd - globalStart).TotalMilliseconds;

        for (int i = 0; i < FireflyCount; i++)
        {
            double currentTimeMs = rng.Next(0, 3000) + (i * 30);

            while (currentTimeMs < totalVideoMs)
            {
                var fly = new SubtitleLineViewModel();

                int layerRoll = rng.Next(0, 100);
                int size, alpha;
                double glowBorder, glowBlur;

                if (layerRoll < 55) // Background — small, dimmer
                {
                    size = rng.Next(8, 20);
                    glowBorder = 1.0;
                    glowBlur = 2.5;
                    alpha = 0xA0;
                }
                else if (layerRoll < 85) // Midground
                {
                    size = rng.Next(22, 42);
                    glowBorder = 2.5;
                    glowBlur = 5.5;
                    alpha = 0x40;
                }
                else // Foreground — large, bright
                {
                    size = rng.Next(45, 75);
                    glowBorder = 4.5;
                    glowBlur = 10.0;
                    alpha = 0x20;
                }

                int flightDuration = rng.Next(1800, 5500);
                fly.StartTime = globalStart.Add(TimeSpan.FromMilliseconds(currentTimeMs));
                fly.EndTime = fly.StartTime.Add(TimeSpan.FromMilliseconds(flightDuration));

                if (fly.StartTime >= globalEnd) break;

                // Organic drift: move to a nearby random position
                int startX = rng.Next(40, w - 40);
                int startY = rng.Next(40, h - 40);
                int endX = Math.Clamp(startX + rng.Next(-200, 201), 40, w - 40);
                int endY = Math.Clamp(startY + rng.Next(-130, 131), 40, h - 40);

                string color = coreColors[rng.Next(coreColors.Length)];
                string hexAlpha = alpha.ToString("X2");

                int fadeDur = rng.Next(400, 900);

                // Glow pulse: blur swells to peak at mid-flight then subsides
                int pulseTime = flightDuration / 2;
                int maxBlur = (int)(glowBlur * 1.7);
                int minBlur = (int)(glowBlur * 0.6);

                string tags =
                    $"\\an5\\bord{glowBorder:F1}\\shad0\\3c&H00FFFF&\\blur{(int)glowBlur}" +
                    $"\\1c{color}\\alpha&H{hexAlpha}&\\fad({fadeDur},{fadeDur})" +
                    $"\\fscx{size}\\fscy{size}" +
                    $"\\move({startX},{startY},{endX},{endY})" +
                    $"\\t(0,{pulseTime},\\blur{maxBlur})\\t({pulseTime},{flightDuration},\\blur{minBlur})";

                fly.Text = "{" + tags + "}•";
                result.Add(fly);

                // Dark pause between segments — the firefly's "off" blink phase
                currentTimeMs += flightDuration + rng.Next(300, 2000);
            }
        }

        result.AddRange(subtitles);
        return result;
    }
}

