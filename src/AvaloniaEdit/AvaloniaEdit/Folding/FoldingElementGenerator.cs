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
using AvaloniaEdit.Rendering;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Media.TextFormatting;

namespace AvaloniaEdit.Folding
{
    /// <summary>
    /// A <see cref="VisualLineElementGenerator"/> that produces line elements for folded <see cref="FoldingSection"/>s.
    /// </summary>
    public sealed class FoldingElementGenerator : VisualLineElementGenerator, ITextViewConnect
    {
        private readonly List<TextView> _textViews = new List<TextView>();
        private FoldingManager _foldingManager;

        #region FoldingManager property / connecting with TextView
        /// <summary>
        /// Gets/Sets the folding manager from which the foldings should be shown.
        /// </summary>
        public FoldingManager FoldingManager
        {
            get
            {
                return _foldingManager;
            }
            set
            {
                if (_foldingManager != value)
                {
                    if (_foldingManager != null)
                    {
                        foreach (var v in _textViews)
                            _foldingManager.RemoveFromTextView(v);
                    }
                    _foldingManager = value;
                    if (_foldingManager != null)
                    {
                        foreach (var v in _textViews)
                            _foldingManager.AddToTextView(v);
                    }
                }
            }
        }

        void ITextViewConnect.AddToTextView(TextView textView)
        {
            _textViews.Add(textView);
            _foldingManager?.AddToTextView(textView);
        }

        void ITextViewConnect.RemoveFromTextView(TextView textView)
        {
            _textViews.Remove(textView);
            _foldingManager?.RemoveFromTextView(textView);
        }
        #endregion

        /// <inheritdoc/>
        public override void StartGeneration(ITextRunConstructionContext context)
        {
            base.StartGeneration(context);
            if (_foldingManager != null)
            {
                if (!_foldingManager.TextViews.Contains(context.TextView))
                    throw new ArgumentException("Invalid TextView");
                if (context.Document != _foldingManager.Document)
                    throw new ArgumentException("Invalid document");
            }
        }

        /// <inheritdoc/>
        public override int GetFirstInterestedOffset(int startOffset)
        {
            if (_foldingManager != null)
            {
                foreach (var fs in _foldingManager.GetFoldingsContaining(startOffset))
                {
                    // Test whether we're currently within a folded folding (that didn't just end).
                    // If so, create the fold marker immediately.
                    // This is necessary if the actual beginning of the fold marker got skipped due to another VisualElementGenerator.
                    if (fs.IsFolded && fs.EndOffset > startOffset)
                    {
                        //return startOffset;
                    }
                }
                return _foldingManager.GetNextFoldedFoldingStart(startOffset);
            }
            else
            {
                return -1;
            }
        }

        /// <inheritdoc/>
        public override VisualLineElement ConstructElement(int offset)
        {
            if (_foldingManager == null)
                return null;
            var foldedUntil = -1;
            FoldingSection foldingSection = null;
            foreach (var fs in _foldingManager.GetFoldingsContaining(offset))
            {
                if (fs.IsFolded)
                {
                    if (fs.EndOffset > foldedUntil)
                    {
                        foldedUntil = fs.EndOffset;
                        foldingSection = fs;
                    }
                }
            }
            if (foldedUntil > offset && foldingSection != null)
            {
                // Handle overlapping foldings: if there's another folded folding
                // (starting within the foldingSection) that continues after the end of the folded section,
                // then we'll extend our fold element to cover that overlapping folding.
                bool foundOverlappingFolding;
                do
                {
                    foundOverlappingFolding = false;
                    foreach (var fs in FoldingManager.GetFoldingsContaining(foldedUntil))
                    {
                        if (fs.IsFolded && fs.EndOffset > foldedUntil)
                        {
                            foldedUntil = fs.EndOffset;
                            foundOverlappingFolding = true;
                        }
                    }
                } while (foundOverlappingFolding);

				string title = foldingSection.Title;
				if (string.IsNullOrEmpty(title))
					title = "...";
				var properties = new VisualLineElementTextRunProperties(CurrentContext.GlobalTextRunProperties);
				properties.SetForegroundBrush(TextBrush);
				var text = TextFormatter.Current.FormatLine(new SimpleTextSource(title, properties), 0, double.MaxValue, new GenericTextParagraphProperties(properties));
				return new FoldingLineElement(foldingSection, text, foldedUntil - offset, TextBrush);
			} else {
				return null;
			}
		}

        private sealed class FoldingLineElement : FormattedTextElement
        {
            private readonly FoldingSection _fs;
            private readonly IBrush _textBrush;

            public FoldingLineElement(FoldingSection fs, TextLine text, int documentLength, IBrush textBrush) : base(text, documentLength)
            {
                _fs = fs;
                _textBrush = textBrush;
            }

			public override TextRun CreateTextRun(int startVisualColumn, ITextRunConstructionContext context)
			{
				return new FoldingLineTextRun(this, this.TextRunProperties, _textBrush);
			}

            //DOUBLETAP
            protected internal override void OnPointerPressed(PointerPressedEventArgs e)
            {
                _fs.IsFolded = false;
                e.Handled = true;
            }
        }

        private sealed class FoldingLineTextRun : FormattedTextRun
        {
            private readonly IBrush _textBrush;

            public FoldingLineTextRun(FormattedTextElement element, TextRunProperties properties, IBrush textBrush)
                : base(element, properties)
            {
                _textBrush = textBrush;
            }

            public override void Draw(DrawingContext drawingContext, Point origin)
            {
                var (width, height) = Size;
                var r = new Rect(origin.X, origin.Y, width, height);
                drawingContext.DrawRectangle(new ImmutablePen(_textBrush.ToImmutable()), r);
                base.Draw(drawingContext, origin);
            }
        }

        /// <summary>
        /// Default brush for folding element text. Value: Brushes.Gray
        /// </summary>
        public static IBrush DefaultTextBrush { get; } = Brushes.Gray;

        /// <summary>
        /// Gets/sets the brush used for folding element text.
        /// </summary>
        public static IBrush TextBrush { get; set; } = DefaultTextBrush;
    }
}
