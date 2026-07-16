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
using System.Linq;
using System.Text;
using Avalonia.Input;
using AvaloniaEdit.Document;
using AvaloniaEdit.Utils;

namespace AvaloniaEdit.Editing
{
    /// <summary>
    /// Base class for selections.
    /// </summary>
    public abstract class Selection
    {
        /// <summary>
        /// Creates a new simple selection that selects the text from startOffset to endOffset.
        /// </summary>
        public static Selection Create(TextArea textArea, int startOffset, int endOffset)
        {
            if (textArea == null)
                throw new ArgumentNullException(nameof(textArea));
            if (startOffset == endOffset)
                return textArea.EmptySelection;
            return new SimpleSelection(textArea,
                new TextViewPosition(textArea.Document.GetLocation(startOffset)),
                new TextViewPosition(textArea.Document.GetLocation(endOffset)));
        }

        internal static Selection Create(TextArea textArea, TextViewPosition start, TextViewPosition end)
        {
            if (textArea == null)
                throw new ArgumentNullException(nameof(textArea));
            if (textArea.Document.GetOffset(start.Location) == textArea.Document.GetOffset(end.Location) && start.VisualColumn == end.VisualColumn)
                return textArea.EmptySelection;
            return new SimpleSelection(textArea, start, end);
        }

        /// <summary>
        /// Creates a new simple selection that selects the text in the specified segment.
        /// </summary>
        public static Selection Create(TextArea textArea, ISegment segment)
        {
            if (segment == null)
                throw new ArgumentNullException(nameof(segment));
            return Create(textArea, segment.Offset, segment.EndOffset);
        }

        internal TextArea TextArea { get; }

        /// <summary>
        /// Constructor for Selection.
        /// </summary>
        protected Selection(TextArea textArea)
        {
            TextArea = textArea ?? throw new ArgumentNullException(nameof(textArea));
        }

        /// <summary>
        /// Gets the start position of the selection.
        /// </summary>
        public abstract TextViewPosition StartPosition { get; }

        /// <summary>
        /// Gets the end position of the selection.
        /// </summary>
        public abstract TextViewPosition EndPosition { get; }

        /// <summary>
        /// Gets the selected text segments.
        /// </summary>
        public abstract IEnumerable<SelectionSegment> Segments { get; }

        /// <summary>
        /// Gets the smallest segment that contains all segments in this selection.
        /// May return null if the selection is empty.
        /// </summary>
        public abstract ISegment SurroundingSegment { get; }

        /// <summary>
        /// Replaces the selection with the specified text.
        /// </summary>
        public abstract void ReplaceSelectionWithText(string newText);

        internal string AddSpacesIfRequired(string newText, TextViewPosition start, TextViewPosition end)
        {
            if (EnableVirtualSpace && InsertVirtualSpaces(newText, start, end))
            {
                var line = TextArea.Document.GetLineByNumber(start.Line);
                var lineText = TextArea.Document.GetText(line);
                var vLine = TextArea.TextView.GetOrConstructVisualLine(line);
                var colDiff = start.VisualColumn - vLine.VisualLength;
                if (colDiff > 0)
                {
                    var additionalSpaces = "";
                    if (!TextArea.Options.ConvertTabsToSpaces && lineText.Trim('\t').Length == 0)
                    {
                        var tabCount = colDiff / TextArea.Options.IndentationSize;
                        additionalSpaces = new string('\t', tabCount);
                        colDiff -= tabCount * TextArea.Options.IndentationSize;
                    }
                    additionalSpaces += new string(' ', colDiff);
                    return additionalSpaces + newText;
                }
            }
            return newText;
        }

        private bool InsertVirtualSpaces(string newText, TextViewPosition start, TextViewPosition end)
        {
            return (!string.IsNullOrEmpty(newText) || !(IsInVirtualSpace(start) && IsInVirtualSpace(end)))
                && newText != "\r\n"
                && newText != "\n"
                && newText != "\r";
        }

        private bool IsInVirtualSpace(TextViewPosition pos)
        {
            return pos.VisualColumn > TextArea.TextView.GetOrConstructVisualLine(TextArea.Document.GetLineByNumber(pos.Line)).VisualLength;
        }

        /// <summary>
        /// Updates the selection when the document changes.
        /// </summary>
        public abstract Selection UpdateOnDocumentChange(DocumentChangeEventArgs e);

        /// <summary>
        /// Gets whether the selection is empty.
        /// </summary>
        public virtual bool IsEmpty => Length == 0;

        /// <summary>
        /// Gets whether virtual space is enabled for this selection.
        /// </summary>
        public virtual bool EnableVirtualSpace => TextArea.Options.EnableVirtualSpace;

