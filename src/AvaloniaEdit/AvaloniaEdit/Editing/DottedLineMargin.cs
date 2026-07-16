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

using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;

namespace AvaloniaEdit.Editing
{
    /// <summary>
    /// Margin for use with the text area.
    /// A vertical dotted line to separate the line numbers from the text view.
    /// </summary>
    public static class DottedLineMargin
    {
        private static readonly object Tag = new object();

        /// <summary>
        /// Creates a vertical dotted line to separate the line numbers from the text view.
        /// </summary>
        public static Control Create()
        {
            var line = new Line
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(0, 1),
                StrokeDashArray = new AvaloniaList<double> { 0, 2 },
                Stretch = Stretch.Fill,
                StrokeThickness = 1,
                StrokeLineCap = PenLineCap.Round,
                Margin = new Thickness(2, 0, 2, 0),
                Tag = Tag
            };

            return line;
        }

        /// <summary>
        /// Gets whether the specified UIElement is the result of a DottedLineMargin.Create call.
        /// </summary>
        public static bool IsDottedLineMargin(Control element)
        {
            var l = element as Line;
            return l != null && l.Tag == Tag;
        }
    }
}
