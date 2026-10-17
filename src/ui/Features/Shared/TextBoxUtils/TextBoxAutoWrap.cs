using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;
using System;

namespace Nikse.SubtitleEdit.Features.Shared.TextBoxUtils;

/// <summary>
/// "Auto-wrap while typing" from SE4 (#16087): when typing makes a single-line subtitle longer
/// than the single line max length, the line is broken at its last space, so the next words go
/// on line two. Only typed text triggers it - not paste, not the text swapping in when another
/// row is selected - and only a single line, so a line the user broke by hand is left alone.
/// </summary>
public static class TextBoxAutoWrap
{
    /// <summary>
    /// Wraps <paramref name="textBox"/> while typing when <see cref="SeGeneral.SubtitleTextBoxAutoWrapWhileTyping"/>
    /// is on. <paramref name="getLanguage"/> gives the language for the auto-break used when the
    /// line is far too long for a single break.
    /// </summary>
    public static void Attach(TextBox textBox, Func<string> getLanguage)
    {
        // TextBox handles TextInput itself, so listen to handled events too - by then the typed
        // character is in the text.
        textBox.AddHandler(InputElement.TextInputEvent, (_, _) =>
        {
            if (!Se.Settings.General.SubtitleTextBoxAutoWrapWhileTyping || textBox.IsReadOnly)
            {
                return;
            }

            var text = textBox.Text ?? string.Empty;
            if (TryWrap(text, textBox.CaretIndex, Se.Settings.General.SubtitleLineMaximumLength, getLanguage, out var newText, out var newCaret))
            {
                textBox.Text = newText;
                textBox.CaretIndex = newCaret;
            }
        }, RoutingStrategies.Bubble, handledEventsToo: true);
    }

    internal static bool TryWrap(string text, int caretIndex, int maxLength, Func<string> getLanguage, out string newText, out int newCaretIndex)
    {
        newText = text;
        newCaretIndex = caretIndex;

        if (maxLength <= 0 || text.Contains('\n') || text.Contains('\r'))
        {
            return false;
        }

        var length = HtmlUtil.RemoveHtmlTags(text, true).Length;
        if (length <= maxLength)
        {
            return false;
        }

        string wrapped;
        if (length > maxLength + 30)
        {
            wrapped = Utilities.AutoBreakLine(text, maxLength, 0, getLanguage());
        }
        else
        {
            var lastSpace = LastSpaceOutsideTags(text);
            if (lastSpace <= 0)
            {
                return false;
            }

            wrapped = text.Remove(lastSpace, 1).Insert(lastSpace, Environment.NewLine);
        }

        var breakIndex = wrapped.IndexOf(Environment.NewLine, StringComparison.Ordinal);
        if (breakIndex <= 0 || wrapped == text)
        {
            return false;
        }

        newText = wrapped;
        newCaretIndex = caretIndex >= text.Length
            ? wrapped.Length
            : MapCaret(text, caretIndex, wrapped);

        return true;
    }

    // The last space that is not inside an HTML tag (<font face="Arial">) or an ASSA override
    // block ({\fn Times New Roman}) - breaking there would corrupt the tag. -1 when none.
    private static int LastSpaceOutsideTags(string text)
    {
        var lastSpace = -1;
        var inHtmlTag = false;
        var inAssaTag = false;
        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (inHtmlTag)
            {
                inHtmlTag = c != '>';
            }
            else if (inAssaTag)
            {
                inAssaTag = c != '}';
            }
            else if (c == '<')
            {
                inHtmlTag = true;
            }
            else if (c == '{')
            {
                inAssaTag = true;
            }
            else if (c == ' ')
            {
                lastSpace = i;
            }
        }

        return lastSpace;
    }

    // Re-breaking can add, remove or move whitespace anywhere in the line, so map the caret by the
    // number of non-whitespace characters in front of it rather than by the length difference.
    private static int MapCaret(string oldText, int caretIndex, string newText)
    {
        var count = 0;
        for (var i = 0; i < caretIndex && i < oldText.Length; i++)
        {
            if (!char.IsWhiteSpace(oldText[i]))
            {
                count++;
            }
        }

        var index = 0;
        var seen = 0;
        while (index < newText.Length && seen < count)
        {
            if (!char.IsWhiteSpace(newText[index]))
            {
                seen++;
            }

            index++;
        }

        // The caret was after whitespace (e.g. the space just typed, now a line break) - keep it
        // after the whitespace in the new text too, so it sits at the start of the next word.
        if (caretIndex > 0 && char.IsWhiteSpace(oldText[caretIndex - 1]))
        {
            while (index < newText.Length && char.IsWhiteSpace(newText[index]))
            {
                index++;
            }
        }

        return Math.Min(index, newText.Length);
    }
}
