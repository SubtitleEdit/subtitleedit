using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Controls.SyntaxTextEditorControl;

/// <summary>
/// The text behind <see cref="SyntaxTextView"/>, stored as lines so the view can lay out only the
/// lines it draws. Line breaks are normalized to <see cref="NewLine"/> (the first break found in
/// the text that was loaded), which keeps offsets stable no matter how the file was written.
///
/// Offsets are absolute character indexes into <see cref="Text"/> and count the line break
/// characters; positions are (line, column), both zero based.
/// </summary>
public sealed class SyntaxTextDocument
{
    private readonly List<string> _lines = new() { string.Empty };

    // Start offset of every line. Rebuilt lazily after an edit: O(lines) and only when something
    // actually asks for an offset, so a burst of edits pays for it once.
    private int[] _lineStarts = new int[1];
    private bool _lineStartsValid;

    private string? _cachedText;

    public SyntaxTextDocument()
    {
        NewLine = Environment.NewLine;
    }

    /// <summary>Raised after every change, with the offset range that changed.</summary>
    public event EventHandler<SyntaxTextDocumentChangedEventArgs>? Changed;

    /// <summary>The line break used between lines - taken from the loaded text.</summary>
    public string NewLine { get; private set; }

    /// <summary>Bumped on every change; the view uses it to drop cached line layouts.</summary>
    public int Version { get; private set; }

    public int LineCount => _lines.Count;

    public string GetLine(int line) => _lines[line];

    public int GetLineLength(int line) => _lines[line].Length;

    public string Text
    {
        get => _cachedText ??= BuildText();
        set => SetText(value);
    }

    public int TextLength
    {
        get
        {
            EnsureLineStarts();

            // _lineStarts is over-allocated, so the last line's start is at LineCount - 1, not [^1].
            return _lineStarts[_lines.Count - 1] + _lines[^1].Length;
        }
    }

    private string BuildText()
    {
        if (_lines.Count == 1)
        {
            return _lines[0];
        }

        // TextLength counts every line plus a NewLine between each, so the result can be
        // written straight into the string's buffer.
        return string.Create(TextLength, this, static (span, doc) =>
        {
            var pos = 0;
            for (var i = 0; i < doc._lines.Count; i++)
            {
                if (i > 0)
                {
                    doc.NewLine.AsSpan().CopyTo(span[pos..]);
                    pos += doc.NewLine.Length;
                }

                doc._lines[i].AsSpan().CopyTo(span[pos..]);
                pos += doc._lines[i].Length;
            }
        });
    }

    private void SetText(string? text)
    {
        text ??= string.Empty;

        NewLine = DetectNewLine(text);
        _lines.Clear();
        SplitInto(_lines, text);
        _lineStartsValid = false;
        _cachedText = null;
        Version++;
        Changed?.Invoke(this, new SyntaxTextDocumentChangedEventArgs(0, 0, text.Length, 0, 0, wholeDocument: true));
    }

    private static string DetectNewLine(string text)
    {
        var index = text.IndexOfAny(['\r', '\n']);
        if (index < 0)
        {
            return Environment.NewLine;
        }

        if (text[index] == '\n')
        {
            return "\n";
        }

        return index + 1 < text.Length && text[index + 1] == '\n' ? "\r\n" : "\r";
    }

    /// <summary>Splits on \r\n, \n and \r without allocating the intermediate array Split would.</summary>
    private static void SplitInto(List<string> target, string text)
    {
        var start = 0;
        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (c != '\r' && c != '\n')
            {
                continue;
            }

            target.Add(text.Substring(start, i - start));
            if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n')
            {
                i++;
            }

            start = i + 1;
        }

