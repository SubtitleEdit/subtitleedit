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

using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Immutable;

using AvaloniaEdit.Utils;

namespace AvaloniaEdit.Rendering
{
    /// <summary>
    /// Renders a ruler at a certain column.
    /// </summary>
    internal sealed class ColumnRulerRenderer : IBackgroundRenderer
    {
        private IPen _pen;
        private IEnumerable<int> _columns;
        private readonly TextView _textView;

        public static readonly Color DefaultForeground = Colors.LightGray;

        public ColumnRulerRenderer(TextView textView)
        {
            _pen = new ImmutablePen(new ImmutableSolidColorBrush(DefaultForeground), 1);
            _textView = textView ?? throw new ArgumentNullException(nameof(textView));
            _textView.BackgroundRenderers.Add(this);
        }

        public KnownLayer Layer => KnownLayer.Background;

        public void SetRuler(IEnumerable<int> columns, IPen pen)
        {
            _columns = columns;
            _pen = pen;
            _textView.InvalidateLayer(Layer);
        }

        public void Draw(TextView textView, DrawingContext drawingContext)
        {
            if (_columns == null)
                return;

            foreach (int column in _columns)
            {
                var offset = textView.WideSpaceWidth * column;
                var pixelSize = PixelSnapHelpers.GetPixelSize(textView);
                var markerXPos = PixelSnapHelpers.PixelAlign(offset, pixelSize.Width);
                markerXPos -= textView.ScrollOffset.X;
                var start = new Point(markerXPos, 0);
                var end = new Point(markerXPos, Math.Max(textView.DocumentHeight, textView.Bounds.Height));

                drawingContext.DrawLine(
                    _pen,
                    start.SnapToDevicePixels(textView),
                    end.SnapToDevicePixels(textView));
            }
        }
    }
}
