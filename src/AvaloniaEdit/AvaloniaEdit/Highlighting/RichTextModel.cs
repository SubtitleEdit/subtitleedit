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
using Avalonia.Media;
using AvaloniaEdit.Document;

namespace AvaloniaEdit.Highlighting
{
    /// <summary>
    /// Stores rich-text formatting.
    /// </summary>
    public sealed class RichTextModel
    {
        private readonly List<int> _stateChangeOffsets = new List<int>();
        private readonly List<HighlightingColor> _stateChanges = new List<HighlightingColor>();

        private int GetIndexForOffset(int offset)
        {
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));
            var index = _stateChangeOffsets.BinarySearch(offset);
            if (index < 0)
            {
                // If no color change exists directly at offset,
                // create a new one.
                index = ~index;
                _stateChanges.Insert(index, _stateChanges[index - 1].Clone());
                _stateChangeOffsets.Insert(index, offset);
            }
            return index;
        }

        private int GetIndexForOffsetUseExistingSegment(int offset)
        {
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));
            var index = _stateChangeOffsets.BinarySearch(offset);
            if (index < 0)
            {
                // If no color change exists directly at offset,
                // return the index of the color segment that contains offset.
                index = ~index - 1;
            }
            return index;
        }

        private int GetEnd(int index)
        {
            // Gets the end of the color segment no. index.
            if (index + 1 < _stateChangeOffsets.Count)
                return _stateChangeOffsets[index + 1];
            return int.MaxValue;
        }

        /// <summary>
        /// Creates a new RichTextModel.
        /// </summary>
        public RichTextModel()
        {
            _stateChangeOffsets.Add(0);
            _stateChanges.Add(new HighlightingColor());
        }

        /// <summary>
        /// Creates a RichTextModel from a CONTIGUOUS list of HighlightedSections.
        /// </summary>
        internal RichTextModel(int[] stateChangeOffsets, HighlightingColor[] stateChanges)
        {
            Debug.Assert(stateChangeOffsets[0] == 0);
            _stateChangeOffsets.AddRange(stateChangeOffsets);
            _stateChanges.AddRange(stateChanges);
        }

        #region UpdateOffsets
        /// <summary>
        /// Updates the start and end offsets of all segments stored in this collection.
        /// </summary>
        /// <param name="e">TextChangeEventArgs instance describing the change to the document.</param>
        public void UpdateOffsets(TextChangeEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));
            UpdateOffsets(e.GetNewOffset);
        }

        /// <summary>
        /// Updates the start and end offsets of all segments stored in this collection.
        /// </summary>
        /// <param name="change">OffsetChangeMap instance describing the change to the document.</param>
        public void UpdateOffsets(OffsetChangeMap change)
        {
            if (change == null)
                throw new ArgumentNullException(nameof(change));
            UpdateOffsets(change.GetNewOffset);
        }

        /// <summary>
        /// Updates the start and end offsets of all segments stored in this collection.
        /// </summary>
        /// <param name="change">OffsetChangeMapEntry instance describing the change to the document.</param>
        public void UpdateOffsets(OffsetChangeMapEntry change)
        {
            UpdateOffsets(change.GetNewOffset);
        }

        private void UpdateOffsets(Func<int, AnchorMovementType, int> updateOffset)
        {
            var readPos = 1;
            var writePos = 1;
            while (readPos < _stateChangeOffsets.Count)
            {
                Debug.Assert(writePos <= readPos);
                var newOffset = updateOffset(_stateChangeOffsets[readPos], AnchorMovementType.Default);
                if (newOffset == _stateChangeOffsets[writePos - 1])
                {
                    // offset moved to same position as previous offset
                    // -> previous segment has length 0 and gets overwritten with this segment
                    _stateChanges[writePos - 1] = _stateChanges[readPos];
                }
                else
                {
                    _stateChangeOffsets[writePos] = newOffset;
                    _stateChanges[writePos] = _stateChanges[readPos];
                    writePos++;
                }
                readPos++;
            }
            // Delete all entries that were not written to
            _stateChangeOffsets.RemoveRange(writePos, _stateChangeOffsets.Count - writePos);
            _stateChanges.RemoveRange(writePos, _stateChanges.Count - writePos);
        }
        #endregion

        /// <summary>
        /// Appends another RichTextModel after this one.
        /// </summary>
        internal void Append(int offset, int[] newOffsets, HighlightingColor[] newColors)
        {
            Debug.Assert(newOffsets.Length == newColors.Length);
            Debug.Assert(newOffsets[0] == 0);
            // remove everything not before offset:
            while (_stateChangeOffsets.Count > 0 && _stateChangeOffsets[^1] >= offset)
            {
                _stateChangeOffsets.RemoveAt(_stateChangeOffsets.Count - 1);
                _stateChanges.RemoveAt(_stateChanges.Count - 1);
            }
            // Append the new segments
            for (var i = 0; i < newOffsets.Length; i++)
            {
                _stateChangeOffsets.Add(offset + newOffsets[i]);
                _stateChanges.Add(newColors[i]);
            }
        }

        /// <summary>
        /// Gets a copy of the HighlightingColor for the specified offset.
        /// </summary>
        public HighlightingColor GetHighlightingAt(int offset)
        {
            return _stateChanges[GetIndexForOffsetUseExistingSegment(offset)].Clone();
        }

        /// <summary>
        /// Applies the HighlightingColor to the specified range of text.
        /// If the color specifies <c>null</c> for some properties, existing highlighting is preserved.
        /// </summary>
        public void ApplyHighlighting(int offset, int length, HighlightingColor color)
        {
            if (color == null || color.IsEmptyForMerge)
            {
                // Optimization: don't split the HighlightingState when we're not changing
                // any property. For example, the "Punctuation" color in C# is
                // empty by default.
                return;
            }
            var startIndex = GetIndexForOffset(offset);
            var endIndex = GetIndexForOffset(offset + length);
            for (var i = startIndex; i < endIndex; i++)
            {
                _stateChanges[i].MergeWith(color);
            }
        }

        /// <summary>
        /// Sets the HighlightingColor for the specified range of text,
        /// completely replacing the existing highlighting in that area.
        /// </summary>
        public void SetHighlighting(int offset, int length, HighlightingColor color)
        {
            if (length <= 0)
                return;
            var startIndex = GetIndexForOffset(offset);
            var endIndex = GetIndexForOffset(offset + length);
            _stateChanges[startIndex] = color != null ? color.Clone() : new HighlightingColor();
            _stateChanges.RemoveRange(startIndex + 1, endIndex - (startIndex + 1));
            _stateChangeOffsets.RemoveRange(startIndex + 1, endIndex - (startIndex + 1));
        }

        /// <summary>
        /// Sets the foreground brush on the specified text segment.
        /// </summary>
        public void SetForeground(int offset, int length, HighlightingBrush brush)
        {
            var startIndex = GetIndexForOffset(offset);
            var endIndex = GetIndexForOffset(offset + length);
            for (var i = startIndex; i < endIndex; i++)
            {
                _stateChanges[i].Foreground = brush;
            }
        }

        /// <summary>
        /// Sets the background brush on the specified text segment.
        /// </summary>
        public void SetBackground(int offset, int length, HighlightingBrush brush)
        {
            var startIndex = GetIndexForOffset(offset);
            var endIndex = GetIndexForOffset(offset + length);
            for (var i = startIndex; i < endIndex; i++)
            {
                _stateChanges[i].Background = brush;
            }
        }

        /// <summary>
        /// Sets the font weight on the specified text segment.
        /// </summary>
        public void SetFontWeight(int offset, int length, FontWeight weight)
        {
            var startIndex = GetIndexForOffset(offset);
            var endIndex = GetIndexForOffset(offset + length);
            for (var i = startIndex; i < endIndex; i++)
            {
                _stateChanges[i].FontWeight = weight;
            }
        }

        /// <summary>
        /// Sets the font style on the specified text segment.
        /// </summary>
        public void SetFontStyle(int offset, int length, FontStyle style)
        {
            var startIndex = GetIndexForOffset(offset);
            var endIndex = GetIndexForOffset(offset + length);
            for (var i = startIndex; i < endIndex; i++)
            {
                _stateChanges[i].FontStyle = style;
            }
        }

        /// <summary>
        /// Retrieves the highlighted sections in the specified range.
        /// The highlighted sections will be sorted by offset, and there will not be any nested or overlapping sections.
        /// </summary>
        public IEnumerable<HighlightedSection> GetHighlightedSections(int offset, int length)
        {
            var index = GetIndexForOffsetUseExistingSegment(offset);
            var pos = offset;
            var endOffset = offset + length;
            while (pos < endOffset)
            {
                var endPos = Math.Min(endOffset, GetEnd(index));
                yield return new HighlightedSection
                {
                    Offset = pos,
                    Length = endPos - pos,
                    Color = _stateChanges[index].Clone()
                };
                pos = endPos;
                index++;
            }
        }

        ///// <summary>
        ///// Creates Run instances that can be used for TextBlock.Inlines.
        ///// </summary>
        ///// <param name="textSource">The text source that holds the text for this RichTextModel.</param>
        //public Run[] CreateRuns(ITextSource textSource)
        //{
        //	Run[] runs = new Run[stateChanges.Count];
        //	for (int i = 0; i < runs.Length; i++) {
        //		int startOffset = stateChangeOffsets[i];
        //		int endOffset = i + 1 < stateChangeOffsets.Count ? stateChangeOffsets[i + 1] : textSource.TextLength;
        //		Run r = new Run(textSource.GetText(startOffset, endOffset - startOffset));
        //		HighlightingColor state = stateChanges[i];
        //		RichText.ApplyColorToTextElement(r, state);
        //		runs[i] = r;
        //	}
        //	return runs;
        //}
    }
}
