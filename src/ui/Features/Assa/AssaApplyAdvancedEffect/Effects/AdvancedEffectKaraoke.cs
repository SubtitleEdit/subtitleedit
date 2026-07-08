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

    /// <summary>
    /// When true, reveal the characters from the end of the text towards the start so the
    /// highlight progresses visually right-to-left. Auto-enabled for right-to-left languages
    /// (Hebrew, Arabic, ...) where the default left-to-right reveal runs backwards. (#12271)
    /// </summary>
    public bool RightToLeft { get; set; }

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
                line.Text = BuildKaraokeText(chars, i, RightToLeft);
                result.Add(line);
            }
        }
        return result;
    }

    private static string BuildKaraokeText(char[] chars, int step, bool rightToLeft)
    {
        var sb = new StringBuilder(chars.Length * 12);
        string? currentColor = null;

        // step is 0-based, so step+1 characters are revealed. Left-to-right reveals the first
        // step+1 characters; right-to-left reveals the last step+1 characters instead.
        var revealFromEnd = chars.Length - 1 - step;

        for (var j = 0; j < chars.Length; j++)
        {
            if (chars[j] == '\n')
            {
                sb.Append("\\N");
                currentColor = null;
                continue;
            }

            var highlight = rightToLeft ? j >= revealFromEnd : j <= step;
            var color = highlight ? HighlightColor : DimColor;
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

