using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect;

public interface IAdvancedEffectDisplay
{
    string Name { get; }
    string Description { get; }
    bool UsesAudio { get; }
    List<SubtitleLineViewModel> ApplyEffect(List<SubtitleLineViewModel> subtitles, int width, int height, WavePeakData2? wavePeaks);
}

public static class AdvancedEffectDisplayFactory
{
    public static List<IAdvancedEffectDisplay> List()
    {
        return new List<IAdvancedEffectDisplay>
        {
            new AdvancedEffectTypewriter(),
            new AdvancedEffectTypewriterWithHighlight(),
            new AdvancedEffectWordByWord(),
            new AdvancedEffectKaraoke(),
            new AdvancedEffectScrambleReveal(),
            new AdvancedEffectRainbowPulse(),
            new AdvancedEffectWave(),
            new AdvancedEffectWaveBlue(),
            new AdvancedEffectStarWarsScroll(),
            new AdvancedEffectEndCreditsScroll(),
            new AdvancedEffectStarfield(),
            new AdvancedEffectRain(),
            new AdvancedEffectFireflies(),
            new AdvancedEffectMatrix(),
            new AdvancedEffectSnow(),
            new AdvancedEffectOldMovie(),
            new AdvancedEffectNeonBurst(),
            new AdvancedEffectGlitch(),
            new AdvancedEffectBounceIn(),
            new AdvancedEffectAudioTextPulse(),
            new AdvancedEffectFadeIn(),
            new AdvancedEffectFadeOut(),
            new AdvancedEffectConfetti(),
            new AdvancedEffectHearts(),
            new AdvancedEffectWordSpacing(),
        }.OrderBy(p => p.Name).ToList();
    }
}

public class AdvancedEffectFadeIn : IAdvancedEffectDisplay
{
    public string Name => "Transition - fade-in";
    public string Description => "A per-line fade-in effect where the screen starts black and reveals the video.";
    public bool UsesAudio => false;

    public List<SubtitleLineViewModel> ApplyEffect(List<SubtitleLineViewModel> subtitles, int width, int height, WavePeakData2? wavePeaks)
    {
        var result = new List<SubtitleLineViewModel>();
        if (subtitles.Count == 0) return result;

        int w = width > 0 ? width : 1280;
        int h = height > 0 ? height : 720;

        foreach (var sub in subtitles)
        {
            result.Add(CreateLineFadeIn(sub.StartTime, sub.Duration.TotalMilliseconds, w, h));
            result.Add(sub);
        }

        return result;
    }

    private SubtitleLineViewModel CreateLineFadeIn(TimeSpan lineStart, double durationMs, int w, int h)
    {
        var fadeIn = new SubtitleLineViewModel();
        fadeIn.StartTime = lineStart;
        // The box only needs to exist for the duration of the fade
        fadeIn.EndTime = lineStart.Add(TimeSpan.FromMilliseconds(durationMs));

        // Vector rectangle covering the whole screen
        string drawBox = $"m 0 0 l {w} 0 l {w} {h} l 0 {h}";

        // \fad(0, durationMs) makes the black box go from Opaque (0) to Transparent (durationMs)
        fadeIn.Text = "{\\p1\\an7\\pos(0,0)\\bord0\\shad0\\1c&H000000&\\fad(0," + durationMs + ")}" + drawBox;

        return fadeIn;
    }
}

public class AdvancedEffectFadeOut : IAdvancedEffectDisplay
{
    public string Name => "Transition - fade-out";
    public string Description => "A per-line fade-out effect where the screen ends black.";
    public bool UsesAudio => false;

    public override string ToString() => Name;

    public List<SubtitleLineViewModel> ApplyEffect(List<SubtitleLineViewModel> subtitles, int width, int height, WavePeakData2? wavePeaks)
    {
        var result = new List<SubtitleLineViewModel>();
        if (subtitles.Count == 0) return result;

        int w = width > 0 ? width : 1280;
        int h = height > 0 ? height : 720;

        foreach (var sub in subtitles)
        {
            result.Add(CreateBlackOverlay(sub.StartTime, sub.Duration.TotalMilliseconds, w, h));
            result.Add(sub);
        }

        return result;
    }

    private SubtitleLineViewModel CreateBlackOverlay(TimeSpan lineStart, double durationMs, int w, int h)
    {
        var fadeOut = new SubtitleLineViewModel();
        fadeOut.StartTime = lineStart;
        fadeOut.EndTime = lineStart.Add(TimeSpan.FromMilliseconds(durationMs));

        // Vector rectangle covering the whole screen
        string drawBox = $"m 0 0 l {w} 0 l {w} {h} l 0 {h}";

        fadeOut.Text = "{\\p1\\an7\\pos(0,0)\\bord0\\shad0\\1c&H000000&\\layer10\\fad(" + durationMs + ",0)}" + drawBox;

        return fadeOut;
    }
}

public class AdvancedEffectNeonBurst : IAdvancedEffectDisplay
{
    public string Name => Se.Language.Assa.AdvancedEffectNeonBurst;
    public string Description => Se.Language.Assa.AdvancedEffectNeonBurstDescription;
    public bool UsesAudio => false;

    public override string ToString() => Name;

    public List<SubtitleLineViewModel> ApplyEffect(List<SubtitleLineViewModel> subtitles, int width, int height, WavePeakData2? wavePeaks)
    {
        var result = new List<SubtitleLineViewModel>();
        if (subtitles.Count == 0) return result;

        Random rng = new Random(subtitles[0].Text.GetHashCode());

        foreach (var sub in subtitles)
        {
            // 1. SELECT NEON COLOR
            // Modern shorts use 'Electric Lime', 'Cyan', or 'Hot Pink'
            string[] neonColors = { "&H00FF00&", "&HFFFF00&", "&HFF00FF&", "&H00FFFF&" };
            string chosenColor = neonColors[rng.Next(neonColors.Length)];

            // 2. CREATE THE GLOW LAYER (The 'Bloom')
            // We create a duplicate line that sits behind the text with a heavy blur
            var glowLayer = new SubtitleLineViewModel()
            {
                StartTime = sub.StartTime,
                EndTime = sub.EndTime,
                // Deep neon glow: High blur, matching color, slightly transparent
                Text = "{\\bord5\\blur8\\shad0\\1c" + chosenColor + "\\3c" + chosenColor + "\\alpha&H60&" +
                       GetPopTags(0, 150) + "}" + HtmlUtil.RemoveHtmlTags(sub.Text, true) // CleanText removes existing tags
            };

            // 3. CREATE THE SHARP TOP LAYER (The 'Core')
            var coreLayer = new SubtitleLineViewModel()
            {
                StartTime = sub.StartTime,
                EndTime = sub.EndTime,
                // Sharp white core with a thin neon border
                Text = "{\\bord2\\blur0.5\\shad0\\1c&HFFFFFF&\\3c" + chosenColor +
                       GetPopTags(0, 150) + "}" + HtmlUtil.RemoveHtmlTags(sub.Text, true)
            };

            result.Add(glowLayer);
            result.Add(coreLayer);
        }

        return result;
    }

