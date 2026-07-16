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
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Input;
using Avalonia.Threading;
using AvaloniaEdit.Document;
using AvaloniaEdit.Utils;

namespace AvaloniaEdit.Editing
{
    /// <summary>
    /// Handles selection of text using the mouse.
    /// </summary>
    internal sealed class SelectionMouseHandler : ITextAreaInputHandler
    {
        #region enum SelectionMode

        private enum SelectionMode
        {
            /// <summary>
            /// no selection (no mouse button down)
            /// </summary>
            None,
            /// <summary>
            /// left mouse button down on selection, might be normal click
            /// or might be drag'n'drop
            /// </summary>
            PossibleDragStart,
            /// <summary>
            /// dragging text
            /// </summary>
            Drag,
            /// <summary>
            /// normal selection (click+drag)
            /// </summary>
            Normal,
            /// <summary>
            /// whole-word selection (double click+drag or ctrl+click+drag)
            /// </summary>
            WholeWord,
            /// <summary>
            /// whole-line selection (triple click+drag)
            /// </summary>
            WholeLine,
            /// <summary>
            /// rectangular selection (alt+click+drag)
            /// </summary>
            Rectangular
        }
        #endregion

        private SelectionMode _mode;
        private AnchorSegment _startWord;
        private (Point position, PointerPressedEventArgs args)? _possibleDragStartMouseState;
        private Point _lastMousePosition;

        #region Constructor + Attach + Detach
        public SelectionMouseHandler(TextArea textArea)
        {
            TextArea = textArea ?? throw new ArgumentNullException(nameof(textArea));
        }

        public TextArea TextArea { get; }

        public void Attach()
        {
            TextArea.PointerPressed += TextArea_MouseLeftButtonDown;
            TextArea.PointerMoved += TextArea_MouseMove;
            TextArea.TextView.ScrollOffsetChanged += TextView_ScrollOffsetChanged;
            TextArea.PointerReleased += TextArea_MouseLeftButtonUp;
            //textArea.QueryCursor += textArea_QueryCursor;
            TextArea.OptionChanged += TextArea_OptionChanged;

            _enableTextDragDrop = TextArea.Options.EnableTextDragDrop;
            if (_enableTextDragDrop)
            {
                AttachDragDrop();
            }
        }

        public void Detach()
        {
            _mode = SelectionMode.None;
            _possibleDragStartMouseState = default;
            TextArea.PointerPressed -= TextArea_MouseLeftButtonDown;
            TextArea.PointerMoved -= TextArea_MouseMove;
            TextArea.TextView.ScrollOffsetChanged -= TextView_ScrollOffsetChanged;
            TextArea.PointerReleased -= TextArea_MouseLeftButtonUp;
            //textArea.QueryCursor -= textArea_QueryCursor;
            TextArea.OptionChanged -= TextArea_OptionChanged;
            if (_enableTextDragDrop)
            {
                DetachDragDrop();
            }
        }

        private void AttachDragDrop()
        {
            DragDrop.SetAllowDrop(TextArea, true);
            TextArea.AddHandler(DragDrop.DragEnterEvent, textArea_DragEnter);
            TextArea.AddHandler(DragDrop.DragOverEvent, textArea_DragOver);
            TextArea.AddHandler(DragDrop.DragLeaveEvent, textArea_DragLeave);
            TextArea.AddHandler(DragDrop.DropEvent, textArea_Drop);
        }

        private void DetachDragDrop()
        {
            DragDrop.SetAllowDrop(TextArea, false);
            TextArea.RemoveHandler(DragDrop.DragEnterEvent, textArea_DragEnter);
            TextArea.RemoveHandler(DragDrop.DragOverEvent, textArea_DragOver);
            TextArea.RemoveHandler(DragDrop.DragLeaveEvent, textArea_DragLeave);
            TextArea.RemoveHandler(DragDrop.DropEvent, textArea_Drop);
        }

        private bool _enableTextDragDrop;

