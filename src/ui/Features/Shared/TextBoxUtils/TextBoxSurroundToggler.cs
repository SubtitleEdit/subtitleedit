using Avalonia.Threading;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Shared.TextBoxUtils;

/// <summary>
/// Toggles a "surround with" pair - like music symbols "♪" - on the selected part of the text
/// in a text box (SE 4 parity, see issue #12873).
/// </summary>
public static class TextBoxSurroundToggler
{
    /// <summary>
    /// Adds (or removes again) the surround symbols around the selected text only.
    /// Returns false when there is nothing selected, or when the whole text is selected -
    /// the caller should then surround the whole subtitle line(s) instead.
    /// </summary>
    public static bool ToggleSelection(ITextBoxWrapper? tb, string surroundLeft, string surroundRight,
        SurroundWithBehavior behavior = SurroundWithBehavior.Toggle)
    {
        if (tb?.Text == null ||
            string.IsNullOrEmpty(surroundLeft) && string.IsNullOrEmpty(surroundRight))
        {
            return false;
        }

        var selectionStart = Math.Min(tb.SelectionStart, tb.SelectionEnd);
        var selectionEnd = Math.Max(tb.SelectionStart, tb.SelectionEnd);
        var selectionLength = selectionEnd - selectionStart;

        if (selectionLength <= 0 || selectionLength >= tb.Text.Length)
        {
            return false;
        }

        // Keep leading/trailing white-space (and new-lines) outside the symbols.
        var selectedText = TextBoxSelectionUtils.SplitOuterWhiteSpace(
            tb.Text.Substring(selectionStart, selectionLength), out var pre, out var post);

        if (selectedText.Length == 0)
        {
            return false;
        }

        // Italic/bold/font tags of the selection stay outside the symbols,
        // so "<i>Hello</i>" becomes "<i>♪ Hello ♪</i>".
        var newText = pre + Apply(behavior, surroundLeft, selectedText, surroundRight, out _) + post;

        tb.Text = tb.Text
            .Remove(selectionStart, selectionLength)
            .Insert(selectionStart, newText);

        Dispatcher.UIThread.Post(() =>
        {
            tb.Focus();
            tb.SelectionStart = selectionStart;
            tb.SelectionEnd = selectionStart + newText.Length;
        });

        return true;
    }

    /// <summary>
    /// Applies a "surround with" pair to <paramref name="text"/> according to <paramref name="behavior"/>.
    /// <paramref name="added"/> tells whether the pair was added (true) or removed (false).
    /// </summary>
    public static string Apply(SurroundWithBehavior behavior, string surroundLeft, string text, string surroundRight, out bool added)
    {
        switch (behavior)
        {
            case SurroundWithBehavior.Add:
                added = true;
                return AddKeepExisting(surroundLeft, text, surroundRight);
            case SurroundWithBehavior.Remove:
                added = false;
                return Utilities.RemoveSymbols(surroundLeft, text, surroundRight);
            case SurroundWithBehavior.RemoveOnce:
                added = false;
                return RemoveOnce(surroundLeft, text, surroundRight);
            default:
                return Utilities.ToggleSymbols(surroundLeft, text, surroundRight, out added);
        }
    }

