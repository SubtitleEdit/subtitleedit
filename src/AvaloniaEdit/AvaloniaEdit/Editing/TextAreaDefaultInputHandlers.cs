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
using System.Windows.Input;
using AvaloniaEdit.Document;
using Avalonia.Input;

namespace AvaloniaEdit.Editing
{
    /// <summary>
    /// Contains the predefined input handlers.
    /// </summary>
    public class TextAreaDefaultInputHandler : TextAreaInputHandler
    {
        /// <summary>
        /// Gets the caret navigation input handler.
        /// </summary>
        public TextAreaInputHandler CaretNavigation { get; }

        /// <summary>
        /// Gets the editing input handler.
        /// </summary>
        public TextAreaInputHandler Editing { get; }

        /// <summary>
        /// Gets the mouse selection input handler.
        /// </summary>
        public ITextAreaInputHandler MouseSelection { get; }

        /// <summary>
        /// Creates a new TextAreaDefaultInputHandler instance.
        /// </summary>
        public TextAreaDefaultInputHandler(TextArea textArea) : base(textArea)
        {
            NestedInputHandlers.Add(CaretNavigation = CaretNavigationCommandHandler.Create(textArea));
            NestedInputHandlers.Add(Editing = EditingCommandHandler.Create(textArea));
            NestedInputHandlers.Add(MouseSelection = new SelectionMouseHandler(textArea));

            AddBinding(ApplicationCommands.Undo, ExecuteUndo, CanExecuteUndo);
            AddBinding(ApplicationCommands.Redo, ExecuteRedo, CanExecuteRedo);
        }

        private void AddBinding(RoutedCommand command, EventHandler<ExecutedRoutedEventArgs> handler, EventHandler<CanExecuteRoutedEventArgs> canExecuteHandler = null)
        {
            CommandBindings.Add(new RoutedCommandBinding(command, handler, canExecuteHandler));
        }

        internal static KeyBinding CreateKeyBinding(ICommand command, KeyModifiers modifiers, Key key)
        {
            return CreateKeyBinding(command, new KeyGesture(key, modifiers));
        }

        internal static KeyBinding CreateKeyBinding(ICommand command, KeyGesture gesture)
        {
            return new KeyBinding { Command = command, Gesture = gesture };
        }

        #region Undo / Redo

        private UndoStack GetUndoStack()
        {
            var document = TextArea.Document;
            return document?.UndoStack;
        }

        private void ExecuteUndo(object sender, ExecutedRoutedEventArgs e)
        {
            var undoStack = GetUndoStack();
            if (undoStack != null)
            {
                if (undoStack.CanUndo)
                {
                    undoStack.Undo();
                    TextArea.Caret.BringCaretToView();
                }
                e.Handled = true;
            }
        }

        private void CanExecuteUndo(object sender, CanExecuteRoutedEventArgs e)
        {
            var undoStack = GetUndoStack();
            if (undoStack != null)
            {
                e.Handled = true;
                e.CanExecute = undoStack.CanUndo;
            }
        }

        private void ExecuteRedo(object sender, ExecutedRoutedEventArgs e)
        {
            var undoStack = GetUndoStack();
            if (undoStack != null)
            {
                if (undoStack.CanRedo)
                {
                    undoStack.Redo();
                    TextArea.Caret.BringCaretToView();
                }
                e.Handled = true;
            }
        }

        private void CanExecuteRedo(object sender, CanExecuteRoutedEventArgs e)
        {
            var undoStack = GetUndoStack();
            if (undoStack != null)
            {
                e.Handled = true;
                e.CanExecute = undoStack.CanRedo;
            }
        }
        #endregion
    }
}
