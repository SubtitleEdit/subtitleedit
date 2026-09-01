using System;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// macOS "Look Up" (#14277): every macOS text field offers it, and it opens the word in
/// Dictionary.app - which shows whatever dictionaries the user has installed, in any language,
/// plus thesaurus and Wikipedia. Dictionary.app is reached through the "dict:" URL scheme, which
/// only exists on macOS, so the menu item using this is macOS-only too.
/// </summary>
public static class MacDictionaryLookup
{
    /// <summary>Longest text shown inside the menu header before it is elided.</summary>
    internal const int MaxHeaderTextLength = 32;

    /// <summary>
    /// The text to look up: a single trimmed line - or null when there is nothing to look up.
    /// A selection can span two lines and can carry tabs, and Dictionary.app searches for the
    /// whole phrase, so the white space is folded into single spaces.
    /// </summary>
    public static string? Normalize(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var oneLine = text
            .Replace("\r\n", " ")
            .Replace('\r', ' ')
            .Replace('\n', ' ')
            .Replace('\t', ' ')
            .Trim();

        while (oneLine.Contains("  ", StringComparison.Ordinal))
        {
            oneLine = oneLine.Replace("  ", " ", StringComparison.Ordinal);
        }

        return oneLine.Length == 0 ? null : oneLine;
    }

    /// <summary>
    /// The "dict:" URL Dictionary.app opens for the text, or null when there is nothing to look up.
    /// </summary>
    public static string? BuildUrl(string? text)
    {
        var word = Normalize(text);
        return word == null ? null : "dict://" + Uri.EscapeDataString(word);
    }

    /// <summary>
    /// The menu header, macOS style: Look up "word". A long selection is elided so one right-click
    /// on a whole subtitle line cannot stretch the context menu across the screen.
    /// </summary>
    public static string BuildHeader(string template, string? text)
    {
        var word = Normalize(text) ?? string.Empty;
        if (word.Length > MaxHeaderTextLength)
        {
            word = word.Substring(0, MaxHeaderTextLength - 1).TrimEnd() + "…";
        }

        // Replace rather than string.Format: the template is translated, and a stray brace in a
        // translation must not throw while a context menu is opening.
        return template.Replace("{0}", word, StringComparison.Ordinal);
    }
}
