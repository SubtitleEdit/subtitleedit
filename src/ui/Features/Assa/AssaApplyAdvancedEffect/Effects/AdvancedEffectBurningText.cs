using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect.Effects;

/// <summary>
/// Burning text: the line flickers through fire colors (yellow/orange/red) with a glowing
/// halo whose blur and border pulse irregularly, while small ember particles rise from the
/// text area, shrink and fade out. Deterministic per line so the preview does not churn.
/// </summary>
public class AdvancedEffectBurningText : IAdvancedEffectDisplay
{
    public string Name => Se.Language.Assa.AdvancedEffectBurningText;
    public string Description => Se.Language.Assa.AdvancedEffectBurningTextDescription;
    public bool UsesAudio => false;
    public int EmberCount { get; set; } = 26;

    public override string ToString() => Name;

    // Fire palette (ASS BGR)
    private const string Yellow = "&H00FFFF&";
    private const string Red = "&H0000FF&";

    private static readonly string[] FlickerColors =
    [
        "&H00FFFF&", // yellow
        "&H00CCFF&", // amber
        "&H0066FF&", // orange
        "&H0033FF&", // deep orange
    ];

    private static readonly string[] EmberColors =
    [
        "&H00FFFF&", // yellow
        "&H00AAFF&", // amber
        "&H0055FF&", // orange
    ];

    public List<SubtitleLineViewModel> ApplyEffect(
        string header, List<SubtitleLineViewModel> subtitles, int width, int height, WavePeakData2? wavePeaks)
    {
        var result = new List<SubtitleLineViewModel>();
        int w = width > 0 ? width : 1280;
        int h = height > 0 ? height : 720;

        foreach (var sub in subtitles)
        {
            var cleanText = Utilities.RemoveSsaTags(sub.Text)
                .Replace("\r\n", "\\N").Replace("\r", "\\N").Replace("\n", "\\N").Trim();
            double durationMs = sub.Duration.TotalMilliseconds;
            if (string.IsNullOrEmpty(cleanText) || durationMs <= 0)
            {
                result.Add(AdvancedEffectUtil.PassThrough(sub));
                continue;
            }

            var rng = new Random(sub.Text.GetHashCode() ^ sub.StartTime.GetHashCode());

            // ── The burning text line: chained \t color/blur flicker ────────────────
            var burning = new SubtitleLineViewModel(sub, generateNewId: true);
            var tags = new StringBuilder("{\\1c" + Yellow + "\\3c" + Red + "\\bord2\\shad0\\blur2");

            // ~150 ms flicker steps, capped so very long lines do not explode the tag chain
            int steps = (int)Math.Clamp(durationMs / 150, 1, 48);
            double stepMs = durationMs / steps;
            for (int s = 0; s < steps; s++)
            {
                int t0 = (int)Math.Round(s * stepMs);
                int t1 = (int)Math.Round((s + 1) * stepMs);
                // Alternate back towards yellow so the flicker reads as a flame, not a fade
                string color = s % 2 == 0 ? FlickerColors[rng.Next(FlickerColors.Length)] : Yellow;
                tags.Append($"\\t({t0},{t1},\\1c{color}\\blur{rng.Next(1, 4)}\\bord{rng.Next(1, 3)})");
            }
            tags.Append('}');

            burning.Text = tags + cleanText;
            result.Add(burning);

            // ── Rising embers above the (assumed bottom-center) text area ───────────
            for (int p = 0; p < EmberCount && result.Count < AdvancedEffectUtil.MaxGeneratedEvents; p++)
            {
                // Power distribution: embers keep spawning through most of the line
                double launchMs = Math.Pow(rng.NextDouble(), 0.6) * durationMs * 0.85;
                int flightMs = rng.Next(900, 2200);

                var start = sub.StartTime.Add(TimeSpan.FromMilliseconds(launchMs));
                var end = start.Add(TimeSpan.FromMilliseconds(flightMs));
                if (end > sub.EndTime)
                {
                    end = sub.EndTime;
                }
                double actualFlightMs = (end - start).TotalMilliseconds;
                if (actualFlightMs < 250)
                {
                    continue;
                }

                int baseX = w / 2 + rng.Next(-w / 5, w / 5 + 1);
                int baseY = h - 40 - rng.Next(0, 61);
                int drift = rng.Next(-60, 61);
                int rise = rng.Next(140, 381);
                int size = rng.Next(14, 43);
                int fadeOut = rng.Next(300, 601);
                string color = EmberColors[rng.Next(EmberColors.Length)];

                var ember = new SubtitleLineViewModel(sub, generateNewId: true);
                ember.StartTime = start;
                ember.EndTime = end;
                ember.Text =
                    $"{{\\an5\\bord0\\shad0\\blur1\\1c{color}\\alpha&H40&" +
                    $"\\fscx{size}\\fscy{size}" +
                    $"\\move({baseX},{baseY},{baseX + drift},{baseY - rise})" +
                    $"\\fad(120,{fadeOut})" +
                    $"\\t(0,{(int)actualFlightMs},\\fscx{size / 2}\\fscy{size / 2})}}•";
                result.Add(ember);
            }
        }

        return result;
    }
}
