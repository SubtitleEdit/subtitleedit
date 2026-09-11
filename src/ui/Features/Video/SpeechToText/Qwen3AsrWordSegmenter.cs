using System.Collections.Generic;
using System.Text;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Common.TextLengthCalculator;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText;

/// <summary>
/// One aligned token from qwen3-asr-cli's <c>--transcribe-align</c> JSON output.
/// For CJK languages the forced aligner emits one token per character, and
/// punctuation marks arrive as their own (often zero-length) tokens.
/// </summary>
public readonly record struct Qwen3AsrWord(string Word, double StartSeconds, double EndSeconds);

/// <summary>
/// Builds subtitle cues from Qwen3 ASR CPP's word-level alignment.
///
/// Mirrors the rules of the engine's own SRT writer (which Subtitle Edit bypasses
/// by requesting JSON): a cue ends on sentence punctuation, on a long pause, or
/// when it grows past the length cap. Spaces are only inserted between Latin
/// tokens - never around CJK characters or before punctuation. The previous
/// implementation split purely on a 0.5 s gap / 80-character cap and joined every
/// token with a space, which for Chinese produced one space-riddled blob cut
/// mid-sentence (issue #14631).
/// </summary>
public static class Qwen3AsrWordSegmenter
{
    /// <summary>Latin script cap; matches the post-processor's paragraph cap for two lines.</summary>
    public const int DefaultMaxCharsLatin = 86;

    /// <summary>CJK cap; ideographs are wider and carry more meaning per character.</summary>
    public const int DefaultMaxCharsCjk = 36;

    /// <summary>A silence this long always ends the cue, punctuation or not.</summary>
    public const double HardPauseSeconds = 1.0;

    /// <summary>A shorter silence ends the cue when the text already ends at a clause boundary.</summary>
    public const double SoftPauseSeconds = 0.5;

    private const string SentenceTerminators = ".!?。！？…";
    private const string ClauseTerminators = ",;:，、；：";
    private const string ClosingMarks = "\"'”’)]】」』）";
    private const string NoSpaceBefore = ".,!?;:%)]}」』】）…" + "，。！？、；：";
    private const string NoSpaceAfter = "“‘([{「『【（《〈¿¡";

    public static Subtitle BuildSubtitle(IReadOnlyList<Qwen3AsrWord> words, int maxCharsLatin = DefaultMaxCharsLatin, int maxCharsCjk = DefaultMaxCharsCjk)
    {
        var subtitle = new Subtitle();
        var text = new StringBuilder();
        var startTime = 0.0;
        var endTime = 0.0;

        void Flush()
        {
            if (text.Length > 0)
            {
                subtitle.Paragraphs.Add(new Paragraph(text.ToString().Trim(), startTime * 1000.0, endTime * 1000.0));
                text.Clear();
            }
        }

        foreach (var word in words)
        {
            var raw = word.Word ?? string.Empty;
            var token = raw.Trim();
            if (token.Length == 0)
            {
                continue;
            }

            // The engine can leave a literal newline inside a token (issue #11717); its own
            // SRT writer treats that as a segment break, so honor it here too.
            var breakAfter = raw.Contains('\n');

            var isPunctuationOnly = IsPunctuationOnly(token);
            if (text.Length == 0 && isPunctuationOnly && subtitle.Paragraphs.Count > 0 && ClosesText(token))
            {
                // A stray closing mark right after a sentence break (e.g. a closing quote emitted
                // as its own token) belongs to the cue that was just closed, not to a new one.
                // An opening mark or a dialog dash ("「", "“", "-") opens the next cue instead.
                var last = subtitle.Paragraphs[subtitle.Paragraphs.Count - 1];
                last.Text += token;
                if (word.EndSeconds * 1000.0 > last.EndTime.TotalMilliseconds)
                {
                    last.EndTime.TotalMilliseconds = word.EndSeconds * 1000.0;
                }

                continue;
            }

            if (text.Length > 0 && !isPunctuationOnly)
            {
                var gap = word.StartSeconds - endTime;
                var endsClause = text.EndsWithAny(ClauseTerminators, ClosingMarks) || text.EndsWithAny(SentenceTerminators, ClosingMarks);
                var maxChars = ContainsCjk(text) ? maxCharsCjk : maxCharsLatin;
                var space = NeedsSpace(text[text.Length - 1], token[0]) ? 1 : 0;
                var tooLong = text.Length + space + token.Length > maxChars;
                if (gap > HardPauseSeconds || (gap > SoftPauseSeconds && endsClause) || tooLong)
                {
                    Flush();
                }
            }

            if (text.Length == 0)
            {
                startTime = word.StartSeconds;
                endTime = word.EndSeconds;
            }
            else
            {
                if (NeedsSpace(text[text.Length - 1], token[0]))
                {
                    text.Append(' ');
                }

                if (word.EndSeconds > endTime)
                {
                    endTime = word.EndSeconds;
                }
            }

            text.Append(token);

            if (breakAfter || EndsSentence(token))
            {
                Flush();
            }
        }

        Flush();
        return subtitle;
    }

    private static bool NeedsSpace(char previous, char next)
    {
        if (char.IsWhiteSpace(previous) || NoSpaceBefore.IndexOf(next) >= 0 || NoSpaceAfter.IndexOf(previous) >= 0)
        {
            return false;
        }

        if (IsCjkOrFullWidth(previous) || IsCjkOrFullWidth(next))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// True when the token ends a sentence: a terminator, optionally followed by
    /// closing quotes/brackets. Checks the end only, so "3.5" or "e.g" do not split.
    /// </summary>
    private static bool EndsSentence(string token)
    {
        var i = token.Length - 1;
        while (i >= 0 && ClosingMarks.IndexOf(token[i]) >= 0)
        {
            i--;
        }

        return i >= 0 && SentenceTerminators.IndexOf(token[i]) >= 0;
    }

    /// <summary>True when the token starts with a closing mark or a terminator, i.e. it attaches to what came before it.</summary>
    private static bool ClosesText(string token)
    {
        return ClosingMarks.IndexOf(token[0]) >= 0 || NoSpaceBefore.IndexOf(token[0]) >= 0;
    }

    private static bool IsPunctuationOnly(string token)
    {
        foreach (var c in token)
        {
            if (char.IsLetterOrDigit(c))
            {
                return false;
            }
        }

        return true;
    }

    private static bool ContainsCjk(StringBuilder text)
    {
        for (var i = 0; i < text.Length; i++)
        {
            if (IsCjkOrFullWidth(text[i]))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsCjkOrFullWidth(char c)
    {
        return CalcCjk.IsCjk(c) || (c >= '\uFF00' && c <= '\uFFEF'); // full-width forms: ，。！？
    }
}
