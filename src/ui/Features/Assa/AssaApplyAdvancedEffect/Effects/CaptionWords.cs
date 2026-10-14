using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect.Effects;

/// <summary>
/// Splits a line into words and line breaks for the word-by-word caption styles (highlight
/// box, single word pop, bold captions), and times each word by its length - a long word is
/// spoken longer than "a", which an even split ignores.
/// </summary>
internal sealed class CaptionWords
{
    private static readonly Regex TokenRegex = new(@"\n|[^\S\n]+|\S+", RegexOptions.Compiled);

    public sealed class Token
    {
        public required string Text { get; init; }
        public bool IsWord { get; init; }
        public bool IsLineBreak { get; init; }
        public int WordIndex { get; init; } = -1;
    }

    public List<Token> Tokens { get; } = new();
    public List<string> Words { get; } = new();

    /// <summary>Word i is active from WordStartMs[i] to WordStartMs[i + 1] (ms from the line start).</summary>
    public double[] WordStartMs { get; private set; } = [];

    public static CaptionWords? Parse(SubtitleLineViewModel line, bool upperCase)
    {
        var text = HtmlUtil.RemoveHtmlTags(Utilities.RemoveSsaTags(line.Text), true)
            .Replace("\\N", "\n").Replace("\\n", "\n").Replace("\r\n", "\n").Replace('\r', '\n').Trim();
        if (upperCase)
        {
            text = text.ToUpper(CultureInfo.CurrentCulture);
        }

        var result = new CaptionWords();
        foreach (Match m in TokenRegex.Matches(text))
        {
            if (m.Value == "\n")
            {
                result.Tokens.Add(new Token { Text = "\\N", IsLineBreak = true });
            }
            else if (string.IsNullOrWhiteSpace(m.Value))
            {
                result.Tokens.Add(new Token { Text = " " });
            }
            else
            {
                result.Tokens.Add(new Token { Text = EscapeBraces(m.Value), IsWord = true, WordIndex = result.Words.Count });
                result.Words.Add(m.Value);
            }
        }

        if (result.Words.Count == 0)
        {
            return null;
        }

        result.WordStartMs = MakeTiming(result.Words, line.Duration.TotalMilliseconds);
        return result;
    }

    private static double[] MakeTiming(List<string> words, double totalMs)
    {
        // weight = letters + a constant, so short words still get a readable slice
        var weights = new double[words.Count];
        var sum = 0.0;
        for (var i = 0; i < words.Count; i++)
        {
            weights[i] = words[i].Length + 2.0;
            sum += weights[i];
        }

        var starts = new double[words.Count + 1];
        for (var i = 0; i < words.Count; i++)
        {
            starts[i + 1] = starts[i] + Math.Max(0, totalMs) * weights[i] / sum;
        }

        starts[words.Count] = Math.Max(0, totalMs);
        return starts;
    }

    /// <summary>
    /// A copy of the line covering only word <paramref name="wordIndex"/>'s time slice, with the
    /// line's positioning tags (a \move continues across the slices).
    /// </summary>
    public SubtitleLineViewModel MakeSlice(SubtitleLineViewModel line, int wordIndex, string posTags, out string slicePosTags)
    {
        var slice = new SubtitleLineViewModel(line, generateNewId: true);
        var startMs = WordStartMs[wordIndex];
        var endMs = WordStartMs[wordIndex + 1];
        slice.StartTime = line.StartTime.Add(TimeSpan.FromMilliseconds(startMs));
        slice.EndTime = wordIndex == Words.Count - 1 ? line.EndTime : line.StartTime.Add(TimeSpan.FromMilliseconds(endMs));
        slicePosTags = string.IsNullOrEmpty(posTags)
            ? string.Empty
            : AdvancedEffectUtil.AdjustMoveForSegment(posTags, startMs, endMs - startMs, line.Duration.TotalMilliseconds);
        return slice;
    }

    public static string GetStyleName(SubtitleLineViewModel line) => string.IsNullOrWhiteSpace(line.Style) ? "Default" : line.Style;

    public static SsaStyle GetStyle(string header, SubtitleLineViewModel line) =>
        AdvancedSubStationAlpha.GetSsaStyle(GetStyleName(line), string.IsNullOrEmpty(header) ? AdvancedSubStationAlpha.DefaultHeader : header);

    /// <summary>\fscx/\fscy for a percentage of the style's own scale.</summary>
    public static string Scale(SsaStyle style, double percent) =>
        $"\\fscx{AdvancedEffectUtil.Tag((double)style.ScaleX * percent / 100)}\\fscy{AdvancedEffectUtil.Tag((double)style.ScaleY * percent / 100)}";

    public static string ToAssColor(SKColor color) => $"&H{color.Blue:X2}{color.Green:X2}{color.Red:X2}&";

    // a literal brace would open/close an override block
    private static string EscapeBraces(string s) => s.Replace("{", "(").Replace("}", ")");
}