    /// <summary>
    /// Applies a "surround with" pair to the texts of several subtitles. With "toggle", the first
    /// (non-empty) line decides whether the pair is added or removed everywhere, so a mixed
    /// selection ends up consistent instead of flipping. With <see cref="SurroundWithScope.EachLine"/>
    /// every line of a text gets its own pair; blank lines are left alone.
    /// </summary>
    public static List<string> ApplyToTexts(SurroundWithBehavior behavior, SurroundWithScope scope,
        string surroundLeft, IEnumerable<string> texts, string surroundRight)
    {
        bool? add = behavior switch
        {
            SurroundWithBehavior.Add => true,
            SurroundWithBehavior.Remove => false,
            SurroundWithBehavior.RemoveOnce => false,
            _ => null,
        };

        string ApplyOne(string text)
        {
            if (add == null)
            {
                var result = Utilities.ToggleSymbols(surroundLeft, text, surroundRight, out var added);
                add = added;
                return result;
            }

            if (behavior == SurroundWithBehavior.Add)
            {
                return AddKeepExisting(surroundLeft, text, surroundRight);
            }

            if (behavior == SurroundWithBehavior.RemoveOnce)
            {
                return RemoveOnce(surroundLeft, text, surroundRight);
            }

            return add.Value
                ? Utilities.AddSymbols(surroundLeft, text, surroundRight)
                : Utilities.RemoveSymbols(surroundLeft, text, surroundRight);
        }

        var results = new List<string>();
        foreach (var text in texts)
        {
            if (scope != SurroundWithScope.EachLine)
            {
                results.Add(ApplyOne(text));
                continue;
            }

            var lines = text.SplitToLines();
            for (var i = 0; i < lines.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(lines[i]))
                {
                    lines[i] = ApplyOne(lines[i]);
                }
            }

            results.Add(string.Join(Environment.NewLine, lines));
        }

        return results;
    }

    /// <summary>
    /// Like <see cref="Utilities.AddSymbols"/>, but without removing the pair first - so firing an
    /// "add" slot twice adds the pair twice, like SE 4's custom tags toggle did (#15531).
    /// </summary>
    private static string AddKeepExisting(string tag, string text, string endTag)
    {
        var pre = string.Empty;
        var post = string.Empty;
        text = Utilities.SplitStartTags(text, ref pre);
        text = Utilities.SplitEndTags(text, ref post);

        if (!string.IsNullOrEmpty(tag) && tag == Configuration.Settings.Tools.MusicSymbol)
        {
            if (Configuration.Settings.Tools.MusicSymbolStyle.Equals("single", StringComparison.OrdinalIgnoreCase))
            {
                return pre + tag + " " + text.Replace(Environment.NewLine, Environment.NewLine + tag + " ") + post;
            }

            return pre + tag + " " + text.Replace(Environment.NewLine, " " + tag + Environment.NewLine + tag + " ") + " " + tag + post;
        }

        return pre + tag + text + endTag + post;
    }

    /// <summary>
    /// Removes one pair only - the outermost one, which is the one <see cref="AddKeepExisting"/>
    /// added last - so an "add" slot fired three times needs three "remove once" presses (#15531).
    /// </summary>
    private static string RemoveOnce(string tag, string text, string endTag)
    {
        var pre = string.Empty;
        var post = string.Empty;
        text = Utilities.SplitStartTags(text, ref pre);
        text = Utilities.SplitEndTags(text, ref post);

        if (!string.IsNullOrEmpty(tag) && tag == Configuration.Settings.Tools.MusicSymbol)
        {
            // Music symbols are added per line with a space ("♪ Hello ♪"), so remove one from each line.
            var lines = text.SplitToLines();
            for (var i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                if (line.StartsWith(tag, StringComparison.Ordinal))
                {
                    line = line.Substring(tag.Length).TrimStart(' ');
                }

                if (line.EndsWith(tag, StringComparison.Ordinal))
                {
                    line = line.Substring(0, line.Length - tag.Length).TrimEnd(' ');
                }

                lines[i] = line;
            }

            return pre + string.Join(Environment.NewLine, lines) + post;
        }

        var start = string.IsNullOrEmpty(tag) ? -1 : text.IndexOf(tag, StringComparison.Ordinal);
        var end = string.IsNullOrEmpty(endTag) ? -1 : text.LastIndexOf(endTag, StringComparison.Ordinal);

        // Same text before and after (like "*"): a single occurrence is only removed once.
        if (start >= 0 && end >= 0 && end < start + tag.Length)
        {
            end = -1;
        }

        if (end >= 0)
        {
            text = text.Remove(end, endTag.Length);
        }

        if (start >= 0)
        {
            text = text.Remove(start, tag.Length);
        }

        return pre + text + post;
    }
}
