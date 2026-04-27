using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect.Effects;

/// <summary>
/// Karaoke-style progressive color reveal: produces N sequential subtitle entries where entry i
/// has characters 0..i in white and the remaining characters dimmed — batching consecutive
/// same-color characters to keep the output compact.
/// </summary>
public class AdvancedEffectKaraoke : IAdvancedEffectDisplay
{
    public string Name => Se.Language.Assa.AdvancedEffectKaraoke;
    public string Description => Se.Language.Assa.AdvancedEffectKaraokeDescription;
    public bool UsesAudio => false;

    private const string HighlightColor = @"{\1c&HFFFFFF&}";
    private const string DimColor = @"{\1c&H808080&}";

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

            var chars = cleanText
                .Replace("\r\n", "\n")
                .Replace("\r", "\n")
                .ToCharArray();
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
                line.Text = BuildKaraokeText(chars, i);
                result.Add(line);
            }
        }
        return result;
    }

    private static string BuildKaraokeText(char[] chars, int highlightUpTo)
    {
        var sb = new StringBuilder(chars.Length * 12);
        string? currentColor = null;

        for (var j = 0; j < chars.Length; j++)
        {
            if (chars[j] == '\n')
            {
                sb.Append("\\N");
                currentColor = null;
                continue;
            }

            var color = j <= highlightUpTo ? HighlightColor : DimColor;
            if (color != currentColor)
            {
                sb.Append(color);
                currentColor = color;
            }
            sb.Append(chars[j]);
        }

        return sb.ToString();
    }
}

