using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
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
        };
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