        private void TextArea_OptionChanged(object sender, PropertyChangedEventArgs e)
        {
            var newEnableTextDragDrop = TextArea.Options.EnableTextDragDrop;
            if (newEnableTextDragDrop != _enableTextDragDrop)
            {
                _enableTextDragDrop = newEnableTextDragDrop;
                if (newEnableTextDragDrop)
                    AttachDragDrop();
                else
                    DetachDragDrop();
            }
        }
        #endregion

        #region Dropping text
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        void textArea_DragEnter(object sender, DragEventArgs e)
        {
            try
            {
                e.DragEffects = GetEffect(e);
                TextArea.Caret.Show();
            }
            catch (Exception ex)
            {
                OnDragException(ex);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        void textArea_DragOver(object sender, DragEventArgs e)
        {
            try
            {
                e.DragEffects = GetEffect(e);
            }
            catch (Exception ex)
            {
                OnDragException(ex);
            }
        }

        DragDropEffects GetEffect(DragEventArgs e)
        {
            if (e.DataTransfer.Contains(DataFormat.Text))
            {
                e.Handled = true;
                int visualColumn;
                bool isAtEndOfLine;
                int offset = GetOffsetFromMousePosition(e.GetPosition(TextArea.TextView), out visualColumn, out isAtEndOfLine);
                if (offset >= 0)
                {
                    TextArea.Caret.Position = new TextViewPosition(TextArea.Document.GetLocation(offset), visualColumn) { IsAtEndOfLine = isAtEndOfLine };
                    TextArea.Caret.DesiredXPos = double.NaN;
                    if (TextArea.ReadOnlySectionProvider.CanInsert(offset))
                    {
                        if ((e.DragEffects & DragDropEffects.Move) == DragDropEffects.Move
                            && (e.KeyModifiers & KeyModifiers.Control) != KeyModifiers.Control)
                        {
                            return DragDropEffects.Move;
                        }
                        else
                        {
                            return e.DragEffects & DragDropEffects.Copy;
                        }
                    }
                }
            }
            return DragDropEffects.None;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        void textArea_DragLeave(object sender, DragEventArgs e)
        {
            try
            {
                e.Handled = true;
                if (!TextArea.IsKeyboardFocusWithin)
                    TextArea.Caret.Hide();
            }
            catch (Exception ex)
            {
                OnDragException(ex);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        void textArea_Drop(object sender, DragEventArgs e)
        {
            try
            {
                DragDropEffects effect = GetEffect(e);
                e.DragEffects = effect;
                if (effect != DragDropEffects.None)
                {
                    int start = TextArea.Caret.Offset;
                    if (_mode == SelectionMode.Drag && TextArea.Selection.Contains(start))
                    {
                        Debug.WriteLine("Drop: did not drop: drop target is inside selection");
                        e.DragEffects = DragDropEffects.None;
                    }
                    else
                    {
                        Debug.WriteLine("Drop: insert at " + start);

                        ////var pastingEventArgs = new DataObjectPastingEventArgs(e.Data, true, DataFormats.UnicodeText);
                        ////TextArea.RaiseEvent(pastingEventArgs);
                        ////if (pastingEventArgs.CommandCancelled)
                        ////    return;

                        string text = EditingCommandHandler.GetTextToPaste(e.DataTransfer, TextArea);
                        if (text == null)
                            return;
                        bool rectangular = e.DataTransfer.Contains(DataFormat.CreateStringApplicationFormat(RectangleSelection.RectangularSelectionDataType));

                        // Mark the undo group with the currentDragDescriptor, if the drag
                        // is originating from the same control. This allows combining
                        // the undo groups when text is moved.
                        TextArea.Document.UndoStack.StartUndoGroup(this.currentDragDescriptor);
                        try
                        {
                            if (rectangular && RectangleSelection.PerformRectangularPaste(TextArea, TextArea.Caret.Position, text, true))
                            {

                            }
                            else
                            {
                                TextArea.Document.Insert(start, text);
                                TextArea.Selection = Selection.Create(TextArea, start, start + text.Length);
                            }
                        }
                        finally
                        {
                            TextArea.Document.UndoStack.EndUndoGroup();
                        }
                    }
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                OnDragException(ex);
            }
        }

        void OnDragException(Exception ex)
        {
            // swallows exceptions during drag'n'drop or reports them incorrectly, so
            // we re-throw them later to allow the application's unhandled exception handler
            // to catch them
            Dispatcher.UIThread.Post(
                new Action(delegate
                {
                    throw new DragDropException("Exception during drag'n'drop", ex);
                }),
                DispatcherPriority.Send);
        }

        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        //void textArea_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        //{
        //	try {
        //		e.UseDefaultCursors = true;
        //		e.Handled = true;
        //	} catch (Exception ex) {
        //		OnDragException(ex);
        //	}
        //}

        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        //void textArea_QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        //{
        //	try {
        //		if (e.EscapePressed) {
        //			e.Action = DragAction.Cancel;
        //		} else if ((e.KeyStates & DragDropKeyStates.LeftMouseButton) != DragDropKeyStates.LeftMouseButton) {
        //			e.Action = DragAction.Drop;
        //		} else {
        //			e.Action = DragAction.Continue;
        //		}
        //		e.Handled = true;
        //	} catch (Exception ex) {
        //		OnDragException(ex);
        //	}
        //}
        #endregion

        #region Start Drag
        object currentDragDescriptor;

        async void StartDrag(PointerPressedEventArgs e)
        {
            // prevent nested StartDrag calls
            _mode = SelectionMode.Drag;

            // mouse capture and Drag'n'Drop doesn't mix
            e.Pointer.Capture(null);

            DataTransfer dataObject = TextArea.Selection.CreateDataObject(TextArea);

            DragDropEffects allowedEffects = DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link;
            var deleteOnMove = TextArea.Selection.Segments.Select(s => new AnchorSegment(TextArea.Document, s)).ToList();
            foreach (ISegment s in deleteOnMove)
            {
                ISegment[] result = TextArea.GetDeletableSegments(s);
                if (result.Length != 1 || result[0].Offset != s.Offset || result[0].EndOffset != s.EndOffset)
                {
                    allowedEffects &= ~DragDropEffects.Move;
                }
            }

            var copyingEventArgs = new DataObjectCopyingEventArgs(dataObject, true);
            TextArea.RaiseEvent(copyingEventArgs);
            if (copyingEventArgs.CommandCancelled)
                return;

            object dragDescriptor = new object();
            this.currentDragDescriptor = dragDescriptor;

            DragDropEffects resultEffect;
            using (TextArea.AllowCaretOutsideSelection())
            {
                var oldCaretPosition = TextArea.Caret.Position;
                try
                {
                    Debug.WriteLine("DoDragDrop with allowedEffects=" + allowedEffects);
                    resultEffect = await DragDrop.DoDragDropAsync(e, dataObject, allowedEffects);
                    Debug.WriteLine("DoDragDrop done, resultEffect=" + resultEffect);
                }
                catch (Exception ex)
                {
                    // ignore COM errors - don't crash on badly implemented drop targets
                    Debug.WriteLine("DoDragDrop failed: " + ex.ToString());
                    return;
                }
                if (resultEffect == DragDropEffects.None)
                {
                    // reset caret if drag was aborted
                    TextArea.Caret.Position = oldCaretPosition;
                }
            }

            this.currentDragDescriptor = null;

            if (deleteOnMove != null && resultEffect == DragDropEffects.Move && (allowedEffects & DragDropEffects.Move) == DragDropEffects.Move)
            {
                bool draggedInsideSingleDocument = (dragDescriptor == TextArea.Document.UndoStack.LastGroupDescriptor);
                if (draggedInsideSingleDocument)
                    TextArea.Document.UndoStack.StartContinuedUndoGroup(null);
                TextArea.Document.BeginUpdate();
                try
                {
                    foreach (ISegment s in deleteOnMove)
                    {
                        TextArea.Document.Remove(s.Offset, s.Length);
                    }
                }
                finally
                {
                    TextArea.Document.EndUpdate();
                    if (draggedInsideSingleDocument)
                        TextArea.Document.UndoStack.EndUndoGroup();
                }
            }
        }
        #endregion

        #region QueryCursor
        // provide the IBeam Cursor for the text area
        //void textArea_QueryCursor(object sender, QueryCursorEventArgs e)
        //{
        //	if (!e.Handled) {
        //		if (mode != SelectionMode.None) {
        //			// during selection, use IBeam cursor even outside the text area
        //			e.Cursor = Cursors.IBeam;
        //			e.Handled = true;
        //		} else if (textArea.TextView.VisualLinesValid) {
        //			// Only query the cursor if the visual lines are valid.
        //			// If they are invalid, the cursor will get re-queried when the visual lines
        //			// get refreshed.
        //			Point p = e.GetPosition(textArea.TextView);
        //			if (p.X >= 0 && p.Y >= 0 && p.X <= textArea.TextView.ActualWidth && p.Y <= textArea.TextView.ActualHeight) {
        //				int visualColumn;
        //				bool isAtEndOfLine;
        //				int offset = GetOffsetFromMousePosition(e, out visualColumn, out isAtEndOfLine);
        //				if (enableTextDragDrop && textArea.Selection.Contains(offset))
        //					e.Cursor = Cursors.Arrow;
        //				else
        //					e.Cursor = Cursors.IBeam;
        //				e.Handled = true;
        //			}
        //		}
        //	}
        //}
        #endregion

        #region LeftButtonDown

        private void TextArea_MouseLeftButtonDown(object sender, PointerPressedEventArgs e)
        {
            var mousePosition = e.GetPosition(TextArea.TextView);
            _lastMousePosition = mousePosition;
            _possibleDragStartMouseState = default;

            var pointer = e.GetCurrentPoint(TextArea);
            if (pointer.Properties.IsLeftButtonPressed == false)
            {
                if (TextArea.RightClickMovesCaret == true && e.Handled == false)
                {
                    SetCaretOffsetToMousePosition(mousePosition);
                }
            }
            else
            {
                TextArea.Cursor = new Cursor(StandardCursorType.Ibeam);

                _mode = SelectionMode.None;
                if (!e.Handled)
                {
                    var modifiers = e.KeyModifiers;
                    var shift = modifiers.HasFlag(KeyModifiers.Shift);
                    if (_enableTextDragDrop && e.ClickCount == 1 && !shift)
                    {
                        var offset = GetOffsetFromMousePosition(mousePosition, out _, out _);
                        if (TextArea.Selection.Contains(offset))
                        {
                            if (TextArea.CapturePointer(e.Pointer))
                            {
                                _mode = SelectionMode.PossibleDragStart;
                                var position = e.GetPosition(TextArea);
                                _possibleDragStartMouseState = (position, e);
                            }
                            e.Handled = true;
                            return;
                        }
                    }

                    var oldPosition = TextArea.Caret.Position;
                    SetCaretOffsetToMousePosition(mousePosition);

                    if (e.Pointer.Type is PointerType.Pen or PointerType.Touch)
                    {
                        return;
                    }

                    if (!shift)
                    {
                        TextArea.ClearSelection();
                    }

                    if (TextArea.CapturePointer(e.Pointer))
                    {
                        if (modifiers.HasFlag(HotkeyConfiguration.BoxSelectionModifiers) && TextArea.Options.EnableRectangularSelection)
                        {
                            _mode = SelectionMode.Rectangular;
                            if (shift && TextArea.Selection is RectangleSelection)
                            {
                                TextArea.Selection = TextArea.Selection.StartSelectionOrSetEndpoint(oldPosition, TextArea.Caret.Position);
                            }
                        }
                        else if (modifiers.HasFlag(KeyModifiers.Control) && e.ClickCount == 1) // e.ClickCount == 1
                        {
                            _mode = SelectionMode.WholeWord;
                            if (shift && !(TextArea.Selection is RectangleSelection))
                            {
                                TextArea.Selection = TextArea.Selection.StartSelectionOrSetEndpoint(oldPosition, TextArea.Caret.Position);
                            }
                        }
                        else if (pointer.Properties.IsLeftButtonPressed && e.ClickCount == 1) // e.ClickCount == 1
                        {
                            _mode = SelectionMode.Normal;
                            if (shift && !(TextArea.Selection is RectangleSelection))
                            {
                                TextArea.Selection = TextArea.Selection.StartSelectionOrSetEndpoint(oldPosition, TextArea.Caret.Position);
                            }
                        }
                        else
                        {
                            SimpleSegment startWord;
                            if (e.ClickCount == 3)
                            {
                                _mode = SelectionMode.WholeLine;
                                startWord = GetLineAtMousePosition(mousePosition);
                            }
                            else
                            {
                                _mode = SelectionMode.WholeWord;
                                startWord = GetWordAtMousePosition(mousePosition);
                            }

                            if (startWord == SimpleSegment.Invalid)
                            {
                                _mode = SelectionMode.None;
                                TextArea.ReleasePointerCapture(e.Pointer);
                                return;
                            }
                            if (shift && !TextArea.Selection.IsEmpty)
                            {
                                if (startWord.Offset < TextArea.Selection.SurroundingSegment.Offset)
                                {
                                    TextArea.Selection = TextArea.Selection.SetEndpoint(new TextViewPosition(TextArea.Document.GetLocation(startWord.Offset)));
                                }
                                else if (startWord.EndOffset > TextArea.Selection.SurroundingSegment.EndOffset)
                                {
                                    TextArea.Selection = TextArea.Selection.SetEndpoint(new TextViewPosition(TextArea.Document.GetLocation(startWord.EndOffset)));
                                }
                                _startWord = new AnchorSegment(TextArea.Document, TextArea.Selection.SurroundingSegment);
                            }
                            else
                            {
                                TextArea.Selection = Selection.Create(TextArea, startWord.Offset, startWord.EndOffset);
                                _startWord = new AnchorSegment(TextArea.Document, startWord.Offset, startWord.Length);
                            }
                        }
                    }
                    e.Handled = true;
                }
            }

        }
        #endregion

        #region LeftButtonClick

        #endregion

        #region LeftButtonDoubleTap

        #endregion

        #region Mouse Position <-> Text coordinates

        private SimpleSegment GetWordAtMousePosition(Point pos)
        {
            var textView = TextArea.TextView;
            if (textView == null) return SimpleSegment.Invalid;
            if (pos.Y < 0)
                pos = pos.WithY(0);
            if (pos.Y > textView.Bounds.Height)
                pos = pos.WithY(textView.Bounds.Height);
            pos += textView.ScrollOffset;
            var line = textView.GetVisualLineFromVisualTop(pos.Y);
            if (line != null && line.TextLines != null)
            {
                var visualColumn = line.GetVisualColumn(pos, TextArea.Selection.EnableVirtualSpace);
                var wordStartVc = line.GetNextCaretPosition(visualColumn + 1, LogicalDirection.Backward, CaretPositioningMode.WordStartOrSymbol, TextArea.Selection.EnableVirtualSpace);
                if (wordStartVc == -1)
                    wordStartVc = 0;
                var wordEndVc = line.GetNextCaretPosition(wordStartVc, LogicalDirection.Forward, CaretPositioningMode.WordBorderOrSymbol, TextArea.Selection.EnableVirtualSpace);
                if (wordEndVc == -1)
                    wordEndVc = line.VisualLength;
                var relOffset = line.FirstDocumentLine.Offset;
                var wordStartOffset = line.GetRelativeOffset(wordStartVc) + relOffset;
                var wordEndOffset = line.GetRelativeOffset(wordEndVc) + relOffset;
                return new SimpleSegment(wordStartOffset, wordEndOffset - wordStartOffset);
            }
            else
            {
                return SimpleSegment.Invalid;
            }
        }

        private SimpleSegment GetLineAtMousePosition(Point pos)
        {
            var textView = TextArea.TextView;
            if (textView == null) return SimpleSegment.Invalid;
            if (pos.Y < 0)
                pos = pos.WithY(0);
            if (pos.Y > textView.Bounds.Height)
                pos = pos.WithY(textView.Bounds.Height);
            pos += textView.ScrollOffset;
            var line = textView.GetVisualLineFromVisualTop(pos.Y);
            return line != null && line.TextLines != null
                ? new SimpleSegment(line.StartOffset, line.LastDocumentLine.EndOffset - line.StartOffset)
                : SimpleSegment.Invalid;
        }

        private int GetOffsetFromMousePosition(Point positionRelativeToTextView, out int visualColumn, out bool isAtEndOfLine)
        {
            visualColumn = 0;
            var textView = TextArea.TextView;
            var pos = positionRelativeToTextView;
            if (pos.Y < 0)
                pos = pos.WithY(0);
            if (pos.Y > textView.Bounds.Height)
                pos = pos.WithY(textView.Bounds.Height);
            pos += textView.ScrollOffset;
            if (pos.Y >= textView.DocumentHeight)
                pos = pos.WithY(textView.DocumentHeight - ExtensionMethods.Epsilon);
            var line = textView.GetVisualLineFromVisualTop(pos.Y);
            if (line != null && line.TextLines != null)
            {
                visualColumn = line.GetVisualColumn(pos, TextArea.Selection.EnableVirtualSpace, out isAtEndOfLine);
                return line.GetRelativeOffset(visualColumn) + line.FirstDocumentLine.Offset;
            }
            isAtEndOfLine = false;
            return -1;
        }

        private int GetOffsetFromMousePositionFirstTextLineOnly(Point positionRelativeToTextView, out int visualColumn)
        {
            visualColumn = 0;
            var textView = TextArea.TextView;
            var pos = positionRelativeToTextView;
            if (pos.Y < 0)
                pos = pos.WithY(0);
            if (pos.Y > textView.Bounds.Height)
                pos = pos.WithY(textView.Bounds.Height);
            pos += textView.ScrollOffset;
            if (pos.Y >= textView.DocumentHeight)
                pos = pos.WithY(textView.DocumentHeight - ExtensionMethods.Epsilon);
            var line = textView.GetVisualLineFromVisualTop(pos.Y);
            if (line != null && line.TextLines != null)
            {
                visualColumn = line.GetVisualColumn(line.TextLines.First(), pos.X, TextArea.Selection.EnableVirtualSpace);
                return line.GetRelativeOffset(visualColumn) + line.FirstDocumentLine.Offset;
            }
            return -1;
        }
        #endregion

        private const int MinimumHorizontalDragDistance = 2;
        private const int MinimumVerticalDragDistance = 2;

        #region MouseMove

        private void TextArea_MouseMove(object sender, PointerEventArgs e)
        {
            if (e.Handled)
                return;

            var mousePosition = e.GetPosition(TextArea.TextView);
            _lastMousePosition = mousePosition;

            if (_mode == SelectionMode.Normal || _mode == SelectionMode.WholeWord || _mode == SelectionMode.WholeLine || _mode == SelectionMode.Rectangular)
            {
                e.Handled = true;
                if (TextArea.TextView.VisualLinesValid)
                {
                    // If the visual lines are not valid, don't extend the selection.
                    // Extending the selection forces a VisualLine refresh, and it is sufficient
                    // to do that on MouseUp, we don't have to do it every MouseMove.
                    ExtendSelectionToMouse(mousePosition);
                }
            }
            else if (_mode == SelectionMode.PossibleDragStart
                     && _possibleDragStartMouseState is { } dragStartState && dragStartState.args.Pointer.Id == e.Pointer.Id)
            {
                e.Handled = true;
                Vector mouseMovement = e.GetPosition(TextArea) - dragStartState.position;
                if (Math.Abs(mouseMovement.X) > MinimumHorizontalDragDistance
                    || Math.Abs(mouseMovement.Y) > MinimumVerticalDragDistance)
                {
                    StartDrag(dragStartState.args);
                }
            }
        }

        private void TextView_ScrollOffsetChanged(object sender, EventArgs e)
        {
            // Handle indirect mouse movement caused by scrolling the text area while holding down
            // the mouse button.
            if (_mode == SelectionMode.Normal 
                || _mode == SelectionMode.WholeWord 
                || _mode == SelectionMode.WholeLine 
                || _mode == SelectionMode.Rectangular)
            {
                if (TextArea.TextView.VisualLinesValid)
                {
                    ExtendSelectionToMouse(_lastMousePosition);
                }
            }
        }
        #endregion

        #region ExtendSelection

        private void SetCaretOffsetToMousePosition(Point mousePosition, ISegment allowedSegment = null)
        {
            int visualColumn;
            bool isAtEndOfLine;
            int offset;
            if (_mode == SelectionMode.Rectangular)
            {
                offset = GetOffsetFromMousePositionFirstTextLineOnly(mousePosition, out visualColumn);
                isAtEndOfLine = true;
            }
            else
            {
                offset = GetOffsetFromMousePosition(mousePosition, out visualColumn, out isAtEndOfLine);
            }

            if (allowedSegment != null)
            {
                offset = offset.CoerceValue(allowedSegment.Offset, allowedSegment.EndOffset);
            }

            if (offset >= 0)
            {
                TextArea.Caret.Position = new TextViewPosition(TextArea.Document.GetLocation(offset), visualColumn) { IsAtEndOfLine = isAtEndOfLine };
                TextArea.Caret.DesiredXPos = double.NaN;
            }
        }

        private void ExtendSelectionToMouse(Point pointerPosition)
        {
            var oldPosition = TextArea.Caret.Position;
            if (_mode == SelectionMode.Normal || _mode == SelectionMode.Rectangular)
            {
                SetCaretOffsetToMousePosition(pointerPosition);
                if (_mode == SelectionMode.Normal && TextArea.Selection is RectangleSelection)
                    TextArea.Selection = new SimpleSelection(TextArea, oldPosition, TextArea.Caret.Position);
                else if (_mode == SelectionMode.Rectangular && !(TextArea.Selection is RectangleSelection))
                    TextArea.Selection = new RectangleSelection(TextArea, oldPosition, TextArea.Caret.Position);
                else
                    TextArea.Selection = TextArea.Selection.StartSelectionOrSetEndpoint(oldPosition, TextArea.Caret.Position);
            }
            else if (_mode == SelectionMode.WholeWord || _mode == SelectionMode.WholeLine)
            {
                var newWord = (_mode == SelectionMode.WholeLine) ? GetLineAtMousePosition(pointerPosition) : GetWordAtMousePosition(pointerPosition);
                if (newWord != SimpleSegment.Invalid && _startWord != null)
                {
                    TextArea.Selection = Selection.Create(TextArea,
                                                          Math.Min(newWord.Offset, _startWord.Offset),
                                                          Math.Max(newWord.EndOffset, _startWord.EndOffset));
                    // moves caret to start or end of selection
                    TextArea.Caret.Offset = newWord.Offset < _startWord.Offset ? newWord.Offset : Math.Max(newWord.EndOffset, _startWord.EndOffset);
                }
            }
            TextArea.Caret.BringCaretToView(5.0);
        }
        #endregion

        #region MouseLeftButtonUp

        private void TextArea_MouseLeftButtonUp(object sender, PointerEventArgs e)
        {
            if (_mode == SelectionMode.None || e.Handled)
                return;

            e.Handled = true;
            var mousePosition = e.GetPosition(TextArea.TextView);
            _lastMousePosition = mousePosition;

            switch (_mode)
            {
                case SelectionMode.PossibleDragStart:
                    // this was not a drag start (mouse didn't move after mousedown)
                    SetCaretOffsetToMousePosition(mousePosition);
                    TextArea.ClearSelection();
                    break;
                case SelectionMode.Normal:
                case SelectionMode.WholeWord:
                case SelectionMode.WholeLine:
                case SelectionMode.Rectangular:
                    if (TextArea.Options.ExtendSelectionOnMouseUp)
                        ExtendSelectionToMouse(mousePosition);
                    break;
            }
            _mode = SelectionMode.None;
            _possibleDragStartMouseState = default;
            TextArea.ReleasePointerCapture(e.Pointer);
        }
        #endregion
    }
}