        target.Add(text.Substring(start));
    }

    private void EnsureLineStarts()
    {
        if (_lineStartsValid)
        {
            return;
        }

        if (_lineStarts.Length < _lines.Count)
        {
            _lineStarts = new int[Math.Max(_lines.Count, _lineStarts.Length * 2)];
        }

        var offset = 0;
        var newLineLength = NewLine.Length;
        for (var i = 0; i < _lines.Count; i++)
        {
            _lineStarts[i] = offset;
            offset += _lines[i].Length + newLineLength;
        }

        _lineStartsValid = true;
    }

    /// <summary>
    /// Adds <paramref name="delta"/> to the start of every line from <paramref name="fromLine"/> on.
    /// An edit inside one line moves everything below it by the same amount, and shifting the ints
    /// is far cheaper than rebuilding the index from the line strings (which walks the whole list
    /// and touches every string - milliseconds per keystroke in a 800 000 line file).
    /// </summary>
    private void ShiftLineStarts(int fromLine, int delta)
    {
        if (!_lineStartsValid || delta == 0)
        {
            return;
        }

        for (var i = fromLine; i < _lines.Count; i++)
        {
            _lineStarts[i] += delta;
        }
    }

    public int GetLineStartOffset(int line)
    {
        EnsureLineStarts();
        return _lineStarts[line];
    }

    public int GetLineEndOffset(int line) => GetLineStartOffset(line) + _lines[line].Length;

    /// <summary>The line containing <paramref name="offset"/>; the offset is clamped to the text.</summary>
    public int GetLineForOffset(int offset)
    {
        EnsureLineStarts();
        if (offset <= 0)
        {
            return 0;
        }

        // Binary search over the line starts; Array.BinarySearch needs the exact length because
        // _lineStarts is over-allocated.
        var index = Array.BinarySearch(_lineStarts, 0, _lines.Count, offset);
        if (index >= 0)
        {
            return index;
        }

        return Math.Min(~index - 1, _lines.Count - 1);
    }

    public SyntaxTextPosition GetPosition(int offset)
    {
        offset = Math.Clamp(offset, 0, TextLength);
        var line = GetLineForOffset(offset);
        var column = Math.Min(offset - GetLineStartOffset(line), _lines[line].Length);
        return new SyntaxTextPosition(line, column);
    }

    public int GetOffset(int line, int column)
    {
        line = Math.Clamp(line, 0, _lines.Count - 1);
        column = Math.Clamp(column, 0, _lines[line].Length);
        return GetLineStartOffset(line) + column;
    }

    public char GetCharAt(int offset)
    {
        var position = GetPosition(offset);
        var line = _lines[position.Line];
        if (position.Column < line.Length)
        {
            return line[position.Column];
        }

        // Inside the line break: report '\n' so word navigation can treat it as one stop.
        return '\n';
    }

    public string GetText(int offset, int length)
    {
        if (length <= 0)
        {
            return string.Empty;
        }

        var start = Math.Clamp(offset, 0, TextLength);
        var end = Math.Clamp(offset + length, start, TextLength);

        var startPosition = GetPosition(start);
        var endPosition = GetPosition(end);

        if (startPosition.Line == endPosition.Line)
        {
            return _lines[startPosition.Line].Substring(startPosition.Column, endPosition.Column - startPosition.Column);
        }

        var sb = new StringBuilder(end - start);
        sb.Append(_lines[startPosition.Line], startPosition.Column, _lines[startPosition.Line].Length - startPosition.Column);
        for (var line = startPosition.Line + 1; line < endPosition.Line; line++)
        {
            sb.Append(NewLine).Append(_lines[line]);
        }

        sb.Append(NewLine).Append(_lines[endPosition.Line], 0, endPosition.Column);
        return sb.ToString();
    }

    public void Insert(int offset, string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        var position = GetPosition(offset);
        var line = _lines[position.Line];
        var head = line[..position.Column];
        var tail = line[position.Column..];
        var linesBefore = _lines.Count;

        if (text.IndexOfAny(['\r', '\n']) < 0)
        {
            _lines[position.Line] = head + text + tail;

            // Nothing moved to another line, so the index only needs shifting.
            ShiftLineStarts(position.Line + 1, text.Length);
        }
        else
        {
            var inserted = new List<string>();
            SplitInto(inserted, text);
            inserted[0] = head + inserted[0];
            inserted[^1] += tail;

            _lines[position.Line] = inserted[0];
            if (inserted.Count > 1)
            {
                _lines.InsertRange(position.Line + 1, inserted.GetRange(1, inserted.Count - 1));
            }

            _lineStartsValid = false;
        }

        OnChanged(offset, 0, text.Length, position.Line, _lines.Count - linesBefore);
    }

    public void Remove(int offset, int length)
    {
        if (length <= 0)
        {
            return;
        }

        var start = Math.Clamp(offset, 0, TextLength);
        var end = Math.Clamp(offset + length, start, TextLength);
        if (start == end)
        {
            return;
        }

        var startPosition = GetPosition(start);
        var endPosition = GetPosition(end);

        var head = _lines[startPosition.Line][..startPosition.Column];
        var tail = _lines[endPosition.Line][endPosition.Column..];
        _lines[startPosition.Line] = head + tail;

        var removedLines = endPosition.Line - startPosition.Line;
        if (removedLines > 0)
        {
            _lines.RemoveRange(startPosition.Line + 1, removedLines);
            _lineStartsValid = false;
        }
        else
        {
            ShiftLineStarts(startPosition.Line + 1, -(end - start));
        }

        OnChanged(start, end - start, 0, startPosition.Line, -removedLines);
    }

    // Callers keep _lineStartsValid up to date themselves: an edit within one line shifts the
    // index, anything that adds or removes lines invalidates it.
    private void OnChanged(int offset, int removedLength, int insertedLength, int startLine, int lineCountDelta)
    {
        _cachedText = null;
        Version++;
        Changed?.Invoke(this, new SyntaxTextDocumentChangedEventArgs(
            offset,
            removedLength,
            insertedLength,
            startLine,
            lineCountDelta,
            wholeDocument: false));
    }
}

public readonly record struct SyntaxTextPosition(int Line, int Column);

public sealed class SyntaxTextDocumentChangedEventArgs : EventArgs
{
    public SyntaxTextDocumentChangedEventArgs(
        int offset,
        int removedLength,
        int insertedLength,
        int startLine,
        int lineCountDelta,
        bool wholeDocument)
    {
        Offset = offset;
        RemovedLength = removedLength;
        InsertedLength = insertedLength;
        StartLine = startLine;
        LineCountDelta = lineCountDelta;
        WholeDocument = wholeDocument;
    }

    public int Offset { get; }

    public int RemovedLength { get; }

    public int InsertedLength { get; }

    /// <summary>The first line the change touched.</summary>
    public int StartLine { get; }

    /// <summary>
    /// How many lines the change added (positive) or removed (negative). Zero means the edit stayed
    /// inside one line, so nothing below it moved - which is what lets the view keep its cache.
    /// </summary>
    public int LineCountDelta { get; }

    /// <summary>True when the whole text was replaced (Text setter) rather than edited.</summary>
    public bool WholeDocument { get; }
}
