using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect.Effects;

/// <summary>
/// Hearts rain: bezier-drawn hearts in three shape variants fall from the top of the screen,
/// tumbling gently and fading, in layered depths with a red/pink palette.
/// </summary>
public class AdvancedEffectHearts : IAdvancedEffectDisplay
{
    public string Name => Se.Language.Assa.AdvancedEffectHearts;
    public string Description => Se.Language.Assa.AdvancedEffectHeartsDescription;
    public bool UsesAudio => false;
    public int HeartCount { get; set; } = 120;

    public override string ToString() => Name;

    // Three distinct bezier heart shapes, each centered at origin (0,0), ~100-unit bounding box
    private static readonly string[] HeartShapes =
    [
        // Classic
        "m 0 -35 b -14 -52 -52 -52 -52 -25 b -52 0 -28 25 0 50 b 28 25 52 0 52 -25 b 52 -52 14 -52 0 -35",
        // Wide/chubby
        "m 0 -30 b -10 -50 -58 -48 -58 -20 b -58 4 -30 28 0 52 b 30 28 58 4 58 -20 b 58 -48 10 -50 0 -30",
        // Tall/slim
        "m 0 -42 b -12 -58 -46 -55 -46 -26 b -46 -4 -24 22 0 55 b 24 22 46 -4 46 -26 b 46 -55 12 -58 0 -42",
    ];

    // Red and pink palette (ASS BGR format)
    private static readonly string[] HeartColors =
    [
        "&H0000FF&",  // Pure red
        "&H0000CC&",  // Dark red
        "&H3C14DC&",  // Crimson
        "&H5A07FF&",  // Rose red
        "&H9314FF&",  // Deep pink
        "&HB469FF&",  // Hot pink
        "&HC1B6FF&",  // Light pink
        "&H6400E6&",  // Magenta-red
    ];

    public List<SubtitleLineViewModel> ApplyEffect(string header, List<SubtitleLineViewModel> subtitles, int width, int height, WavePeakData2? wavePeaks)
    {
        var result = new List<SubtitleLineViewModel>();
        if (subtitles.Count == 0) return result;

        int w = width > 0 ? width : 1280;
        int h = height > 0 ? height : 720;

        foreach (var sub in subtitles)
        {
            var rng = new Random(sub.Text.GetHashCode() ^ sub.StartTime.GetHashCode());
            double durationMs = sub.Duration.TotalMilliseconds;

            for (int p = 0; p < HeartCount; p++)
            {
                // Power distribution: density starts at ~zero and builds naturally — no burst at t=0
                double launchMs = Math.Pow(rng.NextDouble(), 0.4) * durationMs * 0.75;

                // Depth layer determines size, blur, and opacity
                int layerRoll = rng.Next(0, 100);
                int size, alpha;
                double blur;

                if (layerRoll < 55) // Background — small, faded
                {
                    size = rng.Next(22, 48);
                    blur = 0.3;
                    alpha = 0x70;
                }
                else if (layerRoll < 88) // Midground — standard
                {
                    size = rng.Next(45, 95);
                    blur = 0.0;
                    alpha = 0x18;
                }
                else // Foreground — large, soft blur for depth-of-field look
                {
                    size = rng.Next(90, 148);
                    blur = 1.5;
                    alpha = 0x50;
                }

                int fallDuration = rng.Next(2500, 5500);
                // Hearts pre-roll: start before the subtitle so they're already in motion when
                // the text appears. EndTime is pinned to sub.EndTime so \fad fadeOut finishes
                // exactly when the subtitle text disappears.
                const double preRollMs = 700;
                double heartStartOffset = launchMs - preRollMs;
                double actualDur = durationMs - heartStartOffset;
                if (actualDur < 600) continue;

                var heart = new SubtitleLineViewModel(sub, generateNewId: true);
                heart.StartTime = sub.StartTime.Add(TimeSpan.FromMilliseconds(heartStartOffset));
                heart.EndTime = sub.EndTime;

                // Start above the screen, fall past the bottom
                int startX = rng.Next(-30, w + 30);
                int drift = rng.Next(-90, 91);   // gentle left/right sway
                int endX = startX + drift;
                int startY = rng.Next(-(size * 2), -(size / 2));
                int endY = startY + h + size * 3 + rng.Next(50, 200);

                // Slight tumble — not a full spin, just a small rock
                int startAngle = rng.Next(0, 360);
                int tumble = rng.Next(-60, 61);

                int fadeIn = rng.Next(150, 451);
                int fadeOut = rng.Next(250, 551);

                string color = HeartColors[rng.Next(HeartColors.Length)];
                string shape = HeartShapes[rng.Next(HeartShapes.Length)];
                string hexAlpha = alpha.ToString("X2");
                string blurTag = blur > 0 ? $"\\blur{blur:F1}" : "";

                string tags =
                    $"\\p1\\an5\\1c{color}\\bord0\\shad0{blurTag}\\alpha&H{hexAlpha}&" +
                    $"\\fscx{size}\\fscy{size}" +
                    $"\\frz{startAngle}\\t(0,{(int)actualDur},\\frz{startAngle + tumble})" +
                    $"\\move({startX},{startY},{endX},{endY})" +
                    $"\\fad({fadeIn},{fadeOut})";

                heart.Layer = -1000;
                heart.Text = "{" + tags + "}" + shape;
                result.Add(heart);
            }

            result.Add(sub);
        }

        return result;
    }
}

