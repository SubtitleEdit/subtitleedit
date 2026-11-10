using Nikse.SubtitleEdit.Features.Main;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect.Effects;

/// <summary>
/// For effects that build a line up over time (typewriter, scramble reveal, word by word, ...)
/// in a file that marks the active word on each line, like the one-line-per-word output of
/// speech-to-text: a line reveals only its marked word, over the line's own duration, with the
/// earlier words already shown. A line with no marked word (a pause between words) holds what
/// the previous line of the same text showed.
/// </summary>
internal sealed class MarkedReveal
{
    /// <summary>The line's plain text, with '\n' line breaks.</summary>
    public required string Text { get; init; }

    public int WordCount { get; init; }

    /// <summary>Words fully shown from the line start.</summary>
    public int ShownWords { get; init; }

    /// <summary>The word revealed during this line, or -1 for a pause.</summary>
    public int ActiveWord { get; init; }

    /// <summary>Characters [0, CharFrom) are shown from the start, [CharFrom, CharTo) are revealed during the line.</summary>
    public int CharFrom { get; init; }

    public int CharTo { get; init; }

    public bool IsPause => ActiveWord < 0;

    /// <summary>
    /// One entry per line (null = nothing to reveal, pass the line through), or null when no line
    /// marks an active word and the effect should time the words itself.
    /// </summary>
    public static List<MarkedReveal?>? Plan(List<SubtitleLineViewModel> lines)
    {
        if (!CaptionWords.AnyMarked(lines))
        {
            return null;
        }

        var result = new List<MarkedReveal?>(lines.Count);
        MarkedReveal? previous = null;
        foreach (var line in lines)
        {
            var words = CaptionWords.Parse(line, upperCase: false, useMarks: true);
            if (words == null)
            {
                result.Add(null);
                continue;
            }

            var sb = new StringBuilder();
            var wordStart = new int[words.Words.Count];
            var wordEnd = new int[words.Words.Count];
            foreach (var token in words.Tokens)
            {
                if (token.IsWord)
                {
                    wordStart[token.WordIndex] = sb.Length;
                    sb.Append(token.Text);
                    wordEnd[token.WordIndex] = sb.Length;
                }
                else
                {
                    sb.Append(token.IsLineBreak ? "\n" : token.Text);
                }
            }

            var text = sb.ToString();
            MarkedReveal reveal;
            if (words.HasMarkedWord)
            {
                var k = words.MarkedWordIndex;
                reveal = new MarkedReveal
                {
                    Text = text,
                    WordCount = words.Words.Count,
                    ShownWords = k,
                    ActiveWord = k,
                    CharFrom = wordStart[k],
                    CharTo = wordEnd[k],
                };
            }
            else
            {
                var same = previous != null && previous.Text == text;
                var shownWords = same ? previous!.ShownWords + (previous.IsPause ? 0 : 1) : 0;
                var shownChars = same ? previous!.CharTo : 0;
                reveal = new MarkedReveal
                {
                    Text = text,
                    WordCount = words.Words.Count,
                    ShownWords = shownWords,
                    ActiveWord = -1,
                    CharFrom = shownChars,
                    CharTo = shownChars,
                };
            }

            result.Add(reveal);
            previous = reveal;
        }

        return result;
    }

    /// <summary>
    /// Character steps for a line: the index of the last revealed character (-1 = none) and the
    /// step's time span. A pause is one step over the whole line.
    /// </summary>
    public IEnumerable<(int RevealedTo, TimeSpan Start, TimeSpan End)> CharSteps(SubtitleLineViewModel line)
    {
        var count = CharTo - CharFrom;
        if (count <= 0)
        {
            yield return (CharFrom - 1, line.StartTime, line.EndTime);
            yield break;
        }

        var msPerChar = line.Duration.TotalMilliseconds / count;
        for (var i = 0; i < count; i++)
        {
            var start = line.StartTime.Add(TimeSpan.FromMilliseconds(i * msPerChar));
            var end = i == count - 1 ? line.EndTime : line.StartTime.Add(TimeSpan.FromMilliseconds((i + 1) * msPerChar));
            yield return (CharFrom + i, start, end);
        }
    }
}
