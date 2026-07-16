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
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Utils;

namespace AvaloniaEdit.Snippets
{
    /// <summary>
    /// Represents the context of a snippet insertion.
    /// </summary>
    public class InsertionContext
    {
        private enum Status
        {
            Insertion,
            RaisingInsertionCompleted,
            Interactive,
            RaisingDeactivated,
            Deactivated
        }

        /// <summary>
        /// Pre-compiled, SIMD-accelerated search values for newline characters
        /// used during snippet text insertion.
        /// </summary>
        private static readonly SearchValues<char> _newlineChars = SearchValues.Create("\r\n");

        private Status _currentStatus = Status.Insertion;

        /// <summary>
        /// Creates a new InsertionContext instance.
        /// </summary>
        public InsertionContext(TextArea textArea, int insertionPosition)
        {
            TextArea = textArea ?? throw new ArgumentNullException(nameof(textArea));
            Document = textArea.Document;
            SelectedText = textArea.Selection.GetText();
            InsertionPosition = insertionPosition;
            _startPosition = insertionPosition;

            DocumentLine startLine = Document.GetLineByOffset(insertionPosition);
            ISegment indentation = TextUtilities.GetWhitespaceAfter(Document, startLine.Offset);
            Indentation = Document.GetText(indentation.Offset, Math.Min(indentation.EndOffset, insertionPosition) - indentation.Offset);
            Tab = textArea.Options.IndentationString;

            LineTerminator = TextUtilities.GetNewLineFromDocument(Document, startLine.LineNumber);
        }

        /// <summary>
        /// Gets the text area.
        /// </summary>
        public TextArea TextArea { get; }

        /// <summary>
        /// Gets the text document.
        /// </summary>
        public TextDocument Document { get; }

        /// <summary>
        /// Gets the text that was selected before the insertion of the snippet.
        /// </summary>
        public string SelectedText { get; }

        /// <summary>
        /// Gets the indentation at the insertion position.
        /// </summary>
        public string Indentation { get; }

        /// <summary>
        /// Gets the indentation string for a single indentation level.
        /// </summary>
        public string Tab { get; }

        /// <summary>
        /// Gets the line terminator at the insertion position.
        /// </summary>
        public string LineTerminator { get; }

        /// <summary>
        /// Gets/Sets the insertion position.
        /// </summary>
        public int InsertionPosition { get; set; }

        private readonly int _startPosition;
        private AnchorSegment _wholeSnippetAnchor;
        private bool _deactivateIfSnippetEmpty;

        /// <summary>
        /// Gets the start position of the snippet insertion.
        /// </summary>
        public int StartPosition
        {
            get
            {
                if (_wholeSnippetAnchor != null)
                    return _wholeSnippetAnchor.Offset;
                return _startPosition;
            }
        }

