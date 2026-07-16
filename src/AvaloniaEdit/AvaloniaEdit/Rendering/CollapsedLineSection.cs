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
using System.Globalization;

namespace AvaloniaEdit.Rendering
{
    /// <summary>
	/// Represents a collapsed line section.
	/// Use the Uncollapse() method to uncollapse the section.
	/// </summary>
	public sealed class CollapsedLineSection
    {
        private readonly HeightTree _heightTree;

#if DEBUG
        internal string Id;
        private static int _nextId;
#else
		const string Id = "";
#endif

        internal CollapsedLineSection(HeightTree heightTree, DocumentLine start, DocumentLine end)
        {
            _heightTree = heightTree;
            Start = start;
            End = end;
#if DEBUG
            unchecked
            {
                Id = " #" + (_nextId++);
            }
#endif
        }

        /// <summary>
        /// Gets if the document line is collapsed.
        /// This property initially is true and turns to false when uncollapsing the section.
        /// </summary>
        public bool IsCollapsed => Start != null;

        /// <summary>
        /// Gets the start line of the section.
        /// When the section is uncollapsed or the text containing it is deleted,
        /// this property returns null.
        /// </summary>
        public DocumentLine Start { get; internal set; }

        /// <summary>
        /// Gets the end line of the section.
        /// When the section is uncollapsed or the text containing it is deleted,
        /// this property returns null.
        /// </summary>
        public DocumentLine End { get; internal set; }

        /// <summary>
        /// Uncollapses the section.
        /// This causes the Start and End properties to be set to null!
        /// Does nothing if the section is already uncollapsed.
        /// </summary>
        public void Uncollapse()
        {
            if (Start == null)
                return;

            if (!_heightTree.IsDisposed)
            {
                _heightTree.Uncollapse(this);
#if DEBUG
                _heightTree.CheckProperties();
#endif
            }

            Start = null;
            End = null;
        }

        /// <summary>
        /// Gets a string representation of the collapsed section.
        /// </summary>
        public override string ToString()
        {
            return string.Create(CultureInfo.InvariantCulture, $"[CollapsedSection{Id} {nameof(Start)}={(Start != null ? Start.LineNumber.ToString() : "null")} {nameof(End)}={(End != null ? End.LineNumber.ToString() : "null")}]");
        }
    }
}
