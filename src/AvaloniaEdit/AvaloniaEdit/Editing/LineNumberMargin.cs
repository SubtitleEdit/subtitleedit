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
using System.Globalization;
using Avalonia;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using AvaloniaEdit.Utils;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace AvaloniaEdit.Editing
{
    /// <summary>
    /// Margin showing line numbers.
    /// </summary>
    public class LineNumberMargin : AbstractMargin
    {
        private AnchorSegment _selectionStart;
        private bool _selecting;

        /// <summary>
        /// Identifies the <see cref="MinWidthInDigits"/> styled Avalonia property.
        /// </summary>
        public static readonly StyledProperty<int> MinWidthInDigitsProperty =
            AvaloniaProperty.Register<LineNumberMargin, int>(nameof(MinWidthInDigits), 2);

        /// <summary>
        /// Gets or sets the minimum width of the line number margin measured in "number of digits".
        /// This is a styled Avalonia property.
        /// </summary>
        /// <value>The minimum width in number of digits. The default value is 2.</value>
        /// <remarks>
        /// The line number margin may appear too small when there is only one digit. This property
        /// allows to reserve additional space.
        /// </remarks>
        public int MinWidthInDigits
        {
            get => GetValue(MinWidthInDigitsProperty);
            set => SetValue(MinWidthInDigitsProperty, value);
        }

        /// <summary>
        /// The typeface used for rendering the line number margin.
        /// This field is calculated in MeasureOverride() based on the FontFamily etc. properties.
        /// </summary>
        protected Typeface Typeface { get; set; }

        /// <summary>
        /// The font size used for rendering the line number margin.
        /// This field is calculated in MeasureOverride() based on the FontFamily etc. properties.
        /// </summary>
        protected double EmSize { get; set; }

        /// <inheritdoc/>
        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == MinWidthInDigitsProperty)
                OnDocumentLineCountChanged();
        }

        /// <inheritdoc/>
		protected override Size MeasureOverride(Size availableSize)
		{
			Typeface = this.CreateTypeface();
			EmSize = GetValue(TextBlock.FontSizeProperty);

			var text = TextFormatterFactory.CreateFormattedText(
				this,
				new string('9', MaxLineNumberLength),
				Typeface,
				EmSize,
				GetValue(TextBlock.ForegroundProperty)
			);
			return new Size(text.Width, 0);
		}
        
		public override void Render(DrawingContext drawingContext)
		{
			var textView = TextView;
			var renderSize = Bounds.Size;
            
			if (textView is {VisualLinesValid: true}) {
				var foreground = GetValue(TextBlock.ForegroundProperty);
				foreach (var line in textView.VisualLines) {
					var lineNumber = line.FirstDocumentLine.LineNumber;
					var text = TextFormatterFactory.CreateFormattedText(
						this,
						lineNumber.ToString(CultureInfo.CurrentCulture),
						Typeface, EmSize, foreground
					);
					var y = line.GetTextLineVisualYPosition(line.TextLines[0], VisualYPosition.TextTop);
					drawingContext.DrawText(text, new Point(renderSize.Width - text.Width, y - textView.VerticalOffset));
				}
			}
		}

        /// <inheritdoc/>
		protected override void OnTextViewChanged(TextView oldTextView, TextView newTextView)
        {
            if (oldTextView != null)
            {
                oldTextView.VisualLinesChanged -= TextViewVisualLinesChanged;
            }
            base.OnTextViewChanged(oldTextView, newTextView);
            if (newTextView != null)
            {
                newTextView.VisualLinesChanged += TextViewVisualLinesChanged;
            }
            InvalidateVisual();
        }

        /// <inheritdoc/>
        protected override void OnDocumentChanged(TextDocument oldDocument, TextDocument newDocument)
        {
            if (oldDocument != null)
            {
                TextDocumentWeakEventManager.LineCountChanged.RemoveHandler(oldDocument, OnDocumentLineCountChanged);
            }
            base.OnDocumentChanged(oldDocument, newDocument);
            if (newDocument != null)
            {
                TextDocumentWeakEventManager.LineCountChanged.AddHandler(newDocument, OnDocumentLineCountChanged);
            }
            OnDocumentLineCountChanged();
        }

        private void OnDocumentLineCountChanged(object sender, EventArgs e)
        {
            OnDocumentLineCountChanged();
        }

        void TextViewVisualLinesChanged(object sender, EventArgs e)
        {
            InvalidateMeasure();
        }


        /// <summary>
        /// Maximum length of a line number, in characters
        /// </summary>
        protected int MaxLineNumberLength = 1;

        private void OnDocumentLineCountChanged()
        {
            var documentLineCount = Document?.LineCount ?? 1;
            var newLength = documentLineCount.ToString(CultureInfo.CurrentCulture).Length;
            newLength = Math.Max(newLength, MinWidthInDigits);

            if (newLength != MaxLineNumberLength)
            {
                MaxLineNumberLength = newLength;
                InvalidateMeasure();
            }
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);

            if (!e.Handled && TextView != null && TextArea != null)
            {
                e.Handled = true;
                TextArea.Focus();

                var currentSeg = GetTextLineSegment(e);
                if (currentSeg == SimpleSegment.Invalid)
                    return;
                TextArea.Caret.Offset = currentSeg.Offset + currentSeg.Length;
                e.Pointer.Capture(this);
                if (e.Pointer.Captured == this)
                {
                    _selecting = true;
                    _selectionStart = new AnchorSegment(Document, currentSeg.Offset, currentSeg.Length);
                    if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
                    {
                        if (TextArea.Selection is SimpleSelection simpleSelection)
                            _selectionStart = new AnchorSegment(Document, simpleSelection.SurroundingSegment);
                    }
                    TextArea.Selection = Selection.Create(TextArea, _selectionStart);
                    if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
                    {
                        ExtendSelection(currentSeg);
                    }
                    TextArea.Caret.BringCaretToView(0);
                }
            }
        }

        private SimpleSegment GetTextLineSegment(PointerEventArgs e)
        {
            var pos = e.GetPosition(TextView);
            pos = new Point(0, pos.Y.CoerceValue(0, TextView.Bounds.Height) + TextView.VerticalOffset);
            var vl = TextView.GetVisualLineFromVisualTop(pos.Y);
            if (vl == null)
                return SimpleSegment.Invalid;
            var tl = vl.GetTextLineByVisualYPosition(pos.Y);
            var visualStartColumn = vl.GetTextLineVisualStartColumn(tl);
            var visualEndColumn = visualStartColumn + tl.Length;
            var relStart = vl.FirstDocumentLine.Offset;
            var startOffset = vl.GetRelativeOffset(visualStartColumn) + relStart;
            var endOffset = vl.GetRelativeOffset(visualEndColumn) + relStart;
            if (endOffset == vl.LastDocumentLine.Offset + vl.LastDocumentLine.Length)
                endOffset += vl.LastDocumentLine.DelimiterLength;
            return new SimpleSegment(startOffset, endOffset - startOffset);
        }

        private void ExtendSelection(SimpleSegment currentSeg)
        {
            if (currentSeg.Offset < _selectionStart.Offset)
            {
                TextArea.Caret.Offset = currentSeg.Offset;
                TextArea.Selection = Selection.Create(TextArea, currentSeg.Offset, _selectionStart.Offset + _selectionStart.Length);
            }
            else
            {
                TextArea.Caret.Offset = currentSeg.Offset + currentSeg.Length;
                TextArea.Selection = Selection.Create(TextArea, _selectionStart.Offset, currentSeg.Offset + currentSeg.Length);
            }
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            if (_selecting && TextArea != null && TextView != null)
            {
                e.Handled = true;
                var currentSeg = GetTextLineSegment(e);
                if (currentSeg == SimpleSegment.Invalid)
                    return;
                ExtendSelection(currentSeg);
                TextArea.Caret.BringCaretToView(0);
            }
            base.OnPointerMoved(e);
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            if (_selecting)
            {
                _selecting = false;
                _selectionStart = null;
                e.Pointer.Capture(null);
                e.Handled = true;
            }
            base.OnPointerReleased(e);
        }
    }
}
