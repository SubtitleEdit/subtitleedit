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
using System.Linq;
using AvaloniaEdit.Document;

namespace AvaloniaEdit.Rendering
{
    /// <summary>
	/// Base class for <see cref="IVisualLineTransformer"/> that helps
	/// colorizing the document. Derived classes can work with document lines
	/// and text offsets and this class takes care of the visual lines and visual columns.
	/// </summary>
	public abstract class DocumentColorizingTransformer : ColorizingTransformer
	{
		private DocumentLine _currentDocumentLine;
		private int _firstLineStart;
		private int _currentDocumentLineStartOffset, _currentDocumentLineEndOffset;

		/// <summary>
		/// Gets the current ITextRunConstructionContext.
		/// </summary>
		protected ITextRunConstructionContext CurrentContext { get; private set; }

		/// <inheritdoc/>
		protected override void Colorize(ITextRunConstructionContext context)
		{
			CurrentContext = context ?? throw new ArgumentNullException(nameof(context));

			_currentDocumentLine = context.VisualLine.FirstDocumentLine;
			_firstLineStart = _currentDocumentLineStartOffset = _currentDocumentLine.Offset;
			_currentDocumentLineEndOffset = _currentDocumentLineStartOffset + _currentDocumentLine.Length;
			var currentDocumentLineTotalEndOffset = _currentDocumentLineStartOffset + _currentDocumentLine.TotalLength;

			if (context.VisualLine.FirstDocumentLine == context.VisualLine.LastDocumentLine) {
				ColorizeLine(_currentDocumentLine);
			} else {
				ColorizeLine(_currentDocumentLine);
				// ColorizeLine modifies the visual line elements, loop through a copy of the line elements
				foreach (var e in context.VisualLine.Elements.ToArray()) {
					var elementOffset = _firstLineStart + e.RelativeTextOffset;
					if (elementOffset >= currentDocumentLineTotalEndOffset) {
						_currentDocumentLine = context.Document.GetLineByOffset(elementOffset);
						_currentDocumentLineStartOffset = _currentDocumentLine.Offset;
						_currentDocumentLineEndOffset = _currentDocumentLineStartOffset + _currentDocumentLine.Length;
						currentDocumentLineTotalEndOffset = _currentDocumentLineStartOffset + _currentDocumentLine.TotalLength;
						ColorizeLine(_currentDocumentLine);
					}
				}
			}
			_currentDocumentLine = null;
			CurrentContext = null;
		}

		/// <summary>
		/// Override this method to colorize an individual document line.
		/// </summary>
		protected abstract void ColorizeLine(DocumentLine line);

		/// <summary>
		/// Changes a part of the current document line.
		/// </summary>
		/// <param name="startOffset">Start offset of the region to change</param>
		/// <param name="endOffset">End offset of the region to change</param>
		/// <param name="action">Action that changes an individual <see cref="VisualLineElement"/>.</param>
		protected void ChangeLinePart(int startOffset, int endOffset, Action<VisualLineElement> action)
		{
			if (startOffset < _currentDocumentLineStartOffset || startOffset > _currentDocumentLineEndOffset)
				throw new ArgumentOutOfRangeException(nameof(startOffset), startOffset, "Value must be between " + _currentDocumentLineStartOffset + " and " + _currentDocumentLineEndOffset);
			if (endOffset < startOffset || endOffset > _currentDocumentLineEndOffset)
				throw new ArgumentOutOfRangeException(nameof(endOffset), endOffset, "Value must be between " + startOffset + " and " + _currentDocumentLineEndOffset);
			var vl = CurrentContext.VisualLine;
			var visualStart = vl.GetVisualColumn(startOffset - _firstLineStart);
			var visualEnd = vl.GetVisualColumn(endOffset - _firstLineStart);
			if (visualStart < visualEnd) {
				ChangeVisualElements(visualStart, visualEnd, action);
			}
		}
	}
}
