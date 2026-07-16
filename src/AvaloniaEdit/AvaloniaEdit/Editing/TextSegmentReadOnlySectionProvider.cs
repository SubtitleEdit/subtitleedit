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
using AvaloniaEdit.Document;

namespace AvaloniaEdit.Editing
{
    /// <summary>
    /// Implementation for <see cref="IReadOnlySectionProvider"/> that stores the segments
    /// in a <see cref="TextSegmentCollection{T}"/>.
    /// </summary>
    public class TextSegmentReadOnlySectionProvider<T> : IReadOnlySectionProvider where T : TextSegment
    {
        /// <summary>
        /// Gets the collection storing the read-only segments.
        /// </summary>
        public TextSegmentCollection<T> Segments { get; }

        /// <summary>
        /// Creates a new TextSegmentReadOnlySectionProvider instance for the specified document.
        /// </summary>
        public TextSegmentReadOnlySectionProvider(TextDocument textDocument)
        {
            Segments = new TextSegmentCollection<T>(textDocument);
        }

        /// <summary>
        /// Creates a new TextSegmentReadOnlySectionProvider instance using the specified TextSegmentCollection.
        /// </summary>
        public TextSegmentReadOnlySectionProvider(TextSegmentCollection<T> segments)
        {
            Segments = segments ?? throw new ArgumentNullException(nameof(segments));
        }

        /// <summary>
        /// Gets whether insertion is possible at the specified offset.
        /// </summary>
        public virtual bool CanInsert(int offset)
        {
            return Segments.FindSegmentsContaining(offset)
                .All(segment => segment.StartOffset >= offset || offset >= segment.EndOffset);
        }

        /// <summary>
        /// Gets the deletable segments inside the given segment.
        /// </summary>
        public virtual IEnumerable<ISegment> GetDeletableSegments(ISegment segment)
        {
            if (segment == null)
                throw new ArgumentNullException(nameof(segment));

            if (segment.Length == 0 && CanInsert(segment.Offset))
            {
                yield return segment;
                yield break;
            }

            var readonlyUntil = segment.Offset;
            foreach (var ts in Segments.FindOverlappingSegments(segment))
            {
                var start = ts.StartOffset;
                var end = start + ts.Length;
                if (start > readonlyUntil)
                {
                    yield return new SimpleSegment(readonlyUntil, start - readonlyUntil);
                }
                if (end > readonlyUntil)
                {
                    readonlyUntil = end;
                }
            }
            var endOffset = segment.EndOffset;
            if (readonlyUntil < endOffset)
            {
                yield return new SimpleSegment(readonlyUntil, endOffset - readonlyUntil);
            }
        }
    }
}
