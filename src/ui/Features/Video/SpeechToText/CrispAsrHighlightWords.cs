using Nikse.SubtitleEdit.UiLogic.AudioToText;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText;

/// <summary>
/// Crisp ASR's "Highlight word" button: "-ml 1 -sow" makes crispasr write one word per cue,
/// which on its own is not highlighting - post-processing merged the words straight back into
/// plain lines. The words are grouped into cues here and each cue is repeated once per word
/// with that word underlined, as whisper.cpp's word-level output is.
/// </summary>
public static partial class CrispAsrHighlightWords
{
    [GeneratedRegex(@"(?:^|\s)(-ml|--max-len)\s+1(?=\s|$)")]
    private static partial Regex MaxLenOneRegex();

    [GeneratedRegex(@"(?:^|\s)(-sow|--split-on-word)(?=\s|$)")]
    private static partial Regex SplitOnWordRegex();

    /// <summary>
    /// True when the parameters ask for one word per segment, split at word boundaries. A maximum
    /// length on its own is not that - the "Standard" preset sets one.
    /// </summary>
    public static bool IsActive(string? commandLineParameters)
    {
        return !string.IsNullOrEmpty(commandLineParameters) &&
               MaxLenOneRegex().IsMatch(commandLineParameters) &&
               SplitOnWordRegex().IsMatch(commandLineParameters);
    }

    /// <summary>One-word cues to cues with the active word underlined.</summary>
    public static List<ResultText> Build(IReadOnlyList<ResultText> wordCues, int maxCharsLatin)
    {
        var words = wordCues
            .Select(p => new Qwen3AsrWord(p.Text, (double)p.Start, (double)p.End))
            .ToList();

        var subtitle = Qwen3AsrWordSegmenter.BuildHighlightedSubtitle(words, maxCharsLatin);
        return subtitle.Paragraphs.Select(p => new ResultText
        {
            Start = (decimal)p.StartTime.TotalSeconds,
            End = (decimal)p.EndTime.TotalSeconds,
            Text = p.Text,
        }).ToList();
    }
}