        /// <summary>
        /// Inserts text at the insertion position and advances the insertion position.
        /// This method will add the current indentation to every line in <paramref name="text"/> and will
        /// replace newlines with the expected newline for the document.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method uses a two-phase algorithm that is semantically equivalent to the
        /// original implementation:
        /// </para>
        /// <list type="number">
        /// <item>
        /// <description>
        /// <b>Phase 1 - Tab expansion:</b> Replaces tab characters with the configured
        /// indentation string using <see cref="string.Replace(string, string)"/>, which
        /// leverages the runtime's native SIMD-optimized implementation. When
        /// <see cref="Tab"/> equals <c>"\t"</c> (identity replacement), this phase is
        /// skipped entirely.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// <b>Phase 2 - Newline normalization:</b> Scans the fully tab-expanded text for
        /// newline characters using <see cref="SearchValues{T}"/> (SIMD-accelerated) and
        /// replaces them with <see cref="LineTerminator"/> + <see cref="Indentation"/>. Uses
        /// a stack-allocated <see cref="ValueStringBuilder"/> that falls back to
        /// <see cref="ArrayPool{T}"/> for large inputs, minimizing heap allocations.
        /// </description>
        /// </item>
        /// </list>
        /// <para>
        /// The two-phase approach ensures that if <see cref="Tab"/> contains newline characters
        /// (possible via a custom <see cref="TextEditorOptions"/> subclass), those newlines are
        /// correctly processed by the newline normalization phase - including CRLF pairs that
        /// span the boundary between expanded tab content and adjacent source text.
        /// </para>
        /// <para>
        /// The entire transformed result is inserted into the document with a single
        /// <see cref="TextDocument.Insert(int, string)"/> call within a
        /// <see cref="TextDocument.RunUpdate"/> scope, eliminating per-line substring
        /// allocations and reducing document change, anchor update, and undo-grouping overhead.
        /// </para>
        /// <para>
        /// A fast path is provided for text that contains no special characters, avoiding
        /// all intermediate allocations entirely.
        /// </para>
        /// </remarks>
        public void InsertText(string text)
        {
            if (_currentStatus != Status.Insertion)
                throw new InvalidOperationException();

            text = text ?? throw new ArgumentNullException(nameof(text));

            // Fast path: if Tab is identity ("\t") and text has no newlines,
            // or if Tab is not identity and text has no tabs or newlines,
            // insert directly with zero extra allocations
            bool isTabIdentity = Tab == "\t";

            ReadOnlySpan<char> span = text.AsSpan();
            bool hasNewlines = span.IndexOfAny(_newlineChars) >= 0;
            bool hasTabs = !isTabIdentity && span.Contains('\t');

            if (!hasNewlines && !hasTabs)
            {
                using (Document.RunUpdate())
                {
                    Document.Insert(InsertionPosition, text);
                    InsertionPosition += text.Length;
                }
                return;
            }

            // Phase 1: Tab expansion
            // Equivalent to the original's: text = text.Replace("\t", Tab)
            //
            // Uses the runtime's native string.Replace which is SIMD-optimized
            // and significantly faster than a managed StringBuilder loop for
            // this operation. When Tab == "\t" (identity), this is skipped.
            //
            // This must happen before newline normalization so that any newline
            // characters within Tab are visible to Phase 2. This is what makes
            // CRLF boundary merging work correctly - Phase 2 sees the fully
            // expanded text as one contiguous stream.
            string expanded = hasTabs ? text.Replace("\t", Tab) : text;
            ReadOnlySpan<char> expandedSpan = expanded.AsSpan();

            // Check if newlines exist in the expanded text (tab expansion may
            // have introduced newlines if Tab contains \r or \n)
            if (expandedSpan.IndexOfAny(_newlineChars) < 0)
            {
                // Tab expansion produced text with no newlines - insert directly
                using (Document.RunUpdate())
                {
                    Document.Insert(InsertionPosition, expanded);
                    InsertionPosition += expanded.Length;
                }
                return;
            }

            // Phase 2: Newline normalization
            // Equivalent to the original's NewLineFinder.NextNewLine loop
            //
            // Uses a ValueStringBuilder backed by stackalloc for small inputs
            // (typical snippets < 512 chars) with ArrayPool<char> fallback for
            // larger inputs. This eliminates the StringBuilder object allocation
            // and its internal char[] buffer allocation on the heap for the
            // common case.
            //
            // Because this operates on the fully tab-expanded text, CRLF pairs
            // that span tab-expansion boundaries are correctly handled as single
            // units - identical to the original implementation.
            string result;
            const int optimizedStackCharacterCount = 512;
            using (var vsb = new ValueStringBuilder(stackalloc char[optimizedStackCharacterCount]))
            {
                int pos = 0;
                while (pos < expandedSpan.Length)
                {
                    int index = expandedSpan[pos..].IndexOfAny(_newlineChars);
                    if (index < 0)
                    {
                        // No more newlines - append remaining text
                        vsb.Append(expandedSpan[pos..]);
                        break;
                    }

                    // Append literal text before the newline
                    if (index > 0)
                    {
                        vsb.Append(expandedSpan.Slice(pos, index));
                    }

                    pos += index;
                    char c = expandedSpan[pos];

                    // Handle \r\n as a single unit, or \r / \n individually
                    if (c == '\r' && pos + 1 < expandedSpan.Length && expandedSpan[pos + 1] == '\n')
                    {
                        pos += 2; // skip \r\n
                    }
                    else
                    {
                        pos++; // skip \r or \n
                    }

                    // Replace with document's line terminator + indentation
                    vsb.Append(LineTerminator);
                    vsb.Append(Indentation);
                }

                // Single document insert wrapped in RunUpdate() for update-scope
                // and undo-grouping parity with the original implementation
                result = vsb.ToString();
            }

            using (Document.RunUpdate())
            {
                Document.Insert(InsertionPosition, result);
                InsertionPosition += result.Length;
            }
        }

        private readonly Dictionary<SnippetElement, IActiveElement> _elementMap = new Dictionary<SnippetElement, IActiveElement>();
        private readonly List<IActiveElement> _registeredElements = new List<IActiveElement>();

        /// <summary>
        /// Registers an active element. Elements should be registered during insertion and will be called back
        /// when insertion has completed.
        /// </summary>
        /// <param name="owner">The snippet element that created the active element.</param>
        /// <param name="element">The active element.</param>
        public void RegisterActiveElement(SnippetElement owner, IActiveElement element)
        {
            if (owner == null)
                throw new ArgumentNullException(nameof(owner));
            if (element == null)
                throw new ArgumentNullException(nameof(element));
            if (_currentStatus != Status.Insertion)
                throw new InvalidOperationException();

            _elementMap.Add(owner, element);
            _registeredElements.Add(element);
        }

