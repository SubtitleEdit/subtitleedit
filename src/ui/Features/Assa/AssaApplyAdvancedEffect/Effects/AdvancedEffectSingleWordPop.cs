using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect.Effects;

/// <summary>
/// CapCut-style single word captions: one big word at a time, popping in (scale overshoot)
/// as it is spoken.
/// </summary>
public class AdvancedEffectSingleWordPop : IAdvancedEffectDisplay
{
    public string Name => Se.Language.Assa.AdvancedEffectSingleWordPop;
    public string Description => Se.Language.Assa.AdvancedEffectSingleWordPopDescription;
    public bool UsesAudio => false;

    public Color TextColor { get; set; } = Color.FromRgb(0xFF, 0xFF, 0xFF);
    public bool UpperCase { get; set; } = true;
    public int TextSizePercent { get; set; } = 150;

    public override string ToString() => Name;

    public List<SubtitleLineViewModel> ApplyEffect(string header, List<SubtitleLineViewModel> subtitles, int width, int height, WavePeakData2? wavePeaks)
    {
        var result = new List<SubtitleLineViewModel>();
        var color = AdvancedEffectUtil.ToAssColor(TextColor);
        var size = TextSizePercent is >= 50 and <= 500 ? TextSizePercent : 150; // transient spinner values
        foreach (var sub in subtitles)
        {
            var words = CaptionWords.Parse(sub, UpperCase);
            if (words == null)
            {
                result.Add(AdvancedEffectUtil.PassThrough(sub));
                continue;
            }

            var style = CaptionWords.GetStyle(header, sub);
            var posTags = AdvancedEffectUtil.ExtractPositionalTags(sub.Text);
            for (var w = 0; w < words.Words.Count; w++)
            {
                var slice = words.MakeSlice(sub, w, posTags, out var slicePosTags);
                var word = words.Tokens.Find(t => t.WordIndex == w)!.Text;
                slice.Text = slicePosTags +
                             "{\\1c" + color +
                             CaptionWords.Scale(style, size * 0.55) +
                             "\\t(0,90," + CaptionWords.Scale(style, size * 1.12) + ")" +
                             "\\t(90,180," + CaptionWords.Scale(style, size) + ")}" +
                             word;
                result.Add(slice);
            }
        }

        return result;
    }
}
