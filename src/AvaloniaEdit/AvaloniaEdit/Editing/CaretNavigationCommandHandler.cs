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
using System.Diagnostics;
using Avalonia;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using AvaloniaEdit.Utils;
using Avalonia.Input;
using Avalonia.Media.TextFormatting;
using LogicalDirection = AvaloniaEdit.Document.LogicalDirection;

namespace AvaloniaEdit.Editing
{
    internal enum CaretMovementType
    {
        None,
        CharLeft,
        CharRight,
        Backspace,
        WordLeft,
        WordRight,
        LineUp,
        LineDown,
        PageUp,
        PageDown,
        LineStart,
        LineEnd,
        DocumentStart,
        DocumentEnd
    }

    internal static class CaretNavigationCommandHandler
    {
        /// <summary>
        /// Creates a new <see cref="TextAreaInputHandler"/> for the text area.
        /// </summary>
        public static TextAreaInputHandler Create(TextArea textArea)
        {
            var handler = new TextAreaInputHandler(textArea);
            handler.CommandBindings.AddRange(CommandBindings);
            handler.KeyBindings.AddRange(KeyBindings);
            return handler;
        }

        static readonly List<RoutedCommandBinding> CommandBindings = new List<RoutedCommandBinding>();
        static readonly List<KeyBinding> KeyBindings = new List<KeyBinding>();

        private static void AddBinding(
            RoutedCommand command,
            EventHandler<ExecutedRoutedEventArgs> handler,
            EventHandler<CanExecuteRoutedEventArgs> canExecuteHandler = null)
        {
            CommandBindings.Add(new RoutedCommandBinding(command, handler, canExecuteHandler));
        }

        private static void AddBinding(
            RoutedCommand command,
            KeyModifiers modifiers, Key key,
            EventHandler<ExecutedRoutedEventArgs> handler,
            EventHandler<CanExecuteRoutedEventArgs> canExecuteHandler = null)
        {
            AddBinding(command, new KeyGesture(key, modifiers), handler, canExecuteHandler);
        }

        private static void AddBinding(
            RoutedCommand command,
            KeyGesture gesture,
            EventHandler<ExecutedRoutedEventArgs> handler,
            EventHandler<CanExecuteRoutedEventArgs> canExecuteHandler = null)
        {
            AddBinding(command, handler, canExecuteHandler);
            KeyBindings.Add(TextAreaDefaultInputHandler.CreateKeyBinding(command, gesture));
        }

