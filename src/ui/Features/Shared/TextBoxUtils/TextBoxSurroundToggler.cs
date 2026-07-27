using Avalonia.Threading;
using Nikse.SubtitleEdit.Core.Common;
using System;

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
    public static bool ToggleSelection(ITextBoxWrapper? tb, string surroundLeft, string surroundRight)
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

        // "ToggleSymbols" keeps italic/bold/font tags of the selection outside the symbols,
        // so "<i>Hello</i>" becomes "<i>♪ Hello ♪</i>".
        var newText = pre + Utilities.ToggleSymbols(surroundLeft, selectedText, surroundRight, out _) + post;

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
}
