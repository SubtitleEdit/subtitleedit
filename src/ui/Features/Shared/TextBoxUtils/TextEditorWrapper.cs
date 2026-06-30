using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using Nikse.SubtitleEdit.Features.SpellCheck;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.Features.Shared.TextBoxUtils;

public class TextEditorWrapper : ITextBoxWrapper
{
    private readonly TextEditor _textEditor;
    private readonly Border _border;
    private readonly SpellCheckUnderlineTransformer _spellCheckTransformer;
    private readonly SubtitleTextAlignmentTransformer _alignmentTransformer;
    private EventHandler? _alignmentUpdateHandler;
    private TextAlignment _currentAlignment = TextAlignment.Left;
    private int? _wordNavAnchor;

    public bool HasFocus { get; set; }

    public TextEditorWrapper(TextEditor textEditor, Border border)
    {
        _textEditor = textEditor;
        _border = border;

        _spellCheckTransformer = new SpellCheckUnderlineTransformer();
        _spellCheckTransformer.SetTextView(_textEditor.TextArea.TextView);
        _textEditor.TextArea.TextView.LineTransformers.Add(_spellCheckTransformer);

        _alignmentTransformer = new SubtitleTextAlignmentTransformer();
        _textEditor.TextArea.TextView.LineTransformers.Add(_alignmentTransformer);

        // Update alignment when text changes
        _textEditor.TextChanged += (_, _) =>
        {
            if (_currentAlignment != TextAlignment.Left)
            {
                UpdateAlignmentTransform();
            }
        };

        // Fix Option+Right/Left (Alt on macOS) word navigation to match standard TextBox behavior:
        // AvaloniaEdit moves to start of next word; macOS convention is end of current word.
        _textEditor.TextArea.AddHandler(
            InputElement.KeyDownEvent,
            OnTextAreaKeyDown,
            RoutingStrategies.Tunnel);
    }