    private static string GetPopTags(int startOffset, int duration)
    {
        // Starts at 125% size and shrinks down to 100% quickly
        return $"\\fscx125\\fscy125\\t({startOffset},{duration},\\fscx100\\fscy100)";
    }
}

public class AdvancedEffectOldMovie : IAdvancedEffectDisplay
{
    public string Name => Se.Language.Assa.AdvancedEffectOldMovie;
    public string Description => Se.Language.Assa.AdvancedEffectOldMovieDescription;
    public bool UsesAudio => false;

    public override string ToString() => Name;

    public List<SubtitleLineViewModel> ApplyEffect(List<SubtitleLineViewModel> subtitles, int width, int height, WavePeakData2? wavePeaks)
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

public class AdvancedEffectSnow : IAdvancedEffectDisplay
{
    public string Name => Se.Language.Assa.AdvancedEffectSnow;
    public string Description => Se.Language.Assa.AdvancedEffectSnowDescription;
    public bool UsesAudio => false;
    public int FlakeCount { get; set; } = 200;

    public override string ToString() => Name;

    public List<SubtitleLineViewModel> ApplyEffect(List<SubtitleLineViewModel> subtitles, int width, int height, WavePeakData2? wavePeaks)
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

public class AdvancedEffectRain : IAdvancedEffectDisplay
{
    public string Name => Se.Language.Assa.AdvancedEffectRain;
    public string Description => Se.Language.Assa.AdvancedEffectRainDescription;
    public bool UsesAudio => false;

    public override string ToString() => Name;

    public List<SubtitleLineViewModel> ApplyEffect(List<SubtitleLineViewModel> subtitles, int width, int height, WavePeakData2? wavePeaks)
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

public class AdvancedEffectStarfield : IAdvancedEffectDisplay
{
    public string Name => Se.Language.Assa.AdvancedEffectStarfield;
    public string Description => Se.Language.Assa.AdvancedEffectStarfieldDescription;
    public bool UsesAudio => false;
    public int StarCount { get; set; } = 650;
    public decimal SpeedMultiplier { get; set; } = 1.0m;

    public override string ToString() => Name;

    public List<SubtitleLineViewModel> ApplyEffect(List<SubtitleLineViewModel> subtitles, int width, int height, WavePeakData2? wavePeaks)
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

public class AdvancedEffectEndCreditsScroll : IAdvancedEffectDisplay
{
    public string Name => Se.Language.Assa.AdvancedEffectEndCreditsScroll;
    public string Description => Se.Language.Assa.AdvancedEffectEndCreditsScrollDescription;
    public bool UsesAudio => false;

    public override string ToString() => Name;

    public List<SubtitleLineViewModel> ApplyEffect(List<SubtitleLineViewModel> subtitles, int width, int height, WavePeakData2? wavePeaks)
    {
        var result = new List<SubtitleLineViewModel>();
        if (subtitles.Count == 0) return result;

        int screenWidth = width > 0 ? width : 1356;
        int screenHeight = height > 0 ? height : 678;
        int centerX = screenWidth / 2;

        double travelDurationMs = 12000;
        double leadTimeMs = 500;
        int internalLineSpacing = 60; // Slightly increased for the bigger font

        int yStartBase = screenHeight + 20;
        int yEndBase = -150;

        foreach (var sub in subtitles)
        {
            string processedText = sub.Text.Replace("\\N", "\n").Replace("\\n", "\n");
            var clean = Utilities.RemoveSsaTags(processedText);
            if (string.IsNullOrWhiteSpace(clean)) continue;

            var lines = clean.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < lines.Length; i++)
            {
                var charLine = new SubtitleLineViewModel(sub, generateNewId: true);

                double startTimeMs = sub.StartTime.TotalMilliseconds - leadTimeMs;
                if (startTimeMs < 0) startTimeMs = 0;

                charLine.StartTime = TimeSpan.FromMilliseconds(startTimeMs);
                charLine.EndTime = charLine.StartTime.Add(TimeSpan.FromMilliseconds(travelDurationMs));

                int yOffset = i * internalLineSpacing;

                string tags =
                    $@"\an8\fad(300,300)" +
                    $@"\move({centerX},{yStartBase + yOffset},{centerX},{yEndBase + yOffset},0,{(int)travelDurationMs})";

                charLine.Text = "{" + tags + "}" + lines[i].Trim();
                result.Add(charLine);
            }
        }

        return result;
    }
}


public class AdvancedEffectStarWarsScroll : IAdvancedEffectDisplay
{
    public string Name => Se.Language.Assa.AdvancedEffectStarWarsScroll;
    public string Description => Se.Language.Assa.AdvancedEffectStarWarsScrollDescription;
    public bool UsesAudio => false;
    public override string ToString() => Name;

