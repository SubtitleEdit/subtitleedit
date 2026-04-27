using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect.Effects;

public class AdvancedEffectRain : IAdvancedEffectDisplay
{
    public string Name => Se.Language.Assa.AdvancedEffectRain;
    public string Description => Se.Language.Assa.AdvancedEffectRainDescription;
    public bool UsesAudio => false;

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

        int dropCount = 350;

        for (int i = 0; i < dropCount; i++)
        {
            double currentTimeMs = 0;
            while (currentTimeMs < totalVideoMs)
            {
                var drop = new SubtitleLineViewModel();

                // --- 3D LAYER LOGIC ---
                int layerRoll = rng.Next(0, 100);
                int fallDuration, dropWidth, dropLength, alpha;
                double blur;
                string color;

                if (layerRoll < 60) // BACKGROUND (60% of rain) - Tiny, slow, sharp
                {
                    fallDuration = rng.Next(2500, 3500);
                    dropWidth = rng.Next(4, 8);
                    dropLength = rng.Next(80, 150);
                    blur = 0.2;
                    alpha = 180; // Very transparent (&HB4&)
                    color = "&HA0A0A0&"; // Darker Grey
                }
                else if (layerRoll < 90) // MIDGROUND (30% of rain) - Standard
                {
                    fallDuration = rng.Next(1200, 1800);
                    dropWidth = rng.Next(10, 18);
                    dropLength = rng.Next(200, 350);
                    blur = 0.8;
                    alpha = 100; // Semi-transparent (&H64&)
                    color = "&HD0D0D0&"; // Mid Grey
                }
                else // FOREGROUND (10% of rain) - Huge, fast, blurry (The "Close" drops)
                {
                    fallDuration = rng.Next(600, 900);
                    dropWidth = rng.Next(25, 45);
                    dropLength = rng.Next(500, 800);
                    blur = 3.5; // Heavy "Out of focus" look
                    alpha = 60; // Solid but soft (&H3C&)
                    color = "&HFFFFFF&"; // Bright White
                }

                drop.StartTime = globalStart.Add(TimeSpan.FromMilliseconds(currentTimeMs));
                drop.EndTime = drop.StartTime.Add(TimeSpan.FromMilliseconds(fallDuration));

                if (drop.StartTime >= globalEnd) break;

                int startX = rng.Next(-100, screenWidth + 100);
                int endX = startX + rng.Next(15, 40);

                // --- TAG CONSTRUCTION ---
                string hexAlpha = alpha.ToString("X2");
                string tags = $@"\\an5\\bord0\\shad0\\blur{blur:F1}\\1c{color}\\alpha&H{hexAlpha}&\\fad(150,150)" +
                              $@"\\fscx{dropWidth}\\fscy{dropLength}" +
                              $@"\\move({startX}, -100, {endX}, {screenHeight + 100})";

                drop.Text = "{" + tags + "}·";
                result.Add(drop);

                currentTimeMs += (fallDuration * rng.NextDouble());
            }
        }

        result.AddRange(subtitles);
        return result;
    }
}

