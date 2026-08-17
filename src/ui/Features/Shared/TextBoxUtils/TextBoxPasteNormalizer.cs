using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Nikse.SubtitleEdit.Core.Common;
using System;

namespace Nikse.SubtitleEdit.Features.Shared.TextBoxUtils;

/// <summary>
/// Makes a text box paste subtitle text with SE's own line break.
///
/// Avalonia's <see cref="TextBox"/> inserts the clipboard string verbatim - its only sanitizing
/// step strips U+007F, and <see cref="TextBox.NewLine"/> is used for the Enter key only. The
/// Win32 edit control SE4 used normalized pasted line breaks to CRLF by itself, so this never
/// came up before. Everything else in SE composes subtitle text with
/// <see cref="Environment.NewLine"/>, so a paste from a LF file leaves a paragraph holding a
/// line break no other code path produces: tools that rebuild the text then report a change that
/// renders identically - "Remove text for hearing impaired" listing an untouched line (#13591) -
/// and line-break-sensitive edits such as merge miss the break they meant to strip.
/// </summary>
public static class TextBoxPasteNormalizer
{
    /// <summary>
    /// Takes over paste for <paramref name="textBox"/> so pasted line breaks become
    /// <see cref="Environment.NewLine"/>. Only for text boxes that edit subtitle text; a box
    /// showing free-form text (a source view, a log) should paste what the clipboard holds.
    /// </summary>
    public static void NormalizeLineBreaksOnPaste(TextBox textBox)
    {
        textBox.AddHandler(TextBox.PastingFromClipboardEvent, OnPastingFromClipboard);
    }

    private static async void OnPastingFromClipboard(object? sender, RoutedEventArgs e)
    {
        if (sender is not TextBox textBox)
        {
            return;
        }

        e.Handled = true; // suppress the default paste (must be set before the first await)

        var clipboard = TopLevel.GetTopLevel(textBox)?.Clipboard;
        if (clipboard == null)
        {
            return;
        }

        string? text;
        try
        {
            text = await clipboard.TryGetTextAsync();
        }
        catch (Exception)
        {
            return; // clipboard busy/denied - same as Avalonia's own paste, which logs and gives up
        }

        InsertNormalized(textBox, text);
    }

    /// <summary>
    /// Inserts <paramref name="text"/> with SE's line break, replacing the selection.
    /// </summary>
    internal static void InsertNormalized(TextBox textBox, string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        // Assigning SelectedText replaces the selection (or inserts at the caret when there is
        // none) through the same text input path the built-in paste uses: it takes the undo
        // snapshot, drops characters the box cannot hold, and leaves a read-only box alone.
        textBox.SelectedText = text.NormalizeLineBreaks();
    }
}
