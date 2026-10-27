using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect.Effects;

/// <summary>
/// CapCut/short-video "bold" captions: upper case, thick outline and shadow, and the spoken
/// word lights up in a highlight color and grows.
/// </summary>
public class AdvancedEffectBoldCaptions : IAdvancedEffectDisplay
{
    public string Name => Se.Language.Assa.AdvancedEffectBoldCaptions;
    public string Description => Se.Language.Assa.AdvancedEffectBoldCaptionsDescription;
    public bool UsesAudio => false;

    public Color HighlightColor { get; set; } = Color.FromRgb(0xFF, 0xD6, 0x0A);
    public bool UpperCase { get; set; } = true;

    public override string ToString() => Name;

    public List<SubtitleLineViewModel> ApplyEffect(string header, List<SubtitleLineViewModel> subtitles, int width, int height, WavePeakData2? wavePeaks)
    {
        var result = new List<SubtitleLineViewModel>();
        var highlight = AdvancedEffectUtil.ToAssColor(HighlightColor);
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
            var primary = CaptionWords.ToAssColor(style.Primary);
            var outline = AdvancedEffectUtil.Tag(Math.Max((double)style.OutlineWidth, (double)style.FontSize * 0.09));
            var shadow = AdvancedEffectUtil.Tag((double)style.FontSize * 0.05);
            var normalScale = CaptionWords.Scale(style, 100);
            var posTags = AdvancedEffectUtil.ExtractPositionalTags(sub.Text);
            for (var w = 0; w < words.Words.Count; w++)
            {
                var slice = words.MakeSlice(sub, w, posTags, out var slicePosTags);
                sb.Clear();
                sb.Append(slicePosTags)
                  .Append("{\\b1\\3c&H000000&\\4c&H000000&\\4a&H60&\\bord").Append(outline)
                  .Append("\\shad").Append(shadow).Append('}');
                foreach (var token in words.Tokens)
                {
                    if (token.WordIndex == w)
                    {
                        sb.Append("{\\1c").Append(highlight)
                          .Append(CaptionWords.Scale(style, 125)).Append("\\t(0,110,").Append(CaptionWords.Scale(style, 115)).Append(")}")
                          .Append(token.Text)
                          .Append("{\\1c").Append(primary).Append(normalScale).Append('}');
                    }
                    else
                    {
                        sb.Append(token.Text);
                    }
                }

                slice.Text = sb.ToString();
                result.Add(slice);
            }
        }

        return result;
    }
}
