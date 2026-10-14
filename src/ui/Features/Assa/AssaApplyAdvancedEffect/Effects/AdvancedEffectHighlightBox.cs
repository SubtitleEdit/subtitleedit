using Avalonia.Media;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect.Effects;

/// <summary>
/// CapCut-style "highlight" captions: a solid colored box jumps from word to word as the line
/// is spoken. Each word slice is two events - a box layer in an opaque-box style
/// (BorderStyle 3, where libass draws one box per glyph with the glyph's own \3a, so only the
/// active word's box shows) under the normal text layer. Different layers never collide, so
/// both lay out identically.
/// </summary>
public class AdvancedEffectHighlightBox : IAdvancedEffectDisplay, IAdvancedEffectHeaderUpdater
{
    public string Name => Se.Language.Assa.AdvancedEffectHighlightBox;
    public string Description => Se.Language.Assa.AdvancedEffectHighlightBoxDescription;
    public bool UsesAudio => false;

    public Color BoxColor { get; set; } = Color.FromRgb(0x7C, 0x3A, 0xED);
    public Color ActiveTextColor { get; set; } = Color.FromRgb(0xFF, 0xFF, 0xFF);
    public bool UpperCase { get; set; }

    public override string ToString() => Name;

    public static string GetBoxStyleName(string styleName) => styleName + " - highlight box";

    public string UpdateHeader(string header, List<SubtitleLineViewModel> subtitles)
    {
        if (string.IsNullOrEmpty(header))
        {
            header = AdvancedSubStationAlpha.DefaultHeader;
        }

        var boxColor = new SKColor(BoxColor.R, BoxColor.G, BoxColor.B);
        foreach (var styleName in subtitles.Select(CaptionWords.GetStyleName).Distinct())
        {
            var style = AdvancedSubStationAlpha.GetSsaStyle(styleName, header);
            var box = new SsaStyle(style)
            {
                Name = GetBoxStyleName(styleName),
                BorderStyle = "3",
                OutlineWidth = Math.Max(1, Math.Round(style.FontSize * 0.14m, 1)),
                ShadowWidth = 0,
                Outline = boxColor,
                Background = boxColor,
            };
            header = AdvancedSubStationAlpha.UpdateOrAddStyle(header, box);
        }

        return header;
    }

    public List<SubtitleLineViewModel> ApplyEffect(string header, List<SubtitleLineViewModel> subtitles, int width, int height, WavePeakData2? wavePeaks)
    {
        var result = new List<SubtitleLineViewModel>();
        var sb = new StringBuilder();
        var boxColor = AdvancedEffectUtil.ToAssColor(BoxColor);
        var activeColor = AdvancedEffectUtil.ToAssColor(ActiveTextColor);
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
            var pop = CaptionWords.Scale(style, 110) + "\\t(0,120," + CaptionWords.Scale(style, 100) + ")";
            var normalScale = CaptionWords.Scale(style, 100);
            var posTags = AdvancedEffectUtil.ExtractPositionalTags(sub.Text);
            var boxStyle = GetBoxStyleName(CaptionWords.GetStyleName(sub));

            for (var w = 0; w < words.Words.Count; w++)
            {
                var box = words.MakeSlice(sub, w, posTags, out var slicePosTags);
                box.Style = boxStyle;
                box.Layer = sub.Layer;
                sb.Clear();
                sb.Append(slicePosTags).Append("{\\1a&HFF&\\3a&HFF&\\4a&HFF&\\3c").Append(boxColor).Append('}');
                foreach (var token in words.Tokens)
                {
                    if (token.WordIndex == w)
                    {
                        sb.Append("{\\3a&H00&").Append(pop).Append('}').Append(token.Text).Append("{\\3a&HFF&").Append(normalScale).Append('}');
                    }
                    else
                    {
                        sb.Append(token.Text);
                    }
                }

                box.Text = sb.ToString();
                result.Add(box);

                var text = words.MakeSlice(sub, w, posTags, out _);
                text.Layer = sub.Layer + 1;
                sb.Clear();
                sb.Append(slicePosTags);
                foreach (var token in words.Tokens)
                {
                    if (token.WordIndex == w)
                    {
                        sb.Append("{\\1c").Append(activeColor).Append(pop).Append('}').Append(token.Text)
                          .Append("{\\1c").Append(primary).Append(normalScale).Append('}');
                    }
                    else
                    {
                        sb.Append(token.Text);
                    }
                }

                text.Text = sb.ToString();
                result.Add(text);
            }
        }

        return result;
    }
}
