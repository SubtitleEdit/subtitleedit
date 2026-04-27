using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect.Effects;

/// <summary>
/// Reveals text one word at a time: produces N sequential subtitle entries where entry i
/// shows the first i+1 words and spans 1/N of the original duration.
/// </summary>
public class AdvancedEffectWordByWord : IAdvancedEffectDisplay
{
    public string Name => Se.Language.Assa.AdvancedEffectWordByWord;
    public string Description => Se.Language.Assa.AdvancedEffectWordByWordDescription;
    public bool UsesAudio => false;
    public override string ToString() => Name;

    public List<SubtitleLineViewModel> ApplyEffect(string header, List<SubtitleLineViewModel> subtitles, int width, int height, WavePeakData2? wavePeaks)
    {
        var result = new List<SubtitleLineViewModel>();
        foreach (var subtitle in subtitles)
        {
            var cleanText = Utilities.RemoveSsaTags(subtitle.Text)
                .Replace("\r\n", " ")
                .Replace("\r", " ")
                .Replace("\n", " ");

            if (string.IsNullOrWhiteSpace(cleanText))
            {
                result.Add(subtitle);
                continue;
            }

            var words = cleanText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length <= 1)
            {
                result.Add(subtitle);
                continue;
            }

            var totalMs = subtitle.Duration.TotalMilliseconds;
            var msPerWord = totalMs / words.Length;

            for (var i = 0; i < words.Length; i++)
            {
                var line = new SubtitleLineViewModel(subtitle, generateNewId: true);
                var start = subtitle.StartTime.Add(TimeSpan.FromMilliseconds(i * msPerWord));
                var end = i == words.Length - 1
                    ? subtitle.EndTime
                    : subtitle.StartTime.Add(TimeSpan.FromMilliseconds((i + 1) * msPerWord));
                line.StartTime = start;
                line.EndTime = end;
                line.Text = string.Join(" ", words, 0, i + 1);
                result.Add(line);
            }
        }
        return result;
    }
}

