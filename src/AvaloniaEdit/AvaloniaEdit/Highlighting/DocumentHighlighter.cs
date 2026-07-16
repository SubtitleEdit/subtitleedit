// Copyright (c) 2014 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AvaloniaEdit.Document;
using AvaloniaEdit.Utils;
using Avalonia.Threading;
using SpanStack = System.Collections.Immutable.ImmutableStack<AvaloniaEdit.Highlighting.HighlightingSpan>;

namespace AvaloniaEdit.Highlighting
{
    /// <summary>
    /// This class can syntax-highlight a document.
    /// It automatically manages invalidating the highlighting when the document changes.
    /// </summary>
    public class DocumentHighlighter : ILineTracker, IHighlighter
    {
        /// <summary>
        /// Stores the span state at the end of each line.
        /// storedSpanStacks[0] = state at beginning of document
        /// storedSpanStacks[i] = state after line i
        /// </summary>
        private readonly CompressingTreeList<SpanStack> _storedSpanStacks = new CompressingTreeList<SpanStack>(ReferenceEquals);

        private readonly CompressingTreeList<bool> _isValid = new CompressingTreeList<bool>((a, b) => a == b);
        private readonly IHighlightingDefinition _definition;
        private readonly HighlightingEngine _engine;
        private readonly WeakLineTracker _weakLineTracker;
        private bool _isHighlighting;
        private bool _isInHighlightingGroup;
        private bool _isDisposed;

        /// <summary>
        /// Gets the document that this DocumentHighlighter is highlighting.
        /// </summary>
        public IDocument Document { get; }

        /// <summary>
        /// Creates a new DocumentHighlighter instance.
        /// </summary>
        public DocumentHighlighter(TextDocument document, IHighlightingDefinition definition)
        {
            Document = document ?? throw new ArgumentNullException(nameof(document));
            _definition = definition ?? throw new ArgumentNullException(nameof(definition));
            _engine = new HighlightingEngine(definition.MainRuleSet);
            Dispatcher.UIThread.VerifyAccess();
            _weakLineTracker = WeakLineTracker.Register(document, this);
            InvalidateSpanStacks();
        }

        /// <summary>
        /// Disposes the document highlighter.
        /// </summary>
        public void Dispose()
        {
            _weakLineTracker?.Deregister();
            _isDisposed = true;
        }

        void ILineTracker.BeforeRemoveLine(DocumentLine line)
        {
            CheckIsHighlighting();
            var number = line.LineNumber;
            _storedSpanStacks.RemoveAt(number);
            _isValid.RemoveAt(number);
            if (number < _isValid.Count)
            {
                _isValid[number] = false;
                if (number < _firstInvalidLine)
                    _firstInvalidLine = number;
            }
        }

        void ILineTracker.SetLineLength(DocumentLine line, int newTotalLength)
        {
            CheckIsHighlighting();
            var number = line.LineNumber;
            _isValid[number] = false;
            if (number < _firstInvalidLine)
                _firstInvalidLine = number;
        }

        void ILineTracker.LineInserted(DocumentLine insertionPos, DocumentLine newLine)
        {
            CheckIsHighlighting();
            Debug.Assert(insertionPos.LineNumber + 1 == newLine.LineNumber);
            var lineNumber = newLine.LineNumber;
            _storedSpanStacks.Insert(lineNumber, null);
            _isValid.Insert(lineNumber, false);
            if (lineNumber < _firstInvalidLine)
                _firstInvalidLine = lineNumber;
        }

        void ILineTracker.RebuildDocument()
        {
            InvalidateSpanStacks();
        }

        void ILineTracker.ChangeComplete(DocumentChangeEventArgs e)
        {
        }

        private SpanStack _initialSpanStack = SpanStack.Empty;

        /// <summary>
        /// Gets/sets the the initial span stack of the document. Default value is <see cref="SpanStack.Empty" />.
        /// </summary>
        public SpanStack InitialSpanStack
        {
            get => _initialSpanStack;
            set
            {
                _initialSpanStack = value ?? SpanStack.Empty;
                InvalidateHighlighting();
            }
        }

        /// <summary>
        /// Invalidates all stored highlighting info.
        /// When the document changes, the highlighting is invalidated automatically, this method
        /// needs to be called only when there are changes to the highlighting rule set.
        /// </summary>
        public void InvalidateHighlighting()
        {
            InvalidateSpanStacks();
            OnHighlightStateChanged(1, Document.LineCount); // force a redraw with the new highlighting
        }

        /// <summary>
        /// Invalidates stored highlighting info, but does not raise the HighlightingStateChanged event.
        /// </summary>
        private void InvalidateSpanStacks()
        {
            CheckIsHighlighting();
            _storedSpanStacks.Clear();
            _storedSpanStacks.Add(_initialSpanStack);
            _storedSpanStacks.InsertRange(1, Document.LineCount, null);
            _isValid.Clear();
            _isValid.Add(true);
            _isValid.InsertRange(1, Document.LineCount, false);
            _firstInvalidLine = 1;
        }

        private int _firstInvalidLine;

        /// <inheritdoc/>
        public HighlightedLine HighlightLine(int lineNumber)
        {
            ThrowUtil.CheckInRangeInclusive(lineNumber, "lineNumber", 1, Document.LineCount);
            CheckIsHighlighting();
            _isHighlighting = true;
            try
            {
                HighlightUpTo(lineNumber - 1);
                var line = Document.GetLineByNumber(lineNumber);
                var result = _engine.HighlightLine(Document, line);
                UpdateTreeList(lineNumber);
                return result;
            }
            finally
            {
                _isHighlighting = false;
            }
        }