    public List<SubtitleLineViewModel> ApplyEffect(List<SubtitleLineViewModel> subtitles, int width, int height, WavePeakData2? wavePeaks)
    {
        var result = new List<SubtitleLineViewModel>();
        if (subtitles.Count == 0) return result;

        int screenWidth = width > 0 ? width : 1356;
        int screenHeight = height > 0 ? height : 678;
        int centerX = screenWidth / 2;

        string swYellow = "&H00D7FF&";

        // PHYSICS SETTINGS
        double travelDurationMs = 25000;
        double leadTimeMs = 1500; // Even lower to ensure it hits dialogue timing
        int internalLineSpacing = 80;

        int yStartBase = screenHeight + 50;
        int yEndBase = -300;

        foreach (var sub in subtitles)
        {
            string processedText = sub.Text.Replace("\\N", "\n").Replace("\\n", "\n");
            var clean = Utilities.RemoveSsaTags(processedText);
            if (string.IsNullOrWhiteSpace(clean)) continue;

            var lines = clean.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < lines.Length; i++)
            {
                var charLine = new SubtitleLineViewModel(sub, generateNewId: true);

                double startTimeMs = sub.StartTime.TotalMilliseconds - leadTimeMs;
                if (startTimeMs < 0) startTimeMs = 0;

                charLine.StartTime = TimeSpan.FromMilliseconds(startTimeMs);
                // We give it an extra 2 seconds of life to ensure the fade finishes
                charLine.EndTime = charLine.StartTime.Add(TimeSpan.FromMilliseconds(travelDurationMs + 2000));

                int yOffset = i * internalLineSpacing;

                // THE TAGS
                string tags =
                    $@"\an5\b1\bord2\blur0.6\1c{swYellow}" +
                    $@"\frx52\fscy75" +
                    $@"\fad(1200,0)" +
                    $@"\fscx140\fscy140" +
                    $@"\move({centerX},{yStartBase + yOffset},{centerX},{yEndBase + yOffset},0,{(int)travelDurationMs})" +

                    // SCALE
                    $@"\t(0,{(int)travelDurationMs},\fscx35\fscy20)" +

                    // FADE: Start turning transparent at 60% of the journey (around the upper-middle)
                    // and finish by 90% of the journey.
                    $@"\t({(int)(travelDurationMs * 0.6)},{(int)(travelDurationMs * 0.9)},\alpha&HFF&)";

                charLine.Text = "{" + tags + "}" + lines[i].Trim();
                result.Add(charLine);
            }
        }

        return result;
    }
}

public class AdvancedEffectWaveBlue : IAdvancedEffectDisplay
{
    public string Name => Se.Language.Assa.AdvancedEffectWaveBlue;
    public string Description => Se.Language.Assa.AdvancedEffectWaveBlueDescription;
    public bool UsesAudio => false;

    public override string ToString() => Name;

    public List<SubtitleLineViewModel> ApplyEffect(List<SubtitleLineViewModel> subtitles, int width, int height, WavePeakData2? wavePeaks)
    {
        var result = new List<SubtitleLineViewModel>();

        foreach (var sub in subtitles)
        {
            var matches = System.Text.RegularExpressions.Regex.Matches(sub.Text, @"(\{.*?\})|([^{]+)");
            var fullParts = new List<(string Content, bool IsTag)>();
            foreach (System.Text.RegularExpressions.Match m in matches)
                fullParts.Add((m.Value, m.Value.StartsWith("{")));

            var visibleChars = new List<(char Character, int PartIndex, int CharIndex)>();
            for (int p = 0; p < fullParts.Count; p++)
                if (!fullParts[p].IsTag)
                    for (int c = 0; c < fullParts[p].Content.Length; c++)
                        visibleChars.Add((fullParts[p].Content[c], p, c));

            double totalMs = sub.Duration.TotalMilliseconds;
            int waveSpeed = 150; // Faster updates = smoother motion
            double waveFrequency = 0.5; // Distance between peaks

            for (int i = 0; i < visibleChars.Count; i++)
            {
                var target = visibleChars[i];
                if (char.IsWhiteSpace(target.Character)) continue;

                var charLine = new SubtitleLineViewModel(sub, generateNewId: true);
                string oceanTransforms = "";

                for (int time = 0; time < totalMs; time += waveSpeed)
                {
                    // Calculate wave phase (-1.0 to 1.0)
                    double angle = (time / 250.0) - (i * waveFrequency);
                    double sineValue = Math.Sin(angle);

                    // 1. HEIGHT: Scale from 100% to 170%
                    int heightScale = 100 + (int)((sineValue + 1) * 35);

                    // 2. COLOR: Interpolate between Deep Blue and Seafoam Cyan
                    // At sineValue -1: Deep Blue (&HFF8800&)
                    // At sineValue +1: Bright Cyan (&HFFFF00&)
                    string currentColor = (sineValue > 0) ? "&HFFFF00&" : "&HFF8800&";

                    // 3. BLUR: Add a "glow" only at the peaks
                    int blurValue = (sineValue > 0.7) ? 4 : 1;

                    oceanTransforms += $@" \t({time},{time + waveSpeed},\fscy{heightScale}\1c{currentColor}\blur{blurValue})";
                }

                var finalString = new System.Text.StringBuilder();
                for (int p = 0; p < fullParts.Count; p++)
                {
                    if (fullParts[p].IsTag) finalString.Append(fullParts[p].Content);
                    else if (p == target.PartIndex)
                    {
                        string partText = fullParts[p].Content;
                        // Initial State: Deep Blue, Bottom-Anchored
                        finalString.Append(@"{\an2\alpha&HFF&}").Append(partText.Substring(0, target.CharIndex))
                                   .Append(@"{\alpha&H00&\1c&HFF8800&" + oceanTransforms + "}").Append(target.Character)
                                   .Append(@"{\alpha&HFF&}").Append(partText.Substring(target.CharIndex + 1));
                    }
                    else finalString.Append(@"{\an2\alpha&HFF&}").Append(fullParts[p].Content);
                }

                charLine.Text = finalString.ToString();
                result.Add(charLine);
            }
        }
        return result;
    }
}

public class AdvancedEffectWave : IAdvancedEffectDisplay
{
    public string Name => Se.Language.Assa.AdvancedEffectWave;
    public string Description => Se.Language.Assa.AdvancedEffectWaveDescription;
    public bool UsesAudio => false;

    public override string ToString() => Name;

