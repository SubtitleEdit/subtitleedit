using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect.Effects;

/// <summary>
/// Digital glitch effect: layers brief corruption overlays (white flash, chromatic
/// aberration, horizontal smear, scale jitter) on top of each subtitle line.
/// </summary>
public class AdvancedEffectGlitch : IAdvancedEffectDisplay
{
    public string Name => Se.Language.Assa.AdvancedEffectGlitch;
    public string Description => Se.Language.Assa.AdvancedEffectGlitchDescription;
    public bool UsesAudio => false;

    public override string ToString() => Name;

    public List<SubtitleLineViewModel> ApplyEffect(string header, List<SubtitleLineViewModel> subtitles, int width, int height, WavePeakData2? wavePeaks)
    {
        var result = new List<SubtitleLineViewModel>();
        if (subtitles.Count == 0) return result;

        int w = width > 0 ? width : 1280;
        int h = height > 0 ? height : 720;
        int baseX = w / 2;
        int baseY = h - 20; // standard \an2 bottom-centre position

        foreach (var sub in subtitles)
        {
            var cleanText = Utilities.RemoveSsaTags(sub.Text);
            double totalMs = sub.Duration.TotalMilliseconds;
            var rng = new Random(sub.Text.GetHashCode());

            if (string.IsNullOrEmpty(cleanText))
            {
                result.Add(sub);
                continue;
            }

            // PRIMARY TEXT shake: each frame holds its offset for a random duration (60–350 ms),
            // frames are contiguous so the text is always visible and the rhythm is irregular
            for (double t = 0; t < totalMs;)
            {
                int frameDur = (int)Math.Min(rng.Next(60, 350), totalMs - t);
                if (frameDur <= 0) break;

                var frame = new SubtitleLineViewModel(sub, generateNewId: true);
                frame.StartTime = sub.StartTime.Add(TimeSpan.FromMilliseconds(t));
                frame.EndTime = frame.StartTime.Add(TimeSpan.FromMilliseconds(frameDur));

                int dx = rng.Next(-5, 6); // ±5 px horizontal
                int dy = rng.Next(-4, 5); // ±4 px vertical

                frame.Text = $"{{\\an2\\pos({baseX + dx},{baseY + dy})}}" + cleanText;
                result.Add(frame);

                t += frameDur;
            }

            // GLITCH OVERLAYS — semi-transparent ghosts on top of the shaking text
            int glitchCount = rng.Next(12, 28);
            for (int g = 0; g < glitchCount; g++)
            {
                double glitchStartMs = rng.NextDouble() * (totalMs * 0.95);
                int glitchDurMs = rng.Next(100) < 30 ? rng.Next(20, 50) : rng.Next(60, 200);

                var startTime = sub.StartTime.Add(TimeSpan.FromMilliseconds(glitchStartMs));
                var endTime = startTime.Add(TimeSpan.FromMilliseconds(glitchDurMs));
                if (endTime > sub.EndTime) continue;

                var glitch = new SubtitleLineViewModel(sub, generateNewId: true);
                glitch.StartTime = startTime;
                glitch.EndTime = endTime;

                // Higher alpha = more transparent; overlays are now clearly ghostly
                string alphaHex = rng.Next(0x70, 0xC0).ToString("X2");

                string tags;
                switch (rng.Next(6))
                {
                    case 0: // White flash
                        string flashAlpha = rng.Next(0x55, 0x95).ToString("X2");
                        tags = $"\\1c&HFFFFFF&\\3c&HFFFFFF&\\blur0\\shad0\\alpha&H{flashAlpha}&";
                        break;
                    case 1: // Chromatic aberration: cyan text + red shadow displaced right
                        int xShad = rng.Next(6, 20);
                        tags = $"\\1c&HFFFF00&\\3c&HFFFF00&\\alpha&H{alphaHex}&\\xshad{xShad}\\yshad0\\4c&H0000FF&\\blur0.5";
                        break;
                    case 2: // Horizontal smear
                        tags = $"\\1c&HFFFFFF&\\blur{rng.Next(3, 7)}\\fscx{rng.Next(120, 185)}\\shad0\\alpha&H{alphaHex}&";
                        break;
                    case 3: // Font shear (\fax) — digital skew distortion
                        double shear = rng.NextDouble() * 0.6 - 0.3;
                        tags = $"\\fax{shear:F2}\\1c&HFFFFFF&\\alpha&H{alphaHex}&\\blur0.5\\shad0";
                        break;
                    case 4: // Scale jitter + slight tilt
                        int glitchFrz = rng.Next(-6, 7);
                        tags = $"\\fscx{rng.Next(82, 118)}\\fscy{rng.Next(82, 118)}\\frz{glitchFrz}\\1c&HFFFFFF&\\alpha&H{alphaHex}&\\shad0";
                        break;
                    default: // VHS vertical stretch
                        string stretchAlpha = rng.Next(0x70, 0xA5).ToString("X2");
                        tags = $"\\fscx{rng.Next(90, 106)}\\fscy{rng.Next(130, 210)}\\1c&HFFFFFF&\\alpha&H{stretchAlpha}&\\blur{rng.Next(1, 3)}\\shad0";
                        break;
                }

                glitch.Text = "{" + tags + "}" + cleanText;
                result.Add(glitch);
            }
        }

        return result;
    }
}

