using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect.Effects;

/// <summary>
/// Confetti burst: explosive spray of colorful spinning paper pieces that erupt from
/// corner poppers and a center starburst at the start of each subtitle.
/// </summary>
public class AdvancedEffectConfetti : IAdvancedEffectDisplay
{
    public string Name => Se.Language.Assa.AdvancedEffectConfetti;
    public string Description => Se.Language.Assa.AdvancedEffectConfettiDescription;
    public bool UsesAudio => false;
    public int PieceCount { get; set; } = 150;

    public override string ToString() => Name;

    // Vivid confetti palette (ASS BGR format)
    private static readonly string[] ConfettiColors =
    [
        "&H0000FF&",  // Red
        "&H001ECC&",  // Crimson
        "&HFF0000&",  // Blue
        "&HCC1400&",  // Deep blue
        "&H00FF00&",  // Green
        "&H00CC22&",  // Forest green
        "&H00FFFF&",  // Yellow
        "&H00BBFF&",  // Amber
        "&H0066FF&",  // Orange
        "&H0033EE&",  // Deep orange
        "&HFF00FF&",  // Magenta
        "&HCC00FF&",  // Hot pink
        "&H8800EE&",  // Rose
        "&HFFFF00&",  // Cyan
        "&HFF8800&",  // Teal
        "&H00D455&",  // Lime
        "&H00FF88&",  // Spring green
        "&H9900CC&",  // Violet
        "&HFF0088&",  // Blue-violet
        "&HFFFFFF&",  // White
    ];

    // Shapes: rectangle, diamond, circle — all widely supported in subtitle fonts
    private static readonly string[] ConfettiChars = ["■", "◆", "●"];

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

            // Randomly generate 3–6 origins per subtitle; each is drawn from one of five
            // placement categories so the burst composition is unique every time.
            int originCount = rng.Next(3, 7);
            var originList = new List<(int X, int Y, double AngleMin, double AngleMax, int Weight)>(originCount);

            for (int o = 0; o < originCount; o++)
            {
                int type = rng.Next(5);
                int ox, oy, weight;
                double aMin, aMax;

                if (type == 0) // Bottom-edge popper — upward fan
                {
                    ox = rng.Next(w / 10, 9 * w / 10);
                    oy = h + rng.Next(5, 25);
                    aMin = -175.0 + rng.NextDouble() * 20;
                    aMax = aMin + rng.Next(130, 170);
                    weight = rng.Next(18, 38);
                }
                else if (type == 1) // Left-edge launcher — rightward fan
                {
                    ox = rng.Next(-25, 10);
                    oy = rng.Next(h / 6, 5 * h / 6);
                    aMin = -100.0 + rng.NextDouble() * 20;
                    aMax = aMin + rng.Next(160, 200);
                    weight = rng.Next(12, 28);
                }
                else if (type == 2) // Right-edge launcher — leftward fan
                {
                    ox = w + rng.Next(-10, 25);
                    oy = rng.Next(h / 6, 5 * h / 6);
                    aMin = 80.0 + rng.NextDouble() * 20;
                    aMax = aMin + rng.Next(160, 200);
                    weight = rng.Next(12, 28);
                }
                else if (type == 3) // Mid-screen starburst — fully radial
                {
                    ox = rng.Next(w / 5, 4 * w / 5);
                    oy = rng.Next(h / 6, 3 * h / 4);
                    aMin = 0.0;
                    aMax = 360.0;
                    weight = rng.Next(15, 32);
                }
                else // Top-edge drop — downward fan
                {
                    ox = rng.Next(w / 8, 7 * w / 8);
                    oy = rng.Next(-20, 15);
                    aMin = 20.0 + rng.NextDouble() * 20;
                    aMax = aMin + rng.Next(100, 140);
                    weight = rng.Next(8, 20);
                }

                originList.Add((ox, oy, aMin, aMax, weight));
            }

            var origins = originList.ToArray();
            int totalWeight = origins.Sum(o => o.Weight);

            for (int p = 0; p < PieceCount; p++)
            {
                // Weighted origin pick
                int roll = rng.Next(totalWeight);
                int cumulative = 0;
                int oi = 0;
                for (; oi < origins.Length - 1; oi++)
                {
                    cumulative += origins[oi].Weight;
                    if (roll < cumulative) break;
                }
                var (bx, by, aMin, aMax, _) = origins[oi];

                // Staggered launch: most pieces fire in first 250 ms, a few trail up to 450 ms
                double delay = Math.Pow(rng.NextDouble(), 1.6) * 450;
                double pieceDur = durationMs - delay;
                if (pieceDur < 300) continue;

                var piece = new SubtitleLineViewModel(sub, generateNewId: true);
                piece.StartTime = sub.StartTime.Add(TimeSpan.FromMilliseconds(delay));
                piece.EndTime = sub.EndTime;

                double angleRad = (aMin + rng.NextDouble() * (aMax - aMin)) * Math.PI / 180.0;

                // Power-biased travel: most pieces reach mid-distance, a few fly very far
                double travelFactor = Math.Pow(rng.NextDouble(), 0.65);
                int travel = (int)(h * 0.15 + travelFactor * h * 1.0);

                // Gravity bias: squared random so many pieces arc high but some plummet
                double gravFactor = rng.NextDouble() * rng.NextDouble();
                int gravity = (int)(gravFactor * h * 0.75);

                int endX = bx + (int)(Math.Cos(angleRad) * travel);
                int endY = by + (int)(Math.Sin(angleRad) * travel) + gravity;

                string color = ConfettiColors[rng.Next(ConfettiColors.Length)];
                string ch = ConfettiChars[rng.Next(ConfettiChars.Length)];

                // Varied aspect ratios: wide rectangles, near-squares, thin strips
                int sizeX = rng.Next(55, 290);
                int sizeY = rng.Next(18, 95);

                // 1–4 full rotations, random direction and speed
                int startAngle = rng.Next(0, 360);
                int spin = rng.Next(1, 5) * 360 * (rng.Next(2) == 0 ? 1 : -1);

                // Move ends at 65–100 % of flight time; piece lingers at landing spot then fades
                int moveDur = (int)(pieceDur * (0.65 + rng.NextDouble() * 0.35));

                int fs = rng.Next(8, 24);
                int fadeOut = (int)(pieceDur * (0.28 + rng.NextDouble() * 0.30));

                // ~1 in 5 pieces gets a soft blur for a depth-of-field look
                string blurTag = rng.Next(5) == 0 ? $"\\blur{rng.Next(1, 4)}" : "";

                // ~1 in 8 pieces is slightly transparent
                string alphaTag = rng.Next(8) == 0 ? $"\\alpha&H{rng.Next(0x30, 0x90):X2}&" : "";

                string tags =
                    $"\\an5\\1c{color}\\bord0\\shad0\\fs{fs}{blurTag}{alphaTag}" +
                    $"\\fscx{sizeX}\\fscy{sizeY}" +
                    $"\\frz{startAngle}\\t(0,{(int)pieceDur},\\frz{startAngle + spin})" +
                    $"\\move({bx},{by},{endX},{endY},0,{moveDur})" +
                    $"\\fad(50,{fadeOut})";

                piece.Text = "{" + tags + "}" + ch;
                result.Add(piece);
            }

            result.Add(sub);
        }

        return result;
    }
}