    public List<SubtitleLineViewModel> ApplyEffect(List<SubtitleLineViewModel> subtitles, int width, int height, WavePeakData2? wavePeaks)
    {
        var result = new List<SubtitleLineViewModel>();

        foreach (var sub in subtitles)
        {
            var matches = System.Text.RegularExpressions.Regex.Matches(sub.Text, @"(\{.*?\})|([^{]+)");
            var fullParts = new List<(string Content, bool IsTag)>();
            foreach (System.Text.RegularExpressions.Match m in matches)
                fullParts.Add((m.Value, m.Value.StartsWith("{")));

            var visibleChars = new List<(char Character, int PartIndex, int CharIndex)>();
            for (int p = 0; p < fullParts.Count; p++)
                if (!fullParts[p].IsTag)
                    for (int c = 0; c < fullParts[p].Content.Length; c++)
                        visibleChars.Add((fullParts[p].Content[c], p, c));

            double totalMs = sub.Duration.TotalMilliseconds;

            // WAVE SETTINGS
            int waveSpeed = 200; // Time per "step" in the wave
            double waveFrequency = 0.4; // How many letters are "up" at once

            for (int i = 0; i < visibleChars.Count; i++)
            {
                var target = visibleChars[i];
                if (char.IsWhiteSpace(target.Character)) continue;

                var charLine = new SubtitleLineViewModel(sub, generateNewId: true);

                string waveTransforms = "";
                // We create a rolling loop for the duration of the line
                for (int time = 0; time < totalMs; time += waveSpeed)
                {
                    // Calculate the "height" using a sine wave based on time AND character position
                    // This creates the "roll" effect.
                    double angle = (time / 200.0) - (i * waveFrequency);
                    double heightFactor = Math.Sin(angle); // Returns -1.0 to 1.0

                    // Map that to scale: 100% (flat) to 160% (peak)
                    int currentScale = 100 + (int)((heightFactor + 1) * 30);

                    waveTransforms += $@" \t({time},{time + waveSpeed},\fscy{currentScale})";
                }

                var finalString = new System.Text.StringBuilder();
                for (int p = 0; p < fullParts.Count; p++)
                {
                    if (fullParts[p].IsTag) finalString.Append(fullParts[p].Content);
                    else if (p == target.PartIndex)
                    {
                        string partText = fullParts[p].Content;
                        // Force \an2 (bottom center) so the letters grow UPWARDS from the ground
                        finalString.Append(@"{\an2\alpha&HFF&}").Append(partText.Substring(0, target.CharIndex))
                                   .Append(@"{\alpha&H00&" + waveTransforms + "}").Append(target.Character)
                                   .Append(@"{\alpha&HFF&}").Append(partText.Substring(target.CharIndex + 1));
                    }
                    else finalString.Append(@"{\an2\alpha&HFF&}").Append(fullParts[p].Content);
                }

                charLine.Text = finalString.ToString();
                result.Add(charLine);
            }
        }
        return result;
    }
}

public class AdvancedEffectRainbowPulse : IAdvancedEffectDisplay
{
    public string Name => Se.Language.Assa.AdvancedEffectRainbowPulse;
    public string Description => Se.Language.Assa.AdvancedEffectRainbowPulseDescription;
    public bool UsesAudio => false;

    public override string ToString() => Name;

    public List<SubtitleLineViewModel> ApplyEffect(List<SubtitleLineViewModel> subtitles, int width, int height, WavePeakData2? wavePeaks)
    {
        var result = new List<SubtitleLineViewModel>();
        string[] rainbowColors = ["&H0000FF&", "&H00FFFF&", "&H00FF00&", "&HFFFF00&", "&HFF0000&", "&HFF00FF&"];

        foreach (var sub in subtitles)
        {
            // 1. Split the text into parts (Tags vs Text)
            // This regex finds everything inside { } and everything outside.
            var matches = System.Text.RegularExpressions.Regex.Matches(sub.Text, @"(\{.*?\})|([^{]+)");

            var fullParts = new List<(string Content, bool IsTag)>();
            foreach (System.Text.RegularExpressions.Match m in matches)
            {
                if (m.Value.StartsWith("{"))
                    fullParts.Add((m.Value, true));
                else
                    fullParts.Add((m.Value, false));
            }

            // 2. Map every visible character to its position in the full string
            var visibleChars = new List<(char Character, int PartIndex, int CharIndex)>();
            for (int p = 0; p < fullParts.Count; p++)
            {
                if (!fullParts[p].IsTag)
                {
                    for (int c = 0; c < fullParts[p].Content.Length; c++)
                    {
                        visibleChars.Add((fullParts[p].Content[c], p, c));
                    }
                }
            }

            double totalMs = sub.Duration.TotalMilliseconds;
            int stepMs = 200;

            // 3. Create a line for each visible character
            for (int i = 0; i < visibleChars.Count; i++)
            {
                var target = visibleChars[i];
                if (char.IsWhiteSpace(target.Character)) continue;

                var charLine = new SubtitleLineViewModel(sub, generateNewId: true);

                // Rainbow logic
                int startColorIndex = i % rainbowColors.Length;
                string colorCycle = "";
                for (int currentTime = 0, step = 1; currentTime < totalMs; currentTime += stepMs, step++)
                {
                    int nextIndex = (startColorIndex + step) % rainbowColors.Length;
                    colorCycle += $@"\t({currentTime},{currentTime + stepMs},\1c{rainbowColors[nextIndex]})";
                }

                // Construct the string by rebuilding the parts
                var finalString = new System.Text.StringBuilder();
                for (int p = 0; p < fullParts.Count; p++)
                {
                    if (fullParts[p].IsTag)
                    {
                        finalString.Append(fullParts[p].Content);
                    }
                    else if (p == target.PartIndex)
                    {
                        // This part contains our target character
                        string partText = fullParts[p].Content;
                        string left = partText.Substring(0, target.CharIndex);
                        string right = partText.Substring(target.CharIndex + 1);

                        finalString.Append(@"{ \alpha&HFF& }").Append(left)
                                   .Append(@"{ \alpha&H00&\1c").Append(rainbowColors[startColorIndex]).Append(colorCycle).Append(" }")
                                   .Append(target.Character)
                                   .Append(@"{ \alpha&HFF& }").Append(right);
                    }
                    else
                    {
                        // This part is text but doesn't have our active letter, hide it
                        finalString.Append(@"{ \alpha&HFF& }").Append(fullParts[p].Content);
                    }
                }

                charLine.Text = finalString.ToString();
                result.Add(charLine);
            }
        }
        return result;
    }
}

/// <summary>
/// Reveals text one character at a time: produces N sequential subtitle entries where entry i
/// shows the first i+1 characters and spans 1/N of the original duration.
/// </summary>
public class AdvancedEffectTypewriter : IAdvancedEffectDisplay
{
    public string Name => Se.Language.Assa.AdvancedEffectTypewriter;
    public string Description => Se.Language.Assa.AdvancedEffectTypewriterDescription;
    public bool UsesAudio => false;

    public override string ToString() => Name;

