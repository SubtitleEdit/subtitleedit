using Avalonia.Media;
using Avalonia.Threading;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Rendering;
using System;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// In a right-to-left text box the Unicode bidi algorithm visually reorders the neutral
/// punctuation of formatting tags: "{\an8}" displays as "{an8\}" and "&lt;/i&gt;" as "i/&lt;".
/// This generator replaces each tag with an atomic element that renders the tag text in its
/// own little left-to-right line, so tags display literally exactly as typed. The document
/// text is untouched. Active only while the text view flows right-to-left.
///
/// Rendering the tag as one drawable element is deliberate: injecting Unicode directional
/// marks (zero-width characters) into the displayed text fixes the ordering too, but their
/// zero-width glyph runs break Avalonia's TextLine geometry (GetTextBounds and
/// GetCharacterHitFromDistance return garbage), which put selection highlights beside the
/// text and made mouse clicks pick wrong offsets.
///
/// To keep tags editable, the tag under the caret is not atomized: clicking a tag puts the
/// caret at its border, which "opens" it into plain per-character-editable text (bidi-shuffled
/// while open, like a markdown editor showing raw syntax under the caret); moving the caret
/// away snaps it back to the literal display. Tags open only while the text area is focused
/// with an empty selection, and the open state is applied via a posted dispatcher job so that
/// hit-testing inside the input event that moved the caret (e.g. the press starting a mouse
/// drag) still sees the layout the user clicked on.
/// </summary>
public class RtlTagIsolationGenerator : VisualLineElementGenerator
{
    private static readonly Regex TagRegex = new(@"\{\\[^{}\r\n]*\}|</?[a-zA-Z][^<>\r\n]*>", RegexOptions.Compiled);

    private readonly TextArea? _textArea;

    private bool _applyPending;

    /// <summary>Document line number of the currently open tag(s), else 0.</summary>
    private int _openLineNumber;

    /// <summary>Caret offset the open state was computed for.</summary>
    private int _openCaretOffset;

    /// <summary>Union range of the open tag(s), for change detection.</summary>
    private int _openStart, _openEnd;

    public RtlTagIsolationGenerator()
    {
    }

    public RtlTagIsolationGenerator(TextArea textArea)
    {
        _textArea = textArea;
        _textArea.Caret.PositionChanged += OnOpenStateMayHaveChanged;
        _textArea.SelectionChanged += OnOpenStateMayHaveChanged;
        _textArea.GotFocus += OnOpenStateMayHaveChanged;
        _textArea.LostFocus += OnOpenStateMayHaveChanged;
    }

    private void OnOpenStateMayHaveChanged(object? sender, EventArgs e)
    {
        if (_applyPending)
        {
            return;
        }

        _applyPending = true;
        Dispatcher.UIThread.Post(ApplyOpenState, DispatcherPriority.Background);
    }

    private void ApplyOpenState()
    {
        _applyPending = false;
        if (_textArea?.Document == null)
        {
            return;
        }

        var (lineNumber, caretOffset, start, end) = FindOpenState();
        if (lineNumber == _openLineNumber && start == _openStart && end == _openEnd)
        {
            _openCaretOffset = caretOffset;
            return;
        }

        var oldLineNumber = _openLineNumber;
        _openLineNumber = lineNumber;
        _openCaretOffset = caretOffset;
        _openStart = start;
        _openEnd = end;
        RedrawLine(oldLineNumber);
        if (lineNumber != oldLineNumber)
        {
            RedrawLine(lineNumber);
        }
    }

    /// <summary>
    /// The line, caret offset and union range of the tag(s) the caret currently touches, or
    /// all zeros when no tag should be open. Tags may only open while the user is actually
    /// editing at the caret: focused text area, no selection. This keeps the literal display
    /// when the box is just showing text, and keeps the layout stable while a mouse drag
    /// builds up a selection.
    /// </summary>
    private (int LineNumber, int CaretOffset, int Start, int End) FindOpenState()
    {
        if (_textArea?.Document == null
            || _textArea is not { IsFocused: true, Selection.IsEmpty: true }
            || _textArea.TextView.FlowDirection != FlowDirection.RightToLeft)
        {
            return (0, 0, 0, 0);
        }

        var document = _textArea.Document;
        var caretOffset = _textArea.Caret.Offset;
        if (caretOffset < 0 || caretOffset > document.TextLength)
        {
            return (0, 0, 0, 0);
        }

        var line = document.GetLineByOffset(caretOffset);
        var text = document.GetText(line.Offset, line.Length);
        var start = int.MaxValue;
        var end = int.MinValue;
        foreach (Match match in TagRegex.Matches(text))
        {
            var tagStart = line.Offset + match.Index;
            var tagEnd = tagStart + match.Length;
            if (caretOffset >= tagStart && caretOffset <= tagEnd)
            {
                start = Math.Min(start, tagStart);
                end = Math.Max(end, tagEnd);
            }
        }

        return start <= end ? (line.LineNumber, caretOffset, start, end) : (0, 0, 0, 0);
    }

    private void RedrawLine(int lineNumber)
    {
        var document = _textArea?.Document;
        if (_textArea == null || document == null || lineNumber < 1 || lineNumber > document.LineCount)
        {
            return;
        }

        var line = document.GetLineByNumber(lineNumber);
        _textArea.TextView.Redraw(line.Offset, line.Length);
    }

    /// <summary>The applied open state covers this tag, so it stays plain editable text.</summary>
    private bool IsOpenForEditing(int tagStart, int tagEnd)
    {
        return _openLineNumber != 0
               && _openCaretOffset >= tagStart
               && _openCaretOffset <= tagEnd
               && tagStart >= _openStart
               && tagEnd <= _openEnd;
    }

    public override int GetFirstInterestedOffset(int startOffset)
    {
        if (CurrentContext.TextView.FlowDirection != FlowDirection.RightToLeft)
        {
            return -1;
        }

        var endOffset = CurrentContext.VisualLine.LastDocumentLine.EndOffset;
        while (startOffset < endOffset)
        {
            var text = CurrentContext.GetText(startOffset, endOffset - startOffset);
            var match = TagRegex.Match(text.Text, text.Offset, text.Count);
            if (!match.Success)
            {
                return -1;
            }

            var tagStart = startOffset + match.Index - text.Offset;
            if (!IsOpenForEditing(tagStart, tagStart + match.Length))
            {
                return tagStart;
            }

            startOffset = tagStart + match.Length;
        }

        return -1;
    }

    public override VisualLineElement ConstructElement(int offset)
    {
        var endOffset = CurrentContext.VisualLine.LastDocumentLine.EndOffset;
        var text = CurrentContext.GetText(offset, endOffset - offset);
        var match = TagRegex.Match(text.Text, text.Offset, text.Count);
        return match.Success
               && match.Index == text.Offset
               && !IsOpenForEditing(offset, offset + match.Length)
            ? new FormattedTextElement(match.Value, match.Length)
            : null!;
    }
}
