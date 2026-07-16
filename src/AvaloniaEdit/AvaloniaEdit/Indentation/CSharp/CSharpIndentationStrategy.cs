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

namespace AvaloniaEdit.Indentation.CSharp
{
    /// <summary>
    /// Smart indentation for C#.
    /// </summary>
    public class CSharpIndentationStrategy : DefaultIndentationStrategy
    {
        /// <summary>
        /// Creates a new CSharpIndentationStrategy.
        /// </summary>
        public CSharpIndentationStrategy()
        {
        }

        /// <summary>
        /// Creates a new CSharpIndentationStrategy and initializes the settings using the text editor options.
        /// </summary>
        public CSharpIndentationStrategy(TextEditorOptions options)
        {
            IndentationString = options.IndentationString;
        }

        /// <summary>
        /// Gets/Sets the indentation string.
        /// </summary>
        public string IndentationString
        {
            get;
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentException("Indentation string must not be null or empty");
                field = value;
            }
        } = "\t";

        /// <summary>
        /// Performs indentation using the specified document accessor.
        /// </summary>
        /// <param name="document">Object used for accessing the document line-by-line</param>
        /// <param name="keepEmptyLines">Specifies whether empty lines should be kept</param>
        public void Indent(IDocumentAccessor document, bool keepEmptyLines)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));
            var settings = new IndentationSettings
            {
                IndentString = IndentationString,
                LeaveEmptyLines = keepEmptyLines
            };

            var r = new IndentationReformatter();
            r.Reformat(document, settings);
        }

        /// <inheritdoc cref="IIndentationStrategy.IndentLine"/>
        public override void IndentLine(TextDocument document, DocumentLine line)
        {
            var lineNr = line.LineNumber;
            var acc = new TextDocumentAccessor(document, lineNr, lineNr);
            Indent(acc, false);

            var t = acc.Text;
            if (t.Length == 0)
            {
                // use AutoIndentation for new lines in comments / verbatim strings.
                base.IndentLine(document, line);
            }
        }

        /// <inheritdoc cref="IIndentationStrategy.IndentLines"/>
        public override void IndentLines(TextDocument document, int beginLine, int endLine)
        {
            Indent(new TextDocumentAccessor(document, beginLine, endLine), true);
        }
    }
}
