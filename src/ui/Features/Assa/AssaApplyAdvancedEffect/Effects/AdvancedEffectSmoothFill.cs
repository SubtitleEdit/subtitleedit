using Avalonia.Media;
using Nikse.SubtitleEdit.Core.Common;
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

    /// <summary>
    /// For right-to-left languages (Persian, Arabic, Hebrew, ...): keep the words in reading order
    /// and sweep the fill from the right edge of each word. Auto-enabled for RTL text.
    /// </summary>
    public bool RightToLeft { get; set; }

    // libass splits the text into separate bidi runs at every \kf, so RTL words come out in
    // left-to-right order; \fe-1 (libass 0.17+) lays out the whole line as one bidi paragraph.
    // libass always sweeps \kf from a word's left edge, except under a \frz of 90-270 degrees
    // where it sweeps from the right. \frx180\fry180 is itself a half turn around z, so
    // \frx180\fry180\frz(angle+180) renders at the style's own angle with the sweep reversed.
    // The rotation is set per word, so Latin words and numbers in a mixed line still fill
    // left to right.
    private static string FillDirectionTags(bool rightToLeft, decimal angle) => rightToLeft
        ? "\\frx180\\fry180\\frz" + AdvancedEffectUtil.Tag(((angle + 180) % 360 + 360) % 360)
        : "\\frx0\\fry0\\frz" + AdvancedEffectUtil.Tag(angle);

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

            var rightToLeft = RightToLeft && LanguageAutoDetect.ContainsRightToLeftLetter(sub.Text);
            var angle = rightToLeft ? CaptionWords.GetStyle(header, sub).Angle : 0;

            sb.Clear();
            sb.Append(AdvancedEffectUtil.ExtractPositionalTags(sub.Text))
              .Append("{\\1c").Append(AdvancedEffectUtil.ToAssColor(FillColor))
              .Append("\\2c").Append(AdvancedEffectUtil.ToAssColor(BaseColor))
              .Append(rightToLeft ? "\\fe-1" : string.Empty).Append('}');

            // \kf durations are centiseconds; round the running total so the sweep ends with the line
            var doneCs = 0;
            bool? wordRightToLeft = null;
            foreach (var token in words.Tokens)
            {
                if (token.IsWord)
                {
                    var endCs = (int)Math.Round(words.WordStartMs[token.WordIndex + 1] / 10.0);
                    sb.Append("{\\kf").Append(Math.Max(0, endCs - doneCs));
                    doneCs = Math.Max(doneCs, endCs);

                    if (rightToLeft)
                    {
                        var isRightToLeft = LanguageAutoDetect.ContainsRightToLeftLetter(token.Text);
                        if (isRightToLeft != wordRightToLeft)
                        {
                            sb.Append(FillDirectionTags(isRightToLeft, angle));
                            wordRightToLeft = isRightToLeft;
                        }
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