    public List<SubtitleLineViewModel> ApplyEffect(List<SubtitleLineViewModel> subtitles, int width, int height, WavePeakData2? wavePeaks)
    {
        var result = new List<SubtitleLineViewModel>();
        foreach (var subtitle in subtitles)
        {
            var cleanText = Utilities.RemoveSsaTags(subtitle.Text);
            if (string.IsNullOrEmpty(cleanText))
            {
                result.Add(subtitle);
                continue;
            }

            var chars = cleanText.ToCharArray();
            var charCount = chars.Length;
            var totalMs = subtitle.Duration.TotalMilliseconds;
            var msPerChar = totalMs / charCount;

            for (var i = 0; i < charCount; i++)
            {
                var line = new SubtitleLineViewModel(subtitle, generateNewId: true);
                var start = subtitle.StartTime.Add(TimeSpan.FromMilliseconds(i * msPerChar));
                var end = i == charCount - 1
                    ? subtitle.EndTime
                    : subtitle.StartTime.Add(TimeSpan.FromMilliseconds((i + 1) * msPerChar));
                line.StartTime = start;
                line.EndTime = end;
                line.Text = new string(chars, 0, i + 1)
                    .Replace("\r\n", "\\N")
                    .Replace("\r", "\\N")
                    .Replace("\n", "\\N");
                result.Add(line);
            }
        }
        return result;
    }
}

public class AdvancedEffectTypewriterWithHighlight : IAdvancedEffectDisplay
{
    public string Name => Se.Language.Assa.AdvancedEffectTypewriterWithHighlight;
    public string Description => Se.Language.Assa.AdvancedEffectTypewriterWithHighlightDescription;
    public bool UsesAudio => false;

    public override string ToString() => Name;

    public List<SubtitleLineViewModel> ApplyEffect(List<SubtitleLineViewModel> subtitles, int width, int height, WavePeakData2? wavePeaks)
    {
        var result = new List<SubtitleLineViewModel>();

        foreach (var subtitle in subtitles)
        {
            var cleanText = Utilities.RemoveSsaTags(subtitle.Text);
            if (string.IsNullOrEmpty(cleanText))
            {
                result.Add(subtitle);
                continue;
            }

            var text = cleanText.Replace("\r\n", "\n").Replace("\r", "\n");
            var chars = text.ToCharArray();
            var charCount = chars.Length;
            var msPerChar = subtitle.Duration.TotalMilliseconds / charCount;

            for (var i = 0; i < charCount; i++)
            {
                var line = new SubtitleLineViewModel(subtitle, generateNewId: true);

                var start = subtitle.StartTime.Add(TimeSpan.FromMilliseconds(i * msPerChar));
                var end = (i == charCount - 1)
                    ? subtitle.EndTime
                    : subtitle.StartTime.Add(TimeSpan.FromMilliseconds((i + 1) * msPerChar));

                line.StartTime = start;
                line.EndTime = end;

                string previousText = new string(chars, 0, i).Replace("\n", "\\N");
                string activeChar = chars[i].ToString().Replace("\n", "\\N");

                // TAGS:
                // {\r} - Reset revealed text to your default style (clean and sharp).
                // {\bord3\blur5\3c&HFFFFFF&} - Adds a 3px white border, then blurs it heavily.
                // This creates a "glow" halo around the active character only.
                line.Text = "{\\r}" + previousText +
                            "{\\bord3\\blur8\\3c&HFFFFFF&\\1c&HFFFFFF&}" + activeChar;

                result.Add(line);
            }
        }
        return result;
    }
}

/// <summary>
/// Reveals text one word at a time: produces N sequential subtitle entries where entry i
/// shows the first i+1 words and spans 1/N of the original duration.
/// </summary>
public class AdvancedEffectWordByWord : IAdvancedEffectDisplay
{
    public string Name => Se.Language.Assa.AdvancedEffectWordByWord;
    public string Description => Se.Language.Assa.AdvancedEffectWordByWordDescription;
    public bool UsesAudio => false;
    public override string ToString() => Name;

    public List<SubtitleLineViewModel> ApplyEffect(List<SubtitleLineViewModel> subtitles, int width, int height, WavePeakData2? wavePeaks)
    {
        var result = new List<SubtitleLineViewModel>();
        foreach (var subtitle in subtitles)
        {
            var cleanText = Utilities.RemoveSsaTags(subtitle.Text)
                .Replace("\r\n", " ")
                .Replace("\r", " ")
                .Replace("\n", " ");

            if (string.IsNullOrWhiteSpace(cleanText))
            {
                result.Add(subtitle);
                continue;
            }

            var words = cleanText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length <= 1)
            {
                result.Add(subtitle);
                continue;
            }

            var totalMs = subtitle.Duration.TotalMilliseconds;
            var msPerWord = totalMs / words.Length;

            for (var i = 0; i < words.Length; i++)
            {
                var line = new SubtitleLineViewModel(subtitle, generateNewId: true);
                var start = subtitle.StartTime.Add(TimeSpan.FromMilliseconds(i * msPerWord));
                var end = i == words.Length - 1
                    ? subtitle.EndTime
                    : subtitle.StartTime.Add(TimeSpan.FromMilliseconds((i + 1) * msPerWord));
                line.StartTime = start;
                line.EndTime = end;
                line.Text = string.Join(" ", words, 0, i + 1);
                result.Add(line);
            }
        }
        return result;
    }
}

/// <summary>
/// Karaoke-style progressive color reveal: produces N sequential subtitle entries where entry i
/// has characters 0..i in white and the remaining characters dimmed — batching consecutive
/// same-color characters to keep the output compact.
/// </summary>
public class AdvancedEffectKaraoke : IAdvancedEffectDisplay
{
    public string Name => Se.Language.Assa.AdvancedEffectKaraoke;
    public string Description => Se.Language.Assa.AdvancedEffectKaraokeDescription;
    public bool UsesAudio => false;

    private const string HighlightColor = @"{\1c&HFFFFFF&}";
    private const string DimColor = @"{\1c&H808080&}";

    public override string ToString() => Name;

