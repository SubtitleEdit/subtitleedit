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
using Avalonia.Controls;
using AvaloniaEdit.Editing;

namespace AvaloniaEdit.CodeCompletion
{
    /// <summary>
    /// A popup-like window that is attached to a text segment.
    /// </summary>
    public class InsightWindow : CompletionWindowBase
    {
        /// <summary>
        /// Creates a new InsightWindow.
        /// </summary>
        public InsightWindow(TextArea textArea) : base(textArea)
        {
            CloseAutomatically = true;
            AttachEvents();
            Initialize();
        }

        private void Initialize()
        {
            var caret = this.TextArea.Caret.CalculateCaretRectangle();
            var topLevel = TopLevel.GetTopLevel(this.TextArea.TextView) as WindowBase;
            if (topLevel?.Presenter != null)
            {
                var presenter = topLevel.Presenter;
                var pointOnScreen = presenter.PointToScreen(caret.Position - this.TextArea.TextView.ScrollOffset);
                var screen = topLevel.Screens.ScreenFromPoint(pointOnScreen);

                if (screen != null)
                {
                    var scaledWorkingArea = screen.WorkingArea.ToRect(topLevel.RenderScaling);
                    MaxHeight = scaledWorkingArea.Height;
                    MaxWidth = Math.Min(scaledWorkingArea.Width, Math.Max(1000, scaledWorkingArea.Width * 0.6));
                }
            }
        }

        /// <summary>
        /// Gets/Sets whether the insight window should close automatically.
        /// The default value is true.
        /// </summary>
        public bool CloseAutomatically { get; set; }

        /// <inheritdoc/>
        protected override bool CloseOnFocusLost => CloseAutomatically;

        private void AttachEvents()
        {
            TextArea.Caret.PositionChanged += CaretPositionChanged;
        }

        /// <inheritdoc/>
        protected override void DetachEvents()
        {
            TextArea.Caret.PositionChanged -= CaretPositionChanged;
            base.DetachEvents();
        }

        private void CaretPositionChanged(object sender, EventArgs e)
        {
            if (CloseAutomatically)
            {
                var offset = TextArea.Caret.Offset;
                if (offset < StartOffset || offset > EndOffset)
                {
                    Hide();
                }
            }
        }
    }
}
