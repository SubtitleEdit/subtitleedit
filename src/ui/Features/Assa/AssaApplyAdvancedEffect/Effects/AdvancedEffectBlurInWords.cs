using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect.Effects;

/// <summary>
/// Words appear one after another as they are spoken: each starts transparent and blurred,
/// then sharpens and rises from the baseline with a small overshoot. One event per line - the
/// per-word \t times are relative to the line start.
/// </summary>
public class AdvancedEffectBlurInWords : IAdvancedEffectDisplay
{
    public string Name => Se.Language.Assa.AdvancedEffectBlurInWords;
    public string Description => Se.Language.Assa.AdvancedEffectBlurInWordsDescription;
    public bool UsesAudio => false;

    public bool Rise { get; set; } = true;
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

            var style = CaptionWords.GetStyle(header, sub);
            var blur = AdvancedEffectUtil.Tag(Math.Max(3, (double)style.FontSize * 0.12));
            sb.Clear();
            sb.Append(AdvancedEffectUtil.ExtractPositionalTags(sub.Text));
            foreach (var token in words.Tokens)
            {
                if (token.IsWord)
                {
                    var start = (int)Math.Round(words.WordStartMs[token.WordIndex]);
                    var length = (int)Math.Clamp(words.WordStartMs[token.WordIndex + 1] - start, 120, 280);
                    sb.Append("{\\alpha&HFF&\\blur").Append(blur);
                    if (Rise)
                    {
                        sb.Append("\\fscy").Append(AdvancedEffectUtil.Tag((double)style.ScaleY * 0.4))
                          .Append("\\t(").Append(start).Append(',').Append(start + length).Append(",\\alpha&H00&\\blur0\\fscy").Append(AdvancedEffectUtil.Tag((double)style.ScaleY * 1.12)).Append(')')
                          .Append("\\t(").Append(start + length).Append(',').Append(start + length + 100).Append(",\\fscy").Append(AdvancedEffectUtil.Tag((double)style.ScaleY)).Append(')');
                    }
                    else
                    {
                        sb.Append("\\t(").Append(start).Append(',').Append(start + length).Append(",\\alpha&H00&\\blur0)");
                    }

                    sb.Append('}');
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
