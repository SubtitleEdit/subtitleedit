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

using System.Diagnostics;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using AvaloniaEdit.Utils;

namespace AvaloniaEdit.Folding
{
    /// <summary>
    /// A section that can be folded.
    /// </summary>
    public sealed class FoldingSection : TextSegment
    {
        private readonly FoldingManager _manager;
        private bool _isFolded;
        internal CollapsedLineSection[] CollapsedSections;

        /// <summary>
        /// Gets/sets if the section is folded.
        /// </summary>
        public bool IsFolded
        {
            get { return _isFolded; }
            set
            {
                if (_isFolded != value)
                {
                    _isFolded = value;
                    ValidateCollapsedLineSections(); // create/destroy CollapsedLineSection
                    _manager.Redraw(this);
                }
            }
        }

        internal void ValidateCollapsedLineSections()
        {
            if (!_isFolded)
            {
                RemoveCollapsedLineSection();
                return;
            }
            // It is possible that StartOffset/EndOffset get set to invalid values via the property setters in TextSegment,
            // so we coerce those values into the valid range.
            DocumentLine startLine = _manager.Document.GetLineByOffset(StartOffset.CoerceValue(0, _manager.Document.TextLength));
            DocumentLine endLine = _manager.Document.GetLineByOffset(EndOffset.CoerceValue(0, _manager.Document.TextLength));
            if (startLine == endLine)
            {
                RemoveCollapsedLineSection();
            }
            else
            {
                if (CollapsedSections == null)
                    CollapsedSections = new CollapsedLineSection[_manager.TextViews.Count];
                // Validate collapsed line sections
                DocumentLine startLinePlusOne = startLine.NextLine;
                for (int i = 0; i < CollapsedSections.Length; i++)
                {
                    var collapsedSection = CollapsedSections[i];
                    if (collapsedSection == null || collapsedSection.Start != startLinePlusOne || collapsedSection.End != endLine)
                    {
                        // recreate this collapsed section
                        if (collapsedSection != null)
                        {
                            Debug.WriteLine("CollapsedLineSection validation - recreate collapsed section from " + startLinePlusOne + " to " + endLine);
                            collapsedSection.Uncollapse();
                        }
                        CollapsedSections[i] = _manager.TextViews[i].CollapseLines(startLinePlusOne, endLine);
                    }
                }
            }
        }

        /// <inheritdoc/>
        protected override void OnSegmentChanged()
        {
            ValidateCollapsedLineSections();
            base.OnSegmentChanged();
            // don't redraw if the FoldingSection wasn't added to the FoldingManager's collection yet
            if (IsConnectedToCollection)
                _manager.Redraw(this);
        }

        /// <summary>
        /// Gets/Sets the text used to display the collapsed version of the folding section.
        /// </summary>
        public string Title
        {
            get
            {
                return field;
            }
            set
            {
                if (field != value)
                {
                    field = value;
                    if (IsFolded)
                        _manager.Redraw(this);
                }
            }
        }

        /// <summary>
        /// Gets the content of the collapsed lines as text.
        /// </summary>
        public string TextContent => _manager.Document.GetText(StartOffset, EndOffset - StartOffset);

        /// <summary>
        /// Gets/Sets an additional object associated with this folding section.
        /// </summary>
        public object Tag { get; set; }

        internal FoldingSection(FoldingManager manager, int startOffset, int endOffset)
        {
            Debug.Assert(manager != null);
            _manager = manager;
            StartOffset = startOffset;
            Length = endOffset - startOffset;
        }

        private void RemoveCollapsedLineSection()
        {
            if (CollapsedSections != null)
            {
                foreach (var collapsedSection in CollapsedSections)
                {
                    if (collapsedSection?.Start != null)
                        collapsedSection.Uncollapse();
                }
                CollapsedSections = null;
            }
        }
    }
}
