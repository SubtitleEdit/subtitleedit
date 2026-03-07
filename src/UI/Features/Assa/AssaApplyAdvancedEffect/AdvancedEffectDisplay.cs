using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect;

public interface IAdvancedEffectDisplay
{
    string Name { get; }
    string Description { get; }
    List<SubtitleLineViewModel> ApplyEffect(List<SubtitleLineViewModel> subtitles, int width, int height);
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
            new AdvancedEffectShow(),
            new AdvancedEffectOldMovie(),
            new AdvancedEffectTest(),
        }.OrderBy(p => p.Name).ToList();
    }
}

public class AdvancedEffectTest : IAdvancedEffectDisplay
{
    public string Name => Se.Language.Assa.AdvancedEffectNeonBurst;
    public string Description => Se.Language.Assa.AdvancedEffectNeonBurstDescription;

    public override string ToString() => Name;

    public List<SubtitleLineViewModel> ApplyEffect(List<SubtitleLineViewModel> subtitles, int width, int height)
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
                Text = "{\\an5\\bord5\\blur8\\shad0\\1c" + chosenColor + "\\3c" + chosenColor + "\\alpha&H60&" +
                       GetPopTags(0, 150) + "}" + HtmlUtil.RemoveHtmlTags(sub.Text, true) // CleanText removes existing tags
            };

            // 3. CREATE THE SHARP TOP LAYER (The 'Core')
            var coreLayer = new SubtitleLineViewModel()
            {
                StartTime = sub.StartTime,
                EndTime = sub.EndTime,
                // Sharp white core with a thin neon border
                Text = "{\\an5\\bord2\\blur0.5\\shad0\\1c&HFFFFFF&\\3c" + chosenColor +
                       GetPopTags(0, 150) + "}" + HtmlUtil.RemoveHtmlTags(sub.Text, true)
            };

            result.Add(glowLayer);
            result.Add(coreLayer);
        }

        return result;
    }

    // Helper method to create the 'Pop' animation
    private string GetPopTags(int startOffset, int duration)
    {
        // Starts at 125% size and shrinks down to 100% quickly
        return $"\\fscx125\\fscy125\\t({startOffset},{duration},\\fscx100\\fscy100)";
    }

}

public class AdvancedEffectOldMovie : IAdvancedEffectDisplay
{
    public string Name => Se.Language.Assa.AdvancedEffectOldMovie;
    public string Description => Se.Language.Assa.AdvancedEffectOldMovieDescription;

    public override string ToString() => Name;

    public List<SubtitleLineViewModel> ApplyEffect(List<SubtitleLineViewModel> subtitles, int width, int height)
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

public class AdvancedEffectShow : IAdvancedEffectDisplay
{
    public string Name => Se.Language.Assa.AdvancedEffectSnow;
    public string Description => Se.Language.Assa.AdvancedEffectSnowDescription;

    public override string ToString() => Name;

