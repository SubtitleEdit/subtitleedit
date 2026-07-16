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
using System.IO;
using System.Text;

namespace AvaloniaEdit.Document
{
    /// <summary>
    /// A TextWriter implementation that directly inserts into a document.
    /// </summary>
    public class DocumentTextWriter : TextWriter
    {
        private readonly IDocument _document;

        /// <summary>
        /// Creates a new DocumentTextWriter that inserts into document, starting at insertionOffset.
        /// </summary>
        public DocumentTextWriter(IDocument document, int insertionOffset)
        {
            InsertionOffset = insertionOffset;
            _document = document ?? throw new ArgumentNullException(nameof(document));
            var line = document.GetLineByOffset(insertionOffset);
            if (line.DelimiterLength == 0)
                line = line.PreviousLine;
            if (line != null)
            {
                // ReSharper disable once VirtualMemberCallInConstructor
                NewLine = document.GetText(line.EndOffset, line.DelimiterLength);
            }
        }

        /// <summary>
        /// Gets/Sets the current insertion offset.
        /// </summary>
        public int InsertionOffset { get; set; }

        /// <inheritdoc/>
        public override void Write(char value)
        {
            _document.Insert(InsertionOffset, value.ToString());
            InsertionOffset++;
        }

        /// <inheritdoc/>
        public override void Write(char[] buffer, int index, int count)
        {
            _document.Insert(InsertionOffset, new string(buffer, index, count));
            InsertionOffset += count;
        }

        /// <inheritdoc/>
        public override void Write(string value)
        {
            _document.Insert(InsertionOffset, value);
            InsertionOffset += value.Length;
        }

        /// <inheritdoc/>
        public override Encoding Encoding => Encoding.UTF8;
    }
}
