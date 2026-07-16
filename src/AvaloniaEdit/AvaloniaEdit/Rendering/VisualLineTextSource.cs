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
using System.Diagnostics;
using Avalonia.Media.TextFormatting;
using AvaloniaEdit.Document;
using AvaloniaEdit.Utils;
using ITextSource = Avalonia.Media.TextFormatting.ITextSource;

namespace AvaloniaEdit.Rendering
{
	/// <summary>
	/// WPF TextSource implementation that creates TextRuns for a VisualLine.
	/// </summary>
	internal sealed class VisualLineTextSource : ITextSource, ITextRunConstructionContext
	{
		public VisualLineTextSource(VisualLine visualLine)
		{
			VisualLine = visualLine;
		}

		public VisualLine VisualLine { get; private set; }
		public TextView TextView { get; set; }
		public TextDocument Document { get; set; }
		public TextRunProperties GlobalTextRunProperties { get; set; }

		public TextRun GetTextRun(int textSourceCharacterIndex)
		{
			try {
				foreach (VisualLineElement element in VisualLine.Elements) {
					if (textSourceCharacterIndex >= element.VisualColumn
						&& textSourceCharacterIndex < element.VisualColumn + element.VisualLength) {
						int relativeOffset = textSourceCharacterIndex - element.VisualColumn;
						TextRun run = element.CreateTextRun(textSourceCharacterIndex, this);
						if (run == null)
							throw new ArgumentNullException(element.GetType().Name + ".CreateTextRun");
						if (run.Length == 0)
							throw new ArgumentException("The returned TextRun must not have length 0.", element.GetType().Name + ".Length");
						if (relativeOffset + run.Length > element.VisualLength)
							throw new ArgumentException("The returned TextRun is too long.", element.GetType().Name + ".CreateTextRun");
						if (run is InlineObjectRun inlineRun) {
							inlineRun.VisualLine = VisualLine;
							VisualLine.HasInlineObjects = true;
							TextView.AddInlineObject(inlineRun);
						}
						return run;
					}
				}
				if (TextView.Options.ShowEndOfLine && textSourceCharacterIndex == VisualLine.VisualLength) {
					return CreateTextRunForNewLine();
				}
				return new TextEndOfParagraph(1);
			} catch (Exception ex) {
				Debug.WriteLine(ex.ToString());
				throw;
			}
		}

        private TextRun CreateTextRunForNewLine()
        {
            string newlineText = "";
            DocumentLine lastDocumentLine = VisualLine.LastDocumentLine;
            if (lastDocumentLine.DelimiterLength == 2)
            {
                newlineText = TextView.Options.EndOfLineCRLFGlyph;
            }
            else if (lastDocumentLine.DelimiterLength == 1)
            {
                char newlineChar = Document.GetCharAt(lastDocumentLine.Offset + lastDocumentLine.Length);
                if (newlineChar == '\r')
                    newlineText = TextView.Options.EndOfLineCRGlyph;
                else if (newlineChar == '\n')
                    newlineText = TextView.Options.EndOfLineLFGlyph;
                else
                    newlineText = "?";
            }

            var p = new VisualLineElementTextRunProperties(GlobalTextRunProperties);
            p.SetForegroundBrush(TextView.NonPrintableCharacterBrush);
            p.SetFontRenderingEmSize(GlobalTextRunProperties.FontRenderingEmSize - 2);
            var textElement = new FormattedTextElement(TextView.CachedElements.GetTextForNonPrintableCharacter(newlineText, p), 0);

            textElement.RelativeTextOffset = lastDocumentLine.Offset + lastDocumentLine.Length;

            return new FormattedTextRun(textElement, GlobalTextRunProperties);
        }

        public ReadOnlyMemory<char> GetPrecedingText(int textSourceCharacterIndexLimit)
		{
			try {
				foreach (VisualLineElement element in VisualLine.Elements) {
					if (textSourceCharacterIndexLimit > element.VisualColumn
						&& textSourceCharacterIndexLimit <= element.VisualColumn + element.VisualLength) {
						var span = element.GetPrecedingText(textSourceCharacterIndexLimit, this);
						if (span.IsEmpty)
							break;
						int relativeOffset = textSourceCharacterIndexLimit - element.VisualColumn;
						if (span.Length > relativeOffset)
							throw new ArgumentException("The returned TextSpan is too long.", element.GetType().Name + ".GetPrecedingText");
						return span;
					}
				}
				
				return ReadOnlyMemory<char>.Empty;
			} catch (Exception ex) {
				Debug.WriteLine(ex.ToString());
				throw;
			}
		}

		private string _cachedString;
		private int _cachedStringOffset;

		public StringSegment GetText(int offset, int length)
		{
			if (_cachedString != null) {
				if (offset >= _cachedStringOffset && offset + length <= _cachedStringOffset + _cachedString.Length) {
					return new StringSegment(_cachedString, offset - _cachedStringOffset, length);
				}
			}
			_cachedStringOffset = offset;
			return new StringSegment(_cachedString = Document.GetText(offset, length));
		}
	}
}
