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
using AvaloniaEdit.Document;

namespace AvaloniaEdit.Snippets
{
    /// <summary>
    /// An element that binds to a <see cref="SnippetReplaceableTextElement"/> and displays the same text.
    /// </summary>
    public class SnippetBoundElement : SnippetElement
    {
        /// <summary>
        /// Gets/Sets the target element.
        /// </summary>
        public SnippetReplaceableTextElement TargetElement { get; set; }

        /// <summary>
        /// Converts the text before copying it.
        /// </summary>
        public virtual string ConvertText(string input)
        {
            return input;
        }

        /// <inheritdoc/>
        public override void Insert(InsertionContext context)
        {
            if (TargetElement != null)
            {
                var start = context.Document.CreateAnchor(context.InsertionPosition);
                start.MovementType = AnchorMovementType.BeforeInsertion;
                start.SurviveDeletion = true;
                var inputText = TargetElement.Text;
                if (inputText != null)
                {
                    context.InsertText(ConvertText(inputText));
                }
                var end = context.Document.CreateAnchor(context.InsertionPosition);
                end.MovementType = AnchorMovementType.BeforeInsertion;
                end.SurviveDeletion = true;
                var segment = new AnchorSegment(start, end);
                context.RegisterActiveElement(this, new BoundActiveElement(context, TargetElement, this, segment));
            }
        }

        ///// <inheritdoc/>
        //public override Inline ToTextRun()
        //{
        //	if (TargetElement != null) {
        //		string inputText = TargetElement.Text;
        //		if (inputText != null) {
        //			return new Italic(new Run(ConvertText(inputText)));
        //		}
        //	}
        //	return base.ToTextRun();
        //}
    }

    internal sealed class BoundActiveElement : IActiveElement
    {
        private readonly InsertionContext _context;
        private readonly SnippetReplaceableTextElement _targetSnippetElement;
        private readonly SnippetBoundElement _boundElement;
        internal IReplaceableActiveElement TargetElement;
        private AnchorSegment _segment;

        public BoundActiveElement(InsertionContext context, SnippetReplaceableTextElement targetSnippetElement, SnippetBoundElement boundElement, AnchorSegment segment)
        {
            _context = context;
            _targetSnippetElement = targetSnippetElement;
            _boundElement = boundElement;
            _segment = segment;
        }

        public void OnInsertionCompleted()
        {
            TargetElement = _context.GetActiveElement(_targetSnippetElement) as IReplaceableActiveElement;
            if (TargetElement != null)
            {
                TargetElement.TextChanged += targetElement_TextChanged;
            }
        }

        private void targetElement_TextChanged(object sender, EventArgs e)
        {
            // Don't copy text if the segments overlap (we would get an endless loop).
            // This can happen if the user deletes the text between the replaceable element and the bound element.
            if (SimpleSegment.GetOverlap(_segment, TargetElement.Segment) == SimpleSegment.Invalid)
            {
                var offset = _segment.Offset;
                var length = _segment.Length;
                var text = _boundElement.ConvertText(TargetElement.Text);
                if (length != text.Length || text != _context.Document.GetText(offset, length))
                {
                    // Call replace only if we're actually changing something.
                    // Without this check, we would generate an empty undo group when the user pressed undo.
                    _context.Document.Replace(offset, length, text);
                    if (length == 0)
                    {
                        // replacing an empty anchor segment with text won't enlarge it, so we have to recreate it
                        _segment = new AnchorSegment(_context.Document, offset, text.Length);
                    }
                }
            }
        }

        public void Deactivate(SnippetEventArgs e)
        {
            TargetElement.TextChanged -= targetElement_TextChanged;
        }

        public bool IsEditable => false;

        public ISegment Segment => _segment;
    }
}
