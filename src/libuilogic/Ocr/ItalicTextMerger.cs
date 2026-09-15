using System.Text;

namespace Nikse.SubtitleEdit.UiLogic.Ocr;

public class ItalicTextMerger
{
    public static string MergeWithItalicTags(List<BinaryOcrMatcher.CompareMatch> chars)
    {
        if (chars == null || chars.Count == 0)
        {
            return string.Empty;
        }

        var segments = GroupIntoSegments(chars);
        var result = new StringBuilder();
        bool currentlyInItalic = false;

        foreach (var segment in segments)
        {
            if (segment.ShouldBeItalic && !currentlyInItalic)
            {
                result.Append("<i>");
                currentlyInItalic = true;
            }
            else if (!segment.ShouldBeItalic && currentlyInItalic)
            {
                result.Append("</i>");
                currentlyInItalic = false;
            }

            result.Append(segment.Text);
        }

        // Close italic tag if still open
        if (currentlyInItalic)
        {
            result.Append("</i>");
        }

        return result.ToString();
    }

    private static List<TextSegment> GroupIntoSegments(List<BinaryOcrMatcher.CompareMatch> chars)
    {
        return BuildSegments(GroupIntoWords(chars));
    }

    private static List<WordGroup> GroupIntoWords(List<BinaryOcrMatcher.CompareMatch> chars)
    {
        var words = new List<WordGroup>();
        var currentWord = new List<NOcrChar>();
        bool inWhitespace = false;

        foreach (var ch in chars)
        {
            bool isWhitespace = IsWhitespace(ch.Text ?? string.Empty);

            if (isWhitespace != inWhitespace)
            {
                if (currentWord.Count > 0)
                {
                    words.Add(new WordGroup
                    {
                        Chars = new List<NOcrChar>(currentWord),
                        IsWhitespace = inWhitespace
                    });
                    currentWord.Clear();
                }
                inWhitespace = isWhitespace;
            }

            currentWord.Add(new NOcrChar() { Text = ch.Text ?? string.Empty , Italic = ch.Italic });
        }

        if (currentWord.Count > 0)
        {
            words.Add(new WordGroup
            {
                Chars = currentWord,
                IsWhitespace = inWhitespace
            });
        }

        return words;
    }

    //----------------------------

    public static string MergeWithItalicTags(List<NOcrChar> chars)
    {
        if (chars == null || chars.Count == 0)
        {
            return string.Empty;
        }

        var segments = GroupIntoSegments(chars);
        var result = new StringBuilder();
        bool currentlyInItalic = false;

        foreach (var segment in segments)
        {
            if (segment.ShouldBeItalic && !currentlyInItalic)
            {
                result.Append("<i>");
                currentlyInItalic = true;
            }
            else if (!segment.ShouldBeItalic && currentlyInItalic)
            {
                result.Append("</i>");
                currentlyInItalic = false;
            }

            result.Append(segment.Text);
        }

        // Close italic tag if still open
        if (currentlyInItalic)
        {
            result.Append("</i>");
        }

        return result.ToString();
    }

    private static List<TextSegment> GroupIntoSegments(List<NOcrChar> chars)
    {
        return BuildSegments(GroupIntoWords(chars));
    }

    private enum WordSlant
    {
        Whitespace,

        /// <summary>No letter or digit (e.g. "-", ". . .", "*"): the glyphs say nothing about slant.</summary>
        Neutral,

        /// <summary>As many italic as upright letters.</summary>
        Tie,
        Italic,
        Upright,
    }

    /// <summary>
    /// Decides italic per word. Only letters and digits vote: punctuation is shaped the same
    /// upright and slanted (a hyphen trained from italic text matches an upright hyphen with no
    /// wrong pixels), so letting it vote wrapped dialog dashes and ". . ." in italic tags, and an
    /// unknown "*" (never italic) outvoted the italic letters around it (#14886). A word without
    /// letters follows the nearest word on its line, and a tied word follows the words around
    /// it instead of defaulting to italic.
    /// </summary>
    private static List<TextSegment> BuildSegments(List<WordGroup> words)
    {
        var slants = new WordSlant[words.Count];
        var italic = new bool[words.Count];
        for (var i = 0; i < words.Count; i++)
        {
            slants[i] = GetSlant(words[i]);
            italic[i] = slants[i] == WordSlant.Italic;
        }

        for (var i = 0; i < words.Count; i++)
        {
            if (slants[i] == WordSlant.Tie)
            {
                italic[i] = FindItalic(words, slants, italic, i, -1, includeTies: false, stopAtLineBreak: false) ??
                            FindItalic(words, slants, italic, i, 1, includeTies: false, stopAtLineBreak: false) ??
                            false;
            }
        }

        for (var i = 0; i < words.Count; i++)
        {
            if (slants[i] == WordSlant.Neutral)
            {
                // Leading punctuation ("- Hello") belongs to the word after it, trailing
                // punctuation ("Hello . . .") to the word before it.
                italic[i] = FindItalic(words, slants, italic, i, 1, includeTies: true, stopAtLineBreak: true) ??
                            FindItalic(words, slants, italic, i, -1, includeTies: true, stopAtLineBreak: true) ??
                            false;
            }
        }

        var segments = new List<TextSegment>(words.Count);
        for (var i = 0; i < words.Count; i++)
        {
            var isWhitespace = slants[i] == WordSlant.Whitespace;

            // Words and whitespace alternate, so whitespace is inside italic only when the words
            // on both sides of it are italic.
            var shouldBeItalic = isWhitespace
                ? i > 0 && i < words.Count - 1 && italic[i - 1] && italic[i + 1]
                : italic[i];

            segments.Add(new TextSegment
            {
                Text = ConcatCharTexts(words[i].Chars),
                ShouldBeItalic = shouldBeItalic,
                IsWhitespace = isWhitespace
            });
        }

        // Post-process to merge consecutive segments with same formatting
        return MergeConsecutiveSegments(segments);
    }

