using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect.Effects;

public class AdvancedEffectWave : IAdvancedEffectDisplay
{
    public string Name => Se.Language.Assa.AdvancedEffectWave;
    public string Description => Se.Language.Assa.AdvancedEffectWaveDescription;
    public bool UsesAudio => false;

    private static readonly System.Text.RegularExpressions.Regex TagOrTextRegex =
        new(@"(\{.*?\})|([^{]+)", System.Text.RegularExpressions.RegexOptions.Compiled);

    public override string ToString() => Name;

    public List<SubtitleLineViewModel> ApplyEffect(string header, List<SubtitleLineViewModel> subtitles, int width, int height, WavePeakData2? wavePeaks)
    {
        var result = new List<SubtitleLineViewModel>();

        foreach (var sub in subtitles)
        {
            var matches = TagOrTextRegex.Matches(sub.Text);
            var fullParts = new List<(string Content, bool IsTag)>();
            foreach (System.Text.RegularExpressions.Match m in matches)
                fullParts.Add((m.Value, m.Value.StartsWith("{")));

            var visibleChars = new List<(char Character, int PartIndex, int CharIndex)>();
            for (int p = 0; p < fullParts.Count; p++)
                if (!fullParts[p].IsTag)
                    for (int c = 0; c < fullParts[p].Content.Length; c++)
                    {
                        var content = fullParts[p].Content;
                        if (content[c] == '\\' && c + 1 < content.Length && content[c + 1] is 'N' or 'n' or 'h')
                        {
                            // Inline \N/\n/\h stays intact in the rebuilt text but must never
                            // be animated as if it were two visible glyphs
                            c++;
                            continue;
                        }
                        visibleChars.Add((content[c], p, c));
                    }

            double totalMs = sub.Duration.TotalMilliseconds;

            // WAVE SETTINGS
            int waveSpeed = 200; // Time per "step" in the wave
            double waveFrequency = 0.4; // How many letters are "up" at once

            int generated = 0;
            var waveTransforms = new System.Text.StringBuilder();
            for (int i = 0; i < visibleChars.Count; i++)
            {
                var target = visibleChars[i];
                if (char.IsWhiteSpace(target.Character))
                {
                    continue;
                }

                var charLine = new SubtitleLineViewModel(sub, generateNewId: true);

                waveTransforms.Clear();
                // We create a rolling loop for the duration of the line
                for (int time = 0; time < totalMs; time += waveSpeed)
                {
                    // Calculate the "height" using a sine wave based on time AND character position
                    // This creates the "roll" effect.
                    double angle = (time / 200.0) - (i * waveFrequency);
                    double heightFactor = Math.Sin(angle); // Returns -1.0 to 1.0

                    // Map that to scale: 100% (flat) to 160% (peak)
                    int currentScale = 100 + (int)((heightFactor + 1) * 30);

                    waveTransforms.Append($@"\t({time},{time + waveSpeed},\fscy{currentScale})");
                }

                var finalString = new System.Text.StringBuilder();
                for (int p = 0; p < fullParts.Count; p++)
                {
                    if (fullParts[p].IsTag)
                    {
                        finalString.Append(fullParts[p].Content);
                    }
                    else if (p == target.PartIndex)
                    {
                        string partText = fullParts[p].Content;
                        // Force \an2 (bottom center) so the letters grow UPWARDS from the ground
                        finalString.Append(@"{\alpha&HFF&}").Append(partText.Substring(0, target.CharIndex))
                                   .Append(@"{\alpha&H00&").Append(waveTransforms).Append('}').Append(target.Character)
                                   .Append(@"{\alpha&HFF&}").Append(partText.Substring(target.CharIndex + 1));
                    }
                    else
                    {
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

