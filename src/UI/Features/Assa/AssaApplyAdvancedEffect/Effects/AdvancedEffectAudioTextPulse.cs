using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect.Effects;

/// <summary>
/// Audio-reactive subtitle glow: the subtitle text border, blur and colour pulse with the
/// audio volume. Quiet sections show normal text; loud peaks add a bright coloured halo
/// and a slight scale-up, so the text visually "breathes" with the soundtrack.
/// </summary>
public class AdvancedEffectAudioTextPulse : IAdvancedEffectDisplay
{
    public string Name => Se.Language.Assa.AdvancedEffectAudioPulse;
    public string Description => Se.Language.Assa.AdvancedEffectAudioPulseDescription;
    public bool UsesAudio => true;

    public override string ToString() => Name;

    public List<SubtitleLineViewModel> ApplyEffect(
        string header, List<SubtitleLineViewModel> subtitles, int width, int height, WavePeakData2? wavePeaks)
    {
        var result = new List<SubtitleLineViewModel>();
        if (subtitles.Count == 0) return result;

        const int frameMs = 50; // 20 fps

        foreach (var sub in subtitles)
        {
            var cleanText = Utilities.RemoveSsaTags(sub.Text)
                .Replace("\r\n", "\\N").Replace("\r", "\\N").Replace("\n", "\\N").Trim();
            if (string.IsNullOrEmpty(cleanText))
            {
                result.Add(sub);
                continue;
            }

            double durationMs = sub.Duration.TotalMilliseconds;

            // Normalise against the loudest peak in the neighbourhood of this subtitle
            double localPeak = ComputeLocalPeak(wavePeaks,
                sub.StartTime.TotalSeconds - 1.0,
                sub.EndTime.TotalSeconds + 1.0);
            if (localPeak <= 0) localPeak = 1;

            for (double t = 0; t < durationMs; t += frameMs)
            {
                double frameCenterSec = (sub.StartTime.TotalMilliseconds + t) / 1000.0;
                double amp = GetAverageAmplitude(wavePeaks, frameCenterSec, 0.05, localPeak);

                // Scale: 100 % at rest → 115 % at peak (subtle size pulse)
                int scale = 100 + (int)(amp * 15);

                // Border thickness grows with volume → thicker border = more glow
                double bord = 1.5 + amp * 4.5;

                // Blur softens the border into a halo
                double blur = amp * 7.0;

                // Glow colour ramp — same green→yellow→orange→red scale (ASS BGR)
                string glowColor;
                if (amp > 0.85) glowColor = "&H0000FF&"; // red
                else if (amp > 0.70) glowColor = "&H0066FF&"; // orange
                else if (amp > 0.55) glowColor = "&H00FFFF&"; // yellow
                else if (amp > 0.35) glowColor = "&H00FF00&"; // bright green
                else glowColor = "&H007800&"; // dark green (quiet)

                var frame = new SubtitleLineViewModel(sub, generateNewId: true);
                frame.StartTime = sub.StartTime.Add(TimeSpan.FromMilliseconds(t));
                frame.EndTime = sub.StartTime.Add(TimeSpan.FromMilliseconds(Math.Min(t + frameMs, durationMs)));
                frame.Text = $"{{\\an2\\1c&HFFFFFF&\\3c{glowColor}\\bord{bord:F1}\\blur{blur:F1}\\shad0\\fscx{scale}\\fscy{scale}}}" + cleanText;
                result.Add(frame);
            }
        }

        return result;
    }

    private static double GetAverageAmplitude(
        WavePeakData2? wavePeaks, double centerSec, double windowSec, double localPeak)
    {
        if (wavePeaks == null || localPeak <= 0) return 0;
        double half = windowSec / 2.0;
        int start = Math.Max(0, (int)((centerSec - half) * wavePeaks.SampleRate));
        int end = Math.Min(wavePeaks.Peaks.Count - 1,
                                (int)((centerSec + half) * wavePeaks.SampleRate));
        if (start > end) return 0;
        long sum = 0;
        for (int i = start; i <= end; i++)
            sum += wavePeaks.Peaks[i].Abs;
        return (double)(sum / (end - start + 1)) / localPeak;
    }

    private static double ComputeLocalPeak(
        WavePeakData2? wavePeaks, double startSec, double endSec)
    {
        if (wavePeaks == null) return 1;
        int start = Math.Max(0, (int)(startSec * wavePeaks.SampleRate));
        int end = Math.Min(wavePeaks.Peaks.Count - 1, (int)(endSec * wavePeaks.SampleRate));
        int peak = 0;
        for (int i = start; i <= end; i++)
        {
            int abs = wavePeaks.Peaks[i].Abs;
            if (abs > peak) peak = abs;
        }
        return peak > 0 ? peak : Math.Max(1, wavePeaks.HighestPeak);
    }
}

