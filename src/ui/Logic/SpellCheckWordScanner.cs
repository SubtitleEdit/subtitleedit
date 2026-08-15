using Nikse.SubtitleEdit.Features.SpellCheck;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;
using System;
using System.Buffers;
using System.Collections.Generic;

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

        if (IsSpecialPattern(word, text, text.Contains('{'), text.Contains('<')))
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

        // Whether the line has any tag at all is a property of the line, not of the word, but
        // it used to be re-derived per word by scanning back to the nearest '{' / '<'. This
        // runs on every keystroke in the edit box, so settle it once for the whole line.
        var hasAssaTag = text.Contains('{');
        var hasHtmlTag = text.Contains('<');

        foreach (var word in SpellCheckWordLists.Split(text))
        {
            if (string.IsNullOrWhiteSpace(word.Text) || word.Length < 2)
            {
                continue;
            }

            if (IsSpecialPattern(word, text, hasAssaTag, hasHtmlTag))
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

    // Digits and the separators that can appear inside a number. A word made only of these is
    // not spell-checkable - IndexOfAnyExcept answers that with one vectorized scan, where the
    // LINQ All() this replaces paid an enumerator plus a delegate call per character.
    private static readonly SearchValues<char> NumberChars = SearchValues.Create("0123456789.,-");

    // Necessary first letter of every URL pattern tested below ("http", "https", "www"), so a
    // word without one cannot match any of them and skips three substring searches.
    private static readonly SearchValues<char> UrlStartChars = SearchValues.Create("hHwW");

    private static bool IsSpecialPattern(SpellCheckWord word, string text, bool hasAssaTag, bool hasHtmlTag)
    {
        var wordSpan = word.Text.AsSpan();

        // Skip numbers
        if (wordSpan.IndexOfAnyExcept(NumberChars) < 0)
        {
            return true;
        }

        // Skip URLs
        if (wordSpan.ContainsAny(UrlStartChars) &&
            (word.Text.Contains("http://", StringComparison.OrdinalIgnoreCase) ||
             word.Text.Contains("https://", StringComparison.OrdinalIgnoreCase) ||
             word.Text.Contains("www.", StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        // Skip email-like patterns
        if (wordSpan.Contains('@'))
        {
            return true;
        }

        // Skip hashtags
        if (word.Text.StartsWith('#'))
        {
            return true;
        }

        if (hasAssaTag && IsBetweenAssaTags(word, text))
        {
            return true;
        }

        if (hasHtmlTag && IsInsideHtmlTag(word, text))
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
