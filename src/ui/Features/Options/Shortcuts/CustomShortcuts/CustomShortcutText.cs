using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Features.Options.Shortcuts.CustomShortcuts;

/// <summary>
/// The text work of custom shortcut steps, kept free of UI so it can be tested.
/// </summary>
public static class CustomShortcutText
{
    private static readonly TimeSpan RegexTimeout = TimeSpan.FromSeconds(2);

    /// <summary>
    /// Line breaks typed in the step editor come in as "\n" (or "\r\n"); subtitle text uses
    /// <see cref="Environment.NewLine"/>.
    /// </summary>
    public static string NormalizeNewLines(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        return text.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", Environment.NewLine);
    }

    public static string Insert(string text, string insert, CustomShortcutInsertPosition position)
    {
        insert = NormalizeNewLines(insert);
        return position == CustomShortcutInsertPosition.Start
            ? insert + text
            : text + insert;
    }

    public static string Replace(string text, SeCustomShortcutStep step)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(step.Find))
        {
            return text;
        }

        if (step.UseRegex)
        {
            // Like the Multiple replace window: a typed \n (or \r\n) in the pattern is a line-feed,
            // and the match runs against line-feed-normalized text, so "a\nb" matches "a\r\nb" too.
            var options = step.CaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
            var regex = new Regex(RegexUtils.FixNewLine(step.Find), options, RegexTimeout);
            var replaceWith = RegexUtils.FixNewLine(step.ReplaceWith ?? string.Empty).Replace("\r\n", "\n").Replace("\r", "\n");
            return RegexUtils.ReplaceNewLineSafe(regex, text, replaceWith);
        }

        var comparison = step.CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
        return text.Replace(NormalizeNewLines(step.Find), NormalizeNewLines(step.ReplaceWith), comparison);
    }

    /// <summary>
    /// Null when the step's regex is valid (or not a regex), otherwise the parser's message.
    /// </summary>
    public static string? GetRegexError(SeCustomShortcutStep step)
    {
        if (!step.UseRegex || string.IsNullOrEmpty(step.Find))
        {
            return null;
        }

        try
        {
            _ = new Regex(step.Find, RegexOptions.None, RegexTimeout);
            return null;
        }
        catch (ArgumentException ex)
        {
            return ex.Message;
        }
    }
}