        /// <summary>
        /// Gets the selection length.
        /// </summary>
        public abstract int Length { get; }

        /// <summary>
        /// Returns a new selection with the changed end point.
        /// </summary>
        /// <exception cref="NotSupportedException">Cannot set endpoint for empty selection</exception>
        public abstract Selection SetEndpoint(TextViewPosition endPosition);

        /// <summary>
        /// If this selection is empty, starts a new selection from <paramref name="startPosition"/> to
        /// <paramref name="endPosition"/>, otherwise, changes the endpoint of this selection.
        /// </summary>
        public abstract Selection StartSelectionOrSetEndpoint(TextViewPosition startPosition, TextViewPosition endPosition);

        /// <summary>
        /// Gets whether the selection is multi-line.
        /// </summary>
        public virtual bool IsMultiline
        {
            get
            {
                var surroundingSegment = SurroundingSegment;
                if (surroundingSegment == null)
                    return false;
                var start = surroundingSegment.Offset;
                var end = start + surroundingSegment.Length;
                var document = TextArea.Document;
                if (document == null)
                    throw ThrowUtil.NoDocumentAssigned();
                return document.GetLineByOffset(start) != document.GetLineByOffset(end);
            }
        }

        /// <summary>
        /// Gets the selected text.
        /// </summary>
        public virtual string GetText()
        {
            var document = TextArea.Document;
            if (document == null)
                throw ThrowUtil.NoDocumentAssigned();
            StringBuilder b = null;
            string text = null;
            foreach (var s in Segments)
            {
                if (text != null)
                {
                    if (b == null)
                        b = new StringBuilder(text);
                    else
                        b.Append(text);
                }
                text = document.GetText(s);
            }
            if (b != null)
            {
                if (text != null) b.Append(text);
                return b.ToString();
            }
            else
            {
                return text ?? string.Empty;
            }
        }

        // TODO: html
        /// <summary>
        /// Creates a HTML fragment for the selected text.
        /// </summary>
        //public string CreateHtmlFragment(HtmlOptions options)
        //{
        //	if (options == null)
        //		throw new ArgumentNullException("options");
        //	IHighlighter highlighter = textArea.GetService(typeof(IHighlighter)) as IHighlighter;
        //	StringBuilder html = new StringBuilder();
        //	bool first = true;
        //	foreach (ISegment selectedSegment in this.Segments) {
        //		if (first)
        //			first = false;
        //		else
        //			html.AppendLine("<br>");
        //		html.Append(HtmlClipboard.CreateHtmlFragment(textArea.Document, highlighter, selectedSegment, options));
        //	}
        //	return html.ToString();
        //}

        /// <inheritdoc/>
        public abstract override bool Equals(object obj);

        /// <inheritdoc/>
        public abstract override int GetHashCode();

        /// <summary>
        /// Gets whether the specified offset is included in the selection.
        /// </summary>
        /// <returns>True, if the selection contains the offset (selection borders inclusive);
        /// otherwise, false.</returns>
        public virtual bool Contains(int offset)
        {
            if (IsEmpty)
            {
                return false;
            }

            return SurroundingSegment.Contains(offset, 0) &&
                   Segments.Any(s => s.Contains(offset, 0));
        }

        /// <summary>
        /// Creates a data object containing the selection's text.
        /// </summary>
        public virtual DataTransfer CreateDataObject(TextArea textArea)
        {
            DataTransfer data = new DataTransfer();

            // Ensure we use the appropriate newline sequence for the OS
            string text = TextUtilities.NormalizeNewLines(GetText(), Environment.NewLine);

            // Enable drag/drop to Word, Notepad++ and others
            if (EditingCommandHandler.ConfirmDataFormat(textArea, data, DataFormat.Text))
            {
                var item = new DataTransferItem();
                item.Set(DataFormat.Text, text);
                data.Add(item);
            }

            // Enable drag/drop to SciTe:
            // We cannot use SetText, thus we need to use typeof(string).FullName as data format.
            // new DataObject(object) calls SetData(object), which in turn calls SetData(Type, data),
            // which then uses Type.FullName as format.
            // We immitate that behavior here as well:
            ////if (EditingCommandHandler.ConfirmDataFormat(textArea, data, typeof(string).FullName))
            ////{
            ////    data.SetData(typeof(string).FullName, text);
            ////}

            // Also copy text in HTML format to clipboard - good for pasting text into Word
            // or to the SharpDevelop forums.
            ////if (EditingCommandHandler.ConfirmDataFormat(textArea, data, DataFormats.Html))
            ////{
            ////    HtmlClipboard.SetHtml(data, CreateHtmlFragment(new HtmlOptions(textArea.Options)));
            ////}
            return data;
        }
    }
}
