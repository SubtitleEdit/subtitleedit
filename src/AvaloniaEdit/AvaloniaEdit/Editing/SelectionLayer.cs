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
using AvaloniaEdit.Rendering;
using Avalonia.Media;

namespace AvaloniaEdit.Editing
{
    internal sealed class SelectionLayer : Layer
    {
        private readonly TextArea _textArea;

        public SelectionLayer(TextArea textArea) : base(textArea.TextView, KnownLayer.Selection)
        {
            IsHitTestVisible = false;

            _textArea = textArea;

            TextViewWeakEventManager.VisualLinesChanged.AddHandler(TextView, ReceiveWeakEvent);
            TextViewWeakEventManager.ScrollOffsetChanged.AddHandler(TextView, ReceiveWeakEvent);
        }

        private void ReceiveWeakEvent(object sender, EventArgs e)
        {
            InvalidateVisual();
        }

        public override void Render(DrawingContext drawingContext)
        {
            if (!_textArea.TextView.VisualLinesValid)
            {
                return;
            }

            base.Render(drawingContext);

            var selectionBorder = _textArea.SelectionBorder;

            var geoBuilder = new BackgroundGeometryBuilder
            {
                AlignToWholePixels = true,
                BorderThickness = selectionBorder?.Thickness ?? 0,
                ExtendToFullWidthAtLineEnd = _textArea.Selection.EnableVirtualSpace,
                CornerRadius = _textArea.SelectionCornerRadius
            };

            foreach (var segment in _textArea.Selection.Segments)
            {
                geoBuilder.AddSegment(TextView, segment);
            }

            var geometry = geoBuilder.CreateGeometry();
            if (geometry != null)
            {
                drawingContext.DrawGeometry(_textArea.SelectionBrush, selectionBorder, geometry);
            }
        }
    }
}
