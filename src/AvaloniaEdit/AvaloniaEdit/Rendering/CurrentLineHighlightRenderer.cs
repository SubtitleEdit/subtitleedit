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
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace AvaloniaEdit.Rendering
{
    internal sealed class CurrentLineHighlightRenderer : IBackgroundRenderer
    {
        #region Fields

        private int _line;
        private readonly TextView _textView;

        public static readonly Color DefaultBackground = Color.FromArgb(22, 20, 220, 224);
        public static readonly Color DefaultBorder = Color.FromArgb(52, 0, 255, 110);

        #endregion

        #region Properties

        public void SetDefaultColors()
        {
            BorderPen = new ImmutablePen(new ImmutableSolidColorBrush(DefaultBorder), 1);
            BackgroundBrush = new ImmutableSolidColorBrush(DefaultBackground);
        }

        public int Line {
            get { return _line; }
            set {
                if (_line != value) {
                    _line = value;
                    _textView.InvalidateLayer(Layer);
                }
            }
        }

        public KnownLayer Layer => KnownLayer.Background;

        public IBrush BackgroundBrush {
            get; set;
        }

        public IPen BorderPen {
            get; set;
        }

        #endregion

        public CurrentLineHighlightRenderer(TextView textView)
        {
            BorderPen = new ImmutablePen(new ImmutableSolidColorBrush(DefaultBorder), 1);

            BackgroundBrush = new ImmutableSolidColorBrush(DefaultBackground);

            _textView = textView ?? throw new ArgumentNullException(nameof(textView));
            _textView.BackgroundRenderers.Add(this);

            _line = 0;
        }

        public void Draw(TextView textView, DrawingContext drawingContext)
        {
            if (!_textView.Options.HighlightCurrentLine)
                return;

            var builder = new BackgroundGeometryBuilder();

            var visualLine = _textView.GetVisualLine(_line);
            if (visualLine == null) return;

            var linePosY = visualLine.VisualTop - _textView.ScrollOffset.Y;

            builder.AddRectangle(textView, new Rect(0, linePosY, textView.Bounds.Width, visualLine.Height));

            var geometry = builder.CreateGeometry();
            if (geometry != null) {
                drawingContext.DrawGeometry(BackgroundBrush, BorderPen, geometry);
            }
        }
    }
}
