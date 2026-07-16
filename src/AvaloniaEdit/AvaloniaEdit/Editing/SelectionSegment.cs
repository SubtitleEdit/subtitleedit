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
using System.Globalization;
using AvaloniaEdit.Document;

namespace AvaloniaEdit.Editing
{
    /// <summary>
    /// Represents a selected segment.
    /// </summary>
    public class SelectionSegment : ISegment
    {
        /// <summary>
        /// Creates a SelectionSegment from two offsets.
        /// </summary>
        public SelectionSegment(int startOffset, int endOffset)
        {
            StartOffset = Math.Min(startOffset, endOffset);
            EndOffset = Math.Max(startOffset, endOffset);
            StartVisualColumn = EndVisualColumn = -1;
        }

        /// <summary>
        /// Creates a SelectionSegment from two offsets and visual columns.
        /// </summary>
        public SelectionSegment(int startOffset, int startVisualColumn, int endOffset, int endVisualColumn)
        {
            if (startOffset < endOffset || (startOffset == endOffset && startVisualColumn <= endVisualColumn))
            {
                StartOffset = startOffset;
                StartVisualColumn = startVisualColumn;
                EndOffset = endOffset;
                EndVisualColumn = endVisualColumn;
            }
            else
            {
                StartOffset = endOffset;
                StartVisualColumn = endVisualColumn;
                EndOffset = startOffset;
                EndVisualColumn = startVisualColumn;
            }
        }

        /// <summary>
        /// Gets the start offset.
        /// </summary>
        public int StartOffset { get; }

        /// <summary>
        /// Gets the end offset.
        /// </summary>
        public int EndOffset { get; }

        /// <summary>
        /// Gets the start visual column.
        /// </summary>
        public int StartVisualColumn { get; }

        /// <summary>
        /// Gets the end visual column.
        /// </summary>
        public int EndVisualColumn { get; }

        /// <inheritdoc/>
        int ISegment.Offset => StartOffset;

        /// <inheritdoc/>
        public int Length => EndOffset - StartOffset;

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Create(CultureInfo.InvariantCulture,
                $"[{nameof(SelectionSegment)} {nameof(StartOffset)}={StartOffset}, {nameof(EndOffset)}={EndOffset}, StartVC={StartVisualColumn}, EndVC={EndVisualColumn}]");
        }
    }
}
