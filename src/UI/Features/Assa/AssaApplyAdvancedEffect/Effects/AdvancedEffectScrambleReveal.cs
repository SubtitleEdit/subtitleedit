using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect.Effects;

/// <summary>
/// Scramble reveal: produces N sequential subtitle entries where entry i shows the correct
/// characters at positions 0..i and stable random placeholder characters at positions i+1..N-1.
/// The random seed is derived from the subtitle text so scrambling is deterministic.
/// </summary>
public class AdvancedEffectScrambleReveal : IAdvancedEffectDisplay
{
    public string Name => Se.Language.Assa.AdvancedEffectScrambleReveal;
    public string Description => Se.Language.Assa.AdvancedEffectScrambleRevealDescription;
    public bool UsesAudio => false;

    private static readonly char[] ScramblePool =
        "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789#@$%&".ToCharArray();

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

            var rng = new Random(subtitle.Text.GetHashCode());
            var scrambled = new char[charCount];
            for (var k = 0; k < charCount; k++)
            {
                scrambled[k] = chars[k] is '\n' or ' '
                    ? chars[k]
                    : ScramblePool[rng.Next(ScramblePool.Length)];
            }

            for (var i = 0; i < charCount; i++)
            {
                var line = new SubtitleLineViewModel(subtitle, generateNewId: true);
                var start = subtitle.StartTime.Add(TimeSpan.FromMilliseconds(i * msPerChar));
                var end = i == charCount - 1
                    ? subtitle.EndTime
                    : subtitle.StartTime.Add(TimeSpan.FromMilliseconds((i + 1) * msPerChar));
                line.StartTime = start;
                line.EndTime = end;
                line.Text = BuildScrambledText(chars, scrambled, i);
                result.Add(line);
            }
        }
        return result;
    }

    private static string BuildScrambledText(char[] real, char[] scrambled, int revealedUpTo)
    {
        var sb = new StringBuilder(real.Length);
        for (var j = 0; j < real.Length; j++)
        {
            if (real[j] == '\n')
            {
                sb.Append("\\N");
                continue;
            }
            sb.Append(j <= revealedUpTo ? real[j] : scrambled[j]);
        }
        return sb.ToString();
    }
}

