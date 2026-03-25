using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect.Effects;

public class AdvancedEffectWaveBlue : IAdvancedEffectDisplay
{
    public string Name => Se.Language.Assa.AdvancedEffectWaveBlue;
    public string Description => Se.Language.Assa.AdvancedEffectWaveBlueDescription;
    public bool UsesAudio => false;

    public override string ToString() => Name;

    public List<SubtitleLineViewModel> ApplyEffect(string header, List<SubtitleLineViewModel> subtitles, int width, int height, WavePeakData2? wavePeaks)
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
                        finalString.Append(@"{\alpha&HFF&}").Append(partText.Substring(0, target.CharIndex))
                                   .Append(@"{\alpha&H00&\1c&HFF8800&" + oceanTransforms + "}").Append(target.Character)
                                   .Append(@"{\alpha&HFF&}").Append(partText.Substring(target.CharIndex + 1));
                    }
                    else finalString.Append(@"{\alpha&HFF&}").Append(fullParts[p].Content);
                }

                charLine.Text = finalString.ToString();
                result.Add(charLine);
            }
        }
        return result;
    }
}