    public List<SubtitleLineViewModel> ApplyEffect(List<SubtitleLineViewModel> subtitles, int width, int height, WavePeakData2? wavePeaks)
    {
        var result = new List<SubtitleLineViewModel>();
        foreach (var subtitle in subtitles)
        {
            var cleanText = Utilities.RemoveSsaTags(subtitle.Text);
            if (string.IsNullOrEmpty(cleanText))
            {
                result.Add(subtitle);
                continue;
            }

            var chars = cleanText
                .Replace("\r\n", "\n")
                .Replace("\r", "\n")
                .ToCharArray();
            var charCount = chars.Length;
            var totalMs = subtitle.Duration.TotalMilliseconds;
            var msPerChar = totalMs / charCount;

            for (var i = 0; i < charCount; i++)
            {
                var line = new SubtitleLineViewModel(subtitle, generateNewId: true);
                var start = subtitle.StartTime.Add(TimeSpan.FromMilliseconds(i * msPerChar));
                var end = i == charCount - 1
                    ? subtitle.EndTime
                    : subtitle.StartTime.Add(TimeSpan.FromMilliseconds((i + 1) * msPerChar));
                line.StartTime = start;
                line.EndTime = end;
                line.Text = BuildKaraokeText(chars, i);
                result.Add(line);
            }
        }
        return result;
    }

    private static string BuildKaraokeText(char[] chars, int highlightUpTo)
    {
        var sb = new StringBuilder(chars.Length * 12);
        string? currentColor = null;

        for (var j = 0; j < chars.Length; j++)
        {
            if (chars[j] == '\n')
            {
                sb.Append("\\N");
                currentColor = null;
                continue;
            }

            var color = j <= highlightUpTo ? HighlightColor : DimColor;
            if (color != currentColor)
            {
                sb.Append(color);
                currentColor = color;
            }
            sb.Append(chars[j]);
        }

        return sb.ToString();
    }
}

/// <summary>
/// Scramble reveal: produces N sequential subtitle entries where entry i shows the correct
/// characters at positions 0..i and stable random placeholder characters at positions i+1..N-1.
/// The random seed is derived from the subtitle text so scrambling is deterministic.
/// </summary>
public class AdvancedEffectScrambleReveal : IAdvancedEffectDisplay
{
    public string Name => Se.Language.Assa.AdvancedEffectScrambleReveal;
    public string Description => Se.Language.Assa.AdvancedEffectScrambleRevealDescription;
    public bool UsesAudio => false;

    private static readonly char[] ScramblePool =
        "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789#@$%&".ToCharArray();

    public override string ToString() => Name;

    public List<SubtitleLineViewModel> ApplyEffect(List<SubtitleLineViewModel> subtitles, int width, int height, WavePeakData2? wavePeaks)
    {
        var result = new List<SubtitleLineViewModel>();
        foreach (var subtitle in subtitles)
        {
            var cleanText = Utilities.RemoveSsaTags(subtitle.Text);
            if (string.IsNullOrEmpty(cleanText))
            {
                result.Add(subtitle);
                continue;
            }

            var chars = cleanText
                .Replace("\r\n", "\n")
                .Replace("\r", "\n")
                .ToCharArray();
            var charCount = chars.Length;
            var totalMs = subtitle.Duration.TotalMilliseconds;
            var msPerChar = totalMs / charCount;

            var rng = new Random(subtitle.Text.GetHashCode());
            var scrambled = new char[charCount];
            for (var k = 0; k < charCount; k++)
            {
                scrambled[k] = chars[k] is '\n' or ' '
                    ? chars[k]
                    : ScramblePool[rng.Next(ScramblePool.Length)];
            }

            for (var i = 0; i < charCount; i++)
            {
                var line = new SubtitleLineViewModel(subtitle, generateNewId: true);
                var start = subtitle.StartTime.Add(TimeSpan.FromMilliseconds(i * msPerChar));
                var end = i == charCount - 1
                    ? subtitle.EndTime
                    : subtitle.StartTime.Add(TimeSpan.FromMilliseconds((i + 1) * msPerChar));
                line.StartTime = start;
                line.EndTime = end;
                line.Text = BuildScrambledText(chars, scrambled, i);
                result.Add(line);
            }
        }
        return result;
    }

    private static string BuildScrambledText(char[] real, char[] scrambled, int revealedUpTo)
    {
        var sb = new StringBuilder(real.Length);
        for (var j = 0; j < real.Length; j++)
        {
            if (real[j] == '\n')
            {
                sb.Append("\\N");
                continue;
            }
            sb.Append(j <= revealedUpTo ? real[j] : scrambled[j]);
        }
        return sb.ToString();
    }
}

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

    public List<SubtitleLineViewModel> ApplyEffect(List<SubtitleLineViewModel> subtitles, int width, int height, WavePeakData2? wavePeaks)
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

/// <summary>
/// Bounce-in effect: each character springs in one at a time (staggered left to right)
/// using elastic scale physics — 0% → 140% → 85% → 110% → 100%.
/// </summary>
public class AdvancedEffectBounceIn : IAdvancedEffectDisplay
{
    public string Name => Se.Language.Assa.AdvancedEffectBounceIn;
    public string Description => Se.Language.Assa.AdvancedEffectBounceInDescription;
    public bool UsesAudio => false;

    public override string ToString() => Name;