    /// <summary>
    /// Italic state of the nearest word with letters in <paramref name="direction"/> (ties only
    /// when <paramref name="includeTies"/>), or null when there is none.
    /// </summary>
    private static bool? FindItalic(List<WordGroup> words, WordSlant[] slants, bool[] italic, int index, int direction, bool includeTies, bool stopAtLineBreak)
    {
        for (var i = index + direction; i >= 0 && i < words.Count; i += direction)
        {
            var slant = slants[i];
            if (slant == WordSlant.Whitespace)
            {
                if (stopAtLineBreak && ContainsLineBreak(words[i].Chars))
                {
                    return null;
                }

                continue;
            }

            if (slant is WordSlant.Italic or WordSlant.Upright || includeTies && slant == WordSlant.Tie)
            {
                return italic[i];
            }
        }

        return null;
    }

    private static WordSlant GetSlant(WordGroup word)
    {
        if (word.IsWhitespace)
        {
            return WordSlant.Whitespace;
        }

        var italicCount = 0;
        var uprightCount = 0;
        for (var i = 0; i < word.Chars.Count; i++)
        {
            var ch = word.Chars[i];
            if (!HasLetterOrDigit(ch.Text))
            {
                continue;
            }

            if (ch.Italic)
            {
                italicCount++;
            }
            else
            {
                uprightCount++;
            }
        }

        if (italicCount == 0 && uprightCount == 0)
        {
            return WordSlant.Neutral;
        }

        if (italicCount == uprightCount)
        {
            return WordSlant.Tie;
        }

        return italicCount > uprightCount ? WordSlant.Italic : WordSlant.Upright;
    }

    private static bool HasLetterOrDigit(string text)
    {
        for (var i = 0; i < text.Length; i++)
        {
            if (char.IsLetterOrDigit(text[i]))
            {
                return true;
            }
        }

        return false;
    }

    private static bool ContainsLineBreak(List<NOcrChar> chars)
    {
        for (var i = 0; i < chars.Count; i++)
        {
            if (chars[i].Text.Contains('\n'))
            {
                return true;
            }
        }

        return false;
    }

    private static List<WordGroup> GroupIntoWords(List<NOcrChar> chars)
    {
        var words = new List<WordGroup>();
        var currentWord = new List<NOcrChar>();
        bool inWhitespace = false;

        foreach (var ch in chars)
        {
            bool isWhitespace = IsWhitespace(ch.Text);

            if (isWhitespace != inWhitespace)
            {
                if (currentWord.Count > 0)
                {
                    words.Add(new WordGroup
                    {
                        Chars = new List<NOcrChar>(currentWord),
                        IsWhitespace = inWhitespace
                    });
                    currentWord.Clear();
                }
                inWhitespace = isWhitespace;
            }

            currentWord.Add(ch);
        }

        if (currentWord.Count > 0)
        {
            words.Add(new WordGroup
            {
                Chars = currentWord,
                IsWhitespace = inWhitespace
            });
        }

        return words;
    }

    private static List<TextSegment> MergeConsecutiveSegments(List<TextSegment> segments)
    {
        if (segments.Count <= 1) return segments;

        var merged = new List<TextSegment>();
        var currentSegment = segments[0];
        StringBuilder? mergedText = null; // string concat per merged segment was quadratic

        for (int i = 1; i < segments.Count; i++)
        {
            var nextSegment = segments[i];

            // Merge if both have same italic formatting
            if (currentSegment.ShouldBeItalic == nextSegment.ShouldBeItalic)
            {
                mergedText ??= new StringBuilder(currentSegment.Text);
                mergedText.Append(nextSegment.Text);
                currentSegment.IsWhitespace = currentSegment.IsWhitespace && nextSegment.IsWhitespace;
            }
            else
            {
                if (mergedText != null)
                {
                    currentSegment.Text = mergedText.ToString();
                    mergedText = null;
                }

                merged.Add(currentSegment);
                currentSegment = nextSegment;
            }
        }

        if (mergedText != null)
        {
            currentSegment.Text = mergedText.ToString();
        }

        merged.Add(currentSegment);
        return merged;
    }

    private static bool IsWhitespace(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return true;
        }

        for (var i = 0; i < text.Length; i++)
        {
            if (!char.IsWhiteSpace(text[i]))
            {
                return false;
            }
        }

        return true;
    }

    private static string ConcatCharTexts(List<NOcrChar> chars)
    {
        if (chars.Count == 1)
        {
            return chars[0].Text;
        }

        var sb = new StringBuilder(chars.Count);
        for (var i = 0; i < chars.Count; i++)
        {
            sb.Append(chars[i].Text);
        }

        return sb.ToString();
    }

    private class WordGroup
    {
        public List<NOcrChar> Chars { get; set; } = new List<NOcrChar>();
        public bool IsWhitespace { get; set; }
    }

    private class TextSegment
    {
        public string Text { get; set; } = string.Empty;
        public bool ShouldBeItalic { get; set; }
        public bool IsWhitespace { get; set; }
    }
}
