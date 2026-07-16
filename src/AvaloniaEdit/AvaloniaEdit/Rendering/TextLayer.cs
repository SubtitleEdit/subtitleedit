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
using Avalonia;

namespace AvaloniaEdit.Rendering
{
    /// <summary>
    /// The control that contains the text.
    /// 
    /// This control is used to allow other UIElements to be placed inside the TextView but
    /// behind the text.
    /// The text rendering process (VisualLine creation) is controlled by the TextView, this
    /// class simply displays the created Visual Lines.
    /// </summary>
    /// <remarks>
    /// This class does not contain any input handling and is invisible to hit testing. Input
    /// is handled by the TextView.
    /// This allows UIElements that are displayed behind the text, but still can react to mouse input.
    /// </remarks>
    internal sealed class TextLayer : Layer
    {
        /// <summary>
        /// the index of the text layer in the layers collection
        /// </summary>
        internal int Index;

        public TextLayer(TextView textView) : base(textView, KnownLayer.Text)
        {
        }

        private readonly List<VisualLineDrawingVisual> _visuals = new List<VisualLineDrawingVisual>();

        internal void SetVisualLines(ICollection<VisualLine> visualLines)
        {
            foreach (var v in _visuals)
            {
                if (v.VisualLine.IsDisposed)
                {
                    VisualChildren.Remove(v);
                }
            }

            _visuals.Clear();
            foreach (var newLine in visualLines)
            {
                var visual = newLine.Render();
                if (!visual.IsAdded)
                {
                    VisualChildren.Add(visual);
                    visual.IsAdded = true;
                }

                _visuals.Add(visual);
            }

            InvalidateArrange();
        }

        protected override void ArrangeCore(Rect finalRect)
        {
            base.ArrangeCore(finalRect);
            TextView.ArrangeTextLayer(_visuals);
        }
    }
}