        static CaretNavigationCommandHandler()
        {
            var keymap = HotkeyConfiguration.Keymap;
            var boxSelectionModifiers = HotkeyConfiguration.BoxSelectionModifiers;

            AddBinding(EditingCommands.MoveLeftByCharacter, KeyModifiers.None, Key.Left, OnMoveCaret(CaretMovementType.CharLeft));
            AddBinding(EditingCommands.SelectLeftByCharacter, keymap.SelectionModifiers, Key.Left, OnMoveCaretExtendSelection(CaretMovementType.CharLeft));
            AddBinding(RectangleSelection.BoxSelectLeftByCharacter, boxSelectionModifiers | keymap.SelectionModifiers, Key.Left, OnMoveCaretBoxSelection(CaretMovementType.CharLeft));
            AddBinding(EditingCommands.MoveRightByCharacter, KeyModifiers.None, Key.Right, OnMoveCaret(CaretMovementType.CharRight));
            AddBinding(EditingCommands.SelectRightByCharacter, keymap.SelectionModifiers, Key.Right, OnMoveCaretExtendSelection(CaretMovementType.CharRight));
            AddBinding(RectangleSelection.BoxSelectRightByCharacter, boxSelectionModifiers | keymap.SelectionModifiers, Key.Right, OnMoveCaretBoxSelection(CaretMovementType.CharRight));

            AddBinding(EditingCommands.MoveLeftByWord, keymap.WholeWordTextActionModifiers, Key.Left, OnMoveCaret(CaretMovementType.WordLeft));
            AddBinding(EditingCommands.SelectLeftByWord, keymap.WholeWordTextActionModifiers | keymap.SelectionModifiers, Key.Left, OnMoveCaretExtendSelection(CaretMovementType.WordLeft));
            AddBinding(RectangleSelection.BoxSelectLeftByWord, keymap.WholeWordTextActionModifiers | boxSelectionModifiers | keymap.SelectionModifiers, Key.Left, OnMoveCaretBoxSelection(CaretMovementType.WordLeft));
            AddBinding(EditingCommands.MoveRightByWord, keymap.WholeWordTextActionModifiers, Key.Right, OnMoveCaret(CaretMovementType.WordRight));
            AddBinding(EditingCommands.SelectRightByWord, keymap.WholeWordTextActionModifiers | keymap.SelectionModifiers, Key.Right, OnMoveCaretExtendSelection(CaretMovementType.WordRight));
            AddBinding(RectangleSelection.BoxSelectRightByWord, keymap.WholeWordTextActionModifiers | boxSelectionModifiers | keymap.SelectionModifiers, Key.Right, OnMoveCaretBoxSelection(CaretMovementType.WordRight));

            AddBinding(EditingCommands.MoveUpByLine, KeyModifiers.None, Key.Up, OnMoveCaret(CaretMovementType.LineUp));
            AddBinding(EditingCommands.SelectUpByLine, keymap.SelectionModifiers, Key.Up, OnMoveCaretExtendSelection(CaretMovementType.LineUp));
            AddBinding(RectangleSelection.BoxSelectUpByLine, boxSelectionModifiers | keymap.SelectionModifiers, Key.Up, OnMoveCaretBoxSelection(CaretMovementType.LineUp));
            AddBinding(EditingCommands.MoveDownByLine, KeyModifiers.None, Key.Down, OnMoveCaret(CaretMovementType.LineDown));
            AddBinding(EditingCommands.SelectDownByLine, keymap.SelectionModifiers, Key.Down, OnMoveCaretExtendSelection(CaretMovementType.LineDown));
            AddBinding(RectangleSelection.BoxSelectDownByLine, boxSelectionModifiers | keymap.SelectionModifiers, Key.Down, OnMoveCaretBoxSelection(CaretMovementType.LineDown));

            AddBinding(EditingCommands.MoveDownByPage, KeyModifiers.None, Key.PageDown, OnMoveCaret(CaretMovementType.PageDown));
            AddBinding(EditingCommands.SelectDownByPage, keymap.SelectionModifiers, Key.PageDown, OnMoveCaretExtendSelection(CaretMovementType.PageDown));
            AddBinding(EditingCommands.MoveUpByPage, KeyModifiers.None, Key.PageUp, OnMoveCaret(CaretMovementType.PageUp));
            AddBinding(EditingCommands.SelectUpByPage, keymap.SelectionModifiers, Key.PageUp, OnMoveCaretExtendSelection(CaretMovementType.PageUp));

            foreach (var keyGesture in keymap.MoveCursorToTheStartOfLine)
                AddBinding(EditingCommands.MoveToLineStart, keyGesture, OnMoveCaret(CaretMovementType.LineStart));
            foreach (var keyGesture in keymap.MoveCursorToTheStartOfLineWithSelection)
                AddBinding(EditingCommands.SelectToLineStart, keyGesture, OnMoveCaretExtendSelection(CaretMovementType.LineStart));
            foreach (var keyGesture in keymap.MoveCursorToTheEndOfLine)
                AddBinding(EditingCommands.MoveToLineEnd, keyGesture, OnMoveCaret(CaretMovementType.LineEnd));
            foreach (var keyGesture in keymap.MoveCursorToTheEndOfLineWithSelection)
                AddBinding(EditingCommands.SelectToLineEnd, keyGesture, OnMoveCaretExtendSelection(CaretMovementType.LineEnd));

            AddBinding(RectangleSelection.BoxSelectToLineStart, boxSelectionModifiers | keymap.SelectionModifiers, Key.Home, OnMoveCaretBoxSelection(CaretMovementType.LineStart));
            AddBinding(RectangleSelection.BoxSelectToLineEnd, boxSelectionModifiers | keymap.SelectionModifiers, Key.End, OnMoveCaretBoxSelection(CaretMovementType.LineEnd));

            foreach (var keyGesture in keymap.MoveCursorToTheStartOfDocument)
                AddBinding(EditingCommands.MoveToDocumentStart, keyGesture, OnMoveCaret(CaretMovementType.DocumentStart));
            foreach (var keyGesture in keymap.MoveCursorToTheStartOfDocumentWithSelection)
                AddBinding(EditingCommands.SelectToDocumentStart, keyGesture, OnMoveCaretExtendSelection(CaretMovementType.DocumentStart));
            foreach (var keyGesture in keymap.MoveCursorToTheEndOfDocument)
                AddBinding(EditingCommands.MoveToDocumentEnd, keyGesture, OnMoveCaret(CaretMovementType.DocumentEnd));
            foreach (var keyGesture in keymap.MoveCursorToTheEndOfDocumentWithSelection)
                AddBinding(EditingCommands.SelectToDocumentEnd, keyGesture, OnMoveCaretExtendSelection(CaretMovementType.DocumentEnd));

            AddBinding(ApplicationCommands.SelectAll, OnSelectAll, CanSelectAll);
        }