    private void OnTextAreaKeyDown(object? sender, KeyEventArgs e)
    {
        if (sender is not TextArea textArea)
            return;

        var isAlt = (e.KeyModifiers & KeyModifiers.Alt) != 0;
        var isCtrl = (e.KeyModifiers & KeyModifiers.Control) != 0;
        var isShift = (e.KeyModifiers & KeyModifiers.Shift) != 0;

        // Only apply on macOS: Option+Arrow is the word navigation key there.
        // On Windows/Linux, Ctrl+Arrow is used and AvaloniaEdit's default behavior
        // already matches the platform convention.
        if (!isAlt || isCtrl || (e.Key != Key.Right && e.Key != Key.Left) || !RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return;

        var document = textArea.Document;
        var caretOffset = textArea.Caret.Offset;

        var newOffset = e.Key == Key.Right
            ? GetWordRightOffset(document, caretOffset)
            : GetWordLeftOffset(document, caretOffset);

        if (isShift)
        {
            // Initialise or reset the anchor: on first Shift+Option press, or whenever
            // the selection was cleared externally (click, non-Shift move) since last time.
            if (_wordNavAnchor == null || textArea.Selection.IsEmpty)
                _wordNavAnchor = caretOffset;

            textArea.Caret.Offset = newOffset;
            textArea.Selection = Selection.Create(textArea, _wordNavAnchor.Value, newOffset);
        }
        else
        {
            _wordNavAnchor = null;
            textArea.Caret.Offset = newOffset;
            textArea.Selection = Selection.Create(textArea, newOffset, newOffset);
        }

        e.Handled = true;
    }

    private static bool IsWordChar(char c) => char.IsLetterOrDigit(c) || c == '_';
    private static bool IsNonNewlineWhiteSpace(char c) => c != '\n' && char.IsWhiteSpace(c);

    private static int GetWordRightOffset(TextDocument document, int offset)
    {
        var length = document.TextLength;
        if (offset >= length)
            return length;

        // Skip non-newline whitespace (spaces, tabs)
        while (offset < length && IsNonNewlineWhiteSpace(document.GetCharAt(offset)))
            offset++;

        if (offset >= length)
            return length;

        // A newline is its own stop — consume it and land at start of next line
        if (document.GetCharAt(offset) == '\n')
            return offset + 1;

        // Skip a contiguous run of the same character category (alphanumeric vs punctuation)
        var wordChar = IsWordChar(document.GetCharAt(offset));
        while (offset < length
               && !char.IsWhiteSpace(document.GetCharAt(offset))
               && IsWordChar(document.GetCharAt(offset)) == wordChar)
            offset++;

        return offset;
    }

    private static int GetWordLeftOffset(TextDocument document, int offset)
    {
        if (offset <= 0)
            return 0;

        // Skip non-newline whitespace backward
        while (offset > 0 && IsNonNewlineWhiteSpace(document.GetCharAt(offset - 1)))
            offset--;

        if (offset == 0)
            return 0;

        // A newline is its own stop — consume it and land at end of previous line
        if (document.GetCharAt(offset - 1) == '\n')
            return offset - 1;

        // Skip a contiguous run of the same character category backward
        var wordChar = IsWordChar(document.GetCharAt(offset - 1));
        while (offset > 0
               && !char.IsWhiteSpace(document.GetCharAt(offset - 1))
               && IsWordChar(document.GetCharAt(offset - 1)) == wordChar)
            offset--;

        return offset;
    }

    public string Text
    {
        get => _textEditor.Text;
        set => _textEditor.Text = value;
    }

    public string SelectedText
    {
        get => _textEditor.SelectedText;
        set => _textEditor.SelectedText = value;
    }

    public int SelectionStart
    {
        get => _textEditor.SelectionStart;
        set => _textEditor.SelectionStart = value;
    }

    public int SelectionLength
    {
        get => _textEditor.SelectionLength;
        set => _textEditor.SelectionLength = value;
    }

    public int SelectionEnd
    {
        get => _textEditor.SelectionStart + _textEditor.SelectionLength;
        set => _textEditor.SelectionLength = Math.Max(0, value - _textEditor.SelectionStart);
    }

    public void Select(int start, int length)
    {
        // AvaloniaEdit's SelectionStart setter calls Select(value, currentSelectionLength),
        // which throws if value + currentSelectionLength > DocumentLength. Clear first.
        _textEditor.SelectionLength = 0;
        _textEditor.SelectionStart = start;
        _textEditor.SelectionLength = length;
        _textEditor.CaretOffset = start + length;
    }

    public int CaretIndex
    {
        get => _textEditor.CaretOffset;
        set => _textEditor.CaretOffset = value;
    }

    public void Focus()
    {
        _textEditor.Focus();
    }

    public Control TextControl => _textEditor;
    public Control ContentControl => _border;

    public bool IsFocused => HasFocus;

    public void Cut()
    {
        _textEditor.Cut();
    }

    public void Copy()
    {
        _textEditor.Copy();
    }

    public void Paste()
    {
        _textEditor.Paste();
    }

    public void SelectAll()
    {
        _textEditor.SelectAll();
    }

    public void ClearSelection()
    {
        _textEditor.SelectionLength = 0;
    }

    public void SetAlignment(TextAlignment alignment)
    {
        _alignmentTransformer.Alignment = alignment;
        _currentAlignment = alignment;

        var textArea = _textEditor.TextArea;
        var textView = textArea.TextView;

        // Remove old handler if exists
        if (_alignmentUpdateHandler != null)
        {
            textView.LayoutUpdated -= _alignmentUpdateHandler;
            _alignmentUpdateHandler = null;
        }

        // Keep the TextView and TextArea stretched to ensure proper hit testing
        textArea.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
        textView.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;

        // For centered or right alignment, use RenderTransform on TextArea
        if (alignment == TextAlignment.Center || alignment == TextAlignment.Right)
        {
            _alignmentUpdateHandler = (_, _) =>
            {
                UpdateAlignmentTransform();
            };

            textView.LayoutUpdated += _alignmentUpdateHandler;

            // Trigger initial update
            UpdateAlignmentTransform();
        }
        else
        {
            textArea.RenderTransform = null;
        }

        textView.Redraw();
    }

    private void UpdateAlignmentTransform()
    {
        var textArea = _textEditor.TextArea;
        var textView = textArea.TextView;

        if (textView.Bounds.Width <= 0 || textArea.Bounds.Width <= 0)
        {
            return;
        }

        // Measure the actual text width using FormattedText
        double maxLineWidth = 0;

        if (!string.IsNullOrEmpty(_textEditor.Text))
        {
            var typeface = new Typeface(_textEditor.FontFamily);
            var lines = _textEditor.Text.Split('\n');

            foreach (var line in lines)
            {
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                var formattedText = new FormattedText(
                    line,
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    typeface,
                    _textEditor.FontSize,
                    Brushes.Black);

                if (formattedText.Width > maxLineWidth)
                {
                    maxLineWidth = formattedText.Width;
                }
            }
        }

        var availableWidth = textArea.Bounds.Width;

        if (maxLineWidth > 0 && maxLineWidth < availableWidth)
        {
            double offset = _currentAlignment == TextAlignment.Center
                ? (availableWidth - maxLineWidth) / 2
                : availableWidth - maxLineWidth;

            textArea.RenderTransform = new TranslateTransform(offset, 0);
        }
        else
        {
            textArea.RenderTransform = null;
        }
    }

    public void EnableSpellCheck(ISpellCheckManager spellCheckManager)
    {
        _spellCheckTransformer.SetSpellCheckManager(spellCheckManager);
        _spellCheckTransformer.IsEnabled = true;
    }

    public void DisableSpellCheck()
    {
        _spellCheckTransformer.IsEnabled = false;
    }

    public void RefreshSpellCheck()
    {
        _spellCheckTransformer.Refresh();
    }

    public bool IsSpellCheckEnabled => _spellCheckTransformer.IsEnabled;

    /// <summary>
    /// Gets the word at the specified text offset position.
    /// </summary>
    public SpellCheckWord? GetWordAtOffset(int offset)
    {
        if (string.IsNullOrEmpty(Text) || offset < 0 || offset > Text.Length)
        {
            return null;
        }

        var words = SpellCheckWordLists.Split(Text);
        return words.FirstOrDefault(w => offset >= w.Index && offset < w.Index + w.Length);
    }

    /// <summary>
    /// Gets the word at the current caret position.
    /// </summary>
    public SpellCheckWord? GetWordAtCaret()
    {
        return GetWordAtOffset(CaretIndex);
    }

    /// <summary>
    /// Gets the word at the specified mouse position.
    /// Returns null if the position is not over a word.
    /// </summary>
    public SpellCheckWord? GetWordAtPosition(PointerEventArgs e)
    {
        var position = e.GetPosition(_textEditor.TextArea.TextView);
        var visualPosition = _textEditor.TextArea.TextView.GetPosition(position);

        if (visualPosition == null)
        {
            return null;
        }

        var offset = _textEditor.Document.GetOffset(visualPosition.Value.Location);
        return GetWordAtOffset(offset);
    }

    /// <summary>
    /// Checks if the word at the specified offset is misspelled.
    /// </summary>
    public bool IsWordMisspelledAtOffset(int offset)
    {
        var word = GetWordAtOffset(offset);
        if (word == null || string.IsNullOrWhiteSpace(word.Text))
        {
            return false;
        }

        if (!IsSpellCheckEnabled || _spellCheckTransformer == null)
        {
            return false;
        }

        return _spellCheckTransformer.IsWordMisspelled(word, Text);
    }

    /// <summary>
    /// Gets spelling suggestions for the word at the specified offset.
    /// </summary>
    public System.Collections.Generic.List<string>? GetSuggestionsForWordAtOffset(int offset)
    {
        var word = GetWordAtOffset(offset);
        if (word == null || string.IsNullOrWhiteSpace(word.Text))
        {
            return null;
        }

        return _spellCheckTransformer.GetSuggestions(word.Text);
    }
}