    public List<SubtitleLineViewModel> ApplyEffect(List<SubtitleLineViewModel> subtitles, int width, int height, WavePeakData2? wavePeaks)
    {
        var result = new List<SubtitleLineViewModel>();

        foreach (var sub in subtitles)
        {
            var cleanText = Utilities.RemoveSsaTags(sub.Text);
            if (string.IsNullOrEmpty(cleanText))
            {
                result.Add(sub);
                continue;
            }

            var chars = cleanText.Replace("\r\n", "\n").Replace("\r", "\n").ToCharArray();

            // Adapt stagger so even long lines feel snappy; min 20ms, max 80ms
            int nonSpaceCount = chars.Count(c => c != ' ' && c != '\n');
            int staggerMs = nonSpaceCount > 0
                ? (int)Math.Max(20, Math.Min(80, sub.Duration.TotalMilliseconds / 3.0 / nonSpaceCount))
                : 50;

            for (int i = 0; i < chars.Length; i++)
            {
                if (chars[i] == ' ' || chars[i] == '\n')
                    continue;

                var line = new SubtitleLineViewModel(sub, generateNewId: true);
                line.StartTime = sub.StartTime.Add(TimeSpan.FromMilliseconds(i * staggerMs));
                line.EndTime = sub.EndTime;

                var sb = new StringBuilder();
                for (int j = 0; j < chars.Length; j++)
                {
                    if (chars[j] == '\n')
                    {
                        sb.Append("\\N");
                        continue;
                    }

                    if (j == i)
                    {
                        // Elastic spring: scale up with overshoot, then settle
                        sb.Append("{\\alpha&H00&\\fscx0\\fscy0" +
                                  "\\t(0,180,\\fscx140\\fscy140)" +
                                  "\\t(180,300,\\fscx85\\fscy85)" +
                                  "\\t(300,420,\\fscx110\\fscy110)" +
                                  "\\t(420,500,\\fscx100\\fscy100)}");
                    }
                    else
                    {
                        // Transparent placeholder — preserves spacing but invisible
                        sb.Append("{\\alpha&HFF&}");
                    }

                    sb.Append(chars[j]);
                }

                line.Text = sb.ToString();
                result.Add(line);
            }
        }

        return result;
    }
}

/// <summary>
/// Matrix effect: falling green character columns (background) with a letter-by-letter
/// green-to-white reveal that builds the subtitle text from the rain.
/// </summary>
public class AdvancedEffectMatrix : IAdvancedEffectDisplay
{
    public string Name => Se.Language.Assa.AdvancedEffectMatrix;
    public string Description => Se.Language.Assa.AdvancedEffectMatrixDescription;
    public bool UsesAudio => false;
    public int ColumnCount { get; set; } = 55;

    private static readonly char[] MatrixPool =
        "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789$!?@#%&*<>[]{}|/;:".ToCharArray();

    private const string HeadColor = "&HCCFFCC&"; // bright white-green stream head
    private const string BrightGreen = "&H00FF00&"; // pure green
    private const string MidGreen = "&H007800&"; // mid-green trail
    private const string DarkGreen = "&H003400&"; // dark-green tail

    public override string ToString() => Name;

    public List<SubtitleLineViewModel> ApplyEffect(List<SubtitleLineViewModel> subtitles, int width, int height, WavePeakData2? wavePeaks)
    {
        var result = new List<SubtitleLineViewModel>();
        if (subtitles.Count == 0) return result;

        var rng = new Random(subtitles[0].Text.GetHashCode());

        int w = width > 0 ? width : 1280;
        int h = height > 0 ? height : 720;

        var globalStart = subtitles.Min(s => s.StartTime);
        var globalEnd = subtitles.Max(s => s.EndTime);
        double totalVideoMs = (globalEnd - globalStart).TotalMilliseconds;

        // ── MATRIX RAIN COLUMNS ─────────────────────────────────────────────────
        int cols = ColumnCount;
        int colW = w / cols;
        int fs = Math.Max(10, (int)(colW * 0.65)); // compact grid

        for (int col = 0; col < cols; col++)
        {
            int colX = col * colW + colW / 2;
            double streamStart = rng.Next(0, (int)(totalVideoMs * 0.25)) + col * 25;

            while (streamStart < totalVideoMs)
            {
                int streamLen = rng.Next(8, 18);
                int fallDur = rng.Next(1200, 3500);
                int interval = fallDur / (streamLen + 2); // stagger between chars in stream

                for (int j = 0; j < streamLen; j++)
                {
                    double charStartMs = streamStart + j * interval;
                    if (charStartMs >= totalVideoMs) break;

                    var entry = new SubtitleLineViewModel();
                    entry.StartTime = globalStart.Add(TimeSpan.FromMilliseconds(charStartMs));
                    entry.EndTime = entry.StartTime.Add(TimeSpan.FromMilliseconds(fallDur));
                    if (entry.StartTime >= globalEnd) continue;

                    char ch = MatrixPool[rng.Next(MatrixPool.Length)];

                    string color, extra;
                    int alpha;
                    if (j == 0) // head — first launched, deepest, brightest
                    {
                        color = HeadColor;
                        extra = $"\\blur3.5\\bord0.5\\3c{BrightGreen}";
                        alpha = 0x00;
                    }
                    else
                    {
                        float pos = (float)j / (streamLen - 1); // 0 = head … 1 = tail
                        color = pos < 0.35f ? BrightGreen : pos < 0.65f ? MidGreen : DarkGreen;
                        float trailBlur = Math.Max(1.0f, 3.0f - pos * 2.0f);
                        extra = $"\\blur{trailBlur:F1}\\bord0";
                        alpha = Math.Min(0xD0, (int)(pos * 200));
                    }

                    string tags =
                        $"\\an5\\1c{color}{extra}\\alpha&H{alpha:X2}&\\shad0\\fs{fs}" +
                        $"\\move({colX},-20,{colX},{h + 20})";

                    entry.Text = "{" + tags + "}" + ch;
                    result.Add(entry);
                }

                streamStart += fallDur + rng.Next(100, 2500);
            }
        }

        // ── SUBTITLE CHARACTERS FALL INTO POSITION ───────────────────────────────────────
        // Each character falls the full screen height in two seamless halves:
        //   Part 1 — scrambled green, falls from top to the reveal point
        //   Part 2 — actual white char, continues falling from the same Y to the bottom
        // The reveal fraction varies left-to-right, creating a diagonal sweep as the text descends.
        for (int subIdx = 0; subIdx < subtitles.Count; subIdx++)
        {
            var sub = subtitles[subIdx];
            // With preRoll applied, every subtitle's chars land at restY at that subtitle's StartTime.
            // So we only need the raw gap to the next subtitle to know when to clear old text.
            double nextSubOffsetMs = subIdx + 1 < subtitles.Count
                ? (subtitles[subIdx + 1].StartTime - sub.StartTime).TotalMilliseconds
                : double.MaxValue;
            var cleanText = Utilities.RemoveSsaTags(sub.Text)
                .Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ").Trim();
            if (string.IsNullOrEmpty(cleanText))
            {
                result.Add(sub);
                continue;
            }

            var chars = cleanText.ToCharArray();
            int charCount = chars.Length;
            double fallDuration = sub.Duration.TotalMilliseconds;

            var subRng = new Random(sub.Text.GetHashCode());
            // preRollMs: shift every animation backwards so each char arrives at restY at sub.StartTime.
            // restStartMs = fallDuration × (h×0.88 + 20) / (h + 40) for any revealFrac, so this is constant.
            double preRollMs = fallDuration * (h * 0.88 + 20.0) / (h + 40.0);
            var scrambled = new char[charCount];
            for (int k = 0; k < charCount; k++)
                scrambled[k] = chars[k] == ' ' ? ' ' : MatrixPool[subRng.Next(MatrixPool.Length)];

            int startCol = Math.Max(0, (cols - charCount) / 2);
            int visibleCount = chars.Count(c => c != ' ');

            // Reveal wave spans from 55 % to 80 % of the fall — lower-screen region
            const double revealFracStart = 0.55;
            const double revealFracEnd = 0.80;

            int visIdx = 0;
            for (int i = 0; i < charCount; i++)
            {
                int colIdx = Math.Min(startCol + i, cols - 1);
                int charX = colIdx * colW + colW / 2;

                if (chars[i] == ' ') continue;

                double revealFrac = visibleCount > 1
                    ? revealFracStart + (revealFracEnd - revealFracStart) * visIdx / (visibleCount - 1)
                    : (revealFracStart + revealFracEnd) / 2.0;

                double charRevealMs = fallDuration * revealFrac;
                int revealY = (int)(-20 + (h + 40) * revealFrac); // Y exactly at the reveal moment

                // Part 1: scrambled green char falls from top to revealY (starts before sub.StartTime)
                var falling = new SubtitleLineViewModel(sub, generateNewId: true);
                falling.StartTime = sub.StartTime.Add(TimeSpan.FromMilliseconds(-preRollMs));
                falling.EndTime = sub.StartTime.Add(TimeSpan.FromMilliseconds(charRevealMs - preRollMs));
                falling.Text = $"{{\\an5\\1c{BrightGreen}\\blur3\\bord0\\shad0\\fs{fs}" +
                               $"\\move({charX},-20,{charX},{revealY})}}" + scrambled[i];
                result.Add(falling);

                // Part 2: actual char falls from revealY down to restY (with flash on reveal)
                int restY = (int)(h * 0.88); // resting line — near bottom, subtitle-style
                double fallToRestMs = (restY - revealY) * fallDuration / (h + 40.0);
                double restStartMs = charRevealMs + fallToRestMs;

                if (fallToRestMs > 0)
                {
                    var revealed = new SubtitleLineViewModel(sub, generateNewId: true);
                    revealed.StartTime = sub.StartTime.Add(TimeSpan.FromMilliseconds(charRevealMs - preRollMs));
                    revealed.EndTime = sub.StartTime; // arrives at restY exactly at subtitle start time
                    int flashDur = Math.Min(300, (int)fallToRestMs);
                    revealed.Text = $"{{\\an5\\1c{HeadColor}\\blur3\\bord1\\shad0\\fs{fs}" +
                                    $"\\t(0,{flashDur},\\1c&HFFFFFF&\\blur1)" +
                                    $"\\move({charX},{revealY},{charX},{restY})}}" + chars[i];
                    result.Add(revealed);
                }

                // Part 3: char rests at restY until subtitle ends so the text is readable
                if (restStartMs < fallDuration)
                {
                    var resting = new SubtitleLineViewModel(sub, generateNewId: true);
                    resting.StartTime = sub.StartTime; // all chars land at restY at subtitle start time
                    resting.EndTime = sub.StartTime.Add(TimeSpan.FromMilliseconds(
                        Math.Min(2000, nextSubOffsetMs)));
                    resting.Text = $"{{\\an5\\pos({charX},{restY})\\1c&HFFFFFF&\\blur0.5\\bord1\\shad0\\fs{fs}}}" + chars[i];
                    result.Add(resting);
                }

                visIdx++;
            }
        }

        return result;
    }
}

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

