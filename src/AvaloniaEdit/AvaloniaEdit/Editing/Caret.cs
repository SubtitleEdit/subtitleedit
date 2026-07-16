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
using System.Diagnostics;
using Avalonia;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using AvaloniaEdit.Utils;
using Avalonia.Media;
using Avalonia.Threading;

namespace AvaloniaEdit.Editing
{
    /// <summary>
    /// Helper class with caret-related methods.
    /// </summary>
    public sealed class Caret
    {
        private const double CaretWidth = 0.5;

        private readonly TextArea _textArea;
        private readonly TextView _textView;
        private readonly CaretLayer _caretAdorner;
        private bool _visible;

        internal Caret(TextArea textArea)
        {
            _textArea = textArea;
            _textView = textArea.TextView;
            _position = new TextViewPosition(1, 1, 0);

			_caretAdorner = new CaretLayer(textArea);
			_textView.InsertLayer(_caretAdorner, KnownLayer.Caret, LayerInsertionPosition.Replace);
			_textView.VisualLinesChanged += TextView_VisualLinesChanged;
			_textView.ScrollOffsetChanged += TextView_ScrollOffsetChanged;
		}

        internal void UpdateIfVisible()
        {
            if (_visible)
            {
                Show();
            }
        }

        private void TextView_VisualLinesChanged(object sender, EventArgs e)
        {
            if (_visible)
            {
                Show();
            }
            // required because the visual columns might have changed if the
            // element generators did something differently than on the last run
            // (e.g. a FoldingSection was collapsed)
            InvalidateVisualColumn();
        }

        private void TextView_ScrollOffsetChanged(object sender, EventArgs e)
        {
            _caretAdorner?.InvalidateVisual();
        }

        private TextViewPosition _position;

        /// <summary>
        /// Gets/Sets the position of the caret.
        /// Retrieving this property will validate the visual column (which can be expensive).
        /// Use the <see cref="Location"/> property instead if you don't need the visual column.
        /// </summary>
        public TextViewPosition Position
        {
            get
            {
                ValidateVisualColumn();
                return _position;
            }
            set
            {
                if (_position != value)
                {
                    _position = value;

                    _storedCaretOffset = -1;

                    ValidatePosition();
                    InvalidateVisualColumn();
                    RaisePositionChanged();
                    Log("Caret position changed to " + value);
                    if (_visible)
                        Show();
                }
            }
        }

        /// <summary>
        /// Gets the caret position without validating it.
        /// </summary>
        internal TextViewPosition NonValidatedPosition => _position;

        /// <summary>
        /// Gets/Sets the location of the caret.
        /// The getter of this property is faster than <see cref="Position"/> because it doesn't have
        /// to validate the visual column.
        /// </summary>
        public TextLocation Location
        {
            get => _position.Location;
            set => Position = new TextViewPosition(value);
        }

        /// <summary>
        /// Gets/Sets the caret line.
        /// </summary>
        public int Line
        {
            get => _position.Line;
            set => Position = new TextViewPosition(value, _position.Column);
        }

        /// <summary>
        /// Gets/Sets the caret column.
        /// </summary>
        public int Column
        {
            get => _position.Column;
            set => Position = new TextViewPosition(_position.Line, value);
        }

        /// <summary>
        /// Gets/Sets the caret visual column.
        /// </summary>
        public int VisualColumn
        {
            get
            {
                ValidateVisualColumn();
                return _position.VisualColumn;
            }
            set => Position = new TextViewPosition(_position.Line, _position.Column, value);
        }

        private bool _isInVirtualSpace;

        /// <summary>
        /// Gets whether the caret is in virtual space.
        /// </summary>
        public bool IsInVirtualSpace
        {
            get
            {
                ValidateVisualColumn();
                return _isInVirtualSpace;
            }
        }

        private int _storedCaretOffset;

        internal void OnDocumentChanging()
        {
            _storedCaretOffset = Offset;
            InvalidateVisualColumn();
        }

