using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect.Effects;

/// <summary>
/// Word flip 3D: each word folds in around the horizontal axis (\frx 90 -> 0) with a small
/// elastic pop as it is spoken - one sequential event per word, so nothing overlaps and the
/// line never reflows (upcoming words hold their space as invisible placeholders).
/// </summary>
public class AdvancedEffectWordFlip3D : IAdvancedEffectDisplay
{
    public string Name => Se.Language.Assa.AdvancedEffectWordFlip3D;
    public string Description => Se.Language.Assa.AdvancedEffectWordFlip3DDescription;
    public bool UsesAudio => false;

    private static readonly Regex TokenRegex = new(@"\S+|\s+", RegexOptions.Compiled);

    public override string ToString() => Name;

    public List<SubtitleLineViewModel> ApplyEffect(
        string header, List<SubtitleLineViewModel> subtitles, int width, int height, WavePeakData2? wavePeaks)
    {
        var result = new List<SubtitleLineViewModel>();
        var sb = new StringBuilder();
        var plan = MarkedReveal.Plan(subtitles);

        for (var n = 0; n < subtitles.Count; n++)
        {
            var sub = subtitles[n];
            var reveal = plan?[n];
            var cleanText = plan == null ? Utilities.RemoveSsaTags(sub.Text) : reveal?.Text ?? string.Empty;
            var totalMs = sub.Duration.TotalMilliseconds;
            if (string.IsNullOrWhiteSpace(cleanText) || totalMs <= 0)
            {
                result.Add(AdvancedEffectUtil.PassThrough(sub));
                continue;
            }

            cleanText = cleanText.Replace("\r\n", "\n").Replace("\r", "\n");
            var tokens = TokenRegex.Matches(cleanText).Select(m => m.Value).ToList();
            var wordCount = tokens.Count(t => !string.IsNullOrWhiteSpace(t));
            if (wordCount == 0)
            {
                result.Add(AdvancedEffectUtil.PassThrough(sub));
                continue;
            }

            // a line per word with the active word marked: one event that flips in just that word,
            // with the earlier words shown (a pause flips nothing)
            if (reveal != null && reveal.ShownWords == 0 && reveal.IsPause)
            {
                continue; // nothing spoken yet
            }

            var steps = reveal == null ? wordCount : 1;
            var msPerWord = totalMs / steps;
            string posTags = AdvancedEffectUtil.ExtractPositionalTags(sub.Text);

            // Flip timing scaled to the word window: fold in over the first ~45%,
            // overshoot slightly, then settle.
            int flipMs = (int)Math.Min(320, msPerWord);
            int foldEndMs = (int)(flipMs * 0.45);
            int overshootEndMs = (int)(flipMs * 0.75);
            string flipTags =
                "{\\alpha&H00&\\frx90\\fscy80" +
                $"\\t(0,{foldEndMs},\\frx0\\fscy106)" +
                $"\\t({foldEndMs},{overshootEndMs},\\fscy98)" +
                $"\\t({overshootEndMs},{flipMs},\\fscy100)}}";

            // Placeholders reset the animated tags so the flip never leaks into
            // neighbouring words, and hidden words keep the line layout stable.
            const string shownTags = "{\\alpha&H00&\\frx0\\fscy100}";
            const string hiddenTags = "{\\alpha&HFF&\\frx0\\fscy100}";

            for (int step = 0; step < steps; step++)
            {
                var w = reveal == null ? step : reveal.ShownWords;
                var flipWord = reveal == null || !reveal.IsPause ? w : -1;
                var line = new SubtitleLineViewModel(sub, generateNewId: true);
                line.StartTime = sub.StartTime.Add(TimeSpan.FromMilliseconds(step * msPerWord));
                line.EndTime = step == steps - 1
                    ? sub.EndTime
                    : sub.StartTime.Add(TimeSpan.FromMilliseconds((w + 1) * msPerWord));

                sb.Clear();
                if (!string.IsNullOrEmpty(posTags))
                {
                    // Keep one continuous motion across the word-events (see AdjustMoveForSegment)
                    sb.Append(AdvancedEffectUtil.AdjustMoveForSegment(
                        posTags, step * msPerWord, (line.EndTime - line.StartTime).TotalMilliseconds, totalMs));
                }

                string? currentTags = null;
                int wordIndex = 0;
                foreach (var token in tokens)
                {
                    if (string.IsNullOrWhiteSpace(token))
                    {
                        if (token.Contains('\n'))
                        {
                            sb.Append(token.Replace("\n", "\\N"));
                            currentTags = null; // re-emit state after a line break
                        }
                        else
                        {
                            sb.Append(token);
                        }
                        continue;
                    }

                    var tags = wordIndex < w ? shownTags : wordIndex == flipWord ? flipTags : hiddenTags;
                    if (tags != currentTags)
                    {
                        sb.Append(tags);
                        currentTags = tags;
                    }
                    sb.Append(token);
                    wordIndex++;
                }

                line.Text = sb.ToString();
                result.Add(line);
            }
        }

        return result;
    }
}
