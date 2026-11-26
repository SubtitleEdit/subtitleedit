using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.Common;
using System;
using System.Linq;

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
    /// <remarks>
    /// With <paramref name="getLanguageCode"/> the box edits subtitle lines: a paste with more
    /// lines than the line limit allows - text copied from a PDF keeps the PDF's own line
    /// wrapping (#17137) - is joined and auto-broken again, see <see cref="LooksHardWrapped"/>.
    /// </remarks>
    public static void NormalizeLineBreaksOnPaste(TextBox textBox, Func<string>? getLanguageCode = null)
    {
        textBox.AddHandler(TextBox.PastingFromClipboardEvent, (sender, e) => OnPastingFromClipboard(sender, e, getLanguageCode));
    }

    private static async void OnPastingFromClipboard(object? sender, RoutedEventArgs e, Func<string>? getLanguageCode)
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

        if (getLanguageCode != null &&
            !string.IsNullOrEmpty(text) &&
            Se.Settings.General.SubtitleTextBoxLimitNewLines &&
            LooksHardWrapped(text, Se.Settings.General.MaxNumberOfLines))
        {
            PasteAndAutoBreak(textBox, text, getLanguageCode());
            return;
        }

        InsertNormalized(textBox, text);
    }

    /// <summary>
    /// Replaces the selection of <paramref name="textBox"/> with <paramref name="pasted"/> as one
    /// line and auto-breaks the whole text again.
    /// </summary>
    internal static void PasteAndAutoBreak(TextBox textBox, string pasted, string languageCode)
    {
        if (textBox.IsReadOnly)
        {
            return;
        }

        var text = textBox.Text ?? string.Empty;
        textBox.Text = ComposePasteAndAutoBreak(text, textBox.SelectionStart, textBox.SelectionEnd, pasted, languageCode);
        textBox.CaretIndex = textBox.Text.Length;
    }

    /// <summary>
    /// <paramref name="text"/> with the range <paramref name="selectionStart"/>-<paramref name="selectionEnd"/>
    /// replaced by <paramref name="pasted"/> joined to one line, then unbroken and auto-broken
    /// as a whole. A space is kept between the pasted text and the words around it, except
    /// next to a script written without spaces between words.
    /// </summary>
    internal static string ComposePasteAndAutoBreak(string text, int selectionStart, int selectionEnd, string pasted, string languageCode)
    {
        text ??= string.Empty;
        var start = Math.Clamp(Math.Min(selectionStart, selectionEnd), 0, text.Length);
        var end = Math.Clamp(Math.Max(selectionStart, selectionEnd), start, text.Length);
        var before = text.Substring(0, start);
        var after = text.Substring(end);

        var oneLine = UnbreakLine(NormalizeBreakChars(pasted ?? string.Empty)).Trim();
        if (oneLine.Length == 0)
        {
            return text;
        }

        // No space next to Chinese, Japanese, Thai... - those scripts have none between words.
        if (before.Length > 0 && !char.IsWhiteSpace(before[^1]) && UnspacedScript.NeedsSpaceBetween(before[^1], oneLine[0]))
        {
            oneLine = " " + oneLine;
        }

        if (after.Length > 0 && !char.IsWhiteSpace(after[0]) && UnspacedScript.NeedsSpaceBetween(oneLine[^1], after[0]))
        {
            oneLine += " ";
        }

        var unbroken = UnbreakLine(before + oneLine + after);
        return Utilities.AutoBreakLine(unbroken, languageCode);
    }

    /// <summary>
    /// <see cref="Utilities.UnbreakLine"/>, except that a break next to a script written without
    /// spaces between words (Chinese, Japanese, Thai...) is joined without one - a PDF wraps such
    /// text anywhere, and a space there would be a new character in the text (#17137).
    /// </summary>
    internal static string UnbreakLine(string text)
    {
        var lines = text.SplitToLines();
        if (lines.Count == 1)
        {
            return lines[0];
        }

        var trimmed = lines.Select(line => line.Trim()).ToList();
        var touchesUnspacedScript = false;
        for (var i = 1; i < trimmed.Count && !touchesUnspacedScript; i++)
        {
            touchesUnspacedScript = trimmed[i - 1].Length > 0 && trimmed[i].Length > 0 &&
                                    !UnspacedScript.NeedsSpaceBetween(trimmed[i - 1][^1], trimmed[i][0]);
        }

        return touchesUnspacedScript
            ? UnspacedScript.JoinLines(trimmed).FixExtraSpaces()
            : Utilities.UnbreakLine(text);
    }

    /// <summary>
    /// True when <paramref name="text"/> looks like running text wrapped by its source (a PDF, a
    /// web page, an e-mail) rather than subtitle lines: more lines than
    /// <paramref name="maxNumberOfLines"/>, no blank line between them (separate paragraphs or
    /// subtitles), no dialog dash starting a line, and no time codes.
    /// </summary>
    internal static bool LooksHardWrapped(string text, int maxNumberOfLines)
    {
        if (string.IsNullOrWhiteSpace(text) || maxNumberOfLines < 1)
        {
            return false;
        }

        var lines = NormalizeBreakChars(text).Trim().SplitToLines();
        if (lines.Count <= maxNumberOfLines)
        {
            return false;
        }

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.Length == 0 || trimmed.Contains("-->"))
            {
                return false;
            }

            var first = HtmlUtil.RemoveHtmlTags(trimmed, true).TrimStart();
            if (first.Length > 0 && (first[0] == '-' || first[0] == '‐' || first[0] == '–' || first[0] == '—'))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Turns the line breaks <see cref="StringExtensions.SplitToLines(string)"/> does not know
    /// (paragraph separator, next line, vertical tab, form feed - PDF viewers copy some of these)
    /// into '\n'; left alone the text box shows them as a box or nothing at all.
    /// </summary>
    internal static string NormalizeBreakChars(string text)
    {
        if (string.IsNullOrEmpty(text) || text.AsSpan().IndexOfAny("\u2029\u0085\v\f") < 0)
        {
            return text;
        }

        return text.Replace('\u2029', '\n').Replace('\u0085', '\n').Replace('\v', '\n').Replace('\f', '\n');
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
        textBox.SelectedText = NormalizeBreakChars(text).NormalizeLineBreaks();
    }
}