        private static void CanSelectAll(object target, CanExecuteRoutedEventArgs args)
        {
            var textArea = GetTextArea(target);
            if (textArea is { Document: not null })
            {
                args.Handled = true;
                args.CanExecute = true;
            }
        }

        private static void OnSelectAll(object target, ExecutedRoutedEventArgs args)
        {
            var textArea = GetTextArea(target);
            if (textArea?.Document != null)
            {
                args.Handled = true;
                textArea.Caret.Offset = textArea.Document.TextLength;
                textArea.Selection = Selection.Create(textArea, 0, textArea.Document.TextLength);
            }
        }

        private static TextArea GetTextArea(object target)
        {
            return target as TextArea;
        }

        private static EventHandler<ExecutedRoutedEventArgs> OnMoveCaret(CaretMovementType direction)
        {
            return (target, args) =>
            {
                var textArea = GetTextArea(target);
                if (textArea?.Document != null)
                {
                    args.Handled = true;
                    textArea.ClearSelection();
                    MoveCaret(textArea, direction);
                    textArea.Caret.BringCaretToView();
                }
            };
        }

        private static EventHandler<ExecutedRoutedEventArgs> OnMoveCaretExtendSelection(CaretMovementType direction)
        {
            return (target, args) =>
            {
                var textArea = GetTextArea(target);
                if (textArea?.Document != null)
                {
                    args.Handled = true;
                    var oldPosition = textArea.Caret.Position;
                    MoveCaret(textArea, direction);
                    textArea.Selection = textArea.Selection.StartSelectionOrSetEndpoint(oldPosition, textArea.Caret.Position);
                    textArea.Caret.BringCaretToView();
                }
            };
        }

        private static EventHandler<ExecutedRoutedEventArgs> OnMoveCaretBoxSelection(CaretMovementType direction)
        {
            return (target, args) =>
            {
                var textArea = GetTextArea(target);
                if (textArea?.Document != null)
                {
                    args.Handled = true;
                    // First, convert the selection into a rectangle selection
                    // (this is required so that virtual space gets enabled for the caret movement)
                    if (textArea.Options.EnableRectangularSelection && !(textArea.Selection is RectangleSelection))
                    {
                        textArea.Selection = textArea.Selection.IsEmpty
                            ? new RectangleSelection(textArea, textArea.Caret.Position, textArea.Caret.Position)
                            : new RectangleSelection(textArea, textArea.Selection.StartPosition,
                                textArea.Caret.Position);
                    }
                    // Now move the caret and extend the selection
                    var oldPosition = textArea.Caret.Position;
                    MoveCaret(textArea, direction);
                    textArea.Selection = textArea.Selection.StartSelectionOrSetEndpoint(oldPosition, textArea.Caret.Position);
                    textArea.Caret.BringCaretToView();
                }
            };
        }

        #region Caret movement
        internal static void MoveCaret(TextArea textArea, CaretMovementType direction)
        {
            var desiredXPos = textArea.Caret.DesiredXPos;
            textArea.Caret.Position = GetNewCaretPosition(textArea.TextView, textArea.Caret.Position, direction, textArea.Selection.EnableVirtualSpace, ref desiredXPos);
            textArea.Caret.DesiredXPos = desiredXPos;
        }

