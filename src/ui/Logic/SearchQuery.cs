using System;
using System.Linq;

namespace Nikse.SubtitleEdit.Logic;

internal static class SearchQuery
{
    public static string[] Tokenize(string text) =>
        text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

    public static bool Matches(string? text, string[] tokens, bool allowTypos = false)
    {
        return text != null && tokens.All(token =>
            text.Contains(token, StringComparison.OrdinalIgnoreCase) ||
            (allowTypos && token.Length >= 4 && ContainsSimilarWord(text, token)));
    }

    private static bool ContainsSimilarWord(string text, string token)
    {
        var start = 0;
        while (start < text.Length)
        {
            if (!char.IsLetterOrDigit(text[start]))
            {
                start++;
                continue;
            }

            var end = start + 1;
            while (end < text.Length && char.IsLetterOrDigit(text[end]))
            {
                end++;
            }

            var word = text.AsSpan(start, end - start);
            if (IsWithinOneEdit(word, token))
            {
                return true;
            }

            start = end;
        }

        return false;
    }

    private static bool IsWithinOneEdit(ReadOnlySpan<char> word, ReadOnlySpan<char> token)
    {
        if (Math.Abs(word.Length - token.Length) > 1)
        {
            return false;
        }

        while (!word.IsEmpty && !token.IsEmpty &&
               char.ToUpperInvariant(word[0]) == char.ToUpperInvariant(token[0]))
        {
            word = word[1..];
            token = token[1..];
        }

        if (word.Length > token.Length)
        {
            return word[1..].Equals(token, StringComparison.OrdinalIgnoreCase);
        }

        if (token.Length > word.Length)
        {
            return word.Equals(token[1..], StringComparison.OrdinalIgnoreCase);
        }

        return word.IsEmpty ||
               word[1..].Equals(token[1..], StringComparison.OrdinalIgnoreCase) ||
               (word.Length >= 2 &&
                char.ToUpperInvariant(word[0]) == char.ToUpperInvariant(token[1]) &&
                char.ToUpperInvariant(word[1]) == char.ToUpperInvariant(token[0]) &&
                word[2..].Equals(token[2..], StringComparison.OrdinalIgnoreCase));
    }
}
