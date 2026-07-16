using System;

using AvaloniaEdit.Document;

namespace AvaloniaEdit.TextMate
{
    public class DocumentSnapshot
    {
        private LineRange[] _lineRanges;
        private readonly TextDocument _document;
        private ITextSource _textSource;
        private readonly object _lock = new object();
        private int _lineCount;

        public int LineCount
        {
            get { lock (_lock) { return _lineCount; } }
        }

        public DocumentSnapshot(TextDocument document)
        {
            ArgumentNullException.ThrowIfNull(document);
            _document = document;
            _lineRanges = new LineRange[document.LineCount];

            Update(null);
        }

        public void RemoveLines(int startLine, int endLine)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(startLine);
            ArgumentOutOfRangeException.ThrowIfLessThan(endLine, startLine);
            lock (_lock)
            {
                ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(endLine, _lineCount);

                int removeCount = endLine - startLine + 1;
                int shiftCount = _lineCount - (endLine + 1);

                if (shiftCount > 0)
                {
                    Array.Copy(_lineRanges, endLine + 1, _lineRanges, startLine, shiftCount);
                }

                _lineCount -= removeCount;
                Array.Resize(ref _lineRanges, _lineCount);
            }
        }

        public string GetLineText(int lineIndex)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(lineIndex);
            lock (_lock)
            {
                ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(lineIndex, _lineCount);

                var lineRange = _lineRanges[lineIndex];
                return _textSource.GetText(lineRange.Offset, lineRange.Length);
            }
        }

        public string GetLineTextIncludingTerminator(int lineIndex)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(lineIndex);
            lock (_lock)
            {
                ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(lineIndex, _lineCount);

                var lineRange = _lineRanges[lineIndex];
                return _textSource.GetText(lineRange.Offset, lineRange.TotalLength);
            }
        }

        public ReadOnlyMemory<char> GetLineTextIncludingTerminatorAsMemory(int lineIndex)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(lineIndex);
            lock (_lock)
            {
                ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(lineIndex, _lineCount);

                var lineRange = _lineRanges[lineIndex];
                return _textSource.GetTextAsMemory(lineRange.Offset, lineRange.TotalLength);
            }
        }

        public string GetLineTerminator(int lineIndex)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(lineIndex);
            lock (_lock)
            {
                ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(lineIndex, _lineCount);

                var lineRange = _lineRanges[lineIndex];
                return _textSource.GetText(lineRange.Offset + lineRange.Length, lineRange.TotalLength - lineRange.Length);
            }
        }

        public int GetLineLength(int lineIndex)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(lineIndex);
            lock (_lock)
            {
                ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(lineIndex, _lineCount);

                return _lineRanges[lineIndex].Length;
            }
        }

        public int GetTotalLineLength(int lineIndex)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(lineIndex);
            lock (_lock)
            {
                ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(lineIndex, _lineCount);

                return _lineRanges[lineIndex].TotalLength;
            }
        }

        public string GetText()
        {
            lock (_lock)
            {
                return _textSource.Text;
            }
        }

        public void Update(DocumentChangeEventArgs e)
        {
            lock (_lock)
            {
                _lineCount = _document.Lines.Count;

                if (e?.OffsetChangeMap != null && _lineRanges != null && _lineCount == _lineRanges.Length)
                {
                    // it's a single-line change
                    // update the offsets usign the OffsetChangeMap
                    RecalculateOffsets(e);
                }
                else
                {
                    // recompute all the line ranges
                    // based in the document lines
                    RecomputeAllLineRanges(e);
                }

                _textSource = _document.CreateSnapshot();
            }
        }

        private void RecalculateOffsets(DocumentChangeEventArgs e)
        {
            var changedLine = _document.GetLineByOffset(e.Offset);
            int lineIndex = changedLine.LineNumber - 1;

            _lineRanges[lineIndex].Offset = changedLine.Offset;
            _lineRanges[lineIndex].Length = changedLine.Length;
            _lineRanges[lineIndex].TotalLength = changedLine.TotalLength;

            for (int i = lineIndex + 1; i < _lineCount; i++)
            {
                _lineRanges[i].Offset = e.OffsetChangeMap.GetNewOffset(_lineRanges[i].Offset);
            }
        }

        private void RecomputeAllLineRanges(DocumentChangeEventArgs e)
        {
            if (_lineRanges.Length != _lineCount)
            {
                Array.Resize(ref _lineRanges, _lineCount);
            }

            int currentLineIndex = (e != null) ?
                _document.GetLineByOffset(e.Offset).LineNumber - 1 : 0;
            var currentLine = _document.GetLineByNumber(currentLineIndex + 1);

            while (currentLine != null)
            {
                _lineRanges[currentLineIndex].Offset = currentLine.Offset;
                _lineRanges[currentLineIndex].Length = currentLine.Length;
                _lineRanges[currentLineIndex].TotalLength = currentLine.TotalLength;
                currentLine = currentLine.NextLine;
                currentLineIndex++;
            }
        }

        struct LineRange
        {
            public int Offset;
            public int Length;
            public int TotalLength;
        }
    }
}