        internal static TextViewPosition GetNewCaretPosition(TextView textView, TextViewPosition caretPosition, CaretMovementType direction, bool enableVirtualSpace, ref double desiredXPos)
        {
            switch (direction)
            {
                case CaretMovementType.None:
                    return caretPosition;
                case CaretMovementType.DocumentStart:
                    desiredXPos = double.NaN;
                    return new TextViewPosition(0, 0);
                case CaretMovementType.DocumentEnd:
                    desiredXPos = double.NaN;
                    return new TextViewPosition(textView.Document.GetLocation(textView.Document.TextLength));
            }
            var caretLine = textView.Document.GetLineByNumber(caretPosition.Line);
            var visualLine = textView.GetOrConstructVisualLine(caretLine);
            var textLine = visualLine.GetTextLine(caretPosition.VisualColumn, caretPosition.IsAtEndOfLine);
            switch (direction)
            {
                case CaretMovementType.CharLeft:
                    desiredXPos = double.NaN;
                    // do not move caret to previous line in virtual space
                    if (caretPosition.VisualColumn == 0 && enableVirtualSpace)
                        return caretPosition;
                    return GetPrevCaretPosition(textView, caretPosition, visualLine, CaretPositioningMode.Normal, enableVirtualSpace);
                case CaretMovementType.Backspace:
                    desiredXPos = double.NaN;
                    return GetPrevCaretPosition(textView, caretPosition, visualLine, CaretPositioningMode.EveryCodepoint, enableVirtualSpace);
                case CaretMovementType.CharRight:
                    desiredXPos = double.NaN;
                    return GetNextCaretPosition(textView, caretPosition, visualLine, CaretPositioningMode.Normal, enableVirtualSpace);
                case CaretMovementType.WordLeft:
                    desiredXPos = double.NaN;
                    return GetPrevCaretPosition(textView, caretPosition, visualLine, CaretPositioningMode.WordStart, enableVirtualSpace);
                case CaretMovementType.WordRight:
                    desiredXPos = double.NaN;
                    return GetNextCaretPosition(textView, caretPosition, visualLine, CaretPositioningMode.WordStart, enableVirtualSpace);
                case CaretMovementType.LineUp:
                case CaretMovementType.LineDown:
                case CaretMovementType.PageUp:
                case CaretMovementType.PageDown:
                    return GetUpDownCaretPosition(textView, caretPosition, direction, visualLine, textLine, enableVirtualSpace, ref desiredXPos);
                case CaretMovementType.LineStart:
                    desiredXPos = double.NaN;
                    return GetStartOfLineCaretPosition(caretPosition.VisualColumn, visualLine, textLine, enableVirtualSpace);
                case CaretMovementType.LineEnd:
                    desiredXPos = double.NaN;
                    return GetEndOfLineCaretPosition(visualLine, textLine);
                default:
                    throw new NotSupportedException(direction.ToString());
            }
        }
        #endregion

        #region Home/End

        private static TextViewPosition GetStartOfLineCaretPosition(int oldVisualColumn, VisualLine visualLine, TextLine textLine, bool enableVirtualSpace)
        {
            var newVisualCol = visualLine.GetTextLineVisualStartColumn(textLine);
            if (newVisualCol == 0)
                newVisualCol = visualLine.GetNextCaretPosition(newVisualCol - 1, LogicalDirection.Forward, CaretPositioningMode.WordStart, enableVirtualSpace);
            if (newVisualCol < 0)
                throw ThrowUtil.NoValidCaretPosition();
            // when the caret is already at the start of the text, jump to start before whitespace
            if (newVisualCol == oldVisualColumn)
                newVisualCol = 0;
            return visualLine.GetTextViewPosition(newVisualCol);
        }

        private static TextViewPosition GetEndOfLineCaretPosition(VisualLine visualLine, TextLine textLine)
        {
            var newVisualCol = visualLine.GetTextLineVisualStartColumn(textLine) + textLine.Length - textLine.NewLineLength;
            var pos = visualLine.GetTextViewPosition(newVisualCol);
            pos.IsAtEndOfLine = true;
            return pos;
        }
        #endregion

        #region By-character / By-word movement

        private static TextViewPosition GetNextCaretPosition(TextView textView, TextViewPosition caretPosition, VisualLine visualLine, CaretPositioningMode mode, bool enableVirtualSpace)
        {
            var pos = visualLine.GetNextCaretPosition(caretPosition.VisualColumn, LogicalDirection.Forward, mode, enableVirtualSpace);
            if (pos >= 0)
            {
                return visualLine.GetTextViewPosition(pos);
            }
            else
            {
                // move to start of next line
                var nextDocumentLine = visualLine.LastDocumentLine.NextLine;
                if (nextDocumentLine != null)
                {
                    var nextLine = textView.GetOrConstructVisualLine(nextDocumentLine);
                    pos = nextLine.GetNextCaretPosition(-1, LogicalDirection.Forward, mode, enableVirtualSpace);
                    if (pos < 0)
                        throw ThrowUtil.NoValidCaretPosition();
                    return nextLine.GetTextViewPosition(pos);
                }
                else
                {
                    // at end of document
                    Debug.Assert(visualLine.LastDocumentLine.Offset + visualLine.LastDocumentLine.TotalLength == textView.Document.TextLength);
                    return new TextViewPosition(textView.Document.GetLocation(textView.Document.TextLength));
                }
            }
        }

