using Nikse.SubtitleEdit.Logic.Ocr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Features.Ocr.NOcr;

public class ItalicTextMerger
{
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

        foreach (var word in words)
        {
            bool shouldBeItalic = word.IsWhitespace ? false : GetMajorityItalic(word.Chars);
            string text = string.Concat(word.Chars.Select(c => c.Text));

            // If this is whitespace, check if we can merge it with surrounding italic content
            if (word.IsWhitespace)
            {
                // Check if previous and next non-whitespace segments are both italic
                bool prevIsItalic = GetPreviousNonWhitespaceItalic(segments);
                bool nextIsItalic = GetNextNonWhitespaceItalic(words, words.IndexOf(word));

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

        for (int i = 1; i < segments.Count; i++)
        {
            var nextSegment = segments[i];

            // Merge if both have same italic formatting
            if (currentSegment.ShouldBeItalic == nextSegment.ShouldBeItalic)
            {
                currentSegment = new TextSegment
                {
                    Text = currentSegment.Text + nextSegment.Text,
                    ShouldBeItalic = currentSegment.ShouldBeItalic,
                    IsWhitespace = currentSegment.IsWhitespace && nextSegment.IsWhitespace
                };
            }
            else
            {
                merged.Add(currentSegment);
                currentSegment = nextSegment;
            }
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

        int italicCount = wordChars.Count(c => c.Italic);
        int nonItalicCount = wordChars.Count - italicCount;

        return italicCount >= nonItalicCount;
    }

    private static bool IsWhitespace(string text)
    {
        return string.IsNullOrEmpty(text) || text.All(char.IsWhiteSpace);
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