    public List<SubtitleLineViewModel> ApplyEffect(List<SubtitleLineViewModel> subtitles, int width, int height)
    {
        var result = new List<SubtitleLineViewModel>();
        if (subtitles.Count == 0) return result;

        Random rng = new Random(subtitles[0].Text.GetHashCode());

        int screenWidth = width > 0 ? width : 1280;
        int screenHeight = height > 0 ? height : 720;

        var globalStart = subtitles.Min(s => s.StartTime);
        var globalEnd = subtitles.Max(s => s.EndTime);
        double totalVideoMs = (globalEnd - globalStart).TotalMilliseconds;

        int flakeCount = 200;

        for (int i = 0; i < flakeCount; i++)
        {
            // INITIAL DELAY: This creates the "start slow" effect. 
            // Higher index flakes start much later in the video.
            double currentTimeMs = rng.Next(0, 5000) + (i * 20);

            while (currentTimeMs < totalVideoMs)
            {
                var flake = new SubtitleLineViewModel();

                int layerRoll = rng.Next(0, 100);
                int fallDuration, size, alpha;
                double blur;

                // --- REVISED SNOW SIZES (Smaller & Lighter) ---
                if (layerRoll < 70) // BACKGROUND - 70% of snow, tiny specks
                {
                    fallDuration = rng.Next(10000, 15000);
                    size = rng.Next(10, 25); // Was 20-40
                    blur = 0.3;
                    alpha = 180;
                }
                else if (layerRoll < 95) // MIDGROUND - Gentle flakes
                {
                    fallDuration = rng.Next(6000, 9000);
                    size = rng.Next(30, 50); // Was 50-80
                    blur = 1.0;
                    alpha = 100;
                }
                else // FOREGROUND - Out of focus "Close" flakes
                {
                    fallDuration = rng.Next(4000, 6000);
                    size = rng.Next(80, 130); // Was 150-250
                    blur = 4.0;
                    alpha = 60;
                }

                flake.StartTime = globalStart.Add(TimeSpan.FromMilliseconds(currentTimeMs));
                flake.EndTime = flake.StartTime.Add(TimeSpan.FromMilliseconds(fallDuration));

                if (flake.StartTime >= globalEnd) break;

                int startX = rng.Next(-100, screenWidth + 100);
                int endX = startX + rng.Next(20, 80); // Drift

                // ROTATION: Adding a slow spin for more 3D realism
                int startRotation = rng.Next(0, 360);
                int endRotation = startRotation + rng.Next(-180, 180);

                string hexAlpha = alpha.ToString("X2");
                string tags = $@"\\an5\\bord0\\shad0\\blur{blur:F1}\\1c&HFFFFFF&\\alpha&H{hexAlpha}&\\fad(1200,1200)" +
                              $@"\\fscx{size}\\fscy{size}\\frz{startRotation}\\t(0,{fallDuration},\\frz{endRotation})" +
                              $@"\\move({startX}, -50, {endX}, {screenHeight + 50})";

                flake.Text = "{" + tags + "}•";
                result.Add(flake);

                currentTimeMs += fallDuration;
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

    public override string ToString() => Name;

    public List<SubtitleLineViewModel> ApplyEffect(List<SubtitleLineViewModel> subtitles, int width, int height)
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

    public override string ToString() => Name;

    public List<SubtitleLineViewModel> ApplyEffect(List<SubtitleLineViewModel> subtitles, int width, int height)
    {
        var result = new List<SubtitleLineViewModel>();
        if (subtitles.Count == 0) return result;

        Random rng = new Random(subtitles[0].Text.GetHashCode());

        int screenWidth = width > 0 ? width : 1280;
        int screenHeight = height > 0 ? height : 720;
        int centerX = screenWidth / 2;
        int centerY = screenHeight / 2;

        var globalStart = subtitles.Min(s => s.StartTime);
        var globalEnd = subtitles.Max(s => s.EndTime);
        double totalVideoMs = (globalEnd - globalStart).TotalMilliseconds;

        // 1. INCREASED DENSITY
        int starCount = 300;

        for (int i = 0; i < starCount; i++)
        {
            double currentTimeMs = 0;

            while (currentTimeMs < totalVideoMs)
            {
                var star = new SubtitleLineViewModel();

                // Randomize how long this specific star takes to cross the screen
                int lifeSpan = rng.Next(3500, 6500); // Faster speed feels more like warp

                // Timing
                star.StartTime = globalStart.Add(TimeSpan.FromMilliseconds(currentTimeMs));
                star.EndTime = star.StartTime.Add(TimeSpan.FromMilliseconds(lifeSpan));

                // Ensure we don't exceed the video end
                if (star.StartTime >= globalEnd) break;
                if (star.EndTime > globalEnd) star.EndTime = globalEnd;

                // PHYSICS
                double angle = rng.NextDouble() * 2 * Math.PI;
                int travelDist = 3000; // Far off-screen

                // First Wave logic to fill the screen instantly at 0:00
                double startProg = (currentTimeMs == 0) ? rng.NextDouble() : 0;

                int startX = centerX + (int)(Math.Cos(angle) * (travelDist * startProg));
                int startY = centerY + (int)(Math.Sin(angle) * (travelDist * startProg));
                int endX = centerX + (int)(Math.Cos(angle) * travelDist);
                int endY = centerY + (int)(Math.Sin(angle) * travelDist);

                // STYLING & SPECTRA
                int starType = rng.Next(0, 10);
                bool isGiant = starType >= 8;
                string color = isGiant ? (rng.Next(0, 2) == 0 ? "&HFFDADA&" : "&HACE6FF&") : "&HFFFFFF&";

                // STREAK EFFECT: Use rapid movement between two coordinates to create "blur"
                int blurX1 = startX - 3; // Shift by 3 pixels
                int blurX2 = startX + 3;
                string motionBlur = $"\\t({(lifeSpan / 2)},{lifeSpan},\\move({blurX1},{startY},{blurX2},{startY},0,20))";

                // Fade and scaling (Unlinked fscx/y)
                int fadeIn = (startProg == 0) ? 1000 : 0;
                int initialSize = isGiant ? rng.Next(25, 40) : rng.Next(8, 18);

                // \fscx(startScaleX): The horizontal width (stretches away from center)
                // \fscy(startScaleY): The constant vertical height (star's 'thickness')
                string tags = $@"\\an5\\bord0\\shad0\\blur1\\1c{color}\\fad({fadeIn},200)" +
                              $@"\\fscx{initialSize}\\fscy{initialSize}" +
                              $@"\\t(0,{lifeSpan},\\fscx{(isGiant ? 800 : 400)}\\fscy{(isGiant ? 20 : 10)})" +
                              $@"\\move({startX},{startY},{endX},{endY})" +
                              motionBlur; // Apply the motion blur oscillation last

                star.Text = "{" + tags + "}·";
                result.Add(star);

                // Stagger the rebirth of stars so they don't all pop in at once
                currentTimeMs += (lifeSpan * (1.0 - startProg)) + rng.Next(0, 300);
            }
        }

        // Dialogue on top
        result.AddRange(subtitles);
        return result;
    }
}

public class AdvancedEffectEndCreditsScroll : IAdvancedEffectDisplay
{
    public string Name => Se.Language.Assa.AdvancedEffectEndCreditsScroll;
    public string Description => Se.Language.Assa.AdvancedEffectEndCreditsScrollDescription;

    public override string ToString() => Name;

    public List<SubtitleLineViewModel> ApplyEffect(List<SubtitleLineViewModel> subtitles, int width, int height)
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
                    $@"\an8\b1\fs52\fsp2\blur0.6\bord1.5\shad1\fad(300,300)" +
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

    public override string ToString() => Name;

    public List<SubtitleLineViewModel> ApplyEffect(List<SubtitleLineViewModel> subtitles, int width, int height)
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

    public override string ToString() => Name;

    public List<SubtitleLineViewModel> ApplyEffect(List<SubtitleLineViewModel> subtitles, int width, int height)
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

    public override string ToString() => Name;

    public List<SubtitleLineViewModel> ApplyEffect(List<SubtitleLineViewModel> subtitles, int width, int height)
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

    public override string ToString() => Name;

    public List<SubtitleLineViewModel> ApplyEffect(List<SubtitleLineViewModel> subtitles, int width, int height)
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

    public override string ToString() => Name;

    public List<SubtitleLineViewModel> ApplyEffect(List<SubtitleLineViewModel> subtitles, int width, int height)
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

    public override string ToString() => Name;

    public List<SubtitleLineViewModel> ApplyEffect(List<SubtitleLineViewModel> subtitles, int width, int height)
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

    public override string ToString() => Name;

    public List<SubtitleLineViewModel> ApplyEffect(List<SubtitleLineViewModel> subtitles, int width, int height)
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

    private const string HighlightColor = @"{\1c&HFFFFFF&}";
    private const string DimColor = @"{\1c&H808080&}";

    public override string ToString() => Name;

    public List<SubtitleLineViewModel> ApplyEffect(List<SubtitleLineViewModel> subtitles, int width, int height)
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

    private static readonly char[] ScramblePool =
        "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789#@$%&".ToCharArray();

    public override string ToString() => Name;

    public List<SubtitleLineViewModel> ApplyEffect(List<SubtitleLineViewModel> subtitles, int width, int height)
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