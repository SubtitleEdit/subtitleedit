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
using System.Globalization;
using AvaloniaEdit.Document;
using AvaloniaEdit.Utils;

namespace AvaloniaEdit.Editing
{
    /// <summary>
    /// A simple selection.
    /// </summary>
    public sealed class SimpleSelection : Selection
    {
        private readonly TextViewPosition _start;
        private readonly TextViewPosition _end;
        private readonly int _startOffset;
        private readonly int _endOffset;

        /// <summary>
        /// Creates a new SimpleSelection instance.
        /// </summary>
        internal SimpleSelection(TextArea textArea, TextViewPosition start, TextViewPosition end)
            : base(textArea)
        {
            _start = start;
            _end = end;
            _startOffset = textArea.Document.GetOffset(start.Location);
            _endOffset = textArea.Document.GetOffset(end.Location);
        }

        /// <inheritdoc/>
        public override IEnumerable<SelectionSegment> Segments => ExtensionMethods.Sequence(new SelectionSegment(_startOffset, _start.VisualColumn, _endOffset, _end.VisualColumn));

        /// <inheritdoc/>
        public override ISegment SurroundingSegment => new SelectionSegment(_startOffset, _endOffset);

        /// <inheritdoc/>
        public override void ReplaceSelectionWithText(string newText)
        {
            if (newText == null)
                throw new ArgumentNullException(nameof(newText));
            using (TextArea.Document.RunUpdate())
            {
                var segmentsToDelete = TextArea.GetDeletableSegments(SurroundingSegment);
                for (var i = segmentsToDelete.Length - 1; i >= 0; i--)
                {
                    if (i == segmentsToDelete.Length - 1)
                    {
                        if (segmentsToDelete[i].Offset == SurroundingSegment.Offset && segmentsToDelete[i].Length == SurroundingSegment.Length)
                        {
                            newText = AddSpacesIfRequired(newText, _start, _end);
                        }
                        if (string.IsNullOrEmpty(newText))
                        {
                            // place caret at the beginning of the selection
                            // ReSharper disable once ImpureMethodCallOnReadonlyValueField
                            TextArea.Caret.Position = _start.CompareTo(_end) <= 0 ? _start : _end;
                        }
                        else
                        {
                            // place caret so that it ends up behind the new text
                            TextArea.Caret.Offset = segmentsToDelete[i].EndOffset;
                        }
                        TextArea.Document.Replace(segmentsToDelete[i], newText);
                    }
                    else
                    {
                        TextArea.Document.Remove(segmentsToDelete[i]);
                    }
                }
                if (segmentsToDelete.Length != 0)
                {
                    TextArea.ClearSelection();
                }
            }
        }

        public override TextViewPosition StartPosition => _start;

        public override TextViewPosition EndPosition => _end;

        /// <inheritdoc/>
        public override Selection UpdateOnDocumentChange(DocumentChangeEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));
            int newStartOffset, newEndOffset;
            if (_startOffset <= _endOffset)
            {
                newStartOffset = e.GetNewOffset(_startOffset);
                newEndOffset = Math.Max(newStartOffset, e.GetNewOffset(_endOffset, AnchorMovementType.BeforeInsertion));
            }
            else
            {
                newEndOffset = e.GetNewOffset(_endOffset);
                newStartOffset = Math.Max(newEndOffset, e.GetNewOffset(_startOffset, AnchorMovementType.BeforeInsertion));
            }
            return Create(
                TextArea,
                new TextViewPosition(TextArea.Document.GetLocation(newStartOffset), _start.VisualColumn),
                new TextViewPosition(TextArea.Document.GetLocation(newEndOffset), _end.VisualColumn)
            );
        }

        /// <inheritdoc/>
        public override bool IsEmpty => _startOffset == _endOffset && _start.VisualColumn == _end.VisualColumn;

        /// <inheritdoc/>
        public override int Length => Math.Abs(_endOffset - _startOffset);

        /// <inheritdoc/>
        public override Selection SetEndpoint(TextViewPosition endPosition)
        {
            return Create(TextArea, _start, endPosition);
        }

        public override Selection StartSelectionOrSetEndpoint(TextViewPosition startPosition, TextViewPosition endPosition)
        {
            var document = TextArea.Document;
            if (document == null)
                throw ThrowUtil.NoDocumentAssigned();
            return Create(TextArea, _start, endPosition);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                return _startOffset * 27811 + _endOffset + TextArea.GetHashCode();
            }
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            var other = obj as SimpleSelection;
            if (other == null) return false;
            // ReSharper disable ImpureMethodCallOnReadonlyValueField
            return _start.Equals(other._start) && _end.Equals(other._end)
                && _startOffset == other._startOffset && _endOffset == other._endOffset
                && TextArea == other.TextArea;
            // ReSharper restore ImpureMethodCallOnReadonlyValueField
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Create(CultureInfo.InvariantCulture, $"[{nameof(SimpleSelection)} Start={_start}, End={_end}]");
        }
    }
}
