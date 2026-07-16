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

using System.Collections.Generic;
using System.Diagnostics;

namespace AvaloniaEdit.Rendering
{
    internal struct HeightTreeLineNode
    {
        internal HeightTreeLineNode(double height)
        {
            CollapsedSections = null;
            Height = height;
        }

        internal double Height;
        internal List<CollapsedLineSection> CollapsedSections;

        internal readonly bool IsDirectlyCollapsed
        {
            get { return CollapsedSections != null; }
        }

        internal void AddDirectlyCollapsed(CollapsedLineSection section)
        {
            if (CollapsedSections == null)
                CollapsedSections = new List<CollapsedLineSection>();
            CollapsedSections.Add(section);
        }

        internal void RemoveDirectlyCollapsed(CollapsedLineSection section)
        {
            Debug.Assert(CollapsedSections.Contains(section));
            CollapsedSections.Remove(section);
            if (CollapsedSections.Count == 0)
                CollapsedSections = null;
        }

        /// <summary>
        /// Returns 0 if the line is directly collapsed, otherwise, returns <see cref="Height"/>.
        /// </summary>
        internal readonly double TotalHeight
        {
            get
            {
                return IsDirectlyCollapsed ? 0 : Height;
            }
        }
    }
}
