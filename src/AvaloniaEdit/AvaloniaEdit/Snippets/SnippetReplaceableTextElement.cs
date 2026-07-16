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
using System.Linq;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;

namespace AvaloniaEdit.Snippets
{
    /// <summary>
    /// Text element that is supposed to be replaced by the user.
    /// Will register an <see cref="IReplaceableActiveElement"/>.
    /// </summary>
    public class SnippetReplaceableTextElement : SnippetTextElement
    {
        /// <inheritdoc/>
        public override void Insert(InsertionContext context)
        {
            var start = context.InsertionPosition;
            base.Insert(context);
            var end = context.InsertionPosition;
            context.RegisterActiveElement(this, new ReplaceableActiveElement(context, start, end));
        }

        ///// <inheritdoc/>
        //public override Inline ToTextRun()
        //{
        //	return new Italic(base.ToTextRun());
        //}
    }

    /// <summary>
    /// Interface for active element registered by <see cref="SnippetReplaceableTextElement"/>.
    /// </summary>
    public interface IReplaceableActiveElement : IActiveElement
    {
        /// <summary>
        /// Gets the current text inside the element.
        /// </summary>
        string Text { get; }

        /// <summary>
        /// Occurs when the text inside the element changes.
        /// </summary>
        event EventHandler TextChanged;
    }

    internal sealed class ReplaceableActiveElement : IReplaceableActiveElement
    {
        private readonly InsertionContext _context;
        private readonly int _startOffset;
        private readonly int _endOffset;
        private TextAnchor _start;
        private TextAnchor _end;

        public ReplaceableActiveElement(InsertionContext context, int startOffset, int endOffset)
        {
            _context = context;
            _startOffset = startOffset;
            _endOffset = endOffset;
        }

        private void AnchorDeleted(object sender, EventArgs e)
        {
            _context.Deactivate(new SnippetEventArgs(DeactivateReason.Deleted));
        }

        public void OnInsertionCompleted()
        {
            // anchors must be created in OnInsertionCompleted because they should move only
            // due to user insertions, not due to insertions of other snippet parts
            _start = _context.Document.CreateAnchor(_startOffset);
            _start.MovementType = AnchorMovementType.BeforeInsertion;
            _end = _context.Document.CreateAnchor(_endOffset);
            _end.MovementType = AnchorMovementType.AfterInsertion;
            _start.Deleted += AnchorDeleted;
            _end.Deleted += AnchorDeleted;

            // Be careful with references from the document to the editing/snippet layer - use weak events
            // to prevent memory leaks when the text area control gets dropped from the UI while the snippet is active.
            // The InsertionContext will keep us alive as long as the snippet is in interactive mode.
            TextDocumentWeakEventManager.TextChanged.AddHandler(_context.Document, OnDocumentTextChanged);

            _background = new Renderer { Layer = KnownLayer.Background, Element = this };
            _foreground = new Renderer { Layer = KnownLayer.Text, Element = this };
            _context.TextArea.TextView.BackgroundRenderers.Add(_background);
            _context.TextArea.TextView.BackgroundRenderers.Add(_foreground);
            _context.TextArea.Caret.PositionChanged += Caret_PositionChanged;
            Caret_PositionChanged(null, null);

            Text = GetText();
        }

        public void Deactivate(SnippetEventArgs e)
        {
            TextDocumentWeakEventManager.TextChanged.RemoveHandler(_context.Document, OnDocumentTextChanged);
            _context.TextArea.TextView.BackgroundRenderers.Remove(_background);
            _context.TextArea.TextView.BackgroundRenderers.Remove(_foreground);
            _context.TextArea.Caret.PositionChanged -= Caret_PositionChanged;
        }

        private bool _isCaretInside;

        private void Caret_PositionChanged(object sender, EventArgs e)
        {
            var s = Segment;
            if (s != null)
            {
                var newIsCaretInside = s.Contains(_context.TextArea.Caret.Offset, 0);
                if (newIsCaretInside != _isCaretInside)
                {
                    _isCaretInside = newIsCaretInside;
                    _context.TextArea.TextView.InvalidateLayer(_foreground.Layer);
                }
            }
        }

        private Renderer _background, _foreground;

        public string Text { get; private set; }

        private string GetText()
        {
            if (_start.IsDeleted || _end.IsDeleted)
                return string.Empty;
            return _context.Document.GetText(_start.Offset, Math.Max(0, _end.Offset - _start.Offset));
        }

        public event EventHandler TextChanged;

        void OnDocumentTextChanged(object sender, EventArgs e)
        {
            var newText = GetText();
            if (Text != newText)
            {
                Text = newText;
                TextChanged?.Invoke(this, e);
            }
        }

        public bool IsEditable => true;

        public ISegment Segment
        {
            get
            {
                if (_start.IsDeleted || _end.IsDeleted)
                    return null;
                return new SimpleSegment(_start.Offset, Math.Max(0, _end.Offset - _start.Offset));
            }
        }

        private sealed class Renderer : IBackgroundRenderer
        {
            private static readonly IBrush BackgroundBrush = CreateBackgroundBrush();
            private static readonly Pen ActiveBorderPen = CreateBorderPen();

            private static IBrush CreateBackgroundBrush()
            {
				var b = new ImmutableSolidColorBrush(Colors.LimeGreen, 0.4);
                return b;
            }

            private static Pen CreateBorderPen()
            {
                var p = new Pen(Brushes.Black, dashStyle: DashStyle.Dot);
                return p;
            }

            internal ReplaceableActiveElement Element;

            public KnownLayer Layer { get; set; }

            public void Draw(TextView textView, DrawingContext drawingContext)
            {
                var s = Element.Segment;
                if (s != null)
                {
                    var geoBuilder = new BackgroundGeometryBuilder
                    {
                        AlignToWholePixels = true,
                        BorderThickness = ActiveBorderPen?.Thickness ?? 0
                    };
                    if (Layer == KnownLayer.Background)
                    {
                        geoBuilder.AddSegment(textView, s);
                        var geometry = geoBuilder.CreateGeometry(); 
                        if(geometry != null)
                        {
                            drawingContext.DrawGeometry(BackgroundBrush, null, geometry);
                        }
                    }
                    else
                    {
                        // draw foreground only if active
                        if (Element._isCaretInside)
                        {
                            geoBuilder.AddSegment(textView, s);
                            foreach (var boundElement in Element._context.ActiveElements.OfType<BoundActiveElement>())
                            {
                                if (boundElement.TargetElement == Element)
                                {
                                    geoBuilder.AddSegment(textView, boundElement.Segment);
                                    geoBuilder.CloseFigure();
                                }
                            }
                            var geometry = geoBuilder.CreateGeometry(); 
                            if(geometry != null)
                            {
                                drawingContext.DrawGeometry(null, ActiveBorderPen, geometry);
                            }
                        }
                    }
                }
            }
        }
    }
}