        /// <summary>
        /// Gets the span stack at the end of the specified line.
        /// -> GetSpanStack(1) returns the spans at the start of the second line.
        /// </summary>
        /// <remarks>
        /// GetSpanStack(0) is valid and will return <see cref="InitialSpanStack"/>.
        /// The elements are returned in inside-out order (first element of result enumerable is the color of the innermost span).
        /// </remarks>
        public SpanStack GetSpanStack(int lineNumber)
        {
            ThrowUtil.CheckInRangeInclusive(lineNumber, "lineNumber", 0, Document.LineCount);
            if (_firstInvalidLine <= lineNumber)
            {
                UpdateHighlightingState(lineNumber);
            }
            return _storedSpanStacks[lineNumber];
        }

        /// <inheritdoc/>
        public IEnumerable<HighlightingColor> GetColorStack(int lineNumber)
        {
            return GetSpanStack(lineNumber).Select(s => s.SpanColor).Where(s => s != null);
        }

        private void CheckIsHighlighting()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException("DocumentHighlighter");
            }
            if (_isHighlighting)
            {
                throw new InvalidOperationException("Invalid call - a highlighting operation is currently running.");
            }
        }

        /// <inheritdoc/>
        public void UpdateHighlightingState(int lineNumber)
        {
            CheckIsHighlighting();
            _isHighlighting = true;
            try
            {
                HighlightUpTo(lineNumber);
            }
            finally
            {
                _isHighlighting = false;
            }
        }

        /// <summary>
        /// Sets the engine's CurrentSpanStack to the end of the target line.
        /// Updates the span stack for all lines up to (and including) the target line, if necessary.
        /// </summary>
        private void HighlightUpTo(int targetLineNumber)
        {
            for (var currentLine = 0; currentLine <= targetLineNumber; currentLine++)
            {
                if (_firstInvalidLine > currentLine)
                {
                    // (this branch is always taken on the first loop iteration, as firstInvalidLine > 0)

                    if (_firstInvalidLine <= targetLineNumber)
                    {
                        // Skip valid lines to next invalid line:
                        _engine.CurrentSpanStack = _storedSpanStacks[_firstInvalidLine - 1];
                        currentLine = _firstInvalidLine;
                    }
                    else
                    {
                        // Skip valid lines to target line:
                        _engine.CurrentSpanStack = _storedSpanStacks[targetLineNumber];
                        break;
                    }
                }
                Debug.Assert(EqualSpanStacks(_engine.CurrentSpanStack, _storedSpanStacks[currentLine - 1]));
                _engine.ScanLine(Document, Document.GetLineByNumber(currentLine));
                UpdateTreeList(currentLine);
            }
            Debug.Assert(EqualSpanStacks(_engine.CurrentSpanStack, _storedSpanStacks[targetLineNumber]));
        }

        private void UpdateTreeList(int lineNumber)
        {
            if (!EqualSpanStacks(_engine.CurrentSpanStack, _storedSpanStacks[lineNumber]))
            {
                _isValid[lineNumber] = true;
                //Debug.WriteLine("Span stack in line " + lineNumber + " changed from " + storedSpanStacks[lineNumber] + " to " + spanStack);
                _storedSpanStacks[lineNumber] = _engine.CurrentSpanStack;
                if (lineNumber + 1 < _isValid.Count)
                {
                    _isValid[lineNumber + 1] = false;
                    _firstInvalidLine = lineNumber + 1;
                }
                else
                {
                    _firstInvalidLine = int.MaxValue;
                }
                if (lineNumber + 1 < Document.LineCount)
                    OnHighlightStateChanged(lineNumber + 1, lineNumber + 1);
            }
            else if (_firstInvalidLine == lineNumber)
            {
                _isValid[lineNumber] = true;
                _firstInvalidLine = _isValid.IndexOf(false);
                if (_firstInvalidLine < 0)
                    _firstInvalidLine = int.MaxValue;
            }
        }

        private static bool EqualSpanStacks(SpanStack a, SpanStack b)
        {
            // We must use value equality between the stacks because HighlightingColorizer.OnHighlightStateChanged
            // depends on the fact that equal input state + unchanged line contents produce equal output state.
            if (a == b)
                return true;
            if (a == null || b == null)
                return false;
            while (!a.IsEmpty && !b.IsEmpty)
            {
                if (a.Peek() != b.Peek())
                    return false;
                a = a.Pop();
                b = b.Pop();
                if (a == b)
                    return true;
            }
            return a.IsEmpty && b.IsEmpty;
        }

        /// <inheritdoc/>
        public event HighlightingStateChangedEventHandler HighlightingStateChanged;

        /// <summary>
        /// Is called when the highlighting state at the end of the specified line has changed.
        /// </summary>
        /// <remarks>This callback must not call HighlightLine or InvalidateHighlighting.
        /// It may call GetSpanStack, but only for the changed line and lines above.
        /// This method must not modify the document.</remarks>
        protected virtual void OnHighlightStateChanged(int fromLineNumber, int toLineNumber)
        {
            HighlightingStateChanged?.Invoke(fromLineNumber, toLineNumber);
        }

        /// <inheritdoc/>
        public HighlightingColor DefaultTextColor => null;

        /// <inheritdoc/>
        public void BeginHighlighting()
        {
            if (_isInHighlightingGroup)
                throw new InvalidOperationException("Highlighting group is already open");
            _isInHighlightingGroup = true;
        }

        /// <inheritdoc/>
        public void EndHighlighting()
        {
            if (!_isInHighlightingGroup)
                throw new InvalidOperationException("Highlighting group is not open");
            _isInHighlightingGroup = false;
        }

        /// <inheritdoc/>
        public HighlightingColor GetNamedColor(string name)
        {
            return _definition.GetNamedColor(name);
        }
    }
}
