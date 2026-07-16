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

using AvaloniaEdit.Document;

namespace AvaloniaEdit.Snippets
{
    /// <summary>
    /// Creates a named anchor that can be accessed by other SnippetElements.
    /// </summary>
    public sealed class SnippetAnchorElement : SnippetElement
    {
        /// <summary>
        /// Gets or sets the name of the anchor.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Creates a SnippetAnchorElement with the supplied name.
        /// </summary>
        public SnippetAnchorElement(string name)
        {
            Name = name;
        }

        /// <inheritdoc />
        public override void Insert(InsertionContext context)
        {
            var start = context.Document.CreateAnchor(context.InsertionPosition);
            start.MovementType = AnchorMovementType.BeforeInsertion;
            start.SurviveDeletion = true;
            var segment = new AnchorSegment(start, start);
            context.RegisterActiveElement(this, new AnchorElement(segment, Name, context));
        }
    }

    /// <summary>
    /// AnchorElement created by SnippetAnchorElement.
    /// </summary>
    public sealed class AnchorElement : IActiveElement
    {
        /// <inheritdoc />
        public bool IsEditable => false;

        private AnchorSegment _segment;
        private readonly InsertionContext _context;

        /// <inheritdoc />
        public ISegment Segment => _segment;

        /// <summary>
        /// Creates a new AnchorElement.
        /// </summary>
        public AnchorElement(AnchorSegment segment, string name, InsertionContext context)
        {
            _segment = segment;
            _context = context;
            Name = name;
        }

        /// <summary>
        /// Gets or sets the text at the anchor.
        /// </summary>
        public string Text
        {
            get => _context.Document.GetText(_segment);
            set
            {
                var offset = _segment.Offset;
                var length = _segment.Length;
                _context.Document.Replace(offset, length, value);
                if (length == 0)
                {
                    // replacing an empty anchor segment with text won't enlarge it, so we have to recreate it
                    _segment = new AnchorSegment(_context.Document, offset, value.Length);
                }
            }
        }

        /// <summary>
        /// Gets or sets the name of the anchor.
        /// </summary>
        public string Name { get; }

        /// <inheritdoc />
        public void OnInsertionCompleted()
        {
        }

        /// <inheritdoc />
        public void Deactivate(SnippetEventArgs e)
        {
        }
    }
}
