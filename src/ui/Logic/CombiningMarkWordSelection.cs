using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;
using System.Globalization;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Keeps combining marks inside the word for double-click selection and Ctrl+Backspace/Delete in
/// every text box. Avalonia's word logic only counts letters and digits as word characters, so a
/// non-spacing mark - Arabic tanween in "مرحبًا", Hebrew niqqud, a decomposed accent - split the word
/// in two: double-click selected one half, Ctrl+Backspace removed only the mark (#16750).
/// The handlers run after Avalonia's own and only touch selections that end next to such a mark,
/// so text without combining marks behaves exactly as before.
/// </summary>
public static class CombiningMarkWordSelection
{
    private static TextBox? _wordDragTextBox;
    private static bool _registered;

    /// <summary>
    /// Letters, digits, combining marks and the zero-width (non-)joiners Persian uses inside words.
    /// '_' is deliberately left out to keep Avalonia's word stops for plain text.
    /// </summary>
    public static bool IsWordChar(char c)
    {
        if (char.IsLetterOrDigit(c) || c == '‌' || c == '‍')
        {
            return true;
        }

        return IsCombiningMark(c);
    }

    public static bool IsCombiningMark(char c)
    {
        var category = CharUnicodeInfo.GetUnicodeCategory(c);
        return category is UnicodeCategory.NonSpacingMark or
                           UnicodeCategory.SpacingCombiningMark or
                           UnicodeCategory.EnclosingMark;
    }

    /// <summary>
    /// Widens a selection whose ends cut through a word at a combining mark. Ends that already sit
    /// on a word boundary are returned unchanged.
    /// </summary>
    public static (int Start, int End) SnapToWordEdges(string text, int start, int end)
    {
        if (string.IsNullOrEmpty(text) || start >= end)
        {
            return (start, end);
        }

        start = Math.Clamp(start, 0, text.Length);
        end = Math.Clamp(end, 0, text.Length);

        while (start > 0 && start < text.Length && IsWordChar(text[start - 1]) && IsWordChar(text[start]))
        {
            start--;
        }

        while (end > 0 && end < text.Length && IsWordChar(text[end - 1]) && IsWordChar(text[end]))
        {
            end++;
        }

        return (start, end);
    }

    /// <summary>Start of the word Ctrl+Backspace removes: trailing whitespace, then one word.</summary>
    public static int PreviousWordStart(string text, int caret)
    {
        var index = Math.Clamp(caret, 0, text.Length);
        while (index > 0 && text[index - 1] != '\n' && char.IsWhiteSpace(text[index - 1]))
        {
            index--;
        }

        while (index > 0 && IsWordChar(text[index - 1]))
        {
            index--;
        }

        return index;
    }

    /// <summary>End of the word Ctrl+Delete removes: one word, then the whitespace after it.</summary>
    public static int NextWordEnd(string text, int caret)
    {
        var index = Math.Clamp(caret, 0, text.Length);
        while (index < text.Length && IsWordChar(text[index]))
        {
            index++;
        }

        while (index < text.Length && text[index] != '\r' && text[index] != '\n' && char.IsWhiteSpace(text[index]))
        {
            index++;
        }

        return index;
    }

    private static bool ContainsCombiningMark(string text, int start, int end)
    {
        for (var i = Math.Max(0, start); i < Math.Min(end, text.Length); i++)
        {
            if (IsCombiningMark(text[i]))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Registers the app-wide text box handlers. Idempotent - safe to call again (e.g. from tests).</summary>
    public static void Register()
    {
        if (_registered)
        {
            return;
        }

        _registered = true;

        // Bubble + handledEventsToo: TextBox marks these handled after making its own selection,
        // and class handlers added here run after TextBox's built-in ones.
        InputElement.PointerPressedEvent.AddClassHandler<TextBox>(OnPointerPressed, RoutingStrategies.Bubble, handledEventsToo: true);
        InputElement.PointerMovedEvent.AddClassHandler<TextBox>(OnPointerMoved, RoutingStrategies.Bubble, handledEventsToo: true);
        InputElement.PointerReleasedEvent.AddClassHandler<TextBox>((_, _) => _wordDragTextBox = null, RoutingStrategies.Bubble, handledEventsToo: true);

        // Tunnel: select the whole word before TextBox runs its own Ctrl+Backspace/Delete, which then
        // just deletes the selection (keeping undo).
        InputElement.KeyDownEvent.AddClassHandler<TextBox>(OnKeyDown, RoutingStrategies.Tunnel);
    }

    private static void OnPointerPressed(TextBox textBox, PointerPressedEventArgs e)
    {
        _wordDragTextBox = null;
        if (e.ClickCount != 2 || !e.GetCurrentPoint(textBox).Properties.IsLeftButtonPressed)
        {
            return;
        }

        _wordDragTextBox = textBox;
        SnapSelection(textBox);
    }

    private static void OnPointerMoved(TextBox textBox, PointerEventArgs e)
    {
        // Dragging after a double-click extends by words; Avalonia recomputes the range on every move.
        if (ReferenceEquals(_wordDragTextBox, textBox) && e.GetCurrentPoint(textBox).Properties.IsLeftButtonPressed)
        {
            SnapSelection(textBox);
        }
    }

    internal static void SnapSelection(TextBox textBox)
    {
        var text = textBox.Text;
        if (string.IsNullOrEmpty(text) || textBox.PasswordChar != default)
        {
            return;
        }

        var anchor = textBox.SelectionStart;
        var active = textBox.SelectionEnd;
        var (start, end) = SnapToWordEdges(text, Math.Min(anchor, active), Math.Max(anchor, active));
        if (start == Math.Min(anchor, active) && end == Math.Max(anchor, active))
        {
            return;
        }

        if (anchor <= active)
        {
            textBox.SelectionStart = start;
            textBox.SelectionEnd = end;
        }
        else
        {
            textBox.SelectionStart = end;
            textBox.SelectionEnd = start;
        }
    }

    private static void OnKeyDown(TextBox textBox, KeyEventArgs e)
    {
        if (e.Key is not (Key.Back or Key.Delete) || textBox.IsReadOnly || textBox.PasswordChar != default)
        {
            return;
        }

        var wordModifiers = Application.Current?.PlatformSettings?.HotkeyConfiguration.WholeWordTextActionModifiers
                            ?? KeyModifiers.Control;
        if (e.KeyModifiers != wordModifiers || textBox.SelectionStart != textBox.SelectionEnd)
        {
            return;
        }

        var text = textBox.Text ?? string.Empty;
        var caret = Math.Clamp(textBox.CaretIndex, 0, text.Length);
        var (start, end) = e.Key == Key.Back
            ? (PreviousWordStart(text, caret), caret)
            : (caret, NextWordEnd(text, caret));

        if (start < end && ContainsCombiningMark(text, start, end))
        {
            textBox.SelectionStart = start;
            textBox.SelectionEnd = end;
        }
    }
}
