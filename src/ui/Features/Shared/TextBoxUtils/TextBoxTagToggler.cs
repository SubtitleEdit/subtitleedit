using Avalonia.Threading;
using Nikse.SubtitleEdit.Core.Common;
using System;

namespace Nikse.SubtitleEdit.Features.Shared.TextBoxUtils;

/// <summary>
/// Toggles a formatting tag like &lt;i&gt; (or "{\i1}" for ASSA) in a text box, on the
/// selected text or on the whole/current line when nothing is selected.
/// </summary>
public static class TextBoxTagToggler
{
    public static bool ToggleTag(ITextBoxWrapper? tb, string tag, bool isAssa)
    {
        if (tb == null || tb.Text == null)
        {
            return false;
        }

        var selectionStart = Math.Min(tb.SelectionStart, tb.SelectionEnd);
        var selectionEnd = Math.Max(tb.SelectionStart, tb.SelectionEnd);
        var selectionLength = selectionEnd - selectionStart;

        // No text selected - toggle the whole line (or just the current dialog line).
        if (selectionLength == 0)
        {
            var text = tb.Text;
            var lines = text.SplitToLines();

            // Find the line where the caret is currently located (do not count wrapped lines).
            var numberOfNewLines = 0;
            for (var i = 0; i < selectionStart && i < text.Length; i++)
            {
                if (text[i] == '\n')
                {
                    numberOfNewLines++;
                }
            }

            var selectedLineIdx = Math.Min(numberOfNewLines, lines.Count - 1);
            var selectedLine = lines[selectedLineIdx];

            // When the caret is on a dialog line ("- ..."), only toggle that line so the
            // other speaker's line keeps its formatting.
            var isDialog = selectedLine.StartsWith('-') ||
                           selectedLine.StartsWith("<" + tag + ">-", StringComparison.OrdinalIgnoreCase);

            var textLen = text.Length;
            if (isDialog)
            {
                lines[selectedLineIdx] = HtmlUtil.ToggleTag(selectedLine, tag, false, isAssa);
                text = string.Join(Environment.NewLine, lines);
            }
            else
            {
                text = HtmlUtil.ToggleTag(text, tag, false, isAssa);
            }

            tb.Text = text;

            // Keep the caret next to where it was, shifting by the length of the opening
            // tag inserted before it: "<i>" (3) for HTML, "{\i1}" (5) for ASSA.
            var openTagLength = isAssa ? tag.Length + 4 : tag.Length + 2;
            var newCaret = textLen > text.Length
                ? Math.Max(selectionStart - openTagLength, 0)
                : selectionStart + openTagLength;
            Dispatcher.UIThread.Post(() =>
            {
                tb.Focus();
                tb.SelectionStart = newCaret;
                tb.SelectionEnd = newCaret;
            });
        }
        else
        {
            // Move leading/trailing white-space (spaces and new-lines) outside the tag so
            // " 'word'" becomes " <i>'word'</i>" instead of "<i> 'word'</i>".
            var pre = string.Empty;
            var post = string.Empty;
            var selectedText = tb.Text.Substring(selectionStart, selectionLength);
            while (selectedText.EndsWith(' ') || selectedText.EndsWith(Environment.NewLine, StringComparison.Ordinal) ||
                   selectedText.StartsWith(' ') || selectedText.StartsWith(Environment.NewLine, StringComparison.Ordinal))
            {
                if (selectedText.EndsWith(' '))
                {
                    post = " " + post;
                    selectedText = selectedText.Remove(selectedText.Length - 1);
                }

                if (selectedText.EndsWith(Environment.NewLine, StringComparison.Ordinal))
                {
                    post = Environment.NewLine + post;
                    selectedText = selectedText.Remove(selectedText.Length - Environment.NewLine.Length);
                }

                if (selectedText.StartsWith(' '))
                {
                    pre += " ";
                    selectedText = selectedText.Remove(0, 1);
                }

                if (selectedText.StartsWith(Environment.NewLine, StringComparison.Ordinal))
                {
                    pre += Environment.NewLine;
                    selectedText = selectedText.Remove(0, Environment.NewLine.Length);
                }
            }

            selectedText = pre + HtmlUtil.ToggleTag(selectedText, tag, false, isAssa) + post;

            tb.Text = tb.Text
                .Remove(selectionStart, selectionLength)
                .Insert(selectionStart, selectedText);

            Dispatcher.UIThread.Post(() =>
            {
                tb.Focus();
                tb.SelectionStart = selectionStart;
                tb.SelectionEnd = selectionStart + selectedText.Length;
            });
        }

        return true;
    }
}