        internal void OnDocumentChanged(DocumentChangeEventArgs e)
        {
            InvalidateVisualColumn();
            if (_storedCaretOffset >= 0)
            {
                // If the caret is at the end of a selection, we don't expand the selection if something
                // is inserted at the end. Thus we also need to keep the caret in front of the insertion.
                AnchorMovementType caretMovementType;
                if (!_textArea.Selection.IsEmpty && _storedCaretOffset == _textArea.Selection.SurroundingSegment.EndOffset)
                    caretMovementType = AnchorMovementType.BeforeInsertion;
                else
                    caretMovementType = AnchorMovementType.Default;
                var newCaretOffset = e.GetNewOffset(_storedCaretOffset, caretMovementType);
                var document = _textArea.Document;
                if (document != null)
                {
                    // keep visual column
                    Position = new TextViewPosition(document.GetLocation(newCaretOffset), _position.VisualColumn);
                }
            }
            _storedCaretOffset = -1;
        }

        /// <summary>
        /// Gets/Sets the caret offset.
        /// Setting the caret offset has the side effect of setting the <see cref="DesiredXPos"/> to NaN.
        /// </summary>
        public int Offset
        {
            get
            {
                var document = _textArea.Document;
                if (document == null)
                {
                    return 0;
                }
                return document.GetOffset(_position.Location);
            }
            set
            {
                var document = _textArea.Document;
                if (document != null)
                {
                    Position = new TextViewPosition(document.GetLocation(value));
                    DesiredXPos = double.NaN;
                }
            }
        }

        /// <summary>
        /// Gets/Sets the desired x-position of the caret, in device-independent pixels.
        /// This property is NaN if the caret has no desired position.
        /// </summary>
        public double DesiredXPos { get; set; } = double.NaN;

        private void ValidatePosition()
        {
            if (_position.Line < 1)
                _position.Line = 1;
            if (_position.Column < 1)
                _position.Column = 1;
            if (_position.VisualColumn < -1)
                _position.VisualColumn = -1;
            var document = _textArea.Document;
            if (document != null)
            {
                if (_position.Line > document.LineCount)
                {
                    _position.Line = document.LineCount;
                    _position.Column = document.GetLineByNumber(_position.Line).Length + 1;
                    _position.VisualColumn = -1;
                }
                else
                {
                    var line = document.GetLineByNumber(_position.Line);
                    if (_position.Column > line.Length + 1)
                    {
                        _position.Column = line.Length + 1;
                        _position.VisualColumn = -1;
                    }
                }
            }
        }

        /// <summary>
        /// Event raised when the caret position has changed.
        /// If the caret position is changed inside a document update (between BeginUpdate/EndUpdate calls),
        /// the PositionChanged event is raised only once at the end of the document update.
        /// </summary>
        public event EventHandler PositionChanged;

        private bool _raisePositionChangedOnUpdateFinished;