    public List<SubtitleLineViewModel> ApplyEffect(List<SubtitleLineViewModel> subtitles, int width, int height, WavePeakData2? wavePeaks)
    {
        var result = new List<SubtitleLineViewModel>();
        if (subtitles.Count == 0) return result;

        Random rng = new Random(subtitles[0].Text.GetHashCode());

        int w = width > 0 ? width : 1280;
        int h = height > 0 ? height : 720;

        var globalStart = subtitles.Min(s => s.StartTime);
        var globalEnd = subtitles.Max(s => s.EndTime);
        double totalVideoMs = (globalEnd - globalStart).TotalMilliseconds;

        // Warm yellow-amber palette (ASS BGR: B=0, G=varies, R=255 = yellow-to-amber range)
        string[] coreColors = { "&H00FFFF&", "&H00E8FF&", "&H00D7FF&", "&H00C0FF&" };

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
        List<SubtitleLineViewModel> subtitles, int width, int height, WavePeakData2? wavePeaks)
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

    public List<SubtitleLineViewModel> ApplyEffect(List<SubtitleLineViewModel> subtitles, int width, int height, WavePeakData2? wavePeaks)
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

    public List<SubtitleLineViewModel> ApplyEffect(List<SubtitleLineViewModel> subtitles, int width, int height, WavePeakData2? wavePeaks)
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

public class AdvancedEffectWordSpacing : IAdvancedEffectDisplay
{
    public string Name => Se.Language.Assa.AdvancedEffectWordSpacing;
    public string Description => Se.Language.Assa.AdvancedEffectWordSpacingDescription;
    public bool UsesAudio => false;
    public decimal SpacingPixels { get; set; } = 10m;

    public override string ToString() => Name;

    public List<SubtitleLineViewModel> ApplyEffect(List<SubtitleLineViewModel> subtitles, int width, int height, WavePeakData2? wavePeaks)
    {
        var result = new List<SubtitleLineViewModel>();

        foreach (var sub in subtitles)
        {
            var newSub = new SubtitleLineViewModel(sub, generateNewId: true);
            
            // Process the text to add \fsp tag for spaces
            string processedText = ProcessTextWithSpacing(sub.Text);
            newSub.Text = processedText;
            
            result.Add(newSub);
        }

        return result;
    }

    private string ProcessTextWithSpacing(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        var result = new StringBuilder();
        bool insideTags = false;
        
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            
            if (c == '{')
            {
                insideTags = true;
                result.Append(c);
            }
            else if (c == '}')
            {
                insideTags = false;
                result.Append(c);
            }
            else if (c == ' ' && !insideTags)
            {
                // Replace space with tagged space using \fsp
                result.Append($"{{\\fsp{SpacingPixels}}} {{\\fsp0}}");
            }
            else
            {
                result.Append(c);
            }
        }
        
        return result.ToString();
    }
}