        /// <summary>
        /// Returns the active element belonging to the specified snippet element, or null if no such active element is found.
        /// </summary>
        public IActiveElement GetActiveElement(SnippetElement owner)
        {
            if (owner == null)
                throw new ArgumentNullException(nameof(owner));

            return _elementMap.GetValueOrDefault(owner);
        }

        /// <summary>
        /// Gets the list of active elements.
        /// </summary>
        public IEnumerable<IActiveElement> ActiveElements => _registeredElements;

        /// <summary>
        /// Calls the <see cref="IActiveElement.OnInsertionCompleted"/> method on all registered active elements
        /// and raises the <see cref="InsertionCompleted"/> event.
        /// </summary>
        /// <param name="e">The EventArgs to use</param>
        [SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate",
                                                         Justification = "There is an event and this method is raising it.")]
        public void RaiseInsertionCompleted(EventArgs e)
        {
            if (_currentStatus != Status.Insertion)
                throw new InvalidOperationException();

            e ??= EventArgs.Empty;

            _currentStatus = Status.RaisingInsertionCompleted;
            int endPosition = InsertionPosition;
            _wholeSnippetAnchor = new AnchorSegment(Document, _startPosition, endPosition - _startPosition);
            TextDocumentWeakEventManager.UpdateFinished.AddHandler(Document, OnUpdateFinished);
            _deactivateIfSnippetEmpty = (endPosition != _startPosition);

            foreach (IActiveElement element in _registeredElements)
            {
                element.OnInsertionCompleted();
            }
            InsertionCompleted?.Invoke(this, e);
            _currentStatus = Status.Interactive;
            if (_registeredElements.Count == 0)
            {
                // deactivate immediately if there are no interactive elements
                Deactivate(new SnippetEventArgs(DeactivateReason.NoActiveElements));
            }
            else
            {
                _myInputHandler = new SnippetInputHandler(this);
                // disable existing snippet input handlers - there can be only 1 active snippet
                foreach (TextAreaStackedInputHandler h in TextArea.StackedInputHandlers)
                {
                    if (h is SnippetInputHandler)
                        TextArea.PopStackedInputHandler(h);
                }
                TextArea.PushStackedInputHandler(_myInputHandler);
            }
        }

        private SnippetInputHandler _myInputHandler;

        /// <summary>
        /// Occurs when the all snippet elements have been inserted.
        /// </summary>
        public event EventHandler InsertionCompleted;

        /// <summary>
        /// Calls the <see cref="IActiveElement.Deactivate"/> method on all registered active elements.
        /// </summary>
        /// <param name="e">The EventArgs to use</param>
        public void Deactivate(SnippetEventArgs e)
        {
            if (_currentStatus == Status.Deactivated || _currentStatus == Status.RaisingDeactivated)
                return;
            if (_currentStatus != Status.Interactive)
                throw new InvalidOperationException("Cannot call Deactivate() until RaiseInsertionCompleted() has finished.");

            e ??= new SnippetEventArgs(DeactivateReason.Unknown);

            TextDocumentWeakEventManager.UpdateFinished.RemoveHandler(Document, OnUpdateFinished);
            _currentStatus = Status.RaisingDeactivated;
            TextArea.PopStackedInputHandler(_myInputHandler);
            foreach (IActiveElement element in _registeredElements)
            {
                element.Deactivate(e);
            }
            Deactivated?.Invoke(this, e);
            _currentStatus = Status.Deactivated;
        }

        /// <summary>
        /// Occurs when the interactive mode is deactivated.
        /// </summary>
        public event EventHandler<SnippetEventArgs> Deactivated;

        void OnUpdateFinished(object sender, EventArgs e)
        {
            // Deactivate if snippet is deleted. This is necessary for correctly leaving interactive
            // mode if Undo is pressed after a snippet insertion.
            if (_wholeSnippetAnchor.Length == 0 && _deactivateIfSnippetEmpty)
                Deactivate(new SnippetEventArgs(DeactivateReason.Deleted));
        }

        /// <summary>
        /// Adds existing segments as snippet elements.
        /// </summary>
        public void Link(ISegment mainElement, ISegment[] boundElements)
        {
            var main = new SnippetReplaceableTextElement { Text = Document.GetText(mainElement) };
            RegisterActiveElement(main, new ReplaceableActiveElement(this, mainElement.Offset, mainElement.EndOffset));
            foreach (var boundElement in boundElements)
            {
                var bound = new SnippetBoundElement { TargetElement = main };
                var start = Document.CreateAnchor(boundElement.Offset);
                start.MovementType = AnchorMovementType.BeforeInsertion;
                start.SurviveDeletion = true;
                var end = Document.CreateAnchor(boundElement.EndOffset);
                end.MovementType = AnchorMovementType.BeforeInsertion;
                end.SurviveDeletion = true;

                RegisterActiveElement(bound, new BoundActiveElement(this, main, bound, new AnchorSegment(start, end)));
            }
        }
    }
}
