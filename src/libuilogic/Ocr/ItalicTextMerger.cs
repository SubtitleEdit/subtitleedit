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
        var words = GroupIntoWords(chars);
        var segments = new List<TextSegment>();

        // Indexed loop: the loop body used to call words.IndexOf(word) - a linear scan inside
        // the loop already iterating words, i.e. O(n^2) per OCR'd line.
        for (var wordIndex = 0; wordIndex < words.Count; wordIndex++)
        {
            var word = words[wordIndex];
            bool shouldBeItalic = word.IsWhitespace ? false : GetMajorityItalic(word.Chars);
            string text = ConcatCharTexts(word.Chars);

            // If this is whitespace, check if we can merge it with surrounding italic content
            if (word.IsWhitespace)
            {
                // Check if previous and next non-whitespace segments are both italic
                bool prevIsItalic = GetPreviousNonWhitespaceItalic(segments);
                bool nextIsItalic = GetNextNonWhitespaceItalic(words, wordIndex);

                // If surrounded by italic content, include whitespace in italic
                shouldBeItalic = prevIsItalic && nextIsItalic;
            }

            segments.Add(new TextSegment
            {
                Text = text,
                ShouldBeItalic = shouldBeItalic,
                IsWhitespace = word.IsWhitespace
            });
        }

        // Post-process to merge consecutive segments with same formatting
        return MergeConsecutiveSegments(segments);
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
        var words = GroupIntoWords(chars);
        var segments = new List<TextSegment>();

        // Indexed loop: the loop body used to call words.IndexOf(word) - a linear scan inside
        // the loop already iterating words, i.e. O(n^2) per OCR'd line.
        for (var wordIndex = 0; wordIndex < words.Count; wordIndex++)
        {
            var word = words[wordIndex];
            bool shouldBeItalic = word.IsWhitespace ? false : GetMajorityItalic(word.Chars);
            string text = ConcatCharTexts(word.Chars);

            // If this is whitespace, check if we can merge it with surrounding italic content
            if (word.IsWhitespace)
            {
                // Check if previous and next non-whitespace segments are both italic
                bool prevIsItalic = GetPreviousNonWhitespaceItalic(segments);
                bool nextIsItalic = GetNextNonWhitespaceItalic(words, wordIndex);

                // If surrounded by italic content, include whitespace in italic
                shouldBeItalic = prevIsItalic && nextIsItalic;
            }

            segments.Add(new TextSegment
            {
                Text = text,
                ShouldBeItalic = shouldBeItalic,
                IsWhitespace = word.IsWhitespace
            });
        }

        // Post-process to merge consecutive segments with same formatting
        return MergeConsecutiveSegments(segments);
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

    private static bool GetPreviousNonWhitespaceItalic(List<TextSegment> segments)
    {
        for (int i = segments.Count - 1; i >= 0; i--)
        {
            if (!segments[i].IsWhitespace)
                return segments[i].ShouldBeItalic;
        }
        return false;
    }

    private static bool GetNextNonWhitespaceItalic(List<WordGroup> words, int currentIndex)
    {
        for (int i = currentIndex + 1; i < words.Count; i++)
        {
            if (!words[i].IsWhitespace)
                return GetMajorityItalic(words[i].Chars);
        }
        return false;
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

    private static bool GetMajorityItalic(List<NOcrChar> wordChars)
    {
        if (wordChars.Count == 0)
        {
            return false;
        }

        var italicCount = 0;
        for (var i = 0; i < wordChars.Count; i++)
        {
            if (wordChars[i].Italic)
            {
                italicCount++;
            }
        }

        int nonItalicCount = wordChars.Count - italicCount;

        return italicCount >= nonItalicCount;
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
