using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect.Effects;

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

    public List<SubtitleLineViewModel> ApplyEffect(string header, List<SubtitleLineViewModel> subtitles, int width, int height, WavePeakData2? wavePeaks)
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

