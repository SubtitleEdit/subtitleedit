using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect.Effects;

/// <summary>
/// Reveals text one character at a time: produces N sequential subtitle entries where entry i
/// shows the first i+1 characters and spans 1/N of the original duration.
/// </summary>
public class AdvancedEffectTypewriter : IAdvancedEffectDisplay
{
    public string Name => Se.Language.Assa.AdvancedEffectTypewriter;
    public string Description => Se.Language.Assa.AdvancedEffectTypewriterDescription;
    public bool UsesAudio => false;

    public override string ToString() => Name;

    public List<SubtitleLineViewModel> ApplyEffect(string header, List<SubtitleLineViewModel> subtitles, int width, int height, WavePeakData2? wavePeaks)
    {
        var result = new List<SubtitleLineViewModel>();
        foreach (var subtitle in subtitles)
        {
            var cleanText = Utilities.RemoveSsaTags(subtitle.Text);
            if (string.IsNullOrEmpty(cleanText))
            {
                result.Add(subtitle);
                continue;
            }

            var chars = cleanText.ToCharArray();
            var charCount = chars.Length;
            var totalMs = subtitle.Duration.TotalMilliseconds;
            var msPerChar = totalMs / charCount;

            for (var i = 0; i < charCount; i++)
            {
                var line = new SubtitleLineViewModel(subtitle, generateNewId: true);
                var start = subtitle.StartTime.Add(TimeSpan.FromMilliseconds(i * msPerChar));
                var end = i == charCount - 1
                    ? subtitle.EndTime
                    : subtitle.StartTime.Add(TimeSpan.FromMilliseconds((i + 1) * msPerChar));
                line.StartTime = start;
                line.EndTime = end;
                line.Text = new string(chars, 0, i + 1)
                    .Replace("\r\n", "\\N")
                    .Replace("\r", "\\N")
                    .Replace("\n", "\\N");
                result.Add(line);
            }
        }
        return result;
    }
}

