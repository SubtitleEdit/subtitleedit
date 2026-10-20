using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect.Effects;

/// <summary>
/// Smooth karaoke fill: a highlight color sweeps through each word as it is spoken (\kf,
/// where the primary color is the "sung" part and the secondary color the rest).
/// </summary>
public class AdvancedEffectSmoothFill : IAdvancedEffectDisplay
{
    public string Name => Se.Language.Assa.AdvancedEffectSmoothFill;
    public string Description => Se.Language.Assa.AdvancedEffectSmoothFillDescription;
    public bool UsesAudio => false;

    public Color FillColor { get; set; } = Color.FromRgb(0xFF, 0xD6, 0x0A);
    public Color BaseColor { get; set; } = Color.FromRgb(0xFF, 0xFF, 0xFF);
    public bool UpperCase { get; set; }

    public override string ToString() => Name;

    public List<SubtitleLineViewModel> ApplyEffect(string header, List<SubtitleLineViewModel> subtitles, int width, int height, WavePeakData2? wavePeaks)
    {
        var result = new List<SubtitleLineViewModel>();
        var sb = new StringBuilder();
        foreach (var sub in subtitles)
        {
            var words = CaptionWords.Parse(sub, UpperCase);
            if (words == null)
            {
                result.Add(AdvancedEffectUtil.PassThrough(sub));
                continue;
            }

            sb.Clear();
            sb.Append(AdvancedEffectUtil.ExtractPositionalTags(sub.Text))
              .Append("{\\1c").Append(AdvancedEffectUtil.ToAssColor(FillColor))
              .Append("\\2c").Append(AdvancedEffectUtil.ToAssColor(BaseColor)).Append('}');

            // \kf durations are centiseconds; round the running total so the sweep ends with the line
            var doneCs = 0;
            foreach (var token in words.Tokens)
            {
                if (token.IsWord)
                {
                    var endCs = (int)Math.Round(words.WordStartMs[token.WordIndex + 1] / 10.0);
                    sb.Append("{\\kf").Append(Math.Max(0, endCs - doneCs)).Append('}');
                    doneCs = Math.Max(doneCs, endCs);
                }

                sb.Append(token.Text);
            }

            var line = AdvancedEffectUtil.PassThrough(sub);
            line.Text = sb.ToString();
            result.Add(line);
        }

        return result;
    }
}