        private static TextViewPosition GetPrevCaretPosition(TextView textView, TextViewPosition caretPosition, VisualLine visualLine, CaretPositioningMode mode, bool enableVirtualSpace)
        {
            var pos = visualLine.GetNextCaretPosition(caretPosition.VisualColumn, LogicalDirection.Backward, mode, enableVirtualSpace);
            if (pos >= 0)
            {
                return visualLine.GetTextViewPosition(pos);
            }
            else
            {
                // move to end of previous line
                var previousDocumentLine = visualLine.FirstDocumentLine.PreviousLine;
                if (previousDocumentLine != null)
                {
                    var previousLine = textView.GetOrConstructVisualLine(previousDocumentLine);
                    pos = previousLine.GetNextCaretPosition(previousLine.VisualLength + 1, LogicalDirection.Backward, mode, enableVirtualSpace);
                    if (pos < 0)
                        throw ThrowUtil.NoValidCaretPosition();
                    return previousLine.GetTextViewPosition(pos);
                }
                else
                {
                    // at start of document
                    Debug.Assert(visualLine.FirstDocumentLine.Offset == 0);
                    return new TextViewPosition(0, 0);
                }
            }
        }
        #endregion

        #region Line+Page up/down

        private static TextViewPosition GetUpDownCaretPosition(TextView textView, TextViewPosition caretPosition, CaretMovementType direction, VisualLine visualLine, TextLine textLine, bool enableVirtualSpace, ref double xPos)
        {
            // moving up/down happens using the desired visual X position
            if (double.IsNaN(xPos))
                xPos = visualLine.GetTextLineVisualXPosition(textLine, caretPosition.VisualColumn);
            // now find the TextLine+VisualLine where the caret will end up in
            var targetVisualLine = visualLine;
            TextLine targetLine;
            var textLineIndex = visualLine.TextLines.IndexOf(textLine);
            switch (direction)
            {
                case CaretMovementType.LineUp:
                    {
                        // Move up: move to the previous TextLine in the same visual line
                        // or move to the last TextLine of the previous visual line
                        var prevLineNumber = visualLine.FirstDocumentLine.LineNumber - 1;
                        if (textLineIndex > 0)
                        {
                            targetLine = visualLine.TextLines[textLineIndex - 1];
                        }
                        else if (prevLineNumber >= 1)
                        {
                            var prevLine = textView.Document.GetLineByNumber(prevLineNumber);
                            targetVisualLine = textView.GetOrConstructVisualLine(prevLine);
                            targetLine = targetVisualLine.TextLines[targetVisualLine.TextLines.Count - 1];
                        }
                        else
                        {
                            targetLine = null;
                        }
                        break;
                    }
                case CaretMovementType.LineDown:
                    {
                        // Move down: move to the next TextLine in the same visual line
                        // or move to the first TextLine of the next visual line
                        var nextLineNumber = visualLine.LastDocumentLine.LineNumber + 1;
                        if (textLineIndex < visualLine.TextLines.Count - 1)
                        {
                            targetLine = visualLine.TextLines[textLineIndex + 1];
                        }
                        else if (nextLineNumber <= textView.Document.LineCount)
                        {
                            var nextLine = textView.Document.GetLineByNumber(nextLineNumber);
                            targetVisualLine = textView.GetOrConstructVisualLine(nextLine);
                            targetLine = targetVisualLine.TextLines[0];
                        }
                        else
                        {
                            targetLine = null;
                        }
                        break;
                    }
                case CaretMovementType.PageUp:
                case CaretMovementType.PageDown:
                    {
                        // Page up/down: find the target line using its visual position
                        var yPos = visualLine.GetTextLineVisualYPosition(textLine, VisualYPosition.LineMiddle);
                        if (direction == CaretMovementType.PageUp)
                            yPos -= textView.Bounds.Height;
                        else
                            yPos += textView.Bounds.Height;
                        var newLine = textView.GetDocumentLineByVisualTop(yPos);
                        targetVisualLine = textView.GetOrConstructVisualLine(newLine);
                        targetLine = targetVisualLine.GetTextLineByVisualYPosition(yPos);
                        break;
                    }
                default:
                    throw new NotSupportedException(direction.ToString());
            }
            if (targetLine != null)
            {
                var yPos = targetVisualLine.GetTextLineVisualYPosition(targetLine, VisualYPosition.LineMiddle);
                var newVisualColumn = targetVisualLine.GetVisualColumn(new Point(xPos, yPos), enableVirtualSpace);

                // prevent wrapping to the next line; TODO: could 'IsAtEnd' help here?
                var targetLineStartCol = targetVisualLine.GetTextLineVisualStartColumn(targetLine);
                if (newVisualColumn >= targetLineStartCol + targetLine.Length)
                {
                    if (newVisualColumn <= targetVisualLine.VisualLength)
                        newVisualColumn = targetLineStartCol + targetLine.Length - 1;
                }
                return targetVisualLine.GetTextViewPosition(newVisualColumn);
            }
            else
            {
                return caretPosition;
            }
        }
        #endregion
    }
}