        private void RaisePositionChanged()
        {
            if (_textArea.Document != null && _textArea.Document.IsInUpdate)
            {
                _raisePositionChangedOnUpdateFinished = true;
            }
            else
            {
                PositionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        internal void OnDocumentUpdateFinished()
        {
            if (_raisePositionChangedOnUpdateFinished)
            {
                _raisePositionChangedOnUpdateFinished = false;
                PositionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private bool _visualColumnValid;

        private void ValidateVisualColumn()
        {
            if (!_visualColumnValid)
            {
                var document = _textArea.Document;
                if (document != null)
                {
                   // Debug.WriteLine("Explicit validation of caret column");
                    var documentLine = document.GetLineByNumber(_position.Line);
                    RevalidateVisualColumn(_textView.GetOrConstructVisualLine(documentLine));
                }
            }
        }

        private void InvalidateVisualColumn()
        {
            _visualColumnValid = false;
        }

        /// <summary>
        /// Validates the visual column of the caret using the specified visual line.
        /// The visual line must contain the caret offset.
        /// </summary>
        private void RevalidateVisualColumn(VisualLine visualLine)
        {
            if (visualLine == null)
                throw new ArgumentNullException(nameof(visualLine));

            // mark column as validated
            _visualColumnValid = true;

            var caretOffset = _textView.Document.GetOffset(_position.Location);
            var firstDocumentLineOffset = visualLine.FirstDocumentLine.Offset;
            _position.VisualColumn = visualLine.ValidateVisualColumn(_position, _textArea.Selection.EnableVirtualSpace);

            // search possible caret positions
            var newVisualColumnForwards = visualLine.GetNextCaretPosition(_position.VisualColumn - 1, LogicalDirection.Forward, CaretPositioningMode.Normal, _textArea.Selection.EnableVirtualSpace);
            // If position.VisualColumn was valid, we're done with validation.
            if (newVisualColumnForwards != _position.VisualColumn)
            {
                // also search backwards so that we can pick the better match
                var newVisualColumnBackwards = visualLine.GetNextCaretPosition(_position.VisualColumn + 1, LogicalDirection.Backward, CaretPositioningMode.Normal, _textArea.Selection.EnableVirtualSpace);

                if (newVisualColumnForwards < 0 && newVisualColumnBackwards < 0)
                    throw ThrowUtil.NoValidCaretPosition();

                // determine offsets for new visual column positions
                int newOffsetForwards;
                if (newVisualColumnForwards >= 0)
                    newOffsetForwards = visualLine.GetRelativeOffset(newVisualColumnForwards) + firstDocumentLineOffset;
                else
                    newOffsetForwards = -1;
                int newOffsetBackwards;
                if (newVisualColumnBackwards >= 0)
                    newOffsetBackwards = visualLine.GetRelativeOffset(newVisualColumnBackwards) + firstDocumentLineOffset;
                else
                    newOffsetBackwards = -1;

                int newVisualColumn, newOffset;
                // if there's only one valid position, use it
                if (newVisualColumnForwards < 0)
                {
                    newVisualColumn = newVisualColumnBackwards;
                    newOffset = newOffsetBackwards;
                }
                else if (newVisualColumnBackwards < 0)
                {
                    newVisualColumn = newVisualColumnForwards;
                    newOffset = newOffsetForwards;
                }
                else
                {
                    // two valid positions: find the better match
                    if (Math.Abs(newOffsetBackwards - caretOffset) < Math.Abs(newOffsetForwards - caretOffset))
                    {
                        // backwards is better
                        newVisualColumn = newVisualColumnBackwards;
                        newOffset = newOffsetBackwards;
                    }
                    else
                    {
                        // forwards is better
                        newVisualColumn = newVisualColumnForwards;
                        newOffset = newOffsetForwards;
                    }
                }
                Position = new TextViewPosition(_textView.Document.GetLocation(newOffset), newVisualColumn);
            }
            _isInVirtualSpace = (_position.VisualColumn > visualLine.VisualLength);
        }

        private Rect CalcCaretRectangle(VisualLine visualLine)
        {
            if (!_visualColumnValid)
            {
                RevalidateVisualColumn(visualLine);
            }

            var textLine = visualLine.GetTextLine(_position.VisualColumn, _position.IsAtEndOfLine);
            var xPos = visualLine.GetTextLineVisualXPosition(textLine, _position.VisualColumn);
            var lineTop = visualLine.GetTextLineVisualYPosition(textLine, VisualYPosition.LineTop);
            var lineBottom = visualLine.GetTextLineVisualYPosition(textLine, VisualYPosition.LineBottom);

            return new Rect(xPos,
                            lineTop,
                            CaretWidth,
                            lineBottom - lineTop);
        }

		Rect CalcCaretOverstrikeRectangle(VisualLine visualLine)
		{
			if (!_visualColumnValid) {
				RevalidateVisualColumn(visualLine);
			}

			int currentPos = _position.VisualColumn;
			// The text being overwritten in overstrike mode is everything up to the next normal caret stop
			int nextPos = visualLine.GetNextCaretPosition(currentPos, LogicalDirection.Forward, CaretPositioningMode.Normal, true);
			var textLine = visualLine.GetTextLine(currentPos);

			Rect r;
			if (currentPos < visualLine.VisualLength) {
				// If the caret is within the text, use GetTextBounds() for the text being overwritten.
				// This is necessary to ensure the rectangle is calculated correctly in bidirectional text.
				var textBounds = textLine.GetTextBounds(currentPos, nextPos - currentPos)[0];
				r = textBounds.Rectangle;
				var y = r.Y + visualLine.GetTextLineVisualYPosition(textLine, VisualYPosition.LineTop);
				r = r.WithY(y);
			} else {
				// If the caret is at the end of the line (or in virtual space),
				// use the visual X position of currentPos and nextPos (one or more of which will be in virtual space)
				double xPos = visualLine.GetTextLineVisualXPosition(textLine, currentPos);
				double xPos2 = visualLine.GetTextLineVisualXPosition(textLine, nextPos);
				double lineTop = visualLine.GetTextLineVisualYPosition(textLine, VisualYPosition.LineTop);
				double lineBottom = visualLine.GetTextLineVisualYPosition(textLine, VisualYPosition.LineBottom);
				r = new Rect(xPos, lineTop, xPos2 - xPos, lineBottom - lineTop);
			}
			// If the caret is too small (e.g. in front of zero-width character), ensure it's still visible
			if (r.Width < CaretWidth)
				r = r.WithWidth(CaretWidth);
			return r;
		}

        /// <summary>
        /// Returns the caret rectangle. The coordinate system is in device-independent pixels from the top of the document.
        /// </summary>
        public Rect CalculateCaretRectangle()
        {
            if (_textView?.Document != null)
            {
                var visualLine = _textView.GetOrConstructVisualLine(_textView.Document.GetLineByNumber(_position.Line));
                return _textArea.OverstrikeMode ? CalcCaretOverstrikeRectangle(visualLine) : CalcCaretRectangle(visualLine);
            }
            return default;
        }

        /// <summary>
        /// Minimum distance of the caret to the view border.
        /// </summary>
        internal const double MinimumDistanceToViewBorder = 0;

        /// <summary>
        /// Scrolls the text view so that the caret is visible.
        /// </summary>
        public void BringCaretToView()
        {
            BringCaretToView(0);
        }

        public void BringCaretToView(double border)
        {
            var caretRectangle = CalculateCaretRectangle();
            if (caretRectangle != default)
            {
                caretRectangle = caretRectangle.Inflate(border);
                _textView.MakeVisible(caretRectangle);
            }
        }

        /// <summary>
        /// Makes the caret visible and updates its on-screen position.
        /// </summary>
        public void Show()
        {
            Log("Caret.Show()");
            _visible = true;
            if (!_showScheduled)
            {
                _showScheduled = true;
                Dispatcher.UIThread.Post(ShowInternal);
            }
        }

        private bool _showScheduled;
        private bool _hasWin32Caret;

        private void ShowInternal()
        {
            _showScheduled = false;

            // if show was scheduled but caret hidden in the meantime
            if (!_visible)
                return;

            if (_caretAdorner != null && _textView != null)
            {
                var visualLine = _textView.GetVisualLine(_position.Line);
                if (visualLine != null)
                {
                    var caretRect = _textArea.OverstrikeMode ? CalcCaretOverstrikeRectangle(visualLine) : CalcCaretRectangle(visualLine);
                    // TODO: win32 caret
                    // Create Win32 caret so that Windows knows where our managed caret is. This is necessary for
                    // features like 'Follow text editing' in the Windows Magnifier.
                    //if (!hasWin32Caret) {
                    //	hasWin32Caret = Win32.CreateCaret(textView, caretRect.Size);
                    //}
                    //if (hasWin32Caret) {
                    //	Win32.SetCaretPosition(textView, caretRect.Location - textView.ScrollOffset);
                    //}
                    _caretAdorner.Show(caretRect);
                    //textArea.ime.UpdateCompositionWindow();
                }
                else
                {
                    _caretAdorner.Hide();
                }
            }
        }

        /// <summary>
        /// Makes the caret invisible.
        /// </summary>
        public void Hide()
        {
            Log("Caret.Hide()");
            _visible = false;
            if (_hasWin32Caret)
            {
                // TODO: win32 caret
                //Win32.DestroyCaret();
                _hasWin32Caret = false;
            }
            _caretAdorner?.Hide();
        }

        [Conditional("DEBUG")]
        private static void Log(string text)
        {
            // commented out to make debug output less noisy - add back if there are any problems with the caret
            //Debug.WriteLine(text);
        }

        /// <summary>
        /// Gets/Sets the color of the caret.
        /// </summary>
        public IBrush CaretBrush
        {
            get => _caretAdorner.CaretBrush;
            set => _caretAdorner.CaretBrush = value;
        }
    }
}
