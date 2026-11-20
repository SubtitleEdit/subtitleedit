using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using System;

namespace Nikse.SubtitleEdit.Features.Shared.TextBoxUtils;

/// <summary>
/// Stops the subtitle edit box from keeping a stale selection when a command - dialog dashes,
/// surround with, italic, ... - replaces the whole text through the binding. The text box only
/// clamps the old selection range to the new text, so it ends up covering an unrelated part of
/// the text (often stopping a few characters before the end), and the next "surround with"
/// shortcut works on that range instead of the whole line (#15693, #16015, #16131, #17052).
/// SE 4 never had this: setting a WinForms text box's text clears its selection.
/// </summary>
public static class TextBoxStaleSelectionGuard
{
    public enum Fix
    {
        None,
        SelectAll,
        Collapse,
    }

    public static void Attach(TextBox textBox)
    {
        textBox.PropertyChanged += (_, e) =>
        {
            if (e.Property != TextBox.TextProperty)
            {
                return;
            }

            var oldText = e.OldValue as string ?? string.Empty;
            var newText = e.NewValue as string ?? string.Empty;
            var selectionStart = textBox.SelectionStart;
            var selectionEnd = textBox.SelectionEnd;
            var fix = Decide(oldText, newText, selectionStart, selectionEnd);
            if (fix == Fix.None)
            {
                return;
            }

            // A user edit (typing, paste, cut, drop) clears the selection right after the text
            // changes, in the same call. Only a selection still exactly as the text box left it
            // after a replacement from outside is stale. Posted before any selection a
            // command itself posts (surround/italic on selected text), which then wins.
            Dispatcher.UIThread.Post(() =>
            {
                if (!string.Equals(textBox.Text ?? string.Empty, newText, StringComparison.Ordinal) ||
                    textBox.SelectionStart != selectionStart ||
                    textBox.SelectionEnd != selectionEnd)
                {
                    return;
                }

                Apply(textBox, fix);
            });
        };
    }

    /// <summary>
    /// Decides what to do with the selection (already clamped to <paramref name="newText"/> by the
    /// text box) after the text changed from <paramref name="oldText"/>: nothing when there is no
    /// selection, select all again when the whole old text was selected, else drop the selection.
    /// </summary>
    public static Fix Decide(string oldText, string newText, int selectionStart, int selectionEnd)
    {
        if (string.Equals(oldText, newText, StringComparison.Ordinal))
        {
            return Fix.None;
        }

        var start = Math.Min(selectionStart, selectionEnd);
        var end = Math.Max(selectionStart, selectionEnd);
        if (start == end)
        {
            return Fix.None;
        }

        // The whole old text selected: the end was clamped to the shorter of the two texts.
        // Already covering the whole new text (the text got shorter) needs nothing.
        if (start == 0 && oldText.Length > 0 && end >= Math.Min(oldText.Length, newText.Length))
        {
            return end >= newText.Length ? Fix.None : Fix.SelectAll;
        }

        return Fix.Collapse;
    }

    private static void Apply(TextBox textBox, Fix fix)
    {
        var length = textBox.Text?.Length ?? 0;
        if (fix == Fix.SelectAll)
        {
            textBox.SelectionStart = 0;
            textBox.SelectionEnd = length;
            return;
        }

        textBox.SelectionStart = length;
        textBox.SelectionEnd = length;
        textBox.CaretIndex = length;
    }
}
