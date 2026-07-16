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
using Avalonia.Media.TextFormatting;
using AvaloniaEdit.Document;
using LogicalDirection = AvaloniaEdit.Document.LogicalDirection;

namespace AvaloniaEdit.Rendering
{
	/// <summary>
	/// VisualLineElement that represents a piece of text.
	/// </summary>
	public class VisualLineText : VisualLineElement
	{
	    /// <summary>
		/// Gets the parent visual line.
		/// </summary>
		public VisualLine ParentVisualLine { get; }

	    /// <summary>
		/// Creates a visual line text element with the specified length.
		/// It uses the <see cref="ITextRunConstructionContext.VisualLine"/> and its
		/// <see cref="VisualLineElement.RelativeTextOffset"/> to find the actual text string.
		/// </summary>
		public VisualLineText(VisualLine parentVisualLine, int length) : base(length, length)
		{
		    ParentVisualLine = parentVisualLine ?? throw new ArgumentNullException(nameof(parentVisualLine));
		}
		
		/// <summary>
		/// Override this method to control the type of new VisualLineText instances when
		/// the visual line is split due to syntax highlighting.
		/// </summary>
		protected virtual VisualLineText CreateInstance(int length)
		{
			return new VisualLineText(ParentVisualLine, length);
		}
		
		/// <inheritdoc/>
		public override TextRun CreateTextRun(int startVisualColumn, ITextRunConstructionContext context)
		{
			if (context == null)
				throw new ArgumentNullException(nameof(context));
			
			var relativeOffset = startVisualColumn - VisualColumn;

			var offset = context.VisualLine.FirstDocumentLine.Offset + RelativeTextOffset + relativeOffset;

			var text = context.GetText(
				offset,
				DocumentLength - relativeOffset);

			var textSlice = text.Text.AsMemory().Slice(text.Offset, text.Count);

            return new TextCharacters(textSlice, TextRunProperties);
        }

		/// <inheritdoc/>
		public override bool IsWhitespace(int visualColumn)
		{
			var offset = visualColumn - VisualColumn + ParentVisualLine.FirstDocumentLine.Offset + RelativeTextOffset;
			return char.IsWhiteSpace(ParentVisualLine.Document.GetCharAt(offset));
		}

		/// <inheritdoc/>
		public override ReadOnlyMemory<char> GetPrecedingText(int visualColumnLimit, ITextRunConstructionContext context)
		{
			if (context == null)
				throw new ArgumentNullException(nameof(context));

			var relativeOffset = visualColumnLimit - VisualColumn;
			
			var text = context.GetText(context.VisualLine.FirstDocumentLine.Offset + RelativeTextOffset, relativeOffset);
			
			return text.Text.AsMemory().Slice(text.Offset, text.Count);
		}
		
		/// <inheritdoc/>
		public override bool CanSplit => true;

	    /// <inheritdoc/>
		public override void Split(int splitVisualColumn, IList<VisualLineElement> elements, int elementIndex)
		{
			if (splitVisualColumn <= VisualColumn || splitVisualColumn >= VisualColumn + VisualLength)
				throw new ArgumentOutOfRangeException(nameof(splitVisualColumn), splitVisualColumn, "Value must be between " + (VisualColumn + 1) + " and " + (VisualColumn + VisualLength - 1));
			if (elements == null)
				throw new ArgumentNullException(nameof(elements));
			if (elements[elementIndex] != this)
				throw new ArgumentException("Invalid elementIndex - couldn't find this element at the index");
			var relativeSplitPos = splitVisualColumn - VisualColumn;
			var splitPart = CreateInstance(DocumentLength - relativeSplitPos);
			SplitHelper(this, splitPart, splitVisualColumn, relativeSplitPos + RelativeTextOffset);
			elements.Insert(elementIndex + 1, splitPart);
		}
		
		/// <inheritdoc/>
		public override int GetRelativeOffset(int visualColumn)
		{
			return RelativeTextOffset + visualColumn - VisualColumn;
		}
		
		/// <inheritdoc/>
		public override int GetVisualColumn(int relativeTextOffset)
		{
			return VisualColumn + relativeTextOffset - RelativeTextOffset;
		}
		
		/// <inheritdoc/>
		public override int GetNextCaretPosition(int visualColumn, LogicalDirection direction, CaretPositioningMode mode)
		{
			var textOffset = ParentVisualLine.StartOffset + RelativeTextOffset;
			var pos = TextUtilities.GetNextCaretPosition(ParentVisualLine.Document, textOffset + visualColumn - VisualColumn, direction, mode);
			if (pos < textOffset || pos > textOffset + DocumentLength)
				return -1;
		    return VisualColumn + pos - textOffset;
		}
	}
}
