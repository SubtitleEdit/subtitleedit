using Nikse.SubtitleEdit.Features.SpellCheck;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Decides which words in subtitle text count as misspelled: it skips the things a spell checker
/// has no business flagging (numbers, URLs, e-mail addresses, hashtags, and anything inside an
/// HTML or ASSA tag) before asking the spell check manager.
///
/// Used by the subtitle edit box, which draws the red underlines itself
/// (<see cref="Controls.SyntaxHighlightingTextPresenter"/>), and by its context menu.
/// </summary>
public static class SpellCheckWordScanner
{
    /// <summary>
    /// Word-level misspelling check with the special-pattern skipping described above.
    /// </summary>
    public static bool IsWordMisspelled(SpellCheckWord word, string text, ISpellCheckManager spellCheckManager)
    {
        if (string.IsNullOrWhiteSpace(word.Text))
        {
            return false;
        }

        if (IsSpecialPattern(word, text))
        {
            return false;
        }

        return !spellCheckManager.IsWordCorrect(word, text);
    }

    /// <summary>
    /// The misspelled words of <paramref name="text"/> with their positions - what the underlines
    /// are drawn from.
    /// </summary>
    public static List<SpellCheckWord> GetMisspelledWords(string text, ISpellCheckManager spellCheckManager)
    {
        var result = new List<SpellCheckWord>();
        if (string.IsNullOrWhiteSpace(text))
        {
            return result;
        }

        foreach (var word in SpellCheckWordLists.Split(text))
        {
            if (string.IsNullOrWhiteSpace(word.Text) || word.Length < 2)
            {
                continue;
            }

            if (IsSpecialPattern(word, text))
            {
                continue;
            }

            if (!spellCheckManager.IsWordCorrect(word, text))
            {
                result.Add(word);
            }
        }

        return result;
    }

    private static bool IsSpecialPattern(SpellCheckWord word, string text)
    {
        // Skip numbers
        if (word.Text.All(c => char.IsDigit(c) || c == '.' || c == ',' || c == '-'))
        {
            return true;
        }

        // Skip URLs
        if (word.Text.Contains("http://", StringComparison.OrdinalIgnoreCase) ||
            word.Text.Contains("https://", StringComparison.OrdinalIgnoreCase) ||
            word.Text.Contains("www.", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // Skip email-like patterns
        if (word.Text.Contains('@'))
        {
            return true;
        }

        // Skip hashtags
        if (word.Text.StartsWith('#'))
        {
            return true;
        }

        if (IsBetweenAssaTags(word, text))
        {
            return true;
        }

        if (IsInsideHtmlTag(word, text))
        {
            return true;
        }

        return false;
    }

    private static bool IsBetweenAssaTags(SpellCheckWord word, string text)
    {
        if (word == null || string.IsNullOrEmpty(text))
        {
            return false;
        }

        // 1. Find the last occurrence of an opening brace before the word starts
        var openBrace = text.LastIndexOf('{', word.Index);

        // 2. Find the first occurrence of a closing brace after the word starts
        var closeBrace = text.IndexOf('}', word.Index);

        // If both exist, check if there is another closing brace between
        // the opening brace and our word.
        // If not, it means we are currently inside an unclosed tag.
        if (openBrace != -1 && closeBrace != -1 && openBrace < closeBrace)
        {
            // Check if there's a '}' between the '{' and the word.
            // If there is, the word is actually OUTSIDE a tag.
            var closingBeforeWord = text.IndexOf('}', openBrace, word.Index - openBrace);

            return closingBeforeWord == -1;
        }

        return false;
    }

    private static bool IsInsideHtmlTag(SpellCheckWord word, string text)
    {
        if (word == null || string.IsNullOrEmpty(text))
        {
            return false;
        }

        // 1. Find the last opening bracket before the word
        var openBracket = text.LastIndexOf('<', word.Index);

        // 2. Find the next closing bracket after the word starts
        var closeBracket = text.IndexOf('>', word.Index);

        // If both exist in the correct order
        if (openBracket != -1 && closeBracket != -1 && openBracket < closeBracket)
        {
            // Ensure there isn't a '>' between the opening '<' and the word
            // (which would mean the word is outside a tag)
            var closingBeforeWord = text.IndexOf('>', openBracket, word.Index - openBracket);

            return closingBeforeWord == -1;
        }

        return false;
    }
}
