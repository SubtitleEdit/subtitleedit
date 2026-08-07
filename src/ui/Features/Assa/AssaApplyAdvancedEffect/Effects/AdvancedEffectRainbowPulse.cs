using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect.Effects;

public class AdvancedEffectRainbowPulse : IAdvancedEffectDisplay
{
    public string Name => Se.Language.Assa.AdvancedEffectRainbowPulse;
    public string Description => Se.Language.Assa.AdvancedEffectRainbowPulseDescription;
    public bool UsesAudio => false;

    private static readonly System.Text.RegularExpressions.Regex TagOrTextRegex =
        new(@"(\{.*?\})|([^{]+)", System.Text.RegularExpressions.RegexOptions.Compiled);

    public override string ToString() => Name;

    public List<SubtitleLineViewModel> ApplyEffect(string header, List<SubtitleLineViewModel> subtitles, int width, int height, WavePeakData2? wavePeaks)
    {
        var result = new List<SubtitleLineViewModel>();
        string[] rainbowColors = ["&H0000FF&", "&H00FFFF&", "&H00FF00&", "&HFFFF00&", "&HFF0000&", "&HFF00FF&"];

        foreach (var sub in subtitles)
        {
            // 1. Split the text into parts (Tags vs Text)
            // This regex finds everything inside { } and everything outside.
            var matches = TagOrTextRegex.Matches(sub.Text);

            var fullParts = new List<(string Content, bool IsTag)>();
            foreach (System.Text.RegularExpressions.Match m in matches)
            {
                if (m.Value.StartsWith("{"))
                {
                    fullParts.Add((m.Value, true));
                }
                else
                {
                    fullParts.Add((m.Value, false));
                }
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
            int generated = 0;
            var colorCycle = new System.Text.StringBuilder();
            for (int i = 0; i < visibleChars.Count; i++)
            {
                var target = visibleChars[i];
                if (char.IsWhiteSpace(target.Character))
                {
                    continue;
                }

                var charLine = new SubtitleLineViewModel(sub, generateNewId: true);

                // All the per-character lines overlap in time at the same position, so each
                // needs its own ASSA layer: same-layer unpositioned events trigger libass
                // collision avoidance, which would stack them vertically instead of
                // rendering them on top of each other.
                charLine.Layer = sub.Layer + generated;

                // Rainbow logic
                int startColorIndex = i % rainbowColors.Length;
                colorCycle.Clear();
                for (int currentTime = 0, step = 1; currentTime < totalMs; currentTime += stepMs, step++)
                {
                    int nextIndex = (startColorIndex + step) % rainbowColors.Length;
                    colorCycle.Append($@"\t({currentTime},{currentTime + stepMs},\1c{rainbowColors[nextIndex]})");
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

                        finalString.Append(@"{\alpha&HFF&}").Append(left)
                                   .Append(@"{\alpha&H00&\1c").Append(rainbowColors[startColorIndex]).Append(colorCycle).Append('}')
                                   .Append(target.Character)
                                   .Append(@"{\alpha&HFF&}").Append(right);
                    }
                    else
                    {
                        // This part is text but doesn't have our active letter, hide it
                        finalString.Append(@"{\alpha&HFF&}").Append(fullParts[p].Content);
                    }
                }

                charLine.Text = finalString.ToString();
                result.Add(charLine);
                generated++;
            }

            if (generated == 0)
            {
                // Nothing to animate (empty/whitespace-only line) - keep the line as-is
                result.Add(AdvancedEffectUtil.PassThrough(sub));
            }
        }
        return result;
    }
}

